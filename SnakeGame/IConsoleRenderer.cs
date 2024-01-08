using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGameCore;

public interface IRenderer
{
    void DrawHead(Vector2 vector2, float rotation);

    void DrawBody(Vector2 vector2, float rotation);

    void DrawTail(Vector2 vector2);    
}

public interface IConsoleRenderer
{
    /// <summary>
    /// Called prior to any drawing.
    /// </summary>
    void BeginDraw();

    /// <summary>
    /// Draws the specified character to the console.
    /// </summary>
    /// <param name="character">The character to draw</param>
    /// <remarks>
    /// The origin (0, 0) is the top left corner
    /// </remarks>
    void DrawCharacter(int x, int y, char character);
    
    /// <summary>
    /// Called after drawing was completed.
    /// </summary>
    void EndDraw();
}
