using _02_AuditFlowApplication.Data;
using _02_AuditFlowApplication.Models;
using Microsoft.EntityFrameworkCore;
using static _02_AuditFlowApplication.Models.User;

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
            var user = _appDataContext.Users.FirstOrDefault(u => u.Username == username && u.IsActive);

            if (user == null)
                return null;

            // Verify password using BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            return isPasswordValid ? user : null;
        }

        public bool CreateUser(User user, string password)
        {
            // Check if username already exists
            if (_appDataContext.Users.Any(u => u.Username == user.Username))
                return false;

            // Hash the password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            _appDataContext.Users.Add(user);
            _appDataContext.SaveChanges();

            return true;
        }

        public void InitializeDefaultAdmin()
        {
            // Create default admin if no users exist
            if (!_appDataContext.Users.Any())
            {
                var admin = new User
                {
                    Username = "admin",
                    FullName = "System Administrator",
                    Role = UserRole.Manager,
                    IsActive = true
                };

                CreateUser(admin, "Admin123!"); // Default password - should be changed
            }
        }
}
}
