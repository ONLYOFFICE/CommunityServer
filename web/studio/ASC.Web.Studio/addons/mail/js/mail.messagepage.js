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


window.messagePage = (function ($) {
    var isInit = false,
        saveTimeout = null,
        messageIsDirty = false,
        messageIsSending = false,
        isMessageRead = false,
        sortConversationByAsc = true,
        maxConversationShow = 5,
        attachmentMenuItems,
        attachmentEditMenuItems,
        messageMenuItems,
        toEmailAddresses = [],
        savingLock = false,
        repeatSaveFlag = false,
        conversationMoved = false,
        dstFolderType = 0,
        dstUserFolderId = 0,
        conversationDeleted = false,
        hasLinked = false,
        crmContactsInfo = [],
        lastSendMessageId,
        progressBarIntervalId,
        tmplItems = [];

    function setHasLinked(val) {
        if (hasLinked !== val) {
            hasLinked = val;
            mailCache.setHasLinked(mailBox.currentMessageId, hasLinked);
            $('#itemContainer').find('.viewTitle').trigger("update");
        }
    }

    function init() {
        if (isInit === false) {
            isInit = true;
            sortConversationByAsc = (TMMail.option('ConversationSortAsc') === 'true');

            window.Teamlab.bind(window.Teamlab.events.saveMailMessage, onSaveDraft);
            window.Teamlab.bind(window.Teamlab.events.saveMailTemplate, onSaveTemplate);
            window.Teamlab.bind(window.Teamlab.events.markChainAsCrmLinked, onMarkChainAsCrmLinked);
            window.Teamlab.bind(window.Teamlab.events.createMailContact, onCreateMailContact);
        }

        attachmentMenuItems = [
            { selector: "#attachmentActionMenu .downloadAttachment", handler: downloadAttachment },
            { selector: "#attachmentActionMenu .viewAttachment", handler: viewAttachment },
            { selector: "#attachmentActionMenu .editAttachment", handler: editDocumentAttachment },
            { selector: "#attachmentActionMenu .saveAttachmentToDocs", handler: saveAttachmentToDocs },
            { selector: "#attachmentActionMenu .saveAttachmentToCalendar", handler: saveAttachmentToCalendar }
        ];

        attachmentEditMenuItems = [
            { selector: "#attachmentEditActionMenu .downloadAttachment", handler: downloadAttachment },
            { selector: "#attachmentEditActionMenu .viewAttachment", handler: viewAttachment },
            { selector: "#attachmentEditActionMenu .deleteAttachment", handler: deleteAttachment }
        ];

        messageMenuItems = [
            {
                selector: "#messageActionMenu .replyMail", handler: function (id) {
                    window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.buttonClick, "reply");
                    return checkhBlockedImagesBeforeCompose(id, function () {
                        return TMMail.moveToReply(id);
                    });
                }
            },
            {
                selector: "#messageActionMenu .replyAllMail", handler: function (id) {
                    window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.buttonClick, "replyAll");
                    return checkhBlockedImagesBeforeCompose(id, function () {
                        return TMMail.moveToReplyAll(id);
                    });
                }
            },
            {
                selector: "#messageActionMenu .forwardMail",
                handler: function (id) {
                    window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.buttonClick, "forward");
                    return checkhBlockedImagesBeforeCompose(id, function () {
                        return TMMail.moveToForward(id);
                    });
                }
            },
            { selector: "#messageActionMenu .singleViewMail", handler: TMMail.openMessage },
            { selector: "#messageActionMenu .deleteMail", handler: deleteMessage },
            { selector: "#messageActionMenu .printMail", handler: moveToMessagePrint },
            { selector: "#messageActionMenu .alwaysHideImages", handler: alwaysHideImages },
            { selector: "#messageActionMenu .createCrmPerson", handler: createNewCrmPerson },
            { selector: "#messageActionMenu .createCrmCompany", handler: createNewCrmCompany },
            { selector: "#messageActionMenu .createPersonalContact", handler: createPersonalContact }
        ];

        if (ASC.Mail.Constants.CRM_AVAILABLE) {
            messageMenuItems.push({ selector: "#messageActionMenu .exportMessageToCrm", handler: crmLinkPopup.showCrmExportMessagePopup });
        } else {
            $('.exportMessageToCrm.dropdown-item').hide();
        }

        wysiwygEditor.unbind(wysiwygEditor.events.OnChange).bind(wysiwygEditor.events.OnChange, onMessageChanged);
    }

    function hide() {
        closeMessagePanel();
    }

    function showConversationMessage(id, noBlock, checkSender) {
        var params = { action: 'view', conversation_message: true, message_id: id, checkSender: checkSender, loadImages: noBlock };
        //TODO: Load message from cache -> uncomment and fix
        /*var message = mailCache.getMessage(id);
        if (message && isMessageExpanded(message)) {
            console.log("%s found in cache", id);
            params.notRememberContent = true;
            onGetMailMessage(params, message);
        } else {
            console.log("%s not found in cache", id);*/
        serviceManager.getMessage(id, noBlock, params,
        {
            success: onGetMailMessage,
            error: onOpenConversationMessageError
        });
        //}
    }

    function checkCrmLinked(id) {
        if (ASC.Mail.Constants.CRM_AVAILABLE) {
            serviceManager.isConversationLinkedWithCrm(id, { messageId: id },
            {
                success: function (params, status) {
                    hasLinked = status.isLinked;
                },
                error: function (params, e) {
                    hasLinked = false;
                    console.error(e);
                },
                async: true
            });
        }
    }

    function view(id, noBlock) {
        var content = null;
        if(!commonSettingsPage.isConversationsEnabled())
            content = mailCache.get(id);

        if (!content) {
            checkCrmLinked(id);

            serviceManager.getMessage(id,
                noBlock,
                { action: 'view', loadImages: noBlock },
                {
                    success: onGetMailMessage,
                    error: onOpenMessageError
                },
                ASC.Resources.Master.Resource.LoadingProcessing);
        } else {
            console.log("%s found in cache", id);

            var rootMessage = content.messages[0];
            if (rootMessage && rootMessage.wasNew) {
                mailBox.setMessagesReadUnread([id], true, true);
            }

            var params = { action: 'view', id: id, loadImages: noBlock, conversation_message: false };
            hasLinked = content.hasLinked;
            params.notRememberContent = true;
            onGetMailMessage(params, rootMessage);
        }
    }

    function conversation(id, loadAllContent) {
        var params = { action: 'conversation', id: id, loadImages: loadAllContent };
        var content = mailCache.get(id);
        if (content) {
            var chainCountEl = $('#itemContainer .messages .row[data_id="' + id + '"] .chain-counter');
            if (chainCountEl.length > 0 && chainCountEl.attr("value") != content.messages.length) {
                mailCache.remove(id);
                content = null;
            }
        }

        if (!content) {
            console.log("%s not found in cache", id);
            checkCrmLinked(id);
            serviceManager.getConversation(id, loadAllContent, params,
                {
                    success: onGetMailConversation,
                    error: onOpenMessageError
                },
                ASC.Resources.Master.Resource.LoadingProcessing);
        } else {
            console.log("%s found in cache", id);

            var rootMessage = mailCache.getRootMessage(id);
            if (rootMessage && rootMessage.wasNew) {
                mailBox.setConversationReadUnread([id], true, true);
            }
            hasLinked = content.hasLinked;
            params.notRememberContent = true;
            onGetMailConversation(params, content.messages);
        }
    }

    function onOpenConversationMessageError(event) {
        var shortView = $('#itemContainer .itemWrapper .short-view[message_id=' + event.message_id + ']');
        shortView.removeClass('loading');
        shortView.find('.loader').hide();
        window.LoadingBanner.hideLoading();
        window.toastr.error(TMMail.getErrorMessage([window.MailScriptResource.ErrorOpenMessage]));
    }

    function onOpenMessageError() {
        TMMail.moveToInbox();
        window.LoadingBanner.hideLoading();
        window.toastr.error(TMMail.getErrorMessage([window.MailScriptResource.ErrorOpenMessage]));
    }

    function toFile(data) {
        var file = null;

        if (!data) return null;

        try {
            var type;
            if (ASC.Files.Utility.CanImageView(data.title)) {
                type = "image";
            } else {
                if (ASC.Files.Utility.CanWebEdit(data.title)) {
                    type = "editedFile";
                } else {
                    if (ASC.Files.Utility.CanWebView(data.title)) {
                        type = "viewedFile";
                    } else {
                        type = "noViewedFile";
                    }
                }
            }

            //from FileShareLink.GetLink
            var fileUrl = ASC.Files.Utility.GetFileDownloadUrl(data.id);
            if (ASC.Files.Utility.CanWebView(data.title)) {
                fileUrl = ASC.Files.Utility.GetFileWebViewerUrl(data.id);
            }

            file = {
                "title": data.title,
                "access": data.access,
                "type": type,
                "exttype": ASC.Files.Utility.getCssClassByFileTitle(data.title),
                "id": data.id,
                "version": data.version,
                "fileUrl": fileUrl,
                "size": data.pureContentLength,
                "shareable": data.access == ASC.Files.Constants.AceStatusEnum.None ||
                    data.access == ASC.Files.Constants.AceStatusEnum.ReadWrite
                // TODO: fix like in documentPopup.attachSelectedFiles when params "folderShareable" and "encrypted" will be accessible
                /*!!folderShareable
                    && (!file.encrypted
                        && (file.access == ASC.Files.Constants.AceStatusEnum.None
                            || file.access == ASC.Files.Constants.AceStatusEnum.ReadWrite))*/
            }
        } catch (e) {
            console.error(e);
        }

        return file;
    }

    function onCompose() {
        var fileIds = TMMail.extractFileIdsFromAnchor();

        if (fileIds) {
            var arrayOfGetDocFileFunc = fileIds.map(function(fileId) {
                var d = jq.Deferred();

                try {
                    Teamlab.getDocFile({},
                        fileId,
                        {
                            success: function(params, data) {
                                var file = toFile(data);
                                d.resolve(file);
                            },
                            error: function(e, err) {
                                d.resolve({
                                    id: fileId,
                                    title: "",
                                    error: err[0]
                                });
                            }
                        });
                } catch (e) {
                    d.reject(e);
                }

                return d.promise();
            });

            $.when.apply($, arrayOfGetDocFileFunc)
                .done(function () {
                    var files = [];

                    if (arguments) { 
                        var args = [].slice.call(arguments);

                        if (args.length > 0) {
                            files = args.filter(function (f) {
                                if (f && f.error) {
                                    toastr.error(f.error);
                                }

                                return f && !f.error;
                            });
                        }
                    }

                    openEmptyComposeForm(files.length > 0 ? { files: files } : {});
                })
                .fail(function(err) {
                    toastr.error(err[0]);
                    openEmptyComposeForm({});
                });

        } else {
            openEmptyComposeForm({});
        }
    }

    function onComposeTo(params) {
        var addresses;
        if (params) {
            addresses = params.join(', ');
        } else {
            addresses = toEmailAddresses.join(', ');
        }
        return openEmptyComposeForm(addresses ? { email: addresses } : {});
    }

    function onComposeFromCrm(params) {
        crmContactsInfo = params.contacts_info;
        onComposeTo(params.addresses);
    }

    function compose() {
        if (accountsManager.any()) {
            ASC.Controls.AnchorController.move('compose');
            return;
        }
        // no accounts added yet
        var body = $.tmpl('addFirstAccountTmpl');
        popup.addBig(window.MailScriptResource.NewAccount, body);
    }

    function composeTo() {
        if (accountsManager.any()) {
            ASC.Controls.AnchorController.move('composeto');
            return;
        }
        // no accounts added yet
        var body = $.tmpl('addFirstAccountTmpl');
        popup.addBig(window.MailScriptResource.NewAccount, body);
    }

    function edit(id) {
        mailBox.currentMessageId = id;
        serviceManager.getMessage(id, true, { action: (TMMail.isTemplate()) ? 'template' : 'draft', loadImages: true },
        {
            success: onGetMailMessage,
            error: onOpenMessageError
        }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function deleteMessage(id) {
        var chainFlag = false;
        if (TMMail.pageIs('conversation')) {
            chainFlag = true;
        }
        mailBox.deleteMessage(id, MailFilter.getFolder(), chainFlag);
    }

    function alwaysHideImages(id) {
        var senderAddress = ASC.Mail.Utility.ParseAddress(getFromAddress(id));

        if (senderAddress) {
            mailCache.remove([id]);
            hideImagesAction(null, { address: senderAddress.email });
        }
    }

    function isContentBlocked(id) {
        var fullView = $('#itemContainer .full-view[message_id="' + id + '"]');
        var contentBlocked = (fullView.length === 0 ? false : fullView.attr('content_blocked') == "true");
        return contentBlocked;
    }

    function reply(id) {
        var noBlock = !isContentBlocked(id);
        serviceManager.getMessage(id, noBlock, { action: 'reply', loadImages: noBlock },
        {
            success: onGetMailMessage,
            error: onOpenMessageError
        }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function replyAll(id) {
        var noBlock = !isContentBlocked(id);
        serviceManager.getMessage(id, noBlock, { action: 'replyAll', loadImages: noBlock },
        {
            success: onGetMailMessage,
            error: onOpenMessageError
        }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function forward(id) {
        var noBlock = !isContentBlocked(id);
        serviceManager.getMessage(id, noBlock, { action: 'forward', loadImages: noBlock },
        {
            success: onGetMailMessage,
            error: onOpenMessageError
        }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    // obtains message saving "lock" flag and remembers repeat attempts
    // returns true if lock obtained and false otherwise

    function obtainSavingLock() {
        if (true === savingLock) {
            repeatSaveFlag = true;
            return false;
        } else {
            savingLock = true;
            return true;
        }
    }

    // releases message saving lock and tries to repeat saving
    // if repeat attempt flag is set

    function releaseSavingLock() {
        savingLock = false;
        if (repeatSaveFlag) {
            repeatSaveFlag = false;
            saveMessage(false);
        }
    }

    function getEditingMessage() {
        return $('#editMessagePage').data('message');
    }

    function prepareMessageData(id) {
        var from = $('#newmessageFromSelected').attr('mailbox_email'),
            to = jq("#newmessageTo").AdvancedEmailSelector('get'),
            cc = jq("#newmessageCopy").AdvancedEmailSelector('get'),
            bcc = jq("#newmessageBCC").AdvancedEmailSelector('get'),
            subject = $('#newmessageSubject').val(),
            body = wysiwygEditor.getValue(),
            importance = $('#editMessagePage .importance')
                .find('[class^="icon-"], [class*=" icon-"]')
                .is('.icon-important'),
            requestReceipt = $('#editMessagePage .requestReceipt').hasClass("on");

        var labelsCollection = $.makeArray($(".tags .itemTags a").map(function () { return parseInt($(this).attr("tagid")); }));

        var original = getEditingMessage();

        var message = new ASC.Mail.Message();
        message.id = id;
        message.from = from;
        message.to = to;
        message.cc = cc;
        message.bcc = bcc;
        message.subject = subject;
        message.mimeReplyToId = original.mimeReplyToId;
        message.importance = importance;
        message.tags = labelsCollection;
        message.body = body;
        message.attachments = AttachmentManager.GetAttachments();
        message.fileLinksShareMode = $("#shareFileLinksAccessSelector").val() || ASC.Files.Constants.AceStatusEnum.Read;
        message.requestReceipt = requestReceipt;
        message.requestRead = requestReceipt;

        return message;
    }

    function resetDirtyMessage() {
        if (!AttachmentManager.IsLoading()) {
            messageIsDirty = false;
            $('#newMessageSaveMarker').text('');
        }
    }

    function saveMessage(showLoader) {
        if (!obtainSavingLock()) {
            return;
        }

        if (mailBox.currentMessageId < 1) {
            mailBox.currentMessageId = message_id = 0;
        } else {
            message_id = mailBox.currentMessageId;
        }

        resetDirtyMessage();

        var message = prepareMessageData(message_id);

        clearAttachmentsWithZeroSize(message);

        if (showLoader)
            LoadingBanner.displayMailLoading(window.MailScriptResource.SavingMessage);

        ASC.Mail.Utility.SaveMessageInDrafts(message)
            .then(function() {
                if (message_id === 0)
                    serviceManager.updateFolders();
            },
                onErrorSendSaveMessage)
            .always(function() {
                LoadingBanner.hideLoading();
            });

        if (message_id === 0) {
            mailBox.markFolderAsChanged(TMMail.sysfolders.drafts.id);
        }
    }

    function saveMessageTemplate(showLoader) {
        if (!obtainSavingLock()) {
            return;
        }

        if (mailBox.currentMessageId < 1) {
            mailBox.currentMessageId = message_id = 0;
        } else {
            message_id = mailBox.currentMessageId;
        }

        resetDirtyMessage();

        var message = prepareMessageData(message_id);

        clearAttachmentsWithZeroSize(message);

        if (showLoader)
            LoadingBanner.displayMailLoading(window.MailScriptResource.SavingMessage);

        ASC.Mail.Utility.SaveMessageInTemplates(message)
            .then(function () {
                if (message_id === 0)
                    serviceManager.updateFolders();
            },
                onErrorSendSaveMessage)
            .always(function () {
                LoadingBanner.hideLoading();
            });

        if (message_id === 0) {
            mailBox.markFolderAsChanged(TMMail.sysfolders.templates.id);
        }
    }

    function saveMessagePomise(needCopy) {
        var d = jq.Deferred();

        if (mailBox.currentMessageId < 1) {
            mailBox.currentMessageId = message_id = 0;
        } else {
            message_id = mailBox.currentMessageId;
        }

        if (needCopy)
            message_id = 0;

        var message = prepareMessageData(message_id);

        clearAttachmentsWithZeroSize(message);

        ASC.Mail.Utility.SaveMessageInDrafts(message)
           .fail(onErrorSendSaveMessage)
           .always(function () {
               d.resolve();
           });

        return d.promise();
    }

    function saveMessageTemplatePomise(needCopy) {
        var d = jq.Deferred();

        if (mailBox.currentMessageId < 1) {
            mailBox.currentMessageId = message_id = 0;
        } else {
            message_id = mailBox.currentMessageId;
        }

        if (needCopy)
            message_id = 0;

        var message = prepareMessageData(message_id);

        clearAttachmentsWithZeroSize(message);

        ASC.Mail.Utility.SaveMessageInTemplates(message)
           .fail(onErrorSendSaveMessage)
           .always(function () {
               d.resolve();
           });

        return d.promise();
    }

    function onAttachmentsUploadComplete() {
        sendMessage();
        messageIsSending = false;
    }

    function clearAttachmentsWithZeroSize(message) {
        if (message.attachments.length) {
            message.attachments = $.grep(message.attachments, function (item) {
                return item.size > 0;
            });
        }
    }

    function getMessageErrors(message) {
        var errors = [];

        function hasInvalidEmails(emails) {
            return jq.grep(emails, function(v) {
                return !v.isValid;
            }).length > 0;
        }

        function collectErrors(emails) {
            jq.each(emails, function(i, v) {
                if (!v.isValid)
                    errors.push(window.MailScriptResource.ErrorIncorrectAddress + " \"" + TMMail.htmlEncode(v.email) + "\"");
            });
        }

        $("#newmessageTo").removeClass("invalidField");
        if (message.to.length === 0) {
            $("#newmessageTo").addClass("invalidField");
            errors.push(window.MailScriptResource.ErrorEmptyToField);
        }
        else if (hasInvalidEmails(message.to)) {
            $("#newmessageTo").addClass("invalidField");
            collectErrors(message.to);
        }   

        $("#newmessageCopy").removeClass("invalidField");
        if (message.cc.length > 0 && hasInvalidEmails(message.cc)) {
            $("#newmessageCopy").addClass("invalidField");
            collectErrors(message.cc);
        }

        $("#newmessageBCC").removeClass("invalidField");
        if (message.bcc.length > 0 && hasInvalidEmails(message.bcc)) {
            $("#newmessageBCC").addClass("invalidField");
            collectErrors(message.bcc);
        }

        return errors;
    }

    function needCrmLink() {
        return crmContactsInfo.length > 0;
    }

    function sendMessage(messageId, forcibly) {
        if (messageId == undefined) {
            if (mailBox.currentMessageId < 1) {
                mailBox.currentMessageId = messageId = 0;
            } else {
                messageId = mailBox.currentMessageId;
            }
        }

        if (AttachmentManager.IsLoading()) {
            messageIsSending = true;
            window.AttachmentManager.Bind(window.AttachmentManager.CustomEvents.UploadComplete, onAttachmentsUploadComplete);
            window.LoadingBanner.displayMailLoading(window.MailScriptResource.SendingMessage + ": " + window.MailScriptResource.LoadingAttachments);
            return;
        }

        if (needCrmLink()) {
            messageIsSending = true;
            saveMessage(true);
            return;
        }

        messageIsSending = false;

        var message = prepareMessageData(messageId);

        var errors = getMessageErrors(message);

        if (errors.length > 0) {
            window.LoadingBanner.hideLoading();
            var i, len = errors.length;
            for (i = 0; i < len; i++) {
                window.toastr.error(errors[i]);
            }

            TMMail.disableButton($('#editMessagePageHeader .btnSend'), false);
            TMMail.disableButton($('#editMessagePageHeader .btnSave'), false);
            TMMail.disableButton($('#editMessagePageHeader .btnSaveTemplate'), false);
            if (messageId > 0) {
                TMMail.disableButton($('#editMessagePageHeader .btnDelete'), false);
                TMMail.disableButton($('#editMessagePageHeader .btnAddTag'), false);
            }
            return;
        }

        var fileLinks = getMessageFileLinks();

        if (!forcibly && fileLinks.length) {
            var needShowShareDlg = $.grep(fileLinks, function (f) {
                return ASC.Files.Utility.CanWebEdit(f.title, true) && !ASC.Files.Utility.MustConvert(f.title);
            }).length > 0;

            if (needShowShareDlg) {
                window.popup.addBig(MailScriptResource.SharingSettingForFiles, $.tmpl('sharingSettingForFileLinksTmpl'));
                $("#shareFileLinksAccessSelector").tlcombobox({ align: "left" });

                TMMail.disableButton($('#editMessagePageHeader .btnSend'), false);
                TMMail.disableButton($('#editMessagePageHeader .btnSave'), false);
                if (messageId > 0) {
                    TMMail.disableButton($('#editMessagePageHeader .btnDelete'), false);
                    TMMail.disableButton($('#editMessagePageHeader .btnAddTag'), false);
                }
                return false;
            }
        }

        window.LoadingBanner.hideLoading();
        window.LoadingBanner.strLoading = ASC.Resources.Master.Resource.LoadingProcessing;
        clearTimeout(saveTimeout);

        LoadingBanner.displayMailLoading(window.MailScriptResource.SendingMessage);

        ASC.Mail.Utility.SendMessage(message, { skipAccountsCheck: true, skipSave: true })
            .done(onSendMessage)
            .fail(onErrorSendSaveMessage);
    }

    function getMessageFileLinks() {
        var fileLinks = [];
        $($('<div/>').append(wysiwygEditor.getValue())).find('.mailmessage-filelink').each(function () {
            fileLinks.push({ id: $(this).attr('data-fileid'), title: $(this).find('.mailmessage-filelink-link').attr('title') });
        });

        return fileLinks;
    }

    /* redraw item`s custom labels */

    function updateMessageTags(message) {
        var tagsPanel = $('#itemContainer .head[message_id=' + message.id + '] .tags');
        if (tagsPanel) {
            if (message.tagIds && message.tagIds.length) {
                tagsPanel.find('.value .itemTags').empty();
                var foundTags = false;
                $.each(message.tagIds, function (i, value) {
                    if (updateMessageTag(message.id, value)) {
                        foundTags = true;
                    }
                });

                if (foundTags) {
                    tagsPanel.show();
                }
            } else {
                tagsPanel.hide();
            }
        }
    }

    function updateMessageTag(messageId, tagId) {
        var tag = tagsManager.getTag(tagId);
        if (tag) {
            var tagsPanel = $('#itemContainer .head[message_id=' + messageId + '] .tags');
            if (tagsPanel.length) {
                var html = $.tmpl('tagInMessageTmpl', tag, { htmlEncode: TMMail.htmlEncode });
                var $html = $(html);

                tagsPanel.find('.value .itemTags').append($html);

                tagsPanel.find('a.tagDelete').unbind('click').click(function () {
                    messageId = $(this).closest('.message-wrap').attr('message_id');
                    var idTag = $(this).attr('tagid');
                    mailBox.unsetTag(idTag, [messageId]);
                });

                tagsPanel.show(); // show tags panel

                mailCache.setTag(messageId, tagId);

                return true;
            }
        } else // if crm tag was deleted then delete it from mail
        {
            mailBox.unsetTag(tagId, [messageId]);
        }

        return false;
    }

    function setTag(messageId, tagId) {
        updateMessageTag(messageId, tagId);
    }

    function unsetTag(messageId, tagId) {
        var tagsPanel = $('#itemContainer .head[message_id=' + messageId + '] .tags');

        if (tagsPanel.length) {
            var delEl = tagsPanel.find('.value .itemTags .tagDelete[tagid="' + tagId + '"]');

            if (delEl.length) {
                delEl.parent().remove();
            }

            if (!tagsPanel.find('.tag').length) {
                tagsPanel.hide();
            } else {
                tagsPanel.show();
            }
        }

        mailCache.setTag(messageId, tagId, true);
    }

    function isMessageDirty() {
        return messageIsDirty;
    }

    function isMessageSending() {
        return messageIsSending;
    }

    function isSortConversationByAsc() {
        return sortConversationByAsc;
    }

    function isNotTemplateItem() {
        return TMMail.pageIs('writemessage') && !TMMail.isTemplate();
    };

    /* -= Private Methods =- */

    function insertBody(message) {
        var $messageBody = $('#itemContainer .itemWrapper .body[message_id=' + message.id + ']');
        $messageBody.data('message', message);

        var html;
        if (message.isBodyCorrupted) {
            html = $.tmpl('errorBodyTmpl', {
                errorBodyHeader: ASC.Mail.Resources.MailScriptResource.ErrorOpenMessage,
                errorBody: $.trim($.tmpl("messageOpenErrorBodyTmpl").text())
            });

            $messageBody.html(html);
            $messageBody.toggleClass("body-error");
            TMMail.fixMailtoLinks($messageBody);
        } else if (message.hasParseError) {
            html = $.tmpl('errorBodyTmpl', {
                errorBodyHeader: ASC.Mail.Resources.MailScriptResource.ErrorOpenMessage,
                errorBody: $.trim($.tmpl("messageParseErrorBodyTmpl").text())
            });

            $messageBody.html(html);
            $messageBody.toggleClass("body-error");
            TMMail.fixMailtoLinks($messageBody);
        } else {
            if (message.textBodyOnly) {
                if (message.htmlBody) {
                    var quoteRegexp = new RegExp('(^|<br>|<body><pre>)&gt((?!<br>[^&gt]).)*(^|<br>)', 'ig');
                    var quoteArray = quoteRegexp.exec(message.htmlBody);

                    while (quoteArray) {
                        var lastIndex = quoteRegexp.lastIndex;
                        var substr = message.htmlBody.substring(quoteArray.index, lastIndex);
                        var searchStr = quoteArray[0].replace(/^(<br>|<body><pre>)/, '').replace(/<br>$/, '');
                        substr = substr.replace(searchStr, "<blockquote>" + searchStr + "</blockquote>");
                        message.htmlBody = message.htmlBody.substring(0, quoteArray.index) + substr + message.htmlBody.substr(lastIndex);
                        quoteArray = quoteRegexp.exec(message.htmlBody);
                    }
                }
                $messageBody.toggleClass("textOnly", true);
                $messageBody.html(message.htmlBody);

                $messageBody.find('blockquote').each(function () {
                    insertBlockquoteBtn($(this));
                });

                $messageBody.find('.tl-controll-blockquote').each(function () {
                    $(this).click(function () {
                        $(this).next('blockquote').toggle();
                        return false;
                    });
                });

                TMMail.fixMailtoLinks($messageBody);
                $messageBody.find("a").attr('target', '_blank');
                $("#delivery_failure_button").attr('target', '_self');
                if (message.to != undefined) {
                    $("#delivery_failure_faq_link").attr('href', TMMail.getFaqLink(message.to));
                }
                $messageBody.find("a[href*='mailto:']").removeAttr('target');
                displayTrustedImages(message);
            } else {
                var htmlText,
                    contentIsHidden = false;

                if (message.calendarUid) {
                    htmlText = "<blockquote>" + message.htmlBody + "</blockquote>";
                    contentIsHidden = true;
                } else {
                    htmlText = message.htmlBody;
                }

                $messageBody.html(htmlText);
                $messageBody.find("a[href*='mailto:']").removeAttr('target');
                $messageBody.find("a[href*='mailto:']").click(function () {
                    messagePage.setToEmailAddresses([$(this).attr('href').substr(7, $(this).attr('href').length - 1)]);
                    window.location.href = "#composeto";
                    return false;
                });
                $messageBody.find("a").attr('target', '_blank');

                var $blockquote = $($messageBody.find('div.gmail_quote:first, div.yahoo_quoted:first, ' +
                    'blockquote:first, div:has(hr#stopSpelling):last')[0]);
                if ($blockquote) {
                    insertBlockquoteBtn($blockquote);

                    var $btnBlockquote = $messageBody.find('.tl-controll-blockquote');
                    if ($btnBlockquote) {
                        $btnBlockquote.click(function () {
                            if (contentIsHidden) {
                                displayTrustedImages(message);
                                contentIsHidden = false;
                            }
                            $blockquote.toggle();
                            return false;
                        });
                    }
                }

                if (message.to != undefined) {
                    $messageBody.find("a#delivery_failure_faq_link").attr('href', TMMail.getFaqLink(message.to));
                }

                $messageBody.find("a[id='delivery_failure_button']").click(function () {
                    var deliveryFailureMessageId = $(this).attr("mailid");
                    messagePage.edit(deliveryFailureMessageId);
                });

                if (contentIsHidden) {
                    if (message.contentIsBlocked) {
                        var senderAddress = ASC.Mail.Utility.ParseAddress(message.from).email;
                        if (!trustedAddresses.isTrusted(senderAddress)) {
                            $('#id_block_content_popup_' + message.id).show();
                        }
                    }
                } else {
                    displayTrustedImages(message);
                }
            }

            if (message.calendarUid) {
                mailCalendar.loadAttachedCalendar(message);
            }
        }
        $('#itemContainer').height('auto');
    }

    function insertBlockquoteBtn(element) {
        element.before($.tmpl('blockquoteTmpl', {}).get(0).outerHTML);
        element.hide();
    }

    function updateFromSelected() {
        var accounts = accountsManager.getAccountList();
        var account;
        if (accounts.length > 1) {
            var buttons = [];
            for (var i = 0; i < accounts.length; i++) {
                account = accounts[i];
                var explanation = undefined;
                if (account.is_alias) {
                    explanation = window.MailScriptResource.AliasLabel;
                } else if (account.is_group) {
                    continue;
                }

                var text = (new ASC.Mail.Address(TMMail.htmlDecode(account.name), account.email).ToString(true));
                var cssClass = account.enabled ? '' : 'disabled';

                var accountInfo = {
                    text: text,
                    explanation: explanation,
                    handler: selectFromAccount,
                    account: account,
                    css_class: cssClass,
                    signature: account.signature
                };

                buttons.push(accountInfo);

            }
            $('#newmessageFromSelected').actionPanel({ buttons: buttons });
            $('#newmessageFromSelected .baseLinkArrowDown').show();
            $('#newmessageFromSelected').addClass('pointer');
            $('#newmessageFromSelected .baseLinkAction').addClass('baseLinkAction');
        } else {
            $('#newmessageFromSelected .baseLinkArrowDown').hide();
            $('#newmessageFromSelected .baseLinkAction').removeClass('baseLinkAction');
            $('#newmessageFromSelected').removeClass('pointer');
            if (accounts.length === 1) {
                account = accounts[0];
                selectFromAccount({}, {
                    account: account
                });
            } else {
                ASC.Controls.AnchorController.move('#accounts');
            }
        }
    }

    function setEditMessageButtons() {

        updateFromSelected();

        // Send
        $('#editMessagePageHeader .btnSend').unbind('click').click(function () {
            if ($(this).hasClass('disable')) return;

            if (TMMail.isTemplate()) {
                sendMessage(-1);
            } else {
                sendAction();
            }
        });

        // Save
        $('#editMessagePageHeader .btnSave').unbind('click').click(saveAction);

        // Save template
        $('#editMessagePageHeader .btnSaveTemplate').unbind('click').click(saveTemplateAction);

        // Advanced save dropdown
        $('#editMessagePageHeader .arrowDropdown').parent().actionPanel({
            buttons: [
                { text: window.MailScriptResource.SaveDraft, handler: saveAction },
                { text: window.MailScriptResource.SaveTemplate, handler: saveTemplateAction }
            ],
            css: 'stick-over'
        });

        // Delete
        $('#editMessagePageHeader .btnDelete').unbind('click').click(deleteAction);

        if (mailBox.currentMessageId < 1) {
            TMMail.disableButton($('#editMessagePageHeader .btnDelete'), true);
            TMMail.disableButton($('#editMessagePageHeader .btnAddTag'), true);
        }

        // Add tag
        $('#editMessagePageHeader .btnAddTag.unlockAction').unbind('click').click(function () {
            if ($(this).hasClass('disable')) return;

            var options = {
                hideMarkRecipients: true,
                onSelect: function (tag) {
                    mailBox.setTag(tag.id);
                },
                onDeselect: function (tag) {
                    mailBox.unsetConversationsTag(tag.id);
                },
                getUsedTagsIds: function () {
                    return messagePage.getTags();
                }
            }

            tagsDropdown.show($(this), options);
        });

        //Add templates
        $('#editMessagePageHeader .btnAddTemplate.unlockAction').off().mailtemplateadvancedSelector({
            showSaveButton: !TMMail.isTemplate(),
            isInitializeItems: true
        }).on('showList', function (event, item) {
            pasteTemplateToDraft(item);
        }).on('click', function () {
            var $selectorContainer = $('#WriteMessageGroupButtons .advanced-selector-container');
            if ($(this).hasClass('disable')) {
                $selectorContainer.hide();
            } else {
                slideTemplateSelector();
                $selectorContainer.find('.advanced-selector-reset-btn').click();
            }
        });

        $('#AddCopy:visible').unbind('click').click(function () {
            $('.value-group.cc').show();
            $('.value-group.bcc').show();
            $('#newmessageCopy .emailselector-input').focus();
            $(this).remove();
        });
    }

    function slideTemplateSelector() {
        var $selector = $('.advanced-selector-container'),
            $control = $('.btnAddTemplate').offset();

        if ($selector.is(':hidden') || !$control) {
            return;
        }

        if (window.pageYOffset > 70) {
            $selector.css({ position: "fixed", top: 32 });
        } else {
            $selector.css({ position: "absolute", top: $control.top + 24 });
        }
    };

    function pasteTemplateToDraft(id) {
        if (id !== undefined) {
            Teamlab.getMailMessage({}, id, {}, {
                success: function (params, data) {
                    var editor = CKEDITOR.instances.ckMailEditor,
                        editorData = editor.getData();

                    if ($('#newmessageSubject').val() === '') {
                        $('#newmessageSubject').val(data.subject);
                    }

                    if (data.important) {
                        var $importance = $('#editMessagePage .importance');

                        if ($importance.find('i').hasClass('icon-unimportant')) {
                            $importance.click();
                        }
                    }

                    pasteDataToAdvancedEmailSelector($('#newmessageTo'), data.to);
                    pasteDataToAdvancedEmailSelector($('#newmessageCopy'), data.cc);
                    pasteDataToAdvancedEmailSelector($('#newmessageBCC'), data.bcc);

                    if (data.cc.length || data.bcc.length) {
                        $('#AddCopy').click();
                    }

                    if (data.tagIds.length) {
                        for (var i = 0; i < data.tagIds.length; i++) {
                            if (isTagNotAttach(data.tagIds[i])) {
                                setTag(mailBox.currentMessageId, data.tagIds[i]);
                            }
                        }
                    }

                    if (commonSettingsPage.ReplaceMessageBodyEnabled() && !TMMail.pageIs('reply')) {
                        editor.setData(data.htmlBody);
                    } else {
                        editor.setData(data.htmlBody + editorData);
                    }

                    if (data.attachments.length) {
                        pasteAttachmentsFromTemplate(data.attachments);
                    }
                },
                error: function (e, error) {
                    console.log(error);
                }
            });
        }
    }

    function isTagNotAttach(tag) {
        var tags = jq('#itemContainer .head[message_id=' + mailBox.currentMessageId + '] .tags').find('.tagDelete');

        for (var i = 0; i < tags.length; i++) {
            if ($(tags[i]).attr('tagId') == tag) {
                return false;
            }
        }

        return true;
    }

    function pasteDataToAdvancedEmailSelector($control, data) {
        if (!$control.AdvancedEmailSelector('get').length) {
            $control.AdvancedEmailSelector("init", {
                isInPopup: false,
                items: data,
                onChangeCallback: function () {
                    $control.removeClass('invalidField');
                    onMessageChanged();
                }
            });
        }
    }

    function pasteAttachmentsFromTemplate(attachments) {
        var currentAttachments = AttachmentManager.GetAttachments(),
            currentAttachmentsSize = 0,
            pasteAttachments = attachments;

        currentAttachmentsSize = currentAttachments.reduce(function (prev, curr) {
            return prev + curr.size;
        }, 0);

        if (commonSettingsPage.ReplaceMessageBodyEnabled()) {
            currentAttachments = [];
            for (var i = 0; i < pasteAttachments.length; i++) {
                AttachmentManager.CompleteAttachment(pasteAttachments[i]);
                currentAttachments.push(pasteAttachments[i]);
            }
        } else {
            for (var i = 0; i < pasteAttachments.length; i++) {
                for (var j = 0; j < currentAttachments.length; j++) {
                    if (currentAttachments[j].fileId === pasteAttachments[i].fileId) {
                        return false;
                    }
                }

                currentAttachmentsSize += pasteAttachments[i].size;

                if (currentAttachmentsSize > AttachmentManager.MaxTotalSizeInBytes) {
                    pasteAttachments[i].attachAsLinkOffer = true;
                    AttachmentManager.OnAttachDocumentError({ attachment: pasteAttachments[i] }, [window.MailScriptResource.AttachmentsTotalLimitError]);
                }

                AttachmentManager.CompleteAttachment(pasteAttachments[i]);
                currentAttachments.push(pasteAttachments[i]);
            }
        }

        AttachmentManager.InitUploader(currentAttachments);
    }

    function setMenuActionButtons(senderAddress) {
        isMessageRead = true;

        var contentMenu = $("#pageActionContainer .contentMenuWrapper:visible");

        contentMenu.find('.btnReply.unlockAction').unbind('click').click(function () {
            // Google Analytics
            window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.buttonClick, "reply");
            var messageId = mailBox.currentMessageId;

            if (TMMail.pageIs('conversation')) {
                messageId = getActualConversationLastMessageId();
            }

            checkhBlockedImagesBeforeCompose(messageId, function() {
                return TMMail.moveToReply(messageId);
            });
        });

        contentMenu.find('.btnReplyAll.unlockAction').unbind('click').click(function () {
            // Google Analytics
            window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.buttonClick, "replyAll");
            var messageId = mailBox.currentMessageId;

            if (TMMail.pageIs('conversation')) {
                messageId = getActualConversationLastMessageId();
            }

            checkhBlockedImagesBeforeCompose(messageId, function () {
                return TMMail.moveToReplyAll(messageId);
            });
        });

        contentMenu.find('.btnForward.unlockAction').unbind('click').click(function () {
            // Google Analytics
            window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.buttonClick, "forward");
            var messageId = mailBox.currentMessageId;

            if (TMMail.pageIs('conversation')) {
                messageId = getActualConversationLastMessageId();
            }

            checkhBlockedImagesBeforeCompose(messageId, function () {
                return TMMail.moveToForward(messageId);
            });
        });

        contentMenu.find('.btnDelete.unlockAction').unbind('click').click(deleteAction);
        contentMenu.find('.btnSpam.unlockAction').unbind('click').click(spamCurrent);
        contentMenu.find('.btnNotSpam.unlockAction').unbind('click').click(unspamCurrent);
        contentMenu.find('.btnRestore.unlockAction').unbind('click').click(restoreMessageAction);

        // Delete
        contentMenu.find('.menuActionDelete').unbind('click').click(function () {
            if ($(this).hasClass('unlockAction')) {
                deleteCurrent();
            }
        });

        $('#menuActionBack').unbind('click').click(function () {
            mailBox.updateAnchor(true, true);
        });

        contentMenu.find('.menuActionSpam').toggle(MailFilter.getFolder() != TMMail.sysfolders.spam.id);
        contentMenu.find('.menuActionNotSpam').toggle(MailFilter.getFolder() == TMMail.sysfolders.spam.id);

        // Spam
        contentMenu.find('.menuActionSpam').unbind('click').click(function () {
            spamCurrent();
        });

        // NotSpam
        contentMenu.find('.menuActionNotSpam').unbind('click').click(function () {
            unspamCurrent();
        });

        var moreButton = $('.btnMore');

        if (moreButton != undefined) {
            var folderId = parseInt(MailFilter.getFolder());
            var buttons = [];

            var printBtnLabel = $("#itemContainer .message-wrap").length > 1 ? window.MailScriptResource.PrintAllBtnLabel : window.MailScriptResource.PrintBtnLabel;

            switch (folderId) {
                case TMMail.sysfolders.inbox.id:
                case TMMail.sysfolders.userfolder.id:
                    if (ASC.Mail.Constants.PRINT_AVAILABLE)
                        buttons.push({ text: printBtnLabel, handler: moveToPrint });

                    buttons.push({ text: window.MailScriptResource.UnreadLabel, handler: readUnreadMessageAction });
                    if (ASC.Mail.Constants.CRM_AVAILABLE) {
                        buttons.push({ text: window.MailResource.LinkChainWithCRM, handler: crmLinkPopup.showCrmLinkConversationPopup });
                    }
                    break;
                case TMMail.sysfolders.sent.id:
                    if (ASC.Mail.Constants.PRINT_AVAILABLE)
                        buttons.push({ text: printBtnLabel, handler: moveToPrint });
                    buttons.push({ text: window.MailScriptResource.UnreadLabel, handler: readUnreadMessageAction });
                    if (ASC.Mail.Constants.CRM_AVAILABLE) {
                        buttons.push({ text: window.MailResource.LinkChainWithCRM, handler: crmLinkPopup.showCrmLinkConversationPopup });
                    }
                    break;
                case TMMail.sysfolders.trash.id:
                    if (ASC.Mail.Constants.PRINT_AVAILABLE)
                        buttons.push({ text: printBtnLabel, handler: moveToPrint });

                    buttons.push({ text: window.MailScriptResource.UnreadLabel, handler: readUnreadMessageAction });
                    break;
                case TMMail.sysfolders.spam.id:
                    if (ASC.Mail.Constants.PRINT_AVAILABLE)
                        buttons.push({ text: printBtnLabel, handler: moveToPrint });

                    buttons.push({ text: window.MailScriptResource.UnreadLabel, handler: readUnreadMessageAction });
                    break;
                default:
            }

            if (senderAddress) {
                if (!commonSettingsPage.AlwaysDisplayImages()) {
                    if (trustedAddresses.isTrusted(senderAddress)) {
                        buttons.push({
                            text: window.MailScriptResource.HideImagesLabel + ' "' + senderAddress + '"',
                            handler: hideImagesAction,
                            address: senderAddress
                        });
                    }
                }
            }

            moreButton.actionPanel({ buttons: buttons, css: 'stick-over' });
        }

        // Add tag
        contentMenu.find('.btnAddTag.unlockAction').unbind('click').click(function () {
            if ($(this).hasClass('disable')) return;

            var hideCbx = messagePage.getMessageFolder(
                    messagePage.getActualConversationLastMessageId()) ===
                TMMail.sysfolders.trash.id;

            var options = {
                hideMarkRecipients: hideCbx,
                onSelect: function(tag) {
                    var id = tag.id;

                    if (!hideCbx && tagsDropdown.isMarkChecked()) {
                        tag = tagsManager.getTag(id);
                        var addresses = [];

                        var from = ASC.Mail.Utility.ParseAddress(messagePage.getFromAddress(messagePage.getActualConversationLastMessageId()));

                        if (accountsManager.getAccountByAddress(from.email)) {
                            addresses = ASC.Mail.Utility.ParseAddresses(messagePage.getToAddresses(messagePage.getActualConversationLastMessageId())).addresses;
                        } else
                            addresses.push(from);

                        for (var i = 0; i < addresses.length; i++) {
                            var address = addresses[i];
                            var tagAlreadyAdded = 0 < $.grep(tag.addresses, function (val) { return address.EqualsByEmail(val); }).length;
                            if (!tagAlreadyAdded) {
                                tag.addresses.push(address.email);
                            }
                        }
                        tagsManager.updateTag(tag);
                    }

                    mailBox.setTag(id);
                },
                onDeselect: function(tag) {
                    console.log(tag);
                    mailBox.unsetConversationsTag(tag.id);
                },
                getUsedTagsIds: function () {
                    return messagePage.getTags();
                }
            }

            tagsDropdown.show($(this), options);
        });

        // Move to
        contentMenu.find('.btnMoveTo.unlockAction').unbind('click').click(function () {
            if ($(this).hasClass('disable')) {
                return false;
            }

            var options = {
                btnCaption: window.MailResource.MoveHere,
                hideDefaults: false,
                hideRoot: true,
                callback: function (folder) {
                    console.log(".btnMoveTo.unlockAction -> callback", folder);
                    moveTo(folder.folderType, folder.userFolderId);
                }
            };

            var folderType = MailFilter.getFolder();
            if (folderType !== TMMail.sysfolders.userfolder.id) {
                options.disableDefaultId = folderType;
            } else {
                options.disableUFolderId = MailFilter.getUserFolder();
            }

            userFoldersDropdown.show($(this), options);

            return true;
        });
    }

    function setConversationViewActions() {
        $('#sort-conversation').toggleClass('asc', isSortConversationByAsc()).toggleClass('desc', !isSortConversationByAsc());

        $('#sort-conversation').unbind('click').click(function () {
            sortConversationByAsc = !isSortConversationByAsc();

            $('#sort-conversation').toggleClass('asc', isSortConversationByAsc()).toggleClass('desc', !isSortConversationByAsc());

            $('.itemWrapper .conversation-view_scrollable').append($('.message-wrap, .collapsed-messages').get().reverse());

            //restore iframe contents

            TMMail.option('ConversationSortAsc', sortConversationByAsc);
        });

        $('#collapse-conversation').unbind('click').click(function () {
            if ($('.full-view:hidden').length > 0) {
                showCollapsedMessages();

                $('.full-view[loaded="true"]').each(function (index, el) {
                    var messageId = $(el).attr('message_id');
                    if (typeof (messageId) !== 'undefined') {
                        expandConversation(messageId);
                    }
                });

                var messages = [];

                $('.full-view[loaded!="true"]').each(function(index, el) {
                    var messageId = $(el).attr('message_id'),
                        parentWrap = $(el).closest('.message-wrap'),
                        shortView = parentWrap.children('.short-view'),
                        loader = shortView.find('.loader');

                    showConversationMessage(messageId, false, false);
                    shortView.addClass('loading');
                    loader.show();

                    var folder = $(parentWrap).attr('folder');
                    var from = $(el).find('.from').text();
                    messages.push({ folder: folder, from: from });
                });

                checkMessagesSender(messages);
            } else {
                $('.full-view').each(function (index, el) {
                    var messageId = $(el).attr('message_id');
                    collapseConversation(messageId);
                });
            }
        });
    }

    function showBlockquote(messageId) {
        var body = $('#itemContainer .itemWrapper .body[message_id=' + messageId + ']');
        var blockquote = body.find('.tl-controll-blockquote');
        if (blockquote.length > 0) {
            var $blockquote = $(body.find('div.gmail_quote:first, div.yahoo_quoted:first, ' +
                'blockquote:first, div:has(hr#stopSpelling):last')[0]);

            if ($blockquote.is(':hidden'))
                blockquote.trigger("click");
        }
    }

    function initBlockContent(message) {
        $('#id-btn-block-content-' + message.id).unbind('click').click(function () {
            showBlockquote(message.id);
            displayImages(message.id);
        });

        $('#id-btn-always-block-content-' + message.id).unbind('click').click(function () {
            if (TMMail.pageIs('conversation')) {
                var currentSender = message.sender_address;
                trustedAddresses.add(currentSender);
                var conversationMessages = $('#itemContainer').find('.message-wrap');
                var i, len;
                for (i = 0, len = conversationMessages.length; i < len; i++) {
                    var currentMessage = $(conversationMessages[i]);
                    if (currentMessage.find('.full-view[loaded="true"]').length === 1) {
                        // message is loaded
                        var messageId = currentMessage.attr('message_id');
                        var senderAddress = ASC.Mail.Utility.ParseAddress(getFromAddress(messageId));
                        if (senderAddress.EqualsByEmail(currentSender)) {
                            showBlockquote(messageId);
                            displayImages(messageId);
                        }
                    }
                }
                setMenuActionButtons(currentSender);

            } else {
                trustedAddresses.add(message.sender_address);
                showBlockquote(message.id);
                displayImages(message.id);
                setMenuActionButtons(message.sender_address);
            }
        });
    }

    function renameAttr(node, attrName, newAttrName) {
        node.each(function () {
            var val = node.attr(attrName);
            node.attr(newAttrName, val);
            node.removeAttr(attrName);
        });
    }

    function displayImages(messageId) {
        var messageBody = $('#itemContainer .itemWrapper .body[message_id=' + messageId + ']');
        if (messageBody) {
            var styleTag = messageBody.find('style');
            if (styleTag.length > 0) { // style fix
                styleTag.html(styleTag.html().replace(/tl_disabled_/g, ''));
            }

            messageBody.find('*').each(function (index, node) {
                $(node.attributes).each(function () {
                    if (typeof this.nodeValue === 'string') {
                        this.nodeValue = this.nodeValue.replace(/tl_disabled_/g, '');
                    }

                    if (this.nodeName.indexOf('tl_disabled_') > -1) {
                        renameAttr($(node), this.nodeName, this.nodeName.replace(/tl_disabled_/g, ''));
                    }
                });
            });

            $('#itemContainer .full-view[message_id=' + messageId + ']').attr('content_blocked', false);

            $('#id_block_content_popup_' + messageId).remove();

        }
    }

    function selectFromAccount(event, params) {
        if (params.account.is_group)
            return;

        var text = (new ASC.Mail.Address(TMMail.htmlDecode(params.account.name), params.account.email).ToString(true));
        $('#newmessageFromSelected').attr('mailbox_email', params.account.email);
        $('#newmessageFromSelected span').text(text);
        $('#newmessageFromSelected').toggleClass('disabled', !params.account.enabled);
        $('#newmessageFromWarning').toggle(!params.account.enabled);
        wysiwygEditor.setSignature(params.account.signature);
        accountsPanel.mark(params.account.email);
    }

    function deleteMessageAttachment(attachId) {
        serviceManager.deleteMessageAttachment(mailBox.currentMessageId, attachId);
        showSavedTime();
    }

    function showSavedTime() {
        var now = new Date();

        var min = now.getMinutes() + '';

        if (min.length == 1) {
            min = '0' + min;
        }

        var saveTime = now.getHours() + ':' + min;

        $('.savedtime').show();

        $('.savedtime .savedtime-value').text(saveTime);
    }

    function deleteCurrentMessage() {
        resetDirtyMessage();
        messageIsSending = false;
        AttachmentManager.Unbind(AttachmentManager.CustomEvents.UploadComplete);
        if (mailBox.currentMessageId > 0) {
            mailBox.deleteMessage(mailBox.currentMessageId, TMMail.sysfolders.trash.id);
            mailBox.markFolderAsChanged(MailFilter.getFolder());
        }
    }

    function deleteAction() {
        if ($(this).hasClass('disable')) {
            return false;
        }
        // Google Analytics
        window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.actionClick, "delete");
        TMMail.disableButton($('#editMessagePageHeader .btnDelete'), true);
        TMMail.disableButton($('#editMessagePageHeader .btnSend'), true);
        TMMail.disableButton($('#editMessagePageHeader .btnSave'), true);
        TMMail.disableButton($('#editMessagePageHeader .btnAddTag'), true);
        TMMail.disableButton($('#editMessagePageHeader .btnSaveTemplate'), true);
        deleteCurrent();
        return false;
    }

    function saveAction() {
        if ($(this).hasClass('disable')) {
            return false;
        }
        // Google Analytics
        window.ASC.Mail.ga_track(ga_Categories.createMail, ga_Actions.buttonClick, "save");
        TMMail.disableButton($('#editMessagePage .btnSave'), true);

        if (TMMail.isTemplate()) {
            var onAfterMoveCallback = function () {
                TMMail.moveToDraftItem(mailBox.currentMessageId);
            }
            saveMessageTemplatePomise(true).done(function () {
                mailBox.moveMessage(mailBox.currentMessageId, TMMail.sysfolders.templates.id, TMMail.sysfolders.drafts.id, undefined, undefined, onAfterMoveCallback);
            });
        } else {
            saveMessage(true);
        }

        return false;
    }

    function saveTemplateAction() {
        if ($(this).hasClass('disable')) {
            return false;
        }
        // Google Analytics
        window.ASC.Mail.ga_track(ga_Categories.createMailTemplate, ga_Actions.buttonClick, "saveTemplate");
        TMMail.disableButton($('#editMessagePage .btnSaveTemplate'), true);

        if (isNotTemplateItem()) {
            var onAfterMoveCallback = function () {
                TMMail.moveToTemplateItem(mailBox.currentMessageId);
            };
            saveMessagePomise(true).done(function () {
                mailBox.moveMessage(mailBox.currentMessageId, TMMail.sysfolders.drafts.id, TMMail.sysfolders.templates.id, undefined, undefined, onAfterMoveCallback);
            });
        } else {
            saveMessageTemplate(true);
        }

        return false;
    }

    function sendAction(params, forcibly) {
        if ($(this).hasClass('disable')) {
            return false;
        }

        if ($('#newmessageFromSelected').hasClass('disabled')) {
            window.LoadingBanner.hideLoading();
            window.toastr.warning(window.MailScriptResource.SendFromDeactivateAccount);
            return false;
        }

        //google analytics
        window.ASC.Mail.ga_track(ga_Categories.createMail, ga_Actions.buttonClick, "send");
        TMMail.disableButton($('#editMessagePageHeader .btnSend'), true);
        TMMail.disableButton($('#editMessagePageHeader .btnSave'), true);
        TMMail.disableButton($('#editMessagePageHeader .btnDelete'), true);
        TMMail.disableButton($('#editMessagePageHeader .btnAddTag'), true);
        sendMessage(undefined, forcibly);
        return false;
    }

    // returns id of actual last message in chain
    // during chain viewing, messages set could be changed (ex. last message could be deleted)
    // values stored in mailBox.currentMessageId or acnhor are not valid any more
    // so actual last message will be detected from markup
    function getActualConversationLastMessageId() {
        var messageId;

        var folder = MailFilter.getFolder();

        var content = mailCache.get(mailBox.currentMessageId);

        if (content) {
            var folderMessages = content.messages.filter(function(val) {
                    return val.folder == folder;
                })
                .sort(function(m1, m2) {
                    var dt1 = new Date(m1.date);
                    var dt2 = new Date(m2.date);
                    
                    if (dt1 < dt2)
                        return 1;
                    if (dt1 > dt2)
                        return -1;
                    return 0;
                });

            messageId = folderMessages.length > 0 ? folderMessages[0].id : mailBox.currentMessageId;
        } else {
            messageId = mailBox.currentMessageId;
        }

        return messageId;
    }

    function deleteCurrent() {
        if (TMMail.pageIs('conversation')) {
            mailBox.deleteConversation(getActualConversationLastMessageId(), MailFilter.getFolder());
        } else {
            mailBox.deleteMessage(mailBox.currentMessageId, MailFilter.getFolder());
        }
    }

    function restoreMessageAction() {
        // Google Analytics
        window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.actionClick, "restore");

        if (TMMail.pageIs('conversation')) {
            mailBox.restoreConversations([getActualConversationLastMessageId()]);
        } else {
            mailBox.restoreMessages([mailBox.currentMessageId]);
        }
    }

    function hideImagesAction(event, params) {
        trustedAddresses.remove(params.address);
        ASC.Controls.AnchorController.move(ASC.Controls.AnchorController.getAnchor());
    }

    function readUnreadMessageAction() {
        isMessageRead = !isMessageRead;
        // Google Analytics
        if (isMessageRead) {
            window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.actionClick, "read");
        } else {
            window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.actionClick, "unread");
        }

        if (TMMail.pageIs('conversation')) {
            mailBox.setConversationReadUnread([getActualConversationLastMessageId()], isMessageRead);
        } else {
            mailBox.setMessagesReadUnread([mailBox.currentMessageId], isMessageRead);
        }

        mailBox.updateAnchor(true, true);
    }

    function spamUnspamAction(event, params) {
        if (params.spam) {
            // Google Analytics
            window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.actionClick, "spam");
            spamCurrent();
        } else {
            // Google Analytics
            window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.actionClick, "not_spam");
            unspamCurrent();
        }
    }

    function moveToPrint() {
        if (TMMail.pageIs('conversation')) {
            moveToConversationPrint(getActualConversationLastMessageId());
        } else {
            moveToMessagePrint(mailBox.currentMessageId);
        }
    }

    function moveToConversationPrint(conversationId) {
        var simIds = [];
        var squIds = [];
        var sortAsc = isSortConversationByAsc();

        var fullView = $('#itemContainer .itemWrapper .full-view[loaded=true]');
        for (var i = 0, len = fullView.length; i < len; i++) {
            var messageId = $(fullView[i]).attr('message_id');
            if ($(fullView[i]).find('#id_block_content_popup_' + messageId).length == 0) {
                simIds.push(messageId);
            }

            var $messageBody = $('#itemContainer .itemWrapper .body[message_id=' + messageId + ']');
            var $quote = $messageBody.find('div.gmail_quote:first, div.yahoo_quoted:first, blockquote:first, div:has(hr#stopSpelling):last');
            if ($quote.is(':visible')) {
                squIds.push(messageId);
            }
        }

        var html = '';
        if (simIds.length != fullView.length) {
            html = $.tmpl('imagesBlockedPopupTmpl', { text: MailScriptResource.ConversationImagesBlockedPopupBody });
        }
        else if ($('#itemContainer .itemWrapper .full-view[loaded!=true]').length != 0) {
            html = $.tmpl('imagesBlockedPopupTmpl', { text: MailScriptResource.ConversationCollapsedBlockedPopupBody });
        }

        if (html != "") {
            $(html).find('.buttons .okBtn').bind("click", function () {
                TMMail.moveToConversationPrint(conversationId, simIds, squIds, sortAsc);
                window.popup.hide();
            });
            window.popup.addBig(MailScriptResource.MessageImagesBlockedPopupHeader, html);
        } else {
            TMMail.moveToConversationPrint(conversationId, simIds, squIds, sortAsc);
        }
    }

    function checkhBlockedImagesBeforeCompose(messageId, successFunc) {
        var hasBlockedImages = $('#itemContainer .itemWrapper .full-view[loaded=true]').find('#id_block_content_popup_' + messageId).length > 0;
        var html = '';
        if (hasBlockedImages) {
            html = $.tmpl('imagesBlockedPopupTmpl', { text: MailScriptResource.MessageImagesBlockedContinuePopupBody });
        }

        if (html !== "") {
            $(html).find('.buttons .okBtn').bind("click", function () {
                if (typeof (successFunc) === "function") {
                    successFunc();
                }

                window.popup.hide();
            });
            window.popup.addBig(MailScriptResource.MessageImagesBlockedContinuePopupHeader, html);
        } else {
            if (typeof (successFunc) === "function") {
                successFunc();
            }
        }
    }

    function moveToMessagePrint(messageId) {
        var $blockImagesBox = $('#id_block_content_popup_' + messageId);

        var $messageBody = $('#itemContainer .itemWrapper .body[message_id=' + messageId + ']');
        var $quote = $messageBody.find('div.gmail_quote:first, div.yahoo_quoted:first, blockquote:first, div:has(hr#stopSpelling):last');
        var showQuotes = $quote.is(':visible');

        if ($blockImagesBox.length) {
            var html = $.tmpl('imagesBlockedPopupTmpl', { text: MailScriptResource.MessageImagesBlockedPopupBody });
            $(html).find('.buttons .okBtn').bind("click", function () {
                TMMail.moveToMessagePrint(messageId, false, showQuotes);
                window.popup.hide();
            });
            window.popup.addBig(MailScriptResource.MessageImagesBlockedPopupHeader, html);
        } else {
            TMMail.moveToMessagePrint(messageId, true, showQuotes);
        }
    }

    function moveTo(folderType, userFolderId) {
        if (TMMail.pageIs('conversation')) {
            mailBox.moveConversation(getActualConversationLastMessageId(), MailFilter.getFolder(), folderType, userFolderId);
        } else {
            mailBox.moveMessage(mailBox.currentMessageId, MailFilter.getFolder(), folderType, null, userFolderId);
        }
    }

    function spamCurrent() {
        if (TMMail.pageIs('conversation')) {
            mailBox.moveConversation(getActualConversationLastMessageId(), MailFilter.getFolder(), TMMail.sysfolders.spam.id);
        } else {
            mailBox.moveMessage(mailBox.currentMessageId, MailFilter.getFolder(), TMMail.sysfolders.spam.id);
        }
    }

    function unspamCurrent() {
        if (TMMail.pageIs('conversation')) {
            mailBox.restoreConversations([getActualConversationLastMessageId()], true);
        } else {
            mailBox.restoreMessages([mailBox.currentMessageId], true);
        }
    }

    function initMessagePanel(message, action) {
        updateMessageTags(message);

        if ('draft' == action || 'template' == action || 'forward' == action || 'reply' == action || 'compose' == action || 'replyAll' == action) {
            var from = ASC.Mail.Utility.ParseAddress(message.from).email;
            var account = undefined;
            if (from !== "") {
                account = accountsManager.getAccountByAddress(from) || accountsManager.getDefaultAccount();
            }

            updateFromAccountField(account);

            messageIsDirty = false;

            if ('reply' == action || 'replyAll' == action) {
                message.attachments = [];
            }

            AttachmentManager.InitUploader(message.attachments);

            updateEditAttachmentsActionMenu();

            /*
            setTimeout(function () {
                AttachmentManager.InitUploader(message.attachments);
                updateEditAttachmentsActionMenu();
            }, 10); // Dirty trick for Opera 12
            */

            $('#editMessagePageHeader .on-close-link').bind("click", messagePage.onLeaveMessage);
            $('#tags_panel div.tag').bind("click", messagePage.onLeaveMessage);
            $('#tags_panel div.tag').each(function () {
                var elementData = $._data(this),
                    events = elementData.events;

                var onClickHandlers = events['click'];

                // Only one handler. Nothing to change.
                if (onClickHandlers.length == 1) {
                    return;
                }

                onClickHandlers.splice(0, 0, onClickHandlers.pop());
            });

            $('#editMessagePage .importance')
                .unbind('click')
                .click(function () {
                    toggleImportance($(this));
                    onMessageChanged();
                });

            $('#editMessagePage .requestReceipt').unbind('click')
                .click(function () {
                    $(this).toggleClass("on");
                });
        }
    }

    function updateReplyTo(value) {
        var message = getEditingMessage();
        message.mimeReplyToId = value;
        setEditingMessage(message);
    }

    function setEditingMessage(message) {
        $('#editMessagePage').data('message', message);
    }

    function bindOnMessageChanged() {
        $('#newmessageSubject').bind('textchange', function () {
            // Subject has changed, then it's a new chain;
            updateReplyTo("");
            onMessageChanged();
        });
        $('#newmessageImportance').bind('click', onMessageChanged);
        $('div.itemTags').bind('DOMNodeInserted DOMNodeRemoved', onMessageChanged);
    }

    function onMessageChanged() {
        if (TMMail.pageIs('writemessage')) {
            jq.confirmBeforeUnload(function () { return messageIsDirty });
            $(this).removeClass('invalidField');
            clearTimeout(saveTimeout);
            setDirtyMessage();
            saveTimeout = setTimeout(function () {
                if (messageIsDirty && !TMMail.isTemplate()) {
                    saveMessage(false);
                }
            }, TMMail.saveMessageInterval);
        }
    }

    function onLeaveMessage(e) {
        if (TMMail.pageIs('writemessage')) {
            if (messagePage.isMessageSending()) {
                if (confirm(window.MailScriptResource.MessageNotSent)) {
                    deleteCurrentMessage();
                } else {
                    if (e != undefined) {
                        e.preventDefault();
                        e.stopPropagation();
                    }
                    return false;
                }
            } else if (isMessageDirty() && isNotTemplateItem()) {
                saveMessage(true);
            }
            else if (TMMail.isTemplate()) {
                if (messageIsDirty) {
                    var body = $.tmpl('contentLossWarning');

                    popup.addBig(window.MailScriptResource.SaveTemplateChanges, body);

                    e.preventDefault();
                    e.stopPropagation();
                    return false;
                }
            }

            closeCompose(e);
        }
        return true;
    }

    function closeMessagePanel() {
        AttachmentManager.Unbind(AttachmentManager.CustomEvents.UploadComplete);
        clearTimeout(saveTimeout);
        wysiwygEditor.close();
        saveTimeout = null;

        resetDirtyMessage();
        AttachmentManager.StopUploader();

        $('#attachments_upload_btn').unbind('mouseenter');

        $('#newmessageTo').unbind('textchange');

        $('#newmessageCopy').unbind('textchange');

        $('#newmessageBCC').unbind('textchange');

        $('#newmessageSubject').unbind('textchange');

        $('#newmessageImportance').unbind('click');

        $('div.itemTags').unbind('DOMNodeInserted DOMNodeRemoved');

        $('#pageActionContainer').empty();
        $('#editMessagePageFooter').empty();
        $('#editMessagePage').hide();

        $('#tags_panel div.tag').unbind('click', messagePage.onLeaveMessage);

        TMMail.hideAllActionPanels();

        jq(".page-content").off("scroll");
    }

    function setDirtyMessage() {
        messageIsDirty = true;
        $('#newMessageSaveMarker').text(' *');
    }

    // sets jquery or string object as wysiwig editor content

    function setWysiwygEditorValue(message, action) {
        if (action == 'reply' || action == 'replyAll') {
            wysiwygEditor.setReply(message);
        } else if (action == 'forward') {
            wysiwygEditor.setForward(message);
        } else {
            wysiwygEditor.setDraft(message);
        }
    }

    /* -= Callbacks =- */

    function onSaveTemplate(params, message) {
        if (mailBox.currentMessageId != message.id && TMMail.isTemplate()) {
            TMMail.moveToTemplateItem(message.id);
        }

        params.needRefreshAttachments = AttachmentManager.GetAttachments().some(function (v) { return v.tempStoredUrl; });

        onSaveMessage(params, message);
    }

    function onSaveDraft(params, message) {
        if (mailBox.currentMessageId != message.id && TMMail.isTemplate()) {
            TMMail.moveToDraftItem(message.id);
        }

        params.needRefreshAttachments = TMMail.pageIs('forward') && mailBox.currentMessageId < 1 && message.attachments.length > 0;

        onSaveMessage(params, message);
    }

    function onSaveMessage(params, message) {
        TMMail.disableButton(jq('#WriteMessageGroupButtons').find('.menuAction:not(.btnDelete), .buttonDropdown ,.arrowDropdown'), false);

        mailBox.markFolderAsChanged(TMMail.sysfolders.templates.id);
        mailBox.markFolderAsChanged(TMMail.sysfolders.drafts.id);

        showSavedTime();
        resetDirtyMessage();

        if (message.id > 0) {
            if (params.needRefreshAttachments) {
                var attachmentManager = AttachmentManager,
                    currentAttachments = attachmentManager.GetAttachments();

                for (var i = 0; i < currentAttachments.length; i++) {
                    var currentAttachment = currentAttachments[i];

                    if (currentAttachment.tempStoredUrl) {
                        attachmentManager.UpdateAttachment(i, message.attachments[i]);
                    }
                }
            }

            $('#itemContainer .head[message_id]:visible').attr('message_id', message.id);
            mailBox.currentMessageId = message.id;
            TMMail.disableButton($('#editMessagePageHeader .btnDelete'), false);
            TMMail.disableButton($('#editMessagePageHeader .btnAddTag'), false);
        }

        releaseSavingLock();
        setEditingMessage(message);

        if (needCrmLink()) {
            serviceManager.markChainAsCrmLinked(mailBox.currentMessageId, crmContactsInfo, {}, ASC.Resources.Master.Resource.LoadingProcessing);
        }
    }

    function onErrorSendSaveMessage(params, error) {
        var $editMessageButtons = $('#WriteMessageGroupButtons'),
            error = jq.isArray(error) ? error[0] : error;

        if (!navigator.onLine) {
            error = MailScriptResource.ErrorLostInternetConnection;
        }

        if (error === "Message body exceeded limit") {
            window.toastr.warning(
                MailScriptResource.ErrorMailMessageSizeBody,
                MailScriptResource.ErrorMailMessageSizeHead.format(ASC.Mail.Constants.MAXIMUM_MESSAGE_BODY_SIZE / 1024),
                {
                    'closeButton': true,
                    'timeOut': '0',
                    'extendedTimeOut': '0'
                });

            TMMail.disableButton($editMessageButtons.find('.menuAction:not(.btnDelete), .buttonDropdown ,.arrowDropdown'), true);
        } else {
            window.toastr.warning(error);
            TMMail.disableButton($editMessageButtons.find('.menuAction:not(.btnDelete), .buttonDropdown ,.arrowDropdown'), false);
        }

        releaseSavingLock();
        LoadingBanner.hideLoading();
    }

    function onSendMessage(options, messageId) {

        lastSendMessageId = messageId;

        resetDirtyMessage(); // because it is saved while trying to send

        mailBox.markFolderAsChanged(TMMail.sysfolders.drafts.id);

        $('#itemContainer').height('auto');

        mailBox.markFolderAsChanged(TMMail.sysfolders.inbox.id); // for delivery failure messages

        mailBox.markFolderAsChanged(TMMail.sysfolders.sent.id);

        LoadingBanner.hideLoading();

        ASC.Controls.AnchorController.move(TMMail.sysfolders.inbox.name);
    }

    function refreshMailAfterSent(state) {
        serviceManager.updateFolders();
        window.LoadingBanner.hideLoading();

        if (state === -1)
            mailAlerts.check(lastSendMessageId ? { showFailureOnlyMessageId: lastSendMessageId } : {});
    }

    function openEmptyComposeForm(params) {
        var messageTemplate = new ASC.Mail.Message();
        params = params || {};

        MailFilter.reset();
        closeMessagePanel();
        mailBox.hidePages();
        $('#itemContainer').height('auto');

        mailBox.hideContentDivs();
        mailBox.hideLoadingMask();

        var editMessagePageHeaderHtml = $.tmpl('editMessagePageHeaderTmpl');
        $('#pageActionContainer').append(editMessagePageHeaderHtml);

        var writeMessageHtml = $.tmpl('writeMessageTmpl');
        var editMessageContainer = $('#itemContainer').find('#editMessagePage');
        if (editMessageContainer.length > 0) {
            editMessageContainer.replaceWith(writeMessageHtml);
        } else {
            $('#itemContainer').append(writeMessageHtml);
        }

        var action = 'compose';

        var activeAccount = accountsPanel.getActive() || accountsManager.getDefaultAccount();
        if (activeAccount) {
            messageTemplate.from = activeAccount.email;
        }

        initMessagePanel(messageTemplate, action);

        mailBox.currentMessageId = 0;

        if (params.email) {
            messageTemplate.to = params.email;
        }

        showComposeMessageCommon(messageTemplate, action);

        bindOnMessageChanged();

        if (params.files)
            AttachmentManager.SelectDocuments({}, { data: params.files, asLink: false });

        mailBox.stickActionMenuToTheTop();
    }

    function sanitizeMessage(message, loadImages) {
        if (!message.sanitized &&
            !message.textBodyOnly &&
            message.hasOwnProperty("htmlBody") &&
            message.htmlBody.length > 0 &&
            message.folder !== TMMail.sysfolders.drafts.id &&
            message.folder !== TMMail.sysfolders.templates.id &&
            !ASC.Mail.Utility.IsEqualEmail(ASC.Mail.Constants.MAIL_DAEMON_EMAIL, message.from)) {

            var res = ASC.Mail.Sanitizer.Sanitize(message.htmlBody, {
                urlProxyHandler: ASC.Mail.Constants.PROXY_HTTP_URL,
                needProxyHttp: ASC.Mail.Constants.NEED_PROXY_HTTP_URL,
                loadImages: loadImages
            });

            message.htmlBody = res.html;
            message.sanitized = res.sanitized;
            message.contentIsBlocked = res.imagesBlocked;

            if (!res.sanitized && res.html.length > 0)
                message.isBodyCorrupted = true;
        }
    }

    function onGetMailMessage(params, message) {
        sanitizeMessage(message, params.loadImages);

        var originalMessage = $.extend({}, message); // Copy message
        message.conversation_message = params.conversation_message;
        if (!params.conversation_message) {
            closeMessagePanel();
            mailBox.hidePages();
            $('#itemContainer').height('auto');
            mailBox.hideContentDivs();
            if (!TMMail.pageIs('composenewmessage')) {
                mailBox.unmarkAllPanels(false);

                if (message.folder !== TMMail.sysfolders.userfolder.id) {
                    folderPanel.markFolder(message.folder);
                } else {
                    userFoldersPanel.markFolder(message.userFolderId);
                }
            }
        }
        if (params.action == 'reply' || 'forward' == params.action || 'replyAll' == params.action) {
            mailBox.currentMessageId = 0;
        }
        mailBox.hideLoadingMask();

        if (MailFilter.getFolder() == undefined) {
            MailFilter.setFolder(message.folder);
        }

        preprocessMessages(params.action, [message], message.folder);

        var html;
        if (isComposeAction(params.action)) {
            message.original = originalMessage;
            message.original.preprocessAddresses = preprocessAddresses;
            if ('forward' == params.action) {
                message.to = "";

                if (message.subject.indexOf(window.MailScriptResource.ForwardSubjectPrefix) !== 0) {
                    message.subject = window.MailScriptResource.ForwardSubjectPrefix + ": " + message.subject;
                }

            } else if (('reply' == params.action || 'replyAll' == params.action) &&
                message.subject && message.subject.search(new RegExp(window.MailScriptResource.ReplySubjectPrefix, 'i')) !== 0) {
                message.subject = window.MailScriptResource.ReplySubjectPrefix + ": " + message.subject;
            }

            var editMessagePageHeaderHtml = $.tmpl('editMessagePageHeaderTmpl', message, { fileSizeToStr: AttachmentManager.GetSizeString });
            $('#pageActionContainer').append(editMessagePageHeaderHtml);

            var writeMessageHtml = $.tmpl('writeMessageTmpl', message, { fileSizeToStr: AttachmentManager.GetSizeString });
            var editMessageContainer = $('#itemContainer').find('#editMessagePage');
            if (editMessageContainer.length > 0) {
                editMessageContainer.replaceWith(writeMessageHtml);
            } else {
                $('#itemContainer').append(writeMessageHtml);
            }
            releaseSavingLock();
        } else {
            if (!params.notRememberContent) {
                originalMessage.expanded = false;
                mailCache.updateMessage(message.id, originalMessage);
            }

            $('#itemContainer').find('.full-view .from').bind('click', function () {
                messagePage.setToEmailAddresses([message.from]);
                messagePage.composeTo();
            });

            if (!params.conversation_message) {
                var data = {
                    messages: [message],
                    folder: message.folder,
                    last_message: message,
                    important: message.important,
                    fileSizeToStr: AttachmentManager.GetSizeString,
                    cutFileName: AttachmentManager.CutFileName,
                    getFileNameWithoutExt: AttachmentManager.GetFileName,
                    getFileExtension: AttachmentManager.GetFileExtension,
                    htmlEncode: TMMail.htmlEncode,
                    asc: false,
                    action: params.action,
                    wordWrap: TMMail.wordWrap,
                    hasLinked: hasLinked
                };

                var actionHtml = $.tmpl("messageActionTmpl", null, data);

                $('#pageActionContainer').append(actionHtml);

                $('#pageActionContainer').find('.viewTitle').dotdotdot({ wrap: 'letter', height: 20, watch: 'window' });

                html = $.tmpl("messageTmpl", null, data);
                
                $('#itemContainer').append(html);

                TMMail.scrollTop();
            } else {
                var $messageBody = $('#itemContainer .itemWrapper .body[message_id=' + message.id + ']');

                if ($messageBody.length > 0) {
                    var $fullView = $messageBody.parent();

                    if ($fullView.length > 0) {
                        if ($fullView.children('.error-popup').length < 1 && message.contentIsBlocked) {
                            message.sender_address = ASC.Mail.Utility.ParseAddress(message.from).email;
                            if (!trustedAddresses.isTrusted(message.sender_address)) {
                                $messageBody.before($.tmpl("messageBlockContent", message, {}));
                            }
                        }
                    }
                }
            }

        }

        if (!commonSettingsPage.isConversationsEnabled() &&
            MailFilter.getFolder() === TMMail.sysfolders.inbox.id &&
            !params.notRememberContent) {
            var messagesAllRead = $.extend(true, [], [message]);
            $.each(messagesAllRead, function(i, m) { m.wasNew = false });
            var content = $.extend(true, {}, { messages: messagesAllRead, hasLinked: hasLinked });
            mailCache.set(message.id, content);
        }

        showMessagesCommon([message], params.action);
        if (isComposeAction(params.action)) {
            showComposeMessageCommon(message, params.action);
        } else {
            showMessageCommon(message, params.action);
            setMenuActionButtons(message.sender_address);
            updateAttachmentsActionMenu();
        }

        initImageZoom();

        if (!params.conversation_message) {
            var itemContainer = $('#itemContainer');
            itemContainer.find('.full-view[message_id="' + message.id + '"]').attr('is_single', true);
            itemContainer.find('.full-view .head').actionMenu('messageActionMenu', messageMenuItems, pretreatmentConversationMessage);
            itemContainer.find('.message-wrap .short-view').actionMenu('messageActionMenu', messageMenuItems, pretreatmentConversationMessage);
            checkMessagesSender([message]);
        }
        else if (params.checkSender) {
            checkMessagesSender([message]);
        }

        $('.header-crm-link').unbind('click').bind('click', crmLinkPopup.showCrmLinkConversationPopup);

        tuneNextPrev();

        bindOnMessageChanged();
    }

    function onGetMailConversation(params, messages) {
        var important = false;
        var folderId = TMMail.sysfolders.inbox.id;
        var lastMessage = null;
        var needRemember = true;

        $.each(messages, function (i, m) {
            var senderAddress = ASC.Mail.Utility.ParseAddress(m.from);
            var loadImages = senderAddress.isValid ? trustedAddresses.isTrusted(senderAddress.email) : params.loadImages;

            sanitizeMessage(m, loadImages);

            if (isMessageExpanded(m) && m.isBodyCorrupted) {
                needRemember = false;
            }

            important |= m.important;

            if (m.id == mailBox.currentMessageId) {
                folderId = m.folder;
                lastMessage = m;
            }
        });

        if (params.notRememberContent) {
            needRemember = false;
        }

        if (lastMessage == null) {
            return;
        }

        if (needRemember) {
            var messagesAllRead = $.extend(true, [], messages);
            $.each(messagesAllRead, function(i, m) { m.wasNew = false });
            var content = $.extend(true, {}, { messages: messagesAllRead, hasLinked: hasLinked });
            mailCache.set(params.id, content);
        }

        closeMessagePanel();
        mailBox.hidePages();
        $('#itemContainer').height('auto');
        mailBox.hideContentDivs();
        mailBox.hideLoadingMask();

        if (messagePage.conversation_moved) {
            TMMail.showCompleteActionHint(TMMail.action_types.move, true, 1, messagePage.dstFolderType, messagePage.dstUserFolderId);
            messagePage.conversation_moved = false;
            messagePage.dstFolderType = 0;
            messagePage.dstUserFolderId = 0;
        }

        if (messagePage.conversation_deleted) {
            TMMail.showCompleteActionHint(TMMail.action_types.delete_messages, true, 1);
            messagePage.conversation_deleted = false;
        }

        mailBox.unmarkAllPanels(false);

        if (folderId !== TMMail.sysfolders.userfolder.id) {
            folderPanel.markFolder(folderId);
        } else {
            userFoldersPanel.markFolder(lastMessage.userFolderId);
        }

        if (!isSortConversationByAsc()) {
            messages.reverse();
        }

        preprocessMessages(params.action, messages, folderId);

        var data = {
            messages: messages,
            folder: folderId,
            last_message: lastMessage,
            important: important,
            maxShowCount: maxConversationShow,
            fileSizeToStr: AttachmentManager.GetSizeString,
            cutFileName: AttachmentManager.CutFileName,
            getFileNameWithoutExt: AttachmentManager.GetFileName,
            getFileExtension: AttachmentManager.GetFileExtension,
            htmlEncode: TMMail.htmlEncode,
            wordWrap: TMMail.wordWrap,
            hasLinked: hasLinked
        };

        var actionHtml = $.tmpl("messageActionTmpl", null, data);

        $('#pageActionContainer').append(actionHtml);

        $("#pageActionContainer").find(".viewTitle").dotdotdot({ wrap: "letter", height: 20, watch: 'window' });

        var html = $.tmpl('messageTmpl', null, data);

        $("#itemContainer").append(html);

        TMMail.scrollTop();

        showMessagesCommon(messages, params.action);

        var verifiableMessages = [];
        $.each(messages, function(index, message) {
            showMessageCommon(message, params.action);

            if (isMessageExpanded(message)) {
                verifiableMessages.push(message);
            }
        });

        checkMessagesSender(verifiableMessages);

        if ('draft' == params.action || 'template' == params.action || 'reply' == params.action || 'forward' == params.action) {
            setEditMessageButtons();
        } else {
            setMenuActionButtons(messages.length == 1 ? ASC.Mail.Utility.ParseAddress(messages[0].from).email : undefined);
            setConversationViewActions();
        }

        updateAttachmentsActionMenu();

        var itemContainer = $('#itemContainer');

        if (1 == messages.length) {
            itemContainer.find('.full-view[message_id="' + messages[0].id + '"]').attr('is_single', true);
        }

        itemContainer.find(".full-view .head").actionMenu('messageActionMenu', messageMenuItems, pretreatmentConversationMessage);
        itemContainer.find('.message-wrap .short-view').actionMenu('messageActionMenu', messageMenuItems, pretreatmentConversationMessage);

        $('.header-crm-link').unbind('click').bind('click', crmLinkPopup.showCrmLinkConversationPopup);
        tuneNextPrev();

        jq(".page-content").off("scroll").on("scroll", function () {
            var height = jq(".page-content").scrollTop();
            $("#MessageGroupButtons .menu-action-on-top").toggle(height > 0);
        });
    }

    function updateAttachmentsActionMenu() {
        $('#itemContainer').find('.attachments').actionMenu('attachmentActionMenu', attachmentMenuItems, pretreatmentAttachments);
        bindAllAttachmentsCommonActions();
    }

    function updateEditAttachmentsActionMenu() {
        $('#mail_attachments').actionMenu('attachmentEditActionMenu', attachmentEditMenuItems, pretreatmentAttachments);
    }

    function displayTrustedImages(message, forcibly) {
        if (forcibly != undefined) {
            if (forcibly) {
                displayImages(message.id);
            }
            return;
        }

        if (message.contentIsBlocked) {
            var senderAddress = ASC.Mail.Utility.ParseAddress(message.from).email;
            if (trustedAddresses.isTrusted(senderAddress)) {
                displayImages(message.id);
            } else if ($('#id_block_content_popup_' + message.id).length == 0) {
                // Conversation sort algorithm: user has clicked the 'Display images' and #id_block_content_popup has been removed;
                displayImages(message.id);
            } else {
                $('#id_block_content_popup_' + message.id).show();
            }
        }
    }

    function initImageZoom() {
        window.StudioManager.initImageZoom();
    }

    function checkMessagesSender(messages) {
        var addresses = [];
        var i, len;
        for (i = 0, len = messages.length; i < len; i++) {
            if (messages[i].folder == TMMail.sysfolders.inbox.id) {
                var address = ASC.Mail.Utility.ParseAddress(messages[i].from).email;
                if (addresses.indexOf(address) === -1)
                    addresses.push(address);
            }
        }

        for (i = 0, len = addresses.length; i < len; i++) {
            var email = addresses[i];
            if (ASC.Mail.Constants.CRM_AVAILABLE) {
                contactsManager.inCrmContacts(email).done(function(result) {
                    updateMessagesSender(result.email, 'is_crm', result.exists);
                });
            }
            contactsManager.inPersonalContacts(email).done(function(result) {
                updateMessagesSender(result.email, 'is_personal', result.exists);
            });

        }
    }

    function updateMessagesSender(address, attribute, value) {
        var messages = $('#itemContainer').find('.message-wrap');
        for (var i = 0, len = messages.length; i < len; i++) {
            if ($(messages[i]).attr('folder') == TMMail.sysfolders.inbox.id) {
                var from = ASC.Mail.Utility.ParseAddress($(messages[i]).find('.full-view .from').text()).email;
                if (from == address) {
                    $(messages[i]).find('.full-view').attr(attribute, value);
                }
            }
        }
    }

    function onCreateMailContact(params, contact) {
        $.each(contact.emails, function (i, v) {
            updateMessagesSender(v.value, 'is_personal', true);
        });
    }

    function isMessageExpanded(message) {
        return message.wasNew || "undefined" != typeof message.htmlBody || (!MailFilter.isBlank() && messageMatchFilter(message));
    }

    function preprocessMessages(action, messages, folder) {
        var index, len, hiddenCount = 0;
        var wasNewFlag = false;
        for (index = 0, len = messages.length; index < len; index++) {
            var message = messages[index];

            message.subject = message.subject || "";

            wasNewFlag |= message.wasNew;

            if (!message.hasOwnProperty("expanded"))
                message.expanded = isMessageExpanded(message);

            var lastOrFirst = (0 == index || messages.length - 1 == index);
            var nextExpanded = messages.length > index + 1 && isMessageExpanded(messages[index + 1]);
            var prevExpanded = 0 != index && messages[index - 1].expanded;

            if (message.expanded || lastOrFirst || prevExpanded || nextExpanded) {
                message.visible = true;
                if (hiddenCount == 1) {
                    messages[index - 1].visible = true;
                    hiddenCount = 0;
                }
                message.hidden_count = hiddenCount;
                hiddenCount = 0;
            } else {
                message.visible = false;
                message.hidden_count = 0;
                hiddenCount += 1;
            }

            var addr = ASC.Mail.Utility.ParseAddress(message.from);
            message.fromName = addr.isValid ? (addr.name || addr.email) : message.from;

            message.displayDate = window.ServiceFactory.getDisplayDate(window.ServiceFactory.serializeDate(message.receivedDate));
            message.displayTime = window.ServiceFactory.getDisplayTime(window.ServiceFactory.serializeDate(message.receivedDate));

            switch (action) {
                case 'reply':
                case 'replyAll':
                    message.isAnswered = true;
                    break;
                case 'forward':
                    message.isForwarded = true;
                    message.cc = "";
                    message.bcc = "";
                    break;
            }

            if (action === "reply" || action === "forward" || action === "replyAll") {
                message.mimeReplyToId = message.mimeMessageId;
                message.mimeMessageId = "";
                if (message.folder !== TMMail.sysfolders.sent.id) {
                    var receiverAddress = ASC.Mail.Utility.ParseAddress(message.address),
                        receiverAccount = accountsManager.getAccountByAddress(receiverAddress.email),
                        toAddress = ASC.Mail.Utility.ParseAddress(message.to),
                        toAccount = accountsManager.getAccountByAddress(toAddress.email),
                        toEqualsReceiver = toAddress.EqualsByEmail(receiverAddress);

                    if (action === "replyAll") {
                        var emails = toEqualsReceiver
                            ? []
                            : (!toAccount || toAccount.mailbox_id !== receiverAccount.mailbox_id)
                                ? ASC.Mail.Utility.ParseAddresses(message.to).addresses
                                : [];

                        if (message.cc)
                            emails = emails.concat(ASC.Mail.Utility.ParseAddresses(message.cc).addresses);

                        emails = $.map(emails, function (e) {
                            return (receiverAddress.isValid && receiverAddress.EqualsByEmail(e)) ?
                                undefined :
                                e.ToString();
                        });
                        message.cc = emails.join(", ");
                    } else {
                        message.cc = "";
                    }

                    var account = toEqualsReceiver
                                    ? receiverAccount
                                    : toAccount || receiverAccount;

                    if (!account) {
                        var def = accountsManager.getDefaultAccount();
                        if (def)
                            account = def.email;
                    }

                    var to;
                    if (message.replyTo) {
                        var replyTo = ASC.Mail.Utility.ParseAddress(message.replyTo);
                        var from = ASC.Mail.Utility.ParseAddress(message.from);

                        if (replyTo.EqualsByEmail(from) && !replyTo.name && from.name) {
                            to = message.from;
                        } else {
                            to = message.replyTo;
                        }
                    } else {
                        to = message.from;
                    }

                    message.to = to;
                    message.from = (account && !account.is_group) ? (new ASC.Mail.Address(account.name, account.email, true)).ToString() : message.address;
                }

                message.id = 0;
            } else {
                action = 'view';
                var toyou = true;
                message.from = preprocessAddresses(message.from);
                if (message.from.indexOf(message.address) !== -1) {
                    toyou = false;
                }
                message.to = preprocessAddresses(message.to);
                if (message.to.indexOf(message.address) !== -1) {
                    toyou = false;
                }
                if (message.cc !== "") {
                    message.cc = preprocessAddresses(message.cc);
                    if (message.cc.indexOf(message.address) !== -1) {
                        toyou = false;
                    }
                }
                if (message.bcc !== "") {
                    message.bcc = preprocessAddresses(message.bcc);
                    if (message.folder == TMMail.sysfolders.sent.id ||
                        message.folder == TMMail.sysfolders.drafts.id ||
                        message.folder == TMMail.sysfolders.templates.id ||
                        message.restoreFolderId == TMMail.sysfolders.sent.id) {
                        toyou = false;
                    } else {
                        message.bcc = "";
                    }
                }

                message.toyou = toyou;
            }

            message.template_name = message.folder == TMMail.sysfolders.drafts.id == TMMail.sysfolders.templates.id ? "editMessageTmpl" : "messageShortTmpl";
            message.sender_address = ASC.Mail.Utility.ParseAddress(message.from).email;
            message.full_size = 0;

            if (message.hasAttachments) {
                var i, count;
                for (i = 0, count = message.attachments.length; i < count; i++) {
                    message.full_size += message.attachments[i].size;
                    AttachmentManager.CompleteAttachment(message.attachments[i]);

                }
                message.download_all_url = "";
            }
        }

        if (wasNewFlag) {
            switch (folder) {
                case TMMail.sysfolders.sent.id:
                    serviceManager.updateFolders();
                    break;
                case TMMail.sysfolders.inbox.id:
                case TMMail.sysfolders.spam.id:
                    folderPanel.decrementUnreadCount(folder);
                    break;
                case TMMail.sysfolders.userfolder.id:
                    userFoldersPanel.decrementUnreadCount();
                    break;
                default:
                    break;
            }
        }
    }

    // check message meets for filter conditions

    function messageMatchFilter(message) {
        if (true === MailFilter.getImportance() && !message.important) {
            return false;
        }

        if (true === MailFilter.getUnread() && !message.wasNew) {
            return false;
        }

        if (true === MailFilter.getAttachments() && !message.hasAttachments) {
            return false;
        }

        var period = MailFilter.getPeriod();
        if (0 != period.from && 0 != period.to && (message.date < period.from || message.date > period.to)) {
            return false;
        }

        var tags = MailFilter.getTags();
        if (tags.length > 0) {
            if (0 == message.tagIds.length) {
                return false;
            }
            for (var i = 0, len = tags.length; i < len; i++) {
                if (-1 == $.inArray(tags[i], message.tagIds)) {
                    return false;
                }
            }
        }

        var search = MailFilter.getSearch();
        if ('' != search) {
            var messageText = message.from + ' ' + message.to + ' ' + message.subject;
            if (-1 == messageText.toLowerCase().indexOf(search.toLowerCase())) {
                return false;
            }
        }

        if (MailFilter.getTo() && -1 == message.to.toLowerCase().indexOf(MailFilter.getTo().toLowerCase())) {
            return false;
        }

        if (MailFilter.getFrom() && -1 == message.from.toLowerCase().indexOf(MailFilter.getFrom().toLowerCase())) {
            return false;
        }

        return true;
    }

    function preprocessAddresses(addressStr) {
        var addresses = ASC.Mail.Utility.ParseAddresses(addressStr).addresses;
        var newAddresses = [];
        for (var i = 0, len = addresses.length; i < len; i++) {
            if (addresses[i].isValid && !addresses[i].name) {
                var contact = contactsManager.getTLContactsByEmail(addresses[i].email);
                if (contact && contact.displayName) {
                    addresses[i].name = contact.displayName;
                }

                if (!addresses[i].name) {
                    var account = accountsManager.getAccountByAddress(addresses[i].email);
                    if (account && account.name) {
                        addresses[i].name = account.name;
                    }
                }
            }

            newAddresses.push(addresses[i].ToString(addresses[i].isValid ? false : (addresses[i].name.indexOf(",") !== -1 ? false : true)));
        }
        return newAddresses.join(", ");
    }

    function expandConversation(messageId) {
        var shortView = $('#itemContainer .itemWrapper .short-view[message_id=' + messageId + ']');

        if (!shortView) {
            return;
        }

        var parentWrap = shortView.closest('.message-wrap'),
            fullView = parentWrap.find('.full-view');

        shortView.removeClass('loading');
        shortView.toggleClass('new', false);
        fullView.attr('loaded', true);
        fullView.show();
        shortView.hide();

        $('#collapse-conversation').html(($('.full-view:hidden').length > 0) ? window.MailScriptResource.ExpandAllLabel : window.MailScriptResource.CollapseAllLabel);
    }

    function collapseConversation(messageId) {
        var fullView = $('#itemContainer .itemWrapper .full-view[message_id=' + messageId + ']');

        if (!fullView) {
            return;
        }

        var parentWrap = fullView.closest('.message-wrap'),
            shortView = parentWrap.find('.short-view'),
            loader = shortView.find('.loader');

        loader.hide();
        shortView.show();
        fullView.hide();

        $('#collapse-conversation').html(($('.full-view:hidden').length > 0) ? window.MailScriptResource.ExpandAllLabel : window.MailScriptResource.CollapseAllLabel);
    }

    // expand collapsed in "N more" panel messages rows

    function showCollapsedMessages() {
        $('.collapsed-messages').hide();

        $.each($('.message-wrap:hidden'), function (index, value) {
            var messageWrap = $(value);
            if (messageWrap != undefined) {
                messageWrap.show();
            }
        });
    }

    function toggleImportance(el) {
        var icon = el.find('[class^="icon-"], [class*=" icon-"]');
        var newimportance = icon.is('.icon-unimportant');
        icon.toggleClass('icon-unimportant').toggleClass('icon-important');

        var title;
        if (newimportance) {
            title = MailScriptResource.ImportantLabel;
        } else {
            title = MailScriptResource.NotImportantLabel;
        }
        icon.attr('title', title);

        return newimportance;
    }

    function showMessagesCommon(messages, action) {
        if ('view' !== action && 'conversation' !== action) {
            initMessagePanel(messages[0], action);
            return;
        }
        $('#pageActionContainer .head-subject .importance').unbind('click').click(function () {
            var newimportance = toggleImportance($(this));

            if (TMMail.pageIs('conversation')) {
                var messageId = getActualConversationLastMessageId();

                if (commonSettingsPage.isConversationsEnabled()) {
                    mailBox.updateConversationImportance(messageId, newimportance);
                } else {
                    mailBox.updateMessageImportance(messageId, newimportance);
                }

                var conversationMessages = mailBox.getConversationMessages();
                var i, len = conversationMessages.length;
                for (i = 0; i < len; i++) {
                    messageId = $(conversationMessages[i]).attr('message_id');
                    mailBox.setImportanceInCache(messageId);
                }

            } else {
                messageId = mailBox.currentMessageId;
                mailBox.updateMessageImportance(messageId, newimportance);
                mailBox.setImportanceInCache(messageId);
            }
        });

        $('#itemContainer').find('.full-view .from').bind('click', function () {
            messagePage.setToEmailAddresses([$(this).text()]);
            messagePage.composeTo();
        });

        if ('conversation' === action) {
            $('.collapsed-messages').click(function () {
                showCollapsedMessages();
            });
        }

        $.each(messages, function (i, v) {
            if (v.expanded && 'undefined' !== typeof v.htmlBody) {
                expandConversation(v.id);
            }
        });

        if (messages.length > 1) {
            tuneFullView();
        }

        title = isSortConversationByAsc() ? messages[0].subject : messages[messages.length - 1].subject;
        title = title || window.MailScriptResource.NoSubject;
        TMMail.setPageHeaderTitle(title);
        TMMail.resizeContent();

    }

    function tuneFullView() {
        var parent = $('#itemContainer .itemWrapper'),
            fullView = parent.find('.full-view .head');

        parent.find('.short-view').unbind('click').click(function () {
            var shortView = $(this);
            var messageId = shortView.attr('message_id');

            if (typeof messageId != 'undefined') {
                var parentWrap = shortView.closest('.message-wrap'),
                    fullView = parentWrap.find('.full-view'),
                    loader = shortView.find('.loader');

                if (!fullView.attr('loaded')) {
                    showConversationMessage(messageId, false, true);
                    shortView.addClass('loading');
                    loader.show();
                } else {
                    expandConversation(messageId);
                }
            }
        });

        fullView.addClass('pointer');
        fullView.unbind('click').click(function (e) {
            if ('' != ASC.Mail.getSelectionText()) {
                return;
            }
            var el = $(e.target);
            if (el.is('.entity-menu') || el.is('.from') || el.is('.tag') || el.is('.tagDelete') || el.closest(".calendarView").length > 0) {
                return;
            }

            var messageId = $(this).parent().attr('message_id');

            if (typeof messageId != 'undefined') {
                collapseConversation(messageId);
            }
        });
    }

    function tuneNextPrev() {
        var anchor = ASC.Controls.AnchorController.getAnchor();
        var head = $('#MessageGroupButtons');
        head.show();
        var prevBtn = head.find(".btnPrev");
        var nextBtn = head.find(".btnNext");
        var cache = filterCache.getCache(MailFilter);

        if (mailBox.currentMessageId == cache.first) {
            prevBtn.toggleClass("unlockAction", false);
        } else {
            prevBtn.toggleClass("unlockAction", true);
        }

        if (mailBox.currentMessageId == cache.last) {
            nextBtn.toggleClass("unlockAction", false);
        } else {
            nextBtn.toggleClass("unlockAction", true);
        }

        prevBtn.off('click').on('click', function () {
            if (!$(this).hasClass("unlockAction"))
                return;

            ASC.Controls.AnchorController.move('#' + anchor + '/prev');
        });

        nextBtn.off('click').on('click', function () {
            if (!$(this).hasClass("unlockAction"))
                return;

            ASC.Controls.AnchorController.move('#' + anchor + '/next');
        });
    }

    function isComposeAction(action) {
        return action == 'reply' || action == 'replyAll' || action == 'forward' || action == 'draft' || action == 'template';
    }

    function setComposeFocus(message) {
        //Set focus to 1st empty field

        var $input = setFocusForMobile(message);

        wysiwygEditor.unbind(wysiwygEditor.events.OnFocus).bind(wysiwygEditor.events.OnFocus, function () {
            wysiwygEditor.unbind(wysiwygEditor.events.OnFocus);
            if ($input)
                $input.focus();
        });
    }

    function setFocusForMobile(message) {
        var $input = null;

        if (message.to === "" || jq.isArray(message.to) && message.to.length === 0) {
            $input = $('#newmessageTo .emailselector-input');
        } else if ($('#newmessageCopy:visible').length > 0 && (message.cc === "" || jq.isArray(message.cc) && message.cc.length === 0)) {
            $input = $('#newmessageCopy .emailselector-input');
        } else if ($('#newmessageBCC:visible').length > 0 && (message.bcc === "" || jq.isArray(message.bcc) && message.bcc.length === 0)) {
            $input = $('#newmessageBCC .emailselector-input');
        } else if ($('#newmessageSubject:visible').length > 0 && message.subject === "") {
            $input = $('#newmessageSubject');
        }

        if (!$input) return null;

        $input.focus();
        $input.css('cursor', 'pointer').click();
        $input.css('cursor', 'text');

        return $input;
    }

    function showComposeMessageCommon(message, action) {
        var title;
        switch (action) {
            case "reply":
            case "replyAll":
                title = window.MailScriptResource.PageHeaderReply;
                break;
            case "forward":
                title = window.MailScriptResource.PageHeaderForward;
                break;
            case "draft":
                title = window.MailScriptResource.PageHeaderDraft;
                break;
            case "template":
                title = window.MailScriptResource.PageHeaderTemplate;
                break;
            case "compose":
                title = window.MailScriptResource.NewMessage;
                break;
            default:
                // ToDo: handle unknown action here
                return;
        }
        TMMail.setPageHeaderTitle(title);
        setWysiwygEditorValue(message, action);
        $("#editMessagePage").show();
        setEditMessageButtons();
        jq("#newmessageTo").AdvancedEmailSelector("init", {
            isInPopup: false,
            items: action !== "forward" ? message.to : "",
            onChangeCallback: function () {
                $('#newmessageTo').removeClass('invalidField');
                onMessageChanged();
            }
        });
        jq("#newmessageCopy").AdvancedEmailSelector("init", {
            isInPopup: false,
            items: message.cc,
            onChangeCallback: function () {
                $('#newmessageCopy').removeClass('invalidField');
                onMessageChanged();
            }
        });
        jq("#newmessageBCC").AdvancedEmailSelector("init", {
            isInPopup: false,
            items: message.bcc,
            onChangeCallback: function () {
                $('#newmessageBCC').removeClass('invalidField');
                onMessageChanged();
            }
        });
        if (message.cc.length > 0 || message.bcc.length > 0) {
            $(".value-group.cc").show();
            $(".value-group.bcc").show();
            $("#AddCopy:visible").remove();
        }

        $("#newmessageTo").trigger("input");
        $('#newmessageCopy').trigger('input');
        $('#newmessageBCC').trigger('input');

        setEditingMessage(message);

        setComposeFocus(message);   
    }

    function showMessageCommon(message) {
        insertBody(message);
        updateMessageTags(message);
        initBlockContent(message);
        initImageZoom();
    }

    function pretreatmentConversationMessage(id, dropdownItemId) {
        var senderAddress = ASC.Mail.Utility.ParseAddress(getFromAddress(id)).email;
        var message = $('#itemContainer').find('.full-view[message_id="' + id + '"]');
        var dropdownEl = $("#" + dropdownItemId);
        var menuHideImages = dropdownEl.find(".alwaysHideImages");
        var menuViewMessage = dropdownEl.find(".singleViewMail");
        var singleViewMailSeporator = dropdownEl.find(".singleViewMailSeporator");
        var menuPersonalContact = dropdownEl.find(".createPersonalContact");
        var menuCrmPerson = dropdownEl.find(".createCrmPerson");
        var menuCrmCompany = dropdownEl.find(".createCrmCompany");

        if (menuHideImages.length == 1) {
            if ($('#id_block_content_popup_' + id).length > 0) {
                menuHideImages.hide();
            } else {
                if (!commonSettingsPage.AlwaysDisplayImages() && trustedAddresses.isTrusted(senderAddress)) {
                    menuHideImages.text(window.MailScriptResource.HideImagesLabel + ' "' + senderAddress + '"');
                    menuHideImages.show();
                } else {
                    menuHideImages.hide();
                }
            }
        }

        if (message.attr('is_single') == 'true') {
            menuViewMessage.hide();
            singleViewMailSeporator.hide();
        } else {
            menuViewMessage.show();
            singleViewMailSeporator.show();
        }

        if (accountsManager.getAccountByAddress(senderAddress)) {
            menuCrmPerson.hide();
            menuCrmCompany.hide();
            menuPersonalContact.hide();
        } else {
            if (message.attr('is_crm') == 'true') {
                menuCrmPerson.hide();
                menuCrmCompany.hide();
            } else {
                menuCrmPerson.show();
                menuCrmCompany.show();
            }

            if (message.attr('is_personal') == 'true') {
                menuPersonalContact.hide();
            } else {
                menuPersonalContact.show();
            }
        }

    }

    function pretreatmentAttachments(id, dropdownItemId) {
        var dropdownItem = $("#" + dropdownItemId);
        if (dropdownItem.length == 0) {
            return;
        }

        var viewMenu = dropdownItem.find(".viewAttachment");
        var editMenu = dropdownItem.find(".editAttachment");
        var downloadMenu = dropdownItem.find(".downloadAttachment");
        var deleteMenu = dropdownItem.find(".deleteAttachment");
        var saveToDocsMenu = dropdownItem.find(".saveAttachmentToDocs");
        var saveToCalnedarMenu = dropdownItem.find(".saveAttachmentToCalendar");

        var menu = $('#itemContainer .attachments_list .entity-menu[data_id="' + id + '"]');
        var name = menu.attr('name');

        if (TMMail.pageIs('writemessage')) {
            deleteMenu.show();
        }

        if (TMMail.pageIs('writemessage')) {
            var attachment = AttachmentManager.GetAttachment(id);
            if (!attachment || (attachment.fileId <= 0 && !attachment.tempStoredUrl)) {
                downloadMenu.hide();
                viewMenu.hide();
                editMenu.hide();
            } else if (attachment.tempStoredUrl) {
                downloadMenu.show();
                if (attachment.canView || attachment.isImage || attachment.isMedia || attachment.isCalendar) {
                    viewMenu.show();
                } else {
                    viewMenu.hide();
                }
                editMenu.hide();
            } else {
                downloadMenu.show();
                editMenu.hide(); // No edit document in compose/draft/reply/replyAll/forward where delete_button is visible

                if (attachment.canView || attachment.isImage || attachment.isMedia || attachment.isCalendar) {
                    viewMenu.show();
                } else {
                    viewMenu.hide();
                }
            }
        } else {
            downloadMenu.show();
            saveToDocsMenu.show();

            if (ASC.Mail.Constants.CALENDAR_AVAILABLE &&
                TMMail.canViewAsCalendar(name)) {
                saveToCalnedarMenu.show();
            } else {
                saveToCalnedarMenu.hide();
            }

            if (TMMail.canViewInDocuments(name) || ASC.Files.Utility.CanImageView(name) || ASC.Files.MediaPlayer.canPlay(name) || TMMail.canViewAsCalendar(name)) {
                viewMenu.show();
            } else {
                viewMenu.hide();
            }

            if (!TMMail.canEditInDocuments(name)) {
                editMenu.hide();
            } else {
                editMenu.show();
            }
        }
    }

    function saveAttachmentToCalendar(id) {
        var fileName = $('#itemContainer .attachments_list .row[data_id="' + id + '"] .file-name').text();
        mailCalendar.exportAttachmentToCalendar(id, fileName);
    }

    function saveAttachmentToDocs(id) {
        var attachmentName = $('#itemContainer .attachments_list .row[data_id="' + id + '"] .file-name').text();

        var showFileSelector = function () {
            ASC.Files.FileSelector.onSubmit = function (folderId) {
                var folderName = jq("#fileSelectorTree .node-selected a[data-id='" + folderId + "']").prop("title");
                serviceManager.exportAttachmentToDocuments(id, folderId,
                    {fileName: attachmentName, folderName: folderName},
                    {
                        success: function (params) {
                            window.toastr.success(
                                window.MailScriptResource.SaveAttachmentToDocsSuccess
                                    .replace('%file_name%', params.fileName)
                                    .replace('%folder_name%', params.folderName));
                        },
                        error: onErrorExportAttachmentsToMyDocuments
                    },
                    ASC.Resources.Master.Resource.LoadingProcessing);
            };

            $('#filesFolderUnlinkButton').hide();

            ASC.Files.FileSelector.fileSelectorTree.resetFolder();
            ASC.Files.FileSelector.openDialog(null, true);
        };

        if (ASC.Files.Utility.MustConvert(attachmentName)) {
            ASC.Files.ConfrimConvert.showDialog(showFileSelector, "", true);
        } else {
            showFileSelector();
        }
    }

    function editDocumentAttachment(id) {
        var attachmentName = $('#itemContainer .attachments_list .row[data_id="' + id + '"] .file-name').text();

        var openDocumentPage = function () {
            window.open(TMMail.getEditDocumentUrl(id), '_blank');
        };

        if (ASC.Files.Utility.MustConvert(attachmentName)) {
            ASC.Files.ConfrimConvert.showDialog(openDocumentPage);
        } else {
            openDocumentPage();
        }
    }

    function downloadAttachment(id) {
        var url = '';

        if (TMMail.pageIs('writemessage')) {
            var attachment = AttachmentManager.GetAttachment(id);
            if (!attachment) return;

            url = attachment.tempStoredUrl || TMMail.getAttachmentDownloadUrl(attachment.fileId);
        } else {
            url = TMMail.getAttachmentDownloadUrl(id);
        }
        window.open(url, 'Download');
    }

    function viewAttachment(id) {
        var url = '';
        var name = $('#itemContainer .attachments_list .entity-menu[data_id="' + id + '"]').attr('name');

        if (ASC.Files.Utility.CanImageView(name) || ASC.Files.MediaPlayer.canPlay(name)) {
            $('#itemContainer .attachments_list .row[data_id="' + id + '"] a').click();
        } else {
            if (TMMail.pageIs('writemessage')) {
                var attachment = AttachmentManager.GetAttachment(id);
                if (!attachment) return;

                url = attachment.tempStoredUrl || TMMail.getAttachmentDownloadUrl(attachment.fileId);
            } else {
                url = TMMail.getAttachmentDownloadUrl(id);
            }

            if (TMMail.canViewAsCalendar(name)) {
                mailCalendar.showCalendarInfo(url, name);
            }
            else {
                if (TMMail.pageIs('writemessage')) {
                    var attachment = AttachmentManager.GetAttachment(id);
                    id = attachment.fileId;
                }

                window.open(TMMail.getViewDocumentUrl(id), '_blank');
            }
        }
    }

    function deleteAttachment(id) {
        var deleteButton = $('#itemContainer .attachments_list .row[data_id="' + id + '"] .delete_attachment');
        if (deleteButton.length > 0) {
            deleteButton.click();
        }
    }

    function createNewCrmCompany(id) {
        createCrmContact('company', id);
    }

    function createNewCrmPerson(id) {
        createCrmContact('people', id);
    }

    function createCrmContact(type, id) {
        var from = getFromAddress(id);
        var addr = ASC.Mail.Utility.ParseAddress(from);
        var url = "../../Products/CRM/Default.aspx?action=manage&type={0}&email={1}&fullname={2}"
            .format(type, encodeURIComponent(addr.email), encodeURIComponent(addr.name));

        var htmlTmpl = $.tmpl('crmLinkConfirmPopupTmpl');
        htmlTmpl.find('.buttons .createAndLink')
            .unbind('click')
            .bind('click', function () {
                window.contactsManager.forgetCrmContact(addr.email);
                url += "&linkMessageId={0}".format(id);
                popup.hide();
                window.open(url, "_blank");
            });

        htmlTmpl.find('.buttons .justCreate')
            .unbind('click')
            .bind('click', function () {
                window.contactsManager.forgetCrmContact(addr.email);
                popup.hide();
                window.open(url, "_blank");
            });

        popup.addBig(window.MailScriptResource.LinkConversationPopupHeader, htmlTmpl);
    }
    
    function createPersonalContact(id) {
        var fromAddress = ASC.Mail.Utility.ParseAddress(getFromAddress(id));
        var contact= {};
        contact.id = -1;
        contact.name = fromAddress.name || "";
        contact.description = "";
        contact.emails = [];
        contact.phones = [];
        contact.emails.push({ id: -1, isPrimary: true, value: fromAddress.email });
        editContactModal.show(contact, true);
    }

    function updateFromAccountField(selectedAccount) {
        if (selectedAccount === undefined) {
            var accounts = accountsManager.getAccountList();

            if (accounts.length != 0) {
                selectedAccount = accounts[0];
            }
            for (var i = 0; i < accounts.length; i++) {
                if (accounts[i].enabled) {
                    selectedAccount = accounts[i];
                    break;
                }
            }
            for (var i = 0; i < accounts.length; i++) {
                if (accounts[i].is_default) {
                    selectedAccount = accounts[i];
                    break;
                }
            }
        }
        if (selectedAccount === undefined || selectedAccount == null) {
            return;
        }

        selectFromAccount({}, {
            account: selectedAccount
        });
    }

    function getTags() {
        var res = [];
        $('#itemContainer .head .tags .value .itemTags .tagDelete').each(function (index, value) {
            res.push(parseInt($(value).attr('tagid')));
        });
        return res;
    }

    function hasTag(tagId) {
        return $('#itemContainer .head:visible .tags .value .itemTags .tagDelete[tagid="' + tagId + '"]').length;
    }

    function getFromAddress(messageId) {
        return $('#itemContainer .message-wrap[message_id="' + messageId + '"] .row .value .from').text();
    }

    function getToAddresses(messageId) {
        return $('#itemContainer .message-wrap[message_id="' + messageId + '"] .to-addresses').text();
    }

    function getMessageFolder(messageId) {
        return $('#itemContainer .message-wrap[message_id="' + messageId + '"]').attr("folder");
    }

    function getCurrentConversationIds() {
        var ids = [];
        if (!TMMail.pageIs('conversation')) {
            return undefined;
        } else {
            var messages = $('#itemContainer').find('.message-wrap');
            for (var i = 0; i < messages.length; i++) {
                ids.push($(messages[i]).attr('message_id'));
            }
        }
        return ids;
    }

    function setToEmailAddresses(emails) {
        toEmailAddresses = emails;
    }

    function bindAllAttachmentsCommonActions() {
        $('#itemContainer .attachments-buttons .exportAttachemntsToMyDocs')
            .unbind('click')
            .bind('click',
                function () {
                    var rootNode = $(this).closest('.attachments');
                    var messageId = rootNode.attr('message_id');
                    var attachmentsCount = rootNode.find('.row').length;
                    serviceManager.exportAllAttachmentsToMyDocuments(messageId, { count: attachmentsCount }, {
                        success: function (params, realCount) {
                            window.toastr.success(
                                window.MailScriptResource.SaveAllAttachmentsToMyDocsSuccess
                                .replace('%real_count%', realCount)
                                .replace('%count%', params.count));
                        },
                        error: onErrorExportAttachmentsToMyDocuments
                    }, ASC.Resources.Master.Resource.LoadingProcessing);
                });

        $('#itemContainer .attachments-buttons .downloadAllAttachments')
            .unbind('click')
            .bind('click',
                function () {
                    var $this = $(this),
                        rootNode = $this.closest('.attachments'),
                        messageId = rootNode.attr('message_id');

                    if ($this.hasClass('gray'))
                        return;

                    $this.addClass('gray');

                    var res = {
                        header: MailApiResource.SetupTenantAndUserHeader,
                        percentage: 0
                    };

                    ProgressDialog.init(res, jq("#bottomLoaderPanel"), null, 1);

                    Teamlab.downloadAttachmentsAll({}, messageId, {
                        success: function (params, data) {

                            ProgressDialog.setProgress(res.percentage, res.header);

                            ProgressDialog.show();

                            progressBarIntervalId = setInterval(function () {
                                return checkDownloadAllAttachmentsStatus(data);
                            },
                            1000);
                        },
                        error: function (params, error) {
                            administrationError.showErrorToastr("downloadAttachmentsAll", error);
                            $this.removeClass('gray');
                        }
                    });
                });
    }

    function translateOperationStatus(percent) {
        if (percent === 1) return MailApiResource.SetupTenantAndUserHeader;
        if (percent === 5) return MailApiResource.GetAttachmentsHeader;
        if (percent >= 10 && percent < 85) return MailApiResource.ZippingAttachmentsHeader;
        if (percent === 85) return MailApiResource.PreparationArchiveHeader;
        if (percent === 90) return MailApiResource.CreatingLinkHeader;
        if (percent === 100) return MailApiResource.FinishedHeader;
    }

    function checkDownloadAllAttachmentsStatus(operation) {
        serviceManager.getMailOperationStatus(operation.id,
        null,
        {
            success: function (params, data) {
                var status = translateOperationStatus(data.percents);

                if (data.completed) {
                    clearInterval(progressBarIntervalId);
                    progressBarIntervalId = null;

                    if (data.source.length) {
                        var link = window.location.origin + data.source;

                        $('#bufferLink').attr('href', link).get(0).click();
                    }

                    if (data.error.length) {
                        toastr.error(data.error, null, {
                            "closeButton": true,
                            "timeOut": "0",
                            "extendedTimeOut": "0"
                        });
                    }

                    ProgressDialog.setProgress(data.percents, status);

                    setTimeout(function() {
                        ProgressDialog.close();
                        $('.downloadAllAttachments').removeClass('gray');
                    }, 1000);
                } else {
                    ProgressDialog.setProgress(data.percents, status);
                }
            },
            error: function (e, error) {
                console.error("checkDownloadAllAttachmentsStatus", e, error);
                clearInterval(progressBarIntervalId);
                progressBarIntervalId = null;

                $('.downloadAllAttachments').removeClass('gray');
            }
        });
    }

    function onErrorExportAttachmentsToMyDocuments() {
        window.toastr.error(window.MailScriptResource.SaveAttachmentsToDocumentsFailure);
    }

    function onMarkChainAsCrmLinked() {
        window.LoadingBanner.hideLoading();
        window.toastr.success(window.MailScriptResource.LinkConversationText);

        if (needCrmLink()) {
            crmContactsInfo = [];
        }

        if (messageIsSending) {
            sendMessage();
        }
    }

    function cancelSending() {
        window.LoadingBanner.hideLoading();
        window.AttachmentManager.Unbind(window.AttachmentManager.CustomEvents.UploadComplete, onAttachmentsUploadComplete);
        resetDirtyMessage();
        TMMail.disableButton($('#editMessagePageHeader .btnSend'), false);
        TMMail.disableButton($('#editMessagePageHeader .btnSave'), false);
        TMMail.disableButton($('#editMessagePageHeader .btnSaveTemplate'), false);
        if (mailBox.currentMessageId > 0) {
            TMMail.disableButton($('#editMessagePageHeader .btnDelete'), false);
            TMMail.disableButton($('#editMessagePageHeader .btnAddTag'), false);
        }
    }

    function closeCompose(e) {
        var newAnchor;
        
        switch (e.target.className) {
            case "mail":
                newAnchor = TMMail.sysfolders.inbox.name;
                break;
            case "main-button-text":
                newAnchor = "compose";
                break;
            case "on-close-link":
                var lastAnchor = ASC.Controls.AnchorController.getLastAnchor();
                newAnchor = (!lastAnchor || TMMail.anchors.writemessage.test(lastAnchor)) 
                    ? TMMail.isTemplate() 
                        ? TMMail.sysfolders.templates.name
                        : TMMail.sysfolders.inbox.name 
                    : lastAnchor;
                break;
            default:
                var lastAnchor = ASC.Controls.AnchorController.getLastAnchor();
                newAnchor = lastAnchor || TMMail.sysfolders.inbox.name;
                break;
        }

        if(newAnchor) {
            console.log("Change location", newAnchor);
            window.location.href = '#' + newAnchor;
        }
    }

    return {
        init: init,
        hide: hide,
        view: view,
        conversation: conversation,
        edit: edit,
        compose: compose,
        composeTo: composeTo,
        reply: reply,
        replyAll: replyAll,
        forward: forward,
        onLeaveMessage: onLeaveMessage,
        deleteMessageAttachment: deleteMessageAttachment,
        deleteCurrentMessage: deleteCurrentMessage,
        sendMessage: sendMessage,
        saveMessageTemplate: saveMessageTemplate,
        saveMessage: saveMessage,
        isMessageDirty: isMessageDirty,
        isMessageSending: isMessageSending,
        resetDirtyMessage: resetDirtyMessage,
        setDirtyMessage: setDirtyMessage,
        setTag: setTag,
        unsetTag: unsetTag,
        getTags: getTags,
        hasTag: hasTag,
        onCompose: onCompose,
        onComposeTo: onComposeTo,
        onComposeFromCrm: onComposeFromCrm,
        getFromAddress: getFromAddress,
        getToAddresses: getToAddresses,
        getMessageFolder: getMessageFolder,
        isSortConversationByAsc: isSortConversationByAsc,
        getCurrentConversationIds: getCurrentConversationIds,
        getActualConversationLastMessageId: getActualConversationLastMessageId,
        initImageZoom: initImageZoom,
        updateFromSelected: updateFromSelected,
        setToEmailAddresses: setToEmailAddresses,
        conversation_moved: conversationMoved,
        dstFolderType : dstFolderType,
        dstUserFolderId: dstUserFolderId,
        conversation_deleted: conversationDeleted,
        setHasLinked: setHasLinked,
        updateAttachmentsActionMenu: updateAttachmentsActionMenu,
        updateEditAttachmentsActionMenu: updateEditAttachmentsActionMenu,
        editDocumentAttachment: editDocumentAttachment,
        selectFromAccount: selectFromAccount,
        refreshMailAfterSent: refreshMailAfterSent,
        preprocessMessages: preprocessMessages,
        sendAction: sendAction,
        saveTemplateAction: saveTemplateAction,
        updateFromAccountField: updateFromAccountField,
        cancelSending: cancelSending,
        saveMessagePomise: saveMessagePomise,
        saveMessageTemplatePomise: saveMessageTemplatePomise,
        closeCompose: closeCompose,
        moveTo: moveTo,
        slideTemplateSelector: slideTemplateSelector
    };

})(jQuery);