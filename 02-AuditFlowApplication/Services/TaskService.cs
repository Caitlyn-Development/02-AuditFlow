using _02_AuditFlowApplication.Data;
using _02_AuditFlowApplication.Models;
using Microsoft.Data.Sqlite;

namespace _02_AuditFlowApplication.Services
{
    public class TaskService
    {
        public List<AuditTask> GetAllTasks()
        {
            List<AuditTask> tasks = new List<AuditTask>();

            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"SELECT t.TaskId, t.TaskName, t.Description, t.AuditId, t.AssignedToUserId, t.DueDate, t.Status, t.CreatedDate,a.AuditName,u.FullName,
                COUNT(CASE WHEN e.Status = 'Approved' THEN 1 END) as EvidenceCount
                FROM Tasks t
                INNER JOIN Audits a ON t.AuditId = a.AuditId
                INNER JOIN Users u ON t.AssignedToUserId = u.UserId
                LEFT JOIN Evidence e ON e.TaskId = t.TaskId
                GROUP BY t.TaskId, t.TaskName, t.Description, t.AuditId,t.AssignedToUserId, t.DueDate, t.Status, t.CreatedDate, a.AuditName, u.FullName ORDER BY t.DueDate";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tasks.Add(new AuditTask
                        {
                            TaskId = reader.GetInt32(0),
                            TaskName = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            AuditId = reader.GetInt32(3),
                            AssignedToUserId = reader.GetInt32(4),
                            DueDate = DateTime.Parse(reader.GetString(5)),
                            Status = ParseAuditStatus(reader.GetString(6)),
                            CreatedDate = DateTime.Parse(reader.GetString(7)),
                            AuditName = reader.GetString(8),
                            AssignedToUser = new User
                            {
                                UserID = reader.GetInt32(4),
                                FullName = reader.GetString(9)
                            },
                            EvidenceCount = reader.GetInt32(10)
                        });
                    }
                }
            }

            return tasks;
        }

        private AuditTaskStatus ParseAuditStatus(string status)
        {
            return status switch
            {
                "Not Started" => AuditTaskStatus.NotStarted,
                "In Progress" => AuditTaskStatus.InProgress,
                "On Hold" => AuditTaskStatus.OnHold,
                "Completed" => AuditTaskStatus.Completed,
                "Overdue" => AuditTaskStatus.Overdue,
                _ => AuditTaskStatus.NotStarted
            };
        }

        public List<AuditTask> GetTasksByStatus(AuditTaskStatus status)
        {
            return GetAllTasks().Where(a => a.Status == status).ToList();
        }

        public void UpdateTaskStatus(int taskId, AuditTaskStatus newStatus)
        {
            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                string statusString = newStatus switch
                {
                    AuditTaskStatus.NotStarted => "Not Started",
                    AuditTaskStatus.InProgress => "In Progress",
                    AuditTaskStatus.OnHold => "On Hold",
                    AuditTaskStatus.Completed => "Completed",
                    AuditTaskStatus.Overdue => "Overdue",
                    _ => "Not Started"
                };

                string query = "UPDATE Tasks SET Status = @status WHERE TaskId = @taskId";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@status", statusString);
                    command.Parameters.AddWithValue("@taskId", taskId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
