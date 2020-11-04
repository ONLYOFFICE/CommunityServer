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


/*
 Caches information about filtered messageses list such as first and last messages ids and etc.
 Usefull for detecting message position and prev next displaing.
*/

window.filterCache = (function() {
    var cache;

    function init() {
        cache = {};
        window.Teamlab.bind(window.Teamlab.events.getMailFilteredConversations, onGetMailConversations);
        window.Teamlab.bind(window.Teamlab.events.getMailFilteredMessages, onGetMailConversations);
    }

    function filterHash(filter) {
        return TMMail.strHash(filter.toAnchor(true, {}, false));
    }

    function onGetMailConversations(params, conversations) {
        if (!conversations || 0 === conversations.length) {
            return;
        }

        var folder = MailFilter.getFolder();
        var folderCache = (cache[folder] = cache[folder] || {});
        var hash = filterHash(MailFilter);
        var filterCache = (folderCache[hash] = folderCache[hash] || {});

        var hasNext = (MailFilter.getPrevFlag()) || params.__total > MailFilter.getPageSize();
        var hasPrev = (!MailFilter.getPrevFlag() && MailFilter.getFromDate()) ||
        (MailFilter.getPrevFlag() && params.__total > MailFilter.getPageSize());

        if (!hasPrev) {
            filterCache.first = conversations[0].id;
        }

        if (!hasNext) {
            filterCache.last = conversations[conversations.length - 1].id;
        }

        var orderCache = (filterCache['conversations'] = filterCache['conversations'] || []);
        for (var i = 0, len = conversations.length - 1; i < len; i++) {
            orderCache[conversations[i].id] = orderCache[conversations[i].id] || {};
            orderCache[conversations[i].id].next = conversations[i + 1].id;

            orderCache[conversations[i + 1].id] = orderCache[conversations[i + 1].id] || {};
            orderCache[conversations[i + 1].id].prev = conversations[i].id;
        }
    }

    // get next or previous conversation id, or 0

    function getId(filter, id, next) {
        try {
            return cache[filter.getFolder()][filterHash(filter)]['conversations'][id][true === next ? 'next' : 'prev'] || 0;
        } catch(e) {
        }

        return 0;
    }

    // try to get next conversation id from cache or return 0

    function getNextId(filter, id) {
        return getId(filter, id, true);
    }

    // try to get prev conversation id from cache or return 0

    function getPrevId(filter, id) {
        return getId(filter, id, false);
    }

    // get cached filter info

    function getCache(filter) {
        if (cache[filter.getFolder()]) {
            return cache[filter.getFolder()][filterHash(filter)] || {};
        }
        return {};
    }

    // drop folder cached values for filter

    function drop(folder) {
        cache[folder] = {};
    }

    return {
        init: init,
        drop: drop,
        getCache: getCache,
        getNextId: getNextId,
        getPrevId: getPrevId
    };
})();