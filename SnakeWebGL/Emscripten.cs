using System;
using System.Runtime.InteropServices;

namespace SnakeWebGL;

internal static class Emscripten
{
	[DllImport("emscripten", EntryPoint = "emscripten_request_animation_frame_loop")]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	internal static extern unsafe void RequestAnimationFrameLoop(void* f, nint userDataPtr);

    // emscripten_webgl_create_context

    [DllImport("emscripten", EntryPoint = "emscripten_webgl_get_current_context")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static extern unsafe IntPtr WebGlGetCurrentContext();

    [DllImport("emscripten", EntryPoint = "emscripten_webgl_get_context_attributes")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static extern unsafe int WebGlGetContextAttributes(IntPtr context, out EmscriptenWebGLContextAttributes userDataPtr);

    //EM_BOOL emscripten_webgl_enable_extension(EMSCRIPTEN_WEBGL_CONTEXT_HANDLE context, const char* extension)

    [DllImport("emscripten", EntryPoint = "emscripten_webgl_enable_extension")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    internal static extern unsafe bool WebGlEnableExtension(IntPtr context, string extension);

}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct EmscriptenWebGLContextAttributes
{
    public bool alpha;
    public bool depth;
    public bool stencil;
    public bool antialias;
    public bool premultipliedAlpha;
    public bool preserveDrawingBuffer;
    public int powerPreference; // THIS IS A ENUM https://github.com/emscripten-core/emscripten/blob/380a9dda76a35fdd4f4cd94c606d6d375862392a/system/include/emscripten/html5_webgl.h#L24
    public bool failIfMajorPerformanceCaveat;

    public int majorVersion;
    public int minorVersion;

    public bool enableExtensionsByDefault;
    public bool explicitSwapControl;
    public int proxyContextToMainThread;
    public bool renderViaOffscreenBackBuffer;

    public override string ToString()
    {
        return $"{{ Alpha: {alpha}, " +
            $"Depth: {depth}, " +
            $"Stencil: {stencil}, " +
            $"Antialias: {antialias}, " +
            $"PremultipliedAlpha: {premultipliedAlpha}, " +
            $"PreserveDrawingBuffer: {preserveDrawingBuffer}, " +
            $"PowerPreference: {powerPreference}, " +
            $"FailIfMajorPerformanceCaveat: {failIfMajorPerformanceCaveat}, " +
            $"MajorVersion: {majorVersion}, " +
            $"MinorVersion: {minorVersion}, " +
            $"EnableExtensionsByDefault: {enableExtensionsByDefault}, " +
            $"ExplicitSwapControl: {explicitSwapControl}, " +
            $"ProxyContextToMainThread: {proxyContextToMainThread}, " +
            $"RenderViaOffscreenBackBuffer: {renderViaOffscreenBackBuffer}}}";
    }
}
