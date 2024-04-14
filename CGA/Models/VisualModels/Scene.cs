using System.Collections.Generic;
using System.Numerics;
using ObjVisualizer.GraphicsComponents;
using ObjVisualizer.MathModule;

namespace ObjVisualizer.Models.VisualModels
{
    internal class Scene
    {
        public Camera Camera { get; set; }

        public Matrix4x4 ModelMatrix;
        public Matrix4x4 ViewMatrix;
        public Matrix4x4 ProjectionMatrix;
        public Matrix4x4 ViewPortMatrix;

        public bool ChangeStatus { get; set; }

        private static Scene? Instance;

        public List<PointLight> Light;

        public bool Ambient { get => _ambient; set { _ambient = value; } }
        public bool Specular { get => _specular; set { _specular = value; } }

        private bool _ambient;
        private bool _specular;

        public Stage Stage;

        public GraphicsObject GraphicsObjects = null!;

        private Matrix4x4 RotateMatrix;
        private Matrix4x4 ScaleMatrix;
        private Matrix4x4 MoveMatrix;

        private Scene()
        {
            ModelMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
            ViewMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
            ProjectionMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
            ViewPortMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
            RotateMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
            ScaleMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
            MoveMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
            Camera = new Camera(Vector3.Zero, Vector3.Zero, Vector3.Zero, 0, 0, 0, 0);
            Light = [];
            ChangeStatus = true;
        }

        public static Scene GetScene()
        {
            Instance ??= new Scene();

            return Instance;
        }

        public void UpdateViewMatix()
        {
            ViewMatrix = Matrix4x4.CreateLookAt(Camera.Eye, Camera.Target, Camera.Up);
            //ViewMatrix = Matrix4x4.Transpose(MatrixOperator.GetViewMatrix(Camera));
        }

        public void SceneResize(int NewWindowWidth, int NewWindowHeight)
        {
            Camera.ChangeCameraAspect(NewWindowWidth, NewWindowHeight);
            ViewPortMatrix = Matrix4x4.Transpose(MatrixOperator.GetViewPortMatrix(NewWindowWidth, NewWindowHeight));

        }

        public Vector4 GetTransformedVertex(Vector4 Vertex, out bool isOut)
        {
            Vertex = Vector4.Transform(Vertex, ViewMatrix);
            Vertex = Vector4.Transform(Vertex, ProjectionMatrix);

            if (Vertex.W < 0)
            {
                isOut = false;
            }
            else
                isOut = true;
            Vertex = Vector4.Divide(Vertex, Vertex.W);
            Vertex = Vector4.Transform(Vertex, ViewPortMatrix);

            return Vertex;
        }
        public Vector4 GetViewVertex(Vector4 Vertex)
        {
            Vertex = Vector4.Transform(Vertex, ViewMatrix);
            Vertex = Vector4.Transform(Vertex, ProjectionMatrix);
            var W = Vertex.Z;
            var temp = Vector4.Divide(Vertex, Vertex.W);
            return new Vector4(temp.X, temp.Y, temp.Z, W);

        }

        public Vector4 GetTransformedVertex(Vector4 Vertex)
        {
            Vertex = Vector4.Transform(Vertex, ViewMatrix);
            Vertex = Vector4.Transform(Vertex, ProjectionMatrix);
            Vertex = Vector4.Divide(Vertex, Vertex.W);
            Vertex = Vector4.Transform(Vertex, ViewPortMatrix);

            return Vertex;
        }

        public void UpdateViewMatrix()
        {
            ViewMatrix = Matrix4x4.Transpose(MatrixOperator.GetViewMatrix(Camera));
        }

        public void ResetTransformMatrixes()
        {
            RotateMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
            MoveMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
            ScaleMatrix = Matrix4x4.Transpose(Matrix4x4.Identity);
        }
    }

    public enum Stage
    {
        Stage1,
        Stage2,
        Stage3,
        Stage4,
        Stage5
    }
}
