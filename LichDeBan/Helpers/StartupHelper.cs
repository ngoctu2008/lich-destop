using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace LichDeBan.Helpers
{
    public static class StartupHelper
    {
        private const string AppName = "LichDeBan_VietnameseLunar";

        public static void SetStartup(bool enable)
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (key != null)
                    {
                        if (enable)
                        {
                            // Lấy đường dẫn chính xác của file exe hiện tại đang chạy
                            string? exePath = Process.GetCurrentProcess().MainModule?.FileName;
                            if (!string.IsNullOrEmpty(exePath))
                            {
                                key.SetValue(AppName, $"\"{exePath}\"");
                            }
                        }
                        else
                        {
                            key.DeleteValue(AppName, false);
                        }
                    }
                }
            }
            catch
            {
                // Bỏ qua nếu không có quyền ghi registry hoặc có lỗi xảy ra
            }
        }
    }
}
