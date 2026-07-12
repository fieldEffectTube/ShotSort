using System;
using System.IO;
using ShotSort.Utils;

namespace ShotSort.Core
{
    public static class FileSyncManager
    {
        public const string FolderHasPerson = "有人";
        public const string FolderSelected = "精选";
        public const string FolderKept = "保留";
        public const string FolderPendingDelete = "待删除";

        public static void EnsureClassifyFolders(string rootDir)
        {
            var folders = new[] { FolderHasPerson, FolderSelected, FolderKept, FolderPendingDelete };
            foreach (var name in folders)
            {
                var path = Path.Combine(rootDir, name);
                if (!Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                        DebugLogger.Log($"FileSyncManager: 创建分类目录 {name}");
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        DebugLogger.Log($"FileSyncManager: 无权限创建目录 {path} - {ex.Message}");
                    }
                }
            }
        }

        public static bool MoveToClassify(string sourceJpg, string targetFolder, string rootDir)
        {
            var targetDir = Path.Combine(rootDir, targetFolder);
            if (!Directory.Exists(targetDir))
            {
                try { Directory.CreateDirectory(targetDir); }
                catch (Exception ex)
                {
                    DebugLogger.LogError($"FileSyncManager: 无法创建目录 {targetDir}", ex);
                    return false;
                }
            }

            if (!MoveFile(sourceJpg, targetDir))
            {
                DebugLogger.Log($"FileSyncManager: 移动 JPG 失败，跳过 RAW {Path.GetFileName(sourceJpg)}");
                return false;
            }

            var pairedRaw = FindPairedRawSafe(sourceJpg);
            if (pairedRaw != null)
                MoveFile(pairedRaw, targetDir);

            DebugLogger.Log($"FileSyncManager: {Path.GetFileName(sourceJpg)} → {targetFolder}");
            return true;
        }

        public static bool DeletePhoto(string sourceJpg, bool deleteRaw)
        {
            var jpgDeleted = SafeDeleteFile(sourceJpg);
            if (!jpgDeleted)
            {
                DebugLogger.Log($"FileSyncManager: 删除 JPG 失败 {Path.GetFileName(sourceJpg)}");
                return false;
            }

            DebugLogger.Log($"FileSyncManager: 已删除 {Path.GetFileName(sourceJpg)}");

            if (deleteRaw)
            {
                var pairedRaw = FindPairedRawSafe(sourceJpg);
                if (pairedRaw != null)
                {
                    var rawDeleted = SafeDeleteFile(pairedRaw);
                    if (rawDeleted)
                        DebugLogger.Log($"FileSyncManager: 已删除配对 RAW {Path.GetFileName(pairedRaw)}");
                }
            }
            return true;
        }

        public static void MoveToPendingDelete(string sourceJpg, string rootDir)
        {
            MoveToClassify(sourceJpg, FolderPendingDelete, rootDir);
        }

        private static bool MoveFile(string source, string targetDir)
        {
            if (!File.Exists(source))
                return false;

            try
            {
                var fileName = Path.GetFileName(source);
                var destPath = Path.Combine(targetDir, fileName);

                // 文件已在目标目录（同一路径），无需移动
                if (string.Equals(source, destPath, StringComparison.OrdinalIgnoreCase))
                    return true;

                // 目标已存在同名文件：若内容相同（同长度同修改时间）视为重复，跳过移动并删除源文件
                if (File.Exists(destPath))
                {
                    var srcInfo = new FileInfo(source);
                    var dstInfo = new FileInfo(destPath);
                    if (srcInfo.Length == dstInfo.Length && srcInfo.LastWriteTimeUtc == dstInfo.LastWriteTimeUtc)
                    {
                        File.Delete(source);
                        return true;
                    }

                    // 真正的不同文件同名，追加 UUID 避免丢失
                    destPath = Path.Combine(targetDir,
                        $"{Path.GetFileNameWithoutExtension(source)}_{Guid.NewGuid():N}{Path.GetExtension(source)}");
                }

                File.Move(source, destPath);
                return true;
            }
            catch (IOException ex)
            {
                DebugLogger.Log($"FileSyncManager: 文件被占用，跳过移动 {Path.GetFileName(source)} - {ex.Message}");
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                DebugLogger.Log($"FileSyncManager: 无权限移动 {Path.GetFileName(source)} - {ex.Message}");
                return false;
            }
        }

        private static bool SafeDeleteFile(string path)
        {
            if (!File.Exists(path))
                return false;

            try
            {
                File.Delete(path);
                return true;
            }
            catch (IOException ex)
            {
                DebugLogger.Log($"FileSyncManager: 文件被占用，跳过删除 {Path.GetFileName(path)} - {ex.Message}");
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                DebugLogger.Log($"FileSyncManager: 无权限删除 {Path.GetFileName(path)} - {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 安全查找配对 RAW 文件，查找失败时返回 null（降级为仅操作 JPG）
        /// </summary>
        private static string? FindPairedRawSafe(string jpgPath)
        {
            try
            {
                return RawFileHelper.FindPairedRaw(jpgPath);
            }
            catch
            {
                return null;
            }
        }
    }
}
