using System.Numerics;
using System.Diagnostics;
using System.Drawing;
using System;

namespace SnakeCore
{
    [DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
    public readonly record struct Direction : IEquatable<Direction>
    {
        public static Direction None = new Direction(0, 0);

        public static Direction Up = new Direction(0, -1);

        public static Direction Right = new Direction(1, 0);

        public static Direction Down = new Direction(0, 1);

        public static Direction Left = new Direction(-1, 0);

        public readonly int X;

        public readonly int Y;

        /// <summary>
        /// Private constructor so people cant override it.
        /// </summary>
        private Direction(int x, int y)
        {
            X = x; 
            Y = y;
        }

        /// <inheritdoc />
        public Direction Inverse() => new Direction(X * -1, Y * -1);

        /// <inheritdoc />
        public override string ToString()
        {
            if (this == None)
            {
                return "None";
            }
            else if (this == Up)
            {
                return "Up";
            }
            else if (this == Right)
            {
                return "Right";
            }
            else if (this == Down)
            {
                return "Down";
            }
            else if (this == Left)
            {
                return "Left";
            }
            return $"<{X}, {Y}>";
        }
        
        public Vector2 ToVector2() => new Vector2(X, Y);
    }
}
