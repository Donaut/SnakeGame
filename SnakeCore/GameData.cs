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

        public static readonly Vector2[] Eye = new Vector2[]
        {
            new(-1f, -.5f),
            new(-1f, .5f),
            new(.1f, 0)
        };

        public static readonly Vector2[] EyeDead = new Vector2[]
        {

        };
    }
}
