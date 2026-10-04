using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace NotifySync
{
    /// <summary>
    /// Reads and writes the settings file without ever losing it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Jellyfin reads every plugin's settings through one shared serializer, cached by type NAME.
    /// When two versions of the plugin run side by side, the second one gets the first one's
    /// serializer and throws <see cref="InvalidCastException"/>. BasePlugin then rewrites the file
    /// with the defaults, and that write fails as well, after emptying the file. Replayed on a
    /// test server on 2026-10-04: NotifySync.xml went from 710 bytes to its 38-byte XML header,
    /// every library, category and quota gone.
    /// </para>
    /// <para>
    /// Here each type has its own serializer, an unreadable file is never replaced by the
    /// defaults, and a write goes to a file next to it that is renamed in one step: if the
    /// write fails, the old file is still whole.
    /// </para>
    /// </remarks>
    public static class ConfigurationFile
    {
        private static readonly XmlWriterSettings WriterSettings = new ()
        {
            Indent = true,
            Encoding = new UTF8Encoding(false)
        };

        /// <summary>
        /// Reads the settings.
        /// </summary>
        /// <typeparam name="T">The settings type.</typeparam>
        /// <param name="path">The file.</param>
        /// <param name="problem">Why the file could not be read, or <c>null</c>.</param>
        /// <returns>The file's settings; the defaults when it is missing or unreadable.</returns>
        public static T Read<T>(string path, out string? problem)
            where T : class, new()
        {
            problem = null;

            if (!File.Exists(path))
            {
                return new T();
            }

            try
            {
                using var stream = File.OpenRead(path);
                using var reader = XmlReader.Create(stream);

                if (Serializer<T>.Instance.Deserialize(reader) is T value)
                {
                    return value;
                }

                problem = "empty file";
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or XmlException)
            {
                // The serializer wraps the reading error: its cause says more.
                problem = ex.InnerException?.Message ?? ex.Message;
            }

            return new T();
        }

        /// <summary>
        /// Writes the settings.
        /// </summary>
        /// <typeparam name="T">The settings type.</typeparam>
        /// <param name="path">The file.</param>
        /// <param name="value">The settings.</param>
        public static void Write<T>(string path, T value)
            where T : class, new()
        {
            ArgumentNullException.ThrowIfNull(value);

            Replace(path, stream =>
            {
                using var writer = XmlWriter.Create(stream, WriterSettings);
                Serializer<T>.Instance.Serialize(writer, value);
            });
        }

        /// <summary>
        /// Replaces a file in one step: the new content is written next to it first.
        /// </summary>
        /// <param name="path">The file.</param>
        /// <param name="write">Writes the new content.</param>
        public static void Replace(string path, Action<Stream> write)
        {
            ArgumentNullException.ThrowIfNull(write);

            var fullPath = Path.GetFullPath(path);
            var directory = Path.GetDirectoryName(fullPath) ?? throw new ArgumentException("Path without a folder.", nameof(path));
            Directory.CreateDirectory(directory);

            // In the same folder: the rename stays on the same disk, so it happens in one piece.
            var temporary = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";

            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    write(stream);
                    stream.Flush(flushToDisk: true);
                }

                File.Move(temporary, fullPath, overwrite: true);
            }
            finally
            {
                // After a successful rename there is nothing left at that name.
                TryDelete(temporary);
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // A leftover temporary file harms nothing: the next one gets another name.
            }
        }

        private static class Serializer<T>
        {
            // This type's own: Jellyfin's, cached by name, may belong to another version of the
            // plugin loaded alongside this one.
            internal static readonly XmlSerializer Instance = new (typeof(T));
        }
    }
}
