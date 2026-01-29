using _02_AuditFlowApplication.Helpers;
using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Services;
using System.Windows;
using System.Windows.Controls;

namespace _02_AuditFlowApplication.Views
{
    public partial class AuditView : UserControl
    {
        private readonly AuditService _auditService;
        private List<Audit> _allAudits;
        private AuditType? _selectedType = null;
        private readonly AuditStatus? _selectedStatus = null;

        public AuditView()
        {
            InitializeComponent();
            _auditService = new AuditService();
            LoadAudits();
            
        }

        private void LoadAudits()
        {
            try
            {
                _allAudits = _auditService.GetAllAudits();
                AuditsGrid.ItemsSource = _allAudits;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading audits: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void StatusFilterBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_allAudits == null || StatusFilterBox.SelectedItem == null)
                return;

            var selectedItem = (ComboBoxItem)StatusFilterBox.SelectedItem;
            string selectedStatus = selectedItem.Content.ToString();

            List<Audit> filteredAudits;

            switch (selectedStatus)
            {
                case "Not Started":
                    filteredAudits = _allAudits.Where(a => a.Status == AuditStatus.NotStarted).ToList();
                    break;
                case "In Progress":
                    filteredAudits = _allAudits.Where(a => a.Status == AuditStatus.InProgress).ToList();
                    break;
                case "Completed":
                    filteredAudits = _allAudits.Where(a => a.Status == AuditStatus.Completed).ToList();
                    break;
                case "Overdue":
                    filteredAudits = _allAudits.Where(a => a.Status == AuditStatus.Overdue).ToList();
                    break;
                default: // "All Status"
                    filteredAudits = _allAudits;
                    break;
            }

            AuditsGrid.ItemsSource = filteredAudits;
        }

        private void TypeFilterBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_allAudits == null || TypeFilterBox.SelectedItem == null)
                return;

            var selectedItem = (ComboBoxItem)TypeFilterBox.SelectedItem;
            string selectedType = selectedItem.Content.ToString();

            _selectedType = selectedType switch
            {
                "Security" => AuditType.Security,
                "Safety" => AuditType.Safety,
                "Quality" => AuditType.Quality,
                "Data Protection" => AuditType.DataProtection,
                "Financial" => AuditType.Financial,
                _ => null
            };

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allAudits == null)
                return;

            var filteredAudits = _allAudits.AsEnumerable();

            if (_selectedType.HasValue)
            {
                filteredAudits = filteredAudits.Where(a => a.Type == _selectedType.Value);
            }

            if (_selectedStatus.HasValue)
            {
                filteredAudits = filteredAudits.Where(a => a.Status == _selectedStatus.Value);
            }

            AuditsGrid.ItemsSource = filteredAudits.ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                SearchPopup.IsOpen = false;
                ApplyFilters(); // Show all audits (or filtered audits)
                return;
            }

            // Search for audits matching the text
            var searchResults = _allAudits
                .Where(a => a.AuditName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (searchResults.Any())
            {
                SearchResultsListBox.ItemsSource = searchResults;
                SearchPopup.IsOpen = true;
            }
            else
            {
                // need to do pop saying no results found
                SearchPopup.IsOpen = false;
            }
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
            {
                SearchTextBox_TextChanged(sender, null);
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            System.Threading.Tasks.Task.Delay(200).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() => SearchPopup.IsOpen = false);
            });
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
