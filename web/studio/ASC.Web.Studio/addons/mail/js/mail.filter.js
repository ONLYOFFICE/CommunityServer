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

window.MailFilter = (function($) {
    var isInit = false;

    var _attachments,
        _tags,
        _from,
        _importance,
        _page_size,
        _period_from,
        _period_to,
        _folder,
        _search,
        _sort,
        _sort_order,
        _to,
        _unread,
        _from_date,
        _from_message,
        _prev_flag;

    var init = function() {
        if (isInit === false) {
            isInit = true;

            _attachments = false;
            _tags = new Array();
            _from = undefined;
            _importance = false;
            _page_size = TMMail.option('MessagesPageSize');
            _period_from = 0;
            _period_to = 0;
            _folder = '1';
            _search = '';
            _sort = 'date';
            _sort_order = 'descending';
            _to = undefined;
            _unread = undefined;
            _from_date = undefined;
            _from_message = undefined;
            _prev_flag = false;

            reset();
        }
    };

    var reset = function() {
        _resetFolder();
        _resetSearch();
        _resetTags();
        _resetFrom();
        _resetTo();
    };


    /*to*/
    var setTo = function(mailbox) {
        _to = mailbox;
    };

    var getTo = function() {
        return _to;
    };

    var _resetTo = function() {
        _to = undefined;
    };

    /*from*/
    var setFrom = function(mailbox) {
        _from = mailbox;
    };

    var getFrom = function() {
        return _from;
    };

    var _resetFrom = function() {
        _from = undefined;
    };


    /*tags*/
    var addTag = function(tagid) {
        if (-1 == $.inArray(tagid, _tags))
            _tags.push(tagid);
    };

    var removeTag = function(tagid) {
        _tags = $.grep(_tags, function(value) {
            return value != tagid;
        });
    };

    var removeAllTags = function() {
        _resetTags();
    };

    // toggles tag selection state
    // returns new state: true - selected, false - not selected
    var toggleTag = function (tagid) {
        var res = -1 == $.inArray(tagid, _tags);
        if (res)
            addTag(tagid);
        else
            removeTag(tagid);
        return res;
    };

    var getTags = function() {
        var res = _tags;
        return res;
    };

    var _parseTags = function(tags) {
        var arr = tags.split(',');
        $.each(arr, function(index, value) {
            addTag(value);
        });
    };

    var _resetTags = function() {
        _tags = new Array();
    };


    /*unread*/
    var setUnread = function(unread) {
        _unread = unread;
    };

    var getUnread = function() {
        return _unread;
    };


    /*attachments*/
    var getAttachments = function() {
        return _attachments;
    };

    var setAttachments = function(attachments) {
        _attachments = attachments;
    };


    /*period*/
    var getPeriod = function() {
        return { from: _period_from,
            to: _period_to
        };
    };

    var setPeriod = function(period) {
        _period_from = period.from;
        _period_to = period.to;
    };

    /*importance*/
    var setImportance = function(importance) {
        _importance = importance;
    };

    var getImportance = function() {
        return _importance;
    };


    /*primary folder*/
    var setFolder = function(folder) {
        _folder = folder;
    };

    var _resetFolder = function() {
        _folder = '1';
    };

    var getFolder = function() {
        return _folder;
    };

    /* from date*/
    var setFromDate = function(new_from_date) {
        _from_date = new_from_date;
    };

    var getFromDate = function() {
        return _from_date;
    };

    /* from message*/
    var setFromMessage = function(new_from_message) {
        _from_message = new_from_message;
    };

    var getFromMessage = function() {
        return _from_message;
    };

    /* prev flag*/
    var setPrevFlag = function(new_val) {
        _prev_flag = new_val;
    };

    var getPrevFlag = function() {
        return _prev_flag;
    };

    /*search*/
    var setSearch = function(searchfilter) {
        _search = searchfilter;
    };

    var getSearch = function() {
        return _search;
    };

    var _resetSearch = function() {
        _search = '';
    };

    /*sort*/
    var setSort = function(sort) {
        _sort = sort;
    };

    var getSort = function() {
        return _sort;
    };

    var _resetSort = function() {
        _sort = 'date';
    };

    var setSortOrder = function(order) {
        _sort_order = order;
    };

    var getSortOrder = function() {
        return _sort_order;
    };

    var _resetSortOrder = function() {
        _sort_order = 'descending';
    };


    /*page & page size*/
    var setPageSize = function(new_size) {
        _page_size = parseInt(new_size);
    };

    var getPageSize = function(new_page) {
        return _page_size;
    };

    /*anchor*/
    var fromAnchor = function(folder, params) {
        reset();

        var to,
            tags,
            importance,
            unread,
            from,
            search,
            attachments,
            period,
            sort,
            sortorder,
            page_size,
            from_date,
            from_message,
            prev_flag;

        if (typeof params !== 'undefined') {
            to = TMMail.getParamsValue(params, /to=([^\/]+)/);
            tags = TMMail.getParamsValue(params, /tag=([^\/]+)/);
            importance = TMMail.getParamsValue(params, /(importance)/);
            unread = TMMail.getParamsValue(params, /unread=([^\/]+)/);
            attachments = TMMail.getParamsValue(params, /(attachments)/);
            from = TMMail.getParamsValue(params, /from=([^\/]+)/);
            page_size = TMMail.getParamsValue(params, /page_size=(\d+)/);
            period = TMMail.getParamsValue(params, /period=([^\/]+)/);
            search = TMMail.getParamsValue(params, /search=([^\/]+)/);
            sort = TMMail.getParamsValue(params, /sort=([^\/]+)/);
            sortorder = TMMail.getParamsValue(params, /sortorder=([^\/]+)/);
            from_date = TMMail.getParamsValue(params, /from_date=([^\/]+)/);
            from_message = TMMail.getParamsValue(params, /from_message=([^\/]+)/);
            prev_flag = TMMail.getParamsValue(params, /prev=([^\/]+)/);
        }

        var itemId = TMMail.GetSysFolderIdByName(folder, TMMail.sysfolders.inbox.id);
        setFolder(itemId);

        if (to)
            setTo(decodeURIComponent(to));
        else
            _resetTo();

        if (from)
            setFrom(decodeURIComponent(from));
        else
            _resetFrom();

        if (tags)
            _parseTags(tags);

        if (importance)
            setImportance(true);
        else
            setImportance(false);

        if (attachments)
            setAttachments(true);
        else
            setAttachments(false);

        if (unread) {
            if ('true' == unread)
                setUnread(true);
            else
                setUnread(false);
        }
        else
            setUnread(undefined);

        if (search)
            setSearch(decodeURIComponent(search));
        else
            _resetSearch();

        if (sort)
            setSort(sort);
        else
            _resetSort();

        if (sortorder)
            setSortOrder(sortorder);
        else
            _resetSortOrder();

        if (period) {
            from = parseInt(period.split(',')[0]);
            to = parseInt(period.split(',')[1]);
            setPeriod({ from: from, to: to });
        }
        else {
            setPeriod({ from: 0, to: 0 });
        }

        if (from_date)
            _from_date = new Date(from_date);
        else
            _from_date = undefined;

        if (from_message)
            _from_message = +from_message;
        else
            _from_message = undefined;

        if ('true' == prev_flag)
            _prev_flag = true;
        else
            _prev_flag = false;

        if (page_size)
            setPageSize(page_size);
        else
            setPageSize(TMMail.option('MessagesPageSize'));
    };

    var toAnchor = function(include_paging_info, data, skip_prev_next) {
        var res = '/';

        var f = {};
        f.tags = _tags;
        f.unread = getUnread();
        f.importance = getImportance();
        f.attachments = getAttachments();
        f.to = getTo();
        f.from = getFrom();
        f.sort_order = getSortOrder();
        f.period = getPeriod();
        f.search = getSearch();
        f.page_size = _page_size;
        f.from_date = _from_date;
        f.from_message = _from_message;
        f.prev_flag = _prev_flag;

        $.extend(f, data);

        if (0 < f.tags.length) {
            res += 'tag=';
            $.each(f.tags, function(index, value) {
                res += value;
                if (index != f.tags.length - 1)
                    res += ',';
            });
            res += '/';
        }

        if (undefined !== f.unread) {
            if (f.unread)
                res += 'unread=true/';
            else
                res += 'unread=false/';
        }

        if (f.importance)
            res += 'importance/';

        if (f.attachments)
            res += 'attachments/';

        if (f.to)
            res += 'to=' + encodeURIComponent(f.to) + '/';

        if (f.from)
            res += 'from=' + encodeURIComponent(f.from) + '/';

        // skip sort order if it has default value
        if (f.sort_order && f.sort_order != 'descending')
            res += 'sortorder=' + f.sort_order + '/';

        if (f.period.to > 0)
            res += 'period=' + f.period.from + ',' + f.period.to + '/';

        if ('' != f.search)
            res += 'search=' + encodeURIComponent(f.search) + '/';

        if (include_paging_info === true) {
            res += 'page_size=' + f.page_size + '/';
        }

        if (true === skip_prev_next)
            return res;

        if (f.from_date)
            res += 'from_date=' + f.from_date + '/';

        if (f.from_message)
            res += 'from_message=' + f.from_message + '/';

        if (true === f.prev_flag)
            res += 'prev=true/';

        return res;
    };

    var anchorHasMarkStatus = function(status, anchor) {
        if (!anchor) anchor = ASC.Controls.AnchorController.getAnchor();
        if (status == 'read' || status == 'unread') {
            return undefined !== TMMail.getParamsValue(anchor, /\/unread=([^\/]+)/);
        }
        if (status == 'important' || status == 'normal') {
            return undefined !== TMMail.getParamsValue(anchor, /\/(importance)/);
        }
        return false;
    };

    var isBlank = function() {
        if (0 < _tags.length ||
            _to != undefined && 0 < _to.length ||
            _from != undefined && 0 < _from.length ||
            _attachments != false ||
            _importance != false ||
            _period_from != 0 ||
            _period_to != 0 ||
            _search != '' ||
            _unread != undefined)
            return false;

        return true;
    };

    function toData() {
        var res = {};
        res.folder = _folder;
        res.page_size = _page_size;
        if (getUnread() != undefined)
            res.unread = getUnread();
        if (getAttachments())
            res.attachments = true;
        if (getImportance())
            res.important = true;
        if (0 != _period_from && 0 != _period_to) {
            res.period_from = _period_from;
            res.period_to = _period_to;
        }

        if (getFrom()) res.find_address = _from;
        if (getTo()) {
            var mail_box = accountsManager.getAccountByAddress(_to);
            if (mail_box)
                res.mailbox_id = mail_box.mailbox_id;
        }

        if (getTags().length > 0)
            res.tags = getTags();
        if (getSearch() != '')
            res.search = getSearch();
        if (getSort()) {
            res.sort = getSort();
            res.sortorder = getSortOrder();
        }
        if (undefined != _from_date)
            res.from_date = _from_date;
        if (undefined != _from_message)
            res.from_message = _from_message;
        if (true === _prev_flag)
            res.prev_flag = true;
        return res;
    }

    return {
        init: init,

        getUnread: getUnread,
        setUnread: setUnread,

        getAttachments: getAttachments,
        setAttachments: setAttachments,

        getPeriod: getPeriod,
        setPeriod: setPeriod,

        addTag: addTag,
        removeTag: removeTag,
        removeAllTags: removeAllTags,
        toggleTag: toggleTag,
        getTags: getTags,


        getImportance: getImportance,
        setImportance: setImportance,

        getFolder: getFolder,

        setSearch: setSearch,
        getSearch: getSearch,

        setTo: setTo,
        getTo: getTo,

        setFrom: setFrom,
        getFrom: getFrom,

        setSort: setSort,
        getSort: getSort,
        setSortOrder: setSortOrder,
        getSortOrder: getSortOrder,

        setPageSize: setPageSize,
        getPageSize: getPageSize,

        setFolder: setFolder,

        setFromDate: setFromDate,
        getFromDate: getFromDate,

        setFromMessage: setFromMessage,
        getFromMessage: getFromMessage,

        setPrevFlag: setPrevFlag,
        getPrevFlag: getPrevFlag,

        fromAnchor: fromAnchor,
        toAnchor: toAnchor,
        toData: toData,

        anchorHasMarkStatus: anchorHasMarkStatus,

        isBlank: isBlank,
        reset: reset
    };
})(jQuery);
