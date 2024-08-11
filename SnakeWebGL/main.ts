// @ts-ignore
import { dotnet } from './_framework/dotnet.js';
import * as Sentry from "@sentry/browser";

const { setModuleImports, getAssemblyExports, getConfig } = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();
const config = getConfig();
const isDebug = config?.applicationEnvironment == 'Development';



Sentry.init({
    dsn: 'https://e17358aac4344e759b8f1b748f8c1544@todo.dyndns.hu/1',
    release: '1.0.0',
    //debug: isDebug,
    environment: isDebug ? "debug" : undefined,
    integrations: [

        // If you use a bundle with performance monitoring enabled, add the BrowserTracing integration
        Sentry.browserTracingIntegration(),
        // If you use a bundle with session replay enabled, add the Replay integration
        //Sentry.replayIntegration(),
    ],
    tracesSampleRate: isDebug ? 1.0 : 0.3,
});


const exports = await getAssemblyExports(config.mainAssemblyName);

console.log(`Is debug: ${isDebug}`);
console.log(`C# exports: ${exports}`);
console.log(`dotnet: ${dotnet}`);
console.log(`config: ${config}`);

const interop = exports.WebGL.Sample.Interop;

var canvas = globalThis.document.getElementById("canvas") as HTMLCanvasElement;
dotnet.instance.Module["canvas"] = canvas;

const keyBoard: { [key: string]: any } = {
    prevKeys: {},
    currKeys: {}
}

setModuleImports("main.js", {
    initialize: () => {
        var checkCanvasResize = (dispatch: boolean) => {
            var devicePixelRatio = window.devicePixelRatio || 1.0;
            var displayWidth = window.innerWidth * devicePixelRatio;
            var displayHeight = window.innerHeight * devicePixelRatio;

            if (canvas.width != displayWidth || canvas.height != displayHeight) {
                canvas.width = window.innerWidth;
                canvas.height = window.innerHeight;
                dispatch = true;
            }

            if (dispatch) interop.OnCanvasResize(displayWidth, displayHeight, devicePixelRatio);
        }

        function checkCanvasResizeFrame() {
            checkCanvasResize(false);
            requestAnimationFrame(checkCanvasResizeFrame); // The callback only called after this method returns.
        }

        var keyDown = (e: KeyboardEvent) => {
            keyBoard.currKeys[e.code] = true;
        };

        var keyUp = (e: KeyboardEvent) => {
            keyBoard.currKeys[e.code] = false;
        };

        canvas.addEventListener("keydown", keyDown, false);
        canvas.addEventListener("keyup", keyUp, false);
        checkCanvasResize(true);
        checkCanvasResizeFrame();

        canvas.tabIndex = 1000;
    },

    updateInput: () => {
        keyBoard.prevKeys = { ...keyBoard.currKeys };
    },

    isKeyPressed: (key: string) => {
        var res = !keyBoard.currKeys[key] && keyBoard.prevKeys[key];

        return res;
    }
});

await dotnet.run();