using System;
using System.Windows;
using LichDeBan.Helpers;
using LichDeBan.Services;
using LichDeBan.ViewModels;

namespace LichDeBan
{
    public partial class QuickNoteWindow : Window
    {
        public QuickNoteWindow()
        {
            InitializeComponent();
            NoteDatePicker.SelectedDate = DateTime.Today;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (NoteDatePicker.SelectedDate.HasValue)
            {
                DateTime selectedDate = NoteDatePicker.SelectedDate.Value;
                string noteContent = NoteTextBox.Text;
                int repeatType = RepeatCombo.SelectedIndex;

                LunarInfo lunarInfo = LunarCalendarHelper.GetLunarInfo(selectedDate);

                // Lưu vào hệ thống
                StorageManager.SetNoteForDate(selectedDate, noteContent, repeatType, lunarInfo);

                // Cập nhật lại UI Main Window nếu đang hiển thị
                if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow && mainWindow.DataContext is MainViewModel vm)
                {
                    vm.GenerateCalendar(); // Tải lại lưới
                }
            }
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
