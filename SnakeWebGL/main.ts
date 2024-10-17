// @ts-ignore
import { dotnet } from './_framework/dotnet.js';

const { setModuleImports, getAssemblyExports, getConfig, runMain } = await dotnet
    //.withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    //.withApplicationArguments("start")
    .create();
const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);
const interop = exports.SnakeWebGL.Interop;

var canvas = globalThis.document.getElementById("canvas") as HTMLCanvasElement;
dotnet.instance.Module["canvas"] = canvas;

const keyBoard: { [key: string]: any } = {
    prevKeys: {},
    currKeys: {}
}

const resizeObserver = new ResizeObserver(onResize);
try {
    // only call us of the number of device pixels changed
    resizeObserver.observe(canvas, { box: 'device-pixel-content-box' });
} catch (ex) {
    // device-pixel-content-box is not supported so fallback to this
    resizeObserver.observe(canvas, { box: 'content-box' });
}

setModuleImports("main.js", {
    initialize: () => {
        function step() {
            requestAnimationFrame(step); // The callback only called after this method returns.
        }

        var keyDown = (e: KeyboardEvent) => {
            keyBoard.currKeys[e.code] = true;
        };

        var keyUp = (e: KeyboardEvent) => {
            keyBoard.currKeys[e.code] = false;
        };

        canvas.addEventListener("keydown", keyDown, false);
        canvas.addEventListener("keyup", keyUp, false);
        step();
    },

    updateInput: () => {
        keyBoard.prevKeys = { ...keyBoard.currKeys };
    },

    isKeyPressed: (key: string) => {
        return !keyBoard.currKeys[key] && keyBoard.prevKeys[key];
    }
});

//await runMain();
await dotnet.run();

function onResize(entries: ResizeObserverEntry[]) {
    for (const entry of entries) {
        let width;
        let height;
        let dpr = window.devicePixelRatio;
        if (entry.devicePixelContentBoxSize) {
            // NOTE: Only this path gives the correct answer
            // The other paths are imperfect fallbacks
            // for browsers that don't provide anyway to do this
            width = entry.devicePixelContentBoxSize[0].inlineSize;
            height = entry.devicePixelContentBoxSize[0].blockSize;
            dpr = 1; // it's already in width and height
        } else if (entry.contentBoxSize) {
            if (entry.contentBoxSize[0]) {
                width = entry.contentBoxSize[0].inlineSize;
                height = entry.contentBoxSize[0].blockSize;
            } else {
                // but old versions of Firefox treat it as a single item
                width = entry.contentBoxSize?.inlineSize;
                height = entry.contentBoxSize?.blockSize;
            }
        } else {
            width = entry.contentRect.width;
            height = entry.contentRect.height;
        }
        const displayWidth = Math.round(width * dpr);
        const displayHeight = Math.round(height * dpr);
        interop?.OnCanvasResize(canvas.width, canvas.height);
    }
}
