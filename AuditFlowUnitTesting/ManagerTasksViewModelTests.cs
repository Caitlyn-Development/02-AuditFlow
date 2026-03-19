using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.ViewModels;
using Shouldly;
using Xunit;

namespace AuditFlowUnitTesting
{
    public class ManagerTaskViewModelTests
    {
        private readonly ManagerTaskViewModel _viewModel;
        private readonly List<AuditTask> _testTasks;
        private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

        public ManagerTaskViewModelTests()
        {
            _viewModel = new ManagerTaskViewModel();
            _testTasks = new List<AuditTask>
            {
                new AuditTask
                {
                    TaskId = 1,
                    TaskName = "Review Server Logs",
                    AuditId = 1,
                    AuditName = "IT Systems Audit",
                    AssignedToUserId = 2,
                    AssignedToUser = new User { UserID = 2, FullName = "Sarah Johnson", Role = UserRole.Auditor },
                    DueDate = DateTime.Now.AddDays(5),
                    Status = AuditTaskStatus.InProgress
                },
                new AuditTask
                {
                    TaskId = 2,
                    TaskName = "Network Security Scan",
                    AuditId = 1,
                    AuditName = "IT Systems Audit",
                    AssignedToUserId = 3,
                    AssignedToUser = new User { UserID = 3, FullName = "Mike Williams", Role = UserRole.Auditor },
                    DueDate = DateTime.Now.AddDays(10),
                    Status = AuditTaskStatus.NotStarted
                },
                new AuditTask
                {
                    TaskId = 3,
                    TaskName = "Financial Records Review",
                    AuditId = 2,
                    AuditName = "Financial Compliance Q3",
                    AssignedToUserId = 2,
                    AssignedToUser = new User { UserID = 2, FullName = "Sarah Johnson", Role = UserRole.Auditor },
                    DueDate = DateTime.Now.AddDays(-5),
                    Status = AuditTaskStatus.Completed
                },
                new AuditTask
                {
                    TaskId = 4,
                    TaskName = "GDPR Compliance Check",
                    AuditId = 3,
                    AuditName = "Data Privacy Review",
                    AssignedToUserId = 3,
                    AssignedToUser = new User { UserID = 3, FullName = "Mike Williams", Role = UserRole.Auditor },
                    DueDate = DateTime.Now.AddDays(-10),
                    Status = AuditTaskStatus.Overdue
                }
            };

            SetAllTasks(_viewModel, _testTasks);
        }

        private void SetAllTasks(ManagerTaskViewModel viewModel, List<AuditTask> tasks)
        {
            var field = typeof(ManagerTaskViewModel)
                .GetField("_allTasks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(viewModel, tasks);

            var filteredProp = typeof(ManagerTaskViewModel)
                .GetProperty(nameof(ManagerTaskViewModel.FilteredTasks));
            filteredProp?.SetValue(viewModel, tasks);
        }

        #region FilterByStatus

        [Fact]
        public void SelectedStatus_WhenSetToInProgress_ShouldReturnOnlyInProgressTasks()
        {
            _viewModel.SelectedStatus = AuditTaskStatus.InProgress;

            _viewModel.FilteredTasks.ShouldAllBe(t => t.Status == AuditTaskStatus.InProgress);
            _viewModel.FilteredTasks.Count.ShouldBe(1);
        }

        [Fact]
        public void SelectedStatus_WhenSetToCompleted_ShouldReturnOnlyCompletedTasks()
        {
            _viewModel.SelectedStatus = AuditTaskStatus.Completed;

            _viewModel.FilteredTasks.ShouldAllBe(t => t.Status == AuditTaskStatus.Completed);
            _viewModel.FilteredTasks.Count.ShouldBe(1);
        }

        [Fact]
        public void SelectedStatus_WhenSetToNotStarted_ShouldReturnOnlyNotStartedTasks()
        {
            _viewModel.SelectedStatus = AuditTaskStatus.NotStarted;

            _viewModel.FilteredTasks.ShouldAllBe(t => t.Status == AuditTaskStatus.NotStarted);
            _viewModel.FilteredTasks.Count.ShouldBe(1);
        }

        [Fact]
        public void SelectedStatus_WhenSetToOverdue_ShouldReturnOnlyOverdueTasks()
        {
            _viewModel.SelectedStatus = AuditTaskStatus.Overdue;

            _viewModel.FilteredTasks.ShouldAllBe(t => t.Status == AuditTaskStatus.Overdue);
            _viewModel.FilteredTasks.Count.ShouldBe(1);
        }

        [Fact]
        public void SelectedStatus_WhenSetToNull_ShouldReturnAllTasks()
        {
            _viewModel.SelectedStatus = AuditTaskStatus.Completed;
            _viewModel.SelectedStatus = null;

            _viewModel.FilteredTasks.Count.ShouldBe(4);
        }

        #endregion

        #region FilterByAudit

        [Fact]
        public void SelectedAudit_WhenSetToITSystemsAudit_ShouldReturnOnlyITSystemsTasks()
        {
            _viewModel.SelectedAudit = "IT Systems Audit";

            _viewModel.FilteredTasks.ShouldAllBe(t => t.AuditName == "IT Systems Audit");
            _viewModel.FilteredTasks.Count.ShouldBe(2);
        }

        [Fact]
        public void SelectedAudit_WhenSetToFinancialCompliance_ShouldReturnOnlyFinancialTasks()
        {
            _viewModel.SelectedAudit = "Financial Compliance Q3";

            _viewModel.FilteredTasks.ShouldAllBe(t => t.AuditName == "Financial Compliance Q3");
            _viewModel.FilteredTasks.Count.ShouldBe(1);
        }

        [Fact]
        public void SelectedAudit_WhenSetToNull_ShouldReturnAllTasks()
        {
            _viewModel.SelectedAudit = "IT Systems Audit";
            _viewModel.SelectedAudit = null;

            _viewModel.FilteredTasks.Count.ShouldBe(4);
        }

        [Fact]
        public void SelectedAudit_WhenSetToNonExistent_ShouldReturnEmptyList()
        {
            _viewModel.SelectedAudit = "NonExistentAudit";

            _viewModel.FilteredTasks.ShouldBeEmpty();
        }

        #endregion

        #region FilterByUser

        [Fact]
        public void SelectedUser_WhenSetToSarahJohnson_ShouldReturnOnlySarahsTasks()
        {
            _viewModel.SelectedUser = "Sarah Johnson";

            _viewModel.FilteredTasks.ShouldAllBe(t => t.AssignedToUser.FullName == "Sarah Johnson");
            _viewModel.FilteredTasks.Count.ShouldBe(2);
        }

        [Fact]
        public void SelectedUser_WhenSetToMikeWilliams_ShouldReturnOnlyMikesTasks()
        {
            _viewModel.SelectedUser = "Mike Williams";

            _viewModel.FilteredTasks.ShouldAllBe(t => t.AssignedToUser.FullName == "Mike Williams");
            _viewModel.FilteredTasks.Count.ShouldBe(2);
        }

        [Fact]
        public void SelectedUser_WhenSetToNull_ShouldReturnAllTasks()
        {
            _viewModel.SelectedUser = "Sarah Johnson";
            _viewModel.SelectedUser = null;

            _viewModel.FilteredTasks.Count.ShouldBe(4);
        }

        [Fact]
        public void SelectedUser_WhenSetToNonExistent_ShouldReturnEmptyList()
        {
            _viewModel.SelectedUser = "NonExistentUser";

            _viewModel.FilteredTasks.ShouldBeEmpty();
        }

        #endregion

        #region CombinedFilters

        [Fact]
        public void ApplyFilters_WhenStatusAndAuditSet_ShouldReturnMatchingTasks()
        {
            _viewModel.SelectedAudit = "IT Systems Audit";
            _viewModel.SelectedStatus = AuditTaskStatus.InProgress;

            _viewModel.FilteredTasks.Count.ShouldBe(1);
            _viewModel.FilteredTasks[0].TaskName.ShouldBe("Review Server Logs");
        }

        [Fact]
        public void ApplyFilters_WhenUserAndStatusSet_ShouldReturnMatchingTasks()
        {
            _viewModel.SelectedUser = "Sarah Johnson";
            _viewModel.SelectedStatus = AuditTaskStatus.Completed;

            _viewModel.FilteredTasks.Count.ShouldBe(1);
            _viewModel.FilteredTasks[0].TaskName.ShouldBe("Financial Records Review");
        }

        [Fact]
        public void ApplyFilters_WhenAllFiltersSet_ShouldReturnMatchingTasks()
        {
            _viewModel.SelectedAudit = "IT Systems Audit";
            _viewModel.SelectedUser = "Sarah Johnson";
            _viewModel.SelectedStatus = AuditTaskStatus.InProgress;

            _viewModel.FilteredTasks.Count.ShouldBe(1);
            _viewModel.FilteredTasks[0].TaskName.ShouldBe("Review Server Logs");
        }

        [Fact]
        public void ApplyFilters_WhenNoMatchingCombination_ShouldReturnEmptyList()
        {
            _viewModel.SelectedAudit = "Financial Compliance Q3";
            _viewModel.SelectedStatus = AuditTaskStatus.InProgress;

            _viewModel.FilteredTasks.ShouldBeEmpty();
        }

        #endregion

        #region Search

        [Fact]
        public void GetSearchSuggestions_WithPartialMatch_ShouldReturnMatchingTasks()
        {
            var suggestions = _viewModel.GetSearchSuggestions("Review");

            suggestions.Count.ShouldBe(1);
            suggestions[0].TaskName.ShouldBe("Review Server Logs");
        }

        [Fact]
        public void GetSearchSuggestions_WithPartialMatchMultiple_ShouldReturnAllMatches()
        {
            var suggestions = _viewModel.GetSearchSuggestions("N");

            suggestions.Count.ShouldBe(1);
            suggestions[0].TaskName.ShouldBe("Network Security Scan");
        }

        [Fact]
        public void GetSearchSuggestions_WithEmptyString_ShouldReturnEmptyList()
        {
            var suggestions = _viewModel.GetSearchSuggestions(string.Empty);

            suggestions.ShouldBeEmpty();
        }

        [Fact]
        public void GetSearchSuggestions_WithNull_ShouldReturnEmptyList()
        {
            var suggestions = _viewModel.GetSearchSuggestions(null);

            suggestions.ShouldBeEmpty();
        }

        [Fact]
        public void GetSearchSuggestions_WithNoMatch_ShouldReturnEmptyList()
        {
            var suggestions = _viewModel.GetSearchSuggestions("XYZ");

            suggestions.ShouldBeEmpty();
        }

        [Fact]
        public void GetSearchSuggestions_IsCaseInsensitive()
        {
            var suggestions = _viewModel.GetSearchSuggestions("review");

            suggestions.Count.ShouldBe(1);
            suggestions[0].TaskName.ShouldBe("Review Server Logs");
        }

        #endregion

        #region FileValidation

        [Fact]
        public void IsValidFileType_WithPdfExtension_ShouldReturnTrue()
        {
            var result = _viewModel.IsValidFileType("document.pdf", _allowedExtensions);

            result.ShouldBeTrue();
        }

        [Fact]
        public void IsValidFileType_WithDocxExtension_ShouldReturnTrue()
        {
            var result = _viewModel.IsValidFileType("document.docx", _allowedExtensions);

            result.ShouldBeTrue();
        }

        [Fact]
        public void IsValidFileType_WithJpgExtension_ShouldReturnTrue()
        {
            var result = _viewModel.IsValidFileType("image.jpg", _allowedExtensions);

            result.ShouldBeTrue();
        }

        [Fact]
        public void IsValidFileType_WithPngExtension_ShouldReturnTrue()
        {
            var result = _viewModel.IsValidFileType("image.png", _allowedExtensions);

            result.ShouldBeTrue();
        }

        [Fact]
        public void IsValidFileType_WithInvalidExtension_ShouldReturnFalse()
        {
            var result = _viewModel.IsValidFileType("file.exe", _allowedExtensions);

            result.ShouldBeFalse();
        }

        [Fact]
        public void IsValidFileType_WithZipExtension_ShouldReturnFalse()
        {
            var result = _viewModel.IsValidFileType("archive.zip", _allowedExtensions);

            result.ShouldBeFalse();
        }

        [Fact]
        public void IsValidFileType_WithUpperCaseExtension_ShouldReturnTrue()
        {
            var result = _viewModel.IsValidFileType("document.PDF", _allowedExtensions);

            result.ShouldBeTrue();
        }

        [Fact]
        public void IsValidFileType_WithMixedCaseExtension_ShouldReturnTrue()
        {
            var result = _viewModel.IsValidFileType("document.Pdf", _allowedExtensions);

            result.ShouldBeTrue();
        }

        [Fact]
        public void IsValidFileType_WithNoExtension_ShouldReturnFalse()
        {
            var result = _viewModel.IsValidFileType("filewithnoextension", _allowedExtensions);

            result.ShouldBeFalse();
        }

        #endregion

        #region PropertyChanged

        [Fact]
        public void FilteredTasks_WhenChanged_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ManagerTaskViewModel.FilteredTasks))
                    propertyChangedRaised = true;
            };

            _viewModel.SelectedStatus = AuditTaskStatus.Completed;

            propertyChangedRaised.ShouldBeTrue();
        }

        [Fact]
        public void SelectedStatus_WhenChanged_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ManagerTaskViewModel.SelectedStatus))
                    propertyChangedRaised = true;
            };

            _viewModel.SelectedStatus = AuditTaskStatus.Completed;

            propertyChangedRaised.ShouldBeTrue();
        }

        [Fact]
        public void SelectedAudit_WhenChanged_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ManagerTaskViewModel.SelectedAudit))
                    propertyChangedRaised = true;
            };

            _viewModel.SelectedAudit = "IT Systems Audit";

            propertyChangedRaised.ShouldBeTrue();
        }

        [Fact]
        public void SelectedUser_WhenChanged_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ManagerTaskViewModel.SelectedUser))
                    propertyChangedRaised = true;
            };

            _viewModel.SelectedUser = "Sarah Johnson";

            propertyChangedRaised.ShouldBeTrue();
        }

        #endregion
    }
}
