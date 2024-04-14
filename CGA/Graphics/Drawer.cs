using ObjVisualizer.Models.DataModels;
using ObjVisualizer.Models.DataModels;
using ObjVisualizer.Models.VisualModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Media3D;

namespace ObjVisualizer.Graphics
{
    internal class Drawer(int width, int height, IntPtr drawBuffer, int stride)
    {
        private readonly float[,] ZBuffer = new float[height, width];
        private Vector3[] colors = new Vector3[height * width];
        private List<Vector3[]> bloom_buffer = new List<Vector3[]>()
        {
            new Vector3[height * width],
            new Vector3[height * width],
            new Vector3[height * width],
            new Vector3[height * width],
            new Vector3[height * width],
            new Vector3[height * width],
            new Vector3[height * width],
            new Vector3[height * width],
        };
        private readonly Random Random = new();
        float[] weight = new float[5] { 0.259027f, 0.2195946f, 0.1376216f, 0.069054f, 0.030216f };
        bool horizontal = true, first_iteration = true;
        private IntPtr Buffer = drawBuffer;
        private readonly int _width = width;
        private readonly int _height = height;
        private int _stride = stride;

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
                    new(),
                    new(),
                    new(),
                    new(),
                    new(),
                    new(),
                    new()), color);
            }
        }

        public unsafe void Rasterize(IList<Vector4> vertices, IList<Vector3> normales, IList<Vector4> real, IList<Vector4> viewVerteces, Scene scene)
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
                    new(),
                    new(),
                    new(),
                    new(real[0].X, real[0].Y, real[0].Z),
                    new(real[i].X, real[i].Y, real[i].Z),
                    new(real[i + 1].X, real[i + 1].Y, real[i + 1].Z),
                    viewVerteces[0],
                    viewVerteces[i],
                    viewVerteces[i + 1]), scene);
            }
        }

        public unsafe void Rasterize(IList<Vector4> vertices, IList<Vector3> textels, IList<Vector4> real, IList<Vector4> view, Scene scene, bool optional = true)
        {
            for (int i = 1; i < vertices.Count - 1; i++)
            {
                MyRasterizeTriangleTexture(new(
                    new(vertices[0].X, vertices[0].Y, vertices[0].Z),
                    new(vertices[i].X, vertices[i].Y, vertices[i].Z),
                    new(vertices[i + 1].X, vertices[i + 1].Y, vertices[i + 1].Z),
                    new(),
                    new(),
                    new(),
                    new(textels[0].X, textels[0].Y),
                    new(textels[i].X, textels[i].Y),
                    new(textels[i + 1].X, textels[i + 1].Y),
                    new(real[0].X, real[0].Y, real[0].Z),
                    new(real[i].X, real[i].Y, real[i].Z),
                    new(real[i + 1].X, real[i + 1].Y, real[i + 1].Z),
                    view[0],
                    view[i],
                    view[i + 1]), scene);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<float> InterpolateTexture(int i0, float d0, int i1, float d1)
        {
            if (i0 == i1)
            {
                return [d0];
            }

            var values = new List<float>();

            float a = (d1 - d0) / (i1 - i0);
            float d = d0;

            for (int i = i0; i < i1; i++)
            {
                values.Add(d);
                d += a;
            }
            values.Add(d);
            d += a;

            return values;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                (var z02, var z012) = TraingleInterpolation(A, 1 / triangle.A.Z, B, 1 / triangle.B.Z, C, 1 / triangle.C.Z);

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
                            if (z > ZBuffer[y, x])
                            {
                                ZBuffer[y, x] = z;
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

        private unsafe void MyRasterizeTriangleTexture(Triangle triangle, Scene scene, bool bloom = false)
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
                    (triangle.ViewB, triangle.ViewA) = (triangle.ViewA, triangle.ViewB);
                    (triangle.TextelB, triangle.TextelA) = (triangle.TextelA, triangle.TextelB);
                    (triangle.RealB, triangle.RealA) = (triangle.RealA, triangle.RealB);


                }
                if (triangle.C.Y < triangle.A.Y)
                {
                    (triangle.C, triangle.A) = (triangle.A, triangle.C);
                    (triangle.ViewC, triangle.ViewA) = (triangle.ViewA, triangle.ViewC);
                    (triangle.TextelC, triangle.TextelA) = (triangle.TextelA, triangle.TextelC);
                    (triangle.RealC, triangle.RealA) = (triangle.RealA, triangle.RealC);



                }
                if (triangle.C.Y < triangle.B.Y)
                {
                    (triangle.B, triangle.C) = (triangle.C, triangle.B);
                    (triangle.ViewB, triangle.ViewC) = (triangle.ViewC, triangle.ViewB);
                    (triangle.TextelB, triangle.TextelC) = (triangle.TextelC, triangle.TextelB);
                    (triangle.RealB, triangle.RealC) = (triangle.RealC, triangle.RealB);



                }
                int YA = (int)float.Round(triangle.A.Y, 0);
                int YB = (int)float.Round(triangle.B.Y, 0);
                int YC = (int)float.Round(triangle.C.Y, 0);

                float ZInvA = 1 / triangle.ViewA.W;
                float ZInvB = 1 / triangle.ViewB.W;
                float ZInvC = 1 / triangle.ViewC.W;


                (var x02, var x012) = TraingleInterpolation(YA, triangle.A.X, YB, triangle.B.X, YC, triangle.C.X);

                (var rx02, var rx012) = TraingleInterpolation(YA, triangle.RealA.X, YB, triangle.RealB.X, YC, triangle.RealC.X);
                (var ry02, var ry012) = TraingleInterpolation(YA, triangle.RealA.Y, YB, triangle.RealB.Y, YC, triangle.RealC.Y);
                (var rz02, var rz012) = TraingleInterpolation(YA, triangle.RealA.Z, YB, triangle.RealB.Z, YC, triangle.RealC.Z);

                (var vz02, var vz012) = TraingleInterpolation(YA, ZInvA, YB, ZInvB, YC, ZInvC);


                (var u02, var u012) = TraingleInterpolationTexture(YA, triangle.TextelA.X * ZInvA, YB, triangle.TextelB.X * ZInvB, YC, triangle.TextelC.X * ZInvC);
                (var v02, var v012) = TraingleInterpolationTexture(YA, triangle.TextelA.Y * ZInvA, YB, triangle.TextelB.Y * ZInvB, YC, triangle.TextelC.Y * ZInvC);

                var m = (int)float.Floor(x012.Count / 2.0f);
                List<float> x_left;
                List<float> x_right;
                List<float> u_left;
                List<float> u_right;
                List<float> v_left;
                List<float> v_right;
                List<float> vz_left;
                List<float> vz_right;


                List<float> rx_right, ry_right, rz_right;
                List<float> rx_left, ry_left, rz_left;

                if ((int)float.Round(x02[m]) <= (int)float.Round(x012[m]))
                {
                    (x_left, x_right) = (x02, x012);

                    (u_left, u_right) = (u02, u012);
                    (v_left, v_right) = (v02, v012);
                    (vz_left, vz_right) = (vz02, vz012);



                    (rx_left, rx_right) = (rx02, rx012);
                    (ry_left, ry_right) = (ry02, ry012);
                    (rz_left, rz_right) = (rz02, rz012);

                }
                else
                {
                    (x_left, x_right) = (x012, x02);

                    (u_left, u_right) = (u012, u02);
                    (v_left, v_right) = (v012, v02);
                    (vz_left, vz_right) = (vz012, vz02);



                    (rx_left, rx_right) = (rx012, rx02);
                    (ry_left, ry_right) = (ry012, ry02);
                    (rz_left, rz_right) = (rz012, rz02);

                }
                int YDiffTop = 0;
                int YDiffTopI = 0;
                int TopY = (int)float.Round(triangle.A.Y);
                if (triangle.A.Y < 0)
                {
                    YDiffTop = -(int)float.Round(triangle.A.Y);
                    YDiffTopI = (int)float.Round(triangle.C.Y);
                    TopY = 0;
                }
                for (int y = TopY; y <= (int)float.Round(triangle.C.Y); y++)
                {
                    if (y < 0 || y >= _height)
                        continue;
                    var index = (y - TopY + YDiffTop);
                    {
                        var xl = (int)float.Round(x_left[index]);
                        var xr = (int)float.Round(x_right[index]);


                        (var rxl, var rxr) = (rx_left[index], rx_right[index]);
                        (var ryl, var ryr) = (ry_left[index], ry_right[index]);
                        (var rzl, var rzr) = (rz_left[index], rz_right[index]);

                        (var vzl, var vzr) = (vz_left[index], vz_right[index]);

                        (var ul, var ur) = (u_left[index], u_right[index]);
                        (var vl, var vr) = (v_left[index], v_right[index]);
                        if (xl == xr)
                            continue;

                        float ku, kv, kvz, krx, kry, krz;
                        if (xl == xr)
                        {
                            ku = ul;
                            kv = vl;
                            kvz = vzl;
                            krx = rxl;
                            kry = ryl;
                            krz = rzl;
                        }
                        else
                        {
                            ku = (ul - ur) / (xl - xr);
                            kv = (vl - vr) / (xl - xr);
                            kvz = (vzl - vzr) / (xl - xr);
                            krx = (rxl - rxr) / (xl - xr);
                            kry = (ryl - ryr) / (xl - xr);
                            krz = (rzl - rzr) / (xl - xr);
                        }


                        for (int x = xl; x <= xr; x++)
                        {
                            if (x < 0 || x >= _width)
                                continue;
                            var z = (rzl + krz * (x - xl));
                            var vz = (vzl + kvz * (x - xl));

                            if (vz > ZBuffer[y, x])
                            {

                                ZBuffer[y, x] = vz;
                                byte* pixelPtr = data + y * _stride + x * 3;
                                var vertex = new Vector3(rxl + krx * (x - xl), ryl + kry * (x - xl), rzl + krz * (x - xl));
                                var tx = float.Abs(((ul + ku * (x - xl)) / vz) * scene.GraphicsObjects.KdMap.Width) % scene.GraphicsObjects.KdMap.Width;
                                var ty = float.Abs((1 - (vl + kv * (x - xl)) / vz) * scene.GraphicsObjects.KdMap.Height) % scene.GraphicsObjects.KdMap.Height;
                                int textureByteKd = (int)((1 - (vl + kv * (x - xl)) / vz) * scene.GraphicsObjects.KdMap.Height) * scene.GraphicsObjects.KdMap.Stride + (int)((ul + ku * (x - xl)) / vz * scene.GraphicsObjects.KdMap.Width) * scene.GraphicsObjects.KdMap.ColorSize / 8;
                                Vector3 newColor = new Vector3(scene.GraphicsObjects.KdMap.MapData[textureByteKd + 2] / 255.0f, scene.GraphicsObjects.KdMap.MapData[textureByteKd + 1] / 255.0f, scene.GraphicsObjects.KdMap.MapData[textureByteKd + 0] / 255.0f);
                                Vector3 lightResult, emissionColor;
                                if (scene.GraphicsObjects.NormMap != null)
                                {
                                    int textureByteNorm = (int)((1 - (vl + kv * (x - xl)) / vz) * scene.GraphicsObjects.NormMap.Height) * scene.GraphicsObjects.NormMap.Stride + (int)((ul + ku * (x - xl)) / vz * scene.GraphicsObjects.NormMap.Width) * scene.GraphicsObjects.NormMap.ColorSize / 8;
                                    int textureByteMrao = (int)((1 - (vl + kv * (x - xl)) / vz) * scene.GraphicsObjects.MraoMap.Height) * scene.GraphicsObjects.MraoMap.Stride + (int)((ul + ku * (x - xl)) / vz * scene.GraphicsObjects.MraoMap.Width) * scene.GraphicsObjects.MraoMap.ColorSize / 8;
                                    int textureByteEmi = (int)((1 - (vl + kv * (x - xl)) / vz) * scene.GraphicsObjects.EmiMap.Height) * scene.GraphicsObjects.EmiMap.Stride + (int)((ul + ku * (x - xl)) / vz * scene.GraphicsObjects.EmiMap.Width) * scene.GraphicsObjects.EmiMap.ColorSize / 8;
                                    Vector3 normal = new Vector3((scene.GraphicsObjects.NormMap.MapData[textureByteNorm + 2] / 255.0f) * 2 - 1, (scene.GraphicsObjects.NormMap.MapData[textureByteNorm + 1] / 255.0f) * 2 - 1, (scene.GraphicsObjects.NormMap.MapData[textureByteNorm + 0] / 255.0f) * 2 - 1);
                                    emissionColor = new Vector3((scene.GraphicsObjects.EmiMap.MapData[textureByteEmi + 0] / 255.0f), (scene.GraphicsObjects.EmiMap.MapData[textureByteEmi + 1] / 255.0f), (scene.GraphicsObjects.EmiMap.MapData[textureByteEmi + 2] / 255.0f));
                                    lightResult = new(0, 0, 0);
                                    for (int i = 0; i < scene.Light.Count; i++)
                                        lightResult += scene.Light[i].CalculateLightWithMaps(vertex, normal, scene.Camera.Eye, scene.GraphicsObjects.MraoMap.MapData[textureByteMrao + 0]);
                                }
                                else
                                {
                                    lightResult = new(0f, 0f, 0f);
                                    emissionColor = new(0f, 0f, 0f);
                                }
                                if (emissionColor.X > 0.0f || emissionColor.Y > 0.0f || emissionColor.Z > 0.0f)
                                {
                                    newColor += emissionColor;
                                    bloom_buffer[0][y *_width + x] = emissionColor*20;
                                }
                                newColor.X = ((newColor.X) * (lightResult.X > 1.0 ? 1 : lightResult.X));
                                newColor.Y = ((newColor.Y) * (lightResult.Y > 1 ? 1 : lightResult.Y));
                                newColor.Z = ((newColor.Z ) * (lightResult.Z > 1 ? 1 : lightResult.Z));
                                colors[y * _width + x] = newColor;
                            }
                        }
                    }
                }
            }
        }

        private Color CalculateColor(Vector3 point, Vector3 normal, Scene scene, Color baseColor)
        {
            var light = new Vector3(0, 0, 0);
            for (int i = 0; i < scene.Light.Count; i++)
                light += scene.Light[i].CalculateLightWithSpecular(point, normal, scene.Camera.Eye);
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

        private (List<float>, List<float>) TraingleInterpolationTexture(int y0, float v0, int y1, float v1, int y2, float v2)
        {
            var v01 = InterpolateTexture(y0, v0, y1, v1);
            var v12 = InterpolateTexture(y1, v1, y2, v2);
            var v02 = InterpolateTexture(y0, v0, y2, v2);
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
                    (triangle.ViewB, triangle.ViewA) = (triangle.ViewA, triangle.ViewB);
                    (triangle.RealB, triangle.RealA) = (triangle.RealA, triangle.RealB);
                }
                if (triangle.C.Y < triangle.A.Y)
                {
                    (triangle.C, triangle.A) = (triangle.A, triangle.C);
                    (triangle.NormalC, triangle.NormalA) = (triangle.NormalA, triangle.NormalC);
                    (triangle.ViewC, triangle.ViewA) = (triangle.ViewA, triangle.ViewC);
                    (triangle.RealC, triangle.RealA) = (triangle.RealA, triangle.RealC);

                }
                if (triangle.C.Y < triangle.B.Y)
                {
                    (triangle.B, triangle.C) = (triangle.C, triangle.B);
                    (triangle.NormalB, triangle.NormalC) = (triangle.NormalC, triangle.NormalB);
                    (triangle.ViewB, triangle.ViewC) = (triangle.ViewC, triangle.ViewB);
                    (triangle.RealB, triangle.RealC) = (triangle.RealC, triangle.RealB);

                }
                int YA = (int)float.Round(triangle.A.Y, 0);
                int YB = (int)float.Round(triangle.B.Y, 0);
                int YC = (int)float.Round(triangle.C.Y, 0);

                (var x02, var x012) = TraingleInterpolation(YA, triangle.A.X, YB, triangle.B.X, YC, triangle.C.X);
                (var z02, var z012) = TraingleInterpolation(YA, 1 / triangle.ViewA.Z, YB, 1 / triangle.ViewB.Z, YC, 1 / triangle.ViewC.Z);


                (var nx02, var nx012) = TraingleInterpolation(YA, triangle.NormalA.X, YB, triangle.NormalB.X, YC, triangle.NormalC.X);
                (var ny02, var ny012) = TraingleInterpolation(YA, triangle.NormalA.Y, YB, triangle.NormalB.Y, YC, triangle.NormalC.Y);
                (var nz02, var nz012) = TraingleInterpolation(YA, triangle.NormalA.Z, YB, triangle.NormalB.Z, YC, triangle.NormalC.Z);

                (var rx02, var rx012) = TraingleInterpolation(YA, triangle.RealA.X, YB, triangle.RealB.X, YC, triangle.RealC.X);
                (var ry02, var ry012) = TraingleInterpolation(YA, triangle.RealA.Y, YB, triangle.RealB.Y, YC, triangle.RealC.Y);
                (var rz02, var rz012) = TraingleInterpolation(YA, triangle.RealA.Z, YB, triangle.RealB.Z, YC, triangle.RealC.Z);

                var m = (int)float.Floor(x012.Count / 2.0f);
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
                int TopY = (int)float.Round(triangle.A.Y);
                if (triangle.A.Y < 0)
                {
                    YDiffTop = -(int)float.Round(triangle.A.Y);
                    YDiffTopI = (int)float.Round(triangle.C.Y);
                    TopY = 0;
                }
                for (int y = TopY; y <= (int)float.Round(triangle.C.Y); y++)
                {
                    if (y < 0 || y >= _height)
                        continue;
                    var index = (y - TopY + YDiffTop);
                    //if (index < x_left.Count && index < x_right.Count)
                    {
                        var xl = (int)float.Round(x_left[index]);
                        var xr = (int)float.Round(x_right[index]);
                        var zl = z_left[index];
                        var zr = z_right[index];
                        (var nxl, var nxr) = (nx_left[index], nx_right[index]);
                        (var nyl, var nyr) = (ny_left[index], ny_right[index]);
                        (var nzl, var nzr) = (nz_left[index], nz_right[index]);

                        (var rxl, var rxr) = (rx_left[index], rx_right[index]);
                        (var ryl, var ryr) = (ry_left[index], ry_right[index]);
                        (var rzl, var rzr) = (rz_left[index], rz_right[index]);
                        var zscan = Interpolate(xl, zl, xr, zr);
                        if (zscan.Count == 0)
                            continue;

                        var nxscan = Interpolate(xl, nxl, xr, nxr);
                        var nyscan = Interpolate(xl, nyl, xr, nyr);
                        var nzscan = Interpolate(xl, nzl, xr, nzr);
                        var rxscan = Interpolate(xl, rxl, xr, rxr);
                        var ryscan = Interpolate(xl, ryl, xr, ryr);
                        var rzscan = Interpolate(xl, rzl, xr, rzr);


                        for (int x = xl; x <= xr; x++)
                        {
                            if (x < 0 || x >= _width)
                                continue;
                            var z = zscan[x - xl];


                            //lock (SpincLocker[y][x])
                            //{
                            if (z > ZBuffer[y, x])
                            {
                                ZBuffer[y, x] = z;
                                byte* pixelPtr = data + y * _stride + x * 3;
                                var vertex = new Vector3(rxscan[x - xl], ryscan[x - xl], rzscan[x - xl]);
                                var normal = new Vector3(nxscan[x - xl], nyscan[x - xl], nzscan[x - xl]);
                                Color color = CalculateColor(vertex, Vector3.Normalize(normal), scene, baseColor);
                                *pixelPtr++ = color.B;
                                *pixelPtr++ = color.G;
                                *pixelPtr = color.R;
                            }
                            //}


                        }

                    }


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

        public void Draw(bool _bloom = false)
        {
            unsafe
            {
                if(_bloom)
                    bloom();
                byte* data = (byte*)Buffer.ToPointer();
                Parallel.For(0, _height, y =>
                {
                    if (!(y < 0 || y >= _height))
                    { 
                        for (int x = 0; x <= _width; x++)
                        {
                            if (x < 0 || x >= _width)
                                continue;
                            byte* pixelPtr = data + y * _stride + x * 3;
                            int num = y * _stride + x;
                            const float gamma = 1.0f / 2.2f;
                            const float exposure = 1.0f;
                            Vector3 newColor;
                            if (_bloom)
                                newColor = colors[y * _width + x] + bloom_buffer[1][y * _width + x];
                            else
                                newColor = colors[y * _width + x];
                            Vector3 mapped = new Vector3((float)(1.0f - Math.Exp(newColor.X * (-exposure))), (float)(1.0f - Math.Exp(newColor.Y * (-exposure))), (float)(1.0f - Math.Exp(newColor.Z * (-exposure))));
                            mapped = new Vector3((float)Math.Pow(mapped[0], gamma), (float)Math.Pow(mapped[1], gamma), (float)Math.Pow(mapped[2], gamma));
                            *pixelPtr++ = (byte)((mapped.X * 255));
                            *pixelPtr++ = (byte)((mapped.Y * 255));
                            *pixelPtr = (byte)((mapped.Z * 255));
                        }
                    }
                });
            }
        }

        public void downsampling4x(ref Vector3[] buf, ref Vector3[] bufOut, int buf_width, int buf_height)
        {
            for (int y = 0; y < buf_height; y++)
            {
                for (int x = 0; x < buf_width; x++)
                {
                    int newInd = x + y * width;
                    int ind = newInd * 2;
                    bufOut[newInd] = (buf[ind] + buf[ind + 1] + buf[ind + width] + buf[ind + 1 + width]) / 4;
                    newInd++;
                }
            }
        }

        public Vector3 texture(Vector3[] buf, Vector2 coords, Vector2 scale)
        {
            if (coords.X < 0 || coords.Y < 0 || coords.X >= scale.X || coords.Y >= scale.Y)
            {
                return new Vector3(0, 0, 0);
            }
            return buf[(int)(coords.X + coords.Y * width)];
        }

        public void guassian(ref Vector3[] buf, ref Vector3[] bufOut, Vector2 TexCoords, bool horizontal, Vector2 new_scale)
        {
            Vector3 result = texture(buf, TexCoords, new_scale) * weight[0];
            int tempY = (int)(TexCoords.Y * width);

            if (horizontal)
            {
                for (int i = 1; i < 5; ++i)
                {
                    if (TexCoords.X + i < width)
                    {
                        result = result + buf[(int)(TexCoords.X + i + tempY)] * weight[i];
                    }
                    if (TexCoords.X - i >= 0)
                    {
                        result = result + buf[(int)(TexCoords.X - i + tempY)] * weight[i];
                    }
                }
            }
            else
            {
                for (int i = 1; i < 5; ++i)
                {
                    if (TexCoords.Y + i < height)
                    {
                        result = result + buf[(int)(TexCoords.X + (TexCoords.Y + i) * width)] * weight[i];
                    }
                    if (TexCoords.Y - i >= 0)
                    {
                        result = result + buf[(int)(TexCoords.X + (TexCoords.Y - i) * width)] * weight[i];
                    }
                }
            }
            bufOut[(int)(TexCoords.X + tempY)] = result;
        }

        public void addBuffers(ref Vector3[] bufA, ref Vector3[] bufB, int buf_width, int buf_height)
        {
            for (int x = 0; x < buf_width; x++)
            {
                for (int y = 0; y < buf_height; y++)
                {
                    int ind = x + y * width;
                    bufA[ind] = bufA[ind] + bufB[ind];
                }
            }
        }

        public void upsampling4x(ref Vector3[] buf, ref Vector3[] bufOut, int buf_width, int buf_height)
        {

            for (int ind = 0; ind < width * height; ind++)
            {
                bufOut[ind] = new Vector3(0.0f, 0.0f, 0.0f);
            }

            for (int x = 0; x < buf_width; x++)
            {
                for (int y = 0; y < buf_height; y++)
                {
                    int ind = x + y * width;
                    bufOut[ind * 2] = buf[ind];
                }
            }

            int amount = 2;

            Vector2 new_scale = new(buf_width* 2, buf_height * 2);

            for (int j = 0; j < amount; j++)
            {
                for (int x = 0; x < new_scale.X; x++)
                {
                    for (int y = 0; y < new_scale.Y; y++)
                    {
                        var sourceImage1 = first_iteration ? bufOut : (horizontal ? bufOut : buf);
                        var sourceImage2 = horizontal ? buf : bufOut;
                        guassian(ref sourceImage1, ref sourceImage2, new Vector2(x, y), horizontal, new_scale);

                        if (horizontal)
                            buf = sourceImage2;
                        else
                            bufOut = sourceImage2;

                        if (first_iteration)
                        {
                            bufOut = sourceImage1;
                        }
                        else
                        {
                            if (horizontal)
                                bufOut = sourceImage1;
                            else
                                buf = sourceImage1;
                        }
                    }
                }
            
                horizontal = !horizontal;
                if (first_iteration)
                    first_iteration = false;
            }
        }

        public void bloom() 
        {
            int downscaling_iterations_amount = 0;
            Vector2 new_scale = new(width, height);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < 3; i++)
            {

                if (new_scale.X % 2 == 1 || new_scale.Y % 2 == 1) break;
                downscaling_iterations_amount++;

                new_scale.X /= 2;
                new_scale.Y /= 2;
                if (i == 0)
                {
                    var buf = bloom_buffer[0];
                    var bufOut = bloom_buffer[0];
                    downsampling4x(ref buf, ref bufOut, (int)new_scale.X, (int)new_scale.Y);
                    bloom_buffer[0] = bufOut;
                }
                else
                {
                    var buf = bloom_buffer[2 * (i - 1)];
                    var bufOut = bloom_buffer[2 * i];
                    downsampling4x(ref buf, ref bufOut, (int)new_scale.X, (int)new_scale.Y);
                    bloom_buffer[2 * i] = bufOut;
                }

                bool horizontal = true, first_iteration = true;
                int amount = 4;

                for (int j = 0; j < amount; j++)
                {
                    Parallel.For(0, (int)new_scale.X, x => {
                        for (int y = 0; y < new_scale.Y; y++)
                        {
                            var buf = first_iteration ? bloom_buffer[2 * i] : bloom_buffer[2 * i + (!horizontal ? 1 : 0)];
                            var bufOut = bloom_buffer[2 * i + (horizontal ? 1 : 0)];
                            var destinationImage = bloom_buffer[2 * i + (horizontal ? 1 : 0)];
                            guassian(ref buf, ref bufOut, new Vector2(x, y), horizontal, new_scale);
                            bloom_buffer[2 * i + (horizontal ? 1 : 0)] = bufOut;
                        }
                    });
                    horizontal = !horizontal;
                    if (first_iteration)
                        first_iteration = false;
                }
            }
            stopwatch.Stop();
            Stopwatch stopwatch1 = new Stopwatch();
            stopwatch1.Start();
            for (int i = downscaling_iterations_amount - 1; i > 0; i--)
            {
                var buf = bloom_buffer[i * 2];
                var bufOut = bloom_buffer[i * 2 - 1];
                upsampling4x(ref buf, ref bufOut, (int)new_scale.X, (int)new_scale.Y);
                bloom_buffer[i * 2] = buf;
                bloom_buffer[i * 2 - 1] = bufOut;
                new_scale.X *= 2;
                new_scale.Y *= 2;
                var bufA = bloom_buffer[(i - 1) * 2];
                var bufB = bloom_buffer[i * 2 - 1];
                addBuffers(ref bufA, ref bufB, (int)new_scale.X, (int)new_scale.Y);
                bloom_buffer[(i - 1) * 2] = bufA;
                bloom_buffer[i * 2 - 1] = bufB;
            }
            var buf1 = bloom_buffer[0];
            var bufOut2 = bloom_buffer[1];
            upsampling4x(ref buf1, ref bufOut2, (int)new_scale.X, (int)new_scale.Y);
            bloom_buffer[0] = buf1;
            bloom_buffer[1] = bufOut2;
            stopwatch1.Stop();
        }
    }
}
