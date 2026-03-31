using _02_AuditFlowApplication.Helpers;
using System.Windows.Controls;

namespace _02_AuditFlowApplication.Views
{
    public partial class ManagerLayout : UserControl
    {
        public ManagerLayout()
        {
            InitializeComponent();
            NavigationHelper.WireManagerNavigation(DashboardButton, AuditsButton, TasksButton, UsersButton, LogoutButton);
        }

        public void SetActiveButton(string activeButton)
        {
            DashboardButton.Tag = null;
            AuditsButton.Tag = null;
            TasksButton.Tag = null;
            UsersButton.Tag = null;

            switch (activeButton)
            {
                case "Dashboard": DashboardButton.Tag = "Selected"; break;
                case "Audits": AuditsButton.Tag = "Selected"; break;
                case "Tasks": TasksButton.Tag = "Selected"; break;
                case "Users": UsersButton.Tag = "Selected"; break;
            }
        }
    }
}
