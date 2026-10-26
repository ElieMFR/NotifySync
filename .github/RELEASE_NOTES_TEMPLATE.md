# Release notes

Every GitHub release has the same shape: English, then French.

## Rules

- **One short bold line per change**, saying what the user sees — never how it was done. No file, class or setting names, no code.
- **No introduction.** The notes open on the sections, after the channel banner when there is one.
- **Each version has its own wording.** Never copy the previous release's lines.
- A beta lists only what it adds since the previous beta; a stable gathers the betas of its cycle.
- Sections, in this order, only those that apply: `🛡️ Security`, `✨ New` or `✨ Improvement`, `🐛 Fixes`, `🔧 Internal — nothing changes on screen`.

## Banners

A beta opens with:

```markdown
> 🧪 **Beta channel.** To be validated before it reaches stable. To test it, add this repository in Dashboard → Plugins → Repositories:
> `https://raw.githubusercontent.com/ElieMFR/NotifySync/refs/heads/jellyfin-12/repository-12-beta.json`
> *Canal beta — à valider avant passage en stable. Pour la tester, ajoutez ce dépôt dans Tableau de bord → Extensions → Dépôts (lien ci-dessus).*
```

Once it ships, that banner becomes:

```markdown
> ✅ **Validated and shipped as stable [vX.Y.Z.0](https://github.com/ElieMFR/NotifySync/releases/tag/vX.Y.Z.0).** Nothing left to test here.
> *Validée et publiée en stable [vX.Y.Z.0](https://github.com/ElieMFR/NotifySync/releases/tag/vX.Y.Z.0). Plus rien à tester ici.*
```

A Jellyfin 12 release opens with:

```markdown
> 🚀 **For Jellyfin 12.0 and newer.** On Jellyfin 10.11.x, keep version [5.8.4.0](https://github.com/ElieMFR/NotifySync/releases/tag/v5.8.4.0) — this build will not load there.
> *Pour Jellyfin 12.0 et plus récent. En 10.11.x, gardez la version [5.8.4.0](https://github.com/ElieMFR/NotifySync/releases/tag/v5.8.4.0) — ce build ne s'y chargera pas.*
```

A Jellyfin 12 beta opens with the beta banner, then the 🚀 one; once it ships, its link goes to `vX.Y.Z.0-jf12`. A 10.11 release has no banner.

## Shape

```markdown
## 🇬🇧 English

### 🐛 Fixes

- **A card announcing one season now opens that season, not the whole series.**

### 🔧 Internal — nothing changes on screen

- **…**

---

## 🇫🇷 Français

### 🐛 Corrections

- **Une carte qui annonce une saison ouvre cette saison, plus la série entière.**

### 🔧 Interne — rien ne change à l'écran

- **…**
```

In French the sections are `🛡️ Sécurité`, `✨ Nouveauté` or `✨ Amélioration`, `🐛 Corrections`, `🔧 Interne — rien ne change à l'écran`.

## Titles

- Jellyfin 12: `🔔 NotifySync vX.Y.Z.0 — Jellyfin 12`
- Jellyfin 12 beta: `🔔 NotifySync vX.Y.Z.W — Jellyfin 12 — Beta`
- Jellyfin 10.11 (frozen at 5.8.4.0): `🔔 NotifySync vX.Y.Z.0 — Jellyfin 10.11`
