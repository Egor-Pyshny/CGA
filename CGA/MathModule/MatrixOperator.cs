using System;
using System.Numerics;
using ObjVisualizer.Models.VisualModels;

namespace ObjVisualizer.MathModule
{
    internal class MatrixOperator
    {
        public static Matrix4x4 Scale(Vector3 scale) =>
            new(scale.X, 0, 0, 0,
                0, scale.Y, 0, 0,
                0, 0, scale.Z, 0,
                0, 0, 0, 1.0f);

        public static Matrix4x4 Move(Vector3 transition) =>
            new(1f, 0, 0, transition.X,
                0, 1f, 0, transition.Y,
                0, 0, 1f, transition.Z,
                0, 0, 0, 1.0f);

        public static Matrix4x4 GetModelMatrix() =>
            new(1.0f, 0, 0, 0,
                 0, 1.0f, 0, 0,
                 0, 0, 1.0f, 0,
                 0, 0, 0, 1.0f);

        public static Matrix4x4 RotateX(double angle) =>
            new(1.0f, 0, 0, 0,
                0, (float)Math.Cos(angle), -(float)Math.Sin(angle), 0,
                0, (float)Math.Sin(angle), (float)Math.Cos(angle), 0,
                0, 0, 0, 1.0f);

        public static Matrix4x4 RotateY(double angle) =>
            new((float)Math.Cos(angle), 0, (float)Math.Sin(angle), 0,
                0, 1.0f, 0, 0,
                -(float)Math.Sin(angle), 0, (float)Math.Cos(angle), 0,
                0, 0, 0, 1.0f);

        public static Matrix4x4 RotateZ(double angle) =>
            new((float)Math.Cos(angle), -(float)Math.Sin(angle), 0, 0,
                (float)Math.Sin(angle), 0, (float)Math.Cos(angle), 0,
                0, 1.0f, 0, 0,
                0, 0, 0, 1.0f);

        public static Matrix4x4 GetViewMatrix(Camera camera)
        {
            Vector3 ZAxis = Vector3.Normalize(Vector3.Subtract(camera.Eye, camera.Target));
            Vector3 XAxis = Vector3.Normalize(Vector3.Cross(camera.Up, ZAxis));
            Vector3 YAxis = Vector3.Normalize(Vector3.Cross(ZAxis, XAxis));


            return new Matrix4x4(XAxis.X, XAxis.Y, XAxis.Z, -Vector3.Dot(XAxis, camera.Eye),
                                 YAxis.X, YAxis.Y, YAxis.Z, -Vector3.Dot(YAxis, camera.Eye),
                                 ZAxis.X, ZAxis.Y, ZAxis.Z, -Vector3.Dot(ZAxis, camera.Eye),
                                 0, 0, 0, 1.0f);
        }

        public static Matrix4x4 GetProjectionMatrix(Camera camera) =>
            new(1.0f / (camera.Aspect * (float)Math.Tan(camera.FOV / 2.0f)), 0, 0, 0,
                0, 1.0f / (float)Math.Tan(camera.FOV / 2.0f), 0, 0,
                0, 0, camera.ZFar / (camera.ZNear - camera.ZFar), camera.ZNear * camera.ZFar / (camera.ZNear - camera.ZFar),
                0, 0, -1.0f, 0);

        public static Matrix4x4 GetViewPortMatrix(int Width, int Height) =>
            new(Width >> 1, 0, 0, Width >> 1,
                0, -(Height >> 1), 0, Height >> 1,
                0, 0, 1.0f, 0,
                0, 0, 0, 1.0f);
    }
}
