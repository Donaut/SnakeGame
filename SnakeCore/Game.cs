using System.Drawing;
using System.Numerics;

namespace SnakeCore
{
    public delegate void StateUpdate(float elapsedSeconds, Direction direction);

    public class Game
    {
        private readonly int _width;
        private readonly int _height;

        // Fields for the snake movement
        private readonly List<Vector2> _snake = new List<Vector2>();
        private bool _waitingForFirstInput = true;
        private float _t = -.5f;
        private List<Direction> _directions = new();

        // Fields for the snake drawing
        private Vector2[] _snakeHead = Array.Empty<Vector2>();
        private Vector2 _snakeHeadOffset = Vector2.Zero;
        private Vector2 _snakeHeadRotation;
        private Color _snakeColor = Color.FromArgb(78, 124, 246);

        private Vector2 _tailOffset = Vector2.Zero;

        private Vector2 _shakeOffset = Vector2.Zero;

        private float _shakeDurationRemaining = .15f;
        private float _goingBackStart = .1f;
        private float _goingBackRemaining = .1f;
        private float _waitingMenuStart = 1.1f;
        private float _waitingMenuRemaining = 1.1f;

        private Vector2 _lastTail;
        private Vector2 _prevLastTail;

        private Vector2 _lastDirection;

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

        public float Speed { get; set; } = 7;

        private StateUpdate _stateUpdate;

        /// <summary>
        /// Creat the default game.
        /// </summary>
        /// <param name="width">The width of the map</param>
        /// <param name="height">The height of the map</param>
        public Game(int width = 13, int height = 10)
        {
            _width = width;
            _height = height;
        }

        /// <summary>
        /// Resets the game. 
        /// </summary>
        public void Initialize()
        {
            var center = new Vector2(_width / 2, _height / 2);
            center = new Vector2(12, 2);

            _snake.Clear();
            _snake.Add(center);
            _snake.Add(center + new Vector2(0, 1));
            _snake.Add(center + new Vector2(0, 2));

            _waitingForFirstInput = true;
            _t = 0;

            _directions.Clear();
            _directions.Add(Direction.Up);

            _directions.Add(Direction.Left);
            _directions.Add(Direction.Left);
            _directions.Add(Direction.Up);

            _snakeHead = GameData.Block;
            _snakeHeadOffset = Vector2.Zero;
            _snakeHeadRotation = Direction.Up.ToVector2();

            _shakeOffset = Vector2.Zero;

            IsDead = false;
            Points = 0;

            _stateUpdate = AwaitStart;
        }

        //public void Draw<TRenderer>(float elapsedSeconds, TRenderer effectRenderer) where TRenderer : IRenderer
        public void Draw(float elapsedSeconds, IRenderer renderer)
        {
            var effectRenderer = new RendererWrapper(renderer, _shakeOffset); 

            var block = GameData.Block;

            for (var x = 0; x < _width; x++)
            {
                for (var y = 0; y < _height; y++)
                {
                    effectRenderer.DrawTriangle(
                            block,
                            0,
                            new Vector2(x, y),
                            (x + y) % 2 == 0 ? Color.FromArgb(162, 209, 73) : Color.FromArgb(170, 215, 81)
                        );
                }
            }

            var direction = _directions.First().ToVector2();
            var snakeColor = new HSLColor(_snakeColor);
            var head = _snake[0];

            var neckFinal = Vector2.Lerp(head, head + direction, _t);
            effectRenderer.DrawTriangle(block, 0, neckFinal, snakeColor); // Draw neck

            var tailFinal = _snake.Last() + _tailOffset;
            var tailColor = snakeColor;
            tailColor.Luminosity *= MathF.Pow(0.98f, _snake.Count);
            effectRenderer.DrawTriangle(block, 0, tailFinal, tailColor); // Draw tailDirection

            for (var i = 0; i < _snake.Count - 1; i++)
            {
                effectRenderer.DrawTriangle(block, 0, _snake[i], snakeColor);
                snakeColor.Luminosity *= .98f;
            }

            var headVertices = _snakeHead;
            var headFinal = Vector2.Clamp(head + _snakeHeadOffset, new Vector2(0, 0), new Vector2(Width - 1, Height - 1));

            var headRotation = MathF.Atan2(_snakeHeadRotation.Y, _snakeHeadRotation.X);
            effectRenderer.DrawTriangle(headVertices, headRotation, headFinal, _snakeColor);
            effectRenderer.DrawTriangle(GameData.Eye, headRotation, headFinal, Color.Black);
        }



        void InGame(float elapsedSeconds, Direction direction)
        {
            if (direction != Direction.None
                && direction.Inverse() != _directions.Last())
            // && _directions.Count < 3) This looks bad its seems we need to replace the old ones with the new ones.
            {
                _directions.Add(direction);
            }

            _t += elapsedSeconds / (1F / Speed);

            // We moved one block
            if (_t >= 1)
            {
                var newHead = _snake[0] + _directions.First().ToVector2();

                _prevLastTail = _lastTail;
                _lastTail = _snake.Last();

                _lastDirection = _directions.First().ToVector2();

                if (_directions.Count > 1) // If no user input keep the last.
                    _directions.RemoveAt(0);

                _t = 0;

                _snake.Insert(0, newHead);
                _snake.RemoveAt(_snake.Count - 1);

                var nextHead = newHead + _directions.First().ToVector2();
                if (nextHead.X < 0 || nextHead.X > Width - 1 || nextHead.Y < 0 || nextHead.Y > Height - 1)
                {
                    var lastDirection = _directions[0];
                    _directions.Clear();
                    _directions.Add(lastDirection);

                    ChangeState(Shake);
                    goto updateAnimation;
                    return;
                }


            }

        updateAnimation:;

            var currDirection = _directions[0].ToVector2();
            var nextDirection = (_directions.Count > 1 ? _directions[1] : _directions[0]).ToVector2();

            var p0 = currDirection / 2;
            var p2 = currDirection + nextDirection / 2;
            var p1 = p0 + currDirection / 2;

            _snakeHeadOffset = Vector2Extensions.LerpQuadraticBezier(p0, p2, p1, _t);
            _snakeHeadRotation = (_directions.Count > 1 ? _directions[1] : _directions[0]).ToVector2();

            var tailDirection = _snake[^2] - _snake[^1];
            _tailOffset = Vector2.Lerp(Vector2.Zero, tailDirection, _t);
        }

        

        void Shake(float elapsedSeconds, Direction direction)
        {
            _shakeDurationRemaining -= elapsedSeconds;

            var xOffset = Random.Shared.NextSingle(-.1f, .1f);
            var yOffset = Random.Shared.NextSingle(-.1f, .1f);
            _shakeOffset = new Vector2(xOffset, yOffset);

            if (_shakeDurationRemaining < 0)
            {
                //_shakeOffset = Vector2.Zero;
                //_tailOffset = _snake[^2] - _snake[^1];

                _snake.RemoveAt(0);
                _snake.Add(_lastTail);

                //_snakeHeadOffset = _lastDirection.ToVector2();

                ChangeState(GoingBack);
                GoingBack(0, Direction.None);
            }
        }

        

        void GoingBack(float elapsedSeconds, Direction direction)
        {
            var xOffset = Random.Shared.NextSingle(-.1f, .1f);
            var yOffset = Random.Shared.NextSingle(-.1f, .1f);
            _shakeOffset = new Vector2(xOffset, yOffset);

            _goingBackRemaining =  Math.Clamp(_goingBackRemaining - elapsedSeconds, 0, 1);
            var goingT = _goingBackRemaining / _goingBackStart;

            var tailDirection = _snake[^2] - _snake[^1];
            _tailOffset = Vector2.Lerp(Vector2.Zero, tailDirection, goingT);
            
            _snakeHeadOffset = Vector2.Lerp(Vector2.Zero, _lastDirection, goingT);

            if(goingT <= 0)
            {
                _snake.Add(_prevLastTail);

                var headDirection = _snake[0] - _snake[1];
                _snakeHeadRotation = headDirection;

                ChangeState(WaitingForMenu);
                WaitingForMenu(0, Direction.None);
            }
        }

        void WaitingForMenu(float elapsedSeconds, Direction direction)
        {
            _waitingMenuRemaining = Math.Clamp(_waitingMenuRemaining - elapsedSeconds, 0, 1);
            var menuT = _waitingMenuRemaining / _waitingMenuStart;

            var tailDirection = _snake[^2] - _snake[^1];
            var tailT2 = Math.Clamp(1 - (_waitingMenuStart - _waitingMenuRemaining) / (1F / Speed), 0, 1);

            _tailOffset = Vector2.Lerp(tailDirection / 2, tailDirection, tailT2);
        }

        void Nothing(float elapsedSeconds, Direction direction)
        {
            ;
        }

        void ChangeState(StateUpdate update)
        {
            _stateUpdate = update;
        }

        public void Update(float elapsedSeconds, Direction direction)
        {
            _stateUpdate(elapsedSeconds, direction);
        }

        void AwaitStart(float elapsedSeconds, Direction direction)
        {
            if (direction != Direction.None)
            {
                _directions.Add(direction);
                _waitingForFirstInput = false;
            }

            if (!_waitingForFirstInput)
            {
                ChangeState(InGame);
            }
        }
    }

    struct RendererWrapper : IRenderer
    {
        private readonly IRenderer _renderer;
        private readonly Vector2 _transitionOffset;

        public RendererWrapper(IRenderer renderer, Vector2 transitionOffset)
        {
            _renderer = renderer;
            _transitionOffset = transitionOffset;
        }

        public void DrawTriangle(ReadOnlySpan<Vector2> vertices, float rotation, Vector2 transition, Color color)
        {
            transition += _transitionOffset;

            _renderer.DrawTriangle(vertices, rotation, transition, color);
        }
    }
}
