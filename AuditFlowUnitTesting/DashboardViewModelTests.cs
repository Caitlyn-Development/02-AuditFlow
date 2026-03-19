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
            // ViewModel constructor catches all exceptions from AuditService
            // so it will safely initialise with empty events if DB unavailable
            _viewModel = new DashboardViewModel();
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
        public void GetDaysInMonth_ForMonthWith31Days_ShouldReturn31()
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

        #endregion
    }
}