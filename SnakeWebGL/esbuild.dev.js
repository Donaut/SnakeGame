import * as esbuild from 'esbuild'
await esbuild.build({
    entryPoints: ['main.ts'],
    format: 'esm',
    bundle: true,
    external: ['./_framework/*'],
    outfile: './wwwroot/main.js',
    //minify: true,
    //sourcemap: 'external'
})