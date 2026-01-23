using _02_AuditFlowApplication.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace _02_AuditFlowApplication.Helpers
{
    public static class NavigationHelper
    {
        public static void NavigateToDashboard(Window currentWindow)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            currentWindow.Close();
        }

        public static void NavigateToManagerDash(Window currentWindow)
        {
            ManagerDashboard managerDashboard = new ManagerDashboard();
            managerDashboard.Show();
            currentWindow.Close();
        }

        public static void NavigateToAudits(Window currentWindow)
        {
            Views.AuditView auditView = new Views.AuditView();
            auditView.Show();
            currentWindow.Close();
        }

        public static void NavigateToTasks(Window currentWindow)
        {
            Views.TaskView taskView = new Views.TaskView();
            taskView.Show();
            currentWindow.Close();
        }

        public static void Logout(Window currentWindow)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            currentWindow.Close();
        }

        public static void AuditorLogin(Window currentWindow)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            currentWindow.Close();
        }

        public static void ManagerLogin(Window currentWindow)
        {
            ManagerLoginWindow managerLoginWindow = new ManagerLoginWindow();
            managerLoginWindow.Show();
            currentWindow.Close();
        }
    }
}
