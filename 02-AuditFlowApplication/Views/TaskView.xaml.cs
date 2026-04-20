using _02_AuditFlowApplication.Data;
using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.ViewModels;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace _02_AuditFlowApplication.Views
{
    public partial class TaskView : UserControl
    {
        private readonly TaskViewModel _viewModel;
        private List<string> _uploadedFiles = new List<string>();
        private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

        public TaskView()
        {
            InitializeComponent();
            _viewModel = new TaskViewModel();
            DataContext = _viewModel;
            DatabaseHelper.UpdateOverdueStatuses();
            LoadTasks();

            Layout.SetActiveButton("Tasks");
        }

        private void LoadTasks()
        {
            try
            {
                var currentUser = Application.Current.Properties["CurrentUser"] as User;
                if (currentUser == null)
                {
                    MessageBox.Show("No user logged in.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _viewModel.LoadTasks(currentUser.UserID);
                TasksGrid.ItemsSource = _viewModel.FilteredTasks;
                LoadTaskSelectionComboBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tasks: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTaskSelectionComboBox()
        {
            TaskSelectionComboBox.Items.Clear();

            TaskSelectionComboBox.Items.Add(new ComboBoxItem
            {
                Content = "Select a Task..",
                IsSelected = true
            });

            foreach (var task in _viewModel.FilteredTasks)
            {
                TaskSelectionComboBox.Items.Add(new ComboBoxItem
                {
                    Content = task.TaskName,
                    Tag = task
                });
            }

            TaskSelectionComboBox.SelectedIndex = 0;
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

        private void ApplyFilters()
        {
            _viewModel.ApplyFilters();
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
            var task = border?.DataContext as AuditTask;
            if (task == null) return;

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
                    FontFamily = new FontFamily("Verdana"),
                    FontSize = 14,
                    Padding = new Thickness(15, 8, 15, 8),
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
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
                        TasksGrid.ItemsSource = _viewModel.FilteredTasks;

                    popup.IsOpen = false;
                    Window.GetWindow(this).PreviewMouseDown -= outsideClickHandler;
                };

                panel.Children.Add(btn);
            }

            container.Child = panel;
            popup.Child = container;
            popup.IsOpen = true;
        }

        #region Evidence Upload

        private void DropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Any(file => _viewModel.IsValidFileType(file, _allowedExtensions)))
                {
                    e.Effects = DragDropEffects.Copy;
                    DropZoneBorder.Background = new SolidColorBrush(Color.FromArgb(26, 0, 59, 73));
                }
                else
                    e.Effects = DragDropEffects.None;
            }
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private void DropZone_DragLeave(object sender, DragEventArgs e)
            => DropZoneBorder.Background = Brushes.Transparent;

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            DropZoneBorder.Background = Brushes.Transparent;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files)
                {
                    if (_viewModel.IsValidFileType(file, _allowedExtensions))
                        AddFileToList(file);
                    else
                        MessageBox.Show($"File type not supported: {Path.GetFileName(file)}",
                            "Invalid File Type", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void DropZone_Click(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Supported Files (*.pdf;*.doc;*.docx;*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.pdf;*.doc;*.docx;*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Title = "Select Evidence Files"
            };

            if (openFileDialog.ShowDialog() == true)
                foreach (var file in openFileDialog.FileNames)
                    AddFileToList(file);
        }

        private void AddFileToList(string filePath)
        {
            if (_uploadedFiles.Contains(filePath))
            {
                MessageBox.Show($"File already added: {Path.GetFileName(filePath)}",
                    "Duplicate File", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _uploadedFiles.Add(filePath);
            DisplayUploadedFile(filePath);
            SubmitEvidenceButton.IsEnabled = _uploadedFiles.Count > 0;
        }

        private void DisplayUploadedFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var fileExtension = Path.GetExtension(filePath).ToLower();

            var outerGrid = new Grid { Width = 580, Height = 70 };

            var fileBorder = new Border
            {
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(10),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(15),
                BorderBrush = new SolidColorBrush(Color.FromArgb(51, 30, 30, 30))
            };

            var innerGrid = new Grid();
            innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var iconPath = GetIconForFileType(fileExtension);
            var icon = new SharpVectors.Converters.SvgViewbox
            {
                Width = 40,
                Height = 40,
                Source = new Uri(iconPath, UriKind.Relative),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(icon, 0);
            innerGrid.Children.Add(icon);

            var fileNameText = new TextBlock
            {
                Text = fileName,
                FontFamily = new FontFamily("Verdana"),
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            Grid.SetColumn(fileNameText, 1);
            innerGrid.Children.Add(fileNameText);

            fileBorder.Child = innerGrid;

            var removeButton = new Button
            {
                Width = 24,
                Height = 24,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                Background = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Tag = filePath,
                Content = "✕",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30))
            };

            removeButton.Click += RemoveFile_Click;
            outerGrid.Children.Add(fileBorder);
            outerGrid.Children.Add(removeButton);
            UploadedFilesPanel.Children.Add(outerGrid);
        }

        private string GetIconForFileType(string extension)
        {
            return extension switch
            {
                ".pdf" => "/Resources/Svg/file-pdf-regular-full.svg",
                ".doc" or ".docx" => "/Resources/Svg/file-lines-regular-full.svg",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "/Resources/Svg/file-image-regular-full.svg",
                _ => "/Resources/Svg/file-regular-full.svg"
            };
        }

        private void RemoveFile_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var filePath = button.Tag as string;

            _uploadedFiles.Remove(filePath);

            var outerGrid = button.Parent as Grid;
            UploadedFilesPanel.Children.Remove(outerGrid);

            SubmitEvidenceButton.IsEnabled = _uploadedFiles.Count > 0;
        }

        private void SubmitEvidence_Click(object sender, RoutedEventArgs e)
        {
            if (TaskSelectionComboBox.SelectedItem == null || TaskSelectionComboBox.SelectedIndex == 0)
            {
                MessageBox.Show("Please select a task before submitting evidence.",
                    "No Task Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_uploadedFiles.Count == 0)
            {
                MessageBox.Show("Please upload at least one file before submitting.",
                    "No Files", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedItem = TaskSelectionComboBox.SelectedItem as ComboBoxItem;
            var selectedTask = selectedItem?.Tag as AuditTask;

            if (selectedTask == null)
            {
                MessageBox.Show("Error retrieving selected task.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var currentUser = Application.Current.Properties["CurrentUser"] as User;
            if (currentUser == null)
            {
                MessageBox.Show("No user logged in.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var (success, error) = _viewModel.SubmitEvidence(selectedTask, _uploadedFiles, currentUser.UserID);

            if (success)
            {
                MessageBox.Show($"Successfully uploaded {_uploadedFiles.Count} file(s) for task: {selectedTask.TaskName}",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                _uploadedFiles.Clear();
                UploadedFilesPanel.Children.Clear();
                SubmitEvidenceButton.IsEnabled = false;
                TaskSelectionComboBox.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show($"Error submitting evidence: {error}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}