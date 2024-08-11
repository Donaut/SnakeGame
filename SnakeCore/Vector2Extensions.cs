using System.Numerics;

namespace SnakeCore
{
    internal static class Vector2Extensions
    {
        public static Vector2 Floor(this Vector2 @this, MidpointRounding mode = default)
        {
            return new Vector2(
                MathF.Floor(@this.X),
                MathF.Floor(@this.Y));
        }

        public static Vector2 Round(this Vector2 @this, MidpointRounding mode = default)
        {
            return new Vector2(
                MathF.Round(@this.X, mode),
                MathF.Round(@this.Y, mode));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="P0">Start point</param>
        /// <param name="P2">End point</param>
        /// <param name="P1">Control point</param>
        /// <param name="t">Number beetwen 0 - 1</param>
        /// <returns></returns>
        public static Vector2 LerpQuadraticBezier(Vector2 P0, Vector2 P2, Vector2 P1, float t)
        {
            // (1 - t)^2 * P0 + 2 * (1 - t) * t * P1 + t^2 * P1
            return (1 - t) * (1 - t) * P0 + 2 * (1 - t) * t * P1 + t * t * P2;
        }
    }
}
