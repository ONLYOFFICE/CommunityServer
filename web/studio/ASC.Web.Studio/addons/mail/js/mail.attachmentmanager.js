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


window.AttachmentManager = (function($) {
    var attachments = [],
        dataBeforeSave = [],
        documentsInLoad = [],
        selectedFiles = [],
        attachedFiles = [],
        copiedFiles = [],
        failedUploadedFiles = [],

        nextId = 0,
        nextOrderNumber = 0,
        maxOneAttachmentSize = 15,
        maxOneAttachmentBytes = maxOneAttachmentSize * 1024 * 1024,
        maxAllAttachmentsSize = 25,
        maxAllAttachmentsBytes = maxAllAttachmentsSize * 1024 * 1024,

        saveHeandlerId = null,
        needAttachDocuments = false,
        uploadContainerId = 'newMessage',
        filesContainer = 'mail_attachments',
        isSaving = false,
        maxFileNameLen = 63,
        supportedCustomEvents = { UploadComplete: 'on_upload_completed' },
        eventsHandler = $({}),
        filenameColumnPaddingConst = 4,

        resizeTimer = null,

        dragDropEnabled = ('draggable' in document.createElement('span')),
        uploader = null,
        uploadQueue = new Array(),
        uploaderBusy = false,
        uploadStatus = {
            QUEUED: 0,
            STARTED: 1,
            STOPED: 2,
            DONE: 3,
            FAILED: 4
        },

        $attachmentsClearBtn;

    function init(loadedFiles) {
        nextOrderNumber = 0;

        $attachmentsClearBtn = $('#attachments_clear_btn');

        stopUploader();

        window.DocumentsPopup.unbind(window.DocumentsPopup.events.SelectFiles);

        uploader = createFileuploadInput('attachments_browse_btn');
        uploader.fileupload({
            url: null,
            autoUpload: false,
            singleFileUploads: true,
            sequentialUploads: true,
            progressInterval: 1000,
            dropZone: dragDropEnabled ? jq('#' + uploadContainerId) : null
        });

        uploader
            .bind('fileuploadadd', onUploadAdd)
            .bind('fileuploadsubmit', onUploadSubmit)
            .bind('fileuploadsend', onUploadSend)
            .bind('fileuploadprogress', onUploadProgress)
            .bind('fileuploaddone', onUploadDone)
            .bind('fileuploadfail', onUploadFail)
            .bind('fileuploadalways', onUploadAlways)
            .bind('fileuploadstart', onUploadStart)
            .bind('fileuploadstop', onUploadStop);

        if (dragDropEnabled) {
            $('#' + uploadContainerId)
                .bind('dragenter', function () {
                    return false;
                })
                .bind('dragleave', function () {
                    return hideDragHighlight();
                })
                .bind('dragover', function () {
                    showDragHighlight();
                    if ($.browser.safari) {
                        return true;
                    }
                    return false;
                })
                .bind('drop', function () {
                    hideDragHighlight();
                    return false;
                });
        }

        window.DocumentsPopup.bind(window.DocumentsPopup.events.SelectFiles, selectDocuments);
        saveHeandlerId = window.Teamlab.bind(window.Teamlab.events.saveMailMessage, onSaveMessage);

        $('#attachments_limit_txt').text(window.MailScriptResource.AttachmentsLimitLabel
            .replace('%1', maxOneAttachmentSize)
            .replace('%2', maxAllAttachmentsSize));

        addAttachments(loadedFiles);

        window.messagePage.initImageZoom();

        $(window).resize(function() {
            if (window.TMMail.pageIs('writemessage')) {
                clearTimeout(resizeTimer);
                resizeTimer = setTimeout(function() {
                    correctFileNameWidth();
                }, 100);
            }
        });
    }

    function onUploadAdd(e, data) {
        var file = data.files[0];
        file.orderNumber = getOrderNumber();
        file.percent = 0;
        file.status = uploadStatus.QUEUED;

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
        file.needSaveToTemp = TMMail.isTemplate();
        jq(this).fileupload('option', 'formData', [{ name: 'name', value: file.name }]);
        jq(this).fileupload('option', 'url', generateSubmitUrl({
            messageId: mailBox.currentMessageId,
            copyToMy: file.attachAsLinkOffer ? 1 : 0,
            needSaveToTemp: TMMail.isTemplate()
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

        var response = jq.parseJSON(data.result);

        if (response) {
            if (!response.Success) {
                if (!response.Data) {
                    response.Data = {
                        fileId: -1,
                        fileName: file.name,
                        orderNumber: file.orderNumber,
                        attachedAsLink: file.attachAsLinkOffer
                    };
                }

                response.Data.error = response.Message;
                response.Data.size = 0;
            }

            if (response.Data.attachedAsLink) {
                response.Data.id = response.Data.fileId;
                response.Data.title = response.Data.fileName;
                response.Data.orderNumber = file.orderNumber;
                response.Data.fileUrl = response.FileURL;

                if (response.Data.error) {
                    window.messagePage.cancelSending();
                    showFileLinkAttachmentErrorStatus(file.orderNumber, response.Data.error);
                    correctFileNameWidth();
                    var pos = searchFileIndex(copiedFiles, file.orderNumber);
                    if (pos > -1) {
                        copiedFiles.splice(pos, 1);
                    }
                } else {
                    insertFileLinksToMessage([response.Data]);
                    isSaving = true;
                    if (TMMail.isTemplate()) {
                        isSaving = false;
                        var pos = searchFileIndex(copiedFiles, file.orderNumber);
                        if (pos > -1) {
                            copiedFiles.splice(pos, 1);
                        }
                        completeCopiedFileLinkAttachmentsProgressStatus([response.Data]);
                    } else {
                        window.messagePage.saveMessagePomise().done(function () {
                            isSaving = false;
                            var pos = searchFileIndex(copiedFiles, file.orderNumber);
                            if (pos > -1) {
                                copiedFiles.splice(pos, 1);
                            }
                            completeCopiedFileLinkAttachmentsProgressStatus([response.Data]);
                        });
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
            errorMsg = jq.parseJSON(data.jqXHR.responseText).Message;
        }

        if (file.orderNumber == undefined || file.orderNumber < 0) {
            file.orderNumber = getOrderNumber();
            var attachment = convertPUfileToAttachment(file);
            addAttachment(attachment);
        }
        
        if (file.attachAsLinkOffer) {
            failedUploadedFiles.push(data);
        }

        data.result = JSON.stringify({
            Success: false,
            FileName: file.name,
            FileURL: '',
            Data: {
                contentId: null,
                contentType: '',
                fileId: -1,
                fileName: file.name,
                fileNumber: -1,
                size: 0,
                storedName: '',
                streamId: '',
                attachAsLinkOffer: file.attachAsLinkOffer,
                orderNumber: file.orderNumber
            },
            Message: errorMsg
        });

        onUploadDone(e, data);
    }

    function onUploadAlways() {
        runFileUploading();
    }

    function onUploadStart() {
        window.messagePage.setDirtyMessage();
    }

    function onUploadStop() {
        uploaderBusy = false;
        completeUploading();
    }

    function createFileuploadInput(buttonId) {
        var inputObj = jq('#fileupload');

        if (inputObj.length) {
            return inputObj;
        }

        var buttonObj = jq('#' + buttonId);

        inputObj = jq('<input/>')
            .attr('id', 'fileupload')
            .attr('type', 'file')
            .attr('multiple', 'multiple')
            .css("width", "0")
            .css("height", "0")
            .hide();

        inputObj.appendTo(buttonObj.parent());

        buttonObj.on('click', function (e) {
            e.preventDefault();
            jq('#fileupload').click();
        });

        return inputObj;
    }

    function correctFile(e, data) {
        var file = data.files[0];

        if (file.size <= 0) {
            onUploadFail(e, data, window.MailScriptResource.AttachmentsEmptyFileNotSupportedError);
            return false;
        }

        if (maxOneAttachmentBytes < file.size) {
            file.attachAsLinkOffer = true;
            onUploadFail(e, data, window.MailAttachmentsResource.PL_FILE_SIZE_ERROR);
            return false;
        }

        var totalSize = getTotalAttachmentsSize();

        if (maxAllAttachmentsBytes < totalSize) {
            file.attachAsLinkOffer = true;
            onUploadFail(e, data, window.MailScriptResource.AttachmentsTotalLimitError);
            return false;
        }

        return true;
    }

    function runFileUploading() {
        if (mailBox.currentMessageId < 1) {
            if (!isSaving) {
                isSaving = true;
                window.messagePage.saveMessage();
            }
        } else {
            var uploadData = getUploadDataByStatus(uploadStatus.QUEUED);
            if (uploadData) {
                uploaderBusy = true;
                uploadData.submit();
            }
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
            fileId: -1,
            fileName: file.name,
            orderNumber: file.orderNumber,
            size: file.size,
            storedName: '',
            streamId: '',
            error: file.error || ''
        };

        completeAttachment(attachment);

        return attachment;
    }

    function convertDocumentToAttachment(document) {
        var name = document.title;
        var ext = getAttachmentExtension(name);
        var newExt = '';
        var isInternalDocument = false;
        switch (ext) {
            case '.doct':
                newExt = '.docx';
                break;
            case '.xlst':
                newExt = '.xlsx';
                break;
            case '.pptt':
                newExt = '.pptx';
                break;
            default:
                break;
        }

        if (newExt != '') {
            name = getAttachmentName(name) + newExt;
            isInternalDocument = true;
        }

        var attachment = {
            contentId: null,
            contentType: '',
            fileId: -1,
            fileName: name,
            docId: document.id,
            orderNumber: document.orderNumber,
            size: isInternalDocument ? 0 : +document.size,
            storedName: '',
            streamId: '',
            error: document.error || ''
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
                    sizeString = size + ' ' + window.MailScriptResource.Bytes;
                } else {
                    sizeString = (size / kb).toFixed(2) + ' ' + window.MailScriptResource.Kilobytes;
                }
            } else {
                sizeString = (size / mb).toFixed(2) + ' ' + window.MailScriptResource.Megabytes;
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

    function addAttachment(attachment, status, withoutQuota) {
        attachment.operation = status ? status : 0;
        attachment.attachedAsLink = false;

        var html = prepareFileRow(attachment);

        $('#' + filesContainer + ' tbody').append(html);

        attachments.push(attachment);
        if (!withoutQuota) {
            calculateAttachments();
        }

        window.messagePage.updateEditAttachmentsActionMenu();
        $attachmentsClearBtn.show();
    }

    function getFileNameMaxWidth() {
        var fileinfoList = $('#' + filesContainer + ' .file_info');
        if (fileinfoList.length == 0) {
            return undefined;
        }

        var maxTableWidth = Math.max.apply(null, fileinfoList.map(function() {
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
            attachment.fileId = updateInfo.fileId;
            attachment.fileName = updateInfo.fileName;
            attachment.size = updateInfo.size || 0;
            attachment.storedName = updateInfo.storedName;
            attachment.fileNumber = updateInfo.fileNumber;
            attachment.streamId = updateInfo.streamId;
            attachment.error = updateInfo.error || '';
            attachment.attachedAsLink = updateInfo.attachedAsLink;
            attachment.attachAsLinkOffer = updateInfo.attachAsLinkOffer;
            attachment.tempStoredUrl = updateInfo.tempStoredUrl;

            completeAttachment(attachment);
            calculateAttachments();

            var html = prepareFileRow(attachment);
            var maxTableWidth = getFileNameMaxWidth();
            if ($.isNumeric(maxTableWidth)) {
                $(html).find('.file_info').width(maxTableWidth);
            }

            $('#' + filesContainer + ' .row[data_id=' + orderNumber + ']').replaceWith(html);
            window.messagePage.initImageZoom();
            window.messagePage.updateEditAttachmentsActionMenu();
        } else {
            // Attachemnt has been removed, need remove it from storage
            deleteStoredAttachment(updateInfo.fileId);
        }
    }

    function prepareFileRow(attachment) {
        if (attachment == undefined) {
            return '';
        }

        var html = $.tmpl('attachmentTmpl', attachment, {
            cutFileName: cutFileName,
            fileSizeToStr: getSizeString,
            getFileNameWithoutExt: getAttachmentName,
            getFileExtension: getAttachmentExtension,
            isTempAttach: TMMail.isTemplate()
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

    function updateFileLinkAttachmentProgressStatus(orderNumber, percent, text) {
        var $attachment = $('#mail_attachments .row[data_id="' + orderNumber + '"]');
        $attachment.addClass('inactive');

        $attachment.find('.delete_attachment').hide();
        $attachment.find('.menu_column').empty();

        $attachment.find('.load_result .file-load-result').remove();
        $attachment.find('.load_result .attach-filelink-btn').remove();

        if (percent == 100) {
            $attachment.find('.load_result').html('<span class="file-load-result uploaded-text" title="' + text + '"> ' + text + '</span>');
            $attachment.find('.attachment-progress').hide();
        } else {
            $attachment.find('.attachment-progress .progress-label').text(text);
            $attachment.find('.attachment-progress .progress-slider').css('width', percent + '%');;
            $attachment.find('.attachment-progress').show();
        }
    }

    function hideFileLinkedAttachment(orderNumber) {
        var $attachment = $('#mail_attachments .row[data_id="' + orderNumber + '"]');
        $attachment
            .queue("fx", function () {
                var self = this;
                setTimeout(function() {
                    $(self).dequeue();
                }, 1000);
            })
            .animate({ "opacity": 0.1 }, 1500)
            .fadeOut(300, function() {
                $(this).remove();
            });
    }

    function showFileLinkAttachmentErrorStatus(orderNumber, text) {
        var $attachment = $('#mail_attachments .row[data_id="' + orderNumber + '"]');

        $attachment.find('.delete_attachment').show();
        $attachment.find('.load_result').empty();
        $attachment.find('.load_result').html('<span class="file-load-result red-text" title="' + text + '"> ' + text + '</span>');
    }

    function removeAllAttachments() {
        var tempCollection = attachments.slice(); // clone array
        var i, len = tempCollection.length;
        for (i = 0; i < len; i++) {
            var attachment = tempCollection[i];
            removeAttachment(attachment.orderNumber, false);
        }
        calculateAttachments();
        $attachmentsClearBtn.hide();
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

    function deleteStoredAttachment(fileId) {
        if (fileId != undefined && fileId > 0) {
            if (mailBox.currentMessageId > 1) {
                window.messagePage.deleteMessageAttachment(fileId);
            }
        }
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
            var attachment = attachments[pos];
            deleteStoredAttachment(attachment.fileId);
            attachments.splice(pos, 1);

            if (attachments.length == 0) {
                $attachmentsClearBtn.hide();
            }
        }

        pos = searchFileIndex(documentsInLoad, orderNumber);
        if (pos > -1) {
            var document = documentsInLoad[pos];
            deleteStoredAttachment(document.fileId);
            documentsInLoad.splice(pos, 1);
        }

        removeAttachemntRow(orderNumber);
        calculateAttachments();
        removeFromUploaderQueue(orderNumber);

        if (needColumnWidthCorrection == undefined || needColumnWidthCorrection) {
            correctFileNameWidth();
        }
    }

    function removeAttachemntRow(orderNumber) {
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
                return window.MailScriptResource.AttachmentsExecutableWarning;
            default:
                return '';
        }
    }

    function selectDocuments(e, res) {
        var documents = res.data;
        var asLinks = res.asLinks;

        selectedFiles = selectedFiles.concat(documents);

        if (asLinks) {
            attachFileLinks(documents);
            return;
        }

        if (mailBox.currentMessageId < 1) {
            if (!needAttachDocuments) {
                needAttachDocuments = true;
                dataBeforeSave = res;
                window.messagePage.saveMessage();
            } else {
                $.merge(dataBeforeSave, res);
            }

            return;
        }

        var totalNewSize = getTotalUploadedSize();

        var i, len = documents.length;
        for (i = 0; i < len; i++) {
            var document = documents[i];
            document.orderNumber = getOrderNumber();

            var attachment = convertDocumentToAttachment(document);

            addAttachment(attachment);

            if (attachment.size > maxOneAttachmentBytes) {
                onUploadFail(null, {
                    files: [{
                        name: attachment.fileName,
                        docId: attachment.docId,
                        orderNumber: attachment.orderNumber,
                        size: attachment.size,
                        attachAsLinkOffer: true
                    }]
                }, window.MailAttachmentsResource.PL_FILE_SIZE_ERROR);
            } else if (totalNewSize + attachment.size > maxAllAttachmentsBytes) {
                attachment.attachAsLinkOffer = true;
                onAttachDocumentError({ attachment: attachment }, [window.MailScriptResource.AttachmentsTotalLimitError]);
            } else {
                totalNewSize += attachment.size;
                documentsInLoad.push(attachment);
                displayAttachmentProgress(attachment.orderNumber, true);
                setAttachmentProgress(attachment.orderNumber, 100);

                var data = {
                    fileId: document.id,
                    version: document.version,
                    needSaveToTemp: TMMail.isTemplate()
                };

                serviceManager.attachDocuments(mailBox.currentMessageId, data, { attachment: attachment }, { success: onAttachDocument, error: onAttachDocumentError });
            }
        }
        correctFileNameWidth();
        onUploadStart();
        dataBeforeSave = [];
    }

    function freeLoadedDocument(documnent) {
        var pos = searchFileIndex(documentsInLoad, documnent.orderNumber);
        if (pos > -1) {
            displayAttachmentProgress(documnent.orderNumber, false);
            documentsInLoad.splice(pos, 1);
        }
    }

    function onAttachDocumentError(params, error) {
        var attachment = params.attachment;
        attachment.error = error[0];
        attachment.size = 0;

        updateAttachment(params.attachment.orderNumber, attachment);
        freeLoadedDocument(attachment);
    }

    function onAttachDocument(params, document) {
        if (document) {
            updateAttachment(params.attachment.orderNumber, document);
            freeLoadedDocument(params.attachment);
        } else {
            onAttachDocumentError(params, window.MailScriptResource.AttachmentsUnknownError);
        }

        completeUploading();
    }

    function completeUploading() {
        if (documentsInLoad.length == 0 && !uploaderBusy) {
            if (!TMMail.isTemplate()) {
                window.messagePage.resetDirtyMessage();
            }
            calculateAttachments();
            triggerUploadComplete();
        }
    }

    function onSaveMessage() {
        if (mailBox.currentMessageId > 0) {
            TMMail.disableButton($('#editMessagePage .btnSave'), false);
            isSaving = false;

            if (needAttachDocuments) {
                needAttachDocuments = false;
                selectDocuments({}, dataBeforeSave);
                dataBeforeSave = [];
            }

            if (saveHeandlerId) {
                window.Teamlab.unbind(saveHeandlerId);
            }

            runFileUploading();
        }
    }

    function isLoading() {
        return documentsInLoad.length > 0 || uploaderBusy || attachedFiles.length > 0 || copiedFiles.length > 0;
    }

    function calculateAttachments() {
        if (isSaving) {
            return;
        }

        var fullSizeInBytes = 0;

        var i, len = attachments.length;
        var attachmentsCount = 0;
        for (i = 0; i < len; i++) {
            var attachment = attachments[i];
            fullSizeInBytes += attachment.size;
            if (attachment.size > 0) {
                attachmentsCount++;
            }
        }

        $('#attachments_count_label').text(window.MailResource.Attachments + (attachmentsCount > 0 ? ' (' + attachmentsCount + '): ' : ':'));
        $('#full-size-label').text(fullSizeInBytes > 0 ? window.MailResource.FullSize + ': ' + getSizeString(fullSizeInBytes) : '');
    }

    function getLoadedAttachments() {
        var loadedAttachments = [];
        var i, len = attachments.length;
        for (i = 0; i < len; i++) {
            var file = attachments[i];
            if (file.fileId > 0 || TMMail.isTemplate()) {
                loadedAttachments.push(file);
            }
        }
        return loadedAttachments;
    }

    function getTotalUploadedSize() {
        var totalUploadedBytes = 0;
        var i, len = attachments.length;
        for (i = 0; i < len; i++) {
            var attachment = attachments[i];
            if (attachment.fileId != undefined && attachment.fileId > 0) {
                totalUploadedBytes += attachment.size;
            }
        }
        return totalUploadedBytes;
    }

    function getTotalAttachmentsSize() {
        var totalBytes = 0;
        var i, len = attachments.length;
        for (i = 0; i < len; i++) {
            var attachment = attachments[i];
            totalBytes += attachment.size;
        }
        return totalBytes;
    }

    function hideDragHighlight() {
        $('#' + uploadContainerId)
            .removeClass('attachment_drag_highlight');
    }

    function showDragHighlight() {
        $('#' + uploadContainerId)
            .addClass('attachment_drag_highlight');
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
            window.Teamlab.unbind(saveHeandlerId);
        }
    }

    function generateSubmitUrl(data) {
        var submitUrl = 'UploadProgress.ashx?submit=ASC.Web.Mail.HttpHandlers.FilesUploader,ASC.Web.Mail';
        for (var prop in data) {
            submitUrl += '&{0}={1}'.format(prop, data[prop]);
        }
        return submitUrl;
    }

    function reloadAttachments(files) {
        var tempCollection = attachments.slice(); // clone array
        var i, len = tempCollection.length;
        for (i = 0; i < len; i++) {
            var attachment = tempCollection[i];
            if (attachment.fileId != -1) {
                removeAttachment(attachment.orderNumber);
            }
        }
        calculateAttachments();
        addAttachments(files);
        window.messagePage.initImageZoom();
    }

    function cutFileName(name) {
        if (name.length <= maxFileNameLen) {
            return name;
        }
        return name.substr(0, maxFileNameLen - 3) + '...';
    }

    function getAttachment(orderNumber) {
        var pos = searchFileIndex(attachments, orderNumber);
        if (pos > -1) {
            return attachments[pos];
        }
        return null;
    }

    function completeAttachment(attachment) {
        var name = attachment.fileName,
            fileId = attachment.fileId || (attachment.tempStoredUrl ? getNextId() : 0),
            ext = getAttachmentExtension(name),
            warn = getAttachmentWarningByExt(ext);

        attachment.iconCls = window.ASC.Files.Utility.getCssClassByFileTitle(name, true);

        if (fileId > 0 || attachment.tempStoredUrl) {
            attachment.isImage = window.ASC.Files.Utility.CanImageView(name);
            attachment.isMedia = window.ASC.Files.MediaPlayer.canPlay(name);
            attachment.isCalendar = TMMail.canViewAsCalendar(name);
            attachment.canView = attachment.tempStoredUrl ? false : window.TMMail.canViewInDocuments(name);
            attachment.canEdit = attachment.tempStoredUrl ? false : window.TMMail.canEditInDocuments(name);
        } else {
            attachment.isImage = false;
            attachment.isMedia = false;
            attachment.canView = false;
            attachment.canEdit = false;
            attachment.isCalendar = false;
        }

        attachment.warn = warn;
        attachment.operation = 0;

        if (fileId <= 0 && !attachment.tempStoredUrl) {
            attachment.handlerUrl = '';
        } else if (attachment.tempStoredUrl) {
            attachment.handlerUrl = attachment.isCalendar
                ? "javascript:mailCalendar.showCalendarInfo(\"{0}\", \"{1}\");".format(attachment.tempStoredUrl, name)
                : attachment.tempStoredUrl;
        } else {
            attachment.handlerUrl =
                attachment.isCalendar
                    ? "javascript:mailCalendar.showCalendarInfo(\"{0}\", \"{1}\");".format(window.TMMail.getAttachmentDownloadUrl(fileId), name)
                    : attachment.canView
                        ? window.TMMail.getViewDocumentUrl(fileId)
                        : window.TMMail.getAttachmentDownloadUrl(fileId);
        }

        return attachment;
    }

    function getNextId() {
        return --nextId;
    }

    function getOrderNumber() {
        return nextOrderNumber++;
    }

    function triggerUploadComplete() {
        if (!isLoading()) {
            eventsHandler.trigger(supportedCustomEvents.UploadComplete, {});
        }
    }

    //#region attached file links

    function attachFileLink(fileId, fileOrderNumber) {
        var attachments = getLoadedAttachments();

        if (!fileId && !fileOrderNumber) {
            return;
        }
        if (!fileId && findFailedUploadedFileIndex(fileOrderNumber) > -1) {
            uploadFileToMyDocumentsAndInsertFileLinkToMessage(fileOrderNumber);
            return;
        }
        if (!fileId && attachments[fileOrderNumber]) {
            Teamlab.exportAttachmentToMyDocuments(null, attachments[fileOrderNumber].fileId, {
                success: function (r, data) {
                    attachFileLinks([{
                        id: data,
                        forceCopying: true,
                        orderNumber: fileOrderNumber,
                        shareable: true,
                        title: attachments[fileOrderNumber].fileName,
                        webUrl: "/Products/Files/HttpHandlers/filehandler.ashx?action=redirect&fileid=" + data
                    }]);
                    return;
                }
            });
        }

        for (var i = 0; i < selectedFiles.length; i++) {
            if (selectedFiles[i].id == fileId) {
                selectedFiles[i].forceCopying = true;
                selectedFiles[i].orderNumber = fileOrderNumber;

                attachFileLinks([selectedFiles[i]]);
                return;
            }
        }
    }

    function attachFileLinks(files) {
        for (var i = 0; i < files.length; i++) {
            var file = files[i];

            if (file.shareable) {
                attachedFiles.push(file);
            } else {
                copiedFiles.push(file);
            }
        }

        if (copiedFiles.length) {
            window.popup.hide();
            setTimeout(function() {
                window.popup.addBig(MailScriptResource.Warning, $.tmpl('filesCannotBeAttachedAsLinksTmpl'));
            }, 0);
        } else {
            completeCopiedFileLinkAttachmentsProgressStatus(files);
            insertFileLinksToMessage(files);
            clearAttachedFiles();
        }
    }

    function copyFilesToMyDocumentsAndInsertFileLinksToMessage() {
        var aFiles = attachedFiles.slice(0);
        var cFiles = copiedFiles.slice(0);

        for (var i = 0; i < cFiles.length; i++) {
            if (!cFiles[i].orderNumber) {
                cFiles[i].orderNumber = getOrderNumber();
            }
        }

        window.messagePage.setDirtyMessage();
        addCopiedFileLinkAttachments(cFiles);
        window.popup.hide();

        copyFilesToMyDocuments(cFiles, function (err, files) {
            if (err) {
                for (i = 0; i < cFiles.length; i++) {
                    showFileLinkAttachmentErrorStatus(cFiles[i].orderNumber, MailScriptResource.CopyFileToMyDocumentsFolderErrorMsg);
                }
                correctFileNameWidth();
                clearAttachedFiles();
                window.messagePage.cancelSending();
            } else {
                clearAttachedFiles();
                insertFileLinksToMessage(aFiles.concat(files));

                isSaving = true;
                if (TMMail.isTemplate()) {
                    isSaving = false;
                    completeCopiedFileLinkAttachmentsProgressStatus(cFiles);
                } else {
                    window.messagePage.saveMessagePomise().done(function () {
                        isSaving = false;
                        completeCopiedFileLinkAttachmentsProgressStatus(cFiles);
                    });
                }
            }
        });
    }

    function cancelCopyingFilesToMyDocuments() {
        clearAttachedFiles();
        window.popup.hide();
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
                success: function(params, data) {
                    var operationId = data.length && data[0].id;
                    if (!operationId) {
                        cb({});
                    }

                    getCopyingFilesStatus(operationId, files, ASC.Files.Constants.FOLDER_ID_MY_FILES, cb);
                },
                error: function(params, err) {
                    cb(err);
                }
            });
    }

    function getCopyingFilesStatus(operationId, files, folderId, cb) {
        window.Teamlab.getOperationStatuses({
            success: function(par, statuses) {
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
                    setTimeout(function() {
                        getCopyingFilesStatus(operationId, files, folderId, cb);
                    }, 250);
                } else {
                    cb({});
                }
            },
            error: function(par, err) {
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

    function addCopiedFileLinkAttachments(files) {
        for (var i = 0; i < files.length; i++) {
            var file = files[i];
            var attachment = convertDocumentToAttachment(file);
            attachment.size = 0;

            if (file.forceCopying) {
                updateFileLinkAttachmentProgressStatus(file.orderNumber, 0, MailScriptResource.CopyingToMyDocumentsLabel);
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
            updateFileLinkAttachmentProgressStatus(files[i].orderNumber, 100, MailScriptResource.InsertedViaLink);
            hideFileLinkedAttachment(files[i].orderNumber);
        }

        correctFileNameWidth();
        if (!TMMail.isTemplate()) {
            window.messagePage.resetDirtyMessage();
        }
        triggerUploadComplete();
    }

    function findFailedUploadedFileIndex(fileOrderNumber) {
        for (var i = 0; i < failedUploadedFiles.length; i++) {
            var file = failedUploadedFiles[i].files[0];
            if (file.orderNumber == fileOrderNumber) {
                return i;
            }
        }

        return -1;
    }

    function uploadFileToMyDocumentsAndInsertFileLinkToMessage(fileOrderNumber) {
        var i = findFailedUploadedFileIndex(fileOrderNumber);
        if (i < 0)
            return;

        var file = failedUploadedFiles[i].files[0];
        copiedFiles.push(file);
        updateFileLinkAttachmentProgressStatus(fileOrderNumber, 0, MailScriptResource.CopyingToMyDocumentsLabel);
        failedUploadedFiles[i].submit();
    }

    function insertFileLinksToMessage(files) {
        for (var i = 0; i < files.length; i++) {
            files[i].fileName = AttachmentManager.GetFileName(files[i].title);
            files[i].ext = ASC.Files.Utility.GetFileExtension(files[i].title);

            if (!files[i].fileUrl) {
                var fileUrl = ASC.Files.Utility.GetFileDownloadUrl(files[i].id);
                if (ASC.Files.Utility.CanWebView(files[i].title)) {
                    fileUrl = ASC.Files.Utility.GetFileWebViewerUrl(files[i].id);
                }

                files[i].fileUrl = fileUrl;
            }
        }

        window.messagePage.setDirtyMessage();

        wysiwygEditor.insertFileLinks(files);
    }

    function clearAttachedFiles() {
        attachedFiles = [];
        copiedFiles = [];
    }

    //#endregion

    function bind(eventName, fn) {
        eventsHandler.bind(eventName, fn);
    }

    function unbind(eventName) {
        eventsHandler.unbind(eventName);
    }

    return {
        GetAttachments: getLoadedAttachments,
        MaxAttachmentSize: maxOneAttachmentSize,
        MaxTotalSize: maxAllAttachmentsSize,
        InitUploader: init,
        StopUploader: stopUploader,
        UpdateAttachment: updateAttachment,
        RemoveAttachment: removeAttachment,
        RemoveAttachemntRow: removeAttachemntRow,
        RemoveAll: removeAllAttachments,
        IsLoading: isLoading,
        GetFileName: getAttachmentName,
        GetFileExtension: getAttachmentExtension,
        GetSizeString: getSizeString,
        ReloadAttachments: reloadAttachments,
        CutFileName: cutFileName,
        GetAttachment: getAttachment,
        CompleteAttachment: completeAttachment,
        CustomEvents: supportedCustomEvents,
        AttachFileLink: attachFileLink,
        SelectDocuments: selectDocuments,

        MaxTotalSizeInBytes: maxAllAttachmentsBytes,
        OnAttachDocumentError: onAttachDocumentError,

        CopyFilesToMyDocumentsAndInsertFileLinksToMessage: copyFilesToMyDocumentsAndInsertFileLinksToMessage,
        CancelCopyingFilesToMyDocuments: cancelCopyingFilesToMyDocuments,

        Bind: bind,
        Unbind: unbind
    };
})(jQuery);