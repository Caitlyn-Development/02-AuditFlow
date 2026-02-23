using _02_AuditFlowApplication.Helpers;
using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Services;
using System.Windows;
using System.Windows.Controls;

namespace _02_AuditFlowApplication.Views
{
    /// <summary>
    /// Interaction logic for ManagerAuditView.xaml
    /// </summary>
    public partial class ManagerUserView : UserControl
    {
        private readonly UserService _userService;
        private List<User> _allUsers;

        public ManagerUserView()
        {
            InitializeComponent();
            _userService = new UserService();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                _allUsers = _userService.GetAllUsers();
                UsersGrid.ItemsSource = _allUsers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int userId = (int)button.Tag;
            var userToEdit = _allUsers.FirstOrDefault(u => u.UserID == userId);

            if (userToEdit == null) return;

            var dialog = new EditUserDialog(userToEdit);
            if (dialog.ShowDialog() == true)
                LoadUsers();
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int userId = (int)button.Tag;
            var userToDelete = _allUsers.FirstOrDefault(u => u.UserID == userId);

            if (userToDelete == null) return;

            if (userToDelete.IsActive)
            {
                MessageBox.Show(
                    $"'{userToDelete.Username}' ({userToDelete.FullName}) is currently active and cannot be deleted.\n\nPlease deactivate the user before deleting.",
                    "Cannot Delete Active User",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete the following user?\n\nUsername: {userToDelete.Username}\nFull Name: {userToDelete.FullName}\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var currentUser = Application.Current.Properties["CurrentUser"] as User;
                    _userService.LogUserDeletion(currentUser, userToDelete);
                    _userService.DeleteUser(userToDelete);
                    MessageBox.Show("User deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveUser_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string fullName = FullNameBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            string role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Validate all fields are filled
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword) || role == null)
            {
                MessageBox.Show("All fields must be filled in.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate passwords match
            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if username or full name already exists
            if (_userService.UsernameExists(username))
            {
                MessageBox.Show($"A user with the username '{username}' already exists.", "Duplicate User",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_userService.FullNameExists(fullName))
            {
                MessageBox.Show($"A user with the full name '{fullName}' already exists.", "Duplicate User",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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

            var currentUser = Application.Current.Properties["CurrentUser"] as User;
            _userService.LogUserCreation(currentUser, newUser);

            MessageBox.Show($"User '{username}' created successfully.", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

            // Clear the fields
            UsernameBox.Text = string.Empty;
            FullNameBox.Text = string.Empty;
            PasswordBox.Password = string.Empty;
            ConfirmPasswordBox.Password = string.Empty;
            RoleComboBox.SelectedIndex = -1;

            LoadUsers();
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToManagerDash();
        }

        private void AuditsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToManagerAudits();
        }

        private void TasksButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToManagerTasks();
        }

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToManagerUsers();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Logout();
        }
    }
}
