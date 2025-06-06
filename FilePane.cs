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

        public DirectoryInfo CurrentDir { get; private set; } = null!;
        private readonly Stack<DirectoryInfo> history = new();

        public FilePane(ListView list, TextBlock pathText, ComboBox driveSelector, Button backButton)
        {
            List = list;
            PathText = pathText;
            DriveSelector = driveSelector;
            BackButton = backButton;
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
            foreach (var d in dir.GetDirectories()) items.Add(d);
            foreach (var f in dir.GetFiles()) items.Add(f);
            List.ItemsSource = items;
            PathText.Text = dir.FullName;
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
