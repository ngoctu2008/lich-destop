using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LichDeBan.Models;
using LichDeBan.Helpers;

namespace LichDeBan.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<DayCellModel> Days { get; set; } = new ObservableCollection<DayCellModel>();

        private DateTime _currentMonth;
        public DateTime CurrentMonth
        {
            get => _currentMonth;
            set
            {
                _currentMonth = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MonthYearText));
                GenerateCalendar();
            }
        }

        public string MonthYearText => $"Tháng {_currentMonth.Month} Năm {_currentMonth.Year}";

        public MainViewModel()
        {
            CurrentMonth = DateTime.Today;
        }

        public void GenerateCalendar()
        {
            Days.Clear();

            DateTime firstDayOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month);

            // Xác định ngày bắt đầu của tuần (Thứ Hai)
            int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            // Trong .NET, Chủ nhật là 0. Chuyển sang: Thứ Hai = 0, Chủ nhật = 6
            int offset = (startDayOfWeek == 0) ? 6 : startDayOfWeek - 1;

            DateTime startDate = firstDayOfMonth.AddDays(-offset);

            // Tổng số ô hiển thị (6 tuần = 42 ngày)
            for (int i = 0; i < 42; i++)
            {
                DateTime date = startDate.AddDays(i);

                // Lấy thông tin âm lịch
                LunarInfo lunarInfo = LunarCalendarHelper.GetLunarInfo(date);

                Days.Add(new DayCellModel
                {
                    Date = date,
                    SolarDay = date.Day,
                    LunarDayText = lunarInfo.ToString(),
                    Notes = Services.StorageManager.GetNoteForDate(date),
                    IsToday = date.Date == DateTime.Today,
                    IsCurrentMonth = date.Month == CurrentMonth.Month
                });
            }
        }

        public void NextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
        }

        public void PreviousMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}