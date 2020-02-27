/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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