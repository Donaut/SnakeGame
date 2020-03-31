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

        char[,] map;
       
        List<BodyParts> snake = new List<BodyParts>();

        Tuple<int, int> food;

        bool isDead;

        int width;
        int height;
        /// <summary>
        /// Creat the default game.
        /// </summary>
        /// <param name="width">The width of the map</param>
        /// <param name="height">The height of the map</param>
        public SnakeGame(int width = 24, int height = 40)
        {
            this.width = width;
            this.height = height;
            map = new char[width, height];
            GenerateFood();
        }

        /// <summary>
        /// Start The Game.
        /// </summary>
        /// <param name="FPS"></param>
        public void Run(int FPS = 8)
        {
            
            for (int i = 0; i < 14; ++i)
            {
                snake.Add(new BodyParts(10, 20 + i));
            }


            Stopwatch timer = new Stopwatch();
            timer.Start();
            while (true)
            {
                Console.Title = $"Points : {Points}";

                if (timer.Elapsed.Milliseconds < 1000 / FPS)
                    continue;

                int elapsedMilliseconds = timer.Elapsed.Milliseconds;
                timer.Restart();

                Update(elapsedMilliseconds);

                if (isDead)
                    break;

                Draw();
            }

            // The Snake Is Dead.

        }

        Direction PrevDiredction, direction;
        int elapsedMilliSeconds = 0;
        void Update(int elapsedMilliseconds)
        {
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

            BodyParts tail = snake[snake.Count - 1];

            // Remove tail from body
            snake.RemoveAt(snake.Count - 1);

            // Get head position
            BodyParts next = snake[0];

            // Calculate the next Head Position
            BodyParts newPosition = CalculateNextPosition(next, direction);

            // We modify the head position so when we go backwards we hit the second object in the list.
            // Example 
            // HeadPosition: x:10 y:10
            // SnakeParts  : x:11 y:10
            // SnakeParts  : x:12 y:10
            // If we move backwards we collide with the second snake parts not with the head.
            // Prevents the player from going backward
            if (snake[1].Equals(newPosition))
            {
                newPosition = CalculateNextPosition(next, PrevDiredction);
                direction = PrevDiredction;

#if DEBUG
                Console.ReadLine();
#endif
            }
            // Add the new Head to The List.
            snake.Insert(0, newPosition);

            this.elapsedMilliSeconds += elapsedMilliseconds;
            if (this.elapsedMilliSeconds >= 5000)
            {
                food = GenerateFood();
                this.elapsedMilliSeconds = 0;
            }

            // Check if the Snake collide with himself.
            if (BodyParts.FindDuplicates(snake))
            {
                isDead = true;
            }

            //Checks if the snake goes out of the world.
            BodyParts head = snake[0];
            if (head.X < 0 || head.X >= map.GetLength(0) || head.Y < 0 || head.Y >= map.GetLength(1))
            {
                isDead = true;
            }

            if (head.X == food.Item1 && head.Y == food.Item2)
            {
                Points++;
                snake.Add(tail);
                food = GenerateFood();
                this.elapsedMilliSeconds = 0;
            }

            

            PrevDiredction = direction;

        }

        void Draw()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    map[i, j] = '.';
                }
            }

            for (int i = 0; i < snake.Count; i++)
            {
                var item = snake.ToList()[i];

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

            map[food.Item1, food.Item2] = '@';

            

            // Never use the Console.Clear() method because it causes flickering.
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    Console.Write(map[i, j]);
                }
                Console.WriteLine();
            }

        }


        Tuple<int, int> GenerateFood()
        {
            // What if the food is spwan inside the snake?
            // We loop until it dosen't.
            while (true)
            {
                Random rnd = new Random();
                int x = rnd.Next(0, width);
                int y = rnd.Next(0, height);
                food = (x, y).ToTuple();

                foreach (var item in snake)
                {
                    if (item.X == food.Item1 && item.Y == food.Item2)
                    {
                        continue;
                    }
                }

                return food;
            }
        }

        /// <summary>
        /// Calculate the Snake Head next psoiton.
        /// </summary>
        /// <param name="next">The snake head</param>
        /// <param name="direction">The direction you want to move.</param>
        /// <returns></returns>
        BodyParts CalculateNextPosition(BodyParts next, Direction direction)
        {
            // Calculate where the head should be next based on the snake's direction
            if (direction == Direction.Left)
                next = new BodyParts(next.X, next.Y - 1);
            if (direction == Direction.Right)
                next = new BodyParts(next.X, next.Y + 1);
            if (direction == Direction.Up)
                next = new BodyParts(next.X - 1, next.Y);
            if (direction == Direction.Down)
                next = new BodyParts(next.X + 1, next.Y);

            return next;
        }

        /// <summary>
        /// Represents the snake body parts.
        /// </summary>
        class BodyParts : IEquatable<BodyParts>
        {
            /// <summary>
            /// The X coordinate of the body part
            /// </summary>
            public int X { get; private set; }

            /// <summary>
            /// The Y coordinate of the body part
            /// </summary>
            public int Y { get; private set; }

            public BodyParts(int x, int y)
            {
                X = x;
                Y = y;
            }

            /// <summary>
            /// Checks that the two objects have the same X and Y Coordinates.
            /// </summary>
            /// <param name="other">The Other Points Object</param>
            /// <returns></returns>
            public bool Equals(BodyParts other)
            {
                if (this.X == other.X && this.Y == other.Y)
                {
                    return true;
                }

                return false;
            }


            /// <summary>
            /// Checks if the object is in the List.
            /// </summary>
            /// <param name="points">The list.</param>
            /// <returns></returns>
            public bool Contains(IEnumerable<BodyParts> points)
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
            public static bool FindDuplicates(List<BodyParts> points)
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
}
