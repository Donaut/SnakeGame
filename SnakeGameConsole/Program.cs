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

        var previousKeyStates = new List<Directions>(16);
        var currentKeyStates = new List<Directions>(16);

        var game = new Game(20, 20);
        game.Start();

        //var renderer = new ConsoleRenderer(game.Width, game.Height);
        var renderer = new ConsoleRenderer2(game.Width, game.Height);

        const double dt = 1 / 60f; // 60 FPS (1 / 60 = 0.01666666)

        Directions direction = Directions.None;
        var waitingForFirstInput = true;
        var pointsBuffer = new char[32];

        var fps = 0;
        var fpsFrameTime = 0D;

        double currentTime = Stopwatch.GetTimestamp();
        double accumulator = 0.0f;
        while (true)
        {
            double newTime = Stopwatch.GetTimestamp();
            double frameTime = (newTime - currentTime) / Stopwatch.Frequency;
            currentTime = newTime;

            accumulator += frameTime;

            fpsFrameTime += frameTime;

            //Trace.WriteLine(frameTime);
            if (fpsFrameTime > 1)
            {
                //Console.Beep();
                Trace.WriteLine($"FPS: {fps}, frame time: {frameTime}");
                fps = 0;
                fpsFrameTime = 0;
            }

            // Swap input buffers
            (currentKeyStates, previousKeyStates) = (previousKeyStates, currentKeyStates);
            currentKeyStates.Clear();

            // Pool input.
            /*
             * Currently if somebody presses multiple key at once makes it possible that some directions are ignored by the game.
             * After playing for some time i noticed sometimes this causes uninented directions. 
             * Im thinking about not reading all the avalible keys and stop when a new pressed key is detected.
             */
            while (Console.KeyAvailable)
            {
                var consoleKey = Console.ReadKey(true).Key switch
                {
                    ConsoleKey.W or ConsoleKey.UpArrow => Directions.Up,
                    ConsoleKey.D or ConsoleKey.RightArrow => Directions.Right,
                    ConsoleKey.S or ConsoleKey.DownArrow => Directions.Down,
                    ConsoleKey.A or ConsoleKey.LeftArrow => Directions.Left,
                    _ => Directions.None
                };
                ;
                currentKeyStates.Add(consoleKey);
                waitingForFirstInput = false;
            }

            // Read the first pressed button 
            foreach (var nextDirection in currentKeyStates)
            {
                if(nextDirection == Directions.None)
                    continue;

                var hasBeenPressed = currentKeyStates.Contains(nextDirection) && !previousKeyStates.Contains(nextDirection);
                if (hasBeenPressed)
                {
                    direction = nextDirection;
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

            fps++;
            game.DrawConsole(renderer);
        }
    }

    class ConsoleRenderer2 : IConsoleRenderer
    {
        private char[,] _frontBuffer;
        private char[,] _backBuffer;
        private readonly int _width;
        private readonly int _height;

        public ConsoleRenderer2(int width, int height)
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

    [Obsolete()]
    class ConsoleRenderer : IConsoleRenderer
    {
        private char[,] _buffer;
        private readonly int _width;
        private readonly int _height;

        public ConsoleRenderer(int width, int height)
        {
            _width = width;
            _height = height;
            _buffer = new char[height, width];
        }

        public void BeginDraw()
        {
            var buffer = MemoryMarshal.CreateSpan(ref Unsafe.As<byte, char>(ref MemoryMarshal.GetArrayDataReference(_buffer)), _width * _height);
            buffer.Fill(' ');
        }

        public void DrawCharacter(int x, int y, char character)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return;

            _buffer[y, x] = character;
        }

        public void EndDraw()
        {
            Console.SetCursorPosition(0, 0);

            // Draw top border
            for (var i = 0; i <= _width + 1; i++)
                Console.Write('#');
            Console.Write("\r\n");

            var buffer = MemoryMarshal.CreateSpan(ref Unsafe.As<byte, char>(ref MemoryMarshal.GetArrayDataReference(_buffer)), _width * _height);
            for (var y = 0; y < _height; y++)
            {
                Console.Write('#'); // Draw left border
                for (var x = 0; x < _width; x++)
                {
                    Console.Write(buffer[y * _width + x]);
                }
                Console.Write('#'); // Draw right border
                Console.Write("\r\n");
            }

            // Draw bottom border
            for (var i = 0; i <= _width + 1; i++)
                Console.Write('#');
        }
    }
}
