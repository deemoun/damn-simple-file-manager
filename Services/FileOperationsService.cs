using System;
using System.IO;
using System.Linq;
using System.Windows;
using DamnSimpleFileManager;

namespace DamnSimpleFileManager.Services
{
    public class FileOperationsService
    {
        public void Copy(FilePane source, FilePane dest, Window owner)
        {
            foreach (FileSystemInfo item in source.List.SelectedItems.Cast<FileSystemInfo>().Where(i => i is not ParentDirectoryInfo).ToList())
            {
                string target = Path.Combine(dest.CurrentDir.FullName, item.Name);
                try
                {
                    if (Settings.CopyConfirmation)
                    {
                        var result = MessageBox.Show(
                            owner,
                            Localization.Get("Confirm_Copy", item.FullName, target),
                            Localization.Get("Confirm_Copy_Title"),
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);
                        if (result != MessageBoxResult.Yes)
                            continue;
                    }

                    if (item is FileInfo)
                    {
                        File.Copy(item.FullName, target, true);
                    }
                    else if (item is DirectoryInfo)
                    {
                        CopyDirectory(item.FullName, target);
                    }
                    dest.LoadDirectory(dest.CurrentDir);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(owner, Localization.Get("Error_Copy", ex.Message));
                }
            }
        }

        public void Move(FilePane source, FilePane dest, Window owner)
        {
            foreach (FileSystemInfo item in source.List.SelectedItems.Cast<FileSystemInfo>().Where(i => i is not ParentDirectoryInfo).ToList())
            {
                string target = Path.Combine(dest.CurrentDir.FullName, item.Name);
                try
                {
                    if (Settings.MoveConfirmation)
                    {
                        var result = MessageBox.Show(
                            owner,
                            Localization.Get("Confirm_Move", item.FullName, target),
                            Localization.Get("Confirm_Move_Title"),
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);
                        if (result != MessageBoxResult.Yes)
                            continue;
                    }

                    MoveWithFallback(item.FullName, target);
                    source.LoadDirectory(source.CurrentDir);
                    dest.LoadDirectory(dest.CurrentDir);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(owner, Localization.Get("Error_Move", ex.Message));
                }
            }
        }

        public void Delete(FilePane pane, Window owner)
        {
            var selectedItems = pane.List.SelectedItems.Cast<FileSystemInfo>().Where(i => i is not ParentDirectoryInfo).ToList();
            if (selectedItems.Count == 0)
                return;

            var result = MessageBox.Show(
                owner,
                Localization.Get("Confirm_Delete", selectedItems.Count),
                Localization.Get("Confirm_Delete_Title"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;

            foreach (var item in selectedItems)
            {
                try
                {
                    if (item is FileInfo)
                    {
                        File.Delete(item.FullName);
                    }
                    else if (item is DirectoryInfo)
                    {
                        Directory.Delete(item.FullName, true);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(owner, Localization.Get("Error_Delete", ex.Message));
                }
            }

            pane.LoadDirectory(pane.CurrentDir);
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);
            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(destinationDir, Path.GetFileName(file)), true);
            foreach (var directory in Directory.GetDirectories(sourceDir))
                CopyDirectory(directory, Path.Combine(destinationDir, Path.GetFileName(directory)));
        }

        private static void MoveWithFallback(string source, string destination)
        {
            try
            {
                if (File.Exists(source))
                {
                    File.Move(source, destination, true);
                }
                else if (Directory.Exists(source))
                {
                    if (Directory.Exists(destination))
                    {
                        Directory.Delete(destination, true);
                    }
                    Directory.Move(source, destination);
                }
                else
                {
                    throw new FileNotFoundException("Source does not exist", source);
                }
            }
            catch (IOException)
            {
                if (File.Exists(source))
                {
                    File.Copy(source, destination, true);
                    File.Delete(source);
                }
                else if (Directory.Exists(source))
                {
                    if (Directory.Exists(destination))
                    {
                        Directory.Delete(destination, true);
                    }
                    CopyDirectory(source, destination);
                    Directory.Delete(source, true);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
