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


window.mailCache = (function($) {
    var cache = [],
        cacheLimit = 25,
        inLoad = [];

    function loadToCache(ids) {
        if (inLoad.length === 0)
            inLoad = ids.slice();
        else
            return;

        var needItems = cacheLimit - cache.length;
        if (needItems.length > 0 && ids.length > needItems)
            ids = ids.slice(0, needItems);

        if (cache.length >= cacheLimit) {
            forgetOldest(ids.length);
        }

        async.each(ids, function (id, callback) {
            var data = { loadAll: false, markRead: false };
            console.log("%s is loading to cache", id);
            async.parallel([
                function (cb) {

                    if (commonSettingsPage.isConversationsEnabled()) {
                        Teamlab.getMailConversation({ skipIO: true },
                            id,
                            data,
                            {
                                success: function(params, messages) {
                                    cb(null, messages);
                                },
                                error: function(err) {
                                    cb(err);
                                },
                                async: true
                            });
                    } else {
                        Teamlab.getMailMessage({ skipIO: true },
                            id,
                            data,
                            {
                                success: function (params, messages) {
                                    cb(null, messages);
                                },
                                error: function (err) {
                                    cb(err);
                                },
                                async: true
                            });
                    }
                },
                function (cb) {
                    if (ASC.Mail.Constants.CRM_AVAILABLE) {
                        Teamlab.isConversationLinkedWithCrm(null, id,
                        {
                            success: function (params, status) {
                                cb(null, status);
                            },
                            error: function (params, e) {
                                cb(null, { isLinked: false });
                                console.error(e);
                            },
                            async: true
                        });
                    } else {
                        cb(null, { isLinked: false });
                    }
                }
            ], function (err, data) {
                if (err) {
                    callback(err);
                } else {
                    var content = $.extend(true, {}, { messages: commonSettingsPage.isConversationsEnabled() ? data[0] : [data[0]], hasLinked: data[1].isLinked });
                    set(id, content);
                    callback();
                }
            });
        }, function (err) {
            if (err) {
                console.error("Failed loadToCache", arguments);
            } else {
                console.info("Success loadToCache (%d items). Cache length %d", ids.length, cache.length);
            }
            inLoad = [];
        });
    }

    function getCacheItem(id) {
        id = parseInt(id);

        var cacheItem = null;
        for (var i = 0, n = cache.length; i < n; i++) {
            if (cache[i].id == id) {
                cacheItem = cache[i];
                break;
            }
        }

        return cacheItem;
    }

    function getCacheIndex(id) {
        id = parseInt(id);

        for (var i = 0, n = cache.length; i < n; i++) {
            if (cache[i].id == id) {
                return i;
            }
        }

        return -1;
    }

    function searchMessage(id, messages) {
        for (var i = 0, n = messages.length; i < n; i++) {
            var message = messages[i];
            if (message.id == id) {
                return message;
            }
        }

        return null;
    }

    function getRootMessage(id) {
        var item = getCacheItem(id);
        return item ? item.root : null;
    }

    function getMessage(id) {
        var item = getCacheItemByMessageId(id);
        if (!item)
            return null;

        var message = null,
            rootId = item.id;

        for (var i = 0, n = item.content.messages.length; i < n; i++) {
            var m = item.content.messages[i];
            if (m.id == rootId) {
                message = m;
                break;
            }
        }

        return message;
    }

    function get(id) {
        var cacheItem = getCacheItem(id);
        if (cacheItem && cacheItem.content) {
            return cacheItem.content;
        }

        return undefined;
    }

    function getCacheItemByMessageId(id) {
        for (var i = 0, n = cache.length; i < n; i++) {
            var messages = cache[i].content.messages;
            for (var j = 0, m = messages.length; j < m; j++) {
                if (messages[j].id == id)
                    return cache[i];
            }
        }

        return undefined;
    }

    function getByMessageId(id) {
        var item = getCacheItemByMessageId(id);
        return item ? item.content : undefined;
    }

    function getAll() {
        return cache;
    }

    function getAllMessagesIds(distinct) {
        var ids = [];
        for (var i = 0, n = cache.length; i < n; i++) {
            var messages = cache[i].content.messages;
            if (messages.length > 0) {
                ids = ids.concat(messages.map(function(vM) {
                    return vM.id;
                }));
            }
        }

        return ids.length > 1
            ? (distinct
                ? $.unique(ids.sort())
                : ids.sort())
            : ids;
    }

    function findMissingIds(ids) {
        var allCachedIds = getAllMessagesIds(true),
            missingIds = [];

        if (allCachedIds.length === 0)
            return ids;

        for (var i = 0, n = ids.length; i < n; i++) {
            if ($.inArray(ids[i], allCachedIds) === -1) {
                missingIds.push(ids[i]);
            }
        }

        return missingIds;
    }

    function forgetOldest(count) {
        var oldItems = [];

        var allUnread = $.grep(cache, function (item) {
            return item.unread === false;
        });

        if (allUnread.length > 0) {
            allUnread.sort(function(a, b) {
                if (a.date.isBefore(b.start))
                    return 1;
                if (a.date.isAfter(b.start))
                    return -1;
                return 0;
            });
            oldItems = allUnread.slice(0, count);
        } else {
            var tmp = cache.slice();
            tmp.sort(function (a, b) {
                if (a.date.isBefore(b.date))
                    return -1;
                if (a.date.isAfter(b.date))
                    return 1;
                return 0;
            });

            oldItems = tmp.slice(0, count);
        }

        var forgetIds = oldItems.map(function(v) { return v.id });

        remove(forgetIds);
    }

    function set(id, content) {
        if (!content.hasOwnProperty("messages") ||
            !$.isArray(content.messages) ||
            content.messages.length === 0)
            return;

        var cacheItem = getCacheItem(id);
        if (cacheItem) {
            cacheItem.content = content;
        } else {
            var root;
            if (content.messages.length > 1) {
                root = searchMessage(id, content.messages);
                for (var i = 0, n = content.messages.length; i < n; i++) {
                    var messageId = content.messages[i].id;
                    var oldRoot = getCacheItem(messageId);
                    if (oldRoot) {
                        remove([oldRoot.id]); //TODO: no need to remove, but copy to the new conversation
                        break;
                    }
                }
            } else {
                root = content.messages[0];
            }

            if (cache.length >= cacheLimit) {
                forgetOldest(1);
            }

            id = parseInt(id);

            cache.push({
                id: id,
                content: content,
                date: moment(new Date(root.date)),
                root: root,
                unread: root.wasNew
            });

            console.log("%s was cached. Cache length %d", id, cache.length);
        }
    }

    function setHasLinked(id, hasLinked) {
        var content = get(id);
        if (content) {
            content.hasLinked = hasLinked;
            set(id, content);
        }
    }

    function setImportant(ids, important) {
        if (!ids) return;

        if (!(ids instanceof Array)) {
            ids = [ids];
        }

        for (var i = 0, n = ids.length; i < n; i++) {
            var id = ids[i];

            var content = get(id);
            if (content) {
                for (var j = 0, m = content.messages.length; j < m; j++) {
                    var msg = content.messages[j];
                    if (msg.important !== important) {
                        msg.important = important;
                    }
                }
            }
        }
    }

    function setRead(ids, isRead) {
        if (!ids) return;

        if (!(ids instanceof Array)) {
            ids = [ids];
        }

        for (var i = 0, n = ids.length; i < n; i++) {
            var id = ids[i];
            var item = getCacheItem(id);
            if (item) {
                var needUpdate = false;
                var messages = item.content.messages;
                for (var j = 0, m = messages.length; j < m; j++) {
                    var msg = messages[j];
                    if (msg.wasNew !== !isRead) {
                        msg.wasNew = !isRead;
                        needUpdate = true;
                    }
                }
                if (needUpdate)
                    item.unread = !isRead;
            }
        }
    }

    function setTag(id, tagId, remove) {
        var content = get(id);
        if (content) {
            tagId = parseInt(tagId);
            var needUpdate = false;
            for (var i = 0, n = content.messages.length; i < n; i++) {
                var msg = content.messages[i];
                if (msg.tagIds && msg.tagIds.length > 0) {
                    var index = $.inArray(tagId, msg.tagIds);
                    if (index > -1) {
                        if (remove) {
                            msg.tagIds.splice(index, 1);
                            needUpdate = true;
                        }
                    } else {
                        if (!remove) {
                            msg.tagIds.push(tagId);
                            needUpdate = true;
                        }
                    }
                    
                } else {
                    if (!remove) {
                        msg.tagIds = [tagId];
                        needUpdate = true;
                    }
                }
            }
            if (needUpdate)
                set(id, content);
        }
    }

    function setFolder(ids, folder, userFolderId) {
        for (var i = 0, n = ids.length; i < n; i++) {
            var id = ids[i];
            var content = get(id);
            if (content) {
                var needUpdate = false;
                for (var j = 0, m = content.messages.length; j < m; j++) {
                    var msg = content.messages[j];
                    if (msg.folder != folder) {
                        msg.folder = folder;
                        needUpdate = true;
                    }

                    if (msg.userFolderId != userFolderId) {
                        msg.userFolderId = userFolderId;
                        needUpdate = true;
                    }
                }
                if (needUpdate)
                    set(id, content);
            }
        }
    }

    function setFolderToMessage(id, folder) {
        //TODO: create method for remove message in chain
    }

    function updateMessage(id, newMessage) {
        var item = getCacheItemByMessageId(id);
        if (!item)
            return;

        var content = item.content;
        for (var i = 0, n = content.messages.length; i < n; i++) {
            var message = content.messages[i];

            if (message.id == id) {
                content.messages[i] = newMessage;
                break;
            }
        }
    }

    function remove(ids) {
        for (var i = 0, n = ids.length; i < n; i++) {
            var id = parseInt(ids[i]);
            var cacheIndex = getCacheIndex(id);
            if (cacheIndex > -1) {
                cache.splice(cacheIndex, 1);
                console.log("%s removed from cache. Cache length %d", id, cache.length);
            }
        }
    }

    function clear() {
        cache = [];
    }

    return {
        loadToCache: loadToCache,
        get: get,
        getAll: getAll,
        getAllMessagesIds: getAllMessagesIds,
        getRootMessage: getRootMessage,
        getMessage: getMessage,
        getByMessageId: getByMessageId,
        findMissingIds: findMissingIds,
        set: set,
        setHasLinked: setHasLinked,
        setImportant: setImportant,
        setTag: setTag,
        setRead: setRead,
        setFolder: setFolder,
        updateMessage: updateMessage,
        remove: remove,
        clear: clear
    };

})(jQuery);