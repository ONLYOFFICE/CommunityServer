/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

window.ASC.Files.ImageViewer = (function () {
    var isInit = false;
    var scalingInProcess = false;
    var action;
    var imageCollection;
    var currentFolderId;

    var odimensions = {};
    var ndimensions = {};
    var imgRef;
    var oScale = 0;
    var nScale = 0;
    var scaleStep = 15;
    var rotateAngel = 0;
    var mouseOffset;
    var windowDimensions = {};
    var imageAreaDimensions = {};
    var centerArea = {};

    var init = function (fileId) {
        ASC.Files.Actions.hideAllActionPanels();
        if (scalingInProcess) {
            return;
        }

        ASC.Files.Mouse.mouseBtn = false;
        if (isInit === false) {
            isInit = true;

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetSiblingsImage, onGetImageViewerData);
            imgRef = jq("#imageViewerContainer");
            imgRef.bind("dblclick", mouseDoubleClickEvent);
            imgRef.bind("mousedown", mouseDownEvent);
        }

        prepareWorkspace();
        positionParts();

        action = "open";

        var filterSettings = ASC.Files.Filter.getFilterSettings();

        ASC.Files.ServiceManager.getSiblingsImage(ASC.Files.ServiceManager.events.GetSiblingsImage,
            {
                fileId: fileId,
                filter: filterSettings.filter,
                subjectId: filterSettings.subject,
                search: filterSettings.text
            },
            { orderBy: filterSettings.sorter });
    };

    var isView = function () {
        return jq("#imageViewerClose").is(":visible");
    };

    var getPreviewHash = function (fileId) {
        return "preview/" + fileId;
    };

    var nextImageIndex = function () {
        var nextIndex = imageCollection.selectedIndex + 1;

        if (nextIndex > imageCollection.length - 1) {
            nextIndex = 0;
        }
        return nextIndex;
    };

    var prevImageIndex = function () {
        var prevIndex = imageCollection.selectedIndex - 1;

        if (prevIndex < 0) {
            prevIndex = imageCollection.length - 1;
        }
        return prevIndex;
    };

    var prepareWorkspace = function () {
        LoadingBanner.hideLoading();
        LoadingBanner.displayLoading();
        ASC.Files.Mouse.finishMoveTo();
        jq("body").addClass("scroll-fix");

        if (jq.browser.msie && jq.browser.version <= 7) {
            jq("html").addClass("scroll-fix");
            jq("<style type=\"text/css\"> .studioContentClassOverride{ position: static !important; } </style>").appendTo("head");
            jq("#studioPageContent").addClass("studioContentClassOverride");
        }

        jq(window).bind("resize.ImageViewer", positionParts);
        jq(document).bind("mousewheel.ImageViewer", mouseWheelEvent);
        jq(document).bind("keydown.ImageViewer", keyDownEvent);

        jq.blockUI({
            message: jq("#fileViewerDialog"),
            css: {
                position: "inherit"
            },
            overlayCSS: {
                backgroundColor: "Black",
                cursor: "default",
                opacity: "0.5"
            },
            onOverlayClick: ASC.Files.ImageViewer.closeImageViewer
        });
        PopupKeyUpActionProvider.EnableEsc = false;

        jq("#imageViewerToolbox, #imageViewerClose").show();

        jq(".blockUI.blockOverlay").css({ cursor: "default" });
        jq(".blockUI.blockMsg").css({ border: "none" });
    };

    var resetWorkspace = function () {
        if (!ASC.Files.ImageViewer.isView()) {
            //if not init - do nothing
            return;
        }

        jq("#viewerOtherActions").hide();
        jq("body").removeClass("scroll-fix");

        if (jq.browser.msie && jq.browser.version <= 7) {
            jq("html").removeClass("scroll-fix");
            jq("#studioPageContent").removeClass("studioContentClassOverride");
        }

        LoadingBanner.hideLoading();
        PopupKeyUpActionProvider.CloseDialog();
        scalingInProcess = false;

        jq(window).unbind("resize.ImageViewer");
        jq(document).unbind("mousewheel.ImageViewer");
        jq(document).unbind("keydown.ImageViewer");
        jq(document).unbind("mousemove.ImageViewer");
        jq(document).unbind("mouseup.ImageViewer");

        ASC.Files.Mouse.mouseBtn = false;
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

        if (typeof callbackHandle == "undefined" || callbackHandle == null) {
            callbackHandle = function () {
                scalingInProcess = false;
                jq("#imageViewerInfo").hide();
                oScale = nScale;
            };
        }

        if (typeof setting.speed == "undefined" || setting.speed == null) {
            setting.speed = 250;
        }

        if (setting.speed != 0) {
            var scaleSign = 1;

            var getDimensionOffset = function (side) {
                var diff = (ndimensions[side] - oldDimensions[side]) / 2;

                if (diff < 0) {
                    scaleSign = -1;
                }

                if (diff > 0) {
                    return "-=" + diff + "px";
                } else {
                    return "+=" + (-diff) + "px";
                }
            };

            imgRef.animate({ width: ndimensions.w + "px", height: ndimensions.h + "px", left: getDimensionOffset("w"), top: getDimensionOffset("h") },
                {
                    duration: setting.speed,
                    easing: "linear",
                    complete: callbackHandle,
                    step: function (now, obj) {
                        jq("#imageViewerInfo span").text(Math.round(oScale + scaleSign * scaleStep * obj.pos) + "%");
                    }
                });
        } else {
            imgRef.css({ width: ndimensions.w + "px", height: ndimensions.h + "px" });
            callbackHandle();
        }
    };

    var imageOnLoad = function () {
        if (!ASC.Files.ImageViewer.isView()) {
            return;
        }

        LoadingBanner.hideLoading();

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

        if (action == "open") {
            ndimensions = { w: 10, h: 10 };

            imgRef.css({
                "left": centerArea.w - ndimensions.w / 2,
                "top": centerArea.h - ndimensions.h / 2
            }).show();

            setNewDimensions({ ls: nScale + "%" });
        } else {
            setNewDimensions({ ls: nScale + "%", speed: 0 }, function () {
                scalingInProcess = false;

                imgRef.css({
                    "left": centerArea.w - ndimensions.w / 2,
                    "top": centerArea.h - ndimensions.h / 2
                }).show();
                oScale = nScale;
            });
        }

        tempimg.remove();

        resetImageInfo();

        ASC.Files.Marker.removeNewIcon("file", imageCollection[imageCollection.selectedIndex].fileId);

        imageCollection[imageCollection.selectedIndex].load = true;

        preshowImage();
    };

    var imageOnError = function () {
        if (!imgRef.attr("src").length) {
            return;
        }
        ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.PreviewError.format(imageCollection[imageCollection.selectedIndex].title), true);
        imageCollection.splice(imageCollection.selectedIndex, 1);
        imageCollection.selectedIndex = prevImageIndex();
        ASC.Files.ImageViewer.nextImage(true);
    };

    var resetImageInfo = function () {
        if (isImageLoadCompleted()) {
            var imageName = imageCollection[imageCollection.selectedIndex].title;
            var text = "{0} ({1}x{2})".format(imageName, odimensions.w, odimensions.h);
            jq("#imageViewerToolbox div.image-info").text(text).attr("title", text);
            ASC.Files.UI.setDocumentTitle(imageName);
        } else {
            jq("#imageViewerToolbox div.image-info").html("&nbsp").removeAttr("title");
        }
    };

    var isImageLoadCompleted = function () {
        return !jq.isEmptyObject(odimensions);
    };

    var showImage = function () {
        odimensions = {};
        ndimensions = {};
        rotateAngel = 0;

        jq("#imageViewerInfo, #imageViewerContainer").hide();

        LoadingBanner.displayLoading();

        imgRef[0].onload = imageOnLoad;
        imgRef[0].onerror = imageOnError;

        imgRef.attr("src", ASC.Files.Utility.GetFileViewUrl(imageCollection[imageCollection.selectedIndex].fileId, imageCollection[imageCollection.selectedIndex].version));

        jq("#imagePrev").attr("href", "#" + ASC.Files.ImageViewer.getPreviewHash(imageCollection[prevImageIndex()].fileId));
        jq("#imageNext").attr("href", "#" + ASC.Files.ImageViewer.getPreviewHash(imageCollection[nextImageIndex()].fileId));

        var fileObj = ASC.Files.UI.getEntryObject("file", imageCollection[imageCollection.selectedIndex].fileId);

        if (ASC.Files.UI.accessDelete(fileObj) && !ASC.Files.UI.lockedForMe(fileObj)) {
            jq("#imageDelete").show();
        } else {
            jq("#imageDelete").hide();
        }
    };

    var preshowImage = function () {
        if (imageCollection.length == 0 || jq.browser.mobile) {
            return;
        }

        var nextIndex = nextImageIndex();
        if (imageCollection[nextIndex].load) {
            return;
        }

        jq("#imageViewerPreload").onload = function () {
            var loadIndex = nextIndex;
            imageCollection[loadIndex].load = true;
        };

        jq("#imageViewerPreload").removeAttr("src");
        jq("#imageViewerPreload").attr("src", ASC.Files.Utility.GetFileViewUrl(imageCollection[nextIndex].fileId, imageCollection[nextIndex].version));
    };

    var mouseDownEvent = function (event) {
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

        jq("#imageViewerToolbox, #viewerOtherActions").hide();
    };

    var mouseMoveEvent = function (event) {
        var x = event.pageX,
            y = event.pageY;
        if (x < 0 || y < 0 || x > jq(window).width() || y + 100 > jq(window).height()) {
            return;
        }
        imgRef.css({
            "cursor": "pointer",
            "left": x - mouseOffset.x,
            "top": y - mouseOffset.y
        });
    };

    var mouseUpEvent = function () {
        imgRef.css("cursor", "move");
        jq(document).unbind("mouseup.ImageViewer");
        jq(document).unbind("mousemove.ImageViewer");

        imgRef[0].ondragstart = null;
        document.ondragstart = null;
        document.body.onselectstart = null;

        jq("#imageViewerToolbox").show();
    };

    var mouseWheelEvent = function (event, delta) {
        if (scalingInProcess) {
            return false;
        }

        if (delta > 0) {
            ASC.Files.ImageViewer.zoomImage(true);
        } else if (delta < 0) {
            ASC.Files.ImageViewer.zoomImage(false);
        }

        return false;
    };

    var mouseDoubleClickEvent = function () {
        if (scalingInProcess) {
            return;
        }

        var ls = windowDimensions.w > windowDimensions.h ? windowDimensions.w : windowDimensions.h;

        setNewDimensions({ ls: ls + "px" });
    };

    var keyDownEvent = function (e) {
        var key = e.keyCode || e.which;
        var keyCode = ASC.Files.Common.keyCode;

        switch (key) {
            case keyCode.left:
                ASC.Files.ImageViewer.nextImage(false);
                return false;
            case keyCode.spaceBar:
            case keyCode.right:
                ASC.Files.ImageViewer.nextImage(true);
                return false;
            case keyCode.up:
                ASC.Files.ImageViewer.zoomImage(true);
                return false;
            case keyCode.down:
                ASC.Files.ImageViewer.zoomImage(false);
                return false;
            case keyCode.esc:
                ASC.Files.ImageViewer.closeImageViewer();
                return false;
            case keyCode.deleteKey:
                ASC.Files.ImageViewer.deleteImage();
                return false;
            case keyCode.home:
            case keyCode.end:
            case keyCode.pageDown:
            case keyCode.pageUP:
                return false;
        }
        return true;
    };

    var calculateDimensions = function () {
        windowDimensions = { w: jq(window).width(), h: jq(window).height() };

        var centerAreaOx = windowDimensions.w / 2 + jq(window).scrollLeft();
        var centerAreaOy = windowDimensions.h / 2 + jq(window).scrollTop() - jq("#imageViewerToolbox").height() / 2;

        centerArea = { w: centerAreaOx, h: centerAreaOy };

        imageAreaDimensions = { w: windowDimensions.w, h: windowDimensions.h - jq("#imageViewerToolbox").height() };
    };

    var positionParts = function () {
        calculateDimensions();

        jq("#imageViewerInfo").css({
            "left": centerArea.w,
            "top": centerArea.h
        });

        jq("#imageViewerClose").css({
            "right": 10,
            "top": jq(window).scrollTop() + 15
        });

        jq("#imageViewerToolbox").css({
            "top": windowDimensions.h + jq(window).scrollTop() - jq("#imageViewerToolbox").height()
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

    var nextImage = function (next) {
        if (scalingInProcess) {
            return;
        }

        if (imageCollection.length == 0 || imageCollection.selectedIndex < 0) {
            ASC.Files.ImageViewer.closeImageViewer();
            return;
        }

        if (next) {
            action = "nextImage";

            var goToIndex = nextImageIndex();
        } else {
            action = "prevImage";

            goToIndex = prevImageIndex();
        }
        if (imageCollection.selectedIndex == goToIndex) {
            return;
        }
        imageCollection.selectedIndex = goToIndex;

        var fileId = imageCollection[imageCollection.selectedIndex].fileId;
        var hash = ASC.Files.ImageViewer.getPreviewHash(fileId);
        ASC.Files.Anchor.move(hash, true);

        showImage();
    };

    var closeImageViewer = function () {
        if (isImageLoadCompleted()) {
            setNewDimensions({ ls: "10px", speed: 500 }, ASC.Files.ImageViewer.resetWorkspace);
        } else {
            imgRef[0].onload = null;
            imgRef[0].oneror = null;
            imgRef.removeAttr("src");
            ASC.Files.ImageViewer.resetWorkspace();
        }
        var safeMode = ASC.Files.Folders.currentFolder.id == currentFolderId;
        ASC.Files.Anchor.navigationSet(currentFolderId, safeMode);

        if (safeMode) {
            ASC.Files.UI.setDocumentTitle();
        }
    };

    var zoomImage = function (directionIn, fullScale) {
        if (scalingInProcess || fullScale && nScale == 100) {
            return;
        }

        nScale += scaleStep * (directionIn ? 1 : -1);
        nScale = fullScale ? 100 : nScale;

        nScale = Math.min(nScale, 1000);
        nScale = Math.max(nScale, scaleStep);

        jq("#imageViewerInfo span").text(nScale + "%");
        jq("#imageViewerInfo").show();
        setNewDimensions({ ls: nScale + "%" });
    };

    var deleteImage = function () {
        if (scalingInProcess) {
            return;
        }

        var fileId = imageCollection[imageCollection.selectedIndex].fileId;

        var fileObj = ASC.Files.UI.getEntryObject("file", fileId);

        if (!ASC.Files.UI.accessDelete(fileObj) || ASC.Files.UI.lockedForMe(fileObj)) {
            return;
        }

        var data = {};
        data.entry = new Array();
        ASC.Files.UI.blockObjectById("file", fileId, true, ASC.Files.FilesJSResources.DescriptRemove);
        Encoder.EncodeType = "!entity";
        data.entry.push(Encoder.htmlEncode("file_" + fileId));
        Encoder.EncodeType = "entity";

        ASC.Files.ServiceManager.deleteItem(ASC.Files.ServiceManager.events.DeleteItem, { list: [fileId], doNow: true }, { stringList: data });

        imageCollection.splice(imageCollection.selectedIndex, 1);
        imageCollection.selectedIndex = prevImageIndex();

        ASC.Files.ImageViewer.nextImage(true);
    };

    var downloadImage = function () {
        if (scalingInProcess) {
            return;
        }

        window.open(ASC.Files.Utility.GetFileViewUrl(imageCollection[imageCollection.selectedIndex].fileId, imageCollection[imageCollection.selectedIndex].version),
            "new", "fullscreen = 1, resizable = 1, location=1, toolbar=1");
    };

    //event handler

    var onGetImageViewerData = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined" || typeof jsonData == "undefined" || jsonData.length == 0) {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            ASC.Files.ImageViewer.closeImageViewer();
            return;
        }
        currentFolderId = jsonData.key;

        var data = jsonData.value;
        var selectedIndex = data.length;
        var imageCollectionTmp = new Array();

        for (var i = 0; i < data.length; i++) {
            var title = data[i].Value.split("&")[1];
            var version = data[i].Value.split("&")[0];
            if (ASC.Files.Utility.CanImageView(title)) {
                imageCollectionTmp.push(
                    {
                        fileId: data[i].Key,
                        version: version,
                        title: title
                    });
                if (data[i].Key == params.fileId) {
                    selectedIndex = imageCollectionTmp.length - 1;
                }
            }
        }

        imageCollection = imageCollectionTmp;
        imageCollection.selectedIndex = selectedIndex;

        if (imageCollection.selectedIndex >= imageCollection.length) {
            ASC.Files.ImageViewer.closeImageViewer();
            return;
        }

        showImage();
    };

    return {
        init: init,
        isView: isView,
        getPreviewHash: getPreviewHash,

        nextImage: nextImage,
        closeImageViewer: closeImageViewer,
        resetWorkspace: resetWorkspace,

        rotateImage: rotateImage,
        zoomImage: zoomImage,

        deleteImage: deleteImage,
        downloadImage: downloadImage
    };
})();

(function ($) {
    jq.dropdownToggle({
        switcherSelector: "#viewerOtherActionsSwitch",
        dropdownID: "viewerOtherActions"
    });

    $(function () {
        jq("#imageViewerClose").click(function () {
            ASC.Files.ImageViewer.closeImageViewer();
            return false;
        });

        jq("#imageZoomIn").click(function () {
            ASC.Files.ImageViewer.zoomImage(true);
            return false;
        });

        jq("#imageZoomOut").click(function () {
            ASC.Files.ImageViewer.zoomImage(false);
            return false;
        });

        jq("#imageFullScale").click(function () {
            ASC.Files.ImageViewer.zoomImage(true, true);
            return false;
        });

        jq("#imagePrev").click(function () {
            ASC.Files.ImageViewer.nextImage(false);
            return false;
        });

        jq("#imageNext").click(function () {
            ASC.Files.ImageViewer.nextImage(true);
            return false;
        });

        jq("#imageRotateLeft").click(function () {
            ASC.Files.ImageViewer.rotateImage(true);
            return false;
        });

        jq("#imageRotateRight").click(function () {
            ASC.Files.ImageViewer.rotateImage(false);
            return false;
        });

        jq("#imageDelete").click(function () {
            ASC.Files.ImageViewer.deleteImage();
            return false;
        });

        jq("#imageDownload").click(function () {
            ASC.Files.ImageViewer.downloadImage();
            return false;
        });
    });
})(jQuery);