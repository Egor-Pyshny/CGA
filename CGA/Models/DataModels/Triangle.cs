using System;
using System.Collections.Generic;
using System.Numerics;

namespace CGA.Models.DataModels
{
    public struct Triangle(Vector4 a, Vector4 b, Vector4 c)
    {
        public Vector4 A { get; set; } = a;
        public Vector4 B { get; set; } = b;
        public Vector4 C { get; set; } = c;

        public readonly IEnumerable<LineSegment> GetHorizontalLines()
        {
            float minY = Math.Min(Math.Min(A.Y, B.Y), C.Y);
            float maxY = Math.Max(Math.Max(A.Y, B.Y), C.Y);

            for (float y = minY; y <= maxY; y += 1f)
            {
                yield return FindIntersectingSegmentY(A, B, C, y);
            }
        }

        public readonly IEnumerable<LineSegment> GetVerticalLines()
        {
            float minX = Math.Min(Math.Min(A.X, B.X), C.X);
            float maxX = Math.Max(Math.Max(A.X, B.X), C.X);

            for (float x = minX; x <= maxX; x += 1f)
            {
                yield return FindIntersectingSegmentX(A, B, C, x);
            }
        }

        public readonly LineSegment FindIntersectingSegmentX(Vector4 point1, Vector4 point2, Vector4 point3, float x)
        {
            Vector4[] trianglePoints = [point1, point2, point3];
            Vector4 leftPoint = Vector4.Zero;
            Vector4 rightPoint = Vector4.Zero;

            for (int i = 0; i < 3; i++)
            {
                Vector4 currentPoint = trianglePoints[i];
                Vector4 nextPoint = trianglePoints[(i + 1) % 3];

                if ((currentPoint.X <= x && nextPoint.X >= x) || (currentPoint.X >= x && nextPoint.X <= x))
                {
                    float t = (x - currentPoint.X) / (nextPoint.X - currentPoint.X);
                    Vector4 intersectionPoint = currentPoint + t * (nextPoint - currentPoint);

                    if (leftPoint == Vector4.Zero)
                    {
                        leftPoint = intersectionPoint;
                    }
                    else
                    {
                        rightPoint = intersectionPoint;
                        break;
                    }
                }
            }

            return new(leftPoint, rightPoint);
        }

        public readonly LineSegment FindIntersectingSegmentY(Vector4 point1, Vector4 point2, Vector4 point3, float y)
        {
            Vector4[] trianglePoints = [point1, point2, point3];
            Vector4 leftPoint = Vector4.Zero;
            Vector4 rightPoint = Vector4.Zero;

            for (int i = 0; i < 3; i++)
            {
                Vector4 currentPoint = trianglePoints[i];
                Vector4 nextPoint = trianglePoints[(i + 1) % 3];

                if ((currentPoint.Y <= y && nextPoint.Y >= y) || (currentPoint.Y >= y && nextPoint.Y <= y))
                {
                    float t = (y - currentPoint.Y) / (nextPoint.Y - currentPoint.Y);
                    Vector4 intersectionPoint = currentPoint + t * (nextPoint - currentPoint);

                    if (leftPoint == Vector4.Zero)
                    {
                        leftPoint = intersectionPoint;
                    }
                    else
                    {
                        rightPoint = intersectionPoint;
                        break;
                    }
                }
            }

            return new(leftPoint, rightPoint);
        }

        public Vector3 NormalVector()
        {
            Vector3 vertex1 = new Vector3(A.X,A.Y,A.Z);
            Vector3 vertex2 = new Vector3(B.X, B.Y, B.Z);
            Vector3 vertex3 = new Vector3(C.X, C.Y, C.Z);
            Vector3 vector1 = vertex2 - vertex1;
            Vector3 vector2 = vertex3 - vertex1;
            return Vector3.Cross(vector1, vector2);
        }
    }
}
