namespace DamnSimpleFileManager.Utils
{
    public class FileBookmarkItem
    {
        public string Path { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Available => System.IO.Directory.Exists(Path) || System.IO.File.Exists(Path)
            ? Localization.Get("General_Yes")
            : Localization.Get("General_No");
    }
}
