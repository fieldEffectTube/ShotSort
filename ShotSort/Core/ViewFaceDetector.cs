using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using SkiaSharp;
using ViewFaceCore;
using ViewFaceCore.Configs;
using ViewFaceCore.Configs.Enums;
using ViewFaceCore.Core;
using ViewFaceCore.Models;
using ShotSort.Models;
using ShotSort.Utils;
using VFEyeState = ViewFaceCore.Models.EyeState;

namespace ShotSort.Core
{
    public class ViewFaceDetector : IDisposable
    {
        [DllImport("ViewFaceBridge", EntryPoint = "Quality_PoseEx", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NativeQualityOfPoseEx(ref FaceImage img, FaceRect faceRect,
            FaceMarkPoint[] points, int pointsLength, ref int level, ref float score,
            float yawLow = 25, float yawHigh = 10, float pitchLow = 20, float pitchHigh = 10,
            float rollLow = 33.33f, float rollHigh = 16.67f);

        [DllImport("ViewFaceBridge", EntryPoint = "GetQualityOfClarityExHandler", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr NativeGetClarityExHandler(float blur_thresh = 0.8f, int deviceType = 0);

        [DllImport("ViewFaceBridge", EntryPoint = "Quality_ClarityEx", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NativeQualityOfClarityEx(IntPtr handler, ref FaceImage img,
            FaceRect faceRect, FaceMarkPoint[] points, int pointsLength, ref int level, ref float score);

        [DllImport("ViewFaceBridge", EntryPoint = "DisposeQualityOfClarityEx", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NativeDisposeClarityEx(IntPtr handler);
        private const int MaxDetectWidth = 1920;
        private const int MaxDetectHeight = 1920;
        private const float SideFaceYawThreshold = 25f;

        private FaceDetector? _faceDetector;
        private FaceDetector? _faceDetectorLowThreshold;
        private FaceLandmarker? _faceLandmarker68;
        private FaceLandmarker? _faceLandmarker5;
        private EyeStateDetector? _eyeStateDetector;
        private IntPtr _clarityExHandle;
        private bool _disposed;
        private bool _initialized;
        private bool _eyeStateDetectorAvailable;
        private bool _poseExAvailable;
        private bool _clarityExAvailable;

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
                DebugLogger.Log("ViewFaceDetector: FaceDetector 创建成功 (Threshold=0.7)");
            }
            catch (Exception ex)
            {
                InitError = $"FaceDetector 初始化失败: {ex.Message}";
                DebugLogger.LogError("ViewFaceDetector: FaceDetector 初始化失败", ex);
                return;
            }

            try
            {
                var lowThresholdConfig = new FaceDetectConfig
                {
                    DeviceType = DeviceType.CPU,
                    FaceSize = 40,
                    Threshold = 0.3,
                };

                _faceDetectorLowThreshold = new FaceDetector(lowThresholdConfig);
                DebugLogger.Log("ViewFaceDetector: FaceDetector(低阈值) 创建成功 (Threshold=0.3, FaceSize=40)");
            }
            catch (Exception ex)
            {
                _faceDetectorLowThreshold = null;
                DebugLogger.LogError("ViewFaceDetector: FaceDetector(低阈值) 初始化失败（非致命）", ex);
            }

            try
            {
                _faceLandmarker68 = new FaceLandmarker(new FaceLandmarkConfig(MarkType.Normal));
                DebugLogger.Log("ViewFaceDetector: FaceLandmarker68 创建成功");
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
                DebugLogger.Log("ViewFaceDetector: FaceLandmarker5 创建成功");
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

            // PoseEx 通过直接 P/Invoke 调用，不需要预初始化 handle
            _poseExAvailable = true;
            DebugLogger.Log("ViewFaceDetector: PoseEx (侧脸检测) 已就绪");

            try
            {
                _clarityExHandle = NativeGetClarityExHandler(0.8f, 0);
                _clarityExAvailable = _clarityExHandle != IntPtr.Zero;
                if (_clarityExAvailable)
                    DebugLogger.Log("ViewFaceDetector: ClarityEx (清晰度评估) 已就绪");
                else
                    DebugLogger.Log("ViewFaceDetector: ClarityEx handle 为零，不可用");
            }
            catch (Exception ex)
            {
                _clarityExAvailable = false;
                DebugLogger.LogError("ViewFaceDetector: ClarityEx 初始化失败（非致命）", ex);
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

                DebugLogger.Log($"Detect: 处理 {Path.GetFileName(imagePath)} (原始→bitmap={skBitmap.Width}x{skBitmap.Height})");
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
                FaceCount = 0,
                EyeState = Models.EyeState.Open,
                ClarityScore = 0,
                IsBlur = false,
                LowConfidenceFace = false,
                LowQualityFace = false
            };

            try
            {
                using var faceImage = skBitmap.ToFaceImage();
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var faces = _faceDetector!.Detect(faceImage);
                sw.Stop();
                bool highConfidence = faces != null && faces.Length > 0;
                DebugLogger.Log($"Detect: bitmap={skBitmap.Width}x{skBitmap.Height}, FaceDetect(阈值=0.7) 返回 {faces?.Length ?? 0} 张人脸, 耗时={sw.ElapsedMilliseconds}ms");

                if (faces != null && faces.Length > 0)
                {
                    for (int i = 0; i < faces.Length; i++)
                        DebugLogger.Log($"  face[{i}]: score={faces[i].Score:F3} pos=({faces[i].Location.X},{faces[i].Location.Y}) {faces[i].Location.Width}x{faces[i].Location.Height}");
                }

                if (!highConfidence)
                {
                    // 0.7 阈值未检出，使用 0.3 低阈值回退检测
                    DebugLogger.Log($"Detect: 0.7阈值未检出，尝试0.3低阈值回退检测...");
                    var fallbackFaces = DetectWithFallbackThreshold(faceImage);
                    if (fallbackFaces != null && fallbackFaces.Length > 0)
                    {
                        DebugLogger.Log($"Detect: 0.3阈值回退检测到 {fallbackFaces.Length} 张人脸（低置信度人像）！");
                        for (int i = 0; i < fallbackFaces.Length; i++)
                            DebugLogger.Log($"  fallback face[{i}]: score={fallbackFaces[i].Score:F3} pos=({fallbackFaces[i].Location.X},{fallbackFaces[i].Location.Y}) {fallbackFaces[i].Location.Width}x{fallbackFaces[i].Location.Height}");
                        faces = fallbackFaces;
                        result.LowConfidenceFace = true;
                    }
                    else
                    {
                        DebugLogger.Log($"Detect: 0.3阈值仍无人脸，图片不含人像 (bitmap={skBitmap.Width}x{skBitmap.Height})");
                        return result;
                    }
                }

                result.HasFace = true;
                result.FaceCount = faces!.Length;

                if (!fullAnalysis)
                    return result;

                // 低置信度人像：跳过闭眼/质量检测（画面模糊，检测结果不可靠）
                if (result.LowConfidenceFace)
                {
                    DebugLogger.Log($"Detect: 低置信度人像，跳过闭眼/质量检测");
                    return result;
                }

                // 多人照片（>3人）：跳过闭眼/质量检测（景深原因，远距离人像模糊属正常）
                if (faces.Length > 3)
                {
                    DebugLogger.Log($"Detect: {faces.Length}张人脸 → 多人场景，跳过闭眼/质量检测");
                    return result;
                }

                // === 1-3人照片：进行闭眼检测 + ClarityEx 质量评估 ===
                bool anyClosed = false;
                bool anyLowQuality = false;

                for (int faceIdx = 0; faceIdx < faces.Length; faceIdx++)
                {
                    var face = faces[faceIdx];
                    var markPoints68 = _faceLandmarker68!.Mark(faceImage, face);
                    if (markPoints68 == null || markPoints68.Length < 48)
                        continue;

                    // === ClarityEx 清晰度评估 ===
                    if (_clarityExAvailable)
                    {
                        try
                        {
                            var (clarityLevel, clarityScore) = EvaluateClarity(faceImage, face.Location, markPoints68);
                            bool isLowQuality = clarityLevel <= 0;
                            DebugLogger.Log($"  人物#{faceIdx + 1} ClarityEx: level={clarityLevel} score={clarityScore:F3} → {(isLowQuality ? "低质量" : "清晰")}");
                            if (isLowQuality) anyLowQuality = true;
                            if (faceIdx == 0) result.ClarityScore = clarityScore;
                        }
                        catch (Exception ex)
                        {
                            DebugLogger.LogError($"  人物#{faceIdx + 1} ClarityEx 异常", ex);
                        }
                    }

                    // === PoseEx 侧脸检测 ===
                    float yaw = 0;
                    bool isSideFace = false;
                    bool? rightEyeOccluded = null;

                    if (_poseExAvailable)
                    {
                        try
                        {
                            yaw = DetectPoseYaw(faceImage, face, markPoints68);
                            isSideFace = Math.Abs(yaw) > SideFaceYawThreshold;
                            if (isSideFace)
                                rightEyeOccluded = yaw > 0;
                            DebugLogger.Log($"  人物#{faceIdx + 1} PoseEx: yaw={yaw:F1}° 侧脸={isSideFace} 遮挡眼={(rightEyeOccluded == true ? "右" : rightEyeOccluded == false ? "左" : "无")}");
                        }
                        catch (Exception ex)
                        {
                            DebugLogger.LogError($"  人物#{faceIdx + 1} PoseEx 异常", ex);
                        }
                    }

                    // === EyeStateDetector(5点) ===
                    EyeStateResult? eyeResult5 = null;
                    if (_eyeStateDetectorAvailable && _faceLandmarker5 != null)
                    {
                        try
                        {
                            var pts5 = _faceLandmarker5.Mark(faceImage, face);
                            if (pts5 != null && pts5.Length > 0)
                                eyeResult5 = _eyeStateDetector!.Detect(faceImage, pts5);
                        }
                        catch { }
                    }

                    // === EyeStateDetector(68点) ===
                    EyeStateResult? eyeResult68 = null;
                    if (_eyeStateDetectorAvailable)
                    {
                        try { eyeResult68 = _eyeStateDetector!.Detect(faceImage, markPoints68); }
                        catch { }
                    }

                    // === EAR (68点几何) ===
                    double leftEar = CalcEar(markPoints68, 36, 37, 38, 39, 40, 41);
                    double rightEar = CalcEar(markPoints68, 42, 43, 44, 45, 46, 47);

                    // === 像素分析 ===
                    double leftDarkRatio = AnalyzeEyePixels(skBitmap, markPoints68, true);
                    double rightDarkRatio = AnalyzeEyePixels(skBitmap, markPoints68, false);

                    // === 综合判断（侧脸感知）===
                    bool leftClosed = DetermineEyeClosed(
                        eyeResult5?.LeftEyeState, eyeResult68?.LeftEyeState, leftEar,
                        skBitmap, markPoints68, true, rightEyeOccluded == false, out string leftMethod);
                    bool rightClosed = DetermineEyeClosed(
                        eyeResult5?.RightEyeState, eyeResult68?.RightEyeState, rightEar,
                        skBitmap, markPoints68, false, rightEyeOccluded == true, out string rightMethod);

                    bool faceClosed = leftClosed || rightClosed;
                    if (faceClosed) anyClosed = true;

                    DebugLogger.Log($"  人物#{faceIdx + 1} ({face.Location.X},{face.Location.Y}) {face.Location.Width}x{face.Location.Height}" +
                        $" | EyeState5=({eyeResult5?.LeftEyeState}/{eyeResult5?.RightEyeState})" +
                        $" EyeState68=({eyeResult68?.LeftEyeState}/{eyeResult68?.RightEyeState})" +
                        $" EAR=({leftEar:F3}/{rightEar:F3})" +
                        $" 暗比=({leftDarkRatio:F3}/{rightDarkRatio:F3})" +
                        $" yaw={yaw:F1}°" +
                        $" → {(faceClosed ? "闭眼" : "睁眼")} [左:{leftMethod} 右:{rightMethod}]");
                }

                if (anyClosed)
                    result.EyeState = Models.EyeState.Closed;
                if (anyLowQuality)
                    result.LowQualityFace = true;
            }
            catch (Exception ex)
            {
                DebugLogger.LogError("DetectFromSkBitmap 异常", ex);
            }

            return result;
        }

        /// <summary>
        /// 使用低阈值检测器回退检测，用于略微模糊照片的人脸识别。
        /// </summary>
        private FaceInfo[] DetectWithFallbackThreshold(FaceImage faceImage)
        {
            if (_faceDetectorLowThreshold == null)
            {
                DebugLogger.Log("DetectWithFallbackThreshold: 低阈值检测器不可用");
                return Array.Empty<FaceInfo>();
            }

            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var faces = _faceDetectorLowThreshold.Detect(faceImage);
                sw.Stop();
                DebugLogger.Log($"DetectWithFallbackThreshold: 0.3阈值检测返回 {faces?.Length ?? 0} 张人脸, 耗时={sw.ElapsedMilliseconds}ms");
                return faces ?? Array.Empty<FaceInfo>();
            }
            catch (Exception ex)
            {
                DebugLogger.LogError("DetectWithFallbackThreshold: 异常", ex);
                return Array.Empty<FaceInfo>();
            }
        }

        /// <summary>
        /// 通过 PoseEx 获取人脸偏航角(yaw)。
        /// yaw > 0 → 头向右转（右眼远离镜头）
        /// yaw &lt; 0 → 头向左转（左眼远离镜头）
        /// </summary>
        private static float DetectPoseYaw(FaceImage faceImage, FaceInfo face, FaceMarkPoint[] markPoints)
        {
            int level = -1;
            float score = -1;
            NativeQualityOfPoseEx(ref faceImage, face.Location, markPoints, markPoints.Length,
                ref level, ref score,
                yawLow: SideFaceYawThreshold, yawHigh: 10,
                pitchLow: 20, pitchHigh: 10,
                rollLow: 33.33f, rollHigh: 16.67f);
            return score;
        }

        /// <summary>
        /// 通过 ClarityEx 评估人脸清晰度。
        /// 返回 (level, score)，level: Low=0, Medium=1, High=2
        /// </summary>
        private (int level, float score) EvaluateClarity(FaceImage faceImage, FaceRect faceRect, FaceMarkPoint[] markPoints)
        {
            int level = -1;
            float score = -1;
            NativeQualityOfClarityEx(_clarityExHandle, ref faceImage, faceRect,
                markPoints, markPoints.Length, ref level, ref score);
            return (level, score);
        }

        /// <summary>
        /// 综合多种方法判断单眼是否闭合，支持侧脸感知。
        /// 
        /// 优先级：EyeState5点 > EyeState68点 > 像素分析 > EAR
        /// 
        /// 侧脸逻辑：
        /// - 正面：两眼都可见，Random/Unknown → 保守视为闭眼
        /// - 侧脸遮挡眼：Random/Unknown 是因为看不到眼，不应视为闭眼，跳过此眼
        /// - 侧脸可见眼：仍然正常判断 Close/Unknown→闭眼
        /// </summary>
        private bool DetermineEyeClosed(VFEyeState? eyeState5, VFEyeState? eyeState68,
            double ear, SKBitmap? bitmap, FaceMarkPoint[]? markPoints, bool isLeftEye,
            bool isOccludedBySideFace, out string method)
        {
            // 侧脸遮挡的眼：Random/Unknown 是因为看不到，不视为闭眼
            if (isOccludedBySideFace)
            {
                // 只有明确 Close 才算闭眼（通过可见部分的线索判断）
                if (eyeState5.HasValue && eyeState5.Value == VFEyeState.Close)
                { method = "5点Close(侧脸遮挡)"; return true; }
                if (eyeState68.HasValue && eyeState68.Value == VFEyeState.Close)
                { method = "68点Close(侧脸遮挡)"; return true; }

                // Random/Unknown 在侧脸遮挡情况下视为"无法判断"而非"闭眼"
                if (eyeState5.HasValue && eyeState5.Value == VFEyeState.Open)
                { method = "5点Open(侧脸遮挡)"; return false; }
                if (eyeState68.HasValue && eyeState68.Value == VFEyeState.Open)
                { method = "68点Open(侧脸遮挡)"; return false; }

                // 所有方法都无效时，侧脸遮挡眼不判定为闭眼
                method = "侧脸遮挡→跳过";
                return false;
            }

            // === 以下为正面或侧脸可见眼的判断逻辑 ===

            // EyeStateDetector(5点)
            if (eyeState5.HasValue)
            {
                if (eyeState5.Value == VFEyeState.Close) { method = "5点Close"; return true; }
                if (eyeState5.Value == VFEyeState.Unknown) { method = "5点Unknown→闭眼"; return true; }
                if (eyeState5.Value == VFEyeState.Open) { method = "5点Open"; return false; }
                // Random → 回退
            }

            // EyeStateDetector(68点)
            if (eyeState68.HasValue)
            {
                if (eyeState68.Value == VFEyeState.Close) { method = "68点Close"; return true; }
                if (eyeState68.Value == VFEyeState.Unknown) { method = "68点Unknown→闭眼"; return true; }
                if (eyeState68.Value == VFEyeState.Open) { method = "68点Open"; return false; }
                // Random → 回退
            }

            // 像素分析 fallback
            if (bitmap != null && markPoints != null && markPoints.Length >= 48)
            {
                double darkRatio = AnalyzeEyePixels(bitmap, markPoints, isLeftEye);
                const double darkRatioThreshold = 0.15;
                if (darkRatio < darkRatioThreshold)
                {
                    method = $"像素暗比={darkRatio:F3}<{darkRatioThreshold}";
                    return true;
                }
                method = $"像素暗比={darkRatio:F3}>={darkRatioThreshold}";
                return false;
            }

            // EAR fallback
            const double earThreshold = 0.24;
            if (ear < earThreshold)
            {
                method = $"EAR={ear:F3}<{earThreshold}";
                return true;
            }

            method = $"EAR={ear:F3}>={earThreshold}";
            return false;
        }

        /// <summary>
        /// 像素级眼部分析：裁剪眼部区域，计算暗像素比例。
        /// 睁眼：可见虹膜/瞳孔（暗色），暗像素比例高。
        /// 闭眼：只有眼睑（皮肤色），暗像素比例低。
        /// </summary>
        private static double AnalyzeEyePixels(SKBitmap bitmap, FaceMarkPoint[] pts, bool isLeftEye)
        {
            int startIdx = isLeftEye ? 36 : 42;
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;

            for (int i = startIdx; i < startIdx + 6; i++)
            {
                if (pts[i].X < minX) minX = pts[i].X;
                if (pts[i].Y < minY) minY = pts[i].Y;
                if (pts[i].X > maxX) maxX = pts[i].X;
                if (pts[i].Y > maxY) maxY = pts[i].Y;
            }

            int pad = (int)Math.Max(3, (maxX - minX) * 0.3);
            int x0 = Math.Max(0, (int)minX - pad);
            int y0 = Math.Max(0, (int)minY - pad);
            int x1 = Math.Min(bitmap.Width - 1, (int)maxX + pad);
            int y1 = Math.Min(bitmap.Height - 1, (int)maxY + pad);

            int w = x1 - x0 + 1;
            int h = y1 - y0 + 1;
            if (w <= 0 || h <= 0) return 0.5;

            long totalBrightness = 0;
            int pixelCount = w * h;
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    var c = bitmap.GetPixel(x, y);
                    totalBrightness += (c.Red * 77 + c.Green * 150 + c.Blue * 29) >> 8;
                }
            }

            double avgBrightness = (double)totalBrightness / pixelCount;
            double darkThreshold = avgBrightness * 0.45;

            int darkCount = 0;
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    var c = bitmap.GetPixel(x, y);
                    int brightness = (c.Red * 77 + c.Green * 150 + c.Blue * 29) >> 8;
                    if (brightness < darkThreshold) darkCount++;
                }
            }

            return (double)darkCount / pixelCount;
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
            FaceCount = 0,
            EyeState = Models.EyeState.Open,
            ClarityScore = 0,
            IsBlur = false,
            LowConfidenceFace = false,
            LowQualityFace = false
        };

        public void Dispose()
        {
            if (!_disposed)
            {
                _faceDetector?.Dispose();
                _faceDetectorLowThreshold?.Dispose();
                _faceLandmarker68?.Dispose();
                _faceLandmarker5?.Dispose();
                _eyeStateDetector?.Dispose();
                if (_clarityExAvailable && _clarityExHandle != IntPtr.Zero)
                {
                    try { NativeDisposeClarityEx(_clarityExHandle); } catch { }
                }
                _disposed = true;
            }
        }
    }
}
