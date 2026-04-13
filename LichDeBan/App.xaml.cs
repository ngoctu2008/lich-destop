using System;
using System.Drawing;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using LichDeBan.Helpers;

namespace LichDeBan
{
    public partial class App : System.Windows.Application
    {
        private TaskbarIcon? _notifyIcon;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Bắt đầu Hook chuột
            MouseHook.Start();

            // Khởi tạo Tray Icon từ Resources
            _notifyIcon = (TaskbarIcon)FindResource("MyNotifyIcon");

            // Icon mặc định (có thể dùng icon riêng nếu có)
            _notifyIcon.Icon = SystemIcons.Application;

            // Mở cửa sổ chính
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Dừng Hook chuột
            MouseHook.Stop();

            // Xóa Tray Icon
            _notifyIcon?.Dispose();
        }

        private void ToggleLock_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.ToggleLock();
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem đã mở chưa
            foreach (Window win in System.Windows.Application.Current.Windows)
            {
                if (win is SettingsWindow)
                {
                    win.Activate();
                    return;
                }
            }

            var settingsWindow = new SettingsWindow();
            settingsWindow.Show();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}