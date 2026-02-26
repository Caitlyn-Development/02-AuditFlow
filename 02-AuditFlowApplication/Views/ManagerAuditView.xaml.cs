using _02_AuditFlowApplication.Helpers;
using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _02_AuditFlowApplication.Views
{
    /// <summary>
    /// Interaction logic for ManagerAuditView.xaml
    /// </summary>
    public partial class ManagerAuditView : UserControl
    {
        private readonly AuditService _auditService;
        private List<Audit> _allAudits;
        private AuditType? _selectedType = null;
        private readonly AuditStatus? _selectedStatus = null;

        public ManagerAuditView()
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

        private void RecurringCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            RecurrencePanel.Visibility = Visibility.Visible;
        }

        private void RecurringCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            RecurrencePanel.Visibility = Visibility.Collapsed;
            RecurrenceComboBox.SelectedIndex = -1;
        }

        private AuditType ParseAuditType(string type)
        {
            return type switch
            {
                "Security" => AuditType.Security,
                "Safety" => AuditType.Safety,
                "Quality" => AuditType.Quality,
                "Data Protection" => AuditType.DataProtection,
                "Financial" => AuditType.Financial,
                _ => AuditType.Unknown
            };
        }

        private RecurrenceFrequency? ParseRecurrenceFrequency(string frequency)
        {
            return frequency switch
            {
                "Weekly" => RecurrenceFrequency.Weekly,
                "Monthly" => RecurrenceFrequency.Monthly,
                "Quarterly" => RecurrenceFrequency.Quarterly,
                "Annual" => RecurrenceFrequency.Annual,
                _ => null
            };
        }

        private void SaveAudit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string auditName = AuditNameBox.Text.Trim();
                string auditType = (AuditTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                DateTime? startDate = StartDatePicker.SelectedDate;
                DateTime? endDate = EndDatePicker.SelectedDate;
                bool isRecurring = RecurringCheckbox.IsChecked == true;
                string recurrence = (RecurrenceComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                if (string.IsNullOrWhiteSpace(auditName) || auditType == null || startDate == null || endDate == null)
                {
                    MessageBox.Show("Audit Name, Type, Start Date and End Date are all required.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (endDate <= startDate)
                {
                    MessageBox.Show("End Date must be after Start Date.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (isRecurring && string.IsNullOrEmpty(recurrence))
                {
                    MessageBox.Show("Please select a recurrence frequency.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_auditService.AuditNameExists(auditName))
                {
                    MessageBox.Show($"An audit with the name '{auditName}' already exists.", "Duplicate Audit",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var currentUser = Application.Current.Properties["CurrentUser"] as User;

                var newAudit = new Audit
                {
                    AuditName = auditName,
                    Type = ParseAuditType(auditType),
                    StartDate = startDate.Value,
                    EndDate = endDate.Value,
                    IsRecurring = isRecurring,
                    RecurrenceType = isRecurring ? ParseRecurrenceFrequency(recurrence) : null,
                    Status = AuditStatus.NotStarted,
                    CreatedByUserID = currentUser.UserID,
                    CreatedDate = DateTime.Now
                };

                _auditService.CreateAudit(newAudit);
                _auditService.LogAuditCreation(currentUser, newAudit);

                MessageBox.Show($"Audit '{auditName}' created successfully.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                AuditNameBox.Text = string.Empty;
                AuditTypeComboBox.SelectedIndex = -1;
                StartDatePicker.SelectedDate = null;
                EndDatePicker.SelectedDate = null;
                RecurringCheckbox.IsChecked = false;
                RecurrenceComboBox.SelectedIndex = -1;

                LoadAudits();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving audit: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToManagerDash();
        }

        private void AuditsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToManagerAudits();
        }

        private void TasksButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToManagerTasks();
        }

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToManagerUsers();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Logout();
        }
    }
}
