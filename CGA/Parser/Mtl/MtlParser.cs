using ObjVisualizer.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace ObjVisualizer.Parser.Mtl
{
    internal class MtlParser(string filePath) : IMtlParser
    {

        private static readonly Dictionary<string, ImageFormat> _formatDictionary = new()
        {
            { ".bmp", ImageFormat.Bmp },
            { ".gif", ImageFormat.Gif },
            { ".jpg", ImageFormat.Jpeg },
            { ".jpeg", ImageFormat.Jpeg },
            { ".png", ImageFormat.Png },
            { ".tiff", ImageFormat.Tiff }
        };

        private readonly string _mtlName = Path.GetFileName(filePath);
        private readonly string _mtlDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;

        private const string MAP_KD = "map_kd";
        private const string MAP_MRAO = "map_mrao";
        private const string NORM = "norm";

        public ImageData GetMapKdBytes() => GetBitmapBytes(MAP_KD);

        public ImageData GetMapMraoBytes() => GetBitmapBytes(MAP_MRAO);

        public ImageData GetNormBytes() => GetBitmapBytes(NORM);

        private ImageData GetBitmapBytes(string paramName)
        {
            string line;

            using var reader = new StreamReader(_mtlName);
            {
                do
                {
                    line = reader.ReadLine() ?? string.Empty;
                }
                while (!line.Contains(paramName, StringComparison.InvariantCultureIgnoreCase) && line != string.Empty);
            }
            if (line == string.Empty)
                return null;
            var fileName = line.Split(' ')[1];
            var fileExtension = Path.GetExtension(fileName);
            var bitmap = new Bitmap(fileName);

            PixelFormat pixelFormat = bitmap.PixelFormat;
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            byte[] imageBytes;
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, pixelFormat);
            var stride = bmpData.Stride;
            int dataSize = Math.Abs(stride) * bitmap.Height;
            imageBytes = new byte[dataSize];
            Marshal.Copy(bmpData.Scan0, imageBytes, 0, dataSize);
            bitmap.UnlockBits(bmpData);

            var bitsPerPixel = GetBitsPerPixel(pixelFormat);

            return new ImageData(stride, (short)bitsPerPixel, bitmap.Width, bitmap.Height, imageBytes);
        }

        static int GetBitsPerPixel(PixelFormat pixelFormat)
        {
            if (pixelFormat == PixelFormat.Format24bppRgb)
            {
                return 24;
            }
            else if (pixelFormat == PixelFormat.Format32bppArgb || pixelFormat == PixelFormat.Format32bppRgb)
            {
                return 32;
            }
            else if (pixelFormat == PixelFormat.Format8bppIndexed)
            {
                return 8;
            }

            return 0;
        }

        //static byte[] GetImageBytes(Bitmap bitmap)
        //{



        //    return imageBytes;
        //}
    }
}
