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

        public void UpdateAuditStatus(int auditId, AuditStatus newStatus)
        {
            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                string statusString = newStatus switch
                {
                    AuditStatus.NotStarted => "Not Started",
                    AuditStatus.InProgress => "In Progress",
                    AuditStatus.Completed => "Completed",
                    AuditStatus.Overdue => "Overdue",
                    _ => "Not Started"
                };

                string query = "UPDATE Audits SET Status = @status WHERE AuditId = @auditId";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@status", statusString);
                    command.Parameters.AddWithValue("@auditId", auditId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public bool AuditNameExists(string auditName)
        {
            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Audits WHERE AuditName = @auditName";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@auditName", auditName);
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        public void CreateAudit(Audit audit)
        {
            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"INSERT INTO Audits 
            (AuditName, AuditType, StartDate, EndDate, IsRecurring, RecurrenceFrequency, Status, CreatedByUserId, CreatedDate)
            VALUES (@auditName, @auditType, @startDate, @endDate, @isRecurring, @recurrenceFrequency, @status, @createdByUserId, @createdDate)";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@auditName", audit.AuditName);
                    command.Parameters.AddWithValue("@auditType", audit.Type.ToString());
                    command.Parameters.AddWithValue("@startDate", audit.StartDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@endDate", audit.EndDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@isRecurring", audit.IsRecurring ? 1 : 0);
                    command.Parameters.AddWithValue("@recurrenceFrequency", audit.RecurrenceType.HasValue ? audit.RecurrenceType.ToString() : (object)DBNull.Value);
                    command.Parameters.AddWithValue("@status", audit.Status.ToString());
                    command.Parameters.AddWithValue("@createdByUserId", audit.CreatedByUserID);
                    command.Parameters.AddWithValue("@createdDate", audit.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.ExecuteNonQuery();

                    string getIdQuery = "SELECT last_insert_rowid()";
                    using (SqliteCommand idCommand = new SqliteCommand(getIdQuery, connection))
                    {
                        audit.AuditId = Convert.ToInt32(idCommand.ExecuteScalar());
                    }
                }
            }
        }

        public void LogAuditCreation(User createdBy, Audit audit)
        {
            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"INSERT INTO AuditLog
                    (CreatedByUserId, CreatedByUsername, AuditId, AuditName, ChangeDescription, ChangeDate)
                    VALUES (@createdByUserId, @createdByUsername, @auditId, @auditName, @changeDescription, @changeDate)";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@createdByUserId", createdBy.UserID);
                    command.Parameters.AddWithValue("@createdByUsername", createdBy.Username);
                    command.Parameters.AddWithValue("@auditId", audit.AuditId);
                    command.Parameters.AddWithValue("@auditName", audit.AuditName);
                    command.Parameters.AddWithValue("@changeDescription", $"Audit '{audit.AuditName}' of type '{audit.Type}' created. Start: {audit.StartDate:dd/MM/yyyy}, End: {audit.EndDate:dd/MM/yyyy}. Recurring: {audit.IsRecurring}.");
                    command.Parameters.AddWithValue("@changeDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateAudit(Audit audit)
        {
            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"UPDATE Audits 
                         SET AuditName = @auditName,
                             StartDate = @startDate,
                             EndDate = @endDate
                         WHERE AuditId = @auditId";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@auditName", audit.AuditName);
                    command.Parameters.AddWithValue("@startDate", audit.StartDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@endDate", audit.EndDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@auditId", audit.AuditId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteAudit(int auditId)
        {
            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM Audits WHERE AuditId = @auditId";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@auditId", auditId);
                    command.ExecuteNonQuery();
                }
            }
        }

    }
}
