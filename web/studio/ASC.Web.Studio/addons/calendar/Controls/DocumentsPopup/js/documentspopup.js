/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


window.CalendarDocumentsPopup = (function ($) {
    var isInit = false,
        el,
        loader,
        frameParent,
        frameUrl,
        frame,
        supportedCustomEvents = {SelectFiles: "on_documents_selected"},
        eventsHandler = jq({});

    function initElements() {
        el = jq("#popupDocumentUploader");
        loader = el.find(".loader-page");
        frameParent = el.find("#attachFrame");
        frameUrl = frameParent.data("frame");

        jq("<iframe/>",
            {
                "frameborder": 0,
                "height": "532px",
                "id": "fileChoiceFrame",
                "scrolling": "no",
                "src": frameUrl,
                "onload": "javascript:CalendarDocumentsPopup.onFrameLoaded();return false;"
            })
            .appendTo("#attachFrame");

        frame = frameParent.find("iframe").get(0);
    }

    function attachSelectedFiles (selectedFiles, folderShareable) {
        try {
            var listfiles = [];

            for (var i = 0; i < selectedFiles.length; i++) {
                var file = selectedFiles[i];

                //from FileShareLink.GetLink
                var fileUrl = location.origin + ASC.Files.Utility.GetFileDownloadUrl(file.id);
                if (ASC.Files.Utility.CanWebView(file.title)) {
                    fileUrl = location.origin + ASC.Files.Utility.GetFileWebViewerUrl(file.id);
                }

                var fileTmpl = {
                    title: file.title,
                    id: file.id,
                    fileUrl: fileUrl,
                    size: file.content_length,
                    operation: 0,
                    isUploaded: true,
                    isNew: true,
                    error: '',
                    warn: '',
                    iconCls: ASC.Files.Utility.getCssClassByFileTitle(file.title, true),
                    shareable: !!folderShareable
                        && (!file.encrypted
                            && (file.access == ASC.Files.Constants.AceStatusEnum.None
                                || file.access == ASC.Files.Constants.AceStatusEnum.ReadWrite)),
                    denySharing: ASC.Files.UI.denySharing(file)
                };

                listfiles.push(fileTmpl);
            }

            PopupKeyUpActionProvider.EnableEsc = true;

            eventsHandler.trigger(supportedCustomEvents.SelectFiles, {data: listfiles, asLinks: true});

            jq.unblockUI();
        } catch (e) {
            console.error(e);
        }
    }

    function bindEvents () {
        window.addEventListener("message",
            function (message) {
                if (frameUrl.indexOf(message.source.location.pathname) === -1) {
                    return;
                }

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

        showLoader();
    }

    function bind (eventName, fn) {
        eventsHandler.bind(eventName, fn);
    }

    function unbind (eventName) {
        eventsHandler.unbind(eventName);
    }

    function showLoader() {
        loader.show();
        frameParent.css("visibility", "hidden");
    }

    function hideLoader() {
        loader.hide();
        frameParent.css("visibility", "visible");
    }

    function onFrameLoaded() {
        try {
            var contentWindow = frame.contentWindow;
            var fileChoice = contentWindow.ASC.Files.FileChoice;

            fileChoice.eventAfter = onLinksSelectorChanged;

            if (fileChoice.isEventAfterTriggered && fileChoice.isEventAfterTriggered()) {
                onLinksSelectorChanged();
            }
        } catch (ex) {
            onError(ex);
        }
    }

    function onLinksSelectorChanged() {
        try {
            var contentWindow = frame.contentWindow;
            var fileSelectorTree = contentWindow.ASC.Files.FileSelector.fileSelectorTree;

            if (fileSelectorTree.selectedFolderId == ASC.Files.Constants.FOLDER_ID_RECENT ||
                fileSelectorTree.selectedFolderId == ASC.Files.Constants.FOLDER_ID_FAVORITES) {
                fileSelectorTree.clickOnFolder(ASC.Files.Constants.FOLDER_ID_MY_FILES);
            }

            fileSelectorTree.getFolderData(ASC.Files.Constants.FOLDER_ID_RECENT).entryObject.toggleClass("display-none", true);
            fileSelectorTree.getFolderData(ASC.Files.Constants.FOLDER_ID_FAVORITES).entryObject.toggleClass("display-none", true);

            hideLoader();
        } catch (ex) {
            onError(ex);
        }
    };

    function onError(error) {
        console.error(error);
        hideLoader();
    }

    return {
        init: init,
        bind: bind,
        unbind: unbind,
        events: supportedCustomEvents,
        showPortalDocUploader: showPortalDocUploader,
        onFrameLoaded: onFrameLoaded
    };
})(jQuery);