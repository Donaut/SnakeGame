using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SnakeGame;

namespace SnakeGame
{
    public class SnakeGame
    {
        public int Points { get; private set; }
        private Food food;

        private readonly char[,] map;
        private readonly int width;
        private readonly int height;

        private readonly List<Snake> snake = new List<Snake>();
        private bool isDead;

        Direction previousDiredction, direction;
        int elapsedMilliSeconds;
        /// <summary>
        /// Creat the default game.
        /// </summary>
        /// <param name="width">The width of the map</param>
        /// <param name="height">The height of the map</param>
        public SnakeGame(int width = 50, int height = 20)
        {
            this.width = width;
            this.height = height;
            map = new char[width, height];
        }

        /// <summary>
        /// Start The Game.
        /// </summary>
        /// <param name="FPS"></param>
        public void Run(int FPS = 8)
        {
            //Creat The Snake Default Body
            for (int i = 0; i < 5; ++i)
            {
                snake.Add(new Snake(10, 12 + i));
            }

            food = GenerateFood();

            Stopwatch timer = new Stopwatch();
            timer.Start();
            // Usualy the elapsedMillisecond used to multiply some value. Evrything multiplied by 1 is result in the origianl valu
            int elapsedMilliseconds = 1;
            while (true)
            {
                // Logic for the FPS control
                while (true)
                {
                    if (timer.Elapsed.Milliseconds < 1000 / FPS)
                        continue;

                    elapsedMilliseconds = timer.Elapsed.Milliseconds;
                    timer.Restart();
                    break;
                }

                Update(elapsedMilliseconds);

                if (isDead)
                    break;

                Draw();

                
            }
        }

        #region UpdateAndDraw
        void Update(int elapsedMilliseconds)
        {
            Console.Title = $"Points : {Points.ToString()}";

            ConsoleKeyInfo key = default;
            if (Console.KeyAvailable)
                key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    direction = Direction.Up;
                    break;
                case ConsoleKey.DownArrow:
                    direction = Direction.Down;
                    break;
                case ConsoleKey.LeftArrow:
                    direction = Direction.Left;
                    break;
                case ConsoleKey.RightArrow:
                    direction = Direction.Right;
                    break;
            }

            // Get The Taile Position before we delet it.
            Snake tail = snake[snake.Count - 1];

            // Remove tail from body
            snake.RemoveAt(snake.Count - 1);

            // Get head position
            Snake next = snake[0];

            // Calculate the next Head Position
            Snake newPosition = CalculateNextPosition(next, direction);

            // We modify the head position so when we go backwards we hit an object in the list.
            // Example 
            // HeadPosition: x:10 y:10
            // SnakeParts  : x:11 y:10
            // SnakeParts  : x:12 y:10
            // If we move backwards we collide with the snake parts.
            // Prevents the player from going backward
            if (snake.IsContainDuplicates(newPosition))
            {
                newPosition = CalculateNextPosition(next, previousDiredction);
                direction = previousDiredction;
            }
            // Add the new Head to The List.
            snake.Insert(0, newPosition);

            // If the food is older than 5 five second we creat a new one.
            this.elapsedMilliSeconds += elapsedMilliseconds;
            if (this.elapsedMilliSeconds >= 5000)
            {
                food = GenerateFood();
                this.elapsedMilliSeconds = 0;
            }

            // Check if the Snake collide with himself.
            if (snake.IsContainDuplicates())
            {
                isDead = true;
            }

            //Checks if the snake goes out of the world.
            Snake head = snake[0];
            if (head.X < 0 || head.X >= map.GetLength(0) || head.Y < 0 || head.Y >= map.GetLength(1))
            {
                isDead = true;
            }

            if (head.X == food.X && head.Y == food.Y)
            {
                Points++;
                snake.Add(tail);
                food = GenerateFood();
                this.elapsedMilliSeconds = 0;
            }

            previousDiredction = direction;
        }

        void Draw()
        {
            // raw pointers to make more afficient.
            //fixed (char* pointer = MAP)
            //{
            //    // Clearing the map.
            //    for (int i = 0; i < MAP.GetLength(0) * MAP.GetLength(1); i++)
            //    {
            //        pointer[i] = '.';
            //    }

            //    //Drawing the snake.
            //    for (int i = 0; i < snake.Count; i++)
            //    {
            //        var item = snake[i];

            //        pointer[item.X * height + item.Y] = 'O';

            //        if (i == 0)
            //        {
            //            pointer[item.X * height + item.Y] = 'I';
            //        }

            //        if (i == snake.Count - 1)
            //        {
            //            pointer[item.X * height + item.Y] = '#';
            //        }
            //    }

            //    // Drawing the Cake
            //    pointer[food.X * height + food.Y] = '@';

                  //TODO: Use stringBuilder here too
            //    //Drawing The Map
            //    Console.SetCursorPosition(0, 0);
            //    for (int y = 0; y < MAP.GetLength(1); y++)
            //    {
            //        for (int x = 0; x < MAP.GetLength(0); x++)
            //        {
            //            Console.Write(pointer[x * height + y]);
            //        }
            //        Console.WriteLine();
            //    }
            //}

            // Clearing the map
            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    map[x, y] = '.';
                }
            }

            ////Drawing the snake.
            for (int i = 0; i < snake.Count; i++)
            {
                var item = snake[i];

                map[item.X, item.Y] = 'O';

                if (i == 0)
                {
                    map[item.X, item.Y] = 'I';
                }

                if (i == snake.Count - 1)
                {
                    map[item.X, item.Y] = '#';
                }
            }

            // Drawing the food
            map[food.X, food.Y] = '@';

            // And finally drawing the updated map
            Console.SetCursorPosition(0, 0);
            StringBuilder stringBuilder = new StringBuilder();
            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    stringBuilder.Append(map[x, y]);
                }
                stringBuilder.AppendLine();
            }
            Console.WriteLine(stringBuilder);

        }
        #endregion


        Food GenerateFood()
        {
            var range = Extensions.CreateValidPlaces(width, height, snake);
            Random rnd = new Random();
            var position = range.ElementAt(rnd.Next(0, range.Count()));
            return position;
        }

        /// <summary>
        /// Calculate the Snake Head next psoiton.
        /// </summary>
        /// <param name="next">The snake head</param>
        /// <param name="direction">The direction you want to move.</param>
        /// <returns></returns>
        Snake CalculateNextPosition(Snake next, Direction direction)
        {
            // Calculate where the head should be next based on the snake's direction
            if (direction == Direction.Left)
                next = new Snake(next.X - 1, next.Y);
            if (direction == Direction.Right)
                next = new Snake(next.X + 1, next.Y);
            if (direction == Direction.Up)
                next = new Snake(next.X, next.Y - 1);
            if (direction == Direction.Down)
                next = new Snake(next.X, next.Y + 1);

            return next;
        }
    }
}
