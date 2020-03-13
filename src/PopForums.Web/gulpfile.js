/// <binding AfterBuild="default" />
const babel = require('gulp-babel');
const bump = require('gulp-bump');
const concat = require('gulp-concat');
const cssmin = require('gulp-cssmin');
const gulp = require('gulp');
const injectStr = require('gulp-inject-string');
const merge = require('merge-stream');
const path = require('path');
const rename = require('gulp-rename');
const resx_out = require('gulp-resx-out');
const sourcemaps = require('gulp-sourcemaps');
const uglify = require('gulp-uglify');

const nodeRoot = "./node_modules/";
const targetPath = "./wwwroot/lib/";

gulp.task("copies", function () {
	return merge([
		gulp.src(nodeRoot + "bootstrap/dist/**/*").pipe(gulp.dest(targetPath + "/bootstrap/dist")),
		// excluding popper as this is included in the .bundle.min bootstrap packages
		// gulp.src(nodeRoot + "popper.js/dist/umd/popper.min.js").pipe(gulp.dest(targetPath + "/popper.js/dist")),
		gulp.src(nodeRoot + "jquery/dist/**/*").pipe(gulp.dest(targetPath + "/jquery/dist")),
		gulp.src(nodeRoot + "@microsoft/signalr/dist/browser/**/*").pipe(gulp.dest(targetPath + "/signalr/dist")),
		gulp.src(nodeRoot + "tinymce/**/*").pipe(gulp.dest(targetPath + "/tinymce")),
		gulp.src(nodeRoot + "vue/dist/**/*").pipe(gulp.dest(targetPath + "/vue/dist")),
		gulp.src(nodeRoot + "vue-router/dist/**/*").pipe(gulp.dest(targetPath + "/vue-router/dist")),
		gulp.src(nodeRoot + "axios/dist/**/*").pipe(gulp.dest(targetPath + "/axios/dist")),
		gulp.src(targetPath + "PopForums/src/Fonts/**/*").pipe(gulp.dest(targetPath + "/PopForums/dist/Fonts"))
	]);
});

gulp.task("js", function() {
	return merge([["Admin.js"], ["timesTranslated.js", "BrowserDateTimeCulture.js", "PopForums.js"]]
		.map((globs) => 
			gulp.src(globs.map((g) => "./wwwroot/lib/PopForums/src/" + g))
				.pipe(sourcemaps.init())
				.pipe(babel({ presets: ["@babel/preset-env"], sourceMap: true }))
				.pipe(concat(globs[globs.length - 1]))
				.pipe(uglify())
				.pipe(rename({ suffix: '.min' }))
				.pipe(sourcemaps.write("./"))
				.pipe(gulp.dest(targetPath + "/PopForums/dist"))));
});

gulp.task("css", function () {
	return gulp.src("./wwwroot/lib/PopForums/src/*.css")
		.pipe(cssmin())
		.pipe(rename({ suffix: '.min' }))
		.pipe(gulp.dest(targetPath + "/PopForums/dist"));
});

gulp.task("bump", function () {
	return gulp.src("./wwwroot/lib/PopForums/package.json")
		.pipe(bump({ type: "prerelease" }))
		.pipe(gulp.dest("./wwwroot/lib/PopForums/"));
});

gulp.task("resx2js", function () {
	//return string that should be written to file
	function onwrite(result, file) {
		const fileName = path.parse(file.path).name;
		const langMatch = /\.([\w-]+)$/.exec(fileName);
		const lang = langMatch === null
			? "en"
			: langMatch[1];
		return `timesTranslated["${lang}"] = ${JSON.stringify(result)};`;
	}

	const timeStrs = ["LessThanMinute", "MinutesAgo", "TodayTime", "YesterdayTime"];
	//return null to skip, or object { name: '', value: '' }
	function onparse(item, element, result, file) {
		if (timeStrs.includes(item.name)) {
			return {
				name: item.name,
				value: item.value
			};
		}
		return null;
	}
	return gulp.src("./../PopForums/Resources/*.resx")
		.pipe(resx_out({
			delimiter: '.',
			onwrite,
			onparse
		}))
		.pipe(concat("timesTranslated.js"))
		.pipe(injectStr.prepend('const timesTranslated = {};'))
		.pipe(gulp.dest(targetPath + "/PopForums/src"));
});

gulp.task("min", gulp.series(["resx2js", "copies", "js", "css", "bump"]));

gulp.task("default", gulp.series("min"));
