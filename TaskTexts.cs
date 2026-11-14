using System;

namespace NotifySync
{
    /// <summary>
    /// Names and descriptions of the scheduled tasks, in the server's display language: French,
    /// Portuguese, English for any other.
    /// </summary>
    /// <remarks>
    /// Kept free of Jellyfin types so the tests can run without a server. The dashboard reads
    /// these from the server on each display; whether it shows them per user or per server
    /// language is not verified.
    /// </remarks>
    internal static class TaskTexts
    {
        /// <summary>The scheduled tasks that carry a text.</summary>
        internal enum Task
        {
            /// <summary>The monitored collections scan.</summary>
            CollectionScan,

            /// <summary>The initial history scan.</summary>
            HistoryScan,

            /// <summary>The database compaction.</summary>
            DatabaseMaintenance,
        }

        /// <summary>
        /// Picks the language from the server's display language.
        /// </summary>
        /// <param name="uiCulture">The server's language, for example "pt-BR", or <c>null</c>.</param>
        /// <returns><c>fr</c>, <c>pt</c> or <c>en</c>.</returns>
        internal static string Language(string? uiCulture)
        {
            var language = (uiCulture ?? string.Empty).Split('-', '_')[0].ToLowerInvariant();
            return language == "fr" || language == "pt" ? language : "en";
        }

        /// <summary>
        /// The task's name.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="uiCulture">The server's language.</param>
        /// <returns>The name.</returns>
        internal static string Name(Task task, string? uiCulture) => (task, Language(uiCulture)) switch
        {
            (Task.CollectionScan, "fr") => "NotifySync : analyse des collections",
            (Task.CollectionScan, "pt") => "NotifySync: verificação das coleções",
            (Task.CollectionScan, _) => "NotifySync: Collection Scan",
            (Task.HistoryScan, "fr") => "NotifySync : analyse de l'historique",
            (Task.HistoryScan, "pt") => "NotifySync: verificação do histórico",
            (Task.HistoryScan, _) => "NotifySync: History Scan",
            (Task.DatabaseMaintenance, "fr") => "NotifySync : maintenance de la base",
            (Task.DatabaseMaintenance, "pt") => "NotifySync: manutenção do banco de dados",
            _ => "NotifySync: Database Maintenance",
        };

        /// <summary>
        /// The task's description.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="uiCulture">The server's language.</param>
        /// <returns>The description.</returns>
        internal static string Description(Task task, string? uiCulture) => (task, Language(uiCulture)) switch
        {
            (Task.CollectionScan, "fr") => "Analyse les collections surveillées pour détecter les nouveaux ajouts et créer des notifications.",
            (Task.CollectionScan, "pt") => "Verifica as coleções monitoradas para detectar novas adições e criar notificações.",
            (Task.CollectionScan, _) => "Scans the monitored collections for new additions and creates notifications.",
            (Task.HistoryScan, "fr") => "Analyse la bibliothèque pour générer les notifications initiales, seulement tant qu'aucune notification n'est enregistrée. Pour tout reconstruire, utilisez « Régénérer l'historique » dans les paramètres du plugin.",
            (Task.HistoryScan, "pt") => "Verifica a biblioteca para gerar as notificações iniciais, somente enquanto nenhuma notificação estiver registrada. Para recriar tudo, use \"Regenerar histórico\" nas configurações do plugin.",
            (Task.HistoryScan, _) => "Scans the library to generate the initial notifications, only while none is stored. To rebuild everything, use \"Regenerate history\" in the plugin settings.",
            (Task.DatabaseMaintenance, "fr") => "Compacte la base de données NotifySync (VACUUM) pour récupérer l'espace libéré par les ajouts et les suppressions.",
            (Task.DatabaseMaintenance, "pt") => "Compacta o banco de dados do NotifySync (VACUUM) para recuperar o espaço liberado por adições e remoções.",
            _ => "Compacts the NotifySync database (VACUUM) to reclaim the space freed by additions and removals.",
        };
    }
}
