using System.Numerics;

namespace SnakeCore
{
    internal class GameData
    {
        public static readonly Vector2[] Block = new Vector2[]
        {
            new(-1, -1),
            new(-1, 1),
            new(1, 1),
            new(-1, -1),
            new(1, 1),
            new(1, -1)
        };

        public static readonly Vector2[] EyeOuter = new Vector2[]
        {
            // EYE 1
            new(-1, -.7f),
            new(-1, -.1f),
            new(-.4f, -.1f),
            new(-1f, -.7f),
            new(-.4f, -.1f),
            new(-.4f, -.7f),

            // EYE 2
            new(-1, .7f),
            new(-1, .1f),
            new(-.4f, .1f),
            new(-1f, .7f),
            new(-.4f, .1f),
            new(-.4f, .7f),
        };

        public static readonly Vector2[] EyeInner = new Vector2[]
        {
            // EYE 1
            new(-.9f, -.6f),
            new(-.9f, -.2f),
            new(-.5f, -.2f),
            new(-.9f, -.6f),
            new(-.5f, -.2f),
            new(-.5f, -.6f),

            // EYE 2
            new(-.9f, .6f),
            new(-.9f, .2f),
            new(-.5f, .2f),
            new(-.9f, .6f),
            new(-.5f, .2f),
            new(-.5f, .6f),
        };

        public static readonly Vector2[] EyeDead = new Vector2[]
        {

        };
    }
}
