using System;
using System.Runtime.InteropServices.JavaScript;

namespace WebGL.Sample;

internal static partial class Interop
{
	[JSImport("initialize", "main.js")]
	public static partial void Initialize();

	[JSExport]
	public static void OnKeyPress(string key)
	{
		Test.KeyPresses.Enqueue(key);
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
