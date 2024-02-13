using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SnakeFNA
{
    internal class FNARenderer : SnakeCore.IRenderer
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

        public FNARenderer(GraphicsDevice graphicsDevice, int width, int height)
        {
            _graphicsDevice = graphicsDevice;
            _width = width;
            _height = height;

            _effect = new BasicEffect(graphicsDevice);
            _buffer = new VertexPositionColor[4096 * 3];

            _effect.VertexColorEnabled = true;
            
            RasterizerState.CullNone.ScissorTestEnable = true;
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

            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            _graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            var viewport = _graphicsDevice.Viewport;

            _cellSize = Math.Min(viewport.Width, viewport.Height) / Math.Max(_width, _height);
            _cellSize -= (_cellSize % 2);

            var offsetY = (viewport.Height - _height * _cellSize) / 2 + _cellSize / 2;
            var offsetX = (viewport.Width - _width * _cellSize) / 2 + _cellSize / 2;
            var world = Matrix.Identity;
            world *= Matrix.CreateTranslation(offsetX, offsetY, 0);

            var gameWidth = _cellSize * _width;
            var gameHeight = _cellSize * _height;

            _prevScissor = _graphicsDevice.ScissorRectangle;
            _graphicsDevice.ScissorRectangle = new Rectangle(offsetX - _cellSize / 2, offsetY - _cellSize / 2, gameWidth, gameHeight);

            _effect.World = world;
            _effect.Projection = Matrix.CreateOrthographicOffCenter(0f, viewport.Width, viewport.Height, 0f, 0f, 1f);
            _effect.CurrentTechnique.Passes[0].Apply();

            

            
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

            

            _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _buffer, 0, _triangleCount);
            _graphicsDevice.ScissorRectangle = _prevScissor;
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
    }
}
