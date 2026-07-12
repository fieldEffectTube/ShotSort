namespace ShotSort.Models
{
    public enum PhotoFileType
    {
        JPG,
        RAW
    }

    public enum PhotoCategory
    {
        None,
        HasPerson,
        LowConfidencePerson,
        LowQualityPerson,
        Selected,
        Kept,
        PendingDelete
    }

    public class PhotoItem
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public PhotoFileType FileType { get; set; }
        public string? PairedRawPath { get; set; }
        public ClassifyResult? AiResult { get; set; }
        public PhotoCategory Category { get; set; } = PhotoCategory.None;
    }
}
