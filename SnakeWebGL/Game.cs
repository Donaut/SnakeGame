using Silk.NET.OpenGLES;
using SnakeCore;
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("browser")]

namespace SnakeWebGL;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexPositionColor
{
    public Vector2 Position;
    public Vector3 Color;
};

public class Game
{
    private WebGlRenderer _renderer;
    private SnakeCore.Game _game;

    private Game(GL gl, int canvasWidth, int canvasHeight)
    {
        _game = new();
        _renderer = new WebGlRenderer(gl, _game.Width, _game.Height, canvasWidth, canvasHeight);
        
        _game.Initialize();
    }

    public void Update(float elapsedSeconds, Direction direction)
    {
        _game.Update(elapsedSeconds, direction);
    }

    public void Draw()
    {
        _renderer.BeginRender();

        _game.Draw(0.016f, _renderer);

        _renderer.EndRender();
    }

    internal void CanvasResized(int canvasWidth, int canvasHeight)
    {
        _renderer.CanvasWidth = canvasWidth;
        _renderer.CanvasHeight = canvasHeight;
    }

    public static Game Create(GL gl, int canvasWidth, int canvasHeight)
    {
        return new Game(gl, canvasWidth, canvasHeight);
    }

    class WebGlRenderer : IRenderer
    {
        private readonly GL Gl;

        private VertexPositionColor[] _buffer;
        
        private readonly uint _shaderProgram;
        private readonly int _modelLocation;
        private readonly int _projectionLocation;
        private readonly uint _vao;
        private readonly uint _vbo;

        private int _cellSize;
        private int _triangleCount;

        public int GameWidth { get; set; }

        public int GameHeight { get; set; }

        public int CanvasWidth { get; set; }

        public int CanvasHeight { get; set; }

        public unsafe WebGlRenderer(GL gl, int gameWidth, int gameHeight, int canvasWidth, int canvasHeight)
        {
            Gl = gl;
            GameWidth = gameWidth;
            GameHeight = gameHeight;

            CanvasWidth = canvasWidth;
            CanvasHeight = canvasHeight;

            _buffer = new VertexPositionColor[4096 * 2];

            _shaderProgram = Gl.CreateProgram();
            
            var vertexSource =
                """
                #version 300 es
                
                layout(location = 0) in mediump vec2 vertexPos;
                layout(location = 1) in mediump vec3 vertexCol;
                
                out highp vec3 color;
                
                // GLSL uses the reverse order to a System.Numerics.Matrix3x2
                uniform mat4 model;
                uniform mat4 projection;
                
                void main()
                {
                	gl_Position = projection * model * vec4(vertexPos.xy, 1.0, 1.0);
                	color = vertexCol;
                }
                """;

            var vertexShader = gl.CreateShader(ShaderType.VertexShader);
            gl.ShaderSource(vertexShader, vertexSource);
            gl.CompileShader(vertexShader);
            gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int res);

            var fragmentSource =
                """
                #version 300 es

                in highp vec3 color;

                out highp vec4 diffuse;

                void main()
                {    
                    diffuse = vec4(color, 1.0);
                }  
                """;

            var fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
            gl.ShaderSource(fragmentShader, fragmentSource);
            gl.CompileShader(fragmentShader);

            gl.AttachShader(_shaderProgram, vertexShader);
            gl.AttachShader(_shaderProgram, fragmentShader);
            gl.LinkProgram(_shaderProgram);


            _modelLocation = gl.GetUniformLocation(_shaderProgram, "model"u8);
            _projectionLocation = gl.GetUniformLocation(_shaderProgram, "projection"u8);

            _vao = Gl.GenVertexArray();
            Gl.BindVertexArray(_vao);

            _vbo = Gl.GenBuffer();

            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            
            gl.EnableVertexAttribArray(0); // vertex
            gl.EnableVertexAttribArray(1); // color

            int vert_size = Marshal.SizeOf<Vector2>();
            int colr_size = Marshal.SizeOf<Vector3>();
            int stride = Marshal.SizeOf<VertexPositionColor>();

            gl.VertexAttribPointer(0, vert_size / sizeof(float), VertexAttribPointerType.Float, false, (uint)stride, (void*)0);
            gl.VertexAttribPointer(1, colr_size / sizeof(float), VertexAttribPointerType.Float, false, (uint)stride, (void*)vert_size);

            // cleanup
            Gl.BindVertexArray(0);
        }

        public void BeginRender()
        {
            _cellSize = Math.Min(CanvasWidth, CanvasHeight) / Math.Max(GameWidth, GameHeight);
            _cellSize -= (_cellSize % 2);

            _triangleCount = 0;
        }

        public void DrawTriangle(ReadOnlySpan<Vector2> vertices, float rotation, Vector2 transition, Color color)
        {
            var world = Matrix3x2.Identity;
            world *= Matrix3x2.CreateScale(_cellSize / 2);
            world *= Matrix3x2.CreateRotation(rotation);
            world *= Matrix3x2.CreateTranslation(transition * _cellSize);

            var start = _triangleCount * 3;

            for (var i = 0; i < vertices.Length; i++)
            {
                var vertex = Vector2.Transform(vertices[i], world);

                _buffer[start].Position = vertex;
                _buffer[start].Color = new Vector3(color.R / (float)byte.MaxValue, color.G / (float)byte.MaxValue, color.B / (float)byte.MaxValue);

                start++;
            }

            _triangleCount += vertices.Length / 3;
        }

        public unsafe void EndRender()
        {
            var offsetX = (CanvasWidth - GameWidth * _cellSize) / 2 + _cellSize / 2;
            var offsetY = (CanvasHeight - GameHeight * _cellSize) / 2 + _cellSize / 2;

            var world = Matrix4x4.Identity;
            world *= Matrix4x4.CreateTranslation(offsetX, offsetY, 0);

            var projection = Matrix4x4.CreateOrthographicOffCenter(0f, CanvasWidth, CanvasHeight, 0f, 0f, 2f);

            Gl.UseProgram(_shaderProgram);
            Gl.UniformMatrix4(_modelLocation, 1, false, (float*)&world);
            Gl.UniformMatrix4(_projectionLocation, 1, false, (float*)&projection);

            Gl.BindVertexArray(_vao);

            Gl.Viewport(0, 0, (uint)CanvasWidth, (uint)CanvasHeight);

            // Update buffer
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            Gl.BufferData<VertexPositionColor>(GLEnum.ArrayBuffer, _buffer.AsSpan(0, _triangleCount * 3), BufferUsageARB.StreamDraw);

            // dispatch GL commands
            Gl.ClearColor(0.392f, 0.584f, 0.929f, 1.0f);
            Gl.Clear(ClearBufferMask.ColorBufferBit);

            // draw
            Gl.DrawArrays(GLEnum.Triangles, 0, (uint)_triangleCount * 3);
            
            // Cleanup
            Gl.UseProgram(0);
            Gl.BindVertexArray(0);
        }
    }
}
