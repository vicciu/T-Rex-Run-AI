using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TRexAI
{
    /// <summary>
    /// Screen Capture code from: https://www.cyotek.com/blog/capturing-screenshots-using-csharp-and-p-invoke
    /// </summary>
    public static class ScreenShotCapture
    {
        #region DLLImports
        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int nxDest, int nyDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int nHeight);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);
        #endregion

        const int SRCCOPY = 0x00CC0020;

        const int CAPTUREBLT = 0x40000000;

        static Bitmap CaptureRegion(Rectangle region)
        {
            IntPtr desktophWnd;
            IntPtr desktopDc;
            IntPtr memoryDc;
            IntPtr bitmap;
            IntPtr oldBitmap;
            bool success;
            Bitmap result;

            desktophWnd = GetDesktopWindow();
            desktopDc = GetWindowDC(desktophWnd);
            memoryDc = CreateCompatibleDC(desktopDc);
            bitmap = CreateCompatibleBitmap(desktopDc, region.Width, region.Height);
            oldBitmap = SelectObject(memoryDc, bitmap);

            success = BitBlt(memoryDc, 0, 0, region.Width, region.Height, desktopDc, region.Left, region.Top, SRCCOPY | CAPTUREBLT);

            try
            {
                if (!success)
                {
                    throw new System.ComponentModel.Win32Exception();
                }

                result = Image.FromHbitmap(bitmap);
            }
            finally
            {
                SelectObject(memoryDc, oldBitmap);
                DeleteObject(bitmap);
                DeleteDC(memoryDc);
                ReleaseDC(desktophWnd, desktopDc);
            }

            return result;
        }

        /// <summary>
        /// Converts a bitmap image to a byte array in BGRA format.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        static byte[] BitmapToBGRAArray(Bitmap image)
        {
            byte[] bmpArray;
            byte[] bgraArray;

            using (var memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, ImageFormat.Bmp);
                bmpArray = memoryStream.ToArray();

                //The created bmp file has a 54 byte header so we have to get rid of it.
                bgraArray = new byte[bmpArray.Length - 54];
                Array.Copy(bmpArray, 54, bgraArray, 0, bmpArray.Length - 54);
            }

            return bgraArray;
        }

        static byte[] BGRAImageToGrayscale(byte[] rawBGRAImage)
        {
            if (rawBGRAImage.Length % 4 != 0)
            {
                Console.WriteLine("Error Converting Image. It should be 32bpp for conversion");
                return null;
            }

            var grayscaleArraySize = rawBGRAImage.Length / 4;
            var grayscaleArray = new byte[grayscaleArraySize];

            for (int i = 0; i < grayscaleArraySize; i++)
            {
                grayscaleArray[i] = (byte)(rawBGRAImage[i * 4] * 0.0722 + rawBGRAImage[i * 4 + 1] * 0.7152 + rawBGRAImage[i * 4 + 2] * 0.2126);
            }

            return grayscaleArray;
        }

        public static byte[] GetGrayscaleScreenCapture(Rectangle screenshotArea, bool saveScreenshot)
        {
            var bitmap = CaptureRegion(screenshotArea);
            if (bitmap == null)
            {
                Console.WriteLine("ERROR: Failed to capture screenshot.");
                return null;
            }

            if (saveScreenshot)
            {
                try
                {
                    bitmap.Save("testCapture.png");
                }
                catch { }
            }
            
            var rawBGRAImage = BitmapToBGRAArray(bitmap);
            var rawGrayImage = BGRAImageToGrayscale(rawBGRAImage);

            return rawGrayImage;
        }
    }
}
