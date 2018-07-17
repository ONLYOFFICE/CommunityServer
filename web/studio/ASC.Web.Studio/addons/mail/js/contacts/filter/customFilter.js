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


window.customFilter = (function ($) {
    var isInit = false;
    var filter;
    var events = $({});

    function init() {
        if (!isInit) {
            isInit = true;

            $('#customFilter').advansedFilter({
                maxfilters: -1,
                sorters: [
                    { id: 'displayName', title: MailScriptResource.FilterByTitle, sortOrder: 'ascending', def: true }
                ],
                filters: [
                    {
                        type: "combobox",
                        id: "personal",
                        apiparamname: "contactType",
                        title: MailScriptResource.FilterPersonalContacts,
                        filtertitle: MailScriptResource.FilterShowGroup,
                        group: MailScriptResource.FilterShowGroup,
                        groupby: 'type',
                        options:
                        [
                            { value: '1', classname: '', title: MailScriptResource.FilterPersonalContacts, def: true },
                            { value: '0', classname: '', title: MailScriptResource.FilterFrequentlyContacted }
                        ]
                    },
                    {
                        type: "combobox",
                        id: "auto",
                        apiparamname: "contactType",
                        title: MailScriptResource.FilterFrequentlyContacted,
                        filtertitle: MailScriptResource.FilterShowGroup,
                        group: MailScriptResource.FilterShowGroup,
                        groupby: 'type',
                        options:
                        [
                            { value: '1', classname: '', title: MailScriptResource.FilterPersonalContacts },
                            { value: '0', classname: '', title: MailScriptResource.FilterFrequentlyContacted, def: true }
                        ]
                    }
                ]
            }).bind('setfilter', onSetFilter).bind('resetfilter', onResetFilter).bind('resetallfilters', onResetAllFilters);

            // filter object initialization should follow after advanced filter plugin call - because
            // its replace target element with new markup
            filter = $('#customFilter');
            events.trigger('ready');
        }
    };

    function onSetFilter(evt, $container, filterItem, value, selectedfilters) {
        events.trigger('set', [filterItem, value, selectedfilters]);
    };

    function onResetFilter(evt, $container, filterItem, selectedfilters) {
        events.trigger('reset', [filterItem, selectedfilters]);
    };

    function onResetAllFilters(evt, $container, filterid, selectedfilters) {
        events.trigger('resetall', [selectedfilters]);
    };

    var showItem = function (id, params) {
        if (!params) {
            params = {};
        }
        filter.advansedFilter({ filters: [{ id: id, params: params }] });
    };

    function show() {
        filter.parent().show();
    };

    function hide() {
        filter.parent().hide();
    };

    function update() {
        filter.advansedFilter('resize');
    };

    function clear() {
        filter.advansedFilter(null);
    };

    var setPersonal = function () {
        showItem('personal', { value: '1' });
    };

    var setAuto = function () {
        showItem('auto', { value: '0' });
    };

    function setSearch(text) {
        filter.advansedFilter({ filters: [{ type: 'text', id: 'text', params: { value: text } }]});
    };

    function setSort(sort, order) {
        filter.advansedFilter({ sorters: [{ id: sort, selected: true, dsc: 'descending' == order }]});
    };

    return {
        init: init,

        clear: clear,
        update: update,

        show: show,
        hide: hide,

        setPersonal: setPersonal,
        setAuto: setAuto,
        setSearch: setSearch,
        setSort: setSort,

        events: events
    };
})(jQuery);