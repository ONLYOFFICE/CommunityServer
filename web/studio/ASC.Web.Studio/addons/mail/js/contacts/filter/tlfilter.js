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


window.tlFilter = (function($) {
    var isInit = false;
    var filter;
    var events = $({});

    var init = function() {
        if (!isInit) {
            isInit = true;

            $('#tlFilter').advansedFilter({
                anykey: true,
                anykeytimeout: 1000,
                maxfilters: -1,
                hintDefaultDisable: true,
                sorters: [
                    { id: 'firstname', title: ASC.Resources.Master.Resource.FirstName, sortOrder: 'ascending', def: ASC.Mail.Master.userDisplayFormat == 1 },
                    { id: 'lastname', title: ASC.Resources.Master.Resource.LastName, sortOrder: 'ascending', def: ASC.Mail.Master.userDisplayFormat == 2 }
                ],
                filters: [
                    {
                        type: "combobox",
                        id: "group",
                        apiparamname: "group",
                        title: ASC.Mail.Constants.FiLTER_BY_GROUP_LOCALIZE,
                        group: MailScriptResource.FilterShowGroup,
                        options: [],
                        defaulttitle: MailScriptResource.FilterChoose
                    }
                ]
            }).bind('setfilter', onSetFilter).bind('resetfilter', onResetFilter).bind('resetallfilters', onResetAllFilters);

            // filter object initialization should follow after advanced filter plugin call - because
            // its replace target element with new markup
            filter = $('#tlFilter');

            tlGroups.events.bind('update', onUpdateGroups);
        }
    };

    var onSetFilter = function(evt, $container, filterItem, value, selectedfilters) {
        events.trigger('set', [filterItem, value, selectedfilters]);
    };

    var onResetFilter = function(evt, $container, filterItem, selectedfilters) {
        events.trigger('reset', [filterItem, selectedfilters]);
    };

    var onResetAllFilters = function(evt, $container, filterid, selectedfilters) {
        events.trigger('resetall', [selectedfilters]);
    };

    var show = function() {
        filter.parent().show();
    };

    var hide = function() {
        filter.parent().hide();
    };

    var update = function() {
        filter.advansedFilter('resize');
        tlGroups.update();
    };

    var onUpdateGroups = function() {
        var groups = [];
        $.each(tlGroups.getGroups(), function(index, value) {
            groups.push({ value: value.id, classname: '', title: value.name });
        });
        filter.advansedFilter({ filters: [{ type: 'combobox', id: 'group', options: groups, enable: groups.length > 0 }] });
        events.trigger('ready');
    };

    var clear = function() {
        filter.advansedFilter(null);
    };

    var setGroup = function(value) {
        filter.advansedFilter({ filters: [{ type: 'combobox', id: 'group', params: { value: value } }] });
    };

    var setSearch = function(text) {
        filter.advansedFilter({ filters: [{ type: 'text', id: 'text', params: { value: text } }] });
    };

    var setSort = function(sort, order) {
        filter.advansedFilter({ sorters: [{ id: sort, selected: true, dsc: 'descending' == order }] });
    };

    return {
        init: init,

        clear: clear,
        update: update,

        show: show,
        hide: hide,

        setGroup: setGroup,
        setSearch: setSearch,
        setSort: setSort,

        events: events
    };
})(jQuery);