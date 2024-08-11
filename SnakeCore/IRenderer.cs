using System.Collections;
using System.Drawing;
using System.Numerics;

namespace SnakeCore;

public interface IRenderer<T> : IRenderer
{
    new T CreateImage(int width, int height, ReadOnlySpan<byte> data);

    ImageHandle IRenderer.CreateImage(int width, int height, ReadOnlySpan<byte> data) => new ImageHandle<T>(CreateImage(width, height, data), width, height);

    //Image CreateImage(int Width, int Height, ReadOnlySpan<byte> data);

    void DrawImage(T image, Vector2 position, Vector2 size, float rotation, Vector2 origin, Rectangle sourceRectangle, Color color);

    void IRenderer.DrawImage(ImageHandle image, Vector2 position, Vector2 size, float rotation, Vector2 origin, Rectangle sourceRectangle, Color color)
    {
        if (!(image is ImageHandle<T> handle))
        {
            throw new InvalidCastException("Failed to cast image to ImageHandle<T>.");
        }

        DrawImage(handle.Value, position, size, rotation, origin, sourceRectangle, color);
    } 
}

public record class ImageHandle<T>(T Value, int Width, int Height) : ImageHandle(Width, Height);


public interface IRenderer
{
    ImageHandle CreateImage(int width, int height, ReadOnlySpan<byte> data);

    void DrawImage(ImageHandle image, Vector2 position, Vector2 size, float rotation, Vector2 origin, Rectangle sourceRectangle, Color color);

    void DrawTriangle(ReadOnlySpan<Vector2> vertices, float rotation, Vector2 transition, Color color);
}


public record class ImageHandle(int Width, int Height);