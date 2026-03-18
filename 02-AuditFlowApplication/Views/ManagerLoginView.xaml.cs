using _02_AuditFlowApplication.Helpers;
using _02_AuditFlowApplication.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace _02_AuditFlowApplication.Views
{
    public partial class ManagerLoginView : UserControl
    {
        private readonly ManagerLoginViewModel _viewModel;

        public ManagerLoginView()
        {
            InitializeComponent();
            _viewModel = new ManagerLoginViewModel();
            DataContext = _viewModel;
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
                var image = errorMessage.Contains("Auditor")
                    ? MessageBoxImage.Warning
                    : MessageBoxImage.Error;

                var title = errorMessage.Contains("Auditor")
                    ? "Access Denied"
                    : "Login Failed";

                MessageBox.Show(errorMessage, title, MessageBoxButton.OK, image);
            }
        }

        private void AuditorLoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.ShowLogin();
        }
    }
}