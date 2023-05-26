/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
        filesSettings = null,
        el,
        attachFilesAsLinks,
        $attachFilesAsLinksSelector,
        loader,
        frameParent,
        frameUrl,
        frame,
        supportedCustomEvents = {SelectFiles: "on_documents_selected"},
        eventsHandler = jq({});

    function initElements() {
        el = jq("#popupDocumentUploader");
        attachFilesAsLinks = el.find("#attachFilesAsLinks");
        $attachFilesAsLinksSelector = el.find("#attachFilesAsLinksSelector");
        loader = el.find(".loader-page");
        frameParent = el.find("#attachFrame");
        frameUrl = frameParent.data("frame");

        el.toggleClass("without-links", attachFilesAsLinks.length == 0);

        jq("<iframe/>",
            {
                "frameborder": 0,
                "height": "532px",
                "id": "fileChoiceFrame",
                "scrolling": "no",
                "src": frameUrl,
                "onload": "javascript:DocumentsPopup.onFrameLoaded();return false;"
            })
            .appendTo(frameParent);

        frame = frameParent.find("iframe").get(0);
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

                var shareable = !!folderShareable || file.folder_id == ASC.Files.Constants.FOLDER_ID_MY_FILES;

                var fileTmpl = {
                    title: file.title,
                    access: file.access,
                    denyDownload: ASC.Files.UI.denyDownload(file),
                    denySharing: ASC.Files.UI.denySharing(file),
                    type: type,
                    exttype: ASC.Files.Utility.getCssClassByFileTitle(file.title),
                    id: file.id,
                    version: file.version,
                    fileUrl: fileUrl,
                    size: file.content_length,
                    shareable: shareable
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
                if (!message || typeof message.data != "string") {
                    return;
                }

                if (frameUrl.indexOf(message.source.location.pathname) === -1) {
                    return;
                }

                try {
                    var data = JSON.parse(message.data);
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

        $attachFilesAsLinksSelector.on("change", onLinksSelectorChanged);
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

    function showLoader () {
        loader.show();
        attachFilesAsLinks.css("visibility", "hidden");
        frameParent.css("visibility", "hidden");
    }

    function hideLoader () {
        loader.hide();
        attachFilesAsLinks.css("visibility", "visible");
        frameParent.css("visibility", "visible");
    }

    function bind (eventName, fn) {
        eventsHandler.on(eventName, fn);
    }

    function unbind (eventName) {
        eventsHandler.off(eventName);
    }

    function onFrameLoaded() {
        try {
            filesSettings = null;

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
            var checked = $attachFilesAsLinksSelector.is(':checked');
            var contentWindow = frame.contentWindow;
            var fileSelectorTree = contentWindow.ASC.Files.FileSelector.fileSelectorTree;

            if (checked && (fileSelectorTree.selectedFolderId == ASC.Files.Constants.FOLDER_ID_RECENT || fileSelectorTree.selectedFolderId == ASC.Files.Constants.FOLDER_ID_FAVORITES)) {
                fileSelectorTree.clickOnFolder(ASC.Files.Constants.FOLDER_ID_MY_FILES);
            }

            var recentData = fileSelectorTree.getFolderData(ASC.Files.Constants.FOLDER_ID_RECENT);
            var favoritesData = fileSelectorTree.getFolderData(ASC.Files.Constants.FOLDER_ID_FAVORITES);

            initFilesSettings(recentData, favoritesData);

            if (filesSettings.displayRecent) {
                recentData.entryObject.toggleClass("display-none", checked);
            }

            if (filesSettings.displayFavorites) {
                favoritesData.entryObject.toggleClass("display-none", checked);
            }

            hideLoader();
        } catch (ex) {
            onError(ex);
        }
    };

    function initFilesSettings(recentData, favoritesData) {
        if (!filesSettings) {
            filesSettings = {
                displayRecent: recentData ? !recentData.entryObject.hasClass("display-none") : false,
                displayFavorites: favoritesData ? !favoritesData.entryObject.hasClass("display-none") : false
            };
        }
    }

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
        onFrameLoaded: onFrameLoaded,
    };
})(jQuery);