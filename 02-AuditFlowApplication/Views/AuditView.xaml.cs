using _02_AuditFlowApplication.Helpers;
using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Services;
using _02_AuditFlowApplication.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace _02_AuditFlowApplication.Views
{
    public partial class AuditView : UserControl
    {
        private readonly AuditService _auditService;
        private readonly AuditViewModel _auditViewModel;

        public AuditView()
        {
            InitializeComponent();
            _auditService = new AuditService();
            _auditViewModel = new AuditViewModel();
            DataContext = _auditViewModel;
            LoadAudits();

            NavigationHelper.WireAuditorNavigation(DashboardButton, AuditsButton, TasksButton, LogoutButton);
        }

        private void LoadAudits()
        {
            try
            {
                var currentUser = Application.Current.Properties["CurrentUser"] as User;
                if (currentUser == null)
                {
                    MessageBox.Show("No user logged in.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var taskService = new TaskService();
                var userTasks = taskService.GetAllTasks()
                    .Where(t => t.AssignedToUserId == currentUser.UserID)
                    .ToList();

                var assignedAuditIds = userTasks
                    .Select(t => t.AuditId)
                    .Distinct()
                    .ToHashSet();

                var allAudits = _auditService.GetAllAudits();
                var assignedAudits = allAudits
                    .Where(a => assignedAuditIds.Contains(a.AuditId))
                    .ToList();

                _auditViewModel.LoadAudits(assignedAudits);
                AuditsGrid.ItemsSource = _auditViewModel.FilteredAudits;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading audits: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StatusFilterBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_auditViewModel == null || StatusFilterBox.SelectedItem == null) return;

            var selectedItem = (ComboBoxItem)StatusFilterBox.SelectedItem;
            string selectedStatus = selectedItem.Content.ToString();

            _auditViewModel.SelectedStatus = selectedStatus switch
            {
                "Not Started" => AuditStatus.NotStarted,
                "In Progress" => AuditStatus.InProgress,
                "Completed" => AuditStatus.Completed,
                "Overdue" => AuditStatus.Overdue,
                _ => null
            };

            AuditsGrid.ItemsSource = _auditViewModel.FilteredAudits;
        }

        private void TypeFilterBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_auditViewModel == null || TypeFilterBox.SelectedItem == null) return;

            var selectedItem = (ComboBoxItem)TypeFilterBox.SelectedItem;
            string selectedType = selectedItem.Content.ToString();

            _auditViewModel.SelectedType = selectedType switch
            {
                "Security" => AuditType.Security,
                "Safety" => AuditType.Safety,
                "Quality" => AuditType.Quality,
                "Data Protection" => AuditType.DataProtection,
                "Financial" => AuditType.Financial,
                _ => null
            };

            AuditsGrid.ItemsSource = _auditViewModel.FilteredAudits;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim();
            var suggestions = _auditViewModel.GetSearchSuggestions(searchText);

            if (string.IsNullOrEmpty(searchText))
            {
                SearchPopup.IsOpen = false;
                _auditViewModel.SearchText = string.Empty;
                AuditsGrid.ItemsSource = _auditViewModel.FilteredAudits;
                return;
            }

            if (suggestions.Any())
            {
                SearchResultsListBox.ItemsSource = suggestions;
                SearchPopup.IsOpen = true;
            }
            else
            {
                SearchPopup.IsOpen = false;
            }

            _auditViewModel.SearchText = searchText;
            AuditsGrid.ItemsSource = _auditViewModel.FilteredAudits;
        }

        private void SearchResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultsListBox.SelectedItem is Audit selectedAudit)
            {
                AuditsGrid.ItemsSource = new List<Audit> { selectedAudit };
                SearchTextBox.Text = selectedAudit.AuditName;
                SearchPopup.IsOpen = false;
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SearchTextBox.Text))
                SearchTextBox_TextChanged(sender, null);
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            using Task _ = Task.Delay(200).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() => SearchPopup.IsOpen = false);
            });
        }
    }
}
