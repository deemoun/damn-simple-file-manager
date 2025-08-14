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
            ApplyLocalization();
            LinksList.ItemsSource = linkManager.GetAllLinks();
        }

        private void ApplyLocalization()
        {
            Title = Localization.Get("Lynkr_Title");
            UrlTextBox.Text = Localization.Get("Lynkr_UrlPlaceholder");
            UrlTextBox.Tag = UrlTextBox.Text;
            DescriptionTextBox.Text = Localization.Get("Lynkr_DescriptionPlaceholder");
            DescriptionTextBox.Tag = DescriptionTextBox.Text;
            AddButton.Content = Localization.Get("Lynkr_Button_Add");
            UrlColumn.Header = Localization.Get("Lynkr_Column_Url");
            DescriptionColumn.Header = Localization.Get("Lynkr_Column_Description");
            OpenButton.Content = Localization.Get("Lynkr_Button_Open");
            DeleteButton.Content = Localization.Get("Lynkr_Button_Delete");
            ImportButton.Content = Localization.Get("Lynkr_Button_Import");
            ExportButton.Content = Localization.Get("Lynkr_Button_Export");
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
                MessageBox.Show(this, Localization.Get("Lynkr_Error_InvalidUrl"), Localization.Get("Lynkr_Title"));
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
            var dialog = new OpenFileDialog { Filter = $"{Localization.Get("Lynkr_JsonFilter")} (*.json)|*.json" };
            if (dialog.ShowDialog() == true)
            {
                linkManager.ImportFromFile(dialog.FileName);
                LinksList.ItemsSource = linkManager.GetAllLinks();
                LinksList.Items.Refresh();
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { Filter = $"{Localization.Get("Lynkr_JsonFilter")} (*.json)|*.json", FileName = "links.json" };
            if (dialog.ShowDialog() == true)
            {
                linkManager.ExportToFile(dialog.FileName);
            }
        }
    }
}
