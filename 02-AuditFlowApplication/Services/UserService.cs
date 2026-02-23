using _02_AuditFlowApplication.Data;
using _02_AuditFlowApplication.Models;
using Microsoft.Data.Sqlite;

namespace _02_AuditFlowApplication.Services
{
    public class UserService
    {
        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();

            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"SELECT UserId, Username, PasswordHash, FullName, Role, 
                                IsActive, CreatedDate 
                                FROM Users";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            UserID = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            PasswordHash = reader.GetString(2),
                            FullName = reader.GetString(3),
                            Role = ParseUserRole(reader.GetString(4)),
                            IsActive = reader.GetInt32(5) == 1,
                            CreatedDate = DateTime.Parse(reader.GetString(6))
                        });
                    }
                }
            }

            return users;
        }

        private UserRole ParseUserRole(string role)
        {
            return role switch
            {
                "Manager" => UserRole.Manager,
                "Auditor" => UserRole.Auditor,
                _ => UserRole.Unknown,
            };
        }

        public void UpdateUser(User user, string newPasswordHash = null)
        {
            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                string query = newPasswordHash != null
                    ? "UPDATE Users SET FullName = @fullName, Role = @role, IsActive = @isActive, PasswordHash = @passwordHash WHERE UserId = @userId"
                    : "UPDATE Users SET FullName = @fullName, Role = @role, IsActive = @isActive WHERE UserId = @userId";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@fullName", user.FullName);
                    command.Parameters.AddWithValue("@role", user.Role.ToString());
                    command.Parameters.AddWithValue("@isActive", user.IsActive ? 1 : 0);
                    command.Parameters.AddWithValue("@userId", user.UserID);

                    if (newPasswordHash != null)
                        command.Parameters.AddWithValue("@passwordHash", newPasswordHash);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void LogUserChange(User changedBy, User affectedUser, string changeDescription)
        {
            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"INSERT INTO UserAuditLog 
            (ChangedByUserId, ChangedByUsername, AffectedUserId, AffectedUsername, ChangeDescription, ChangeDate)
            VALUES (@changedByUserId, @changedByUsername, @affectedUserId, @affectedUsername, @changeDescription, @changeDate)";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@changedByUserId", changedBy.UserID);
                    command.Parameters.AddWithValue("@changedByUsername", changedBy.Username);
                    command.Parameters.AddWithValue("@affectedUserId", affectedUser.UserID);
                    command.Parameters.AddWithValue("@affectedUsername", affectedUser.Username);
                    command.Parameters.AddWithValue("@changeDescription", changeDescription);
                    command.Parameters.AddWithValue("@changeDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.ExecuteNonQuery();
                }
            }

        }

        public void DeleteUser(User user)
        {
            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                // Reassign tasks to admin (UserId = 1)
                string reassignTasks = "UPDATE Tasks SET AssignedToUserId = 1 WHERE AssignedToUserId = @userId";
                string reassignEvidence = "UPDATE Evidence SET SubmittedByUserId = 1 WHERE SubmittedByUserId = @userId";
                string deleteUser = "DELETE FROM Users WHERE UserId = @userId";

                using (SqliteCommand cmd = new SqliteCommand(reassignTasks, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", user.UserID);
                    cmd.ExecuteNonQuery();
                }

                using (SqliteCommand cmd = new SqliteCommand(reassignEvidence, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", user.UserID);
                    cmd.ExecuteNonQuery();
                }

                using (SqliteCommand cmd = new SqliteCommand(deleteUser, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", user.UserID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void LogUserDeletion(User deletedBy, User deletedUser)
        {
            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"INSERT INTO UserAuditLog 
            (ChangedByUserId, ChangedByUsername, AffectedUserId, AffectedUsername, ChangeDescription, ChangeDate)
            VALUES (@changedByUserId, @changedByUsername, @affectedUserId, @affectedUsername, @changeDescription, @changeDate)";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@changedByUserId", deletedBy.UserID);
                    command.Parameters.AddWithValue("@changedByUsername", deletedBy.Username);
                    command.Parameters.AddWithValue("@affectedUserId", deletedUser.UserID);
                    command.Parameters.AddWithValue("@affectedUsername", deletedUser.Username);
                    command.Parameters.AddWithValue("@changeDescription", $"User '{deletedUser.Username}' ({deletedUser.FullName}) was deleted.");
                    command.Parameters.AddWithValue("@changeDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.ExecuteNonQuery();
                }
            }
        }

        public void LogUserCreation(User createdBy, User newUser)
        {
            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"INSERT INTO UserAuditLog 
            (ChangedByUserId, ChangedByUsername, AffectedUserId, AffectedUsername, ChangeDescription, ChangeDate)
            VALUES (@changedByUserId, @changedByUsername, @affectedUserId, @affectedUsername, @changeDescription, @changeDate)";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@changedByUserId", createdBy.UserID);
                    command.Parameters.AddWithValue("@changedByUsername", createdBy.Username);
                    command.Parameters.AddWithValue("@affectedUserId", newUser.UserID);
                    command.Parameters.AddWithValue("@affectedUsername", newUser.Username);
                    command.Parameters.AddWithValue("@changeDescription", $"New user '{newUser.Username}' ({newUser.FullName}) created with role '{newUser.Role}'.");
                    command.Parameters.AddWithValue("@changeDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.ExecuteNonQuery();
                }
            }
        }

        public bool UsernameExists(string username)
        {
            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE Username = @username";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        public bool FullNameExists(string fullName)
        {
            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE FullName = @fullName";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@fullName", fullName);
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        public void CreateUser(User user)
        {
            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = @"INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive, CreatedDate)
                         VALUES (@username, @passwordHash, @fullName, @role, @isActive, @createdDate)";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("@fullName", user.FullName);
                    command.Parameters.AddWithValue("@role", user.Role.ToString());
                    command.Parameters.AddWithValue("@isActive", user.IsActive ? 1 : 0);
                    command.Parameters.AddWithValue("@createdDate", user.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.ExecuteNonQuery();

                    // Retrieve the generated ID back into the user object
                    string getIdQuery = "SELECT last_insert_rowid()";
                    using (SqliteCommand idCommand = new SqliteCommand(getIdQuery, connection))
                    {
                        user.UserID = Convert.ToInt32(idCommand.ExecuteScalar());
                    }
                }
            }
        }
    }
}
