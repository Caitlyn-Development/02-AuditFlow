using _02_AuditFlowApplication.Helpers;
using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace _02_AuditFlowApplication.Views
{
    public partial class ManagerUserView : UserControl
    {
        private readonly ManagerUserViewModel _viewModel;

        public ManagerUserView()
        {
            InitializeComponent();
            _viewModel = new ManagerUserViewModel();
            DataContext = _viewModel;
            LoadUsers();

            NavigationHelper.WireManagerNavigation(DashboardButton, AuditsButton, TasksButton, UsersButton, LogoutButton);
        }

        private void LoadUsers()
        {
            try
            {
                _viewModel.LoadUsers();
                UsersGrid.ItemsSource = _viewModel.FilteredUsers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int userId = (int)button.Tag;
            var userToEdit = _viewModel.GetUserById(userId);

            if (userToEdit == null) return;

            var dialog = new EditUserDialog(userToEdit);
            dialog.ShowDialog();
            LoadUsers();
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int userId = (int)button.Tag;
            var userToDelete = _viewModel.GetUserById(userId);

            if (userToDelete == null) return;

            var (canDelete, cantDeleteMessage) = _viewModel.CanDeleteUser(userToDelete);

            if (!canDelete)
            {
                MessageBox.Show(cantDeleteMessage, "Cannot Delete Active User",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete the following user?\n\nUsername: {userToDelete.Username}\nFull Name: {userToDelete.FullName}\n\nThis action cannot be undone.",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var currentUser = Application.Current.Properties["CurrentUser"] as User;
                var (success, error) = _viewModel.DeleteUser(userToDelete, currentUser);

                if (success)
                {
                    MessageBox.Show("User deleted successfully.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadUsers();
                }
                else
                {
                    MessageBox.Show($"Error deleting user: {error}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveUser_Click(object sender, RoutedEventArgs e)
        {
            string role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            var (success, errorMessage) = _viewModel.CreateUser(
                UsernameBox.Text.Trim(),
                FullNameBox.Text.Trim(),
                PasswordBox.Password,
                ConfirmPasswordBox.Password,
                role);

            if (!success)
            {
                var image = errorMessage.Contains("already exists")
                    ? MessageBoxImage.Error
                    : MessageBoxImage.Warning;

                MessageBox.Show(errorMessage, "Validation Error", MessageBoxButton.OK, image);
                return;
            }

            var currentUser = Application.Current.Properties["CurrentUser"] as User;
            _viewModel.LogUserCreation(currentUser, UsernameBox.Text.Trim());

            MessageBox.Show($"User '{UsernameBox.Text.Trim()}' created successfully.", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

            UsernameBox.Text = string.Empty;
            FullNameBox.Text = string.Empty;
            PasswordBox.Password = string.Empty;
            ConfirmPasswordBox.Password = string.Empty;
            RoleComboBox.SelectedIndex = -1;

            LoadUsers();
        }
    }
}
