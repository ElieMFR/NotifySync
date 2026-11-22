using System;
using System.Collections.Generic;

namespace NotifySync
{
    /// <summary>
    /// When a track counts as a favorite through its artist. Kept free of Jellyfin types so the
    /// tests can run without a server.
    /// </summary>
    internal static class FavoriteRules
    {
        /// <summary>
        /// True when one of the track's artists is a favorite of the user.
        /// </summary>
        /// <param name="trackArtists">The track's artists and album artists.</param>
        /// <param name="favoriteArtists">The names of the artists the user marked as favorites.</param>
        /// <returns><c>true</c> when the track is a favorite through its artist.</returns>
        internal static bool IsByFavoriteArtist(IEnumerable<string>? trackArtists, ISet<string> favoriteArtists)
        {
            if (trackArtists == null || favoriteArtists.Count == 0)
            {
                return false;
            }

            foreach (var artist in trackArtists)
            {
                if (!string.IsNullOrWhiteSpace(artist) && favoriteArtists.Contains(artist))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Builds the lookup set of favorite artist names: names compare without regard to case.
        /// </summary>
        /// <param name="names">The favorite artists' names.</param>
        /// <returns>The set.</returns>
        internal static HashSet<string> NameSet(IEnumerable<string?> names)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in names)
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    set.Add(name);
                }
            }

            return set;
        }
    }
}
