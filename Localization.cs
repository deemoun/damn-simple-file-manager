using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DamnSimpleFileManager
{
    internal static class Localization
    {
        private static readonly Dictionary<string, string> _strings = new();
        private static readonly HashSet<string> _availableLanguages = LoadAvailableLanguages();

        public static void LoadLanguage(string name)
        {
            _strings.Clear();
            if (!TryLoadLanguage(name))
            {
                TryLoadLanguage("English");
            }
        }

        public static void LoadSystemLanguage()
        {
            var language = CultureInfo.CurrentUICulture.EnglishName.Split('(')[0].Trim();
            LoadLanguage(language);
        }

        private static bool TryLoadLanguage(string name)
        {
            var safeName = Path.GetFileNameWithoutExtension(name);
            if (safeName.IndexOf(Path.DirectorySeparatorChar) >= 0 ||
                safeName.IndexOf(Path.AltDirectorySeparatorChar) >= 0)
                return false;

            if (!_availableLanguages.Contains(safeName))
                return false;

            var baseDir = AppContext.BaseDirectory;
            var path = Path.Combine(baseDir, "Languages", $"{safeName}.lang");
            if (!File.Exists(path))
                return false;

            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;
                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    _strings[parts[0].Trim()] = parts[1].Trim();
                }
            }

            return true;
        }

        public static string Get(string key) => _strings.TryGetValue(key, out var value) ? value : key;

        public static string Get(string key, params object[] args) => string.Format(Get(key), args);

        private static HashSet<string> LoadAvailableLanguages()
        {
            var baseDir = AppContext.BaseDirectory;
            var dir = Path.Combine(baseDir, "Languages");
            if (!Directory.Exists(dir))
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            return Directory.GetFiles(dir, "*.lang")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}
