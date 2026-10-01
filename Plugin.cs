using System;
using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace NotifySync
{
    /// <summary>
    /// The main plugin class for NotifySync.
    /// </summary>
    public sealed class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages, IDisposable
    {
        private readonly IApplicationPaths _applicationPaths;
        private NotificationManager? _notificationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">The application paths.</param>
        /// <param name="xmlSerializer">The XML serializer.</param>
        /// <param name="libraryManager">The library manager.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="userDataManager">The user data manager.</param>
        public Plugin(
            IApplicationPaths applicationPaths,
            IXmlSerializer xmlSerializer,
            ILibraryManager libraryManager,
            ILoggerFactory loggerFactory,
            IUserDataManager userDataManager)
            : base(applicationPaths, xmlSerializer)
        {
            _applicationPaths = applicationPaths;
            Instance = this;
            _notificationManager = new NotificationManager(libraryManager, loggerFactory.CreateLogger<NotificationManager>(), userDataManager);
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static Plugin? Instance { get; private set; }

        /// <inheritdoc />
        public override string Name => "NotifySync";

        /// <inheritdoc />
        public override Guid Id => Guid.Parse("95655672-2342-4321-8291-321312312312");

        /// <inheritdoc />
        /// <remarks>
        /// The dashboard's plugin page shows this first and falls back to the catalog's text,
        /// which describes a release channel, not the plugin.
        /// </remarks>
        public override string Description =>
            "Ajoute une cloche de notifications à Jellyfin : les derniers films, séries et musiques ajoutés, sans quitter la page.";

        /// <summary>
        /// Gets the notification manager.
        /// </summary>
        public NotificationManager? NotificationManager => _notificationManager;

        /// <summary>
        /// Gets the permanent data folder path for this plugin.
        /// </summary>
        public string PluginDataFolderPath
        {
            get
            {
                // DataPath points to the persistent data folder (e.g., C:/ProgramData/Jellyfin/Server/data)
                return Path.Combine(_applicationPaths.DataPath, "NotifySync");
            }
        }

        /// <inheritdoc />
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "NotifySync",
                    DisplayName = "NotifySync",
                    EmbeddedResourcePath = GetType().Namespace + ".ConfigurationPage.html",
                    MenuIcon = "notifications",
                    EnableInMainMenu = true
                }
            };
        }

        /// <inheritdoc />
        public override void UpdateConfiguration(BasePluginConfiguration configuration)
        {
            base.UpdateConfiguration(configuration);

            // A save changes what the bell shows (quota, filter order) without touching a single
            // notification — tell the manager, or the cached views would hide it until the
            // library next changed.
            _notificationManager?.OnConfigurationChanged();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _notificationManager?.Dispose();
            _notificationManager = null;
            GC.SuppressFinalize(this);
        }
    }
}
