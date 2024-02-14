using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;

using Silk.NET.OpenGLES;
using SnakeWebGL;

[assembly: SupportedOSPlatform("browser")]

namespace WebGL.Sample;

public static class Test
{
    private static Game? Game { get; set; }

    private static double? currentTime = null; // Milliseconds, 100 is 0.1 Seconds
    private static double accumulator = 0; // Seconds, 1.5 is 1 second and 500 millisecond

    [UnmanagedCallersOnly]
    public static int Frame(double newTime, int userData)
    {
        ArgumentNullException.ThrowIfNull(Game);

        const float dt = 1 / 60f; // 60 FPS (1 / 60 = 0.01666666)
        currentTime ??= newTime;

        var frameTime = (newTime - (int)currentTime) / 1000;
        accumulator += frameTime;

        var direction = SnakeCore.Direction.None;
        if (Interop.IsKeyPressed(Keys.W))
            direction = SnakeCore.Direction.Up;
        else if (Interop.IsKeyPressed(Keys.D))
            direction = SnakeCore.Direction.Right;
        else if (Interop.IsKeyPressed(Keys.S))
            direction = SnakeCore.Direction.Down;
        else if (Interop.IsKeyPressed(Keys.A))
            direction = SnakeCore.Direction.Left;

        if(accumulator >= dt)
        {
            Interop.UpdateInput();
        }

        while (accumulator >= dt)
        {
            Game.Update(dt, direction);
            accumulator -= dt;
        }

        Game.Draw();

        currentTime = newTime;
        return 1; // The return value should be a bolean, false if the Frame is canceled and the animation should stop
    }

    private static int CanvasWidth { get; set; }

    private static int CanvasHeight { get; set; }

    public static void CanvasResized(int width, int height)
    {
        CanvasWidth = width;
        CanvasHeight = height;
        Game?.CanvasResized(CanvasWidth, CanvasHeight);
        //Demo?.CanvasResized(CanvasWidth, CanvasHeight);
    }

    public async static Task Main(string[] args)
    {
        Console.WriteLine($"Hello from dotnet!");

        var display = EGL.GetDisplay(IntPtr.Zero);
        if (display == IntPtr.Zero)
            throw new Exception("Display was null");

        if (!EGL.Initialize(display, out int major, out int minor))
            throw new Exception("Initialize() returned false.");

        int[] attributeList = new int[]
        {
            EGL.EGL_RED_SIZE  , 8,
            EGL.EGL_GREEN_SIZE, 8,
            EGL.EGL_BLUE_SIZE , 8,
            //EGL.EGL_DEPTH_SIZE, 24,
            //EGL.EGL_STENCIL_SIZE, 8,
            //EGL.EGL_SURFACE_TYPE, EGL.EGL_WINDOW_BIT,
            //EGL.EGL_RENDERABLE_TYPE, EGL.EGL_OPENGL_ES3_BIT,
            EGL.EGL_SAMPLES, 16, //MSAA, 16 samples
            //EGL.EGL_SAMPLES, 0, //MSAA, 16 samples
            EGL.EGL_NONE
        };

        var config = IntPtr.Zero;
        var numConfig = IntPtr.Zero;
        if (!EGL.ChooseConfig(display, attributeList, ref config, (IntPtr)1, ref numConfig))
            throw new Exception("ChoseConfig() failed");
        if (numConfig == IntPtr.Zero)
            throw new Exception("ChoseConfig() returned no configs");

        if (!EGL.BindApi(EGL.EGL_OPENGL_ES_API))
            throw new Exception("BindApi() failed");

        // No other attribute is supported...
        int[] ctxAttribs = new int[]
        {
            EGL.EGL_CONTEXT_CLIENT_VERSION, 3,
            EGL.EGL_NONE 
        };

        var context = EGL.CreateContext(display, config, (IntPtr)EGL.EGL_NO_CONTEXT, ctxAttribs);
        if (context == IntPtr.Zero)
            throw new Exception("CreateContext() failed");

        // now create the surface
        var surface = EGL.CreateWindowSurface(display, config, IntPtr.Zero, IntPtr.Zero);
        if (surface == IntPtr.Zero)
            throw new Exception("CreateWindowSurface() failed");

        if (!EGL.MakeCurrent(display, surface, surface, context))
            throw new Exception("MakeCurrent() failed");

        //_ = EGL.DestroyContext(display, context);
        //_ = EGL.DestroySurface(display, surface);
        //_ = EGL.Terminate(display);

        TrampolineFuncs.ApplyWorkaroundFixingInvocations();
        
        var gl = GL.GetApi(EGL.GetProcAddress);
        Interop.Initialize();

        Game = Game.Create(gl, CanvasWidth, CanvasHeight);

        unsafe
        {
            // https://emscripten.org/docs/api_reference/html5.h.html?highlight=emscripten_request_animation_frame#c.emscripten_request_animation_frame_loop
            Emscripten.RequestAnimationFrameLoop((delegate* unmanaged<double, int, int>)&Frame, 10);
        }
    }
}
