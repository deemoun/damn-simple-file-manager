using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DamnSimpleFileManager.Windows;

public partial class InputDialog : Window
{
    public InputDialog()
    {
        InitializeComponent();
    }

    public string InputText
    {
        get => InputTextBox.Text;
        set => InputTextBox.Text = value;
    }

    public string Message
    {
        set => MessageText.Text = value;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        OkButton.IsEnabled = !string.IsNullOrWhiteSpace(InputTextBox.Text);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        InputTextBox.Focus();
        InputTextBox.SelectAll();
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            DialogResult = false;
        }
    }
}
