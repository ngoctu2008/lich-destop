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
            // Sử dụng BeginInvoke thay vì Invoke để không block luồng hook (tránh Windows hủy Hook)
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    Helpers.Logger.Log($"Mouse double click detected at physical screen: {p.X}, {p.Y}");

                    // Thử phương pháp sử dụng PresentationSource thay vì PointFromScreen do khi cửa sổ ở dưới WorkerW,
                    // một số hàm WPF có thể bị ngắt quãng.
                    var source = PresentationSource.FromVisual(this);
                    if (source == null || source.CompositionTarget == null)
                    {
                        Helpers.Logger.Log("PresentationSource is null.");
                        return;
                    }

                    System.Windows.Point dipPoint = source.CompositionTarget.TransformFromDevice.Transform(p);
                    Helpers.Logger.Log($"DIP Point: {dipPoint.X}, {dipPoint.Y}");
                    Helpers.Logger.Log($"Window Bounds: Left={this.Left}, Top={this.Top}, Width={this.Width}, Height={this.Height}");

                    // Kiểm tra xem chuột có nằm trong cửa sổ không
                    if (dipPoint.X >= this.Left && dipPoint.X <= this.Left + this.Width &&
                        dipPoint.Y >= this.Top && dipPoint.Y <= this.Top + this.Height)
                    {
                        // Lấy vị trí tương đối so với cửa sổ
                        System.Windows.Point relativePoint = new System.Windows.Point(dipPoint.X - this.Left, dipPoint.Y - this.Top);
                        Helpers.Logger.Log($"Relative Point: {relativePoint.X}, {relativePoint.Y}");

                        // Thực hiện HitTest chuyên sâu đi xuyên qua các lớp trong suốt
                        object? foundModel = Helpers.HitTestHelper.FindDataContextCore(this, relativePoint);

                        if (foundModel is Models.DayCellModel cellModel)
                        {
                            Helpers.Logger.Log($"Cell Model matched: {cellModel.Date}");
                            OpenNoteEditor(cellModel, p);
                        }
                        else
                        {
                            Helpers.Logger.Log("No DayCellModel DataContext found or hit test failed.");
                        }
                    }
                    else
                    {
                        Helpers.Logger.Log("Click is outside the window bounds.");
                    }
                }
                catch (Exception ex)
                {
                    Helpers.Logger.Log($"Exception in MouseHook: {ex.Message}");
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