using System.Windows;
using _02_AuditFlowApplication.Helpers;

namespace _02_AuditFlowApplication
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            NavigationHelper.Initialise(ContentArea);
            NavigationHelper.ShowLogin();
        }

      
    }
}
