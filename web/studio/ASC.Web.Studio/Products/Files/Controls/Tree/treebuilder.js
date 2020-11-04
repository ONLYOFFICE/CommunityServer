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


window.ASC.Files.Tree = (function () {
    var isInit = false;
    var treeViewContainer;
    var treeViewSelector;

    var pathParts = new Array();
    var folderIdCurrentRoot = null;

    var displayFavoritesStatus = false;
    var displayRecentStatus = false;
    var displayTemplatesStatus = false;
    var displayPrivacy = false;

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

            var selectorPrivacyData = treeViewSelector.getFolderData(ASC.Files.Constants.FOLDER_ID_PRIVACY);
            if (selectorPrivacyData) {
                displayPrivacy = true;
                selectorPrivacyData.entryObject.addClass("privacy-node");
            }

            var favoritesData = treeViewContainer.getFolderData(ASC.Files.Constants.FOLDER_ID_FAVORITES);
            if (favoritesData) {
                displayFavoritesStatus = !favoritesData.entryObject.hasClass("display-none");
            }

            var recentData = treeViewContainer.getFolderData(ASC.Files.Constants.FOLDER_ID_RECENT);
            if (recentData) {
                displayRecentStatus = !recentData.entryObject.hasClass("display-none");
            }

            var templatesData = treeViewContainer.getFolderData(ASC.Files.Constants.FOLDER_ID_TEMPLATES);
            if (templatesData) {
                displayTemplatesStatus = !templatesData.entryObject.hasClass("display-none");
            }
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
            if (!treeViewContainer.getFolderData(ASC.Files.Tree.pathParts[i + 1])) {
                treeViewContainer.resetFolder(parentId);
            }
            treeViewContainer.expandFolder(parentId, true, true);
        }

        treeViewContainer.setCurrent(ASC.Files.Folders.currentFolder.id);
        if (ASC.Files.Tree.pathParts.length != 1
            || treeViewSelector.getFolderData(ASC.Files.Folders.currentFolder.id)) {
            treeViewSelector.setCurrent(ASC.Files.Folders.currentFolder.id);
        }
    };

    var showSelect = function () {
        treeViewSelector.rollUp();
        treeViewSelector.setCurrent(ASC.Files.Folders.currentFolder.id);

        if (displayPrivacy) {
            var needPrivacy = ASC.Files.Folders.folderContainer == "privacy";
            treeViewSelector.entryObject().toggleClass("only-privacy", needPrivacy);
        }
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
            return false;
        }

        var beOpened = treeViewContainer.resetFolder(folderId);
        treeViewSelector.resetFolder(folderId);

        if (ASC.Files.ThirdParty) {
            ASC.Files.ThirdParty.docuSignFolderSelectorReset(folderId);
        }

        return beOpened;
    };

    var reloadFolder = function (folderId) {
        var beOpened = ASC.Files.Tree.resetFolder(folderId);

        if (beOpened) {
            treeViewContainer.expandFolder(folderId);
        }
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
                    href = href + "#" + ASC.Files.Common.getCorrectHash(folderId);
                    location.href = href;
                    return;
                }
            }
        }

        ASC.Files.Filter.clearFilter(true);
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

    var displayFavorites = function (set) {
        if (set === undefined) {
            return displayFavoritesStatus;
        }

        displayFavoritesStatus = (set === true);

        var favoritesContainer = treeViewContainer.getFolderData(ASC.Files.Constants.FOLDER_ID_FAVORITES).entryObject;
        favoritesContainer.toggleClass("display-none", !displayFavoritesStatus);

        return displayFavoritesStatus;
    };

    var displayRecent = function (set) {
        if (set === undefined) {
            return displayRecentStatus;
        }

        displayRecentStatus = (set === true);

        var recentContainer = treeViewContainer.getFolderData(ASC.Files.Constants.FOLDER_ID_RECENT).entryObject;
        recentContainer.toggleClass("display-none", !displayRecentStatus);

        return displayRecentStatus;
    };

    var displayTemplates = function (set) {
        if (set === undefined) {
            return displayTemplatesStatus;
        }

        displayTemplatesStatus = (set === true);

        var templatesContainer = treeViewContainer.getFolderData(ASC.Files.Constants.FOLDER_ID_TEMPLATES).entryObject;
        templatesContainer.toggleClass("display-none", !displayTemplatesStatus);

        return displayTemplatesStatus;
    };

    return {
        init: init,
        resetFolder: resetFolder,
        reloadFolder: reloadFolder,

        getFolderTitle: getFolderTitle,
        getParentId: getParentId,

        pathParts: pathParts,

        folderIdCurrentRoot: folderIdCurrentRoot,

        updateTreePath: updateTreePath,

        showSelect: showSelect,
        getPathTitle: getPathTitle,
        getFolderData: getFolderData,

        displayFavorites: displayFavorites,
        displayRecent: displayRecent,
        displayTemplates: displayTemplates,
    };
})();

(function ($) {
    $(function () {

        if (jq("#treeViewPanelSelector").length == 0)
            return;

        ASC.Files.Tree.init();
    });
})(jQuery);