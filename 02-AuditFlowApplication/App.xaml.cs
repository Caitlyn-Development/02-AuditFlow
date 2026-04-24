using _02_AuditFlowApplication.Data;
using System.Windows;

namespace _02_AuditFlowApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DatabaseHelper.InitialiseDatabase();
            DatabaseHelper.UpdateOverdueStatuses();

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
