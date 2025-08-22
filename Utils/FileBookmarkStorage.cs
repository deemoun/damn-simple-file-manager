using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DamnSimpleFileManager.Utils
{
    public static class FileBookmarkStorage
    {
        private const string FileName = "file_bookmarks.json";

        public static List<FileBookmarkItem> Load()
        {
            if (!File.Exists(FileName)) return new List<FileBookmarkItem>();
            var json = File.ReadAllText(FileName);
            return JsonSerializer.Deserialize<List<FileBookmarkItem>>(json) ?? new List<FileBookmarkItem>();
        }

        public static void Save(List<FileBookmarkItem> bookmarks)
        {
            var json = JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileName, json);
        }
    }
}
