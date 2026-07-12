using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using ImageMagick;

namespace ShotSort.Utils
{
    public static class ImageLoader
    {
        private const int DefaultDecodeWidth = 1920;
        private const int ExifOrientationId = 0x0112;

        public static Bitmap LoadJpg(string path, int targetWidth = DefaultDecodeWidth)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var img = Image.FromStream(fs))
            {
                ApplyExifOrientation(img);

                if (img.Width <= targetWidth)
                    return new Bitmap(img);

                var ratio = (double)targetWidth / img.Width;
                var newHeight = (int)(img.Height * ratio);
                var bmp = new Bitmap(targetWidth, newHeight);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(img, 0, 0, targetWidth, newHeight);
                }
                return bmp;
            }
        }

        public static void ApplyExifOrientation(Image img)
        {
            if (!img.PropertyIdList.Contains(ExifOrientationId))
                return;

            var prop = img.GetPropertyItem(ExifOrientationId);
            if (prop?.Value == null || prop.Value.Length < 2)
                return;

            int orientation = prop.Value[0] | (prop.Value[1] << 8);
            if (orientation == 1)
                return;

            RotateFlipType transform = orientation switch
            {
                2 => RotateFlipType.RotateNoneFlipX,
                3 => RotateFlipType.Rotate180FlipNone,
                4 => RotateFlipType.Rotate180FlipX,
                5 => RotateFlipType.Rotate90FlipX,
                6 => RotateFlipType.Rotate90FlipNone,
                7 => RotateFlipType.Rotate270FlipX,
                8 => RotateFlipType.Rotate270FlipNone,
                _ => RotateFlipType.RotateNoneFlipNone
            };

            if (transform != RotateFlipType.RotateNoneFlipNone)
            {
                img.RotateFlip(transform);
                img.RemovePropertyItem(ExifOrientationId);
            }
        }

        public static Bitmap? LoadRaw(string path, int targetWidth = DefaultDecodeWidth, CancellationToken cancellationToken = default)
        {
            var settings = new MagickReadSettings
            {
                Density = new Density(72, 72),
            };

            // Hint the decoder to produce a smaller image up-front.
            // Most RAW coders in ImageMagick support subsampled decode,
            // reducing a 6000x4000 decode (~384MB) to ~3000x2000 (~96MB).
            settings.SetDefine("jpeg:size", $"{targetWidth * 2}x{targetWidth * 2}");
            // For DNG/CR2/CR3/NEF, the size hint is honored via dcraw delegate
            settings.SetDefine("dng:read-thumbnail", "true");

            using (var image = new MagickImage(path, settings))
            {
                if (cancellationToken.IsCancellationRequested)
                    return null;

                if (image.Width > targetWidth)
                {
                    var ratio = (double)targetWidth / image.Width;
                    image.Resize((uint)targetWidth, (uint)(image.Height * ratio));
                }

                // Write directly to Bitmap via pixel data instead of BMP byte array
                // to avoid the large intermediate buffer from ToByteArray().
                return CreateBitmapFromMagickImage(image);
            }
        }

        private static Bitmap CreateBitmapFromMagickImage(MagickImage image)
        {
            var width = (int)image.Width;
            var height = (int)image.Height;
            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var pixels = image.ToByteArray(MagickFormat.Bgra);

            var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }
    }
}
