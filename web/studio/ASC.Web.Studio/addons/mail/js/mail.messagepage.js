/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


window.messagePage = (function($) {
    var isInit = false,
        saveTimeout = null,
        messageIsDirty = false,
        messageIsSending = false,
        isMessageRead = false,
        randomGuid = '',
        sortConversationByAsc = true,
        maxConversationShow = 5,
        attachmentMenuItems,
        attachmentEditMenuItems,
        singleMessageMenuItems,
        messageMenuItems,
        toEmailAddresses = [],
        savingLock = false,
        repeatSaveFlag = false,
        conversationMoved = false,
        conversationDeleted = false,
        hasLinked = false,
        crmContactsInfo = [];

    var nodeType = {
        element: 1,
        text: 3
    };

    function setHasLinked(val) {
        hasLinked = val;
    }

    function init() {
        if (isInit === false) {
            isInit = true;
            sortConversationByAsc = (TMMail.option('ConversationSortAsc') === 'true');

            serviceManager.bind(window.Teamlab.events.getMailMessageTemplate, onGetMailMessageTemplate);
            serviceManager.bind(window.Teamlab.events.getMailRandomGuid, onGetMailRandomGuid);
            serviceManager.bind(window.Teamlab.events.sendMailMessage, onSendMessage);
            serviceManager.bind(window.Teamlab.events.saveMailMessage, onSaveMessage);
            serviceManager.bind(window.Teamlab.events.isConversationLinkedWithCrm, onGetConversationLinkStatus);
            serviceManager.bind(window.Teamlab.events.exportAllAttachmentsToMyDocuments, onExportAttachmentsToMyDocuments);
            serviceManager.bind(window.Teamlab.events.exportAttachmentToMyDocuments, onExportAttachmentToMyDocuments);
            serviceManager.bind(window.Teamlab.events.markChainAsCrmLinked, onMarkChainAsCrmLinked);
        }

        attachmentMenuItems = [
            { selector: "#attachmentActionMenu .downloadAttachment", handler: downloadAttachment },
            { selector: "#attachmentActionMenu .viewAttachment", handler: viewAttachment },
            { selector: "#attachmentActionMenu .editAttachment", handler: editDocumentAttachment },
            { selector: "#attachmentActionMenu .saveAttachmentToMyDocs", handler: saveAttachmentToMyDocs }
        ];

        attachmentEditMenuItems = [
            { selector: "#attachmentEditActionMenu .downloadAttachment", handler: downloadAttachment },
            { selector: "#attachmentEditActionMenu .viewAttachment", handler: viewAttachment },
            { selector: "#attachmentEditActionMenu .deleteAttachment", handler: deleteAttachment }
        ];

        singleMessageMenuItems = [
            { selector: "#singleMessageActionMenu .replyMail", handler: TMMail.moveToReply },
            { selector: "#singleMessageActionMenu .replyAllMail", handler: TMMail.moveToReplyAll },
            { selector: "#singleMessageActionMenu .forwardMail", handler: TMMail.moveToForward },
            { selector: "#singleMessageActionMenu .deleteMail", handler: deleteMessage },
            { selector: "#singleMessageActionMenu .printMail", handler: moveToMessagePrint },
            { selector: "#singleMessageActionMenu .alwaysHideImages", handler: alwaysHideImages }
        ];

        messageMenuItems = [
            { selector: "#messageActionMenu .replyMail", handler: TMMail.moveToReply },
            { selector: "#messageActionMenu .replyAllMail", handler: TMMail.moveToReplyAll },
            { selector: "#messageActionMenu .singleViewMail", handler: TMMail.openMessage },
            { selector: "#messageActionMenu .forwardMail", handler: TMMail.moveToForward },
            { selector: "#messageActionMenu .deleteMail", handler: deleteMessage },
            { selector: "#messageActionMenu .printMail", handler: moveToMessagePrint },
            { selector: "#messageActionMenu .alwaysHideImages", handler: alwaysHideImages }
        ];

        if (TMMail.availability.CRM) {
            singleMessageMenuItems.push({ selector: "#singleMessageActionMenu .exportMessageToCrm", handler: exportMessageToCrm });
            messageMenuItems.push({ selector: "#messageActionMenu .exportMessageToCrm", handler: exportMessageToCrm });
        } else {
            $('.exportMessageToCrm.dropdown-item').hide();
        }

        wysiwygEditor.unbind(wysiwygEditor.events.OnChange).bind(wysiwygEditor.events.OnChange, onMessageChanged);
    }

    function hide() {
        closeMessagePanel();
        var magnificPopup = $.magnificPopup.instance;
        if (magnificPopup) magnificPopup.close();
    }

    function showConversationMessage(id, noBlock) {
        serviceManager.getMessage(id, noBlock, false, { action: 'view', conversation_message: true, message_id: id },
            {
                success: onGetMailMessage,
                error: onOpenConversationMessageError
            });
    }

    function view(id, noBlock) {
        serviceManager.getMessage(id, noBlock, false, { action: 'view' },
            {
                success: onGetMailMessage,
                error: onOpenMessageError
            }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function conversation(id, loadAllContent) {
        var content = seekRememberedContent(id);

        if (!content) {
            serviceManager.getConversation(id, loadAllContent, { action: 'conversation', id: id },
                {
                    success: onGetMailConversation,
                    error: onOpenMessageError
                }, ASC.Resources.Master.Resource.LoadingProcessing);
        } else {
            onGetMailConversation({ action: 'conversation', id: id }, content);
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

    function onCompose() {
        serviceManager.getMessageTemplate({}, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function onComposeTo(params) {
        var addresses;
        if (params) {
            addresses = params.join(', ');
        } else {
            addresses = toEmailAddresses.join(', ');
        }
        serviceManager.getMessageTemplate(addresses !== undefined ? { email: addresses } : {}, ASC.Resources.Master.Resource.LoadingProcessing);
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
        ASC.Controls.AnchorController.safemove('draftitem/' + id);
        serviceManager.getMessage(id, true, false, { action: 'draft' },
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
        if (TMMail.pageIs('conversation')) {
            var senderAddress = TMMail.parseEmailFromFullAddress(getFromAddress(id));
            hideImagesAction(null, { address: senderAddress });
        }
    }

    function exportMessageToCrm(messageId) {
        var html = CrmLinkPopup.getCrmExportControl(messageId);
        window.popup.init();
        window.popup.addBig(window.MailScriptResource.ExportConversationPopupHeader, html);
    }

    function isContentBlocked(id) {
        var fullView = $('#itemContainer .full-view[message_id="' + id + '"]');
        var contentBlocked = (fullView.length == 0 ? true : fullView.attr('content_blocked') == "true");
        return contentBlocked;
    }

    function reply(id) {
        serviceManager.generateGuid();
        serviceManager.getMessage(id, !isContentBlocked(id), true, { action: 'reply' },
            {
                success: onGetMailMessage,
                error: onOpenMessageError
            }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function replyAll(id) {
        serviceManager.generateGuid();
        serviceManager.getMessage(id, !isContentBlocked(id), true, { action: 'replyAll' },
            {
                success: onGetMailMessage,
                error: onOpenMessageError
            }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function forward(id) {
        serviceManager.generateGuid();
        serviceManager.getMessage(id, !isContentBlocked(id), true, { action: 'forward' },
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
            saveMessage();
        }
    }

    function saveMessage() {
        if (!obtainSavingLock()) {
            return;
        }

        if (mailBox.currentMessageId < 1) {
            mailBox.currentMessageId = message_id = 0;
        } else {
            message_id = mailBox.currentMessageId;
        }

        resetDirtyMessage();

        var data = prepareMessageData(false);

        if (data != undefined) {
            serviceManager.saveMessage(message_id,
                data.From,
                data.Subject,
                data.To,
                data.Cc,
                data.Bcc,
                data.HtmlBody,
                data.Attachments,
                data.StreamId,
                data.MimeMessageId,
                data.MimeReplyToId,
                data.Important,
                data.Labels,
                {},
                { error: onErrorSendMessage },
                window.MailScriptResource.SavingMessage);

            serviceManager.updateFolders();
        }
    }

    function onAttachmentsUploadComplete() {
        sendMessage();
        messageIsSending = false;
    }

    function sendMessage(messageId) {
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
            window.LoadingBanner.strLoading = window.MailScriptResource.SendingMessage + ": " + window.MailScriptResource.LoadingAttachments;
            window.LoadingBanner.displayMailLoading(true, true);
            return;
        }

        if (needCrmLink()) {
            messageIsSending = true;
            saveMessage();
            return;
        }

        messageIsSending = false;

        var data = prepareMessageData(true);

        window.LoadingBanner.hideLoading();
        window.LoadingBanner.strLoading = ASC.Resources.Master.Resource.LoadingProcessing;

        if (data != undefined) {
            clearTimeout(saveTimeout);
            serviceManager.sendMessage(messageId,
                data.From,
                data.Subject,
                data.To,
                data.Cc,
                data.Bcc,
                data.HtmlBody,
                data.Attachments,
                data.StreamId,
                data.MimeMessageId,
                data.MimeReplyToId,
                data.Important,
                data.Labels,
                data.FileLinksShareMode,
                {},
                { error: onErrorSendMessage },
                window.MailScriptResource.SendingMessage);
        } else {
            TMMail.disableButton($('#editMessagePage .btnSend'), false);
            TMMail.disableButton($('#editMessagePage .btnSave'), false);
            if (messageId > 0) {
                TMMail.disableButton($('#editMessagePage .btnDelete'), false);
            }
            TMMail.disableButton($('#editMessagePage .btnAddTag'), false);
        }
    }

    function getAddressesArray(str) {
        if ('string' !== typeof str) {
            return [];
        }

        var arr = str.split(/\s*,\s*/);

        $.each(arr, function(i, v) {
            $.trim(v);
        });

        arr = $.grep(arr, function(v) {
            return '' != v;
        });

        return arr;
    }

    function parseAddressesByRfc(str) {
        var errors = [];
        var addresses = [];

        var emailRegex = /["\'].*?["\']\s*<.*?>|<.*?>|[^\s]+@[^\s,]+/g;

        if ('string' !== typeof str || str.length == 0) {
            errors.push(window.MailScriptResource.ErrorEmptyToField);
            return { addresses: addresses, errors: errors };
        }

        var notRfcData = str.split(emailRegex);
        $.each(notRfcData, function(index, value) {
            var trimmedItem = $.trim(value).replace(',', '');
            if (trimmedItem.length > 0) {
                errors.push(window.MailScriptResource.ErrorIncorrectAddress + " \"" + trimmedItem + "\"");
            }
        });
        if (errors.length > 0) {
            return { addresses: addresses, errors: errors };
        }

        //matchs: "letters + symbols" <text>, 'letters + symbols' <text>, <text>, text@text.text
        var rfcData = str.match(emailRegex);
        $.each(rfcData, function(index, value) {
            if (TMMail.reEmail.exec(value) === null) {
                errors.push(window.MailScriptResource.ErrorIncorrectAddress + " \"" + value + "\"");
            } else {
                addresses.push(value);
            }
        });

        return { addresses: addresses, errors: errors };
    }

    function prepareMessageData(needValidation) {
        var to, cc, bcc,
            toStr = $('#newmessageTo').val(),
            ccStr = $('#newmessageCopy').val(),
            bccStr = $('#newmessageBCC').val(),
            from = $('#newmessageFromSelected').attr('mailbox_email'),
            subject = $('#newmessageSubject').val(),
            importance = $('#newmessageImportance')[0].checked,
            streamId = $('#newMessage').attr('streamId'),
            body;

        body = wysiwygEditor.getValue();

        if (needValidation && !validateMessage()) {
            return undefined;
        }

        to = getAddressesArray(toStr);
        cc = getAddressesArray(ccStr);
        bcc = getAddressesArray(bccStr);

        var data = {};
        var labelsCollection = $.makeArray($(".tags .itemTags a").map(function() { return parseInt($(this).attr("tagid")); }));

        var message = getEditingMessage();

        data.From = from;
        data.To = to;
        data.Cc = cc;
        data.Bcc = bcc;
        data.Subject = subject;
        data.MimeReplyToId = message.mimeReplyToId;
        data.MimeMessageId = message.mimeMessageId;
        data.Important = importance;
        data.Labels = labelsCollection;
        data.HtmlBody = body;
        data.StreamId = streamId;
        data.Attachments = AttachmentManager.GetAttachments();
        data.FileLinksShareMode = $('#sharingSettingForFileLinksTmplBox input[name="shareFileLinksAccessSelector"]:checked').val();

        return data;
    }

    function validateMessage() {
        var toStr = $('#newmessageTo').val();
        var ccStr = $('#newmessageCopy').val();
        var bccStr = $('#newmessageBCC').val();

        var parseResult = parseAddressesByRfc(toStr);
        errors = parseResult.errors;
        if (errors.length == 0 && parseResult.addresses.length == 0) {
            errors.push(window.MailScriptResource.ErrorEmptyToField);
        }

        $('#newmessageTo').css('border-color', errors.length > 0 ? '#c00' : 'null');

        if (!errors.length && ccStr.length) {
            parseResult = parseAddressesByRfc(ccStr);
            errors = parseResult.errors;
            $('#newmessageCopy').css('border-color', parseResult.errors.length > 0 ? '#c00' : 'null');
        } else {
            $('#newmessageCopy').css('border-color', 'null');
        }

        if (!errors.length && bccStr.length) {
            parseResult = parseAddressesByRfc(bccStr);
            errors = parseResult.errors;
            $('#newmessageBCC').css('border-color', parseResult.errors.length > 0 ? '#c00' : 'null');
        } else {
            $('#newmessageBCC').css('border-color', 'null');
        }

        if (errors.length > 0) {
            window.LoadingBanner.hideLoading();
            var i, len = errors.length;
            for (i = 0; i < len; i++) {
                window.toastr.error(errors[i]);
            }
            return false;
        } else {
            return true;
        }
    }

    function getMessageFileLinks() {
        var fileLinks = [];
        $($('<div/>').append(wysiwygEditor.getValue())).find('.mailmessage-filelink').each(function () {
            fileLinks.push($(this).attr('data-fileid'));
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
                $.each(message.tagIds, function(i, value) {
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
        if (tag != undefined) {
            var tagsPanel = $('#itemContainer .head[message_id=' + messageId + '] .tags');

            if (tagsPanel) {
                var html = $.tmpl('tagInMessageTmpl', tag, { htmlEncode: TMMail.htmlEncode });
                var $html = $(html);

                tagsPanel.find('.value .itemTags').append($html);

                tagsPanel.find('a.tagDelete').unbind('click').click(function() {
                    messageId = $(this).closest('.message-wrap').attr('message_id');
                    var idTag = $(this).attr('tagid');
                    mailBox.unsetTag(idTag, [messageId]);
                });

                tagsPanel.show(); // show tags panel

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

        if (tagsPanel) {
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

    /* -= Private Methods =- */

    function insertBody(message) {

        var $messageBody = $('#itemContainer .itemWrapper .body[message_id=' + message.id + ']');

        $messageBody.data('message', message);
        var html;
        if (message.isBodyCorrupted) {
            html = $.tmpl('messageOpenErrorTmpl');
            $messageBody.html(html);
            TMMail.fixMailtoLinks($messageBody);
        } else if (message.hasParseError) {
            html = $.tmpl('messageParseErrorTmpl');
            $messageBody.html(html);
            TMMail.fixMailtoLinks($messageBody);
        } else {
            var iframeSupported = document.createElement("iframe");
            if (!iframeSupported || message.textBodyOnly) {

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

                $messageBody.html(message.htmlBody);

                $messageBody.find('blockquote').each(function() {
                    insertBlockquoteBtn($(this));
                });

                $messageBody.find('.tl-controll-blockquote').each(function() {
                    $(this).click(function() {
                        $(this).next('blockquote').toggle();
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
                var $frame = createBodyIFrame(message);
                $messageBody.html($frame);
            }
        }
        $('#itemContainer').height('auto');
    }

    function createBodyIFrame(message, showImages, bodyRenderedCallback, imagesLoadedCallback) {
        var $frame = $('<iframe id="message_body_frame_' + message.id +
            '" scrolling="auto" frameborder="0" width="100%" style="height:0;">An iframe capable browser is required to view this web site.</iframe>');

        $frame.bind('load', function() {
            $frame.unbind('load');
            var $body = $(this).contents().find('body');
            $body.css('cssText', 'height: 0;');
            var htmlText = '<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">' + message.htmlBody;
            var htmlBodyContents = $('<div>'.concat(htmlText).concat('</div>')).contents();

            // remove br tags from the end
            for (var i = htmlBodyContents.length - 1; i >= 0; i--) {
                var item = htmlBodyContents[i];
                if (item.nodeName == 'BR') {
                    htmlBodyContents.splice(i, 1);
                } else {
                    break;
                }
            }

            $body.append(htmlBodyContents.length > 0 ? htmlBodyContents : htmlText);
            $body.find("a[href*='mailto:']").click(function() {
                messagePage.setToEmailAddresses([$(this).attr('href').substr(7, $(this).attr('href').length - 1)]);
                window.location.href = "#composeto";
                return false;
            });
            $body.find("a").attr('target', '_blank');

            var $blockquote = $($body.find('div.gmail_quote:first, div.yahoo_quoted:first, ' +
                'blockquote:first, div:has(hr#stopSpelling):last')[0]);

            if ($blockquote) {
                insertBlockquoteBtn($blockquote);
                var $head = $(this).contents().find("head");
                $head.append($("<link/>",
                    { rel: "stylesheet", href: "/addons/mail/app_themes/default/iframe_content.css", type: "text/css" }));

                var $btnBlockquote = $body.find('.tl-controll-blockquote');
                if ($btnBlockquote) {
                    $btnBlockquote.click(function() {
                        $blockquote.toggle();
                        var iframe = $('#message_body_frame_' + message.id);
                        iframe.attr('scrolling', 'no');
                        window.setImmediate(function() {
                            iframe.attr('scrolling', 'auto');
                            if ($blockquote.is(':hidden')) {
                                var heightDiff = $blockquote.outerHeight();
                                var newHeight = parseInt(iframe.css('height').replace('px', ''), 10) - heightDiff;
                                iframe.css('height', newHeight + 'px');
                            } else {
                                updateIframeSize(message.id, true);
                            }
                        });
                    });
                }
            }

            if (message.to != undefined) {
                $body.find("a#delivery_failure_faq_link").attr('href', TMMail.getFaqLink(message.to));
            }

            $body.find("a[id='delivery_failure_button']").click(function() {
                var deliveryFailureMessageId = $(this).attr("mailid");
                messagePage.edit(deliveryFailureMessageId);
            });
            $body.find("a[href*='mailto:']").removeAttr('target');
            updateIframeSize(message.id, true);
            displayTrustedImages(message, showImages, imagesLoadedCallback);

            if (bodyRenderedCallback) {
                bodyRenderedCallback($frame);
            }
        });

        return $frame;
    }

    function insertBlockquoteBtn(element) {
        element.before($.tmpl('blockquoteTmpl', {}).get(0).outerHTML);
        element.hide();
    }

    function getTextNodeParams(textNode) {
        var param = null;
        if (document.createRange) {
            var range = document.createRange();
            range.selectNodeContents(textNode);
            if (range.getBoundingClientRect) {
                var rect = range.getBoundingClientRect();
                if (rect) {
                    param = rect;
                }
            }
        }
        return param;
    }

    function updateIframeSize(id, finish) {
        /** IMPORTANT: All framed documents *must* have a DOCTYPE applied **/
        var iframe = $('#message_body_frame_' + id);
        if (iframe[0] == undefined) {
            return;
        }

        var body = iframe.contents().find('body');

        body.css('cssText', 'height: 0; max-width: ' + iframe.width() + 'px !important;');

        var contents = body.contents();
        var newHeight = 0;
        var textNodeParam;

        if (window.ActiveXObject || window.sidebar) { // IE, Firefox
            for (var i = 0; i < contents.length; i++) {
                var offset = 0;
                var item = contents[i];

                if (item.nodeType == nodeType.element) {
                    offset = $(item).offset().top + Math.max(item.scrollHeight, item.clientHeight, $(item).outerHeight(true));
                } else if (item.nodeType == nodeType.text) {
                    textNodeParam = getTextNodeParams(item);
                    offset = textNodeParam == null ? 0 : textNodeParam.bottom + 1;
                }

                if (newHeight < offset) {
                    newHeight = offset;
                }
            }
            newHeight += body.offset().top;
        } else {
            newHeight = iframe.contents().outerHeight(true);
            if (true == finish) {
                newHeight += body.offset().top;
            }
        }

        // for scroll
        if (window.ActiveXObject) {
            newHeight += 15;
        } else if (true == finish) {
            newHeight += 10;
        }

        iframe.css('height', newHeight + 'px');
    }

    function activateUploader(attachments) {
        var streamId = TMMail.translateSymbols($('#newMessage').attr('streamId'), false);
        AttachmentManager.InitUploader(streamId, attachments);
    }

    function updateFromSelected() {
        var accounts = accountsManager.getAccountList();
        if (accounts.length > 1) {
            var buttons = [];
            for (var i = 0; i < accounts.length; i++) {
                var account = accounts[i];
                var explanation = undefined;
                if (account.is_alias) {
                    explanation = window.MailScriptResource.AliasLabel;
                } else if (account.is_group) {
                    continue;
                }

                var title = TMMail.ltgt(account.name + " <" + account.email + ">");
                var cssClass = account.enabled ? '' : 'disabled';

                var accountInfo = {
                    text: title,
                    explanation: explanation,
                    handler: selectFromAccount,
                    account: account,
                    css_class: cssClass,
                    title: title,
                    signature: account.signature
                };

                buttons.push(accountInfo);

            }
            $('#newmessageFromSelected').actionPanel({ buttons: buttons });
            $('#newmessageFromSelected .arrow-down').show();
            $('#newmessageFromSelected').addClass('pointer');
            $('#newmessageFromSelected .baseLinkAction').addClass('baseLinkAction');
        } else {
            $('#newmessageFromSelected .arrow-down').hide();
            $('#newmessageFromSelected .baseLinkAction').removeClass('baseLinkAction');
            $('#newmessageFromSelected').removeClass('pointer');
            if (accounts.length == 1) {
                account = accounts[0];
                selectFromAccount({}, {
                    account: account,
                    title: TMMail.ltgt(account.name + " <" + account.email + ">")
                });
            } else {
                ASC.Controls.AnchorController.move('#accounts');
            }
        }
    }

    function setEditMessageButtons() {
        updateFromSelected();

        // Send
        $('#editMessagePage .btnSend').unbind('click').click(sendAction);

        // Save
        $('#editMessagePage .btnSave').unbind('click').click(saveAction);
        // Delete
        $('#editMessagePage .btnDelete').unbind('click').click(deleteAction);

        if (mailBox.currentMessageId < 1) {
            TMMail.disableButton($('#editMessagePage .btnDelete'), true);
        }

        // Add tag
        $('#editMessagePage .btnAddTag.unlockAction').unbind('click').click(function() {
            tagsDropdown.show($(this));
        });

        if ($('#newmessageCopy').val() != "" || $('#newmessageBCC').val() != "") {
            $('.value-group.cc').show();
            $('.value-group.bcc').show();
            $('#AddCopy:visible').remove();
            $('#newmessageCopy').trigger('input');
            $('#newmessageBCC').trigger('input');
        } else {
            $('#AddCopy:visible').unbind('click').click(function() {
                $('.value-group.cc').show();
                $('.value-group.bcc').show();
                $('#newmessageCopy').focus();
                $(this).remove();
            });
        }
    }

    function setMenuActionButtons(senderAddress) {
        isMessageRead = true;

        $('.btnReply.unlockAction').unbind('click').click(function() {
            // Google Analytics
            window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.buttonClick, "reply");
            var messageId = mailBox.currentMessageId;

            if (TMMail.pageIs('conversation')) {
                messageId = getActualConversationLastMessageId();
            }

            TMMail.moveToReply(messageId);
        });

        $('.btnReplyAll.unlockAction').unbind('click').click(function() {
            // Google Analytics
            window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.buttonClick, "replyAll");
            var messageId = mailBox.currentMessageId;

            if (TMMail.pageIs('conversation')) {
                messageId = getActualConversationLastMessageId();
            }

            TMMail.moveToReplyAll(messageId);
        });

        $('.btnForward.unlockAction').unbind('click').click(function() {
            // Google Analytics
            window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.buttonClick, "forward");
            var messageId = mailBox.currentMessageId;
            var activeFolderId = $('#studio_sidePanel #foldersContainer .active').attr('folderid');

            if (TMMail.pageIs('conversation')) {
                messageId = getActualConversationLastMessageId('[folder="' + activeFolderId + '"]');
            }

            TMMail.moveToForward(messageId);
        });

        $('.btnDelete.unlockAction').unbind('click').click(deleteAction);

        // Delete
        $('#itemContainer .contentMenuWrapper:visible .menuActionDelete').unbind('click').click(function() {
            if ($(this).hasClass('unlockAction')) {
                deleteCurrent();
            }
        });

        $('#menuActionBack').unbind('click').click(function() {
            mailBox.updateAnchor(true, true);
        });

        $('#itemContainer .contentMenuWrapper:visible .menuActionSpam').toggle(MailFilter.getFolder() != TMMail.sysfolders.spam.id);
        $('#itemContainer .contentMenuWrapper:visible .menuActionNotSpam').toggle(MailFilter.getFolder() == TMMail.sysfolders.spam.id);

        // Spam
        $('#itemContainer .contentMenuWrapper:visible .menuActionSpam').unbind('click').click(function() {
            spamCurrent();
        });

        // NotSpam
        $('#itemContainer .contentMenuWrapper:visible .menuActionNotSpam').unbind('click').click(function() {
            unspamCurrent();
        });

        var moreButton = $('.btnMore');

        if (moreButton != undefined) {
            var folderId = parseInt(MailFilter.getFolder());
            var buttons = [];

            var printBtnLabel = $("#itemContainer .message-wrap").length > 1 ? window.MailScriptResource.PrintAllBtnLabel : window.MailScriptResource.PrintBtnLabel;

            switch (folderId) {
                case TMMail.sysfolders.inbox.id:
                    if (window.mailPrintAvailable)
                        buttons.push({ text: printBtnLabel, handler: moveToPrint });
                    buttons.push({ text: window.MailScriptResource.SpamLabel, handler: spamUnspamAction, spam: true });
                    buttons.push({ text: window.MailScriptResource.UnreadLabel, handler: readUnreadMessageAction });
                    if (TMMail.availability.CRM) {
                        buttons.push({ text: window.MailResource.LinkChainWithCRM, handler: showLinkChainPopup });
                    }
                    break;
                case TMMail.sysfolders.sent.id:
                    if (window.mailPrintAvailable)
                        buttons.push({ text: printBtnLabel, handler: moveToPrint });
                    buttons.push({ text: window.MailScriptResource.UnreadLabel, handler: readUnreadMessageAction });
                    if (TMMail.availability.CRM) {
                        buttons.push({ text: window.MailResource.LinkChainWithCRM, handler: showLinkChainPopup });
                    }
                    break;
                case TMMail.sysfolders.trash.id:
                    if (window.mailPrintAvailable)
                        buttons.push({ text: printBtnLabel, handler: moveToPrint });
                    buttons.push({ text: window.MailScriptResource.RestoreBtnLabel, handler: restoreMessageAction });
                    buttons.push({ text: window.MailScriptResource.UnreadLabel, handler: readUnreadMessageAction });
                    break;
                case TMMail.sysfolders.spam.id:
                    if (window.mailPrintAvailable)
                        buttons.push({ text: printBtnLabel, handler: moveToPrint });
                    buttons.push({ text: window.MailScriptResource.NotSpamLabel, handler: spamUnspamAction, spam: false });
                    buttons.push({ text: window.MailScriptResource.UnreadLabel, handler: readUnreadMessageAction });
                    break;
                default:
            }

            if (senderAddress) {
                if (trustedAddresses.isTrusted(senderAddress)) {
                    buttons.push({ text: window.MailScriptResource.HideImagesLabel + ' "' + senderAddress + '"', handler: hideImagesAction, address: senderAddress });
                }
            }

            moreButton.actionPanel({ buttons: buttons, css: 'stick-over' });
        }

        // Add tag
        $('.btnAddTag.unlockAction').unbind('click').click(function() {
            tagsDropdown.show($(this));
        });
    }

    function showLinkChainPopup() {
        var html = CrmLinkPopup.getCrmLinkControl(hasLinked);
        window.popup.init();
        window.popup.addBig(window.MailScriptResource.LinkConversationPopupHeader, html);
    }

    function setConversationViewActions() {
        $('#sort-conversation').toggleClass('asc', isSortConversationByAsc()).toggleClass('desc', !isSortConversationByAsc());

        $('#sort-conversation').unbind('click').click(function() {
            sortConversationByAsc = !isSortConversationByAsc();

            $('#sort-conversation').toggleClass('asc', isSortConversationByAsc()).toggleClass('desc', !isSortConversationByAsc());

            $('.itemWrapper').append($('.message-wrap, .collapsed-messages').get().reverse());

            //restore iframe contents
            $('iframe[id^=message_body_frame_]').each(function() {
                var value = $(this);
                var message = value.parent().data('message');
                message.contentIsBlocked = true;
                insertBody(message);
            });

            TMMail.option('ConversationSortAsc', sortConversationByAsc);
        });

        $('#collapse-conversation').unbind('click').click(function() {
            if ($('.full-view:hidden').length > 0) {
                showCollapsedMessages();

                $('.full-view[loaded="true"]').each(function(index, el) {
                    var messageId = $(el).attr('message_id');
                    if (typeof(messageId) !== 'undefined') {
                        expandConversation(messageId);
                    }
                });

                $('.full-view[loaded!="true"]').each(function(index, el) {
                    var messageId = $(el).attr('message_id'),
                        parentWrap = $(el).closest('.message-wrap'),
                        shortView = parentWrap.children('.short-view'),
                        loader = shortView.find('.loader');

                    showConversationMessage(messageId, false);
                    shortView.addClass('loading');
                    loader.show();
                });
            } else {
                $('.full-view').each(function(index, el) {
                    var messageId = $(el).attr('message_id');
                    collapseConversation(messageId);
                });
            }
        });
    }

    function initBlockContent(message) {
        $('#id-btn-block-content-' + message.id).unbind('click').click(function() {
            displayImages(message.id);
        });

        $('#id-btn-always-block-content-' + message.id).unbind('click').click(function() {
            if (TMMail.pageIs('conversation')) {
                var currentSender = message.sender_address;
                trustedAddresses.add(currentSender);
                var conversationMessages = $('#itemContainer').find('.message-wrap');
                var i, len;
                for (i = 0, len = conversationMessages.length; i < len; i++) {
                    var currentMessage = $(conversationMessages[i]);
                    if (currentMessage.find('.full-view[loaded="true"]').size() == 1) {
                        // message is loaded
                        var messageId = currentMessage.attr('message_id');
                        var senderAddress = TMMail.parseEmailFromFullAddress(getFromAddress(messageId));
                        if (currentSender == senderAddress) {
                            displayImages(messageId);
                        }
                    }
                }
                setMenuActionButtons(currentSender);

            } else {
                trustedAddresses.add(message.sender_address);
                displayImages(message.id);
                setMenuActionButtons(message.sender_address);
            }
        });
    }

    function renameAttr(node, attrName, newAttrName) {
        node.each(function() {
            var val = node.attr(attrName);
            node.attr(newAttrName, val);
            node.removeAttr(attrName);
        });
    }

    function displayImages(messageId, cb) {
        var iframe = $('#message_body_frame_' + messageId);
        iframe.attr('scrolling', 'no');
        var messageBody = iframe.contents().find('body');
        if (messageBody) {
            var styleTag = messageBody.find('style');
            if (styleTag.length > 0) { // style fix
                styleTag.html(styleTag.html().replace(/tl_disabled_/g, ''));
            }

            messageBody.find('*').each(function(index, node) {
                $(node.attributes).each(function() {
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

            var intervalHandler = setInterval(function() {
                updateIframeSize(messageId);
            }, 50);

            messageBody.waitForImages({
                finished: function() {
                    setImmediate(function() {
                        clearInterval(intervalHandler);
                        updateIframeSize(messageId, true);
                        setImmediate(function() {
                            iframe.attr('scrolling', 'auto');
                            //For horizontal scroll visualisation. If you remove that block horizontal scrolling will not appears in all browsers except IE >9 and Firefox.
                            if (iframe.contents().outerWidth() > iframe.width()) {
                                messageBody.append('<div class="temp" style="height: 1px"/>');
                                messageBody.find('.temp').remove();
                            }

                            if (cb) {
                                cb();
                            }
                        });
                    });
                },
                waitForAll: true
            });
        } else if (cb) {
            cb();
        }
    }

    function hideAllActionPanels() {
        $.each($('.actionPanel:visible'), function(index, value) {
            var popup = $(value);
            if (popup != undefined) {
                popup.hide();
            }
        });
    }

    function selectFromAccount(event, params) {
        if (params.account.is_group)
            return;

        $('#newmessageFromSelected').attr('mailbox_email', params.account.email);
        $('#newmessageFromSelected span').html(params.title);
        $('#newmessageFromSelected').toggleClass('disabled', !params.account.enabled);
        $('#newmessageFromWarning').toggle(!params.account.enabled);
        wysiwygEditor.setSignature(params.account.signature);
        accountsPanel.mark(params.account.email);
    }

    function deleteMessageAttachment(attachId) {
        serviceManager.deleteMessageAttachment(mailBox.currentMessageId, attachId);
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
        TMMail.disableButton($('#editMessagePage .btnDelete'), true);
        TMMail.disableButton($('#editMessagePage .btnSend'), true);
        TMMail.disableButton($('#editMessagePage .btnSave'), true);
        TMMail.disableButton($('#editMessagePage .btnAddTag'), true);
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
        saveMessage();
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

        if (!validateMessage()) {
            return false;
        }

        var fileLinks = getMessageFileLinks();
        if (!forcibly && fileLinks.length) {
            window.popup.addBig(MailScriptResource.SharingSettingForFiles, $.tmpl('sharingSettingForFileLinksTmpl'));
            return false;
        }

        //google analytics
        window.ASC.Mail.ga_track(ga_Categories.createMail, ga_Actions.buttonClick, "send");
        TMMail.disableButton($('#editMessagePage .btnSend'), true);
        TMMail.disableButton($('#editMessagePage .btnSave'), true);
        TMMail.disableButton($('#editMessagePage .btnDelete'), true);
        TMMail.disableButton($('#editMessagePage .btnAddTag'), true);
        sendMessage();
        return false;
    }

    // returns id of actual last message in chain
    // during chain viewing, messages set could be changed (ex. last message could be deleted)
    // values stored in mailBox.currentMessageId or acnhor are not valid any more
    // so actual last message will be detected from markup

    function getActualConversationLastMessageId(selector) {
        var messages = $('.itemWrapper:visible .message-wrap');

        if (selector && messages.has(selector)) {
            messages = messages.filter(selector);
        }

        if (isSortConversationByAsc()) {
            return +messages.last().attr('message_id');
        }

        return +messages.first().attr('message_id');
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
        var fullView = $('#itemContainer .itemWrapper .full-view[loaded=true]');
        for (var i = 0, len = fullView.length; i < len; i++) {
            var messageId = $(fullView[i]).attr('message_id');
            if ($(fullView[i]).find('#id_block_content_popup_' + messageId).length == 0)
                simIds.push(messageId);
        }

        var html = "";
        if (simIds.length != fullView.length)
            html = $.tmpl('imagesBlockedPopupTmpl', { text: MailScriptResource.ConversationImagesBlockedPopupBody });
        else if ($('#itemContainer .itemWrapper .full-view[loaded!=true]').length != 0)
            html = $.tmpl('imagesBlockedPopupTmpl', { text: MailScriptResource.ConversationCollapsedBlockedPopupBody });
        
        if (html != "") {
            $(html).find('.buttons .print').bind("click", function () {
                TMMail.moveToConversationPrint(conversationId, simIds);
                window.popup.hide();
            });
            window.popup.addBig(MailScriptResource.MessageImagesBlockedPopupHeader, html);
        }
        else
            TMMail.moveToConversationPrint(conversationId, simIds);
    }

    function moveToMessagePrint(messageId) {
        var $blockImagesBox = $('#id_block_content_popup_' + messageId);
        if ($blockImagesBox.length) {
            var html = $.tmpl('imagesBlockedPopupTmpl', { text: MailScriptResource.MessageImagesBlockedPopupBody });
            $(html).find('.buttons .print').bind("click", function () {
                TMMail.moveToMessagePrint(messageId, false);
                window.popup.hide();
            });
            window.popup.addBig(MailScriptResource.MessageImagesBlockedPopupHeader, html);
        } else {
            TMMail.moveToMessagePrint(messageId, true);
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
            mailBox.restoreConversations([getActualConversationLastMessageId()]);
        } else {
            mailBox.restoreMessages([mailBox.currentMessageId]);
        }
    }

    function isAppleMobile() {
        if (navigator && navigator.userAgent && navigator.userAgent != null) {
            var strUserAgent = navigator.userAgent.toLowerCase();
            var arrMatches = strUserAgent.match(/(iphone|ipod|ipad)/);
            if (arrMatches) {
                return true;
            }
        }

        return false;
    }

    function initMessagePanel(message, action) {
        updateMessageTags(message);

        if ('draft' == action || 'forward' == action || 'reply' == action || 'compose' == action || 'replyAll' == action) {
            var from = TMMail.parseEmailFromFullAddress(message.from);
            var account = undefined;
            if (from != "") {
                account = accountsManager.getAccountByAddress(from);
            }

            updateFromAccountField(account);

            messageIsDirty = false;

            $('#newmessageTo').autoResize({
                extraSpace: 0,
                limit: 56,
                cleanInput: true
            });

            $('#newmessageTo').emailAutocomplete({ multiple: true });

            $('#newmessageCopy').autoResize({
                extraSpace: 0,
                limit: 56
            });

            $('#newmessageCopy').emailAutocomplete({ multiple: true });

            $('#newmessageBCC').autoResize({
                extraSpace: 0,
                limit: 56
            });

            $('#newmessageBCC').emailAutocomplete({ multiple: true });

            if ('reply' == action || 'replyAll' == action) {
                message.attachments = [];
            }

            setTimeout(function() {
                // Uploading files not supported on MAC OS
                if (isAppleMobile()) {
                    AttachmentManager.HideUpload();
                } else {
                    activateUploader(message.attachments);
                    updateEditAttachmentsActionMenu();
                }
            }, 10); // Dirty trick for Opera 12

            $('#tags_panel div.tag').bind("click", messagePage.onLeaveMessage);
            $('#tags_panel div.tag').each(function() {
                var elementData = $._data(this),
                    events = elementData.events;

                var onClickHandlers = events['click'];

                // Only one handler. Nothing to change.
                if (onClickHandlers.length == 1) {
                    return;
                }

                onClickHandlers.splice(0, 0, onClickHandlers.pop());
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

    function getEditingMessage() {
        return $('#editMessagePage').data('message');
    }

    function bindOnMessageChanged() {
        $('#newmessageTo').bind('textchange', onMessageChanged);
        $('#newmessageCopy').bind('textchange', onMessageChanged);
        $('#newmessageBCC').bind('textchange', onMessageChanged);
        $('#newmessageSubject').bind('textchange', function() {
            // Subject has changed, then it's a new chain;
            updateReplyTo("");
            onMessageChanged();
        });
        $('#newmessageImportance').bind('click', onMessageChanged);
        $('div.itemTags').bind('DOMNodeInserted DOMNodeRemoved', onMessageChanged);
    }

    function onMessageChanged() {
        clearTimeout(saveTimeout);
        setDirtyMessage();
        saveTimeout = setTimeout(function() {
            if (messageIsDirty) {
                saveMessage();
            }
        }, TMMail.saveMessageInterval);
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
            } else if (isMessageDirty()) {
                saveMessage();
            }
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

        $('#editMessagePageHeader').empty();
        $('#editMessagePageFooter').empty();
        $('#editMessagePage').hide();

        $('#tags_panel div.tag').unbind('click', messagePage.onLeaveMessage);
    }

    function resetDirtyMessage() {
        if (!AttachmentManager.IsLoading()) {
            messageIsDirty = false;
            $('#newMessageSaveMarker').text('');
        }
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

    function onSaveMessage(params, message) {
        TMMail.disableButton($('#editMessagePage .btnSave'), false);
        TMMail.disableButton($('#editMessagePage .btnSend'), false);
        TMMail.disableButton($('#editMessagePage .btnAddTag'), false);

        mailBox.markFolderAsChanged(TMMail.sysfolders.drafts.id);

        var now = new Date();

        var min = now.getMinutes() + '';

        if (min.length == 1) {
            min = '0' + min;
        }

        var saveTime = now.getHours() + ':' + min;

        $('.savedtime').show();

        $('.savedtime .savedtime-value').text(saveTime);

        resetDirtyMessage();

        if (message.id > 0) {
            if (mailBox.currentMessageId < 1 && message.attachments.length > 0) {
                var attachments = [];
                $.each(message.attachments, function(index, value) {
                    if (value.contentId == undefined || value.contentId == '') {
                        AttachmentManager.CompleteAttachment(value);
                        attachments.push(value);
                    }
                });
                if (attachments.length > 0) {
                    AttachmentManager.ReloadAttachments(attachments);
                }
            }

            $('#itemContainer .head[message_id]:visible').attr('message_id', message.id);
            mailBox.currentMessageId = message.id;
            TMMail.disableButton($('#editMessagePage .btnDelete'), false);
        }

        releaseSavingLock();

        setEditingMessage(message);

        if (needCrmLink()) {
            serviceManager.markChainAsCrmLinked(mailBox.currentMessageId, crmContactsInfo, {}, ASC.Resources.Master.Resource.LoadingProcessing);
        }
    }

    function onErrorSendMessage(params, error) {
        var errorLimitCnt = $('#id_block_errors_container');
        errorLimitCnt.show();
        errorLimitCnt.find('span').text(error[0]);
        TMMail.disableButton($('#editMessagePage .btnSave'), false);
        TMMail.disableButton($('#editMessagePage .btnSend'), false);
        TMMail.disableButton($('#editMessagePage .btnAddTag'), false);
        TMMail.disableButton($('#editMessagePage .btnDelete'), false);
        releaseSavingLock();
    }

    function onSendMessage() {

        resetDirtyMessage(); // because it is saved while trying to send

        mailBox.markFolderAsChanged(TMMail.sysfolders.drafts.id);

        $('#itemContainer').height('auto');

        mailBox.markFolderAsChanged(TMMail.sysfolders.inbox.id); // for delivery failure messages

        mailBox.markFolderAsChanged(TMMail.sysfolders.sent.id);

        mailBox.updateAnchor(true);

        serviceManager.updateFolders();

        if (!ASC.Resources.Master.Hub.Url ||
            ($.connection && $.connection.hub.state !== $.connection.connectionState.connected)) {
            setTimeout(function() {
                mailAlerts.check();
                window.LoadingBanner.hideLoading();
                window.toastr.success(window.MailScriptResource.SentMessageText);
            }, 3000);
        }
    }

    function showMessageNotification(state) {
        serviceManager.updateFolders();
        window.LoadingBanner.hideLoading();
        if (state == 0) {
            window.toastr.success(window.MailScriptResource.SentMessageText);
        } else if (state == 1) {
            window.toastr.error(window.MailScriptResource.ErrorSendMessage);
            mailAlerts.check();
        }
    }

    function onGetMailMessageTemplate(params, messageTemplate) {
        MailFilter.reset();
        closeMessagePanel();
        mailBox.hidePages();
        $('#itemContainer').height('auto');

        mailBox.hideContentDivs();
        mailBox.hideLoadingMask();

        var writeMessageHtml = $.tmpl('writeMessageTmpl', messageTemplate, { fileSizeToStr: AttachmentManager.GetSizeString });
        var editMessageContainer = $('#itemContainer').find('#editMessagePage');
        if (editMessageContainer.length > 0) {
            editMessageContainer.replaceWith(writeMessageHtml);
        } else {
            $('#itemContainer').append(writeMessageHtml);
        }

        var action = 'compose';

        if (messageTemplate.from == '') {
            var activeAccount = accountsPanel.getActive();
            if (activeAccount) {
                if (!activeAccount.is_group)
                    messageTemplate.from = activeAccount.email;
            }
        }

        initMessagePanel(messageTemplate, action);

        mailBox.currentMessageId = 0;

        if (params.email) {
            messageTemplate.to = params.email;
            $('#newmessageTo').val(params.email);
            $('#newmessageTo').trigger('input');
        }

        showComposeMessageCommon(messageTemplate, action);

        bindOnMessageChanged();

        mailBox.stickActionMenuToTheTop();
    }

    function onGetMailRandomGuid(params, guid) {
        randomGuid = guid;
    }

    function onGetMailMessage(params, message) {
        var originalMessage = $.extend({}, message); // Copy message
        message.conversation_message = params.conversation_message;
        if (!params.conversation_message) {
            closeMessagePanel();
            hideAllActionPanels();
            mailBox.hidePages();
            $('#itemContainer').height('auto');
            mailBox.hideContentDivs();
            if (!TMMail.pageIs('writemessage')) {
                folderPanel.markFolder(message.folder);
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
            if ('forward' == params.action && message.subject.indexOf(window.MailScriptResource.ForwardSubjectPrefix) != 0) {
                message.subject = window.MailScriptResource.ForwardSubjectPrefix + ": " + message.subject;
                message.to = "";
            } else if (('reply' == params.action || 'replyAll' == params.action) && message.subject.indexOf(window.MailScriptResource.ReplySubjectPrefix) != 0) {
                message.subject = window.MailScriptResource.ReplySubjectPrefix + ": " + message.subject;
            }

            var writeMessageHtml = $.tmpl('writeMessageTmpl', message, { fileSizeToStr: AttachmentManager.GetSizeString });
            var editMessageContainer = $('#itemContainer').find('#editMessagePage');
            if (editMessageContainer.length > 0) {
                editMessageContainer.replaceWith(writeMessageHtml);
            } else {
                $('#itemContainer').append(writeMessageHtml);
            }
            releaseSavingLock();
        } else {

            $('#itemContainer').find('.full-view .from').bind('click', function() {
                messagePage.setToEmailAddresses([message.from]);
                messagePage.composeTo();
            });

            if (!params.conversation_message) {
                html = $.tmpl("messageTmpl", null, {
                    messages: [message],
                    last_message: message,
                    important: message.important,
                    fileSizeToStr: AttachmentManager.GetSizeString,
                    cutFileName: AttachmentManager.CutFileName,
                    getFileNameWithoutExt: AttachmentManager.GetFileName,
                    getFileExtension: AttachmentManager.GetFileExtension,
                    htmlEncode: TMMail.htmlEncode,
                    asc: false,
                    action: params.action,
                    crm_available: TMMail.availability.CRM,
                    wordWrap: TMMail.wordWrap
                });

                $('#itemContainer').append(html);
            } else {
                var $messageBody = $('#itemContainer .itemWrapper .body[message_id=' + message.id + ']');

                if ($messageBody.size() > 0) {
                    var $fullView = $messageBody.parent();

                    if ($fullView.size() > 0) {
                        if ($fullView.children('.error-popup').size() < 1 && message.contentIsBlocked) {
                            message.sender_address = TMMail.parseEmailFromFullAddress(message.from);
                            if (!trustedAddresses.isTrusted(message.sender_address)) {
                                $messageBody.before($.tmpl("messageBlockContent", message, {}));
                            }
                        }
                    }
                }
            }

        }

        showMessagesCommon([message], params.action);
        if (isComposeAction(params.action)) {
            showComposeMessageCommon(message, params.action);
        } else {
            showMessageCommon(message, params.action);
            setMenuActionButtons(message.sender_address);
        }

        initIamgeZoom();

        updateAttachmentsActionMenu();

        if (!params.conversation_message) {
            $('#itemContainer').find('.full-view .head').actionMenu('singleMessageActionMenu', singleMessageMenuItems, pretreatmentConversationMessage);
        }

        tuneNextPrev();

        bindOnMessageChanged();
    }

    function onGetConversationLinkStatus(params, status) {
        if (status) {
            $('.header-crm-link').show();
            hasLinked = true;
        } else {
            hasLinked = false;
        }
    }

    function rememberContent(id, content) {
        var $messageRow = $('#itemContainer .messages tr[data_id=' + id + ']');
        if ($messageRow) {
            var contentClone = $.extend(true, {}, { content: content });
            $messageRow.data('content', contentClone);
        }
    }

    function seekRememberedContent(id) {
        var $messageRow = $('#itemContainer .messages tr[data_id=' + id + ']');
        if ($messageRow && $messageRow.data('content')) {
            return $messageRow.data('content').content;
        }

        return undefined;
    }

    function onGetMailConversation(params, messages) {
        var important = false;
        var folderId = TMMail.sysfolders.inbox.id;
        var lastMessage = null;
        var needRemember = true;
        $.each(messages, function(i, m) {
            if (isMessageExpanded(m) && m.isBodyCorrupted) {
                needRemember = false;
            }

            important |= m.important;

            if (m.id == mailBox.currentMessageId) {
                folderId = m.folder;
                lastMessage = m;
            }
        });

        if (lastMessage == null) {
            return;
        }

        if (needRemember) {
            rememberContent(params.id, messages);
        }

        closeMessagePanel();
        mailBox.hidePages();
        $('#itemContainer').height('auto');
        mailBox.hideContentDivs();
        mailBox.hideLoadingMask();

        if (messagePage.conversation_moved) {
            TMMail.showCompleteActionHint(TMMail.action_types.move, true, 1, messagePage.dst_folder_id);
            messagePage.conversation_moved = false;
            messagePage.dst_folder_id = 0;
        }

        if (messagePage.conversation_deleted) {
            TMMail.showCompleteActionHint(TMMail.action_types.delete_messages, true, 1);
            messagePage.conversation_deleted = false;
        }

        folderPanel.markFolder(folderId);

        if (!isSortConversationByAsc()) {
            messages.reverse();
        }

        preprocessMessages(params.action, messages, folderId);

        var html = $.tmpl('messageTmpl', null, {
            messages: messages,
            last_message: lastMessage,
            important: important,
            maxShowCount: maxConversationShow,
            fileSizeToStr: AttachmentManager.GetSizeString,
            cutFileName: AttachmentManager.CutFileName,
            getFileNameWithoutExt: AttachmentManager.GetFileName,
            getFileExtension: AttachmentManager.GetFileExtension,
            htmlEncode: TMMail.htmlEncode,
            crm_available: TMMail.availability.CRM,
            wordWrap: TMMail.wordWrap
        });

        $('#itemContainer').append(html);

        showMessagesCommon(messages, params.action);

        $.each(messages, function(index, message) {
            showMessageCommon(message, params.action);
        });

        if ('draft' == params.action || 'reply' == params.action || 'forward' == params.action) {
            setEditMessageButtons();
        } else {
            setMenuActionButtons(messages.length == 1 ? TMMail.parseEmailFromFullAddress(messages[0].from) : undefined);
            setConversationViewActions();
        }

        updateAttachmentsActionMenu();

        if (1 == messages.length) {
            $('#itemContainer').find('.full-view .head').actionMenu('singleMessageActionMenu', singleMessageMenuItems, pretreatmentConversationMessage);
        } else {
            $('#itemContainer').find('.full-view .head').actionMenu('messageActionMenu', messageMenuItems, pretreatmentConversationMessage);
        }

        $('.header-crm-link').unbind('click').bind('click', showLinkChainPopup);
        serviceManager.isConversationLinkedWithCrm(messages[0].id);
        tuneNextPrev();
    }

    function updateAttachmentsActionMenu() {
        $('#itemContainer').find('.attachments').actionMenu('attachmentActionMenu', attachmentMenuItems, pretreatmentAttachments);
        bindAllAttachmentsCommonActions();
    }

    function updateEditAttachmentsActionMenu() {
        $('#mail_attachments').actionMenu('attachmentEditActionMenu', attachmentEditMenuItems, pretreatmentAttachments);
    }

    function displayTrustedImages(message, forcibly, cb) {
        if (forcibly != undefined) {
            if (forcibly) {
                displayImages(message.id, cb);
            } else if (cb) {
                cb();
            }
            return;
        }

        if (message.contentIsBlocked) {
            var senderAddress = TMMail.parseEmailFromFullAddress(message.from);
            if (trustedAddresses.isTrusted(senderAddress)) {
                message.contentIsBlocked = false;
                displayImages(message.id, cb);
            } else if ($('#id_block_content_popup_' + message.id).length == 0) {
                // Conversation sort algorithm: user has clicked the 'Display images' and #id_block_content_popup has been removed;
                displayImages(message.id, cb);
            } else {
                $('#id_block_content_popup_' + message.id).show();
                if (cb) {
                    cb();
                }
            }
        } else if (cb) {
            cb();
        }
    }

    function initIamgeZoom() {
        window.StudioManager.initImageZoom();
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

            message.fromName =
                TMMail.parseFullNameFromFullAddress(message.from).trim() ||
                    TMMail.parseEmailFromFullAddress(message.from).trim() ||
                    message.from;

            message.displayDate = window.ServiceFactory.getDisplayDate(window.ServiceFactory.serializeDate(message.receivedDate));
            message.displayTime = window.ServiceFactory.getDisplayTime(window.ServiceFactory.serializeDate(message.receivedDate));

            switch (action) {
                case 'reply':
                case 'replyAll':
                    message.isAnswered = true;
                    break;
                case 'forward':
                    message.isForwarded = true;
                    break;
            }

            if (action == 'reply' || action == 'forward' || action == 'replyAll') {
                message.mimeReplyToId = message.mimeMessageId;
                message.mimeMessageId = "";

                if (message.folder != TMMail.sysfolders.sent.id) {
                    if (action == 'replyAll') {
                        // prepare cc email adresses: get cc and add to
                        var ccs = [];
                        var emails = [];

                        var recieverEmail = TMMail.parseEmailFromFullAddress(message.address).toLowerCase();

                        $.each(message.to.split(',').concat(message.cc.split(',')), function(idx, val) {
                            if ('' == val) {
                                return;
                            }

                            var email = TMMail.parseEmailFromFullAddress(val).toLowerCase();

                            if (email == message.from || email == recieverEmail) {
                                return;
                            }

                            if (-1 != $.inArray(email, emails)) {
                                return;
                            }

                            emails.push(email);
                            ccs.push(val);
                        });

                        message.cc = ccs.join(', ');

                    } else {
                        message.cc = '';
                    }

                    var email = TMMail.parseEmailFromFullAddress(message.to).trim();
                    var account = accountsManager.getAccountByAddress(email);
                    message.to = message.from;
                    message.from = (account && !account.is_group) ? account.name + " <" + account.email + ">" : message.address;
                }

                message.id = 0;
                message.streamId = randomGuid;
            } else if (action = 'view') {
                message.from = preprocessAddresses(message.from);
                message.to = preprocessAddresses(message.to);
                if (message.cc != "") {
                    message.cc = preprocessAddresses(message.cc);
                }
                if (message.bcc != "") {
                    message.bcc = preprocessAddresses(message.bcc);
                }
            }

            message.template_name = message.folder == TMMail.sysfolders.drafts.id ? "editMessageTmpl" : "messageShortTmpl";
            message.sender_address = TMMail.parseEmailFromFullAddress(message.from);
            message.full_size = 0;

            if (message.hasAttachments) {
                var i, count;
                for (i = 0, count = message.attachments.length; i < count; i++) {
                    message.full_size += message.attachments[i].size;
                    AttachmentManager.CompleteAttachment(message.attachments[i]);

                }
                message.download_all_url = TMMail.getAttachmentsDownloadAllUrl(message.id);
            }
        }

        if (wasNewFlag && TMMail.sysfolders.drafts.id != folder) {
            folderPanel.decrementUnreadCount(folder);
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

    function preprocessAddresses(adressString) {
        var adresses = adressString.split(',');
        var count = adresses.length;
        var result = "";
        for (var i = 0; i < count; i++) {
            var addr = adresses[i].trim();
            if (TMMail.parseFullNameFromFullAddress(addr) != "") {
                result += addr;
            } else {
                var contact = contactsManager.getTLContactsByEmail(TMMail.parseEmailFromFullAddress(addr));
                if (contact != null) {
                    result += '"' + contact.firstName + ' ' + contact.lastName + '" <' + addr + '>';
                } else {
                    result += addr;
                }
            }
            if (i < count - 1) {
                result += ', ';
            }
        }
        return result;
    }

    function expandConversation(messageId) {
        var shortView = $('#itemContainer .itemWrapper .short-view[message_id=' + messageId + ']');

        if (!shortView) {
            return;
        }

        var parentWrap = shortView.closest('.message-wrap'),
            fullView = parentWrap.find('.full-view');

        shortView.removeClass('loading');
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

        $.each($('.message-wrap:hidden'), function(index, value) {
            var messageWrap = $(value);
            if (messageWrap != undefined) {
                messageWrap.show();
            }
        });
    }

    function showMessagesCommon(messages, action) {
        if ('view' == action || 'conversation' == action) {
            $('#itemContainer .head-subject .importance').unbind('click').click(function() {
                var icon = $(this).find('[class^="icon-"], [class*=" icon-"]');
                var newimportance = icon.is('.icon-unimportant');
                icon.toggleClass('icon-unimportant').toggleClass('icon-important');

                var title;
                if (newimportance) {
                    title = MailScriptResource.ImportantLabel;
                } else {
                    title = MailScriptResource.NotImportantLabel;
                }
                icon.attr('title', title);

                if (TMMail.pageIs('conversation')) {
                    var messageId = getActualConversationLastMessageId();
                    mailBox.updateConversationImportance(messageId, newimportance);

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

            $('#itemContainer').find('.full-view .from').bind('click', function() {
                messagePage.setToEmailAddresses([$(this).text()]);
                messagePage.composeTo();
            });

            if ('conversation' == action) {
                $('.collapsed-messages').click(function() {
                    showCollapsedMessages();
                });

                $.each(messages, function(index, message) {
                    if (!message.expanded) {
                        return;
                    }

                    if ('undefined' == typeof message.htmlBody) {
                        showConversationMessage(message.id, false);
                    }

                    var parentWrap = $('#itemContainer .message-wrap[message_id=' + message.id + ']'),
                        shortView = parentWrap.children('.short-view'),
                        loader = shortView.find('.loader');

                    shortView.addClass('loading');
                    loader.show();
                });
            }

            $.each(messages, function(i, v) {
                if ('undefined' != typeof v.htmlBody) {
                    expandConversation(v.id);
                }
            });

            if (messages.length > 1) {
                tuneFullView();
            }

            title = isSortConversationByAsc() ? messages[0].subject : messages[messages.length - 1].subject;
            title = title || window.MailScriptResource.NoSubject;
            TMMail.setPageHeaderTitle(title);
        } else {
            initMessagePanel(messages[0], action);
        }
    }

    function tuneFullView() {
        $('#itemContainer .itemWrapper .full-view .head').addClass('pointer');

        $('.short-view').unbind('click').click(function() {
            var shortView = $(this);
            var messageId = shortView.attr('message_id');

            if (typeof messageId != 'undefined') {
                var parentWrap = shortView.closest('.message-wrap'),
                    fullView = parentWrap.find('.full-view'),
                    loader = shortView.find('.loader');

                if (!fullView.attr('loaded')) {
                    showConversationMessage(messageId, false);
                    shortView.addClass('loading');
                    loader.show();
                } else {
                    expandConversation(messageId);
                }
            }
        });

        $('.full-view .head').unbind('click').click(function(e) {
            if ('' != ASC.Mail.getSelectionText()) {
                return;
            }
            var el = $(e.target);
            if (el.is('.menu') || el.is('.from') || el.is('.tag') || el.is('.tagDelete')
                || el.is('.AddToCRMContacts') || el.parent().is('.AddToCRMContacts')) {
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
        var head = $('.menu-action-simple-pagenav');
        head.show();
        var prevBtn = head.find('.pagerPrevButtonCSSClass');
        var nextBtn = head.find('.pagerNextButtonCSSClass');
        var cache = filterCache.getCache(MailFilter);

        if (mailBox.currentMessageId == cache.first) {
            prevBtn.remove();
        } else {
            prevBtn.attr('href', '#' + anchor + '/prev');
        }

        if (mailBox.currentMessageId == cache.last) {
            nextBtn.remove();
        } else {
            nextBtn.attr('href', '#' + anchor + '/next');
        }
    }

    function isComposeAction(action) {
        return action == 'reply' || action == 'replyAll' || action == 'forward' || action == 'draft';
    }

    function showComposeMessageCommon(message, action) {
        var title;
        switch (action) {
            case 'reply':
            case 'replyAll':
                title = window.MailScriptResource.PageHeaderReply;
                break;
            case 'forward':
                title = window.MailScriptResource.PageHeaderForward;
                break;
            case 'draft':
                title = window.MailScriptResource.PageHeaderDraft;
                break;
            case 'compose':
                title = window.MailScriptResource.NewMessage;
                break;
            default:
                // ToDo: handle unknown action here
                return;
        }
        TMMail.setPageHeaderTitle(title);
        setWysiwygEditorValue(message, action);
        $('#editMessagePage').show();
        $('#newmessageTo').trigger('input');
        setEditMessageButtons();
        setComposeFocus(message);
        setEditingMessage(message);
    }

    function setComposeFocus(message) {
        //Set focus to 1st empty field
        wysiwygEditor.setFocus();
        wysiwygEditor.unbind(wysiwygEditor.events.OnFocus).bind(wysiwygEditor.events.OnFocus, function() {
            wysiwygEditor.unbind(wysiwygEditor.events.OnFocus);
            if (message.to == '') {
                $('#newmessageTo').focus();
            } else if ($('#newmessageCopy:visible').length > 0 && message.cc == '') {
                $('#newmessageCopy').focus();
            } else if ($('#newmessageBCC:visible').length > 0 && message.bcc == '') {
                $('#newmessageBCC').focus();
            } else if ($('#newmessageSubject:visible').length > 0 && message.subject == '') {
                $('#newmessageSubject').focus();
            }
        });
    }

    function showMessageCommon(message) {
        $('#itemContainer .full-view[message_id=' + message.id + '] .AddToCRMContacts').actionPanel(
            {
                buttons: [
                    { text: window.MailScriptResource.CreateNewCRMPerson, handler: createNewCrmContact, type: 'people', from: message.from },
                    { text: window.MailScriptResource.CreateNewCRMCompany, handler: createNewCrmContact, type: 'company', from: message.from }],
                horizontal_target: 'span'
            });

        insertBody(message);
        updateMessageTags(message);
        initBlockContent(message);
        initIamgeZoom();
    }

    function pretreatmentConversationMessage(id, dropdownItemId) {
        var senderAddress = TMMail.parseEmailFromFullAddress(getFromAddress(id));
        var menu = $("#" + dropdownItemId + " .alwaysHideImages");
        if (menu.size() == 1) {
            if ($('#id_block_content_popup_' + id).size() > 0) {
                menu.hide();
            } else {
                if (trustedAddresses.isTrusted(senderAddress)) {
                    menu.text(window.MailScriptResource.HideImagesLabel + ' "' + senderAddress + '"');
                    menu.show();
                } else {
                    menu.hide();
                }
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
        var saveToMyDocsMenu = dropdownItem.find(".saveAttachmentToMyDocs");

        var menu = $('.menu[data_id="' + id + '"]');
        var name = menu.attr('name');

        if (TMMail.pageIs('writemessage')) {
            deleteMenu.show();
        }

        if (TMMail.pageIs('writemessage')) {
            var attachment = AttachmentManager.GetAttachment(id);
            if (!attachment || attachment.fileId <= 0) {
                downloadMenu.hide();
                viewMenu.hide();
                editMenu.hide();
            } else {
                downloadMenu.show();
                editMenu.hide(); // No edit document in compose/draft/reply/replyAll/forward where delete_button is visible

                if (attachment.canView || attachment.isImage) {
                    viewMenu.show();
                } else {
                    viewMenu.hide();
                }
            }
        } else {
            downloadMenu.show();
            saveToMyDocsMenu.show();

            if (TMMail.canViewInDocuments(name) || ASC.Files.Utility.CanImageView(name)) {
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

    function saveAttachmentToMyDocs(id) {
        var attachmentName = $('.row[data_id="' + id + '"] .file-name');
        serviceManager.exportAttachmentToMyDocuments(id, { fileName: attachmentName.text() }, { error: onErrorExportAttachmentsToMyDocuments }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function onExportAttachmentToMyDocuments(params) {
        window.toastr.success(window.MailScriptResource.SaveAttachmentToMyDocsSuccess.replace('%file_name%', params.fileName));
    }

    function editDocumentAttachment(id) {
        if (!ASC.Resources.Master.TenantTariffDocsEdition) {
            window.StudioBlockUIManager.blockUI("#tariffLimitDocsEditionPanel", 500, 300, 0);
            return;
        }
        window.open(TMMail.getEditDocumentUrl(id), '_blank');
    }

    function downloadAttachment(id) {
        if (TMMail.pageIs('writemessage')) {
            var attachment = AttachmentManager.GetAttachment(id);
            if (attachment != null) {
                id = attachment.fileId;
            }
        }
        window.open(TMMail.getAttachmentDownloadUrl(id), 'Download');
    }

    function viewAttachment(id) {
        if (!ASC.Resources.Master.TenantTariffDocsEdition) {
            window.StudioBlockUIManager.blockUI("#tariffLimitDocsEditionPanel", 500, 300, 0);
            return;
        }
        var name = $('.menu[data_id="' + id + '"]').attr('name');
        if (ASC.Files.Utility.CanImageView(name)) {
            $('#itemContainer .row[data_id="' + id + '"] a').click();
        } else {
            if (TMMail.pageIs('writemessage')) {
                var attachment = AttachmentManager.GetAttachment(id);
                if (attachment != null) {
                    id = attachment.fileId;
                }
            }

            window.open(TMMail.getViewDocumentUrl(id), '_blank');
        }
    }

    function deleteAttachment(id) {
        var deleteButton = $('.row[data_id="' + id + '"] .delete_attachment');
        if (deleteButton.length > 0) {
            deleteButton.click();
        }
    }

    function createNewCrmContact(event, buttonContext) {
        window.open('../../products/crm/default.aspx?action=manage&type=' + buttonContext.type +
            '&email=' + encodeURIComponent(TMMail.parseEmailFromFullAddress(buttonContext.from)) +
            '&fullname=' + encodeURIComponent(TMMail.parseFullNameFromFullAddress(buttonContext.from)), "_blank");
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

        var title = TMMail.ltgt(selectedAccount.name + " <" + selectedAccount.email + ">");

        selectFromAccount({}, {
            account: selectedAccount,
            title: title
        });
    }

    function getTags() {
        var res = [];
        $('#itemContainer .head .tags .value .itemTags .tagDelete').each(function(index, value) {
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
                function() {
                    var rootNode = $(this).closest('.attachments');
                    var messageId = rootNode.attr('message_id');
                    var attachmentsCount = rootNode.find('.row').length;
                    serviceManager.exportAllAttachmentsToMyDocuments(messageId, { count: attachmentsCount }, { error: onErrorExportAttachmentsToMyDocuments }, ASC.Resources.Master.Resource.LoadingProcessing);
                });
    }

    function onErrorExportAttachmentsToMyDocuments() {
        window.toastr.error(window.MailScriptResource.SaveAttachmentsToDocumentsFailure);
    }

    function onExportAttachmentsToMyDocuments(params, realCount) {
        window.toastr.success(window.MailScriptResource.SaveAllAttachmentsToMyDocsSuccess.replace('%real_count%', realCount).replace('%count%', params.count));
    }

    function needCrmLink() {
        return crmContactsInfo.length > 0;
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
        initImageZoom: initIamgeZoom,
        updateFromSelected: updateFromSelected,
        setToEmailAddresses: setToEmailAddresses,
        conversation_moved: conversationMoved,
        conversation_deleted: conversationDeleted,
        setHasLinked: setHasLinked,
        updateAttachmentsActionMenu: updateAttachmentsActionMenu,
        updateEditAttachmentsActionMenu: updateEditAttachmentsActionMenu,
        editDocumentAttachment: editDocumentAttachment,
        selectFromAccount: selectFromAccount,
        showMessageNotification: showMessageNotification,
        createBodyIFrame: createBodyIFrame,
        preprocessMessages: preprocessMessages,
        sendAction: sendAction,
        updateFromAccountField: updateFromAccountField
    };

})(jQuery);