namespace ShotSort.Models
{
    public enum EyeState
    {
        Open,
        Closed,
        OneEyeClosed,
        BothClosed,
        MultiClosed,
    }

    public class ClassifyResult
    {
        public bool HasFace { get; set; }
        public int FaceCount { get; set; }
        public EyeState EyeState { get; set; }
        public float ClarityScore { get; set; }
        public bool IsBlur { get; set; }
        /// <summary>
        /// 低置信度人像：0.7 阈值未检出，0.4 阈值检出
        /// </summary>
        public bool LowConfidenceFace { get; set; }
        /// <summary>
        /// 低质量人像：ClarityEx 评估为模糊（仅≤3人照片）
        /// </summary>
        public bool LowQualityFace { get; set; }
    }
}
