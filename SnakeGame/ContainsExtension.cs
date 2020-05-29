using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    /// <summary>
    /// Extension method for all the arrays.
    /// </summary>
    
    public static class ContainsExtension
    {
        /// <summary>
        /// Extension method for the SnakeBodyPart array types.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="food"></param>
        /// <returns>True if it collides with a BodyPart otherwise False.</returns>
        public static bool ContainsFood(this IEnumerable<SnakeBodyParts> list, Food food)
        {
            foreach (var item in list)
            {
                if (item.Equals(food))
                    return true;
            }

            return false;
        }
    }
}
