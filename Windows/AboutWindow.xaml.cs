using System.Windows;
using System.Diagnostics;
using System.Windows.Navigation;
using System.Reflection;

namespace DamnSimpleFileManager.Windows
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            var v = Assembly.GetExecutingAssembly().GetName().Version;
            var version = v is null ? "1.0.0" : $"{v.Major}.{v.Minor}.{v.Build}";
            AppNameTextBlock.Text += $" v{version}";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
