using System;
using System.Threading;
using System.Windows;

namespace DamnSimpleFileManager.Windows
{
    public partial class CopyProgressWindow : Window
    {
        public IProgress<double> Progress { get; }
        public CancellationTokenSource Cancellation { get; } = new();

        public CopyProgressWindow()
        {
            InitializeComponent();
            Progress = new Progress<double>(v => ProgressBar.Value = v);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Cancellation.Cancel();
        }
    }
}
