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


window.ASC.Files.CreateMenu = (function () {
    var isInit = false;

    var init = function () {
        if (isInit === false) {
            isInit = true;
        }

        jq.dropdownToggle({
            switcherSelector: "#menuCreateNewButton:not(.disable)",
            dropdownID: "newDocumentPanel",
            inPopup: true,
            addTop: 4
        });

        jq.dropdownToggle({
            switcherSelector: "#menuUploadActionsButton:not(.disable)",
            dropdownID: "uploadActions",
            rightPos: true,
            beforeShowFunction: function () {
                jq("#buttonFolderUpload").toggleClass("disable", ASC.Files.Folders.folderContainer == "privacy");
            },
            inPopup: true,
            addTop: 4
        });

        jq.dropdownToggle({
            switcherSelector: "#createByTemplate:not(.disable)",
            dropdownID: "filesTemplatesPanel",
            sideToggle: true,
            addLeft: 4,
            beforeShowFunction: function () {
                jq("#filesTemplateLoader").show();
                jq("#filesTemplateEmpty, #filesTemplateList").hide();
                ASC.Files.Folders.getTemplateList();
            },
        });
    };

    var updateCreateDocList = function () {
        if (!ASC.Files.Utility.CanWebEdit(ASC.Files.Utility.Resource.InternalFormats.Document)) {
            jq("#createDocument").remove();
            jq("#emptyContainer .empty-folder-create-document").remove();
        }

        if (!ASC.Files.Utility.CanWebEdit(ASC.Files.Utility.Resource.InternalFormats.Spreadsheet)) {
            jq("#createSpreadsheet").remove();
            jq("#emptyContainer .empty-folder-create-spreadsheet").remove();
        }

        if (!ASC.Files.Utility.CanWebEdit(ASC.Files.Utility.Resource.InternalFormats.Presentation)) {
            jq("#createPresentation").remove();
            jq("#emptyContainer .empty-folder-create-presentation").remove();
        }

        if (!jq(".empty-folder-create-editor a").length) {
            jq(".empty-folder-create-editor").remove();
        }
    };

    var disableMenu = function (enable) {
        var listButtons = jq("#menuUploadActionsButton, #buttonUpload, #buttonFolderUpload, #createDocument, #createSpreadsheet, #createPresentation, #createNewFolder" +
            (!ASC.Files.Tree.folderIdCurrentRoot
                ? ", .page-menu .menu-actions .menu-main-button"
                : ""));

        listButtons.toggleClass("disable", !enable);
        ASC.Files.ChunkUploads.disableBrowseButton(!enable);
    };

    var toggleCreateByTemplate = function (enable) {
        jq("#createByTemplate").toggleClass("display-none", !enable);
    }

    return {
        init: init,
        updateCreateDocList: updateCreateDocList,

        disableMenu: disableMenu,
        toggleCreateByTemplate: toggleCreateByTemplate,
    };
})();

(function ($) {
    ASC.Files.CreateMenu.init();

    $(function () {
        ASC.Files.CreateMenu.updateCreateDocList();

        jq(document).on("click", "#createDocument:not(.disable), #createSpreadsheet:not(.disable), #createPresentation:not(.disable)", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.typeNewDoc = this.id.replace("create", "").toLowerCase();
            ASC.Files.Folders.createNewDoc();
        });

        jq("#emptyContainer .empty-folder-create a").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.typeNewDoc = (
                jq(this).hasClass("empty-folder-create-document")
                    ? "document"
                    : (jq(this).hasClass("empty-folder-create-spreadsheet")
                        ? "spreadsheet"
                        : (jq(this).hasClass("empty-folder-create-presentation")
                            ? "presentation"
                            : ""
                        )));
            ASC.Files.Folders.createNewDoc();
        });

        jq(document).on("click", "#createNewFolder:not(.disable), #emptyContainer .empty-folder-create-folder", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Folders.createFolder();
        });
    });
})(jQuery);