using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace DamnSimpleFileManager.Utils
{
    public static class AppData
    {
        public static readonly string DirectoryPath;

        static AppData()
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            DirectoryPath = Path.Combine(basePath, "DamnSimpleFileManager");
            EnsureDirectory();
        }

        private static void EnsureDirectory()
        {
            if (Directory.Exists(DirectoryPath)) return;

            var userSid = WindowsIdentity.GetCurrent().User!;
            var security = new DirectorySecurity();
            security.SetOwner(userSid);
            security.AddAccessRule(new FileSystemAccessRule(
                userSid,
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow));

            Directory.CreateDirectory(DirectoryPath, security);
        }

        public static string GetPath(string fileName)
        {
            var path = Path.Combine(DirectoryPath, fileName);
            if (!File.Exists(path))
            {
                using (File.Create(path)) { }
                var userSid = WindowsIdentity.GetCurrent().User!;
                var security = new FileSecurity();
                security.SetOwner(userSid);
                security.AddAccessRule(new FileSystemAccessRule(
                    userSid,
                    FileSystemRights.FullControl,
                    AccessControlType.Allow));
                File.SetAccessControl(path, security);
            }
            return path;
        }
    }
}

