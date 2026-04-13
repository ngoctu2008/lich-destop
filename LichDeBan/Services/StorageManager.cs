using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LichDeBan.Services
{
    public static class StorageManager
    {
        private static readonly string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LichDeBan");
        private static readonly string DataFilePath = Path.Combine(AppDataFolder, "notes.json");

        // Dictionary chứa ghi chú đơn lẻ: Key là chuỗi ngày "yyyy-MM-dd", Value là nội dung ghi chú
        private static Dictionary<string, string> _notesCache = new Dictionary<string, string>();

        // Ghi chú lặp Dương Lịch: Key là "MM-dd"
        private static Dictionary<string, string> _solarRepeatNotes = new Dictionary<string, string>();

        // Ghi chú lặp Âm Lịch: Key là "MM-dd"
        private static Dictionary<string, string> _lunarRepeatNotes = new Dictionary<string, string>();

        public class StorageData
        {
            public Dictionary<string, string> Notes { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> SolarRepeat { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> LunarRepeat { get; set; } = new Dictionary<string, string>();
        }

        static StorageManager()
        {
            if (!Directory.Exists(AppDataFolder))
            {
                Directory.CreateDirectory(AppDataFolder);
            }
            LoadNotes();
        }

        private static void LoadNotes()
        {
            if (File.Exists(DataFilePath))
            {
                try
                {
                    string json = File.ReadAllText(DataFilePath);

                    // Thử parse format mới có lặp lại
                    var data = JsonSerializer.Deserialize<StorageData>(json);
                    if (data != null && (data.Notes.Count > 0 || data.SolarRepeat.Count > 0 || data.LunarRepeat.Count > 0))
                    {
                        _notesCache = data.Notes;
                        _solarRepeatNotes = data.SolarRepeat;
                        _lunarRepeatNotes = data.LunarRepeat;
                        return;
                    }

                    // Parse fallback format cũ (chỉ có Dictionary)
                    var oldData = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (oldData != null)
                    {
                        _notesCache = oldData;
                    }
                }
                catch (Exception)
                {
                    _notesCache = new Dictionary<string, string>();
                }
            }
        }

        public static void SaveNotes()
        {
            try
            {
                var data = new StorageData
                {
                    Notes = _notesCache,
                    SolarRepeat = _solarRepeatNotes,
                    LunarRepeat = _lunarRepeatNotes
                };
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(DataFilePath, json);
            }
            catch (Exception)
            {
                // Bỏ qua lỗi ghi file
            }
        }

        public static string GetNoteForDate(DateTime date, Helpers.LunarInfo lunarInfo)
        {
            string result = "";
            string fullKey = date.ToString("yyyy-MM-dd");
            string solarKey = date.ToString("MM-dd");
            string lunarKey = $"{lunarInfo.Month:D2}-{lunarInfo.Day:D2}";

            if (_solarRepeatNotes.TryGetValue(solarKey, out string? sNote))
            {
                result += sNote;
            }

            if (_lunarRepeatNotes.TryGetValue(lunarKey, out string? lNote))
            {
                if (result.Length > 0) result += "\n";
                result += lNote;
            }

            if (_notesCache.TryGetValue(fullKey, out string? note))
            {
                if (result.Length > 0) result += "\n";
                result += note;
            }

            return result;
        }

        public static string GetRawNoteForDate(DateTime date)
        {
            string key = date.ToString("yyyy-MM-dd");
            return _notesCache.TryGetValue(key, out string? note) ? note : "";
        }

        public static void SetNoteForDate(DateTime date, string note, int repeatType, Helpers.LunarInfo lunarInfo)
        {
            string fullKey = date.ToString("yyyy-MM-dd");
            string solarKey = date.ToString("MM-dd");
            string lunarKey = $"{lunarInfo.Month:D2}-{lunarInfo.Day:D2}";

            // Xóa ghi chú cũ nếu tồn tại trong loại lặp này
            if (string.IsNullOrWhiteSpace(note))
            {
                if (repeatType == 0) _notesCache.Remove(fullKey);
                else if (repeatType == 1) _solarRepeatNotes.Remove(solarKey);
                else if (repeatType == 2) _lunarRepeatNotes.Remove(lunarKey);
            }
            else
            {
                if (repeatType == 0) _notesCache[fullKey] = note.Trim();
                else if (repeatType == 1) _solarRepeatNotes[solarKey] = note.Trim();
                else if (repeatType == 2) _lunarRepeatNotes[lunarKey] = note.Trim();
            }
            SaveNotes();
        }
    }
}