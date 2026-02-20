using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Views;
using System.Windows;
using System.Windows.Controls;

namespace _02_AuditFlowApplication.Helpers
{
    public static class NavigationHelper
    {

        private static ContentControl _contentControl;
        private static UserControl _currentView;

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

        public static void NavigateToDashboard()
        {
            var user = Application.Current.Properties["CurrentUser"] as User;

            if (user == null)
            {
                NavigateToView(new LoginView());
                return;
            }

            if (user.Role == UserRole.Manager)
                NavigateToView(new ManagerDashboardView());
            else
                NavigateToView(new DashboardView());
        }

        public static void NavigateToManagerDash()
        {
            NavigateToView(new ManagerDashboardView());
        }

        public static void NavigateToAudits()
        {
            NavigateToView(new AuditView());
        }

        public static void NavigateToManagerAudits()
        {
            NavigateToView(new ManagerAuditView());
        }

        public static void NavigateToTasks()
        {
            NavigateToView(new TaskView());
        }

        public static void NavigateToManagerTasks()
        {
            NavigateToView(new ManagerTaskView());
        }

        public static void Logout()
        {
            NavigateToView(new LoginView());
        }

        public static void ShowLogin()
        {
            NavigateToView(new LoginView());
        }

        public static void ManagerLogin()
        {
            NavigateToView(new ManagerLoginView());
        }
    }
}
