using System.Security.Claims;
using NotifySync;

// Who may read whose bell, and what counts as a real file upgrade. A mistake in the first shows
// one account another's new titles (measured once in production); in the second, the bell
// announces a rename as an upgrade or misses a real one (the 5.5.9.2 regressions).
var failed = 0;

void Check(string name, bool ok)
{
    Console.WriteLine((ok ? "OK    " : "FAIL  ") + name);
    if (!ok)
    {
        failed++;
    }
}

// Claims as Jellyfin 12.4's CustomAuthenticationHandler sets them (read in Jellyfin.Api.dll).
static ClaimsPrincipal Caller(string name, string role, Guid userId, bool apiKey) =>
    new ClaimsPrincipal(new ClaimsIdentity(
        new[]
        {
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role),
            new Claim("Jellyfin-UserId", userId.ToString("N")),
            new Claim("Jellyfin-IsApiKey", apiKey.ToString()),
        },
        "Custom"));

var alice = Guid.NewGuid();
var bob = Guid.NewGuid();
var admin = Guid.NewGuid();
Guid? ByName(string name) => name switch
{
    "alice" => alice,
    "bob" => bob,
    "admin" => admin,
    _ => null,
};

UserAccess Access(ClaimsPrincipal caller, Guid requested)
    => AccessRules.CheckUserAccess(caller, requested.ToString("N"), ByName, out _);

var aliceCaller = Caller("alice", "User", alice, false);
var adminCaller = Caller("admin", "Administrator", admin, false);
var apiKeyCaller = Caller(string.Empty, "Administrator", Guid.Empty, true);

// EXG-012: a bell route answers for the caller's own account, or for any as an administrator.
Check("EXG-012 account reads its own bell", Access(aliceCaller, alice) == UserAccess.Allowed);
Check("EXG-012 account reads its own bell, id with dashes", AccessRules.CheckUserAccess(aliceCaller, alice.ToString("D"), ByName, out _) == UserAccess.Allowed);
Check("EXG-012 account cannot read another account's bell", Access(aliceCaller, bob) == UserAccess.Denied);
Check("EXG-012 administrator reads any bell", Access(adminCaller, bob) == UserAccess.Allowed);
Check("EXG-012 API keys are not supported", Access(apiKeyCaller, alice) == UserAccess.NoIdentity);
Check("EXG-012 unknown name is refused", Access(Caller("ghost", "User", Guid.NewGuid(), false), bob) == UserAccess.UnknownUser);

// EXG-013: the admin routes (Refresh, ScanCollections, DeletedItems).
Check("EXG-013 account is not an administrator", !AccessRules.IsAdministrator(aliceCaller));
Check("EXG-013 administrator passes", AccessRules.IsAdministrator(adminCaller));
Check("EXG-013 API key passes", AccessRules.IsAdministrator(apiKeyCaller));
Check("EXG-013 anonymous is refused", !AccessRules.IsAdministrator(new ClaimsPrincipal(new ClaimsIdentity())));

// Collections: a card follows its collection through the settings and renames.
var colA = Guid.NewGuid();
var colB = Guid.NewGuid();
var onlyA = new HashSet<Guid> { colA };
string CardOf(Guid col) => $"col:{col:N}:{Guid.NewGuid()}";
Check("collection card of a monitored collection is shown", !CollectionRules.IsFromUnmonitoredCollection(CardOf(colA), onlyA));
Check("collection card of an unchecked collection is hidden", CollectionRules.IsFromUnmonitoredCollection(CardOf(colB), onlyA));
Check("no collection monitored hides every collection card", CollectionRules.IsFromUnmonitoredCollection(CardOf(colA), new HashSet<Guid>()));
Check("a library notification is never hidden by the rule", !CollectionRules.IsFromUnmonitoredCollection(Guid.NewGuid().ToString(), new HashSet<Guid>()));
Check("card belongs to its own collection", CollectionRules.BelongsTo(CardOf(colA), colA));
Check("card does not belong to another collection", !CollectionRules.BelongsTo(CardOf(colA), colB));
Check("settings ids that do not parse are ignored", CollectionRules.MonitoredSet(new[] { colA.ToString("N"), "not-a-guid" }).SetEquals(onlyA));

// Upgrade detection: what changed between the deleted file and the one that replaced it.
static NotificationItem File(string path, long? size = null) => new NotificationItem { FilePath = path, Size = size };
string? Kind(string oldPath, string newPath, long? oldSize = null, long? newSize = null)
    => MediaChangeDetector.ClassifyUpgrade(File(oldPath, oldSize), File(newPath, newSize));

Check("upgrade 1080p to 2160p is quality", Kind("/m/Film (2020) [WEBDL-1080p] [x264].mkv", "/m/Film (2020) [WEBDL-2160p] [x264].mkv") == "quality");
Check("upgrade x264 to x265 is codec", Kind("/m/Film [1080p] [x264].mkv", "/m/Film [1080p] [x265].mkv") == "codec");
Check("upgrade VOSTFR to MULTI is audio", Kind("/m/Film [VOSTFR] [1080p].mkv", "/m/Film [MULTI] [1080p].mkv") == "audio");
Check("upgrade with several changes lists them all", Kind("/m/Film [1080p] [x264].mkv", "/m/Film [2160p] [x265] [MULTI].mkv") == "quality,codec,audio");
Check("removing an audio track is no upgrade", Kind("/m/Film [MULTI] [1080p].mkv", "/m/Film [VOSTFR] [1080p].mkv") == null);
Check("plain rename is no upgrade", Kind("/m/Film [1080p].mkv", "/m/Film (2020) [1080p].mkv") == null);
Check("same size under another name is no upgrade", Kind("/m/Film [1080p] [x264].mkv", "/m/Film [2160p] [x265].mkv", 1000, 1000) == null);
Check("same path is no upgrade", Kind("/m/Film [1080p].mkv", "/m/Film [1080p].mkv") == null);
Check("no old path gives no signal", Kind(string.Empty, "/m/Film [2160p].mkv") == null);

// Moves: an identical size against a recent deletion means the file only moved.
Check("same size back again is a move", MediaChangeDetector.IsSameFileBackAgain(1234, 1234));
Check("different size is not a move", !MediaChangeDetector.IsSameFileBackAgain(1234, 1235));
Check("unknown size is not a move", !MediaChangeDetector.IsSameFileBackAgain(null, 1234));

// A monitored library typed by name (the admin's manual list) names the card's category.
var libA = Guid.NewGuid();
var libB = Guid.NewGuid();
var names = new Dictionary<Guid, string> { [libA] = "Films", [libB] = "Anime" };
string? NameOf(Guid g) => names.TryGetValue(g, out var n) ? n : null;
Check("source name from a library id", CollectionRules.MonitoredSourceName(new[] { libA.ToString() }, new[] { libA }, Guid.Empty, NameOf) == "Films");
Check("source name from a typed name", CollectionRules.MonitoredSourceName(new[] { "films" }, new[] { Guid.NewGuid(), libA }, Guid.Empty, NameOf) == "Films");
Check("a typed name that is no ancestor gives none", CollectionRules.MonitoredSourceName(new[] { "Anime" }, new[] { libA }, Guid.Empty, NameOf) == null);
Check("a blank entry is skipped", CollectionRules.MonitoredSourceName(new[] { " ", libB.ToString() }, new[] { libB }, Guid.Empty, NameOf) == "Anime");

// The card says what changed in the resolution, in the direction it went.
Check("1080p to 4K reads as such", MediaChangeDetector.ResolutionChange("/m/Film [1080p].mkv", "/m/Film [2160p].mkv") == "1080p → 4K");
Check("a drop keeps its real direction", MediaChangeDetector.ResolutionChange("/m/Film.2160p.mkv", "/m/Film.1080p.mkv") == "4K → 1080p");
Check("720p to 1080p", MediaChangeDetector.ResolutionChange("/m/Film.720p.mkv", "/m/Film.1080p.mkv") == "720p → 1080p");
Check("the same resolution says nothing", MediaChangeDetector.ResolutionChange("/m/Film.1080p.x264.mkv", "/m/Film.1080p.x265.mkv") == null);
Check("an unmarked name says nothing", MediaChangeDetector.ResolutionChange("/m/Film.mkv", "/m/Film.2160p.mkv") == null);

Console.WriteLine(failed == 0 ? "All tests passed." : failed + " test(s) failed.");
return failed == 0 ? 0 : 1;
