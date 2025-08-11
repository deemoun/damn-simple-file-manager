using System;
using System.IO;
using System.Runtime.Serialization;

namespace DamnSimpleFileManager
{
    [Serializable]
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
        public override string Extension => inner.Extension;

        public override FileAttributes Attributes
        {
            get => inner.Attributes;
            set => inner.Attributes = value;
        }

        public override DateTime CreationTime
        {
            get => inner.CreationTime;
            set => inner.CreationTime = value;
        }

        public override DateTime CreationTimeUtc
        {
            get => inner.CreationTimeUtc;
            set => inner.CreationTimeUtc = value;
        }

        public override DateTime LastAccessTime
        {
            get => inner.LastAccessTime;
            set => inner.LastAccessTime = value;
        }

        public override DateTime LastAccessTimeUtc
        {
            get => inner.LastAccessTimeUtc;
            set => inner.LastAccessTimeUtc = value;
        }

        public override DateTime LastWriteTime
        {
            get => inner.LastWriteTime;
            set => inner.LastWriteTime = value;
        }

        public override DateTime LastWriteTimeUtc
        {
            get => inner.LastWriteTimeUtc;
            set => inner.LastWriteTimeUtc = value;
        }

        public override void Delete()
        {
            inner.Delete();
        }

        public override void Refresh()
        {
            inner.Refresh();
        }

        protected override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Let the base serialize its state
            base.GetObjectData(info, context);
            // Persist the path so this can be reconstructed if deserialized
            info.AddValue("ParentDirectoryInnerFullName", inner.FullName);
        }
    }
}