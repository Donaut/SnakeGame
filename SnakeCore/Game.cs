using StbImageSharp;
using System.Buffers;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http.Headers;
using System.Numerics;
using System.Resources;
using static System.Formats.Asn1.AsnWriter;

namespace SnakeCore;

public delegate void StateUpdate(float elapsedSeconds, Direction direction);

public class Game
{
    // Fields for the snake movement
    private readonly List<Vector2> _snake = new List<Vector2>();
    private bool _waitingForFirstInput = true;
    private float _t = -.5f;

    private Direction _currDirection = Direction.None;
    private List<Direction> _nextDirections = new();

    //private Direction _currDirection = Direction.None;
    //private Direction _nextDirection = Direction.None;

    // Fields for the snake drawing
    private Vector2 _snakeHeadOffset = Vector2.Zero;
    private Vector2 _snakeHeadRotation;
    private Vector2 _tailOffset = Vector2.Zero;
    private Vector2 _shakeOffset = Vector2.Zero;


    private readonly float _shakeDurationStart = .16f;
    private float _shakeDurationRemaining = .16f;

    private readonly float _goingBackStart1 = .18f;
    private float _goingBackRemaining1 = .18f;

    private readonly float _goingBackStart2 = .06f;
    private float _goingBackRemaining2 = .06f;

    private readonly float _waitingMenuStart = 1.1f;
    private float _waitingMenuRemaining = 1.1f;

    private Direction _lastDirection1 = Direction.None;
    private Direction _lastDirection2 = Direction.None;

    private Vector2 _removedTail1;
    private Vector2 _removedTail2;

    /// <summary>
    /// RENAME TO CELLS COUNT
    /// </summary>
    private int Width { get; set; } = 10;

    /// <summary>
    /// RENAME TO CELLS COUNT
    /// </summary>
    private int Height { get; set; } = 10;

    public int Points { get; protected set; }

    public bool IsDead { get; protected set; }

    public float Speed { get; set; } = 5;

    //private int _cellSize = 100;

    private StateUpdate _stateUpdate;

    private ImageHandle _eyeImage;
    private ImageHandle _eatImage;
    private ImageHandle _cellImage;

    /// <summary>
    /// The game is designed in 
    /// </summary>
    public int DesignWidth { get; } = 720;

    public int DesignHeight { get; } = 720;

    public Game()
    {
    }

    /// <summary>
    /// Resets the game. 
    /// </summary>
    public void Initialize(IRenderer renderer)
    {
        var eyeImage = ImageResult.FromMemory(Resource.px_blink, ColorComponents.RedGreenBlueAlpha);
        _eyeImage = renderer.CreateImage(eyeImage.Width, eyeImage.Height, eyeImage.Data);

        var eatImage = ImageResult.FromMemory(Resource.px_eat, ColorComponents.RedGreenBlueAlpha);
        _eatImage = renderer.CreateImage(eatImage.Width, eatImage.Height, eatImage.Data);

        var _cellSize = 100;
        var cellImageBytesLength = _cellSize * _cellSize;
        var cellImageBytes = ArrayPool<byte>.Shared.Rent(cellImageBytesLength * 4);
        Array.Fill(cellImageBytes, (byte)255);
        _cellImage = renderer.CreateImage(_cellSize, _cellSize, cellImageBytes.AsSpan());

        var center = new Vector2(Width / 2, Height / 2);
        center = new Vector2(4, center.Y);

        _snake.Clear();
        _snake.Add(center);
        _snake.Add(center + new Vector2(-1, 0));
        _snake.Add(center + new Vector2(-2, 0));

        _waitingForFirstInput = true;
        _t = 0;

        _currDirection = Direction.Right;
        _nextDirections.Clear();
        //_nextDirections.Add(Direction.Up);

        _shakeDurationRemaining = _shakeDurationStart;
        _goingBackRemaining1 = _goingBackStart1;
        _goingBackRemaining2 = _goingBackStart2;
        _waitingMenuRemaining = _waitingMenuStart;

        IsDead = false;
        Points = 0;

        _stateUpdate = InGame;
    }

    void InGame(float elapsedSeconds, Direction direction)
    {
        if (_waitingForFirstInput
            && direction != Direction.None
            && _currDirection.Inverse() != direction)
        {
            _waitingForFirstInput = false;
            _nextDirections.Add(direction);
        }
        
        if(_waitingForFirstInput)
            return;


        if (direction != Direction.None)
        {
            if (!_nextDirections.Contains(direction))
                //_nextDirection = direction;
                _nextDirections.Add(direction);
            else
                Debug.WriteLine($"Dropped direction: {direction}");
        }

        _t += elapsedSeconds / (1F / Speed);

        // Moved one block
        if (_t >= 1)
        {
            var newHead = _snake[0] + _currDirection.ToVector2();

            _lastDirection2 = _lastDirection1;
            _lastDirection1 = _currDirection;

            var previousRemovedTail = _removedTail2;
            _removedTail2 = _removedTail1;
            _removedTail1 = _snake[^1];

            if (_nextDirections.Count > 0) // If no user input keep the last.
            {
                _currDirection = _nextDirections[0];
                _nextDirections.RemoveAt(0);
            }

            _t = 0;

            _snake.Insert(0, newHead);
            _snake.RemoveAt(_snake.Count - 1);

            var nextHead = newHead + _currDirection.ToVector2();
            if (nextHead.X < 0 || nextHead.X > Width - 1 || nextHead.Y < 0 || nextHead.Y > Height - 1)
            {
                _snake.RemoveAt(0);
                _snake.Add(_removedTail1);

                _removedTail1 = _removedTail2;
                _removedTail2 = previousRemovedTail;

                ChangeState(GoingBack);
                return;
            }
        }

        // Animation
        var currDirection = _currDirection.ToVector2();
        var nextDirection = (_nextDirections.Count > 0 ? _nextDirections[0] : _currDirection).ToVector2();

        var p0 = currDirection / 2;
        var p2 = currDirection + nextDirection / 2;
        var p1 = currDirection;

        var headOffset = Vector2Extensions.LerpQuadraticBezier(p0, p2, p1, _t);

        _snakeHeadOffset = headOffset;
        _snakeHeadRotation = nextDirection;

        _tailOffset = Vector2.Lerp(Vector2.Zero, _snake[^2] - _snake[^1], _t);
    }

    void GoingBack(float elapsedSeconds, Direction direction)
    {
        if(_shakeDurationRemaining > 0)
        {
            _shakeDurationRemaining -= elapsedSeconds;

            var xOffset = Random.Shared.NextSingle(-.1f, .1f);
            var yOffset = Random.Shared.NextSingle(-.1f, .1f);
            _shakeOffset = new Vector2(xOffset, yOffset);

            if(_shakeDurationRemaining <= 0)
            {
                // Initialize next state
                ;
            }
        }
        else if (_goingBackRemaining1 > 0)
        {
            _goingBackRemaining1 -= elapsedSeconds;

            //TODO: Snake head here too!
            //TODO: HEAD?

            var currDirection = _lastDirection1.ToVector2();
            var p0 = currDirection / 2;
            var p1 = currDirection;
            var tailDirection = _snake[^1] - _snake[^2];

            _snakeHeadOffset = Vector2.Lerp(p0, p1, _goingBackRemaining1 / _goingBackStart1);
            _tailOffset = -Vector2.Lerp(Vector2.Zero, tailDirection, _goingBackRemaining1 / _goingBackStart1);
            //_tailOffset = new Vector2(.5f, 0);

            if (_goingBackRemaining1 <= 0)
            {
                // Initialize next state
                // TODO: Change snake head animation

                _snake.Add(_removedTail1);

                // Copied from the next state!
                var tailDirection1 = _snake[^1] - _snake[^2];
                _tailOffset = -Vector2.Lerp(tailDirection1 / 2, tailDirection1, _goingBackRemaining2 / _goingBackStart2);
                
                ;
            }
        }
        else if(_goingBackRemaining2 > 0)
        {
            _goingBackRemaining2 -= elapsedSeconds;
            
            var tailDirection = _snake[^1] - _snake[^2];
            _tailOffset = -Vector2.Lerp(tailDirection / 2, tailDirection, _goingBackRemaining2 / _goingBackStart2);

            if (_goingBackRemaining2 <= 0)
            {
                // Initialize next state
                ;
            }
        }
        else if(_waitingMenuRemaining > 0)
        {
            _waitingMenuRemaining -= elapsedSeconds;

            if(_waitingMenuRemaining <= 0)
            {
                // Initialize next state
                ;
            }
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

    public void Draw(float elapsedSeconds, IRenderer renderer)
    {
        var tileWidth = DesignWidth / Width;
        var tileHeight = DesignHeight / Height;

        var scaleFactor = new Vector2(tileWidth, tileHeight) / new Vector2(_cellImage.Width, _cellImage.Width);

        var tileSize = new Vector2(_cellImage.Width, _cellImage.Height) * scaleFactor;

        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                var lightGreen = Color.FromArgb(162, 209, 73);
                var darkGreen = Color.FromArgb(170, 215, 81);
                var color = (x + y) % 2 == 0 ? lightGreen : darkGreen;

                renderer.DrawImage(_cellImage, new Vector2(x, y) * tileSize, tileSize, 0, Vector2.Zero, color);
            }
        }

        var snakeColor = Color.FromArgb(78, 124, 246);

        var head = _snake[0];

        var headFinal = Vector2.Clamp(head + _snakeHeadOffset, new Vector2(0, 0), new Vector2(Width - 1, Height - 1)) * tileSize;
        var headRotation = MathF.Atan2(_snakeHeadRotation.Y, _snakeHeadRotation.X);
        var look = headRotation;

        var eyePosition1 = Vector2.Transform(new Vector2(-30, -30) * scaleFactor, Quaternion.CreateFromYawPitchRoll(0, 0, look)) + headFinal + tileSize / 2; // + headFinal + tileSize / 2;
        var eyePosition2 = Vector2.Transform(new Vector2(-30, 30) * scaleFactor, Quaternion.CreateFromYawPitchRoll(0, 0, look)) + headFinal + tileSize / 2;// + headFinal + tileSize / 2;

        eyePosition1 = eyePosition1.Round();
        eyePosition2 = eyePosition2.Round();

        var snake = _snake;

        var tailFinal = (snake[^1] + _tailOffset) * tileSize;
        renderer.DrawImage(_cellImage, tailFinal, tileSize, 0, Vector2.Zero, snakeColor.Dark(0.04f * snake.Count));
        
        for (var i = 0; i < snake.Count - 1; i++)
        {
            var body = snake[i];
            var bodyFinal = body * tileSize;
            renderer.DrawImage(_cellImage, bodyFinal, tileSize, 0, Vector2.Zero, snakeColor.Dark(0.04f * i));
        }

        var direction = _currDirection.ToVector2();
        var neckFinal = Vector2.Lerp(head, head + direction, _t) * tileSize;
        renderer.DrawImage(_cellImage, neckFinal, tileSize, 0, Vector2.Zero, snakeColor);

        renderer.DrawImage(_cellImage, headFinal, tileSize, 0, Vector2.Zero, snakeColor);
        renderer.DrawImage(_eyeImage, eyePosition1, new Vector2(40, 40) * scaleFactor, headRotation, new Vector2(20, 20), new Rectangle(20, 20, 40, 40), Color.White);
        renderer.DrawImage(_eyeImage, eyePosition2, new Vector2(40, 40) * scaleFactor, headRotation, new Vector2(20, 20), new Rectangle(20, 20, 40, 40), Color.White);

    }
}
