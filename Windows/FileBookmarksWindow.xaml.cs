using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DamnSimpleFileManager.Utils;

namespace DamnSimpleFileManager.Windows
{
    public partial class FileBookmarksWindow : Window
    {
        private readonly FileBookmarkManager bookmarkManager = new();

        public FileBookmarksWindow()
        {
            InitializeComponent();
            ApplyLocalization();
            BookmarksList.ItemsSource = bookmarkManager.GetAllBookmarks();
        }

        private void ApplyLocalization()
        {
            Title = Localization.Get("FileBookmarks_Title");
            PathTextBox.Text = Localization.Get("FileBookmarks_PathPlaceholder");
            PathTextBox.Tag = PathTextBox.Text;
            DescriptionTextBox.Text = Localization.Get("FileBookmarks_DescriptionPlaceholder");
            DescriptionTextBox.Tag = DescriptionTextBox.Text;
            AddButton.Content = Localization.Get("FileBookmarks_Button_Add");
            BookmarkCurrentButton.Content = Localization.Get("FileBookmarks_Button_AddCurrent");
            PathColumn.Header = Localization.Get("FileBookmarks_Column_Path");
            DescriptionColumn.Header = Localization.Get("FileBookmarks_Column_Description");
            AvailableColumn.Header = Localization.Get("FileBookmarks_Column_Available");
            OpenButton.Content = Localization.Get("FileBookmarks_Button_Open");
            DeleteButton.Content = Localization.Get("FileBookmarks_Button_Delete");
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.Text == tb.Tag?.ToString())
            {
                tb.Clear();
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = tb.Tag?.ToString();
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var path = PathTextBox.Text.Trim();
            if (path == (string)PathTextBox.Tag)
                path = string.Empty;
            var description = DescriptionTextBox.Text.Trim();
            if (description == (string)DescriptionTextBox.Tag)
                description = string.Empty;
            if (bookmarkManager.AddBookmark(path, string.IsNullOrWhiteSpace(description) ? null : description))
            {
                PathTextBox.Text = (string)PathTextBox.Tag;
                DescriptionTextBox.Text = (string)DescriptionTextBox.Tag;
                BookmarksList.Items.Refresh();
            }
        }

        private void BookmarkCurrent_Click(object sender, RoutedEventArgs e)
        {
            if (Owner is MainWindow main)
            {
                var path = main.GetCurrentLeftPath();
                var description = DescriptionTextBox.Text.Trim();
                if (description == (string)DescriptionTextBox.Tag)
                    description = string.Empty;
                if (bookmarkManager.AddBookmark(path, string.IsNullOrWhiteSpace(description) ? null : description))
                {
                    DescriptionTextBox.Text = (string)DescriptionTextBox.Tag;
                    BookmarksList.Items.Refresh();
                }
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (BookmarksList.SelectedItem is FileBookmarkItem item && Owner is MainWindow main)
            {
                if (Directory.Exists(item.Path) || File.Exists(item.Path))
                {
                    main.OpenPathFromBookmark(item.Path);
                    Close();
                }
                else
                {
                    MessageBox.Show(this, Localization.Get("FileBookmarks_Error_PathNotFound"), Localization.Get("FileBookmarks_Title"));
                }
            }
        }

        private void BookmarksList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Open_Click(sender, e);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (BookmarksList.SelectedItem is FileBookmarkItem item)
            {
                bookmarkManager.DeleteBookmark(item.Path);
                BookmarksList.Items.Refresh();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
