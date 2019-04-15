/// <binding AfterBuild="default" />

var gulp = require("gulp");
var merge = require("merge-stream");

var nodeRoot = "./node_modules/";
var targetPath = "./wwwroot/lib/";

gulp.task("default", function () {
	var streams = [
		gulp.src(nodeRoot + "bootstrap/dist/**/*").pipe(gulp.dest(targetPath + "/bootstrap/dist")),
		gulp.src(nodeRoot + "popper.js/dist/umd/popper.min.js").pipe(gulp.dest(targetPath + "/popper.js/dist")),
		gulp.src(nodeRoot + "jquery/dist/**/*").pipe(gulp.dest(targetPath + "/jquery/dist")),
		gulp.src(nodeRoot + "@aspnet/signalr/dist/browser/**/*").pipe(gulp.dest(targetPath + "/signalr/dist")),
		gulp.src(nodeRoot + "tinymce/**/*").pipe(gulp.dest(targetPath + "/tinymce")),
		gulp.src(nodeRoot + "vue/dist/**/*").pipe(gulp.dest(targetPath + "/vue/dist")),
		gulp.src(nodeRoot + "vue-router/dist/**/*").pipe(gulp.dest(targetPath + "/vue-router/dist")),
		gulp.src(nodeRoot + "axios/dist/**/*").pipe(gulp.dest(targetPath + "/axios/dist"))
	];
	return merge(streams);
});