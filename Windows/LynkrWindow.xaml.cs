using System.Windows;
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
            if (linkManager.AddLink(UrlTextBox.Text))
            {
                if (!string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
                    linkManager.SetDescription(UrlTextBox.Text.Trim(), DescriptionTextBox.Text.Trim());
                UrlTextBox.Clear();
                DescriptionTextBox.Clear();
                LinksList.Items.Refresh();
            }
            else
            {
                MessageBox.Show(this, "Invalid URL", "Lynkr");
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
