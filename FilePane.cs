using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;

namespace DamnSimpleFileManager
{
    internal class FilePane
    {
        public ListView List { get; }
        public TextBlock PathText { get; }
        public ComboBox DriveSelector { get; }
        public Button BackButton { get; }
        public TextBlock DiskInfo { get; }

        public bool ShowHidden { get; set; }

        public DirectoryInfo CurrentDir { get; private set; } = null!;
        private readonly Stack<DirectoryInfo> history = new();

        public FilePane(ListView list, TextBlock pathText, ComboBox driveSelector, Button backButton, TextBlock diskInfo)
        {
            List = list;
            PathText = pathText;
            DriveSelector = driveSelector;
            BackButton = backButton;
            DiskInfo = diskInfo;
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

        public void LoadDirectory(DirectoryInfo dir)
        {
            var items = new ObservableCollection<FileSystemInfo>();
            foreach (var d in dir.GetDirectories())
            {
                if (!ShowHidden && d.Attributes.HasFlag(FileAttributes.Hidden)) continue;
                items.Add(d);
            }
            foreach (var f in dir.GetFiles())
            {
                if (!ShowHidden && f.Attributes.HasFlag(FileAttributes.Hidden)) continue;
                items.Add(f);
            }
            List.ItemsSource = items;
            PathText.Text = dir.FullName;
            try
            {
                var drive = new DriveInfo(dir.Root.FullName);
                DiskInfo.Text = $"Свободно: {drive.AvailableFreeSpace / (1024 * 1024 * 1024)} ГБ";
            }
            catch
            {
                DiskInfo.Text = string.Empty;
            }
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
