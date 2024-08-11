using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnakeCore;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Transactions;

namespace SnakeFNA
{
    internal class FNARenderer : IRenderer<Texture2D>
    {
        private bool beginCalled;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly int _width;
        private readonly int _height;
        private readonly BasicEffect _effect;

        private VertexPositionColor[] _buffer;
        private int _triangleCount = 0;
        private int _cellSize;
        private Rectangle _prevScissor;
        private SpriteBatch _spriteBatch;
        private Texture2D _pixel;

        public FNARenderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _width = 1; // This is the game cell width and height dont use them!
            _height = 1;

            _effect = new BasicEffect(graphicsDevice);
            _buffer = new VertexPositionColor[4096 * 3];

            _effect.VertexColorEnabled = true;
            
            RasterizerState.CullNone.ScissorTestEnable = true;

            _spriteBatch = new SpriteBatch(graphicsDevice);

            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new Color[] { Color.White });
        }

        public void Begin()
        {
            if(beginCalled)
            {
                throw new InvalidOperationException(
					"Begin has been called before calling End" +
					" after the last call to Begin." +
					" Begin cannot be called again until" +
					" End has been successfully called."
				);
            }

            beginCalled = true;
            _triangleCount = 0;

            //_graphicsDevice.RasterizerState = RasterizerState.CullNone;
            //_graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            var viewport = _graphicsDevice.Viewport;

            _cellSize = Math.Min(viewport.Width, viewport.Height) / Math.Max(_width, _height);
            _cellSize -= (_cellSize % 2);

            var offsetY = (viewport.Height - _height * _cellSize) / 2 + _cellSize / 2;
            var offsetX = (viewport.Width - _width * _cellSize) / 2 + _cellSize / 2;
            var world = Matrix.Identity;
            //world *= Matrix.CreateTranslation(offsetX, offsetY, 0);

            var gameWidth = _cellSize * _width;
            var gameHeight = _cellSize * _height;

            //_prevScissor = _graphicsDevice.ScissorRectangle;
            //_graphicsDevice.ScissorRectangle = new Rectangle(offsetX - _cellSize / 2, offsetY - _cellSize / 2, gameWidth, gameHeight);

            //_effect.World = world;
            //_effect.Projection = Matrix.CreateOrthographicOffCenter(0f, viewport.Width, viewport.Height, 0f, 0f, 1f);
            //_effect.CurrentTechnique.Passes[0].Apply();
            
            var onePixelOffset = Matrix.Identity;
            //onePixelOffset *= Matrix.CreateTranslation(200, 200, 0);


            _spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                null,
                onePixelOffset);
        }

        public void End()
        {
            if (!beginCalled)
            {
                throw new InvalidOperationException(
                    "End was called, but Begin has not yet" +
                    " been called. You must call Begin" +
                    " successfully before you can call End."
                );
            }

            beginCalled = false;


            //_graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _buffer, 0, _triangleCount);
            //_graphicsDevice.ScissorRectangle = _prevScissor;

            _spriteBatch.End();
        }

        public void DrawTriangle(ReadOnlySpan<System.Numerics.Vector2> vertices, float rotation, System.Numerics.Vector2 transition, System.Drawing.Color color)
        {
            if(!beginCalled)
            {
                throw new InvalidOperationException(
                    "DrawTriangle was called, but Begin has not yet" +
                    " been called. You must call Begin" +
                    " successfully before you can call DrawTriangle."
                );
            }
                
            // TODO: If we can implement hardware instancing all these matrix calculations can be moved into the GPU
            var world = System.Numerics.Matrix3x2.Identity;
            world *= System.Numerics.Matrix3x2.CreateScale(_cellSize / 2);
            world *= System.Numerics.Matrix3x2.CreateRotation(rotation);
            world *= System.Numerics.Matrix3x2.CreateTranslation(transition * _cellSize);

            var start = _triangleCount * 3;

            for (var i = 0; i < vertices.Length; i++)
            {
                var vertex = System.Numerics.Vector2.Transform(vertices[i], world);
                var newColor = new Color(color.R, color.G, color.B, color.A);

                _buffer[start].Position = new Vector3(vertex.X, vertex.Y, 0);
                _buffer[start].Color = newColor;
                
                start++;
            }

            _triangleCount += vertices.Length / 3;
        }

        public unsafe Texture2D CreateImage(int width, int height, ReadOnlySpan<byte> data)
        {
            var texture = new Texture2D(_graphicsDevice, width, height, false, SurfaceFormat.Color);
            fixed(byte* p = data) texture.SetDataPointerEXT(0, null, (nint)p, data.Length);
            
            return texture;
        }

        public void DrawImage(Texture2D image, System.Numerics.Vector2 position, System.Numerics.Vector2 size, float rotation, System.Numerics.Vector2 origin, System.Drawing.Rectangle sourceRectangle, System.Drawing.Color color)
        {   
            var destinationRectangle = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
            var xnaSourceRectangle = new Rectangle(sourceRectangle.X, sourceRectangle.Y, sourceRectangle.Width, sourceRectangle.Height);
            var xnaColor = Color.FromNonPremultiplied(color.R, color.G, color.B, color.A);
            var xnaOrigin = new Vector2(origin.X, origin.Y);
            //xnaOrigin = Vector2.Zero;
            _spriteBatch.Draw(image, destinationRectangle, xnaSourceRectangle, xnaColor, rotation, xnaOrigin, SpriteEffects.None, 0);
        }
    }
}
