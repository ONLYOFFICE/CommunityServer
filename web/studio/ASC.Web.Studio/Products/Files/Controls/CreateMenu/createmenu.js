/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
            addTop: 4
        });
    };

    var updateCreateDocList = function () {
        var unauthorizedPartner = !!jq("#UnauthorizedPartnerPanel").length;
        
        if (!ASC.Files.Utility.CanWebEdit(ASC.Files.Utility.Resource.InternalFormats.Document) || unauthorizedPartner) {
            jq("#createDocument").remove();
            jq("#emptyContainer .empty-folder-create-document").remove();
        }

        if (!ASC.Files.Utility.CanWebEdit(ASC.Files.Utility.Resource.InternalFormats.Spreadsheet) || unauthorizedPartner) {
            jq("#createSpreadsheet").remove();
            jq("#emptyContainer .empty-folder-create-spreadsheet").remove();
        }

        if (!ASC.Files.Utility.CanWebEdit(ASC.Files.Utility.Resource.InternalFormats.Presentation) || unauthorizedPartner) {
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