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


/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
window.messagePage = (function($) {
    var 
        is_init = false,
        save_timeout = null,
        message_is_dirty = false,
        message_is_sending = false,
        is_message_read = false,
        random_guid = '',
        sort_conversation_by_asc = true,
        max_conversation_show = 5,
        attachment_menu_items,
        attachment_edit_menu_items,
        single_message_menu_items,
        message_menu_items,
        to_email_addresses = [],
        saving_lock = false,
        repeat_save_flag = false,
        conversation_moved = false,
        conversation_deleted = false,
        has_linked = false,
        crm_contacts_info = [];

    var node_type = {
        element: 1,
        text: 3
    };

    function setHasLinked(val){
        has_linked = val;
    }

    function init() {
        if (is_init === false) {
            is_init = true;
            sort_conversation_by_asc = (TMMail.option('ConversationSortAsc') === 'true');

            serviceManager.bind(window.Teamlab.events.getMailMessage, onGetMailMessage);
            serviceManager.bind(window.Teamlab.events.getMailConversation, onGetMailConversation);
            serviceManager.bind(window.Teamlab.events.getMailMessageTemplate, onGetMailMessageTemplate);
            serviceManager.bind(window.Teamlab.events.getMailRandomGuid, onGetMailRandomGuid);
            serviceManager.bind(window.Teamlab.events.sendMailMessage, onSendMessage);
            serviceManager.bind(window.Teamlab.events.saveMailMessage, onSaveMessage);
            serviceManager.bind(window.Teamlab.events.isConversationLinkedWithCrm, onGetConversationLinkStatus);
            serviceManager.bind(window.Teamlab.events.exportAllAttachmentsToMyDocuments, onExportAttachmentsToMyDocuments);
            serviceManager.bind(window.Teamlab.events.exportAttachmentToMyDocuments, onExportAttachmentToMyDocuments);
            serviceManager.bind(window.Teamlab.events.markChainAsCrmLinked, onMarkChainAsCrmLinked);
        }

        attachment_menu_items = [
            { selector: "#attachmentActionMenu .downloadAttachment", handler: downloadAttachment },
            { selector: "#attachmentActionMenu .viewAttachment", handler: viewAttachment },
            { selector: "#attachmentActionMenu .editAttachment", handler: editDocumentAttachment },
            { selector: "#attachmentActionMenu .saveAttachmentToMyDocs", handler: saveAttachmentToMyDocs }
        ];

        attachment_edit_menu_items = [
            { selector: "#attachmentEditActionMenu .downloadAttachment", handler: downloadAttachment },
            { selector: "#attachmentEditActionMenu .viewAttachment", handler: viewAttachment },
            { selector: "#attachmentEditActionMenu .deleteAttachment", handler: deleteAttachment }
        ];

        single_message_menu_items = [
            { selector: "#singleMessageActionMenu .replyMail", handler: TMMail.moveToReply },
            { selector: "#singleMessageActionMenu .replyAllMail", handler: TMMail.moveToReplyAll },
            { selector: "#singleMessageActionMenu .forwardMail", handler: TMMail.moveToForward },
            { selector: "#singleMessageActionMenu .deleteMail", handler: deleteMessage },
            { selector: "#singleMessageActionMenu .alwaysHideImages", handler: alwaysHideImages }
        ];

        message_menu_items = [
            { selector: "#messageActionMenu .replyMail", handler: TMMail.moveToReply },
            { selector: "#messageActionMenu .replyAllMail", handler: TMMail.moveToReplyAll },
            { selector: "#messageActionMenu .singleViewMail", handler: TMMail.openMessage },
            { selector: "#messageActionMenu .forwardMail", handler: TMMail.moveToForward },
            { selector: "#messageActionMenu .deleteMail", handler: deleteMessage },
            { selector: "#messageActionMenu .alwaysHideImages", handler: alwaysHideImages }
        ];

        if (TMMail.availability.CRM) {
            single_message_menu_items.push({ selector: "#singleMessageActionMenu .exportMessageToCrm", handler: exportMessageToCrm });
            message_menu_items.push({ selector: "#messageActionMenu .exportMessageToCrm", handler: exportMessageToCrm });
        } else {
            $('.exportMessageToCrm.dropdown-item').hide();
        }

        wysiwygEditor.unbind(wysiwygEditor.events.OnChange).bind(wysiwygEditor.events.OnChange, onMessageChanged);
    }

    function hide() {
        closeMessagePanel();
    }

    function showConversationMessage(id, no_block) {
        serviceManager.getMessage(id, no_block, false, { action: 'view', conversation_message: true, message_id: id }, { error: onOpenConversationMessageError });
    }

    function view(id, no_block) {
        serviceManager.getMessage(id, no_block, false, { action: 'view' }, { error: onOpenMessageError }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function conversation(id, load_all_content) {
        var content = seekRememberedContent(id);

        if (!content)
            serviceManager.getConversation(id, load_all_content, { action: 'conversation', id: id }, { error: onOpenMessageError }, ASC.Resources.Master.Resource.LoadingProcessing);
        else
            onGetMailConversation({ action: 'conversation', id: id }, content);
    }

    function onOpenConversationMessageError(event, errors) {
        var short_view = $('#itemContainer .itemWrapper .short-view[message_id=' + event.message_id + ']');
        short_view.removeClass('loading');
        short_view.find('.loader').hide();
        window.LoadingBanner.hideLoading();
        window.toastr.error(TMMail.getErrorMessage([window.MailScriptResource.ErrorOpenMessage]));
    }

    function onOpenMessageError(event, errors) {
        TMMail.moveToInbox();
        window.LoadingBanner.hideLoading();
        window.toastr.error(TMMail.getErrorMessage([window.MailScriptResource.ErrorOpenMessage]));
    }

    function onCompose() {
        serviceManager.getMessageTemplate({}, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function onComposeTo(params) {
        var addresses;
        if (params) addresses = params.join(', ');
        else addresses = to_email_addresses.join(', ');
        serviceManager.getMessageTemplate(addresses !== undefined ? { email: addresses} : {}, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function onComposeFromCrm(params) {
        crm_contacts_info = params.contacts_info;
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
        serviceManager.getMessage(id, true, false, { action: 'draft' }, { error: onOpenMessageError }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function deleteMessage(id) {
        var chain_flag = false;
        if (TMMail.pageIs('conversation'))
            chain_flag = true;
        mailBox.deleteMessage(id, MailFilter.getFolder(), chain_flag);
    }

    function alwaysHideImages(id) {
        if (TMMail.pageIs('conversation')) {
            var sender_address = TMMail.parseEmailFromFullAddress(getFromAddress(id));
            hideImagesAction(null, { address: sender_address });
        }
    }

    function exportMessageToCrm(message_id) {
        var html = CrmLinkPopup.getCrmExportControl(message_id);
        window.popup.init();
        window.popup.addBig(window.MailScriptResource.ExportConversationPopupHeader, html);
    }

    function isContentBlocked(id) {
        var full_view = $('#itemContainer .full-view[message_id="' + id + '"]');
        var content_blocked = (full_view.length == 0 ? true : full_view.attr('content_blocked') == "true");
        return content_blocked;
    }

    function reply(id) {
        serviceManager.generateGuid();
        serviceManager.getMessage(id, !isContentBlocked(id), true, { action: 'reply' }, { error: onOpenMessageError }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function replyAll(id) {
        serviceManager.generateGuid();
        serviceManager.getMessage(id, !isContentBlocked(id), true, { action: 'replyAll' }, { error: onOpenMessageError }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function forward(id) {
        serviceManager.generateGuid();
        serviceManager.getMessage(id, !isContentBlocked(id), true, { action: 'forward' }, { error: onOpenMessageError }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    // obtains message saving "lock" flag and remembers repeat attempts
    // returns true if lock obtained and false otherwise
    function obtainSavingLock() {
        if (true === saving_lock) {
            repeat_save_flag = true;
            return false;
        } else {
            saving_lock = true;
            return true;
        }
    }

    // releases message saving lock and tries to repeat saving
    // if repeat attempt flag is set
    function releaseSavingLock() {
        saving_lock = false;
        if (repeat_save_flag) {
            repeat_save_flag = false;
            saveMessage();
        }
    }

    function saveMessage() {
        if (!obtainSavingLock())
            return;

        if (mailBox.currentMessageId < 1)
            mailBox.currentMessageId = message_id = 0;
        else
            message_id = mailBox.currentMessageId;

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
        message_is_sending = false;
    }

    function sendMessage(message_id) {
        if (message_id == undefined) {
            if (mailBox.currentMessageId < 1)
                mailBox.currentMessageId = message_id = 0;
            else
                message_id = mailBox.currentMessageId;
        }

        if (AttachmentManager.IsLoading()) {
            message_is_sending = true;
            window.AttachmentManager.Bind(window.AttachmentManager.CustomEvents.UploadComplete, onAttachmentsUploadComplete);
            window.LoadingBanner.strLoading = window.MailScriptResource.SendingMessage + ": " + window.MailScriptResource.LoadingAttachments;
            window.LoadingBanner.displayMailLoading(true, true);
            return;
        }

        if (needCrmLink()) {
            message_is_sending = true;
            saveMessage();
            return;
        }

        message_is_sending = false;

        var data = prepareMessageData(true);

        window.LoadingBanner.hideLoading();
        window.LoadingBanner.strLoading = ASC.Resources.Master.Resource.LoadingProcessing;

        if (data != undefined) {
            clearTimeout(save_timeout);
            serviceManager.sendMessage(message_id,
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
                window.MailScriptResource.SendingMessage);
        } else {
            TMMail.disableButton($('#editMessagePage .btnSend'), false);
            TMMail.disableButton($('#editMessagePage .btnSave'), false);
            if (message_id > 0) TMMail.disableButton($('#editMessagePage .btnDelete'), false);
            TMMail.disableButton($('#editMessagePage .btnAddTag'), false);
        }
    }

    function getAddressesArray(str) {
        if ('string' !== typeof str)
            return [];

        var arr = str.split(/\s*,\s*/);

        $.each(arr, function(i, v) {
            $.trim(v);
        });

        arr = $.grep(arr, function(v) {
            return '' != v;
        });

        return arr;
    };
    
    function parseAddressesByRFC(str) {
        var errors = [];
        var addresses = [];

        var email_regex = /["\'].*?["\']\s*<.*?>|<.*?>|[^\s]+@[^\s,]+/g;

        if ('string' !== typeof str || str.length == 0) {
            errors.push(window.MailScriptResource.ErrorEmptyToField);
            return { addresses: addresses, errors: errors };
        }

        var not_rfc_data = str.split(email_regex);
        $.each(not_rfc_data, function (index, value) {
            var trimmed_item = $.trim(value).replace(',', '');
            if (trimmed_item.length > 0)
                errors.push(window.MailScriptResource.ErrorIncorrectAddress + " \"" + trimmed_item + "\"");
        });
        if (errors.length > 0)
            return { addresses: addresses, errors: errors };

        //matchs: "letters + symbols" <text>, 'letters + symbols' <text>, <text>, text@text.text
        var rfc_data = str.match(email_regex);
        $.each(rfc_data, function (index, value) {
            if (TMMail.reEmail.exec(value) === null) {
                errors.push(window.MailScriptResource.ErrorIncorrectAddress + " \"" + value + "\"");
            } else
                addresses.push(value);
        });

        return { addresses: addresses, errors: errors };
    };

    function prepareMessageData(needValidation) {
        var
            to = [], cc = [], bcc = [],
            errors = [],
            to_str = $('#newmessageTo').val(),
            cc_str = $('#newmessageCopy').val(),
            bcc_str = $('#newmessageBCC').val(),
            from = $('#newmessageFromSelected').attr('mailbox_email'),
            subject = $('#newmessageSubject').val(),
            importance = $('#newmessageImportance')[0].checked,
            stream_id = $('#newMessage').attr('streamId'),
            body = '';

        body = wysiwygEditor.getValue();

        if (!needValidation) {
            to = getAddressesArray(to_str);
            cc = getAddressesArray(cc_str);
            bcc = getAddressesArray(bcc_str);
        }
        else {
            var parse_result = parseAddressesByRFC(to_str);
            errors = parse_result.errors;
            to = parse_result.addresses;
            if (errors.length == 0 && to.length == 0)
                errors.push(window.MailScriptResource.ErrorEmptyToField);

            $('#newmessageTo').css('border-color', errors.length > 0 ? '#c00' : 'null');

            if (!errors.length && cc_str.length) {
                parse_result = parseAddressesByRFC(cc_str);
                errors = parse_result.errors;
                cc = parse_result.addresses;
                $('#newmessageCopy').css('border-color', parse_result.errors.length > 0 ? '#c00' : 'null');
            } else {
                $('#newmessageCopy').css('border-color', 'null');
            }

            if (!errors.length && bcc_str.length) {
                parse_result = parseAddressesByRFC(bcc_str);
                errors = parse_result.errors;
                bcc = parse_result.addresses;
                $('#newmessageBCC').css('border-color', parse_result.errors.length > 0 ? '#c00' : 'null');
            } else {
                $('#newmessageBCC').css('border-color', 'null');
            }

            if (errors.length > 0) {
                window.LoadingBanner.hideLoading();
                var i, len = errors.length;
                for (i = 0; i < len; i++) {
                    window.toastr.error(errors[i]);
                }
                return undefined;
            }
        }

        var data = {};
        var labels_collection = $.makeArray($(".tags .itemTags a").map(function () { return parseInt($(this).attr("tagid")); }));

        var message = getEditingMessage();

        data.From = from;
        data.To = to;
        data.Cc = cc;
        data.Bcc = bcc;
        data.Subject = subject;
        data.MimeReplyToId = message.mimeReplyToId;
        data.MimeMessageId = message.mimeMessageId;
        data.Important = importance;
        data.Labels = labels_collection;
        data.HtmlBody = body;
        data.StreamId = stream_id;
        data.Attachments = AttachmentManager.GetAttachments();

        return data;
    }

    /* redraw item`s custom labels */

    function updateMessageTags(message) {
        var tags_panel = $('#itemContainer .head[message_id=' + message.id + '] .tags');
        if (tags_panel) {
            if (message.tagIds && message.tagIds.length) {
                tags_panel.find('.value .itemTags').empty();
                var found_tags = false;
                $.each(message.tagIds, function(i, value) {
                    if (updateMessageTag(message.id, value))
                        found_tags = true;
                });

                if (found_tags)
                    tags_panel.show();
            } else {
                tags_panel.hide();
            }
        }
    }

    function updateMessageTag(message_id, tag_id) {
        var tag = tagsManager.getTag(tag_id);
        if (tag != undefined) {
            var tags_panel = $('#itemContainer .head[message_id=' + message_id + '] .tags');

            if (tags_panel) {
                var html = $.tmpl('tagInMessageTmpl', tag, { htmlEncode: TMMail.htmlEncode });
                var $html = $(html);

                tags_panel.find('.value .itemTags').append($html);

                tags_panel.find('a.tagDelete').unbind('click').click(function() {
                    message_id = $(this).closest('.message-wrap').attr('message_id');
                    var id_tag = $(this).attr('tagid');
                    mailBox.unsetTag(id_tag, [message_id]);
                });

                tags_panel.show(); // show tags panel

                return true;
            }
        }
        else // if crm tag was deleted then delete it from mail
            mailBox.unsetTag(tag_id, [message_id]);

        return false;
    }

    function setTag(message_id, tag_id) {
        updateMessageTag(message_id, tag_id);
    }

    function unsetTag(message_id, tag_id) {
        var tags_panel = $('#itemContainer .head[message_id=' + message_id + '] .tags');

        if (tags_panel) {
            var del_el = tags_panel.find('.value .itemTags .tagDelete[tagid="' + tag_id + '"]');

            if (del_el.length)
                del_el.parent().remove();

            if (!tags_panel.find('.tag').length)
                tags_panel.hide();
            else
                tags_panel.show();

        }
    }

    function isMessageDirty() {
        return message_is_dirty;
    }

    function isMessageSending() {
        return message_is_sending;
    }

    function isSortConversationByAsc() {
        return sort_conversation_by_asc;
    }

    /* -= Private Methods =- */

    function insertBody(message) {

        var $message_body = $('#itemContainer .itemWrapper .body[message_id=' + message.id + ']');

        $message_body.data('message', message);
        var html;
        if (message.isBodyCorrupted) {
            html = $.tmpl('messageOpenErrorTmpl');
            $message_body.html(html);
            TMMail.fixMailtoLinks($message_body);
        }
        else if (message.hasParseError) {
            html = $.tmpl('messageParseErrorTmpl');
            $message_body.html(html);
            TMMail.fixMailtoLinks($message_body);
        }
        else {
            var iframe_supported = document.createElement("iframe");
            if (!iframe_supported || message.textBodyOnly) {

                if (message.htmlBody) {
                    var quote_regexp = new RegExp('(^|<br>|<body><pre>)&gt((?!<br>[^&gt]).)*(^|<br>)', 'ig');
                    var quote_array = quote_regexp.exec(message.htmlBody);

                    while (quote_array) {
                        var last_index = quote_regexp.lastIndex;
                        var substr = message.htmlBody.substring(quote_array.index, last_index);
                        var search_str = quote_array[0].replace(/^(<br>|<body><pre>)/, '').replace(/<br>$/, '');
                        substr = substr.replace(search_str, "<blockquote>" + search_str + "</blockquote>");
                        message.htmlBody = message.htmlBody.substring(0, quote_array.index) + substr + message.htmlBody.substr(last_index);
                        quote_array = quote_regexp.exec(message.htmlBody);
                    }
                }

                $message_body.html(message.htmlBody);

                $message_body.find('blockquote').each(function () {
                    insertBlockquoteBtn($(this));
                });

                $message_body.find('.tl-controll-blockquote').each(function () {
                    $(this).click(function () {
                        $(this).next('blockquote').toggle();
                    });
                });

                TMMail.fixMailtoLinks($message_body);
                $message_body.find("a").attr('target', '_blank');
                $("#delivery_failure_button").attr('target', '_self');
                if (message.to != undefined) {
                    $("#delivery_failure_faq_link").attr('href', TMMail.getFaqLink(message.to));
                }
                $message_body.find("a[href*='mailto:']").removeAttr('target');
                displayTrustedImages(message);
            } else {
                var $frame = createBodyIFrame(message);
                $message_body.html($frame);
            }
        }
        $('#itemContainer').height('auto');
    }

    function createBodyIFrame(message) {
        var $frame = $('<iframe id="message_body_frame_' + message.id +
        '" scrolling="auto" frameborder="0" width="100%" style="height:0;">An iframe capable browser is required to view this web site.</iframe>');

        $frame.bind('load', function() {
            $frame.unbind('load');
            var $body = $(this).contents().find('body');
            $body.css('cssText', 'height: 0;');
            var html_text = '<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">' + message.htmlBody;
            var html_body_contents = $('<div>'.concat(html_text).concat('</div>')).contents();

            // remove br tags from the end
            for (var i = html_body_contents.length - 1; i >= 0; i--) {
                var item = html_body_contents[i];
                if (item.nodeName == 'BR') {
                    html_body_contents.splice(i, 1);
                } else {
                    break;
                }
            }

            $body.append(html_body_contents.length > 0 ? html_body_contents : html_text);
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

                var $btn_blockquote = $body.find('.tl-controll-blockquote');
                if ($btn_blockquote) {
                    $btn_blockquote.click(function () {
                        $blockquote.toggle();
                        var iframe = $('#message_body_frame_' + message.id);
                        iframe.attr('scrolling', 'no');
                        window.setImmediate(function () {
                            iframe.attr('scrolling', 'auto');
                            if ($blockquote.is(':hidden')) {
                                var height_diff = $blockquote.outerHeight();
                                var new_height = parseInt(iframe.css('height').replace('px', ''), 10) - height_diff;
                                iframe.css('height', new_height + 'px');
                            }
                            else
                                updateIframeSize(message.id, true);
                        });
                    });
                }
            }

            if (message.to != undefined) {
                $body.find("a#delivery_failure_faq_link").attr('href', TMMail.getFaqLink(message.to));
            }

            $body.find("a[id='delivery_failure_button']").click(function() {
                var delivery_failure_message_id = $(this).attr("mailid");
                messagePage.edit(delivery_failure_message_id);
            });
            $body.find("a[href*='mailto:']").removeAttr('target');
            updateIframeSize(message.id, true);
            displayTrustedImages(message);
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
        if (iframe[0]== undefined) return;

        var body = iframe.contents().find('body');

        body.css('cssText', 'height: 0; max-width: ' + iframe.width() + 'px !important;');

        var contents = body.contents();
        var new_height = 0;
        var text_node_param;

        if (window.ActiveXObject || window.sidebar) { // IE, Firefox
            for (var i = 0; i < contents.length; i++) {
                var offset = 0;
                var item = contents[i];

                if (item.nodeType == node_type.element) {
                    offset = $(item).offset().top + Math.max(item.scrollHeight, item.clientHeight, $(item).outerHeight(true));
                } else if (item.nodeType == node_type.text) {
                    text_node_param = getTextNodeParams(item);
                    offset = text_node_param == null ? 0 : text_node_param.bottom + 1;
                }

                if (new_height < offset) new_height = offset;
            }
            new_height += body.offset().top;
        } else {
            new_height = iframe.contents().outerHeight(true);
            if (true == finish) new_height += body.offset().top;
        }

        // for scroll
        if (window.ActiveXObject) new_height += 15;
        else if(true == finish) new_height += 10;

        iframe.css('height', new_height + 'px');
    }

    function activateUploader(attachments) {
        var stream_id = TMMail.translateSymbols($('#newMessage').attr('streamId'), false);
        AttachmentManager.InitUploader(stream_id, attachments);
    }

    function updateFromSelected() {
        var accounts = accountsManager.getAccountList();
        if (accounts.length > 1) {
            var buttons = [];
            for (var i = 0; i < accounts.length; i++) {
                var account = accounts[i];
                var explanation = undefined;
                if (account.is_alias) explanation = window.MailScriptResource.AliasLabel;
                else if (account.is_group) continue;
               
                var title = TMMail.ltgt(account.name + " <" + account.email + ">");
                var css_class = account.enabled ? '' : 'disabled';

                var account_info = {
                    text: title,
                    explanation: explanation,
                    handler: selectFromAccount,
                    mailbox_email: account.email,
                    account_enabled: account.enabled,
                    css_class: css_class,
                    title: title,
                    signature: account.signature
                };

                buttons.push(account_info);

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
                    mailbox_email: account.email,
                    account_enabled: account.enabled,
                    title: TMMail.ltgt(account.name + " <" + account.email + ">"),
                    signature: account.signature
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
        }
        else {
            $('#AddCopy:visible').unbind('click').click(function() {
                $('.value-group.cc').show();
                $('.value-group.bcc').show();
                $('#newmessageCopy').focus();
                $(this).remove();
            });
        }
    }

    function setMenuActionButtons(sender_address) {
        is_message_read = true;

        $('.btnReply.unlockAction').unbind('click').click(function() {
            // Google Analytics
            window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.buttonClick, "reply");
            var message_id = mailBox.currentMessageId;

            if (TMMail.pageIs('conversation'))
                message_id = getActualConversationLastMessageId();

            TMMail.moveToReply(message_id);
        });

        $('.btnReplyAll.unlockAction').unbind('click').click(function() {
            // Google Analytics
            window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.buttonClick, "replyAll");
            var message_id = mailBox.currentMessageId;

            if (TMMail.pageIs('conversation'))
                message_id = getActualConversationLastMessageId();

            TMMail.moveToReplyAll(message_id);
        });

        $('.btnForward.unlockAction').unbind('click').click(function() {
            // Google Analytics
            window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.buttonClick, "forward");
            var message_id = mailBox.currentMessageId;
            var active_folder_id = $('#studio_sidePanel #foldersContainer .active').attr('folderid');

            if (TMMail.pageIs('conversation'))
                message_id = getActualConversationLastMessageId('[folder="' + active_folder_id + '"]');

            TMMail.moveToForward(message_id);
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

        var more_button = $('.btnMore');

        if (more_button != undefined) {
            var folder_id = parseInt(MailFilter.getFolder());
            var buttons = [];

            switch (folder_id) {
                case TMMail.sysfolders.inbox.id:
                    buttons.push({ text: window.MailScriptResource.SpamLabel, handler: spamUnspamAction, spam: true });
                    buttons.push({ text: window.MailScriptResource.UnreadLabel, handler: readUnreadMessageAction });
                    if(TMMail.availability.CRM)
                        buttons.push({ text: window.MailResource.LinkChainWithCRM, handler: showLinkChainPopup });
                    break;
                case TMMail.sysfolders.sent.id:
                    buttons.push({ text: window.MailScriptResource.UnreadLabel, handler: readUnreadMessageAction });
                    if(TMMail.availability.CRM)
                        buttons.push({ text: window.MailResource.LinkChainWithCRM, handler: showLinkChainPopup });
                    break;
                case TMMail.sysfolders.trash.id:
                    buttons.push({ text: window.MailScriptResource.RestoreBtnLabel, handler: restoreMessageAction });
                    buttons.push({ text: window.MailScriptResource.UnreadLabel, handler: readUnreadMessageAction });
                    break;
                case TMMail.sysfolders.spam.id:
                    buttons.push({ text: window.MailScriptResource.NotSpamLabel, handler: spamUnspamAction, spam: false });
                    buttons.push({ text: window.MailScriptResource.UnreadLabel, handler: readUnreadMessageAction });
                    break;
                default:
            }

            if (sender_address) {
                if (trustedAddresses.isTrusted(sender_address)) {
                    buttons.push({ text: window.MailScriptResource.HideImagesLabel + ' "' + sender_address + '"', handler: hideImagesAction, address: sender_address });
                }
            }

            more_button.actionPanel({ buttons: buttons, css: 'stick-over' });
        }

        // Add tag
        $('.btnAddTag.unlockAction').unbind('click').click(function() {
            tagsDropdown.show($(this));
        });
    }

    function showLinkChainPopup() {
        var html = CrmLinkPopup.getCrmLinkControl(has_linked);
        window.popup.init();
        window.popup.addBig(window.MailScriptResource.LinkConversationPopupHeader, html);
    }

    function setConversationViewActions() {
        $('#sort-conversation').toggleClass('asc', isSortConversationByAsc()).toggleClass('desc', !isSortConversationByAsc());

        $('#sort-conversation').unbind('click').click(function() {
            sort_conversation_by_asc = !isSortConversationByAsc();

            $('#sort-conversation').toggleClass('asc', isSortConversationByAsc()).toggleClass('desc', !isSortConversationByAsc());

            $('.itemWrapper').append($('.message-wrap, .collapsed-messages').get().reverse());

            //restore iframe contents
            $('iframe[id^=message_body_frame_]').each(function(){
                var value = $(this);
                var message = value.parent().data('message');
                message.contentIsBlocked = true;
                insertBody(message);
            });

            TMMail.option('ConversationSortAsc', sort_conversation_by_asc);
        });

        $('#collapse-conversation').unbind('click').click(function() {
            if ($('.full-view:hidden').length > 0) {
                showCollapsedMessages();

                $('.full-view[loaded="true"]').each(function(index, el) {
                    var message_id = $(el).attr('message_id');
                    if (typeof (message_id) !== 'undefined') {
                        expandConversation(message_id);
                    }
                });

                $('.full-view[loaded!="true"]').each(function(index, el) {
                    var message_id = $(el).attr('message_id'),
                        parent_wrap = $(el).closest('.message-wrap'),
                        short_view = parent_wrap.children('.short-view'),
                        loader = short_view.find('.loader');

                    showConversationMessage(message_id, false);
                    short_view.addClass('loading');
                    loader.show();
                });
            } else {
                $('.full-view').each(function(index, el) {
                    var message_id = $(el).attr('message_id');
                    collapseConversation(message_id);
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
                var current_sender = message.sender_address;
                trustedAddresses.add(current_sender);
                var conversation_messages = $('#itemContainer').find('.message-wrap');
                var i, len;
                for (i = 0, len = conversation_messages.length; i < len; i++) {
                    var current_message = $(conversation_messages[i]);
                    if (current_message.find('.full-view[loaded="true"]').size() == 1) {
                        // message is loaded
                        var message_id = current_message.attr('message_id');
                        var sender_address = TMMail.parseEmailFromFullAddress(getFromAddress(message_id));
                        if (current_sender == sender_address) {
                            displayImages(message_id);
                        }
                    }
                }
                setMenuActionButtons(current_sender);

            } else {
                trustedAddresses.add(message.sender_address);
                displayImages(message.id);
                setMenuActionButtons(message.sender_address);
            }
        });
    }

    function renameAttr(node, attr_name, new_attr_name) {
        node.each(function () {
            var val = node.attr(attr_name);
            node.attr(new_attr_name, val);
            node.removeAttr(attr_name);
        });
    };

    function displayImages(message_id) {
        var iframe = $('#message_body_frame_' + message_id);
        iframe.attr('scrolling', 'no');
        var message_body = iframe.contents().find('body');
        if (message_body) {
            var style_tag = message_body.find('style');
            if (style_tag.length > 0) { // style fix
                style_tag.html(style_tag.html().replace(/tl_disabled_/g, ''));
            }

            message_body.find('*').each(function(index, node) {
                $(node.attributes).each(function() {
                    if (typeof this.nodeValue === 'string')
                        this.nodeValue = this.nodeValue.replace(/tl_disabled_/g, '');

                    if (this.nodeName.indexOf('tl_disabled_') > -1)
                        renameAttr($(node), this.nodeName, this.nodeName.replace(/tl_disabled_/g, ''));
                });
            });

            $('#itemContainer .full-view[message_id=' + message_id + ']').attr('content_blocked', false);

            $('#id_block_content_popup_' + message_id).remove();

            var interval_handler = setInterval(function() {
                updateIframeSize(message_id);
            }, 50);

            message_body.waitForImages({
                finished: function() {
                    setImmediate(function () {
                        clearInterval(interval_handler);
                        updateIframeSize(message_id, true);
                        setImmediate(function () {
                            iframe.attr('scrolling', 'auto');
                            //For horizontal scroll visualisation. If you remove that block horizontal scrolling will not appears in all browsers except IE >9 and Firefox.
                            if (iframe.contents().outerWidth() > iframe.width()) {
                                message_body.append('<div class="temp" style="height: 1px"/>');
                                message_body.find('.temp').remove();
                            }
                        });
                    });
                },
                waitForAll: true
            });
        }
    }

    function hideAllActionPanels() {
        $.each($('.actionPanel:visible'), function(index, value) {
            var popup = $(value);
            if (popup != undefined)
                popup.hide();
        });
    }

    function selectFromAccount(event, params) {
        $('#newmessageFromSelected').attr('mailbox_email', params.mailbox_email);
        $('#newmessageFromSelected span').html(params.title);
        $('#newmessageFromSelected').toggleClass('disabled', !params.account_enabled);
        $('#newmessageFromWarning').toggle(!params.account_enabled);
        wysiwygEditor.setSignature(params.signature);
        accountsPanel.mark(params.mailbox_email);
    }

    function deleteMessageAttachment(attach_id) {
        serviceManager.deleteMessageAttachment(mailBox.currentMessageId, attach_id);
    }

    function deleteCurrentMessage() {
        resetDirtyMessage();
        message_is_sending = false;
        AttachmentManager.Unbind(AttachmentManager.CustomEvents.UploadComplete);
        if (mailBox.currentMessageId > 0) {
            mailBox.deleteMessage(mailBox.currentMessageId, TMMail.sysfolders.trash.id);
            mailBox.markFolderAsChanged(MailFilter.getFolder());
        }
    }

    function deleteAction() {
        if ($(this).hasClass('disable'))
            return false;
        // Google Analytics
        window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.actionClick, "delete");
        TMMail.disableButton($('#editMessagePage .btnDelete'), true);
        TMMail.disableButton($('#editMessagePage .btnSend'), true);
        TMMail.disableButton($('#editMessagePage .btnSave'), true);
        TMMail.disableButton($('#editMessagePage .btnAddTag'), true);
        deleteCurrent();
    }

    function saveAction() {
        if ($(this).hasClass('disable'))
            return false;
        // Google Analytics
        window.ASC.Mail.ga_track(ga_Categories.createMail, ga_Actions.buttonClick, "save");
        TMMail.disableButton($('#editMessagePage .btnSave'), true);
        saveMessage();
    }

    function sendAction() {
        if ($(this).hasClass('disable'))
            return false;

        if ($('#newmessageFromSelected').hasClass('disabled')) {
            window.LoadingBanner.hideLoading();
            window.toastr.warning(window.MailScriptResource.SendFromDeactivateAccount);
            return false;
        }

        //google analytics
        window.ASC.Mail.ga_track(ga_Categories.createMail, ga_Actions.buttonClick, "send");
        TMMail.disableButton($('#editMessagePage .btnSend'), true);
        TMMail.disableButton($('#editMessagePage .btnSave'), true);
        TMMail.disableButton($('#editMessagePage .btnDelete'), true);
        TMMail.disableButton($('#editMessagePage .btnAddTag'), true);
        messagePage.sendMessage();
    }

    // returns id of actual last message in chain
    // during chain viewing, messages set could be changed (ex. last message could be deleted)
    // values stored in mailBox.currentMessageId or acnhor are not valid any more
    // so actual last message will be detected from markup
    function getActualConversationLastMessageId(selector) {
        var messages = $('.itemWrapper:visible .message-wrap');

        if (selector && messages.has(selector))
            messages = messages.filter(selector);

        if (isSortConversationByAsc())
            return +messages.last().attr('message_id');

        return +messages.first().attr('message_id');
    }

    function deleteCurrent() {
        if (TMMail.pageIs('conversation'))
            mailBox.deleteConversation(getActualConversationLastMessageId(), MailFilter.getFolder());
        else
            mailBox.deleteMessage(mailBox.currentMessageId, MailFilter.getFolder());
    }

    function restoreMessageAction() {
        // Google Analytics
        window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.actionClick, "restore");

        if (TMMail.pageIs('conversation'))
            mailBox.restoreConversations([getActualConversationLastMessageId()]);
        else
            mailBox.restoreMessages([mailBox.currentMessageId]);
    }

    function hideImagesAction(event, params) {
        trustedAddresses.remove(params.address);
        ASC.Controls.AnchorController.move(ASC.Controls.AnchorController.getAnchor());
    }

    function readUnreadMessageAction() {
        is_message_read = !is_message_read;
        // Google Analytics
        if (is_message_read) window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.actionClick, "read");
        else window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.actionClick, "unread");

        if (TMMail.pageIs('conversation'))
            mailBox.setConversationReadUnread([getActualConversationLastMessageId()], is_message_read);
        else
            mailBox.setMessagesReadUnread([mailBox.currentMessageId], is_message_read);

        mailBox.updateAnchor(true, true);
    }

    function spamUnspamAction(event, params) {
        if (params.spam) {
            // Google Analytics
            window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.actionClick, "spam");
            spamCurrent();
        }
        else {
            // Google Analytics
            window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.actionClick, "not_spam");
            unspamCurrent();
        }
    }

    function spamCurrent() {
        if (TMMail.pageIs('conversation'))
            mailBox.moveConversation(getActualConversationLastMessageId(), MailFilter.getFolder(), TMMail.sysfolders.spam.id);
        else
            mailBox.moveMessage(mailBox.currentMessageId, MailFilter.getFolder(), TMMail.sysfolders.spam.id);
    }

    function unspamCurrent() {
        if (TMMail.pageIs('conversation'))
            mailBox.restoreConversations([getActualConversationLastMessageId()]);
        else
            mailBox.restoreMessages([mailBox.currentMessageId]);
    }

    function isAppleMobile() {
        if (navigator && navigator.userAgent && navigator.userAgent != null) {
            var str_user_agent = navigator.userAgent.toLowerCase();
            var arr_matches = str_user_agent.match(/(iphone|ipod|ipad)/);
            if (arr_matches)
                return true;
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

            message_is_dirty = false;

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
                if (isAppleMobile())
                    AttachmentManager.HideUpload();
                else {
                    activateUploader(message.attachments);
                    updateEditAttachmentsActionMenu();
                }
            }, 10); // Dirty trick for Opera 12

            $('#tags_panel div.tag').bind("click", messagePage.onLeaveMessage);
            $('#tags_panel div.tag').each(function() {
                var 
                    element_data = $._data(this),
                    events = element_data.events;

                var on_click_handlers = events['click'];

                // Only one handler. Nothing to change.
                if (on_click_handlers.length == 1) {
                    return;
                }

                on_click_handlers.splice(0, 0, on_click_handlers.pop());
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
        $('#newmessageSubject').bind('textchange', function () {
            // Subject has changed, then it's a new chain;
            updateReplyTo("");
            onMessageChanged();
        });
        $('#newmessageImportance').bind('click', onMessageChanged);
        $('div.itemTags').bind('DOMNodeInserted DOMNodeRemoved', onMessageChanged);
    }

    function onMessageChanged() {
        clearTimeout(save_timeout);
        setDirtyMessage();
        save_timeout = setTimeout(function() {
            if (message_is_dirty) saveMessage();
        }, TMMail.saveMessageInterval);
    }

    function onLeaveMessage(e) {
        if (TMMail.pageIs('writemessage')) {
            if (messagePage.isMessageSending()) {
                if (confirm(window.MailScriptResource.MessageNotSent)) {
                    deleteCurrentMessage();
                }
                else {
                    if (e != undefined) {
                        e.preventDefault();
                        e.stopPropagation();
                    }
                    return false;
                }
            }
            else if (isMessageDirty()) {
                saveMessage();
            }
        }
        return true;
    }

    function closeMessagePanel() {
        AttachmentManager.Unbind(AttachmentManager.CustomEvents.UploadComplete);
        clearTimeout(save_timeout);
        wysiwygEditor.close();
        save_timeout = null;

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
            message_is_dirty = false;
            $('#newMessageSaveMarker').text('');
        }
    }

    function setDirtyMessage() {
        message_is_dirty = true;
        $('#newMessageSaveMarker').text(' *');
    }

    // sets jquery or string object as wysiwig editor content
    function setWysiwygEditorValue(message, action) {
        if(action == 'reply' || action == 'replyAll')
            wysiwygEditor.setReply(message);
        else if (action == 'forward')
            wysiwygEditor.setForward(message);
        else
            wysiwygEditor.setDraft(message);
    }

    /* -= Callbacks =- */

    function onSaveMessage(params, message) {
        TMMail.disableButton($('#editMessagePage .btnSave'), false);
        TMMail.disableButton($('#editMessagePage .btnSend'), false);
        TMMail.disableButton($('#editMessagePage .btnAddTag'), false);

        mailBox.markFolderAsChanged(TMMail.sysfolders.drafts.id);

        var now = new Date();

        var min = now.getMinutes() + '';

        if (min.length == 1) min = '0' + min;

        var save_time = now.getHours() + ':' + min;

        $('.savedtime').show();

        $('.savedtime .savedtime-value').text(save_time);

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
                if (attachments.length > 0)
                    AttachmentManager.ReloadAttachments(attachments);
            }

            $('#itemContainer .head[message_id]:visible').attr('message_id', message.id);
            mailBox.currentMessageId = message.id;
            TMMail.disableButton($('#editMessagePage .btnDelete'), false);
        }

        releaseSavingLock();

        setEditingMessage(message);

        if(needCrmLink())
            serviceManager.markChainAsCrmLinked(mailBox.currentMessageId, crm_contacts_info, {}, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function onErrorSendMessage(params, error) {
        var error_limit_cnt = $('#id_block_errors_container');
        error_limit_cnt.show();
        error_limit_cnt.find('span').text(error[0]);
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

        setTimeout(function() {
            serviceManager.updateFolders();
            mailAlerts.check();
            window.LoadingBanner.hideLoading();
            window.toastr.success(window.MailScriptResource.SentMessageText);
        }, 3000);
    }

    function onGetMailMessageTemplate(params, message_template) {
        MailFilter.reset();
        closeMessagePanel();
        mailBox.hidePages();
        $('#itemContainer').height('auto');

        mailBox.hideContentDivs();
        mailBox.hideLoadingMask();

        var write_message_html = $.tmpl('writeMessageTmpl', message_template, { fileSizeToStr: AttachmentManager.GetSizeString });
        var edit_message_container = $('#itemContainer').find('#editMessagePage');
        if (edit_message_container.length > 0)
            edit_message_container.replaceWith(write_message_html);
        else {
            $('#itemContainer').append(write_message_html);
        }

        var action = 'compose';

        if (message_template.from == '') {
            var active_account = accountsPanel.getActive();
            if (active_account)
                message_template.from = active_account.email;
        }

        initMessagePanel(message_template, action);

        mailBox.currentMessageId = 0;

        if (params.email) {
            message_template.to = params.email;
            $('#newmessageTo').val(params.email);
            $('#newmessageTo').trigger('input');
        }

        showComposeMessageCommon(message_template, action);

        bindOnMessageChanged();
        
        mailBox.stickActionMenuToTheTop();
    }

    function onGetMailRandomGuid(params, guid) {
        random_guid = guid;
    }

    function onGetMailMessage(params, message) {
        var original_message = $.extend({}, message); // Copy message
        message.conversation_message = params.conversation_message;
        if (!params.conversation_message) {
            closeMessagePanel();
            hideAllActionPanels();
            mailBox.hidePages();
            $('#itemContainer').height('auto');
            mailBox.hideContentDivs();
            if(!TMMail.pageIs('writemessage')) folderPanel.markFolder(message.folder);
        }
        if (params.action == 'reply' || 'forward' == params.action || 'replyAll' == params.action) {
            mailBox.currentMessageId = 0;
        }
        mailBox.hideLoadingMask();

        if (MailFilter.getFolder() == undefined)
            MailFilter.setFolder(message.folder);

        preprocessMessages(params.action, [message], message.folder);

        var html;
        if (isComposeAction(params.action)) {
            message.original = original_message;
            if ('forward' == params.action && message.subject.indexOf(window.MailScriptResource.ForwardSubjectPrefix) != 0) {
                message.subject = window.MailScriptResource.ForwardSubjectPrefix + ": " + message.subject;
                message.to = "";
            }
            else if (('reply' == params.action || 'replyAll' == params.action) && message.subject.indexOf(window.MailScriptResource.ReplySubjectPrefix) != 0)
                message.subject = window.MailScriptResource.ReplySubjectPrefix + ": " + message.subject;

            var write_message_html = $.tmpl('writeMessageTmpl', message, { fileSizeToStr: AttachmentManager.GetSizeString });
            var edit_message_container = $('#itemContainer').find('#editMessagePage');
            if(edit_message_container.length > 0)
                edit_message_container.replaceWith(write_message_html);
            else {
                $('#itemContainer').append(write_message_html);
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
            }
            else {
                var $message_body = $('#itemContainer .itemWrapper .body[message_id=' + message.id + ']');

                if ($message_body.size() > 0) {
                    var $full_view = $message_body.parent();

                    if ($full_view.size() > 0) {
                        if ($full_view.children('.error-popup').size() < 1 && message.contentIsBlocked) {
                            message.sender_address = TMMail.parseEmailFromFullAddress(message.from);
                            if (!trustedAddresses.isTrusted(message.sender_address)) {
                                $message_body.before($.tmpl("messageBlockContent", message, {}));
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

        if (!params.conversation_message)
            $('#itemContainer').find('.full-view .head').actionMenu('singleMessageActionMenu', single_message_menu_items, pretreatmentConversationMessage);

        tuneNextPrev();

        bindOnMessageChanged();
    }

    function onGetConversationLinkStatus(params, status){
        if(status){
            $('.header-crm-link').show();
            has_linked = true;
        } else {
            has_linked = false;
        }
    }

    function rememberContent(id, content) {
        var $message_row = $('#itemContainer .messages tr[data_id=' + id + ']');
        if ($message_row) {
            var content_clone = $.extend(true, {}, { content: content });
            $message_row.data('content', content_clone);
        }
    }
    
    function seekRememberedContent(id) {
        var $message_row = $('#itemContainer .messages tr[data_id=' + id + ']');
        if ($message_row && $message_row.data('content'))
            return $message_row.data('content').content;

        return undefined;
    }

    function onGetMailConversation(params, messages) {
        var important = false;
        var folder_id = TMMail.sysfolders.inbox.id;
        var last_message = null;
        var need_remember = true;
        $.each(messages, function (i, m) {
            if (isMessageExpanded(m) && m.isBodyCorrupted) {
                need_remember = false;
            }
            
            important |= m.important;

            if (m.id == mailBox.currentMessageId) {
                folder_id = m.folder;
                last_message = m;
            }
        });

        if (last_message == null)
            return;

        if (need_remember)
            rememberContent(params.id, messages);

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

        folderPanel.markFolder(folder_id);

        if (!isSortConversationByAsc())
            messages.reverse();

        preprocessMessages(params.action, messages, folder_id);

        var html = $.tmpl('messageTmpl', null, {
            messages: messages,
            last_message: last_message,
            important: important,
            maxShowCount: max_conversation_show,
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
            $('#itemContainer').find('.full-view .head').actionMenu('singleMessageActionMenu', single_message_menu_items, pretreatmentConversationMessage);
        } else {
            $('#itemContainer').find('.full-view .head').actionMenu('messageActionMenu', message_menu_items, pretreatmentConversationMessage);
        }

        $('.header-crm-link').unbind('click').bind('click', showLinkChainPopup);
        serviceManager.isConversationLinkedWithCrm(messages[0].id);
        tuneNextPrev();
    }

    function updateAttachmentsActionMenu() {
        $('#itemContainer').find('.attachments').actionMenu('attachmentActionMenu', attachment_menu_items, pretreatmentAttachments);
        bindAllAttachmentsCommonActions();
    }

    function updateEditAttachmentsActionMenu() {
        $('#mail_attachments').actionMenu('attachmentEditActionMenu', attachment_edit_menu_items, pretreatmentAttachments);
    }

    function displayTrustedImages(message) {
        if (message.contentIsBlocked) {
            var sender_address = TMMail.parseEmailFromFullAddress(message.from);
            if (trustedAddresses.isTrusted(sender_address)) {
                message.contentIsBlocked = false;
                displayImages(message.id);
            }
            else if ($('#id_block_content_popup_' + message.id).length == 0) {
                // Conversation sort algorithm: user has clicked the 'Display images' and #id_block_content_popup has been removed;
                displayImages(message.id);
            } else
                $('#id_block_content_popup_' + message.id).show();
        }
    }

    function initIamgeZoom() {
        window.StudioManager.initImageZoom();
    }

    function isMessageExpanded(message) {
        return message.wasNew || "undefined" != typeof message.htmlBody || (!MailFilter.isBlank() && messageMatchFilter(message));
    }

    function preprocessMessages(action, messages, folder) {
        var index, len, hidden_count = 0;
        var was_new_flag = false;
        for (index = 0, len = messages.length; index < len; index++) {
            var message = messages[index];

            message.subject = message.subject || "";

            was_new_flag |= message.wasNew;

            message.expanded = isMessageExpanded(message);

            var last_or_first = (0 == index || messages.length - 1 == index);
            var next_expanded = messages.length > index + 1 && isMessageExpanded(messages[index + 1]);
            var prev_expanded = 0 != index && messages[index - 1].expanded;

            if (message.expanded || last_or_first || prev_expanded || next_expanded) {
                message.visible = true;
                if (hidden_count == 1) {
                    messages[index - 1].visible = true;
                    hidden_count = 0;
                }
                message.hidden_count = hidden_count;
                hidden_count = 0;
            } else {
                message.visible = false;
                message.hidden_count = 0;
                hidden_count += 1;
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

                        var reciever_email = TMMail.parseEmailFromFullAddress(message.address).toLowerCase();

                        $.each(message.to.split(',').concat(message.cc.split(',')), function(index, val) {
                            if ('' == val)
                                return;

                            var email = TMMail.parseEmailFromFullAddress(val).toLowerCase();

                            if (email == message.from || email == reciever_email)
                                return;

                            if (-1 != $.inArray(email, emails))
                                return;

                            emails.push(email);
                            ccs.push(val);
                        });

                        message.cc = ccs.join(', ');

                        message.to = message.from;
                        message.from = message.address;
                    } else {
                        message.to = message.from;
                        message.from = message.address;
                        message.cc = '';
                    }
                }

                message.id = 0;
                message.streamId = random_guid;
            }
            else if (action = 'view') {
                message.from = preprocessAddresses(message.from);
                message.to = preprocessAddresses(message.to);
                if (message.cc != "") message.cc = preprocessAddresses(message.cc);
                if (message.bcc != "") message.bcc = preprocessAddresses(message.bcc);
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

        if (was_new_flag && TMMail.sysfolders.drafts.id != folder) {
            folderPanel.decrementUnreadCount(folder);
        }
    }

    // check message meets for filter conditions
    function messageMatchFilter(message) {
        if (true === MailFilter.getImportance() && !message.important)
            return false;

        if (true === MailFilter.getUnread() && !message.wasNew)
            return false;

        if (true === MailFilter.getAttachments() && !message.hasAttachments)
            return false;

        var period = MailFilter.getPeriod();
        if (0 != period.from && 0 != period.to && (message.date < period.from || message.date > period.to))
            return false;

        var tags = MailFilter.getTags();
        if (tags.length > 0) {
            if (0 == message.tagIds.length)
                return false;
            for (var i = 0, len = tags.length; i < len; i++) {
                if (-1 == $.inArray(tags[i], message.tagIds))
                    return false;
            }
        }

        var search = MailFilter.getSearch();
        if ('' != search) {
            var message_text = message.from + ' ' + message.to + ' ' + message.subject;
            if (-1 == message_text.toLowerCase().indexOf(search.toLowerCase()))
                return false;
        }

        if (MailFilter.getTo() && -1 == message.to.toLowerCase().indexOf(MailFilter.getTo().toLowerCase()))
            return false;

        if (MailFilter.getFrom() && -1 == message.from.toLowerCase().indexOf(MailFilter.getFrom().toLowerCase()))
            return false;

        return true;
    }

    function preprocessAddresses(adress_string) {
        var adresses = adress_string.split(',');
        var count = adresses.length;
        var result = "";
        for (var i = 0; i < count; i++) {
            var addr = adresses[i].trim();
            if (TMMail.parseFullNameFromFullAddress(addr) != "") result += addr;
            else {
                var contact = contactsManager.getTLContactsByEmail(TMMail.parseEmailFromFullAddress(addr));
                if (contact != null)
                    result += '"' + contact.firstName + ' ' + contact.lastName + '" <' + addr + '>';
                else result += addr;
            }
            if (i < count - 1) result += ', ';
        }
        return result;
    };

    function expandConversation(message_id) {
        var short_view = $('#itemContainer .itemWrapper .short-view[message_id=' + message_id + ']');

        if (!short_view)
            return;

        var parent_wrap = short_view.closest('.message-wrap'),
            full_view = parent_wrap.find('.full-view');

        short_view.removeClass('loading');
        full_view.attr('loaded', true);
        full_view.show();
        short_view.hide();

        $('#collapse-conversation').html(($('.full-view:hidden').length > 0) ? window.MailScriptResource.ExpandAllLabel : window.MailScriptResource.CollapseAllLabel);
    }

    function collapseConversation(message_id) {
        var full_view = $('#itemContainer .itemWrapper .full-view[message_id=' + message_id + ']');

        if (!full_view)
            return;

        var parent_wrap = full_view.closest('.message-wrap'),
            short_view = parent_wrap.find('.short-view'),
            loader = short_view.find('.loader');

        loader.hide();
        short_view.show();
        full_view.hide();

        $('#collapse-conversation').html(($('.full-view:hidden').length > 0) ? window.MailScriptResource.ExpandAllLabel : window.MailScriptResource.CollapseAllLabel);
    }

    // expand collapsed in "N more" panel messages rows
    function showCollapsedMessages() {
        $('.collapsed-messages').hide();

        $.each($('.message-wrap:hidden'), function(index, value) {
            var message_wrap = $(value);
            if (message_wrap != undefined)
                message_wrap.show();
        });
    }

    function showMessagesCommon(messages, action) {
        if ('view' == action || 'conversation' == action) {
            $('#itemContainer .head-subject .importance').unbind('click').click(function() {
                var icon = $(this).find('[class^="icon-"], [class*=" icon-"]');
                var newimportance = icon.is('.icon-unimportant');
                icon.toggleClass('icon-unimportant').toggleClass('icon-important');

                var title;
                if (newimportance) title = MailScriptResource.ImportantLabel;
                else title = MailScriptResource.NotImportantLabel;
                icon.attr('title', title);
                
                if (TMMail.pageIs('conversation')) {
                    var message_id = getActualConversationLastMessageId();
                    mailBox.updateConversationImportance(message_id, newimportance);

                    var conversation_messages = mailBox.getConversationMessages();
                    var i, len = conversation_messages.length;
                    for (i = 0; i < len; i++) {
                        message_id = $(conversation_messages[i]).attr('message_id');
                        mailBox.setImportanceInCache(message_id);
                    }

                } else {
                    message_id = mailBox.currentMessageId;
                    mailBox.updateMessageImportance(message_id, newimportance);
                    mailBox.setImportanceInCache(message_id);
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
                    if (!message.expanded)
                        return;

                    if ('undefined' == typeof message.htmlBody) showConversationMessage(message.id, false);

                    var parent_wrap = $('#itemContainer .message-wrap[message_id=' + message.id + ']'),
                        short_view = parent_wrap.children('.short-view'),
                        loader = short_view.find('.loader');

                    short_view.addClass('loading');
                    loader.show();
                });
            }

            $.each(messages, function(i, v) {
                if ('undefined' != typeof v.htmlBody)
                    expandConversation(v.id);
            });

            if (messages.length > 1)
                tuneFullView();

            title = isSortConversationByAsc() ? messages[0].subject : messages[messages.length - 1].subject;
            title = title || window.MailScriptResource.NoSubject;
            TMMail.setPageHeaderTitle(title);
        }
        else {
            initMessagePanel(messages[0], action);
        }
    }

    function tuneFullView() {
        $('#itemContainer .itemWrapper .full-view .head').addClass('pointer');

        $('.short-view').unbind('click').click(function() {
            var short_view = $(this);
            var message_id = short_view.attr('message_id');

            if (typeof message_id != 'undefined') {
                var parent_wrap = short_view.closest('.message-wrap'),
                    full_view = parent_wrap.find('.full-view'),
                    loader = short_view.find('.loader');

                if (!full_view.attr('loaded')) {
                    showConversationMessage(message_id, false);
                    short_view.addClass('loading');
                    loader.show();
                } else {
                    expandConversation(message_id);
                }
            }
        });

        $('.full-view .head').unbind('click').click(function(e) {
            if ('' != ASC.Mail.getSelectionText())
                return;
            var el = $(e.target);
            if (el.is('.menu') || el.is('.from') || el.is('.tag') || el.is('.tagDelete')
                || el.is('.AddToCRMContacts') || el.parent().is('.AddToCRMContacts'))
                return;

            var message_id = $(this).parent().attr('message_id');

            if (typeof message_id != 'undefined')
                collapseConversation(message_id);
        });
    }

    function tuneNextPrev() {
        var anchor = ASC.Controls.AnchorController.getAnchor();
        var head = $('.menu-action-simple-pagenav');
        head.show();
        var prev_btn = head.find('.pagerPrevButtonCSSClass');
        var next_btn = head.find('.pagerNextButtonCSSClass');
        var cache = filterCache.getCache(MailFilter);

        if (mailBox.currentMessageId == cache.first)
            prev_btn.remove();
        else
            prev_btn.attr('href', '#' + anchor + '/prev');

        if (mailBox.currentMessageId == cache.last)
            next_btn.remove();
        else
            next_btn.attr('href', '#' + anchor + '/next');
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
        wysiwygEditor.unbind(wysiwygEditor.events.OnFocus).bind(wysiwygEditor.events.OnFocus, function () {
            wysiwygEditor.unbind(wysiwygEditor.events.OnFocus);
            if (message.to == '') {
                $('#newmessageTo').focus();
            }
            else if ($('#newmessageCopy:visible').length > 0 && message.cc == '') {
                $('#newmessageCopy').focus();
            }
            else if ($('#newmessageBCC:visible').length > 0 && message.bcc == '') {
                $('#newmessageBCC').focus();
            }
            else if ($('#newmessageSubject:visible').length > 0 && message.subject == '') {
                $('#newmessageSubject').focus();
            }
        });
    }

    function showMessageCommon(message, action) {
        $('#itemContainer .full-view[message_id=' + message.id + '] .AddToCRMContacts').actionPanel(
        {   buttons: [
                { text: window.MailScriptResource.CreateNewCRMPerson, handler: createNewCrmContact, type: 'people', from: message.from },
                { text: window.MailScriptResource.CreateNewCRMCompany, handler: createNewCrmContact, type: 'company', from: message.from }],
            horizontal_target: 'span'
        });

        insertBody(message);
        updateMessageTags(message);
        initBlockContent(message);
        initIamgeZoom();
    }

    function pretreatmentConversationMessage(id, dropdown_item_id) {
        var sender_address = TMMail.parseEmailFromFullAddress(getFromAddress(id));
        var menu = $("#" + dropdown_item_id + " .alwaysHideImages");
        if (menu.size() == 1) {
            if ($('#id_block_content_popup_' + id).size() > 0) {
                menu.hide();
            } else {
                if (trustedAddresses.isTrusted(sender_address)) {
                    menu.text(window.MailScriptResource.HideImagesLabel + ' "' + sender_address + '"');
                    menu.show();
                } else {
                    menu.hide();
                }
            }
        }
    }

    function pretreatmentAttachments(id, dropdown_item_id) {
        var dropdown_item = $("#" + dropdown_item_id);
        if (dropdown_item.length == 0)
            return;

        var view_menu = dropdown_item.find(".viewAttachment");
        var edit_menu = dropdown_item.find(".editAttachment");
        var download_menu = dropdown_item.find(".downloadAttachment");
        var delete_menu = dropdown_item.find(".deleteAttachment");
        var save_to_my_docs_menu = dropdown_item.find(".saveAttachmentToMyDocs");

        var menu = $('.menu[data_id="' + id + '"]');
        var name = menu.attr('name');

        if (TMMail.pageIs('writemessage'))
            delete_menu.show();


        if (TMMail.pageIs('writemessage')) {
            var attachment = AttachmentManager.GetAttachment(id);
            if (!attachment || attachment.fileId <= 0)
            {
                download_menu.hide();
                view_menu.hide();
                edit_menu.hide();
            }
            else {
                download_menu.show();
                edit_menu.hide(); // No edit document in compose/draft/reply/replyAll/forward where delete_button is visible

                if (attachment.canView || attachment.isImage)
                    view_menu.show();
                else
                    view_menu.hide();
            }
        }
        else {
            download_menu.show();
            save_to_my_docs_menu.show();

            if (TMMail.canViewInDocuments(name) || ASC.Files.Utility.CanImageView(name))
                view_menu.show();
            else
                view_menu.hide();

            if (!TMMail.canEditInDocuments(name))
                edit_menu.hide();
            else
                edit_menu.show();
        }
    }

    function saveAttachmentToMyDocs(id) {
        var attachment_name = $('.row[data_id="' + id + '"] .file-name');
        serviceManager.exportAttachmentToMyDocuments(id, { fileName: attachment_name.text()}, { error: onErrorExportAttachmentsToMyDocuments }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function onExportAttachmentToMyDocuments(params, id_document) {
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
            if (attachment != null)
                id = attachment.fileId;
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
                if (attachment != null)
                    id = attachment.fileId;
            }

            window.open(TMMail.getViewDocumentUrl(id), '_blank');
        }
    }

    function deleteAttachment(id) {
        var delete_button = $('.row[data_id="' + id + '"] .delete_attachment');
        if (delete_button.length > 0) {
            delete_button.click();
        }
    }

    function createNewCrmContact(event, button_context) {
        window.open('../../products/crm/default.aspx?action=manage&type=' + button_context.type +
            '&email=' + encodeURIComponent(TMMail.parseEmailFromFullAddress(button_context.from)) +
            '&fullname=' + encodeURIComponent(TMMail.parseFullNameFromFullAddress(button_context.from))
            , "_blank");
    }

    function updateFromAccountField(selected_account) {
        if (selected_account === undefined) {
            var accounts = accountsManager.getAccountList();

            if (accounts.length != 0) selected_account = accounts[0];
            for (var i = 0; i < accounts.length; i++) {
                if (accounts[i].enabled) {
                    selected_account = accounts[i];
                    break;
                }
            }
        }
        if (selected_account === undefined || selected_account == null) return;

        var title = TMMail.ltgt(selected_account.name + " <" + selected_account.email + ">");

        selectFromAccount({}, {
            mailbox_email: selected_account.email,
            title: title,
            account_enabled: selected_account.enabled,
            signature: selected_account.signature
        });

    }

    function getTags() {
        var res = [];
        $('#itemContainer .head .tags .value .itemTags .tagDelete').each(function (index, value) {
            res.push(parseInt($(value).attr('tagid')));
        });
        return res;
    }

    function hasTag(tag_id) {
        return $('#itemContainer .head:visible .tags .value .itemTags .tagDelete[tagid="' + tag_id + '"]').length;
    }

    function getFromAddress(message_id) {
        return $('#itemContainer .message-wrap[message_id="' + message_id + '"] .row .value .from').text();
    }

    function getToAddresses(message_id) {
        return $('#itemContainer .message-wrap[message_id="' + message_id + '"] .to-addresses').text();
    }

    function getMessageFolder(message_id) {
        return $('#itemContainer .message-wrap[message_id="' + message_id + '"]').attr("folder");
    }

    function getCurrentConversationIds() {
        var ids = [];
        if (!TMMail.pageIs('conversation')) return undefined;
        else {
            var messages = $('#itemContainer').find('.message-wrap');
            for (var i = 0; i < messages.length; i++) {
                ids.push($(messages[i]).attr('message_id'));
            }
        }
        return ids;
    }

    function setToEmailAddresses(emails) {
        to_email_addresses = emails;
    }

    function bindAllAttachmentsCommonActions() {
        $('#itemContainer .attachments-buttons .exportAttachemntsToMyDocs')
            .unbind('click')
            .bind('click',
                function() {
                    var root_node = $(this).closest('.attachments');
                    var message_id = root_node.attr('message_id');
                    var attachments_count = root_node.find('.row').length;
                    serviceManager.exportAllAttachmentsToMyDocuments(message_id, { count: attachments_count }, { error: onErrorExportAttachmentsToMyDocuments }, ASC.Resources.Master.Resource.LoadingProcessing);
                });
    }

    function onErrorExportAttachmentsToMyDocuments() {
        window.toastr.error(window.MailScriptResource.SaveAttachmentsToDocumentsFailure);
    }

    function onExportAttachmentsToMyDocuments(params, real_count) {
        window.toastr.success(window.MailScriptResource.SaveAllAttachmentsToMyDocsSuccess.replace('%real_count%', real_count).replace('%count%', params.count));
    }

    function needCrmLink() {
        return crm_contacts_info.length > 0;
    }

    function onMarkChainAsCrmLinked() {
        window.LoadingBanner.hideLoading();
        window.toastr.success(window.MailScriptResource.LinkConversationText);

        if (needCrmLink())
            crm_contacts_info = [];

        if (message_is_sending)
            sendMessage();
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
        conversation_moved: conversation_moved,
        conversation_deleted: conversation_deleted,
        setHasLinked: setHasLinked,
        updateAttachmentsActionMenu: updateAttachmentsActionMenu,
        updateEditAttachmentsActionMenu: updateEditAttachmentsActionMenu,
        editDocumentAttachment: editDocumentAttachment,
        selectFromAccount: selectFromAccount
    };

})(jQuery);

