using SnakeWebGL;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

namespace WebGL.Sample;

internal static partial class Interop
{
    [JSImport("initialize", "main.js")]
    public static partial void Initialize();

    [JSImport("isKeyPressed", "main.js")]
    public static partial bool IsKeyPressed(string code);

    [JSImport("updateInput", "main.js")]
    public static partial void UpdateInput();

    [JSExport]
    public static void OnKeyAdd(string key)
    {

    }

    [JSExport]
    public static void OnKeyRemove(string key)
    {
        
    }

    [JSExport]
    public static void OnMouseMove(float x, float y)
    {

    }

    [JSExport]
    public static void OnMouseDown(bool shift, bool ctrl, bool alt, int button)
    {
    }

    [JSExport]
    public static void OnMouseUp(bool shift, bool ctrl, bool alt, int button)
    {
    }

    [JSExport]
    public static void OnCanvasResize(float width, float height, float devicePixelRatio)
    {
        Test.CanvasResized((int)width, (int)height);
    }

    
}
