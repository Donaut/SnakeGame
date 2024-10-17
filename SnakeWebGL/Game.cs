using Silk.NET.OpenGLES;
using SnakeCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("browser")]

namespace SnakeWebGL;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexPositionColorTexture
{
    public Vector2 Position;
    public Vector3 Color;
    public Vector2 Texture;
};

public record Texture2D
{
    public uint Handle { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
}

public class Game
{
    private WebGlRenderer _renderer;
    private SnakeCore.Game _game;


    private Game(GL gl)
    {
        _game = new();
        _renderer = new WebGlRenderer(gl, _game.DesignWidth, _game.DesignHeight);

        _game.Initialize(_renderer);
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
        _renderer.SetCanvasSize(canvasWidth, canvasHeight);
    }

    public static Game Create(GL gl)
    {
        return new Game(gl);
    }

    class WebGlRenderer : IRenderer<Texture2D>
    {
        private readonly GL Gl;
        private readonly int _gameWidth;
        private readonly int _gameHeight;

        private readonly uint _shaderProgram;
        private readonly int _mvpLocation;
        private readonly uint _vao;
        private readonly uint _vbo;

        private int _bufferPosition;
        private int _texturesPosition;
        private VertexPositionColorTexture[] _buffer;
        private Texture2D[] _textures;

        public unsafe WebGlRenderer(GL gl, int gameWidth, int gameHeight)
        {
            Gl = gl;
            _gameWidth = gameWidth;
            _gameHeight = gameHeight;

            _buffer = new VertexPositionColorTexture[1024];
            _textures = new Texture2D[_buffer.Length / 6];

            _shaderProgram = Gl.CreateProgram();

            var vertexSource =
                """
                #version 300 es
                
                layout(location = 0) in highp vec2 vertexPos;
                layout(location = 1) in mediump vec3 vertexCol;
                layout(location = 2) in highp vec2 vertexUv;

                out highp vec3 color;
                out highp vec2 uv;

                // GLSL uses the reverse order to a System.Numerics.Matrix3x2
                uniform mat4 mvp;
                
                void main()
                {
                	gl_Position = mvp * vec4(vertexPos.xy, 0, 1.0);
                	color = vertexCol;
                    uv = vertexUv;
                }
                """;

            //Shader
            var vertexShader = gl.CreateShader(ShaderType.VertexShader);
            gl.ShaderSource(vertexShader, vertexSource);
            gl.CompileShader(vertexShader);
            var compileStatus = (SpecialNumbers)gl.GetShader(vertexShader, ShaderParameterName.CompileStatus);
            if(compileStatus == SpecialNumbers.False)
                throw new Exception($"Failed to compile the vertex shader.");
            
            var fragmentSource =
                """
                #version 300 es

                in highp vec3 color;
                in highp vec2 uv;

                uniform sampler2D u_texture;

                out highp vec4 diffuse;

                void main()
                {    
                   diffuse = vec4(color, 1.0);
                   //diffuse = vec4(uv, 0, 1.0);
                   diffuse = texture(u_texture, uv) * vec4(color, 1.0);
                   //diffuse = texture(u_texture, uv) * vec4(1.0);
                }  
                """;

            var fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
            gl.ShaderSource(fragmentShader, fragmentSource);
            gl.CompileShader(fragmentShader);
            compileStatus = (SpecialNumbers)gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus);
            if(compileStatus == SpecialNumbers.False)
                throw new Exception("Failed to compile the fragment shader.");

            gl.AttachShader(_shaderProgram, vertexShader);
            gl.AttachShader(_shaderProgram, fragmentShader);
            gl.LinkProgram(_shaderProgram);

            _mvpLocation = gl.GetUniformLocation(_shaderProgram, "mvp"u8);

            _vao = Gl.GenVertexArray();
            Gl.BindVertexArray(_vao);

            _vbo = Gl.GenBuffer();

            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

            gl.EnableVertexAttribArray(0); // vertex
            gl.EnableVertexAttribArray(1); // color
            gl.EnableVertexAttribArray(2); // uv

            int vert_size = Marshal.SizeOf<Vector2>();
            int colr_size = Marshal.SizeOf<Vector3>();
            int text_size = Marshal.SizeOf<Vector2>();
            int stride = Marshal.SizeOf<VertexPositionColorTexture>();

            gl.VertexAttribPointer(0, vert_size / sizeof(float), VertexAttribPointerType.Float, false, (uint)stride, (void*)0);
            gl.VertexAttribPointer(1, colr_size / sizeof(float), VertexAttribPointerType.Float, false, (uint)stride, (void*)vert_size);
            gl.VertexAttribPointer(2, text_size / sizeof(float), VertexAttribPointerType.Float, false, (uint)stride, (void*)(vert_size + colr_size));
            // cleanup
            Gl.BindVertexArray(0);
        }

        Texture2D IRenderer<Texture2D>.CreateImage(int width, int height, ReadOnlySpan<byte> data)
        {
            var id = Gl.GenTexture();
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2D, id);

            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

            Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            //Gl.GenerateMipmap(TextureTarget.Texture2D);

            return new Texture2D() {
                Handle = id,
                Width = width,
                Height = height,
            };
        }

        public unsafe void DrawImage(Texture2D texture, Vector2 position, Vector2 size, float rotation, Vector2 origin, Rectangle sourceRectangle, Color color)
        {
            Span<VertexPositionColorTexture> vertices = _buffer.AsSpan(_bufferPosition, 6);

            var sourceX = sourceRectangle.X / (float)texture.Width;
            var sourceY = sourceRectangle.Y / (float)texture.Height;
            var sourceW = sourceRectangle.Width / (float)texture.Width;
            var sourceH = sourceRectangle.Height / (float)texture.Height;

            var rotationSin = MathF.Sin(rotation);
            var rotationCos = MathF.Cos(rotation);

            float cornerX = -origin.X * size.X;
            float cornerY = -origin.Y * size.Y;
            vertices[0].Position.X = (
                (-rotationSin * cornerY) +
                (rotationCos * cornerX) +
                position.X
            );
            vertices[0].Position.Y = (
                (rotationCos * cornerY) +
                (rotationSin * cornerX) +
                position.Y
            );
            cornerX = (1.0f - origin.X) * size.X;
            cornerY = -origin.Y * size.Y;
            vertices[1].Position.X = (
                (-rotationSin * cornerY) +
                (rotationCos * cornerX) +
                position.X
            );
            vertices[1].Position.Y = (
                (rotationCos * cornerY) +
                (rotationSin * cornerX) +
                position.Y
            );
            cornerX = -origin.X * size.X;
            cornerY = (1.0f - origin.Y) * size.Y;
            vertices[2].Position.X = (
                (-rotationSin * cornerY) +
                (rotationCos * cornerX) +
                position.X
            );
            vertices[2].Position.Y = (
                (rotationCos * cornerY) +
                (rotationSin * cornerX) +
                position.Y
            );


            cornerX = (1.0f - origin.X) * size.X;
            cornerY = -origin.Y * size.Y;
            vertices[3].Position.X = (
                (-rotationSin * cornerY) +
                (rotationCos * cornerX) +
                position.X
            );
            vertices[3].Position.Y = (
                (rotationCos * cornerY) +
                (rotationSin * cornerX) +
                position.Y
            );
            cornerX = (1.0f - origin.X) * size.X;
            cornerY = (1.0f - origin.Y) * size.Y;
            vertices[4].Position.X = (
                (-rotationSin * cornerY) +
                (rotationCos * cornerX) +
                position.X
            );
            vertices[4].Position.Y = (
                (rotationCos * cornerY) +
                (rotationSin * cornerX) +
                position.Y
            );
            cornerX = -origin.X * size.X;
            cornerY = (1.0f - origin.Y) * size.Y;
            vertices[5].Position.X = (
                (-rotationSin * cornerY) +
                (rotationCos * cornerX) +
                position.X
            );
            vertices[5].Position.Y = (
                (rotationCos * cornerY) +
                (rotationSin * cornerX) +
                position.Y
            );

            vertices[0].Texture.X = sourceX;
            vertices[0].Texture.Y = sourceH + sourceY;
            vertices[1].Texture.X = sourceW + sourceX;
            vertices[1].Texture.Y = sourceH + sourceY;
            vertices[2].Texture.X = sourceX;
            vertices[2].Texture.Y = sourceY;
            vertices[3].Texture.X = sourceW + sourceX;
            vertices[3].Texture.Y = sourceH + sourceY;
            vertices[4].Texture.X = sourceW + sourceX;
            vertices[4].Texture.Y = sourceY;
            vertices[5].Texture.X = sourceX;
            vertices[5].Texture.Y = sourceY;

            var colorF = new Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
            vertices[0].Color = colorF;
            vertices[1].Color = colorF;
            vertices[2].Color = colorF;
            vertices[3].Color = colorF;
            vertices[4].Color = colorF;
            vertices[5].Color = colorF;

            _textures[_texturesPosition] = texture;

            _bufferPosition += 6;
            _texturesPosition += 1;
        }

        public void BeginRender()
        {
            _bufferPosition = 0;
            _texturesPosition = 0;
        }

        public unsafe void EndRender()
        {
            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactor.SrcColor, BlendingFactor.One);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
            Gl.BlendFunc(BlendingFactor.DstColor, BlendingFactor.OneMinusSrcColor);
            Gl.BlendFunc(BlendingFactor.DstAlpha, BlendingFactor.OneMinusSrcAlpha);

            var projection = Matrix4x4.CreateOrthographicOffCenter(0f, _gameWidth, _gameHeight, 0f, 0f, 2f);
            Gl.UseProgram(_shaderProgram);
            Gl.UniformMatrix4(_mvpLocation, 1, false, (float*)&projection);

            Gl.BindVertexArray(_vao);

            // Update buffer
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            Gl.BufferData<VertexPositionColorTexture>(GLEnum.ArrayBuffer, _buffer.AsSpan(0, _bufferPosition), BufferUsageARB.StreamDraw);

            // dispatch GL commands
            Gl.ClearColor(0.392f, 0.584f, 0.929f, 1.0f);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Texture2D curTexture = _textures[0];
            var offset = 0;
            for(var i = 1; i < _texturesPosition; i++)
            {
                var nextTexture = _textures[i];
                if(curTexture != nextTexture)
                {
                    Gl.BindTexture(TextureTarget.Texture2D, curTexture.Handle);
                    Gl.DrawArrays(GLEnum.Triangles, offset * 6, (uint)(i - offset) * 6);

                    curTexture = nextTexture;
                    offset = i;
                }
            }

            if(offset != _texturesPosition)
            {
                Gl.BindTexture(TextureTarget.Texture2D, curTexture.Handle);
                Gl.DrawArrays(GLEnum.Triangles, offset * 6, (uint)(_texturesPosition - offset) * 6);
            }
        }

        internal void SetCanvasSize(int canvasWidth, int canvasHeight)
        {
            Gl.Viewport(0, 0, (uint)canvasWidth, (uint)canvasHeight);
        }
    }
}
