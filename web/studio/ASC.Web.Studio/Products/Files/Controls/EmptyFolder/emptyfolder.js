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

window.ASC.Files.EmptyScreen = (function () {
    var isInit = false;

    var init = function () {
        if (isInit === false) {
            isInit = true;
        }
        jq.dropdownToggle({
            switcherSelector: "#emptyContainer .hintCreate",
            dropdownID: "hintCreatePanel",
            fixWinSize: false
        });

        jq.dropdownToggle({
            switcherSelector: "#emptyContainer .hintUpload",
            dropdownID: "hintUploadPanel",
            fixWinSize: false
        });

        jq.dropdownToggle({
            switcherSelector: "#emptyContainer .hintOpen",
            dropdownID: "hintOpenPanel",
            fixWinSize: false
        });

        jq.dropdownToggle({
            switcherSelector: "#emptyContainer .hintEdit",
            dropdownID: "hintEditPanel",
            fixWinSize: false
        });
    };

    var displayEmptyScreen = function () {
        ASC.Files.UI.hideAllContent(true);
        if (ASC.Files.Mouse) {
            ASC.Files.Mouse.finishMoveTo();
            ASC.Files.Mouse.finishSelecting();
        }

        jq("#filesMainContent, #switchViewFolder, #mainContentHeader, #pageNavigatorHolder, .folder-row-toparent").hide();
        jq("#emptyContainer > div").hide();

        if (!ASC.Files.Filter || ASC.Files.Filter.getFilterSettings().filter == 0 && ASC.Files.Filter.getFilterSettings().text == "") {
            jq(".files-filter").hide();

            jq("#emptyContainer .empty-folder-create").toggle(ASC.Files.UI.accessEdit());

            jq("#emptyContainer_" + ASC.Files.Folders.folderContainer).show();

            ASC.Files.UI.checkButtonBack(".empty-folder-toparent");
        } else {
            jq("#emptyContainer_filter").show();
        }

        jq("#emptyContainer").show();
        ASC.Files.UI.stickContentHeader();
    };

    var hideEmptyScreen = function () {
        if (jq("#filesMainContent").is(":visible")) {
            return;
        }
        ASC.Files.UI.hideAllContent(true);
        if (ASC.Files.Mouse) {
            ASC.Files.Mouse.finishMoveTo();
            ASC.Files.Mouse.finishSelecting();
        }

        jq("#emptyContainer").hide();

        ASC.Files.UI.checkButtonBack(".to-parent-folder", ".folder-row-toparent");

        jq(".files-filter").show();
        if (ASC.Files.Filter) {
            ASC.Files.Filter.resize();
        }

        jq("#filesMainContent, #switchViewFolder, #mainContentHeader").show();
        ASC.Files.UI.stickContentHeader();
    };

    return {
        init: init,

        hideEmptyScreen: hideEmptyScreen,
        displayEmptyScreen: displayEmptyScreen
    };
})();

(function ($) {
    ASC.Files.EmptyScreen.init();
    $(function () {
    });
})(jQuery);