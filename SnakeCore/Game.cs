using StbImageSharp;
using System.Buffers;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http.Headers;
using System.Numerics;
using System.Resources;

namespace SnakeCore;

public delegate void StateUpdate(float elapsedSeconds, Direction direction);

public class Game
{
    // Fields for the snake movement
    private readonly List<Vector2> _snake = new List<Vector2>();
    private bool _waitingForFirstInput = true;
    private float _t;

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
    /// TODO: Find better name
    /// Width of the map in cells
    /// </summary>
    private int Width { get; set; } = 10;

    /// <summary>
    /// TODO: Find better name
    /// Height of the map in cells
    /// </summary>
    private int Height { get; set; } = 10;

    public int Points { get; protected set; }

    public bool IsDead { get; protected set; }

    public float Speed { get; set; } = 5;

    private StateUpdate? _stateUpdate;

    //private ImageHandle? _eyeImage;
    //private Rectangle _eyeAnimation1;
    //private Rectangle _eyeAnimation2;

    //private ImageHandle? _eatImage;
    private ImageHandle? _cellImage;

    private ImageHandle? _spriteSheet;
    
    public int DesignWidth { get; } = 700;

    public int DesignHeight { get; } = 700;

    public Game() { }

    /// <summary>
    /// Resets the game. 
    /// </summary>
    public void Initialize(IRenderer renderer)
    {
        //var eyeImage = ImageResult.FromMemory(Resource.px_blink, ColorComponents.RedGreenBlueAlpha);
        //_eyeImage = renderer.CreateImage(eyeImage.Width, eyeImage.Height, eyeImage.Data);

        //_eyeAnimation1 = new Rectangle(20, 20, 40, 40);
        //_eyeAnimation2 = new Rectangle(20, 260, 40, 40);

        //var eatImage = ImageResult.FromMemory(Resource.px_eat, ColorComponents.RedGreenBlueAlpha);
        //_eatImage = renderer.CreateImage(eatImage.Width, eatImage.Height, eatImage.Data);

        var cellImage = ImageResult.FromMemory(Resource.px_cell, ColorComponents.RedGreenBlueAlpha);
        _cellImage = renderer.CreateImage(1, 1, cellImage.Data);

        var spriteSheet = ImageResult.FromMemory(Resource.px_snake, ColorComponents.RedGreenBlueAlpha);
        _spriteSheet = renderer.CreateImage(spriteSheet.Width, spriteSheet.Height, spriteSheet.Data);

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
        if(_waitingForFirstInput
            && direction != Direction.None
            && _currDirection.Inverse() != direction)
        {
            _waitingForFirstInput = false;
            _nextDirections.Add(direction);
        }

        if(_waitingForFirstInput)
            return;


        if(direction != Direction.None && !_nextDirections.Contains(direction) && _nextDirections.LastOrDefault(_currDirection) != direction.Inverse())
        {
            _nextDirections.Add(direction);
        }

        _t += elapsedSeconds / (1F / Speed);

        if(_t >= 1)
        {
            var newHead = _snake[0] + _currDirection.ToVector2();

            _lastDirection2 = _lastDirection1;
            _lastDirection1 = _currDirection;

            var previousRemovedTail = _removedTail2;
            _removedTail2 = _removedTail1;
            _removedTail1 = _snake[^1];

            if(_nextDirections.Count > 0) // If no user input keep the last.
            {
                _currDirection = _nextDirections[0];
                _nextDirections.RemoveAt(0);
            }

            _t = 0;

            _snake.Insert(0, newHead);
            _snake.RemoveAt(_snake.Count - 1);

            var nextHead = newHead + _currDirection.ToVector2();
            var isDead = false;
            for(var i = 1; i < _snake.Count; i++)
            {
                if(_snake[i] == nextHead)
                {
                    isDead = true;
                    break;
                }
            }
            if(nextHead.X < 0 || nextHead.X > Width - 1 || nextHead.Y < 0 || nextHead.Y > Height - 1
               || isDead)
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
        else if(_goingBackRemaining1 > 0)
        {
            _goingBackRemaining1 -= elapsedSeconds;

            var currDirection = _lastDirection1.ToVector2();
            var p0 = currDirection / 2;
            var p1 = currDirection;
            var tailDirection = _snake[^1] - _snake[^2];

            _snakeHeadOffset = Vector2.Lerp(p0, p1, _goingBackRemaining1 / _goingBackStart1);
            _tailOffset = -Vector2.Lerp(Vector2.Zero, tailDirection, _goingBackRemaining1 / _goingBackStart1);

            if(_goingBackRemaining1 <= 0)
            {
                // Initialize next state
                _snake.Add(_removedTail1);

                _snakeHeadRotation = _lastDirection1.ToVector2();

                // Copied from the next state!
                var tailDirection1 = _snake[^1] - _snake[^2];
                _tailOffset = -Vector2.Lerp(tailDirection1 / 2, tailDirection1, _goingBackRemaining2 / _goingBackStart2);
            }
        }
        else if(_goingBackRemaining2 > 0)
        {
            _goingBackRemaining2 -= elapsedSeconds;

            var tailDirection = _snake[^1] - _snake[^2];
            _tailOffset = -Vector2.Lerp(tailDirection / 2, tailDirection, _goingBackRemaining2 / _goingBackStart2);

            if(_goingBackRemaining2 <= 0)
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
        //if(x != Task.Running)
        //x = Animation.StartAnimation(ShakeAnimation, 100);

        _stateUpdate!(elapsedSeconds, direction);
    }

    // WATCH OUT FOR DRAW ORDER!
    public void Draw(float elapsedSeconds, IRenderer renderer)
    {
        Debug.Assert(_cellImage != null);
        Debug.Assert(_spriteSheet != null);
        
        var tileSize = new Vector2(DesignWidth / Width, DesignHeight / Height);
        for(var x = 0; x < Width; x++)
        {
            for(var y = 0; y < Height; y++)
            {
                var lightGreen = Color.FromArgb(162, 209, 73);
                var darkGreen = Color.FromArgb(170, 215, 81);
                var color = (x + y) % 2 == 0 ? lightGreen : darkGreen;

                renderer.DrawImage(_cellImage, new Vector2(x, y) * tileSize, tileSize, 0, Vector2.Zero, color);
            }
        }

        var snake = _snake;
        var snakeColor = Color.FromArgb(78, 124, 246);

        var head = snake[0];

        var headFinal = Vector2.Clamp(head + _snakeHeadOffset, new Vector2(0, 0), new Vector2(Width - 1, Height - 1)) * tileSize;
        var headRotation = MathF.Atan2(_snakeHeadRotation.Y, _snakeHeadRotation.X);

        var tailFinal = (snake[^1] + _tailOffset) * tileSize;
        //renderer.DrawImage(_cellImage, tailFinal, tileSize, 0, Vector2.Zero, snakeColor.Dark(0.04f * snake.Count));
        //renderer.DrawImage(_spriteSheet, tailFinal, tileSize, headRotation, Vector2.Zero, new Rectangle(0, 0, 37, 43), Color.White);

        for(var i = snake.Count - 1; i >= 1; i--)
        {
            var body = _snake[i];
            var bodyDirection = _snake[i - 1] - body;
            var bodyRotation = MathF.Atan2(bodyDirection.Y, bodyDirection.X);

            var offset = tileSize / 2 * bodyDirection;

            renderer.DrawImage(_spriteSheet, body * tileSize + tileSize / 2, tileSize, bodyRotation, new Vector2(18.5f, 21.5f), new Rectangle(0, 0, 37, 43), Color.White);
            renderer.DrawImage(_spriteSheet, body * tileSize + tileSize / 2 + offset, tileSize, bodyRotation, new Vector2(18.5f, 21.5f), new Rectangle(0, 0, 37, 43), Color.White);
        }

        //for (var i = 0; i < snake.Count - 1; i++)
        //{
        //    var body = snake[i];
        //    //renderer.DrawImage(_cellImage, body * tileSize, tileSize, 0, Vector2.Zero, snakeColor.Dark(0.04f * i));
        //}


        var headOrigin = new Vector2(18.5f, 21.5f);
        var direction = _currDirection.ToVector2();
        var neckFinal = Vector2.Lerp(head, head + direction, _t) * tileSize;
        //renderer.DrawImage(_cellImage, neckFinal, tileSize, 0, Vector2.Zero, snakeColor);

        //renderer.DrawImage(_cellImage, headFinal, tileSize, 0, Vector2.Zero, snakeColor);
        renderer.DrawImage(_spriteSheet, headFinal + tileSize / 2, tileSize, headRotation, headOrigin, new Rectangle(0, 43, 37, 43), Color.White);

        //var eyeSize = tileSize / 2.5F;
        //var eyeRotation = headRotation;
        //var eyePosition1 = Vector2.Transform(Vector2.Zero, Matrix3x2.CreateRotation(eyeRotation, tileSize / 2)) + headFinal;
        //var eyePosition2 = Vector2.Transform(new Vector2(0, tileSize.Y - eyeSize.Y), Matrix3x2.CreateRotation(eyeRotation, tileSize / 2)) + headFinal;

        //renderer.DrawImage(_eyeImage, eyePosition1, eyeSize, eyeRotation, Vector2.Zero, new Rectangle(20, 20, 40, 40), Color.White);
        //renderer.DrawImage(_eyeImage, eyePosition2, eyeSize, eyeRotation, Vector2.Zero, new Rectangle(20, 20, 40, 40), Color.White);
    }

    async ValueTask Animation() 
    {
        for (var i = 0; i < 10; i++)
        {
            // Shake animation
            var xOffset = Random.Shared.NextSingle(-.1f, .1f);
            var yOffset = Random.Shared.NextSingle(-.1f, .1f);
            _shakeOffset = new Vector2(xOffset, yOffset);

            await Task.Delay(16);       
        }

        for(var i = 0; i < 12; i++)
        {
            var currDirection = _lastDirection1.ToVector2();
            var p0 = currDirection / 2;
            var p1 = currDirection;
            var tailDirection = _snake[^1] - _snake[^2];

            _snakeHeadOffset = Vector2.Lerp(p0, p1, _goingBackRemaining1 / _goingBackStart1);
            _tailOffset = -Vector2.Lerp(Vector2.Zero, tailDirection, _goingBackRemaining1 / _goingBackStart1);

            await Task.Delay(16);
        }

        _snake.Add(_removedTail1);

        _snakeHeadRotation = _lastDirection1.ToVector2();

        // Copied from the next state!
        var tailDirection1 = _snake[^1] - _snake[^2];
        _tailOffset = -Vector2.Lerp(tailDirection1 / 2, tailDirection1, _goingBackRemaining2 / _goingBackStart2);


        //if (_shakeDurationRemaining > 0)
        //{
        //    _shakeDurationRemaining -= elapsedSeconds;

        //    var xOffset = Random.Shared.NextSingle(-.1f, .1f);
        //    var yOffset = Random.Shared.NextSingle(-.1f, .1f);
        //    _shakeOffset = new Vector2(xOffset, yOffset);

        //    if (_shakeDurationRemaining <= 0)
        //    {
        //        // Initialize next state
        //        ;
        //    }
        //}
        //else if (_goingBackRemaining1 > 0)
        //{
        //    _goingBackRemaining1 -= elapsedSeconds;

        //    var currDirection = _lastDirection1.ToVector2();
        //    var p0 = currDirection / 2;
        //    var p1 = currDirection;
        //    var tailDirection = _snake[^1] - _snake[^2];

        //    _snakeHeadOffset = Vector2.Lerp(p0, p1, _goingBackRemaining1 / _goingBackStart1);
        //    _tailOffset = -Vector2.Lerp(Vector2.Zero, tailDirection, _goingBackRemaining1 / _goingBackStart1);

        //    if (_goingBackRemaining1 <= 0)
        //    {
        //        // Initialize next state
        //        _snake.Add(_removedTail1);

        //        _snakeHeadRotation = _lastDirection1.ToVector2();

        //        // Copied from the next state!
        //        var tailDirection1 = _snake[^1] - _snake[^2];
        //        _tailOffset = -Vector2.Lerp(tailDirection1 / 2, tailDirection1, _goingBackRemaining2 / _goingBackStart2);
        //    }
        //}
    }
}