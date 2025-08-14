using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DamnSimpleFileManager.Utils
{
    public static class LinkStorage
    {
        private const string FileName = "links.json";

        public static List<LinkItem> Load()
        {
            if (!File.Exists(FileName)) return new List<LinkItem>();
            var json = File.ReadAllText(FileName);
            return JsonSerializer.Deserialize<List<LinkItem>>(json) ?? new List<LinkItem>();
        }

        public static void Save(List<LinkItem> links)
        {
            var json = JsonSerializer.Serialize(links, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileName, json);
        }
    }
}
