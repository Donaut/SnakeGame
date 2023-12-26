using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGameCore
{
    public class Game
    {
        private readonly int _width;
        private readonly int _height;
        
        private readonly List<Point> _snake = [];
        private Directions _direction = Directions.None;
        private readonly Random _random = new(); // Dont want to use Random.Shared
        // Doesn't matter the direction when the threshold is reached we will move the head to the current direction
        private float _moveDirection = 0;

        private List<int> _foodLocations = [];
        private List<Point> _foods = new();
        private Point _tail;


        /// <summary>
        /// Width of the map
        /// </summary>
        public int Width => _width;

        /// <summary>
        /// Height of the map
        /// </summary>
        public int Height => _height;

        public int Points { get; protected set; }

        public bool IsDead { get; protected set; }

        /// <summary>
        /// Creat the default game.
        /// </summary>
        /// <param name="width">The width of the map</param>
        /// <param name="height">The height of the map</param>
        public Game(int width = 70, int height = 27)
        {
            _width = width;
            _height = height;

            
            _foodLocations = new List<int>(Width * Height);
        }

        /// <summary>
        /// Resets the game. 
        /// </summary>
        public void Reset()
        {
            // Setup snake
            var center = new Point(Width / 2, Height / 2);

            _snake.Clear();
            _snake.Add(center);
            _snake.Add(new(center.X, center.Y + 1));
            _snake.Add(new(center.X, center.Y + 2));

            // We store in a list all the locations where the food can spwan.
            // We will manually update this list when needed to reflect the game changes.
            _foodLocations.Clear();
            for (var i = 0; i < Width * Height; i++)
            {
                _foodLocations.Add(i);
            }
            RemoveFoodLocation(center);
            RemoveFoodLocation(new (center.X, center.Y + 1));
            RemoveFoodLocation(new (center.X, center.Y + 2));
            
            _foods.Clear();
            for (var i = 0; i < 10; i++)
            {
                if (TryGenerateFoodLocation(out var food))
                {
                    _foods.Add(food);
                    RemoveFoodLocation(food);
                }
            }

            _tail = _snake.Last();
            _direction = Directions.Up;
            IsDead = false;

            Points = 0;
        }

        public void Update(float elapsedSeconds, Directions direction)
        {
            if (IsDead)
            {
                return;
            }

            // We dont allow the player to move into the snake itself and if no direction was specified we continue on the current direction.
            if (_direction.Inverse() == direction
                || direction == Directions.None)
            {
                direction = _direction;
            }

            var speed = 9f;

            // Move snake
            if (_moveDirection >= 1f)
            {
                var nextHead = direction.ToPoint();
                var head = _snake[0];
                nextHead.Offset(head);

                _tail = _snake.Last();

                _snake.Insert(0, nextHead);
                _snake.RemoveAt(_snake.Count - 1);

                _moveDirection = 0;
                _direction = direction;

                AddFoodLocation(_tail);
                RemoveFoodLocation(nextHead);
            }
            else if (_direction != direction)
            {
                var nextHead = direction.ToPoint();
                var head = _snake[0];
                nextHead.Offset(head);

                _tail = _snake.Last();

                _snake.Insert(0, nextHead);
                _snake.RemoveAt(_snake.Count - 1);

                _moveDirection = 0;
                _direction = direction;

                AddFoodLocation(_tail);
                RemoveFoodLocation(nextHead);
            }
            


            _moveDirection += speed * elapsedSeconds;

            // Check food collision
            {
                var head = _snake[0];

                var index = _foods.IndexOf(head);

                if (index != -1)
                {
                    _snake.Add(_tail);
                    RemoveFoodLocation(_tail);
                    //AddFoodLocation(head);
                    //AddFoodLocation(_foods[index]);
                    _foods.RemoveAt(index);

                    if (TryGenerateFoodLocation(out var food))
                    {
                        _foods.Add(food);
                    }

                    Points += 10;
                }
            }

            // Check collision
            {
                var head = _snake[0];
                if (head.X < 0 || head.X >= Width || head.Y < 0 || head.Y >= Height)
                {
                    IsDead = true;
                    goto endOfCollisionCheck; // FUCK FLAGS
                }
                // Collision check
                for (int i = 1; i < _snake.Count; i++)
                {
                    var body = _snake[i];
                    if (head == body)
                    {
                        IsDead = true;
                        goto endOfCollisionCheck; // FUCK FLAGS
                    }
                }
                endOfCollisionCheck:;
            }
        }

        /// <summary>
        /// Simplified interface for drawing to the console.
        /// </summary>
        public void DrawConsole<TRenderer>(TRenderer renderer) where TRenderer : IConsoleRenderer
        {
            renderer.BeginDraw();
            try
            {
                // TODO: The snake tail and head í0
                for (var i = 0; i < _snake.Count; i++)
                {
                    var current = _snake[i];

                    renderer.DrawCharacter(current.X, current.Y, '#');
                }

                for (var i = 0; i < _foods.Count; i++)
                {
                    var current = _foods[i];

                    renderer.DrawCharacter(current.X, current.Y, '@');
                }
            }
            finally
            {
                renderer.EndDraw();
            }
        }

        private void RemoveFoodLocation(Point point, [CallerLineNumber] int sourceLineNumber = 0)
        {
            var x = point.X;
            var y = point.Y;

            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return;

            var item = point.Y * Width + point.X;
            var index = _foodLocations.BinarySearch(item);
            if (index < 0)
            {
                Trace.WriteLine($"Alredy removed possible food location. Location: {point}, Source line: {sourceLineNumber}");
                return;
            }

            _foodLocations.RemoveAt(index);
        }

        private void AddFoodLocation(Point point, [CallerLineNumber] int sourceLineNumber = 0)
        {
            var x = point.X;
            var y = point.Y;

            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return;

            var item = point.Y * Width + point.X;
            var index = ~_foodLocations.BinarySearch(item);

            if (index < 0)
            {
                Trace.WriteLine($"Alredy added possible food location. Location: {point}, Source line: {sourceLineNumber}");
                return;
            }

            _foodLocations.Insert(index, item);
        }

        private bool TryGenerateFoodLocation(out Point location)
        {
            var index = _random.Next(0, _foodLocations.Count);

            if (index >= _foodLocations.Count)
            {
                location = Point.Empty;
                return false;
            }

            var item = _foodLocations[index];

            var x = item % Width;
            var y = item / Width;

            location = new Point(x, y);
            return true;
        }
    }
}
