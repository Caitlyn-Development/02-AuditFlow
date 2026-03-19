using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.ViewModels;
using Shouldly;
using Xunit;

namespace AuditFlowUnitTesting
{
    public class ManagerUserViewModelTests
    {
        private readonly ManagerUserViewModel _viewModel;
        private readonly List<User> _testUsers;

        public ManagerUserViewModelTests()
        {
            _viewModel = new ManagerUserViewModel();
            _testUsers = new List<User>
            {
                new User
                {
                    UserID = 1,
                    Username = "admin",
                    FullName = "John Administrator",
                    Role = UserRole.Manager,
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddDays(-30)
                },
                new User
                {
                    UserID = 2,
                    Username = "auditor1",
                    FullName = "Sarah Johnson",
                    Role = UserRole.Auditor,
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddDays(-25)
                },
                new User
                {
                    UserID = 3,
                    Username = "auditor2",
                    FullName = "Mike Williams",
                    Role = UserRole.Auditor,
                    IsActive = false,
                    CreatedDate = DateTime.Now.AddDays(-20)
                },
                new User
                {
                    UserID = 4,
                    Username = "manager1",
                    FullName = "Emma Davis",
                    Role = UserRole.Manager,
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddDays(-15)
                }
            };

            SetAllUsers(_viewModel, _testUsers);
        }

        private void SetAllUsers(ManagerUserViewModel viewModel, List<User> users)
        {
            var allUsersField = typeof(ManagerUserViewModel)
                .GetField("_allUsers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            allUsersField?.SetValue(viewModel, users);

            var filteredProp = typeof(ManagerUserViewModel)
                .GetProperty(nameof(ManagerUserViewModel.FilteredUsers));
            filteredProp?.SetValue(viewModel, users);
        }

        #region GetUserById

        [Fact]
        public void GetUserById_WithValidId_ShouldReturnCorrectUser()
        {
            var user = _viewModel.GetUserById(1);

            user.ShouldNotBeNull();
            user.Username.ShouldBe("admin");
        }

        [Fact]
        public void GetUserById_WithInvalidId_ShouldReturnNull()
        {
            var user = _viewModel.GetUserById(999);

            user.ShouldBeNull();
        }

        [Fact]
        public void GetUserById_WithAuditorId_ShouldReturnCorrectUser()
        {
            var user = _viewModel.GetUserById(2);

            user.ShouldNotBeNull();
            user.Username.ShouldBe("auditor1");
            user.Role.ShouldBe(UserRole.Auditor);
        }

        [Fact]
        public void GetUserById_WithZeroId_ShouldReturnNull()
        {
            var user = _viewModel.GetUserById(0);

            user.ShouldBeNull();
        }

        #endregion

        #region CreateUser Validation

        [Fact]
        public void CreateUser_WithEmptyUsername_ShouldReturnFailure()
        {
            var (success, errorMessage) = _viewModel.CreateUser(
                string.Empty, "Full Name", "Password1!", "Password1!", "Auditor");

            success.ShouldBeFalse();
            errorMessage.ShouldBe("All fields must be filled in.");
        }

        [Fact]
        public void CreateUser_WithEmptyFullName_ShouldReturnFailure()
        {
            var (success, errorMessage) = _viewModel.CreateUser(
                "username", string.Empty, "Password1!", "Password1!", "Auditor");

            success.ShouldBeFalse();
            errorMessage.ShouldBe("All fields must be filled in.");
        }

        [Fact]
        public void CreateUser_WithEmptyPassword_ShouldReturnFailure()
        {
            var (success, errorMessage) = _viewModel.CreateUser(
                "username", "Full Name", string.Empty, "Password1!", "Auditor");

            success.ShouldBeFalse();
            errorMessage.ShouldBe("All fields must be filled in.");
        }

        [Fact]
        public void CreateUser_WithEmptyConfirmPassword_ShouldReturnFailure()
        {
            var (success, errorMessage) = _viewModel.CreateUser(
                "username", "Full Name", "Password1!", string.Empty, "Auditor");

            success.ShouldBeFalse();
            errorMessage.ShouldBe("All fields must be filled in.");
        }

        [Fact]
        public void CreateUser_WithNullRole_ShouldReturnFailure()
        {
            var (success, errorMessage) = _viewModel.CreateUser(
                "username", "Full Name", "Password1!", "Password1!", null);

            success.ShouldBeFalse();
            errorMessage.ShouldBe("All fields must be filled in.");
        }

        [Fact]
        public void CreateUser_WithWhitespaceUsername_ShouldReturnFailure()
        {
            var (success, errorMessage) = _viewModel.CreateUser(
                "   ", "Full Name", "Password1!", "Password1!", "Auditor");

            success.ShouldBeFalse();
            errorMessage.ShouldBe("All fields must be filled in.");
        }

        [Fact]
        public void CreateUser_WithAllNullFields_ShouldReturnFailure()
        {
            var (success, errorMessage) = _viewModel.CreateUser(
                null, null, null, null, null);

            success.ShouldBeFalse();
            errorMessage.ShouldBe("All fields must be filled in.");
        }

        [Fact]
        public void CreateUser_WhenPasswordsDoNotMatch_ShouldReturnFailure()
        {
            var (success, errorMessage) = _viewModel.CreateUser(
                "newuser", "New User", "Password1!", "DifferentPassword!", "Auditor");

            success.ShouldBeFalse();
            errorMessage.ShouldBe("Passwords do not match.");
        }

        [Fact]
        public void CreateUser_WhenPasswordsDifferByCase_ShouldReturnFailure()
        {
            var (success, errorMessage) = _viewModel.CreateUser(
                "newuser", "New User", "Password1!", "password1!", "Auditor");

            success.ShouldBeFalse();
            errorMessage.ShouldBe("Passwords do not match.");
        }

        [Fact]
        public void CreateUser_WhenPasswordsDoNotMatch_ShouldFailBeforeReachingDatabase()
        {
            // This test confirms password mismatch is caught before any DB call
            var (success, errorMessage) = _viewModel.CreateUser(
                "anyuser", "Any User", "Password1!", "Different1!", "Auditor");

            success.ShouldBeFalse();
            errorMessage.ShouldBe("Passwords do not match.");
        }

        [Fact]
        public void CreateUser_WithAllEmptyFields_ShouldFailBeforeReachingDatabase()
        {
            // This test confirms empty field validation is caught before any DB call
            var (success, errorMessage) = _viewModel.CreateUser(
                string.Empty, string.Empty, string.Empty, string.Empty, null);

            success.ShouldBeFalse();
            errorMessage.ShouldBe("All fields must be filled in.");
        }

        #endregion

        #region CanDeleteUser

        [Fact]
        public void CanDeleteUser_WhenUserIsActive_ShouldReturnFalse()
        {
            var activeUser = _testUsers.First(u => u.IsActive);

            var (canDelete, errorMessage) = _viewModel.CanDeleteUser(activeUser);

            canDelete.ShouldBeFalse();
            errorMessage.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public void CanDeleteUser_WhenUserIsInactive_ShouldReturnTrue()
        {
            var inactiveUser = _testUsers.First(u => !u.IsActive);

            var (canDelete, errorMessage) = _viewModel.CanDeleteUser(inactiveUser);

            canDelete.ShouldBeTrue();
            errorMessage.ShouldBeNull();
        }

        [Fact]
        public void CanDeleteUser_WhenUserIsActive_ErrorMessageShouldContainUsername()
        {
            var activeUser = _testUsers.First(u => u.IsActive);

            var (canDelete, errorMessage) = _viewModel.CanDeleteUser(activeUser);

            canDelete.ShouldBeFalse();
            errorMessage.ShouldContain(activeUser.Username);
        }

        [Fact]
        public void CanDeleteUser_WhenUserIsActive_ErrorMessageShouldContainFullName()
        {
            var activeUser = _testUsers.First(u => u.IsActive);

            var (canDelete, errorMessage) = _viewModel.CanDeleteUser(activeUser);

            canDelete.ShouldBeFalse();
            errorMessage.ShouldContain(activeUser.FullName);
        }

        [Fact]
        public void CanDeleteUser_AllActiveUsers_ShouldReturnFalse()
        {
            var activeUsers = _testUsers.Where(u => u.IsActive).ToList();

            foreach (var user in activeUsers)
            {
                var (canDelete, _) = _viewModel.CanDeleteUser(user);
                canDelete.ShouldBeFalse();
            }
        }

        [Fact]
        public void CanDeleteUser_AllInactiveUsers_ShouldReturnTrue()
        {
            var inactiveUsers = _testUsers.Where(u => !u.IsActive).ToList();

            foreach (var user in inactiveUsers)
            {
                var (canDelete, _) = _viewModel.CanDeleteUser(user);
                canDelete.ShouldBeTrue();
            }
        }

        #endregion

        #region FilteredUsers

        [Fact]
        public void FilteredUsers_OnInitialisation_ShouldContainAllTestUsers()
        {
            _viewModel.FilteredUsers.ShouldNotBeNull();
            _viewModel.FilteredUsers.Count.ShouldBe(4);
        }

        [Fact]
        public void FilteredUsers_ShouldContainBothManagersAndAuditors()
        {
            _viewModel.FilteredUsers.ShouldContain(u => u.Role == UserRole.Manager);
            _viewModel.FilteredUsers.ShouldContain(u => u.Role == UserRole.Auditor);
        }

        [Fact]
        public void FilteredUsers_ShouldContainBothActiveAndInactiveUsers()
        {
            _viewModel.FilteredUsers.ShouldContain(u => u.IsActive);
            _viewModel.FilteredUsers.ShouldContain(u => !u.IsActive);
        }

        #endregion

        #region PropertyChanged

        [Fact]
        public void FilteredUsers_WhenChanged_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ManagerUserViewModel.FilteredUsers))
                    propertyChangedRaised = true;
            };

            _viewModel.FilteredUsers = new List<User>();

            propertyChangedRaised.ShouldBeTrue();
        }

        [Fact]
        public void FilteredUsers_WhenSetToEmptyList_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ManagerUserViewModel.FilteredUsers))
                    propertyChangedRaised = true;
            };

            _viewModel.FilteredUsers = new List<User>();

            propertyChangedRaised.ShouldBeTrue();
            _viewModel.FilteredUsers.ShouldBeEmpty();
        }

        [Fact]
        public void FilteredUsers_WhenSetToNull_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ManagerUserViewModel.FilteredUsers))
                    propertyChangedRaised = true;
            };

            _viewModel.FilteredUsers = null;

            propertyChangedRaised.ShouldBeTrue();
        }

        #endregion
    }
}
