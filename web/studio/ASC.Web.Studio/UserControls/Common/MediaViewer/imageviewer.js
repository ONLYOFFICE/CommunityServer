/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


window.ASC.Files.ImageViewer = (function () {
    var scalingInProcess = false;
    var currentFileName;

    var isInit;

    var imgRef;
    var header;
    var zoomPercBox;

    var odimensions = {};
    var ndimensions = {};
    var oScale = 0;
    var cScale = 0;
    var nScale = 0;
    var scaleStep = 10;
    var rotateAngel = 0;
    var mouseOffset;
    var windowDimensions = {};
    var imageAreaDimensions = {};
    var centerArea = {};
    var onImageLoad;
    var onNextImage;
    var onError;
    var onLock;

    var options;
    var headerHeight;
    var controlsHeight;

    var zoomInBtn;
    var zoomOutBtn;
    var zoomToActualBtn;
    var rotateLeftBtn;
    var rotateRightBtn;

    var init = function (optionsParam, headerRef, headerH, controlsH, onLoad, nextImage, error, lockChange) {
        if (isInit || !optionsParam) {
            return;
        }
        isInit = true;

        options = optionsParam;
        header = headerRef;
        headerHeight = controlsH;
        controlsHeight = controlsH;
        onImageLoad = onLoad;
        onNextImage = nextImage;
        onError = error;
        onLock = lockChange;

        imgRef = jq("#imageViewerContainer");
        zoomPercBox = jq("#imageViewerInfo");

        zoomInBtn = jq("#imageZoomIn");
        zoomOutBtn = jq("#imageZoomOut");
        zoomToActualBtn = jq("#imageFullScale");
        rotateLeftBtn = jq("#imageRotateLeft");
        rotateRightBtn = jq("#imageRotateRight");

        return {
            getPreviewHash: getPreviewHash,

            setOptions: setOptions,
            showImage: showImage,

            closeImageViewer: closeImageViewer,
            resetWorkspace: resetWorkspace,

            rotateImage: rotateImage,
            zoomImage: zoomImage,
            zoomImageFull: zoomImageFull
        };
    };

    var setOptions = function (optionsParam) {
        if (!isInit || !optionsParam) {
            return;
        }

        options = optionsParam;
    };

    var getPreviewHash = function (fileId) {
        return "preview/" + fileId;
    };

    var prepareWorkspace = function () {
        imgRef.show();

        imgRef.on("dblclick", mouseDoubleClickEvent);
        imgRef.on("mousedown", mouseDownEvent);
        imgRef.on("touchmove", touchMoveEvent);
        imgRef.on("touchend", touchEndEvent);
        imgRef.on("load", imageOnLoad);
        imgRef.on("error", function () {
            onError(ASC.Files.FilesJSResources.PreviewError.format(currentFileName));
        });

        jq(window).bind("resize.ImageViewer", calculateDimensions);
        jq(document).bind("mousewheel.ImageViewer", mouseWheelEvent);
        jq(document).bind("touchmove.ImageViewer", multiTouchEvent);

        zoomInBtn.on("click", function () { zoomImage(true); });
        zoomOutBtn.on("click", function () { zoomImage(false); });
        zoomToActualBtn.on("click", function () { zoomImageFull(); });
        rotateLeftBtn.on("click", function () { rotateImage(true); });
        rotateRightBtn.on("click", function () { rotateImage(false); });
    };

    var resetWorkspace = function () {
        imgRef.hide();
        imgRef.removeAttr("src");

        scalingInProcess = false;
        onLock(false);

        jq(document).unbind("mousemove.ImageViewer");
        jq(document).unbind("mouseup.ImageViewer");

        imgRef.off("dblclick", mouseDoubleClickEvent);
        imgRef.off("mousedown", mouseDownEvent);
        imgRef.off("touchmove", touchMoveEvent);
        imgRef.off("touchend", touchEndEvent);
        imgRef.off("load", imageOnLoad);

        jq(window).unbind("resize.ImageViewer");
        jq(document).unbind("mousewheel.ImageViewer");
        jq(document).unbind("touchmove.ImageViewer");

        zoomInBtn.off("click");
        zoomOutBtn.off("click");
        zoomToActualBtn.off("click");
        rotateLeftBtn.off("click");
        rotateRightBtn.off("click");
    };

    var getnewSize = function (side, nvalue) {
        var otherside = (side == "w") ? "h" : "w";
        if (typeof nvalue == "undefined" || nvalue == null) {
            var newSize = ndimensions[otherside] * odimensions[side] / odimensions[otherside];
        } else {
            newSize = (/%/.test(nvalue)) ? parseInt(nvalue) / 100 * odimensions[side] : parseInt(nvalue);
        }
        ndimensions[side] = Math.round(newSize);
    };

    var setNewDimensions = function (setting, callbackHandle) {
        var sortbysize = (odimensions.w > odimensions.h) ? ["w", "h"] : ["h", "w"];
        setting[sortbysize[0]] = setting.ls;
        setting[sortbysize[1]] = null;

        var sortbyavail = (setting.w) ? ["w", "h"] : (setting.h) ? ["h", "w"] : [];

        var oldDimensions = { w: ndimensions.w, h: ndimensions.h };

        getnewSize(sortbyavail[0], setting[sortbyavail[0]]);
        getnewSize(sortbyavail[1], setting[sortbyavail[1]]);

        scalingInProcess = true;
        onLock(true);

        if (typeof callbackHandle == "undefined" || callbackHandle == null) {
            callbackHandle = function () {
                scalingInProcess = false;
                onLock(false);
                zoomPercBox.hide();
                cScale = nScale;

                var top = parseFloat(imgRef.css("top"));
                var left = parseFloat(imgRef.css("left"));

                if (left < -ndimensions.w || windowDimensions.w < left || top < -ndimensions.h || windowDimensions.h < top)
                    imgRef.css({
                        "left": centerArea.w - ndimensions.w / 2,
                        "top": centerArea.h - ndimensions.h / 2
                    });
            };
        }

        if (typeof setting.speed == "undefined" || setting.speed == null) {
            setting.speed = 250;
        }

        var getDimensionOffset = function (side) {
            if (setting.toPtr) {
                var diff = (ndimensions[side] - oldDimensions[side]) / 2 + (oldDimensions[side] / 2 - mouseOffset[side == "h" ? "y" : "x"]) * (1 - ndimensions[side] / oldDimensions[side]);
            } else if (setting.toCenter) {
                return centerArea[side] - (ndimensions[side] / 2);
            } else {
                diff = (ndimensions[side] - oldDimensions[side]) / 2;
            }

            if (diff > 0) {
                return "-=" + diff + "px";
            } else {
                return "+=" + (-diff) + "px";
            }
        };

        if (setting.speed != 0) {
            imgRef.animate({ width: ndimensions.w + "px", height: ndimensions.h + "px", left: getDimensionOffset("w"), top: getDimensionOffset("h") },
                {
                    duration: setting.speed,
                    easing: "linear",
                    complete: callbackHandle,
                    step: function (now, obj) {
                        jq("#imageViewerInfo span").text(cScale + Math.round((nScale - cScale) * obj.pos) + "%");
                    }
                });
        } else {
            imgRef.css({ width: ndimensions.w + "px", height: ndimensions.h + "px", left: getDimensionOffset("w"), top: getDimensionOffset("h") });
            callbackHandle();
        }
    };

    var imageOnLoad = function () {
        imgRef[0].style.cssText = "";

        var tempimg = jq("<img src=\"" + imgRef.attr("src") + "\" class=\"image-viewer-temp\" />").prependTo("body");

        odimensions = { w: tempimg.width(), h: tempimg.height() };

        if (odimensions.w == 0 && odimensions.h == 0) {
            odimensions.w = imgRef.attr("naturalWidth") || imgRef[0].naturalWidth;
            odimensions.h = imgRef.attr("naturalHeight") || imgRef[0].naturalHeight;
        }

        var similarityFactors = { w: odimensions.w / imageAreaDimensions.w, h: odimensions.h / imageAreaDimensions.h };
        var lsf = (similarityFactors.w > similarityFactors.h) ? "w" : "h";

        nScale = (similarityFactors[lsf] > 1) ?
            Math.round(imageAreaDimensions[lsf] * 100.0 / odimensions[lsf]) :
            100;

        nScale = Math.max(nScale, scaleStep);

        ndimensions = { w: 10, h: 10 };

        imgRef.css({
            "left": centerArea.w - ndimensions.w / 2,
            "top": centerArea.h - ndimensions.h / 2
        }).show();

        oScale = nScale;

        setNewDimensions({ speed: 0, ls: nScale + "%" });

        tempimg.remove();

        resetImageInfo();

        onImageLoad();
    };

    var resetImageInfo = function () {
        if (isImageLoadCompleted()) {
            var imageName = currentFileName;
            var text = "{0} ({1}x{2})".format(imageName, odimensions.w, odimensions.h);
            header.text(text);
        }
    };

    var isImageLoadCompleted = function () {
        return !jq.isEmptyObject(odimensions);
    };

    var showImage = function (ext, fileId, fileName) {
        odimensions = {};
        ndimensions = {};
        rotateAngel = 0;
        scalingInProcess = false;
        currentFileName = fileName;

        prepareWorkspace();
        calculateDimensions();

        imgRef.hide();

        var tiffreg = /^\.tif[f]?$/gi;

        if (tiffreg.test(ext)) {
            var xhr = new XMLHttpRequest();
            xhr.responseType = 'arraybuffer';
            xhr.open('GET', options.downloadAction(fileId));
            xhr.onload = function (e) {
                var tiff = new Tiff({ buffer: xhr.response });

                imgRef.attr("src", tiff.toDataURL());
            };
            xhr.send();
        }
        else {
            imgRef.attr("src", options.downloadAction(fileId));
        }
    };

    var mouseDownEvent = function (event) {
        if (scalingInProcess || event.which != 1) {
            return false;
        }

        mouseOffset = { x: event.pageX - imgRef.offset().left, y: event.pageY - imgRef.offset().top };

        jq(document).unbind("mouseup.ImageViewer");
        jq(document).bind("mouseup.ImageViewer", mouseUpEvent);

        jq(document).unbind("mousemove.ImageViewer");
        jq(document).bind("mousemove.ImageViewer", mouseMoveEvent);

        document.ondragstart = function () {
            return false;
        };
        document.body.onselectstart = function () {
            return false;
        };
        imgRef[0].ondragstart = function () {
            return false;
        };

        return false;
    };

    var mouseMoveEvent = function (event) {
        var x = event.pageX,
            y = event.pageY,
            w = windowDimensions.w,
            h = windowDimensions.h;

        x = x < 0 ? 0 : x > w ? w : x;
        y = y < 0 ? 0 : y > h ? h : y;

        imgRef.css({
            "cursor": "pointer",
            "left": x - mouseOffset.x,
            "top": y - mouseOffset.y
        });
    };


    var lastTouchY;
    var lastTouchX;
    var offsetTouchY;
    var offsetTouchX;
    var isTouchMoving;
    var startedTouch;

    var swipeDurationThreshold = 1000; // More time than this, and it isn't a swipe.
    var swipeHorizontalDistanceThreshold = 30; // Swipe horizontal displacement must be more than this.
    var swipeVerticalDistanceThreshold = 10; // Swipe vertical displacement must be less than this.

    var pinchZoomThreshold = 10; // Pinch zoom displacement must be more than this.

    var touchMoveEvent = function (e) {
        if (e.originalEvent.touches.length > 1)
        {
            e.originalEvent.preventDefault();
            return true;
        }

        var touchY = e.originalEvent.touches[0].clientY;
        var touchX = e.originalEvent.touches[0].clientX;

        if (!startedTouch) {
            lastTouchY = touchY;
            lastTouchX = touchX;
            startedTouch = (new Date()).getTime();
            return false;
        }

        offsetTouchY = lastTouchY - touchY;
        offsetTouchX = lastTouchX - touchX;

        lastTouchY = touchY;
        lastTouchX = touchX;

        imgRef.css({
            "left": parseFloat(imgRef.css("left")) - offsetTouchX,
            "top": parseFloat(imgRef.css("top")) - offsetTouchY
        });

        return false;
    };

    var multiTouchEvent = function (e) {
        if (e.originalEvent.touches.length <= 1) {
            return true;
        }

        e.stopPropagation();
        e.originalEvent.preventDefault();

        if (!lastTouchX) {
            lastTouchY = e.originalEvent.touches[0].clientY;
            lastTouchX = e.originalEvent.touches[0].clientX;
            offsetTouchY = e.originalEvent.touches[1].clientY;
            offsetTouchX = e.originalEvent.touches[1].clientX;
            return false;
        }

        var touchY = e.originalEvent.touches[0].clientY;
        var touchX = e.originalEvent.touches[0].clientX;
        var secondaryTouchY = e.originalEvent.touches[1].clientY;
        var secondaryTouchX = e.originalEvent.touches[1].clientX;

        function distance(x1, y1, x2, y2) {
            return Math.sqrt(Math.pow(x2 - x1, 2) + Math.pow(y2 - y1, 2));
        }

        var oldDistance = distance(lastTouchX, lastTouchY, offsetTouchX, offsetTouchY);
        var newDistance = distance(touchX, touchY, secondaryTouchX, secondaryTouchY);

        if (Math.abs(newDistance - oldDistance) > pinchZoomThreshold) {
            mouseOffset = { x: (touchX + secondaryTouchX) / 2 - imgRef.offset().left, y: (touchY + secondaryTouchY) / 2 - imgRef.offset().top };

            zoomImage(oldDistance < newDistance, true);

            lastTouchY = touchY;
            lastTouchX = touchX;
            offsetTouchY = secondaryTouchY;
            offsetTouchX = secondaryTouchX;
        }
        return false;
    }

    var touchEndEvent = function (e) {
        if (Math.abs(offsetTouchY) < swipeVerticalDistanceThreshold && Math.abs(offsetTouchX) > swipeHorizontalDistanceThreshold
            && (new Date()).getTime() - startedTouch < swipeDurationThreshold) {
            onNextImage(offsetTouchX > 0);
        }

        lastTouchY = 0;
        lastTouchX = 0;
        isTouchMoving = false;
        startedTouch = 0;
    };

    var mouseUpEvent = function () {
        imgRef.css("cursor", "move");
        jq(document).unbind("mouseup.ImageViewer");
        jq(document).unbind("mousemove.ImageViewer");

        imgRef[0].ondragstart = null;
        document.ondragstart = null;
        document.body.onselectstart = null;
    };

    var mouseWheelEvent = function (event, delta) {
        if (jq(".blockUI:visible").length) return true;

        if (scalingInProcess) {
            return false;
        }

        mouseOffset = { x: event.pageX - imgRef.offset().left, y: event.pageY - imgRef.offset().top };

        zoomImage(delta > 0, event.target.id == "imageViewerContainer");
        return false;
    };

    var mouseDoubleClickEvent = function (event) {
        if (scalingInProcess) {
            return;
        }

        mouseOffset = { x: event.pageX - imgRef.offset().left, y: event.pageY - imgRef.offset().top };

        nScale = oScale;
        zoomPercBox.show();
        setNewDimensions({ ls: nScale + "%", toPtr: true });
    };

    var calculateDimensions = function () {
        windowDimensions = { w: jq(window).width(), h: jq(window).height() };

        var centerAreaOx = windowDimensions.w / 2 + jq(window).scrollLeft();
        var centerAreaOy = windowDimensions.h / 2 + jq(window).scrollTop();

        centerArea = { w: centerAreaOx, h: centerAreaOy };

        imageAreaDimensions = { w: windowDimensions.w, h: windowDimensions.h - headerHeight - controlsHeight };

        zoomPercBox.css({
            "left": centerArea.w,
            "top": centerArea.h
        });
    };

    var rotateImage = function (toLeft) {
        if (scalingInProcess) {
            return;
        }

        rotateAngel += 90 * (toLeft ? -1 : 1);

        if (!jq.browser.msie) {
            var rotateCssAttr = "rotate({0}deg)".format(rotateAngel);

            imgRef.css({
                "-moz-transform": rotateCssAttr,
                "-o-transform": rotateCssAttr,
                "-webkit-transform": rotateCssAttr,
                "transform": rotateCssAttr
            });
        } else {
            var rad = (rotateAngel * Math.PI) / 180.0;
            var filter = "progid:DXImageTransform.Microsoft.Matrix(sizingMethod=\"auto expand\", M11 = " + Math.cos(rad) + ", M12 = " + (-Math.sin(rad)) + ", M21 = " + Math.sin(rad) + ", M22 = " + Math.cos(rad) + ")";

            var imgOffset = imgRef.offset();

            imgRef.css(
                {
                    "-ms-filter": filter,
                    "filter": filter
                });

            var rotateOffset = { left: -Math.round((imgRef.width() - ndimensions.w) / 2), top: -Math.round((imgRef.height() - ndimensions.h) / 2) };

            imgRef.css(
                {
                    "left": imgOffset.left + rotateOffset.left,
                    "top": imgOffset.top + rotateOffset.top
                });

            ndimensions.w = imgRef.width();
            ndimensions.h = imgRef.height();
        }
    };

    var closeImageViewer = function () {
        resetWorkspace();
    };

    var zoomImage = function (directionIn, toPtr) {
        if (scalingInProcess) {
            return;
        }

        if ((!directionIn && nScale <= scaleStep) || (directionIn && nScale >= 1000)) {
            zoomPercBox.show();
            setTimeout(function () { zoomPercBox.hide(); }, 500);
            return;
        }

        if (nScale < 100) {
            var step = scaleStep * (directionIn ? 1 : -1);
        } else {
            step = scaleStep * Math.floor(nScale / 50) * (directionIn ? 1 : -1);
        }

        nScale += step;

        if (nScale > 1000) {
            nScale = 1000;
        }
        if (nScale < scaleStep) {
            nScale = scaleStep;
        }

        zoomPercBox.show();
        setNewDimensions({ ls: nScale + "%", toPtr: toPtr });
    };

    var zoomImageFull = function () {
        if (scalingInProcess || nScale == 100) {
            return;
        }

        nScale = 100;

        zoomPercBox.show();
        setNewDimensions({ ls: nScale + "%", toCenter: true });
    };

    return {
        init: init,
        getPreviewHash: getPreviewHash
    };
})();