namespace ShotSort.Models
{
    public enum EyeState
    {
        Open,
        OneEyeClosed,
        BothClosed,
        MultiClosed
    }

    public class ClassifyResult
    {
        public bool HasFace { get; set; }
        public EyeState EyeState { get; set; }
        public float ClarityScore { get; set; }
        public bool IsBlur { get; set; }
    }
}
