// @ts-ignore
import { dotnet } from './_framework/dotnet.js';

Sentry.init({
	dsn: 'https://e17358aac4344e759b8f1b748f8c1544@todo.dyndns.hu/1'
})

// https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop?view=aspnetcore-8.0
const { setModuleImports, getAssemblyExports, getConfig } = await dotnet
	.withDiagnosticTracing(false)
	.withApplicationArgumentsFromQuery()
	.create();

const config = getConfig();

const exports = await getAssemblyExports(config.mainAssemblyName);
console.log(exports);
const interop = exports.WebGL.Sample.Interop;

var canvas = globalThis.document.getElementById("canvas");
dotnet.instance.Module["canvas"] = canvas;

const keyBoard = {
	prevKeys: {},
	currKeys: {}
}

setModuleImports("main.js", {
	initialize: () => {
		var checkCanvasResize = (dispatch) => {
			var devicePixelRatio = window.devicePixelRatio || 1.0;
			var displayWidth = canvas.clientWidth * devicePixelRatio;
			var displayHeight = canvas.clientHeight * devicePixelRatio;

			// @ts-ignore
			if (canvas.width != displayWidth || canvas.height != displayHeight) {
				// @ts-ignore
				canvas.width = displayWidth;
				// @ts-ignore
				canvas.height = displayHeight;
				dispatch = true;
			}

			if (dispatch) interop.OnCanvasResize(displayWidth, displayHeight, devicePixelRatio);
		}

		function checkCanvasResizeFrame() {
			checkCanvasResize(false);
			requestAnimationFrame(checkCanvasResizeFrame); // The callback only called after this method returns.
		}

		/** @param {KeyboardEvent} e */
		var keyDown = (e) => {
			keyBoard.currKeys[e.code] = true;
		};

		/** @param {KeyboardEvent} e */
		var keyUp = (e) => {
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

	isKeyPressed: (key) => {
		var res = !keyBoard.currKeys[key] && keyBoard.prevKeys[key];

		return res;
	}
});

await dotnet.run();
