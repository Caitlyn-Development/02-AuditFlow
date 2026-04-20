using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Services;

namespace _02_AuditFlowApplication.ViewModels
{
    public class ManagerDashboardViewModel : DashboardViewModel
    {
        private readonly AuditService _managerAuditService;

        public ManagerDashboardViewModel(int userId) : base(userId)
        {
            _managerAuditService = new AuditService();
            LoadManagerDashboardData();
        }

        public void LoadManagerDashboardData()
        {
            try
            {
                var allAudits = _managerAuditService.GetAllAudits();

                // All audits system wide
                TotalAudits = allAudits.Count;
                AuditsInProgress = allAudits.Count(a => a.Status == AuditStatus.InProgress);
                CompletedAudits = allAudits.Count(a => a.Status == AuditStatus.Completed);
                OverdueAudits = allAudits.Count(a => a.Status == AuditStatus.Overdue);

                // Upcoming deadlines for current month
                UpcomingDeadlines = allAudits
                    .Where(a => a.EndDate.Month == CurrentMonth.Month
                             && a.EndDate.Year == CurrentMonth.Year
                             && a.Status != AuditStatus.Completed)
                    .OrderBy(a => a.EndDate)
                    .ToList();

                // Calendar events from all audits
                var events = new Dictionary<DateTime, List<string>>();
                foreach (var audit in allAudits)
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
            finally
            {
                BuildCalendar();
            }
        }

        // Override month navigation to reload all audits
        public new void GoToPreviousMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
            var allAudits = _managerAuditService.GetAllAudits();
            UpcomingDeadlines = allAudits
                .Where(a => a.EndDate.Month == CurrentMonth.Month
                         && a.EndDate.Year == CurrentMonth.Year
                         && a.Status != AuditStatus.Completed)
                .OrderBy(a => a.EndDate)
                .ToList();
            BuildCalendar();
        }

        public new void GoToNextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            var allAudits = _managerAuditService.GetAllAudits();
            UpcomingDeadlines = allAudits
                .Where(a => a.EndDate.Month == CurrentMonth.Month
                         && a.EndDate.Year == CurrentMonth.Year
                         && a.Status != AuditStatus.Completed)
                .OrderBy(a => a.EndDate)
                .ToList();
            BuildCalendar();
        }
    }
}
