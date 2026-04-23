using _02_AuditFlowApplication.Helpers;
using _02_AuditFlowApplication.ViewModels;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;

namespace _02_AuditFlowApplication.Views
{
    public partial class LoginView : UserControl
    {
        private readonly LoginViewModel _viewModel;
        private bool _passwordVisible = false;

        public LoginView()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;

            UsernameTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Return)
                {
                    PasswordBox.Focus();
                    e.Handled = true;
                }
            };

            PasswordBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Return)
                {
                    LoginButton_Click(s, e);
                    e.Handled = true;
                }
            };

            PasswordTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Return)
                {
                    LoginButton_Click(s, e);
                    e.Handled = true;
                }
            };
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = _passwordVisible ? PasswordTextBox.Text : PasswordBox.Password;

            var (success, user, errorMessage) = _viewModel.Login(username, password);

            if (success)
            {
                Application.Current.Properties["CurrentUser"] = user;
                NavigationHelper.NavigateToDashboard();
            }
            else
            {
                MessageBox.Show(errorMessage, "Login Failed",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _passwordVisible = !_passwordVisible;

            if (_passwordVisible)
            {
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Focus();
                PasswordTextBox.CaretIndex = PasswordTextBox.Text.Length;

                TogglePasswordIcon.Source = new Uri("/Resources/Svg/eye-slash-solid-full.svg", UriKind.Relative);
                TogglePasswordButton.SetValue(AutomationProperties.NameProperty, "Hide password");
            }
            else
            {
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Focus();

                TogglePasswordIcon.Source = new Uri("/Resources/Svg/eye-solid-full.svg", UriKind.Relative);
                TogglePasswordButton.SetValue(AutomationProperties.NameProperty, "Show password");
            }
        }

        private void ManagerLoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.ManagerLogin();
        }
    }
}