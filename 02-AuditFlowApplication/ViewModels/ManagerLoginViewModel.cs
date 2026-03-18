using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Services;
using System.ComponentModel;

namespace _02_AuditFlowApplication.ViewModels
{
    public class ManagerLoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthenticationService _authService;
        private string _username;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public ManagerLoginViewModel()
        {
            _authService = new AuthenticationService();
        }

        public (bool Success, User User, string ErrorMessage) Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (false, null, "Please enter username and password.");

            User user = _authService.AuthenticateUser(username, password);

            if (user == null)
                return (false, null, "Invalid username or password.");

            if (user.Role != UserRole.Manager)
                return (false, null, "Auditors must use the Auditor Login.");

            return (true, user, null);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}