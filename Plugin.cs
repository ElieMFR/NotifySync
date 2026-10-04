using System;
using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Configuration;
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
        private readonly IServerConfigurationManager _serverConfiguration;
        private readonly ILogger<Plugin> _logger;
        private readonly object _saveLock = new ();
        private NotificationManager? _notificationManager;

        /// <summary>
        /// False while the settings file on disk could not be read: the settings in memory are
        /// then the defaults, and saving them would replace the real ones.
        /// </summary>
        private volatile bool _configurationRead;

        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">The application paths.</param>
        /// <param name="xmlSerializer">The XML serializer.</param>
        /// <param name="libraryManager">The library manager.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="userDataManager">The user data manager.</param>
        /// <param name="serverConfiguration">The server configuration, for its display language.</param>
        public Plugin(
            IApplicationPaths applicationPaths,
            IXmlSerializer xmlSerializer,
            ILibraryManager libraryManager,
            ILoggerFactory loggerFactory,
            IUserDataManager userDataManager,
            IServerConfigurationManager serverConfiguration)
            : base(applicationPaths, xmlSerializer)
        {
            _applicationPaths = applicationPaths;
            _serverConfiguration = serverConfiguration;
            _logger = loggerFactory.CreateLogger<Plugin>();

            // Read here, by our own serializer, so BasePlugin never loads the settings itself:
            // its loader replaces any file it cannot read with the defaults, and empties it in
            // the attempt. See ConfigurationFile.
            Configuration = ConfigurationFile.Read<PluginConfiguration>(ConfigurationFilePath, out var problem);
            _configurationRead = problem is null;
            if (problem is not null)
            {
                _logger.LogWarning(
                    "NotifySync: settings unreadable ({Problem}) — running on the defaults until they are saved again; {Path} is left as it is.",
                    problem,
                    ConfigurationFilePath);
            }

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
        /// French on a French server, English otherwise: see <see cref="PluginDescription"/>. Read on
        /// every display, so it follows a language change without a restart.
        /// </remarks>
        public override string Description => PluginDescription.For(_serverConfiguration?.Configuration?.UICulture);

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

        /// <summary>
        /// Saves the settings through our own serializer, in one step (see <see cref="ConfigurationFile"/>).
        /// While the file on disk could not be read, only a save from the settings page goes through:
        /// any other would replace the real settings with the defaults.
        /// </summary>
        /// <param name="config">The settings.</param>
        public override void SaveConfiguration(PluginConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            lock (_saveLock)
            {
                if (!_configurationRead)
                {
                    _logger.LogWarning("NotifySync: settings unreadable — {Path} is not overwritten.", ConfigurationFilePath);
                    return;
                }

                ConfigurationFile.Write(ConfigurationFilePath, config);
            }
        }

        /// <inheritdoc />
        public override void UpdateConfiguration(BasePluginConfiguration configuration)
        {
            // Saved from the settings page: from now on these are the real settings, even if the
            // file could not be read at startup.
            _configurationRead = true;
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
