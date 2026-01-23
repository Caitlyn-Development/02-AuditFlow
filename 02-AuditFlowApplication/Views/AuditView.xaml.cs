using _02_AuditFlowApplication.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace _02_AuditFlowApplication.Views
{
    public partial class AuditView : Window
    {
        private DateTime currentMonth;
        private Dictionary<DateTime, List<string>> auditEvents;

        public AuditView()
        {
            InitializeComponent();
            currentMonth = new DateTime(2026, 10, 1); // October 2026 as shown in wireframe
            InitializeAuditEvents();
            PopulateCalendar();

            // Wire up calendar navigation buttons
            PrevMonthButton.Click += PrevMonthButton_Click;
            NextMonthButton.Click += NextMonthButton_Click;
        }

        private void InitializeAuditEvents()
        {
            // Initialize sample audit events to match wireframe
            auditEvents = new Dictionary<DateTime, List<string>>
            {
                { new DateTime(2026, 10, 2), new List<string> { "Risk Compliance" } },
                { new DateTime(2026, 10, 13), new List<string> { "Supplier Assess" } },
                { new DateTime(2026, 10, 22), new List<string> { "IT Systems Audit" } }
            };
        }

        private void PopulateCalendar()
        {
            CalendarGrid.Children.Clear();

            // Update month/year display
            CalendarMonthYear.Text = currentMonth.ToString("MMMM yyyy");

            // Get first day of month
            DateTime firstDayOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);

            // Find what day of week the 1st falls on (Monday = 1, Sunday = 7)
            int startDayOfWeek = ((int)firstDayOfMonth.DayOfWeek == 0) ? 7 : (int)firstDayOfMonth.DayOfWeek;

            // Get days in current month
            int daysInMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);

            // Get days in previous month
            DateTime prevMonth = currentMonth.AddMonths(-1);
            int daysInPrevMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);

            int dayCounter = 1;
            int nextMonthDayCounter = 1;

            // Create 5 weeks (35 cells)
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

                // Determine if this cell is for previous month, current month, or next month
                if (i < startDayOfWeek - 1)
                {
                    // Previous month days
                    dayCell.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F8F8"));
                    int prevMonthDay = daysInPrevMonth - (startDayOfWeek - 2 - i);
                    dayNumber.Text = prevMonthDay.ToString();
                    dayNumber.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCCCCC"));
                }
                else if (dayCounter <= daysInMonth)
                {
                    // Current month days
                    dayCell.Background = Brushes.White;
                    dayNumber.Text = dayCounter.ToString();

                    // Check if this date has any audit events
                    DateTime currentDate = new DateTime(currentMonth.Year, currentMonth.Month, dayCounter);

                    if (auditEvents.ContainsKey(currentDate))
                    {
                        foreach (string eventName in auditEvents[currentDate])
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
                    }

                    dayCounter++;
                }
                else
                {
                    // Next month days
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
            currentMonth = currentMonth.AddMonths(-1);
            PopulateCalendar();
        }

        private void NextMonthButton_Click(object sender, RoutedEventArgs e)
        {
            currentMonth = currentMonth.AddMonths(1);
            PopulateCalendar();
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToDashboard(this);
        }

        private void AuditsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToAudits(this);
        }

        private void TasksButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToTasks(this);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Logout(this);
        }
    }
}
