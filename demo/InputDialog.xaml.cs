using System.Windows;

namespace DesktopOrganizer
{
    public partial class InputDialog : Window
    {
        public string ResponseText { get; private set; }

        public InputDialog(string title, string message, string defaultResponse = "")
        {
            InitializeComponent();
            Title = title;
            MessageText.Text = message;
            ResponseTextBox.Text = defaultResponse;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ResponseText = ResponseTextBox.Text;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}