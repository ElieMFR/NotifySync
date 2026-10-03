using System;

namespace NotifySync
{
    /// <summary>
    /// The text of the dashboard's plugin page, in the server's language.
    /// </summary>
    /// <remarks>
    /// Jellyfin shows <c>Plugin.Description</c> on a loaded plugin's page. Before the restart that
    /// follows an install, and in the catalog, it shows the catalog's description instead: a fixed
    /// file read by every server, so it carries both texts (<c>repository-12.json</c>,
    /// <c>repository-12-beta.json</c>) and must say the same thing.
    /// </remarks>
    internal static class PluginDescription
    {
        /// <summary>The French text.</summary>
        internal const string French =
            "Ajoute une cloche de notifications à Jellyfin : les derniers films, séries et musiques ajoutés, sans quitter la page.";

        /// <summary>The English text.</summary>
        internal const string English =
            "Adds a notification bell to Jellyfin: the latest movies, series and music added, without leaving the page.";

        /// <summary>
        /// Picks the text from the server's display language.
        /// </summary>
        /// <param name="uiCulture">The server's language, for example "fr-FR", or <c>null</c>.</param>
        /// <returns>French for any variant of French, English for any other language.</returns>
        internal static string For(string? uiCulture)
        {
            var language = (uiCulture ?? string.Empty).Split('-', '_')[0];
            return string.Equals(language, "fr", StringComparison.OrdinalIgnoreCase) ? French : English;
        }
    }
}
