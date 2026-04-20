using _02_AuditFlowApplication.Models;
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
        private List<CalendarCell> _calendarCells;
        private List<Audit> _userAudits;
        private int _userId;

        // Stat card properties
        private int _totalAudits;
        private int _auditsInProgress;
        private int _completedAudits;
        private int _overdueAudits;

        public int TotalAudits
        {
            get => _totalAudits;
            set { _totalAudits = value; OnPropertyChanged(nameof(TotalAudits)); }
        }

        public int AuditsInProgress
        {
            get => _auditsInProgress;
            set { _auditsInProgress = value; OnPropertyChanged(nameof(AuditsInProgress)); }
        }

        public int CompletedAudits
        {
            get => _completedAudits;
            set { _completedAudits = value; OnPropertyChanged(nameof(CompletedAudits)); }
        }

        public int OverdueAudits
        {
            get => _overdueAudits;
            set { _overdueAudits = value; OnPropertyChanged(nameof(OverdueAudits)); }
        }

        // Upcoming deadlines
        private List<Audit> _upcomingDeadlines;
        public List<Audit> UpcomingDeadlines
        {
            get => _upcomingDeadlines;
            set { _upcomingDeadlines = value; OnPropertyChanged(nameof(UpcomingDeadlines)); }
        }

        public class CalendarCell
        {
            public string DayNumber { get; set; }
            public bool IsCurrentMonth { get; set; }
            public bool IsOtherMonth { get; set; }
            public List<string> Events { get; set; } = new List<string>();
            public DateTime? Date { get; set; }
        }

        public List<CalendarCell> CalendarCells
        {
            get => _calendarCells;
            set { _calendarCells = value; OnPropertyChanged(nameof(CalendarCells)); }
        }

        public string CalendarMonthYear
        {
            get => _calendarMonthYear;
            set { _calendarMonthYear = value; OnPropertyChanged(nameof(CalendarMonthYear)); }
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
            protected set
            {
                _auditEvents = value;
                OnPropertyChanged(nameof(AuditEvents));
            }
        }

        public DashboardViewModel(int userId)
        {
            _auditService = new AuditService();
            _userId = userId;
            CurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            LoadDashboardData();
        }

        public void LoadDashboardData()
        {
            try
            {
                _userAudits = _auditService.GetAuditsForUser(_userId);

                // Stat cards
                TotalAudits = _userAudits.Count;
                AuditsInProgress = _userAudits.Count(a => a.Status == AuditStatus.InProgress);
                CompletedAudits = _userAudits.Count(a => a.Status == AuditStatus.Completed);
                OverdueAudits = _userAudits.Count(a => a.Status == AuditStatus.Overdue);

                // Upcoming deadlines - audits due this month
                UpcomingDeadlines = _userAudits
                    .Where(a => a.EndDate.Month == CurrentMonth.Month
                             && a.EndDate.Year == CurrentMonth.Year
                             && a.Status != AuditStatus.Completed)
                    .OrderBy(a => a.EndDate)
                    .ToList();

                // Calendar events
                var events = new Dictionary<DateTime, List<string>>();
                foreach (var audit in _userAudits)
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
                _userAudits = new List<Audit>();
                AuditEvents = new Dictionary<DateTime, List<string>>();
            }
            finally
            {
                BuildCalendar();
            }
        }

        public void LoadAuditEvents() => LoadDashboardData();

        public void BuildCalendar()
        {
            var cells = new List<CalendarCell>();
            int startDayOfWeek = GetStartDayOfWeek();
            int daysInMonth = GetDaysInMonth();
            int daysInPrevMonth = GetDaysInPreviousMonth();
            int dayCounter = 1;
            int nextMonthDayCounter = 1;

            for (int i = 0; i < 35; i++)
            {
                if (i < startDayOfWeek - 1)
                {
                    cells.Add(new CalendarCell
                    {
                        DayNumber = (daysInPrevMonth - (startDayOfWeek - 2 - i)).ToString(),
                        IsOtherMonth = true
                    });
                }
                else if (dayCounter <= daysInMonth)
                {
                    var date = new DateTime(CurrentMonth.Year, CurrentMonth.Month, dayCounter);
                    cells.Add(new CalendarCell
                    {
                        DayNumber = dayCounter.ToString(),
                        IsCurrentMonth = true,
                        Date = date,
                        Events = GetEventsOnDate(date)
                    });
                    dayCounter++;
                }
                else
                {
                    cells.Add(new CalendarCell
                    {
                        DayNumber = nextMonthDayCounter.ToString(),
                        IsOtherMonth = true
                    });
                    nextMonthDayCounter++;
                }
            }

            CalendarCells = cells;
        }

        public void GoToPreviousMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
            // Update upcoming deadlines for new month
            if (_userAudits != null)
            {
                UpcomingDeadlines = _userAudits
                    .Where(a => a.EndDate.Month == CurrentMonth.Month
                             && a.EndDate.Year == CurrentMonth.Year
                             && a.Status != AuditStatus.Completed)
                    .OrderBy(a => a.EndDate)
                    .ToList();
            }
            BuildCalendar();
        }

        public void GoToNextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            // Update upcoming deadlines for new month
            if (_userAudits != null)
            {
                UpcomingDeadlines = _userAudits
                    .Where(a => a.EndDate.Month == CurrentMonth.Month
                             && a.EndDate.Year == CurrentMonth.Year
                             && a.Status != AuditStatus.Completed)
                    .OrderBy(a => a.EndDate)
                    .ToList();
            }
            BuildCalendar();
        }

        public bool HasEventsOnDate(DateTime date) =>
            AuditEvents != null && AuditEvents.ContainsKey(date);

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
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}