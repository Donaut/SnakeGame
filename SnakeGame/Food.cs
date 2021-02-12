using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    public class Food
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Food(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Food(Snake snake) => new Food(snake.X, snake.Y);
    }
}
