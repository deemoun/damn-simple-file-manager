using System.Collections.Generic;
using System.Linq;

namespace DamnSimpleFileManager.Utils
{
    public class FileBookmarkManager
    {
        private List<FileBookmarkItem> bookmarks = new();

        public FileBookmarkManager()
        {
            bookmarks = FileBookmarkStorage.Load();
        }

        public bool AddBookmark(string path, string? description)
        {
            var input = path.Trim();
            if (string.IsNullOrWhiteSpace(input)) return false;
            bookmarks.Add(new FileBookmarkItem { Path = input, Description = description });
            FileBookmarkStorage.Save(bookmarks);
            return true;
        }

        public void DeleteBookmark(string path)
        {
            var item = bookmarks.FirstOrDefault(b => b.Path == path);
            if (item != null)
            {
                bookmarks.Remove(item);
                FileBookmarkStorage.Save(bookmarks);
            }
        }

        public List<FileBookmarkItem> GetAllBookmarks() => bookmarks;
    }
}
