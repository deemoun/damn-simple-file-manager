using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleFileManager
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

        private void List_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list = (ListView)sender;
            if (list.SelectedItem is DirectoryInfo dir)
            {
                if (list == LeftList)
                {
                    leftDir = dir;
                    LoadList(LeftList, leftDir);
                }
                else
                {
                    rightDir = dir;
                    LoadList(RightList, rightDir);
                }
            }
            else if (list.SelectedItem is FileInfo file)
            {
                Process.Start(new ProcessStartInfo(file.FullName) { UseShellExecute = true });
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var list = LeftList.IsKeyboardFocusWithin ? LeftList : RightList;
                if (list.SelectedItem is DirectoryInfo dir)
                {
                    if (list == LeftList)
                    {
                        leftDir = dir;
                        LoadList(LeftList, leftDir);
                    }
                    else
                    {
                        rightDir = dir;
                        LoadList(RightList, rightDir);
                    }
                }
                else if (list.SelectedItem is FileInfo file)
                {
                    Process.Start(new ProcessStartInfo(file.FullName) { UseShellExecute = true });
                }
                e.Handled = true;
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, IntPtr apidl, uint dwFlags);

        private void List_RightClick(object sender, MouseButtonEventArgs e)
        {
            var list = (ListView)sender;
            if (list.SelectedItem is FileSystemInfo selectedItem)
            {
                try
                {
                    Process.Start("explorer.exe", $"/select,\"{selectedItem.FullName}\"");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось открыть контекстное меню: {ex.Message}");
                }
            }
        }

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