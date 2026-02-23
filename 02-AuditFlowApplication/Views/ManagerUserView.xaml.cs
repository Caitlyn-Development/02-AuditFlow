using _02_AuditFlowApplication.Helpers;
using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Services;
using System.Windows;
using System.Windows.Controls;

namespace _02_AuditFlowApplication.Views
{
    /// <summary>
    /// Interaction logic for ManagerAuditView.xaml
    /// </summary>
    public partial class ManagerUserView : UserControl
    {
        private readonly UserService _userService;
        private List<User> _allUsers;

        public ManagerUserView()
        {
            InitializeComponent();
            _userService = new UserService();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                _allUsers = _userService.GetAllUsers();
                UsersGrid.ItemsSource = _allUsers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToManagerDash();
        }

        private void AuditsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToManagerAudits();
        }

        private void TasksButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToManagerTasks();
        }

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToManagerUsers();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Logout();
        }
    }
}
