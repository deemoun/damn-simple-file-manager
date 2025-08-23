using System;
using System.Collections.Generic;
using System.IO;

namespace DamnSimpleFileManager
{
    internal static class Settings
    {
        private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "dsfm.ini");
        private static readonly Dictionary<string, string> Values = new();
        private const long MaxConfigSizeBytes = 1 * 1024 * 1024;

        public static bool ShowHiddenFiles =>
            Values.TryGetValue("hidden_files", out var value) && bool.TryParse(value, out var result)
                ? result
                : true;

        public static bool CopyConfirmation =>
            Values.TryGetValue("copy_confirmation", out var value) && bool.TryParse(value, out var result)
                ? result
                : true;

        public static bool MoveConfirmation =>
            Values.TryGetValue("move_confirmation", out var value) && bool.TryParse(value, out var result)
                ? result
                : true;

        public static bool RecycleBinDelete =>
            Values.TryGetValue("recycle_bin_delete", out var value) && bool.TryParse(value, out var result)
                ? result
                : true;

        public static bool DarkTheme =>
            Values.TryGetValue("dark_theme", out var value) && bool.TryParse(value, out var result)
                ? result
                : false;

        public static bool StartSinglePane =>
            Values.TryGetValue("start_single_pane", out var value) && bool.TryParse(value, out var result)
                ? result
                : false;

        public static void Load()
        {
            var changed = false;
            Values.Clear();
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var info = new FileInfo(ConfigPath);
                    if (info.Length <= MaxConfigSizeBytes)
                    {
                        string[] lines = Array.Empty<string>();
                        try
                        {
                            lines = File.ReadAllLines(ConfigPath);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Error reading settings file", ex);
                        }

                        try
                        {
                            foreach (var line in lines)
                            {
                                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith(";"))
                                    continue;

                                var parts = line.Split('=', 2);
                                if (parts.Length != 2)
                                    continue;

                                var key = parts[0].Trim().ToLowerInvariant();
                                var val = parts[1].Trim();

                                switch (key)
                                {
                                    case "hidden_files":
                                    case "copy_confirmation":
                                    case "move_confirmation":
                                    case "recycle_bin_delete":
                                    case "dark_theme":
                                    case "start_single_pane":
                                        if (bool.TryParse(val, out var boolVal))
                                            Values[key] = boolVal.ToString().ToLowerInvariant();
                                        else
                                        {
                                            Logger.Log($"Invalid value for '{key}' in settings file");
                                            changed = true;
                                        }
                                        break;
                                    default:
                                        Logger.Log($"Unknown setting '{key}' ignored");
                                        changed = true;
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Error parsing settings file", ex);
                        }
                    }
                    else
                    {
                        Logger.Log($"Settings file '{ConfigPath}' exceeds maximum size");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading settings", ex);
            }

            if (!Values.ContainsKey("hidden_files"))
            {
                Values["hidden_files"] = "true";
                changed = true;
            }
            if (!Values.ContainsKey("copy_confirmation"))
            {
                Values["copy_confirmation"] = "true";
                changed = true;
            }
            if (!Values.ContainsKey("move_confirmation"))
            {
                Values["move_confirmation"] = "true";
                changed = true;
            }
            if (!Values.ContainsKey("recycle_bin_delete"))
            {
                Values["recycle_bin_delete"] = "true";
                changed = true;
            }
            if (!Values.ContainsKey("dark_theme"))
            {
                Values["dark_theme"] = "false";
                changed = true;
            }
            if (!Values.ContainsKey("start_single_pane"))
            {
                Values["start_single_pane"] = "false";
                changed = true;
            }
            if (changed)
                Save();
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

