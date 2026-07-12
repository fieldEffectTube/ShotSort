using System;
using System.IO;
using System.Reflection;
using ShotSort.Utils;

namespace ShotSort.Core
{
    public static class ViewFaceCoreInitializer
    {
        private static bool _initialized;

        public static void EnsureInitialized()
        {
            if (_initialized) return;

            DebugLogger.Log("ViewFaceCoreInitializer: 开始初始化 ViewFaceCore 运行环境...");

            // 计算模型路径：exe所在目录/viewfacecore/models
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;
            var modelPath = Path.Combine(exeDir, "viewfacecore", "models");
            var libraryPath = Path.Combine(exeDir, "viewfacecore", "win", "x64");

            DebugLogger.Log($"ViewFaceCoreInitializer: exeDir={exeDir}");
            DebugLogger.Log($"ViewFaceCoreInitializer: modelPath={modelPath} (exists={Directory.Exists(modelPath)})");
            DebugLogger.Log($"ViewFaceCoreInitializer: libraryPath={libraryPath} (exists={Directory.Exists(libraryPath)})");

            // 列出模型文件
            if (Directory.Exists(modelPath))
            {
                var files = Directory.GetFiles(modelPath);
                DebugLogger.Log($"ViewFaceCoreInitializer: 模型文件数={files.Length}");
                foreach (var f in files)
                {
                    DebugLogger.Log($"  model: {Path.GetFileName(f)} ({new FileInfo(f).Length} bytes)");
                }
            }

            // 列出 native DLL
            if (Directory.Exists(libraryPath))
            {
                var files = Directory.GetFiles(libraryPath);
                DebugLogger.Log($"ViewFaceCoreInitializer: native DLL数={files.Length}");
                foreach (var f in files)
                {
                    if (!f.EndsWith(".bak"))
                        DebugLogger.Log($"  dll: {Path.GetFileName(f)} ({new FileInfo(f).Length} bytes)");
                }
            }

            // 尝试通过反射提前设置模型路径
            try
            {
                var nativeType = Type.GetType("ViewFaceCore.Native.ViewFaceNative, ViewFaceCore");
                if (nativeType != null)
                {
                    DebugLogger.Log($"ViewFaceCoreInitializer: 找到 ViewFaceNative 类型");

                    // 检查是否有 SetModelPathWindows 方法
                    var setMethod = nativeType.GetMethod("SetModelPathWindows",
                        BindingFlags.Public | BindingFlags.Static);
                    DebugLogger.Log($"ViewFaceCoreInitializer: SetModelPathWindows 方法={setMethod}");

                    if (setMethod != null)
                    {
                        DebugLogger.Log($"ViewFaceCoreInitializer: 调用 SetModelPathWindows(\"{modelPath}\")...");
                        setMethod.Invoke(null, new object[] { modelPath });
                        DebugLogger.Log("ViewFaceCoreInitializer: SetModelPathWindows 调用成功");
                    }
                }
                else
                {
                    DebugLogger.Log("ViewFaceCoreInitializer: 未找到 ViewFaceNative 类型");
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogError("ViewFaceCoreInitializer: 设置模型路径异常", ex);
            }

            _initialized = true;
            DebugLogger.Log("ViewFaceCoreInitializer: 初始化完成");
        }
    }
}
