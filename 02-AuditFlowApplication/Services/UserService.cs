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
    }
}
