using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Views;
using System.Windows;
using System.Windows.Controls;

namespace _02_AuditFlowApplication.Helpers
{
    public static class NavigationHelper
    {
        private static ContentControl? _contentControl;
        private static UserControl? _currentView;

        public static void Initialise(ContentControl contentControl)
        {
            _contentControl = contentControl;
        }

        public static void NavigateToView(UserControl newView)
        {
            if (_contentControl == null)
                throw new InvalidOperationException("Navigation Helper not initialised");
            _currentView = newView;
            _contentControl.Content = newView;
        }

        private static void NavigateSafely(Func<UserControl> viewFactory)
        {
            try
            {
                NavigateToView(viewFactory());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\nInner: {ex.InnerException?.Message}",
                    "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void NavigateToDashboard()
        {
            if (Application.Current.Properties["CurrentUser"] is not User user)
            {
                NavigateSafely(() => new LoginView());
                return;
            }

            NavigateSafely(() => user.Role == UserRole.Manager
                ? new ManagerDashboardView()
                : (UserControl)new DashboardView());
        }

        public static void NavigateToManagerDash() => NavigateSafely(() => new ManagerDashboardView());
        public static void NavigateToAudits() => NavigateSafely(() => new AuditView());
        public static void NavigateToManagerAudits() => NavigateSafely(() => new ManagerAuditView());
        public static void NavigateToTasks() => NavigateSafely(() => new TaskView());
        public static void NavigateToManagerTasks() => NavigateSafely(() => new ManagerTaskView());
        public static void NavigateToManagerUsers() => NavigateSafely(() => new ManagerUserView());
        public static void Logout() => NavigateSafely(() => new LoginView());
        public static void ShowLogin() => NavigateSafely(() => new LoginView());
        public static void ManagerLogin() => NavigateSafely(() => new ManagerLoginView());

        public static void WireAuditorNavigation(
            Button? dashboardButton = null,
            Button? auditsButton = null,
            Button? tasksButton = null,
            Button? logoutButton = null)
        {
            if (dashboardButton != null) dashboardButton.Click += (s, e) => NavigateToDashboard();
            if (auditsButton != null) auditsButton.Click += (s, e) => NavigateToAudits();
            if (tasksButton != null) tasksButton.Click += (s, e) => NavigateToTasks();
            if (logoutButton != null) logoutButton.Click += (s, e) => Logout();
        }

        public static void WireManagerNavigation(
            Button? dashboardButton = null,
            Button? auditsButton = null,
            Button? tasksButton = null,
            Button? usersButton = null,
            Button? logoutButton = null)
        {
            if (dashboardButton != null) dashboardButton.Click += (s, e) => NavigateToManagerDash();
            if (auditsButton != null) auditsButton.Click += (s, e) => NavigateToManagerAudits();
            if (tasksButton != null) tasksButton.Click += (s, e) => NavigateToManagerTasks();
            if (usersButton != null) usersButton.Click += (s, e) => NavigateToManagerUsers();
            if (logoutButton != null) logoutButton.Click += (s, e) => Logout();
        }
    }
}
