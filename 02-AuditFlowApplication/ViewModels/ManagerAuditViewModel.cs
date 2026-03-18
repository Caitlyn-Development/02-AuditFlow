using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Services;
using System.ComponentModel;

namespace _02_AuditFlowApplication.ViewModels
{
    public class ManagerAuditViewModel : INotifyPropertyChanged
    {
        private readonly AuditService _auditService;
        private List<Audit> _allAudits;
        private List<Audit> _filteredAudits;
        private AuditType? _selectedType;
        private AuditStatus? _selectedStatus;
        private string _searchText;

        public List<Audit> FilteredAudits
        {
            get => _filteredAudits;
            set
            {
                _filteredAudits = value;
                OnPropertyChanged(nameof(FilteredAudits));
            }
        }

        public AuditType? SelectedType
        {
            get => _selectedType;
            set
            {
                _selectedType = value;
                OnPropertyChanged(nameof(SelectedType));
                ApplyFilters();
            }
        }

        public AuditStatus? SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;
                OnPropertyChanged(nameof(SelectedStatus));
                ApplyFilters();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                ApplyFilters();
            }
        }

        public ManagerAuditViewModel()
        {
            _auditService = new AuditService();
        }

        public void LoadAudits()
        {
            _allAudits = _auditService.GetAllAudits();
            FilteredAudits = _allAudits;
        }

        public void ApplyFilters()
        {
            if (_allAudits == null) return;

            var filtered = _allAudits.AsEnumerable();

            if (SelectedType.HasValue)
                filtered = filtered.Where(a => a.Type == SelectedType.Value);

            if (SelectedStatus.HasValue)
                filtered = filtered.Where(a => a.Status == SelectedStatus.Value);

            if (!string.IsNullOrWhiteSpace(SearchText))
                filtered = filtered.Where(a => a.AuditName.StartsWith(SearchText, StringComparison.OrdinalIgnoreCase));

            FilteredAudits = filtered.ToList();
        }

        public List<Audit> GetSearchSuggestions(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText) || _allAudits == null)
                return new List<Audit>();

            return _allAudits
                .Where(a => a.AuditName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public (bool Success, string ErrorMessage) SaveAudit(string auditName, string auditType,
            DateTime? startDate, DateTime? endDate, bool isRecurring, string recurrence, int createdByUserId)
        {
            if (string.IsNullOrWhiteSpace(auditName) || auditType == null || startDate == null || endDate == null)
                return (false, "Audit Name, Type, Start Date and End Date are all required.");

            if (endDate <= startDate)
                return (false, "End Date must be after Start Date.");

            if (isRecurring && string.IsNullOrEmpty(recurrence))
                return (false, "Please select a recurrence frequency.");

            if (_auditService.AuditNameExists(auditName))
                return (false, $"An audit with the name '{auditName}' already exists.");

            var newAudit = new Audit
            {
                AuditName = auditName,
                Type = ParseAuditType(auditType),
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                IsRecurring = isRecurring,
                RecurrenceType = isRecurring ? ParseRecurrenceFrequency(recurrence) : null,
                Status = AuditStatus.NotStarted,
                CreatedByUserID = createdByUserId,
                CreatedDate = DateTime.Now
            };

            _auditService.CreateAudit(newAudit);
            return (true, null);
        }

        public (bool Success, string ErrorMessage) UpdateAuditStatus(Audit audit, AuditStatus newStatus)
        {
            try
            {
                _auditService.UpdateAuditStatus(audit.AuditId, newStatus);
                audit.Status = newStatus;
                FilteredAudits = _allAudits.ToList();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public void LogAuditCreation(User createdBy, string auditName)
        {
            var audit = _allAudits?.FirstOrDefault(a => a.AuditName == auditName);
            if (audit != null)
                _auditService.LogAuditCreation(createdBy, audit);
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}