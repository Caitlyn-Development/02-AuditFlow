using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Services;
using System.Windows;
using System.Windows.Controls;

namespace _02_AuditFlowApplication.Views
{
    public partial class EditUserDialog : Window
    {
        private readonly User _userToEdit;
        private readonly User _currentUser;
        private readonly UserService _userService;

        public EditUserDialog(User userToEdit)
        {
            InitializeComponent();
            _userToEdit = userToEdit;
            _currentUser = Application.Current.Properties["CurrentUser"] as User;
            _userService = new UserService();
            PopulateFields();
        }

        private void PopulateFields()
        {
            FullNameBox.Text = _userToEdit.FullName;
            RoleComboBox.SelectedIndex = _userToEdit.Role == UserRole.Auditor ? 0 : 1;
            StatusComboBox.SelectedIndex = _userToEdit.IsActive ? 0 : 1;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var changes = new List<string>();

            string newFullName = FullNameBox.Text.Trim();
            string newPassword = PasswordBox.Password;
            string newRole = ((ComboBoxItem)RoleComboBox.SelectedItem).Content.ToString();
            bool newIsActive = StatusComboBox.SelectedIndex == 0;

            if (newFullName != _userToEdit.FullName)
                changes.Add($"FullName: '{_userToEdit.FullName}' → '{newFullName}'");

            if (!string.IsNullOrWhiteSpace(newPassword))
                changes.Add("Password changed");

            if (newRole != _userToEdit.Role.ToString())
                changes.Add($"Role: '{_userToEdit.Role}' → '{newRole}'");

            if (newIsActive != _userToEdit.IsActive)
                changes.Add($"IsActive: '{_userToEdit.IsActive}' → '{newIsActive}'");

            if (changes.Count == 0)
            {
                MessageBox.Show("No changes were made.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _userToEdit.FullName = newFullName;
            _userToEdit.Role = newRole == "Auditor" ? UserRole.Auditor : UserRole.Manager;
            _userToEdit.IsActive = newIsActive;

            string passwordHash = !string.IsNullOrWhiteSpace(newPassword) ? BCrypt.Net.BCrypt.HashPassword(newPassword) : null;

            _userService.UpdateUser(_userToEdit, passwordHash);
            _userService.LogUserChange(_currentUser, _userToEdit, string.Join(", ", changes));

            MessageBox.Show("User updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
