using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ShotSort.Models;
using ShotSort.Utils;

namespace ShotSort.Core
{
    public static class FileScanner
    {
        private static readonly string[] JpgExtensions = { ".jpg", ".jpeg" };
        private static readonly string[] RawExtensions = { ".cr2", ".cr3", ".dng", ".nef", ".raf" };

        private static readonly HashSet<string> JpgSet = new(JpgExtensions, StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> RawSet = new(RawExtensions, StringComparer.OrdinalIgnoreCase);

        public static ScanResult ScanWithCategories(string directory, bool loadJpg, bool loadRaw)
        {
            if (!Directory.Exists(directory))
            {
                DebugLogger.Log($"FileScanner: 目录不存在 {directory}");
                return new ScanResult { Photos = new List<PhotoItem>(), JpgCount = 0, RawCount = 0 };
            }

            DebugLogger.Log($"FileScanner: 开始分类扫描 {directory} (JPG={loadJpg}, RAW={loadRaw})");

            var photos = new ConcurrentBag<PhotoItem>();
            var categoryFolders = new Dictionary<string, PhotoCategory>(StringComparer.OrdinalIgnoreCase)
            {
                [FileSyncManager.FolderHasPerson] = PhotoCategory.HasPerson,
                [FileSyncManager.FolderSelected] = PhotoCategory.Selected,
                [FileSyncManager.FolderKept] = PhotoCategory.Kept,
                [FileSyncManager.FolderPendingDelete] = PhotoCategory.PendingDelete
            };

            // 扫描根目录（未分类照片）
            var rootResult = Scan(directory, loadJpg, loadRaw);

            // 扫描子目录（分类照片，跳过待删除）
            foreach (var kvp in categoryFolders)
            {
                if (kvp.Value == PhotoCategory.PendingDelete) continue;

                var subDir = Path.Combine(directory, kvp.Key);
                if (!Directory.Exists(subDir)) continue;

                var subResult = Scan(subDir, loadJpg, loadRaw);
                foreach (var photo in subResult.Photos)
                {
                    photo.Category = kvp.Value;
                    photos.Add(photo);
                }
            }

            // 合并：根目录照片 + 分类子目录照片
            var allPhotos = rootResult.Photos.Concat(photos)
                .OrderBy(p => p.FileName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var jpgCount = allPhotos.Count(p => p.FileType == PhotoFileType.JPG);
            var rawCount = allPhotos.Count(p => p.FileType == PhotoFileType.RAW);

            DebugLogger.Log($"FileScanner: 分类扫描完成 - JPG={jpgCount}, RAW={rawCount}, 分类数={photos.Count}");

            return new ScanResult
            {
                Photos = allPhotos,
                JpgCount = jpgCount,
                RawCount = rawCount
            };
        }

        public static ScanResult Scan(string directory, bool loadJpg, bool loadRaw)
        {
            if (!Directory.Exists(directory))
            {
                DebugLogger.Log($"FileScanner: 目录不存在 {directory}");
                return new ScanResult { Photos = new List<PhotoItem>(), JpgCount = 0, RawCount = 0 };
            }

            DebugLogger.Log($"FileScanner: 开始扫描 {directory} (JPG={loadJpg}, RAW={loadRaw})");

            var files = Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly);
            DebugLogger.Log($"FileScanner: 目录中共 {files.Length} 个文件");

            var photos = new ConcurrentBag<PhotoItem>();

            // 收集 JPG 文件名（不含扩展名），用于快速判断 RAW 是否配对
            HashSet<string> jpgBaseNames = new(StringComparer.OrdinalIgnoreCase);
            if (loadJpg && loadRaw)
            {
                foreach (var f in files)
                {
                    var ext = Path.GetExtension(f);
                    if (JpgSet.Contains(ext))
                        jpgBaseNames.Add(Path.GetFileNameWithoutExtension(f));
                }
            }

            var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

            Parallel.ForEach(files, options, file =>
            {
                var ext = Path.GetExtension(file);

                if (loadJpg && JpgSet.Contains(ext))
                {
                    var item = new PhotoItem
                    {
                        FilePath = file,
                        FileName = Path.GetFileName(file),
                        Extension = ext.ToLowerInvariant(),
                        FileType = PhotoFileType.JPG,
                        PairedRawPath = RawFileHelper.FindPairedRaw(file),
                        AiResult = null
                    };
                    photos.Add(item);
                }
                else if (loadRaw && RawSet.Contains(ext))
                {
                    // 仅添加未与 JPG 配对的独立 RAW 文件
                    var baseName = Path.GetFileNameWithoutExtension(file);
                    if (!jpgBaseNames.Contains(baseName))
                    {
                        var item = new PhotoItem
                        {
                            FilePath = file,
                            FileName = Path.GetFileName(file),
                            Extension = ext.ToLowerInvariant(),
                            FileType = PhotoFileType.RAW,
                            PairedRawPath = null,
                            AiResult = null
                        };
                        photos.Add(item);
                    }
                }
            });

            var list = photos.OrderBy(p => p.FileName, StringComparer.OrdinalIgnoreCase).ToList();
            var jpgCount = list.Count(p => p.FileType == PhotoFileType.JPG);
            var rawCount = list.Count(p => p.FileType == PhotoFileType.RAW);

            DebugLogger.Log($"FileScanner: 扫描完成 - JPG={jpgCount}, RAW={rawCount}");

            return new ScanResult
            {
                Photos = list,
                JpgCount = jpgCount,
                RawCount = rawCount
            };
        }
    }

    public class ScanResult
    {
        public List<PhotoItem> Photos { get; set; } = new();
        public int JpgCount { get; set; }
        public int RawCount { get; set; }
    }
}
