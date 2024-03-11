using System;
using System.Collections.Generic;
using System.Numerics;

namespace ObjVisualizer.Models.DataModels
{
    internal struct Triangle(Vector3 a, Vector3 b, Vector3 c)
    {
        public Vector3 A { get; set; } = a;
        public Vector3 B { get; set; } = b;
        public Vector3 C { get; set; } = c;

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

        public readonly LineSegment FindIntersectingSegmentX(Vector3 point1, Vector3 point2, Vector3 point3, float x)
        {
            Vector3[] trianglePoints = [point1, point2, point3];
            Vector3 leftPoint = Vector3.Zero;
            Vector3 rightPoint = Vector3.Zero;

            for (int i = 0; i < 3; i++)
            {
                Vector3 currentPoint = trianglePoints[i];
                Vector3 nextPoint = trianglePoints[(i + 1) % 3];

                if (currentPoint.X <= x && nextPoint.X >= x || currentPoint.X >= x && nextPoint.X <= x)
                {
                    float t = (x - currentPoint.X) / (nextPoint.X - currentPoint.X);
                    Vector3 intersectionPoint = currentPoint + t * (nextPoint - currentPoint);

                    if (leftPoint == Vector3.Zero)
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

        public readonly LineSegment FindIntersectingSegmentY(Vector3 point1, Vector3 point2, Vector3 point3, float y)
        {
            Vector3[] trianglePoints = [point1, point2, point3];
            Vector3 leftPoint = Vector3.Zero;
            Vector3 rightPoint = Vector3.Zero;

            for (int i = 0; i < 3; i++)
            {
                Vector3 currentPoint = trianglePoints[i];
                Vector3 nextPoint = trianglePoints[(i + 1) % 3];

                if (currentPoint.Y <= y && nextPoint.Y >= y || currentPoint.Y >= y && nextPoint.Y <= y)
                {
                    float t = (y - currentPoint.Y) / (nextPoint.Y - currentPoint.Y);
                    Vector3 intersectionPoint = currentPoint + t * (nextPoint - currentPoint);

                    if (leftPoint == Vector3.Zero)
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
    }
}
