using System;
using System.Security.Claims;

namespace NotifySync
{
    /// <summary>
    /// Outcome of <see cref="AccessRules.CheckUserAccess"/>.
    /// </summary>
    internal enum UserAccess
    {
        /// <summary>The caller asks for its own data, or is an administrator.</summary>
        Allowed,

        /// <summary>The request carries no user name or id.</summary>
        NoIdentity,

        /// <summary>The caller's name matches no Jellyfin account.</summary>
        UnknownUser,

        /// <summary>The caller asks for another account's data.</summary>
        Denied,
    }

    /// <summary>
    /// Who may read or change whose bell. Kept free of Jellyfin types so the tests can run
    /// without a server; the controller supplies the one lookup that needs one.
    /// </summary>
    internal static class AccessRules
    {
        /// <summary>
        /// True for a Jellyfin administrator. An API key also carries the Administrator role
        /// and its own claim, so it passes the admin-only routes.
        /// </summary>
        /// <param name="user">The request principal.</param>
        /// <returns><c>true</c> when the caller may use the admin routes.</returns>
        internal static bool IsAdministrator(ClaimsPrincipal user)
            => user.IsInRole("Administrator")
                || string.Equals(user.FindFirst("Jellyfin-IsApiKey")?.Value, "true", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Whether the caller may read or change the bell of <paramref name="requestedUserId"/>:
        /// its own, or any as an administrator. The caller is found by name first, and an API
        /// key, which Jellyfin gives an empty name, is refused before the admin check: API keys
        /// are not supported on these routes.
        /// </summary>
        /// <param name="user">The request principal.</param>
        /// <param name="requestedUserId">The account whose bell is asked for.</param>
        /// <param name="userIdByName">Finds an account id from its name, or null.</param>
        /// <param name="currentUserId">The caller's id as resolved, for the log.</param>
        /// <returns>The decision.</returns>
        internal static UserAccess CheckUserAccess(
            ClaimsPrincipal user,
            string requestedUserId,
            Func<string, Guid?> userIdByName,
            out string currentUserId)
        {
            currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            if (string.IsNullOrEmpty(currentUserId))
            {
                currentUserId = user.FindFirst("UserId")?.Value ?? user.Identity?.Name ?? string.Empty;
            }

            if (string.IsNullOrEmpty(currentUserId))
            {
                return UserAccess.NoIdentity;
            }

            if (!Guid.TryParse(currentUserId, out _))
            {
                var byName = userIdByName(currentUserId);
                if (byName == null)
                {
                    return UserAccess.UnknownUser;
                }

                currentUserId = byName.Value.ToString("N");
            }

            if (IdHelper.NormalizeId(currentUserId) == IdHelper.NormalizeId(requestedUserId))
            {
                return UserAccess.Allowed;
            }

            return IsAdministrator(user) ? UserAccess.Allowed : UserAccess.Denied;
        }
    }
}
