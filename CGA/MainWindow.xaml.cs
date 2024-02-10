using CGA.Models.DataModels;
using CGA.Models.VisualModels;
using CGA.Parser;
using System.Windows;
using System.Windows.Media.Imaging;
using CGA.MyGraphics;
using System.Numerics;
using System.Windows.Media.Media3D;
using Camera = CGA.Models.VisualModels.Camera;
using CGA.MathModule;
using System.Windows.Input;
using System.Diagnostics;
using System.Collections;
using System.Windows.Controls;

namespace CGA
{
    public partial class MainWindow : Window
    {
        private WriteableBitmap wBitmap;
        private OBJParser parser;
        private Mesh entity;
        private Screen screen;
        private Camera camera;
        private Graphics renderer;
        private ArrayList objects = new ArrayList() { "ball.obj", "ship.obj", "airplane.obj", "teapot.obj", "cat.obj", "fish.obj", "shuttle.obj" };

        private Matrix4x4 WorldModel = Matrix4x4.Identity;

        public MainWindow()
        {
            InitializeComponent();
            entity = Mesh.loadMesh("geo.obj");
            object_selector.ItemsSource = objects;
            vertexes.Content = "Vertex: " + entity.getVertexes().Count.ToString();
            faces.Content = "Faces: " + entity.getFaces().Count.ToString();
            var eye = new Vector3(0, 0, 1);
            var target = new Vector3(0, 0, 0);
            var up = new Vector3(0, 1, 0);
            screen = new Screen(700, 700);
            camera = new Camera(120, 700 / 700, 0.1f, 10, eye, target, up);

            var scale = 0.05f;
            
            /*WorldModel = Matrixes.Movement(target) * Matrixes.Scale(new Vector3(scale, scale, scale)) *
                         camera.GetMatrix() *
                         Matrixes.Projection(camera.FOV, camera.Aspect, camera.zNear, camera.zFar) *
                         screen.GetMatrix();*/

            WorldModel = Matrixes.Scale(new Vector3(scale, scale, scale)) *
                         camera.GetMatrix() *
                         Matrixes.Projection(camera.FOV, camera.Aspect, camera.zNear, camera.zFar) *
                         screen.GetMatrix();

            renderer = new Graphics(img);
            renderer.DrawEntityMesh(WorldModel, entity, screen.Width, screen.Height);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            bool draw = true;
            switch (e.Key)
            {
                case Key.D:
                    UpdateWorldModel(Matrixes.RotateZ(5));
                    break;
                case Key.A:
                    UpdateWorldModel(Matrixes.RotateZ(-5));
                    break;
                case Key.W:
                    UpdateWorldModel(Matrixes.RotateX(5));
                    break;
                case Key.S:
                    UpdateWorldModel(Matrixes.RotateX(-5));
                    break;
                case Key.Q:
                    UpdateWorldModel(Matrixes.RotateY(5));
                    break;
                case Key.E:
                    UpdateWorldModel(Matrixes.RotateY(-5));
                    break;
                case Key.Add or Key.OemPlus:
                    var scale = 1.1f;
                    UpdateWorldModel(Matrixes.Scale(new Vector3(scale, scale, scale)));
                    break;
                case Key.Subtract or Key.OemMinus:
                    scale = 0.9f;
                    UpdateWorldModel(Matrixes.Scale(new Vector3(scale, scale, scale)));
                    break;
                case Key.Left:
                    UpdateWorldModel(Matrixes.Movement(new Vector3(0.3f, 0, 0)));
                    break;
                case Key.Right:
                    UpdateWorldModel(Matrixes.Movement(new Vector3(-0.3f, 0, 0)));
                    break;
                case Key.Up:
                    UpdateWorldModel(Matrixes.Movement(new Vector3(0, 0.3f, 0)));
                    break;
                case Key.Down:
                    UpdateWorldModel(Matrixes.Movement(new Vector3(0, -0.3f, 0)));
                    break;
                default:
                    draw = false;
                    break;
            }
            if (draw)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                renderer.DrawEntityMesh(WorldModel, entity, 700, 700);
                stopwatch.Stop();
                ms.Content = "Time: " + stopwatch.ElapsedMilliseconds.ToString() + "ms";
            }
        }

        public void UpdateWorldModel(Matrix4x4 m)
        {
            WorldModel = m * WorldModel;
        }

        private void object_selector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            /*string path = (sender as ComboBox).SelectedValue.ToString();
            entity = Mesh.loadMesh(path);
            object_selector.ItemsSource = objects;
            vertexes.Content = "Vertex: " + entity.getVertexes().Count.ToString();
            faces.Content = "Faces: " + entity.getFaces().Count.ToString();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            renderer.DrawEntityMesh(WorldModel, entity, 700, 700);
            stopwatch.Stop();*/
        }
    }
}
