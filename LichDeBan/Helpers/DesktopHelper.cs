using System;
using System.Runtime.InteropServices;

namespace LichDeBan.Helpers
{
    /// <summary>
    /// Lớp hỗ trợ các hàm API Win32 để đưa cửa sổ xuống dưới các biểu tượng desktop.
    /// </summary>
    public static class DesktopHelper
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, UIntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out UIntPtr lpdwResult);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string? windowTitle);

        /// <summary>
        /// Gắn cửa sổ WPF vào lớp WorkerW phía sau biểu tượng Desktop.
        /// </summary>
        /// <param name="windowHandle">Handle của cửa sổ WPF</param>
        public static void PinToDesktop(IntPtr windowHandle)
        {
            // Tìm cửa sổ Progman (Desktop)
            IntPtr progman = FindWindow("Progman", null);

            if (progman == IntPtr.Zero)
            {
                return;
            }

            // Gửi thông báo 0x052C để yêu cầu Progman tạo một cửa sổ WorkerW phía sau các biểu tượng desktop
            UIntPtr result;
            SendMessageTimeout(progman, 0x052C, new UIntPtr(0), IntPtr.Zero, 0x0000, 1000, out result);

            IntPtr workerW = IntPtr.Zero;

            // Liệt kê các cửa sổ để tìm cửa sổ WorkerW vừa được tạo
            EnumWindows((hWnd, lParam) =>
            {
                IntPtr p = FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", null);

                if (p != IntPtr.Zero)
                {
                    // Lấy cửa sổ anh em liền kề (WorkerW)
                    workerW = FindWindowEx(IntPtr.Zero, hWnd, "WorkerW", null);
                }

                return true;
            }, IntPtr.Zero);

            if (workerW != IntPtr.Zero)
            {
                // Đặt cửa sổ WPF làm con của WorkerW
                SetParent(windowHandle, workerW);
            }
        }

        /// <summary>
        /// Tách cửa sổ khỏi desktop (để mở khóa vị trí).
        /// </summary>
        /// <param name="windowHandle">Handle của cửa sổ WPF</param>
        public static void UnpinFromDesktop(IntPtr windowHandle)
        {
            // Đặt lại cha của cửa sổ là null (Desktop tiêu chuẩn)
            SetParent(windowHandle, IntPtr.Zero);
        }
    }
}
