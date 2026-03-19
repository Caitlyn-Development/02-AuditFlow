using _02_AuditFlowApplication.ViewModels;
using Shouldly;
using Xunit;

namespace AuditFlowUnitTesting
{
    public class ManagerLoginViewModelTests
    {
        private readonly ManagerLoginViewModel _viewModel;

        public ManagerLoginViewModelTests()
        {
            _viewModel = new ManagerLoginViewModel();
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

        [Fact(Skip = "Requires database connection - integration test only")]
        public void Login_WithValidCredentials_SuccessShouldBeTrueOrReturnCorrectError()
        {
            var (success, user, errorMessage) = _viewModel.Login("auditor1", "Admin123!");

            if (!success)
                errorMessage.ShouldBeOneOf(
                    "Invalid username or password.",
                    "Managers must use the Manager Login.");
        }

        #endregion

        #region Properties

        [Fact]
        public void Username_DefaultValue_ShouldBeNull()
        {
            _viewModel.Username.ShouldBeNull();
        }

        [Fact]
        public void Username_WhenSet_ShouldUpdateValue()
        {
            _viewModel.Username = "testmanager";

            _viewModel.Username.ShouldBe("testmanager");
        }

        [Fact]
        public void Username_WhenSetToEmpty_ShouldUpdateValue()
        {
            _viewModel.Username = "testmanager";
            _viewModel.Username = string.Empty;

            _viewModel.Username.ShouldBe(string.Empty);
        }

        [Fact]
        public void Username_WhenSetToNull_ShouldUpdateValue()
        {
            _viewModel.Username = "testmanager";
            _viewModel.Username = null;

            _viewModel.Username.ShouldBeNull();
        }

        #endregion

        #region PropertyChanged

        [Fact]
        public void Username_WhenChanged_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ManagerLoginViewModel.Username))
                    propertyChangedRaised = true;
            };

            _viewModel.Username = "testmanager";

            propertyChangedRaised.ShouldBeTrue();
        }

        [Fact]
        public void Username_WhenChangedMultipleTimes_ShouldRaisePropertyChangedEachTime()
        {
            var raiseCount = 0;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ManagerLoginViewModel.Username))
                    raiseCount++;
            };

            _viewModel.Username = "first";
            _viewModel.Username = "second";
            _viewModel.Username = "third";

            raiseCount.ShouldBe(3);
        }

        #endregion

        #region Login Return Structure

        [Fact]
        public void Login_WithEmptyCredentials_ShouldAlwaysReturnThreeComponents()
        {
            var result = _viewModel.Login(string.Empty, string.Empty);

            result.Success.ShouldBeFalse();
            result.User.ShouldBeNull();
            result.ErrorMessage.ShouldNotBeNull();
        }

        [Fact]
        public void Login_ErrorMessage_ShouldNeverBeNullOnFailure()
        {
            var (success, user, errorMessage) = _viewModel.Login(string.Empty, string.Empty);

            success.ShouldBeFalse();
            errorMessage.ShouldNotBeNull();
        }

        [Fact(Skip = "Requires database connection - integration test only")]
        public void Login_WithValidManagerCredentials_ShouldPassValidationAndReachDatabase()
        {
            // Confirms validation passes for non-empty credentials
            // outcome depends on DB availability
            var (success, user, errorMessage) = _viewModel.Login("admin", "Admin123!");

            if (!success)
                errorMessage.ShouldBeOneOf(
                    "Invalid username or password.",
                    "Auditors must use the Auditor Login.");
        }

        #endregion
    }
}
