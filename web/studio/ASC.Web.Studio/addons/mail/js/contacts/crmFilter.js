/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


window.crmFilter = (function($) {
    var isInit = false;
    var filter;
    var events = $({});

    var init = function() {
        if (!isInit) {
            isInit = true;

            $('#crmFilter').advansedFilter({
                maxfilters: -1,
                sorters: [
                    { id: 'displayname', title: MailScriptResource.FilterByTitle, sortOrder: 'ascending', def: true },
                    { id: 'contacttype', title: MailScriptResource.FilterByContactStage, sortOrder: 'ascending' }
                ],
                filters: [
                    {
                        type: "combobox",
                        id: "company",
                        apiparamname: "contactListView",
                        title: MailScriptResource.FilterCompany,
                        filtertitle: MailScriptResource.FilterShowGroup,
                        group: MailScriptResource.FilterShowGroup,
                        groupby: 'type',
                        options:
                        [
                            { value: "company", classname: '', title: MailScriptResource.FilterCompany, def: true },
                            { value: "person", classname: '', title: MailScriptResource.FilterPerson },
                            { value: "withopportunity", classname: '', title: MailScriptResource.FilterWithOpportunity }
                        ]
                    },
                    {
                        type: "combobox",
                        id: "person",
                        apiparamname: "contactListView",
                        title: MailScriptResource.FilterPerson,
                        filtertitle: MailScriptResource.FilterShowGroup,
                        group: MailScriptResource.FilterShowGroup,
                        groupby: 'type',
                        options:
                        [
                            { value: "company", classname: '', title: MailScriptResource.FilterCompany },
                            { value: "person", classname: '', title: MailScriptResource.FilterPerson, def: true },
                            { value: "withopportunity", classname: '', title: MailScriptResource.FilterWithOpportunity }
                        ]
                    },
                    {
                        type: "combobox",
                        id: "withopportunity",
                        apiparamname: "contactListView",
                        title: MailScriptResource.FilterWithOpportunity,
                        filtertitle: MailScriptResource.FilterShowGroup,
                        group: MailScriptResource.FilterShowGroup,
                        groupby: 'type',
                        options:
                        [
                            { value: "company", classname: '', title: MailScriptResource.FilterCompany },
                            { value: "person", classname: '', title: MailScriptResource.FilterPerson },
                            { value: "withopportunity", classname: '', title: MailScriptResource.FilterWithOpportunity, def: true }
                        ]
                    },
                    {
                        type: "combobox",
                        id: "stages",
                        apiparamname: "contactStage",
                        title: MailScriptResource.FilterContactStage,
                        group: MailScriptResource.FilterAnotherGroup,
                        options: [],
                        defaulttitle: MailScriptResource.FilterChoose,
                        enable: false
                    },
                    {
                        type: "combobox",
                        id: "tags",
                        apiparamname: "tags",
                        title: MailScriptResource.FilterWithTags,
                        group: MailScriptResource.FilterAnotherGroup,
                        options: [],
                        defaulttitle: MailScriptResource.FilterChoose,
                        multiple: true,
                        enable: false
                    }
                ]
            }).bind('setfilter', onSetFilter).bind('resetfilter', onResetFilter).bind('resetallfilters', onResetAllFilters);

            // filter object initialization should follow after advansed filter plugin call - because
            // its replace target element with new markup
            filter = $('#crmFilter');

            tagsManager.events.bind('refresh', onUpdateTags);
            tagsManager.events.bind('delete', onUpdateTags);
            tagsManager.events.bind('create', onUpdateTags);
            tagsManager.events.bind('update', onUpdateTags);

            contactStages.events.bind('update', onUpdateStages);
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

    var showItem = function(id, params) {
        if (!params) {
            params = {};
        }
        filter.advansedFilter({ filters: [{ id: id, params: params }] });
    };

    var show = function() {
        filter.parent().show();
    };

    var hide = function() {
        filter.parent().hide();
    };

    var update = function() {
        filter.advansedFilter('resize');
        onUpdateTags();
        contactStages.update();
    };

    var onUpdateTags = function() {
        var tags = [];
        $.each(tagsManager.getAllTags(), function(index, value) {
            if (0 > value.id) {
                tags.push({ value: value.name, classname: 'to', title: value.name });
            }
        });
        filter.advansedFilter({ filters: [{ type: 'combobox', id: 'tags', options: tags, enable: tags.length > 0 }] });
    };

    var onUpdateStages = function() {
        var stages = [];
        $.each(contactStages.getStages(), function(index, value) {
            stages.push({ value: value.id, classname: 'colorFilterItem color_' + value.color.replace('#', ''), title: value.title });
        });
        filter.advansedFilter({ filters: [{ type: 'combobox', id: 'stages', options: stages, enable: stages.length > 0 }] });
        events.trigger('ready');
    };

    var clear = function() {
        filter.advansedFilter(null);
    };

    var setCompany = function() {
        showItem('company', { value: 'company' });
    };

    var setPerson = function() {
        showItem('person', { value: 'person' });
    };

    var setOpportunity = function() {
        showItem('withopportunity', { value: 'withopportunity' });
    };

    var setStage = function(value) {
        filter.advansedFilter({ filters: [{ type: 'combobox', id: 'stages', params: { value: value } }] });
    };
    var setTags = function(tags) {
        filter.advansedFilter({ filters: [{ type: 'combobox', id: 'tags', params: { value: tags } }] });
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

        setCompany: setCompany,
        setPerson: setPerson,
        setOpportunity: setOpportunity,
        setStage: setStage,
        setTags: setTags,
        setSearch: setSearch,
        setSort: setSort,

        events: events
    };
})(jQuery);