using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace LichDeBan.Helpers
{
    /// <summary>
    /// Lớp hỗ trợ bắt các sự kiện chuột cấp thấp (Low-Level Mouse Hook).
    /// </summary>
    public static class MouseHook
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;

        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        // Theo dõi click đúp
        private static uint _lastClickTime = 0;
        private static POINT _lastClickPos = new POINT { x = 0, y = 0 };

        [DllImport("user32.dll")]
        private static extern uint GetDoubleClickTime();

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        private const int SM_CXDOUBLECLK = 36;
        private const int SM_CYDOUBLECLK = 37;

        // Sự kiện được kích hoạt khi người dùng nhấp đúp chuột trái
        public static event EventHandler<System.Windows.Point>? OnMouseDoubleClick;

        /// <summary>
        /// Bắt đầu theo dõi sự kiện chuột.
        /// </summary>
        public static void Start()
        {
            _hookID = SetHook(_proc);
        }

        /// <summary>
        /// Dừng theo dõi sự kiện chuột.
        /// </summary>
        public static void Stop()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule!)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName!), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT))!;

                uint currentTime = hookStruct.time;
                POINT currentPos = hookStruct.pt;

                uint maxTime = GetDoubleClickTime();
                int maxDx = GetSystemMetrics(SM_CXDOUBLECLK) / 2;
                int maxDy = GetSystemMetrics(SM_CYDOUBLECLK) / 2;

                if (currentTime - _lastClickTime <= maxTime &&
                    Math.Abs(currentPos.x - _lastClickPos.x) <= maxDx &&
                    Math.Abs(currentPos.y - _lastClickPos.y) <= maxDy)
                {
                    OnMouseDoubleClick?.Invoke(null, new System.Windows.Point(currentPos.x, currentPos.y));
                    _lastClickTime = 0; // Đặt lại để tránh click lần 3 tính là double click
                }
                else
                {
                    _lastClickTime = currentTime;
                    _lastClickPos = currentPos;
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
