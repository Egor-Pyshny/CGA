using ObjVisualizer.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace ObjVisualizer.Graphics
{
    internal class Drawer(int width, int height, nint drawBuffer, int stride)
    {
        private readonly Random Random = new();
        private readonly List<List<double>> ZBuffer = Enumerable.Range(0, height)
            .Select(_ => Enumerable.Repeat(double.MaxValue, width).ToList())
            .ToList();

        private readonly nint Buffer = drawBuffer;

        private readonly int _width = width;
        private readonly int _height = height;
        private readonly int _stride = stride;

        public unsafe void Rasterize(IList<Vector4> vertices, Color color)
        {
            for (int i = 1; i < vertices.Count - 1; i++)
            {
                MyRasterizeTriangle(new(
                    new(vertices[0].X, vertices[0].Y, vertices[0].Z),
                    new(vertices[i].X, vertices[i].Y, vertices[i].Z),
                    new(vertices[i + 1].X, vertices[i + 1].Y, vertices[i + 1].Z)), color);
            }
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

        private unsafe void MyRasterizeTriangle(Triangle triangle, Color color)
        {
            if (triangle.A.X > 0 && triangle.B.X > 0 && triangle.C.X > 0 &&
                triangle.A.Y > 0 && triangle.B.Y > 0 && triangle.C.Y > 0 &&
                triangle.A.X < _width && triangle.B.X < _width && triangle.C.X < _width &&
                triangle.A.Y < _height && triangle.B.Y < _height && triangle.C.Y < _height)
            {

                byte* data = (byte*)Buffer.ToPointer();

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
                var topY = int.Max(0, (int)float.Ceiling(triangle.A.Y));
                var bottomY = int.Min(_height, (int)float.Ceiling(triangle.C.Y));
                for (int y = topY; y < bottomY; y++)
                {
                    var index = (int)(y - triangle.A.Y);

                    //if (index < x_left.Count && index < x_right.Count)
                    {
                        var leftX = int.Max(0, (int)float.Ceiling(x_left[index]));
                        var rightX = int.Min(_width, (int)float.Ceiling(x_right[index]));
                        //var xl = (int)x_left[index];
                        //var xr = (int)x_right[index];
                        var zl = z_left[index];
                        var zr = z_right[index];
                        var zscan = Interpolate(leftX, zl, rightX, zr);

                        for (int x = leftX; x < rightX; x++)
                        {
                            var z = zscan[x - leftX];
                            if (z < ZBuffer[y][x])
                            {
                                ZBuffer[y][x] = z;
                                byte* pixelPtr = data + y * _stride + x * 3;
                                *pixelPtr++ = color.R;
                                *pixelPtr++ = color.G;
                                *pixelPtr = color.B;
                            }
                        }
                    }
                }
            }
        }

        private unsafe void RasterizeTriangle(Triangle triangle)
        {
            foreach (var line in triangle.GetHorizontalLines())
            {
                if (line.Left.X > 0 && line.Left.Y > 0 &&
                    line.Right.X > 0 && line.Right.Y > 0 &&
                    line.Left.X < _width && line.Left.Y < _height &&
                    line.Right.X < _width && line.Right.Y < _height)
                {
                    DrawLine(line.Left, line.Right, (byte*)Buffer.ToPointer(), Color.White);
                }
            }
        }

        public unsafe void DrawLine(Vector3 p1, Vector3 p2, byte* data, Color color)
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
            int ystep = y1 < y2 ? 1 : -1;
            int y = y1;
            int row, col;

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

                if (ZBuffer[row][col] > z1 + zStep * (x - x1))
                {
                    byte* pixelPtr = data + row * _stride + col * 3;

                    ZBuffer[row][col] = z1 + zStep * (x - x1);

                    *pixelPtr++ = color.B;
                    *pixelPtr++ = color.G;
                    *pixelPtr = color.R;
                }

                error -= dy;

                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
        }

        public unsafe void DrawLine(int x0, int y0, int x1, int y1, byte* data, int stride)
        {
            bool step = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);

            if (step)
            {
                (x0, y0) = (y0, x0);
                (x1, y1) = (y1, x1);
            }

            if (x0 > x1)
            {
                (x0, x1) = (x1, x0);
                (y0, y1) = (y1, y0);
            }

            int dx = x1 - x0;
            int dy = Math.Abs(y1 - y0);
            int error = dx / 2;
            int ystep = (y0 < y1) ? 1 : -1;
            int y = y0;
            int var1, var2;

            for (int x = x0; x <= x1; x++)
            {
                if (step)
                {
                    var1 = x;
                    var2 = y;
                }
                else
                {
                    var1 = y;
                    var2 = x;
                }

                byte* pixelPtr = data + var1 * stride + var2 * 3;

                *pixelPtr++ = 255;
                *pixelPtr++ = 255;
                *pixelPtr = 255;

                error -= dy;

                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
        }
    }
}
