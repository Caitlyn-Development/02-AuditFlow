using _02_AuditFlowApplication.Data;
using _02_AuditFlowApplication.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace _02_AuditFlowApplication.Services
{
    public class EvidenceService
    {
        public List<Evidence> GetReviewedEvidence()
        {
            var evidenceList = new List<Evidence>();

            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
            SELECT 
                e.EvidenceId,
                e.TaskId,
                e.FileName,
                e.FilePath,
                e.FileSize,
                e.SubmittedByUserId,
                e.SubmittedDate,
                e.Status,
                e.ReviewedByUserId,
                e.ReviewedDate,
                e.RejectionReason,
                t.TaskName,
                a.AuditName,
                su.FullName AS SubmittedByName,
                ru.FullName AS ReviewedByName
            FROM Evidence e
            INNER JOIN Tasks t ON e.TaskId = t.TaskId
            INNER JOIN Audits a ON t.AuditId = a.AuditId
            INNER JOIN Users su ON e.SubmittedByUserId = su.UserId
            LEFT JOIN Users ru ON e.ReviewedByUserId = ru.UserId
            WHERE e.Status IN ('Approved', 'Rejected')
            GROUP BY e.TaskId, e.Status
            ORDER BY e.ReviewedDate DESC";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        evidenceList.Add(new Evidence
                        {
                            EvidenceId = reader.GetInt32(0),
                            TaskId = reader.GetInt32(1),
                            FileName = reader.GetString(2),
                            FilePath = reader.GetString(3),
                            FileSize = reader.GetInt64(4),
                            SubmittedByUserId = reader.GetInt32(5),
                            SubmittedDate = DateTime.Parse(reader.GetString(6)),
                            Status = ParseEvidenceStatus(reader.GetString(7)),
                            ReviewedByUserId = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                            ReviewedDate = reader.IsDBNull(9) ? null : DateTime.Parse(reader.GetString(9)),
                            RejectionReason = reader.IsDBNull(10) ? null : reader.GetString(10),
                            Task = new AuditTask
                            {
                                TaskId = reader.GetInt32(1),
                                TaskName = reader.GetString(11),
                                AuditName = reader.GetString(12)
                            },
                            SubmittedBy = new User
                            {
                                UserID = reader.GetInt32(5),
                                FullName = reader.GetString(13)
                            },
                            ReviewedBy = reader.IsDBNull(8) ? null : new User
                            {
                                UserID = reader.GetInt32(8),
                                FullName = reader.IsDBNull(14) ? null : reader.GetString(14)
                            }
                        });
                    }
                }
            }

            return evidenceList;
        }

        public List<Evidence> GetPendingEvidence()
        {
            var evidenceList = new List<Evidence>();

            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT 
                        e.EvidenceId,
                        e.TaskId,
                        e.FileName,
                        e.FilePath,
                        e.FileSize,
                        e.SubmittedByUserId,
                        e.SubmittedDate,
                        e.Status,
                        e.ReviewedByUserId,
                        e.ReviewedDate,
                        e.RejectionReason,
                        t.TaskName,
                        a.AuditName,
                        su.FullName AS SubmittedByName
                    FROM Evidence e
                    INNER JOIN Tasks t ON e.TaskId = t.TaskId
                    INNER JOIN Audits a ON t.AuditId = a.AuditId
                    INNER JOIN Users su ON e.SubmittedByUserId = su.UserId
                    WHERE e.Status = 'Pending Review'
                    ORDER BY e.SubmittedDate DESC";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        evidenceList.Add(new Evidence
                        {
                            EvidenceId = reader.GetInt32(0),
                            TaskId = reader.GetInt32(1),
                            FileName = reader.GetString(2),
                            FilePath = reader.GetString(3),
                            FileSize = reader.GetInt64(4),
                            SubmittedByUserId = reader.GetInt32(5),
                            SubmittedDate = DateTime.Parse(reader.GetString(6)),
                            Status = ParseEvidenceStatus(reader.GetString(7)),
                            ReviewedByUserId = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                            ReviewedDate = reader.IsDBNull(9) ? null : DateTime.Parse(reader.GetString(9)),
                            RejectionReason = reader.IsDBNull(10) ? null : reader.GetString(10),
                            Task = new AuditTask
                            {
                                TaskId = reader.GetInt32(1),
                                TaskName = reader.GetString(11),
                                AuditName = reader.GetString(12)
                            },
                            SubmittedBy = new User
                            {
                                UserID = reader.GetInt32(5),
                                FullName = reader.GetString(13)
                            }
                        });
                    }
                }
            }

            return evidenceList;
        }

        private EvidenceStatus ParseEvidenceStatus(string status)
        {
            return status switch
            {
                "Approved" => EvidenceStatus.Approved,
                "Rejected" => EvidenceStatus.Rejected,
                _ => EvidenceStatus.PendingReview
            };
        }

        public List<List<Evidence>> GetPendingEvidenceGroupedByTask()
        {
            var allPending = GetPendingEvidence();

            return allPending
                .GroupBy(e => e.TaskId)
                .Select(g => g.ToList())
                .ToList();
        }

        public void UpdateEvidenceStatus(int evidenceId, EvidenceStatus status, int reviewedByUserId, string rejectionReason)
        {
            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                string statusString = status switch
                {
                    EvidenceStatus.Approved => "Approved",
                    EvidenceStatus.Rejected => "Rejected",
                    _ => "Pending Review"
                };

                string query = @"UPDATE Evidence 
                         SET Status = @status, 
                             ReviewedByUserId = @reviewedByUserId, 
                             ReviewedDate = @reviewedDate,
                             RejectionReason = @rejectionReason
                         WHERE EvidenceId = @evidenceId";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@status", statusString);
                    command.Parameters.AddWithValue("@reviewedByUserId", reviewedByUserId);
                    command.Parameters.AddWithValue("@reviewedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@rejectionReason", rejectionReason ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@evidenceId", evidenceId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void SubmitEvidence(Evidence evidence)
        {
            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"INSERT INTO Evidence 
            (TaskId, FileName, FilePath, FileSize, SubmittedByUserId, SubmittedDate, Status)
            VALUES (@taskId, @fileName, @filePath, @fileSize, @submittedByUserId, @submittedDate, @status)";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@taskId", evidence.TaskId);
                    command.Parameters.AddWithValue("@fileName", evidence.FileName);
                    command.Parameters.AddWithValue("@filePath", evidence.FilePath);
                    command.Parameters.AddWithValue("@fileSize", evidence.FileSize);
                    command.Parameters.AddWithValue("@submittedByUserId", evidence.SubmittedByUserId);
                    command.Parameters.AddWithValue("@submittedDate", evidence.SubmittedDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@status", "Pending Review");
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
