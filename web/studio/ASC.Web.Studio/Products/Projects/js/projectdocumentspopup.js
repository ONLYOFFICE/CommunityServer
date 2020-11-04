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


window.ProjectDocumentsPopup = (function () {
    var isInit = false;
    var rootFolderId;
    var isAddedFile;
    var appendToListAttachFiles;

    var init = function (projectFolderId, isAddedFileMethod, appendToListAttachFilesMethod) {
        if (rootFolderId && rootFolderId !== projectFolderId) {
            isInit = false;
        }

        if (!isInit) {
            isInit = true;
            rootFolderId = projectFolderId;

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

                    attachSelectedFiles(data.files);
                },
                false);

            jq("#portalDocUploader").off("click").on("click", showPortalDocUploader);

            isAddedFile = isAddedFileMethod;
            appendToListAttachFiles = appendToListAttachFilesMethod;
        }
    };

    var showPortalDocUploader = function () {
        if (jq("#fileChoiceFrame").data("folder") != rootFolderId) {
            jq("#fileChoiceFrame").remove();

            var frameUrl = jq("#attachFrame").data("frame");
            frameUrl += "&folderid=" + encodeURIComponent(rootFolderId);
            jq("<iframe/>",
                {
                    "data-folder": rootFolderId,
                    "frameborder": 0,
                    "height": "535px",
                    "id": "fileChoiceFrame",
                    "scrolling": "no",
                    "src": frameUrl,
                    "width": "100%",
                    "onload": "javascript:ProjectDocumentsPopup.frameLoad(" + rootFolderId + ");return false;"
                })
                .appendTo("#attachFrame");
        }

        PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#popupDocumentUploader", 1002);

        jq("#attachFrame").css("visibility", "hidden");
        jq("#popupDocumentUploader .loader-page").show();
    };

    var attachSelectedFiles = function (selectedFiles) {
        var listfiles = new Array();
        for (var i = 0; i < selectedFiles.length; i++) {
            var file = selectedFiles[i];
            var fileName = file.title;
            var exttype = ASC.Files.Utility.getCssClassByFileTitle(file.title, true);
            var fileId = file.id;
            var version = file.version;
            var versionGroup = file.version_group;
            var access = file.access;

            var type;
            if (ASC.Files.Utility.CanImageView(fileName)) {
                type = "image";
            } else {
                if (ASC.Files.Utility.CanWebEdit(fileName)) {
                    type = "editedFile";
                } else {
                    if (ASC.Files.Utility.CanWebView(fileName)) {
                        type = "viewedFile";
                    } else {
                        type = "noViewedFile";
                    }
                }
            }

            if (!isAddedFile(fileName, fileId)) {

                var viewUrl = ASC.Files.Utility.GetFileDownloadUrl(fileId);
                var docEditUrl = ASC.Files.Utility.GetFileWebEditorUrl(fileId);
                var editUrl = ASC.Files.Utility.GetFileWebEditorUrl(fileId);

                var fileTmpl = {
                    title: fileName,
                    access: access,
                    type: type,
                    exttype: exttype,
                    id: fileId,
                    version: version,
                    versionGroup: versionGroup,
                    viewUrl: viewUrl,
                    editUrl: editUrl,
                    docEditUrl: docEditUrl,
                    fromProjectDocs: true,
                    trashAction: "deattach"
                };
                listfiles.push(fileTmpl);

                fileTmpl.attachFromPrjDocFlag = true;
                jq(document).trigger("addFile", fileTmpl);
            }
        }

        if (listfiles.length != 0) {
            appendToListAttachFiles(listfiles);
        }

        PopupKeyUpActionProvider.EnableEsc = true;
        jq.unblockUI();
    };

    function frameLoad (folderId) {
        jq("#fileChoiceFrame")[0].contentWindow.ASC.Files.FileChoice.eventAfter = function () {
            jq("#fileChoiceFrame")[0].contentWindow.ASC.Files.FileSelector.fileSelectorTree.displayAsRoot(folderId);

            jq("#popupDocumentUploader .loader-page").hide();
            jq("#attachFrame").css("visibility", "visible");
        };
    }

    return {
        init: init,

        frameLoad: frameLoad
    };
})(jQuery);