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
        }

        private void MouseHook_OnMouseDoubleClick(object? sender, System.Windows.Point p)
        {
            Dispatcher.Invoke(() =>
            {
                // Chuyển đổi từ Physical Pixels sang Device Independent Pixels (DIPs)
                var source = PresentationSource.FromVisual(this);
                if (source == null || source.CompositionTarget == null) return;

                System.Windows.Point dipPoint = source.CompositionTarget.TransformFromDevice.Transform(p);

                // Kiểm tra xem chuột có nằm trong cửa sổ không
                if (dipPoint.X >= this.Left && dipPoint.X <= this.Left + this.Width &&
                    dipPoint.Y >= this.Top && dipPoint.Y <= this.Top + this.Height)
                {
                    // Lấy vị trí tương đối so với cửa sổ
                    System.Windows.Point relativePoint = new System.Windows.Point(dipPoint.X - this.Left, dipPoint.Y - this.Top);

                    // Thử tìm phần tử UI tại vị trí đó
                    HitTestResult hitResult = VisualTreeHelper.HitTest(this, relativePoint);
                    if (hitResult != null)
                    {
                        // Tìm đối tượng DayCellModel thông qua DataContext của phần tử được nhấp
                        DependencyObject current = hitResult.VisualHit;
                        while (current != null && !(current is FrameworkElement fe && fe.DataContext is Models.DayCellModel))
                        {
                            current = VisualTreeHelper.GetParent(current);
                        }

                        if (current is FrameworkElement element && element.DataContext is Models.DayCellModel cellModel)
                        {
                            OpenNoteEditor(cellModel, p);
                        }
                    }
                }
            });
        }

        private void OpenNoteEditor(Models.DayCellModel cellModel, System.Windows.Point screenPoint)
        {
            // Đảm bảo không mở nhiều cửa sổ cùng lúc
            foreach (Window win in System.Windows.Application.Current.Windows)
            {
                if (win is NoteEditorWindow) return;
            }

            var editor = new NoteEditorWindow(cellModel.Date, cellModel.Notes);

            // Đặt vị trí cửa sổ editor gần con trỏ chuột
            editor.Left = screenPoint.X;
            editor.Top = screenPoint.Y;

            editor.ShowDialog();

            if (editor.IsSaved)
            {
                string newNote = editor.NoteContent;
                cellModel.Notes = newNote;
                Services.StorageManager.SetNoteForDate(cellModel.Date, newNote);
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

                // Loại bỏ viền và làm trong suốt hoàn toàn
                MainBorder.BorderBrush = System.Windows.Media.Brushes.Transparent;
                MainBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(51, 255, 255, 255)); // 20% White

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