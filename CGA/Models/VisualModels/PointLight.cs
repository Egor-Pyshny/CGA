using System.Numerics;

namespace ObjVisualizer.Models.VisualModels
{
    internal readonly struct PointLight(float x, float y, float z, float intency)
    {
        public readonly float X = x;
        public readonly float Y = y;
        public readonly float Z = z;
        public readonly float Intency = intency;

        public float CalculateLight(Vector3 point, Vector3 normal)
        {
            Vector3 l = new Vector3(X, Y, Z) - point;

            float lightResult = 0.0f;
            float angle = Vector3.Dot(normal, l);

            if (angle > 0)
            {
                lightResult = Intency * angle / (l.Length() * normal.Length());
            }

            return lightResult;
        }
    }
}
