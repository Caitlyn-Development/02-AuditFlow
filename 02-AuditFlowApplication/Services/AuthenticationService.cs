using _02_AuditFlowApplication.Data;
using _02_AuditFlowApplication.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace _02_AuditFlowApplication.Services
{
    public class AuthenticationService
    {
        private readonly AppDataContext _appDataContext;

        public AuthenticationService()
        {
            _appDataContext = new AppDataContext();
        }

        public User AuthenticateUser(string username, string password)
        {
            using (SqliteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                string query = "SELECT UserId, Username, PasswordHash, FullName, Role, IsActive, CreatedDate FROM Users WHERE Username = @username AND IsActive = 1";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            User user = new User
                            {
                                UserID = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                FullName = reader.GetString(3),
                                Role = ParseUserRole(reader.GetString(4)),
                                IsActive = reader.GetInt32(5) == 1,
                                CreatedDate = DateTime.Parse(reader.GetString(6))
                            };

                            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

                            return isPasswordValid ? user : null;
                        }
                    }
                }
            }

            return null;
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

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool CreateUser(User user, string password)
        {
            if (_appDataContext.Users.Any(u => u.Username == user.Username))
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            _appDataContext.Users.Add(user);
            _appDataContext.SaveChanges();
            return true;
        }

        public void InitializeDefaultAdmin()
        {
            if (!_appDataContext.Users.Any())
            {
                var admin = new User
                {
                    Username = "admin",
                    FullName = "System Administrator",
                    Role = UserRole.Manager,
                    IsActive = true
                };
                CreateUser(admin, "Admin123!");
            }
        }
    }
}
