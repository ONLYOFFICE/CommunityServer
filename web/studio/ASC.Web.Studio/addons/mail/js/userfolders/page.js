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


window.userFoldersPage = (function($) {
    var isInit = false,
        page,
        container,
        btnCreate,
        btnEdit,
        btnDelete,
        idPrefix = "uf_page_",
        idRoot = "v_root",
        isLoading = false,
        stateId = "jstreeUserFoldersPage";

    function init() {
        if (isInit === true) return;
        isInit = true;

        page = $('#user_folders_page');

        btnCreate = page.find('#createUserFolder');

        btnCreate.click(function() {
            if (!$(this).hasClass('disable')) {
                createFolder();
            }
            return false;
        });

        btnEdit = page.find('#editUserFolder');

        btnEdit.click(function() {
            if (!$(this).hasClass('disable')) {
                editFolder();
            }
            return false;
        });

        btnDelete = page.find('#deleteUserFolder');

        btnDelete.click(function() {
            if (!$(this).hasClass('disable')) {
                deleteFolder();
            }
            return false;
        });

        container = $('#userFolderTree .userFolders')
            .on("loading.jstree",
                function(e) {
                    $(e.currentTarget).find(".jstree-anchor").text(ASC.Mail.Resources.MailResource.LoadingLabel);
                })
            .on("changed.jstree",
                function(e, data) {
                    setupButtons(data.changed);
                })
            .jstree({
                "plugins": ["wholerow", "state", "changed", "dnd"],
                'core': {
                    "animation": 0,
                    "multiple": false,
                    "force_text": true,
                    'data': loadTree,
                    'check_callback': function (op, node, nodeParent, nodePosition, more) {
                        if (more && more.dnd && op === 'move_node' && nodeParent.id === "#") {
                            return false;
                        }
                        return true;
                    },
                    "keyboard": {
                        "f2": false
                    }
                },
                "state": { "key": stateId }
            });

        $(document)
            .on("dnd_scroll.vakata",
                function(e, data) {
                    console.log("dnd_scroll.vakata", data);
                })
            .on("dnd_start.vakata",
                function(e, data) {
                    console.log("dnd_start.vakata", data);
                })
            .on("dnd_stop.vakata",
                function(e, data) {
                    console.log("dnd_stop.vakata", data);

                    var ref = container.jstree(true);

                    var node = ref.get_node(data.data.nodes);

                    var id = node.id.replace(idPrefix, "");

                    var toId = node.parent === idRoot ? 0 : node.parent.replace(idPrefix, "");

                    console.log("userFoldersManager.moveTo", id, toId);

                    userFoldersManager.moveTo(id, toId);
                });

        userFoldersManager.bind(userFoldersManager.events.OnCreate, onCreated);
        userFoldersManager.bind(userFoldersManager.events.OnUpdate, onEdited);
        userFoldersManager.bind(userFoldersManager.events.OnDelete, onDeleted);
        userFoldersManager.bind(userFoldersManager.events.OnError, refresh);
    }

    var rootNode = {
        id: idRoot,
        text: "..",
        icon: "",
        state: {
            opened: true,
            disabled: false,
            selected: true
        },
        children: false
    };

    function loadTree(node, cb) {
        isLoading = true;

        userFoldersManager.loadTree(node.id === "#" ? 0 : node.id.replace(idPrefix, ""))
            .then(function(params, data) {
                    var result = data.map(function(v) {
                        return {
                            id: idPrefix + v.id,
                            parent: v.parent === 0 ? "#" : idPrefix + v.parent,
                            text: v.name,
                            icon: "",
                            state: {
                                opened: false,
                                disabled: false,
                                selected: false
                            },
                            li_attr: { unread: v.unread_count },
                            a_attr: { folderid: v.id, href: "#userfolder={0}".format(v.id) },
                            children: v.folder_count > 0 ? true : false
                        };
                    });

                    if (node.id === "#") {
                        rootNode.children = result;
                        cb(rootNode);
                        isLoading = false;
                        return;
                    }

                    cb(result);
                    isLoading = false;
                },
                function(params, errors) {
                    console.error("Error", errors, arguments);
                    window.toastr.error(errors[0]);
                    refresh();
                    isLoading = false;
                });
    }

    function createFolder() {
        var ref = container.jstree(true),
            sel = ref.get_selected();

        var parentFolder = {
            id: 0,
            name: ""
        };

        if (sel.length && sel[0] !== idRoot) {
            sel = sel[0];
            var node = ref.get_node(sel);

            parentFolder.id = parseInt(node.id.replace(idPrefix, ""));
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

    function editFolder() {
        var ref = container.jstree(true),
            sel = ref.get_selected();
        if (!sel.length || sel[0] === "#") {
            return false;
        }
        var nodeId = sel[0];
        if (nodeId === idRoot)
            return false;

        var node = ref.get_node(nodeId);

        var folder = {
            id: parseInt(node.id.replace(idPrefix, "")),
            name: node.text,
            parent: node.parent !== idRoot 
                ? parseInt(node.parent.replace(idPrefix, ""))
                : 0
        };

        var parentFolder = {
            id: folder.parent,
            name: ""
        };

        if (node.parent !== idRoot) {
            node = ref.get_node(node.parent);
            parentFolder.name = node.text;
        }

        var options = {
            disableSelector: userFoldersManager.getList().length === 0
                ? true
                : false
        };

        userFoldersManager.update(folder, parentFolder, options);

        return true;
    }

    function deleteFolder() {
        var ref = container.jstree(true),
            sel = ref.get_selected();
        if (!sel.length || sel[0] === "#") {
            return false;
        }

        var nodeId = sel[0];
        if (nodeId === idRoot)
            return false;

        var node = ref.get_node(nodeId);

        var folder = {
            id: parseInt(node.id.replace(idPrefix, "")),
            name: node.text,
            parent: node.parent !== idRoot
                ? parseInt(node.parent.replace(idPrefix, ""))
                : 0
        }

        userFoldersManager.remove(folder);

        return true;
    }

    function onCreated(e, folder) {
        console.log("userFoldersPage -> onCreated", folder);

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
            children: false
        });

        if (!res) {
            refresh();
        } else {
            ref.open_node(parentId);
        }

        show();
    }

    function onEdited(e, newFolder, oldFolder) {
        console.log("userFoldersPage -> onEdited", newFolder, oldFolder);

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
                else {
                    ref.open_node(newParentId);
                }

            } else {
                refresh();
            }
        }
    }

    function onDeleted(e, folder) {
        console.log("userFoldersPage -> onDeleted", folder);

        var ref = container.jstree(true);

        var id = idPrefix + folder.id;

        var node = ref.get_node(id);

        var children = ref.get_node(node.parent).children;

        var index = children.indexOf(id);
        var nextItem = -1;
        if (index >= 0 && index < children.length - 1)
            nextItem = children[index + 1];

        var res = ref.delete_node(id);

        if (!res) {
            refresh();
        }

        if (nextItem >= 0) {
            ref.select_node(nextItem);
        } else {
            if (node.parent !== "#") {
                ref.select_node(node.parent);
            }
        }

        show();
    }

    function setupButtons(changes) {
        var selectedId = changes.selected;
        var deselectedId = changes.deselected;

        if (!selectedId || !deselectedId)
            return false;

        var ref = container.jstree(true);
        var selectedNode = ref.get_node(selectedId);
        var deselectedNode = ref.get_node(deselectedId);

        if (selectedNode.parent === "#" || deselectedNode.parent === "#")
            disableButtons(selectedNode.parent === "#");

        return true;
    }

    function disableButtons(disable, all) {
        if (all) {
            btnCreate.toggleClass("disable", disable);
        }

        btnEdit.toggleClass("disable", disable);
        btnDelete.toggleClass("disable", disable);
    }

    function refresh() {
        console.log("userFoldersPage -> refresh");

        container.jstree("refresh");
    }

    function show() {
        if (!TMMail.pageIs('foldersettings')) return;

        if (isLoading || userFoldersManager.isLoading()) {
            window.LoadingBanner.displayMailLoading();
            setTimeout(show, 100);
            return;
        }

        window.LoadingBanner.hideLoading();

        if (!userFoldersManager.getList().length) {
            hide();
            blankPages.showEmptyUserFolders();
        } else {
            blankPages.hide();
            page.show();
        }
    }

    function hide() {
        if (!page) return;

        page.hide();
    }

    return {
        init: init,

        show: show,
        hide: hide,

        refresh: refresh,

        createFolder: createFolder
    };
})
(jQuery);