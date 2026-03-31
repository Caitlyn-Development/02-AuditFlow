using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Services;
using System.ComponentModel;
using System.IO;

namespace _02_AuditFlowApplication.ViewModels
{
    public class ManagerTaskViewModel : INotifyPropertyChanged
    {
        private readonly TaskService _taskService;
        private readonly AuditService _auditService;
        private readonly UserService _userService;

        private List<AuditTask> _allTasks;
        private List<AuditTask> _filteredTasks;
        private string? _selectedAudit;
        private string? _selectedUser;
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

        public string? SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
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

        public ManagerTaskViewModel()
        {
            _taskService = new TaskService();
            _auditService = new AuditService();
            _userService = new UserService();
        }

        public void LoadTasks()
        {
            _allTasks = _taskService.GetAllTasks();
            FilteredTasks = _allTasks;
        }

        public AuditTask GetTaskById(int taskId)
        {
            return _allTasks?.FirstOrDefault(t => t.TaskId == taskId);
        }

        public List<User> GetAuditors()
        {
            return _userService.GetAllUsers()
                .Where(u => u.Role == UserRole.Auditor)
                .OrderBy(u => u.FullName)
                .ToList();
        }

        public List<string> GetAuditNames()
        {
            return _auditService.GetAllAudits()
                .Select(a => a.AuditName)
                .Distinct()
                .OrderBy(name => name)
                .ToList();
        }

        public void ApplyFilters()
        {
            if (_allTasks == null) return;

            var filtered = _allTasks.AsEnumerable();

            if (!string.IsNullOrEmpty(_selectedAudit))
                filtered = filtered.Where(a => a.AuditName == _selectedAudit);

            if (!string.IsNullOrEmpty(_selectedUser))
                filtered = filtered.Where(a => a.AssignedToUser != null && a.AssignedToUser.FullName == _selectedUser);

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