using System;
using System.Collections.Generic;
using System.IO;

namespace DamnSimpleFileManager
{
    internal static class Settings
    {
        private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "dsfm.ini");
        private static readonly Dictionary<string, string> Values = new();

        public static bool ShowHiddenFiles =>
            Values.TryGetValue("hidden_files", out var value)
                ? value.Equals("true", StringComparison.OrdinalIgnoreCase)
                : true;

        public static void Load()
        {
            Values.Clear();
            if (File.Exists(ConfigPath))
            {
                foreach (var line in File.ReadAllLines(ConfigPath))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith(";"))
                        continue;
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                        Values[parts[0].Trim().ToLowerInvariant()] = parts[1].Trim();
                }
            }

            if (!Values.ContainsKey("hidden_files"))
            {
                Values["hidden_files"] = "true";
                Save();
            }
        }

        public static void Save()
        {
            var lines = new List<string>();
            foreach (var kvp in Values)
            {
                lines.Add($"{kvp.Key}={kvp.Value}");
            }
            File.WriteAllLines(ConfigPath, lines);
        }

        public static void Set(string key, string value)
        {
            Values[key] = value;
            Save();
        }

        public static string Get(string key, string defaultValue = "") =>
            Values.TryGetValue(key, out var value) ? value : defaultValue;
    }
}

