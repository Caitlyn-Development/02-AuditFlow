using _02_AuditFlowApplication.Data;
using _02_AuditFlowApplication.Models;
using Microsoft.Data.Sqlite;

namespace _02_AuditFlowApplication.Services
{
    public class TaskService
    {
        public List<AuditTask> GetTasksByUserId(int userId)
        {
            List<AuditTask> tasks = new List<AuditTask>();

            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT 
                        t.TaskId, 
                        t.TaskName, 
                        t.Description, 
                        t.AuditId, 
                        t.AssignedToUserId, 
                        t.DueDate, 
                        t.Status, 
                        t.CreatedDate,
                        a.AuditName,
                        u.FullName,
                        (SELECT COUNT(*) FROM Evidence WHERE TaskId = t.TaskId) as EvidenceCount
                    FROM Tasks t
                    INNER JOIN Audits a ON t.AuditId = a.AuditId
                    INNER JOIN Users u ON t.AssignedToUserId = u.UserId
                    WHERE t.AssignedToUserId = @userId
                    ORDER BY t.DueDate";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string statusString = reader.GetString(6);
                            AuditTaskStatus status = ConvertStringToTaskStatus(statusString);

                            tasks.Add(new AuditTask
                            {
                                TaskId = reader.GetInt32(0),
                                TaskName = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                                AuditId = reader.GetInt32(3),
                                AssignedToUserId = reader.GetInt32(4),
                                DueDate = DateTime.Parse(reader.GetString(5)),
                                Status = status,
                                CreatedDate = DateTime.Parse(reader.GetString(7)),
                                //AuditName = reader.GetString(8),
                                //AssignedToName = reader.GetString(9),
                                //EvidenceCount = reader.GetInt32(10)
                            });
                        }
                    }
                }
            }

            return tasks;
        }

        public List<AuditTask> GetAllTasks()
        {
            List<AuditTask> tasks = new List<AuditTask>();

            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT 
                        t.TaskId, 
                        t.TaskName, 
                        t.Description, 
                        t.AuditId, 
                        t.AssignedToUserId, 
                        t.DueDate, 
                        t.Status, 
                        t.CreatedDate,
                        a.AuditName,
                        u.FullName,
                        (SELECT COUNT(*) FROM Evidence WHERE TaskId = t.TaskId) as EvidenceCount
                    FROM Tasks t
                    INNER JOIN Audits a ON t.AuditId = a.AuditId
                    INNER JOIN Users u ON t.AssignedToUserId = u.UserId
                    ORDER BY t.DueDate";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string statusString = reader.GetString(6);
                        AuditTaskStatus status = ConvertStringToTaskStatus(statusString);

                        tasks.Add(new AuditTask
                        {
                            TaskId = reader.GetInt32(0),
                            TaskName = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            AuditId = reader.GetInt32(3),
                            AssignedToUserId = reader.GetInt32(4),
                            DueDate = DateTime.Parse(reader.GetString(5)),
                            Status = status,  // Now using enum
                            CreatedDate = DateTime.Parse(reader.GetString(7)),
                            //AuditName = reader.GetString(8),
                            //AssignedToName = reader.GetString(9),
                            //EvidenceCount = reader.GetInt32(10)
                        });
                    }
                }
            }

            return tasks;
        }

        // convert database string to enum
        private AuditTaskStatus ConvertStringToTaskStatus(string statusString)
        {
            switch (statusString)
            {
                case "Not Started":
                    return AuditTaskStatus.NotStarted;
                case "In Progress":
                    return AuditTaskStatus.InProgress;
                case "On Hold":
                    return AuditTaskStatus.OnHold;
                case "Completed":
                    return AuditTaskStatus.Completed;
                default:
                    return AuditTaskStatus.NotStarted;
            }
        }

        // convert enum to database string
        public static string ConvertTaskStatusToString(AuditTaskStatus status)
        {
            switch (status)
            {
                case AuditTaskStatus.NotStarted:
                    return "Not Started";
                case AuditTaskStatus.InProgress:
                    return "In Progress";
                case AuditTaskStatus.OnHold:
                    return "On Hold";
                case AuditTaskStatus.Completed:
                    return "Completed";
                default:
                    return "Not Started";
            }
        }
    }
}
