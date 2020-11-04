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


window.folderFilter = (function($) {
    var isInit = false,
        filter,
        skipTagsHide = false, // skip tags filter hide, if user removed all tags from filter control
        events = $({}),
        prevSearch = '',
        options = {},
        defaultTagIdReplacement = -1000000000;

    var init = function() {
        if (!isInit) {
            isInit = true;

            options = {
                anykey: true,
                anykeytimeout: 1000,
                maxfilters: -1,
                hintDefaultDisable: true,
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
                        options: [],
                        defaulttitle: MailScriptResource.FilterChoose
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
                        type: 'combobox',
                        id: 'lastweek',
                        title: MailScriptResource.FilterPeriodLastWeek,
                        filtertitle: MailScriptResource.FilterByPeriod,
                        group: MailScriptResource.FilterPeriodGroup,
                        groupby: "byDate",
                        options:
                        [
                            { value: "lastweek", classname: '', title: MailScriptResource.FilterPeriodLastWeek, def: true },
                            { value: "yesterday", classname: '', title: MailScriptResource.FilterPeriodYesterday },
                            { value: "today", classname: '', title: MailScriptResource.FilterPeriodToday }
                        ]
                    },
                    {
                        type: 'combobox',
                        id: 'yesterday',
                        title: MailScriptResource.FilterPeriodYesterday,
                        filtertitle: MailScriptResource.FilterByPeriod,
                        group: MailScriptResource.FilterPeriodGroup,
                        groupby: "byDate",
                        options:
                        [
                            { value: "lastweek", classname: '', title: MailScriptResource.FilterPeriodLastWeek },
                            { value: "yesterday", classname: '', title: MailScriptResource.FilterPeriodYesterday, def: true },
                            { value: "today", classname: '', title: MailScriptResource.FilterPeriodToday }
                        ]
                    },
                    {
                        type: 'combobox',
                        id: 'today',
                        title: MailScriptResource.FilterPeriodToday,
                        filtertitle: MailScriptResource.FilterByPeriod,
                        group: MailScriptResource.FilterPeriodGroup,
                        groupby: "byDate",
                        options:
                        [
                            { value: "lastweek", classname: '', title: MailScriptResource.FilterPeriodLastWeek },
                            { value: "yesterday", classname: '', title: MailScriptResource.FilterPeriodYesterday },
                            { value: "today", classname: '', title: MailScriptResource.FilterPeriodToday, def: true }
                        ]
                    },
                    {
                        type: 'daterange',
                        id: 'period',
                        title: MailScriptResource.FilterPeriodCustom,
                        filtertitle: MailScriptResource.FilterPeriodCustom,
                        group: MailScriptResource.FilterPeriodGroup
                    },
                    {
                        type: 'flag',
                        id: 'calendar',
                        title: MailScriptResource.FilterWithCalendar,
                        group: MailScriptResource.FilterAnotherGroup
                    }
                ]
            };

            $('#FolderFilter').advansedFilter(options).bind('setfilter', onSetFilter).bind('resetfilter', onResetFilter).bind('resetallfilters', onResetAllFilters);

            // filter object initialization should follow after advansed filter plugin call - because
            // its replace target element with new markup
            filter = $('#FolderFilter');

            filter.find('div.btn-show-filters:first').bind('click', onShowFilters);
        }
    };

    function onShowFilters() {
        var withTagsFilterLink = filter.find('li.filter-item[data-id="tag"]');
        if (withTagsFilterLink) {
            if (tagsManager.getAllTags().length > 0) {
                withTagsFilterLink.show();
            } else {
                withTagsFilterLink.hide();
            }
        }
    }

    var sortFirstTime = true; //initialization raises onSetFilter event with default sorter - it's a workaround

    function setPeriodWithinFilter($container, value) {
        MailFilter.setPeriod({ from: 0, to: 0 });
        MailFilter.setPeriodWithin(value.value);
    }

    function setPeriodFilter($container, value) {
        MailFilter.setPeriodWithin('');
        MailFilter.setPeriod({ from: value.from, to: value.to });
        var toDateContainer = $container.find('span.to-daterange-selector:first span.datepicker-container:first');
        if (toDateContainer) {
            toDateContainer.datepicker("option", "maxDate", new Date());
        } // not select future dates
    }

    var onSetFilter = function(evt, $container, filterItem, value) {
        switch (filterItem.id) {
            case 'unread':
            case 'read':
                toggleUnread(value.value === "unread");
                break;
            case 'important':
                MailFilter.setImportance(true);
                break;
            case 'calendar':
                MailFilter.setWithCalendar(true);
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
                hideItem('yesterday');
                hideItem('lastweek');
                hideItem('period');
                setPeriodWithinFilter($container, value);
                break;
            case 'yesterday':
                hideItem('today');
                hideItem('lastweek');
                hideItem('period');
                setPeriodWithinFilter($container, value);
                break;
            case 'lastweek':
                hideItem('today');
                hideItem('yesterday');
                hideItem('period');
                setPeriodWithinFilter($container, value);
                break;
            case 'period':
                hideItem('today');
                hideItem('yesterday');
                hideItem('lastweek');
                setPeriodFilter($container, value);
                break;
            case 'text':
                MailFilter.setSearch(value.value);
                prevSearch = value.value;
                break;
            case 'tag':
                if (null == value.value) {
                    MailFilter.removeAllTags();
                    skipTagsHide = true;
                    break;
                }

                $.each(value.value, function (i, vNew) {
                    vNew.id = vNew.id === defaultTagIdReplacement ? -1 : vNew.id;
                    MailFilter.addTag(vNew);
                });

                $.each(MailFilter.getTags(), function(i, v) {
                    var isSet = false;
                    $.each(value.value, function(j, vNew) {
                        if (v == vNew) {
                            isSet = true;
                        }
                    });
                    if (!isSet) {
                        MailFilter.removeTag(v);
                    }
                });
                break;
            case 'sorter':
                //ToDo refactore
                if (sortFirstTime) {
                    sortFirstTime = false;
                    return;
                }
                if (MailFilter.getSort() == value.id && MailFilter.getSortOrder() == value.sortOrder) {
                    return;
                }
                MailFilter.setSort(value.id);
                MailFilter.setSortOrder(value.sortOrder);
                break;
            default:
                return;
        }

        window.ASC.Mail.ga_track(ga_Categories.folder, ga_Actions.filterClick, filterItem.id);

        //reset paging
        MailFilter.setFromDate(undefined);
        MailFilter.setFromMessage(undefined);
        MailFilter.setPrevFlag(false);

        mailBox.updateAnchor();
    };

    var onResetFilter = function(evt, $container, filterItem) {
        switch (filterItem.id) {
            case 'unread':
            case 'read':
                toggleUnread(undefined);
                break;
            case 'important':
                MailFilter.setImportance(false);
                break;
            case 'calendar':
                MailFilter.setWithCalendar(false);
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
                MailFilter.setPeriodWithin('');
                break;
            case 'period':
                MailFilter.setPeriod({ from: 0, to: 0 });
                break;
            case 'text':
                MailFilter.setSearch('');
                break;
            case 'tag':
                MailFilter.removeAllTags();
                break;
            case 'sorter':
                //ToDo refactore
                return undefined;
            default:
                return;
        }

        //reset paging
        MailFilter.setFromDate(undefined);
        MailFilter.setFromMessage(undefined);
        MailFilter.setPrevFlag(false);

        mailBox.updateAnchor();
    };

    var reset = function() {
        MailFilter.setImportance(false);
        MailFilter.setWithCalendar(false);
        MailFilter.setAttachments(false);
        MailFilter.setTo('');
        MailFilter.setFrom('');
        MailFilter.setPeriod({ from: 0, to: 0 });
        MailFilter.setPeriodWithin('');
        MailFilter.setSearch('');
        MailFilter.removeAllTags();
        toggleUnread(undefined);
        mailBox.updateAnchor();
    };

    var onResetAllFilters = function() {
        reset();
    };

    var toggleUnread = function(flag) {
        MailFilter.setUnread(flag);
    };

    var showItem = function(id, params) {
        if (!params) {
            params = {};
        }
        filter.advansedFilter({ filters: [{ id: id, params: params }] });
    };

    var hideItem = function(id) {
        filter.advansedFilter({ filters: [{ id: id, reset: true }] });
    };

    var setUnread = function(flag) {
        if (undefined !== flag) {
            if (flag) {
                showItem('unread', { value: "unread" });
            } else {
                showItem('read', { value: "read" });
            }
        } else {
            hideItem('read');
            hideItem('unread');
        }
    };

    var setImportance = function(importance) {
        if (importance) {
            showItem('important');
        } else {
            hideItem('important');
        }
    };

    var setWithCalendar = function (withCalendar) {
        if (withCalendar) {
            showItem('calendar');
        } else {
            hideItem('calendar');
        }
    };

    var setAttachments = function(attachments) {
        if (attachments) {
            showItem('attachments');
        } else {
            hideItem('attachments');
        }
    };

    var setTo = function(to) {
        if (undefined === to) {

            var selected = false;
            $.each(filter.advansedFilter(), function (index, value) {
                if ('to' == value.id) {
                    selected = true;
                    return;
                }
            });

            if (selected) {
                hideItem('to');
            } else {
                var visible = filter.find('.advansed-filter-filters .filter-item[data-id="to"]').length > 0;
                if (!visible) {
                    hideItem('to');
                }
            }

        } else {
            filter.advansedFilter({ filters: [{ type: 'combobox', id: 'to', params: { value: to } }] });
        }
        events.trigger('to', [to]); //ToDo: Remove it if it isn't necessary
    };

    var setFrom = function(from) {
        if (undefined === from) {

            var selected = false;
            $.each(filter.advansedFilter(), function (index, value) {
                if ('from' == value.id) {
                    selected = true;
                    return;
                }
            });

            if (selected) {
                hideItem('from');
            } else {
                var visible = filter.find('.advansed-filter-filters .filter-item[data-id="from"]').length > 0;
                if (!visible) {
                    hideItem('from');
                }
            }

        } else {
            filter.advansedFilter({ filters: [{ type: fromSenderFilter.type, id: 'from', params: { value: from } }] });
        }
    };

    var setPeriod = function(period) {
        if (period.period.to > 0) {
            hideItem('today');
            hideItem('yesterday');
            hideItem('lastweek');
            filter.advansedFilter({ filters: [{ type: 'daterange', id: 'period', params: { to: period.period.to, from: period.period.from } }] });
        } else if (period.period_within != '') {
            hideItem('period');
            filter.advansedFilter({ filters: [{ type: 'combobox', id: period.period_within, params: { value: period.period_within } }] });
        } else {
            hideItem('today');
            hideItem('yesterday');
            hideItem('lastweek');
            hideItem('period');
        }
    };

    var setSearch = function(text) {
        if (prevSearch == text) {
            return;
        }
        filter.advansedFilter({ filters: [{ type: 'text', id: 'text', params: { value: text } }] });
    };

    var setSort = function(sort, order) {
        if (undefined !== sort && undefined !== order) {
            filter.advansedFilter({ sorters: [{ id: sort, selected: true, dsc: 'descending' == order }] });
        } else {
            filter.advansedFilter({ sorters: [{ id: 'date', selected: true, dsc: true }] });
        }
    };

    var setTags = function(tags) {
        if (tags.length) {
            var listTags = tags.map(function(id) {
                id = +id === -1 ? defaultTagIdReplacement : id;
                return id;
            });
            filter.advansedFilter({ filters: [{ type: 'combobox', id: 'tag', params: { value: listTags } }] });
        } else {
            if (true === skipTagsHide) {
                skipTagsHide = false;
                return;
            }

            hideItem('tag');
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
        filter.advansedFilter({ filters: [{ type: 'combobox', id: 'to', options: toOptions }] });

        var tags = [];
        $.each(tagsManager.getAllTags(), function(index, value) {
            tags.push({ value: value.id === -1 ? defaultTagIdReplacement : value.id, classname: 'to', title: value.name });
        });
        filter.advansedFilter({ filters: [{ type: 'combobox', id: 'tag', options: tags }] });

        filter.advansedFilter('resize');
    };

    // updates to & from filter titles depending on current page

    function updateToFromFilterTitles() {
        var fromIndex = 4;
        var toIndex = 5;
        var fromId = 'from';
        var toId = 'to';
        if (TMMail.pageIs('sent') || TMMail.pageIs('drafts')) {
            fromIndex = 5;
            toIndex = 4;
            fromId = 'to';
            toId = 'from';
        }
        options.filters[fromIndex].filtertitle = MailScriptResource.FilterFromSender;
        options.filters[toIndex].filtertitle = MailScriptResource.FilterToMailAddress;
        setFilterTitle(fromId, MailScriptResource.FilterFromSender);
        setFilterTitle(toId, MailScriptResource.FilterToMailAddress);
    }

    var setFilterTitle = function(id, title) {
        var menuItem = filter.find('.advansed-filter-list .filter-item[data-id="' + id + '"]');
        menuItem.attr('title', title);
        menuItem.find('.inner-text').html(title);
        filter.find('.advansed-filter-filters .filter-item[data-id="' + id + '"] .title').html(title);
    };

    var show = function() {
        updateToFromFilterTitles();
        filter.parent().show();
    };

    var hide = function() {
        filter.parent().hide();
    };

    return {
        init: init,

        clear: clear,
        update: update,
        reset: reset,

        setUnread: setUnread,
        setImportance: setImportance,
        setWithCalendar: setWithCalendar,
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