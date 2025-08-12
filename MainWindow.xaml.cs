using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.VisualBasic;
using DamnSimpleFileManager.Services;

namespace DamnSimpleFileManager
{
    public partial class MainWindow : Window
    {
        private readonly FilePane leftPane;
        private readonly FilePane rightPane;
        private FilePane activePane;
        private readonly FileOperationsService fileOperationsService;

        public MainWindow()
        {
            InitializeComponent();
            Localization.LoadSystemLanguage();
            ApplyLocalization();

            leftPane = new FilePane(LeftList, LeftPathText, LeftDriveSelector, LeftBackButton, LeftSpaceText);
            rightPane = new FilePane(RightList, RightPathText, RightDriveSelector, RightBackButton, RightSpaceText);
            PopulateDriveSelectors();

            fileOperationsService = new FileOperationsService();
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
                            MessageBox.Show(this, Localization.Get("Error_OpenFolder", ex.Message));
                        }
                    }
                    else
                    {
                        MessageBox.Show(this, Localization.Get("Error_FolderNotFound", dir.FullName));
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
                            MessageBox.Show(this, Localization.Get("Error_OpenFolder", ex.Message));
                        }
                    }
                    else
                    {
                        MessageBox.Show(this, Localization.Get("Error_FolderNotFound", dir.FullName));
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
                            MessageBox.Show(this, Localization.Get("Error_OpenFile", ex.Message));
                        }
                    }
                    else
                    {
                        MessageBox.Show(this, Localization.Get("Error_FileNotFound", file.FullName));
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
                MessageBox.Show(this, Localization.Get("Error_OpenTerminal", ex.Message));
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutWindow
            {
                Owner = this
            };
            about.ShowDialog();
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
                    MessageBox.Show(this, Localization.Get("Error_ContextMenu", ex.Message));
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
                MessageBox.Show(Application.Current.MainWindow!, Localization.Get("Error_InvalidName"), Localization.Get("Error_InvalidName_Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
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
                Localization.Get("Default_FolderName"),
                (int)(Left + (ActualWidth - 300) / 2),
                (int)(Top + (ActualHeight - 150) / 2)).Trim();
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
                Localization.Get("Default_FileName"),
                (int)(Left + (ActualWidth - 300) / 2),
                (int)(Top + (ActualHeight - 150) / 2)).Trim();
            if (!string.IsNullOrWhiteSpace(name) && ValidateName(name))
            {
                File.Create(Path.Combine(pane.CurrentDir.FullName, name)).Close();
                pane.LoadDirectory(pane.CurrentDir);
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            fileOperationsService.Copy(ActivePane, InactivePane, this);
        }

        private void Move_Click(object sender, RoutedEventArgs e)
        {
            fileOperationsService.Move(ActivePane, InactivePane, this);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            fileOperationsService.Delete(ActivePane, this);
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

    }
}
