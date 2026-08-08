// Front-end asset build: minifies wwwroot/*.js and wwwroot/*.css (with sourcemaps)
// and copies vendored node_modules assets into wwwroot/lib.
// TypeScript compilation (Client/*.ts -> wwwroot/PopForums.js) is handled natively
// by Microsoft.TypeScript.MSBuild as part of `dotnet build`, not here.
const esbuild = require("esbuild");
const fs = require("fs");
const path = require("path");

const wwwroot = "wwwroot";
const distPath = path.join(wwwroot, "lib", "PopForums", "dist");
const libPath = path.join(wwwroot, "lib");
const nodeRoot = "node_modules";

function copyFile(src, dest) {
	fs.mkdirSync(path.dirname(dest), { recursive: true });
	fs.copyFileSync(src, dest);
}

function copyFiles(srcDir, fileNames, destDir) {
	for (const name of fileNames) {
		copyFile(path.join(srcDir, name), path.join(destDir, name));
	}
}

function copyDir(srcDir, destDir) {
	fs.mkdirSync(destDir, { recursive: true });
	fs.cpSync(srcDir, destDir, { recursive: true });
}

function minifyJs(fileName) {
	const src = path.join(wwwroot, fileName);
	const minName = fileName.replace(/\.js$/, ".min.js");
	copyFile(src, path.join(distPath, fileName));
	esbuild.buildSync({
		entryPoints: [src],
		outfile: path.join(distPath, minName),
		bundle: false,
		minify: true,
		sourcemap: true,
		target: "es2018",
		logLevel: "warning"
	});
}

function minifyCss(fileName) {
	const src = path.join(wwwroot, fileName);
	const minName = fileName.replace(/\.css$/, ".min.css");
	copyFile(src, path.join(distPath, fileName));
	esbuild.buildSync({
		entryPoints: [src],
		outfile: path.join(distPath, minName),
		bundle: false,
		minify: true,
		sourcemap: true,
		logLevel: "warning"
	});
}

fs.mkdirSync(distPath, { recursive: true });

for (const fileName of fs.readdirSync(wwwroot)) {
	if (fileName.endsWith(".js")) minifyJs(fileName);
	else if (fileName.endsWith(".css")) minifyCss(fileName);
}

copyFiles(
	path.join(nodeRoot, "bootstrap", "dist", "js"),
	["bootstrap.bundle.js", "bootstrap.bundle.js.map", "bootstrap.bundle.min.js", "bootstrap.bundle.min.js.map"],
	path.join(libPath, "bootstrap", "dist", "js")
);
copyFiles(
	path.join(nodeRoot, "bootstrap", "dist", "css"),
	["bootstrap.css", "bootstrap.css.map", "bootstrap.min.css", "bootstrap.min.css.map"],
	path.join(libPath, "bootstrap", "dist", "css")
);
copyDir(path.join(nodeRoot, "@microsoft", "signalr", "dist", "browser"), path.join(libPath, "signalr", "dist"));
copyDir(path.join(nodeRoot, "tinymce"), path.join(libPath, "tinymce"));
copyFiles(
	path.join(nodeRoot, "vue", "dist"),
	["vue.global.js", "vue.global.prod.js"],
	path.join(libPath, "vue", "dist")
);
copyFiles(
	path.join(nodeRoot, "vue-router", "dist"),
	["vue-router.global.js", "vue-router.global.prod.js"],
	path.join(libPath, "vue-router", "dist")
);
copyDir(path.join(nodeRoot, "axios", "dist"), path.join(libPath, "axios", "dist"));
copyDir(path.join(wwwroot, "Fonts"), path.join(distPath, "Fonts"));
