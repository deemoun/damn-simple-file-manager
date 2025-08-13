using System;
using System.IO;

namespace DamnSimpleFileManager
{
    internal static class Logger
    {
        private static readonly object _lock = new();
        private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "dsfm.log");

        public static void Log(string message)
        {
            Write("INFO", message);
        }

        public static void LogError(string message, Exception ex)
        {
            Write("ERROR", $"{message}: {ex}");
        }

        private static void Write(string level, string message)
        {
            try
            {
                var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}";
                lock (_lock)
                {
                    File.AppendAllText(LogPath, line);
                }
            }
            catch
            {
                // Intentionally ignore logging failures
            }
        }
    }
}

