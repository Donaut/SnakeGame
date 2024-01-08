using SnakeGameCore;
using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SnakeGameConsole;

internal class Program
{
    static void Main(string[] args)
    {
        Console.CursorVisible = false;

        var previousKeyStates = new List<Direction>(16);
        var currentKeyStates = new List<Direction>(16);
        
        var game = new Game();
        

        //var renderer = new ConsoleRenderer(game.Width, game.Height);
        var renderer = new ConsoleRenderer(game.Width, game.Height);
        
        restart:;

        game.Reset();

        const double dt = 1 / 60f; // 60 FPS (1 / 60 = 0.01666666)

        Direction direction = Direction.None;
        var waitingForFirstInput = true;
        var pointsBuffer = new char[32];

        double currentTime = Stopwatch.GetTimestamp();
        double accumulator = 0.0f;
        while (!game.IsDead)
        {
            double newTime = Stopwatch.GetTimestamp();
            double frameTime = (newTime - currentTime) / Stopwatch.Frequency;
            currentTime = newTime;

            if(frameTime > dt * 5)
                continue; // Too large skip happened we dont try to catch up.
            accumulator += frameTime;

            // Pool input.
            (currentKeyStates, previousKeyStates) = (previousKeyStates, currentKeyStates);
            currentKeyStates.Clear();

            while (Console.KeyAvailable)
            {
                var nextDirection = Console.ReadKey(true).Key switch
                {
                    ConsoleKey.W or ConsoleKey.UpArrow => Direction.Up,
                    ConsoleKey.D or ConsoleKey.RightArrow => Direction.Right,
                    ConsoleKey.S or ConsoleKey.DownArrow => Direction.Down,
                    ConsoleKey.A or ConsoleKey.LeftArrow => Direction.Left,
                    _ => Direction.None
                };
                
                if(nextDirection == Direction.None)
                    continue;

                currentKeyStates.Add(nextDirection);
                waitingForFirstInput = false;

                var hasBeenPressed = !previousKeyStates.Contains(nextDirection); // && currentKeyStates.Contains(consoleKey);
                if (hasBeenPressed)
                {
                    direction = nextDirection;
                    // If we find a new key which has been pressed we break out early to avoid consuming uneccessary key presses.
                    break;
                }
            }
            
            while (accumulator >= dt)
            {
                if (!waitingForFirstInput)
                    game.Update(0.016f, direction);
                accumulator -= dt;
            }
            
            // 0 means put the number there and the ' symbol is like escaping so we dont treat 'Points: ' as a format but just a string
            game.Points.TryFormat(pointsBuffer, out var _, "'Points:' 0"); 
            
            ConsoleExtensions.SetConsoleTitle(waitingForFirstInput ? "Press a key (W, A, S, D) to start" : pointsBuffer);

            game.DrawConsole(renderer);
        }

        Thread.Sleep(2000);
        goto restart;
    }

    class ConsoleRenderer : IConsoleRenderer
    {
        private char[,] _frontBuffer;
        private char[,] _backBuffer;
        private readonly int _width;
        private readonly int _height;

        public ConsoleRenderer(int width, int height)
        {
            _width = width;
            _height = height;
            _frontBuffer = new char[height, width];
            _backBuffer = new char[height, width];

            AsSpan(_frontBuffer).Fill(' ');

            Console.SetCursorPosition(0, 0);

            // Draw top border
            for (var i = 0; i <= _width + 1; i++)
                Console.Write('#');
            Console.Write("\r\n");

            for (var y = 0; y < _height; y++)
            {
                Console.Write('#'); // Draw left border
                for (var x = 0; x < _width; x++)
                    Console.Write(' ');
                Console.Write('#'); // Draw right border
                Console.Write("\r\n");
            }

            for (var i = 0; i <= _width + 1; i++)
                Console.Write('#');
            Console.Write("\r\n");
        }

        public void BeginDraw()
        {
            AsSpan(_backBuffer).Fill(' ');
        }

        public void DrawCharacter(int x, int y, char character)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return;

            _backBuffer[y, x] = character;
        }

        public void EndDraw()
        {
            var frontBuffer = AsSpan(_frontBuffer);
            var backBuffer = AsSpan(_backBuffer);

            for (var y = 0; y < _height; y++)
            {
                var start = y * _width; // Start of the row
                var length = _width; // End of the row

                var frontSlice = frontBuffer.Slice(start, length);
                var backSlice = backBuffer.Slice(start, length);

                if (!backSlice.SequenceEqual(frontSlice)) // Only draw rows that are changed
                {
                    Console.SetCursorPosition(1, y + 1); // + 1 to handle the border offset
                    for (var x = 0; x < backSlice.Length; x++)
                    {
                        Console.Write(backSlice[x]);
                    }
                }
            }

            var temp = _backBuffer;
            _backBuffer = _frontBuffer;
            _frontBuffer = temp;
        }

        private Span<char> AsSpan(char[,] array)
        {
            return MemoryMarshal.CreateSpan(ref Unsafe.As<byte, char>(ref MemoryMarshal.GetArrayDataReference(array)), _width * _height);
        }
    }
}
