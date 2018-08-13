/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
    var showImagesIds;
    var showQuotesIds;
    var sortAsc;

    var viewTmpl = 'message-print-tmpl';

    var $html;
    var $pageContent;
    var $view;
    var $title;

    function init(conversation, id, simIds, squIds, asc) {
        showImagesIds = simIds;
        showQuotesIds = squIds;
        sortAsc = asc;

        initElements();

        if (conversation) {
            conversationId = id;
            getConversation(conversationId, function(messages) {
                if (!sortAsc) {
                    messages.reverse();
                }

                renderView(messages);
                printView();
            });
        } else {
            messageId = id;
            getMessage(messageId, function(message) {
                renderView([message]);
                printView();
            });
        }
    }

    function initElements() {
        $html = $('html');
        $pageContent = $('#studioPageContent');
        $title = $html.find('title');
    }

    function getMessage(id, cb) {
        return serviceManager.getMessage(id, false, {}, {
            success: function (params, message) {
                var res = ASC.Mail.Sanitizer.Sanitize(message.htmlBody, {
                    urlProxyHandler: ASC.Mail.Constants.PROXY_HTTP_URL,
                    needProxyHttp: ASC.Mail.Constants.NEED_PROXY_HTTP_URL,
                    loadImages: false
                });

                message.htmlBody = res.html;
                message.contentIsBlocked = res.imagesBlocked;
                message.sanitized = res.sanitized;

                messagePage.preprocessMessages(null, [message]);
                cb(message);
            },
            error: function() {
                showErrorMessage();
            }
        });
    }

    function getConversation(id, cb) {
        return serviceManager.getConversation(id, true, {}, {
            success: function (params, messages) {

                for (var i = 0, n = messages.length; i < n; i++) {
                    var message = messages[i];
                    var res = ASC.Mail.Sanitizer.Sanitize(message.htmlBody, {
                        urlProxyHandler: ASC.Mail.Constants.PROXY_HTTP_URL,
                        needProxyHttp: ASC.Mail.Constants.NEED_PROXY_HTTP_URL,
                        loadImages: false
                    });

                    message.htmlBody = res.html;
                    message.contentIsBlocked = res.imagesBlocked;
                    message.sanitized = res.sanitized;
                }

                messagePage.preprocessMessages(null, messages);
                cb(messages);
            },
            error: function() {
                showErrorMessage();
            }
        });
    }

    function renderView(messages) {
        $title.text(messages[0].subject || MailScriptResource.NoSubject);

        for (var i = 0; i < messages.length; i++) {
            messages[i].printedHtmlBody = getMessageBodyForPrint(messages[i]);
        }

        $view = $.tmpl(viewTmpl, { messages: messages }, {
            fileSizeToStr: AttachmentManager.GetSizeString,
            cutFileName: AttachmentManager.CutFileName,
            getFileNameWithoutExt: AttachmentManager.GetFileName,
            getFileExtension: AttachmentManager.GetFileExtension
        });

        hideLoader();
        $pageContent.append($view);
    }

    function getMessageBodyForPrint(message) {
        var $body = $('<div/>').html(message.htmlBody);

        // remove br tags from the end
        var $lastEl = $body.children(':last-child');
        $lastEl.prevUntil(':not(br)').remove();
        if ($lastEl.is('br')) {
            $lastEl.remove();
        }

        //set up links
        $body.find('a').attr('target', '_blank');

        //set up images
        var showImages = showImagesIds.indexOf(message.id.toString()) != -1;
        var senderAddress = ASC.Mail.Utility.ParseAddress(message.from).email;
        if (trustedAddresses.isTrusted(senderAddress) || showImages) {
            $body.find('img[tl_disabled_src]').each(function() {
                var $el = $(this);
                $el.attr('src', $el.attr('tl_disabled_src'));
            });
        }
        
        //set up quotes
        var showQuotes = showQuotesIds.indexOf(message.id.toString()) != -1;
        if (!showQuotes) {
            var $quote = $body.find('div.gmail_quote:first, div.yahoo_quoted:first, blockquote:first, div:has(hr#stopSpelling):last');
            $quote.after('<div class="quote-hidden-msg">' + MailScriptResource.QuotedTextHiddenMsg + '</div>');
            $quote.hide();
        }

        return $body.html();
    }

    function printView() {
        setTimeout(function() {
            window.print();
        }, 300);
    }

    function hideLoader() {
        $('.loader-page').remove();
    }

    function showErrorMessage() {
        toastr.error(ASC.Resources.Master.Resource.CommonJSErrorMsg);
    }

    return {
        init: init
    };
})(jQuery);