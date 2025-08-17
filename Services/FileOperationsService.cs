using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualBasic.FileIO;
using Interaction = Microsoft.VisualBasic.Interaction;
using DamnSimpleFileManager;
using DamnSimpleFileManager.Windows;

namespace DamnSimpleFileManager.Services
{
    internal class FileOperationsService
    {
        public async Task Copy(FilePaneViewModel source, FilePaneViewModel dest, IEnumerable<FileSystemInfo> items, Window owner, IProgress<double> progress, CancellationToken token, bool confirm = true)
        {
            var selectedItems = items.Where(i => i is not ParentDirectoryInfo).ToList();
            if (selectedItems.Count == 0)
            {
                progress.Report(100);
                return;
            }

            long totalBytes = selectedItems.Sum(GetTotalSize);
            if (totalBytes == 0)
            {
                progress.Report(100);
                return;
            }

            long copied = 0;

            foreach (FileSystemInfo item in selectedItems)
            {
                token.ThrowIfCancellationRequested();
                string target = Path.Combine(dest.CurrentDir.FullName, item.Name);
                Logger.Log($"Copying '{item.FullName}' to '{target}'");
                try
                {
                    if (confirm && Settings.CopyConfirmation)
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

                    copied = await CopyItemAsync(item, target, progress, token, totalBytes, copied);
                    dest.LoadDirectory(dest.CurrentDir);
                }
                catch (OperationCanceledException)
                {
                    if (File.Exists(target))
                        File.Delete(target);
                    if (Directory.Exists(target))
                        Directory.Delete(target, true);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error copying '{item.FullName}' to '{target}'", ex);
                    MessageBox.Show(owner, Localization.Get("Error_Copy", ex.Message));
                }
            }
        }

        public async Task Move(FilePaneViewModel source, FilePaneViewModel dest, IEnumerable<FileSystemInfo> items, Window owner, IProgress<double> progress, CancellationToken token, bool confirm = true)
        {
            var selectedItems = items.Where(i => i is not ParentDirectoryInfo).ToList();
            if (selectedItems.Count == 0)
            {
                progress.Report(100);
                return;
            }

            long totalBytes = selectedItems.Sum(GetTotalSize);
            if (totalBytes == 0)
            {
                progress.Report(100);
                return;
            }

            long copied = 0;

            foreach (FileSystemInfo item in selectedItems)
            {
                token.ThrowIfCancellationRequested();
                string target = Path.Combine(dest.CurrentDir.FullName, item.Name);
                Logger.Log($"Moving '{item.FullName}' to '{target}'");
                try
                {
                    if (confirm && Settings.MoveConfirmation)
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

                    try
                    {
                        if (item is FileInfo)
                        {
                            File.Move(item.FullName, target, true);
                            copied += GetTotalSize(item);
                            progress.Report(copied * 100.0 / totalBytes);
                        }
                        else if (item is DirectoryInfo)
                        {
                            Directory.Move(item.FullName, target);
                            copied += GetTotalSize(item);
                            progress.Report(copied * 100.0 / totalBytes);
                        }
                    }
                    catch (IOException)
                    {
                        copied = await CopyItemAsync(item, target, progress, token, totalBytes, copied);
                        if (item is FileInfo)
                            File.Delete(item.FullName);
                        else if (item is DirectoryInfo)
                            Directory.Delete(item.FullName, true);
                    }

                    source.LoadDirectory(source.CurrentDir);
                    dest.LoadDirectory(dest.CurrentDir);
                }
                catch (OperationCanceledException)
                {
                    if (File.Exists(target))
                        File.Delete(target);
                    if (Directory.Exists(target))
                        Directory.Delete(target, true);
                    throw;
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

            var messageKey = Settings.RecycleBinDelete ? "Confirm_Delete_RecycleBin" : "Confirm_Delete";
            var result = MessageBox.Show(
                owner,
                Localization.Get(messageKey, selectedItems.Count),
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
                        if (Settings.RecycleBinDelete)
                            FileSystem.DeleteFile(item.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        else
                            File.Delete(item.FullName);
                    }
                    else if (item is DirectoryInfo)
                    {
                        if (Settings.RecycleBinDelete)
                            FileSystem.DeleteDirectory(item.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        else
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
            string path = pane.CurrentDir.FullName;
            var dialog = new InputDialog
            {
                Owner = owner,
                Title = $"{Localization.Get("Prompt_CreateFolder")} - {path}"
            };
            dialog.Message = $"{Localization.Get("Prompt_FolderName")}\n{path}";
            dialog.InputText = Localization.Get("Default_FolderName");
            if (dialog.ShowDialog() == true)
            {
                string name = dialog.InputText.Trim();
                if (!string.IsNullOrWhiteSpace(name) && ValidateName(name, owner))
                {
                    string target = Path.Combine(path, name);
                    if (Directory.Exists(target))
                    {
                        Logger.Log($"Folder already exists '{target}'");
                        MessageBox.Show(owner, Localization.Get("Error_FolderExists", target), Localization.Get("Error_FolderExists_Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (File.Exists(target))
                    {
                        Logger.Log($"File already exists '{target}'");
                        MessageBox.Show(owner, Localization.Get("Error_FileExists", target), Localization.Get("Error_FileExists_Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    Directory.CreateDirectory(target);
                    Logger.Log($"Created folder '{name}' in '{path}'");
                    pane.LoadDirectory(pane.CurrentDir);
                }
            }
        }

        public void CreateFile(FilePaneViewModel pane, Window owner)
        {
            string path = pane.CurrentDir.FullName;
            var dialog = new InputDialog
            {
                Owner = owner,
                Title = $"{Localization.Get("Prompt_CreateFile")} - {path}"
            };
            dialog.Message = $"{Localization.Get("Prompt_FileName")}\n{path}";
            dialog.InputText = Localization.Get("Default_FileName");
            if (dialog.ShowDialog() == true)
            {
                string name = dialog.InputText.Trim();
                if (!string.IsNullOrWhiteSpace(name) && ValidateName(name, owner))
                {
                    string target = Path.Combine(path, name);
                    if (File.Exists(target))
                    {
                        Logger.Log($"File already exists '{target}'");
                        MessageBox.Show(owner, Localization.Get("Error_FileExists", target), Localization.Get("Error_FileExists_Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (Directory.Exists(target))
                    {
                        Logger.Log($"Folder already exists '{target}'");
                        MessageBox.Show(owner, Localization.Get("Error_FolderExists", target), Localization.Get("Error_FolderExists_Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    File.Create(target).Close();
                    Logger.Log($"Created file '{name}' in '{path}'");
                    pane.LoadDirectory(pane.CurrentDir);
                }
            }
        }

        public void Rename(FilePaneViewModel pane, FileSystemInfo item, Window owner)
        {
            string path = pane.CurrentDir.FullName;
            string name = Interaction.InputBox(
                $"{Localization.Get("Prompt_Rename")}\n{path}",
                $"{Localization.Get("Prompt_Rename_Title")} - {path}",
                item.Name,
                (int)(owner.Left + (owner.ActualWidth - 300) / 2),
                (int)(owner.Top + (owner.ActualHeight - 150) / 2)).Trim();
            if (!string.IsNullOrWhiteSpace(name) && name != item.Name && ValidateName(name, owner))
            {
                try
                {
                    string target = Path.Combine(path, name);
                    if (item is FileInfo)
                        File.Move(item.FullName, target, true);
                    else if (item is DirectoryInfo)
                        Directory.Move(item.FullName, target);
                    Logger.Log($"Renamed '{item.FullName}' to '{target}'");
                    pane.LoadDirectory(pane.CurrentDir);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error renaming '{item.FullName}' to '{name}'", ex);
                    MessageBox.Show(owner, Localization.Get("Error_Rename", ex.Message));
                }
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

        private static long GetTotalSize(FileSystemInfo item)
        {
            if (item is FileInfo file)
                return file.Length;

            if (item is DirectoryInfo dir)
            {
                long size = 0;
                foreach (var child in dir.GetFileSystemInfos())
                {
                    size += GetTotalSize(child);
                }
                return size;
            }

            return 0;
        }

        private static async Task<long> CopyItemAsync(FileSystemInfo source, string destination, IProgress<double> progress, CancellationToken token, long totalBytes, long copied)
        {
            if (source is FileInfo file)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                try
                {
                    const int bufferSize = 81920;
                    await using var sourceStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                    await using var destStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, useAsync: true);
                    var buffer = new byte[bufferSize];
                    int bytesRead;
                    while ((bytesRead = await sourceStream.ReadAsync(buffer.AsMemory(0, buffer.Length), token)) > 0)
                    {
                        await destStream.WriteAsync(buffer.AsMemory(0, bytesRead), token);
                        copied += bytesRead;
                        progress.Report(copied * 100.0 / totalBytes);
                        token.ThrowIfCancellationRequested();
                    }
                }
                catch
                {
                    if (File.Exists(destination))
                        File.Delete(destination);
                    throw;
                }
            }
            else if (source is DirectoryInfo dir)
            {
                Directory.CreateDirectory(destination);
                foreach (var child in dir.GetFileSystemInfos())
                {
                    var childDest = Path.Combine(destination, child.Name);
                    copied = await CopyItemAsync(child, childDest, progress, token, totalBytes, copied);
                }
            }
            return copied;
        }
    }
}
