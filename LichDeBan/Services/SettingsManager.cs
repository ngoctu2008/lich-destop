using System;
using System.IO;
using System.Text.Json;
using LichDeBan.Models;

namespace LichDeBan.Services
{
    public static class SettingsManager
    {
        private static readonly string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LichDeBan");
        private static readonly string SettingsFilePath = Path.Combine(AppDataFolder, "settings.json");

        public static AppSettings CurrentSettings { get; private set; } = new AppSettings();

        static SettingsManager()
        {
            if (!Directory.Exists(AppDataFolder))
            {
                Directory.CreateDirectory(AppDataFolder);
            }
            LoadSettings();
        }

        public static void LoadSettings()
        {
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        CurrentSettings = settings;
                    }
                }
                catch
                {
                    // Lỗi đọc, dùng default
                    CurrentSettings = new AppSettings();
                }
            }
        }

        public static void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(CurrentSettings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch
            {
                // Bỏ qua
            }
        }
    }
}
