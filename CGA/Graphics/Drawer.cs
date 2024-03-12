using ObjVisualizer.Models.DataModels;
using ObjVisualizer.Models.VisualModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace ObjVisualizer.Graphics
{
    internal class Drawer(int width, int height, IntPtr drawBuffer, int stride)
    {
        private readonly Random Random = new();
        private readonly List<List<double>> ZBuffer = Enumerable.Range(0, height)
            .Select(_ => Enumerable.Repeat(double.MaxValue, width).ToList())
            .ToList();

        private readonly IntPtr Buffer = drawBuffer;

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
                    new(vertices[i + 1].X, vertices[i + 1].Y, vertices[i + 1].Z),
                    new(),
                    new(),
                    new(),
                    new(),
                    new(),
                    new()), color);
            }
        }

        public unsafe void Rasterize(IList<Vector4> vertices, IList<Vector3> normales, IList<Vector4> originalVertexes, Scene scene)
        {
            for (int i = 1; i < vertices.Count - 1; i++)
            {
                MyRasterizeTrianglePhong(new(
                    new(vertices[0].X, vertices[0].Y, vertices[0].Z),
                    new(vertices[i].X, vertices[i].Y, vertices[i].Z),
                    new(vertices[i + 1].X, vertices[i + 1].Y, vertices[i + 1].Z),
                    new(normales[0].X, normales[0].Y, normales[0].Z),
                    new(normales[i].X, normales[i].Y, normales[i].Z),
                    new(normales[i + 1].X, normales[i + 1].Y, normales[i + 1].Z),
                    new(originalVertexes[0].X, originalVertexes[0].Y, originalVertexes[0].Z),
                    new(originalVertexes[i].X, originalVertexes[i].Y, originalVertexes[i].Z),
                    new(originalVertexes[i + 1].X, originalVertexes[i + 1].Y, originalVertexes[i + 1].Z)), scene);
            }
        }

        private static List<float> Interpolate(int i0, float d0, int i1, float d1)
        {
            if (i0 == i1)
            {
                return [d0];
            }

            var values = new List<float>();

            float a = (d1 - d0) / (i1 - i0);
            float d = d0;

            for (int i = i0; i <= i1; i++)
            {
                values.Add(d);
                d += a;
            }

            return values;
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

        private unsafe void MyRasterizeTriangle(Triangle triangle, Color color)
        {
            if ((triangle.A.X > 0 && triangle.A.Y > 0 && triangle.A.X < _width && triangle.A.Y < _height) ||
            (triangle.B.X > 0 && triangle.B.Y > 0 && triangle.B.X < _width && triangle.B.Y < _height) ||
            (triangle.C.X > 0 && triangle.C.Y > 0 && triangle.C.X < _width && triangle.C.Y < _height))
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
                int A = (int)float.Round(triangle.A.Y, 0);
                int B = (int)float.Round(triangle.B.Y, 0);
                int C = (int)float.Round(triangle.C.Y, 0);


                (var x02, var x012) = TraingleInterpolation(A, triangle.A.X, B, triangle.B.X, C, triangle.C.X);
                (var z02, var z012) = TraingleInterpolation(A, triangle.A.Z, B, triangle.B.Z, C, triangle.C.Z);

                var m = (int)Math.Floor(x012.Count / 2.0f);
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
                int YDiffTop = 0;
                int YDiffTopI = 0;
                int TopY = (int)float.Round(triangle.A.Y, 0);
                if (triangle.A.Y < 0)
                {
                    YDiffTop = -(int)float.Round(triangle.A.Y, 0);
                    YDiffTopI = (int)float.Round(triangle.C.Y, 0);
                    TopY = 0;
                }

                for (int y = TopY; y <= (int)float.Round(triangle.C.Y, 0); y++)
                {
                    if (y < 0 || y >= _height)
                        continue;
                    var index = (y - TopY + YDiffTop);
                    if (index < x_left.Count && index < x_right.Count)
                    {
                        var xl = (int)float.Round(x_left[index], 0);
                        var xr = (int)float.Round(x_right[index], 0);
                        var zl = z_left[index];
                        var zr = z_right[index];
                        var zscan = Interpolate(xl, zl, xr, zr);
                        for (int x = xl; x < xr; x++)
                        {
                            if (x < 0 || x >= _width)
                                continue;
                            var z = zscan[x - xl];
                            if (z < ZBuffer[y][x])
                            {
                                ZBuffer[y][x] = z;
                                byte* pixelPtr = data + y * _stride + x * 3;
                                *pixelPtr++ = color.B;
                                *pixelPtr++ = color.G;
                                *pixelPtr = color.R;
                            }

                        }

                    }


                }
            }
        }

        private Color CalculateColor(Vector3 point, Vector3 normal, Scene scene, Color baseColor)
        {
            var light = scene.Light.CalculateLightLaba3(point, normal, scene.Camera.Eye);
            var color = Color.FromArgb(
                (byte)(light.X * baseColor.R > 255 ? 255 : light.X * baseColor.R),
                (byte)(light.Y * baseColor.G > 255 ? 255 : light.Y * baseColor.G),
                (byte)(light.Z * baseColor.B > 255 ? 255 : light.Z * baseColor.B));
            return color;
        }

        private (List<float>, List<float>) TraingleInterpolation(int y0, float v0, int y1, float v1, int y2, float v2)
        {
            var v01 = Interpolate(y0, v0, y1, v1);
            var v12 = Interpolate(y1, v1, y2, v2);
            var v02 = Interpolate(y0, v0, y2, v2);
            v01.RemoveAt(v01.Count - 1);
            var v012 = v01.Concat(v12).ToList();
            return (v02, v012);
        }
        private unsafe void MyRasterizeTrianglePhong(Triangle triangle, Scene scene)
        {
            if ((triangle.A.X > 0 && triangle.A.Y > 0 && triangle.A.X < _width && triangle.A.Y < _height) ||
            (triangle.B.X > 0 && triangle.B.Y > 0 && triangle.B.X < _width && triangle.B.Y < _height) ||
            (triangle.C.X > 0 && triangle.C.Y > 0 && triangle.C.X < _width && triangle.C.Y < _height))
            {
                Color baseColor = Color.White;
                byte* data = (byte*)Buffer.ToPointer();
                if (triangle.B.Y < triangle.A.Y)
                {
                    (triangle.B, triangle.A) = (triangle.A, triangle.B);
                    (triangle.NormalB, triangle.NormalA) = (triangle.NormalA, triangle.NormalB);
                    (triangle.RealB, triangle.RealA) = (triangle.RealA, triangle.RealB);
                }
                if (triangle.C.Y < triangle.A.Y)
                {
                    (triangle.C, triangle.A) = (triangle.A, triangle.C);
                    (triangle.NormalC, triangle.NormalA) = (triangle.NormalA, triangle.NormalC);
                    (triangle.RealC, triangle.RealA) = (triangle.RealA, triangle.RealC);
                }
                if (triangle.C.Y < triangle.B.Y)
                {
                    (triangle.B, triangle.C) = (triangle.C, triangle.B);
                    (triangle.NormalB, triangle.NormalC) = (triangle.NormalC, triangle.NormalB);
                    (triangle.RealB, triangle.RealC) = (triangle.RealC, triangle.RealB);
                }
                int YA = (int)float.Round(triangle.A.Y, 0);
                int YB = (int)float.Round(triangle.B.Y, 0);
                int YC = (int)float.Round(triangle.C.Y, 0);

                (var x02, var x012) = TraingleInterpolation(YA, triangle.A.X, YB, triangle.B.X, YC, triangle.C.X);
                (var z02, var z012) = TraingleInterpolation(YA, triangle.A.Z, YB, triangle.B.Z, YC, triangle.C.Z);


                (var nx02, var nx012) = TraingleInterpolation(YA, triangle.NormalA.X, YB, triangle.NormalB.X, YC, triangle.NormalC.X);
                (var ny02, var ny012) = TraingleInterpolation(YA, triangle.NormalA.Y, YB, triangle.NormalB.Y, YC, triangle.NormalC.Y);
                (var nz02, var nz012) = TraingleInterpolation(YA, triangle.NormalA.Z, YB, triangle.NormalB.Z, YC, triangle.NormalC.Z);

                (var rx02, var rx012) = TraingleInterpolation(YA, triangle.RealA.X, YB, triangle.RealB.X, YC, triangle.RealC.X);
                (var ry02, var ry012) = TraingleInterpolation(YA, triangle.RealA.Y, YB, triangle.RealB.Y, YC, triangle.RealC.Y);
                (var rz02, var rz012) = TraingleInterpolation(YA, triangle.RealA.Z, YB, triangle.RealB.Z, YC, triangle.RealC.Z);

                var m = (int)Math.Floor(x012.Count / 2.0f);
                List<float> x_left;
                List<float> x_right;
                List<float> z_left;
                List<float> z_right;
                List<float> nx_right, ny_right, nz_right;
                List<float> nx_left, ny_left, nz_left;

                List<float> rx_right, ry_right, rz_right;
                List<float> rx_left, ry_left, rz_left;

                if (x02[m] < x012[m])
                {
                    (x_left, x_right) = (x02, x012);
                    (z_left, z_right) = (z02, z012);

                    (nx_left, nx_right) = (nx02, nx012);
                    (ny_left, ny_right) = (ny02, ny012);
                    (nz_left, nz_right) = (nz02, nz012);
                    (rx_left, rx_right) = (rx02, rx012);
                    (ry_left, ry_right) = (ry02, ry012);
                    (rz_left, rz_right) = (rz02, rz012);

                }
                else
                {
                    (x_left, x_right) = (x012, x02);
                    (z_left, z_right) = (z012, z02);

                    (nx_left, nx_right) = (nx012, nx02);
                    (ny_left, ny_right) = (ny012, ny02);
                    (nz_left, nz_right) = (nz012, nz02);

                    (rx_left, rx_right) = (rx012, rx02);
                    (ry_left, ry_right) = (ry012, ry02);
                    (rz_left, rz_right) = (rz012, rz02);

                }
                int YDiffTop = 0;
                int YDiffTopI = 0;
                int TopY = (int)float.Round(triangle.A.Y, 0);
                if (triangle.A.Y < 0)
                {
                    YDiffTop = -(int)float.Round(triangle.A.Y, 0);
                    YDiffTopI = (int)float.Round(triangle.C.Y, 0);
                    TopY = 0;
                }
                for (int y = TopY; y <= (int)float.Round(triangle.C.Y, 0); y++)
                {
                    if (y < 0 || y >= _height)
                        continue;
                    var index = (y - TopY + YDiffTop);
                    //if (index < x_left.Count && index < x_right.Count)
                    {
                        var xl = (int)float.Round(x_left[index], 0);
                        var xr = (int)float.Round(x_right[index], 0);
                        var zl = z_left[index];
                        var zr = z_right[index];
                        (var nxl, var nxr) = (nx_left[index], nx_right[index]);
                        (var nyl, var nyr) = (ny_left[index], ny_right[index]);
                        (var nzl, var nzr) = (nz_left[index], nz_right[index]);

                        (var rxl, var rxr) = (rx_left[index], rx_right[index]);
                        (var ryl, var ryr) = (ry_left[index], ry_right[index]);
                        (var rzl, var rzr) = (rz_left[index], rz_right[index]);
                        var zscan = Interpolate(xl, zl, xr, zr);

                        var nxscan = Interpolate(xl, nxl, xr, nxr);
                        var nyscan = Interpolate(xl, nyl, xr, nyr);
                        var nzscan = Interpolate(xl, nzl, xr, nzr);
                        var rxscan = Interpolate(xl, rxl, xr, rxr);
                        var ryscan = Interpolate(xl, ryl, xr, ryr);
                        var rzscan = Interpolate(xl, rzl, xr, rzr);


                        for (int x = xl; x < xr; x++)
                        {
                            if (x < 0 || x >= _width)
                                continue;
                            var z = zscan[x - xl];
                            bool GotLock = false;
                            //try
                            //{
                            //    //sl.Enter(ref GotLock);
                            if (z < ZBuffer[y][x])
                            {
                                ZBuffer[y][x] = z;
                                byte* pixelPtr = data + y * _stride + x * 3;
                                var vertex = new Vector3(rxscan[x - xl], ryscan[x - xl], rzscan[x - xl]);
                                var normal = new Vector3(nxscan[x - xl], nyscan[x - xl], nzscan[x - xl]);
                                Color color = CalculateColor(vertex, Vector3.Normalize(normal), scene, baseColor);
                                *pixelPtr++ = color.B;
                                *pixelPtr++ = color.G;
                                *pixelPtr = color.R;
                            }
                            //}finally
                            //{
                            //    //if (GotLock) sl.Exit();
                            //}

                        }

                    }


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
            int ystep = (y1 < y2) ? 1 : -1;
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
    }
}
