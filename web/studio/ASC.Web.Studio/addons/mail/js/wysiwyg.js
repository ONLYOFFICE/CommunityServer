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


window.wysiwygEditor = (function ($) {
    var editorInstance,
        supportedCustomEvents = { OnChange: "onchange", OnFocus: "onfocus" },
        eventsHandler = $({}),
        isEditorReady,
        signatureOnload,
        bodyOnload,
        needCkFocus,
        newCkParagraph = '<p style="font-family:open sans,sans-serif; font-size:12px; margin: 0;">&nbsp;</p>',
        bookmarks;

    function initHandlers() {
        if (!editorInstance ||
            !editorInstance.hasOwnProperty("document")) {
            return;
        }

        if (needCkFocus) {
            setFocus();
            needCkFocus = false;
        }

        var body = editorInstance.document.getBody().$;
        var $body = $(body);
        var button = $body.find(".tl-controll-blockquote")[0];
        if (button) {
            var $button = $(button);

            $button.unbind("click").bind("click", function () {
                showQuote(this);
            });

            var blockquote = $body.find('blockquote');

            if (blockquote.length > 1 && $(blockquote[0]).is(':hidden')) {
                $(blockquote[0]).show();
                $(blockquote[1]).before(button);
                $(blockquote[1]).hide();
            } else if ($(blockquote[0]).is(':hidden')) {
                showQuote($button);
            }

            $button.unbind("contextmenu").bind("contextmenu", function (event) {
                event.stopPropagation ? event.stopPropagation() : (event.cancelBubble = true);
            });
        }

        $body.off("click touchstart", ".delete-btn").on("click touchstart", ".delete-btn", function () {
            var $filelink = $(this).closest(".mailmessage-filelink");
            var $beforelink = $filelink.prev("p");

            $filelink.remove();
            if (!$beforelink.text().trim()) {
                $beforelink.remove();
            }

            eventsHandler.trigger(supportedCustomEvents.OnChange);
        });

        $body.off("click", "a").on("click", "a", function () {
            var el = $(this);

            var title = el.attr("title");
            var fId = el.attr("data-fileid");

            if (ASC.Files.MediaPlayer && title && fId
                && (ASC.Files.MediaPlayer.canPlay(title) || ASC.Files.MediaPlayer.canViewImage(title))) {

                var playlist = [];
                var selIndex = 0;

                $body.find(".mailmessage-filelink-link").each(function (i, v) {
                    var attachTitle = v.title;
                    var attachId = $(v).attr("data-fileid");

                    if (attachTitle && attachId
                        && (ASC.Files.MediaPlayer.canPlay(attachTitle) || ASC.Files.MediaPlayer.canViewImage(attachTitle))) {

                        var correctId = playlist.length;
                        playlist.push({ title: attachTitle, id: correctId, src: ASC.Files.Utility.GetFileDownloadUrl(attachId) });
                        if (attachId == fId)
                            selIndex = correctId;
                    }
                });

                if (playlist.length) {
                    ASC.Files.MediaPlayer.init(-1, {
                        playlist: playlist,
                        playlistPos: selIndex,
                        downloadAction: function (fileId) {
                            return playlist[fileId].src;
                        }
                    });

                    return;
                }
            }

            window.open($(this).attr("href"));
        });

        $body.find(".mailmessage-filelink-link .file-name").dotdotdot({ wrap: "letter", height: 18 });
    }

    function init() {
        close();

        var config = {
            toolbar: "Mail",
            removePlugins: "magicline",
            filebrowserUploadUrl: "fckuploader.ashx?newEditor=true&esid=mail",
            tabIndex: 5,
            resize_dir: "vertical",
            on: {
                change: onTextChange,
                beforeSetMode: function () {
                    if (editorInstance.mode !== "source") {
                        var selection = editorInstance.getSelection();
                        if (selection) {
                            bookmarks = selection.createBookmarks(true);
                        }
                    }
                },
                mode: function () {
                    if (editorInstance.mode === "wysiwyg" && bookmarks) {
                        var selection = editorInstance.getSelection();
                        if (selection) {
                            editorInstance.focus();
                            selection.selectBookmarks(bookmarks);
                        }
                    }

                    initHandlers();
                }
            }
        };

        ckeditorConnector.load(function () {
            editorInstance = $("#ckMailEditor").ckeditor(config).editor;

            isEditorReady = true;

            if (bodyOnload) {
                setBody(bodyOnload);
            }
        });
    }

    function showQuote(control) {
        $(control).parents().find("blockquote:hidden").show();
        $(control).remove();
    }

    function onTextChange() {
        eventsHandler.trigger(supportedCustomEvents.OnChange);
    }

    function getValue() {
        if (editorInstance) {
            var $html = $("<div/>").append(editorInstance.getData());
            showQuote($html.find(".tl-controll-blockquote"));
            return $html.html() || newCkParagraph;
        }
        return "";
    }

    function setFocus() {
        if (editorInstance) {
            if (isEditorReady) {
                editorInstance.focus();
                eventsHandler.trigger(supportedCustomEvents.OnFocus);
            } else {
                needCkFocus = true;
            }
        }
    }

    function setReply(message) {
        init();

        var visibleQoute = false;
        if (TMMail.isIe()) {
            visibleQoute = true;
        }
        var html = $.tmpl("replyMessageHtmlBodyTmpl", { message: message.original, visibleQoute: visibleQoute })
            .get(0)
            .outerHTML;

        setBody(newCkParagraph + html);
    }

    function setForward(message) {
        init();

        var html = $.tmpl("forwardMessageHtmlBodyTmpl", message.original).get(0).outerHTML;
        setBody(newCkParagraph + html);
    }

    function setDraft(message) {
        init();

        if (!TMMail.isIe() && message.htmlBody !== "") {
            var $html = $("<div/>").append(message.htmlBody);
            var blockqoute = $html.find("blockquote:first");
            if (blockqoute) {
                blockqoute.before($.tmpl("blockquoteTmpl", {}).get(0).outerHTML);
                blockqoute.hide();
            }
            message.htmlBody = $html.html();
        }

        setBody(message.htmlBody === "" ? newCkParagraph : message.htmlBody);
    }

    function setBody(body) {
        if (!body) {
            return;
        }
        if (!isEditorReady) {
            bodyOnload = body;
        } else {
            if (!editorInstance) {
                console.error("wysiwyg->setBody: editorInstance is undefined");
                return;
            }

            editorInstance
                .once("dataReady",
                    function() {
                        if (signatureOnload) {
                            insertSignature(signatureOnload);
                            signatureOnload = undefined;
                        }

                        initHandlers();

                        setFocus();
                    });

            editorInstance.setData(body);
            bodyOnload = undefined;
        }
    }

    function setSignature(signature) {
        if (signature == undefined || signature.html == undefined) {
            return;
        }
        if (!isEditorReady) {
            signatureOnload = signature;
        } else {
            updateSignature(signature);
        }
    }

    function insertSignature(signature) {
        if (!signature ||
            !signature.hasOwnProperty("html") ||
            !signature.isActive ||
            !editorInstance ||
            !editorInstance.hasOwnProperty("document")) {
            return;
        }

        var editorBody = $(editorInstance.document.getBody().$);

        var foundSignatures = editorBody.find('> div.tlmail_signature[mailbox_id="' + signature.mailboxId + '"]');

        if (foundSignatures.length === 0) {
            var htmlSignature = $.tmpl("composeSignatureTmpl", signature);
            htmlSignature.data("signature", signature);
            var blockquote = editorBody.find(".reply-text").first();
            if (blockquote.length === 0) {
                blockquote = editorBody.find(".forward-text").first();
            }

            if (blockquote.length === 0) {
                editorBody.append(htmlSignature);
            } else {
                $(newCkParagraph).insertBefore(blockquote.first());
                htmlSignature.insertBefore(blockquote.first());
                $(newCkParagraph).insertBefore(blockquote.first());
            }
        }
    }

    function updateSignature(signature) {
        if (!signature ||
            !signature.hasOwnProperty("html") ||
            !editorInstance ||
            !editorInstance.hasOwnProperty("document")) {
            return;
        }
        if (editorInstance && editorInstance.document) {
            var editorBody = $(editorInstance.document.getBody().$);
            var signatureContainer = editorBody.find("> div.tlmail_signature").last();
            if (signatureContainer.length > 0) {
                if (signature.isActive) {
                    var htmlSignature = $.tmpl("composeSignatureTmpl", signature);
                    htmlSignature.data("signature", signature);
                    signatureContainer.replaceWith(htmlSignature);
                } else {
                    deleteSignature();
                }
            } else {
                if (signature.isActive) {
                    insertSignature(signature);
                }
            }
        }
    }

    function deleteSignature() {
        if (!editorInstance ||
            !editorInstance.hasOwnProperty("document")) {
            return;
        }

        var editorBody = $(editorInstance.document.getBody().$);
        var signatureContainer = editorBody.find("> div.tlmail_signature").last();
        if (signatureContainer.length > 0) {
            signatureContainer.remove();
        }
    }

    function close() {
        if (editorInstance) {
            editorInstance.removeAllListeners();
            editorInstance = undefined;
        }
        isEditorReady = false;
        needCkFocus = false;
        bookmarks = undefined;
    }

    function bind(eventName, fn) {
        return eventsHandler.bind(eventName, fn);
    }

    function unbind(eventName) {
        return eventsHandler.unbind(eventName);
    }

    function insertFileLinks(files) {
        if (!files ||
            !files.length ||
            !editorInstance ||
            !editorInstance.hasOwnProperty("document")) {
            return;
        }

        var templates = $.tmpl("messageFileLink", files);

        if (editorInstance.mode === "wysiwyg") {
            var body = editorInstance.document.getBody().$;
            if (editorInstance.focusManager.hasFocus) {
                var $pos = $(editorInstance.getSelection().getStartElement().$);
                templates.insertBefore($pos);
            } else {
                var otherLinks = $(body).find(".mailmessage-filelink");
                var lastEl;
                if (otherLinks.length > 0) {
                    lastEl = otherLinks.last();
                } else {
                    lastEl = $(body).find("p").first();
                }

                templates.insertAfter(lastEl);
            }
            setFocus();
            $(body).find(".mailmessage-filelink-link .file-name").dotdotdot({ wrap: "letter", height: 18 });
        } else {
            editorInstance.setMode("wysiwyg", function () {
                setFocus();
                insertFileLinks(files);
                editorInstance.setMode("source");
                return false;
            });
        }
        if (!TMMail.isTemplate()) {
            window.messagePage.saveMessage();
        }
    }

    return {
        init: init,
        getValue: getValue,
        setFocus: setFocus,
        setReply: setReply,
        setForward: setForward,
        setDraft: setDraft,
        close: close,
        events: supportedCustomEvents,
        setSignature: setSignature,
        bind: bind,
        unbind: unbind,
        insertFileLinks: insertFileLinks
    };

})(jQuery);