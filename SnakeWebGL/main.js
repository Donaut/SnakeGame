import { dotnet } from './dotnet.js'

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

setModuleImports("main.js", {
	initialize: () => {

		var checkCanvasResize = (dispatch) => {
			var devicePixelRatio = window.devicePixelRatio || 1.0;
			var displayWidth = canvas.clientWidth * devicePixelRatio;
			var displayHeight = canvas.clientHeight * devicePixelRatio;

			if (canvas.width != displayWidth || canvas.height != displayHeight) {
				canvas.width = displayWidth;
				canvas.height = displayHeight;
				dispatch = true;
			}

			if (dispatch)
				interop.OnCanvasResize(displayWidth, displayHeight, devicePixelRatio);
		}

		function checkCanvasResizeFrame() {
			checkCanvasResize(false);
			requestAnimationFrame(checkCanvasResizeFrame);
		}

		/** @param {KeyboardEvent} e */
		var keyPress = (e) => {
			e.stopPropagation();
			if (e.repeat) return;
			let key = e.key;
			interop.OnKeyPress(key);
		};

		/** @param {MouseEvent} e */
		var mouseMove = (e) => {
			let x = e.offsetX;
			let y = e.offsetY;
			interop.OnMouseMove(x, y);
		}

		/** @param {MouseEvent} e */
		var mouseDown = (e) => {
			var shift = e.shiftKey;
			var ctrl = e.ctrlKey;
			var alt = e.altKey;
			var button = e.button;
			interop.OnMouseDown(shift, ctrl, alt, button);
		}

		/** @param {MouseEvent} e */
		var mouseUp = (e) => {
			var shift = e.shiftKey;
			var ctrl = e.ctrlKey;
			var alt = e.altKey;
			var button = e.button;
			interop.OnMouseUp(shift, ctrl, alt, button);
		}

		canvas.addEventListener("keydown", keyPress, false);
		canvas.addEventListener("mousemove", mouseMove, false);
		canvas.addEventListener("mousedown", mouseDown, false);
		canvas.addEventListener("mouseup", mouseUp, false);
		checkCanvasResize(true);
		checkCanvasResizeFrame();

		canvas.tabIndex = 1000;
	},
	
});

await dotnet.run();
