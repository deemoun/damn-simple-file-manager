using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DamnSimpleFileManager
{
    internal class FilePaneViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<FileSystemInfo> Items { get; } = new();
        public ObservableCollection<string> Drives { get; } = new();

        private string? selectedDrive;
        public string? SelectedDrive
        {
            get => selectedDrive;
            set
            {
                if (selectedDrive != value)
                {
                    selectedDrive = value;
                    OnPropertyChanged();
                    if (value != null)
                    {
                        CurrentDir = new DirectoryInfo(value);
                        history.Clear();
                        LoadDirectory(CurrentDir);
                        OnPropertyChanged(nameof(CanGoBack));
                        NavigateBackCommand.RaiseCanExecuteChanged();
                    }
                }
            }
        }

        private string currentPath = string.Empty;
        public string CurrentPath
        {
            get => currentPath;
            private set
            {
                if (currentPath != value)
                {
                    currentPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private string driveInfo = string.Empty;
        public string DriveInfo
        {
            get => driveInfo;
            private set
            {
                if (driveInfo != value)
                {
                    driveInfo = value;
                    OnPropertyChanged();
                }
            }
        }

        public DirectoryInfo CurrentDir { get; private set; } = null!;
        private readonly Stack<DirectoryInfo> history = new();
        private FileSystemWatcher? watcher;

        public RelayCommand NavigateBackCommand { get; }

        public bool CanGoBack => history.Count > 0;

        public event PropertyChangedEventHandler? PropertyChanged;

        public FilePaneViewModel()
        {
            NavigateBackCommand = new RelayCommand(_ => NavigateBack(), _ => CanGoBack);
        }

        public void PopulateDrives()
        {
            Drives.Clear();
            foreach (var drive in System.IO.DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    Drives.Add(drive.Name);
                }
            }

            if (Drives.Count > 0)
            {
                SelectedDrive = Drives[0];
            }
        }

        public void LoadDirectory(DirectoryInfo dir, bool updateWatcher = true)
        {
            if (updateWatcher)
            {
                SetupWatcher(dir);
            }

            Items.Clear();
            if (dir.Parent != null)
            {
                Items.Add(new ParentDirectoryInfo(dir.Parent.FullName));
            }
            foreach (var d in dir.GetDirectories().Where(d => Settings.ShowHiddenFiles || !d.Attributes.HasFlag(FileAttributes.Hidden)))
                Items.Add(d);
            foreach (var f in dir.GetFiles().Where(f => Settings.ShowHiddenFiles || !f.Attributes.HasFlag(FileAttributes.Hidden)))
                Items.Add(f);

            CurrentPath = dir.FullName;

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
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (CurrentDir.Exists)
                {
                    LoadDirectory(CurrentDir, false);
                }
            });
        }

        private void UpdateDriveInfo(DirectoryInfo dir)
        {
            var drive = new System.IO.DriveInfo(dir.Root.FullName);
            long total = drive.TotalSize;
            long free = drive.TotalFreeSpace;
            long used = total - free;
            DriveInfo = Localization.Get(
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
            OnPropertyChanged(nameof(CanGoBack));
            NavigateBackCommand.RaiseCanExecuteChanged();
        }

        public void NavigateBack()
        {
            if (history.Count > 0)
            {
                CurrentDir = history.Pop();
                LoadDirectory(CurrentDir);
                OnPropertyChanged(nameof(CanGoBack));
                NavigateBackCommand.RaiseCanExecuteChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

