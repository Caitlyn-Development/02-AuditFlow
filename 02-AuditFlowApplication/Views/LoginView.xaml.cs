using _02_AuditFlowApplication.Helpers;
using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Services;
using System.Windows;
using System.Windows.Controls;

namespace _02_AuditFlowApplication.Views
{
    public partial class LoginView : UserControl
    {
        private readonly AuthenticationService _authService;

        public LoginView()
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
                if (user.Role != UserRole.Auditor)
                {
                    MessageBox.Show("Managers must use the Manager Login", "Acess Denied",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Application.Current.Properties["CurrentUser"] = user;
                NavigationHelper.NavigateToDashboard();
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Login Failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManagerLoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.ManagerLogin();
        }
    }
}