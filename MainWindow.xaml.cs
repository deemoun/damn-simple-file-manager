using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DamnSimpleFileManager
{
    public partial class MainWindow : Window
    {
        private DirectoryInfo leftDir;
        private DirectoryInfo rightDir;

        public MainWindow()
        {
            InitializeComponent();
            PopulateDriveSelectors();
        }

        private void PopulateDriveSelectors()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    LeftDriveSelector.Items.Add(drive.Name);
                    RightDriveSelector.Items.Add(drive.Name);
                }
            }
            if (LeftDriveSelector.Items.Count > 0)
            {
                LeftDriveSelector.SelectedIndex = 0;
                leftDir = new DirectoryInfo(LeftDriveSelector.SelectedItem.ToString());
                LoadList(LeftList, leftDir);
            }
            if (RightDriveSelector.Items.Count > 1)
            {
                RightDriveSelector.SelectedIndex = 1;
                rightDir = new DirectoryInfo(RightDriveSelector.SelectedItem.ToString());
                LoadList(RightList, rightDir);
            }
        }

        private void LoadList(ListView list, DirectoryInfo dir)
        {
            try
            {
                var items = new ObservableCollection<FileSystemInfo>();
                foreach (var d in dir.GetDirectories()) items.Add(d);
                foreach (var f in dir.GetFiles()) items.Add(f);
                list.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void LeftList_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LeftList.SelectedItem is DirectoryInfo dir)
            {
                leftDir = dir;
                LoadList(LeftList, leftDir);
            }
        }

        private void RightList_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (RightList.SelectedItem is DirectoryInfo dir)
            {
                rightDir = dir;
                LoadList(RightList, rightDir);
            }
        }

        // CopyRight_Click отключён

        private void LeftDriveSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LeftDriveSelector.SelectedItem is string path)
            {
                leftDir = new DirectoryInfo(path);
                LoadList(LeftList, leftDir);
            }
        }

        private void RightDriveSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RightDriveSelector.SelectedItem is string path)
            {
                rightDir = new DirectoryInfo(path);
                LoadList(RightList, rightDir);
            }
        }
    }
}
