using _02_AuditFlowApplication.Data;
using _02_AuditFlowApplication.Models;
using Microsoft.Data.Sqlite;

namespace _02_AuditFlowApplication.Services
{
    public class AuditService
    {
        public List<Audit> GetAllAudits()
        {
            List<Audit> audits = new List<Audit>();

            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                string query = @"SELECT AuditId, AuditName, AuditType, StartDate, EndDate, 
                                IsRecurring, RecurrenceFrequency, Status, CreatedByUserId, CreatedDate 
                                FROM Audits 
                                ORDER BY StartDate DESC";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            audits.Add(new Audit
                            {
                                AuditId = reader.GetInt32(0),
                                AuditName = reader.GetString(1),
                                Type = ParseAuditType(reader.GetString(2)),
                                StartDate = DateTime.Parse(reader.GetString(3)),
                                EndDate = DateTime.Parse(reader.GetString(4)),
                                IsRecurring = reader.GetInt32(5) == 1,
                                RecurrenceType = reader.IsDBNull(6) ? null : ParseRecurrenceFrequency(reader.GetString(6)),
                                Status = ParseAuditStatus(reader.GetString(7)),
                                CreatedByUserID = reader.GetInt32(8),
                                CreatedDate = DateTime.Parse(reader.GetString(9))
                            });
                        }
                    }
                }
            }

            return audits;
        }

        private AuditType ParseAuditType(string type)
        {
            return type switch
            {
                "IT" => AuditType.Security,
                "Security" => AuditType.Security,
                "Safety" => AuditType.Safety,
                "Quality" => AuditType.Quality,
                "Financial" => AuditType.Financial,
                "Procurement" => AuditType.Quality,
                _ => AuditType.Unknown
            };
        }

        private AuditStatus ParseAuditStatus(string status)
        {
            return status switch
            {
                "Not Started" => AuditStatus.NotStarted,
                "In Progress" => AuditStatus.InProgress,
                "Completed" => AuditStatus.Completed,
                "Overdue" => AuditStatus.Overdue,
                _ => AuditStatus.NotStarted
            };
        }

        private RecurrenceFrequency ParseRecurrenceFrequency(string frequency)
        {
            return frequency switch
            {
                "Weekly" => RecurrenceFrequency.Weekly,
                "Monthly" => RecurrenceFrequency.Monthly,
                "Quarterly" => RecurrenceFrequency.Quarterly,
                "Annual" => RecurrenceFrequency.Annual,
                _ => RecurrenceFrequency.Monthly
            };
        }

        public List<Audit> GetAuditsByStatus(AuditStatus status)
        {
            return GetAllAudits().Where(a => a.Status == status).ToList();
        }

        public List<Audit> GetAuditsByType(AuditType type)
        {
            return GetAllAudits().Where(a => a.Type == type).ToList();
        }

        //create GetAuditsByUser
    }
}
