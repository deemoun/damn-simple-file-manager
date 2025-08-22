using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DamnSimpleFileManager.Utils
{
    public static class FileBookmarkStorage
    {
        private const string FileName = "file_bookmarks.json";
        private const long MaxFileSizeBytes = 1 * 1024 * 1024; // 1 MB limit

        public static List<FileBookmarkItem> Load()
        {
            try
            {
                if (!File.Exists(FileName)) return new List<FileBookmarkItem>();
                var fileInfo = new FileInfo(FileName);
                if (fileInfo.Length > MaxFileSizeBytes)
                {
                    Logger.LogError($"Bookmark file '{FileName}' exceeds maximum size", new IOException($"File size {fileInfo.Length} exceeds {MaxFileSizeBytes}"));
                    return new List<FileBookmarkItem>();
                }

                var json = File.ReadAllText(FileName);
                return JsonSerializer.Deserialize<List<FileBookmarkItem>>(json) ?? new List<FileBookmarkItem>();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to load '{FileName}'", ex);
                return new List<FileBookmarkItem>();
            }
        }

        public static void Save(List<FileBookmarkItem> bookmarks)
        {
            var json = JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileName, json);
        }
    }
}
