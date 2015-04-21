/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


window.ASC.Files.Tree = (function () {
    var isInit = false;
    var treeViewContainer;
    var treeViewSelector;

    var pathParts = new Array();
    var folderIdCurrentRoot = null;

    var init = function () {
        if (isInit === false) {
            isInit = true;

            var rootId = null;
            if (jq(".files-content-panel").attr("data-rootid")) {
                ASC.Files.Tree.folderIdCurrentRoot = jq(".files-content-panel").attr("data-rootid");
                rootId = ASC.Files.Constants.FOLDER_ID_PROJECT;
            }

            treeViewContainer = new ASC.Files.TreePrototype("#treeViewContainer", rootId);
            treeViewContainer.clickOnFolder = gotoFolder;
            if (rootId) {
                treeViewContainer.expandFolder(rootId);
            }

            treeViewSelector = new ASC.Files.TreePrototype("#treeViewSelector");
            treeViewSelector.clickOnFolder = movetoFolder;
        }
    };

    var getFolderTitle = function (folderId) {
        return treeViewContainer.getFolderTitle(folderId) || treeViewSelector.getFolderTitle(folderId);
    };

    var getParentId = function (folderId) {
        return treeViewContainer.getParentId(folderId);
    };

    var getFolderData = function (folderId) {
        return treeViewContainer.getFolderData(folderId);
    };

    var updateTreePath = function () {
        for (var i = 0; i < ASC.Files.Tree.pathParts.length - 1; i++) {
            var parentId = ASC.Files.Tree.pathParts[i];
            treeViewContainer.expandFolder(parentId, true, true);
            if (!treeViewContainer.getFolderData(ASC.Files.Tree.pathParts[i + 1])) {
                treeViewContainer.resetFolder(parentId);
                i--;
            }
        }

        treeViewContainer.setCurrent(ASC.Files.Folders.currentFolder.id);
        if (ASC.Files.Folders.currentFolder.id != ASC.Files.Constants.FOLDER_ID_TRASH) {
            treeViewSelector.setCurrent(ASC.Files.Folders.currentFolder.id);
        }
    };

    var showSelect = function (folderId) {
        treeViewSelector.rollUp();
        treeViewSelector.setCurrent(folderId);
    };

    var getPathTitle = function (folderId) {
        var folderTitle = treeViewSelector.getFolderTitle(folderId);
        var path = treeViewSelector.getPath(folderId);

        var pathId = jq(path).map(function (i, fId) {
            return treeViewSelector.getFolderTitle(fId);
        });
        pathId.push(folderTitle);
        var pathTitle = pathId.toArray().join(" > ");

        return pathTitle;
    };

    var resetFolder = function (folderId) {
        if (!ASC.Files.Common.isCorrectId(folderId)) {
            return;
        }

        treeViewContainer.resetFolder(folderId);
        treeViewSelector.resetFolder(folderId);
    };

    var gotoFolder = function (folderId) {
        if (ASC.Files.Tree.folderIdCurrentRoot
            && ASC.Files.Tree.folderIdCurrentRoot != ASC.Files.Constants.FOLDER_ID_PROJECT) {

            var toParent = jq.inArray(folderId + "", ASC.Files.Tree.pathParts) != -1;
            if (!toParent) {
                var path = treeViewContainer.getPath(folderId);
                var toChild = (jq.inArray(ASC.Files.Folders.currentFolder.id, path) != -1);

                if (!toChild) {
                    var rootFolderId = folderId;
                    if (path.length) {
                        rootFolderId = path[0];
                    }
                    var rootFolderData = treeViewContainer.getFolderData(rootFolderId);

                    var href = rootFolderData.folderUrl;
                    href = href.substring(0, href.lastIndexOf("#"));
                    href = href + "#" + folderId;
                    location.href = href;
                    return;
                }
            }
        }

        ASC.Files.Anchor.navigationSet(folderId);
        return;
    };

    var movetoFolder = function (folderId) {
        var folderData = treeViewSelector.getFolderData(folderId);

        if (ASC.Files.UI.accessEdit(folderData)) {

            var folderTitle = treeViewSelector.getFolderTitle(folderId);
            var path = treeViewSelector.getPath(folderId);
            path.push(folderId);

            ASC.Files.Folders.curItemFolderMoveTo(folderId, folderTitle, path);

            return true;
        } else {
            var errorString = ASC.Files.FilesJSResources.ErrorMassage_SecurityException;
            if (folderId == ASC.Files.Constants.FOLDER_ID_PROJECT
                || folderId == ASC.Files.Constants.FOLDER_ID_SHARE) {
                errorString = ASC.Files.FilesJSResources.ErrorMassage_SecurityException_PrivateRoot;
            }
            ASC.Files.UI.displayInfoPanel(errorString, true);

            treeViewSelector.expandFolder(folderId);
            return false;
        }
    };

    return {
        init: init,
        resetFolder: resetFolder,

        getFolderTitle: getFolderTitle,
        getParentId: getParentId,

        pathParts: pathParts,

        folderIdCurrentRoot: folderIdCurrentRoot,

        updateTreePath: updateTreePath,

        showSelect: showSelect,
        getPathTitle: getPathTitle,
        getFolderData: getFolderData,
    };
})();

(function ($) {
    $(function () {
        ASC.Files.Tree.init();
    });
})(jQuery);