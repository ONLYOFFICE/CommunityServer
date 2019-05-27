/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
                anykey: true,
                anykeytimeout: 1000,
                maxfilters: 0,
                hintDefaultDisable: true,
                sorters: [
                    { id: 'displayName', title: MailScriptResource.FilterByTitle, sortOrder: 'ascending', def: true }
                ],
                filters: []
            }).bind('setfilter', onSetFilter).bind('resetfilter', onResetFilter).bind('resetallfilters', onResetAllFilters);

            // filter object initialization should follow after advanced filter plugin call - because
            // its replace target element with new markup
            filter = $('#customFilter');

            filter.find(".advansed-filter-button").hide();

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

    function setSearch(text) {
        filter.advansedFilter({ filters: [{ type: 'text', id: 'text', params: { value: text } }]});
    };

    function setSort(sort, order) {
        filter.advansedFilter({ sorters: [{ id: sort, selected: true, dsc: 'descending' === order }]});
    };

    return {
        init: init,

        clear: clear,
        update: update,

        show: show,
        hide: hide,

        setSearch: setSearch,
        setSort: setSort,

        events: events
    };
})(jQuery);