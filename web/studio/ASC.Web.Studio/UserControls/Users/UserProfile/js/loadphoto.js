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


window.ASC.Controls.LoadPhotoImage = function () {

    var userId;
    var defaultImgUrl;
    var mainImgUrl;
    var mainThumbnailSettings;

    var jcropElement = null;
    var jcropMinSize = [0, 0];
    var jcropMaxSize = [0, 0];
    var jcropAspectRatio = 1;
    
    var dialog;
    var emptyBlock;
    var previewBlock;
    var emptySelection;
    var emptyBlockUploader;
    var previewBlockUploader;
    var mainImg;
    var previewImg;

    var saveDefault = false;

    var tmpImgUrl = null;
    var tmpThumbnailSettings = null;

    var destroyJcropElement = function() {
        mainImg[0].onload = null;
        mainImg.attr({ alt: "", style: "", src: "" });

        if (jcropElement)
            jcropElement.destroy();
    };

    var showDialog = function () {
        destroyJcropElement();
        tmpImgUrl = null;
        tmpThumbnailSettings = mainThumbnailSettings;
        StudioBlockUIManager.blockUI(dialog, 450);

        if (mainImgUrl) {
            changeView(true);
            updateMainImg(mainImgUrl, false);
        } else {
            changeView(false);
        }
    };

    var closeDialog = function () {
        destroyJcropElement();
        PopupKeyUpActionProvider.CloseDialog();
    };

    var disableDialog = function(disable) {
        if (disable) {
            dialog.find(".save-btn").addClass("disable");
            var parent = emptySelection.parent();
            var top = parent.outerHeight() / 2 - emptySelection.outerHeight() / 2;
            var left = parent.outerWidth() / 2 - emptySelection.outerWidth() / 2;
            emptySelection.removeClass("display-none").css("top", top).css("left", left);
        } else {
            dialog.find(".save-btn").removeClass("disable");
            emptySelection.addClass("display-none");
        }
    };

    var changeView = function(preview) {
        if (preview) {
            emptyBlock.addClass("display-none");
            previewBlock.removeClass("display-none");
            saveDefault = false;
        } else {
            emptyBlock.removeClass("display-none");
            previewBlock.addClass("display-none");
            saveDefault = true;
        }

        disableDialog(false);
    };

    var deletePhoto =  function() {
        changeView(false);
    };

    var savePhoto = function() {
        if (jq(this).hasClass("disable")) return;

        if (!jcropElement) {
            closeDialog();
            return;
        }

        var container = jq("#userProfilePhoto");
        var img = container.find("img");
        var position = jcropElement.tellScaled();

        if (!position.w || !position.h) {
            toastr.warning(ASC.Resources.Master.Resource.EmptySelectedArea);
            return;
        }

        var ratio = mainImg[0].naturalWidth / mainImg.width();

        tmpThumbnailSettings = {
            point: { x: parseInt(position.x * ratio), y: parseInt(position.y * ratio) },
            size: { width: parseInt(position.w * ratio), height: parseInt(position.h * ratio) }
        };

        if (!userId) {
            if (saveDefault) {
                container.removeClass("preview");
                img.attr("src", defaultImgUrl).removeAttr("style");
                mainImgUrl = null;
            } else {
                container.addClass("preview");
                changePreviewImage(img, position);
                mainImgUrl = tmpImgUrl || mainImgUrl;
            }

            mainThumbnailSettings = tmpThumbnailSettings;
            closeDialog();
            return;
        }

        var options = {
            before: onBefore,
            error: onError,
            success: onSuccess,
            after: onAfter
        };

        if (saveDefault) {
            Teamlab.removeUserPhoto({}, userId, options);
        } else {
            Teamlab.createUserPhotoThumbnails({}, userId, {
                tmpFile: tmpImgUrl,
                x: tmpThumbnailSettings.point.x,
                y: tmpThumbnailSettings.point.y,
                width: tmpThumbnailSettings.size.width,
                height: tmpThumbnailSettings.size.height
            }, options);
        }
        
        function onError (params, error) {
            console.log(error);
            var err = error[0];
            if (err != null) {
                toastr.error(err);
            }
        }
        
        function onSuccess(params, data) {
            img.attr("src", updateUri(data.max));
            mainImgUrl = saveDefault ? null : updateUri(data.original);
            mainThumbnailSettings = tmpThumbnailSettings;
            toastr.success(ASC.Resources.Master.Resource.ChangesApplied);
            closeDialog();
        }
        
        function onBefore() {
            LoadingBanner.showLoaderBtn(dialog);
        }
        
        function onAfter() {
            LoadingBanner.hideLoaderBtn(dialog);
        }
    };

    var initUploader = function () {
        var action = "ajaxupload.ashx?type=ASC.Web.People.UserPhotoUploader,ASC.Web.People" + (userId ? "&userId=" + userId : "");

        emptyBlockUploader = new AjaxUpload(emptyBlock.find(".upload-btn"), {
            action: action,
            autoSubmit: true,
            onChange: onChange,
            onComplete: onComplete,
            parentDialog: dialog,
            isInPopup: true
        });
        
        previewBlockUploader = new AjaxUpload(previewBlock.find(".upload-btn"), {
            action: action,
            autoSubmit: true,
            onChange: onChange,
            onComplete: onComplete,
            parentDialog: dialog,
            isInPopup: true
        });
        
        function onChange (file, extension) {
            return true; //TODO: check size
        }

        function onComplete(file, response) {
            var result = JSON.parse(response);
            if (result.Success) {
                tmpImgUrl = result.Data;
                destroyJcropElement();
                changeView(true);
                updateMainImg(tmpImgUrl, true);
            } else {
                console.log(result.Message);
                toastr.error(result.Message);
            }
        }
    };
    
    var showPreview = function (coords) {
        if (!previewImg || !previewImg.length)
            return;

        if (!coords || !coords.w || !coords.h) {
            disableDialog(true);
            return;
        }

        previewImg.each(function () {
            changePreviewImage(jq(this), coords);
        });
    };

    var changePreviewImage = function(img, coords) {
        img.attr("src", mainImg.attr("src"));

        var rx = img.parent().width() / coords.w;
        var ry = img.parent().height() / coords.h;

        if (rx == Infinity || ry == Infinity)
            return;

        img.css({
            width: Math.round(rx * mainImg.width()) + 'px',
            height: Math.round(ry * mainImg.height()) + 'px',
            marginLeft: '-' + Math.round(rx * coords.x) + 'px',
            marginTop: '-' + Math.round(ry * coords.y) + 'px'
        });
    };

    var updateMainImg = function (src, resize) {

        mainImg[0].onload = function() {
            var ratio = this.naturalWidth / mainImg.width();

            if (resize) {
                var max = parseInt(Math.max(this.naturalWidth, this.naturalHeight));
                var min = parseInt(Math.min(this.naturalWidth, this.naturalHeight));
                var pos = (max - min) / 2;

                tmpThumbnailSettings = {
                    point: (this.naturalWidth >= this.naturalHeight) ? { x: pos, y: 0 } : { x: 0, y: pos },
                    size: { width: min, height: min }
                };
            }
            
            mainImg.Jcrop({
                onChange: showPreview,
                onSelect: showPreview,
                setSelect: [
                    parseInt(tmpThumbnailSettings.point.x / ratio),
                    parseInt(tmpThumbnailSettings.point.y / ratio),
                    parseInt((tmpThumbnailSettings.point.x + tmpThumbnailSettings.size.width) / ratio),
                    parseInt((tmpThumbnailSettings.point.y + tmpThumbnailSettings.size.height) / ratio)
                ],
                minSize: jcropMinSize,
                maxSize: jcropMaxSize,
                aspectRatio: jcropAspectRatio
            }, function () {
                jcropElement = this;

                //HACK to display circle
                var tracker = dialog.find(".jcrop-holder .jcrop-tracker:first");
                tracker.append(jq("<div class='jcrop-circle'></div>"));
            });
        };
        
        mainImg.attr({ alt: "", style: "", src: updateUri(src) });
    };

    var updateUri = function (uri) {
        return uri + (uri.indexOf("?") == -1 ? "?" : "&") + "_=" + new Date().getTime();
    };

    var init = function(user, min, settings, main, def) {
        userId = user;
        defaultImgUrl = def;
        mainImgUrl = main;
        mainThumbnailSettings = settings;

        jcropMinSize = min;

        dialog = jq("#userPhotoDialog");
        emptyBlock = dialog.find(".empty-block");
        previewBlock = dialog.find(".preview-block");
        emptySelection = dialog.find(".empty-selection");
        mainImg = dialog.find(".preview-block img");
        previewImg = null;

        initUploader();

        dialog.find(".cancel-btn").on("click", closeDialog);

        dialog.find(".delete-btn").on("click", deletePhoto);
        
        dialog.find(".save-btn").on("click", savePhoto);
    };

    var save = function (user, callback) {
        if (!mainImgUrl) {
            callback();
        } else {
            Teamlab.createUserPhotoThumbnails({}, user,
                {
                    tmpFile: mainImgUrl,
                    x: mainThumbnailSettings.point.x,
                    y: mainThumbnailSettings.point.y,
                    width: mainThumbnailSettings.size.width,
                    height: mainThumbnailSettings.size.height
                },
                {
                    after: callback
                });
        }
    };

    return {
        init: init,
        showDialog: showDialog,
        closeDialog: closeDialog,
        save: save
    };
}();