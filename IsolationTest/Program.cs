using System;
using System.IO;
using ViewFaceCore.Core;

var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ShotSort_isolation_test.txt");

void Log(string msg)
{
    var line = $"[{DateTime.Now:HH:mm:ss.fff}] {msg}";
    Console.WriteLine(line);
    File.AppendAllText(logPath, line + Environment.NewLine);
}

try
{
    Log("=== ISOLATION TEST START ===");

    var exeDir = AppDomain.CurrentDomain.BaseDirectory;
    Log($"exeDir={exeDir}");

    // Check files
    var modelPath = Path.Combine(exeDir, "viewfacecore", "models");
    Log($"modelPath exists? {Directory.Exists(modelPath)}");
    if (Directory.Exists(modelPath))
        foreach (var f in Directory.GetFiles(modelPath))
            Log($"  model: {Path.GetFileName(f)} ({new FileInfo(f).Length} bytes)");

    var libPath = Path.Combine(exeDir, "viewfacecore", "win", "x64");
    Log($"libPath exists? {Directory.Exists(libPath)}");
    if (Directory.Exists(libPath))
        foreach (var f in Directory.GetFiles(libPath))
            if (!f.EndsWith(".bak"))
                Log($"  dll: {Path.GetFileName(f)} ({new FileInfo(f).Length} bytes)");

    // The critical test
    Log("--- Creating FaceDetector ---");
    using var detector = new FaceDetector();
    Log($"FaceDetector created! ModelPath={detector.ModelPath}, LibraryPath={detector.LibraryPath}");

    Log("=== ISOLATION TEST SUCCESS ===");
}
catch (Exception ex)
{
    Log($"=== EXCEPTION: {ex.GetType().Name}: {ex.Message} ===");
    Log($"Stack: {ex.StackTrace}");
    if (ex.InnerException != null)
        Log($"Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
}
