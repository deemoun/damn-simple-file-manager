using System.IO;

namespace DamnSimpleFileManager
{
    internal class ParentDirectoryInfo : DirectoryInfo
    {
        public ParentDirectoryInfo(string path) : base(path)
        {
        }

        public new string Name => "..";
    }
}
