using _02_AuditFlowApplication.Data;
using _02_AuditFlowApplication.Models;
using Microsoft.Data.Sqlite;

namespace _02_AuditFlowApplication.Services
{
    public class TaskService
    {
        //public List<AuditTask> GetTasksByUserId(int userId)
        //{
        //    List<AuditTask> tasks = new List<AuditTask>();

        //    using (SqliteConnection connection = DatabaseHelper.GetConnection())
        //    {
        //        connection.Open();
        //        string query = @"
        //            SELECT 
        //                t.TaskId, 
        //                t.TaskName, 
        //                t.Description, 
        //                t.AuditId, 
        //                t.AssignedToUserId, 
        //                t.DueDate, 
        //                t.Status, 
        //                t.CreatedDate,
        //                a.AuditName,
        //                u.FullName,
        //                (SELECT COUNT(*) FROM Evidence WHERE TaskId = t.TaskId) as EvidenceCount
        //            FROM Tasks t
        //            INNER JOIN Audits a ON t.AuditId = a.AuditId
        //            INNER JOIN Users u ON t.AssignedToUserId = u.UserId
        //            WHERE t.AssignedToUserId = @userId
        //            ORDER BY t.DueDate";

        //        using (SqliteCommand command = new SqliteCommand(query, connection))
        //        {
        //            command.Parameters.AddWithValue("@userId", userId);

        //            using (SqliteDataReader reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    string statusString = reader.GetString(6);
        //                    AuditTaskStatus status = ConvertStringToTaskStatus(statusString);

        //                    tasks.Add(new AuditTask
        //                    {
        //                        TaskId = reader.GetInt32(0),
        //                        TaskName = reader.GetString(1),
        //                        Description = reader.IsDBNull(2) ? null : reader.GetString(2),
        //                        AuditId = reader.GetInt32(3),
        //                        AuditName = reader.GetString(4),
        //                        AssignedToUserId = reader.GetInt32(5),
        //                        DueDate = DateTime.Parse(reader.GetString(6)),
        //                        Status = status,
        //                        CreatedDate = DateTime.Parse(reader.GetString(7)),

        //                    });
        //                }
        //            }
        //        }
        //    }

        //    return tasks;
        //}

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

        //public List<Audit> GetTasksByAudit(AuditName type)
        //{
        //    return GetAllAudits().Where(a => a.Type == type).ToList();
        //}

    }
}
