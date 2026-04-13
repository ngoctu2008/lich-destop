using System;

namespace LichDeBan.Models
{
    public class NoteModel
    {
        public string Content { get; set; } = string.Empty;

        // 0: Không lặp, 1: Lặp hàng năm Dương Lịch, 2: Lặp hàng năm Âm Lịch
        public int RepeatType { get; set; } = 0;

        // Dùng cho lặp hàng năm (Lưu Month-Day: ví dụ "04-13")
        public string RepeatKey { get; set; } = string.Empty;
    }
}
