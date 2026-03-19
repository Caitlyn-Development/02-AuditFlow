using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Services;
using System.ComponentModel;
using System.IO;

namespace _02_AuditFlowApplication.ViewModels
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        private readonly TaskService _taskService;
        private List<AuditTask> _allTasks;
        private List<AuditTask> _filteredTasks;
        private string? _selectedAudit;
        private AuditTaskStatus? _selectedStatus;

        public List<AuditTask> FilteredTasks
        {
            get => _filteredTasks;
            set
            {
                _filteredTasks = value;
                OnPropertyChanged(nameof(FilteredTasks));
            }
        }

        public string? SelectedAudit
        {
            get => _selectedAudit;
            set
            {
                _selectedAudit = value;
                OnPropertyChanged(nameof(SelectedAudit));
                ApplyFilters();
            }
        }

        public AuditTaskStatus? SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;
                OnPropertyChanged(nameof(SelectedStatus));
                ApplyFilters();
            }
        }

        public TaskViewModel()
        {
            _taskService = new TaskService();
        }

        public void LoadTasks(int userId)
        {
            _allTasks = _taskService.GetAllTasks()
                .Where(t => t.AssignedToUserId == userId)
                .ToList();
            FilteredTasks = _allTasks;
        }

        public void ApplyFilters()
        {
            if (_allTasks == null) return;

            var filtered = _allTasks.AsEnumerable();

            if (!string.IsNullOrEmpty(_selectedAudit))
                filtered = filtered.Where(a => a.AuditName == _selectedAudit);

            if (_selectedStatus.HasValue)
                filtered = filtered.Where(a => a.Status == _selectedStatus.Value);

            FilteredTasks = filtered.ToList();
        }

        public List<AuditTask> GetSearchSuggestions(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText) || _allTasks == null)
                return new List<AuditTask>();

            return _allTasks
                .Where(a => a.TaskName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public (bool Success, string ErrorMessage) UpdateTaskStatus(AuditTask task, AuditTaskStatus newStatus)
        {
            try
            {
                _taskService.UpdateTaskStatus(task.TaskId, newStatus);
                task.Status = newStatus;
                FilteredTasks = _allTasks.ToList();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool Success, string ErrorMessage) SubmitEvidence(AuditTask task, List<string> uploadedFiles)
        {
            try
            {
                string evidencePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Evidence", task.TaskId.ToString());
                Directory.CreateDirectory(evidencePath);

                foreach (var file in uploadedFiles)
                {
                    string fileName = Path.GetFileName(file);
                    string destPath = Path.Combine(evidencePath, fileName);
                    File.Copy(file, destPath, true);
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public bool IsValidFileType(string filePath, string[] allowedExtensions)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            return allowedExtensions.Contains(extension);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
