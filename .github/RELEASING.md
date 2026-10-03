# Releasing NotifySync

Four channels, one version number per build.

| Channel | Branch | Tag | GitHub release | Catalog | Audience |
|---|---|---|---|---|---|
| **Stable** | `main` | `vX.Y.Z.0` | normal — becomes *Latest* | `repository.json` on `main` | Jellyfin 10.11.x |
| **Beta** | `main` | `vX.Y.Z.W-beta`, W ≥ 1 | pre-release | `repository-beta.json` on `main` | testers on 10.11.x |
| **Jellyfin 12** | `jellyfin-12` | `vX.Y.Z.0-jf12` | pre-release | `repository-12.json` on `jellyfin-12` | Jellyfin 12.x |
| **Jellyfin 12 beta** | `jellyfin-12` | `vX.Y.Z.W-jf12-beta`, W ≥ 1 | pre-release | `repository-12-beta.json` on `jellyfin-12` | testers on 12.x |

- A stable number always ends in `.0`, a beta's never does. The admin page relies on it: its version link adds `-jf12` on a 12.x server, then `-beta` when the last number is not 0.
- Jellyfin 12 releases stay **pre-releases** on purpose. GitHub gives *Latest* to the newest normal release, and a build for 12 must never be offered as the default download to 10.11 users.
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

## A beta

1. Bump to the next beta number in the places above, build, zip.
2. Tag `vX.Y.Z.W-beta` and publish it as a **pre-release** titled `🔔 NotifySync vX.Y.Z.W — Beta`, with notes following [RELEASE_NOTES_TEMPLATE.md](RELEASE_NOTES_TEMPLATE.md).
3. Replace the entry of `repository-beta.json` — changelog label `🧪 Beta — Release Notes`, `targetAbi` `10.11.0.0`.

## Promoting a beta to stable

1. Bump to the next stable number, build, zip.
2. Tag `vX.Y.Z.0` and publish it as a normal release titled `🔔 NotifySync vX.Y.Z.0`. Its notes gather the betas of the cycle.
3. Add the entry to `repository.json` — changelog label `📋 Release Notes vX.Y.Z.0` — and point `repository-beta.json` at the same build.
4. On every beta of the cycle, swap the beta banner for the *shipped as stable* one.

## Jellyfin 12

`jellyfin-12` carries the same C# as `main`, built for .NET 10 against Jellyfin 12. Its `client.js` differs where the 12 web client does: where the bell goes, the authentication header, the WebSocket.

1. Cherry-pick the code commits from `main`, settle `client.js` by hand if needed, build with 0 errors and 0 warnings.
2. Bump the version in the places above.
3. Tag `vX.Y.Z.0-jf12` and publish it as a **pre-release** titled `🔔 NotifySync vX.Y.Z.0 — Jellyfin 12`, its notes opening with the 🚀 banner.
4. Add the entry to `repository-12.json` on `jellyfin-12` — `targetAbi` `12.0.0.0` — and point `repository-12-beta.json` at the same build.

A Jellyfin 12 beta takes steps 1 and 2 with a beta number, then:

3. Tag `vX.Y.Z.W-jf12-beta` and publish it as a **pre-release** titled `🔔 NotifySync vX.Y.Z.W — Jellyfin 12 — Beta`, its notes opening with the beta banner, then the 🚀 one.
4. Replace the entry of `repository-12-beta.json` on `jellyfin-12` — changelog label `🧪 Beta — Release Notes`, `targetAbi` `12.0.0.0`.
