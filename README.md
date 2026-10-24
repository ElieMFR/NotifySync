<h1 align="center">🔔 NotifySync</h1>

<p align="center">
  <img src="https://img.shields.io/github/v/release/peterdu1109/NotifySync?label=version&color=blue&cacheSeconds=3600" alt="Version">
  <img src="https://img.shields.io/badge/.NET-10.0%20%7C%209.0-purple" alt=".NET Framework">
  <img src="https://img.shields.io/badge/Jellyfin-12.x%20%7C%2010.11.x-blueviolet" alt="Jellyfin">
  <img src="https://img.shields.io/badge/license-MIT-green" alt="License">
  <img src="https://img.shields.io/github/stars/peterdu1109/NotifySync?style=flat&color=yellow&cacheSeconds=3600" alt="GitHub stars">
  <img src="https://img.shields.io/github/downloads/peterdu1109/NotifySync/total?color=brightgreen&cacheSeconds=3600" alt="Downloads">
  <img src="https://img.shields.io/github/release-date/peterdu1109/NotifySync?color=orange&cacheSeconds=3600" alt="Last release">
</p>

<p align="center">
  <b>The modern notification center Jellyfin has been waiting for.</b><br>
  <i>Le centre de notifications moderne que Jellyfin attendait.</i>
</p>


---

<p align="center">
  <b>🌐 Language / Langue</b><br>
  <a href="#english">English</a> | <a href="#français">Français</a>
</p>

---

<a name="english"></a>
# English

NotifySync adds a notification bell to Jellyfin, like on streaming platforms. Your users see the latest movies, series and music added without leaving their page.

> 📖 **Version history:** see [GitHub Releases](https://github.com/peterdu1109/NotifySync/releases) for what's new in each version.

## 📑 Table of Contents

- [✨ Key Features](#-key-features)
- [📦 Installation](#-installation)
- [⚙️ Configuration](#️-configuration)
- [❓ Troubleshooting](#-troubleshooting)
- [🎨 Recommended Theme](#-recommended-theme)
- [💬 Support](#-support)
- [👥 Authors & Credits](#-authors--credits)

## ✨ Key Features

*   **Streaming-style bell**: the latest addition is shown large, the others in a list.
*   **Badges and counter**: the red counter clears when you open the bell. Each item keeps its NEW / UPD badge until you dismiss it or watch it.
*   **Real-time**: additions show up as soon as Jellyfin announces them, without reloading the page.
*   **Sorted by date**: Today, This week, Earlier.
*   **Grouping**: episodes that arrived together share one card (`Season 4 • Ep. 1-12`). Tracks are grouped by album.
*   **Upgrades detected**: a replaced file comes back with a blue **UPD** badge that says what changed (Quality, Codec or Audio). Subtitles and metadata trigger nothing.
*   **Moves ignored**: reorganising your libraries announces nothing new.
*   **Nothing incomplete**: a title waits, up to 5 minutes, until Jellyfin has identified it, with its artwork.
*   **Filters**: by category or by favorites.
*   **Dismiss**: an item or a group, or swipe left on mobile. Dismissing does not mark it as played.
*   **Collections**: monitored collections announce their new additions.
*   **Deletion history**: a log for admins.
*   **Synced**: what you read or dismiss on one device is read or dismissed on all of them.
*   **Permissions respected**: everyone only sees the libraries they can access.
*   **Bilingual**: French or English, following your Jellyfin language.
*   **On every screen**: desktop, tablet and mobile. On TV, when the app loads NotifySync's script (Samsung TVs installed with [Apps2Samsung](https://github.com/Apps2Samsung/Apps2Samsung) do), the remote drives it: arrows to browse, OK to open or dismiss, Back to close.

## 📦 Installation

> [!IMPORTANT]
> **Requirements:**
> * Jellyfin **12.x**
> * Jellyfin **10.11.x** (with NotifySync 5.8.4.0)
> * The **File Transformation** plugin, or a manual edit (see [Without File Transformation](#without-file-transformation))

### Steps
1.  Open **Dashboard → Plugins → Manage Repositories** and add these two repositories:

    **NotifySync** (the same for 12.x and 10.11.x):
    ```
    https://raw.githubusercontent.com/peterdu1109/NotifySync/refs/heads/main/repository.json
    ```
    **File Transformation** (adds the bell to Jellyfin and survives its updates):
    ```
    https://www.iamparadox.dev/jellyfin/plugins/manifest.json
    ```
2.  In **Plugins**, filter **Available**, install **NotifySync** and **File Transformation**.
3.  Restart Jellyfin, then `Ctrl+F5` in your browser.
4.  In **Dashboard → Plugins → NotifySync**, check the libraries to monitor, click **Save**, then **Regenerate history**. The bell stays empty until a library is checked.

### Without File Transformation

> [!WARNING]
> To redo after every Jellyfin update, which overwrites `index.html`.

**Linux**
```bash
sudo sed -i 's|</body>|    <script src="/NotifySync/client.js"></script>\n</body>|' /usr/share/jellyfin/web/index.html
```

**Docker**
```bash
docker exec jellyfin sed -i 's|</body>|    <script src="/NotifySync/client.js"></script>\n</body>|' /jellyfin/jellyfin-web/index.html
```

**Windows**: open this file in a text editor started as administrator:

```
C:\Program Files\Jellyfin\Server\jellyfin-web\index.html
```

and paste this line just before `</body>`:

```html
<script src="/NotifySync/client.js"></script>
```

## ⚙️ Configuration

Go to **Dashboard → Plugins → NotifySync**.

| Setting | Default | Description |
|---------|---------|-------------|
| **Quota per Category** | `10` | Maximum number of cards shown per category (1 to 50). |
| **Monitored Libraries** | *(none)* | Check the libraries you want to appear in the bell. |
| **Monitored Collections** | *(none)* | Collections (BoxSets) to watch for new additions. Scanned every 15 minutes. |
| **Category Mapping** | *(empty)* | Rename or group your libraries in the bell (e.g. *"My Movies"* → *"Movies"*). The arrows set the order of the filters. |
| **Manual Selection (IDs or Names)** | *(empty)* | Library IDs or names for advanced setups (Channels, XFusion). |
| **Enable deletion tracking** | `enabled` | Keeps the deletion log. Needed to detect Sonarr/Radarr replacements (deleted, then re-imported within 7 days). |
| **Retention (days)** | `30` | How many days the deletion log is kept (7 to 365). |
| **Regenerate history** | | Full rescan, after changing libraries or categories. |
| **Scan collections now** | | Scans the monitored collections right away. |

## ❓ Troubleshooting

| Issue | Solution |
|-------|----------|
| **Bell doesn't appear** | Check that **File Transformation** is installed. Restart Jellyfin, then `Ctrl+F5`. |
| **No bell and `401 Unauthorized` errors in the browser console** | Your server runs Jellyfin 12 with the old 5.x version. Update NotifySync to 6.x from **Plugins**, restart Jellyfin, then `Ctrl+F5`. |
| **Bell gone after a Jellyfin update** | Without File Transformation, each update overwrites the edit. Redo it, or install File Transformation. |
| **Content or music missing, wrong counter** | Check the library in **Monitored Libraries**, then click **Regenerate history**. |
| **Replaced file shows NEW instead of UPD** | Turn on **Enable deletion tracking**. It is needed when a file is deleted, then re-imported. |
| **A renamed file shows a false UPD badge** | A plain rename triggers nothing. If a badge shows up anyway, dismiss it: it won't come back. |
| **A moved series still shows as NEW** | Turn on **Enable deletion tracking**. Detection only works if the folder keeps its name. |
| **Unauthorized content visible** | The plugin follows Jellyfin permissions. Check the user's restrictions. |
| **No bell on the admin pages** | Normal: the bell only shows on the user pages. |
| **429 Error** | Wait 30 seconds between two clicks on **Regenerate history**. |
| **Incompatible** | Use the repository from [Installation](#-installation). Jellyfin 12.x needs NotifySync 6.x. |

## 🎨 Recommended Theme

NotifySync goes well with [**ElegantFin Cinema Edition**](https://github.com/peterdu1109/ElegantFin), a streaming-inspired Jellyfin theme. Both work together on PC, mobile and TV.

## 💬 Support

Open an issue on [GitHub](https://github.com/peterdu1109/NotifySync/issues) with your Jellyfin version, your NotifySync version, the app or browser you use, and the server log if you can. Contributions and feature ideas are welcome there too.

## 👥 Authors & Credits

NotifySync is maintained by:

- **[ElieMFR](https://github.com/ElieMFR)**: author and project lead
- **[Peterdu1109](https://github.com/peterdu1109)**: co-maintainer

### 🙏 Contributors

<p><a href="https://github.com/muisje" title="@muisje"><img src="https://github.com/muisje.png?size=96" width="48" height="48" alt="@muisje" /></a> <a href="https://github.com/Wernouxe" title="@Wernouxe"><img src="https://github.com/Wernouxe.png?size=96" width="48" height="48" alt="@Wernouxe" /></a> <a href="https://github.com/hstark16" title="@hstark16"><img src="https://github.com/hstark16.png?size=96" width="48" height="48" alt="@hstark16" /></a> <a href="https://github.com/Shillooh" title="@Shillooh"><img src="https://github.com/Shillooh.png?size=96" width="48" height="48" alt="@Shillooh" /></a></p>

Released under the [MIT License](./LICENSE).

---

<a name="français"></a>
# Français

NotifySync ajoute à Jellyfin une cloche de notifications, comme sur les plateformes de streaming. Vos utilisateurs voient les derniers films, séries et musiques ajoutés sans quitter leur page.

> 📖 **Journal des versions :** voir les [Releases GitHub](https://github.com/peterdu1109/NotifySync/releases) pour les nouveautés de chaque version.

## 📑 Sommaire

- [✨ Fonctionnalités](#-fonctionnalités)
- [📦 Installation](#-installation-1)
- [⚙️ Configuration](#️-configuration-1)
- [❓ Dépannage](#-dépannage)
- [🎨 Thème recommandé](#-thème-recommandé)
- [💬 Support](#-support-1)
- [👥 Auteurs & Crédits](#-auteurs--crédits)

## ✨ Fonctionnalités

*   **Cloche style streaming** : le dernier ajout est affiché en grand, les autres en liste.
*   **Badges et compteur** : le compteur rouge s'efface à l'ouverture de la cloche. Chaque élément garde son badge NOUVEAU / MAJ jusqu'à ce que vous le retiriez ou le regardiez.
*   **Temps réel** : les ajouts apparaissent dès que Jellyfin les annonce, sans recharger la page.
*   **Classé par date** : Aujourd'hui, Cette semaine, Plus ancien.
*   **Regroupement** : les épisodes arrivés ensemble forment une seule carte (`Saison 4 • Ép. 1-12`). Les pistes sont groupées par album.
*   **Mises à jour détectées** : un fichier remplacé revient avec un badge bleu **MAJ** qui précise ce qui a changé (Qualité, Codec ou Audio). Les sous-titres et les métadonnées ne déclenchent rien.
*   **Déplacements ignorés** : réorganiser vos bibliothèques n'annonce rien de nouveau.
*   **Rien d'incomplet** : un titre attend, jusqu'à 5 minutes, que Jellyfin l'ait identifié, avec son affiche.
*   **Filtres** : par catégorie ou par favoris.
*   **Retrait** : un élément ou un groupe, ou en glissant vers la gauche sur mobile. Retirer ne marque pas comme vu.
*   **Collections** : les collections surveillées signalent leurs nouveaux ajouts.
*   **Historique des suppressions** : un journal réservé aux admins.
*   **Synchronisé** : ce que vous lisez ou retirez sur un appareil l'est sur tous les autres.
*   **Permissions respectées** : chacun ne voit que les bibliothèques auxquelles il a accès.
*   **Bilingue** : français ou anglais, selon la langue de Jellyfin.
*   **Sur tous les écrans** : ordinateur, tablette et mobile. Sur TV, si l'appli charge le script de NotifySync (c'est le cas des TV Samsung installées avec [Apps2Samsung](https://github.com/Apps2Samsung/Apps2Samsung)), la télécommande la pilote : flèches pour parcourir, OK pour ouvrir ou retirer, Retour pour fermer.

## 📦 Installation

> [!IMPORTANT]
> **Prérequis :**
> * Jellyfin **12.x**
> * Jellyfin **10.11.x** (avec NotifySync 5.8.4.0)
> * L'extension **File Transformation**, ou une modification manuelle (voir [Sans File Transformation](#sans-file-transformation))

### Étapes
1.  Ouvrez **Tableau de bord → Extensions → Gérer les dépôts** et ajoutez ces deux dépôts :

    **NotifySync** (le même pour 12.x et 10.11.x) :
    ```
    https://raw.githubusercontent.com/peterdu1109/NotifySync/refs/heads/main/repository.json
    ```
    **File Transformation** (ajoute la cloche à Jellyfin et résiste à ses mises à jour) :
    ```
    https://www.iamparadox.dev/jellyfin/plugins/manifest.json
    ```
2.  Dans **Extensions**, filtre **Disponible**, installez **NotifySync** et **File Transformation**.
3.  Redémarrez Jellyfin, puis `Ctrl+F5` dans votre navigateur.
4.  Dans **Tableau de bord → Extensions → NotifySync**, cochez les bibliothèques à surveiller, cliquez sur **Sauvegarder** puis **Régénérer l'historique**. La cloche reste vide tant qu'aucune bibliothèque n'est cochée.

### Sans File Transformation

> [!WARNING]
> À refaire après chaque mise à jour de Jellyfin, qui écrase `index.html`.

**Linux**
```bash
sudo sed -i 's|</body>|    <script src="/NotifySync/client.js"></script>\n</body>|' /usr/share/jellyfin/web/index.html
```

**Docker**
```bash
docker exec jellyfin sed -i 's|</body>|    <script src="/NotifySync/client.js"></script>\n</body>|' /jellyfin/jellyfin-web/index.html
```

**Windows** : ouvrez ce fichier dans un éditeur lancé en administrateur :

```
C:\Program Files\Jellyfin\Server\jellyfin-web\index.html
```

et collez cette ligne juste avant `</body>` :

```html
<script src="/NotifySync/client.js"></script>
```

## ⚙️ Configuration

Allez dans **Tableau de bord → Extensions → NotifySync**.

| Paramètre | Défaut | Description |
|-----------|--------|-------------|
| **Quota par catégorie** | `10` | Nombre maximum de cartes affichées par catégorie (1 à 50). |
| **Bibliothèques surveillées** | *(aucune)* | Cochez les bibliothèques que vous souhaitez voir apparaître dans la cloche. |
| **Collections surveillées** | *(aucune)* | Collections (BoxSets) à surveiller pour les nouveaux ajouts. Scannées toutes les 15 minutes. |
| **Mappage des catégories** | *(vide)* | Renommez ou regroupez vos bibliothèques dans la cloche (ex. *"Mes Films"* → *"Films"*). Les flèches fixent l'ordre des filtres. |
| **Sélection manuelle (IDs ou noms)** | *(vide)* | IDs ou noms de bibliothèques pour les configurations avancées (Channels, XFusion). |
| **Activer le suivi des suppressions** | `activé` | Tient le journal des suppressions. Nécessaire pour détecter les remplacements Sonarr/Radarr (suppression puis réimport sous 7 jours). |
| **Rétention (jours)** | `30` | Nombre de jours de conservation du journal des suppressions (7 à 365). |
| **Régénérer l'historique** | | Scan complet, après un changement de bibliothèques ou de catégories. |
| **Scanner les collections** | | Scanne tout de suite les collections surveillées. |

## ❓ Dépannage

| Problème | Solution |
|----------|----------|
| **La cloche n'apparaît pas** | Vérifiez que **File Transformation** est installé. Redémarrez Jellyfin, puis `Ctrl+F5`. |
| **Pas de cloche et des erreurs `401 Unauthorized` dans la console du navigateur** | Votre serveur est en Jellyfin 12 avec l'ancienne version 5.x. Mettez NotifySync à jour en 6.x depuis **Extensions**, redémarrez Jellyfin, puis `Ctrl+F5`. |
| **La cloche a disparu après une mise à jour de Jellyfin** | Sans File Transformation, chaque mise à jour écrase la modification. Refaites-la, ou installez File Transformation. |
| **Contenu ou musique manquant, compteur faux** | Cochez la bibliothèque dans **Bibliothèques surveillées**, puis cliquez sur **Régénérer l'historique**. |
| **Un fichier remplacé apparaît en NOUVEAU au lieu de MAJ** | Cochez **Activer le suivi des suppressions**. Il est nécessaire quand un fichier est supprimé puis réimporté. |
| **Un fichier renommé affiche un faux badge MAJ** | Un simple renommage ne déclenche rien. Si un badge apparaît quand même, retirez-le : il ne reviendra pas. |
| **Une série déplacée apparaît en NOUVEAU** | Cochez **Activer le suivi des suppressions**. La détection ne marche que si le dossier garde son nom. |
| **Contenu non autorisé visible** | Le plugin suit les droits Jellyfin. Vérifiez les restrictions de l'utilisateur. |
| **Pas de cloche sur les pages d'administration** | C'est normal : la cloche n'apparaît que sur les pages des utilisateurs. |
| **Erreur 429** | Attendez 30 secondes entre deux clics sur **Régénérer l'historique**. |
| **Incompatible** | Utilisez le dépôt de l'[Installation](#-installation-1). Jellyfin 12.x demande NotifySync 6.x. |

## 🎨 Thème recommandé

NotifySync va bien avec [**ElegantFin Cinema Edition**](https://github.com/peterdu1109/ElegantFin), un thème Jellyfin inspiré des plateformes de streaming. Les deux fonctionnent ensemble sur PC, mobile et TV.

## 💬 Support

Ouvrez une issue sur [GitHub](https://github.com/peterdu1109/NotifySync/issues) en indiquant votre version de Jellyfin, celle de NotifySync, l'appli ou le navigateur utilisé, et le journal du serveur si possible. Les contributions et idées de fonctionnalités y sont aussi les bienvenues.

## 👥 Auteurs & Crédits

NotifySync est maintenu par :

- **[ElieMFR](https://github.com/ElieMFR)** : auteur et responsable du projet
- **[Peterdu1109](https://github.com/peterdu1109)** : co-mainteneur

### 🙏 Contributeurs

<p><a href="https://github.com/muisje" title="@muisje"><img src="https://github.com/muisje.png?size=96" width="48" height="48" alt="@muisje" /></a> <a href="https://github.com/Wernouxe" title="@Wernouxe"><img src="https://github.com/Wernouxe.png?size=96" width="48" height="48" alt="@Wernouxe" /></a> <a href="https://github.com/hstark16" title="@hstark16"><img src="https://github.com/hstark16.png?size=96" width="48" height="48" alt="@hstark16" /></a> <a href="https://github.com/Shillooh" title="@Shillooh"><img src="https://github.com/Shillooh.png?size=96" width="48" height="48" alt="@Shillooh" /></a></p>

Distribué sous [Licence MIT](./LICENSE).
