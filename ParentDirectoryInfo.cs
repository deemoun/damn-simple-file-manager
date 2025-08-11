using System.IO;

namespace DamnSimpleFileManager
{
    internal class ParentDirectoryInfo : FileSystemInfo
    {
        private readonly DirectoryInfo inner;

        public ParentDirectoryInfo(string path)
        {
            inner = new DirectoryInfo(path);
        }

        public override string Name => "..";
        public override bool Exists => inner.Exists;
        public override string FullName => inner.FullName;

        public override void Delete()
        {
            inner.Delete();
        }

    }
}
