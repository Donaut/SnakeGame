using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SnakeCore;

internal static class IRendererExtensions
{
    public static void DrawImage(this IRenderer renderer, ImageHandle image, Vector2 position, Vector2 size, float rotation, Vector2 origin, Color color)
    {
        renderer.DrawImage(image, position, size, rotation, origin, new Rectangle(0, 0, image.Width, image.Height), color);
    }
}
