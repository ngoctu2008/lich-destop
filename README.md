# Lịch Để Bàn (Vietnamese Lunar Desktop Calendar)

Đây là ứng dụng Lịch để bàn dành cho Windows 10/11 được viết bằng C# WPF (.NET 8).
Ứng dụng tự động gắn chặt vào màn hình nền (phía sau các biểu tượng Desktop), hiển thị lịch Dương và Âm lịch Việt Nam chuẩn xác, đồng thời hỗ trợ ghi chú lặp lại và tùy biến giao diện.

## Tính năng chính
- Gắn chặt vào lớp `WorkerW` của Desktop (không bị đè bởi các ứng dụng khác).
- Hiển thị ngày Dương lịch và Âm lịch Việt Nam (bao gồm Can Chi, Tháng Nhuận).
- Đánh dấu ngày cuối tuần (Thứ 7, Chủ Nhật) và các ngày Lễ Tết lớn của Việt Nam.
- Ghi chú nhắc việc: Double-click vào ngày bất kỳ để tạo ghi chú (hỗ trợ không lặp, lặp theo Dương lịch hoặc Âm lịch).
- Quản lý qua khay hệ thống (System Tray):
  - Khóa/Mở khóa vị trí để kéo thả thay đổi kích thước.
  - Thêm ghi chú nhanh cho ngày bất kỳ.
  - Tùy chỉnh (Đổi màu nền, độ trong suốt, Khởi động cùng Windows).

## Hướng dẫn Đóng gói (Publishing) Ứng dụng

Vì ứng dụng sử dụng .NET 8, bạn có thể đóng gói toàn bộ ứng dụng thành 1 file duy nhất (`.exe`) không cần người dùng cài đặt thêm .NET Runtime.

### Cách 1: Đóng gói thành 1 file duy nhất (Self-Contained)
Người dùng tải về chỉ việc chạy, **không cần** cài đặt .NET 8 trên máy tính của họ.

Mở Terminal (Command Prompt hoặc PowerShell) tại thư mục chứa file `LichDeBan.csproj` và chạy lệnh:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

- `-c Release`: Chế độ tối ưu hóa hiệu năng cao nhất.
- `-r win-x64`: Biên dịch riêng cho kiến trúc Windows 64-bit.
- `--self-contained true`: Bao gồm luôn cả .NET 8 runtime vào bên trong.
- `-p:PublishSingleFile=true`: Nén mọi DLL và tài nguyên vào 1 file `.exe` duy nhất.

**Thư mục đầu ra:** `LichDeBan/bin/Release/net8.0-windows/win-x64/publish/`

### Cách 2: Đóng gói siêu nhẹ (Framework-Dependent)
Cách này sẽ tạo ra 1 file exe cực kỳ nhỏ (chỉ vài MB), nhưng yêu cầu máy tính người dùng **phải cài sẵn** `.NET 8 Desktop Runtime`.

```bash
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

## Yêu cầu hệ thống
- Windows 10 hoặc Windows 11 (yêu cầu hỗ trợ Win32 API WorkerW và WH_MOUSE_LL).
- Nếu chạy file nguồn, cần cài đặt [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
