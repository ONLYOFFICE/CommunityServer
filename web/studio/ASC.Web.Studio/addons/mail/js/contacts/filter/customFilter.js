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