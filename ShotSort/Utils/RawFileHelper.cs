using System.IO;
using System.Linq;

namespace ShotSort.Utils
{
    public static class RawFileHelper
    {
        private static readonly string[] RawExtensions = { ".cr2", ".cr3", ".dng", ".nef", ".raf" };

        public static bool IsRawFile(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return RawExtensions.Contains(ext);
        }

        public static string? FindPairedRaw(string jpgPath)
        {
            var dir = Path.GetDirectoryName(jpgPath);
            if (dir == null) return null;

            var nameWithoutExt = Path.GetFileNameWithoutExtension(jpgPath);

            foreach (var rawExt in RawExtensions)
            {
                var candidate = Path.Combine(dir, nameWithoutExt + rawExt);
                if (File.Exists(candidate))
                    return candidate;
            }

            return null;
        }
    }
}
