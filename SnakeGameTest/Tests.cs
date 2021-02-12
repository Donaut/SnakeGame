using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnakeGame;
using System.Linq;

namespace SnakeGameTest
{
    [TestClass]
    public class Tests
    {
        private List<Snake> snake = new List<Snake>();
        public Tests()
        {
            //Creat The Snake Body
            for (int i = 0; i < 5; ++i)
            {
                snake.Add(new Snake(10, 12 + i));
            }
        }
        [TestMethod]
        public void Food_Outside_Snake()
        {
            var food = new Food(9, 10);

            var result = snake.IsSnakeContainFood(food);

            Assert.AreEqual(false, result, "Broken function");
        }

        [TestMethod]
        public void Food_Inside_Snake()
        {
            var food = new Food(10, 13);

            var result = snake.IsSnakeContainFood(food);

            Assert.AreEqual(true, result, "Broken function");
        }

        [TestMethod]
        public void Snake_Contains_Duplicates()
        {
            var result = snake.IsContainDuplicates();

            Assert.AreEqual(false, result, "Broken function");
        }

        [TestMethod]
        public void Valid_Places_Check()
        {
            var list = Extensions.CreateValidPlaces(50, 20, snake);
            foreach (var item in list)
            {
                if (!(item.X < 50 && item.Y < 20))
                {
                    Assert.Fail("Broken function");
                }
            }
        }
    }
}
