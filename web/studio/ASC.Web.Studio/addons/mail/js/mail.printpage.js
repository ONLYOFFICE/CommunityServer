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


window.printPage = (function($) {
    var conversationId;
    var messageId;

    var viewTmpl = 'message-print-tmpl';

    var $html;
    var $pageContent;
    var $view;

    var renderedBodyFramesQueueSize;
    var loadedImagesFrameQueueSize;

    function init(conversation, id, simIds) {
        initElements();

        if (conversation) {
            conversationId = id;

            getConversation(conversationId, simIds, function (messages) {
                renderView(messages, simIds);
            });
        } else {
            messageId = id;

            getMessage(messageId, simIds, function (message) {

                renderView([message], simIds);
            });
        }
    }

    function initElements() {
        $html = $('html');
        $pageContent = $('#studioPageContent');
    }

    function getMessage(id, simIds, cb) {
        var unblocked = (simIds.indexOf(id.toString()) != -1) ? true : false;
        return serviceManager.getMessage(id, unblocked, true, {}, {
            success: function(params, message) {
                messagePage.preprocessMessages(null, [message]);
                cb(message);
            }, 
            error: function () {
                showErrorMessage();
            }
        });
    }

    function getConversation(id, simIds, cb) {
        return serviceManager.getConversation(id, true, {}, {
            success: function(params, messages) {
                messagePage.preprocessMessages(null, messages);
                cb(messages);
            },
            error: function() {
                showErrorMessage();
            }
        });
    }

    function renderView(messages, simIds) {
        $html.find('title').text(messages[0].subject);

        $view = $.tmpl(viewTmpl, { messages: messages }, {
            fileSizeToStr: AttachmentManager.GetSizeString,
            cutFileName: AttachmentManager.CutFileName,
            getFileNameWithoutExt: AttachmentManager.GetFileName,
            getFileExtension: AttachmentManager.GetFileExtension
        });
        $pageContent.append($view);

        renderedBodyFramesQueueSize = messages.length;
        loadedImagesFrameQueueSize = messages.length;

        $.each(messages, function(i, message) {
            var $messageBody = $view.find('.message-print-box[data-messageid="' + message.id + '"] .body');

            var showImages = (simIds.indexOf(message.id.toString()) != -1) ? true : false;

            var senderAddress = TMMail.parseEmailFromFullAddress(message.from);
            if (trustedAddresses.isTrusted(senderAddress)) {
                showImages = true;
            }

            var delayPrint = false;
            var content = "";
            if (message.textBodyOnly) {
                content = message.htmlBody;
            } else {
                content = messagePage.createBodyIFrame(message, showImages, renderBodyFrameHandler, loadImagesFrameHandler);
                delayPrint = true;
            }

            $messageBody.append(content);

            if (!delayPrint)
                window.print();
        });
    }

    function renderBodyFrameHandler($frame) {
        showBlockquotes($frame);

        if (--renderedBodyFramesQueueSize == 0 && loadedImagesFrameQueueSize == -1) {
            setTimeout(function() {
                window.print();
            }, 0);
        }
    }

    function loadImagesFrameHandler() {
        if (--loadedImagesFrameQueueSize == 0 ) {
            if (renderedBodyFramesQueueSize == 0) {
                setTimeout(function() {
                    window.print();
                }, 0);
            } else {
                --loadedImagesFrameQueueSize;
            }
        }
    }

    function showBlockquotes($frame) {
        var $blockquoteControlBtn = $frame.contents().find('.tl-controll-blockquote');

        $blockquoteControlBtn.click();
        $blockquoteControlBtn.remove();
    }

    function showErrorMessage() {
        toastr.error(ASC.Resources.Master.Resource.CommonJSErrorMsg);
    }

    return {
        init: init
    };
})(jQuery);