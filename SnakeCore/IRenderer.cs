using System.Drawing;
using System.Numerics;

namespace SnakeCore;

public interface IRenderer<T> : IRenderer
{
    new T CreateImage(int width, int height, ReadOnlySpan<byte> data);

    ImageHandle IRenderer.CreateImage(int width, int height, ReadOnlySpan<byte> data) => new ImageHandle<T>(CreateImage(width, height, data), width, height);

    void DrawImage(T image, Vector2 position, Vector2 size, float rotation, Vector2 origin, Rectangle sourceRectangle, Color color);

    void IRenderer.DrawImage(ImageHandle image, Vector2 position, Vector2 size, float rotation, Vector2 origin, Rectangle sourceRectangle, Color color)
    {
        if (image is not ImageHandle<T> handle)
        {
            throw new InvalidCastException($"Failed to cast image to ImageHandle<{typeof(T).Name}>.");
        }

        DrawImage(handle.Value, position, size, rotation, origin, sourceRectangle, color);
    } 
}

public interface IRenderer
{
    ImageHandle CreateImage(int width, int height, ReadOnlySpan<byte> data);

    void DrawImage(ImageHandle image, Vector2 position, Vector2 size, float rotation, Vector2 origin, Rectangle sourceRectangle, Color color);
}

public record class ImageHandle<T>(T Value, int Width, int Height) : ImageHandle(Width, Height);

public record class ImageHandle(int Width, int Height);