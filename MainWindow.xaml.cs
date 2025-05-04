using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DamnSimpleFileManager
{
    public partial class MainWindow : Window
    {
        private DirectoryInfo leftDir = null!;
        private DirectoryInfo rightDir = null!;
        private readonly Stack<DirectoryInfo> leftHistory = new();
        private readonly Stack<DirectoryInfo> rightHistory = new();

        public MainWindow()
        {
            InitializeComponent();
            PopulateDriveSelectors();
            UpdateBackButtons();
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

            UpdateBackButtons();
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
                    leftHistory.Push(leftDir);
                    leftDir = dir;
                    LoadList(LeftList, leftDir);
                }
                else
                {
                    rightHistory.Push(rightDir);
                    rightDir = dir;
                    LoadList(RightList, rightDir);
                }
                UpdateBackButtons();
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
                        leftHistory.Push(leftDir);
                        leftDir = dir;
                        LoadList(LeftList, leftDir);
                    }
                    else
                    {
                        rightHistory.Push(rightDir);
                        rightDir = dir;
                        LoadList(RightList, rightDir);
                    }
                    UpdateBackButtons();
                }
                else if (list.SelectedItem is FileInfo file)
                {
                    Process.Start(new ProcessStartInfo(file.FullName) { UseShellExecute = true });
                }
                e.Handled = true;
            }
        }

        private void LeftDriveSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LeftDriveSelector.SelectedItem is string path)
            {
                leftDir = new DirectoryInfo(path);
                leftHistory.Clear();
                LoadList(LeftList, leftDir);
                UpdateBackButtons();
            }
        }

        private void RightDriveSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RightDriveSelector.SelectedItem is string path)
            {
                rightDir = new DirectoryInfo(path);
                rightHistory.Clear();
                LoadList(RightList, rightDir);
                UpdateBackButtons();
            }
        }

        private void LeftBack_Click(object sender, RoutedEventArgs e)
        {
            if (leftHistory.Count > 0)
            {
                leftDir = leftHistory.Pop();
                LoadList(LeftList, leftDir);
                UpdateBackButtons();
            }
        }

        private void RightBack_Click(object sender, RoutedEventArgs e)
        {
            if (rightHistory.Count > 0)
            {
                rightDir = rightHistory.Pop();
                LoadList(RightList, rightDir);
                UpdateBackButtons();
            }
        }

        private void UpdateBackButtons()
        {
            LeftBackButton.IsEnabled = leftHistory.Count > 0;
            RightBackButton.IsEnabled = rightHistory.Count > 0;
        }

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
    }
}
