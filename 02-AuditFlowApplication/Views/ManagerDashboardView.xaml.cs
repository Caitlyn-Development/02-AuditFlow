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
            _viewModel = new ManagerDashboardViewModel();
            DataContext = _viewModel;
            Layout.SetActiveButton("Dashboard");

            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ManagerDashboardViewModel.CalendarCells))
                    PopulateCalendar();
            };

            PrevMonthButton.Click += PrevMonthButton_Click;
            NextMonthButton.Click += NextMonthButton_Click;

            PopulateCalendar();

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
                    Height = 80,
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                    BorderThickness = new Thickness(1)
                };

                StackPanel cellContent = new StackPanel
                {
                    Margin = new Thickness(8)
                };

                TextBlock dayNumber = new TextBlock
                {
                    FontSize = 14,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"))
                };

                if (i < startDayOfWeek - 1)
                {
                    dayCell.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F8F8"));
                    int prevMonthDay = daysInPrevMonth - (startDayOfWeek - 2 - i);
                    dayNumber.Text = prevMonthDay.ToString();
                    dayNumber.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCCCCC"));
                }
                else if (dayCounter <= daysInMonth)
                {
                    dayCell.Background = Brushes.White;
                    dayNumber.Text = dayCounter.ToString();

                    DateTime currentDate = new DateTime(_viewModel.CurrentMonth.Year, _viewModel.CurrentMonth.Month, dayCounter);

                    foreach (var eventName in _viewModel.GetEventsOnDate(currentDate))
                    {
                        Border eventBadge = new Border
                        {
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7C4DFF")),
                            CornerRadius = new CornerRadius(10),
                            Margin = new Thickness(0, 4, 0, 0),
                            HorizontalAlignment = HorizontalAlignment.Left
                        };

                        TextBlock eventText = new TextBlock
                        {
                            Text = eventName,
                            FontSize = 9,
                            FontWeight = FontWeights.SemiBold,
                            Foreground = Brushes.White,
                            TextWrapping = TextWrapping.Wrap
                        };

                        eventBadge.Child = eventText;
                        cellContent.Children.Add(eventBadge);
                    }

                    dayCounter++;
                }
                else
                {
                    dayCell.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F8F8"));
                    dayNumber.Text = nextMonthDayCounter.ToString();
                    dayNumber.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCCCCC"));
                    nextMonthDayCounter++;
                }

                cellContent.Children.Insert(0, dayNumber);
                dayCell.Child = cellContent;
                CalendarGrid.Children.Add(dayCell);
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