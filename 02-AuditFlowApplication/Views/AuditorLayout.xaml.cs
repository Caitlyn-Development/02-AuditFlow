using _02_AuditFlowApplication.Helpers;
using System.Windows.Controls;

namespace _02_AuditFlowApplication.Views
{
    public partial class AuditorLayout : UserControl
    {
        public AuditorLayout()
        {
            InitializeComponent();
            NavigationHelper.WireAuditorNavigation(DashboardButton, AuditsButton, TasksButton, LogoutButton);
        }

        public void SetActiveButton(string activeButton)
        {
            DashboardButton.Tag = null;
            AuditsButton.Tag = null;
            TasksButton.Tag = null;

            switch (activeButton)
            {
                case "Dashboard": DashboardButton.Tag = "Selected"; break;
                case "Audits": AuditsButton.Tag = "Selected"; break;
                case "Tasks": TasksButton.Tag = "Selected"; break;
            }
        }
    }
}