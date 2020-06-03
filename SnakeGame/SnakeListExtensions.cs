using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    /// <summary>
    /// Extension methods for all the arrays.
    /// </summary>

    public static class SnakeListExtensions
    {
        /// <summary>
        /// Extension method for the SnakeBodyPart array types.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="food"></param>
        /// <returns>True if it collides with a BodyPart otherwise False.</returns>
        public static bool IsContainsFood(this IEnumerable<SnakeBodyParts> list, Food food)
        {
            foreach (var item in list)
            {
                if (item.Equals(food))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks the list to find elements that has the same position.
        /// </summary>
        /// <param name="points">The list</param>
        /// <returns></returns>
        public static bool IsContainDuplicates(this IList<SnakeBodyParts> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[i].Equals(list[j])) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks the list to find elements with the same position.
        /// </summary>
        /// <param name="points">The list</param>
        /// <returns>True if it's find an element with the same value otherwise false.</returns>
        public static bool IsContainDuplicates(this IList<SnakeBodyParts> list, SnakeBodyParts part)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Equals(part)) return true;
            }

            return false;
        }
    }


}
