/// <binding AfterBuild="default" />

var gulp = require("gulp");
var merge = require("merge-stream");

var nodeRoot = "./node_modules/";
var targetPath = "./wwwroot/lib/";

gulp.task("default", function () {
	var streams = [
		gulp.src(nodeRoot + "bootstrap/dist/**/*").pipe(gulp.dest(targetPath + "/bootstrap/dist")),
		gulp.src(nodeRoot + "jquery/dist/**/*").pipe(gulp.dest(targetPath + "/jquery/dist")),
		gulp.src(nodeRoot + "@aspnet/signalr/dist/**/*").pipe(gulp.dest(targetPath + "/jquery/dist")),
		gulp.src(nodeRoot + "tinymce/**/*").pipe(gulp.dest(targetPath + "/tinymce"))
	];
	return merge(streams);
});