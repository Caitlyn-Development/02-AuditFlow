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

                TotalAudits = allAudits.Count;
                AuditsInProgress = allAudits.Count(a => a.Status == AuditStatus.InProgress);
                CompletedAudits = allAudits.Count(a => a.Status == AuditStatus.Completed);
                OverdueAudits = allAudits.Count(a => a.Status == AuditStatus.Overdue);

                UpcomingDeadlines = allAudits
                    .Where(a => a.EndDate.Month == DateTime.Now.Month
                             && a.EndDate.Year == DateTime.Now.Year
                             && a.Status != AuditStatus.Completed)
                    .OrderBy(a => a.EndDate)
                    .ToList();

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
                UpcomingDeadlines = new List<Audit>();
                TotalAudits = 0;
                AuditsInProgress = 0;
                CompletedAudits = 0;
                OverdueAudits = 0;
            }
            finally
            {
                BuildCalendar();
            }
        }

        public new void GoToPreviousMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
            try
            {
                var allAudits = _managerAuditService.GetAllAudits();
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

        public new void GoToNextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            try
            {
                var allAudits = _managerAuditService.GetAllAudits();
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
    }
}
