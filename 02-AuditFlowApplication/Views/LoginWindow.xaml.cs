using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Services;
using System.Windows;
using System.Windows.Controls;

namespace _02_AuditFlowApplication.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthenticationService _authService;

        public LoginWindow()
        {
            InitializeComponent();
            _authService = new AuthenticationService();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter username and password.", "Login Failed",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            User user = _authService.AuthenticateUser(username, password);

            if (user != null)
            {
                // Store current user in application properties
                Application.Current.Properties["CurrentUser"] = user;

                // Open main window
                var mainWindow = new MainWindow();
                mainWindow.Show();

                // Close login window
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Login Failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}