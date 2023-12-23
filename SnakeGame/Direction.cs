using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Windows.Markup;
using System.Diagnostics;
using System.Drawing;

namespace SnakeGameCore
{
    [DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
    public readonly struct Directions : IEquatable<Directions>
    {
        public static Directions None = new Directions(0, 0);

        public static Directions Up = new Directions(0, -1);

        public static Directions Right = new Directions(1, 0);

        public static Directions Down = new Directions(0, 1);

        public static Directions Left = new Directions(-1, 0);

        public readonly int X;

        public readonly int Y;

        /// <summary>
        /// Private constructor so people cant override it.
        /// </summary>
        private Directions(int x, int y)
        {
            X = x; 
            Y = y;
        }

        /// <inheritdoc />
        public Directions Inverse() => new Directions(X * -1, Y * -1);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Directions directions && Equals(directions);

        /// <inheritdoc />
        public bool Equals(Directions other) => X == other.X && Y == other.Y;

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(X, Y);

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

            return string.Empty;
        }

        public static bool operator ==(Directions left, Directions right) => left.Equals(right);

        public static bool operator !=(Directions left, Directions right) => !(left == right);

        public Point ToPoint() => new Point(X, Y);

        public Vector2 ToVector2() => new Vector2(X, Y);
    }
}
