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
window.folderFilter = (function($) {
    var isInit = false,
        filter,
        skip_tags_hide = false, // skip tags filter hide, if user removed all tags from filter control
        events = $({}),
        prev_search = '',
        options = {};

    var init = function() {
        if (!isInit) {
            isInit = true;

            var now = new Date();
            var today = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0);
            var yesterday = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0);
            yesterday.setDate(today.getDate() - 1);
            var lastweek = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0);
            lastweek.setDate(today.getDate() - 7);

            options = {
                anykey: true,
                anykeytimeout: 1000,
                maxfilters: -1,
                colcount: 2,
                sorters: [
                    { id: 'date', title: MailScriptResource.FilterByDate, sortOrder: 'descending', def: true }
                ],
                filters: [
                    {
                        type: 'combobox',
                        id: 'unread',
                        title: MailScriptResource.FilterUnread,
                        filtertitle: MailScriptResource.FilterStatusGroup + ":",
                        group: MailScriptResource.FilterStatusGroup,
                        groupby: "status",
                        options:
                        [
                            { value: "unread", title: MailScriptResource.FilterUnread, def: true },
                            { value: "read", title: MailScriptResource.FilterRead }
                        ]
                    },
                    {
                        type: 'combobox',
                        id: 'read',
                        title: MailScriptResource.FilterRead,
                        filtertitle: MailScriptResource.FilterStatusGroup + ":",
                        group: MailScriptResource.FilterStatusGroup,
                        groupby: "status",
                        options:
                        [
                            { value: "unread", title: MailScriptResource.FilterUnread },
                            { value: "read", title: MailScriptResource.FilterRead, def: true }
                        ]
                    },
                    {
                        type: 'flag',
                        id: 'important',
                        title: MailScriptResource.FilterImportant,
                        group: MailScriptResource.FilterAnotherGroup
                    },
                    {
                        type: 'flag',
                        id: 'attachments',
                        title: MailScriptResource.FilterWithAttachments,
                        group: MailScriptResource.FilterAnotherGroup
                    },
                    {
                        type: fromSenderFilter.type,
                        id: 'from',
                        title: MailScriptResource.FilterFromSender,
                        group: MailScriptResource.FilterAnotherGroup,
                        create: fromSenderFilter.create,
                        customize: fromSenderFilter.customize,
                        destroy: fromSenderFilter.destroy,
                        process: fromSenderFilter.process
                    },
                    {
                        type: 'combobox',
                        id: 'to',
                        title: MailScriptResource.FilterToMailAddress,
                        group: MailScriptResource.FilterAnotherGroup,
                        options: []
                    },
                    {
                        type: 'combobox',
                        id: 'tag',
                        title: MailScriptResource.FilterWithTags,
                        group: MailScriptResource.FilterAnotherGroup,
                        options: [],
                        multiple: true,
                        defaulttitle: MailScriptResource.ChooseTag
                    },
                    {
                        type: 'daterange',
                        id: 'today',
                        title: MailScriptResource.FilterPeriodToday,
                        filtertitle: " ",
                        group: MailScriptResource.FilterPeriodGroup,
                        bydefault: { from: today.getTime(), to: today.getTime() }
                    },
                    {
                        type: 'daterange',
                        id: 'yesterday',
                        title: MailScriptResource.FilterPeriodYesterday,
                        filtertitle: " ",
                        group: MailScriptResource.FilterPeriodGroup,
                        bydefault: { from: yesterday.getTime(), to: yesterday.getTime() }
                    },
                    {
                        type: 'daterange',
                        id: 'lastweek',
                        title: MailScriptResource.FilterPeriodLastWeek,
                        filtertitle: " ",
                        group: MailScriptResource.FilterPeriodGroup,
                        bydefault: { from: lastweek.getTime(), to: today.getTime() }
                    },
                    {
                        type: 'daterange',
                        id: 'period',
                        title: MailScriptResource.FilterPeriodCustom,
                        filtertitle: " ",
                        group: MailScriptResource.FilterPeriodGroup
                    }
                ]
            };

            $('#FolderFilter').advansedFilter(options).bind('setfilter', _onSetFilter).bind('resetfilter', _onResetFilter).bind('resetallfilters', _onResetAllFilters);

            // filter object initialization should follow after advansed filter plugin call - because
            // its replace target element with new markup
            filter = $('#FolderFilter');

            filter.find('div.btn-show-filters:first').bind('click', _onShowFilters);
        }
    };

    function _onShowFilters() {
        var with_tags_filter_link = filter.find('li.filter-item[data-id="tag"]');
        if (with_tags_filter_link) {
            if ($('#id_tags_panel_content .tag').length > 0) {
                with_tags_filter_link.show();
            } else {
                with_tags_filter_link.hide();
            }
        }
    }

    var _sort_first_time = true; //initialization raises onSetFilter event with default sorter - it's a workaround

    function _setPeriodFilter($container, value) {
        MailFilter.setPeriod({ from: value.from, to: value.to });
        var to_date_container = $container.find('span.to-daterange-selector:first span.datepicker-container:first');
        if (to_date_container) 
            to_date_container.datepicker("option", "maxDate", new Date()); // not select future dates
    }

    var _onSetFilter = function(evt, $container, filter_item, value, selectedfilters) {
        switch (filter_item.id) {
            case 'unread':
            case 'read':
                _toggleUnread(value.value === "unread");
                break;
            case 'important':
                MailFilter.setImportance(true);
                break;
            case 'attachments':
                MailFilter.setAttachments(true);
                break;
            case 'to':
                MailFilter.setTo(value.value);
                break;
            case 'from':
                MailFilter.setFrom(value.value);
                break;
            case 'today':
                _hideItem('yesterday');
                _hideItem('lastweek');
                _hideItem('period');
                _setPeriodFilter($container, value);
                break;
            case 'yesterday':
                _hideItem('today');
                _hideItem('lastweek');
                _hideItem('period');
                _setPeriodFilter($container, value);
                break;
            case 'lastweek':
                _hideItem('today');
                _hideItem('yesterday');
                _hideItem('period');
                _setPeriodFilter($container, value);
                break;
            case 'period':
                _hideItem('today');
                _hideItem('yesterday');
                _hideItem('lastweek');
                _setPeriodFilter($container, value);
                break;
            case 'text':
                MailFilter.setSearch(value.value);
                prev_search = value.value;
                break;

            case 'tag':
                if (null == value.value) {
                    MailFilter.removeAllTags();
                    skip_tags_hide = true;
                    break;
                }

                $.each(value.value, function (i, v_new) {
                    MailFilter.addTag(v_new);
                });

                $.each(MailFilter.getTags(), function (i, v) {
                    var is_set = false;
                    $.each(value.value, function(j, v_new) {
                        if (v == v_new)
                            is_set = true;
                    });
                    if (!is_set)
                        MailFilter.removeTag(v);
                });
                break;

            case 'sorter': //ToDo refactore
                if (_sort_first_time) {
                    _sort_first_time = false;
                    return;
                }
                if (MailFilter.getSort() == value.id && MailFilter.getSortOrder() == value.sortOrder)
                    return;
                MailFilter.setSort(value.id);
                MailFilter.setSortOrder(value.sortOrder);
                //reset paging
                MailFilter.setFromDate(undefined);
                MailFilter.setFromMessage(undefined);
                MailFilter.setPrevFlag(false);
                break;
            default:
                return;
        }

        window.ASC.Mail.ga_track(ga_Categories.folder, ga_Actions.filterClick, filter_item.id);

        mailBox.updateAnchor();
    };

    var _onResetFilter = function(evt, $container, filter_item, selectedfilters) {
        switch (filter_item.id) {
            case 'unread':
            case 'read':
                _toggleUnread(undefined);
                break;
            case 'important':
                MailFilter.setImportance(false);
                break;
            case 'attachments':
                MailFilter.setAttachments(false);
                break;
            case 'to':
                MailFilter.setTo('');
                break;
            case 'from':
                MailFilter.setFrom('');
                break;
            case 'today':
            case 'yesterday':
            case 'lastweek':
            case 'period':
                MailFilter.setPeriod({ from: 0, to: 0 });
                break;
            case 'text':
                MailFilter.setSearch('');
                break;
            case 'tag':
                MailFilter.removeAllTags();
                break;
            case 'sorter': //ToDo refactore
                return undefined;
            default:
                return;
        }
        mailBox.updateAnchor();
    };

    var reset = function() {
        MailFilter.setImportance(false);
        MailFilter.setAttachments(false);
        MailFilter.setTo('');
        MailFilter.setFrom('');
        MailFilter.setPeriod({ from: 0, to: 0 });
        MailFilter.setSearch('');
        MailFilter.removeAllTags();
        _toggleUnread(undefined);
        mailBox.updateAnchor();
    };

    var _onResetAllFilters = function(evt, $container, filterid, selectedfilters) {
        reset();
    };

    var _toggleUnread = function(flag) {
        MailFilter.setUnread(flag);
    };

    var _showItem = function (id, params) {
        if (!params) params = {};
        filter.advansedFilter({ filters: [{ id: id, params: params }] });
    };

    var _hideItem = function(id) {
        filter.advansedFilter({ filters: [{ id: id, reset: true}] });
    };

    var setUnread = function(flag) {
        if (undefined !== flag) {
            if (flag) {
                _showItem('unread', {value:"unread"});
            }
            else {
                _showItem('read', { value: "read" });
            }
        }
        else {
            _hideItem('read');
            _hideItem('unread');
        }
    };

    var setImportance = function(importance) {
        if (importance)
            _showItem('important');
        else
            _hideItem('important');
    };

    var setAttachments = function(attachments) {
        if (attachments)
            _showItem('attachments');
        else
            _hideItem('attachments');
    };

    var setTo = function(to) {
        if (undefined === to)
            _hideItem('to');
        else
            filter.advansedFilter({ filters: [{ type: 'combobox', id: 'to', params: { value: to}}] });
        events.trigger('to', [to]); //ToDo: Remove it if it isn't necessary
    };

    var setFrom = function(from) {
        if (undefined === from) {
            _hideItem('from');
        } else
            filter.advansedFilter({ filters: [{ type: fromSenderFilter.type, id: 'from', params: { value: from } }] });
    };

    var setPeriod = function(period) {
        if (period.to > 0) {
            //search through seleted filter items
            var selected_filters = filter.advansedFilter();
            for(var i=0; i<selected_filters.length; i++) {
                var val = selected_filters[i];
                if ('today' === val.id || 'yesterday' === val.id || 'lastweek' === val.id)
                    return;
            };
            filter.advansedFilter({ filters: [{ type: 'daterange', id: 'period', params: { to: period.to, from: period.from } }] });
        } else {
            _hideItem('today');
            _hideItem('yesterday');
            _hideItem('lastweek');
            _hideItem('period');
        }
    };

    var setSearch = function(text) {
        if(prev_search == text)
            return;
        filter.advansedFilter({ filters: [{ type: 'text', id: 'text', params: { value: text}}] });
    };

    var setSort = function(sort, order) {
        if (undefined !== sort && undefined !== order) {
            filter.advansedFilter({ sorters: [{ id: sort, selected: true, dsc: 'descending' == order}] });
        } else {
            filter.advansedFilter({ sorters: [{ id: 'date', selected: true, dsc: true}] });
        }
    };

    var setTags = function(tags) {
        if (tags.length) {
            filter.advansedFilter({ filters: [{ type: 'combobox', id: 'tag', params: { value: tags}}] });
        } else {
            if (true === skip_tags_hide) {
                skip_tags_hide = false;
                return;
            }
            $.each(filter.advansedFilter(), function (index, value) {
                if ('tag' == value.id) _hideItem('tag');
            });
        }
    };

    var clear = function() {
        filter.advansedFilter(null);
    };

    var update = function() {
        var toOptions = [];
        $.each(accountsManager.getAccountList(), function(index, value) {
            toOptions.push({ value: value.email, classname: 'to', title: value.email });
        });
        filter.advansedFilter({ filters: [{ type: 'combobox', id: 'to', options: toOptions}] });

        var tags = [];
        $.each(tagsManager.getAllTags(), function(index, value) {
            if (value.lettersCount > 0)
                tags.push({ value: value.id, classname: 'to', title: value.name });
        });
        filter.advansedFilter({ filters: [{ type: 'combobox', id: 'tag', options: tags}] });

        filter.advansedFilter('resize');
    };

    // updates to & from filter titles depending on current page
    function updateToFromFilterTitles() {
        var from_index = 4;
        var to_index = 5;
            var from_id = 'from';
            var to_id = 'to';
        if (TMMail.pageIs('sent') || TMMail.pageIs('drafts')) {
            from_index = 5;
            to_index = 4;
            from_id = 'to';
            to_id = 'from';
        }
        options.filters[from_index].filtertitle = MailScriptResource.FilterFromSender;
        options.filters[to_index].filtertitle = MailScriptResource.FilterToMailAddress;
        _setFilterTitle(from_id, MailScriptResource.FilterFromSender);
        _setFilterTitle(to_id, MailScriptResource.FilterToMailAddress);
    }

    var _setFilterTitle = function (id, title) {
        var menu_item = filter.find('.advansed-filter-list .filter-item[data-id="' + id + '"]');
        menu_item.attr('title', title);
        menu_item.find('.inner-text').html(title);
        filter.find('.advansed-filter-filters .filter-item[data-id="' + id + '"] .title').html(title);
    };

    var show = function () {
        updateToFromFilterTitles();
        filter.parent().show();
    };

    var hide = function () {
        filter.parent().hide();
    };

    return {
        init: init,

        clear: clear,
        update: update,
        reset: reset,

        setUnread: setUnread,
        setImportance: setImportance,
        setAttachments: setAttachments,
        setTo: setTo,
        setFrom: setFrom,
        setPeriod: setPeriod,
        setSearch: setSearch,
        setSort: setSort,
        setTags: setTags,

        show: show,
        hide: hide,

        events: events
    };
})(jQuery);