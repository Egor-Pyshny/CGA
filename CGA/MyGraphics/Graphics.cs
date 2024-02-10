using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using CGA.Models.DataModels;
using System.IO;
using System.Windows.Shapes;

namespace CGA.MyGraphics
{
    public class Graphics
    {
        Image image { get; set; }
        public int RenderColor { get; set; }
        public int dpiX { get; set; } = 96;
        public int dpiY { get; set; } = 96;

        public Graphics(Image image)
        {
            this.image = image;
            RenderColor = (255 << 16) | (255 << 8) | (255);
        }

        public void DrawEntityMesh(Matrix4x4 worldModel, Mesh entity, float width, float height)
        {
            var wBitmap = new WriteableBitmap((int)700, (int)700, dpiX, dpiY, PixelFormats.Bgr32, null);
            image.Source = wBitmap;

            wBitmap.Lock();
            Vector4 t3 = new Vector4(), t4 = new Vector4();
            var faces = entity.getFaces();
            var positions = entity.GetPositionsInWorldModel(worldModel);
            var bmpInfo = new WritableBitmapInfo(wBitmap.BackBuffer, wBitmap.BackBufferStride, wBitmap.Format.BitsPerPixel);
            Parallel.ForEach(faces, (face) =>
            {
                int x2, y2;
                for (int i = 0; i <= face.g_vertexes.Length-1; i++)
                {
                    int x1 = (int)(700 / 2 + positions[face.g_vertexes[i]].X);
                    x1 = (int)Math.Max(0, Math.Min(x1, width - 1));
                    int y1 = (int)(700 / 2 + positions[face.g_vertexes[i]].Y);
                    y1 = (int)Math.Max(0, Math.Min(y1, height - 1));
                    if (i == face.g_vertexes.Length - 1)
                    {
                        x2 = (int)(700 / 2 + positions[face.g_vertexes[0]].X);
                        x2 = (int)Math.Max(0, Math.Min(x2, width - 1));
                        y2 = (int)(700 / 2 + positions[face.g_vertexes[0]].Y);
                        y2 = (int)Math.Max(0, Math.Min(y2, height - 1));
                    }
                    else
                    {
                        x2 = (int)(700 / 2 + positions[face.g_vertexes[i + 1]].X);
                        x2 = (int)Math.Max(0, Math.Min(x2, width - 1));
                        y2 = (int)(700 / 2 + positions[face.g_vertexes[i + 1]].Y);
                        y2 = (int)Math.Max(0, Math.Min(y2, height - 1));
                    }
                    DrawLine(bmpInfo, x1, y1, x2, y2);
                }
            });
            wBitmap.AddDirtyRect(new Int32Rect(0, 0, (int)700, (int)700));
            wBitmap.Unlock();
        }

        private record WritableBitmapInfo(nint BackBuffer, int BackBufferStride, int FormatBitsPerPixel);

        private void DrawLine(WritableBitmapInfo bmp, int x1, int y1, int x2, int y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            var steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
            if (steps == 0)
                return;

            dx = dx / steps;
            dy = dy / steps;

            unsafe
            {
                var backBuffer = bmp.BackBuffer;
                int bmpStride = bmp.BackBufferStride;
                int pixelSize = bmp.FormatBitsPerPixel / 8;
                var startOfBuffer = backBuffer;
                double row = y1, column = x1;
                for (int i = 0; i < steps; i++)
                {
                    backBuffer += (int)row * bmpStride;
                    backBuffer += (int)column * pixelSize;

                    (*(int*)backBuffer) = RenderColor;

                    column += dx;
                    row += dy;
                    backBuffer = startOfBuffer;
                }
            }

        }
    }
}
