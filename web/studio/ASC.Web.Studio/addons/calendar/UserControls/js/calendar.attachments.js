/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


(function ($) {

    var instances = [],
        attachments = [],
        selectedFiles = [],
        attachedFiles = [],
        copiedFiles = [];

    var attachmentResources = g_fcOptions.attachments;

    var nextId = 0,
        nextOrderNumber = 0,
        maxFileSizeInMegaBytes = attachmentResources.maxFileSizeInMegaBytes,
        maxFileSize = maxFileSizeInMegaBytes * 1024 * 1024,

        saveHeandlerId = null,
        filesContainer = 'calendar_event_attachments',
        maxFileNameLen = 53,
        filenameColumnPaddingConst = 4,
        warningDialog = null;

        uploader = null,
        uploadQueue = new Array(),
        uploaderBusy = false,
        uploadStatus = {
            QUEUED: 0,
            STARTED: 1,
            STOPED: 2,
            DONE: 3,
            FAILED: 4
        };

    function getInstance(obj) {
        var instance = null;

        $.each(instances, function (index, item) {
            if (obj.is(item.input)) {
                instance = item;
                return false;
            }
            return true;
        });

        return instance;
    }

    function removeInstance(obj) {
        $.each(instances, function (index, item) {
            if (obj.is(item.obj)) {
                instances.splice(index, 1);
                return false;
            }
            return true;
        });
    }

    function fileUpload(obj, options) {
        var self = this;

        this.input = obj;
        this.container = options.container;
        this.canEdit = true;

        this.init = function () {
            $attachmentsClearBtn = $('#attachments_clear_btn');

            stopUploader();

            window.CalendarDocumentsPopup.unbind(window.CalendarDocumentsPopup.events.SelectFiles);

            uploader = createFileuploadInput('attachments_browse_btn');
            uploader.fileupload({
                url: null,
                autoUpload: false,
                singleFileUploads: true,
                progressInterval: 1000,
                dropZone: false
            });

            uploader
                .bind('fileuploadadd', onUploadAdd)
                .bind('fileuploadsubmit', onUploadSubmit)
                .bind('fileuploadsend', onUploadSend)
                .bind('fileuploadprogress', onUploadProgress)
                .bind('fileuploaddone', onUploadDone)
                .bind('fileuploadfail', onUploadFail)
                .bind('fileuploadalways', onUploadAlways);

            window.CalendarDocumentsPopup.bind(window.CalendarDocumentsPopup.events.SelectFiles, selectDocuments);

            $('#attachments_limit_txt').text(attachmentResources.limitLabel.replace('%1', maxFileSizeInMegaBytes));

            jq(document).on("click", "#" + filesContainer + " .delete_attachment", function (e) {
                var fileId = this.id.replace("remove_attachment_", "");
                removeAttachment(fileId);
                return false;
            });

            self.renderContainer();
        };

        this.renderContainer = function () {
            self.container.find('tbody').empty();
        };

        this.getItems = function () {
            return attachments;
        };

        this.addAttachments = function (attachmentsList) {
            attachments = [];
            addAttachments(attachmentsList);
        };
    }

    function onUploadAdd(e, data) {
        var file = data.files[0];
        file.orderNumber = getOrderNumber();
        file.percent = 0;
        file.status = uploadStatus.QUEUED;
        file.title = file.name;

        var attachment = convertPUfileToAttachment(file);
        addAttachment(attachment);

        correctFileNameWidth();

        if (!correctFile(e, data)) {
            file.status = uploadStatus.FAILED;
        }

        uploadQueue.push(data);

        if (!uploaderBusy) {
            runFileUploading();
        }
    }

    function onUploadSubmit(e, data) {
        var file = data.files[0];
        file.status = uploadStatus.STARTED;
        file.needSaveToTemp = false;
        $(this).fileupload('option', 'formData', [{ name: 'name', value: file.title }]);
        $(this).fileupload('option', 'url', generateSubmitUrl({
            eventId: 0,
            copyToMy: 0
        }));
    }

    function onUploadSend(e, data) {
        var file = data.files[0];
        displayAttachmentProgress(file.orderNumber, true);

        return file.status == uploadStatus.STARTED;
    }

    function onUploadProgress(e, data) {
        var file = data.files[0];
        file.percent = parseInt(data.loaded / data.total * 100, 10);

        setAttachmentProgress(file.orderNumber, file.percent);
    }

    function onUploadDone(e, data) {
        var file = data.files[0];
        file.percent = 100;
        file.status = uploadStatus.DONE;

        displayAttachmentProgress(file.orderNumber, false);

        var response = $.parseJSON(data.result);

        if (response) {
            if (!response.Success) {
                if (!response.Data) {
                    response.Data = {
                        id: -1,
                        title: file.title,
                        orderNumber: file.orderNumber,
                        fileUrl: ''
                    };
                }

                response.Data.error = response.Message;
                response.Data.size = 0;
            }

            if (response.Data.attachedAsLink) {
                response.Data.id = response.Data.id;
                response.Data.title = response.Data.title;
                response.Data.orderNumber = file.orderNumber;
                response.Data.fileUrl = response.FileURL;

                if (response.Data.error) {
                    showFileLinkAttachmentErrorStatus(file.orderNumber, response.Data.error);
                    correctFileNameWidth();
                    var pos = searchFileIndex(copiedFiles, file.orderNumber);
                    if (pos > -1) {
                        copiedFiles.splice(pos, 1);
                    }
                }
            } else {
                updateAttachment(file.orderNumber, response.Data);
            }
        }
    }

    function onUploadFail(e, data, msg) {
        var file = data.files[0];
        file.percent = 0;
        file.status = data.errorThrown == 'abort' ? uploadStatus.STOPED : uploadStatus.FAILED;

        var errorMsg = msg || data.errorThrown || data.textStatus;
        if (data.jqXHR && data.jqXHR.responseText) {
            errorMsg = $.parseJSON(data.jqXHR.responseText).Message;
        }

        if (file.orderNumber == undefined || file.orderNumber < 0) {
            file.orderNumber = getOrderNumber();
            var attachment = convertPUfileToAttachment(file);
            addAttachment(attachment);
        }

        data.result = JSON.stringify({
            Success: false,
            Title: file.title,
            FileURL: '',
            Data: {
                contentId: null,
                contentType: '',
                id: -1,
                title: file.title,
                size: 0,
                orderNumber: file.orderNumber,
                fileUrl: '',
                isNew: true
            },
            Message: errorMsg
        });

        onUploadDone(e, data);
    }

    function onUploadAlways() {
        runFileUploading();
    }

    function createFileuploadInput(buttonId) {
        var inputObj = $('#fileupload');

        if (inputObj.length) {
            return inputObj;
        }

        var buttonObj = $('#' + buttonId);

        inputObj = $('<input/>')
            .attr('id', 'fileupload')
            .attr('type', 'file')
            .attr('multiple', 'multiple')
            .css("width", "0")
            .css("height", "0")
            .hide();

        inputObj.appendTo(buttonObj.parent());

        buttonObj.on('click', function (e) {
            e.preventDefault();
            $('#fileupload').click();
        });

        $('#attachments_clear_btn').on('click', function () {
            removeAllAttachments();
        });

        return inputObj;
    }

    function correctFile(e, data) {
        var file = data.files[0];

        if (file.size <= 0) {
            onUploadFail(e, data, attachmentResources.emptyFileNotSupportedError);
            return false;
        }

        if (maxFileSize < file.size) {
            onUploadFail(e, data, attachmentResources.fileSizeError);
            return false;
        }

        return true;
    }

    function runFileUploading() {
        var uploadData = getUploadDataByStatus(uploadStatus.QUEUED);
        if (uploadData) {
            uploaderBusy = true;
            uploadData.submit();
        }
    }

    function getUploadDataByStatus(status) {
        for (var i = 0; i < uploadQueue.length; i++) {
            if (uploadQueue[i].files[0].status == status) {
                return uploadQueue[i];
            }
        }
        return null;
    }

    function getUploadDataByOrderNumber(number) {
        for (var i = 0; i < uploadQueue.length; i++) {
            if (uploadQueue[i].files[0].orderNumber == number) {
                return uploadQueue[i];
            }
        }
        return null;
    }

    function convertPUfileToAttachment(file) {
        var attachment = {
            contentId: null,
            contentType: '',
            id: -1,
            title: file.title,
            orderNumber: file.orderNumber,
            size: file.size,
            error: file.error || '',
            fileUrl: ''
        };

        completeAttachment(attachment);

        return attachment;
    }

    function convertDocumentToAttachment(document) {
        var attachment = {
            contentId: null,
            contentType: '',
            id: document.id,
            originalId: document.originalId,
            title: document.title,
            orderNumber: document.orderNumber,
            size: +document.size,
            storedName: '',
            streamId: '',
            error: document.error || '',
            fileUrl: document.fileUrl
        };

        completeAttachment(attachment);

        return attachment;
    }

    function getSizeString(size) {
        var sizeString = '';
        if (size != undefined) {
            var mb = 1024 * 1024;
            var kb = 1024;
            if (size <= mb) {
                if (size <= kb) {
                    sizeString = size + ' ' + attachmentResources.bytes;
                } else {
                    sizeString = (size / kb).toFixed(2) + ' ' + attachmentResources.kilobytes;
                }
            } else {
                sizeString = (size / mb).toFixed(2) + ' ' + attachmentResources.megabytes;
            }
        }
        return sizeString;
    }

    function addAttachments(attachmentsList) {
        var i, len = attachmentsList.length;
        for (i = 0; i < len; i++) {
            var attachment = attachmentsList[i];
            attachment.orderNumber = getOrderNumber();
            addAttachment(attachment);
        }
        correctFileNameWidth();
    }

    function addAttachment(attachment, status) {
        attachment.operation = status ? status : 0;
        attachment.attachedAsLink = false;
        if (attachment.isNew) {
            attachment.isUploaded = false;
        }

        var html = prepareFileRow(attachment);

        $('#' + filesContainer + ' tbody').append(html);

        attachments.push(attachment);
        $attachmentsClearBtn.show();
    }

    function getFileNameMaxWidth() {
        var fileinfoList = $('#' + filesContainer + ' .file_info');
        if (fileinfoList.length == 0) {
            return undefined;
        }

        var maxTableWidth = Math.max.apply(null, fileinfoList.map(function () {
            return $(this).find('.file-name').outerWidth(true) + $(this).find('.fullSizeLabel').outerWidth(true);
        }).get());
        return $.isNumeric(maxTableWidth) ? maxTableWidth + filenameColumnPaddingConst : maxTableWidth;
    }

    function correctFileNameWidth() {
        var fileinfoList = $('#' + filesContainer + ' .file_info');
        if (fileinfoList.length == 0) {
            return;
        }

        var maxTableWidth = getFileNameMaxWidth();
        if ($.isNumeric(maxTableWidth)) {
            fileinfoList.animate({ 'width': maxTableWidth }, 'normal');
        }
    }

    function updateAttachment(orderNumber, updateInfo) {
        if (orderNumber == undefined || orderNumber < 0) {
            return;
        }

        var pos = searchFileIndex(attachments, orderNumber);
        if (pos > -1) {
            var attachment = attachments[pos];
            attachment.contentType = updateInfo.contentType;
            attachment.id = updateInfo.id;
            attachment.title = updateInfo.title;
            attachment.size = updateInfo.size || 0;
            attachment.error = updateInfo.error || '';
            attachment.attachedAsLink = updateInfo.attachedAsLink;
            attachment.fileUrl = updateInfo.fileUrl;

            completeAttachment(attachment);

            var html = prepareFileRow(attachment);
            var maxTableWidth = getFileNameMaxWidth();
            if ($.isNumeric(maxTableWidth)) {
                $(html).find('.file_info').width(maxTableWidth);
            }

            $('#' + filesContainer + ' .row[data_id=' + orderNumber + ']').replaceWith(html);
        }
    }

    function prepareFileRow(attachment) {
        if (attachment == undefined) {
            return '';
        }

        var html = $("#attachmentEventTmpl").tmpl(attachment, {
            cutFileName: cutFileName,
            fileSizeToStr: getSizeString,
            getFileNameWithoutExt: getAttachmentName,
            getFileExtension: getAttachmentExtension
        });

        return html;
    }

    function displayAttachmentProgress(orderNumber, show) {
        var itemProgress = $('#item_progress_' + orderNumber);
        if (itemProgress != undefined) {
            if (show) {
                itemProgress.show();
            } else {
                itemProgress.hide();
            }
        }
    }

    function setAttachmentProgress(orderNumber, percent) {
        if (percent == undefined || percent < 0) {
            return;
        }

        var itemProgress = $('#item_progress_' + orderNumber + ':visible .progress-slider');
        if (itemProgress != undefined && itemProgress.length == 1) {
            itemProgress.css('width', percent + '%');
        }
    }

    function showFileLinkAttachmentErrorStatus(orderNumber, text) {
        var $attachment = $('#calendar_event_attachments .row[data_id="' + orderNumber + '"]');

        $attachment.find('.delete_attachment').show();
        $attachment.find('.load_result').empty();
        $attachment.find('.load_result').html('<span class="file-load-result red-text" title="' + text + '"> ' + text + '</span>');
    }

    function updateFileLinkAttachmentProgressStatus(orderNumber, percent, text) {
        var $attachment = $('#calendar_event_attachments .row[data_id="' + orderNumber + '"]');
        $attachment.addClass('inactive');

        $attachment.find('.load_result .file-load-result').remove();

        if (percent == 100) {
            $attachment.find('.load_result').html('<span class="file-load-result uploaded-text" title="' + text + '"> ' + text + '</span>');
            $attachment.find('.attachment-progress').hide();
        } else {
            $attachment.find('.attachment-progress .progress-label').text(text);
            $attachment.find('.attachment-progress .progress-slider').css('width', percent + '%');
            $attachment.find('.attachment-progress').show();
        }
    }

    function searchFileIndex(collection, orderNumber) {
        var pos = -1;
        var i, len = collection.length;
        for (i = 0; i < len; i++) {
            var file = collection[i];
            if (file.orderNumber == orderNumber) {
                pos = i;
                break;
            }
        }
        return pos;
    }

    function removeFromUploaderQueue(orderNumber) {
        var uploadData = getUploadDataByOrderNumber(orderNumber);
        if (uploadData) {
            var file = uploadData.files[0];
            if (file.status == uploadStatus.STARTED) {
                uploadData.abort();
            } else if (file.status == uploadStatus.QUEUED) {
                file.status = uploadStatus.STOPED;
            }
        }
    }

    function removeAttachment(orderNumber, needColumnWidthCorrection) {
        var pos = searchFileIndex(attachments, orderNumber);
        if (pos > -1) {
            attachments.splice(pos, 1);

            if (attachments.length == 0) {
                $attachmentsClearBtn.hide();
            }
        }

        removeAttachmentRow(orderNumber);
        removeFromUploaderQueue(orderNumber);

        if (needColumnWidthCorrection == undefined || needColumnWidthCorrection) {
            correctFileNameWidth();
        }
    }

    function removeAllAttachments() {
        var tempCollection = attachments.slice(); // clone array
        var i, len = tempCollection.length;
        for (i = 0; i < len; i++) {
            var attachment = tempCollection[i];
            removeAttachment(attachment.orderNumber, false);
        }
        $attachmentsClearBtn.hide();
    }

    function removeAttachmentRow(orderNumber) {
        $('#' + filesContainer + ' .row[data_id=' + orderNumber + ']').remove();
    }

    function getAttachmentName(fullName) {
        if (fullName) {
            var lastDotIndex = fullName.lastIndexOf('.');
            return lastDotIndex > -1 ? fullName.substr(0, lastDotIndex) : fullName;
        }
        return '';
    }

    function getAttachmentExtension(fullName) {
        if (fullName) {
            var lastDotIndex = fullName.lastIndexOf('.');
            return lastDotIndex > -1 ? fullName.substr(lastDotIndex) : '';
        }
        return '';
    }

    function getAttachmentWarningByExt(ext) {
        switch (ext) {
            case '.exe':
                return attachmentResources.executableWarning;
            default:
                return '';
        }
    }

    function stopUploader() {
        $('#' + filesContainer + ' tbody').empty();

        attachments = [];
        dataBeforeSave = [];
        documentsInLoad = [];
        selectedFiles = [];
        attachedFiles = [];
        copiedFiles = [];
        failedUploadedFiles = [];

        uploadQueue.clear();

        if (saveHeandlerId) {
            $(window).Teamlab.unbind(saveHeandlerId);
        }
    }

    function generateSubmitUrl(data) {
        var submitUrl = 'UploadProgress.ashx?submit=ASC.Web.Calendar.Handlers.FilesUploader,ASC.Web.Calendar';
        for (var prop in data) {
            submitUrl += '&{0}={1}'.format(prop, data[prop]);
        }
        return submitUrl;
    }

    function cutFileName(name) {
        if (name.length <= maxFileNameLen) {
            return name;
        }
        return name.substr(0, maxFileNameLen - 3) + '...';
    }

    function completeAttachment(attachment) {
        var name = attachment.title,
            ext = getAttachmentExtension(name),
            warn = getAttachmentWarningByExt(ext);

        attachment.iconCls = window.ASC.Files.Utility.getCssClassByFileTitle(name, true);

        attachment.warn = warn;
        attachment.operation = 0;
        attachment.isUploaded = true;
        attachment.isNew = true;

        return attachment;
    }

    function getNextId() {
        return ++nextId;
    }

    function getOrderNumber() {
        return nextOrderNumber++;
    }

    function selectDocuments(e, res) {
        var documents = res.data;

        selectedFiles = selectedFiles.concat(documents);

        attachFileLinks(documents);
        correctFileNameWidth();
    }

    function attachFileLinks(files) {
        for (var i = 0; i < files.length; i++) {
            var file = files[i];

            if (attachments.find(x => x.id == file.id || x.originalId == file.id)) {
                continue;
            }

            if (file.shareable && !file.denySharing) {
                attachedFiles.push(file);
            } else {
                copiedFiles.push(file);
            }
        }

        if (copiedFiles.length) {

            setTimeout(showWarningDialog, 0);

        } else {
            for (var i = 0; i < attachedFiles.length; i++) {
                insertFileLinkToEvent(attachedFiles[i]);

                var attachment = attachedFiles[i];
                attachment.size = 0;
                attachment.orderNumber = getOrderNumber();
                addAttachment(attachment);
            }

            completeCopiedFileLinkAttachmentsProgressStatus(attachedFiles);

            clearAttachedFiles();
        }
    }

    function completeCopiedFileLinkAttachmentsProgressStatus(files) {
        for (var i = 0; i < files.length; i++) {
            displayAttachmentProgress(files[i].orderNumber, true);
            updateFileLinkAttachmentProgressStatus(files[i].orderNumber, 100, attachmentResources.uploadedLabel);
            files[i].isUploaded = true;
        }

        correctFileNameWidth();
    }

    function insertFileLinkToEvent(file) {
        if (!file.fileUrl) {
            var fileUrl = location.origin + ASC.Files.Utility.GetFileDownloadUrl(file.id);
            if (ASC.Files.Utility.CanWebView(file.title)) {
                fileUrl = location.origin + ASC.Files.Utility.GetFileWebViewerUrl(file.id);
            }
            file.fileUrl = fileUrl;
        }
    }

    function copyFilesToMyDocumentsAndInsertFileLinksToEvent() {
        var aFiles = attachedFiles.slice(0);
        var cFiles = copiedFiles.slice(0);

        for (var i = 0; i < aFiles.length; i++) {
            if (!aFiles[i].orderNumber) {
                aFiles[i].orderNumber = getOrderNumber();
            }
        }

        hideWarningDialog();

        cFiles = cFiles.filter(function (item) {
            if (item.denyDownload) {
                showFileLinkAttachmentErrorStatus(item.orderNumber, attachmentResources.documentAccessDeniedError);
                return false;
            }
            return true;
        });

        copyFilesToMyDocuments(cFiles, function (err, files) {
            if (err) {
                for (i = 0; i < cFiles.length; i++) {
                    showFileLinkAttachmentErrorStatus(cFiles[i].orderNumber, attachmentResources.copyFileToMyDocumentsFolderErrorMsg);
                }
                correctFileNameWidth();
                clearAttachedFiles();
            } else {
                clearAttachedFiles();

                if (files.length == cFiles.length) {
                    for (i = 0; i < cFiles.length; i++) {
                        files[i].orderNumber = getOrderNumber();
                        files[i].originalId = cFiles[i].id;
                    }
                }

                var allFiles = aFiles.concat(files);
                addSharingFileLinkAttachments(allFiles);
                completeCopiedFileLinkAttachmentsProgressStatus(allFiles);
            }
        });
    }

    function cancelCopyingFilesToMyDocuments() {
        clearAttachedFiles();
        hideWarningDialog();
    }

    function copyFilesToMyDocuments(files, cb) {
        var fileIds = [];
        for (var i = 0; i < files.length; i++) {
            fileIds.push(files[i].id);
        }

        window.Teamlab.copyBatchItems(
            {
                destFolderId: ASC.Files.Constants.FOLDER_ID_MY_FILES,
                fileIds: fileIds,
                conflictResolveType: 2
            },
            {
                success: function (params, data) {
                    var operationId = data.length && data[0].id;
                    if (!operationId) {
                        cb({});
                    }

                    getCopyingFilesStatus(operationId, files, ASC.Files.Constants.FOLDER_ID_MY_FILES, cb);
                },
                error: function (params, err) {
                    cb(err);
                }
            });
    }

    function getCopyingFilesStatus(operationId, files, folderId, cb) {
        window.Teamlab.getOperationStatuses({
            success: function (par, statuses) {
                var status = getOperationStatus(statuses, operationId);
                if (status != null && !status.error && status.progress == 100) {
                    var copyingFiles = [];
                    for (var i = 0, len = status.files.length; i < len; i++) {
                        if (status.files[i].folderId == folderId)
                            copyingFiles.push(status.files[i]);
                    }
                    cb(null, copyingFiles);
                } else if (status != null && !status.error) {
                    updateCopiedFileLinkdAttachmentsProgressStatus(files, status);
                    setTimeout(function () {
                        getCopyingFilesStatus(operationId, files, folderId, cb);
                    }, 500);
                } else {
                    cb({});
                }
            },
            error: function (par, err) {
                cb(err);
            }
        });
    }

    function getOperationStatus(statuses, statusId) {
        for (var i = 0; i < statuses.length; i++) {
            if (statuses[i].id == statusId) {
                return statuses[i];
            }
        }
        return null;
    }

    function addSharingFileLinkAttachments(files) {
        for (var i = 0; i < files.length; i++) {
            var file = files[i];

            insertFileLinkToEvent(file);

            var attachment = convertDocumentToAttachment(file);
            attachment.size = 0;

            if (file.forceCopying) {
                updateFileLinkAttachmentProgressStatus(file.orderNumber, 0, attachmentResources.copyingToMyDocumentsLabel);
            } else {
                addAttachment(attachment, 1, true);
            }
        }
    }

    function updateCopiedFileLinkdAttachmentsProgressStatus(files, status) {
        for (var i = 0; i < files.length; i++) {
            setAttachmentProgress(files[i].orderNumber, status.progress);
        }
    }

    function completeCopiedFileLinkAttachmentsProgressStatus(files) {
        for (var i = 0; i < files.length; i++) {
            updateFileLinkAttachmentProgressStatus(files[i].orderNumber, 100, attachmentResources.insertedViaLink);
        }

        correctFileNameWidth();
    }

    function initWarningDialog() {
        warningDialog = $('#commonPopup');
        warningDialog.find('div.containerHeaderBlock:first td:first').html(attachmentResources.warningLabel);
        warningDialog.find('div.containerBodyBlock:first').html($('#filesCannotBeAttachedAsLinksTmpl').tmpl());

        jq("#copyFilesToMyDocuments").on("click", function () {
            copyFilesToMyDocumentsAndInsertFileLinksToEvent();
            return false;
        });

        jq("#cancelCopyingToMyDocuments").on("click", function () {
            cancelCopyingFilesToMyDocuments();
            return false;
        });
    }

    function showWarningDialog() {
        if (warningDialog == null) {
            initWarningDialog();
        }
        StudioBlockUIManager.blockUI(warningDialog, 530, { bindEvents: false });
    }

    function hideWarningDialog() {
        if (warningDialog.is(':visible')) {
            $.unblockUI();
        }
    }

    function clearAttachedFiles() {
        attachedFiles = [];
        copiedFiles = [];
    }

    var methods = {
        init: function (options) {
            var obj = $(this);
            removeInstance(obj);
            var instance = new fileUpload(obj, options);
            instances.push(instance);
            instance.init();
            return this;
        },
        get: function () {
            var obj = $(this);
            var instance = getInstance(obj);
            return instance ? instance.getItems() : null;
        },
        set: function (data, canEdit) {
            var obj = $(this);
            var instance = getInstance(obj);
            if (instance) {
                instance.addAttachments(data);
                instance.canEdit = canEdit == undefined ? true : canEdit;
                instance.renderContainer();
            }
        }
    };

    $.fn.FileUpload = function (method) {

        if (this.length && methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }

        return this;
    };


})(jQuery);