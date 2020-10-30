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

if (!Array.prototype.findIndex) {
    Array.prototype.findIndex = function (predicate) {
        if (this == null) {
            throw new TypeError('Array.prototype.findIndex called on null or undefined');
        }
        if (typeof predicate !== 'function') {
            throw new TypeError('predicate must be a function');
        }
        var list = Object(this);
        var length = list.length >>> 0;
        var thisArg = arguments[1];
        var value;

        for (var i = 0; i < length; i++) {
            value = list[i];
            if (predicate.call(thisArg, value, i, list)) {
                return i;
            }
        }
        return -1;
    };
}

window.userFoldersManager = (function($) {
    var isInit = false;
    var userFolders = [];
    var eventsHandler = $({});
    var supportedCustomEvents = {
        OnCreate: "uf.create",
        OnUpdate: "uf.update",
        OnDelete: "uf.delete",
        OnUnreadChanged: "uf.unread_changed",
        OnError: "uf.error"
    };
    var progressBarIntervalId = null;
    var getStatusTimeout = 1000;
    var onLoading = true;

    function init() {
        if (isInit === true) return;

        if (ASC.Mail.Presets.UserFolders) {
            var folderList = $.map(ASC.Mail.Presets.UserFolders,
                function(el) {
                    el.name = TMMail.htmlDecode(el.name);
                    return el;
                });

            onGetMailUserFolders({}, folderList);
        }

        userFoldersPage.init();
        userFoldersPanel.init();
        userFoldersDropdown.init();

        window.Teamlab.bind(window.Teamlab.events.getMailUserFolders, onGetMailUserFolders);
    }

    function refreshUserFolders(folders, isFullReload) {
        var folder, i, index;

        for (i = 0; i < folders.length; i++) {
            folder = folders[i];
            index = userFolders.findIndex(function (f) {
                return f.id === folder.id;
            });
            if (index > -1) {
                var oldUserFolder = userFolders[index];
                if (oldUserFolder.unread_count !== folder.unread_count ||
                    oldUserFolder.unread_chain_count !== folder.unread_chain_count) {
                    eventsHandler.trigger(supportedCustomEvents.OnUnreadChanged, folder);
                }

                userFolders[index] = folder;
            } else {
                userFolders.push(folder);

                if (isFullReload) {
                    eventsHandler.trigger(supportedCustomEvents.OnCreate, folder);
                }
            }
        }

        if (isFullReload) {
            for (i = 0; i < userFolders.length; i++) {
                folder = userFolders[i];
                index = folders.findIndex(function (f) {
                    return f.id === folder.id;
                });
                if (index === -1) {
                    removeWithChildren(folder.id);
                    eventsHandler.trigger(supportedCustomEvents.OnDelete, folder);
                }
            }
        }
    }

    function onGetMailUserFolders(params, folders) {
        refreshUserFolders(folders, params.isFullReload);
        onLoading = false;
    }

    function loadTree(parentId) {
        var d = $.Deferred();

        parentId = typeof parentId === "undefined" ? 0 : parseInt(parentId);

        var folders = userFolders.filter(function(f) {
            return f.parent === parentId;
        });

        if (!folders.length) {
            reloadTree(parentId)
                .then(d.resolve, d.reject);
        } else {
            d.resolve({}, folders);
        }

        return d.promise();
    }

    function reloadTree(parentId, ids) {
        var d = $.Deferred();

        window.Teamlab.getMailUserFolders({
                isFullReload: parentId === 0
            },
            parentId,
            ids,
            {
                success: d.resolve,
                error: d.reject
            }
        );

        return d.promise();
    }

    function getList() {
        return userFolders || [];
    }

    function get(id) {
        id = parseInt(id);

        var index = userFolders.findIndex(function(f) {
            return f.id === id;
        });

        return index > -1 ? userFolders[index] : null;
    }

    function create(initFolder, initParentFolder, options) {
        userFoldersModal.showCreate(
            initFolder,
            initParentFolder,
            {
                disableSelector: options.disableSelector,
                onSave: function(folder) {
                    window.Teamlab.createMailFolder({},
                        folder.name,
                        folder.parent,
                        {
                            success: function (params, newFolder) {
                                userFoldersModal.hide();

                                userFolders.push(newFolder);

                                if (newFolder.parent > 0) {
                                    var folders = userFolders.filter(function(f) {
                                        return f.id === newFolder.parent;
                                    });
                                    folders.forEach(function(f) {
                                        f.folder_count++;
                                    });
                                }

                                window.toastr.success(MailScriptResource.InfoCreateFolder.format(Encoder.htmlEncode(newFolder.name)));

                                eventsHandler.trigger(supportedCustomEvents.OnCreate, newFolder);
                            },
                            error: function (params, errors) {
                                window.toastr.error(Encoder.htmlEncode(errors[0]));
                                eventsHandler.trigger(supportedCustomEvents.OnError, errors[0]);
                            }
                        });


                }
            });
    }

    function update(initFolder, initParentFolder, options) {
        userFoldersModal.showEdit(
            initFolder,
            initParentFolder,
            {
                disableSelector: options.disableSelector,
                onSave: function (newFolder) {
                    updateFolder(initFolder, newFolder);
                }
            });
    }

    function moveTo(id, toId) {
        var initFolder = get(id);

        if (initFolder.parent === toId) {
            eventsHandler.trigger(supportedCustomEvents.OnError, "Folder already exists in this folder");
            return;
        }

        var newFolder = {
            id: initFolder.id,
            name: initFolder.name,
            parent: toId
        };

        updateFolder(initFolder, newFolder);
    }

    function updateFolder(initFolder, folder) {
        window.Teamlab.updateMailFolder({},
            folder.id,
            folder.name,
            folder.parent,
            {
                success: function(params, newFolder) {
                    userFoldersModal.hide();

                    var reloadNewParent = false;
                    var reloadOldParent = false;
                    var index;

                    index = userFolders.findIndex(function(f) {
                        return f.id === newFolder.id;
                    });
                    if (index > -1) {
                        userFolders[index] = newFolder;
                    }

                    if (initFolder.name !== newFolder.name) {
                        window.toastr.success(MailScriptResource.InfoRenameFolder.format(Encoder.htmlEncode(initFolder.name), Encoder.htmlEncode(newFolder.name)));
                    }

                    if (initFolder.parent !== newFolder.parent) {
                        var folders;
                        if (newFolder.parent > 0) {
                            index = userFolders.findIndex(function(f) {
                                return f.id === newFolder.parent;
                            });
                            if (index > -1) {
                                var newFolderParent = userFolders[index];
                                newFolderParent.folder_count++;

                                folders = userFolders.filter(function(f) {
                                    return f.parent === newFolderParent.id;
                                });

                                if (newFolderParent.folder_count !== folders.length) {
                                    reloadNewParent = true;
                                }
                            }
                        }

                        if (initFolder.parent > 0) {
                            index = userFolders.findIndex(function(f) {
                                return f.id === initFolder.parent;
                            });
                            if (index > -1) {
                                var initFolderParent = userFolders[index];
                                initFolderParent.folder_count--;

                                folders = userFolders.filter(function(f) {
                                    return f.parent === initFolderParent.id;
                                });

                                if (initFolderParent.folder_count !== folders.length) {
                                    reloadOldParent = true;
                                }
                            }
                        }

                        try {
                            if (newFolder.parent > 0) {
                                window.toastr.success(MailScriptResource.InfoMoveFolder
                                    .format(Encoder.htmlEncode(newFolder.name), Encoder.htmlEncode(get(newFolder.parent).name)));
                            } else {
                                window.toastr.success(MailScriptResource.InfoMoveFolderToRoot
                                    .format(Encoder.htmlEncode(newFolder.name)));
                            }
                        } catch (e) {
                            console.error(e);
                        }
                    }

                    if (reloadNewParent || reloadOldParent) {
                        reloadTree(reloadNewParent ? newFolder.parent : initFolder.parent)
                            .then(function(params, data) {
                                refreshUserFolders(data);

                                if (reloadNewParent && reloadOldParent) {
                                    reloadTree(initFolder.parent)
                                        .then(function(params, data) {

                                            refreshUserFolders(data);

                                            eventsHandler
                                                .trigger(supportedCustomEvents.OnUpdate,
                                                [newFolder, initFolder]);
                                        });
                                } else {
                                    eventsHandler
                                        .trigger(supportedCustomEvents.OnUpdate, [newFolder, initFolder]);
                                }
                            });
                    } else {
                        eventsHandler.trigger(supportedCustomEvents.OnUpdate, [newFolder, initFolder]);
                    }
                },
                error: function(params, errors) {
                    window.toastr.error(Encoder.htmlEncode(errors[0]));
                    eventsHandler.trigger(supportedCustomEvents.OnError, errors[0]);
                }
            });
    }

    function checkRemoveUserFolderStatus(operation, options) {
        serviceManager.getMailOperationStatus(operation.id,
        null,
        {
            success: function (params, data) {
                if (data.completed) {
                    clearInterval(progressBarIntervalId);
                    progressBarIntervalId = null;

                    options.onSuccess.call();
                    window.LoadingBanner.hideLoading();
                }
            },
            error: function (params, errors) {
                console.error("checkRemoveUserFolderStatus", e, errors);
                clearInterval(progressBarIntervalId);
                progressBarIntervalId = null;

                options.onError.call(params, errors);
                window.LoadingBanner.hideLoading();
            }
        });
    }

    function removeWithChildren(id) {
        var index = userFolders.findIndex(function(f) {
            return f.id === id;
        });
        if (index > -1) {
            userFolders.splice(index, 1);

            var folders = userFolders
                .filter(function(f) {
                    return f.parent === id;
                });

            folders.forEach(function(f) {
                removeWithChildren(f.id);
            });
        }
    }

    function remove(folder) {
        userFoldersModal.showDelete(
            folder,
            {
                onSave: function(removeFolder) {
                    window.Teamlab.removeMailFolder({},
                        removeFolder.id,
                        {
                            success: function (params, data) {
                                userFoldersModal.hide();

                                window.LoadingBanner.displayMailLoading();

                                progressBarIntervalId = setInterval(function() {
                                        return checkRemoveUserFolderStatus(data,
                                        {
                                            onSuccess: function() {
                                                removeWithChildren(removeFolder.id);

                                                if (removeFolder.parent > 0) {
                                                    var index = userFolders.findIndex(function (f) {
                                                        return f.id === removeFolder.parent;
                                                    });

                                                    if (index > -1) {
                                                        userFolders[index].folder_count--;
                                                    }
                                                }

                                                window.toastr.success(MailScriptResource.InfoRemoveFolder.format(Encoder.htmlEncode(removeFolder.name)));

                                                eventsHandler.trigger(supportedCustomEvents.OnDelete, removeFolder);
                                            },
                                            onError: function (params, errors) {
                                                window.toastr.error(errors[0]);
                                                eventsHandler.trigger(supportedCustomEvents.OnError, errors[0]);
                                            }
                                        });
                                    },
                                    getStatusTimeout);
                            },
                            error: function (params, errors) {
                                window.toastr.error(errors[0]);
                                eventsHandler.trigger(supportedCustomEvents.OnError, errors[0]);
                            }
                        });
                }
            }
        );
    }

    function bind(eventName, fn) {
        eventsHandler.bind(eventName, fn);
    }

    function unbind(eventName) {
        eventsHandler.unbind(eventName);
    }

    function isLoading() {
        return onLoading;
    }

    return {
        init: init,
        bind: bind,
        unbind: unbind,
        events: supportedCustomEvents,

        loadTree: loadTree,
        reloadTree: function(parentId) {
            reloadTree(parentId)
                .then(onGetMailUserFolders);
        },

        getList: getList,
        get: get,
        moveTo: moveTo,

        create: create,
        update: update,
        remove: remove,

        isLoading: isLoading
    };
})(jQuery);