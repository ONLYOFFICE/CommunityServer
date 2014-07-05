/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
window.wysiwygEditor = (function ($) {
    var editor_instance,
        supported_custom_events = { OnChange: "onchange", OnFocus: 'onfocus' },
        events_handler = $({}),
        is_editor_ready,
        signature_onload, 
        need_ck_focus, 
        new_ck_paragraph = '<p>&nbsp;</p>';

    function init() {
        close();

        var config = {
            toolbar: 'Mail',
            removePlugins: 'resize, magicline',
            filebrowserUploadUrl: 'fckuploader.ashx?newEditor=true&esid=mail',
            tabIndex: 5,
            on: {
                instanceReady: function() {
                    is_editor_ready = true;
                    if (need_ck_focus) {
                        setFocus();
                        need_ck_focus = false;
                    }
                    var body = editor_instance.document.getBody().$;
                    var button = $(body).find('.tl-controll-blockquote')[0];
                    if (button) {
                        $(button).unbind('click').bind('click', function() {
                            showQuote(this);
                        });
                        $(button).bind("contextmenu", function (event) {
                            event.stopPropagation ? event.stopPropagation() : (event.cancelBubble = true);
                        });
                    }
                },
                change: onTextChange,
                dataReady: function() {
                    if (signature_onload) {
                        insertSignature(signature_onload);
                        signature_onload = undefined;
                    }
                }
            }
        };

        editor_instance = $('#ckMailEditor').ckeditor(config).editor;
    }
    
    function showQuote(control) {
        $(control).next('blockquote').show();
        $(control).remove();
    }

    function onTextChange() {
        events_handler.trigger(supported_custom_events.OnChange);
    }

    function getValue() {
        if (editor_instance) {
            var $html = $('<div/>').append(editor_instance.getData());
            showQuote($html.find('.tl-controll-blockquote'));
            return $html.html();
        }
        return '';
    }

    function setFocus() {
        if (editor_instance) {
            if (is_editor_ready) {
                editor_instance.focus();
                events_handler.trigger(supported_custom_events.OnFocus);
            } else {
                need_ck_focus = true;
            }
        }
    }

    function setReply(message) {
        init();
        if (editor_instance) {
            var visible_qoute = false;
            if (TMMail.isIe()) visible_qoute = true;
            var html = $.tmpl('replyMessageHtmlBodyTmpl', { message: message.original, visibleQoute: visible_qoute }).get(0).outerHTML;
            editor_instance.setData(new_ck_paragraph + html);
        }
    }

    function setForward(message) {
        init();
        if (editor_instance) {
            var html = $.tmpl('forwardMessageHtmlBodyTmpl', message.original).get(0).outerHTML;
            editor_instance.setData(new_ck_paragraph + html);
        }
    }

    function setDraft(message) {
        init();
        if (editor_instance) {
            if (!TMMail.isIe() && message.htmlBody != '') {
                var $html = $('<div/>').append(message.htmlBody);
                var blockqoute = $html.find('blockquote:first');
                if (blockqoute) {
                    blockqoute.before($.tmpl('blockquoteTmpl', {}).get(0).outerHTML);
                    blockqoute.hide();
                }
                message.htmlBody = $html.html();
            }
            editor_instance.setData(message.htmlBody == '' ? new_ck_paragraph : message.htmlBody);
        }
    }

    function setSignature(signature) {
        if (signature == undefined || signature.html == undefined) return;
        if (!is_editor_ready)
            signature_onload = signature;
        else
            updateSignature(signature);
    }

    function insertSignature(signature) {
        if (signature == undefined || signature.html == undefined) return;
        if (editor_instance && signature.isActive) {
            var editor_body = $(editor_instance.document.getBody().$);

            var found_signatures = editor_body.find('> div.tlmail_signature[mailbox_id="' + signature.mailboxId + '"]');

            if (found_signatures.length == 0) {
                var html_signature = $.tmpl("composeSignatureTmpl", signature);
                html_signature.data('signature', signature);
                editor_body.append(html_signature);
            }
        }
    }

    function updateSignature(signature) {
        if (signature == undefined || signature.html == undefined) return;
        if (editor_instance) {
            var editor_body = $(editor_instance.document.getBody().$);
            var signature_container = editor_body.find('> div.tlmail_signature').last();
            if (signature_container.length > 0) {
                if (signature.isActive) {
                    var html_signature = $.tmpl("composeSignatureTmpl", signature);
                    html_signature.data('signature', signature);
                    signature_container.replaceWith(html_signature);
            } else
                    deleteSignature();
            } else {
                if (signature.isActive)
                    insertSignature(signature);
            }
        }
    }

    function deleteSignature() {
        var editor_body = $(editor_instance.document.getBody().$);
        var signature_container = editor_body.find('> div.tlmail_signature').last();
        if (signature_container.length > 0)
            signature_container.remove();
    }

    function close() {
        if (editor_instance) {
            editor_instance.removeAllListeners();
            editor_instance = undefined;
        }
        is_editor_ready = false;
        need_ck_focus = false;
    }
    
    function bind(eventName, fn) {
        return events_handler.bind(eventName, fn);
    }

    function unbind(eventName) {
        return events_handler.unbind(eventName);
    }

    return {
        init: init,
        getValue: getValue,
        setFocus: setFocus,
        setReply: setReply,
        setForward: setForward,
        setDraft: setDraft,
        close: close,
        events: supported_custom_events,
        setSignature: setSignature,
        bind: bind,
        unbind: unbind
    };

})(jQuery);