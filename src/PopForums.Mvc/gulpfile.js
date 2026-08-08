/// <binding BeforeBuild="default" />

var gulp = require("gulp"),
	merge = require("merge-stream"),
	babel = require("gulp-babel"),
	cleancss = require("gulp-clean-css"),
	terser = require("gulp-terser"),
	rename = require("gulp-rename"),
	typescript = require("gulp-typescript");

var project = typescript.createProject("Client/tsconfig.json")
var nodeRoot = "./node_modules/";
var targetPath = "./wwwroot/lib/";

gulp.task("ts", function () {
	return project.src().pipe(project()).js.pipe(gulp.dest("wwwroot"));
});

gulp.task("copies", function () {
	var streams = [
		gulp.src(nodeRoot + "bootstrap/dist/js/bootstrap.bundle.*").pipe(gulp.dest(targetPath + "/bootstrap/dist/js")),
		gulp.src(nodeRoot + "bootstrap/dist/css/bootstrap.css").pipe(gulp.dest(targetPath + "/bootstrap/dist/css")),
		gulp.src(nodeRoot + "bootstrap/dist/css/bootstrap.css.map").pipe(gulp.dest(targetPath + "/bootstrap/dist/css")),
		gulp.src(nodeRoot + "bootstrap/dist/css/bootstrap.min.css").pipe(gulp.dest(targetPath + "/bootstrap/dist/css")),
		gulp.src(nodeRoot + "bootstrap/dist/css/bootstrap.min.css.map").pipe(gulp.dest(targetPath + "/bootstrap/dist/css")),
		gulp.src(nodeRoot + "@microsoft/signalr/dist/browser/**/*").pipe(gulp.dest(targetPath + "/signalr/dist")),
		gulp.src(nodeRoot + "tinymce/**/*").pipe(gulp.dest(targetPath + "/tinymce")),
		gulp.src(nodeRoot + "vue/dist/vue.global.*").pipe(gulp.dest(targetPath + "/vue/dist")),
		gulp.src(nodeRoot + "vue-router/dist/vue-router.global.*").pipe(gulp.dest(targetPath + "/vue-router/dist")),
		gulp.src(nodeRoot + "axios/dist/**/*").pipe(gulp.dest(targetPath + "/axios/dist")),
		gulp.src("./wwwroot/Fonts/**/*").pipe(gulp.dest(targetPath + "/PopForums/dist/Fonts"))
	];
	return merge(streams);
});

function jsTask() {
	var raw = gulp.src("./wwwroot/*.js", { allowEmpty: true })
		.pipe(gulp.dest(targetPath + "/PopForums/dist"));

	var minified = gulp.src("./wwwroot/*.js", { allowEmpty: true, sourcemaps: true })
		.pipe(babel({ presets: ["@babel/preset-env"] }))
		.pipe(terser())
		.pipe(rename({ suffix: '.min' }))
		.pipe(gulp.dest(targetPath + "/PopForums/dist", { sourcemaps: "." }));

	return merge(raw, minified);
}

function cssTask() {
	var raw = gulp.src("./wwwroot/*.css", { allowEmpty: true })
		.pipe(gulp.dest(targetPath + "/PopForums/dist"));

	var minified = gulp.src("./wwwroot/*.css", { allowEmpty: true, sourcemaps: true })
		.pipe(cleancss())
		.pipe(rename({ suffix: '.min' }))
		.pipe(gulp.dest(targetPath + "/PopForums/dist", { sourcemaps: "." }));

	return merge(raw, minified);
}

gulp.task("js", jsTask);
gulp.task("css", cssTask);

gulp.task("default", gulp.series(["ts","copies","js","css"]));