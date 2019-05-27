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

        var defaultOptions = {
            css: {
                marginLeft: '-500px',
                marginTop: '-323px'
            },
            bindEvents: false
        }

        StudioBlockUIManager.blockUI(el, 1000, 646, null, null, defaultOptions);

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