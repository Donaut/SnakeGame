using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    /// <summary>
    /// Represents the snake body parts.
    /// </summary>
    public class SnakeBodyParts : IEquatable<SnakeBodyParts>, IEquatable<Food>
    {
        /// <summary>
        /// The X coordinate of the body part
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// The Y coordinate of the body part
        /// </summary>
        public int Y { get; private set; }

        public SnakeBodyParts(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Checks that the two objects have the same X and Y Coordinates.
        /// </summary>
        /// <param name="other">The Other Points Object</param>
        /// <returns></returns>
        public bool Equals(SnakeBodyParts other)
        {
            if (this.X == other.X && this.Y == other.Y)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the food inside in the SnakeBody.
        /// </summary>
        /// <param name="other">The Food Object</param>
        /// <returns></returns>
        public bool Equals(Food other)
        {
            if (this.X == other.X && this.Y == other.Y)
                return true;

            return false;
        }


        /// <summary>
        /// Checks if the object is in the List.
        /// </summary>
        /// <param name="points">The list.</param>
        /// <returns></returns>
        public bool Contains(IEnumerable<SnakeBodyParts> points)
        {
            foreach (var item in points)
            {
                if (this.Equals(item))
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Checks the list to find duplicates.
        /// </summary>
        /// <param name="points">The list</param>
        /// <returns></returns>
        public static bool FindDuplicates(List<SnakeBodyParts> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    if (points[i].Equals(points[j])) return true;
                }
            }

            return false;
        }
    }
}
