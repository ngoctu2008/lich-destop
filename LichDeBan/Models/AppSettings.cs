using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LichDeBan.Models
{
    public class AppSettings : INotifyPropertyChanged
    {
        private string _backgroundColor = "#33FFFFFF";
        public string BackgroundColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; OnPropertyChanged(); }
        }

        private double _opacity = 1.0;
        public double Opacity
        {
            get => _opacity;
            set { _opacity = value; OnPropertyChanged(); }
        }

        private bool _runAtStartup = false;
        public bool RunAtStartup
        {
            get => _runAtStartup;
            set { _runAtStartup = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
