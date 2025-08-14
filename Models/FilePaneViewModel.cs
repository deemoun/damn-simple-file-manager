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

        internal enum SortField
        {
            Name,
            Size,
            CreationTime
        }

        private SortField currentSortField = SortField.Name;
        private bool sortAscending = true;

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

        private string totalSpace = string.Empty;
        public string TotalSpace
        {
            get => totalSpace;
            private set
            {
                if (totalSpace != value)
                {
                    totalSpace = value;
                    OnPropertyChanged();
                }
            }
        }

        private string usedSpace = string.Empty;
        public string UsedSpace
        {
            get => usedSpace;
            private set
            {
                if (usedSpace != value)
                {
                    usedSpace = value;
                    OnPropertyChanged();
                }
            }
        }

        private string freeSpace = string.Empty;
        public string FreeSpace
        {
            get => freeSpace;
            private set
            {
                if (freeSpace != value)
                {
                    freeSpace = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TotalLabel => Localization.Get("DriveInfo_Total") + ":";
        public string UsedLabel => Localization.Get("DriveInfo_Used") + ":";
        public string FreeLabel => Localization.Get("DriveInfo_Free") + ":";
        public string NoItemsMessage => Localization.Get("NoItemsMessage");

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
            foreach (var d in dir.GetDirectories().Where(d =>
                             Settings.ShowHiddenFiles ||
                             (!d.Attributes.HasFlag(FileAttributes.Hidden) &&
                              !d.Attributes.HasFlag(FileAttributes.System))))
                Items.Add(d);
            foreach (var f in dir.GetFiles().Where(f =>
                             Settings.ShowHiddenFiles ||
                             (!f.Attributes.HasFlag(FileAttributes.Hidden) &&
                              !f.Attributes.HasFlag(FileAttributes.System))))
                Items.Add(f);

            SortItems();

            CurrentPath = dir.FullName;

            UpdateDriveInfo(dir);
        }

        public void Sort(SortField field)
        {
            if (currentSortField == field)
            {
                sortAscending = !sortAscending;
            }
            else
            {
                currentSortField = field;
                sortAscending = true;
            }
            SortItems();
        }

        private void SortItems()
        {
            var parent = Items.OfType<ParentDirectoryInfo>().FirstOrDefault();
            var items = Items.Where(i => i is not ParentDirectoryInfo).ToList();

            IEnumerable<FileSystemInfo> sorted = currentSortField switch
            {
                SortField.Name => sortAscending
                    ? items.OrderBy(i => i.Name, StringComparer.CurrentCultureIgnoreCase)
                    : items.OrderByDescending(i => i.Name, StringComparer.CurrentCultureIgnoreCase),
                SortField.Size => sortAscending
                    ? items.OrderBy(i => i is FileInfo fi ? fi.Length : 0L)
                    : items.OrderByDescending(i => i is FileInfo fi ? fi.Length : 0L),
                SortField.CreationTime => sortAscending
                    ? items.OrderBy(i => i.CreationTime)
                    : items.OrderByDescending(i => i.CreationTime),
                _ => items
            };

            Items.Clear();
            if (parent != null)
            {
                Items.Add(parent);
            }

            foreach (var item in sorted)
            {
                Items.Add(item);
            }
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
            TotalSpace = FormatBytes(total);
            UsedSpace = FormatBytes(used);
            FreeSpace = FormatBytes(free);
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

