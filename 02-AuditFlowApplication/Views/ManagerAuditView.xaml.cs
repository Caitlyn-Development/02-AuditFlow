using _02_AuditFlowApplication.Helpers;
using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace _02_AuditFlowApplication.Views
{
    public partial class ManagerAuditView : UserControl
    {
        private readonly ManagerAuditViewModel _viewModel;

        public ManagerAuditView()
        {
            InitializeComponent();
            _viewModel = new ManagerAuditViewModel();
            DataContext = _viewModel;
            LoadAudits();

            NavigationHelper.WireManagerNavigation(DashboardButton, AuditsButton, TasksButton, UsersButton, LogoutButton);
        }

        private void LoadAudits()
        {
            try
            {
                _viewModel.LoadAudits();
                AuditsGrid.ItemsSource = _viewModel.FilteredAudits;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading audits: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StatusFilterBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel == null || StatusFilterBox.SelectedItem == null) return;

            var selectedItem = (ComboBoxItem)StatusFilterBox.SelectedItem;
            string selectedStatus = selectedItem.Content.ToString();

            _viewModel.SelectedStatus = selectedStatus switch
            {
                "Not Started" => AuditStatus.NotStarted,
                "In Progress" => AuditStatus.InProgress,
                "Completed" => AuditStatus.Completed,
                "Overdue" => AuditStatus.Overdue,
                _ => null
            };

            AuditsGrid.ItemsSource = _viewModel.FilteredAudits;
        }

        private void TypeFilterBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel == null || TypeFilterBox.SelectedItem == null) return;

            var selectedItem = (ComboBoxItem)TypeFilterBox.SelectedItem;
            string selectedType = selectedItem.Content.ToString();

            _viewModel.SelectedType = selectedType switch
            {
                "Security" => AuditType.Security,
                "Safety" => AuditType.Safety,
                "Quality" => AuditType.Quality,
                "Data Protection" => AuditType.DataProtection,
                "Financial" => AuditType.Financial,
                _ => null
            };

            AuditsGrid.ItemsSource = _viewModel.FilteredAudits;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim();
            var suggestions = _viewModel.GetSearchSuggestions(searchText);

            if (string.IsNullOrEmpty(searchText))
            {
                SearchPopup.IsOpen = false;
                _viewModel.SearchText = string.Empty;
                AuditsGrid.ItemsSource = _viewModel.FilteredAudits;
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

            _viewModel.SearchText = searchText;
            AuditsGrid.ItemsSource = _viewModel.FilteredAudits;
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
            System.Threading.Tasks.Task.Delay(200).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() => SearchPopup.IsOpen = false);
            });
        }

        private void RecurringCheckbox_Checked(object sender, RoutedEventArgs e)
            => RecurrencePanel.Visibility = Visibility.Visible;

        private void RecurringCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            RecurrencePanel.Visibility = Visibility.Collapsed;
            RecurrenceComboBox.SelectedIndex = -1;
        }

        private bool IsDescendantOf(DependencyObject element, DependencyObject parent)
        {
            while (element != null)
            {
                if (element == parent) return true;
                element = VisualTreeHelper.GetParent(element);
            }
            return false;
        }

        private void StatusBorder_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var audit = border?.DataContext as Audit;
            if (audit == null) return;

            e.Handled = true;

            var popup = new Popup
            {
                PlacementTarget = border,
                Placement = PlacementMode.Bottom,
                StaysOpen = true,
                AllowsTransparency = true
            };

            var container = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(4)
            };
            container.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 10,
                Opacity = 0.2,
                ShadowDepth = 2
            };

            MouseButtonEventHandler outsideClickHandler = null;
            outsideClickHandler = (s, args) =>
            {
                var clickedElement = args.OriginalSource as DependencyObject;
                if (!IsDescendantOf(clickedElement, container))
                {
                    popup.IsOpen = false;
                    Window.GetWindow(this).PreviewMouseDown -= outsideClickHandler;
                }
            };

            popup.Opened += (s, args) =>
                Window.GetWindow(this).PreviewMouseDown += outsideClickHandler;

            popup.Closed += (s, args) =>
                Window.GetWindow(this).PreviewMouseDown -= outsideClickHandler;

            var panel = new StackPanel();

            var statuses = new[]
            {
                ("Not Started", AuditStatus.NotStarted),
                ("In Progress", AuditStatus.InProgress),
                ("Completed", AuditStatus.Completed),
                ("Overdue", AuditStatus.Overdue)
            };

            foreach (var (label, status) in statuses)
            {
                var capturedStatus = status;
                var btn = new Button
                {
                    Content = label,
                    FontFamily = new FontFamily("Verdana"),
                    FontSize = 14,
                    Padding = new Thickness(15, 8, 15, 8),
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Width = 150,
                    IsEnabled = audit.Status != status
                };

                btn.Click += (s, args) =>
                {
                    var (success, error) = _viewModel.UpdateAuditStatus(audit, capturedStatus);
                    if (!success)
                        MessageBox.Show($"Error updating status: {error}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        AuditsGrid.ItemsSource = _viewModel.FilteredAudits;

                    popup.IsOpen = false;
                    Window.GetWindow(this).PreviewMouseDown -= outsideClickHandler;
                };

                panel.Children.Add(btn);
            }

            container.Child = panel;
            popup.Child = container;
            popup.IsOpen = true;
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

                var currentUser = Application.Current.Properties["CurrentUser"] as User;

                var (success, errorMessage) = _viewModel.SaveAudit(
                    auditName, auditType, startDate, endDate,
                    isRecurring, recurrence, currentUser.UserID);

                if (!success)
                {
                    MessageBox.Show(errorMessage, "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _viewModel.LogAuditCreation(currentUser, auditName);

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
    }
}