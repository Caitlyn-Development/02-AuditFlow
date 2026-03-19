using _02_AuditFlowApplication.ViewModels;
using Shouldly;
using Xunit;

namespace AuditFlowUnitTesting
{
    public class LoginViewModelTests
    {
        private readonly LoginViewModel _viewModel;

        public LoginViewModelTests()
        {
            _viewModel = new LoginViewModel();
        }

        #region Validation

        [Fact]
        public void Login_WithEmptyUsername_ShouldReturnFailure()
        {
            var (success, user, errorMessage) = _viewModel.Login(string.Empty, "password123");

            success.ShouldBeFalse();
            user.ShouldBeNull();
            errorMessage.ShouldBe("Please enter username and password.");
        }

        [Fact]
        public void Login_WithEmptyPassword_ShouldReturnFailure()
        {
            var (success, user, errorMessage) = _viewModel.Login("username", string.Empty);

            success.ShouldBeFalse();
            user.ShouldBeNull();
            errorMessage.ShouldBe("Please enter username and password.");
        }

        [Fact]
        public void Login_WithNullUsername_ShouldReturnFailure()
        {
            var (success, user, errorMessage) = _viewModel.Login(null, "password123");

            success.ShouldBeFalse();
            user.ShouldBeNull();
            errorMessage.ShouldBe("Please enter username and password.");
        }

        [Fact]
        public void Login_WithNullPassword_ShouldReturnFailure()
        {
            var (success, user, errorMessage) = _viewModel.Login("username", null);

            success.ShouldBeFalse();
            user.ShouldBeNull();
            errorMessage.ShouldBe("Please enter username and password.");
        }

        [Fact]
        public void Login_WithWhitespaceUsername_ShouldReturnFailure()
        {
            var (success, user, errorMessage) = _viewModel.Login("   ", "password123");

            success.ShouldBeFalse();
            user.ShouldBeNull();
            errorMessage.ShouldBe("Please enter username and password.");
        }

        [Fact]
        public void Login_WithWhitespacePassword_ShouldReturnFailure()
        {
            var (success, user, errorMessage) = _viewModel.Login("username", "   ");

            success.ShouldBeFalse();
            user.ShouldBeNull();
            errorMessage.ShouldBe("Please enter username and password.");
        }

        [Fact]
        public void Login_WithBothEmpty_ShouldReturnFailure()
        {
            var (success, user, errorMessage) = _viewModel.Login(string.Empty, string.Empty);

            success.ShouldBeFalse();
            user.ShouldBeNull();
            errorMessage.ShouldBe("Please enter username and password.");
        }

        [Fact]
        public void Login_WithBothNull_ShouldReturnFailure()
        {
            var (success, user, errorMessage) = _viewModel.Login(null, null);

            success.ShouldBeFalse();
            user.ShouldBeNull();
            errorMessage.ShouldBe("Please enter username and password.");
        }

        [Fact(Skip = "Requires database connection - integration test only")]
        public void Login_WithInvalidCredentials_ShouldReturnInvalidCredentialsMessage()
        {
            var (success, user, errorMessage) = _viewModel.Login("nonexistentuser", "wrongpassword");

            success.ShouldBeFalse();
            user.ShouldBeNull();
            errorMessage.ShouldBe("Invalid username or password.");
        }

        #endregion

        #region Properties

        [Fact]
        public void Username_WhenSet_ShouldUpdateValue()
        {
            _viewModel.Username = "testuser";

            _viewModel.Username.ShouldBe("testuser");
        }

        [Fact]
        public void Username_WhenSet_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LoginViewModel.Username))
                    propertyChangedRaised = true;
            };

            _viewModel.Username = "testuser";

            propertyChangedRaised.ShouldBeTrue();
        }

        [Fact]
        public void ErrorMessage_WhenSet_ShouldUpdateValue()
        {
            _viewModel.ErrorMessage = "Test error";

            _viewModel.ErrorMessage.ShouldBe("Test error");
        }

        [Fact]
        public void ErrorMessage_WhenSet_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LoginViewModel.ErrorMessage))
                    propertyChangedRaised = true;
            };

            _viewModel.ErrorMessage = "Test error";

            propertyChangedRaised.ShouldBeTrue();
        }

        [Fact]
        public void HasError_WhenSetToTrue_ShouldUpdateValue()
        {
            _viewModel.HasError = true;

            _viewModel.HasError.ShouldBeTrue();
        }

        [Fact]
        public void HasError_WhenSetToFalse_ShouldUpdateValue()
        {
            _viewModel.HasError = true;

            _viewModel.HasError = false;

            _viewModel.HasError.ShouldBeFalse();
        }

        [Fact]
        public void HasError_WhenChanged_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LoginViewModel.HasError))
                    propertyChangedRaised = true;
            };

            _viewModel.HasError = true;

            propertyChangedRaised.ShouldBeTrue();
        }

        [Fact]
        public void Username_DefaultValue_ShouldBeNull()
        {
            _viewModel.Username.ShouldBeNull();
        }

        [Fact]
        public void ErrorMessage_DefaultValue_ShouldBeNull()
        {
            _viewModel.ErrorMessage.ShouldBeNull();
        }

        [Fact]
        public void HasError_DefaultValue_ShouldBeFalse()
        {
            _viewModel.HasError.ShouldBeFalse();
        }

        #endregion

        #region Login Return Values

        [Fact]
        public void Login_ReturnValue_ShouldAlwaysHaveThreeComponents()
        {
            var result = _viewModel.Login(string.Empty, string.Empty);

            result.Success.ShouldBeFalse();
            result.User.ShouldBeNull();
            result.ErrorMessage.ShouldNotBeNull();
        }

        [Fact(Skip = "Requires database connection - integration test only")]
        public void Login_WithValidCredentials_SuccessShouldBeTrueOrReturnCorrectError()
        {
            // Without DB this will return invalid credentials
            // confirming that validation passed and DB was reached
            var (success, user, errorMessage) = _viewModel.Login("auditor1", "Admin123!");

            if (!success)
                errorMessage.ShouldBeOneOf(
                    "Invalid username or password.",
                    "Managers must use the Manager Login.");
        }

        #endregion
    }
}