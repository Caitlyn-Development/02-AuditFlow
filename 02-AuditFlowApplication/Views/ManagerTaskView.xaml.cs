using _02_AuditFlowApplication.Data;
using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Services;
using _02_AuditFlowApplication.ViewModels;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace _02_AuditFlowApplication.Views
{
    public partial class ManagerTaskView : UserControl
    {
        private readonly ManagerTaskViewModel _viewModel;
        private List<string> _uploadedFiles = new List<string>();
        private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private readonly EvidenceService _evidenceService = new EvidenceService();
        private readonly TaskService _taskService = new TaskService();
        private readonly AuditService _auditService = new AuditService();

        public ManagerTaskView()
        {
            InitializeComponent();
            _viewModel = new ManagerTaskViewModel();
            DataContext = _viewModel;
            DatabaseHelper.UpdateOverdueStatuses();
            Layout.SetActiveButton("Tasks");
            LoadTasks();
            LoadUsers();
            LoadAudits();
            LoadReviewedEvidence();
            LoadPendingEvidence();
            LoadTaskCreationDropdowns();

        }

        private void LoadTasks()
        {
            try
            {
                _viewModel.LoadTasks();
                TasksGrid.ItemsSource = _viewModel.FilteredTasks;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tasks: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUsers()
        {
            var auditors = _viewModel.GetAuditors();

            UsersFilterBox.Items.Clear();
            UsersFilterBox.Items.Add(new ComboBoxItem
            {
                Content = "All Users",
                Style = (Style)FindResource("DefaultComboBoxItemStyle")
            });

            foreach (var user in auditors)
            {
                UsersFilterBox.Items.Add(new ComboBoxItem
                {
                    Content = user.FullName,
                    Style = (Style)FindResource("DefaultComboBoxItemStyle")
                });
            }

            UsersFilterBox.SelectedIndex = 0;
        }

        private void LoadAudits()
        {
            var auditNames = _viewModel.GetAuditNames();

            AuditFilterBox.Items.Clear();
            AuditFilterBox.Items.Add(new ComboBoxItem
            {
                Content = "All Audits",
                Style = (Style)FindResource("DefaultComboBoxItemStyle")
            });

            foreach (var audit in auditNames)
            {
                AuditFilterBox.Items.Add(new ComboBoxItem
                {
                    Content = audit,
                    Style = (Style)FindResource("DefaultComboBoxItemStyle")
                });
            }

            AuditFilterBox.SelectedIndex = 0;
        }

        private void StatusFilterBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel == null || StatusFilterBox.SelectedItem == null) return;

            var selectedItem = (ComboBoxItem)StatusFilterBox.SelectedItem;
            string selectedStatus = selectedItem.Content.ToString();

            _viewModel.SelectedStatus = selectedStatus switch
            {
                "Not Started" => AuditTaskStatus.NotStarted,
                "In Progress" => AuditTaskStatus.InProgress,
                "On Hold" => AuditTaskStatus.OnHold,
                "Completed" => AuditTaskStatus.Completed,
                "Overdue" => AuditTaskStatus.Overdue,
                _ => null
            };

            TasksGrid.ItemsSource = _viewModel.FilteredTasks;
        }

        private void AuditFilterBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel == null || AuditFilterBox.SelectedItem == null) return;

            var selectedItem = (ComboBoxItem)AuditFilterBox.SelectedItem;
            string selectedAudit = selectedItem.Content.ToString();

            _viewModel.SelectedAudit = selectedAudit == "All Audits" ? null : selectedAudit;
            TasksGrid.ItemsSource = _viewModel.FilteredTasks;
        }

        private void UsersFilterBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel == null || UsersFilterBox.SelectedItem == null) return;

            var selectedItem = (ComboBoxItem)UsersFilterBox.SelectedItem;
            string selectedUser = selectedItem.Content.ToString();

            _viewModel.SelectedUser = selectedUser == "All Users" ? null : selectedUser;
            TasksGrid.ItemsSource = _viewModel.FilteredTasks;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim();
            var suggestions = _viewModel.GetSearchSuggestions(searchText);

            if (string.IsNullOrEmpty(searchText))
            {
                SearchPopup.IsOpen = false;
                TasksGrid.ItemsSource = _viewModel.FilteredTasks;
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
        }

        private void SearchResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultsListBox.SelectedItem is AuditTask selectedTask)
            {
                TasksGrid.ItemsSource = new List<AuditTask> { selectedTask };
                SearchTextBox.Text = selectedTask.TaskName;
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

        private void StatusBorder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var task = border?.DataContext as AuditTask;
            if (task == null) return;

            e.Handled = true;

            var popup = new System.Windows.Controls.Primitives.Popup
            {
                PlacementTarget = border,
                Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom,
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

            System.Windows.Input.MouseButtonEventHandler outsideClickHandler = null;
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
                ("Not Started", AuditTaskStatus.NotStarted),
                ("In Progress", AuditTaskStatus.InProgress),
                ("On Hold", AuditTaskStatus.OnHold),
                ("Completed", AuditTaskStatus.Completed),
                ("Overdue", AuditTaskStatus.Overdue)
            };

            foreach (var (label, status) in statuses)
            {
                var capturedStatus = status;
                var btn = new Button
                {
                    Content = label,
                    FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                    FontSize = 14,
                    Padding = new Thickness(15, 8, 15, 8),
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Width = 150,
                    IsEnabled = task.Status != status
                };

                btn.Click += (s, args) =>
                {
                    var (success, error) = _viewModel.UpdateTaskStatus(task, capturedStatus);
                    if (!success)
                        MessageBox.Show($"Error updating status: {error}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                    {
                        TasksGrid.ItemsSource = _viewModel.FilteredTasks;
                        LoadTasks();
                    }

                    popup.IsOpen = false;
                    Window.GetWindow(this).PreviewMouseDown -= outsideClickHandler;
                };

                panel.Children.Add(btn);
            }

            container.Child = panel;
            popup.Child = container;
            popup.IsOpen = true;
        }

        private bool IsDescendantOf(DependencyObject element, DependencyObject parent)
        {
            while (element != null)
            {
                if (element == parent) return true;
                element = System.Windows.Media.VisualTreeHelper.GetParent(element);
            }
            return false;
        }

        private void LoadTaskCreationDropdowns()
        {
            TaskAuditComboBox.Items.Clear();
            var audits = _auditService.GetAllAudits();
            foreach (var audit in audits)
            {
                TaskAuditComboBox.Items.Add(new ComboBoxItem
                {
                    Content = audit.AuditName,
                    Tag = audit
                });
            }

            TaskAssignedToComboBox.Items.Clear();
            var auditors = _viewModel.GetAuditors();
            foreach (var auditor in auditors)
            {
                TaskAssignedToComboBox.Items.Add(new ComboBoxItem
                {
                    Content = auditor.FullName,
                    Tag = auditor.UserID
                });
            }
        }

        private void SaveTask_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskNameBox.Text))
            {
                MessageBox.Show("Task Name is required.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TaskAuditComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select an audit.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TaskAssignedToComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please assign the task to a user.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TaskDueDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Due Date is required.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedAudit = ((ComboBoxItem)TaskAuditComboBox.SelectedItem).Tag as Audit;
            var assignedToUserId = (int)((ComboBoxItem)TaskAssignedToComboBox.SelectedItem).Tag;

            var task = new AuditTask
            {
                TaskName = TaskNameBox.Text.Trim(),
                Description = TaskDescriptionBox.Text.Trim(),
                AuditId = selectedAudit.AuditId,
                AuditName = selectedAudit.AuditName,
                AssignedToUserId = assignedToUserId,
                DueDate = TaskDueDatePicker.SelectedDate.Value,
                Status = AuditTaskStatus.NotStarted,
                CreatedDate = DateTime.Now
            };

            _taskService.CreateTask(task);

            MessageBox.Show("Task created successfully.", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

            TaskNameBox.Text = string.Empty;
            TaskDescriptionBox.Text = string.Empty;
            TaskAuditComboBox.SelectedIndex = -1;
            TaskAssignedToComboBox.SelectedIndex = -1;
            TaskDueDatePicker.SelectedDate = null;

            LoadTasks();
        }

        private void LoadPendingEvidence()
        {
            try
            {
                var groupedEvidence = _evidenceService.GetPendingEvidenceGroupedByTask();
                PendingEvidencePanel.Children.Clear();

                if (!groupedEvidence.Any())
                {
                    PendingEvidencePanel.Children.Add(new TextBlock
                    {
                        Text = "No evidence pending review.",
                        FontFamily = new FontFamily("Verdana"),
                        FontSize = 16,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#969696")),
                        Margin = new Thickness(0, 20, 0, 0)
                    });
                    return;
                }

                foreach (var evidenceGroup in groupedEvidence)
                {
                    PendingEvidencePanel.Children.Add(CreateEvidenceCard(evidenceGroup));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading pending evidence: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Border CreateEvidenceCard(List<Evidence> evidenceGroup)
        {
            var firstEvidence = evidenceGroup.First();

            var card = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromArgb(51, 30, 30, 30)),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 15),
                Background = Brushes.White
            };

            var cardContent = new StackPanel();

            var topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var taskInfo = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };

            taskInfo.Children.Add(new TextBlock
            {
                Text = firstEvidence.Task?.TaskName ?? "Unknown Task",
                FontFamily = new FontFamily("Verdana"),
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1e1e1e")),
                Margin = new Thickness(0, 0, 0, 5)
            });

            taskInfo.Children.Add(new TextBlock
            {
                Text = $"Audit: {firstEvidence.Task?.AuditName ?? "Unknown"} | Submitted by: {firstEvidence.SubmittedBy?.FullName ?? "Unknown"}",
                FontFamily = new FontFamily("Verdana"),
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#646464")),
                Margin = new Thickness(0, 0, 0, 3)
            });

            taskInfo.Children.Add(new TextBlock
            {
                Text = $"Submitted: {firstEvidence.SubmittedDate:dd MMM yyyy, HH:mm}",
                FontFamily = new FontFamily("Verdana"),
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#646464"))
            });

            Grid.SetColumn(taskInfo, 0);
            topRow.Children.Add(taskInfo);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0)
            };

            var approveButton = new Button
            {
                Content = "Approve",
                FontFamily = new FontFamily("Verdana"),
                FontSize = 14,
                Width = 100,
                Height = 40,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromRgb(0, 128, 21)),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(0, 0, 10, 0),
                Cursor = Cursors.Hand,
                Tag = evidenceGroup
            };
            approveButton.Click += ApproveEvidence_Click;

            var rejectButton = new Button
            {
                Content = "Reject",
                FontFamily = new FontFamily("Verdana"),
                FontSize = 14,
                Width = 100,
                Height = 40,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromRgb(189, 0, 0)),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(10, 5, 10, 5),
                Cursor = Cursors.Hand,
                Tag = evidenceGroup
            };
            rejectButton.Click += RejectEvidence_Click;

            buttonPanel.Children.Add(approveButton);
            buttonPanel.Children.Add(rejectButton);

            Grid.SetColumn(buttonPanel, 1);
            topRow.Children.Add(buttonPanel);

            cardContent.Children.Add(topRow);

            cardContent.Children.Add(new Separator
            {
                Margin = new Thickness(0, 15, 0, 15),
                Background = new SolidColorBrush(Color.FromArgb(51, 30, 30, 30))
            });

            var filesSection = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromArgb(51, 30, 30, 30)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15)
            };

            var filesContent = new StackPanel();

            filesContent.Children.Add(new TextBlock
            {
                Text = $"Attached Files: ({evidenceGroup.Count})",
                FontFamily = new FontFamily("Verdana"),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1e1e1e")),
                Margin = new Thickness(0, 0, 0, 10)
            });

            foreach (var evidence in evidenceGroup)
            {
                var fileItem = new Border
                {
                    BorderBrush = new SolidColorBrush(Color.FromArgb(51, 30, 30, 30)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var fileGrid = new Grid();
                fileGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
                fileGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                fileGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var fileIcon = new SharpVectors.Converters.SvgViewbox
                {
                    Width = 30,
                    Height = 30,
                    Source = new Uri(GetIconForFile(evidence.FileName), UriKind.Relative),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(fileIcon, 0);
                fileGrid.Children.Add(fileIcon);

                // File name
                var fileNameText = new TextBlock
                {
                    Text = evidence.FileName,
                    FontFamily = new FontFamily("Verdana"),
                    FontSize = 14,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1e1e1e"))
                };
                Grid.SetColumn(fileNameText, 1);
                fileGrid.Children.Add(fileNameText);

                // View/Download button
                var capturedEvidence = evidence;
                var viewButton = new Button
                {
                    Content = "Download",
                    FontFamily = new FontFamily("Verdana"),
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.White,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#003B49")),
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(12, 6, 12, 6),
                    Cursor = Cursors.Hand,
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = capturedEvidence
                };

                viewButton.Click += (s, args) =>
                {
                    var ev = (s as Button)?.Tag as Evidence;
                    if (ev == null) return;

                    try
                    {
                        if (!File.Exists(ev.FilePath))
                        {
                            MessageBox.Show("File not found.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var saveDialog = new Microsoft.Win32.SaveFileDialog
                        {
                            FileName = ev.FileName,
                            Title = "Save Evidence File"
                        };

                        if (saveDialog.ShowDialog() == true)
                        {
                            File.Copy(ev.FilePath, saveDialog.FileName, true);
                            MessageBox.Show("File downloaded successfully.", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error downloading file: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };

                Grid.SetColumn(viewButton, 2);
                fileGrid.Children.Add(viewButton);

                fileItem.Child = fileGrid;
                filesContent.Children.Add(fileItem);
            }

            filesSection.Child = filesContent;
            cardContent.Children.Add(filesSection);

            card.Child = cardContent;
            return card;
        }

        private string GetIconForFile(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return extension switch
            {
                ".pdf" => "/Resources/Svg/file-pdf-regular-full.svg",
                ".doc" or ".docx" => "/Resources/Svg/file-lines-regular-full.svg",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "/Resources/Svg/file-image-regular-full.svg",
                _ => "/Resources/Svg/file-regular-full.svg"
            };
        }

        private void ApproveEvidence_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var evidenceGroup = button?.Tag as List<Evidence>;
            if (evidenceGroup == null) return;

            var currentUser = Application.Current.Properties["CurrentUser"] as User;

            foreach (var evidence in evidenceGroup)
                _evidenceService.UpdateEvidenceStatus(evidence.EvidenceId, EvidenceStatus.Approved, currentUser.UserID, null);

            MessageBox.Show($"Evidence approved successfully.", "Approved",
                MessageBoxButton.OK, MessageBoxImage.Information);

            LoadPendingEvidence();
            LoadReviewedEvidence();
        }

        private void RejectEvidence_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var evidenceGroup = button?.Tag as List<Evidence>;
            if (evidenceGroup == null) return;

            var reason = Microsoft.VisualBasic.Interaction.InputBox(
                "Please enter a rejection reason:",
                "Reject Evidence",
                "");

            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("A rejection reason is required.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var currentUser = Application.Current.Properties["CurrentUser"] as User;

            foreach (var evidence in evidenceGroup)
                _evidenceService.UpdateEvidenceStatus(evidence.EvidenceId, EvidenceStatus.Rejected, currentUser.UserID, reason);

            MessageBox.Show($"Evidence rejected.", "Rejected",
                MessageBoxButton.OK, MessageBoxImage.Information);

            LoadPendingEvidence();
            LoadReviewedEvidence();
        }

        private void LoadReviewedEvidence()
        {
            try
            {
                var evidence = _evidenceService.GetReviewedEvidence();
                ReviewedEvidenceGrid.ItemsSource = evidence;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading reviewed evidence: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var taskId = (int)button.Tag;
            var task = _viewModel.GetTaskById(taskId);
            if (task == null) return;

            var dialog = new Window
            {
                Title = "Edit Task",
                Width = 500,
                Height = 550,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = System.Windows.Media.Brushes.White
            };

            var mainStack = new StackPanel { Margin = new Thickness(30) };

            mainStack.Children.Add(new TextBlock
            {
                Text = "Edit Task",
                FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 20)
            });

            // Task Name
            mainStack.Children.Add(new TextBlock
            {
                Text = "Task Name",
                FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            });
            var taskNameBox = new TextBox
            {
                Text = task.TaskName,
                Height = 35,
                Padding = new Thickness(8),
                FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 15)
            };
            mainStack.Children.Add(taskNameBox);

            // Description
            mainStack.Children.Add(new TextBlock
            {
                Text = "Description",
                FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            });
            var descriptionBox = new TextBox
            {
                Text = task.Description,
                Height = 70,
                Padding = new Thickness(8),
                FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Margin = new Thickness(0, 0, 0, 15)
            };
            mainStack.Children.Add(descriptionBox);

            // Due Date
            mainStack.Children.Add(new TextBlock
            {
                Text = "Due Date",
                FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            });
            var dueDatePicker = new DatePicker
            {
                SelectedDate = task.DueDate,
                Height = 35,
                FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 15)
            };
            mainStack.Children.Add(dueDatePicker);

            // Assigned To
            mainStack.Children.Add(new TextBlock
            {
                Text = "Assigned To",
                FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            });
            var assignedToComboBox = new ComboBox
            {
                Height = 35,
                FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 25)
            };

            var auditors = _viewModel.GetAuditors();
            foreach (var auditor in auditors)
            {
                assignedToComboBox.Items.Add(new ComboBoxItem
                {
                    Content = auditor.FullName,
                    Tag = auditor.UserID,
                    IsSelected = auditor.UserID == task.AssignedToUserId
                });
            }
            mainStack.Children.Add(assignedToComboBox);

            // Buttons
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal };

            var saveButton = new Button
            {
                Content = "Save Changes",
                Width = 150,
                Height = 40,
                Margin = new Thickness(0, 0, 10, 0),
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#5D3754")),
                Foreground = System.Windows.Media.Brushes.White,
                FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 100,
                Height = 40,
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#9E9E9E")),
                Foreground = System.Windows.Media.Brushes.White,
                FontFamily = new System.Windows.Media.FontFamily("Verdana"),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };

            saveButton.Click += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(taskNameBox.Text))
                {
                    MessageBox.Show("Task Name is required.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (dueDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Due Date is required.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedAuditor = assignedToComboBox.SelectedItem as ComboBoxItem;

                task.TaskName = taskNameBox.Text.Trim();
                task.Description = descriptionBox.Text.Trim();
                task.DueDate = dueDatePicker.SelectedDate.Value;
                if (selectedAuditor != null)
                    task.AssignedToUserId = (int)selectedAuditor.Tag;

                _taskService.UpdateTask(task);
                MessageBox.Show("Task updated successfully.", "Updated",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                dialog.Close();
                LoadTasks();
            };

            cancelButton.Click += (s, args) => dialog.Close();

            buttonPanel.Children.Add(saveButton);
            buttonPanel.Children.Add(cancelButton);
            mainStack.Children.Add(buttonPanel);

            dialog.Content = mainStack;
            dialog.ShowDialog();
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var taskId = (int)button.Tag;
            var task = _viewModel.GetTaskById(taskId);
            if (task == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{task.TaskName}'?\nThis will also delete all associated evidence.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _taskService.DeleteTask(taskId);
                LoadTasks();
                MessageBox.Show("Task deleted successfully.", "Deleted",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
