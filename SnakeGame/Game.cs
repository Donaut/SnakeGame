using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGameCore
{
    public class Game
    {
        private readonly int _width;
        private readonly int _height;
        private List<int> _foodLocations = [];
        private readonly List<Point> _snake = [];
        private Directions _direction = Directions.None;
        private readonly Random _random = new(); // Dont want to use Random.Shared
        // Doesn't matter the direction when the threshold is reached we will move the head to the current direction
        private float _moveDirection = 0;

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
        }

        /// <summary>
        /// Resets the game. 
        /// </summary>
        public void Reset()
        {
            // Setup snake
            var center = new Point(Width / 2, Height / 2);

            // We store in a list all the locations where the food can spwan.
            // We will manually update this list when needed to reflect the game changes.
            _foodLocations = new(Width * Height);
            for (var i = 0; i < Width * Height; i++)
            {
                _foodLocations.Add(i);
            }

            _snake.Clear();
            _snake.Add(center);
            _snake.Add(new(center.X, center.Y + 1));
            _snake.Add(new(center.X, center.Y + 2));

            //RemoveFoodLocation(center);
            //RemoveFoodLocation(new Vector2(center.X, center.Y + 1));
            //RemoveFoodLocation(new Vector2(center.X, center.Y + 2));

            _direction = Directions.Up;
            IsDead = false;

            Points = 0;
        }

        private void RemoveFoodLocation(Vector2 position)
        {
            var index = (int)position.Y * Width + (int)position.X;
            var listIndex = _foodLocations.BinarySearch(index);

            if (listIndex < 0)
                Debug.WriteLine("Couldn't find index for position: {0}", position);
            return;

            _foodLocations.RemoveAt(listIndex);
        }

        private void AddFoodLocation(Vector2 position)
        {
            var index = (int)position.Y * Width + (int)position.X;
            if (index > Width * Height)
                return;

            var listIndex = _foodLocations.BinarySearch(index);

            _foodLocations.Insert(listIndex, index);
        }

        public void Update(float elapsedSeconds, Directions direction)
        {
            if (IsDead)
            {
                // TODO: write logs.
                return;
            }

            var speed = 9f;

            // Move snake
            if (_moveDirection >= 1f)
            {
                // We dont allow the player to move into the snake itself and if no direction was specified we continue on the current direction.
                if (direction == Directions.None ||
                    _direction.Inverse() == direction)
                {
                    direction = _direction;
                }

                var nextHead = direction.ToPoint();
                var head = _snake[0];
                nextHead.Offset(head);

                _snake.Insert(0, nextHead);
                _snake.RemoveAt(_snake.Count - 1);

                _moveDirection = 0;
                _direction = direction;

            }
            else if (_direction != Directions.None
                        && _direction != direction
                        && _direction.Inverse() != direction)
            {
                var nextHead = direction.ToPoint();
                var head = _snake[0];
                nextHead.Offset(head);

                _snake.Insert(0, nextHead);
                _snake.RemoveAt(_snake.Count - 1);

                _moveDirection = 0;
                _direction = direction;
            }

            _moveDirection += speed * elapsedSeconds;

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
                for (var i = 0; i < _snake.Count; i++)
                {
                    var current = _snake[i];

                    renderer.DrawCharacter(current.X, current.Y, '#');
                }
            }
            finally
            {
                renderer.EndDraw();
            }
        }
    }
}
