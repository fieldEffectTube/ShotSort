using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using SkiaSharp;
using ViewFaceCore;
using ViewFaceCore.Configs;
using ViewFaceCore.Configs.Enums;
using ViewFaceCore.Core;
using ViewFaceCore.Models;
using ShotSort.Models;
using ShotSort.Utils;

namespace ShotSort.Core
{
    public class ViewFaceDetector : IDisposable
    {
        private const int MaxDetectWidth = 1920;
        private const int MaxDetectHeight = 1920;

        private FaceDetector? _faceDetector;
        private FaceLandmarker? _faceLandmarker68;
        private FaceLandmarker? _faceLandmarker5;
        private EyeStateDetector? _eyeStateDetector;
        private bool _disposed;
        private bool _initialized;
        private bool _eyeStateDetectorAvailable;
        private static bool _hasDumpedLandmarks;

        public bool IsInitialized => _initialized;
        public string? InitError { get; private set; }

        public ViewFaceDetector()
        {
            DebugLogger.Log("ViewFaceDetector: 开始初始化 AI 模型...");

            GlobalConfig.SetX86Instruction(X86Instruction.SSE2);
            DebugLogger.Log("ViewFaceDetector: 已设置 X86Instruction=SSE2");

            TryInitialize();
        }

        private void TryInitialize()
        {
            try
            {
                var faceDetectConfig = new FaceDetectConfig
                {
                    DeviceType = DeviceType.CPU,
                    FaceSize = 80,
                    Threshold = 0.7,
                };

                _faceDetector = new FaceDetector(faceDetectConfig);
                DebugLogger.Log("ViewFaceDetector: FaceDetector 创建成功");
            }
            catch (Exception ex)
            {
                InitError = $"FaceDetector 初始化失败: {ex.Message}";
                DebugLogger.LogError("ViewFaceDetector: FaceDetector 初始化失败", ex);
                return;
            }

            try
            {
                _faceLandmarker68 = new FaceLandmarker(new FaceLandmarkConfig(MarkType.Normal));
                DebugLogger.Log("ViewFaceDetector: FaceLandmarker68 (68点) 创建成功");
            }
            catch (Exception ex)
            {
                InitError = $"FaceLandmarker68 初始化失败: {ex.Message}";
                DebugLogger.LogError("ViewFaceDetector: FaceLandmarker68 初始化失败", ex);
                return;
            }

            try
            {
                _faceLandmarker5 = new FaceLandmarker(new FaceLandmarkConfig(MarkType.Light));
                DebugLogger.Log("ViewFaceDetector: FaceLandmarker5 (5点) 创建成功");
            }
            catch (Exception ex)
            {
                DebugLogger.LogError("ViewFaceDetector: FaceLandmarker5 初始化失败（非致命）", ex);
            }

            try
            {
                _eyeStateDetector = new EyeStateDetector();
                _eyeStateDetectorAvailable = true;
                DebugLogger.Log("ViewFaceDetector: EyeStateDetector 创建成功");
            }
            catch (Exception ex)
            {
                _eyeStateDetectorAvailable = false;
                DebugLogger.LogError("ViewFaceDetector: EyeStateDetector 初始化失败（非致命）", ex);
            }

            _initialized = true;
            DebugLogger.Log("ViewFaceDetector: 所有 AI 模型初始化完成");
        }

        private SKBitmap? DecodeAndScale(string imagePath)
        {
            using var codec = SKCodec.Create(imagePath);
            if (codec == null) return null;

            var info = codec.Info;
            if (info.Width == 0 || info.Height == 0) return null;

            var targetColorType = SKColorType.Bgra8888;
            var alphaType = info.IsOpaque ? SKAlphaType.Opaque : SKAlphaType.Premul;

            int targetWidth = info.Width;
            int targetHeight = info.Height;

            if (info.Width > MaxDetectWidth || info.Height > MaxDetectHeight)
            {
                var ratio = Math.Min((double)MaxDetectWidth / info.Width, (double)MaxDetectHeight / info.Height);
                targetWidth = (int)(info.Width * ratio);
                targetHeight = (int)(info.Height * ratio);
            }

            var targetInfo = new SKImageInfo(targetWidth, targetHeight, targetColorType, alphaType);
            var bitmap = new SKBitmap(targetInfo);
            var result = codec.GetPixels(targetInfo, bitmap.GetPixels());
            if (result != SKCodecResult.Success)
            {
                bitmap.Dispose();
                var fallback = SKBitmap.Decode(imagePath);
                if (fallback == null) return null;

                if (fallback.Width <= MaxDetectWidth && fallback.Height <= MaxDetectHeight)
                    return fallback;

                var scaled = ScaleBitmap(fallback);
                fallback.Dispose();
                return scaled;
            }

            return bitmap;
        }

        private static SKBitmap ScaleBitmap(SKBitmap source)
        {
            var ratio = Math.Min((double)MaxDetectWidth / source.Width, (double)MaxDetectHeight / source.Height);
            var targetWidth = (int)(source.Width * ratio);
            var targetHeight = (int)(source.Height * ratio);
            var targetInfo = new SKImageInfo(targetWidth, targetHeight, SKColorType.Bgra8888, source.AlphaType);

            var scaled = new SKBitmap(targetInfo);
            using var canvas = new SKCanvas(scaled);
            var paint = new SKPaint { FilterQuality = SKFilterQuality.Medium, IsAntialias = false };
            canvas.DrawBitmap(source, new SKRect(0, 0, targetWidth, targetHeight), paint);
            return scaled;
        }

        public ClassifyResult Detect(string imagePath, bool fullAnalysis = true)
        {
            if (!_initialized) return CreateEmptyResult();

            try
            {
                using var skBitmap = DecodeAndScale(imagePath);
                if (skBitmap == null)
                {
                    DebugLogger.Log($"Detect: DecodeAndScale 返回 null - {Path.GetFileName(imagePath)}");
                    return CreateEmptyResult();
                }

                return DetectFromSkBitmap(skBitmap, fullAnalysis);
            }
            catch (Exception ex)
            {
                DebugLogger.LogError($"Detect: 异常 - {imagePath}", ex);
                return CreateEmptyResult();
            }
        }

        private ClassifyResult DetectFromSkBitmap(SKBitmap skBitmap, bool fullAnalysis = true)
        {
            var result = new ClassifyResult
            {
                HasFace = false,
                EyeState = Models.EyeState.Open,
                ClarityScore = 0,
                IsBlur = false
            };

            try
            {
                using var faceImage = skBitmap.ToFaceImage();
                var faces = _faceDetector!.Detect(faceImage);
                DebugLogger.Log($"Detect: FaceDetect 返回 {faces?.Length ?? 0} 张人脸, fullAnalysis={fullAnalysis}");

                if (faces == null || faces.Length == 0)
                    return result;

                result.HasFace = true;

                if (!fullAnalysis)
                    return result;

                int closedEyeCount = 0;

                for (int faceIdx = 0; faceIdx < faces.Length; faceIdx++)
                {
                    var face = faces[faceIdx];

                    // 获取 68 点关键点
                    var markPoints68 = _faceLandmarker68!.Mark(faceImage, face);
                    DebugLogger.Log($"  人脸#{faceIdx + 1} 位置=({face.Location.X},{face.Location.Y}) {face.Location.Width}x{face.Location.Height} pts68={markPoints68?.Length ?? 0}");

                    if (markPoints68 == null || markPoints68.Length < 48)
                    {
                        DebugLogger.Log($"  人脸#{faceIdx + 1} 68点不足，跳过");
                        continue;
                    }

                    // 首次 dump 关键点
                    if (!_hasDumpedLandmarks)
                    {
                        DumpLandmarkPoints(markPoints68, faceIdx);
                        _hasDumpedLandmarks = true;
                    }

                    // === 方法1: EAR (68点几何计算) ===
                    double leftEar = CalcEar(markPoints68, 36, 37, 38, 39, 40, 41);
                    double rightEar = CalcEar(markPoints68, 42, 43, 44, 45, 46, 47);
                    DebugLogger.Log($"  人脸#{faceIdx + 1} EAR: 左={leftEar:F3} 右={rightEar:F3}");

                    // === 方法2: EyeStateDetector (native) 使用 68 点 ===
                    EyeStateResult? eyeResult68 = null;
                    if (_eyeStateDetectorAvailable)
                    {
                        try
                        {
                            eyeResult68 = _eyeStateDetector!.Detect(faceImage, markPoints68);
                            DebugLogger.Log($"  人脸#{faceIdx + 1} EyeStateDetector(68点): 左={eyeResult68.LeftEyeState}({(int)eyeResult68.LeftEyeState}) 右={eyeResult68.RightEyeState}({(int)eyeResult68.RightEyeState})");
                        }
                        catch (Exception ex)
                        {
                            DebugLogger.LogError($"  人脸#{faceIdx + 1} EyeStateDetector(68点) 异常", ex);
                        }
                    }

                    // === 方法3: EyeStateDetector (native) 使用 5 点 ===
                    EyeStateResult? eyeResult5 = null;
                    if (_eyeStateDetectorAvailable && _faceLandmarker5 != null)
                    {
                        try
                        {
                            var markPoints5 = _faceLandmarker5.Mark(faceImage, face);
                            DebugLogger.Log($"  人脸#{faceIdx + 1} pts5={markPoints5?.Length ?? 0}");
                            if (markPoints5 != null && markPoints5.Length > 0)
                            {
                                eyeResult5 = _eyeStateDetector!.Detect(faceImage, markPoints5);
                                DebugLogger.Log($"  人脸#{faceIdx + 1} EyeStateDetector(5点): 左={eyeResult5.LeftEyeState}({(int)eyeResult5.LeftEyeState}) 右={eyeResult5.RightEyeState}({(int)eyeResult5.RightEyeState})");
                            }
                        }
                        catch (Exception ex)
                        {
                            DebugLogger.LogError($"  人脸#{faceIdx + 1} EyeStateDetector(5点) 异常", ex);
                        }
                    }

                    // 综合判断：优先使用 EyeStateDetector(5点) > EyeStateDetector(68点) > EAR
                    bool isClosed = false;
                    string method = "";

                    if (eyeResult5 != null && eyeResult5.LeftEyeState != ViewFaceCore.Models.EyeState.Random && eyeResult5.RightEyeState != ViewFaceCore.Models.EyeState.Random)
                    {
                        bool l5 = eyeResult5.LeftEyeState == ViewFaceCore.Models.EyeState.Close;
                        bool r5 = eyeResult5.RightEyeState == ViewFaceCore.Models.EyeState.Close;
                        isClosed = l5 || r5;
                        method = $"EyeState5点(左={eyeResult5.LeftEyeState},右={eyeResult5.RightEyeState})";
                    }
                    else if (eyeResult68 != null && eyeResult68.LeftEyeState != ViewFaceCore.Models.EyeState.Random && eyeResult68.RightEyeState != ViewFaceCore.Models.EyeState.Random)
                    {
                        bool l68 = eyeResult68.LeftEyeState == ViewFaceCore.Models.EyeState.Close;
                        bool r68 = eyeResult68.RightEyeState == ViewFaceCore.Models.EyeState.Close;
                        isClosed = l68 || r68;
                        method = $"EyeState68点(左={eyeResult68.LeftEyeState},右={eyeResult68.RightEyeState})";
                    }
                    else
                    {
                        // EAR fallback
                        const double earThreshold = 0.19;
                        isClosed = leftEar < earThreshold || rightEar < earThreshold;
                        method = $"EAR(左={leftEar:F3},右={rightEar:F3},阈值={earThreshold})";
                    }

                    if (isClosed) closedEyeCount++;
                    DebugLogger.Log($"  人脸#{faceIdx + 1} 判定: {(isClosed ? "闭眼" : "睁眼")} 方法={method}");
                }

                if (closedEyeCount >= 2)
                    result.EyeState = Models.EyeState.MultiClosed;
                else if (closedEyeCount == 1)
                    result.EyeState = Models.EyeState.BothClosed;

                DebugLogger.Log($"Detect: {faces.Length}张人脸中 {closedEyeCount} 人闭眼, 最终EyeState={result.EyeState}");
            }
            catch (Exception ex)
            {
                DebugLogger.LogError("DetectFromSkBitmap 异常", ex);
            }

            return result;
        }

        private void DumpLandmarkPoints(FaceMarkPoint[] points, int faceIdx)
        {
            DebugLogger.Log($"=== 人脸#{faceIdx} 68点坐标 dump ===");
            // 只 dump 眼部关键点 (36-47) 和前几个点
            for (int i = 0; i < Math.Min(20, points.Length); i++)
                DebugLogger.Log($"  点[{i}] = ({points[i].X:F1}, {points[i].Y:F1})");
            DebugLogger.Log("  ... (省略中间点) ...");
            for (int i = 36; i < Math.Min(48, points.Length); i++)
                DebugLogger.Log($"  点[{i}] = ({points[i].X:F1}, {points[i].Y:F1})");
            DebugLogger.Log("=== dump 结束 ===");
        }

        private static double CalcEar(FaceMarkPoint[] pts, int i1, int i2, int i3, int i4, int i5, int i6)
        {
            double vertical1 = Distance(pts[i2], pts[i6]);
            double vertical2 = Distance(pts[i3], pts[i5]);
            double horizontal = Distance(pts[i1], pts[i4]);
            if (horizontal < 0.001) return 1.0;
            return (vertical1 + vertical2) / (2.0 * horizontal);
        }

        private static double Distance(FaceMarkPoint a, FaceMarkPoint b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public List<ClassifyResult> BatchDetect(List<string> imagePaths, IProgress<(int completed, string fileName)>? progress = null,
            CancellationToken cancellationToken = default, bool fullAnalysis = true)
        {
            if (!_initialized)
            {
                DebugLogger.Log("BatchDetect: 模型未初始化，返回空结果");
                return new List<ClassifyResult>();
            }

            DebugLogger.Log($"BatchDetect: 开始批量检测，共 {imagePaths.Count} 张图片 (fullAnalysis={fullAnalysis})");
            _hasDumpedLandmarks = false;
            var results = new List<ClassifyResult>(imagePaths.Count);

            var decodeQueue = new BlockingCollection<(int index, SKBitmap? bitmap)>(2);

            var producer = new Thread(() =>
            {
                try
                {
                    for (int i = 0; i < imagePaths.Count; i++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        try
                        {
                            var bitmap = DecodeAndScale(imagePaths[i]);
                            decodeQueue.Add((i, bitmap));
                        }
                        catch (Exception ex)
                        {
                            DebugLogger.LogError($"BatchDecode: 第 {i + 1} 张解码异常", ex);
                            decodeQueue.Add((i, null));
                        }
                    }
                }
                finally
                {
                    decodeQueue.CompleteAdding();
                }
            })
            { IsBackground = true };

            producer.Start();

            int completed = 0;
            foreach (var (index, bitmap) in decodeQueue.GetConsumingEnumerable(cancellationToken))
            {
                if (bitmap == null)
                {
                    results.Add(CreateEmptyResult());
                }
                else
                {
                    using (bitmap)
                    {
                        try
                        {
                            var result = DetectFromSkBitmap(bitmap, fullAnalysis);
                            results.Add(result);
                        }
                        catch (Exception ex)
                        {
                            DebugLogger.LogError($"BatchDetect: 第 {index + 1} 张检测异常", ex);
                            results.Add(CreateEmptyResult());
                        }
                    }
                }

                completed++;
                var fileName = Path.GetFileName(imagePaths[index]);
                progress?.Report((completed, fileName));
            }

            producer.Join();

            DebugLogger.Log($"BatchDetect: 完成，共处理 {results.Count} 张");
            return results;
        }

        private static ClassifyResult CreateEmptyResult() => new()
        {
            HasFace = false,
            EyeState = Models.EyeState.Open,
            ClarityScore = 0,
            IsBlur = false
        };

        public void Dispose()
        {
            if (!_disposed)
            {
                _faceDetector?.Dispose();
                _faceLandmarker68?.Dispose();
                _faceLandmarker5?.Dispose();
                _eyeStateDetector?.Dispose();
                _disposed = true;
            }
        }
    }
}
