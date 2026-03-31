using _02_AuditFlowApplication.Helpers;
using _02_AuditFlowApplication.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace _02_AuditFlowApplication.Views
{
    public partial class LoginView : UserControl
    {
        private readonly LoginViewModel _viewModel;

        public LoginView()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;

            // Enter on username moves to password
            UsernameTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Return)
                {
                    PasswordBox.Focus();
                    e.Handled = true;
                }
            };

            // Enter on password triggers login
            PasswordBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Return)
                {
                    LoginButton_Click(s, e);
                    e.Handled = true;
                }
            };
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var (success, user, errorMessage) = _viewModel.Login(
                UsernameTextBox.Text,
                PasswordBox.Password);

            if (success)
            {
                Application.Current.Properties["CurrentUser"] = user;
                NavigationHelper.NavigateToDashboard();
            }
            else
            {
                var image = errorMessage.Contains("Manager")
                    ? MessageBoxImage.Warning
                    : MessageBoxImage.Error;

                var title = errorMessage.Contains("Manager")
                    ? "Access Denied"
                    : "Login Failed";

                MessageBox.Show(errorMessage, title, MessageBoxButton.OK, image);
            }
        }

        private void ManagerLoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.ManagerLogin();
        }
    }
}