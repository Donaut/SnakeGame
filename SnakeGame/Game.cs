using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SnakeGameCore
{
    public class Game
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _speed;
        private readonly List<Vector2> _snake = [];

        


        private List<int> _foodLocations = [];
        private List<Point> _foods = new();
        private Vector2 _tail;


        private readonly Random _random = new(); // Dont want to use Random.Shared

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
        public Game(int width = 70, int height = 27, int speed = 7)
        {
            _width = width;
            _height = height;
            _speed = speed;

            _foodLocations = new List<int>(Width * Height);
        }

        /// <summary>
        /// Resets the game. 
        /// </summary>
        public void Reset()
        {
            // Setup snake
            var center = new Vector2(Width / 2, Height / 2);

            // TODO: Remove me
            center = new Vector2(0, Height - 1);

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
            RemoveFoodLocation(new((int)center.X, (int)center.Y));
            RemoveFoodLocation(new((int)center.X, (int)center.Y + 1));
            RemoveFoodLocation(new((int)center.X, (int)center.Y + 2));

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
            //_direction = Directions.Up;
            IsDead = false;

            Points = 0;

            //for (var i = 0; i < 10; i++)
            //{
            //    _directions.Enqueue(Direction.Right);
            //    _directions.Enqueue(Direction.Up);
            //}
            //_waitingForFirstInput = false;
        }

        // Doesn't matter the direction when the threshold is reached we will move the head to the current direction
        private Direction _currDirection = Direction.Up;
        private bool _waitingForFirstInput = true;
        private float _t = 0;

        private Queue<Direction> _directions = new();
        public void Update(float elapsedSeconds, Direction direction)
        {
            if (IsDead)
            {
                return;
            }

            if(direction != Direction.None)
            {
                _directions.Enqueue(direction);
                _waitingForFirstInput = false;
            }

            if (_waitingForFirstInput)
                return;

            _t += elapsedSeconds / (1F / _speed);

            //_snake[0] += _direction.ToVector2() * _speed * elapsedSeconds;
            // We moved one block
            if (_t >= 1)
            {
                var newHead = _snake[0] + _currDirection.ToVector2();
                if (newHead.X < 0 || newHead.X >= Width || newHead.Y < 0 || newHead.Y >= Height)
                {
                    IsDead = true;
                    return;
                }

                if(_directions.TryDequeue(out var nextDirection))
                {
                    _currDirection = nextDirection;
                }

                _t = 0;

                _snake.RemoveAt(_snake.Count - 1);
                _snake.Insert(0, newHead);
            }
        }

        public void Draw<TRenderer>(float elapsedSeconds, TRenderer renderer) where TRenderer : IRenderer
        {
            var t = _t; // t is a value beetwen 0 and 1.


            var currDirection = _currDirection.ToVector2();
            var nextDirection = currDirection;
            if (_directions.TryPeek(out var direction))
                nextDirection = direction.ToVector2();

            
            // Draw body
            for (var i = 0; i < _snake.Count - 1; i++)
            {
                var currBody = _snake[i];
                renderer.DrawBody(currBody, 0);
            }

            // Draw tail
            var last = _snake.Last();
            var beforeLast = _snake[_snake.Count - 2];
            var tailDirection = Vector2.Normalize(beforeLast - last);
            var tail = Vector2.Lerp(last, last + tailDirection, t);
            renderer.DrawBody(tail, 0);


            var head = _snake[0];

            // Draw neck
            var neck = Vector2.Lerp(head, head + currDirection, MathF.Min(1F, t * 1.3f));
            renderer.DrawBody(neck, 0);


            var headStart = head + currDirection / 2;
            var headEnd = head + currDirection + nextDirection / 2;
            var rotation = -MathF.Atan2(nextDirection.X, nextDirection.Y) + MathF.PI;

            
            if (t <= .5f) 
            {   
                headEnd = head + currDirection + currDirection / 2;
                rotation = -MathF.Atan2(currDirection.X, currDirection.Y) + MathF.PI;
            }

            var headFinal = Vector2.Lerp(headStart, headEnd, t);
            //t = MathExtensions.EaseOutExpo(t);


            renderer.DrawHead(headFinal, rotation);
        }

        /// <summary>
        /// Simplified interface for drawing to the console.
        /// </summary>
        public void DrawConsole<TRenderer>(TRenderer renderer) where TRenderer : IConsoleRenderer
        {
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

    static class Vector2Extensions
    {
        public static Vector2 EaseOutExpo(this Vector2 @this)
        {
            @this.X = MathExtensions.EaseOutExpo(@this.X);
            @this.Y = MathExtensions.EaseOutExpo(@this.Y);

            return @this;
        }
    }

    static class MathExtensions
    {
        public static float EaseOutExpo(float x) 
        {
            return x == 1F ? 1 : 1 - MathF.Pow(2, -10 * x);
        }
    }
}
