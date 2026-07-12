using System;
using System.IO;
using System.Text;

namespace ShotSort.Utils
{
    public static class DebugLogger
    {
        private static readonly string LogPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "ShotSort_debug.txt");

        private static readonly object LockObj = new();

        public static void Log(string message)
        {
            try
            {
                lock (LockObj)
                {
                    File.AppendAllText(LogPath, $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n");
                }
            }
            catch { }
        }

        public static void LogError(string message, Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[{DateTime.Now:HH:mm:ss.fff}] ERROR: {message}");
            sb.AppendLine($"  Exception: {ex.GetType().Name}: {ex.Message}");
            sb.AppendLine($"  StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                sb.AppendLine($"  InnerException: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                sb.AppendLine($"  InnerStackTrace: {ex.InnerException.StackTrace}");
            }
            try
            {
                lock (LockObj)
                {
                    File.AppendAllText(LogPath, sb.ToString());
                }
            }
            catch { }
        }

        public static string GetLogPath() => LogPath;
    }
}
