using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.ViewModels;
using Shouldly;
using Xunit;

namespace AuditFlowUnitTesting
{
    public class AuditViewModelTests
    {
        private readonly AuditViewModel _viewModel;
        private readonly List<Audit> _testAudits;

        public AuditViewModelTests()
        {
            _viewModel = new AuditViewModel();
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
        }

        #region LoadAudits

        [Fact]
        public void LoadAudits_WithValidAudits_ShouldPopulateFilteredAudits()
        {
            _viewModel.LoadAudits(_testAudits);

            _viewModel.FilteredAudits.ShouldNotBeNull();
            _viewModel.FilteredAudits.Count.ShouldBe(4);
        }

        [Fact]
        public void LoadAudits_WithEmptyList_ShouldReturnEmptyFilteredAudits()
        {
            _viewModel.LoadAudits(new List<Audit>());

            _viewModel.FilteredAudits.ShouldNotBeNull();
            _viewModel.FilteredAudits.ShouldBeEmpty();
        }

        [Fact]
        public void LoadAudits_WithNullList_ShouldNotThrow()
        {
            Should.NotThrow(() => _viewModel.LoadAudits(null));
        }

        #endregion

        #region FilterByStatus

        [Fact]
        public void SelectedStatus_WhenSetToInProgress_ShouldReturnOnlyInProgressAudits()
        {
            _viewModel.LoadAudits(_testAudits);

            _viewModel.SelectedStatus = AuditStatus.InProgress;

            _viewModel.FilteredAudits.ShouldAllBe(a => a.Status == AuditStatus.InProgress);
            _viewModel.FilteredAudits.Count.ShouldBe(1);
        }

        [Fact]
        public void SelectedStatus_WhenSetToCompleted_ShouldReturnOnlyCompletedAudits()
        {
            _viewModel.LoadAudits(_testAudits);

            _viewModel.SelectedStatus = AuditStatus.Completed;

            _viewModel.FilteredAudits.ShouldAllBe(a => a.Status == AuditStatus.Completed);
            _viewModel.FilteredAudits.Count.ShouldBe(1);
        }

        [Fact]
        public void SelectedStatus_WhenSetToNull_ShouldReturnAllAudits()
        {
            _viewModel.LoadAudits(_testAudits);
            _viewModel.SelectedStatus = AuditStatus.Completed;

            _viewModel.SelectedStatus = null;

            _viewModel.FilteredAudits.Count.ShouldBe(4);
        }

        #endregion

        #region FilterByType

        [Fact]
        public void SelectedType_WhenSetToSecurity_ShouldReturnOnlySecurityAudits()
        {
            _viewModel.LoadAudits(_testAudits);

            _viewModel.SelectedType = AuditType.Security;

            _viewModel.FilteredAudits.ShouldAllBe(a => a.Type == AuditType.Security);
            _viewModel.FilteredAudits.Count.ShouldBe(2);
        }

        [Fact]
        public void SelectedType_WhenSetToFinancial_ShouldReturnOnlyFinancialAudits()
        {
            _viewModel.LoadAudits(_testAudits);

            _viewModel.SelectedType = AuditType.Financial;

            _viewModel.FilteredAudits.ShouldAllBe(a => a.Type == AuditType.Financial);
            _viewModel.FilteredAudits.Count.ShouldBe(1);
        }

        [Fact]
        public void SelectedType_WhenSetToNull_ShouldReturnAllAudits()
        {
            _viewModel.LoadAudits(_testAudits);
            _viewModel.SelectedType = AuditType.Security;

            _viewModel.SelectedType = null;

            _viewModel.FilteredAudits.Count.ShouldBe(4);
        }

        #endregion

        #region CombinedFilters

        [Fact]
        public void ApplyFilters_WhenStatusAndTypeSet_ShouldReturnMatchingAudits()
        {
            _viewModel.LoadAudits(_testAudits);

            _viewModel.SelectedType = AuditType.Security;
            _viewModel.SelectedStatus = AuditStatus.InProgress;

            _viewModel.FilteredAudits.Count.ShouldBe(1);
            _viewModel.FilteredAudits[0].AuditName.ShouldBe("IT Systems Audit");
        }

        [Fact]
        public void ApplyFilters_WhenNoMatchingCombination_ShouldReturnEmptyList()
        {
            _viewModel.LoadAudits(_testAudits);

            _viewModel.SelectedType = AuditType.Financial;
            _viewModel.SelectedStatus = AuditStatus.Overdue;

            _viewModel.FilteredAudits.ShouldBeEmpty();
        }

        #endregion

        #region Search

        [Fact]
        public void SearchText_WhenSetToPartialName_ShouldFilterByName()
        {
            _viewModel.LoadAudits(_testAudits);

            _viewModel.SearchText = "IT";

            _viewModel.FilteredAudits.Count.ShouldBe(2);
            _viewModel.FilteredAudits.ShouldAllBe(a => a.AuditName.StartsWith("IT", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void SearchText_WhenSetToExactName_ShouldReturnSingleResult()
        {
            _viewModel.LoadAudits(_testAudits);

            _viewModel.SearchText = "Financial Review";

            _viewModel.FilteredAudits.Count.ShouldBe(1);
            _viewModel.FilteredAudits[0].AuditName.ShouldBe("Financial Review");
        }

        [Fact]
        public void SearchText_WhenCleared_ShouldReturnAllAudits()
        {
            _viewModel.LoadAudits(_testAudits);
            _viewModel.SearchText = "IT";

            _viewModel.SearchText = string.Empty;

            _viewModel.FilteredAudits.Count.ShouldBe(4);
        }

        [Fact]
        public void SearchText_WhenNoMatch_ShouldReturnEmptyList()
        {
            _viewModel.LoadAudits(_testAudits);

            _viewModel.SearchText = "NonExistentAudit";

            _viewModel.FilteredAudits.ShouldBeEmpty();
        }

        [Fact]
        public void GetSearchSuggestions_WithPartialMatch_ShouldReturnMatchingAudits()
        {
            _viewModel.LoadAudits(_testAudits);

            var suggestions = _viewModel.GetSearchSuggestions("IT");

            suggestions.Count.ShouldBe(2);
            suggestions.ShouldAllBe(a => a.AuditName.StartsWith("IT", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void GetSearchSuggestions_WithEmptyString_ShouldReturnEmptyList()
        {
            _viewModel.LoadAudits(_testAudits);

            var suggestions = _viewModel.GetSearchSuggestions(string.Empty);

            suggestions.ShouldBeEmpty();
        }

        [Fact]
        public void GetSearchSuggestions_WithNoMatch_ShouldReturnEmptyList()
        {
            _viewModel.LoadAudits(_testAudits);

            var suggestions = _viewModel.GetSearchSuggestions("XYZ");

            suggestions.ShouldBeEmpty();
        }

        #endregion

        #region PropertyChanged

        [Fact]
        public void FilteredAudits_WhenChanged_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AuditViewModel.FilteredAudits))
                    propertyChangedRaised = true;
            };

            _viewModel.LoadAudits(_testAudits);

            propertyChangedRaised.ShouldBeTrue();
        }

        [Fact]
        public void SelectedStatus_WhenChanged_ShouldRaisePropertyChangedEvent()
        {
            _viewModel.LoadAudits(_testAudits);
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AuditViewModel.SelectedStatus))
                    propertyChangedRaised = true;
            };

            _viewModel.SelectedStatus = AuditStatus.Completed;

            propertyChangedRaised.ShouldBeTrue();
        }

        [Fact]
        public void SelectedType_WhenChanged_ShouldRaisePropertyChangedEvent()
        {
            _viewModel.LoadAudits(_testAudits);
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AuditViewModel.SelectedType))
                    propertyChangedRaised = true;
            };

            _viewModel.SelectedType = AuditType.Security;

            propertyChangedRaised.ShouldBeTrue();
        }

        #endregion
    }
}
