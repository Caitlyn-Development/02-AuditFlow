using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.Services;
using System.ComponentModel;

namespace _02_AuditFlowApplication.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthenticationService _authService;
        private string _username;
        private string _errorMessage;
        private bool _hasError;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public bool HasError
        {
            get => _hasError;
            set
            {
                _hasError = value;
                OnPropertyChanged(nameof(HasError));
            }
        }

        public LoginViewModel()
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

            if (user.Role != UserRole.Auditor)
                return (false, null, "Managers must use the Manager Login.");

            return (true, user, null);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
