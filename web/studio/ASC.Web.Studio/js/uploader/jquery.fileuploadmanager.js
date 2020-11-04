
var FileUploadManager = new function() {

    var config = null;
    var autoUpload = false;
    var dragDropEnabled = ("draggable" in document.createElement("span"));

    var uploadQueue = new Array();

    var uploadStatus = {
        QUEUED: 0,
        STARTED: 1,
        STOPED: 2,
        DONE: 3,
        FAILED: 4
    };


    this.renderedItemTemplate = "asc_html_itemTemplate_uploader";
    this.renderedProgressTemplate = "asc_html_progressTemplate_uploader";
    
    this.uploadedFiles = new Array();
    

    this.Init = function (configObj) {

        configObj.Events = configObj.Events || {};

        config = configObj;

        autoUpload = !config.UploadButton;

        renderUploader();

        createFileuploadInput(jq("#" + config.BrowseButton));

        var uploader = jq("#fileupload").fileupload({
            url: getSubmitUrl(),
            autoUpload: false,
            singleFileUploads: true,
            sequentialUploads: true,
            progressInterval: 1000,
            dropZone: dragDropEnabled ? jq("#" + config.DropZone) : null
        });

        bindEvents(uploader);
    };

    this.StartUpload = function () {
        for (var i = 0; i < uploadQueue.length; i++) {
            if (uploadQueue[i].files[0].status == uploadStatus.QUEUED) {
                uploadQueue[i].submit();
            }
        }
    };

    this.RemoveFile = function (fileId) {
        for (var i = 0; i < uploadQueue.length; i++) {
            if (uploadQueue[i].files[0].id == fileId) {
                uploadQueue[i].abort();
                break;
            }
        }

        if (i != uploadQueue.length)
            uploadQueue.splice(i, 1);
    };

    this.DisableBrowseBtn = function (disable) {
        jq("#fileupload").prop("disabled", disable);
    };

    this.GetFiles = function () {
        return jq.map(uploadQueue, function (uploadData) {
            return uploadData.files[0];
        });
    };

    this.ClearFiles = function () {
        uploadQueue.clear();
        FileUploadManager.uploadedFiles.clear();
    };



    this._uploadStart = function () {
        jq(config.OverAllProcessHolder).show();
    };

    this._uploadAdd = function (e, data) {
        data.id = createNewGuid();

        var file = data.files[0];
        file.id = createNewGuid();
        file.percent = 0;
        file.loaded = 0;

        if (correctFile(file)) {
            file.status = uploadStatus.QUEUED;
            uploadQueue.push(data);
            renderFileRow(file, null);

            if (autoUpload) {
                jq("#fu_item_loading_{0}".format(file.id)).show();
                jq("#fu_item_delete_{0}".format(file.id)).hide();
                data.submit();
            }
        }
    };

    this._uploadSubmit = function (e, data) {
        var file = data.files[0];
        
        if (file.status == uploadStatus.QUEUED) {
            file.status = uploadStatus.STARTED;
        }
    };

    this._uploadSend = function (e, data) {
        var file = data.files[0];

        return file.status == uploadStatus.STARTED;
    };

    this._uploadDone = function (e, data) {
        var file = data.files[0];
        file.percent = 100;
        file.loaded = file.size;
        file.status = uploadStatus.DONE;

        var serverData = JSON.parse(data.result);

        if (serverData.Data) {
            FileUploadManager.uploadedFiles.push({
                fileName: serverData.Data.FileName,
                size: serverData.Data.Size,
                offsetPhysicalPath: serverData.Data.OffsetPhysicalPath,
                contentType: serverData.Data.ContentType
            });
        }

        renderFileRow(file, serverData);
    };

    this._uploadProgress = function (e, data) {
        var file = data.files[0];
        file.percent = parseInt(data.loaded / data.total * 100, 10);
        file.loaded = data.loaded;

        renderFileRow(file, null);

        var size = 0;
        var loaded = 0;

        jq.each(uploadQueue, function (index, uploadData) {
            var fileItem = uploadData.files[0];
            if (fileItem.status != uploadStatus.STOPED && fileItem.status != uploadStatus.FAILED) {
                size += fileItem.size;
                loaded += fileItem.loaded;
            }
        });

        var percent = parseInt(loaded / size * 100, 10);

        if (config.OverAllProcessHolder != null) {
            jq("div." + config.OverAllProcessBarCssClass + " div").css("width", percent + "%");
            jq("td.fu_percent").text(percent + "%");
            jq(config.OverAllProcessHolder).show();
        }
    };

    this._uploadStop = function () {
        jq("div." + config.OverAllProcessBarCssClass + " div").css("width", "100%");
        jq("td.fu_percent").text("100%");
    };

    this._uploadFail = function (e, data) {
        var file = data.files[0];
        file.percent = 0;
        file.loaded = 0;
        file.status = uploadStatus.FAILED;

        if (data.errorThrown != "abort") {
            renderFileRow(file, null);
        }
    };



    this._dragEnter = function () {
         return false;
    };

    this._dragLeave = function () {
        return hideDragHighlight();
    };

    this._dragOver = function () {
        if (dragDropEnabled) {
            showDragHighlight();
        }

        if (jQuery.browser.safari) {
            return true;
        }
        return false;
    };

    this._drop = function () {
        hideDragHighlight();

        return false;
    };

    

    var renderUploader = function () {
        if (autoUpload) {
            jq("#" + config.UploadButton).hide();
        }

        var itemTemplate = config.ItemTemplate ||
            "<div id=\"fu_item_${id}\" class=\"fu_item\"><table cellspacing=\"0\" cellpadding=\"0\" style=\"width:100%;border-bottom:1px solid #D1D1D1;\"><tr style=\"height:40px !important;\" valign=\"middle\"><td style=\"width:70%; padding-left:10px;\"><div class=\"name ${fileNameCss}\" style=\"padding-right:5px;\" title=\"${fileName}\" >${fileName}<input type=\"hidden\" id=\"fu_itemName_hidden_{id}\" value=\"{id}\"/><div class=\"${descriptionCss}\">${descriptionText}</div></div></td><td class=\"size\" style=\"padding-left: 10px; width:20%; {{if exceedLimit}}color: red;{{else}}color: #83888D;{{/if}}\"><div>${fileSize}&nbsp;</div></td><td style=\"width:10%;\">&nbsp;<a id=\"fu_item_loading_${id}\" class=\"${loadingCss}\" style=\"display:none;\"></a><a id=\"fu_item_complete_${id}\" class=\"${completeCss}\" style=\"display:none;\"></a><a id=\"fu_item_delete_${id}\" class=\"fu_item_delete ${deleteCss}\" href=\"#\"></a></td></tr></table><div class=\"studioFileUploaderProgressBar\" style=\"margin-top:-41px; float:left; width:${progressBarWidth};height:40px;\" ></div></div>";

        jQuery.template(FileUploadManager.renderedItemTemplate, itemTemplate);

        var progressTemplate = config.ProgressTemplate ||
            "<table cellpadding='5' cellspacing='0' width='100%' style='padding:10px 0;' class='describe-text'><tr><td width='100'>${overallProgressText}</td><td><div style=\"margin-top: 1px;\" class=\"${overAllProcessBarCssClass}\"><div></div></div></td><td class='fu_percent' width='20'></td></tr></table>";

        jQuery.template(FileUploadManager.renderedProgressTemplate, progressTemplate);

        if (config.OverAllProcessHolder) {
            jq(config.OverAllProcessHolder).html("");
            jq(config.OverAllProcessHolder).append(
                jq.tmpl(FileUploadManager.renderedProgressTemplate, {
                    overallProgressText: config.OverallProgressText,
                    overAllProcessBarCssClass: config.OverAllProcessBarCssClass
                }));

            jq("div." + config.OverAllProcessBarCssClass + " div").css("width", "0%");
            jq("td.fu_percent").text("0%");
        }
    };

    var createFileuploadInput = function (buttonObj) {
        var inputObj = jq("<input/>")
            .attr("id", "fileupload")
            .attr("type", "file")
            .attr("multiple", "multiple")
            .css("width", "0")
            .css("height", "0")
            .hide();

        inputObj.appendTo(buttonObj.parent());

        buttonObj.on("click", function (e) {
            e.preventDefault();
            jq("#fileupload").click();
        });
    };

    var getSubmitUrl = function () {
        var url = "UploadProgress.ashx?submit=" + config.FileUploadHandler;
        var data = config.Data;
        for (var prop in data) {
            url += "&{0}={1}".format(prop, data[prop]);
        }
        return url;
    };

    var bindEvents = function (uploader) {

        uploader
            .bind("fileuploadstart", function () {
                if (config.Events.OnPreUploadStart) {
                    config.Events.OnPreUploadStart();
                }
                if (config.Events.OnUploadStart) {
                    config.Events.OnUploadStart();
                } else {
                    FileUploadManager._uploadStart();
                }
            })

            .bind("fileuploadadd", config.Events.OnUploadAdd ? config.Events.OnUploadAdd : FileUploadManager._uploadAdd)

            .bind("fileuploadsubmit", config.Events.OnUploadSubmit ? config.Events.OnUploadSubmit : FileUploadManager._uploadSubmit)

            .bind("fileuploadsend", config.Events.OnUploadSend ? config.Events.OnUploadSend : FileUploadManager._uploadSend)

            .bind("fileuploadprogress", config.Events.OnUploadProgress ? config.Events.OnUploadProgress : FileUploadManager._uploadProgress)

            .bind("fileuploaddone", config.Events.OnUploadDone ? config.Events.OnUploadDone : FileUploadManager._uploadDone)

            .bind("fileuploadfail", config.Events.OnUploadFail ? config.Events.OnUploadFail : FileUploadManager._uploadFail)

            .bind("fileuploadstop", function () {
                if (config.Events.OnUploadStop) {
                    config.Events.OnUploadStop();
                } else {
                    FileUploadManager._uploadStop();
                }

                if (config.Events.OnPostUploadStop) {
                    config.Events.OnPostUploadStop();
                }
            });


        if (dragDropEnabled && config.DropZone) {
            jq("#" + config.DropZone)
		       .bind("dragenter", FileUploadManager._dragEnter)
		       .bind("dragleave", FileUploadManager._dragLeave)
	           .bind("dragover", FileUploadManager._dragOver)
		       .bind("drop", FileUploadManager._drop);
        }

        jq(document).on("click", "#" + config.TargetContainerID + " .fu_item_delete", function (e) {
            var fileId = this.id.replace("fu_item_delete_", "");
            jq("#fu_item_" + fileId).remove();
            FileUploadManager.RemoveFile(fileId);

            if (config.Events.OnFileRemove)
                config.Events.OnFileRemove(e, getFileById(fileId));

            return false;
        });

        if (!autoUpload) {
            jq("#" + config.UploadButton).click(function (e) {
                FileUploadManager.StartUpload();
                e.preventDefault();
            });
        }

    };

    var getFileById = function (fileId) {
        for (var i = 0; i < uploadQueue.length; i++) {
            if (uploadQueue[i].files[0].id == fileId) {
                return uploadQueue[i].files[0];
            }
        }

        return null;
    };

    var createNewGuid = function () {
        var s4 = function () {
            return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
        };

        return (s4() + s4() + "-" + s4() + "-" + s4() + "-" + s4() + "-" + s4() + s4() + s4());
    };

    var getFileExtension = function (fileTitle) {
        if (typeof fileTitle == "undefined" || fileTitle == null) {
            return "";
        }
        fileTitle = fileTitle.trim();
        var posExt = fileTitle.lastIndexOf(".");
        return 0 <= posExt ? fileTitle.substring(posExt).trim().toLowerCase() : "";
    };

    var correctFile = function (file) {

        var ext = getFileExtension(file.name);

        if (config.UploadableExts && jq.inArray(ext, config.UploadableExts) == -1) {
            displayInfoPanel(config.NotSupportedFormatErrorMsg || "Sorry, this file format isn't supported", true);
            return false;
        }

        if (file.size <= 0) {
            displayInfoPanel(config.EmptyFileErrorMsg || "Empty file", true);
            return false;
        }
        
        if (file.size > config.MaxSize) {
            displayInfoPanel(config.FileSizeErrorMsg || "The maximum file size is exceeded", true);
            return false;
        }

        return true;
    };

    var displayInfoPanel = function (str, warn) {
        if (!str) return;

        if (warn)
            toastr.error(str);
        else
            toastr.success(str);

    };

    var hideDragHighlight = function () {
        if (config.DropZone != null) {
            jq("#" + config.DropZone).css(config.HideDragHighlightStyle == null ? {
                "border-style": "solid",
                "border-color": "#d1d1d1",
                "background-color": "#FFF"
            } : config.HideDragHighlightStyle);
        }
    };
    
    var showDragHighlight = function () {
        if (config.DropZone != null) {
            jq("#" + config.DropZone).css(config.ShowDragHighlightStyle == null ? {
                "border-style": "dashed",
                "border-color": "#98BCA1",
                "background-color": "#E0FFE0"
            } : config.ShowDragHighlightStyle);
        }
    };
       
    var renderFileRow = function (file, serverData) {
        var data = {
            id: file.id,
            fileName: file.name,
            fileNameCss: "",
            returnCode: file.status,
            progressBarCss: "",
            descriptionCss: "studioFileUploaderDescription",
            deleteCss: config.DeleteLinkCSSClass || "linkAction",
            loadingImageCss: config.LoadingImageCSSClass || "",
            completeCss: config.CompleteCSSClass || "",
            descriptionText: "",
            exceedLimit: file.size >= config.MaxSize,
            fileSize: file.size,
            progressBarWidth: file.percent + "%",
            status: file.status,
            serverData: serverData,
        };

        if (jQuery.browser.safari)
            hideDragHighlight();

        var mb = 1024 * 1024;
        var kb = 1024;

        if (data.fileSize <= mb) {
            data.fileSize = (data.fileSize / kb).toFixed(2) + " KB";
        } else {
            data.fileSize = (data.fileSize / mb).toFixed(2) + " MB";
        }

        if (config.OnPrepareRenderData) {
            config.OnPrepareRenderData(data);
        }

        if (jq("#fu_item_" + data.id).length != 0) {
            if (config.Events.OnRenderItemInUploadingProcess != null) {
                config.Events.OnRenderItemInUploadingProcess(data);
            } else {
                jq("#fu_item_" + data.id).replaceWith(jq.tmpl(FileUploadManager.renderedItemTemplate, data));
            }
        } else {
            jq("#" + config.TargetContainerID).append(jq.tmpl(FileUploadManager.renderedItemTemplate, data));
        }

        if (data.status == uploadStatus.STARTED) {
            jq("#fu_item_delete_{0}".format(data.id)).hide();
            jq("#fu_item_loading_{0}".format(data.id)).show();
            jq("#fu_item_{0} div.studioFileUploaderProgressBar".format(data.id)).width(data.progressBarWidth).css("background-color", "#C2DFED");
        }

        if (data.status == uploadStatus.DONE) {
            jq("#fu_item_loading_{0}".format(data.id)).hide();

            if (data.serverData && data.serverData.Success) {
                jq("#fu_item_complete_{0}".format(data.id)).show();

                if (config.DeleteAfterUpload)
                    jq("#fu_item_delete_{0}".format(data.id)).show();
                else
                    jq("#fu_item_delete_{0}".format(data.id)).hide();

                if (config.TransferCompleteSuccessCssStyle != null)
                    jq("#fu_item_{0} div.studioFileUploaderProgressBar".format(data.id)).css(config.TransferCompleteSuccessCssStyle);
                else
                    jq("#fu_item_{0} div.studioFileUploaderProgressBar".format(data.id)).width("100%").css("background-color", "#EDF6FD");

            } else {

                if (config.TransferCompleteFailureCssStyle != null)
                    jq("#fu_item_{0} div.studioFileUploaderProgressBar".format(data.id)).css(config.TransferCompleteFailureCssStyle);
                else
                    jq("#fu_item_{0} div.studioFileUploaderProgressBar".format(data.id)).width("100%").css("background-color", "#FFE4C4");
            }
        }
        
        if (data.status == uploadStatus.FAILED) {
            jq("#fu_item_{0} div.studioFileUploaderProgressBar".format(data.id)).width("100%").css("background-color", "#FFE4C4");
        }

        jq("#" + config.TargetContainerID).scrollTo("#fu_item_" + data.id);
    };
}