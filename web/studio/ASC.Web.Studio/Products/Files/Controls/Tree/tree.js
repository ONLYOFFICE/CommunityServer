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

window.ASC.Files.TreePrototype = function (root, rootId) {
    var getTreeNode = function (folderId) {
        if (folderId == treeNodeRootId) {
            return treeNodeRoot;
        }
        return treeNodeRoot.find(".tree-node[data-id=\"" + folderId + "\"]");
    };

    var getFolderId = function (treeNode) {
        return jq(treeNode).attr("data-id");
    };

    var getParentsTreeNode = function (folderId) {
        var treeNode = getTreeNode(folderId);
        return treeNode.parents(".jstree .tree-node");
    };

    var renderTreeView = function (treeNode, htmlData) {
        if ((treeNode.find("ul").html() || "").trim() != "") {
            resetNode(treeNode);
        }

        if (htmlData != "") {
            treeNode.removeClass("jstree-empty").append("<ul>" + htmlData + "</ul>");
            treeNode.find("ul a").each(function () {
                var hash = getFolderId(this);
                hash = ASC.Files.Constants.URL_BASE + "#" + ASC.Files.Common.fixHash(hash);
                jq(this).attr("href", hash);
            });
        } else {
            treeNode.addClass("jstree-empty");
        }

        treeNode.find(".jstree-load-node").removeClass("jstree-load-node");
        treeNode.addClass("jstree-open").removeClass("jstree-closed");
    };

    var resetNode = function (treeNode) {
        if (!treeNode.is(".tree-node")) {
            treeNode = treeNode.find(".tree-node");
        }
        treeNode.addClass("jstree-closed").removeClass("jstree-open jstree-empty")
            .children("ul").remove();
    };

    var expand = function (e, folderId, open, sync) {
        if (ASC.Files.Common.isCorrectId(folderId)) {
            var treeNode = getTreeNode(folderId);
        } else {
            treeNode = jq(this).parent();
            folderId = getFolderId(treeNode);
        }

        if (treeNode.is(":has(ul)")) {
            treeNode.toggleClass("jstree-closed", open != null ? !open : null).toggleClass("jstree-open", open);
        } else {
            getTreeSubFolders(folderId, sync);
        }
    };

    var clickNode = function () {
        var treeNode = jq(this).parent();
        var folderId = getFolderId(treeNode);

        if (tree.clickOnFolder(folderId) !== false) {
            select(treeNode);
        }
        return false;
    };

    var select = function (treeNode) {
        treeNodeRoot.find("a.selected").removeClass("selected");
        treeNodeRoot.find(".parent-selected").removeClass("parent-selected");

        treeNode.children("a").addClass("selected");
        treeNode.parents(".jstree .jstree-closed").addClass("jstree-open").removeClass("jstree-closed");
        treeNode.parents(".jstree .tree-node").addClass("parent-selected");

        if (treeNode && treeNode.offset()) {
            var nodeY = treeNode.offset().top;
            nodeY -= treeNodeRoot.offset().top;
            nodeY += treeNodeRoot.scrollTop();
            nodeY -= 30;

            treeNodeRoot.scrollTop(nodeY);
        }

        tree.selectedFolderId = getFolderId(treeNode);
    };

    var getTreeSubFolders = function (folderId, ajaxsync) {
        if (!ASC.Files.Common.isCorrectId(folderId)) {
            return;
        }

        getTreeNode(folderId).find(".jstree-expander").addClass("jstree-load-node");

        ASC.Files.ServiceManager.getTreeSubFolders(
            ASC.Files.ServiceManager.events.GetTreeSubFolders,
            { folderId: folderId, ajaxsync: (ajaxsync === true) });
    };

    var openPath = function (folderId) {
        if (!ASC.Files.Common.isCorrectId(folderId)) {
            return;
        }

        ASC.Files.ServiceManager.getTreePath(
            ASC.Files.ServiceManager.events.GetTreePath,
            { folderId: folderId, ajaxsync: true });
    };

    var onGetTreeSubFolders = function (xmlData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            tree.errorMessage(errorMessage);
            return;
        }

        var folderId = params.folderId;
        var htmlData = ASC.Files.TemplateManager.translate(xmlData);

        var treeNode = getTreeNode(folderId);

        if (!treeNode.length) {
            treeNode = treeNodeRoot;
        }

        renderTreeView(treeNode, htmlData);
    };

    var onGetTreePath = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            tree.errorMessage(errorMessage);
            return;
        }

        jsonData.pop();
        jq(jsonData).each(function (i, parentId) {
            expand(null, parentId, true, true);
        });
    };

    /***********************************************************/

    this.getFolderTitle = function (folderId) {
        return getTreeNode(folderId).children("a:first").text().trim();
    };

    this.getPath = function (folderId) {
        return getParentsTreeNode(folderId).map(function (i, treeNode) {
            return getFolderId(treeNode);
        }).toArray().reverse();
    };

    this.getParentId = function (folderId) {
        var parentsNode = getParentsTreeNode(folderId);
        if (!parentsNode.length) {
            return 0;
        }

        return getFolderId(parentsNode[0]);
    };

    this.setCurrent = function (folderId) {
        var treeNode = getTreeNode(folderId);
        if (!treeNode.length) {
            openPath(folderId);
            treeNode = getTreeNode(folderId);
        }

        select(treeNode);
    };

    this.resetFolder = function (folderId) {
        resetNode(getTreeNode(folderId));
    };

    this.rollUp = function () {
        treeNodeRoot.find("a.selected").removeClass("selected");
        treeNodeRoot.find(".parent-selected").removeClass("parent-selected");
        treeNodeRoot.find(".jstree-open").addClass("jstree-closed").removeClass("jstree-open");

        tree.selectedFolderId = null;
    };

    this.expandFolder = function (folderId, open, sync) {
        return expand(null, folderId, open, sync);
    };

    this.selectedFolderId = null;

    this.clickOnFolder = function (folderId) {
    };

    this.errorMessage = function (message) {
        if (ASC.Files.UI) {
            ASC.Files.UI.displayInfoPanel(message, true);
        }
    };

    this.getFolderData = function (folderId) {
        var treeNode = getTreeNode(folderId);
        if (ASC.Files.UI) {
            return ASC.Files.UI.getObjectData(treeNode);
        }

        return {
            entryId: folderId,
            entryType: "folder",
            entryObject: treeNode,
            title: getFolderTitle(folderId)
        };
    };

    this.getDefaultFolderId = function () {
        return getFolderId(treeNodeRoot.find(".tree-node:visible:first"));
    };


    var tree = this;
    var treeNodeRoot = jq(root);
    var treeNodeRootId = rootId;

    treeNodeRoot.on("click", ".jstree-expander", expand);
    treeNodeRoot.on("dblclick", "a", expand);

    treeNodeRoot.on("click", "a", clickNode);

    ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetTreeSubFolders, onGetTreeSubFolders);
    ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetTreePath, onGetTreePath);
};
