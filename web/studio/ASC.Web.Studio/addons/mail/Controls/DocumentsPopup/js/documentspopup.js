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


window.DocumentsPopup = (function ($) {
    var isInit = false,
        el,
        attachFilesAsLinks,
        $attachFilesAsLinksSelector,
        loader,
        supportedCustomEvents = {SelectFiles: "on_documents_selected"},
        eventsHandler = jq({});

    function initElements() {
        el = jq("#popupDocumentUploader");
        attachFilesAsLinks = el.find("#attachFilesAsLinks");
        $attachFilesAsLinksSelector = el.find("#attachFilesAsLinksSelector");
        loader = el.find(".loader-page");

        var frameUrl = jq("#attachFrame").data("frame");
        jq("<iframe/>",
            {
                "frameborder": 0,
                "height": "535px",
                "id": "fileChoiceFrame",
                "scrolling": "no",
                "src": frameUrl,
                "onload": "javascript:DocumentsPopup.hideLoader();return false;"
            })
            .appendTo("#attachFrame");
    }

    function attachSelectedFiles (selectedFiles, folderShareable) {
        try {
            var listfiles = [];

            for (var i = 0; i < selectedFiles.length; i++) {
                var file = selectedFiles[i];

                var type;
                if (ASC.Files.Utility.CanImageView(file.title)) {
                    type = "image";
                } else {
                    if (ASC.Files.Utility.CanWebEdit(file.title)) {
                        type = "editedFile";
                    } else {
                        if (ASC.Files.Utility.CanWebView(file.title)) {
                            type = "viewedFile";
                        } else {
                            type = "noViewedFile";
                        }
                    }
                }

                //from FileShareLink.GetLink
                var fileUrl = ASC.Files.Utility.GetFileDownloadUrl(file.id);
                if (ASC.Files.Utility.CanWebView(file.title)) {
                    fileUrl = ASC.Files.Utility.GetFileWebViewerUrl(file.id);
                }

                var fileTmpl = {
                    title: file.title,
                    access: file.access,
                    type: type,
                    exttype: ASC.Files.Utility.getCssClassByFileTitle(file.title),
                    id: file.id,
                    version: file.version,
                    fileUrl: fileUrl,
                    size: file.content_length,
                    shareable: !!folderShareable
                        && (!file.encrypted
                            && (file.access == ASC.Files.Constants.AceStatusEnum.None
                                || file.access == ASC.Files.Constants.AceStatusEnum.ReadWrite))
                };

                listfiles.push(fileTmpl);
            }

            PopupKeyUpActionProvider.EnableEsc = true;

            eventsHandler.trigger(supportedCustomEvents.SelectFiles, {data: listfiles, asLinks: $attachFilesAsLinksSelector.is(':checked')});

            jq.unblockUI();
        } catch (e) {
            console.error(e);
        }
    }

    function bindEvents () {
        window.addEventListener("message",
            function (message) {
                try {
                    var data = jq.parseJSON(message.data);
                } catch (e) {
                    console.error(e);
                    return;
                }

                if (!data.files) {
                    PopupKeyUpActionProvider.EnableEsc = true;
                    jq.unblockUI();
                    return;
                }

                attachSelectedFiles(data.files, data.folderShareable);
            },
            false);
    }

    function init () {
        if (!isInit) {
            isInit = true;

            initElements();
            bindEvents();
        }
    }

    function showPortalDocUploader () {
        if (!isInit) {
            init();
        }

        PopupKeyUpActionProvider.EnableEsc = false;

        StudioBlockUIManager.blockUI(el, 1000, { bindEvents: false });

        loader.show();
        attachFilesAsLinks.css("visibility", "hidden");
    }

    function bind (eventName, fn) {
        eventsHandler.bind(eventName, fn);
    }

    function unbind (eventName) {
        eventsHandler.unbind(eventName);
    }

    function hideLoader () {
        attachFilesAsLinks.css("visibility", "visible");
        loader.hide();
    }

    return {
        init: init,
        bind: bind,
        unbind: unbind,
        events: supportedCustomEvents,
        showPortalDocUploader: showPortalDocUploader,
        hideLoader: hideLoader
    };
})(jQuery);