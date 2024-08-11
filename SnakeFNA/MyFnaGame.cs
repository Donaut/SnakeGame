using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace SnakeFNA
{
    internal class MyFnaGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly SnakeCore.Game _game;
        private FNARenderer? _renderer;

        private KeyboardState _previousKeyBoard;
        private KeyboardState _currentKeyBoard;

        [STAThread]
        static void Main(string[] args)
        {
            using (var game = new MyFnaGame())
            {
                game.Run();
            }
        }

        private MyFnaGame()
        {
            _game = new SnakeCore.Game();
            _graphics = new GraphicsDeviceManager(this)
            {
                // We need this for anti aliasing to work
                GraphicsProfile = GraphicsProfile.HiDef,
                PreferMultiSampling = true,

                PreferredBackBufferWidth = 750,
                PreferredBackBufferHeight = 750,

            };

            IsMouseVisible = true;

            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            _graphics.ApplyChanges(); // By default the MSAA changes doesn't get aplied at startup.

            
        }

        protected override void LoadContent()
        {
            _renderer = new FNARenderer(GraphicsDevice);
            _game.Initialize(_renderer);
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            _currentKeyBoard = Keyboard.GetState();

            var direction = SnakeCore.Direction.None;
            if (IsKeyPressed(Keys.W))
                direction = SnakeCore.Direction.Up;
            else if (IsKeyPressed(Keys.D))
                direction = SnakeCore.Direction.Right;
            else if (IsKeyPressed(Keys.S))
                direction = SnakeCore.Direction.Down;
            else if (IsKeyPressed(Keys.A))
                direction = SnakeCore.Direction.Left;
            
            //if(IsKeyPressed(Keys.Space))
            //{
            //    _game.Update(0.016f, SnakeCore.Direction.Up);
            //}

            _game.Update((float)gameTime.ElapsedGameTime.TotalSeconds, direction);
            base.Update(gameTime);

            _previousKeyBoard = _currentKeyBoard;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if(_renderer == null)
            {
                throw new InvalidOperationException("Renderer is null.");
            }

            _renderer.Begin();

            var viewPort = GraphicsDevice.Viewport;
            var width = viewPort.Width;
            var height = viewPort.Height;
            _game.Draw((float)gameTime.ElapsedGameTime.TotalSeconds, _renderer);

            _renderer.End();

            base.Draw(gameTime);
        }

        private bool IsKeyPressed(Keys key)
        {
            return _currentKeyBoard.IsKeyDown(key) && !_previousKeyBoard.IsKeyDown(key);
        }
    }
}
