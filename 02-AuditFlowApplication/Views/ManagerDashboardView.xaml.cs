using _02_AuditFlowApplication.Models;
using _02_AuditFlowApplication.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace _02_AuditFlowApplication.Views
{
    public partial class ManagerDashboardView : UserControl
    {
        private readonly ManagerDashboardViewModel _viewModel;

        public ManagerDashboardView()
        {
            InitializeComponent();
            var currentUser = Application.Current.Properties["CurrentUser"] as User;
            _viewModel = new ManagerDashboardViewModel(currentUser?.UserID ?? 0);
            DataContext = _viewModel;

            Layout.SetActiveButton("Dashboard");

            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DashboardViewModel.CalendarCells))
                    PopulateCalendar();
                if (e.PropertyName == nameof(DashboardViewModel.UpcomingDeadlines))
                    PopulateUpcomingDeadlines();
            };

            PrevMonthButton.Click += (s, e) => _viewModel.GoToPreviousMonth();
            NextMonthButton.Click += (s, e) => _viewModel.GoToNextMonth();

            PopulateCalendar();
            PopulateUpcomingDeadlines();
        }

        private void PopulateCalendar()
        {
            CalendarGrid.Children.Clear();
            CalendarMonthYear.Text = _viewModel.CalendarMonthYear;

            int startDayOfWeek = _viewModel.GetStartDayOfWeek();
            int daysInMonth = _viewModel.GetDaysInMonth();
            int daysInPrevMonth = _viewModel.GetDaysInPreviousMonth();
            int dayCounter = 1;
            int nextMonthDayCounter = 1;

            for (int i = 0; i < 35; i++)
            {
                Border dayCell = new Border
                {
                    Margin = new Thickness(2),
                    Height = 110,
                    BorderBrush = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#E0E0E0")),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6)
                };

                StackPanel cellContent = new StackPanel
                {
                    Margin = new Thickness(6)
                };

                TextBlock dayNumber = new TextBlock
                {
                    FontSize = 13,
                    FontFamily = new FontFamily("Verdana"),
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 0, 0, 4)
                };

                if (i < startDayOfWeek - 1)
                {
                    dayCell.Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#F8F8F8"));
                    int prevMonthDay = daysInPrevMonth - (startDayOfWeek - 2 - i);
                    dayNumber.Text = prevMonthDay.ToString();
                    dayNumber.Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#CCCCCC"));
                }
                else if (dayCounter <= daysInMonth)
                {
                    dayCell.Background = Brushes.White;
                    dayNumber.Text = dayCounter.ToString();
                    dayNumber.Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#1E1E1E"));

                    DateTime currentDate = new DateTime(
                        _viewModel.CurrentMonth.Year,
                        _viewModel.CurrentMonth.Month,
                        dayCounter);

                    foreach (var eventName in _viewModel.GetEventsOnDate(currentDate))
                    {
                        Border eventBadge = new Border
                        {
                            Background = new SolidColorBrush(
                                (Color)ColorConverter.ConvertFromString("#F3EDF7")),
                            BorderBrush = new SolidColorBrush(
                                (Color)ColorConverter.ConvertFromString("#5D3754")),
                            BorderThickness = new Thickness(1),
                            CornerRadius = new CornerRadius(6),
                            Margin = new Thickness(0, 3, 0, 0),
                            Padding = new Thickness(6, 4, 6, 4),
                            HorizontalAlignment = HorizontalAlignment.Stretch
                        };

                        Grid badgeGrid = new Grid();
                        badgeGrid.ColumnDefinitions.Add(new ColumnDefinition
                        {
                            Width = new GridLength(3)
                        });
                        badgeGrid.ColumnDefinitions.Add(new ColumnDefinition
                        {
                            Width = new GridLength(1, GridUnitType.Star)
                        });

                        Border accentBar = new Border
                        {
                            Background = new SolidColorBrush(
                                (Color)ColorConverter.ConvertFromString("#5D3754")),
                            CornerRadius = new CornerRadius(3, 0, 0, 3),
                            Margin = new Thickness(0, 0, 6, 0)
                        };
                        Grid.SetColumn(accentBar, 0);
                        badgeGrid.Children.Add(accentBar);

                        TextBlock eventText = new TextBlock
                        {
                            Text = eventName,
                            FontFamily = new FontFamily("Verdana"),
                            FontSize = 13,
                            FontWeight = FontWeights.SemiBold,
                            Foreground = new SolidColorBrush(
                                (Color)ColorConverter.ConvertFromString("#5D3754")),
                            TextWrapping = TextWrapping.Wrap,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        Grid.SetColumn(eventText, 1);
                        badgeGrid.Children.Add(eventText);

                        eventBadge.Child = badgeGrid;
                        cellContent.Children.Add(eventBadge);
                    }

                    dayCounter++;
                }
                else
                {
                    dayCell.Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#F8F8F8"));
                    dayNumber.Text = nextMonthDayCounter.ToString();
                    dayNumber.Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#CCCCCC"));
                    nextMonthDayCounter++;
                }

                cellContent.Children.Insert(0, dayNumber);
                dayCell.Child = cellContent;
                CalendarGrid.Children.Add(dayCell);
            }
        }

        private void PopulateUpcomingDeadlines()
        {
            if (_viewModel.UpcomingDeadlines == null || !_viewModel.UpcomingDeadlines.Any())
            {
                UpcomingDeadlinesGrid.Visibility = Visibility.Collapsed;
                NoDeadlinesText.Visibility = Visibility.Visible;
            }
            else
            {
                UpcomingDeadlinesGrid.Visibility = Visibility.Visible;
                NoDeadlinesText.Visibility = Visibility.Collapsed;
                UpcomingDeadlinesGrid.ItemsSource = _viewModel.UpcomingDeadlines;
            }
        }

        private void PrevMonthButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.GoToPreviousMonth();
            PopulateCalendar();
        }

        private void NextMonthButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.GoToNextMonth();
            PopulateCalendar();
        }
    }
}