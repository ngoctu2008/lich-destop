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

        // Dictionary chứa ghi chú: Key là chuỗi ngày "yyyy-MM-dd", Value là nội dung ghi chú
        private static Dictionary<string, string> _notesCache = new Dictionary<string, string>();

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
                    var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (data != null)
                    {
                        _notesCache = data;
                    }
                }
                catch (Exception)
                {
                    // Lỗi đọc/parse file, bỏ qua và dùng cache trống
                    _notesCache = new Dictionary<string, string>();
                }
            }
        }

        public static void SaveNotes()
        {
            try
            {
                string json = JsonSerializer.Serialize(_notesCache, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(DataFilePath, json);
            }
            catch (Exception)
            {
                // Bỏ qua lỗi ghi file
            }
        }

        public static string GetNoteForDate(DateTime date)
        {
            string key = date.ToString("yyyy-MM-dd");
            if (_notesCache.TryGetValue(key, out string? note))
            {
                return note;
            }
            return string.Empty;
        }

        public static void SetNoteForDate(DateTime date, string note)
        {
            string key = date.ToString("yyyy-MM-dd");
            if (string.IsNullOrWhiteSpace(note))
            {
                if (_notesCache.ContainsKey(key))
                {
                    _notesCache.Remove(key);
                    SaveNotes();
                }
            }
            else
            {
                _notesCache[key] = note.Trim();
                SaveNotes();
            }
        }
    }
}