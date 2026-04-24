using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.ViewModels;
using Shouldly;
using Xunit;

namespace AuditFlowUnitTesting
{
    public class ManagerAuditViewModelTests
    {
        private readonly ManagerAuditViewModel _viewModel;
        private readonly List<Audit> _testAudits;

        public ManagerAuditViewModelTests()
        {
            _viewModel = new ManagerAuditViewModel();
            _testAudits = new List<Audit>
            {
                new Audit
                {
                    AuditId = 1,
                    AuditName = "IT Systems Audit",
                    Type = AuditType.Security,
                    Status = AuditStatus.InProgress,
                    StartDate = DateTime.Now.AddDays(-5),
                    EndDate = DateTime.Now.AddDays(5),
                    CreatedByUserID = 1,
                    CreatedDate = DateTime.Now
                },
                new Audit
                {
                    AuditId = 2,
                    AuditName = "Financial Review",
                    Type = AuditType.Financial,
                    Status = AuditStatus.NotStarted,
                    StartDate = DateTime.Now.AddDays(1),
                    EndDate = DateTime.Now.AddDays(10),
                    CreatedByUserID = 1,
                    CreatedDate = DateTime.Now
                },
                new Audit
                {
                    AuditId = 3,
                    AuditName = "Safety Inspection",
                    Type = AuditType.Safety,
                    Status = AuditStatus.Completed,
                    StartDate = DateTime.Now.AddDays(-10),
                    EndDate = DateTime.Now.AddDays(-1),
                    CreatedByUserID = 1,
                    CreatedDate = DateTime.Now
                },
                new Audit
                {
                    AuditId = 4,
                    AuditName = "IT Security Review",
                    Type = AuditType.Security,
                    Status = AuditStatus.Overdue,
                    StartDate = DateTime.Now.AddDays(-20),
                    EndDate = DateTime.Now.AddDays(-5),
                    CreatedByUserID = 1,
                    CreatedDate = DateTime.Now
                }
            };

            SetAllAudits(_viewModel, _testAudits);
        }

        private void SetAllAudits(ManagerAuditViewModel viewModel, List<Audit> audits)
        {
            var field = typeof(ManagerAuditViewModel)
                .GetField("_allAudits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(viewModel, audits);

            var filteredProp = typeof(ManagerAuditViewModel)
                .GetProperty(nameof(ManagerAuditViewModel.FilteredAudits));
            filteredProp?.SetValue(viewModel, audits);
        }

        #region FilterByStatus

        [Fact]
        public void SelectedStatus_WhenSetToInProgress_ShouldReturnOnlyInProgressAudits()
        {
            _viewModel.SelectedStatus = AuditStatus.InProgress;

            _viewModel.FilteredAudits.ShouldAllBe(a => a.Status == AuditStatus.InProgress);
            _viewModel.FilteredAudits.Count.ShouldBe(1);
        }

        [Fact]
        public void SelectedStatus_WhenSetToCompleted_ShouldReturnOnlyCompletedAudits()
        {
            _viewModel.SelectedStatus = AuditStatus.Completed;

            _viewModel.FilteredAudits.ShouldAllBe(a => a.Status == AuditStatus.Completed);
            _viewModel.FilteredAudits.Count.ShouldBe(1);
        }

        [Fact]
        public void SelectedStatus_WhenSetToNotStarted_ShouldReturnOnlyNotStartedAudits()
        {
            _viewModel.SelectedStatus = AuditStatus.NotStarted;

            _viewModel.FilteredAudits.ShouldAllBe(a => a.Status == AuditStatus.NotStarted);
            _viewModel.FilteredAudits.Count.ShouldBe(1);
        }

        [Fact]
        public void SelectedStatus_WhenSetToOverdue_ShouldReturnOnlyOverdueAudits()
        {
            _viewModel.SelectedStatus = AuditStatus.Overdue;

            _viewModel.FilteredAudits.ShouldAllBe(a => a.Status == AuditStatus.Overdue);
            _viewModel.FilteredAudits.Count.ShouldBe(1);
        }

        [Fact]
        public void SelectedStatus_WhenSetToNull_ShouldReturnAllAudits()
        {
            _viewModel.SelectedStatus = AuditStatus.Completed;
            _viewModel.SelectedStatus = null;

            _viewModel.FilteredAudits.Count.ShouldBe(4);
        }

        #endregion

        #region FilterByType

        [Fact]
        public void SelectedType_WhenSetToSecurity_ShouldReturnOnlySecurityAudits()
        {
            _viewModel.SelectedType = AuditType.Security;

            _viewModel.FilteredAudits.ShouldAllBe(a => a.Type == AuditType.Security);
            _viewModel.FilteredAudits.Count.ShouldBe(2);
        }

        [Fact]
        public void SelectedType_WhenSetToFinancial_ShouldReturnOnlyFinancialAudits()
        {
            _viewModel.SelectedType = AuditType.Financial;

            _viewModel.FilteredAudits.ShouldAllBe(a => a.Type == AuditType.Financial);
            _viewModel.FilteredAudits.Count.ShouldBe(1);
        }

        [Fact]
        public void SelectedType_WhenSetToSafety_ShouldReturnOnlySafetyAudits()
        {
            _viewModel.SelectedType = AuditType.Safety;

            _viewModel.FilteredAudits.ShouldAllBe(a => a.Type == AuditType.Safety);
            _viewModel.FilteredAudits.Count.ShouldBe(1);
        }

        [Fact]
        public void SelectedType_WhenSetToNull_ShouldReturnAllAudits()
        {
            _viewModel.SelectedType = AuditType.Security;
            _viewModel.SelectedType = null;

            _viewModel.FilteredAudits.Count.ShouldBe(4);
        }

        #endregion

        #region CombinedFilters

        [Fact]
        public void ApplyFilters_WhenStatusAndTypeSet_ShouldReturnMatchingAudits()
        {
            _viewModel.SelectedType = AuditType.Security;
            _viewModel.SelectedStatus = AuditStatus.InProgress;

            _viewModel.FilteredAudits.Count.ShouldBe(1);
            _viewModel.FilteredAudits[0].AuditName.ShouldBe("IT Systems Audit");
        }

        [Fact]
        public void ApplyFilters_WhenNoMatchingCombination_ShouldReturnEmptyList()
        {
            _viewModel.SelectedType = AuditType.Financial;
            _viewModel.SelectedStatus = AuditStatus.Overdue;

            _viewModel.FilteredAudits.ShouldBeEmpty();
        }

        [Fact]
        public void ApplyFilters_WhenSearchAndTypeSet_ShouldReturnMatchingAudits()
        {
            _viewModel.SelectedType = AuditType.Security;
            _viewModel.SearchText = "IT Systems";

            _viewModel.FilteredAudits.Count.ShouldBe(1);
            _viewModel.FilteredAudits[0].AuditName.ShouldBe("IT Systems Audit");
        }

        #endregion

        #region Search

        [Fact]
        public void SearchText_WhenSetToPartialName_ShouldFilterByName()
        {
            _viewModel.SearchText = "IT";

            _viewModel.FilteredAudits.Count.ShouldBe(2);
            _viewModel.FilteredAudits.ShouldAllBe(a => a.AuditName.StartsWith("IT", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void SearchText_WhenSetToExactName_ShouldReturnSingleResult()
        {
            _viewModel.SearchText = "Financial Review";

            _viewModel.FilteredAudits.Count.ShouldBe(1);
            _viewModel.FilteredAudits[0].AuditName.ShouldBe("Financial Review");
        }

        [Fact]
        public void SearchText_WhenCleared_ShouldReturnAllAudits()
        {
            _viewModel.SearchText = "IT";
            _viewModel.SearchText = string.Empty;

            _viewModel.FilteredAudits.Count.ShouldBe(4);
        }

        [Fact]
        public void SearchText_WhenNoMatch_ShouldReturnEmptyList()
        {
            _viewModel.SearchText = "NonExistentAudit";

            _viewModel.FilteredAudits.ShouldBeEmpty();
        }

        [Fact]
        public void GetSearchSuggestions_WithPartialMatch_ShouldReturnMatchingAudits()
        {
            var suggestions = _viewModel.GetSearchSuggestions("IT");

            suggestions.Count.ShouldBe(2);
            suggestions.ShouldAllBe(a => a.AuditName.StartsWith("IT", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void GetSearchSuggestions_WithEmptyString_ShouldReturnEmptyList()
        {
            var suggestions = _viewModel.GetSearchSuggestions(string.Empty);

            suggestions.ShouldBeEmpty();
        }

        [Fact]
        public void GetSearchSuggestions_WithNoMatch_ShouldReturnEmptyList()
        {
            var suggestions = _viewModel.GetSearchSuggestions("XYZ");

            suggestions.ShouldBeEmpty();
        }

        [Fact]
        public void GetSearchSuggestions_WithNullString_ShouldReturnEmptyList()
        {
            var suggestions = _viewModel.GetSearchSuggestions(null);

            suggestions.ShouldBeEmpty();
        }

        #endregion

        #region SaveAudit Validation

        [Fact]
        public void SaveAudit_WithEmptyAuditName_ShouldReturnFailure()
        {
            var (success, errorMessage) = _viewModel.SaveAudit(
                string.Empty, "Security", DateTime.Now, DateTime.Now.AddDays(5),
                false, null, 1);

            success.ShouldBeFalse();
            errorMessage.ShouldBe("Audit Name, Type, Start Date and End Date are all required.");
        }

        [Fact]
        public void SaveAudit_WithNullAuditType_ShouldReturnFailure()
        {
            var (success, errorMessage) = _viewModel.SaveAudit(
                "Test Audit", null, DateTime.Now, DateTime.Now.AddDays(5),
                false, null, 1);

            success.ShouldBeFalse();
            errorMessage.ShouldBe("Audit Name, Type, Start Date and End Date are all required.");
        }

        [Fact]
        public void SaveAudit_WithNullStartDate_ShouldReturnFailure()
        {
            var (success, errorMessage) = _viewModel.SaveAudit(
                "Test Audit", "Security", null, DateTime.Now.AddDays(5),
                false, null, 1);

            success.ShouldBeFalse();
            errorMessage.ShouldBe("Audit Name, Type, Start Date and End Date are all required.");
        }

        [Fact]
        public void SaveAudit_WithNullEndDate_ShouldReturnFailure()
        {
            var (success, errorMessage) = _viewModel.SaveAudit(
                "Test Audit", "Security", DateTime.Now, null,
                false, null, 1);

            success.ShouldBeFalse();
            errorMessage.ShouldBe("Audit Name, Type, Start Date and End Date are all required.");
        }

        [Fact]
        public void SaveAudit_WhenEndDateBeforeStartDate_ShouldReturnFailure()
        {
            var (success, errorMessage) = _viewModel.SaveAudit(
                "Test Audit", "Security",
                DateTime.Now.AddDays(5), DateTime.Now,
                false, null, 1);

            success.ShouldBeFalse();
            errorMessage.ShouldBe("End Date must be after Start Date.");
        }

        [Fact]
        public void SaveAudit_WhenEndDateEqualsStartDate_ShouldReturnFailure()
        {
            var date = DateTime.Now;

            var (success, errorMessage) = _viewModel.SaveAudit(
                "Test Audit", "Security", date, date,
                false, null, 1);

            success.ShouldBeFalse();
            errorMessage.ShouldBe("End Date must be after Start Date.");
        }

        [Fact]
        public void SaveAudit_WhenRecurringWithNoFrequency_ShouldReturnFailure()
        {
            var (success, errorMessage) = _viewModel.SaveAudit(
                "Test Audit", "Security",
                DateTime.Now, DateTime.Now.AddDays(5),
                true, null, 1);

            success.ShouldBeFalse();
            errorMessage.ShouldBe("Please select a recurrence frequency.");
        }

        [Fact]
        public void SaveAudit_WhenRecurringWithEmptyFrequency_ShouldReturnFailure()
        {
            var (success, errorMessage) = _viewModel.SaveAudit(
                "Test Audit", "Security",
                DateTime.Now, DateTime.Now.AddDays(5),
                true, string.Empty, 1);

            success.ShouldBeFalse();
            errorMessage.ShouldBe("Please select a recurrence frequency.");
        }

        [Fact(Skip = "Requires database connection - integration test only")]
        public void SaveAudit_WhenNotRecurringWithNoFrequency_ShouldPassValidation()
        {
            // Will fail at DB check but validation should pass
            var (success, errorMessage) = _viewModel.SaveAudit(
                "UniqueAuditNameThatDoesNotExist999", "Security",
                DateTime.Now, DateTime.Now.AddDays(5),
                false, null, 1);

            // Validation passed - either succeeds or fails at DB level
            errorMessage.ShouldNotBe("Audit Name, Type, Start Date and End Date are all required.");
            errorMessage.ShouldNotBe("End Date must be after Start Date.");
            errorMessage.ShouldNotBe("Please select a recurrence frequency.");
        }

        #endregion

        #region PropertyChanged

        [Fact]
        public void FilteredAudits_WhenChanged_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ManagerAuditViewModel.FilteredAudits))
                    propertyChangedRaised = true;
            };

            _viewModel.SelectedStatus = AuditStatus.Completed;

            propertyChangedRaised.ShouldBeTrue();
        }

        [Fact]
        public void SelectedStatus_WhenChanged_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ManagerAuditViewModel.SelectedStatus))
                    propertyChangedRaised = true;
            };

            _viewModel.SelectedStatus = AuditStatus.Completed;

            propertyChangedRaised.ShouldBeTrue();
        }

        [Fact]
        public void SelectedType_WhenChanged_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ManagerAuditViewModel.SelectedType))
                    propertyChangedRaised = true;
            };

            _viewModel.SelectedType = AuditType.Security;

            propertyChangedRaised.ShouldBeTrue();
        }

        [Fact]
        public void SearchText_WhenChanged_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ManagerAuditViewModel.SearchText))
                    propertyChangedRaised = true;
            };

            _viewModel.SearchText = "Test";

            propertyChangedRaised.ShouldBeTrue();
        }

        #endregion
    }
}
