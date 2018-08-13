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
        var listButtons = jq("#buttonUpload, #createDocument, #createSpreadsheet, #createPresentation, #createNewFolder" +
            (!ASC.Files.Tree.folderIdCurrentRoot
                ? ", .page-menu .menu-actions .menu-main-button"
                : ""));

        listButtons.toggleClass("disable", !enable);
        ASC.Files.ChunkUploads.disableBrowseButton(!enable);
    };

    return {
        init: init,
        updateCreateDocList: updateCreateDocList,

        disableMenu: disableMenu
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