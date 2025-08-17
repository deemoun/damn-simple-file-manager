using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using DamnSimpleFileManager;

namespace DamnSimpleFileManager.Utils
{
    public static class LinkStorage
    {
        private const string FileName = "links.json";
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        public static List<LinkItem> Load()
        {
            if (!File.Exists(FileName)) return new List<LinkItem>();
            try
            {
                var fileInfo = new FileInfo(FileName);
                if (fileInfo.Length > MaxFileSizeBytes)
                {
                    Logger.Log($"Links file '{FileName}' exceeds max size; returning empty list.");
                    return new List<LinkItem>();
                }

                var json = File.ReadAllText(FileName);
                return JsonSerializer.Deserialize<List<LinkItem>>(json) ?? new List<LinkItem>();
            }
            catch (IOException ex)
            {
                Logger.LogError($"Failed to read links file '{FileName}'", ex);
            }
            catch (JsonException ex)
            {
                Logger.LogError($"Failed to deserialize links file '{FileName}'", ex);
            }
            return new List<LinkItem>();
        }

        public static void Save(List<LinkItem> links)
        {
            var json = JsonSerializer.Serialize(links, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileName, json);
        }
    }
}
