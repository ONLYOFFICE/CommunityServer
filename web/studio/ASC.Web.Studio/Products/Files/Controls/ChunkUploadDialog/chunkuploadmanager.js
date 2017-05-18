/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


window.ASC.Files.ChunkUploads = (function () {

    //properties
    var isInit = false;
    var tenantQuota = null;
    var uploadFolderId;
    var successfullyUploadedFiles = 0;
    var firstHighlight = {fisrt: true, can: false};

    var uploadQueue = new Array();
    var uploaderBusy = false;
    var uploadSession = 0;

    var convertQueue = new Array();
    var converterBusy = false;
    var convertTimeout = null;

    var createdSubfolders = {};

    var dropElement = document.body;
    var dragLeaveTimeout = null;

    var dragDropEnabled = ("draggable" in document.createElement("span"));
    var progressEnanled = ("value" in document.createElement("progress"));

    var uploadStatus = {
        QUEUED: 0,
        STARTED: 1,
        STOPED: 2,
        DONE: 3,
        FAILED: 4
    };


    //init chunk upload dialog
    var init = function () {
        if (isInit === false) {
            isInit = true;
            StudioManager.addPendingRequest(ASC.Files.ChunkUploads.initTenantQuota);

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.ChunkUploadCheckConversion, onCheckConvertStatus);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.ChunkUploadGetFileFromServer, onGetFileFromServer);
        }

        if (!dragDropEnabled) {
            jq("#emptyContainer .emptyContainer_dragDrop").remove();
        }

        window.onbeforeunload = function () {
            if (ASC.Files.ChunkUploads.uploaderBusy) {
                return (ASC.Files.FilesJSResources.ConfirmLeavePage || "Are you sure you want to leave the current page. Downloading files will be interrupted.");
            }
        };
    };

    var initTenantQuota = function () {
        window.Teamlab.getQuotas({}, {
            success: function (params, data) {
                ASC.Files.ChunkUploads.tenantQuota = data;
                ASC.Files.ChunkUploads.changeQuotaText();

                activateUploader();
            }
        });
    };

    var getUploadSession = function (file) {
        var urlFormat = jq.format("{0}files/{1}/upload/create_session.json", ASC.Resources.Master.ApiPath, file.fid);
        var initResponse = null;

        jq.ajax({
            type: "POST",
            data: {
                fileName: file.name,
                fileSize: file.size,
                relativePath: (file.relativePath || ""),
            },
            url: jq.format(urlFormat, file.fid),
            async: false,
            success: function (data) {
                initResponse = data.response;
            },
            error: function (data) {
                try {
                    var resp = jq.parseJSON(data.responseText);
                    var message = resp.error.message;
                } catch (e) {
                    message = data.statusText;
                }
                initResponse = {"success": false, "message": message};
            }
        });

        return initResponse;
    };

    var initDialogView = function () {
        var storageKey = ASC.Files.Constants.storageKeyUploaderCompactView;
        var compactView = localStorageManager.getItem(storageKey);

        jq("#chunkUploadDialog").toggleClass("min", compactView === true);
        jq("#uploadCompactViewCbx").prop("checked", compactView === true);

        jq(".store-original").prop("checked", (ASC.Files.Common.storeOriginal === true));
    };

    var createFileuploadInput = function (buttonObj) {
        var inputObj = jq("<input/>")
            .attr("id", "fileupload")
            .attr("type", "file")
            .attr("multiple", "multiple")
            .css("width", "0")
            .css("height", "0");

        inputObj.appendTo(buttonObj.parent());

        buttonObj.on("click", function (e) {
            e.preventDefault();
            jq("#fileupload").click();
        });
    };

    var activateUploader = function () {
        if (!jq("#buttonUpload").hasClass("not-ready")) {
            return;
        }

        successfullyUploadedFiles = 0;

        createFileuploadInput(jq("#buttonUpload"));

        var chunkUploader = jq("#fileupload").fileupload({
            url: null,
            autoUpload: false,
            singleFileUploads: true,
            sequentialUploads: true,
            maxChunkSize: ASC.Files.Constants.CHUNK_UPLOAD_SIZE,
            progressInterval: 1000,
            dropZone: dragDropEnabled ? jq(dropElement) : null
        });

        chunkUploader
            .bind("fileuploadadd", onUploadAdd)
            .bind("fileuploadsubmit", onUploadSubmit)
            .bind("fileuploadsend", onUploadSend)
            .bind("fileuploadchunksend", onUploadSend)
            .bind("fileuploadprogress", onUploadProgress)
            .bind('fileuploadchunkdone', onUploadProgress)
            .bind("fileuploaddone", onUploadDone)
            .bind("fileuploadfail", onUploadFail)
            .bind("fileuploadalways", onUploadAlways)
            .bind("fileuploadstart", onUploadStart)
            .bind("fileuploadstop", onUploadStop)
            .bind("fileuploaddrop", onUploadDrop);

        if (dragDropEnabled) {
            jq(dropElement)
                .bind("dragenter", function () { return false; })
                .bind("dragleave", onDragLeave)
                .bind("dragover", onDragOver)
                .bind("drop", onFilesDrop);

            jq(dropElement).css({"position": "static"});
        }

        jq("#buttonUpload").removeClass("not-ready");
    };


    //uploader custom events
    var onUploadStart = function () {
        changeHeaderText(ASC.Files.FilesJSResources.Uploading);
        showUploadDialod();
        ASC.Files.ChunkUploads.uploaderBusy = true;
        jq("#abortUploadigBtn").show();
    };

    var onUploadStop = function () {
        if (successfullyUploadedFiles) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoUploadedSuccess.format(successfullyUploadedFiles));
        }

        ASC.Files.ChunkUploads.initTenantQuota();
        changeHeaderText(ASC.Files.FilesJSResources.UploadComplete);
        ASC.Files.ChunkUploads.uploaderBusy = false;
        jq("#abortUploadigBtn").hide();

        successfullyUploadedFiles = 0;
        uploadSession++;
        ASC.Files.ChunkUploads.createdSubfolders = {};
    };

    var onUploadDrop = function (e, data) {
        if (uploadFolderId) {
            jq.each(data.files, function (index, file) {
                file.fid = uploadFolderId;
            });

            uploadFolderId = null;
        }
    };

    var onUploadAdd = function (e, data) {
        var file = data.files[0];
        var folderId = ASC.Files.Folders.currentFolder.id;
        var canShare = ASC.Files.Share && ASC.Files.Folders.currentFolder.shareable;
        var rightChecked = true;

        if (ASC.Files.Common.isCorrectId(file.fid)) {
            folderId = file.fid;
        } else {
            rightChecked = ASC.Files.UI.accessEdit();
        }

        if (rightChecked && correctFile(file, folderId)) {
            file.id = createNewGuid();
            file.name = ASC.Files.Common.replaceSpecCharacter(file.name);
            file.fid = folderId;
            file.canShare = canShare;
            file.status = uploadStatus.QUEUED;
            file.percent = 0;
            file.loaded = 0;

            renderFileRow(file, false);

            data.id = createNewGuid();
            data.session = uploadSession;

            ASC.Files.ChunkUploads.uploadQueue.push(data);
        }

        if (!ASC.Files.ChunkUploads.uploaderBusy) {
            runFileUploading();
        }
    };

    var onUploadSubmit = function (e, data) {
        var file = data.files[0];
        var session = getUploadSession(file);

        if (session.success) {
            data.url = session.data.location;
            file.status = uploadStatus.STARTED;
            jq(this).fileupload("option", "url", session.data.location);

            getSubfolder(file.fid, file.relativePath, (session.data.path || new Array()));
        } else {
            file.status = uploadStatus.FAILED;
            jq(this).fileupload("option", "url", null);
            ASC.Files.UI.displayInfoPanel(session.message, true);
            showFileUploadingError(file.id, session.message);
        }
    };

    var onUploadSend = function (e, data) {
        var file = data.files[0];
        return file.status == uploadStatus.STARTED;
    };

    var onUploadProgress = function (e, data) {
        var file = data.files[0];

        var result = data.result;

        if (typeof result == "undefined" || result && result.success) {
            file.percent = parseInt(data.loaded / data.total * 100, 10);
            file.loaded = data.loaded;

            if (file.actionText == ASC.Files.FilesJSResources.FileUploading) {
                updateFileRow(file);
                changeHeaderText(null, data.session);
            }

            if (result && result.data && result.data.uploaded) {
                onUploadDone(e, data);

                var uploadData = getUploadDataByStatus(uploadStatus.STARTED);
                if (uploadData && uploadData.files[0].id == file.id) {
                    uploadData.abort();
                }
            }
        } else {
            uploadData = getUploadDataByStatus(uploadStatus.STARTED);
            file.percent = 100;
            file.loaded = file.size;
            file.status = uploadStatus.FAILED;

            ASC.Files.UI.displayInfoPanel(result.message, true);
            showFileUploadingError(file.id, result.message);

            if (uploadData && uploadData.files[0].id == file.id) {
                uploadData.abort();
            }
        }
    };

    var onUploadDone = function (e, data) {
        var file = data.files[0];
        if (file.status == uploadStatus.DONE) {
            return;
        }

        file.percent = 100;
        file.loaded = file.size;

        var result = data.result;
        if (result.success) {
            file.status = uploadStatus.DONE;
            file.data = result.data;

            successfullyUploadedFiles++;

            var canConvert = checkFileConvert(file.data);
            var showData = !canConvert || (canConvert && ASC.Files.Common.storeOriginal);

            if (file.data.folderId != ASC.Files.Folders.currentFolder.id) {

                var relativePath = (file.relativePath || "").split("/")[0];
                var key = file.fid + "/" + relativePath;

                if (ASC.Files.ChunkUploads.createdSubfolders[key]) {
                    correctFolderCount(ASC.Files.ChunkUploads.createdSubfolders[key]);
                } else {
                    correctFolderCount(file.data.folderId);
                }

            } else if (!ASC.Files.UI.isSettingsPanel()) {
                getFileFromServer(file.data.id, file.data.version, showData);
            }

            if (canConvert) {
                addFileToConvertQueue(file.data.id, file.data.version);
                renderFileRow(file, true);

                if (!ASC.Files.ChunkUploads.converterBusy) {
                    ASC.Files.ChunkUploads.converterBusy = true;
                    checkConvertStatus();
                }
            } else {
                updateFileRow(file);
            }

            //track event
            trackingGoogleAnalitics("documents", "upload", "file");
        } else {
            file.status = uploadStatus.FAILED;
            ASC.Files.UI.displayInfoPanel(result.message, true);
            showFileUploadingError(file.id, result.message);
        }
    };

    var onUploadFail = function (e, data) {
        var file = data.files[0];
        file.percent = 100;
        file.loaded = file.size;

        if (data.errorThrown == "abort" || data.textStatus == "abort") {
            if (file.status != uploadStatus.FAILED) {
                file.status = uploadStatus.STOPED;
                showFileUploadingCancel(file.id);

                var uploadHandler = jq(this).fileupload("option", "url");
                uploadHandler += (uploadHandler.indexOf("?") == -1 ? "?" : "&") + "abort=true";
                jq.ajax({
                    type: "POST",
                    url: uploadHandler,
                    async: true,
                    success: function (session) {
                        if (!session.success) {
                            ASC.Files.UI.displayInfoPanel(session.message, true);
                        }
                    },
                    error: function (dataAbort, status, errMsg) {
                        ASC.Files.UI.displayInfoPanel(errMsg, true);
                    }
                });
            }
        } else {
            file.status = uploadStatus.FAILED;
            var msg = data.errorThrown || data.textStatus;
            if (msg) {
                ASC.Files.UI.displayInfoPanel(msg, true);
                showFileUploadingError(file.id, msg);
            }
        }
    };

    var onUploadAlways = function (e, data) {
        runFileUploading();
    };

    var onDragLeave = function (e) {
        if (e.relatedTarget == null) {
            dragLeaveTimeout = setTimeout(ASC.Files.ChunkUploads.hideDragHighlight, 1);
        }
    };

    var onDragOver = function (e) {
        clearTimeout(dragLeaveTimeout);
        var dt = e.originalEvent.dataTransfer;
        if (!canDrop(dt)) {
            return true;
        }

        //bugfix chrome
        if (jq.browser.webkit) {
            dt.dropEffect = "copy";
        }

        showDragHighlight(e);
        return false;
    };

    var onFilesDrop = function (e) {
        var dt = e.originalEvent.dataTransfer;
        if (!canDrop(dt)) {
            return true;
        }

        ASC.Files.ChunkUploads.hideDragHighlight();
        return false;
    };

    var correctFile = function (file, folderId) {
        var posExt = ASC.Files.Utility.GetFileExtension(file.name);

        if (ASC.Files.Constants.UPLOAD_FILTER && jq.inArray(posExt, ASC.Files.Utility.Resource.ExtsUploadable) == -1) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.ErrorMassage_NotSupportedFormat, true);
            return false;
        }

        var sizeF = file.size;

        if (sizeF <= 0) {
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.ErrorMassage_EmptyFile, true);
            return false;
        }

        if (!ASC.Files.ThirdParty || !ASC.Files.ThirdParty.isThirdParty(null, "folder", folderId)) {
            if (ASC.Files.ChunkUploads.tenantQuota == null) {
                return false;
            }
            if (ASC.Files.ChunkUploads.tenantQuota.availableSize < sizeF) {
                if (!ASC.Files.UI.displayTariffLimitStorageExceed()) {
                    ASC.Files.UI.displayInfoPanel(jq.format(ASC.Files.FilesJSResources.ErrorMassage_StorageSize, FileSizeManager.filesSizeToString(ASC.Files.ChunkUploads.tenantQuota.availableSize)), true);
                }
                return false;
            }

            if (ASC.Files.ChunkUploads.tenantQuota.maxFileSize < sizeF) {
                if (!ASC.Files.UI.displayTariffFileSizeExceed()) {
                    ASC.Files.UI.displayInfoPanel(jq.format("{0} ({1})", ASC.Files.FilesJSResources.ErrorMassage_FileSize, FileSizeManager.filesSizeToString(ASC.Files.ChunkUploads.tenantQuota.maxFileSize)), true);
                }
                return false;
            }
        }

        var desktopFileSize = 100 * 1024 * 1024;
        if (!!window["AscDesktopEditor"] && sizeF > desktopFileSize) {
            ASC.Files.UI.displayInfoPanel(jq.format("{0} ({1})", ASC.Files.FilesJSResources.ErrorMassage_FileSize, FileSizeManager.filesSizeToString(desktopFileSize)), true);
            return false;
        }

        return true;
    };

    var getFileDataByObj = function (entryObject) {
        entryObject = jq(entryObject);
        if (!entryObject.is(".fu-row")) {
            entryObject = entryObject.closest(".fu-row");
        }
        if (entryObject.length == 0) {
            return null;
        }

        var fileId = entryObject.attr("id");

        return getFileDataById(fileId);
    };

    var getFileDataById = function (id, version) {
        var file = null;
        for (var i = ASC.Files.ChunkUploads.uploadQueue.length; i > 0; i--) {
            file = ASC.Files.ChunkUploads.uploadQueue[i - 1].files[0];
            if ((file.id == id || file.data && file.data.id == id) && (!version || file.data && file.data.version == parseInt(version))) {
                return file;
            }
        }

        return file;
    };

    var getFileFromServer = function (fileId, fileVersion, showData) {
        ASC.Files.ServiceManager.getFile(ASC.Files.ServiceManager.events.ChunkUploadGetFileFromServer,
            {
                fileId: fileId,
                version: fileVersion,
                show: showData,
                isStringXml: false
            });
    };

    var onGetFileFromServer = function (stringXmlFile, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }
        writeFileRow(params.fileId, stringXmlFile, params.show);
    };

    var getSubfolder = function (currentFolderId, relativePath, path) {
        if (currentFolderId != ASC.Files.Folders.currentFolder.id) {
            return;
        }

        var destinationFolderId = path[path.length - 1];

        if (!currentFolderId || !destinationFolderId
            || currentFolderId == destinationFolderId) {
            return;
        }

        relativePath = (relativePath || "").split("/")[0];

        var key = currentFolderId + "/" + relativePath;
        if (ASC.Files.ChunkUploads.createdSubfolders[key]) {
            return;
        }

        var index = -1;
        jq(path).each(function (i, item) {
            if (item == currentFolderId) {
                index = i;
                return false;
            }
            return true;
        });

        if (index == -1) {
            return;
        }

        var folderId = path[index + 1];

        ASC.Files.ChunkUploads.createdSubfolders[key] = folderId;

        if (ASC.Files.UI.getEntryObject("folder", folderId).length) {
            return;
        }

        ASC.Files.ServiceManager.getFolder(
            ASC.Files.ServiceManager.events.GetItems,
            {
                folderId: folderId,
                parentFolderID: currentFolderId,
                lonelyType: "folder",
            }
        );
    };


    //uploader row actions
    var renderFileRow = function (file, isConverterRow) {
        if (!file) {
            return;
        }

        file.fileTypeCssClass = ASC.Files.Utility.getCssClassByFileTitle(file.name, true);
        file.percent = 0;
        file.showAnim = !progressEnanled;

        if (!isConverterRow) {
            file.actionText = ASC.Files.FilesJSResources.FileUploading;
            file.completeActionText = ASC.Files.FilesJSResources.FileUploaded;
        } else {
            file.actionText = ASC.Files.FilesJSResources.FileConverting;
            file.completeActionText = ASC.Files.FilesJSResources.FileConverted;
        }

        var $newRow = jq("#fileUploaderRowTmpl").tmpl(file); //in bundle var $newRow = jq.tmpl("fileUploaderRowTmpl", file);
        var $exstRow = jq("#" + file.id);

        if ($exstRow.length != 0) {
            $exstRow.replaceWith($newRow);
        } else {
            jq("#uploadFilesTable tbody").append($newRow);
            jq("#uploadFilesTable").parent().scrollTo("#" + file.id);
        }

        if (!isConverterRow) {
            jq("#" + file.id).addClass("upload");
        } else {
            jq("#" + file.id).addClass("convert");
        }
    };

    var updateFileRow = function (file) {
        if (!file) {
            return;
        }

        var $row = jq("#" + file.id);

        ASC.Files.UI.setProgressValue($row, file.percent);
        if (file.status == uploadStatus.DONE && file.percent == 100) {
            $row.removeClass("upload")
                .removeClass("convert")
                .addClass("done");

            if (file.hasOwnProperty("convertedData")) {
                $row.find(".fu-title-cell span").text(file.convertedData.title).attr("title", file.convertedData.title);
                $row.find(".ftFile_21").attr("class", ASC.Files.Utility.getCssClassByFileTitle(file.convertedData.title, true));
            }
        }
    };

    var showFileUploadingError = function (fileId, errorText) {
        jq("#" + fileId)
            .removeClass("upload")
            .removeClass("convert")
            .removeClass("done")
            .removeClass("canceled")
            .addClass("error");

        jq("#" + fileId + " .popup_helper").text(errorText || ASC.Files.FilesJSResources.UnknownErrorText);
    };

    var showFileUploadingCancel = function (fileId) {
        jq("#" + fileId)
            .removeClass("upload")
            .removeClass("convert")
            .removeClass("done")
            .removeClass("error")
            .addClass("canceled");
    };


    //change uploader dialog info text
    var changeHeaderText = function (text, session) {
        if (!text) {
            var count = 0;
            var size = 0;
            var loaded = 0;

            jq.each(ASC.Files.ChunkUploads.uploadQueue, function (index, item) {
                if (item.session == session) {
                    var file = item.files[0];
                    if (file.status != uploadStatus.STOPED) {
                        count++;
                        size += file.size;
                        loaded += file.loaded;
                    }
                }
            });

            var percent = parseInt(loaded / size * 100, 10);

            if (percent == 100) {
                percent = 99;
            }

            if (percent > 0) {
                text = ASC.Files.FilesJSResources.UploadingProgress.format(count, percent);
            } else {
                text = ASC.Files.FilesJSResources.Uploading;
            }
        }

        jq("#chunkUploadDialogHeader").text(text);
    };

    var changeQuotaText = function () {
        var usedSize = window.FileSizeManager.filesSizeToString(ASC.Files.ChunkUploads.tenantQuota.usedSize);
        var storageSize = window.FileSizeManager.filesSizeToString(ASC.Files.ChunkUploads.tenantQuota.storageSize);
        jq("#chunkUploadDialog .upload-info-container .free-space").text(
            ASC.Files.FilesJSResources.UsedSize.format(usedSize, storageSize)
        );
    };


    //dragdrop
    var canDrop = function (dataTransfer) {
        if (!dragDropEnabled) {
            return false;
        }

        if (jq.browser.safari) {
            return false;
        }

        if (!dataTransfer) {
            return false;
        }

        //file?
        //FF
        if (dataTransfer.types.contains && !dataTransfer.types.contains("Files")) {
            return false;
        }

        //Chrome
        if (dataTransfer.types.indexOf && dataTransfer.types.indexOf("Files") == -1) {
            return false;
        }

        return true;
    };

    var hideDragHighlight = function () {
        jq(".may-drop-to").removeClass("may-drop-to");
        jq(".drop-to").removeClass("drop-to");
        jq("#mainContent").removeClass("selected");
        firstHighlight.fisrt = true;
    };

    var showDragHighlight = function (e) {
        if (firstHighlight.fisrt) {
            ASC.Files.Mouse.highlightFolderTo("may-drop-to", true);
            firstHighlight.can = ASC.Files.UI.accessEdit();
            jq("#mainContent").toggleClass("selected", firstHighlight.can);
            firstHighlight.fisrt = false;
        }

        jq(".drop-to").removeClass("drop-to");

        var target = e.target || e.srcElement;
        var dropTo = jq(target).closest(".may-drop-to");
        if (dropTo.length) {
            uploadFolderId = ASC.Files.Mouse.getOverFolderId(e, dropTo, "drop-to");

            jq("#mainContent").toggleClass("selected", false);
        } else {
            jq("#mainContent").toggleClass("selected", firstHighlight.can);
            uploadFolderId = null;
        }
    };


    //upload file actions
    var getUploadDataByStatus = function (status) {
        for (var i = 0; i < ASC.Files.ChunkUploads.uploadQueue.length; i++) {
            if (ASC.Files.ChunkUploads.uploadQueue[i].files[0].status == status) {
                return ASC.Files.ChunkUploads.uploadQueue[i];
            }
        }
        return null;
    };

    var runFileUploading = function () {
        var uploadData = getUploadDataByStatus(uploadStatus.QUEUED);
        if (uploadData) {
            ASC.Files.ChunkUploads.uploaderBusy = true;
            uploadData.submit();
        }
    };

    var abortFileUploading = function (obj) {
        var file = getFileDataByObj(obj);
        if (file != null) {
            var uploadData = getUploadDataByStatus(uploadStatus.STARTED);
            if (uploadData && uploadData.files[0].id == file.id) {
                uploadData.abort();
            } else if (file.status == uploadStatus.QUEUED) {
                file.percent = 100;
                file.loaded = file.size;
                file.status = uploadStatus.STOPED;
                showFileUploadingCancel(file.id);
            }
        }
    };

    var abortUploading = function (clearDialog) {
        if (clearDialog) {
            jq("#uploadFilesTable tbody").html("");
            ASC.Files.ChunkUploads.uploadQueue = new Array();
        } else {
            jq("#uploadFilesTable .fu-row:not(.done)").each(function () {
                var fileId = jq(this).attr("id");
                showFileUploadingCancel(fileId);
            });
        }

        jq.each(ASC.Files.ChunkUploads.uploadQueue, function (index, item) {
            var file = item.files[0];
            if (file.status == uploadStatus.QUEUED) {
                file.percent = 100;
                file.loaded = file.size;
                file.status = uploadStatus.STOPED;
            }
        });

        var uploadData = getUploadDataByStatus(uploadStatus.STARTED);
        if (uploadData) {
            uploadData.abort();
        }
    };

    var shareUploadedFile = function (obj) {
        var fileData = getFileDataByObj(obj);
        if (fileData.canShare) {
            var entryData = fileData.convertedData ? fileData.convertedData : fileData.data;
            ASC.Files.Share.getSharedInfo("file_" + entryData.id, entryData.title);
        }
    };

    var clickOnUploadedFile = function (obj) {
        var fileData = getFileDataByObj(obj);
        var entryData = fileData.convertedData ? fileData.convertedData : fileData.data;
        ASC.Files.Folders.clickOnFile(entryData, true);
    };

    var showUploadingErrorText = function (obj) {
        var row = jq(obj).closest(".fu-row");
        var rowId = row.attr("id");
        jq(obj).helper({
            BlockHelperID: rowId + ' .popup_helper',
            position: 'fixed'
        });
    };

    var disableBrowseButton = function (disable) {
        jq("#fileupload").prop("disabled", disable).css("visibility", disable ? "hidden": "visible");
    };


    //convert functions
    var checkFileConvert = function (fileData) {
        if (!ASC.Files.Constants.ENABLE_UPLOAD_CONVERT) {
            return false;
        }

        return ASC.Files.Utility.MustConvert(fileData.title);
    };

    var addFileToConvertQueue = function (id, version) {
        ASC.Files.Marker.removeNewIcon("file", id);

        ASC.Files.ChunkUploads.convertQueue.push({
            id: id,
            version: version,
            needToRun: true
        });
    };

    var removeFileFromConvertQueue = function (id, version) {
        var itemIndex = -1;

        jq.each(ASC.Files.ChunkUploads.convertQueue, function (index, item) {
            if (item.id == id && item.version == version) {
                itemIndex = index;
            }
        });

        if (itemIndex != -1) {
            ASC.Files.ChunkUploads.convertQueue.splice(itemIndex, 1);
        }
    };

    var changeConvertQueueItemStatus = function (id, version) {
        jq.each(ASC.Files.ChunkUploads.convertQueue, function (index, item) {
            if (item.id == id && item.version == version) {
                item.needToRun = false;
            }
        });
    };

    var checkConvertStatus = function () {
        var data = {};
        data.entry = new Array();

        jq.each(ASC.Files.ChunkUploads.convertQueue, function (index, item) {
            data.entry.push({
                entry: [item.id, item.version, item.needToRun === true]
            });
        });

        if (data.entry.length > 0) {
            ASC.Files.ServiceManager.checkConversion(ASC.Files.ServiceManager.events.ChunkUploadCheckConversion, {}, {stringListList: data});
        } else {
            ASC.Files.ChunkUploads.converterBusy = false;
        }
    };

    var onCheckConvertStatus = function (obj, params, errorMessage) {
        if (typeof obj !== "object" && typeof errorMessage != "undefined" || obj == null) {
            errorMessage = errorMessage || ASC.Files.FilesJSResources.ErrorMassage_ErrorConvert;
        } else if (!obj.length) {
            errorMessage = ASC.Files.FilesJSResources.ErrorMassage_ErrorConvert;
        }

        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            clearTimeout(convertTimeout);
            jq.each(ASC.Files.ChunkUploads.convertQueue, function (index, item) {
                var fileData = getFileDataById(item.id, item.version);
                if (fileData) {
                    showFileUploadingError(fileData.id, errorMessage);
                }
            });
            showFileData();
            ASC.Files.ChunkUploads.convertQueue.clear();
            ASC.Files.ChunkUploads.converterBusy = false;
            return;
        }

        for (var i = 0; i < obj.length; i++) {
            var source = jq.parseJSON(obj[i].source);
            var file = getFileDataById(source.id, source.version);

            changeConvertQueueItemStatus(source.id, source.version);

            if (obj[i].error != "") {
                ASC.Files.UI.displayInfoPanel(obj[i].error, true);
                removeFileFromConvertQueue(source.id, source.version);
                if (file) {
                    showFileUploadingError(file.id, obj[i].error);
                    showFileData(file.data.id);
                }
            } else {
                var percent = obj[i].progress || 0;

                var isDone = false;
                var convertResult = null;
                if (obj[i].result) {
                    convertResult = jq.parseJSON(obj[i].result);
                }

                if (convertResult) {
                    var fileObj = ASC.Files.UI.getEntryObject("file", convertResult.id);
                    var odjData = ASC.Files.UI.getObjectData(fileObj);
                }

                if (odjData && odjData.version == convertResult.version | 0) {
                    percent = 100;
                    isDone = true;
                } else if (percent == 100 && convertResult) {
                    if (convertResult.folderId != ASC.Files.Folders.currentFolder.id) {
                        correctFolderCount(file.fid);
                    } else if (!ASC.Files.UI.isSettingsPanel()) {
                        var stringXmlFile = convertResult.fileXml;
                        writeFileRow(convertResult.id, stringXmlFile, true);
                    }

                    showFileData(source.id);
                    percent = 100;
                    isDone = true;
                }

                if (isDone) {
                    removeFileFromConvertQueue(source.id, source.version);
                }

                if (file) {
                    file.percent = percent;
                    if (isDone && convertResult) {
                        file.convertedData = convertResult;
                    }
                    updateFileRow(file);
                }
            }
        }

        if (ASC.Files.ChunkUploads.convertQueue.length == 0) {
            ASC.Files.ChunkUploads.converterBusy = false;
            clearTimeout(convertTimeout);
        } else {
            convertTimeout = setTimeout(checkConvertStatus, ASC.Files.Constants.REQUEST_CONVERT_DELAY);
        }
    };

    var correctFolderCount = function (folderId) {
        var folderToObj = ASC.Files.UI.getEntryObject("folder", folderId);

        var fileCountObj = folderToObj.find(".countFiles");

        fileCountObj.html((parseInt(fileCountObj.html()) || 0) + 1);
    };


    //upload dialog actions
    var showUploadDialod = function () {
        if (jq("#chunkUploadDialog:visible").length == 0) {
            ASC.Files.Actions.hideAllActionPanels();
            initDialogView();
            jq("#chunkUploadDialog").show();
        }
    };

    var closeUploadDialod = function () {
        ASC.Files.ChunkUploads.abortUploading(true);
        jq("#chunkUploadDialog").hide();
    };

    var toggleUploadDialod = function () {
        jq("#chunkUploadDialog").toggleClass("min");
    };

    var changeCompactView = function (obj) {
        var isCompact = jq(obj).prop("checked");
        var storageKey = ASC.Files.Constants.storageKeyUploaderCompactView;
        localStorageManager.setItem(storageKey, isCompact);
    };

    var writeFileRow = function (fileId, stringXmlFile, showData) {
        var params = {
            fileId: fileId,
            show: showData,
            isStringXml: true
        };

        ASC.Files.EventHandler.onGetFile(stringXmlFile, params);
    };

    var showFileData = function (fileId) {
        if (fileId) {
            var fileObj = ASC.Files.UI.getEntryObject("file", fileId);
            if (fileObj.length != 0 && fileObj.hasClass("new-file")) {
                ASC.Files.EmptyScreen.hideEmptyScreen();
                fileObj.removeClass("new-file").show().yellowFade();
            }
        } else {
            var list = jq("#filesMainContent .new-file");
            if (list.length > 0) {
                ASC.Files.EmptyScreen.hideEmptyScreen();
                list.removeClass("new-file").show().yellowFade();
            }
        }
    };

    var createNewGuid = function () {
        var s4 = function () {
            return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
        };

        return (s4() + s4() + "-" + s4() + "-" + s4() + "-" + s4() + "-" + s4() + s4() + s4());
    };

    return {
        uploadQueue: uploadQueue,
        uploaderBusy: uploaderBusy,

        convertQueue: convertQueue,
        converterBusy: converterBusy,

        init: init,
        initTenantQuota: initTenantQuota,
        tenantQuota: tenantQuota,
        changeQuotaText: changeQuotaText,

        abortFileUploading: abortFileUploading,
        abortUploading: abortUploading,

        clickOnUploadedFile: clickOnUploadedFile,
        shareUploadedFile: shareUploadedFile,
        showUploadingErrorText: showUploadingErrorText,

        disableBrowseButton: disableBrowseButton,

        showUploadDialod: showUploadDialod,
        closeUploadDialod: closeUploadDialod,
        toggleUploadDialod: toggleUploadDialod,

        changeCompactView: changeCompactView,

        createdSubfolders: createdSubfolders,

        hideDragHighlight: hideDragHighlight,
    };
})();

(function ($) {

    $(function () {

        if (jq("#chunkUploadDialog").length == 0)
            return;

        ASC.Files.ChunkUploads.init();

        ASC.Files.Common.storeOriginal = jq("#chunkUploadDialog .store-original").prop("checked");

        jq("#chunkUploadDialog .actions-container.close").click(function () {
            ASC.Files.ChunkUploads.closeUploadDialod();
        });

        jq("#chunkUploadDialog .actions-container.minimize, #chunkUploadDialog .actions-container.maximize").click(function () {
            ASC.Files.ChunkUploads.toggleUploadDialod();
        });

        jq("#chunkUploadDialog").on("dblclick", ".progress-dialog-header", function () {
            ASC.Files.ChunkUploads.toggleUploadDialod();
        });

        jq("#chunkUploadDialog").on("click", ".fu-row.done .share", function () {
            ASC.Files.ChunkUploads.shareUploadedFile(this);
        });

        jq("#chunkUploadDialog").on("click", ".fu-row.done .fu-title-cell span", function () {
            ASC.Files.ChunkUploads.clickOnUploadedFile(this);
        });

        jq("#chunkUploadDialog").on("click", ".fu-row.upload .abort-file-uploadig", function () {
            ASC.Files.ChunkUploads.abortFileUploading(this);
        });

        jq("#chunkUploadDialog").on("click", "#abortUploadigBtn", function () {
            ASC.Files.ChunkUploads.abortUploading(false);
        });

        jq("#chunkUploadDialog").on("click", ".upload-error", function () {
            ASC.Files.ChunkUploads.showUploadingErrorText(this);
        });

        jq.dropdownToggle({
            switcherSelector: "#uploadSettingsSwitcher",
            dropdownID: "uploadSettingsPanel",
            afterShowFunction: function () {
                //hack to dialog with position:fixed
                var pos = jq("#uploadSettingsSwitcher").position();
                var height = jq("#uploadSettingsSwitcher").height() + 5;
                jq("#uploadSettingsPanel").css({ top: pos.top + height + "px", left: pos.left + "px" });
            }
        });

        jq("#chunkUploadDialog").on("change", "#uploadCompactViewCbx", function () {
            ASC.Files.ChunkUploads.changeCompactView(this);
        });

        jq("#chunkUploadDialog .files-container").scroll(function () {
            ASC.Files.Actions.hideAllActionPanels();
        });
    });
})(jQuery);