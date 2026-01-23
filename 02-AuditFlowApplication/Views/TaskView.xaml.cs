using _02_AuditFlowApplication.Helpers;
using System.Collections.Generic;
using System.Windows;

namespace _02_AuditFlowApplication.Views
{
    public partial class TaskView : Window
    {
        private DateTime currentMonth;
        private Dictionary<DateTime, List<string>> auditEvents;

        public TaskView()
        {
            InitializeComponent();
  
            InitializeAuditEvents();
        }

        private void InitializeAuditEvents()
        {
            // Initialize sample audit events to match wireframe
            auditEvents = new Dictionary<DateTime, List<string>>
            {
                { new DateTime(2026, 10, 2), new List<string> { "Risk Compliance" } },
                { new DateTime(2026, 10, 13), new List<string> { "Supplier Assess" } },
                { new DateTime(2026, 10, 22), new List<string> { "IT Systems Audit" } }
            };
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToDashboard(this);
        }

        private void AuditsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToAudits(this);
        }

        private void TasksButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToTasks(this);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Logout(this);
        }
    }
}
