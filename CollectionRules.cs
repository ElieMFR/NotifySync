using System;
using System.Collections.Generic;
using System.Globalization;

namespace NotifySync
{
    /// <summary>
    /// What a collection notification ("col:{collectionId}:{itemId}") says about its collection.
    /// Kept free of Jellyfin types so the tests can run without a server.
    /// </summary>
    internal static class CollectionRules
    {
        /// <summary>
        /// True for a collection notification whose collection is among the given ones.
        /// </summary>
        /// <param name="notificationId">The notification id.</param>
        /// <param name="monitored">The collections monitored now.</param>
        /// <returns><c>true</c> when its collection is monitored.</returns>
        internal static bool IsFromMonitoredCollection(string notificationId, HashSet<Guid> monitored)
        {
            var parts = notificationId.Split(':');
            return parts.Length >= 3 && Guid.TryParse(parts[1], out var collectionId) && monitored.Contains(collectionId);
        }

        /// <summary>
        /// True for a collection notification whose collection is no longer monitored: the card
        /// and its tab stay out of the bell, whatever the database still holds. Any other
        /// notification, a library's, is never hidden by this rule.
        /// </summary>
        /// <param name="notificationId">The notification id.</param>
        /// <param name="monitored">The collections monitored now.</param>
        /// <returns><c>true</c> when the notification must not be shown.</returns>
        internal static bool IsFromUnmonitoredCollection(string notificationId, HashSet<Guid> monitored)
            => notificationId.StartsWith("col:", StringComparison.Ordinal)
                && !IsFromMonitoredCollection(notificationId, monitored);

        /// <summary>
        /// True for a notification created for the given collection.
        /// </summary>
        /// <param name="notificationId">The notification id.</param>
        /// <param name="collectionId">The collection.</param>
        /// <returns><c>true</c> when the notification belongs to that collection.</returns>
        internal static bool BelongsTo(string notificationId, Guid collectionId)
            => notificationId.StartsWith(
                string.Create(CultureInfo.InvariantCulture, $"col:{collectionId:N}:"),
                StringComparison.Ordinal);

        /// <summary>
        /// The collections the settings monitor, as ids.
        /// </summary>
        /// <param name="enabledCollections">The ids as stored in the settings.</param>
        /// <returns>The ids that parse.</returns>
        internal static HashSet<Guid> MonitoredSet(IEnumerable<string>? enabledCollections)
        {
            var set = new HashSet<Guid>();
            foreach (var id in enabledCollections ?? Array.Empty<string>())
            {
                if (Guid.TryParse(id, out var g))
                {
                    set.Add(g);
                }
            }

            return set;
        }
    }
}
