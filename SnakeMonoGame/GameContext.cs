using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using SnakeGameCore;
using System;
using System.Diagnostics;

namespace SnakeMonoGame
{
    public class GameContext : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private SnakeGameCore.Game _game;
        private MonoGameRenderer _renderer;

        private KeyboardState _currentKeyState;
        private KeyboardState _previousKeyState;

        public GameContext()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            _game = new SnakeGameCore.Game(10, 10);
            _game.Reset();

            base.Initialize();
            
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            var head = Content.Load<Texture2D>("./snake_head");
            var body = Content.Load<Texture2D>("./snake_body");

            _renderer = new MonoGameRenderer(_spriteBatch, head, body, body, _game.Width, _game.Height);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _currentKeyState = Keyboard.GetState();

            var pressedKeys = _currentKeyState.GetPressedKeys();

            var direction = Direction.None;
            for (var i = 0; i < pressedKeys.Length; i++)
            {
                var key = pressedKeys[i];

                if(key != Keys.W && key != Keys.D && key != Keys.S && key != Keys.A)
                {
                    continue;
                }

                if(IsKeyDown(key))
                {
                    direction = key switch
                    {
                        Keys.W => Direction.Up,
                        Keys.D => Direction.Right,
                        Keys.S => Direction.Down,
                        Keys.A => Direction.Left,
                    };
                    break;
                }

            }

            
            //if (IsKeyDown(Keys.W))
            //{
            //    if (direction != Directions.None) Trace.WriteLine($"Direction was not registered: {direction}");

            //    direction = Directions.Up;
            //}
            //if (IsKeyDown(Keys.D))
            //{
            //    if (direction != Directions.None) Trace.WriteLine($"Direction was not registered: {direction}");

            //    direction = Directions.Right;
            //}
            //if (IsKeyDown(Keys.S))
            //{
            //    if (direction != Directions.None) Trace.WriteLine($"Direction was not registered: {direction}");

            //    direction = Directions.Down;
            //}
            //if (IsKeyDown(Keys.A))
            //{
            //    if (direction != Directions.None) Trace.WriteLine($"Direction was not registered: {direction}");

            //    direction = Directions.Left;
            //}

            Trace.WriteLineIf(direction != Direction.None, $"Direction : {direction}");

            _game.Update((float)gameTime.ElapsedGameTime.TotalSeconds, direction);

            base.Update(gameTime);

            _previousKeyState = _currentKeyState;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _renderer.BeginDraw();

            _game.Draw((float)gameTime.ElapsedGameTime.TotalSeconds, _renderer);

            _renderer.EndDraw();

            base.Draw(gameTime);
        }

        public bool IsKeyDown(Keys key)
        {
            return _currentKeyState.IsKeyDown(key) && !_previousKeyState.IsKeyDown(key);
        }
    }

    class MonoGameRenderer : SnakeGameCore.IRenderer
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _snakeHead;
        private readonly Texture2D _snakeHeadOpen;
        private readonly Texture2D _snakeBody;
        private readonly int _width;
        private readonly int _height;

        private readonly int _cellSize;
        private readonly Texture2D _cell;
        private readonly int _scale = 2;

        public MonoGameRenderer(SpriteBatch spriteBatch, Texture2D snakeHead, Texture2D snakeHeadOpen, Texture2D snakeBody, int width, int height)
        {
            _spriteBatch = spriteBatch;
            _snakeHead = snakeHead;
            _snakeHeadOpen = snakeHeadOpen;
            _snakeBody = snakeBody;
            _width = width;
            _height = height;

            _cellSize = 16;
            _cell = new Texture2D(spriteBatch.GraphicsDevice, _cellSize, _cellSize);
            var a = new Color[_cell.Width * _cell.Height];
            Array.Fill(a, Color.White);
            _cell.SetData(a);
        }

        public void BeginDraw()
        {
            var transform = Matrix.Identity;
            _spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);

            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    var color = (y + x) % 2 == 0 ? new Color(165, 168, 113) : new Color(36, 159, 57);
                    var position = new Vector2(x, y) * _cellSize * _scale;
                    _spriteBatch.Draw(_cell, position, null, color, 0, Vector2.Zero, _scale, SpriteEffects.None, 0);
                }
            }
        }

        public void DrawHead(System.Numerics.Vector2 position, float rotation)
        {
            Draw(_snakeHead, position, rotation);
        }

        public void DrawBody(System.Numerics.Vector2 position, float rotation)
        {
            var r = Color.White.R;
            var g = Color.White.G;
            var b = Color.White.B;
            
            var color = new Color(r, g, b, byte.MaxValue * .9f);
            Draw(_snakeBody, position, rotation, color);
        }

        public void DrawTail(System.Numerics.Vector2 position)
        {
            Draw(_snakeBody, position, 0);
        }

        private void Draw(Texture2D texture, Vector2 position, float rotation, Color? color = null)
        {
            color ??= Color.White;

            position.X *= _snakeBody.Width + (_cellSize * _scale - texture.Width);
            position.Y *= _snakeBody.Height + (_cellSize * _scale - texture.Height);

            position += new System.Numerics.Vector2(_cellSize * _scale / 2F);

            _spriteBatch.Draw(texture, position, null, color.Value, rotation, new Vector2(texture.Width / 2, texture.Height / 2), _scale, SpriteEffects.None, 0);
        }

        public void EndDraw()
        {
            _spriteBatch.End();
        }
    }
}
