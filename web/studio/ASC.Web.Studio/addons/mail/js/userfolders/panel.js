/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


window.userFoldersPanel = (function($) {
    var container,
        userFolderContainer,
        selectedId,
        isInit = false,
        idPrefix = "uf_panel_",
        idRoot = "#",
        createFolderLink,
        stateId = "jstreeUserFoldersPanel",
        stateReady = false;

    var init = function() {
        if (isInit === true) return;

        isInit = true;

        userFolderContainer = $("#userFolderContainer");

        container = userFolderContainer.find(".userFolders");

        container
            .on("loading.jstree",
                function(e) {
                    $(e.currentTarget).find(".jstree-anchor").text(ASC.Mail.Resources.MailResource.LoadingLabel);
                })
            .on("loaded.jstree", 
                function () {
                    $("#userFoldersManage").show();
                    resize();
                })
            .on("state_ready.jstree",
                function () {
                    stateReady = true;

                    if (!TMMail.pageIs('userfolder') && getSelected()) {
                        unmarkAll();
                    }

                    container.on("select_node.jstree", onSelectNode);
                })
            .on("after_open.jstree after_close.jstree refresh.jstree refresh_node.jstree create_node.jstree delete_node.jstree", resize)
            .jstree({
                "plugins": ["wholerow", "state", "counters"],
                'core': {
                    "animation": 0,
                    "check_callback": true,
                    "multiple": false,
                    "force_text": true,
                    'data': loadTree,
                    "keyboard": {
                        "f2":  false
                    }
                },
                "state": { "key": stateId }
            });

        createFolderLink = $("#userFoldersManage .link")
            .unbind("click")
            .bind("click", createFolder);

        userFoldersManager.bind(userFoldersManager.events.OnCreate, onCreated);
        userFoldersManager.bind(userFoldersManager.events.OnUpdate, onEdited);
        userFoldersManager.bind(userFoldersManager.events.OnDelete, onDeleted);
        userFoldersManager.bind(userFoldersManager.events.OnUnreadChanged, onUnreadChanged);

        setupDroppable();

        selectedId = TMMail.extractUserFolderIdFromAnchor();
    };

    function resize() {
        TMMail.resizeUserFolders();
        markSelectedPath();
    }

    function onSelectNode(e, data) {
        var href = "#userfolder={0}".format(data.node.id.replace(idPrefix, ""));
        ASC.Controls.AnchorController.move(href);
        markSelectedPath();
    }

    function markSelectedPath() {
        var selectedNode = userFolderContainer.find('[aria-selected="true"]'),
            selectedNodes = getSelected(),
            parentNodes = (selectedNodes === undefined) ? false : selectedNodes.parents;

        clearMarkedPath();

        selectedNode.addClass('node-selected');

        if (parentNodes) {
            for (var i = 0; i < parentNodes.length - 1; i++) {
                $('#' + parentNodes[i]).addClass('parent-selected');
            }
        }
    }

    function clearMarkedPath() {
        userFolderContainer.find('li').removeClass('node-selected').removeClass('parent-selected');
    }

    function createFolder() {
        var ref = container.jstree(true),
            sel = ref.get_selected();

        var parentFolder = {
            id: 0,
            name: ""
        };

        if (sel.length) {
            sel = sel[0];
            var node = ref.get_node(sel);

            parentFolder.id = node.id.replace(idPrefix, "");
            parentFolder.name = node.text;
        }

        var newFolder = {
            name: "",
            parent: parentFolder.id
        };

        var options = {
            disableSelector: userFoldersManager.getList().length === 0
                ? true
                : false
        };

        userFoldersManager.create(newFolder, parentFolder, options);

        return true;
    }

    function getFolderByPosition(x, y) {
        var w = $(window);
        var baseElm = $(document.elementFromPoint(x - w.scrollLeft(), y - w.scrollTop()));

        if (!baseElm) return undefined;

        var isUserFolder = baseElm.closest(".userFolders").length > 0;
        var folderId = null;

        if (baseElm.attr("folderid")) {
            folderId = baseElm.attr("folderid");
        } else {
            var aElm = baseElm.closest("a");

            if (aElm && aElm.attr("folderid")) {
                folderId = aElm.attr("folderid");
            } else {
                var liElm = baseElm.closest("li");

                if (liElm && liElm.attr("folderid"))
                    folderId = liElm.attr("folderid");
            }
        }

        if (!folderId)
            return undefined;

        folderId = +folderId;

        return isUserFolder
            ? {
                folderType: TMMail.sysfolders.userfolder.id,
                userFolderId: folderId
            }
            : {
                folderType: folderId,
                userFolderId: null
            };
    }

    function setupDroppable() {
        userFolderContainer.droppable({
            drop: function (event, ui) {
                var folder = getFolderByPosition(ui.offset.left, ui.offset.top);

                if (!folder) return;

                var curFolderType = +MailFilter.getFolder();
                var curUserFolderId = +MailFilter.getUserFolder() || null;

                if (curFolderType === folder.folderType && curUserFolderId === folder.userFolderId) {
                    return;
                }

                mailBox.moveTo(folder.folderType, folder.userFolderId);
            },
            tolerance: "pointer"
        });
    }

    function loadTree(node, cb) {
        userFoldersManager.loadTree(node.id === idRoot ? 0 : node.id.replace(idPrefix, ""))
            .then(function(params, data) {
                var result = data.map(function (v) {
                    return {
                            id: idPrefix + v.id,
                            parent: v.parent === 0 ? idRoot : idPrefix + v.parent,
                            text: v.name,
                            icon: "",
                            state: {
                                opened: false,
                                disabled: false,
                                selected: selectedId && selectedId == v.id ? true : false
                            },
                            li_attr: { folderid: v.id, unread_messages: v.unread_count, unread_chains: v.unread_chain_count },
                            a_attr: { folderid: v.id, href: "#userfolder={0}".format(v.id) },
                            children: v.folder_count > 0 ? true : false
                        };
                    });

                    cb(result);
                },
                function(params, errors) {
                    console.error("loadTree Error", errors, arguments);
                });
    }

    function markFolder(id) {
        var ref = container.jstree(true);
        var nodeId = idPrefix + id;

        if (stateReady)
            container.unbind("select_node.jstree");

        unmarkAll();

        ref.select_node(nodeId);

        MailFilter.setFolder(TMMail.sysfolders.userfolder.id);
        MailFilter.setUserFolder(id);

        selectedId = id;

        if (stateReady)
            container.bind("select_node.jstree", onSelectNode);
    }

    function unmarkAll() {
        container.jstree("deselect_all");
        clearMarkedPath();
    }

    function refresh() {
        console.log("userFoldersPanel -> refresh");
        container.jstree("refresh");
        resize();
    }

    function getSelected() {
        return container.jstree('get_selected', true)[0];
    }

    function onCreated(e, folder) {
        console.log("userFoldersPanel -> onCreated", folder);

        var ref = container.jstree(true);

        var parentId = folder.parent === 0 ? idRoot : idPrefix + folder.parent;

        var res = ref.create_node(parentId, {
            id: idPrefix + folder.id,
            parent: parentId,
            text: folder.name,
            icon: "",
            state: {
                opened: false,
                disabled: false,
                selected: false
            },
            li_attr: { folderid: folder.id, unread_messages: folder.unread_count, unread_chains: folder.unread_chain_count },
            a_attr: { folderid: folder.id, href: "#userfolder={0}".format(folder.id) },
            children: false
        });

        if (!res) {
            refresh();
        }
    }

    function onEdited(e, newFolder, oldFolder) {
        console.log("userFoldersPanel -> onEdited", newFolder, oldFolder);

        var ref = container.jstree(true);

        var id = idPrefix + newFolder.id;

        ref.set_text(id, newFolder.name);

        var oldParentId = oldFolder.parent === 0 ? idRoot : idPrefix + oldFolder.parent;
        var newParentId = newFolder.parent === 0 ? idRoot : idPrefix + newFolder.parent;

        if (oldParentId !== newParentId) {
            if (newParentId !== idRoot && ref.is_loaded(newParentId)) {
                var res = ref.move_node(id,
                    newParentId,
                    "last");

                if (!res) {
                    refresh();
                }
            } else {
                refresh();
            }
        }
    }

    function onDeleted(e, folder) {
        console.log("userFoldersPanel -> onDeleted", folder);

        var ref = container.jstree(true);

        var id = idPrefix + folder.id;

        var res = ref.delete_node(id);

        if (!res) {
            refresh();
        }
    }

    function onUnreadChanged(e, folder) {
        console.log("userFoldersPanel -> OnUnreadChanged", folder);

        var ref = container.jstree(true);

        var id = idPrefix + folder.id;

        var node = ref.get_node(id);

        if (!node) return;

        node.li_attr.unread_messages = folder.unread_count;
        node.li_attr.unread_chains = folder.unread_chain_count;

        ref.redraw_node(node.id);

        resize();

        TMMail.setPageHeaderFolderName(TMMail.sysfolders.userfolder.id);
    }

    function decrementUnreadCount() {
        var node = getSelected();
        if (!node) return;

        node.li_attr.unread_messages--;
        node.li_attr.unread_chains--;

        var ref = container.jstree(true);
        ref.redraw_node(node.id);

        if (node.li_attr.unread_messages === 0 || node.li_attr.unread_chains === 0)
            resize();
    }

    return {
        init: init,
        markFolder: markFolder,
        unmarkAll: unmarkAll,
        refresh: refresh,
        getSelected: getSelected,
        decrementUnreadCount: decrementUnreadCount
    };
})(jQuery);