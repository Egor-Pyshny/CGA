using System.Numerics;

namespace CGA.Models.DataModels
{
    public readonly struct LineSegment(Vector4 left, Vector4 right)
    {
        public readonly Vector4 Left = left;
        public readonly Vector4 Right = right;
    }
}
