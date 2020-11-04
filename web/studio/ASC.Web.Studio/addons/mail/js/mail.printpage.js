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


window.printPage = (function($) {
    var conversationId,
        messageId,
        showImagesIds,
        showQuotesIds,
        sortAsc,
        showAll;

    var viewTmpl = 'message-print-tmpl';

    var $html;
    var $pageContent;
    var $view;
    var $title;

    function init(conversation, id, simIds, squIds, asc, displayAll) {
        showImagesIds = simIds;
        showQuotesIds = squIds;
        sortAsc = asc;
        showAll = displayAll;

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
        $pageContent = $('#studioPageContent .page-content .mainContainerClass');
        $title = $html.find('title');
    }

    function getMessage(id, cb) {
        return serviceManager.getMessage(id, false, {}, {
            success: function (params, message) {

                //set up images
                var loadImages = false;
                var showImages = showImagesIds.indexOf(message.id.toString()) != -1;
                var senderAddress = ASC.Mail.Utility.ParseAddress(message.from).email;
                if (trustedAddresses.isTrusted(senderAddress) || showImages) {
                    loadImages = true;
                }

                var res = ASC.Mail.Sanitizer.Sanitize(message.htmlBody, {
                    urlProxyHandler: ASC.Mail.Constants.PROXY_HTTP_URL,
                    needProxyHttp: ASC.Mail.Constants.NEED_PROXY_HTTP_URL,
                    loadImages: loadImages
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

                    //set up images
                    var loadImages = showAll || false;

                    if (!showAll) {
                        var showImages = showImagesIds.indexOf(message.id.toString()) != -1;
                        var senderAddress = ASC.Mail.Utility.ParseAddress(message.from).email;
                        if (trustedAddresses.isTrusted(senderAddress) || showImages) {
                            loadImages = true;
                        }
                    }

                    var res = ASC.Mail.Sanitizer.Sanitize(message.htmlBody, {
                        urlProxyHandler: ASC.Mail.Constants.PROXY_HTTP_URL,
                        needProxyHttp: ASC.Mail.Constants.NEED_PROXY_HTTP_URL,
                        loadImages: loadImages
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
        $('#firstLoader').remove();
    }

    function showErrorMessage() {
        toastr.error(ASC.Resources.Master.Resource.CommonJSErrorMsg);
    }

    return {
        init: init
    };
})(jQuery);