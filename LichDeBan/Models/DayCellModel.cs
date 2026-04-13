using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LichDeBan.Models
{
    public class DayCellModel : INotifyPropertyChanged
    {
        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        private int _solarDay;
        public int SolarDay
        {
            get => _solarDay;
            set { _solarDay = value; OnPropertyChanged(); }
        }

        private string _lunarDayText = string.Empty;
        public string LunarDayText
        {
            get => _lunarDayText;
            set { _lunarDayText = value; OnPropertyChanged(); }
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        private bool _isToday;
        public bool IsToday
        {
            get => _isToday;
            set { _isToday = value; OnPropertyChanged(); }
        }

        private bool _isCurrentMonth;
        public bool IsCurrentMonth
        {
            get => _isCurrentMonth;
            set { _isCurrentMonth = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}