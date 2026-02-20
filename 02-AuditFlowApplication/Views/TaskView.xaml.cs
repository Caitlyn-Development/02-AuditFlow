using _02_AuditFlowApplication.Helpers;
using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Services;
using Microsoft.Win32;
using SharpVectors.Converters;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace _02_AuditFlowApplication.Views
{
    public partial class TaskView : UserControl
    {
        private readonly TaskService _taskService;
        private List<AuditTask> _allTasks;
        private string? _selectedAudit = null;
        private readonly AuditTaskStatus? _selectedStatus = null;

        private List<string> _uploadedFiles = new List<string>();
        private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".gif", ".bmp" };


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

        private void AuditFilterBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_allTasks == null || AuditFilterBox.SelectedItem == null)
                return;

            var selectedItem = (ComboBoxItem)AuditFilterBox.SelectedItem;
            string selectedAudit = selectedItem.Content.ToString();

            _selectedAudit = selectedAudit == "All Audits" ? null : selectedAudit;

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allTasks == null)
                return;

            var filteredTasks = _allTasks.AsEnumerable();

            if (!string.IsNullOrEmpty(_selectedAudit))
            {
                filteredTasks = filteredTasks.Where(a => a.AuditName == _selectedAudit);
            }

            if (_selectedStatus.HasValue)
            {
                filteredTasks = filteredTasks.Where(a => a.Status == _selectedStatus.Value);
            }

            TasksGrid.ItemsSource = filteredTasks.ToList();
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
            var searchResults = _allTasks
                .Where(a => a.TaskName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
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

        #region Evidence Upload - Drag & Drop

        private void DropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Any(file => IsValidFileType(file)))
                {
                    e.Effects = DragDropEffects.Copy;
                    DropZoneBorder.Background = new SolidColorBrush(Color.FromArgb(26, 0, 59, 73));
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void DropZone_DragLeave(object sender, DragEventArgs e)
        {
            DropZoneBorder.Background = Brushes.Transparent;
        }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            DropZoneBorder.Background = Brushes.Transparent;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (var file in files)
                {
                    if (IsValidFileType(file))
                    {
                        AddFileToList(file);
                    }
                    else
                    {
                        MessageBox.Show($"File type not supported: {Path.GetFileName(file)}\n\nSupported types: PDF, Word (.doc, .docx), Images (.jpg, .png, .gif, .bmp)",
                            "Invalid File Type", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void DropZone_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Supported Files (*.pdf;*.doc;*.docx;*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.pdf;*.doc;*.docx;*.jpg;*.jpeg;*.png;*.gif;*.bmp|" +
                         "PDF Files (*.pdf)|*.pdf|" +
                         "Word Documents (*.doc;*.docx)|*.doc;*.docx|" +
                         "Images (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Title = "Select Evidence Files"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var file in openFileDialog.FileNames)
                {
                    AddFileToList(file);
                }
            }
        }

        private bool IsValidFileType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            return _allowedExtensions.Contains(extension);
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

            var fileBorder = new Border
            {
                Width = 580,
                Height = 70,
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(10),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(15),
                BorderBrush = new SolidColorBrush(Color.FromArgb(51, 30, 30, 30))
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

            // File icon
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
            grid.Children.Add(icon);

            // File name
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
            grid.Children.Add(fileNameText);

            var closeIcon = new SharpVectors.Converters.SvgViewbox
            {
                Width = 30,
                Height = 30,
                Source = new Uri("/Resources/Svg/xmark-solid-full.svg", UriKind.Relative),
            };

            // Remove button
            var removeButton = new Button
            {
                Content = closeIcon,
                Width = 30,
                Height = 30,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = filePath
            };

            removeButton.Click += RemoveFile_Click;
            Grid.SetColumn(removeButton, 2);
            grid.Children.Add(removeButton);

            fileBorder.Child = grid;
            UploadedFilesPanel.Children.Add(fileBorder);
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

            var fileBorder = (button.Parent as Grid)?.Parent as Border;
            UploadedFilesPanel.Children.Remove(fileBorder);

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

            try
            {
                var selectedItem = TaskSelectionComboBox.SelectedItem as ComboBoxItem;
                var selectedTask = selectedItem?.Tag as AuditTask;

                if (selectedTask == null)
                {
                    MessageBox.Show("Error retrieving selected task.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create evidence folder if it doesn't exist
                string evidencePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Evidence", selectedTask.TaskId.ToString());
                Directory.CreateDirectory(evidencePath);

                // Copy files to evidence folder
                foreach (var file in _uploadedFiles)
                {
                    string fileName = Path.GetFileName(file);
                    string destPath = Path.Combine(evidencePath, fileName);
                    File.Copy(file, destPath, true);
                }

                MessageBox.Show($"Successfully uploaded {_uploadedFiles.Count} file(s) for task: {selectedTask.TaskName}",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear the uploaded files list and UI
                _uploadedFiles.Clear();
                UploadedFilesPanel.Children.Clear();
                SubmitEvidenceButton.IsEnabled = false;
                TaskSelectionComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting evidence: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


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
