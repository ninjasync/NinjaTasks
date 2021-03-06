﻿/// <binding Clean='clean' ProjectOpened='sass:watch' />
"use strict";

var gulp = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    sass = require("gulp-sass");

var paths = {
    webroot: "./wwwroot/"
};

var itemsToCopy = {
    './node_modules/angular/angular*.js': paths.webroot + 'lib/angular',
    './node_modules/angular-ui-router/release/angular*.js': paths.webroot + 'lib/angular'
};

paths.js = paths.webroot + "js/**/*.js";
paths.minJs = paths.webroot + "js/**/*.min.js";
paths.css = paths.webroot + "css/**/*.css";
paths.minCss = paths.webroot + "css/**/*.min.css";
paths.concatJsDest = paths.webroot + "js/site.min.js";
paths.concatCssDest = paths.webroot + "css/site.min.css";

paths.sassSource = paths.webroot + "css/**/*.scss"; //where to find sass code
paths.cssOutput = paths.webroot + "css"; //where to output compiled CSS code

gulp.task("clean:js", function (cb) {
    rimraf(paths.concatJsDest, cb);
});

gulp.task("clean:css", function (cb) {
    rimraf(paths.concatCssDest, cb);
});

gulp.task("clean", ["clean:js", "clean:css"]);

gulp.task("min:js", function () {
    return gulp.src([paths.js, "!" + paths.minJs], { base: "." })
        .pipe(concat(paths.concatJsDest))
        .pipe(uglify())
        .pipe(gulp.dest("."));
});

gulp.task('sass', function () {
    gulp.src(paths.sassSource)
        .pipe(sass().on('error', sass.logError))
        .pipe(gulp.dest(paths.cssOutput));
});
gulp.task('sass:watch', function () { //Watch task
    gulp.watch(paths.sassSource, ['sass', 'min:css']);
});


gulp.task("min:css", function () {
    return gulp.src([paths.css, "!" + paths.minCss])
        .pipe(concat(paths.concatCssDest))
        .pipe(cssmin())
        .pipe(gulp.dest("."));
});

gulp.task("min", ["min:js", "min:css"]);

gulp.task('copy', function () {
    for (var src in itemsToCopy) {
        if (!itemsToCopy.hasOwnProperty(src)) continue;
        gulp.src(src)
        .pipe(gulp.dest(itemsToCopy[src]));
    }
});
