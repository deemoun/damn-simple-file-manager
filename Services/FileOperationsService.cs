using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.VisualBasic;
using DamnSimpleFileManager;

namespace DamnSimpleFileManager.Services
{
    internal class FileOperationsService
    {
        public void Copy(FilePaneViewModel source, FilePaneViewModel dest, IEnumerable<FileSystemInfo> items, Window owner)
        {
            foreach (FileSystemInfo item in items.Where(i => i is not ParentDirectoryInfo))
            {
                string target = Path.Combine(dest.CurrentDir.FullName, item.Name);
                Logger.Log($"Copying '{item.FullName}' to '{target}'");
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
                    Logger.LogError($"Error copying '{item.FullName}' to '{target}'", ex);
                    MessageBox.Show(owner, Localization.Get("Error_Copy", ex.Message));
                }
            }
        }

        public void Move(FilePaneViewModel source, FilePaneViewModel dest, IEnumerable<FileSystemInfo> items, Window owner)
        {
            foreach (FileSystemInfo item in items.Where(i => i is not ParentDirectoryInfo))
            {
                string target = Path.Combine(dest.CurrentDir.FullName, item.Name);
                Logger.Log($"Moving '{item.FullName}' to '{target}'");
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
                    Logger.LogError($"Error moving '{item.FullName}' to '{target}'", ex);
                    MessageBox.Show(owner, Localization.Get("Error_Move", ex.Message));
                }
            }
        }

        public void Delete(FilePaneViewModel pane, IEnumerable<FileSystemInfo> items, Window owner)
        {
            var selectedItems = items.Where(i => i is not ParentDirectoryInfo).ToList();
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
                Logger.Log($"Deleting '{item.FullName}'");
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
                    Logger.LogError($"Error deleting '{item.FullName}'", ex);
                    MessageBox.Show(owner, Localization.Get("Error_Delete", ex.Message));
                }
            }

            pane.LoadDirectory(pane.CurrentDir);
        }

        public void CreateFolder(FilePaneViewModel pane, Window owner)
        {
            string name = Interaction.InputBox(
                Localization.Get("Prompt_FolderName"),
                Localization.Get("Prompt_CreateFolder"),
                Localization.Get("Default_FolderName"),
                (int)(owner.Left + (owner.ActualWidth - 300) / 2),
                (int)(owner.Top + (owner.ActualHeight - 150) / 2)).Trim();
            if (!string.IsNullOrWhiteSpace(name) && ValidateName(name, owner))
            {
                Directory.CreateDirectory(Path.Combine(pane.CurrentDir.FullName, name));
                Logger.Log($"Created folder '{name}' in '{pane.CurrentDir.FullName}'");
                pane.LoadDirectory(pane.CurrentDir);
            }
        }

        public void CreateFile(FilePaneViewModel pane, Window owner)
        {
            string name = Interaction.InputBox(
                Localization.Get("Prompt_FileName"),
                Localization.Get("Prompt_CreateFile"),
                Localization.Get("Default_FileName"),
                (int)(owner.Left + (owner.ActualWidth - 300) / 2),
                (int)(owner.Top + (owner.ActualHeight - 150) / 2)).Trim();
            if (!string.IsNullOrWhiteSpace(name) && ValidateName(name, owner))
            {
                File.Create(Path.Combine(pane.CurrentDir.FullName, name)).Close();
                Logger.Log($"Created file '{name}' in '{pane.CurrentDir.FullName}'");
                pane.LoadDirectory(pane.CurrentDir);
            }
        }

        private static bool ValidateName(string name, Window owner)
        {
            if (Path.IsPathRooted(name) || name.Contains("..") || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show(owner, Localization.Get("Error_InvalidName"), Localization.Get("Error_InvalidName_Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
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
