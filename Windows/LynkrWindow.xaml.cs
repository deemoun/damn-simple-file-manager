using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using DamnSimpleFileManager.Utils;

namespace DamnSimpleFileManager.Windows
{
    public partial class LynkrWindow : Window
    {
        private readonly LinkManager linkManager = new();

        public LynkrWindow()
        {
            InitializeComponent();
            LinksList.ItemsSource = linkManager.GetAllLinks();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var url = UrlTextBox.Text.Trim();
            if (url == (string)UrlTextBox.Tag)
                url = string.Empty;

            if (linkManager.AddLink(url))
            {
                var description = DescriptionTextBox.Text.Trim();
                if (description != (string)DescriptionTextBox.Tag && !string.IsNullOrWhiteSpace(description))
                    linkManager.SetDescription(url, description);
                UrlTextBox.Text = (string)UrlTextBox.Tag;
                DescriptionTextBox.Text = (string)DescriptionTextBox.Tag;
                LinksList.Items.Refresh();
            }
            else
            {
                MessageBox.Show(this, "Invalid URL", "Lynkr");
            }
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

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (LinksList.SelectedItem is LinkItem item)
            {
                linkManager.OpenLink(item.Url);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (LinksList.SelectedItem is LinkItem item)
            {
                linkManager.DeleteLink(item.Url);
                LinksList.Items.Refresh();
            }
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" };
            if (dialog.ShowDialog() == true)
            {
                linkManager.ImportFromFile(dialog.FileName);
                LinksList.ItemsSource = linkManager.GetAllLinks();
                LinksList.Items.Refresh();
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { Filter = "JSON files (*.json)|*.json", FileName = "links.json" };
            if (dialog.ShowDialog() == true)
            {
                linkManager.ExportToFile(dialog.FileName);
            }
        }
    }
}
