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
        enum Direction { Left, Right, Up, Down }

        char[,] MAP;

        List<SnakeBodyParts> snake = new List<SnakeBodyParts>();

        Food food;

        bool isDead;

        int width;
        int height;
        /// <summary>
        /// Creat the default game.
        /// </summary>
        /// <param name="width">The width of the map</param>
        /// <param name="height">The height of the map</param>
        public SnakeGame(int width = 50, int height = 20)
        {
            this.width = width;
            this.height = height;
            MAP = new char[width, height];
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
                snake.Add(new SnakeBodyParts(10, 12 + i));
            }

            food = GenerateFood();

            Stopwatch timer = new Stopwatch();
            timer.Start();
            // Usualy the elapsedMillisecond used to multiply some value. Evrything multiplied by 1 is result in the origianl valu
            int elapsedMilliseconds = 1;
            while (true)
            {
                Update(elapsedMilliseconds);

                if (isDead)
                    break;

                Draw();

                // Logic for the FPS control
                while (true)
                {
                    if (timer.Elapsed.Milliseconds < 1000 / FPS)
                        continue;

                    elapsedMilliseconds = timer.Elapsed.Milliseconds;
                    timer.Restart();
                    break;
                }
            }
        }

        Direction PrevDiredction, direction;
        int elapsedMilliSeconds;

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
            SnakeBodyParts tail = snake[snake.Count - 1];

            // Remove tail from body
            snake.RemoveAt(snake.Count - 1);

            // Get head position
            SnakeBodyParts next = snake[0];

            // Calculate the next Head Position
            SnakeBodyParts newPosition = CalculateNextPosition(next, direction);

            // We modify the head position so when we go backwards we hit an object in the list.
            // Example 
            // HeadPosition: x:10 y:10
            // SnakeParts  : x:11 y:10
            // SnakeParts  : x:12 y:10
            // If we move backwards we collide with the snake parts.
            // Prevents the player from going backward
            if (snake.IsContainDuplicates(newPosition))
            {
                newPosition = CalculateNextPosition(next, PrevDiredction);
                direction = PrevDiredction;
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
            SnakeBodyParts head = snake[0];
            if (head.X < 0 || head.X >= MAP.GetLength(0) || head.Y < 0 || head.Y >= MAP.GetLength(1))
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

            PrevDiredction = direction;
        }

        unsafe void Draw()
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
            for (int Y = 0; Y < MAP.GetLength(1); Y++)
            {
                for (int X = 0; X < MAP.GetLength(0); X++)
                {
                    MAP[X, Y] = '.';
                }
            }

            ////Drawing the snake.
            for (int i = 0; i < snake.Count; i++)
            {
                var item = snake[i];

                MAP[item.X, item.Y] = 'O';

                if (i == 0)
                {
                    MAP[item.X, item.Y] = 'I';
                }

                if (i == snake.Count - 1)
                {
                    MAP[item.X, item.Y] = '#';
                }
            }

            // Drawing the food
            MAP[food.X, food.Y] = '@';

            // And finally drawing the updated whole map
            Console.SetCursorPosition(0, 0);
            for (int y = 0; y < MAP.GetLength(1); y++)
            {
                for (int x = 0; x < MAP.GetLength(0); x++)
                {
                    Console.Write(MAP[x, y]);
                }
                Console.WriteLine();
            }
        }
        #endregion

        Food GenerateFood()
        {
            // What if the food is spwan inside the snake?
            // We loop until it dosen't.
            while (true)
            {
                Random rnd = new Random();
                int x = rnd.Next(0, width);
                int y = rnd.Next(0, height);
                var localFood = new Food(x, y);

                if (snake.IsContainsFood(localFood))
                    continue;

                return localFood;
            }
        }

        /// <summary>
        /// Calculate the Snake Head next psoiton.
        /// </summary>
        /// <param name="next">The snake head</param>
        /// <param name="direction">The direction you want to move.</param>
        /// <returns></returns>
        SnakeBodyParts CalculateNextPosition(SnakeBodyParts next, Direction direction)
        {
            // Calculate where the head should be next based on the snake's direction
            if (direction == Direction.Left)
                next = new SnakeBodyParts(next.X - 1, next.Y);
            if (direction == Direction.Right)
                next = new SnakeBodyParts(next.X + 1, next.Y);
            if (direction == Direction.Up)
                next = new SnakeBodyParts(next.X, next.Y - 1);
            if (direction == Direction.Down)
                next = new SnakeBodyParts(next.X, next.Y + 1);

            return next;
        }




    }
}
