using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    public class Snake : IEquatable<Snake>, IEquatable<Food>
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Snake(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Checks that the X and Y properties are the same for both objects.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Snake other)
        {
            if (this.X == other.X && this.Y == other.Y)
                return true;

            return false;
        }

        /// <summary>
        /// Checks that the X and Y properties are the same for both objects.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Food other)
        {
            if (this.X == other.X && this.Y == other.Y)
                return true;

            return false;
        }
        
    }
}
