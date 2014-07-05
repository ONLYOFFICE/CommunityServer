/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
Copyright (c) Ascensio System SIA 2013. All rights reserved.
http://www.teamlab.com
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
    };

    function filterHash(filter) {
        return TMMail.strHash(filter.toAnchor(true, {}, false));
    };

    function onGetMailConversations(params, conversations) {
        if (undefined == conversations.length || 0 == conversations.length)
            return;

        var folder = MailFilter.getFolder();
        var folder_cache = (cache[folder] = cache[folder] || {});
        var hash = filterHash(MailFilter);
        var filter_cache = (folder_cache[hash] = folder_cache[hash] || {});

        var has_next = (true === MailFilter.getPrevFlag()) || params.__total > MailFilter.getPageSize();
        var has_prev = (false === MailFilter.getPrevFlag() && null != MailFilter.getFromDate() && undefined != MailFilter.getFromDate()) || (true === MailFilter.getPrevFlag() && params.__total > MailFilter.getPageSize());

        if (!has_prev)
            filter_cache.first = conversations[0].id;

        if (!has_next)
            filter_cache.last = conversations[conversations.length - 1].id;

        var order_cache = (filter_cache['conversations'] = filter_cache['conversations'] || []);
        for (var i = 0, len = conversations.length - 1; i < len; i++) {
            order_cache[conversations[i].id] = order_cache[conversations[i].id] || {};
            order_cache[conversations[i].id].next = conversations[i + 1].id;

            order_cache[conversations[i + 1].id] = order_cache[conversations[i + 1].id] || {};
            order_cache[conversations[i + 1].id].prev = conversations[i].id;
        };
    };

    // get next or previous conversation id, or 0
    function getNextPrevConversation(filter, id, next) {
        try {
            return cache[filter.getFolder()][filterHash(filter)]['conversations'][id][true === next ? 'next' : 'prev'] || 0;
        }
        catch (e) { }

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
        if (cache[filter.getFolder()])
            return cache[filter.getFolder()][filterHash(filter)] || {};
        return {};
    };

    // drop folder cached values for filter
    function drop(folder) {
        cache[folder] = {};
    };

    return {
        init: init,
        drop: drop,
        getCache: getCache,
        getNextConversation: getNextConversation,
        getPrevConversation: getPrevConversation
    };
})();