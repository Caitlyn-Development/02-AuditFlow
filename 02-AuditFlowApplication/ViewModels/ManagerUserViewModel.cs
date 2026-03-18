using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Services;
using System.ComponentModel;

namespace _02_AuditFlowApplication.ViewModels
{
    public class ManagerUserViewModel : INotifyPropertyChanged
    {
        private readonly UserService _userService;
        private List<User> _allUsers;
        private List<User> _filteredUsers;

        public List<User> FilteredUsers
        {
            get => _filteredUsers;
            set
            {
                _filteredUsers = value;
                OnPropertyChanged(nameof(FilteredUsers));
            }
        }

        public ManagerUserViewModel()
        {
            _userService = new UserService();
        }

        public void LoadUsers()
        {
            _allUsers = _userService.GetAllUsers();
            FilteredUsers = _allUsers;
        }

        public User GetUserById(int userId)
        {
            return _allUsers?.FirstOrDefault(u => u.UserID == userId);
        }

        public (bool Success, string ErrorMessage) CreateUser(string username, string fullName,
            string password, string confirmPassword, string role)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword) || role == null)
                return (false, "All fields must be filled in.");

            if (password != confirmPassword)
                return (false, "Passwords do not match.");

            if (_userService.UsernameExists(username))
                return (false, $"A user with the username '{username}' already exists.");

            if (_userService.FullNameExists(fullName))
                return (false, $"A user with the full name '{fullName}' already exists.");

            var newUser = new User
            {
                Username = username,
                FullName = fullName,
                Role = role == "Manager" ? UserRole.Manager : UserRole.Auditor,
                IsActive = true,
                CreatedDate = DateTime.Now,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            _userService.CreateUser(newUser);
            return (true, null);
        }

        public void LogUserCreation(User createdBy, string username)
        {
            var newUser = _allUsers?.FirstOrDefault(u => u.Username == username);
            if (newUser != null)
                _userService.LogUserCreation(createdBy, newUser);
        }

        public (bool CanDelete, string ErrorMessage) CanDeleteUser(User user)
        {
            if (user.IsActive)
                return (false, $"'{user.Username}' ({user.FullName}) is currently active and cannot be deleted.\n\nPlease deactivate the user before deleting.");

            return (true, null);
        }

        public (bool Success, string ErrorMessage) DeleteUser(User user, User deletedBy)
        {
            try
            {
                _userService.LogUserDeletion(deletedBy, user);
                _userService.DeleteUser(user);
                LoadUsers();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
