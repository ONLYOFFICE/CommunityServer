/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

/*
 Caches information about filtered messageses list such as first and last messages ids and etc.
 Usefull for detecting message position and prev next displaing.
*/

window.filterCache = (function() {
    var cache;

    function init() {
        cache = {};
        serviceManager.bind(window.Teamlab.events.getMailFilteredConversations, onGetMailConversations);
    }

    function filterHash(filter) {
        return TMMail.strHash(filter.toAnchor(true, {}, false));
    }

    function onGetMailConversations(params, conversations) {
        if (undefined == conversations.length || 0 == conversations.length) {
            return;
        }

        var folder = MailFilter.getFolder();
        var folderCache = (cache[folder] = cache[folder] || {});
        var hash = filterHash(MailFilter);
        var filterCache = (folderCache[hash] = folderCache[hash] || {});

        var hasNext = (true === MailFilter.getPrevFlag()) || params.__total > MailFilter.getPageSize();
        var hasPrev = (false === MailFilter.getPrevFlag() && null != MailFilter.getFromDate() && undefined != MailFilter.getFromDate()) || (true === MailFilter.getPrevFlag() && params.__total > MailFilter.getPageSize());

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

    function getNextPrevConversation(filter, id, next) {
        try {
            return cache[filter.getFolder()][filterHash(filter)]['conversations'][id][true === next ? 'next' : 'prev'] || 0;
        } catch(e) {
        }

        return 0;
    }

    // try to get next conversation id from cache or return 0

    function getNextConversation(filter, id) {
        return getNextPrevConversation(filter, id, true);
    }

    // try to get prev conversation id from cache or return 0

    function getPrevConversation(filter, id) {
        return getNextPrevConversation(filter, id, false);
    }

    // get cached filter info

    function getCache(filter) {
        if (cache[filter.getFolder()]) {
            return cache[filter.getFolder()][filterHash(filter)] || {};
        }
        return {};
    }

    ;

    // drop folder cached values for filter

    function drop(folder) {
        cache[folder] = {};
    }

    ;

    return {
        init: init,
        drop: drop,
        getCache: getCache,
        getNextConversation: getNextConversation,
        getPrevConversation: getPrevConversation
    };
})();