# Release notes

Every GitHub release has the same shape: English, then French.

## Rules

- **One short bold line per change**, saying what the user sees — never how it was done. No file, class or setting names, no code.
- **No introduction.** The notes open on the sections, after the channel banner when there is one.
- **Each version has its own wording.** Never copy the previous release's lines.
- A beta lists every change since the last stable; a stable gathers the betas of its cycle.
- Sections, in this order, only those that apply: `🛡️ Security`, `✨ New` or `✨ Improvement`, `🐛 Fixes`, `🔧 Internal — nothing changes on screen`.

## Banners

A beta opens with:

```markdown
> 🧪 **Beta channel.** To be validated before it reaches stable.
> *Canal beta — à valider avant passage en stable.*
```

Once it ships, that banner becomes:

```markdown
> ✅ **Validated and shipped as stable [vX.Y.Z.0](https://github.com/peterdu1109/NotifySync/releases/tag/vX.Y.Z.0).** Nothing left to test here.
> *Validée et publiée en stable [vX.Y.Z.0](https://github.com/peterdu1109/NotifySync/releases/tag/vX.Y.Z.0). Plus rien à tester ici.*
```

A Jellyfin 12 release opens with:

```markdown
> 🚀 **For Jellyfin 12.0 and newer.** On Jellyfin 10.11.x, use the stable release — this build will not load there.
> *Pour Jellyfin 12.0 et plus récent. En 10.11.x, utilisez la version stable — ce build ne s'y chargera pas.*
```

A stable release has no banner.

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

- Stable: `🔔 NotifySync vX.Y.Z.0`
- Beta: `🔔 NotifySync vX.Y.Z.W — Beta`
- Jellyfin 12: `🔔 NotifySync vX.Y.Z.0 — Jellyfin 12`
