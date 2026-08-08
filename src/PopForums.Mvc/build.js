// Front-end asset build: compiles Client/*.ts -> wwwroot/PopForums.js, minifies
// wwwroot/*.js and wwwroot/*.css (with sourcemaps), and copies vendored node_modules
// assets into wwwroot/lib. Must be fully self-sufficient and runnable before `dotnet
// build` (e.g. in CI): Microsoft.TypeScript.MSBuild also compiles TypeScript as part
// of `dotnet build`, but that's for local-dev hot reload convenience, not something
// this script can depend on running first.
const esbuild = require("esbuild");
const fs = require("fs");
const path = require("path");
const { execFileSync } = require("child_process");

const wwwroot = "wwwroot";
const distPath = path.join(wwwroot, "lib", "PopForums", "dist");
const libPath = path.join(wwwroot, "lib");
const nodeRoot = "node_modules";

// The MSBuild target that runs this script depends on CompileTypeScript, which already
// compiled wwwroot/PopForums.js by the time we get here - skip the redundant recompile.
// Standalone invocations (npm run build, CI) need it since nothing else will have.
if (!process.env.POPFORUMS_SKIP_TSC) {
	execFileSync(path.join(nodeRoot, ".bin", "tsc"), ["-p", path.join("Client", "tsconfig.json")], { stdio: "inherit" });
}

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
