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


window.ASC.Files.TreePrototype = function (rootSelector, rootId) {
    var getTreeNode = function (folderId) {
        if (folderId == treeNodeRootId) {
            return treeNodeRoot;
        }
        if (ASC.Files.UI) {
            return treeNodeRoot.find(".tree-node" + ASC.Files.UI.getSelectorId(folderId));
        }
        return treeNodeRoot.find(".tree-node[data-id=\"" + (folderId + "").replace(/\\/g, "\\\\").replace(/\"/g, "\\\"") + "\"]");
    };

    var getFolderId = function (treeNode) {
        return jq(treeNode).attr("data-id");
    };

    var getParentsTreeNode = function (folderId) {
        var treeNode = getTreeNode(folderId);
        return treeNode.parents(".jstree .tree-node");
    };

    var renderTreeView = function (treeNode, htmlData, expandNode) {
        if ((treeNode.find("ul").html() || "").trim() != "") {
            resetNode(treeNode);
        }

        if (htmlData != "") {
            treeNode.removeClass("jstree-empty").append("<ul>" + htmlData + "</ul>");
            treeNode.find("ul a").each(function () {
                var entryId = getFolderId(this);
                var hash = ASC.Files.UI.getEntryLink("folder", entryId);
                jq(this).attr("href", hash);
            });
        } else {
            treeNode.addClass("jstree-empty");
        }

        treeNode.find(".jstree-load-node").removeClass("jstree-load-node");
        if (expandNode !== false) {
            treeNode.addClass("jstree-open").removeClass("jstree-closed");
        }
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

        if (!treeNode.length
            || folderId == ASC.Files.Constants.FOLDER_ID_FAVORITES
            || folderId == ASC.Files.Constants.FOLDER_ID_RECENT
            || folderId == ASC.Files.Constants.FOLDER_ID_PRIVACY && !ASC.Desktop
            || folderId == ASC.Files.Constants.FOLDER_ID_TEMPLATES
            || folderId == ASC.Files.Constants.FOLDER_ID_TRASH) {
            return;
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

        var checkSelected = tree.clickOnFolder(folderId) !== false;
        select(treeNode, checkSelected);
        return false;
    };

    var select = function (treeNode, checkSelected) {
        treeNodeRoot.find(".node-selected").removeClass("node-selected");
        treeNodeRoot.find(".parent-selected").removeClass("parent-selected");

        treeNode.addClass("node-selected");
        treeNode.parents(".jstree .jstree-closed").addClass("jstree-open").removeClass("jstree-closed");
        treeNode.parents(".jstree .tree-node").addClass("parent-selected");

        //if (treeNode && treeNode.offset()) {
        //    var nodeY = treeNode.offset().top;
        //    nodeY -= treeNodeRoot.offset().top;
        //    nodeY += treeNodeRoot.scrollTop();
        //    nodeY -= 30;

        //    if (treeNodeRoot.is("#treeViewContainer")) {
        //        treeNodeRoot.parents(".nav-content").scrollTop(nodeY);
        //    } else {
        //        treeNodeRoot.scrollTop(nodeY);
        //    }
        //}

        tree.selectedFolderId = checkSelected ? getFolderId(treeNode) : null;
    };

    var getTreeSubFolders = function (folderId, ajaxsync) {
        if (!ASC.Files.Common.isCorrectId(folderId)) {
            return;
        }

        getTreeNode(folderId).find(".jstree-expander").addClass("jstree-load-node");

        ASC.Files.ServiceManager.getTreeSubFolders(
            ASC.Files.ServiceManager.events.GetTreeSubFolders,
            {
                treeNodeRoot: treeNodeRoot,
                folderId: folderId,
                ajaxsync: (ajaxsync === true)
            });
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
            if (treeNodeRootId != null) {
                return;
            }
            treeNode = treeNodeRoot;
        }

        renderTreeView(treeNode, htmlData, treeNodeRoot == params.treeNodeRoot);
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

        select(treeNode, true);
    };

    this.resetFolder = function (folderId) {
        var treeNode = getTreeNode(folderId);
        var beOpened = treeNode.hasClass("jstree-open");

        resetNode(treeNode);
        return beOpened;
    };

    this.rollUp = function () {
        treeNodeRoot.removeClass("node-selected");
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

    this.displayAsRoot = function (folderId) {
        var treeNode = getTreeNode(folderId);

        jq(".jstree-root-as").removeClass("jstree-root-as");
        treeNode.parents(".tree-node").addClass("jstree-root-parent").addBack().addClass("jstree-root-as");

        jq(".jstree-root-out").removeClass("jstree-root-out");
        treeNode.parent().children(".tree-node:not([data-id=\"" + (folderId + "").replace(/\\/g, "\\\\").replace(/\"/g, "\\\"") + "\"])").addClass("jstree-root-out");
    };

    this.entryObject = function () {
        return treeNodeRoot;
    };


    var tree = this;
    var treeNodeRoot = jq(rootSelector);
    var treeNodeRootId = rootId;

    treeNodeRoot.on("click", ".jstree-expander", expand);
    treeNodeRoot.on("dblclick", "a", expand);

    treeNodeRoot.on("click", "a", clickNode);

    ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetTreeSubFolders, onGetTreeSubFolders);
    ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetTreePath, onGetTreePath);
};
