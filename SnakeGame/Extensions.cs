using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    public static class Extensions
    {
        public static IEnumerable<Snake> CreateValidPlaces(int width, int height, List<Snake> snake)
        {
            Snake[] array = new Snake[width * height];
            //for (int y = 0; y < height; y++)
            //{
            //    for (int x = 0; x < width; x++)
            //    {
            //        array[y * x] = new Snake(x, y);
            //    }
            //}

            for (int i = 0; i < array.Length; i++)
            {
                int x = i / height;
                int y = i % height;
                array[i] = new Snake(x, y);
            }
            return array.Except(snake);
        }

        /// <summary>
        /// Searches for a duplicates in the list.
        /// </summary>
        /// <param name="points">The list</param>
        /// <returns>True if it's find two or more element with the same value otherwise false.</returns>
        public static bool IsContainDuplicates(this IList<Snake> snakeParts)
        {
            for (int i = 0; i < snakeParts.Count; i++)
            {
                for (int j = i + 1; j < snakeParts.Count; j++)
                {
                    if (snakeParts[i].Equals(snakeParts[j])) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Searches for duplicates based on the specified part.
        /// </summary>
        /// <param name="snakeParts"></param>
        /// <param name="part"></param>
        /// <returns></returns>
        public static bool IsContainDuplicates(this IList<Snake> snakeParts, Snake part)
        {
            for (int i = 0; i < snakeParts.Count; i++)
            {
                if (snakeParts[i].Equals(part)) return true;
            }

            return false;
        }

        /// <summary>
        /// Checks the food is inside the snake.
        /// </summary>
        /// <param name="snakeParts"></param>
        /// <param name="food"></param>
        /// <returns></returns>
        public static bool IsSnakeContainFood(this IEnumerable<Snake> snakeParts, Food food)
        {
            foreach (var item in snakeParts)
            {
                if (item.Equals(food))
                    return true;
            }

            return false;
        }
    }


}
