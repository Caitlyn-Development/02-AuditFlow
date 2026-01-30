using _02_AuditFlowApplication.Helpers;
using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Services;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace _02_AuditFlowApplication.Views
{
    public partial class TaskView : UserControl
    {
        private readonly TaskService _taskService;
        private List<AuditTask> _allTasks;
        //private AuditName? _selectedType = null;
        private readonly AuditTaskStatus? _selectedStatus = null;

        public TaskView()
        {
            InitializeComponent();
            _taskService = new TaskService();
            LoadTasks();

        }
        private void LoadTasks()
        {
            try
            {
                _allTasks = _taskService.GetAllTasks();
                TasksGrid.ItemsSource = _allTasks;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tasks: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void StatusFilterBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_allTasks == null || StatusFilterBox.SelectedItem == null)
                return;

            var selectedItem = (ComboBoxItem)StatusFilterBox.SelectedItem;
            string selectedStatus = selectedItem.Content.ToString();

            List<AuditTask> filteredTasks;

            switch (selectedStatus)
            {
                case "Not Started":
                    filteredTasks = _allTasks.Where(a => a.Status == AuditTaskStatus.NotStarted).ToList();
                    break;
                case "In Progress":
                    filteredTasks = _allTasks.Where(a => a.Status == AuditTaskStatus.InProgress).ToList();
                    break;
                case "On Hold":
                    filteredTasks = _allTasks.Where(a => a.Status == AuditTaskStatus.OnHold).ToList();
                    break;
                case "Completed":
                    filteredTasks = _allTasks.Where(a => a.Status == AuditTaskStatus.Completed).ToList();
                    break;
                case "Overdue":
                    filteredTasks = _allTasks.Where(a => a.Status == AuditTaskStatus.Overdue).ToList();
                    break;
                default: // "All Status"
                    filteredTasks = _allTasks;
                    break;
            }

            TasksGrid.ItemsSource = filteredTasks;
        }

        private void ApplyFilters()
        {
            if (_allTasks == null)
                return;

            var filteredTasks = _allTasks.AsEnumerable();

            //if (_selectedType.HasValue)
            //{
            //    filteredTasks = filteredTasks.Where(a => a.Type == _selectedType.Value);
            //}

            if (_selectedStatus.HasValue)
            {
                filteredTasks = filteredTasks.Where(a => a.Status == _selectedStatus.Value);
            }

            TasksGrid.ItemsSource = filteredTasks.ToList();
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
