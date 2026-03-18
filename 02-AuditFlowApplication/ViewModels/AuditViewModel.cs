using _02_AuditFlowApplication.Models;
using System.ComponentModel;

namespace _02_AuditFlowApplication.ViewModels
{
    public class AuditViewModel : INotifyPropertyChanged
    {
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

        public void LoadAudits(List<Audit> audits)
        {
            _allAudits = audits;
            FilteredAudits = audits;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
