using _02_AuditFlowApplication.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace _02_AuditFlowApplication.Views
{
    public partial class AuditView : UserControl
    {

        public AuditView()
        {
            InitializeComponent();
            
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToDashboard();
        }

        private void AuditsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToAudits();
        }

        private void TasksButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToTasks();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Logout();
        }
    }
}
