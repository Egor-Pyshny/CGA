using ObjVisualizer.Graphics;
using ObjVisualizer.MathModule;
using ObjVisualizer.Models.VisualModels;
using ObjVisualizer.Parser;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using System.Linq;

namespace CGA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Scene MainScene;
        private readonly Image Image;

        private readonly IObjReader Reader;

        private int WindowWidth;
        private int WindowHeight;
        private List<Vector4> Vertexes;
        private List<Vector3> Normales;
        public MainWindow()
        {
            InitializeComponent();

            Reader = ObjReader.GetObjReader("c.obj");
            Vertexes = Reader.Vertices.ToList();
            Normales = Reader.VertexNormals.ToList();
            vertexes.Content = $"Vertexes: {Vertexes.Count}";
            faces.Content = $"Faces: {Reader.Faces.Count()}";

            PreviewMouseWheel += MainWindow_PreviewMouseWheel;
            PreviewKeyDown += MainWindow_PreviewKeyDown;

            WindowWidth = (int)this.Width;
            WindowHeight = (int)this.Height;

            Image = this.img;

            MainScene = Scene.GetScene();
            MainScene.Stage = Scene.LabaStage.Laba1;

            MainScene.Camera = new Camera(new Vector3(0, 0f, 0f), new Vector3(0, 1, 0), new Vector3(0, 0, 1),
                WindowWidth / (float)WindowHeight, 70.0f * ((float)Math.PI / 180.0f), 10.0f, 0.1f);

            MainScene.Camera.Eye = new Vector3(
                        MainScene.Camera.Radius * (float)Math.Cos(MainScene.Camera.CameraPhi) * (float)Math.Sin(MainScene.Camera.CameraZeta),
                        MainScene.Camera.Radius * (float)Math.Cos(MainScene.Camera.CameraZeta),
                        MainScene.Camera.Radius * (float)Math.Sin(MainScene.Camera.CameraPhi) * (float)Math.Sin(MainScene.Camera.CameraZeta));
            MainScene.Light = new PointLight(MainScene.Camera.Eye.X, MainScene.Camera.Eye.Y, MainScene.Camera.Eye.Z, 0.8f);
            MainScene.ViewMatrix = Matrix4x4.Transpose(MatrixOperator.GetViewMatrix(MainScene.Camera));
            MainScene.ProjectionMatrix = Matrix4x4.Transpose(MatrixOperator.GetProjectionMatrix(MainScene.Camera));
            MainScene.ViewPortMatrix = Matrix4x4.Transpose(MatrixOperator.GetViewPortMatrix(WindowWidth, WindowHeight));

            Redraw();
        }

        private void Redraw()
        {
            var writableBitmap = new WriteableBitmap((int)img.Width, (int)img.Height, 96, 96, PixelFormats.Bgr24, null);
            var rect = new Int32Rect(0, 0, (int)img.Width, (int)img.Height);
            IntPtr buffer = writableBitmap.BackBuffer;
            int stride = writableBitmap.BackBufferStride;

            MainScene.Camera.Eye = new Vector3(
                    MainScene.Camera.Radius * (float)Math.Cos(MainScene.Camera.CameraPhi) *
                    (float)Math.Sin(MainScene.Camera.CameraZeta),
                    MainScene.Camera.Radius * (float)Math.Cos(MainScene.Camera.CameraZeta),
                    MainScene.Camera.Radius * (float)Math.Sin(MainScene.Camera.CameraPhi) *
                    (float)Math.Sin(MainScene.Camera.CameraZeta));
            MainScene.Light = new PointLight(MainScene.Camera.Eye.X, MainScene.Camera.Eye.Y, MainScene.Camera.Eye.Z, 0.8f);

            MainScene.UpdateViewMatix();

            var drawer = new Drawer((int)img.Width, (int)img.Height, buffer, stride);

            writableBitmap.Lock();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (MainScene.Stage == Scene.LabaStage.Laba1)
            {
                DrawLab1(buffer, drawer, stride);
            }
            else if (MainScene.Stage == Scene.LabaStage.Laba2)
            {
                DrawLab2(buffer, drawer);
            }
            writableBitmap.AddDirtyRect(rect);
            writableBitmap.Unlock();
            stopwatch.Stop();
            ms.Content = $"Time: {stopwatch.ElapsedMilliseconds}ms";
            MainScene.ModelMatrix = Matrix4x4.Transpose(MatrixOperator.GetModelMatrix());
            MainScene.ChangeStatus = false;

            Image.Source = writableBitmap;
        }

        private void DrawLab1(IntPtr buffer, Drawer drawer, int stride)
        {
            unsafe
            {
                byte* pixels = (byte*)buffer.ToPointer();
                Parallel.ForEach(Reader.Faces, face =>
                {
                    var FaceVertexes = face.VertexIds.ToList();
                    var FaceNormales = face.NormalIds.ToList();
                    var ZeroVertext = Vertexes[FaceVertexes[0] - 1];

                    Vector3 PoliNormal = Vector3.Zero;
                    if (MainScene.Stage == Scene.LabaStage.Laba1)
                    {
                        Vector4 TempVertexI = MainScene.GetTransformedVertex(Vertexes[FaceVertexes[0] - 1]);
                        Vector4 TempVertexJ = MainScene.GetTransformedVertex(Vertexes[FaceVertexes.Last() - 1]);

                        if ((int)TempVertexI.X > 0 && (int)TempVertexJ.X > 0 &&
                                    (int)TempVertexI.Y > 0 && (int)TempVertexJ.Y > 0 &&
                                    (int)TempVertexI.X < WindowWidth && (int)TempVertexJ.X < WindowWidth &&
                                    (int)TempVertexI.Y < WindowHeight && (int)TempVertexJ.Y < WindowHeight)
                        {
                            drawer.DrawLine((int)TempVertexI.X, (int)TempVertexI.Y, (int)TempVertexJ.X, (int)TempVertexJ.Y,
                                pixels, stride);
                        }

                        for (int i = 0; i < FaceVertexes.Count - 1; i++)
                        {
                            TempVertexI = MainScene.GetTransformedVertex(Vertexes[FaceVertexes[i] - 1]);
                            TempVertexJ = MainScene.GetTransformedVertex(Vertexes[FaceVertexes[i + 1] - 1]);

                            if ((int)TempVertexI.X > 0 && (int)TempVertexJ.X > 0 &&
                                (int)TempVertexI.Y > 0 && (int)TempVertexJ.Y > 0 &&
                                (int)TempVertexI.X < WindowWidth && (int)TempVertexJ.X < WindowWidth &&
                                (int)TempVertexI.Y < WindowHeight && (int)TempVertexJ.Y < WindowHeight)
                            {
                                drawer.DrawLine((int)TempVertexI.X, (int)TempVertexI.Y, (int)TempVertexJ.X, (int)TempVertexJ.Y,
                                    pixels, stride);
                            }
                        }
                    }
                });
            }
        }

        private void DrawLab2(IntPtr buffer, Drawer drawer)
        {
            unsafe
            {
                byte* pixels = (byte*)buffer.ToPointer();
                Parallel.ForEach(Reader.Faces, face =>
                {
                    var FaceVertexes = face.VertexIds.ToList();
                    var FaceNormales = face.NormalIds.ToList();
                    var ZeroVertext = Vertexes[FaceVertexes[0] - 1];
                    Vector3 PoliNormal = Vector3.Zero;
                    for (int i = 0; i < FaceNormales.Count; i++)
                    {
                        PoliNormal += Normales[FaceNormales[i] - 1];
                    }
                    if (MainScene.Stage == Scene.LabaStage.Laba2)
                    {
                        if (Vector3.Dot(PoliNormal / FaceNormales.Count, -new Vector3(Vertexes[FaceVertexes[0] - 1].X,
                        Vertexes[FaceVertexes[0] - 1].Y, Vertexes[FaceVertexes[0] - 1].Z) + MainScene.Camera.Eye) > 0)
                        {
                            var triangle = Enumerable.Range(0, FaceVertexes.Count)
                                .Select(i => MainScene.GetTransformedVertex(Vertexes[FaceVertexes[i] - 1]))
                                .ToList();
                            float light = MainScene.Light.CalculateLight(new Vector3(Vertexes[FaceVertexes[0] - 1].X,
                                Vertexes[FaceVertexes[0] - 1].Y, Vertexes[FaceVertexes[0] - 1].Z), PoliNormal);
                            drawer.Rasterize(triangle,
                                Color.FromArgb(
                                    (byte)(light * 0 > 255 ? 0 : light * 0),
                                    (byte)(light * 255 > 255 ? 255 : light * 255),
                                    (byte)(light * 0 > 255 ? 0 : light * 0)));
                        }
                    }
                });
            }
        }

        private void MainWindow_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MainScene.Camera.Radius += -e.Delta / 50;
            if (MainScene.Camera.Radius < 0.01f)
                MainScene.Camera.Radius = 0.01f;
            if (MainScene.Camera.Radius > 5 * MainScene.Camera.Radius)
                MainScene.Camera.Radius = 5 * MainScene.Camera.Radius;
            e.Handled = true;
            Redraw();
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool redraw = true;
            switch (e.Key)
            {
                case Key.D1:
                    MainScene.Stage = Scene.LabaStage.Laba1;
                    break;
                case Key.D2:
                    MainScene.Stage = Scene.LabaStage.Laba2;
                    break;
                case Key.A:
                    MainScene.Camera.Target += new Vector3(-1f, 0, 0);
                    break;
                case Key.D:
                    MainScene.Camera.Target += new Vector3(1f, 0, 0);
                    break;
                case Key.S:
                    MainScene.Camera.Target += new Vector3(0, -1f, 0);
                    break;
                case Key.W:
                    MainScene.Camera.Target += new Vector3(0, 1f, 0);
                    break;
                case Key.Up:
                    {
                        float yoffset = 17;
                        MainScene.Camera.CameraZeta += yoffset * 0.005f;
                        if (MainScene.Camera.CameraZeta > Math.PI)
                            MainScene.Camera.CameraZeta = (float)Math.PI - 0.01f;
                        if (MainScene.Camera.CameraZeta < 0)
                            MainScene.Camera.CameraZeta = 0.01f;
                        break;
                    }
                case Key.Down:
                    {
                        float yoffset = -17;
                        MainScene.Camera.CameraZeta += yoffset * 0.005f;
                        if (MainScene.Camera.CameraZeta > Math.PI)
                            MainScene.Camera.CameraZeta = (float)Math.PI - 0.01f;
                        if (MainScene.Camera.CameraZeta < 0)
                            MainScene.Camera.CameraZeta = 0.01f;
                        break;
                    }
                case Key.Right:
                    {
                        float xoffset = -17;
                        MainScene.Camera.CameraPhi += xoffset * 0.005f;
                        break;
                    }
                case Key.Left:
                    {
                        float xoffset = 17;
                        MainScene.Camera.CameraPhi += xoffset * 0.005f;
                        break;
                    }
                default:
                    redraw = false;
                    break;
            }
            if (redraw)
            {
                Redraw();
            }
        }
    }
}