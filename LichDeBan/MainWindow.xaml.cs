using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using LichDeBan.Helpers;

namespace LichDeBan
{
    public partial class MainWindow : Window
    {
        private bool _isLocked = true;
        private IntPtr _windowHandle;

        public MainWindow()
        {
            InitializeComponent();

            // Thiết lập vị trí mặc định ở góc trên bên phải (1/3 màn hình)
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            this.Width = screenWidth / 3;
            this.Height = screenHeight / 2;
            this.Left = screenWidth - this.Width - 20; // Cách lề phải 20px
            this.Top = 20; // Cách lề trên 20px
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _windowHandle = new WindowInteropHelper(this).Handle;

            if (_isLocked)
            {
                ApplyLockState();
            }

            // Đăng ký sự kiện nhấp đúp chuột từ MouseHook
            MouseHook.OnMouseDoubleClick += MouseHook_OnMouseDoubleClick;

            // Đăng ký nhận sự thay đổi từ Settings
            Services.SettingsManager.CurrentSettings.PropertyChanged += CurrentSettings_PropertyChanged;
            ApplySettings();
        }

        private void CurrentSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplySettings();
        }

        private void ApplySettings()
        {
            var settings = Services.SettingsManager.CurrentSettings;
            this.Opacity = settings.Opacity;

            if (_isLocked) // Chỉ áp dụng Background khi bị khóa (để trong suốt). Khi unlock sẽ có màu báo hiệu
            {
                try
                {
                    MainBorder.Background = new System.Windows.Media.BrushConverter().ConvertFromString(settings.BackgroundColor) as System.Windows.Media.Brush;
                }
                catch
                {
                    MainBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(51, 255, 255, 255));
                }
            }
        }

        private void MouseHook_OnMouseDoubleClick(object? sender, System.Windows.Point p)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    Helpers.Logger.Log($"Mouse DblClick: X={p.X}, Y={p.Y}");

                    // Lấy tọa độ thật của cửa sổ từ Win32 API (vì WPF Left/Top có thể sai khi ở WorkerW)
                    if (Helpers.WindowBoundsHelper.GetWindowRect(_windowHandle, out Helpers.WindowBoundsHelper.RECT rect))
                    {
                        Helpers.Logger.Log($"Win32 Bounds: L={rect.Left}, T={rect.Top}, R={rect.Right}, B={rect.Bottom}");

                        // Kiểm tra chuột có nằm trong cửa sổ vật lý không
                        if (p.X >= rect.Left && p.X <= rect.Right && p.Y >= rect.Top && p.Y <= rect.Bottom)
                        {
                            // Tọa độ tương đối bằng pixel vật lý
                            double relativePhysicalX = p.X - rect.Left;
                            double relativePhysicalY = p.Y - rect.Top;

                            // Lấy DPI hiện tại
                            var dpiInfo = VisualTreeHelper.GetDpi(this);
                            double dpiScaleX = dpiInfo.DpiScaleX;
                            double dpiScaleY = dpiInfo.DpiScaleY;

                            // Chuyển sang DIPs
                            double relativeDipX = relativePhysicalX / dpiScaleX;
                            double relativeDipY = relativePhysicalY / dpiScaleY;

                            System.Windows.Point relativePoint = new System.Windows.Point(relativeDipX, relativeDipY);
                            Helpers.Logger.Log($"DIP Relative Point: {relativePoint.X}, {relativePoint.Y}");

                            // Tìm kiếm Control tại điểm tương đối này
                            object? foundModel = Helpers.HitTestHelper.FindDataContextCore(this, relativePoint);

                            if (foundModel is Models.DayCellModel cellModel)
                            {
                                Helpers.Logger.Log($"Hit success: {cellModel.Date}");
                                OpenNoteEditor(cellModel, p);
                            }
                            else
                            {
                                Helpers.Logger.Log("HitTest failed to find DayCellModel.");
                            }
                        }
                        else
                        {
                            Helpers.Logger.Log("Click outside Win32 bounds.");
                        }
                    }
                    else
                    {
                        Helpers.Logger.Log("GetWindowRect failed.");
                    }
                }
                catch (Exception ex)
                {
                    Helpers.Logger.Log($"Exception: {ex.Message}");
                }
            }));
        }

        private void OpenNoteEditor(Models.DayCellModel cellModel, System.Windows.Point screenPoint)
        {
            // Đảm bảo không mở nhiều cửa sổ cùng lúc
            foreach (Window win in System.Windows.Application.Current.Windows)
            {
                if (win is NoteEditorWindow) return;
            }

            // Lấy lại raw note để hiện lên edit box
            Helpers.LunarInfo currentLunarInfo = Helpers.LunarCalendarHelper.GetLunarInfo(cellModel.Date);
            var (rawNote, repeatType) = Services.StorageManager.GetRawNoteForDate(cellModel.Date, currentLunarInfo);

            var editor = new NoteEditorWindow(cellModel.Date, rawNote, repeatType);

            // Đặt vị trí cửa sổ editor gần con trỏ chuột
            editor.Left = screenPoint.X;
            editor.Top = screenPoint.Y;

            editor.ShowDialog();

            if (editor.IsSaved)
            {
                string newNote = editor.NoteContent;
                int newRepeatType = editor.RepeatType;

                Helpers.LunarInfo lunarInfo = Helpers.LunarCalendarHelper.GetLunarInfo(cellModel.Date);
                Services.StorageManager.SetNoteForDate(cellModel.Date, newNote, newRepeatType, lunarInfo);

                // Cập nhật lại UI text note tổng hợp
                cellModel.Notes = Services.StorageManager.GetNoteForDate(cellModel.Date, lunarInfo);
            }
        }

        /// <summary>
        /// Bật/tắt trạng thái khóa vị trí của cửa sổ
        /// </summary>
        public void ToggleLock()
        {
            _isLocked = !_isLocked;
            ApplyLockState();
        }

        private void ApplyLockState()
        {
            if (_isLocked)
            {
                // Gắn vào Desktop (WorkerW)
                DesktopHelper.PinToDesktop(_windowHandle);

                // Loại bỏ viền và khôi phục màu cấu hình
                MainBorder.BorderBrush = System.Windows.Media.Brushes.Transparent;
                ApplySettings();

                // Vô hiệu hóa khả năng thay đổi kích thước và kéo
                this.ResizeMode = ResizeMode.NoResize;
            }
            else
            {
                // Tách khỏi Desktop
                DesktopHelper.UnpinFromDesktop(_windowHandle);

                // Hiển thị viền để người dùng nhận biết đang ở chế độ chỉnh sửa
                MainBorder.BorderBrush = System.Windows.Media.Brushes.DarkGray;
                MainBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 0, 0, 0)); // Tối hơn một chút để dễ nhìn

                // Cho phép thay đổi kích thước
                this.ResizeMode = ResizeMode.CanResizeWithGrip;
            }
        }

        // Cho phép kéo cửa sổ khi ở chế độ không khóa
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (!_isLocked)
            {
                this.DragMove();
            }
        }

        private void PreviousMonth_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.PreviousMonth();
            }
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.NextMonth();
            }
        }
    }
}