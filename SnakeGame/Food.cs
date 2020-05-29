using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    /// <summary>
    /// The Class that represent a food in the game.
    /// </summary>
    public class Food
    {
        public Food(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// The X coordinate of the body part
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// The Y coordinate of the body part
        /// </summary>
        public int Y { get; private set; }
    }
}
