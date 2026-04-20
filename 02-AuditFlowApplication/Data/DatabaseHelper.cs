using Microsoft.Data.Sqlite;
using System.IO;

namespace _02_AuditFlowApplication.Data
{
    public class DatabaseHelper
    {
        private static string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AuditDatabase.db");
        private static string connectionString = $"Data Source={dbPath}";

        public static void InitialiseDatabase()
        {
            if (!File.Exists(dbPath))
            {
                // Create the database file by opening a connection
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                }

                CreateTables();
                InsertDummyData();
            }
        }

        private static void CreateTables()
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string createUsersTable = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        PasswordHash TEXT NOT NULL,
                        FullName TEXT NOT NULL,
                        Role INTEGER NOT NULL,
                        IsActive INTEGER NOT NULL DEFAULT 1,
                        CreatedDate TEXT NOT NULL
                    )";

                string createAuditsTable = @"
                    CREATE TABLE IF NOT EXISTS Audits (
                        AuditId INTEGER PRIMARY KEY AUTOINCREMENT,
                        AuditName TEXT NOT NULL,
                        AuditType TEXT NOT NULL,
                        StartDate TEXT NOT NULL,
                        EndDate TEXT NOT NULL,
                        IsRecurring INTEGER NOT NULL DEFAULT 0,
                        RecurrenceFrequency TEXT,
                        Status TEXT NOT NULL,
                        CreatedByUserId INTEGER NOT NULL,
                        CreatedDate TEXT NOT NULL,
                        FOREIGN KEY (CreatedByUserId) REFERENCES Users(UserId)
                    )";

                string createTasksTable = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                        TaskId INTEGER PRIMARY KEY AUTOINCREMENT,
                        TaskName TEXT NOT NULL,
                        Description TEXT,
                        AuditId INTEGER NOT NULL,
                        AssignedToUserId INTEGER NOT NULL,
                        DueDate TEXT NOT NULL,
                        Status TEXT NOT NULL,
                        CreatedDate TEXT NOT NULL,
                        FOREIGN KEY (AuditId) REFERENCES Audits(AuditId),
                        FOREIGN KEY (AssignedToUserId) REFERENCES Users(UserId)
                    )";

                string createEvidenceTable = @"
                    CREATE TABLE IF NOT EXISTS Evidence (
                        EvidenceId INTEGER PRIMARY KEY AUTOINCREMENT,
                        TaskId INTEGER NOT NULL,
                        FileName TEXT NOT NULL,
                        FilePath TEXT NOT NULL,
                        FileSize INTEGER NOT NULL,
                        SubmittedByUserId INTEGER NOT NULL,
                        SubmittedDate TEXT NOT NULL,
                        Status TEXT NOT NULL,
                        ReviewedByUserId INTEGER,
                        ReviewedDate TEXT,
                        RejectionReason TEXT,
                        FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId),
                        FOREIGN KEY (SubmittedByUserId) REFERENCES Users(UserId),
                        FOREIGN KEY (ReviewedByUserId) REFERENCES Users(UserId)
                    )";

                string createUserAuditLogTable = @"
                   CREATE TABLE IF NOT EXISTS UserAuditLog (
                      LogId INTEGER PRIMARY KEY AUTOINCREMENT,
                      ChangedByUserId INTEGER NOT NULL,
                      ChangedByUsername TEXT NOT NULL,
                      AffectedUserId INTEGER NOT NULL,
                      AffectedUsername TEXT NOT NULL,
                      ChangeDescription TEXT NOT NULL,
                      ChangeDate TEXT NOT NULL,
                      FOREIGN KEY (ChangedByUserId) REFERENCES Users(UserId)
                  )";

                string createAuditLogTable = @"
                   CREATE TABLE IF NOT EXISTS AuditLog (
                      LogId INTEGER PRIMARY KEY AUTOINCREMENT,
                      CreatedByUserId INTEGER NOT NULL,
                      CreatedByUsername TEXT NOT NULL,
                      AuditId INTEGER NOT NULL,
                      AuditName TEXT NOT NULL,
                      ChangeDescription TEXT NOT NULL,
                      ChangeDate TEXT NOT NULL,
                      FOREIGN KEY (CreatedByUserId) REFERENCES Users(UserId)
                  )";

                string createAuditStatusLogTable = @"
                  CREATE TABLE IF NOT EXISTS AuditStatusLog (
                    LogId INTEGER PRIMARY KEY AUTOINCREMENT,
                    ChangedByUserId INTEGER NOT NULL,
                    ChangedByUsername TEXT NOT NULL,
                    AuditId INTEGER NOT NULL,
                    AuditName TEXT NOT NULL,
                    OldStatus TEXT NOT NULL,
                    NewStatus TEXT NOT NULL,
                    ChangeDate TEXT NOT NULL,
                    FOREIGN KEY (ChangedByUserId) REFERENCES Users(UserId),
                    FOREIGN KEY (AuditId) REFERENCES Audits(AuditId)
                  )";

                ExecuteNonQuery(connection, createUsersTable);
                ExecuteNonQuery(connection, createAuditsTable);
                ExecuteNonQuery(connection, createTasksTable);
                ExecuteNonQuery(connection, createEvidenceTable);
                ExecuteNonQuery(connection, createUserAuditLogTable);
                ExecuteNonQuery(connection, createAuditLogTable);
                ExecuteNonQuery(connection, createAuditStatusLogTable);

                connection.Close();
            }
        }

        private static void InsertDummyData()
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Insert Users with BCrypt hashed passwords
                string insertUsers = @"
                    INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive, CreatedDate) VALUES
                    ('admin', '$2a$11$nYGofFNqphGFPoTsAGOx5uxFvs4ICynYZcgsw45rOIyu9dqNVGXV2', 'John Administrator', 'Manager', 1, '2025-01-01'),
                    ('auditor1', '$2a$11$nYGofFNqphGFPoTsAGOx5uxFvs4ICynYZcgsw45rOIyu9dqNVGXV2', 'Sarah Johnson', 'Auditor', 1, '2025-01-05'),
                    ('auditor2', '$2a$11$XVH3qZJKtqhN9vXhXxJqXOYKZGGqYqH5xKZJKtqhN9vXhXxJqXO', 'Mike Williams', 'Auditor', 1, '2025-01-10'),
                    ('manager1', '$2a$11$nYGofFNqphGFPoTsAGOx5uxFvs4ICynYZcgsw45rOIyu9dqNVGXV2', 'Emma Davis', 'Manager', 1, '2025-01-15'),
                    ('auditor3', '$2a$11$XVH3qZJKtqhN9vXhXxJqXOYKZGGqYqH5xKZJKtqhN9vXhXxJqXO', 'David Brown', 'Auditor', 1, '2025-01-20')
                ";

                // Insert Audits
                string insertAudits = @"
                    INSERT INTO Audits (AuditName, AuditType, StartDate, EndDate, IsRecurring, RecurrenceFrequency, Status, CreatedByUserId, CreatedDate) VALUES
                    ('IT Systems Audit 2025', 'IT', '2026-10-15', '2026-10-22', 0, NULL, 'In Progress', 1, '2025-09-01'),
                    ('Supplier Risk Assessment', 'Procurement', '2026-10-08', '2026-10-13', 0, NULL, 'Not Started', 1, '2025-09-05'),
                    ('Financial Compliance Q3', 'Financial', '2026-09-01', '2026-09-30', 1, 'Quarterly', 'Completed', 1, '2025-08-15'),
                    ('Safety Inspection', 'Safety', '2026-11-01', '2026-11-05', 1, 'Monthly', 'Not Started', 4, '2025-10-01'),
                    ('Data Privacy Review', 'Security', '2026-10-25', '2026-11-10', 0, NULL, 'In Progress', 4, '2025-10-10')
                ";

                // Insert Tasks
                string insertTasks = @"
                    INSERT INTO Tasks (TaskName, Description, AuditId, AssignedToUserId, DueDate, Status, CreatedDate) VALUES
                    ('Review Server Logs', 'Check all server access logs for anomalies', 1, 2, '2026-10-18', 'In Progress', '2025-10-01'),
                    ('Network Security Scan', 'Run comprehensive network security assessment', 1, 3, '2026-10-20', 'Not Started', '2025-10-01'),
                    ('Vulnerability Assessment', 'Identify and document system vulnerabilities', 1, 2, '2026-10-22', 'Not Started', '2025-10-01'),
                    ('Supplier Documentation', 'Collect and verify supplier certifications', 2, 5, '2026-10-10', 'In Progress', '2025-09-20'),
                    ('Risk Matrix Update', 'Update supplier risk assessment matrix', 2, 3, '2026-10-13', 'Not Started', '2025-09-20'),
                    ('Financial Records Review', 'Review Q3 financial statements', 3, 2, '2026-09-25', 'Completed', '2025-09-01'),
                    ('Fire Safety Equipment', 'Inspect all fire extinguishers and alarms', 4, 5, '2026-11-03', 'Not Started', '2025-10-15'),
                    ('Emergency Exit Audit', 'Verify all emergency exits are accessible', 4, 5, '2026-11-04', 'Not Started', '2025-10-15'),
                    ('GDPR Compliance Check', 'Review data processing procedures', 5, 3, '2026-10-30', 'In Progress', '2025-10-12'),
                    ('Data Retention Policy', 'Audit data retention and deletion processes', 5, 2, '2026-11-05', 'Not Started', '2025-10-12')
                ";

                // Insert Evidence
                string insertEvidence = @"
                    INSERT INTO Evidence (TaskId, FileName, FilePath, FileSize, SubmittedByUserId, SubmittedDate, Status, ReviewedByUserId, ReviewedDate, RejectionReason) VALUES
                    (1, 'server_logs_sept.pdf', '/evidence/server_logs_sept.pdf', 2048576, 2, '2026-10-16 14:30:00', 'Pending Review', NULL, NULL, NULL),
                    (1, 'access_report.docx', '/evidence/access_report.docx', 524288, 2, '2026-10-16 15:00:00', 'Pending Review', NULL, NULL, NULL),
                    (4, 'supplier_certificates.zip', '/evidence/supplier_certificates.zip', 5242880, 5, '2026-10-09 10:20:00', 'Approved', 1, '2026-10-10 09:00:00', NULL),
                    (6, 'q3_financial_summary.xlsx', '/evidence/q3_financial_summary.xlsx', 1048576, 2, '2026-09-22 11:45:00', 'Approved', 4, '2026-09-23 08:30:00', NULL),
                    (9, 'gdpr_checklist.pdf', '/evidence/gdpr_checklist.pdf', 786432, 3, '2026-10-28 16:15:00', 'Rejected', 4, '2026-10-29 10:00:00', 'Missing signatures on pages 3 and 7')
                ";

                ExecuteNonQuery(connection, insertUsers);
                ExecuteNonQuery(connection, insertAudits);
                ExecuteNonQuery(connection, insertTasks);
                ExecuteNonQuery(connection, insertEvidence);

                connection.Close();
            }
        }

        public static void UpdateOverdueStatuses()
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();

                // Update overdue audits - only update if not already completed
                string updateAudits = @"
            UPDATE Audits 
            SET Status = 'Overdue'
            WHERE DATE(EndDate) < DATE('now')
            AND Status NOT IN ('Completed', 'Overdue')";

                using (SqliteCommand command = new SqliteCommand(updateAudits, connection))
                    command.ExecuteNonQuery();

                // Update overdue tasks - only update if not already completed
                string updateTasks = @"
            UPDATE Tasks 
            SET Status = 'Overdue'
            WHERE DATE(DueDate) < DATE('now')
            AND Status NOT IN ('Completed', 'Overdue')";

                using (SqliteCommand command = new SqliteCommand(updateTasks, connection))
                    command.ExecuteNonQuery();
            }
        }

        private static void ExecuteNonQuery(SqliteConnection connection, string commandText)
        {
            using (SqliteCommand command = new SqliteCommand(commandText, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static SqliteConnection GetConnection()
        {
            return new SqliteConnection(connectionString);
        }
    }
}