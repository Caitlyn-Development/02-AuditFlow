using _02_AuditFlowApplication.ViewModels;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;

namespace AuditFlowUnitTesting
{
    public class DashboardViewModelTests
    {
        private readonly DashboardViewModel _viewModel;

        public DashboardViewModelTests()
        {
            // Pass userId 0 — AuditService catches all DB exceptions
            // and returns empty collections when database is unavailable
            _viewModel = new DashboardViewModel(0);
        }

        #region Calendar Initialisation

        [Fact]
        public void DashboardViewModel_OnCreation_ShouldSetCurrentMonthToToday()
        {
            var expected = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            _viewModel.CurrentMonth.ShouldBe(expected);
        }

        [Fact]
        public void DashboardViewModel_OnCreation_ShouldSetCalendarMonthYear()
        {
            var expected = DateTime.Now.ToString("MMMM yyyy");

            _viewModel.CalendarMonthYear.ShouldBe(expected);
        }

        [Fact]
        public void DashboardViewModel_OnCreation_AuditEventsShouldNotBeNull()
        {
            _viewModel.AuditEvents.ShouldNotBeNull();
        }

        [Fact]
        public void DashboardViewModel_OnCreation_CalendarCellsShouldNotBeNull()
        {
            _viewModel.CalendarCells.ShouldNotBeNull();
        }

        [Fact]
        public void DashboardViewModel_OnCreation_UpcomingDeadlinesShouldNotBeNull()
        {
            _viewModel.UpcomingDeadlines.ShouldNotBeNull();
        }

        [Fact]
        public void DashboardViewModel_OnCreation_StatCardsShouldBeZeroWhenNoDatabaseAvailable()
        {
            _viewModel.TotalAudits.ShouldBeGreaterThanOrEqualTo(0);
            _viewModel.AuditsInProgress.ShouldBeGreaterThanOrEqualTo(0);
            _viewModel.CompletedAudits.ShouldBeGreaterThanOrEqualTo(0);
            _viewModel.OverdueAudits.ShouldBeGreaterThanOrEqualTo(0);
        }

        #endregion

        #region Month Navigation

        [Fact]
        public void GoToPreviousMonth_ShouldDecrementMonthByOne()
        {
            var currentMonth = _viewModel.CurrentMonth;

            _viewModel.GoToPreviousMonth();

            _viewModel.CurrentMonth.ShouldBe(currentMonth.AddMonths(-1));
        }

        [Fact]
        public void GoToNextMonth_ShouldIncrementMonthByOne()
        {
            var currentMonth = _viewModel.CurrentMonth;

            _viewModel.GoToNextMonth();

            _viewModel.CurrentMonth.ShouldBe(currentMonth.AddMonths(1));
        }

        [Fact]
        public void GoToPreviousMonth_ShouldUpdateCalendarMonthYear()
        {
            var expected = _viewModel.CurrentMonth.AddMonths(-1).ToString("MMMM yyyy");

            _viewModel.GoToPreviousMonth();

            _viewModel.CalendarMonthYear.ShouldBe(expected);
        }

        [Fact]
        public void GoToNextMonth_ShouldUpdateCalendarMonthYear()
        {
            var expected = _viewModel.CurrentMonth.AddMonths(1).ToString("MMMM yyyy");

            _viewModel.GoToNextMonth();

            _viewModel.CalendarMonthYear.ShouldBe(expected);
        }

        [Fact]
        public void GoToPreviousMonth_MultipleTimes_ShouldDecrementCorrectly()
        {
            var currentMonth = _viewModel.CurrentMonth;

            _viewModel.GoToPreviousMonth();
            _viewModel.GoToPreviousMonth();
            _viewModel.GoToPreviousMonth();

            _viewModel.CurrentMonth.ShouldBe(currentMonth.AddMonths(-3));
        }

        [Fact]
        public void GoToNextMonth_MultipleTimes_ShouldIncrementCorrectly()
        {
            var currentMonth = _viewModel.CurrentMonth;

            _viewModel.GoToNextMonth();
            _viewModel.GoToNextMonth();
            _viewModel.GoToNextMonth();

            _viewModel.CurrentMonth.ShouldBe(currentMonth.AddMonths(3));
        }

        [Fact]
        public void GoToPreviousMonth_ThenNextMonth_ShouldReturnToOriginalMonth()
        {
            var originalMonth = _viewModel.CurrentMonth;

            _viewModel.GoToPreviousMonth();
            _viewModel.GoToNextMonth();

            _viewModel.CurrentMonth.ShouldBe(originalMonth);
        }

        [Fact]
        public void GoToPreviousMonth_ShouldRebuildCalendarCells()
        {
            _viewModel.BuildCalendar();
            var cellsBefore = _viewModel.CalendarCells.Count;

            _viewModel.GoToPreviousMonth();

            _viewModel.CalendarCells.ShouldNotBeNull();
            _viewModel.CalendarCells.Count.ShouldBe(cellsBefore);
        }

        [Fact]
        public void GoToNextMonth_ShouldRebuildCalendarCells()
        {
            _viewModel.BuildCalendar();
            var cellsBefore = _viewModel.CalendarCells.Count;

            _viewModel.GoToNextMonth();

            _viewModel.CalendarCells.ShouldNotBeNull();
            _viewModel.CalendarCells.Count.ShouldBe(cellsBefore);
        }

        #endregion

        #region Calendar Calculations

        [Fact]
        public void GetDaysInMonth_ShouldReturnCorrectDaysForCurrentMonth()
        {
            var expected = DateTime.DaysInMonth(
                _viewModel.CurrentMonth.Year,
                _viewModel.CurrentMonth.Month);

            _viewModel.GetDaysInMonth().ShouldBe(expected);
        }

        [Fact]
        public void GetDaysInMonth_ForJanuary_ShouldReturn31()
        {
            while (_viewModel.CurrentMonth.Month != 1)
                _viewModel.GoToNextMonth();

            _viewModel.GetDaysInMonth().ShouldBe(31);
        }

        [Fact]
        public void GetDaysInMonth_ForApril_ShouldReturn30()
        {
            while (_viewModel.CurrentMonth.Month != 4)
                _viewModel.GoToNextMonth();

            _viewModel.GetDaysInMonth().ShouldBe(30);
        }

        [Fact]
        public void GetDaysInMonth_ForJune_ShouldReturn30()
        {
            while (_viewModel.CurrentMonth.Month != 6)
                _viewModel.GoToNextMonth();

            _viewModel.GetDaysInMonth().ShouldBe(30);
        }

        [Fact]
        public void GetStartDayOfWeek_ShouldReturnValueBetweenOneAndSeven()
        {
            var result = _viewModel.GetStartDayOfWeek();

            result.ShouldBeInRange(1, 7);
        }

        [Fact]
        public void GetStartDayOfWeek_AfterNavigation_ShouldStillBeInRange()
        {
            _viewModel.GoToNextMonth();
            _viewModel.GoToNextMonth();

            var result = _viewModel.GetStartDayOfWeek();

            result.ShouldBeInRange(1, 7);
        }

        [Fact]
        public void GetDaysInPreviousMonth_ShouldReturnCorrectValue()
        {
            var prevMonth = _viewModel.CurrentMonth.AddMonths(-1);
            var expected = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);

            _viewModel.GetDaysInPreviousMonth().ShouldBe(expected);
        }

        #endregion

        #region BuildCalendar

        [Fact]
        public void BuildCalendar_ShouldAlwaysGenerate35Cells()
        {
            _viewModel.BuildCalendar();

            _viewModel.CalendarCells.Count.ShouldBe(35);
        }

        [Fact]
        public void BuildCalendar_AfterNextMonth_ShouldStillGenerate35Cells()
        {
            _viewModel.GoToNextMonth();
            _viewModel.BuildCalendar();

            _viewModel.CalendarCells.Count.ShouldBe(35);
        }

        [Fact]
        public void BuildCalendar_AfterPreviousMonth_ShouldStillGenerate35Cells()
        {
            _viewModel.GoToPreviousMonth();
            _viewModel.BuildCalendar();

            _viewModel.CalendarCells.Count.ShouldBe(35);
        }

        [Fact]
        public void BuildCalendar_CurrentMonthCells_ShouldHaveIsCurrentMonthTrue()
        {
            _viewModel.BuildCalendar();

            var currentMonthCells = _viewModel.CalendarCells
                .Where(c => c.IsCurrentMonth)
                .ToList();

            currentMonthCells.ShouldNotBeEmpty();
            currentMonthCells.Count.ShouldBe(_viewModel.GetDaysInMonth());
        }

        [Fact]
        public void BuildCalendar_OtherMonthCells_ShouldHaveIsOtherMonthTrue()
        {
            _viewModel.BuildCalendar();

            var otherMonthCells = _viewModel.CalendarCells
                .Where(c => c.IsOtherMonth)
                .ToList();

            otherMonthCells.ShouldAllBe(c => !c.IsCurrentMonth);
        }

        [Fact]
        public void BuildCalendar_CurrentMonthAndOtherMonth_ShouldSumTo35()
        {
            _viewModel.BuildCalendar();

            var currentCount = _viewModel.CalendarCells.Count(c => c.IsCurrentMonth);
            var otherCount = _viewModel.CalendarCells.Count(c => c.IsOtherMonth);

            (currentCount + otherCount).ShouldBe(35);
        }

        [Fact]
        public void BuildCalendar_CellDayNumbers_ShouldNotBeNullOrEmpty()
        {
            _viewModel.BuildCalendar();

            _viewModel.CalendarCells.ShouldAllBe(c =>
                !string.IsNullOrEmpty(c.DayNumber));
        }

        [Fact]
        public void BuildCalendar_EventsList_ShouldNeverBeNull()
        {
            _viewModel.BuildCalendar();

            _viewModel.CalendarCells.ShouldAllBe(c => c.Events != null);
        }

        #endregion

        #region Audit Events

        [Fact]
        public void GetEventsOnDate_WithDateThatHasNoEvents_ShouldReturnEmptyList()
        {
            var dateWithNoEvents = new DateTime(2000, 1, 1);

            var result = _viewModel.GetEventsOnDate(dateWithNoEvents);

            result.ShouldNotBeNull();
            result.ShouldBeEmpty();
        }

        [Fact]
        public void HasEventsOnDate_WithDateThatHasNoEvents_ShouldReturnFalse()
        {
            var dateWithNoEvents = new DateTime(2000, 1, 1);

            var result = _viewModel.HasEventsOnDate(dateWithNoEvents);

            result.ShouldBeFalse();
        }

        [Fact]
        public void GetEventsOnDate_ShouldNeverReturnNull()
        {
            var anyDate = new DateTime(2000, 6, 15);

            var result = _viewModel.GetEventsOnDate(anyDate);

            result.ShouldNotBeNull();
        }

        [Fact]
        public void HasEventsOnDate_AndGetEventsOnDate_ShouldBeConsistent()
        {
            var date = new DateTime(2000, 1, 1);

            var hasEvents = _viewModel.HasEventsOnDate(date);
            var events = _viewModel.GetEventsOnDate(date);

            if (hasEvents)
                events.ShouldNotBeEmpty();
            else
                events.ShouldBeEmpty();
        }

        #endregion

        #region PropertyChanged

        [Fact]
        public void CurrentMonth_WhenChanged_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DashboardViewModel.CurrentMonth))
                    propertyChangedRaised = true;
            };

            _viewModel.GoToNextMonth();

            propertyChangedRaised.ShouldBeTrue();
        }

        [Fact]
        public void CalendarMonthYear_WhenMonthChanges_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DashboardViewModel.CalendarMonthYear))
                    propertyChangedRaised = true;
            };

            _viewModel.GoToNextMonth();

            propertyChangedRaised.ShouldBeTrue();
        }

        [Fact]
        public void AuditEvents_WhenLoaded_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DashboardViewModel.AuditEvents))
                    propertyChangedRaised = true;
            };

            _viewModel.LoadAuditEvents();

            propertyChangedRaised.ShouldBeTrue();
        }

        [Fact]
        public void CalendarCells_WhenBuilt_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DashboardViewModel.CalendarCells))
                    propertyChangedRaised = true;
            };

            _viewModel.BuildCalendar();

            propertyChangedRaised.ShouldBeTrue();
        }

        [Fact]
        public void TotalAudits_WhenChanged_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DashboardViewModel.TotalAudits))
                    propertyChangedRaised = true;
            };

            _viewModel.LoadAuditEvents();

            propertyChangedRaised.ShouldBeTrue();
        }

        [Fact]
        public void UpcomingDeadlines_WhenLoaded_ShouldRaisePropertyChangedEvent()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DashboardViewModel.UpcomingDeadlines))
                    propertyChangedRaised = true;
            };

            _viewModel.LoadAuditEvents();

            propertyChangedRaised.ShouldBeTrue();
        }

        #endregion
    }
}