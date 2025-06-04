using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Utilities
{
    public static class MathUtils
    {
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Math.Clamp(t, 0, 1);
        }

        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(
                Lerp(a.X, b.X, t),
                Lerp(a.Y, b.Y, t),
                Lerp(a.Z, b.Z, t)
            );
        }

        public static float SmoothStep(float edge0, float edge1, float x)
        {
            float t = Math.Clamp((x - edge0) / (edge1 - edge0), 0, 1);
            return t * t * (3 - 2 * t);
        }

        public static float Distance(Vector3 a, Vector3 b)
        {
            return (b - a).Length;
        }

        public static float DistanceSquared(Vector3 a, Vector3 b)
        {
            return (b - a).LengthSquared;
        }

        public static Vector3 RandomPointInCircle(float radius)
        {
            var random = new Random();
            float angle = (float)(random.NextDouble() * 2 * Math.PI);
            float r = (float)(Math.Sqrt(random.NextDouble()) * radius);
            return new Vector3((float)Math.Cos(angle) * r, (float)Math.Sin(angle) * r, 0);
        }

        public static bool IsPointInRectangle(Vector2 point, Vector2 rectCenter, Vector2 rectSize)
        {
            Vector2 halfSize = rectSize * 0.5f;
            return point.X >= rectCenter.X - halfSize.X &&
                   point.X <= rectCenter.X + halfSize.X &&
                   point.Y >= rectCenter.Y - halfSize.Y &&
                   point.Y <= rectCenter.Y + halfSize.Y;
        }
    }
}
