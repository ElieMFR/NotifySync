# Releasing NotifySync

Four channels, one version number per build. Since 6.0.0.0 Jellyfin 12 is the main line; Jellyfin 10.11 stays on 5.8.4.0.

| Channel | Branch | Tag | GitHub release | Catalog | Audience |
|---|---|---|---|---|---|
| **Jellyfin 12** | `jellyfin-12` | `vX.Y.Z.0-jf12` | normal — becomes *Latest* | `repository.json` on `main` **and** `repository-12.json` on `jellyfin-12` | Jellyfin 12.x |
| **Jellyfin 12 beta** | `jellyfin-12` | `vX.Y.Z.W-jf12-beta`, W ≥ 1 | pre-release | `repository-12-beta.json` on `jellyfin-12` | testers on 12.x |
| **Jellyfin 10.11** | `main` | `vX.Y.Z.0` | normal, never *Latest* | `repository.json` on `main` | Jellyfin 10.11.x — frozen at 5.8.4.0 |

- A stable number always ends in `.0`, a beta's never does. The admin page relies on it: its version link adds `-jf12` on a 12.x server, then `-beta` when the last number is not 0.
- The Jellyfin 12 stable is the one release that takes *Latest*: from 6.0.0.0 on, the default download is the Jellyfin 12 build, and 6.x builds are for Jellyfin 12 only. Should 10.11 ever need another build, publish it with `--latest=false`, so that *Latest* never goes back to a 10.11 build. Betas stay pre-releases.
- Each beta catalog holds a single entry, the current beta. The stable and Jellyfin 12 catalogs keep their history.
- Every catalog names the package **NotifySync**, exactly the `name` of `meta.json`. Jellyfin names a fresh install after its catalog entry, and at startup only removes an older version that carries the same name: under another name, both versions can load together, and Jellyfin then rewrites the settings with their defaults.
- On `jellyfin-12`, both catalogs and `meta.json` carry the same `description`: the French then the English text of `PluginDescription.cs`, with no install steps. Jellyfin shows it in the catalog, and on the plugin page until the restart that loads the plugin; from then on the page shows the plugin's own description, in the server's language.

## Where the version number lives

- `NotifySync.csproj` — `AssemblyVersion`, `FileVersion`, `Version`
- `meta.json` — `version`
- `client.js` — the header comment

The admin page and the README carry no version number: the page reads it from the installed plugin, the README badge from GitHub.

## Build and package

```bash
dotnet build -c Release
```

- The zip holds exactly `NotifySync.dll`, `meta.json` and `client.js`; Jellyfin provides everything else.
- The catalog `checksum` is the zip's MD5, upper case. Check that the uploaded asset, the local zip and the catalog agree before announcing the release.
- The catalog `timestamp` must be in the past **in UTC** (`date -u`) — Jellyfin can hide a version dated in the future.
- Never raise `Jellyfin.*` or `Microsoft.Data.Sqlite` above what the target Jellyfin ships. The plugin binds to the server's copies, and .NET refuses a server version lower than the one the plugin was compiled against: the plugin then shows as *Malfunctioned*.

## Jellyfin 12 — the main line

`jellyfin-12` is built for .NET 10 against Jellyfin 12. Its `client.js` handles the 12 web client and TV apps; `main` keeps the frozen 10.11 code.

1. Commit on `jellyfin-12` and build with 0 errors and 0 warnings.
2. Bump the version in the places above.
3. Tag `vX.Y.Z.0-jf12` and publish it as a **normal release marked *Latest*** (`gh release create … --latest`), titled `🔔 NotifySync vX.Y.Z.0 — Jellyfin 12`. Its notes open with the 🚀 banner and gather the betas of the cycle.
4. Add the entry on top of `repository.json` on `main` **and** of `repository-12.json` on `jellyfin-12` — `targetAbi` `12.0.0.0` — and point `repository-12-beta.json` at the same build.

   `repository.json` on `main` is the one link the README gives, for 10.11 and 12 alike: Jellyfin never offers a version whose `targetAbi` is above the server's, so a 10.11 server keeps 5.8.4.0 and a 12 server gets the newest 6.x — including one that installed 5.8.4.0 from this catalog by mistake, which then sees the update. `repository-12.json` is no longer advertised but stays up to date for the servers that already use it.
5. On every beta of the cycle, swap the beta banner for the *shipped as stable* one.

A Jellyfin 12 beta takes steps 1 and 2 with a beta number, then:

3. Tag `vX.Y.Z.W-jf12-beta` and publish it as a **pre-release** titled `🔔 NotifySync vX.Y.Z.W — Jellyfin 12 — Beta`, its notes following [RELEASE_NOTES_TEMPLATE.md](RELEASE_NOTES_TEMPLATE.md) and opening with the beta banner, then the 🚀 one.
4. Replace the entry of `repository-12-beta.json` on `jellyfin-12` — changelog label `🧪 Beta — Release Notes`, `targetAbi` `12.0.0.0`.

## Jellyfin 10.11 — frozen at 5.8.4.0

Only for a fix that must reach 10.11 servers, on `main`:

1. Bump to the next stable number, build, zip.
2. Tag `vX.Y.Z.0` and publish it as a normal release with `--latest=false`, titled `🔔 NotifySync vX.Y.Z.0 — Jellyfin 10.11`.
3. Add the entry on top of `repository.json` — changelog label `📋 Release Notes vX.Y.Z.0`, `targetAbi` `10.11.0.0` — and point `repository-beta.json` at the same build.
