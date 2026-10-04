namespace NotifySync
{
    /// <summary>
    /// One user's read and dismissed state for one notification, as the database stores it. Kept
    /// whole so it can follow the notification when its title moves to another library.
    /// </summary>
    public sealed class NotificationUserState
    {
        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the user read the notification.
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user dismissed the notification.
        /// </summary>
        public bool IsDismissed { get; set; }

        /// <summary>
        /// Gets or sets when the user read it, exactly as stored.
        /// </summary>
        public string? ReadAt { get; set; }

        /// <summary>
        /// Gets or sets when the user dismissed it, exactly as stored.
        /// </summary>
        public string? DismissedAt { get; set; }
    }
}
