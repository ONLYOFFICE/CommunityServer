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


window.userFoldersDropdown = (function($) {
    var popup,
        popupId = "#selectUserFolderDropdown",
        container,
        showButton,
        isInit = false,
        idPrefix = "uf_dp_f_",
        idPrefixDefault = "uf_dp_def_",
        idRoot = "v_dp_root",
        rootChildren = null,
        isLoading = false,
        stateId = "jstreeUserFoldersDropdown",
        curOptions = null,
        selected = null,
        onCloseClaback = null;

    function init() {
        if (isInit === true) return;

        isInit = true;

        popup = $(popupId);

        container = $("#selectUserFolderDropdown .existsUserFolders")
            .on("loading.jstree",
                function (e) {
                    $(e.currentTarget).find(".jstree-anchor").text(ASC.Mail.Resources.MailResource.LoadingLabel);
                })
            .jstree({
                "plugins": ["wholerow", "state", "types", "themes"],
                'core': {
                    "animation": 0,
                    "check_callback": true,
                    "multiple": false,
                    "force_text": true,
                    'data': loadTree,
                    "keyboard": {
                        "f2": false
                    }
                },
                "themes": {
                    "theme": "default",
                    "dots": false,
                    "icons": true
                },
                "types": {
                    "inbox": {
                        "valid_children": ["none"],
                        "icon": "menu-item-icon inbox",
                        "max_depth": 1
                    },
                    "sent": {
                        "valid_children": ["none"],
                        "icon": "menu-item-icon sent",
                        "max_depth": 1
                    },
                    "draft": {
                        "valid_children": ["none"],
                        "icon": "menu-item-icon drafts",
                        "max_depth": 1
                    },
                    "template": {
                        "valid_children": ["none"],
                        "icon": "menu-item-icon templates",
                        "max_depth": 1
                    },
                    "spam": {
                        "valid_children": ["none"],
                        "icon": "menu-item-icon spam",
                        "max_depth": 1
                    },
                    "trash": {
                        "valid_children": ["none"],
                        "icon": "menu-item-icon trash",
                        "max_depth": 1
                    }
                },
                "state": { "key": stateId }
            });

        userFoldersManager.bind(userFoldersManager.events.OnCreate, onCreated);
        userFoldersManager.bind(userFoldersManager.events.OnUpdate, onEdited);
        userFoldersManager.bind(userFoldersManager.events.OnDelete, onDeleted);

        popup.keyup(function (e) {
                var movetoBtn = popup.find(".moveto_button").not(".disable");

                if (!movetoBtn || !e) return true;

                var code;

                if (e.keyCode)
                    code = e.keyCode;
                else if (e.which)
                    code = e.which;

                if (code === 13) {
                    movetoBtn.trigger("click");
                    return false;
                }

                return true;
            });
    }

    var rootNode = {
        id: idRoot,
        text: window.ASC.Mail.Resources.MailScriptResource.UserFolderNoSelected,
        icon: "",
        state: {
            opened: true,
            disabled: false,
            selected: true
        },
        children: false,
        a_attr: { "class": "noSelected" }
    };

    function loadTree(node, cb) {
        isLoading = true;

        userFoldersManager.loadTree(node.id === "#" ? 0 : node.id.replace(idPrefix, ""))
            .then(function(params, data) {
                    var result = data.map(function(v) {
                        var newNode = {
                            id: idPrefix + v.id,
                            parent: v.parent === 0 ? "#" : idPrefix + v.parent,
                            text: v.name,
                            icon: "",
                            state: {
                                opened: false,
                                disabled: false,
                                selected: false
                            },
                            a_attr: { href: "#userfolder={0}".format(v.id) },
                            children: v.folder_count > 0 ? true : false
                        };

                        if (curOptions) {
                            if (curOptions.disableDefaultId === newNode.id) {
                                newNode.state.disabled = true;
                            } else if (curOptions.disableUFolderId === newNode.id) {
                                newNode.state.disabled = true;
                            } else if (curOptions.hideDefaultId === newNode.id) {
                                newNode.state.hidden = true;
                            } else if (curOptions.hideUFolderId === newNode.id) {
                                newNode.state.hidden = true;
                            }else if (curOptions.hideChildrenId === newNode.parent) {
                                newNode.state.hidden = true;
                            }
                        }

                        return newNode;
                    });

                    if (node.id === "#") {
                        var defaultFolders = getDefaultFolders();
                        var folders = defaultFolders.concat(result);
                        rootNode.children = folders;
                        cb(rootNode);
                        isLoading = false;
                        return;
                    }

                    cb(result);

                    isLoading = false;
                },
                function(params, errors) {
                    console.error("Error", errors, arguments);
                    isLoading = false;
                });
    }

    function getDefaultFolders() {
        var folders = [
            {
                id: idPrefixDefault + TMMail.sysfolders.inbox.id,
                parent:"#",
                text: TMMail.sysfolders.inbox.displayName,
                type: "inbox",
                state: {
                    opened: false,
                    disabled: false,
                    selected: false
                },
                a_attr: { href: "#inbox" },
                children: false
            },
            {
                id: idPrefixDefault + TMMail.sysfolders.sent.id,
                parent: "#",
                text: TMMail.sysfolders.sent.displayName,
                type: "sent",
                state: {
                    opened: false,
                    disabled: false,
                    selected: false
                },
                a_attr: { href: "#sent" },
                children: false
            },
            {
                id: idPrefixDefault + TMMail.sysfolders.drafts.id,
                parent: "#",
                text: TMMail.sysfolders.drafts.displayName,
                type: "draft",
                state: {
                    opened: false,
                    disabled: false,
                    selected: false
                },
                a_attr: { href: "#drafts" },
                children: false
            },
            {
                id: idPrefixDefault + TMMail.sysfolders.templates.id,
                parent: "#",
                text: TMMail.sysfolders.templates.displayName,
                type: "template",
                state: {
                    opened: false,
                    disabled: false,
                    selected: false
                },
                children: false
            },
            {
                id: idPrefixDefault + TMMail.sysfolders.trash.id,
                parent: "#",
                text: TMMail.sysfolders.trash.displayName,
                type: "trash",
                state: {
                    opened: false,
                    disabled: false,
                    selected: false
                },
                a_attr: { href: "#trash" },
                children: false
            },
            {
                id: idPrefixDefault + TMMail.sysfolders.spam.id,
                parent: "#",
                text: TMMail.sysfolders.spam.displayName,
                type: "spam",
                state: {
                    opened: false,
                    disabled: false,
                    selected: false
                },
                a_attr: { href: "#spam" },
                children: false
            }
        ];

        return folders;
    }

    function filteredHide(event) {
        var elt = (event.target) ? event.target : event.srcElement;
        if (!($(elt).is(popupId + ' *') || $(elt).is(popupId))) {
            hide();
        }
    }

    function onScroll() {
        var offset = $(showButton).offset();
        popup.css({ left: offset.left, top: offset.top + 17 }).show();
    }

    function showRoot(needShow) {
        var treeData = container.jstree(true).get_json('#', { flat: false });
        var jsTree = container.jstree(true);

        var newData;

        if (!rootChildren) {
            rootChildren = treeData[0].children;
        }

        if (needShow) {
            rootNode.children = rootChildren;
            newData = rootNode;
        } else {
            newData = rootChildren;
        }

        jsTree.settings.core.data = newData;
        jsTree.refresh();
        jsTree.settings.core.data = loadTree;
    }

    function showDefaults(isShow) {
        var jstree = container.jstree(true);

        var folders = getDefaultFolders();
        var i, n = folders.length;
        for (i = 0; i < n; i++) {
            var id = folders[i].id;

            if (isShow) {
                jstree.show_node(id);
            } else {
                jstree.hide_node(id);
            }
        }
    }

    function hideId(id) {
        var jstree = container.jstree(true);
        jstree.hide_node(id);
    }

    function hideChildren(id) {
        var jstree = container.jstree(true);
        var children = jstree.get_node(id).children;
        jstree.hide_node(children);
    }

    function disableId(id) {
        var jstree = container.jstree(true);
        jstree.disable_node(id);
    }

    function prepareOptions(opts) {
        if (!opts || typeof opts !== "object")
            opts = {};

        var options = {};

        options.hideDefaults = typeof opts.hideDefaults === "undefined" ||
            opts.hideDefaults === null
            ? false
            : opts.hideDefaults;

        options.hideRoot = typeof opts.hideRoot === "undefined" || opts.hideRoot === null
            ? true
            : opts.hideRoot;

        options.hideDefaultId = typeof opts.hideDefaultId === "undefined" || opts.hideDefaultId === null
            ? null
            : idPrefixDefault + opts.hideDefaultId;

        options.hideUFolderId = typeof opts.hideUFolderId === "undefined" || opts.hideUFolderId === null
            ? null
            : idPrefix + opts.hideUFolderId;

        options.hideChildrenId = typeof opts.hideChildrenId === "undefined" || opts.hideChildrenId === null
            ? null
            : idPrefix + opts.hideChildrenId;

        options.disableDefaultId = typeof opts.disableDefaultId === "undefined" || opts.disableDefaultId === null
            ? null
            : idPrefixDefault + opts.disableDefaultId;

        options.disableUFolderId = typeof opts.disableUFolderId === "undefined" || opts.disableUFolderId === null
            ? null
            : idPrefix + opts.disableUFolderId;

        options.callback = typeof opts.callback === "undefined" || typeof opts.callback !== "function"
            ? function(e, folder) {
                console.log("Default callback has been called", folder);
            }
            : opts.callback;

        options.btnCaption = typeof opts.btnCaption === "undefined" || opts.btnCaption === null
            ? window.MailResource.MoveHere
            : opts.btnCaption;

        options.onClose = typeof opts.onClose === "undefined" || typeof opts.onClose !== "function"
            ? function() {
                console.log("Default onClose has been called");
            }
            : opts.onClose;

        return options;
    }

    function show(obj, opts) {
        if (isLoading || userFoldersManager.isLoading()) {
            setTimeout(function () { show(obj, opts); }, 100);
            return;
        }

        if ($(popupId + ':visible').length) {
            hide();
            return;
        }

        var options = prepareOptions(opts);

        curOptions = options;

        showRoot(!options.hideRoot);

        showDefaults(!options.hideDefaults);

        if (options.hideDefaultId) {
            hideId(options.hideDefaultId);
        }

        if (options.hideUFolderId) {
            hideId(options.hideUFolderId);
        }

        if (options.hideChildrenId) {
            hideChildren(options.hideChildrenId);
        }

        if (options.disableDefaultId) {
            disableId(options.disableDefaultId);
        }

        if (options.disableUFolderId) {
            disableId(options.disableUFolderId);
        }

        showButton = obj;
        var offset = $(showButton).offset();
        popup.css({ left: offset.left, top: offset.top + 17 }).show();

        dropdown.regHide(filteredHide);
        dropdown.regScroll(onScroll);

        selected = null;

        var movetoBtn = popup.find(".moveto_button");

        movetoBtn.text(options.btnCaption);
        movetoBtn.attr("title", options.btnCaption);

        movetoBtn.toggleClass('disable', true);

        movetoBtn.unbind("click").bind("click",
            function (e) {
                if ($(this).hasClass('disable') || !selected)
                    return;

                hide();
                options.callback.call(e, selected);
            });

        container.jstree(true).deselect_all();

        container
            .unbind("select_node.jstree")
            .bind("select_node.jstree", function (e, data) {
                movetoBtn.toggleClass('disable', false);

                if (!data.node.state.opened) {
                    data.instance.open_node(data.node);
                }

                var isUserFolder = data.node.id.indexOf(idPrefix) > -1;

                var id = data.node.id === idRoot
                    ? 0
                    : (isUserFolder
                        ? data.node.id.replace(idPrefix, "")
                        : data.node.id.replace(idPrefixDefault, ""));

                var folderType = isUserFolder ? +TMMail.sysfolders.userfolder.id : +id;
                var userFolderId = isUserFolder ? +id : null;

                selected = {
                    id: id,
                    name: data.node.text,
                    folderType: folderType,
                    userFolderId: userFolderId
                }
            });

        if (options.onClose) {
            onCloseClaback = options.onClose;
        }
    }

    function hide() {
        if (!popup) return;

        popup.hide();
        dropdown.unregHide(filteredHide);
        dropdown.unregHide(onScroll);

        var jstree = container.jstree(true);

        showRoot(true);
        showDefaults(true);

        jstree.deselect_all();
        jstree.show_all();
        rootChildren = null;

        if (onCloseClaback) {
            onCloseClaback();
            onCloseClaback = null;
        }
    }

    function refresh() {
        console.log("userFoldersDropdown -> refresh");

        container.jstree(true).refresh();
    }

    function onCreated(e, folder) {
        console.log("userFoldersDropdown -> onCreated", folder);

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
            a_attr: { href: "#userfolder={0}".format(folder.id) },
            children: false
        });

        if (!res) {
            refresh();
        }
    }

    function onEdited(e, newFolder, oldFolder) {
        console.log("userFoldersDropdown -> onEdited", newFolder, oldFolder);

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
        console.log("userFoldersDropdown -> onDeleted", folder);

        var ref = container.jstree(true);

        var id = idPrefix + folder.id;

        var res = ref.delete_node(id);

        if (!res) {
            refresh();
        }
    }

    return {
        init: init,
        show: show,
        hide: hide,
        refresh: refresh
    };
})(jQuery);