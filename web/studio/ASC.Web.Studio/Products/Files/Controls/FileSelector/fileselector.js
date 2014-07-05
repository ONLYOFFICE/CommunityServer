/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
window.ASC.Files.FileSelector = (function () {
    var isInit = false;
    var fileSelectorTree;
    var onSubmit = function (folderId) {
    };

    var init = function () {
        if (!isInit) {
            fileSelectorTree = new ASC.Files.TreePrototype("#fileSelectorTree");
            fileSelectorTree.clickOnFolder = checkFolder;

            jq("#fileSelectorDialog").on("click", "#submitFileSelector:not(.disable)", function () {
                ASC.Files.FileSelector.onSubmit(fileSelectorTree.selectedFolderId);

                PopupKeyUpActionProvider.CloseDialog();
            });
        }
    };

    var checkFolder = function (folderId) {
        jq("#submitFileSelector").addClass("disable");

        var folderData = fileSelectorTree.getFolderData(folderId);
        if (ASC.Files.UI.accessibleItem(folderData) && ASC.Files.Common.isCorrectId(folderId)) {
            jq("#submitFileSelector").removeClass("disable");
            return true;
        } else {
            fileSelectorTree.expandFolder(folderId);
            return false;
        }
    };

    var resetFolder = function (folderId) {
        fileSelectorTree.resetFolder(folderId);
    };

    var openDialog = function (folderId) {
        ASC.Files.UI.blockUI(jq("#fileSelectorDialog"), 440, 650);

        PopupKeyUpActionProvider.EnterAction = "jq(\"#submitFileSelector\").click();";

        fileSelectorTree.rollUp();

        fileSelectorTree.setCurrent(folderId);

        jq("#submitFileSelector").toggleClass("disable", !ASC.Files.Common.isCorrectId(fileSelectorTree.selectedFolderId));
    };

    var setTitle = function (newTitle) {
        jq("#fileSelectorTitle").text((newTitle || "").trim());
    };

    return {
        init: init,
        setTitle: setTitle,

        resetFolder: resetFolder,
        openDialog: openDialog,

        onSubmit: onSubmit,
    };
})();

(function ($) {
    $(function () {
        ASC.Files.FileSelector.init();
    });
})(jQuery);