using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.ComponentModel;
using DamnSimpleFileManager.Services;
using DamnSimpleFileManager.Windows;
using DamnSimpleFileManager.Utils;

namespace DamnSimpleFileManager
{
    public partial class MainWindow : Window
    {
        private readonly FilePaneViewModel leftPane;
        private readonly FilePaneViewModel rightPane;
        private FilePaneViewModel activePane;
        private readonly FileOperationsService fileOperationsService;
        private bool isRightPaneVisible;

        public MainWindow()
        {
            InitializeComponent();
            Logger.Log("MainWindow initialized");
            Localization.LoadSystemLanguage();
            isRightPaneVisible = !Settings.StartSinglePane;
            ApplyLocalization();

            leftPane = new FilePaneViewModel();
            rightPane = new FilePaneViewModel();

            LeftList.DataContext = leftPane;
            LeftPathText.DataContext = leftPane;
            LeftDriveSelector.DataContext = leftPane;
            LeftBackButton.DataContext = leftPane;
            LeftSpaceText.DataContext = leftPane;

            RightList.DataContext = rightPane;
            RightPathText.DataContext = rightPane;
            RightDriveSelector.DataContext = rightPane;
            RightBackButton.DataContext = rightPane;
            RightSpaceText.DataContext = rightPane;

            PopulateDriveSelectors();

            fileOperationsService = new FileOperationsService();
            activePane = leftPane;
            LeftList.Focus();
            LeftList.GotFocus += List_GotFocus;
            RightList.GotFocus += List_GotFocus;
            LeftList.SelectionChanged += List_SelectionChanged;
            RightList.SelectionChanged += List_SelectionChanged;
            UpdateOperationsAvailability();
            UpdatePaneLayout();
        }

        private void ApplyLocalization()
        {
            Logger.Log("Applying localization");
            Title = Localization.Get("App_Title");
            FileMenu.Header = Localization.Get("Menu_File");
            NewFolderMenuItem.Header = Localization.Get("Menu_NewFolder");
            NewFolderMenuItem.InputGestureText = "F7";
            NewFileMenuItem.Header = Localization.Get("Menu_NewFile");
            NewFileMenuItem.InputGestureText = "Shift+F7";
            ViewMenuItem.Header = Localization.Get("Menu_View");
            ViewMenuItem.InputGestureText = "F3";
            CopyMenuItem.Header = Localization.Get("Menu_Copy");
            CopyMenuItem.InputGestureText = "F5";
            MoveMenuItem.Header = Localization.Get("Menu_Move");
            MoveMenuItem.InputGestureText = "F6";
            RenameMenuItem.Header = Localization.Get("Menu_Rename");
            RenameMenuItem.InputGestureText = "F2";
            DeleteMenuItem.Header = Localization.Get("Menu_Delete");
            DeleteMenuItem.InputGestureText = "F8";
            OpenTerminalMenuItem.Header = Localization.Get("Menu_OpenTerminal");
            ToolsMenu.Header = Localization.Get("Menu_Tools");
            TogglePaneMenuItem.Header = Localization.Get(isRightPaneVisible ? "Menu_RemoveSecondPane" : "Menu_AddSecondPane");
            TogglePaneMenuItem.InputGestureText = "Alt+F2";
            ServicesMenuItem.Header = Localization.Get("Menu_Services");
            ControlPanelMenuItem.Header = Localization.Get("Menu_ControlPanel");
            SystemMenuItem.Header = Localization.Get("Menu_System");
            FileBookmarksMenuItem.Header = Localization.Get("Menu_FileBookmarks");
            LynkrMenuItem.Header = Localization.Get("Menu_Lynkr");
            OpenSettingsIniMenuItem.Header = Localization.Get("Menu_OpenSettingsIni");
            ExitMenuItem.Header = Localization.Get("Menu_Exit");
            HelpMenu.Header = Localization.Get("Menu_Help");
            CheckUpdatesMenuItem.Header = Localization.Get("Menu_CheckForUpdates");
            AboutMenuItem.Header = Localization.Get("Menu_About");
            CreateFolderText.Text = Localization.Get("Button_CreateFolder");
            CreateFileText.Text = Localization.Get("Button_CreateFile");
            ViewText.Text = Localization.Get("Button_View") + " (F3)";
            CopyText.Text = Localization.Get("Button_Copy") + " (F5)";
            MoveText.Text = Localization.Get("Button_Move") + " (F6)";
            DeleteText.Text = Localization.Get("Button_Delete") + " (F8)";
            TerminalText.Text = Localization.Get("Button_OpenTerminal");
            LeftTypeHeader.Content = Localization.Get("Column_Type");
            RightTypeHeader.Content = Localization.Get("Column_Type");
            LeftNameHeader.Content = Localization.Get("Column_Name");
            RightNameHeader.Content = Localization.Get("Column_Name");
            LeftSizeHeader.Content = Localization.Get("Column_Size");
            RightSizeHeader.Content = Localization.Get("Column_Size");
            LeftDateHeader.Content = Localization.Get("Column_Created");
            RightDateHeader.Content = Localization.Get("Column_Created");
        }

        private void PopulateDriveSelectors()
        {
            Logger.Log("Populating drive selectors");
            leftPane.PopulateDrives();
            rightPane.PopulateDrives();

            if (rightPane.Drives.Count > 1)
            {
                rightPane.SelectedDrive = rightPane.Drives[1];
            }
        }

        private void OpenSelected(FilePaneViewModel pane, ListView list)
        {
            Logger.Log("Opening selected items");
            foreach (FileSystemInfo item in list.SelectedItems.Cast<FileSystemInfo>().ToList())
            {
                if (item is ParentDirectoryInfo parent)
                {
                    var dir = new DirectoryInfo(parent.FullName);
                    if (dir.Exists)
                    {
                        try
                        {
                            Logger.Log($"Navigating into '{dir.FullName}'");
                            pane.NavigateInto(dir);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Error navigating into '{dir.FullName}'", ex);
                            MessageBox.Show(this, Localization.Get("Error_OpenFolder", ex.Message));
                        }
                    }
                    else
                    {
                        Logger.LogError($"Folder not found: '{dir.FullName}'", new DirectoryNotFoundException(dir.FullName));
                        MessageBox.Show(this, Localization.Get("Error_FolderNotFound", dir.FullName));
                    }
                }
                else if (item is DirectoryInfo dir)
                {
                    if (dir.Exists)
                    {
                        try
                        {
                            Logger.Log($"Navigating into '{dir.FullName}'");
                            pane.NavigateInto(dir);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Error navigating into '{dir.FullName}'", ex);
                            MessageBox.Show(this, Localization.Get("Error_OpenFolder", ex.Message));
                        }
                    }
                    else
                    {
                        Logger.LogError($"Folder not found: '{dir.FullName}'", new DirectoryNotFoundException(dir.FullName));
                        MessageBox.Show(this, Localization.Get("Error_FolderNotFound", dir.FullName));
                    }
                }
                else if (item is FileInfo file)
                {
                    if (file.Exists)
                    {
                        try
                        {
                            Logger.Log($"Opening file '{file.FullName}'");
                            Process.Start(new ProcessStartInfo(file.FullName) { UseShellExecute = true });
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Error opening file '{file.FullName}'", ex);
                            MessageBox.Show(this, Localization.Get("Error_OpenFile", ex.Message));
                        }
                    }
                    else
                    {
                        Logger.LogError($"File not found: '{file.FullName}'", new FileNotFoundException(file.FullName));
                        MessageBox.Show(this, Localization.Get("Error_FileNotFound", file.FullName));
                    }
                }
            }
        }

        private void ViewSelectedFile()
        {
            Logger.Log("ViewSelectedFile called");
            if (ActiveList.SelectedItems.Count == 1 && ActiveList.SelectedItem is FileInfo file)
            {
                if (file.Exists)
                {
                    try
                    {
                        Logger.Log($"Viewing file '{file.FullName}'");
                        Process.Start(new ProcessStartInfo(file.FullName) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error viewing file '{file.FullName}'", ex);
                        MessageBox.Show(this, Localization.Get("Error_OpenFile", ex.Message));
                    }
                }
                else
                {
                    Logger.LogError($"File not found: '{file.FullName}'", new FileNotFoundException(file.FullName));
                    MessageBox.Show(this, Localization.Get("Error_FileNotFound", file.FullName));
                }
            }
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("View button clicked");
            ViewSelectedFile();
        }

        private void UpdateOperationsAvailability()
        {
            Logger.Log("Updating operations availability");
            var canView = ActiveList.SelectedItems.Count == 1 && ActiveList.SelectedItem is FileInfo;
            ViewButton.IsEnabled = canView;
            ViewMenuItem.IsEnabled = canView;

            var hasItems = ActiveList.SelectedItems.Cast<FileSystemInfo>().Any(i => i is not ParentDirectoryInfo);
            CopyButton.IsEnabled = hasItems;
            CopyMenuItem.IsEnabled = hasItems;
            MoveButton.IsEnabled = hasItems;
            MoveMenuItem.IsEnabled = hasItems;
            var canRename = ActiveList.SelectedItems.Count == 1 && ActiveList.SelectedItem is not ParentDirectoryInfo;
            RenameMenuItem.IsEnabled = canRename;
            DeleteButton.IsEnabled = hasItems;
            DeleteMenuItem.IsEnabled = hasItems;
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Logger.Log("List selection changed");
            activePane = ((ListView)sender) == LeftList ? leftPane : rightPane;
            UpdateOperationsAvailability();
        }

        private void List_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is not ListView listView || listView.View is not GridView gridView || gridView.Columns.Count == 0)
            {
                return;
            }

            listView.UpdateLayout();
            double otherColumnsWidth = 0;
            for (int i = 0; i < gridView.Columns.Count - 1; i++)
            {
                otherColumnsWidth += gridView.Columns[i].ActualWidth;
            }

            double availableWidth = listView.ActualWidth - otherColumnsWidth - SystemParameters.VerticalScrollBarWidth - 2;
            if (availableWidth > 0)
            {
                gridView.Columns[gridView.Columns.Count - 1].Width = availableWidth;
            }
        }

        private void List_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            Logger.Log("List double-clicked");
            var list = (ListView)sender;
            var pane = list == LeftList ? leftPane : rightPane;
            OpenSelected(pane, list);
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Logger.Log($"Key pressed: {e.Key}");
            if (e.Key == Key.Enter)
            {
                OpenSelected(activePane, ActiveList);
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
            else if (e.Key == Key.F2 && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                TogglePane();
                e.Handled = true;
            }
            else if (e.Key == Key.F2)
            {
                RenameSelected();
                e.Handled = true;
            }
            else if (e.Key == Key.F3)
            {
                ViewSelectedFile();
                e.Handled = true;
            }
            else if (e.Key == Key.F5)
            {
                await CopySelectedAsync();
                e.Handled = true;
            }
            else if (e.Key == Key.F6)
            {
                await MoveSelectedAsync();
                e.Handled = true;
            }
            else if (e.Key == Key.F7)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    fileOperationsService.CreateFile(ActivePane, this);
                else
                    fileOperationsService.CreateFolder(ActivePane, this);
                e.Handled = true;
            }
            else if (e.Key == Key.F8)
            {
                var items = ActiveList.SelectedItems.Cast<FileSystemInfo>().Where(i => i is not ParentDirectoryInfo).ToList();
                fileOperationsService.Delete(ActivePane, items, this);
                e.Handled = true;
            }
        }

        private void OpenTerminal_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("Open terminal clicked");
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
                Logger.LogError("Error opening terminal", ex);
                MessageBox.Show(this, Localization.Get("Error_OpenTerminal", ex.Message));
            }
        }

        private void OpenServices_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("Open services clicked");
            Process.Start(new ProcessStartInfo("services.msc") { UseShellExecute = true });
        }

        private void OpenControlPanel_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("Open Control Panel clicked");
            Process.Start(new ProcessStartInfo("control") { UseShellExecute = true });
        }

        private void OpenSystem_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("Open System clicked");
            Process.Start(new ProcessStartInfo("control", "system") { UseShellExecute = true });
        }

        private void OpenFileBookmarks_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("Open File Bookmarks clicked");
            var wnd = new FileBookmarksWindow
            {
                Owner = this
            };
            wnd.ShowDialog();
        }

        private void OpenLynkr_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("Open Lynkr clicked");
            var wnd = new LynkrWindow
            {
                Owner = this
            };
            wnd.ShowDialog();
        }

        private void OpenSettingsIni_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("Open Settings ini clicked");
            var configPath = Path.Combine(AppContext.BaseDirectory, "dsfm.ini");
            try
            {
                Process.Start(new ProcessStartInfo("notepad.exe", configPath)
                {
                    UseShellExecute = true
                });
            }
            catch (Win32Exception)
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*"
                };
                if (dialog.ShowDialog() == true)
                {
                    Process.Start(new ProcessStartInfo(dialog.FileName, configPath)
                    {
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error opening settings file", ex);
                MessageBox.Show(this, Localization.Get("Error_OpenFile", ex.Message));
            }
        }

        private void TogglePaneMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TogglePane();
        }

        private void TogglePane()
        {
            isRightPaneVisible = !isRightPaneVisible;
            if (isRightPaneVisible)
            {
                var dir = leftPane.CurrentDir;
                var drive = rightPane.Drives.FirstOrDefault(d => d.Name == dir.Root.FullName);
                if (drive != null)
                {
                    rightPane.SelectedDrive = drive;
                    rightPane.LoadDirectory(dir);
                }
            }
            UpdatePaneLayout();
        }

        private void UpdatePaneLayout()
        {
            if (isRightPaneVisible)
            {
                DrivesGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                BackGrid.ColumnDefinitions[1].Width = GridLength.Auto;
                BackGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                PathsGrid.ColumnDefinitions[1].Width = GridLength.Auto;
                PathsGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                ListsGrid.ColumnDefinitions[1].Width = GridLength.Auto;
                ListsGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                SpaceGrid.ColumnDefinitions[1].Width = GridLength.Auto;
                SpaceGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);

                RightDriveSelector.Visibility = Visibility.Visible;
                RightBackButton.Visibility = Visibility.Visible;
                RightPathText.Visibility = Visibility.Visible;
                RightList.Visibility = Visibility.Visible;
                RightSpaceText.Visibility = Visibility.Visible;

                TogglePaneMenuItem.Header = Localization.Get("Menu_RemoveSecondPane");
            }
            else
            {
                DrivesGrid.ColumnDefinitions[2].Width = new GridLength(0);
                BackGrid.ColumnDefinitions[1].Width = new GridLength(0);
                BackGrid.ColumnDefinitions[2].Width = new GridLength(0);
                PathsGrid.ColumnDefinitions[1].Width = new GridLength(0);
                PathsGrid.ColumnDefinitions[2].Width = new GridLength(0);
                ListsGrid.ColumnDefinitions[1].Width = new GridLength(0);
                ListsGrid.ColumnDefinitions[2].Width = new GridLength(0);
                SpaceGrid.ColumnDefinitions[1].Width = new GridLength(0);
                SpaceGrid.ColumnDefinitions[2].Width = new GridLength(0);

                RightDriveSelector.Visibility = Visibility.Collapsed;
                RightBackButton.Visibility = Visibility.Collapsed;
                RightPathText.Visibility = Visibility.Collapsed;
                RightList.Visibility = Visibility.Collapsed;
                RightSpaceText.Visibility = Visibility.Collapsed;

                TogglePaneMenuItem.Header = Localization.Get("Menu_AddSecondPane");
                if (activePane == rightPane)
                {
                    activePane = leftPane;
                    LeftList.Focus();
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("Exit clicked");
            Close();
        }

        private void CheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("Check for updates clicked");
            Process.Start(new ProcessStartInfo("https://github.com/deemoun/damn-simple-file-manager/releases")
            {
                UseShellExecute = true
            });
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("About clicked");
            var about = new AboutWindow
            {
                Owner = this
            };
            about.ShowDialog();
        }

        private void SpaceText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Logger.Log("Disk space text double-clicked - opening Task Manager");
                Process.Start(new ProcessStartInfo("taskmgr") { UseShellExecute = true });
            }
        }

        public string GetCurrentLeftPath() => leftPane.CurrentDir.FullName;

        public void OpenPathFromBookmark(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    leftPane.NavigateInto(new DirectoryInfo(path));
                }
                else if (File.Exists(path))
                {
                    var file = new FileInfo(path);
                    leftPane.NavigateInto(file.Directory!);
                    var item = leftPane.Items.FirstOrDefault(i => i is FileInfo fi && fi.FullName == file.FullName);
                    if (item != null)
                    {
                        LeftList.SelectedItem = item;
                        LeftList.ScrollIntoView(item);
                    }
                }
                else
                {
                    MessageBox.Show(this, Localization.Get("FileBookmarks_Error_PathNotFound"), Localization.Get("FileBookmarks_Title"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, Localization.Get("Error_OpenFolder", ex.Message));
            }
        }

        private void List_RightClick(object sender, MouseButtonEventArgs e)
        {
            Logger.Log("List right-clicked");
            var list = (ListView)sender;
            var selectedItems = list.SelectedItems
                .OfType<FileSystemInfo>()
                .Where(i => i is not ParentDirectoryInfo)
                .Select(i => i.FullName)
                .ToArray();
            if (selectedItems.Length > 0)
            {
                try
                {
                    ShellContextMenu.ShowForPaths(selectedItems, this);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error opening context menu", ex);
                    MessageBox.Show(this, Localization.Get("Error_ContextMenu", ex.Message));
                }
            }
        }

        private void List_GotFocus(object sender, RoutedEventArgs e)
        {
            Logger.Log("List got focus");
            activePane = ((ListView)sender) == LeftList ? leftPane : rightPane;
            UpdateOperationsAvailability();
        }

        private FilePaneViewModel ActivePane => activePane;
        private FilePaneViewModel InactivePane => activePane == leftPane ? rightPane : leftPane;
        private ListView ActiveList => activePane == leftPane ? LeftList : RightList;

        private void CreateFolder_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("Create folder clicked");
            fileOperationsService.CreateFolder(ActivePane, this);
        }

        private void CreateFile_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("Create file clicked");
            fileOperationsService.CreateFile(ActivePane, this);
        }

        private async Task CopySelectedAsync()
        {
            Logger.Log("Copy clicked");
            var items = ActiveList.SelectedItems.Cast<FileSystemInfo>().Where(i => i is not ParentDirectoryInfo).ToList();
            if (Settings.CopyConfirmation)
            {
                var confirmed = new List<FileSystemInfo>();
                foreach (var item in items)
                {
                    string target = Path.Combine(InactivePane.CurrentDir.FullName, item.Name);
                    var result = MessageBox.Show(this,
                        Localization.Get("Confirm_Copy", item.FullName, target),
                        Localization.Get("Confirm_Copy_Title"),
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                        confirmed.Add(item);
                }
                items = confirmed;
            }
            if (items.Count == 0)
                return;

            var progressWindow = new CopyProgressWindow { Owner = this };
            progressWindow.Show();
            try
            {
                await fileOperationsService.Copy(ActivePane, InactivePane, items, this, progressWindow.Progress, progressWindow.Cancellation.Token, confirm: false);
            }
            catch (OperationCanceledException)
            {
                Logger.Log("Copy operation cancelled");
            }
            finally
            {
                progressWindow.Close();
            }
        }

        private async Task MoveSelectedAsync()
        {
            Logger.Log("Move clicked");
            var items = ActiveList.SelectedItems.Cast<FileSystemInfo>().Where(i => i is not ParentDirectoryInfo).ToList();
            if (Settings.MoveConfirmation)
            {
                var confirmed = new List<FileSystemInfo>();
                foreach (var item in items)
                {
                    string target = Path.Combine(InactivePane.CurrentDir.FullName, item.Name);
                    var result = MessageBox.Show(this,
                        Localization.Get("Confirm_Move", item.FullName, target),
                        Localization.Get("Confirm_Move_Title"),
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                        confirmed.Add(item);
                }
                items = confirmed;
            }
            if (items.Count == 0)
                return;

            var progressWindow = new CopyProgressWindow { Owner = this };
            progressWindow.Show();
            try
            {
                await fileOperationsService.Move(ActivePane, InactivePane, items, this, progressWindow.Progress, progressWindow.Cancellation.Token, confirm: false);
            }
            catch (OperationCanceledException)
            {
                Logger.Log("Move operation cancelled");
            }
            finally
            {
                progressWindow.Close();
            }
        }

        private void RenameSelected()
        {
            Logger.Log("Rename clicked");
            if (ActiveList.SelectedItems.Count == 1 && ActiveList.SelectedItem is FileSystemInfo item && item is not ParentDirectoryInfo)
            {
                fileOperationsService.Rename(ActivePane, item, this);
            }
        }

        private async void Copy_Click(object sender, RoutedEventArgs e) => await CopySelectedAsync();

        private async void Move_Click(object sender, RoutedEventArgs e) => await MoveSelectedAsync();

        private void Rename_Click(object sender, RoutedEventArgs e) => RenameSelected();

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("Delete clicked");
            var items = ActiveList.SelectedItems.Cast<FileSystemInfo>().Where(i => i is not ParentDirectoryInfo).ToList();
            fileOperationsService.Delete(ActivePane, items, this);
        }

        private void List_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Logger.Log($"List key pressed: {e.Key}");
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

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (sender is GridViewColumnHeader header && header.Tag is string tag &&
                Enum.TryParse<FilePaneViewModel.SortField>(tag, out var field))
            {
                var listView = FindAncestor<ListView>(header);
                if (listView != null)
                {
                    var pane = listView == LeftList ? leftPane : rightPane;
                    pane.Sort(field);
                }
            }
        }

        private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T target)
                    return target;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

    }
}
