using _02_AuditFlowApplication.Services;
using System.ComponentModel;

namespace _02_AuditFlowApplication.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly AuditService _auditService;
        private DateTime _currentMonth;
        private Dictionary<DateTime, List<string>> _auditEvents;
        private string _calendarMonthYear;

        public string CalendarMonthYear
        {
            get => _calendarMonthYear;
            set
            {
                _calendarMonthYear = value;
                OnPropertyChanged(nameof(CalendarMonthYear));
            }
        }

        public DateTime CurrentMonth
        {
            get => _currentMonth;
            set
            {
                _currentMonth = value;
                OnPropertyChanged(nameof(CurrentMonth));
                CalendarMonthYear = _currentMonth.ToString("MMMM yyyy");
            }
        }

        public Dictionary<DateTime, List<string>> AuditEvents
        {
            get => _auditEvents;
            private set
            {
                _auditEvents = value;
                OnPropertyChanged(nameof(AuditEvents));
            }
        }

        public DashboardViewModel()
        {
            _auditService = new AuditService();
            CurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            LoadAuditEvents();
        }

        public void LoadAuditEvents()
        {
            try
            {
                var audits = _auditService.GetAllAudits();
                var events = new Dictionary<DateTime, List<string>>();

                foreach (var audit in audits)
                {
                    var date = audit.StartDate.Date;
                    if (!events.ContainsKey(date))
                        events[date] = new List<string>();

                    events[date].Add(audit.AuditName);
                }

                AuditEvents = events;
            }
            catch
            {
                AuditEvents = new Dictionary<DateTime, List<string>>();
            }
        }

        public void GoToPreviousMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
        }

        public void GoToNextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
        }

        public bool HasEventsOnDate(DateTime date)
        {
            return AuditEvents != null && AuditEvents.ContainsKey(date);
        }

        public List<string> GetEventsOnDate(DateTime date)
        {
            if (AuditEvents != null && AuditEvents.ContainsKey(date))
                return AuditEvents[date];
            return new List<string>();
        }

        public int GetDaysInMonth() =>
            DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month);

        public int GetStartDayOfWeek()
        {
            DateTime firstDay = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            return ((int)firstDay.DayOfWeek == 0) ? 7 : (int)firstDay.DayOfWeek;
        }

        public int GetDaysInPreviousMonth()
        {
            DateTime prevMonth = CurrentMonth.AddMonths(-1);
            return DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}