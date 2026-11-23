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

// Scheduled tasks speak the server's language: French, Portuguese, English for any other.
foreach (var task in Enum.GetValues<TaskTexts.Task>())
{
    foreach (var (culture, language) in new (string?, string)[] { ("fr-FR", "fr"), ("pt-BR", "pt"), ("en-US", "en"), ("de-DE", "en"), (null, "en") })
    {
        Check($"task {task} name in {language} ({culture ?? "none"})", TaskTexts.Language(culture) == language && !string.IsNullOrWhiteSpace(TaskTexts.Name(task, culture)));
    }

    var taskNames = new[] { "fr-FR", "pt-PT", "en-GB" }.Select(c => TaskTexts.Name(task, c)).ToArray();
    var descriptions = new[] { "fr", "pt", "en" }.Select(c => TaskTexts.Description(task, c)).ToArray();
    Check($"task {task}: three different names", taskNames.Distinct().Count() == 3);
    Check($"task {task}: three different descriptions", descriptions.Distinct().Count() == 3);
}

Check("English names start with the plugin name", TaskTexts.Name(TaskTexts.Task.HistoryScan, "en") == "NotifySync: History Scan");
Check("French description of the history scan is French", TaskTexts.Description(TaskTexts.Task.HistoryScan, "fr").StartsWith("Analyse la bibliothèque", StringComparison.Ordinal));
Check("an English server no longer gets French text", !TaskTexts.Description(TaskTexts.Task.CollectionScan, "en-US").StartsWith("Analyse", StringComparison.Ordinal));
Check("language ignores case and underscore", TaskTexts.Language("PT_br") == "pt");

// Music by a favorite artist counts as a favorite, by name without regard to case.
var favArtists = FavoriteRules.NameSet(new string?[] { "Daft Punk", " ", null });
Check("a track by a favorite artist is a favorite", FavoriteRules.IsByFavoriteArtist(new[] { "daft punk" }, favArtists));
Check("an album artist counts too", FavoriteRules.IsByFavoriteArtist(new[] { "Someone", "Daft Punk" }, favArtists));
Check("another artist is no favorite", !FavoriteRules.IsByFavoriteArtist(new[] { "Justice" }, favArtists));
Check("no favorite artist means no favorite track", !FavoriteRules.IsByFavoriteArtist(new[] { "Daft Punk" }, FavoriteRules.NameSet(Array.Empty<string?>())));
Check("a track without artists is no favorite", !FavoriteRules.IsByFavoriteArtist(null, favArtists));
Check("blank names never match", favArtists.Count == 1);
Check("favorites are off in a fresh configuration", !new PluginConfiguration().FavoritesInBell);

// The plugin page's description follows the server language, and the catalogs and meta.json say the same.
Check("description in French", PluginDescription.For("fr-FR") == PluginDescription.French);
Check("description in Portuguese", PluginDescription.For("pt-BR") == PluginDescription.Portuguese && PluginDescription.For("pt_PT") == PluginDescription.Portuguese);
Check("description in English for any other language", PluginDescription.For("de-DE") == PluginDescription.English && PluginDescription.For(null) == PluginDescription.English);
var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
var threeTexts = string.Join("\n\n", PluginDescription.French, PluginDescription.English, PluginDescription.Portuguese);
foreach (var file in new[] { "meta.json", "repository.json", "repository-12-beta.json" })
{
    var path = Path.Combine(repoRoot, file);
    Check($"{file} carries the three texts of the plugin page", System.IO.File.Exists(path) && System.IO.File.ReadAllText(path).Contains(threeTexts.Replace("\n", "\\n"), StringComparison.Ordinal));
}

// A replaced movie that Jellyfin names or dates differently must still be matched to its deleted file
// (measured 2026-11-23 on a real server: Radarr upgraded two films, both stayed "new" in the bell).
// Values below are the ones read from that server: same folder, new name and/or new year.
{
    var dbDir = Path.Combine(Path.GetTempPath(), "notifysync-test-" + Guid.NewGuid().ToString("N"));
    var db = new NotificationDatabase(dbDir, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
    const string zw = "​";
    db.SaveDeletedItem("i-lilo", "Lilo et Stitch", "Movie", null, 2002, null, null, "/disk/Film/" + zw + "Lilo and Stitch (2002) {tmdb-11544}/Lilo and Stitch (2002) [MULTI VFF] [Bluray-1080p] [x264] [FR+EN].mkv", null);
    db.SaveDeletedItem("i-snow", "Blanche-Neige et les Sept Nains", "Movie", null, 1938, null, null, "/disk/Film/" + zw + "Snow White and the Seven Dwarfs (1938) {tmdb-408}/Snow White and the Seven Dwarfs (1938) [MULTI VFF] [Bluray-1080p] [x264] [EN+FR].mkv", null);
    db.SaveDeletedItem("i-other", "Autre film", "Movie", null, 2001, null, null, "/disk/Film/" + zw + "Autre film (2001) {tmdb-1}/Autre film (2001) [x264].mkv", null);
    Check("a replaced movie is matched when its name and year are unchanged", db.TryGetDeletedMatch("Autre film", "Movie", 2001) != null);
    Check("a replaced movie is matched by its folder when Jellyfin changed the name and the year (Lilo)",
        db.TryGetDeletedMatch("Lilo & Stitch", "Movie", 2025, null, null, null, "/disk/Film/" + zw + "Lilo and Stitch (2002) {tmdb-11544}/Lilo and Stitch (2002) [MULTI VFF] [Bluray-1080p] [x265] [FR+EN].mkv") is { ItemId: "i-lilo" });
    Check("a replaced movie is matched by its folder when Jellyfin changed the year (Snow White)",
        db.TryGetDeletedMatch("Blanche-Neige et les sept nains", "Movie", 1937, null, null, null, "/disk/Film/" + zw + "Snow White and the Seven Dwarfs (1938) {tmdb-408}/Snow White and the Seven Dwarfs (1938) [MULTI VFF] [Bluray-1080p] [x265] [FR+EN].mkv") is { ItemId: "i-snow" });
    Check("another film in another folder is not matched", db.TryGetDeletedMatch("Un autre titre", "Movie", 1999, null, null, null, "/disk/Film/Un autre titre (1999) {tmdb-9}/Un autre titre (1999) [x265].mkv") == null);
    Check("a folder shared by many films proves nothing", db.TryGetDeletedMatch("Un autre titre", "Movie", 1999, null, null, null, "/disk/Film/Un autre titre.mkv") == null);
    Check("without a path the movie is still matched by name and year only", db.TryGetDeletedMatch("Un autre titre", "Movie", 1999) == null);
    Check("an episode is never matched by its folder", db.TryGetDeletedMatch("Lilo & Stitch", "Episode", 2025, "Serie", 1, 1, "/disk/Film/" + zw + "Lilo and Stitch (2002) {tmdb-11544}/Lilo and Stitch (2002) [x265].mkv") == null);
    Check("movie folder key ignores the invisible characters and the case", MediaChangeDetector.MovieFolderKey("/d/" + zw + "Lilo and Stitch (2002) {TMDB-11544}/f.mkv") == "lilo and stitch (2002) {tmdb-11544}");
    Check("movie folder key is null for a plain shared folder", MediaChangeDetector.MovieFolderKey("/d/Films/f.mkv") == null);
    Check("movie folder key reads Windows paths", MediaChangeDetector.MovieFolderKey(@"D:\Films\Heat (1995)\Heat.mkv") == "heat (1995)");
    try { Directory.Delete(dbDir, true); } catch { /* the pooled connection may still hold the file */ }
}

Console.WriteLine(failed == 0 ? "All tests passed." : failed + " test(s) failed.");
return failed == 0 ? 0 : 1;
