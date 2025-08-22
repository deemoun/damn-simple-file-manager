using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DamnSimpleFileManager.Utils
{
    public static class LinkStorage
    {
        private const string FileName = "links.json";
        private const long MaxFileSizeBytes = 1 * 1024 * 1024; // 1 MB limit

        public static List<LinkItem> Load()
        {
            try
            {
                if (!File.Exists(FileName)) return new List<LinkItem>();
                var fileInfo = new FileInfo(FileName);
                if (fileInfo.Length > MaxFileSizeBytes)
                {
                    Logger.LogError($"Link file '{FileName}' exceeds maximum size", new IOException($"File size {fileInfo.Length} exceeds {MaxFileSizeBytes}"));
                    return new List<LinkItem>();
                }

                var json = File.ReadAllText(FileName);
                return JsonSerializer.Deserialize<List<LinkItem>>(json) ?? new List<LinkItem>();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to load '{FileName}'", ex);
                return new List<LinkItem>();
            }
        }

        public static void Save(List<LinkItem> links)
        {
            var json = JsonSerializer.Serialize(links, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileName, json);
        }
    }
}
