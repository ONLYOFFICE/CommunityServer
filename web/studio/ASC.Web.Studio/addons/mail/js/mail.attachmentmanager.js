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

window.AttachmentManager = (function($) {
    var attachments = [];
    var documents_before_save = [];
    var documents_in_load = [];
    var config = null;
    var next_order_number = 0;
    var max_one_attachment_size = 15;
    var max_one_attachment_bytes = max_one_attachment_size * 1024 * 1024;
    var max_all_attachments_size = 25;
    var max_all_attachments_bytes = max_all_attachments_size * 1024 * 1024;
    var uploader = null;
    var is_switcher_exists = false;
    var save_heandler_id = null;
    var add_docs_heandler_id = null;
    var need_attach_documents = false;
    var upload_container_id = 'newMessage';
    var files_container = 'mail_attachments';
    var init_stream_id = '';
    var is_saving = false;
    var max_file_name_len = 63;
    var supported_custom_events = { UploadComplete: "on_upload_completed" };
    var events_handler = $({});
    var filename_column_padding_const = 4;

    function init(streamId, loadedFiles, initConfig) {
        init_stream_id = streamId;
        next_order_number = 0;
        stopUploader();
        window.DocumentsPopup.unbind(window.DocumentsPopup.events.SelectFiles);
        var data = { stream: streamId, messageId: mailBox.currentMessageId };

        config = initConfig || {
            runtimes: window.ASC.Resources.Master.UploadDefaultRuntimes,
            browse_button: 'attachments_browse_btn',
            container: upload_container_id,
            url: generateSubmitUrl(data),
            flash_swf_url: window.ASC.Resources.Master.UploadFlashUrl,
            drop_element: upload_container_id,
            filters: {
                max_file_size: max_one_attachment_bytes
            },
            init: {
                PostInit: function() {
                    if (uploader.runtime == "flash" ||
                        uploader.runtime == "html4" ||
                        uploader.runtime == "") {
                        is_switcher_exists = true;
                        renderSwitcher();
                    }
                },
                FilesAdded: onFilesAdd,
                BeforeUpload: onBeforeUpload,
                UploadFile: onUploadFile,
                UploadProgress: onUploadProgress,
                FileUploaded: onFileUploaded,
                Error: onError,
                UploadComplete: onUploadComplete,
                StateChanged: onStateChanged
            }
        };

        uploader = new window.plupload.Uploader(config);

        $('#' + upload_container_id)
            .bind("dragenter", function() { return false; })//simply process idle event
            .bind('dragleave', function() {
                return hideDragHighlight();
            })//extinguish lights
            .bind("dragover", function() {
                if (uploader.features.dragdrop) {
                    showDragHighlight();
                }
                if ($.browser.safari) {
                    return true;
                }
                return false;
            })//illuminated area for the cast
            .bind("drop", function() {
                hideDragHighlight();
                return false;
            }); //handler is throwing on the field

        window.DocumentsPopup.bind(window.DocumentsPopup.events.SelectFiles, selectDocuments);
        save_heandler_id = serviceManager.bind(window.Teamlab.events.saveMailMessage, onSaveMessage);
        add_docs_heandler_id = serviceManager.bind(window.Teamlab.events.addMailDocument, onAttachDocument);
        $('#attachments_limit_txt').text(window.MailScriptResource.AttachmentsLimitLabel
            .replace('%1', max_one_attachment_size)
            .replace('%2', max_all_attachments_size));

        addAttachments(loadedFiles);

        uploader.init();

        window.messagePage.initImageZoom();
        
        var resize_timer;
        $(window).resize(function () {
            if (window.TMMail.pageIs('writemessage')) {
                clearTimeout(resize_timer);
                resize_timer = setTimeout(function() {
                    correctFileNameWidth();
                }, 100);
            }
        });
    }

    function renderSwitcher() {
        if (uploader.runtime == "flash" || uploader.runtime == "")
            $("#switcher a").text(window.MailScriptResource.ToBasicUploader);
        else
            $("#switcher a").text(window.MailScriptResource.ToFlashUploader);

        if (is_switcher_exists)
            $("#switcher").show();
    }

    function switchMode() {
        var is_flash = uploader.runtime == "flash" || uploader.runtime == "";
        var runtimes = is_flash ? "html4" : "flash,html4";
        $('#' + files_container + ' tbody').empty();
        uploader.setOption('runtimes', runtimes);
        if (attachments.length > 0) {
            var loaded_files = attachments.slice();
            if (uploader != undefined) {
                uploader.stop();
                uploader.splice();
            }
            attachments = [];
            addAttachments(loaded_files);
        }
        uploader.refresh();
    }

    function onBeforeUpload(up, file) {
        //console.log('onBeforeUpload', up, file);
        var res = getTotalUploadedSize() + (file.size || 0) <= max_all_attachments_bytes;
        if (!res) {
            window.setImmediate(function () {
                up.trigger('Error', { file: file, message: window.MailScriptResource.AttachmentsTotalLimitError });
            });
        };
        return res;
    }

    function onFilesAdd(up, files) {
        // Fires while when the user selects files to upload.
        //console.log('onFilesAdd', up, files);
 
        var file, i, len = files.length;
        for (i = 0; i < len; i++) {
            file = files[i];
            file.orderNumber = getOrderNumber();
            var attachment = convertPUfileToAttachment(file);
            addAttachment(attachment);
            var pos = searchFileIndex(up.files, file.orderNumber);
            if (pos < 0) {
                up.addFile(file);
            }
        }

        correctFileNameWidth();

        if (mailBox.currentMessageId < 1) {
            if (!is_saving) {
                is_saving = true;
                window.messagePage.saveMessage();
            }
            return;
        }

        if (tasksExist() && up.state != window.plupload.STARTED)
            up.start();
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
        var new_ext = '';
        var is_internal_document = false;
        switch (ext) {
            case '.doct':
                new_ext = '.docx';
                break;
            case '.xlst':
                new_ext = '.xlsx';
                break;
            case '.pptt':
                new_ext = '.pptx';
            default:
                break;
        }

        if (new_ext != '') {
            name = getAttachmentName(name) + new_ext;
            is_internal_document = true;
        }

        var attachment = {
            contentId: null,
            contentType: '',
            fileId: -1,
            fileName: name,
            orderNumber: document.orderNumber,
            size: is_internal_document ? 0 : document.size,
            storedName: '',
            streamId: '',
            error: document.error || ''
        };

        completeAttachment(attachment);

        return attachment;
    }

    function onUploadFile(up, file) {
        // Fires when a file is to be uploaded by the runtime.
        //console.log('onUploadFile', up, file);
        displayAttachmentProgress(file.orderNumber, true);
    }

    function onUploadProgress(up, file) {
        // Fires while a file is being uploaded.
        //console.log(('onUploadProgress: Progress ' + file.percent + '% name: ' + file.name + ' data_id: ' + file.fileId), up, file);
        setAttachmentProgress(file.orderNumber, file.percent);
        if (file.status == window.plupload.DONE && file.percent == 100) {
            displayAttachmentProgress(file.orderNumber, false);
        }
    }

    function onFileUploaded(up, file, resp) {
        // Fires when a file is successfully uploaded.
        //console.log('onFileUploaded', up, file, resp);
        displayAttachmentProgress(file.orderNumber, false);
        var response = JSON.parse(resp.response);
        if (response) {
            if (!response.Success) {
                response.Data.error = response.Message;
                response.Data.size = 0;
            }
            updateAttachment(file.orderNumber, response.Data);
        }
    }

    function onStateChanged(up) {
        // Fires when the overall state is being changed for the upload queue.
        switch (up.state) {
            case window.plupload.STARTED:
                onPreUploadStart();
                //console.log('StateChanged -> STARTED', up);
                break;
            case window.plupload.STOPPED:
                //console.log('StateChanged -> STOPPED', up);
                break;
        }
    }

    function getSizeString(size) {
        var size_string = '';
        if (size != undefined) {
            var mb = 1024 * 1024;
            var kb = 1024;
            if (size <= mb)
                if (size <= kb)
                size_string = size + ' ' + window.MailScriptResource.Bytes;
            else
                size_string = (size / kb).toFixed(2) + ' ' + window.MailScriptResource.Kilobytes;
            else
                size_string = (size / mb).toFixed(2) + ' ' + window.MailScriptResource.Megabytes;
        }
        return size_string;
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

    function addAttachment(attachment) {
        if (next_order_number < attachment.orderNumber)
            next_order_number = attachment.orderNumber;

        var html = prepareFileRow(attachment);

        $('#' + files_container + ' tbody').append(html);

        if (attachments == undefined)
            attachments = [];

        if (uploader.runtime == "html4" &&
            attachment.error == '' &&
            (attachment.fileId == undefined || attachment.fileId < 1)) {
            displayAttachmentProgress(attachment.orderNumber, true);
            setAttachmentProgress(attachment.orderNumber, 100);
        }

        attachments.push(attachment);
        calculateAttachments();
        window.messagePage.updateEditAttachmentsActionMenu();
    }

    function getFileNameMaxWidth() {
        var fileinfo_list = $('#' + files_container + ' .file_info');
        if (fileinfo_list.length == 0) return undefined;

        var max_table_width = Math.max.apply(null, fileinfo_list.map(function () {
            return $(this).find('.file-name').outerWidth(true) + $(this).find('.fullSizeLabel').outerWidth(true);
        }).get());
        return $.isNumeric(max_table_width) ? max_table_width + filename_column_padding_const : max_table_width;
    }

    function correctFileNameWidth() {
        //console.log('correctFileNameWidth');
        var fileinfo_list = $('#' + files_container + ' .file_info');
        if (fileinfo_list.length == 0) return;
        
        var max_table_width = getFileNameMaxWidth();
        if ($.isNumeric(max_table_width)) {
            fileinfo_list.animate({ "width": max_table_width }, "normal");
        }
    }

    function updateAttachment(orderNumber, updateInfo) {
        if (orderNumber == undefined || orderNumber < 0) return;

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

            completeAttachment(attachment);
            calculateAttachments();

            var html = prepareFileRow(attachment);
            var max_table_width = getFileNameMaxWidth();
            if ($.isNumeric(max_table_width)) {
                $(html).find('.file_info').width(max_table_width);
            }

            $('#' + files_container + ' .row[data_id=' + orderNumber + ']').replaceWith(html);
            window.messagePage.initImageZoom();
            window.messagePage.updateEditAttachmentsActionMenu();
        } else {
            // Attachemnt has been removed, need remove it from storage
            deleteStoredAttachment(updateInfo.fileId);
        }
    }

    function prepareFileRow(attachment) {
        if (attachment == undefined) return '';

        var html = $.tmpl("attachmentTmpl", attachment, {
            cutFileName: cutFileName,
            fileSizeToStr: getSizeString,
            getFileNameWithoutExt: getAttachmentName,
            getFileExtension: getAttachmentExtension
        });

        return html;
    }

    function displayAttachmentProgress(orderNumber, show) {
        var item_progress = $('#item_progress_' + orderNumber);
        if (item_progress != undefined) {
            if (show) {
                item_progress.show();
            } else {
                item_progress.hide();
            }
        }
    }

    function setAttachmentProgress(orderNumber, percent) {
        if (percent == undefined || percent < 0) return;

        var item_progress = $('#item_progress_' + orderNumber + ':visible .progress-slider');
        if (item_progress != undefined && item_progress.length == 1) {
            item_progress.css('width', percent + '%');
        }
    }

    function removeAllAttachments() {
        var temp_collection = attachments.slice(); // clone array
        var i, len = temp_collection.length;
        for (i = 0; i < len; i++) {
            var attachment = temp_collection[i];
            removeAttachment(attachment.orderNumber, false);
        }
        calculateAttachments();
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
            if (mailBox.currentMessageId > 1)
                window.messagePage.deleteMessageAttachment(fileId);
        }
    }

    function removeFromUploaderQueue(orderNumber) {
        var pos = searchFileIndex(uploader.files, orderNumber);
        if (pos > -1) {
            var need_start = false;
            if (uploader.files[pos].status == window.plupload.STARTED) {
                uploader.stop();
                need_start = true;
            }
            uploader.removeFile(uploader.files[pos]);
            if (need_start)
                uploader.start();
        }
    }

    function removeAttachment(orderNumber, needColumnWidthCorrection) {
        var pos = searchFileIndex(attachments, orderNumber);
        if (pos > -1) {
            var attachment = attachments[pos];
            deleteStoredAttachment(attachment.fileId);
            attachments.splice(pos, 1);
        }

        pos = searchFileIndex(documents_in_load, orderNumber);
        if (pos > -1) {
            var document = documents_in_load[pos];
            deleteStoredAttachment(document.fileId);
            documents_in_load.splice(pos, 1);
        }

        $('#' + files_container + ' .row[data_id=' + orderNumber + ']').remove();
        calculateAttachments();
        removeFromUploaderQueue(orderNumber);

        if (needColumnWidthCorrection == undefined || needColumnWidthCorrection)
            correctFileNameWidth();
    }

    function getAttachmentName(fullName) {
        if (fullName) {
	        var last_dot_index = fullName.lastIndexOf('.');
            return last_dot_index > -1 ? fullName.substr(0, last_dot_index) : fullName;
        }
        return '';
    }

    function getAttachmentExtension(fullName) {
        if (fullName) {
            var last_dot_index = fullName.lastIndexOf('.');
            return last_dot_index > -1 ? fullName.substr(last_dot_index) : '';
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

    function selectDocuments(e, documents) {
        if (mailBox.currentMessageId < 1) {
            if (!need_attach_documents) {
                need_attach_documents = true;
                documents_before_save = documents;
                window.messagePage.saveMessage();
            }
            else
                $.merge(documents_before_save, documents);

            return;
        }

        hideError();

        var total_new_size = getTotalUploadedSize();

        var i, len = documents.data.length;
        for (i = 0; i < len; i++) {
            var document = documents.data[i];
            document.orderNumber = getOrderNumber();
            var attachment = convertDocumentToAttachment(document);

            addAttachment(attachment);

            if (attachment.size > max_one_attachment_bytes) {
                onError(uploader, {
                    code: window.plupload.FILE_SIZE_ERROR,
                    message: window.plupload.translate('File size error.'),
                    file: { name: attachment.fileName, orderNumber: attachment.orderNumber, size: attachment.size }
                });
            } else if (total_new_size + attachment.size > max_all_attachments_bytes) {
                onAttachDocumentError({ attachment: attachment }, [window.MailScriptResource.AttachmentsTotalLimitError]);
            } else {
                total_new_size += attachment.size;
                documents_in_load.push(attachment);
                displayAttachmentProgress(attachment.orderNumber, true);
                setAttachmentProgress(attachment.orderNumber, 100);

                var data = {
                    fileId: document.id,
                    version: document.version,
                    shareLink: document.downloadUrl,
                    streamId: init_stream_id
                };

                serviceManager.attachDocuments(mailBox.currentMessageId, data, { attachment: attachment }, { error: onAttachDocumentError });
            }
        }
        correctFileNameWidth();
        onPreUploadStart();
        documents_before_save = [];
    }

    function freeLoadedDocument(documnent) {
        var pos = searchFileIndex(documents_in_load, documnent.orderNumber);
        if (pos > -1) {
            displayAttachmentProgress(documnent.orderNumber, false);
            documents_in_load.splice(pos, 1);
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
        }
        else
            onAttachDocumentError(params, window.MailScriptResource.AttachmentsUnknownError);

        completeUploading();
    }

    function onPreUploadStart() {
        hideError();
        window.messagePage.setDirtyMessage();
        if (is_switcher_exists)
            $("#switcher").hide();
    }

    function tasksExist() {
        var i, len = uploader.files.length;
        for (i = 0; i < len; i++) {
            var file = uploader.files[i];
            if (file.status == window.plupload.QUEUED) {
                return true;
            }
        }
        return false;
    }

    function completeUploading() {
        if (documents_in_load.length == 0 &&
            uploader.state != window.plupload.STARTED) {
            if (is_switcher_exists)
                $("#switcher").show();
            window.messagePage.resetDirtyMessage();
            calculateAttachments();
            triggerUploadComplete();
        }
    }

    function onUploadComplete() {
        if (tasksExist()) {
            uploader.start();
            return;
        }
        uploader.splice();
        completeUploading();
    }

    function onSaveMessage() {
        if (mailBox.currentMessageId > 0) {
            TMMail.disableButton($('#editMessagePage .btnSave'), false);
            var data = { stream: init_stream_id, messageId: mailBox.currentMessageId };
            uploader.setOption('url', generateSubmitUrl(data));
            is_saving = false;

            if (need_attach_documents) {
                need_attach_documents = false;
                selectDocuments({}, documents_before_save);
                documents_before_save = [];
            }

            if (tasksExist()) {
                uploader.start();
            }

            if (save_heandler_id != null || save_heandler_id != undefined) {
                serviceManager.unbind(save_heandler_id);
            }
        }
    }

    function isLoading() {
        return documents_in_load.length > 0 || uploader != undefined && uploader.state == window.plupload.STARTED;
    }

    function getPlUploaderError(code) {
        switch (code) {
            case window.plupload.GENERIC_ERROR:
                return window.MailAttachmentsResource.PL_GENERIC_ERROR;
            case window.plupload.HTTP_ERROR:
                return window.MailAttachmentsResource.PL_HTTP_ERROR;
            case window.plupload.IO_ERROR:
                return window.MailAttachmentsResource.PL_IO_ERROR;
            case window.plupload.FILE_EXTENSION_ERROR:
                return window.MailAttachmentsResource.PL_FILE_EXTENSION_ERROR;
            case window.plupload.SECURITY_ERROR:
                return window.MailAttachmentsResource.PL_SECURITY_ERROR;
            case window.plupload.INIT_ERROR:
                return window.MailAttachmentsResource.PL_INIT_ERROR;
            case window.plupload.FILE_SIZE_ERROR:
                return window.MailAttachmentsResource.PL_FILE_SIZE_ERROR;
            case window.plupload.IMAGE_FORMAT_ERROR:
                return window.MailAttachmentsResource.PL_IMAGE_FORMAT_ERROR;
            case window.plupload.IMAGE_MEMORY_ERROR:
                return window.MailAttachmentsResource.PL_IMAGE_MEMORY_ERROR;
            case window.plupload.IMAGE_DIMENSIONS_ERROR:
                return window.MailAttachmentsResource.PL_IMAGE_DIMENSIONS_ERROR;
            default:
                return undefined;
        }
    }

    function onError(up, resp) {
        if (resp.file != undefined) {
            var error_message = resp.code != undefined ?
                getPlUploaderError(resp.code) || resp.message :
                resp.message;

            if (resp.file.orderNumber == undefined || resp.file.orderNumber < 0) {
                resp.file.orderNumber = getOrderNumber();
                var attachment = convertPUfileToAttachment(resp.file);
                addAttachment(attachment);
            }

            var new_resp = {
                response: JSON.stringify({
                    Success: false,
                    FileName: resp.file.name,
                    FileURL: '',
                    Data: {
                        contentId: null,
                        contentType: "",
                        fileId: -1,
                        fileName: resp.file.name,
                        fileNumber: -1,
                        size: 0,
                        storedName: "",
                        streamId: ""
                    },
                    Message: error_message
                })
            };

            onFileUploaded(up, resp.file, new_resp);

        } else
            showError(resp.message);
    }

    function hideError() {
        var error_limit_cnt = $('#id_block_errors_container');
        error_limit_cnt.hide();
    }

    function showError(errorText) {
        var error_limit_cnt = $('#id_block_errors_container');
        error_limit_cnt.show();
        error_limit_cnt.find('span').html(errorText);
    }

    function calculateAttachments() {
        if (is_saving)
            return;

        var full_size_in_bytes = 0;

        var i, len = attachments.length;
        for (i = 0; i < len; i++) {
            var attachment = attachments[i];
            full_size_in_bytes += attachment.size;
        }

        $('#attachments_count_label').text(window.MailResource.Attachments + (len > 0 ? " (" + len + "): " : ":"));
        $('#full-size-label').text(full_size_in_bytes > 0 ? window.MailResource.FullSize + ": " + getSizeString(full_size_in_bytes) : '');
    }

    function getLoadedAttachments() {
        var loaded_attachments = [];
        var i, len = attachments.length;
        for (i = 0; i < len; i++) {
            var file = attachments[i];
            if (file.fileId > 0) {
                loaded_attachments.push(
                    {
                        fileId: file.fileId,
                        fileName: file.fileName,
                        size: file.size,
                        contentType: file.contentType,
                        fileNumber: file.fileNumber,
                        storedName: file.storedName,
                        streamId: file.streamId
                    });
            }
        }
        return loaded_attachments;
    }

    function getTotalUploadedSize() {
        var total_uploaded_bytes = 0;
        var i, len = attachments.length;
        for (i = 0; i < len; i++) {
            var attachment = attachments[i];
            if (attachment.fileId != undefined && attachment.fileId > 0)
                total_uploaded_bytes += attachment.size;
        }
        return total_uploaded_bytes;
    }

    function hideDragHighlight() {
        $('#' + upload_container_id)
            .removeClass('attachment_drag_highlight');
    }

    function showDragHighlight() {
        $('#' + upload_container_id)
            .addClass('attachment_drag_highlight');
    }

    function stopUploader() {
        $('#' + files_container + ' tbody').empty();
        attachments = [];
        if (add_docs_heandler_id != null || add_docs_heandler_id != undefined)
            serviceManager.unbind(add_docs_heandler_id);
        if (save_heandler_id != null || save_heandler_id != undefined)
            serviceManager.unbind(save_heandler_id);
        if (uploader != undefined) {
            uploader.unbindAll();
            uploader.stop();
            uploader.splice();
        }
    }

    function generateSubmitUrl(data) {
        var submit_url = window.location.protocol + "//" + window.location.hostname + '/UploadProgress.ashx?submit=ASC.Web.Mail.HttpHandlers.FilesUploader,ASC.Web.Mail';
        for (var prop in data) {
            submit_url += '&{0}={1}'.format(prop, data[prop]);
        }
        return submit_url;
    }

    function hideUpload() {
        $('#' + upload_container_id).hide();
    }

    function showUpload() {
        $('#' + upload_container_id).show();
    }

    function reloadAttachments(files) {
        var temp_collection = attachments.slice(); // clone array
        var i, len = temp_collection.length;
        for (i = 0; i < len; i++) {
            var attachment = temp_collection[i];
            if (attachment.fileId != -1)
                removeAttachment(attachment.orderNumber);
        }
        calculateAttachments();
        addAttachments(files);
        window.messagePage.initImageZoom();
    }

    function cutFileName(name){
        if(name.length <= max_file_name_len)
            return name;
        return name.substr(0, max_file_name_len-3) + '...';
    }

    function getAttachment(orderNumber) {
        var pos = searchFileIndex(attachments, orderNumber);
        if (pos > -1) {
            return attachments[pos];
        }
        return null;
    }
    
    function completeAttachment(attachment) {
        var name = attachment.fileName;
        var file_id = attachment.fileId || -1;
        var ext = getAttachmentExtension(name);
        var warn = getAttachmentWarningByExt(ext);

        attachment.isImage = file_id > 0 ? window.ASC.Files.Utility.CanImageView(name) : false;
        attachment.iconCls = window.ASC.Files.Utility.getCssClassByFileTitle(name, true);
        attachment.canView = file_id > 0 ? window.TMMail.canViewInDocuments(name) : false;
        attachment.canEdit = file_id > 0 ? window.TMMail.canEditInDocuments(name) : false;
        attachment.warn = warn;

        if (file_id <= 0)
            attachment.handlerUrl = '';
        else {
            attachment.handlerUrl = attachment.canView ?
                window.TMMail.getViewDocumentUrl(file_id) :
                window.TMMail.getAttachmentDownloadUrl(file_id);
        }

        return attachment;
    }

    function getOrderNumber() {
        return next_order_number + 1;
    }

    function triggerUploadComplete() {
        if (!isLoading())
            events_handler.trigger(supported_custom_events.UploadComplete, {});
    }
    
    function bind(eventName, fn) {
        events_handler.bind(eventName, fn);
    }

    function unbind(eventName) {
        events_handler.unbind(eventName);
    }

    return {
        GetAttachments: getLoadedAttachments,
        MaxAttachmentSize: max_one_attachment_size,
        MaxTotalSize: max_all_attachments_size,
        InitUploader: init,
        StopUploader: stopUploader,
        RemoveAttachment: removeAttachment,
        RemoveAll: removeAllAttachments,
        IsLoading: isLoading,
        SwitchMode: switchMode,
        ShowUpload: showUpload,
        HideUpload: hideUpload,
        GetFileName: getAttachmentName,
        GetFileExtension: getAttachmentExtension,
        GetSizeString: getSizeString,
        ReloadAttachments: reloadAttachments,
        CutFileName: cutFileName,
        GetAttachment: getAttachment,
        CompleteAttachment: completeAttachment,
        CustomEvents: supported_custom_events,
        Bind: bind,
        Unbind: unbind
    };

})(jQuery)