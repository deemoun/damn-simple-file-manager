using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.VisualBasic;

namespace DamnSimpleFileManager
{
    public partial class MainWindow : Window
    {
        private readonly FilePane leftPane;
        private readonly FilePane rightPane;

        public MainWindow()
        {
            InitializeComponent();
            leftPane = new FilePane(LeftList, LeftPathText, LeftDriveSelector, LeftBackButton);
            rightPane = new FilePane(RightList, RightPathText, RightDriveSelector, RightBackButton);
            PopulateDriveSelectors();
        }

        private void PopulateDriveSelectors()
        {
            leftPane.PopulateDrives();
            rightPane.PopulateDrives();

            if (rightPane.DriveSelector.Items.Count > 1)
            {
                rightPane.DriveSelector.SelectedIndex = 1;
                rightPane.SetDrive(rightPane.DriveSelector.SelectedItem!.ToString()!);
            }
        }


        private void List_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list = (ListView)sender;
            var pane = list == LeftList ? leftPane : rightPane;
            if (list.SelectedItem is DirectoryInfo dir)
            {
                pane.NavigateInto(dir);
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
                var pane = LeftList.IsKeyboardFocusWithin ? leftPane : rightPane;
                if (pane.List.SelectedItem is DirectoryInfo dir)
                {
                    pane.NavigateInto(dir);
                }
                else if (pane.List.SelectedItem is FileInfo file)
                {
                    Process.Start(new ProcessStartInfo(file.FullName) { UseShellExecute = true });
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                if (LeftList.IsKeyboardFocusWithin)
                    RightList.Focus();
                else
                    LeftList.Focus();
                e.Handled = true;
            }
        }

        private void LeftDriveSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LeftDriveSelector.SelectedItem is string path)
            {
                leftPane.SetDrive(path);
            }
        }

        private void RightDriveSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RightDriveSelector.SelectedItem is string path)
            {
                rightPane.SetDrive(path);
            }
        }

        private void LeftBack_Click(object sender, RoutedEventArgs e)
        {
            leftPane.NavigateBack();
        }

        private void RightBack_Click(object sender, RoutedEventArgs e)
        {
            rightPane.NavigateBack();
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

        private FilePane ActivePane => LeftList.IsKeyboardFocusWithin ? leftPane : rightPane;
        private FilePane InactivePane => LeftList.IsKeyboardFocusWithin ? rightPane : leftPane;

        private void CreateFolder_Click(object sender, RoutedEventArgs e)
        {
            var pane = ActivePane;
            string name = Interaction.InputBox("Имя папки:", "Создать папку", "Новая папка");
            if (!string.IsNullOrWhiteSpace(name))
            {
                Directory.CreateDirectory(Path.Combine(pane.CurrentDir.FullName, name));
                pane.LoadDirectory(pane.CurrentDir);
            }
        }

        private void CreateFile_Click(object sender, RoutedEventArgs e)
        {
            var pane = ActivePane;
            string name = Interaction.InputBox("Имя файла:", "Создать файл", "Новый файл.txt");
            if (!string.IsNullOrWhiteSpace(name))
            {
                File.Create(Path.Combine(pane.CurrentDir.FullName, name)).Close();
                pane.LoadDirectory(pane.CurrentDir);
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            var source = ActivePane;
            var dest = InactivePane;
            if (source.List.SelectedItem is FileSystemInfo item)
            {
                string target = Path.Combine(dest.CurrentDir.FullName, item.Name);
                try
                {
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
                    MessageBox.Show($"Ошибка копирования: {ex.Message}");
                }
            }
        }

        private void Move_Click(object sender, RoutedEventArgs e)
        {
            var source = ActivePane;
            var dest = InactivePane;
            if (source.List.SelectedItem is FileSystemInfo item)
            {
                string target = Path.Combine(dest.CurrentDir.FullName, item.Name);
                try
                {
                    if (item is FileInfo)
                    {
                        File.Move(item.FullName, target, true);
                    }
                    else if (item is DirectoryInfo)
                    {
                        if (Directory.Exists(target))
                        {
                            Directory.Delete(target, true);
                        }
                        Directory.Move(item.FullName, target);
                    }
                    source.LoadDirectory(source.CurrentDir);
                    dest.LoadDirectory(dest.CurrentDir);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка перемещения: {ex.Message}");
                }
            }
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);
            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(destinationDir, Path.GetFileName(file)), true);
            foreach (var directory in Directory.GetDirectories(sourceDir))
                CopyDirectory(directory, Path.Combine(destinationDir, Path.GetFileName(directory)));
        }
    }
}