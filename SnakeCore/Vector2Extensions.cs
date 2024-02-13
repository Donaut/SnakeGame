using System.Numerics;

namespace SnakeCore
{
    internal static class Vector2Extensions
    {
        public static Vector2 LerpQuadraticBezier(Vector2 P0, Vector2 P2, Vector2 P1, float t)
        {
            // (1 - t)^2 * P0 + 2 * (1 - t) * t * P1 + t^2 * P2
            return (1 - t) * (1 - t) * P0 + 2 * (1 - t) * t * P1 + t * t * P2;
            //var headFinal = MathF.Pow(1f - t, 2) * headStart + 2f * (1f - t) * t * p1 + t * t * headEnd;
        }

        public static Vector2 Lerp3(Vector2 left, Vector2 middle, Vector2 right, float t)
        {
            if (t < 0)
                return Vector2.Lerp(middle, left, -t);
            else
                return Vector2.Lerp(middle, right, t);
        }
    }
}
