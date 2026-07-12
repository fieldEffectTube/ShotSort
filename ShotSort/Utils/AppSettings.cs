using System;
using System.IO;
using System.Text.Json;

namespace ShotSort.Utils
{
    internal static class AppSettings
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ShotSort", "settings.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public static string LastFolderPath { get; set; } = "";

        public static void Load()
        {
            try
            {
                if (!File.Exists(SettingsPath)) return;
                var json = File.ReadAllText(SettingsPath);
                var data = JsonSerializer.Deserialize<SettingsData>(json);
                if (data?.LastFolderPath != null)
                    LastFolderPath = data.LastFolderPath;
            }
            catch
            {
                LastFolderPath = "";
            }
        }

        public static void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(SettingsPath)!;
                Directory.CreateDirectory(dir);
                var data = new SettingsData { LastFolderPath = LastFolderPath };
                var json = JsonSerializer.Serialize(data, JsonOptions);
                File.WriteAllText(SettingsPath, json);
            }
            catch
            {
                // Best-effort save
            }
        }

        public static string GetValidPathOrDrive()
        {
            var path = LastFolderPath;
            if (string.IsNullOrEmpty(path)) return "";

            if (Directory.Exists(path)) return path;

            // Path no longer exists - fall back to the drive letter
            try
            {
                var root = Path.GetPathRoot(path);
                if (!string.IsNullOrEmpty(root) && Directory.Exists(root))
                    return root;
            }
            catch
            {
            }

            return "";
        }

        private class SettingsData
        {
            public string? LastFolderPath { get; set; }
        }
    }
}
