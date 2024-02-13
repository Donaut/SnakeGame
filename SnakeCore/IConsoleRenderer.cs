using System.Drawing;
using System.Numerics;

namespace SnakeCore;

public interface IRenderer
{
    void DrawTriangle(ReadOnlySpan<Vector2> vertices, float rotation, Vector2 transition, Color color);
}
