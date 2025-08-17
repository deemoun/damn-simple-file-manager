using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DamnSimpleFileManager.Utils
{
    public static class LinkStorage
    {
        private static readonly string FileName = AppData.GetPath("links.json");

        public static List<LinkItem> Load()
        {
            var json = File.ReadAllText(FileName);
            if (string.IsNullOrWhiteSpace(json)) return new List<LinkItem>();
            return JsonSerializer.Deserialize<List<LinkItem>>(json) ?? new List<LinkItem>();
        }

        public static void Save(List<LinkItem> links)
        {
            var json = JsonSerializer.Serialize(links, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileName, json);
        }
    }
}
