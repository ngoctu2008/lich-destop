using System.Collections.Generic;

namespace LichDeBan.Helpers
{
    public static class HolidayHelper
    {
        // Ngày lễ Dương lịch (MM-DD)
        private static readonly Dictionary<string, string> SolarHolidays = new Dictionary<string, string>
        {
            { "01-01", "Tết Dương lịch" },
            { "02-14", "Lễ tình nhân (Valentine)" },
            { "03-08", "Quốc tế Phụ nữ" },
            { "04-30", "Ngày Giải phóng miền Nam" },
            { "05-01", "Ngày Quốc tế Lao động" },
            { "06-01", "Ngày Quốc tế Thiếu nhi" },
            { "09-02", "Quốc khánh" },
            { "10-20", "Ngày Phụ nữ Việt Nam" },
            { "11-20", "Ngày Nhà giáo Việt Nam" },
            { "12-22", "Ngày thành lập QĐND Việt Nam" },
            { "12-24", "Lễ Giáng Sinh" },
        };

        // Ngày lễ Âm lịch (MM-DD)
        private static readonly Dictionary<string, string> LunarHolidays = new Dictionary<string, string>
        {
            { "01-01", "Tết Nguyên Đán" },
            { "01-02", "Mùng 2 Tết" },
            { "01-03", "Mùng 3 Tết" },
            { "01-15", "Tết Nguyên Tiêu" },
            { "03-10", "Giỗ Tổ Hùng Vương" },
            { "04-15", "Lễ Phật Đản" },
            { "05-05", "Tết Đoan Ngọ" },
            { "07-15", "Lễ Vu Lan" },
            { "08-15", "Tết Trung Thu" },
            { "12-23", "Ông Công Ông Táo" }
        };

        public static string GetHoliday(int solarDay, int solarMonth, int lunarDay, int lunarMonth)
        {
            string solarKey = $"{solarMonth:D2}-{solarDay:D2}";
            string lunarKey = $"{lunarMonth:D2}-{lunarDay:D2}";

            string result = "";

            if (LunarHolidays.TryGetValue(lunarKey, out string? lunarHol))
            {
                result = lunarHol;
            }

            if (SolarHolidays.TryGetValue(solarKey, out string? solarHol))
            {
                if (!string.IsNullOrEmpty(result)) result += "\n";
                result += solarHol;
            }

            return result;
        }
    }
}
