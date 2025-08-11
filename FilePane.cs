using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DamnSimpleFileManager
{
    internal class FilePane
    {
        public ListView List { get; }
        public TextBlock PathText { get; }
        public ComboBox DriveSelector { get; }
        public Button BackButton { get; }
        public TextBlock SpaceText { get; }

        public DirectoryInfo CurrentDir { get; private set; } = null!;
        private readonly Stack<DirectoryInfo> history = new();
        private FileSystemWatcher? watcher;

        public FilePane(ListView list, TextBlock pathText, ComboBox driveSelector, Button backButton, TextBlock spaceText)
        {
            List = list;
            PathText = pathText;
            DriveSelector = driveSelector;
            BackButton = backButton;
            SpaceText = spaceText;
        }

        public void PopulateDrives()
        {
            DriveSelector.Items.Clear();
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    DriveSelector.Items.Add(drive.Name);
                }
            }

            if (DriveSelector.Items.Count > 0)
            {
                DriveSelector.SelectedIndex = 0;
                CurrentDir = new DirectoryInfo(DriveSelector.SelectedItem!.ToString()!);
                LoadDirectory(CurrentDir);
            }

            UpdateBackButton();
        }

        public void SetDrive(string path)
        {
            CurrentDir = new DirectoryInfo(path);
            history.Clear();
            LoadDirectory(CurrentDir);
            UpdateBackButton();
        }

        public void LoadDirectory(DirectoryInfo dir, bool updateWatcher = true)
        {
            if (updateWatcher)
            {
                SetupWatcher(dir);
            }

            var items = new ObservableCollection<FileSystemInfo>();
            if (dir.Parent != null)
            {
                items.Add(new ParentDirectoryInfo(dir.Parent.FullName));
            }
            foreach (var d in dir.GetDirectories()) items.Add(d);
            foreach (var f in dir.GetFiles()) items.Add(f);
            List.ItemsSource = items;
            PathText.Text = dir.FullName;

            UpdateDriveInfo(dir);
        }

        private void SetupWatcher(DirectoryInfo dir)
        {
            watcher?.Dispose();
            watcher = new FileSystemWatcher(dir.FullName)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true
            };

            watcher.Changed += OnDirectoryChanged;
            watcher.Created += OnDirectoryChanged;
            watcher.Deleted += OnDirectoryChanged;
            watcher.Renamed += OnDirectoryChanged;
        }

        private void OnDirectoryChanged(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (CurrentDir.Exists)
                {
                    LoadDirectory(CurrentDir, false);
                }
            });
        }

        private void UpdateDriveInfo(DirectoryInfo dir)
        {
            var drive = new DriveInfo(dir.Root.FullName);
            long total = drive.TotalSize;
            long free = drive.TotalFreeSpace;
            long used = total - free;
            SpaceText.Text = Localization.Get(
                "DriveInfo_Format",
                FormatBytes(total),
                FormatBytes(used),
                FormatBytes(free));
        }

        private static string FormatBytes(long bytes)
        {
            var sizes = Localization.Get("SizeUnits").Split(',');
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public void NavigateInto(DirectoryInfo dir)
        {
            history.Push(CurrentDir);
            CurrentDir = dir;
            LoadDirectory(CurrentDir);
            UpdateBackButton();
        }

        public void NavigateBack()
        {
            if (history.Count > 0)
            {
                CurrentDir = history.Pop();
                LoadDirectory(CurrentDir);
                UpdateBackButton();
            }
        }

        public void UpdateBackButton()
        {
            BackButton.IsEnabled = history.Count > 0;
        }
    }
}