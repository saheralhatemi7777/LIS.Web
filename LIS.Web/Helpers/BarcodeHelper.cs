// Helpers/BarcodeHelper.cs
using ZXing;
using ZXing.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace مشروع_ادار_المختبرات.Helpers
{
    public class BarcodeHelper
    {
        public static string GenerateAndSaveQrCode(string text, string folderPath, string fileName)
        {
            if (string.IsNullOrEmpty(text))
                text = "NoName";

            if (string.IsNullOrEmpty(fileName))
                fileName = text.Replace(" ", "_") + ".png";

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var writer = new ZXing.BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,  // <-- هنا نحدد QR Code
                Options = new EncodingOptions
                {
                    Height = 200, // حجم الصورة
                    Width = 200,
                    Margin = 1
                }
            };

            var pixelData = writer.Write(text);

            using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb))
            {
                var bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, pixelData.Width, pixelData.Height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppRgb);

                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }

                string fullPath = Path.Combine(folderPath, fileName);
                bitmap.Save(fullPath, ImageFormat.Png);
                return fullPath; // ترجع المسار الكامل للملف
            }
        }
    }
}
