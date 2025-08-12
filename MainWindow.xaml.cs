using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private FilePane activePane;

        public MainWindow()
        {
            InitializeComponent();
            Localization.LoadSystemLanguage();
            ApplyLocalization();

            leftPane = new FilePane(LeftList, LeftPathText, LeftDriveSelector, LeftBackButton, LeftSpaceText);
            rightPane = new FilePane(RightList, RightPathText, RightDriveSelector, RightBackButton, RightSpaceText);
            PopulateDriveSelectors();

            activePane = leftPane;
            LeftList.Focus();
            LeftList.GotFocus += List_GotFocus;
            RightList.GotFocus += List_GotFocus;
        }

        private void ApplyLocalization()
        {
            Title = Localization.Get("App_Title");
            CreateFolderText.Text = Localization.Get("Button_CreateFolder");
            CreateFileText.Text = Localization.Get("Button_CreateFile");
            CopyText.Text = Localization.Get("Button_Copy");
            MoveText.Text = Localization.Get("Button_Move");
            DeleteText.Text = Localization.Get("Button_Delete");
            TerminalText.Text = Localization.Get("Button_OpenTerminal");
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


        private void OpenSelected(FilePane pane)
        {
            foreach (FileSystemInfo item in pane.List.SelectedItems.Cast<FileSystemInfo>().ToList())
            {
                if (item is ParentDirectoryInfo parent)
                {
                    var dir = new DirectoryInfo(parent.FullName);
                    if (dir.Exists)
                    {
                        try
                        {
                            pane.NavigateInto(dir);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(Localization.Get("Error_OpenFolder", ex.Message));
                        }
                    }
                    else
                    {
                        MessageBox.Show(Localization.Get("Error_FolderNotFound", dir.FullName));
                    }
                }
                else if (item is DirectoryInfo dir)
                {
                    if (dir.Exists)
                    {
                        try
                        {
                            pane.NavigateInto(dir);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(Localization.Get("Error_OpenFolder", ex.Message));
                        }
                    }
                    else
                    {
                        MessageBox.Show(Localization.Get("Error_FolderNotFound", dir.FullName));
                    }
                }
                else if (item is FileInfo file)
                {
                    if (file.Exists)
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(file.FullName) { UseShellExecute = true });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(Localization.Get("Error_OpenFile", ex.Message));
                        }
                    }
                    else
                    {
                        MessageBox.Show(Localization.Get("Error_FileNotFound", file.FullName));
                    }
                }
            }
        }

        private void List_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list = (ListView)sender;
            var pane = list == LeftList ? leftPane : rightPane;
            OpenSelected(pane);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OpenSelected(activePane);
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                if (activePane == leftPane)
                    RightList.Focus();
                else
                    LeftList.Focus();
                e.Handled = true;
            }
            else if (e.Key == Key.F5)
            {
                Copy_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.F6)
            {
                Move_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.F7)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    CreateFile_Click(null, null);
                else
                    CreateFolder_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.F8)
            {
                Delete_Click(null, null);
                e.Handled = true;
            }
        }

        private void OpenTerminal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("powershell.exe")
                {
                    WorkingDirectory = activePane.CurrentDir.FullName,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(Localization.Get("Error_OpenTerminal", ex.Message));
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
            if (list.SelectedItem is FileSystemInfo selectedItem && selectedItem is not ParentDirectoryInfo)
            {
                try
                {
                    Process.Start("explorer.exe", $"/select,\"{selectedItem.FullName}\"");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Localization.Get("Error_ContextMenu", ex.Message));
                }
            }
        }

        private void List_GotFocus(object sender, RoutedEventArgs e)
        {
            activePane = ((ListView)sender) == LeftList ? leftPane : rightPane;
        }

        private FilePane ActivePane => activePane;
        private FilePane InactivePane => activePane == leftPane ? rightPane : leftPane;

        private static bool ValidateName(string name)
        {
            if (Path.IsPathRooted(name) || name.Contains("..") || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show(Localization.Get("Error_InvalidName"), Localization.Get("Error_InvalidName_Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void CreateFolder_Click(object sender, RoutedEventArgs e)
        {
            var pane = ActivePane;
            string name = Interaction.InputBox(
                Localization.Get("Prompt_FolderName"),
                Localization.Get("Prompt_CreateFolder"),
                Localization.Get("Default_FolderName")).Trim();
            if (!string.IsNullOrWhiteSpace(name) && ValidateName(name))
            {
                Directory.CreateDirectory(Path.Combine(pane.CurrentDir.FullName, name));
                pane.LoadDirectory(pane.CurrentDir);
            }
        }

        private void CreateFile_Click(object sender, RoutedEventArgs e)
        {
            var pane = ActivePane;
            string name = Interaction.InputBox(
                Localization.Get("Prompt_FileName"),
                Localization.Get("Prompt_CreateFile"),
                Localization.Get("Default_FileName")).Trim();
            if (!string.IsNullOrWhiteSpace(name) && ValidateName(name))
            {
                File.Create(Path.Combine(pane.CurrentDir.FullName, name)).Close();
                pane.LoadDirectory(pane.CurrentDir);
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            var source = ActivePane;
            var dest = InactivePane;
            foreach (FileSystemInfo item in source.List.SelectedItems.Cast<FileSystemInfo>().Where(i => i is not ParentDirectoryInfo).ToList())
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
                    MessageBox.Show(Localization.Get("Error_Copy", ex.Message));
                }
            }
        }

        private void Move_Click(object sender, RoutedEventArgs e)
        {
            var source = ActivePane;
            var dest = InactivePane;
            foreach (FileSystemInfo item in source.List.SelectedItems.Cast<FileSystemInfo>().Where(i => i is not ParentDirectoryInfo).ToList())
            {
                string target = Path.Combine(dest.CurrentDir.FullName, item.Name);
                try
                {
                    MoveWithFallback(item.FullName, target);
                    source.LoadDirectory(source.CurrentDir);
                    dest.LoadDirectory(dest.CurrentDir);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Localization.Get("Error_Move", ex.Message));
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var pane = ActivePane;
            var selectedItems = pane.List.SelectedItems.Cast<FileSystemInfo>().Where(i => i is not ParentDirectoryInfo).ToList();
            if (selectedItems.Count == 0)
                return;

            var result = MessageBox.Show(
                Localization.Get("Confirm_Delete", selectedItems.Count),
                Localization.Get("Confirm_Delete_Title"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;

            foreach (var item in selectedItems)
            {
                try
                {
                    if (item is FileInfo)
                    {
                        File.Delete(item.FullName);
                    }
                    else if (item is DirectoryInfo)
                    {
                        Directory.Delete(item.FullName, true);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Localization.Get("Error_Delete", ex.Message));
                }
            }

            pane.LoadDirectory(pane.CurrentDir);
        }

        private void List_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var list = (ListView)sender;
                if (Keyboard.FocusedElement is ListViewItem item && list.ItemContainerGenerator.IndexFromContainer(item) >= 0)
                {
                    item.IsSelected = !item.IsSelected;
                    e.Handled = true;
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

        private static void MoveWithFallback(string source, string destination)
        {
            try
            {
                if (File.Exists(source))
                {
                    File.Move(source, destination, true);
                }
                else if (Directory.Exists(source))
                {
                    if (Directory.Exists(destination))
                    {
                        Directory.Delete(destination, true);
                    }
                    Directory.Move(source, destination);
                }
                else
                {
                    throw new FileNotFoundException("Source does not exist", source);
                }
            }
            catch (IOException)
            {
                if (File.Exists(source))
                {
                    File.Copy(source, destination, true);
                    File.Delete(source);
                }
                else if (Directory.Exists(source))
                {
                    if (Directory.Exists(destination))
                    {
                        Directory.Delete(destination, true);
                    }
                    CopyDirectory(source, destination);
                    Directory.Delete(source, true);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
