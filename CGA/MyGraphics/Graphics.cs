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
using System.Windows.Media.Media3D;
using System.Data.Common;
using System.Drawing;

namespace CGA.MyGraphics
{
    public class Graphics
    {
        System.Windows.Controls.Image image { get; set; }
        public int RenderColor { get; set; }
        public int dpiX { get; set; } = 96;
        public int dpiY { get; set; } = 96;

        private List<List<double>> ZBuffer = Enumerable.Range(0, 700)
            .Select(_ => Enumerable.Repeat(double.MaxValue, 700).ToList())
            .ToList();

        public Graphics(System.Windows.Controls.Image image)
        {
            this.image = image;
            RenderColor = (255 << 16) | (255 << 8) | (255);
        }

        public int GetFillColor(float angleCos) {
            byte color = (byte)(255 * angleCos);
            return (color << 16) | (color << 8) | (color);
        }

        private static int CalculateLambertian(Vector3 lightPosition, Vector3 normal, float lightIntensity=0.8f)
        {
            Vector3 lightDirection = Vector3.Normalize(lightPosition);
            float dotProduct = Math.Max(Vector3.Dot(normal, lightDirection), 0.0f);
            float intensity = lightIntensity * dotProduct;
            byte color = (byte)(255 * intensity);
            return (color << 16) | (color << 8) | (color);
        }

        public void DrawEntityMesh(Matrix4x4 worldModel, Mesh entity, float width, float height)
        {
            var wBitmap = new WriteableBitmap((int)700, (int)700, dpiX, dpiY, PixelFormats.Bgr32, null);
            image.Source = wBitmap;
            wBitmap.Lock();
            var faces = entity.getFaces();
            var positions = entity.GetPositionsInWorldModel(worldModel);
            var bmpInfo = new WritableBitmapInfo(wBitmap.BackBuffer, wBitmap.BackBufferStride, wBitmap.Format.BitsPerPixel);
            Parallel.ForEach(faces, (face) =>
            {
                int x2, y2, z2;
                for (int i = 0; i <= face.g_vertexes.Length-1; i++)
                {
                    int z1 = (int)(positions[face.g_vertexes[i]].Z);
                    int x1 = (int)(positions[face.g_vertexes[i]].X);
                    x1 = (int)Math.Max(0, Math.Min(x1, width - 1));
                    int y1 = (int)(positions[face.g_vertexes[i]].Y);
                    y1 = (int)Math.Max(0, Math.Min(y1, height - 1));
                    if (i == face.g_vertexes.Length - 1)
                    {
                        z2 = (int)(positions[face.g_vertexes[0]].Z);
                        x2 = (int)(positions[face.g_vertexes[0]].X);
                        x2 = (int)Math.Max(0, Math.Min(x2, width - 1));
                        y2 = (int)(positions[face.g_vertexes[0]].Y);
                        y2 = (int)Math.Max(0, Math.Min(y2, height - 1));
                    }
                    else
                    {
                        z2 = (int)(positions[face.g_vertexes[i + 1]].Z);
                        x2 = (int)(positions[face.g_vertexes[i + 1]].X);
                        x2 = (int)Math.Max(0, Math.Min(x2, width - 1));
                        y2 = (int)(positions[face.g_vertexes[i + 1]].Y);
                        y2 = (int)Math.Max(0, Math.Min(y2, height - 1));
                    }
                    DrawLine(bmpInfo, x1, y1, x2, y2);
                    //DrawLine(bmpInfo, new Vector3(x1,y1,z1), new Vector3(x2, y2, z2));
                }
            });
            wBitmap.AddDirtyRect(new Int32Rect(0, 0, (int)700, (int)700));
            wBitmap.Unlock();
        }

        private record WritableBitmapInfo(nint BackBuffer, int BackBufferStride, int FormatBitsPerPixel);

        private unsafe void DrawLine(WritableBitmapInfo bmp, Vector3 p1, Vector3 p2)
        {
            int x1 = (int)p1.X;
            int y1 = (int)p1.Y;
            float z1 = p1.Z;
            int x2 = (int)p2.X;
            int y2 = (int)p2.Y;
            float z2 = p2.Z;

            var zDiff = z1 - z2;
            var distance = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
            var zStep = distance == 0 ? 0 : zDiff / distance;

            bool step = Math.Abs(y2 - y1) > Math.Abs(x2 - x1);

            if (step)
            {
                (x1, y1) = (y1, x1);
                (x2, y2) = (y2, x2);
            }

            if (x1 > x2)
            {
                (x1, x2) = (x2, x1);
                (y1, y2) = (y2, y1);
            }

            int dx = x2 - x1;
            int dy = Math.Abs(y2 - y1);
            int error = dx / 2;
            int ystep = (y1 < y2) ? 1 : -1;
            int y = y1;
            int row, col;
            var backBuffer = bmp.BackBuffer;
            int bmpStride = bmp.BackBufferStride;
            int pixelSize = bmp.FormatBitsPerPixel / 8;
            var startOfBuffer = backBuffer;
            for (int x = x1; x <= x2; x++)
            {
                if (step)
                {
                    row = x;
                    col = y;
                }
                else
                {
                    row = y;
                    col = x;
                }

                /*if (ZBuffer[row][col] > z1 + zStep * (x - x1))
                {*/
                    backBuffer += (int)row * bmpStride;
                    backBuffer += (int)col * pixelSize;

                    (*(int*)backBuffer) = RenderColor;
                    backBuffer = startOfBuffer;
                    ZBuffer[row][col] = z1 + zStep * (x - x1);
                //}

                error -= dy;

                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
        }

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

        public unsafe void Rasterize(Matrix4x4 worldModel, Mesh entity)
        {
            var wBitmap = new WriteableBitmap((int)700, (int)700, dpiX, dpiY, PixelFormats.Bgr32, null);
            image.Source = wBitmap;
            ZBuffer = Enumerable.Range(0, 700)
            .Select(_ => Enumerable.Repeat(double.MaxValue, 700).ToList())
            .ToList();
            wBitmap.Lock();
            var faces = entity.getFaces();
            var positions = entity.GetPositionsInWorldModel(worldModel);
            var bmpInfo = new WritableBitmapInfo(wBitmap.BackBuffer, wBitmap.BackBufferStride, wBitmap.Format.BitsPerPixel);
            Parallel.ForEach(faces, (face) =>
            {
                Triangle triangle = new(
                    positions[face.g_vertexes[0]],
                    positions[face.g_vertexes[1]],
                    positions[face.g_vertexes[2]]);
                //int fillColor = GetFillColor(Math.Max(0, Vector3.Dot(new Vector3(200,0,-200), triangle.NormalVector())));
                int fillColor = CalculateLambertian(new Vector3(0, 0, -40), triangle.NormalVector());
                //fillColor = RenderColor;
                MyRasterizeTriangle(bmpInfo, triangle, fillColor);
            });
            wBitmap.AddDirtyRect(new Int32Rect(0, 0, (int)700, (int)700));
            wBitmap.Unlock();
        }

        private static List<float> Interpolate(float i0, float d0, float i1, float d1)
        {
            if (i0 == i1)
            {
                return [d0];
            }

            var values = new List<float>();

            float a = (d1 - d0) / (i1 - i0);
            float d = d0;

            for (int i = (int)i0; i < i1; i++)
            {
                values.Add(d);
                d += a;
            }

            return values;
        }
        private unsafe void MyRasterizeTriangle(WritableBitmapInfo bmp, Triangle triangle, int fillColor)
        {
            if (triangle.A.X > 0 && triangle.B.X > 0 && triangle.C.X > 0 &&
                triangle.A.Y > 0 && triangle.B.Y > 0 && triangle.C.Y > 0 &&
                triangle.A.X < 700 && triangle.B.X < 700 && triangle.C.X < 700 &&
                triangle.A.Y < 700 && triangle.B.Y < 700 && triangle.C.Y < 700)
            {

                if (triangle.B.Y < triangle.A.Y)
                {
                    (triangle.B, triangle.A) = (triangle.A, triangle.B);
                }
                if (triangle.C.Y < triangle.A.Y)
                {
                    (triangle.C, triangle.A) = (triangle.A, triangle.C);
                }
                if (triangle.C.Y < triangle.B.Y)
                {
                    (triangle.B, triangle.C) = (triangle.C, triangle.B);
                }

                var x01 = Interpolate(triangle.A.Y, triangle.A.X, triangle.B.Y, triangle.B.X);
                var x12 = Interpolate(triangle.B.Y, triangle.B.X, triangle.C.Y, triangle.C.X);
                var x02 = Interpolate(triangle.A.Y, triangle.A.X, triangle.C.Y, triangle.C.X);

                var z01 = Interpolate(triangle.A.Y, triangle.A.Z, triangle.B.Y, triangle.B.Z);
                var z12 = Interpolate(triangle.B.Y, triangle.B.Z, triangle.C.Y, triangle.C.Z);
                var z02 = Interpolate(triangle.A.Y, triangle.A.Z, triangle.C.Y, triangle.C.Z);

                x01.RemoveAt(x01.Count - 1);
                var x012 = x01.Concat(x12).ToList();
                z01.RemoveAt(z01.Count - 1);
                var z012 = z01.Concat(z12).ToList();

                var m = (int)Math.Floor(x012.Count / 2.0);

                List<float> x_left;
                List<float> x_right;
                List<float> z_left;
                List<float> z_right;

                if (x02[m] < x012[m])
                {
                    (x_left, x_right) = (x02, x012);
                    (z_left, z_right) = (z02, z012);
                }
                else
                {
                    (x_left, x_right) = (x012, x02);
                    (z_left, z_right) = (z012, z02);
                }

                var backBuffer = bmp.BackBuffer;
                int bmpStride = bmp.BackBufferStride;
                int pixelSize = bmp.FormatBitsPerPixel / 8;
                var startOfBuffer = backBuffer;
                for (int y = (int)triangle.A.Y; y < triangle.C.Y; y++)
                {
                    var index = (int)(y - triangle.A.Y);

                    if (index < x_left.Count && index < x_right.Count)
                    {
                        var xl = (int)x_left[index];
                        var xr = (int)x_right[index];
                        var zl = z_left[index];
                        var zr = z_right[index];
                        var zscan = Interpolate(xl, zl, xr, zr);
                        
                        for (int x = xl; x < xr; x++)
                        {
                            var z = zscan[x - xl];
                            if (z < ZBuffer[y][x])
                            {
                                ZBuffer[y][x] = z;
                                //если сначала отрисуется задняя часть то передняя все равно будет нарисована заново на том же месте
                                backBuffer += (int)y * bmpStride;
                                backBuffer += (int)x * pixelSize;
                                (*(int*)backBuffer) = fillColor;
                                backBuffer = startOfBuffer;
                            }
                        }
                    }
                }
            }
        }
    }
}
