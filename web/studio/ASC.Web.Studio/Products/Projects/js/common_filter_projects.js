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


ASC.Projects.ProjectsAdvansedFilter = (function () {
    var firstload = true,
        basePath = "",
        baseSortBy = "",
        baseFilter = true,
        filter,
        currentSettings;

    var tasksAdditionalFilterId = "t_",
        projectsAdditionalFilterId = "p_",
        milestonesAdditionalFilterId = "m_",
        discussionsAdditionalFilterId = "d_",
        timeTrackingAdditionalFilterId = "r_";

    var teamMemberFilter = "team_member",
        meTeamMemberFilter = "me_team_member",

        meResponsibleForMilestoneFilter = "me_responsible_for_milestone",
        responsibleForMilestoneFilter = "responsible_for_milestone",

        meProjectManagerFilter = "me_project_manager",
        projectManagerFilter = "project_manager",

        meAuthorFilter = "me_author",
        authorFilter = "author",

        userFilter = "user",

        meTasksResponsibleFilter = "me_tasks_responsible",
        tasksResponsibleFilter = "tasks_responsible",

        meTasksCreatorFilter = "me_tasks_creator",
        tasksCreatorFilter = "tasks_creator",

        meTasksFilter = "me_tasks",
        userTasksFilter = "user_tasks",

        noresponsibleFilter = "noresponsible",

        groupFilter = "group",
        followedFilter = "followed",
        tagFilter = "tag",
        notagFilter = "notag",
        textFilter = "text",

        projectFilter = "project",
        myprojectsFilter = "myprojects",

        milestoneFilter = "milestone",
        mymilestonesFilter = "mymilestones",
        nomilestonesFilter = "nomilestones",

        statusFilter = "status",
        substatusFilter = "substatus",
        openFilter = "open",
        closedFilter = "closed",
        archivedFilter = "archived",
        pausedFilter = "paused",

        paymentStatusFilter = "payment_status",
        notChargeableFilter = "notChargeable",
        notBilledFilter = "notBilled",
        billedFilter = "billed",

        periodFilter = "period",
        today2Filter = "today2",
        createdFilter = "created",
        previousweek2Filter = "previousweek2",
        previousmonth2Filter = "previousmonth2",
        period2Filter = "period2",
        overdueFilter = "overdue",
        todayFilter = "today",
        upcomingFilter = "upcoming",
        recentFilter = "recent",
        today3Filter = "today3",
        yesterdayFilter = "yesterday",
        currentweekFilter = "currentweek",
        previousweekFilter = "previousweek",
        currentmonthFilter = "currentmonth",
        previousmonthFilter = "previousmonth",
        currentyearFilter = "currentyear",
        previousyearFilter = "previousyear",
        deadlineFilter = "deadline",
        deadlineStartFilter = "deadlineStart",
        deadlineStopFilter = "deadlineStop",
        createdStartFilter = "createdStart",
        createdStopFilter = "createdStop",
        periodStartFilter = "periodStart",
        periodStopFilter = "periodStop",

        entityFilter = "entity",
        projectEntityFilter = "project_entity",
        milestoneEntityFilter = "milestone_entity",
        discussionEntityFilter = "discussion_entity",
        teamEntityFilter = "team_entity",
        taskEntityFilter = "task_entity",
        subtaskEntityFilter = "subtask_entity",
        timeEntityFilter = "time_entity",
        commentEntityFilter = "comment_entity",
        sortByFilter = "sortBy",
        sortOrderFilter = "sortOrder";

    var typePerson = "person",
        typeCombobox = "combobox",
        typeFlag = "flag",
        typeGroup = "group",
        typeDaterange = "daterange",
        typeSorter = "sorter";

    var userIDgroup = "userid",
        creatoridgroup = "creatorid",
        taskuseridgroup = "taskuserid",
        manageridGroup = "managerid",
        projectGroup = "projectGroup",
        statusGroup = "status",
        createdGroup = "createdGroup",
        deadlineGroup = "deadlineGroup",
        periodGroup = "periodGroup",
        format1 = "/{0}",
        format2 = format1 + "/{1}",
        personHashmark = typePerson + format1,
        createdHashmark = createdFilter + format2,
        comboboxHashmark = typeCombobox + format1,
        deadlineHashmark = deadlineFilter + format2,
        groupHashmark = groupFilter + format1,
        projectHashmark = projectFilter + format1,
        milestineHashmark = milestoneFilter + format1,
        periodHashmark = periodFilter + format2;

    var sortOrderAscending = "ascending",
        sortOrderDescending = "descending",
        sorterComments = "comments",
        sorterCreateOn = "create_on",
        sorterTitle = "title",
        sorterDeadline = "deadline";

    var massNameFilters = [
        teamMemberFilter, meTeamMemberFilter, meResponsibleForMilestoneFilter,
        responsibleForMilestoneFilter, meProjectManagerFilter, projectManagerFilter, meAuthorFilter, authorFilter,
        userFilter, meTasksResponsibleFilter, tasksResponsibleFilter, meTasksCreatorFilter, tasksCreatorFilter,
        meTasksFilter, userTasksFilter, noresponsibleFilter, groupFilter, followedFilter, tagFilter, textFilter, projectFilter,
        myprojectsFilter, milestoneFilter, nomilestonesFilter, mymilestonesFilter, statusFilter, substatusFilter, openFilter, closedFilter, archivedFilter, pausedFilter, paymentStatusFilter, notChargeableFilter, notBilledFilter, billedFilter, overdueFilter, todayFilter, upcomingFilter,
        recentFilter, deadlineStartFilter, deadlineStopFilter, createdStartFilter, createdStopFilter, periodStartFilter, periodStopFilter, entityFilter, projectEntityFilter, milestoneEntityFilter, discussionEntityFilter, teamEntityFilter, taskEntityFilter, subtaskEntityFilter,
        timeEntityFilter, commentEntityFilter, sortByFilter, sortOrderFilter
    ];

    var self, obj, teamlab = Teamlab;

    var baseObj = ASC.Projects,
        resources = baseObj.Resources,
        projectsFilterResource = resources.ProjectsFilterResource,
        common = baseObj.Common,
        currentProjectId = jq.getURLParam('prjID'),
        currentUserId = teamlab.profile.id;

    var milestonesForFilter,
        tagsForFilter,
        teamForFilter;

    var $body = jq("body"),
        clickFilterEventName = "click.filter";

    var currentAdditionalId;

    var customStatuses;

    var init = function (newCurrentAdditionalId, objct, settings) {
        self = this;
        currentProjectId = jq.getURLParam('prjID');
        currentAdditionalId = newCurrentAdditionalId;

        clear();
        initialisation(objct);

        if (typeof filter === "undefined") {
            $body.on(clickFilterEventName, ".clearFilterButton", function () {
                filter.advansedFilter(null);
                return false;
            });
        }

        filter = jq('#ProjectsAdvansedFilter').advansedFilter(
                {
                    store: true,
                    anykey: true,
                    hintDefaultDisable: true,
                    colcount: 3,
                    anykeytimeout: 1000,
                    filters: settings.filters,
                    sorters: settings.sorters
                }
            )
            .unbind('setfilter')
            .unbind('resetfilter')
            .bind('setfilter', onSetFilter)
            .bind('resetfilter', onResetFilter);
    };

    function clear() {
        firstload = true;
        delete window.userSelector;

        if (typeof filter == "undefined") return;

        var visible = jq('#ProjectsAdvansedFilter').is(":visible");
        if (visible) {
            filter.attr("style", "visibility:visible");
        }

        filter.removeClass("is-init");

        jq('#ProjectsAdvansedFilter').advansedFilter("resetText");

        /*        filter.empty();
                filter.removeAttr("class");*/
    };

    function initialisation(objct) {
        basePath = objct.basePath;
        var res = /sortBy=(.+)\&sortOrder=(.+)/ig.exec(basePath);
        if (res && res.length === 3) {
            baseSortBy = res[1];
        }

        obj = objct;

    };

    var getUrlParam = function (name, str) {
        var regexS = "[#&]" + name + "=([^&]*)";
        var regex = new RegExp(regexS);
        var tmpUrl = "#";
        if (str) {
            tmpUrl += str;
        } else {
            tmpUrl += location.hash.substring(1);
        }
        var results = regex.exec(tmpUrl);
        if (results == null)
            return "";
        else
            return results[1];
    };

    var coincidesWithFilter = function (filter) {
        var hash = location.hash.substring(1);

        var sortOrder = getUrlParam(sortOrderFilter, hash);
        var sortBy = getUrlParam(sortByFilter, hash);

        if (sortBy === "" && sortOrder === "") {
            hash = basePath + "&" + hash;
        }

        for (var i = massNameFilters.length; i--;) {
            var filterParam = getUrlParam(massNameFilters[i], filter);
            var hashParam = getUrlParam(massNameFilters[i], hash);
            if (filterParam !== hashParam) {
                return false;
            }
        }
        return true;
    };

    function setFilterByUrl() {
        var hash = location.hash.substring(1);
        if (hash === "") {
            ASC.Projects.Common.setHash("");
            return;
        }
        var sortBy = getUrlParam(sortByFilter),
            sortOrder = getUrlParam(sortOrderFilter);

        if (!sortBy) {
            sortBy = baseSortBy;
        }

        for (var a = 0, b = currentSettings.filters.length; a < b; a++) {
            var currentFilter = currentSettings.filters[a];

            if (!currentFilter.visible) continue;

            var selectedFilter;
            if (currentFilter.id === "text") {
                selectedFilter = getUrlParam(currentFilter.id);
            } else {
                var currentFilterId;
                if (currentFilter.id.indexOf(currentAdditionalId) > -1) {
                    currentFilterId = currentFilter.id.substring(currentAdditionalId.length);

                    if (currentProjectId && currentFilterId.endsWith("_p")) {
                        currentFilterId = currentFilterId.substring(0, currentFilterId.length - 2);
                    }

                    if (currentFilter.type === typeDaterange) {
                        selectedFilter = getUrlParam(currentFilterId + "Start");
                    } else {
                        selectedFilter = getUrlParam(currentFilterId);
                    }
                }
            }

            var groupBy = getUrlParam(currentFilter.groupby);
            if (selectedFilter || groupBy) {
                switch (currentFilter.type) {
                    case typePerson:
                    case typeGroup:
                        currentFilter.params = { id: selectedFilter.toLowerCase() };
                        break;
                    case typeCombobox:
                    case "text":
                        var newParamValue =(selectedFilter || groupBy).toLowerCase();
                        if (currentFilter.options.some(function (f) { return f.value == newParamValue; })) {
                            currentFilter.params = { value: newParamValue };
                        }
                        break;
                    case typeFlag:
                        currentFilter.params = {};
                        break;
                    case typeDaterange:
                        currentFilter.params = { from: selectedFilter, to: getUrlParam(currentFilter.id.substring(2) + "Stop") };
                        break;

                }
            } else {
                currentFilter.reset = true;
                delete currentFilter.params;
            }
        }

        for (var i = 0, j = currentSettings.sorters.length; i < j; i++) {
            currentSettings.sorters[i].selected = currentSettings.sorters[i].id === sortBy;
            currentSettings.sorters[i].sortOrder = sortOrder;
        }

        filter.advansedFilter({ filters: currentSettings.filters, sorters: currentSettings.sorters });
    };

    var makeData = function ($container, type) {
        var data = {}, anchor = "", filters = $container.advansedFilter(), from, to, trueString = "true";
        if (currentProjectId) {
            data.projectId = currentProjectId;
        }
        var filtersLength = filters.length;
        self.baseFilter = filtersLength === 1 && filters[0].id === "sorter";

        for (var filterInd = 0; filterInd < filtersLength; filterInd++) {
            var filtersI = filters[filterInd];
            var params = filtersI.params;
            var options = filtersI.options;
            var id = params.id;
            var val = params.value;
            var filterid = filtersI.id;

            if (filterid !== "sorter" && filterid !== "text") {
                filterid = filterid.substring(2);
            }

            switch (filterid) {
                case meTeamMemberFilter:
                case teamMemberFilter:
                    data.participant = id;
                    anchor = changeParamValue(anchor, teamMemberFilter, data.participant);
                    break;
                case meProjectManagerFilter:
                case projectManagerFilter:
                    data.manager = id;
                    anchor = changeParamValue(anchor, projectManagerFilter, data.manager);
                    break;
                case meResponsibleForMilestoneFilter:
                case addProjectFilterId(meResponsibleForMilestoneFilter):
                case responsibleForMilestoneFilter:
                case addProjectFilterId(responsibleForMilestoneFilter):
                    data.milestoneResponsible = getIdOrValue(params);
                    anchor = changeParamValue(anchor, responsibleForMilestoneFilter, data.milestoneResponsible);
                    break;
                case meTasksResponsibleFilter:
                case addProjectFilterId(meTasksResponsibleFilter):
                case tasksResponsibleFilter:
                case addProjectFilterId(tasksResponsibleFilter):
                    data.participant = getIdOrValue(params);
                    anchor = changeParamValue(anchor, tasksResponsibleFilter, data.participant);
                    break;
                case meTasksCreatorFilter:
                case tasksCreatorFilter:
                    data.creator = id;
                    anchor = changeParamValue(anchor, tasksCreatorFilter, data.creator);
                    break;
                case meAuthorFilter:
                case authorFilter:
                case addProjectFilterId(authorFilter):
                    data.participant = getIdOrValue(params);
                    anchor = changeParamValue(anchor, authorFilter, data.participant);
                    break;
                case userFilter:
                    data.user = id;
                    anchor = changeParamValue(anchor, userFilter, data.user);
                    break;
                case groupFilter:
                    data.departament = id;
                    anchor = changeParamValue(anchor, groupFilter, data.departament);
                    break;
                case nomilestonesFilter:
                    data.nomilestone = trueString;
                    anchor = changeParamValue(anchor, nomilestonesFilter, trueString);
                    break;
                case mymilestonesFilter:
                    data.mymilestones = trueString;
                    anchor = changeParamValue(anchor, mymilestonesFilter, trueString);
                    break;
                case milestoneFilter:
                    if (val === null) continue;
                    data.milestone = val;
                    anchor = changeParamValue(anchor, milestoneFilter, data.milestone);
                    break;
                case noresponsibleFilter:
                    data.participant = "00000000-0000-0000-0000-000000000000";
                    anchor = changeParamValue(anchor, noresponsibleFilter, trueString);
                    break;
                case myprojectsFilter:
                    data.myprojects = trueString;
                    anchor = changeParamValue(anchor, myprojectsFilter, trueString);
                    break;
                case projectFilter:
                    if (val === null) continue;
                    data.projectId = val;
                    anchor = changeParamValue(anchor, projectFilter, data.projectId);
                    break;
                case meTasksFilter:
                case addProjectFilterId(meTasksFilter):
                case userTasksFilter:
                case addProjectFilterId(userTasksFilter):
                    data.taskResponsible = getIdOrValue(params);
                    anchor = changeParamValue(anchor, userTasksFilter, data.taskResponsible);
                    break;
                case followedFilter:
                    data.follow = trueString;
                    anchor = jq.changeParamValue(anchor, followedFilter, trueString);
                    break;
                case tagFilter:
                    if (val === null) continue;
                    data.tag = val;
                    anchor = changeParamValue(anchor, tagFilter, data.tag);
                    break;
                case notagFilter:
                    data.tag = -1;
                    anchor = changeParamValue(anchor, notagFilter, trueString);
                    break;
                case openFilter:
                case pausedFilter:
                case archivedFilter:
                case closedFilter:
                case statusFilter:
                case notChargeableFilter:
                case notBilledFilter:
                case billedFilter:
                    if (val === null) continue;
                    var option = options.find(function (item) { return item.value == val });
                    if (option && typeof (option.sub) === "boolean" && option.sub) {
                        data.substatus = option.value;
                        anchor = changeParamValue(anchor, substatusFilter, data.substatus);
                    } else {
                        data.status = val;
                        anchor = changeParamValue(anchor, statusFilter, data.status);
                    }
                    break;
                case overdueFilter:
                    data.status = "open";
                    data.deadlineStop = teamlab.serializeTimestamp(new Date());
                    anchor = changeParamValue(anchor, overdueFilter, trueString);
                    break;
                case todayFilter:
                case upcomingFilter:
                case deadlineFilter:
                    from = params.from;
                    to = params.to;
                    data.deadlineStart = teamlab.serializeTimestamp(new Date(from));
                    data.deadlineStop = teamlab.serializeTimestamp(new Date(to));
                    anchor = changeParamValue(anchor, deadlineStartFilter, from);
                    anchor = changeParamValue(anchor, deadlineStopFilter, to);
                    break;
                case today2Filter:
                case recentFilter:
                case createdFilter:
                case previousweek2Filter:
                case previousmonth2Filter:
                case period2Filter:
                    from = params.from;
                    to = params.to;
                    data.createdStart = teamlab.serializeTimestamp(new Date(from));
                    data.createdStop = teamlab.serializeTimestamp(new Date(to));
                    anchor = changeParamValue(anchor, createdStartFilter, from);
                    anchor = changeParamValue(anchor, createdStopFilter, to);
                    break;
                case today3Filter:
                case yesterdayFilter:
                case currentweekFilter:
                case previousweekFilter:
                case currentmonthFilter:
                case previousmonthFilter:
                case currentyearFilter:
                case previousyearFilter:
                case periodFilter:
                    from = params.from;
                    to = params.to;
                    data.periodStart = teamlab.serializeTimestamp(new Date(from));
                    data.periodStop = teamlab.serializeTimestamp(new Date(to));
                    anchor = changeParamValue(anchor, periodStartFilter, from);
                    anchor = changeParamValue(anchor, periodStopFilter, to);
                    break;
                case textFilter:
                    data.FilterValue = val;
                    anchor = changeParamValue(anchor, textFilter, data.FilterValue);
                    break;
                case projectEntityFilter:
                case milestoneEntityFilter:
                case discussionEntityFilter:
                case teamEntityFilter:
                case taskEntityFilter:
                case subtaskEntityFilter:
                case timeEntityFilter:
                case commentEntityFilter:
                case entityFilter:
                    if (val === null) continue;
                    data.entity = val;
                    anchor = changeParamValue(anchor, entityFilter, data.entity);
                    break;
                case "sorter":
                    data.sortBy = id;
                    data.sortOrder = params.sortOrder;
                    anchor = changeParamValue(anchor, sortByFilter, data.sortBy);
                    anchor = changeParamValue(anchor, sortOrderFilter, data.sortOrder);
                    break;
            }
        }
        return type === "anchor" ? anchor : data;
    };

    function getIdOrValue(params) {
        return params.id || params.value;
    }

    function changeParamValue(anchor, name, value) {
        return jq.changeParamValue(anchor, name, value);
    }

    var getMilestonesForFilter = function () {
        var milestones = ASC.Projects.Master.Milestones;
        if (!milestones) return [];

        if (currentProjectId) {
            milestones = milestones.filter(function(item) {
                return item.status === 0 && item.projectId == currentProjectId;
            });
        }

        return milestones.map(function(item) {
            return {
                'value': item.id,
                'title': jq.format("[{0}] {1}", item.displayDateDeadline, item.title)
            };
        });
    };

    var getTagsForFilter = function () {
        return  ASC.Projects.Master.Tags;
    };

    var getTeamForFilter = function () {
        if (typeof ASC.Projects.Master.Team === "undefined") return [];

        return  ASC.Projects.Master.Team.map(function (item) {
            return { 'value': item.id, 'title': item.displayName };
        });
    };

    var resize = function () {
        if (typeof (filter) != "undefined" && typeof (filter.filter) != "undefined") {
            filter.advansedFilter("resize");
        }
    };

    function onSetFilter(evt, $container) {
        var path = makeData($container, 'anchor');
        var hash = location.hash.substring(1);
        if (firstload && hash.length) {
            if (!coincidesWithFilter(path)) {
                firstload = false;
                setFilterByUrl();
                return;
            }
        }
        if (firstload) {
            firstload = false;
        } else {
            ASC.Projects.PageNavigator.reset();
        }

        obj.currentFilter = makeData($container, 'data');
        obj.getData();
        if (path !== hash) {
            ASC.Projects.Common.setHash(path);
        }
    };

    function onResetFilter(evt, $container) {
        ASC.Projects.PageNavigator.reset();
        var path = makeData($container, 'anchor');
        ASC.Projects.Common.setHash(path);
        obj.currentFilter = makeData($container, 'data');
        obj.getData();
    };

    var hide = function () {
        if (typeof (filter) != "undefined") {
            filter.hide();
        }
    };

    var show = function () {
        if (typeof (filter) != "undefined") {
            var visible = jq('#ProjectsAdvansedFilter').is(":visible");
            if (!visible) {
                filter.show();
                resize();
            }
        }
    };

    var getFilterForDiscussion = function (visible, settings) {
        var now = new Date();
        var today = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0);
        var lastWeek = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0);
        lastWeek.setDate(lastWeek.getDate() - 7);

        var filters = [];
        if (settings && settings.filters) {
            filters = settings.filters;
        }
        //Author

        var startIndex = filters.length;

        pushFilterItem(typeof common.userInProjectTeam(currentUserId) !== "undefined" && currentProjectId != null && visible,
            filters,
            typeCombobox,
            addProjectFilterId(meAuthorFilter),
            projectsFilterResource.MyDiscussions,
            projectsFilterResource.Author + ":",
            projectsFilterResource.Author,
            personHashmark,
            userIDgroup,
            teamForFilter,
            { value: currentUserId });

        pushFilterItem((currentProjectId != null) && visible,
            filters,
            typeCombobox,
            addProjectFilterId(authorFilter),
            projectsFilterResource.OtherParticipant,
            projectsFilterResource.Author + ":",
            projectsFilterResource.ByParticipant,
            personHashmark,
            userIDgroup,
            teamForFilter,
            { value: currentUserId });

        pushFilterItem((currentProjectId == null) && visible,
            filters,
            typePerson,
            meAuthorFilter,
            projectsFilterResource.MyDiscussions,
            projectsFilterResource.Author + ":",
            projectsFilterResource.Author,
            personHashmark,
            userIDgroup,
            null,
            { id: currentUserId });

        pushFilterItem((currentProjectId == null) && visible,
            filters,
            typePerson,
            authorFilter,
            projectsFilterResource.OtherUsers,
            projectsFilterResource.Author + ":",
            projectsFilterResource.Author,
            personHashmark,
            userIDgroup);

        //Projects

        pushFilterItemsProjects(visible, filters);

        pushFilterItemsWithFixedOptions(visible, filters,
        [
            { value: openFilter, title: projectsFilterResource.StatusOpenDiscussion },
            { value: archivedFilter, title: projectsFilterResource.StatusArchivedDiscussion }
        ]);

        //Creation date
        pushFilterItem(visible, filters,
            typeDaterange,
            today2Filter,
            projectsFilterResource.Today,
            " ",
            projectsFilterResource.ByCreateDate,
            createdHashmark,
            createdGroup,
            null,
            { from: today.getTime(), to: today.getTime() });

        pushFilterItem(visible, filters,
            typeDaterange,
            recentFilter,
            projectsFilterResource.Recent,
            " ",
            projectsFilterResource.ByCreateDate,
            createdHashmark,
            createdGroup,
            null,
            { from: lastWeek.getTime(), to: today.getTime() });

        pushFilterItem(visible, filters,
            typeDaterange,
            createdFilter,
            projectsFilterResource.CustomPeriod,
            " ",
            projectsFilterResource.ByCreateDate,
            createdHashmark,
            createdGroup);

        //Followed
        pushFilterItem(visible, filters,
            typeFlag,
            followedFilter,
            projectsFilterResource.FollowDiscussions,
            "",
            projectsFilterResource.Other,
            followedFilter);

        pushHiddenFilterItems(filters);

        addAdditionalFilterId(filters, startIndex, discussionsAdditionalFilterId);

        var sorters = [];
        if (settings && settings.sorters) {
            sorters = settings.sorters;
        }

        pushSorter(visible, sorters, sorterComments, projectsFilterResource.ByComments, sortOrderDescending, true);
        pushSorter(visible, sorters, sorterCreateOn, projectsFilterResource.ByCreateDate, sortOrderDescending);
        pushSorter(visible, sorters, sorterTitle, projectsFilterResource.ByTitle, sortOrderDescending);

        return {
            filters: filters,
            sorters: sorters
        };
    };

    var getFilterForMilestones = function (visible, settings) {
        var now = new Date();
        var today = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0);
        var inWeek = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0);
        inWeek.setDate(inWeek.getDate() + 7);

        var filters = [];
        if (settings && settings.filters) {
            filters = settings.filters;
        }

        var startIndex = filters.length;

        // Responsible

        pushFilterItemPersonMe(typeof common.userInProjectTeam(currentUserId) !== "undefined" && currentProjectId != null && visible,
            filters,
            addProjectFilterId(meResponsibleForMilestoneFilter),
            projectsFilterResource.ByResponsible,
            userIDgroup);

        pushFilterItem(typeof common.userInProjectTeam(currentUserId) !== "undefined" && currentProjectId != null && visible,
            filters,
            typeCombobox,
            addProjectFilterId(meTasksFilter),
            projectsFilterResource.MyTasks,
            projectsFilterResource.Tasks + ":",
            projectsFilterResource.Tasks,
            personHashmark,
            taskuseridgroup,
            teamForFilter,
            { value: currentUserId });

        pushFilterItem((currentProjectId != null) && visible,
            filters,
            typeCombobox,
            addProjectFilterId(responsibleForMilestoneFilter),
            projectsFilterResource.OtherParticipant,
            projectsFilterResource.ByResponsible + ":",
            projectsFilterResource.ByResponsible,
            personHashmark,
            userIDgroup,
            teamForFilter,
            null,
            projectsFilterResource.Select);

        pushFilterItem((currentProjectId != null) && visible,
            filters,
            typeCombobox,
            addProjectFilterId(userTasksFilter),
            projectsFilterResource.OtherParticipant,
            projectsFilterResource.ByResponsible + ":",
            projectsFilterResource.Tasks,
            personHashmark,
            userIDgroup,
            teamForFilter,
            null,
            projectsFilterResource.Select);

        pushFilterItemPersonMe(currentProjectId == null && visible,
            filters,
            meResponsibleForMilestoneFilter,
            projectsFilterResource.ByResponsible,
            userIDgroup);

        pushFilterItem(currentProjectId == null && visible,
            filters,
            typePerson,
            responsibleForMilestoneFilter,
            projectsFilterResource.OtherUsers,
            projectsFilterResource.ByResponsible + ":",
            projectsFilterResource.ByResponsible,
            personHashmark,
            userIDgroup);

        pushFilterItem(currentProjectId == null && visible,
            filters,
            typePerson,
            meTasksFilter,
            projectsFilterResource.MyTasks,
            projectsFilterResource.Tasks + ":",
            projectsFilterResource.Tasks,
            personHashmark,
            taskuseridgroup,
            null,
            { id: currentUserId });

            //Tasks

        pushFilterItem(currentProjectId == null && visible,
            filters,
            typePerson,
            userTasksFilter,
            projectsFilterResource.OtherUsers,
            projectsFilterResource.Tasks + ":",
            projectsFilterResource.Tasks,
            personHashmark,
            taskuseridgroup);


        // Status
        pushFilterItemsWithFixedOptions(visible, filters,
        [
            { value: openFilter, title: projectsFilterResource.StatusOpenMilestone },
            { value: closedFilter, title: projectsFilterResource.StatusClosedMilestone }
        ]);

        //Due date
        pushFilterItem(visible, filters,
            typeFlag,
            overdueFilter,
            projectsFilterResource.Overdue,
            "",
            projectsFilterResource.DueDate,
            overdueFilter,
            deadlineGroup);

        pushFilterItem(visible, filters,
            typeDaterange,
            todayFilter,
            projectsFilterResource.Today,
            " ",
            projectsFilterResource.DueDate,
            deadlineHashmark,
            deadlineGroup,
            null,
            { from: today.getTime(), to: today.getTime() });

        pushFilterItem(visible, filters,
            typeDaterange,
            upcomingFilter,
            projectsFilterResource.UpcomingMilestones,
            " ",
            projectsFilterResource.DueDate,
            deadlineHashmark,
            deadlineFilter,
            null,
            { from: today.getTime(), to: inWeek.getTime() });

        pushFilterItem(visible, filters,
            typeDaterange,
            deadlineFilter,
            projectsFilterResource.CustomPeriod,
            " ",
            projectsFilterResource.DueDate,
            deadlineHashmark,
            deadlineGroup);

        pushFilterItemsProjects(visible, filters);

        pushHiddenFilterItems(filters);

        addAdditionalFilterId(filters, startIndex, milestonesAdditionalFilterId);

        var sorters = [];

        if (settings && settings.sorters) {
            sorters = settings.sorters;
        }

        pushSorter(visible, sorters, sorterDeadline, projectsFilterResource.ByDeadline, sortOrderAscending, true);
        pushSorter(visible, sorters, sorterCreateOn, projectsFilterResource.ByCreateDate, sortOrderDescending);
        pushSorter(visible, sorters, sorterTitle, projectsFilterResource.ByTitle, sortOrderAscending);

        return {
            filters: filters,
            sorters: sorters
        };
    };

    var getFilterForProjects = function (visible, settings) {
        var filters = [];
        if (settings && settings.filters) {
            filters = settings.filters;
        }

        var startIndex = filters.length;

        // Project manager
        pushFilterItemPersonMe(visible, filters, meProjectManagerFilter, projectsFilterResource.ProjectMenager, manageridGroup);

        pushFilterItem(visible, filters,
            typePerson,
            projectManagerFilter,
            projectsFilterResource.OtherUsers,
            projectsFilterResource.ProjectMenager + ":",
            projectsFilterResource.ProjectMenager,
            personHashmark,
            manageridGroup);

        // Team member
        pushFilterItemPersonMe(visible, filters, meTeamMemberFilter, projectsFilterResource.TeamMember, userIDgroup);

        pushFilterItem(visible, filters,
            typePerson,
            teamMemberFilter,
            projectsFilterResource.OtherUsers,
            projectsFilterResource.TeamMember + ":",
            projectsFilterResource.TeamMember,
            personHashmark,
            userIDgroup);

        pushFilterItem(visible, filters,
            typeGroup,
            typeGroup,
            projectsFilterResource.Groups,
            projectsFilterResource.Groups + ":",
            projectsFilterResource.TeamMember,
            groupHashmark,
            userIDgroup);

        //Status
        pushFilterItemsWithFixedOptions(visible, filters,
        [
            { value: openFilter, title: projectsFilterResource.StatusOpenProject },
            { value: pausedFilter, title: projectsFilterResource.StatusSuspend },
            { value: closedFilter, title: projectsFilterResource.StatusClosedProject }
        ]);

        pushFilterItem(visible, filters,
            typeFlag,
            followedFilter,
            projectsFilterResource.FollowProjects,
            "",
            projectsFilterResource.Other,
            followedFilter);

        if (tagsForFilter) {
            pushFilterItem(visible, filters,
                typeCombobox,
                tagFilter,
                projectsFilterResource.ByTag,
                projectsFilterResource.Tag + ":",
                projectsFilterResource.Other,
                comboboxHashmark,
                null,
                tagsForFilter,
                null,
                projectsFilterResource.Select);

            pushFilterItem(visible, filters,
                typeFlag,
                notagFilter,
                projectsFilterResource.WithoutTag,
                "",
                projectsFilterResource.Other,
                notagFilter);
        }

        pushHiddenFilterItems(filters, 2);

        addAdditionalFilterId(filters, startIndex, projectsAdditionalFilterId);

        var sorters = [];

        if (settings && settings.sorters) {
            sorters = settings.sorters;
        }

        pushSorter(visible, sorters, sorterTitle, projectsFilterResource.ByTitle, sortOrderAscending, true);
        pushSorter(visible, sorters, sorterCreateOn, projectsFilterResource.ByCreateDate, sortOrderDescending);

        return {
            filters: filters,
            sorters: sorters
        };
    };

    var getFilterForTasks = function (visible, settings) {
        var now = new Date(),
            today = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0),
            inWeek = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0);
        inWeek.setDate(inWeek.getDate() + 7);

        var filters = [];
        if (settings && settings.filters) {
            filters = settings.filters;
        }

        var startIndex = filters.length;

        // Responsible

        pushFilterItemPersonMe(typeof common.userInProjectTeam(currentUserId) !== "undefined" && currentProjectId != null && visible,
            filters,
            addProjectFilterId(meTasksResponsibleFilter),
            projectsFilterResource.ByResponsible,
            userIDgroup);

        pushFilterItem(currentProjectId != null && visible,
            filters,
            typePerson,
            addProjectFilterId(tasksResponsibleFilter),
            projectsFilterResource.OtherParticipant,
            projectsFilterResource.ByResponsible + ":",
            projectsFilterResource.ByResponsible,
            personHashmark,
            userIDgroup,
            teamForFilter,
            null,
            projectsFilterResource.Select);

        pushFilterItemPersonMe(currentProjectId == null && visible, filters, meTasksResponsibleFilter, projectsFilterResource.ByResponsible, userIDgroup);

        pushFilterItem(currentProjectId == null && visible,
            filters,
            typePerson,
            tasksResponsibleFilter,
            projectsFilterResource.OtherUsers,
            projectsFilterResource.ByResponsible + ":",
            projectsFilterResource.ByResponsible,
            personHashmark,
            userIDgroup);

        pushFilterItemGroup(visible, filters, projectsFilterResource.ByResponsible, userIDgroup);

        pushFilterItem(visible, filters,
            typeFlag,
            noresponsibleFilter,
            projectsFilterResource.NoResponsible,
            "",
            projectsFilterResource.ByResponsible,
            "noresponsible",
            userIDgroup);

        // Creator
        pushFilterItemPersonMe(visible, filters, meTasksCreatorFilter, projectsFilterResource.ByCreator, creatoridgroup);

        pushFilterItem(visible, filters,
            typePerson,
            tasksCreatorFilter,
            projectsFilterResource.OtherUsers,
            projectsFilterResource.ByCreator + ":",
            projectsFilterResource.ByCreator,
            personHashmark,
            creatoridgroup);

        pushFilterItemsProjects(visible, filters);

        //Milestones
        pushFilterItem(visible, filters,
            typeFlag,
            mymilestonesFilter,
            projectsFilterResource.MyMilestones,
            null,
            projectsFilterResource.ByMilestone,
            "mymilestones",
            milestoneFilter);

        pushFilterItem(visible, filters,
            typeFlag,
            nomilestonesFilter,
            ASC.Projects.Resources.ProjectsJSResource.NoMilestone,
            null,
            projectsFilterResource.ByMilestone,
            "nomilestone",
            milestoneFilter);

        pushFilterItem(visible, filters,
            typeCombobox,
            milestoneFilter,
            projectsFilterResource.OtherMilestones,
            projectsFilterResource.ByMilestone + ":",
            projectsFilterResource.ByMilestone,
            "milestone/{0}",
            milestoneFilter,
            milestonesForFilter,
            null,
            projectsFilterResource.Select);
        // Status
        function getSub(statusType) {
            if (!customStatuses) return;
            
            return customStatuses.filter(function(item) {
                return item.statusType === statusType;
            }).map(function(item) {
                return {
                    value: item.id,
                    title: item.title,
                    sub: true,
                    statusType: statusType
                };
            });
        }

        var openSub = getSub(1);
        var closedSub = getSub(2);

        var filterStatuses = [];

        if (openSub) {
            if (openSub.length === 1) {
                filterStatuses.push(
                    {
                        value: openFilter,
                        title: openSub[0].title
                    }
                );
            } else if (openSub.length > 1) {
                filterStatuses.push(
                    {
                        value: openFilter,
                        title: projectsFilterResource.StatusAllOpenTask,
                        sub: [{ value: openFilter, title: projectsFilterResource.StatusAllOpenTask }].concat(openSub)
                    }
                );
            }
        } else {
            filterStatuses.push(
                {
                    value: openFilter,
                    title: projectsFilterResource.StatusOpenTask
                }
            );
        }

        if (closedSub) {
            if (closedSub.length === 1) {
                filterStatuses.push(
                    {
                        value: closedFilter,
                        title: closedSub[0].title
                    }
                );
            } else if (closedSub.length > 1) {
                filterStatuses.push(
                    {
                        value: closedFilter,
                        title: projectsFilterResource.StatusAllClosedTask,
                        sub: [{ value: closedFilter, title: projectsFilterResource.StatusAllClosedTask }].concat(closedSub)
                    }
                );
            }
        } else {
            filterStatuses.push(
                {
                    value: closedFilter,
                    title: projectsFilterResource.StatusClosedTask
                }
            );
        }

        pushFilterItemsWithFixedOptions(visible, filters, filterStatuses);

        //Due date
        pushFilterItem(visible, filters,
            typeFlag,
            overdueFilter,
            projectsFilterResource.Overdue,
            null,
            projectsFilterResource.DueDate,
            overdueFilter,
            deadlineGroup);

        pushFilterItem(visible, filters,
            typeDaterange,
            todayFilter,
            projectsFilterResource.Today,
            " ",
            projectsFilterResource.DueDate,
            deadlineHashmark,
            deadlineGroup,
            null,
            { from: today.getTime(), to: today.getTime() });

        pushFilterItem(visible, filters,
            typeDaterange,
            upcomingFilter,
            projectsFilterResource.UpcomingMilestones,
            " ",
            projectsFilterResource.DueDate,
            deadlineHashmark,
            deadlineGroup,
            null,
            { from: today.getTime(), to: inWeek.getTime() });

        pushFilterItem(visible, filters,
            typeDaterange,
            deadlineFilter,
            projectsFilterResource.CustomPeriod,
            " ",
            projectsFilterResource.DueDate,
            deadlineHashmark,
            deadlineGroup);

        addAdditionalFilterId(filters, startIndex, tasksAdditionalFilterId);

        var sorters = [];

        if (settings && settings.sorters) {
            sorters = settings.sorters;
        }

        pushSorter(visible, sorters, sorterDeadline, projectsFilterResource.ByDeadline, sortOrderAscending, true);
        pushSorter(visible, sorters, "priority", projectsFilterResource.ByPriority, sortOrderDescending);
        pushSorter(visible, sorters, sorterCreateOn, projectsFilterResource.ByCreateDate, sortOrderDescending);
        pushSorter(visible, sorters, "start_date", projectsFilterResource.ByStartDate, sortOrderDescending);
        pushSorter(visible, sorters, sorterTitle, projectsFilterResource.ByTitle, sortOrderAscending);
        pushSorter(visible, sorters, "sort_order", projectsFilterResource.ByOrder, sortOrderAscending);


        return {
            filters: filters,
            sorters: sorters
        };
    };

    var getFilterForTimeTracking = function (visible, settings) {
        var now = new Date();

        var today = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0);

        var startWeek = new Date(today);
        startWeek.setDate(today.getDate() - today.getDay() + 1);

        var endWeek = new Date(today);
        endWeek.setDate(today.getDate() - today.getDay() + 7);

        var startPreviousWeek = new Date(startWeek);
        startPreviousWeek.setDate(startWeek.getDate() - 7);

        var endPreviousWeek = new Date(startWeek);
        endPreviousWeek.setDate(startWeek.getDate() - 1);

        var startPreviousMonth = new Date(today);
        startPreviousMonth.setMonth(today.getMonth() - 1);
        startPreviousMonth.setDate(1);

        var endPreviousMonth = new Date(startPreviousMonth);
        endPreviousMonth.setMonth(startPreviousMonth.getMonth() + 1);
        endPreviousMonth.setDate(endPreviousMonth.getDate() - 1);


        startPreviousWeek = startPreviousWeek.getTime();
        endPreviousWeek = endPreviousWeek.getTime();
        startPreviousMonth = startPreviousMonth.getTime();
        endPreviousMonth = endPreviousMonth.getTime();

        var filters = [];
        if (settings && settings.filters) {
            filters = settings.filters;
        }

        var startIndex = filters.length;

        pushFilterItemsProjects(visible, filters);

        //Milestones
        pushFilterItem(visible, filters,
            typeFlag,
            mymilestonesFilter,
            projectsFilterResource.MyMilestones,
            "",
            projectsFilterResource.ByMilestone,
            mymilestonesFilter,
            milestoneFilter);

        pushFilterItem(visible, filters,
            typeCombobox,
            milestoneFilter,
            projectsFilterResource.OtherMilestones,
            projectsFilterResource.ByMilestone + ":",
            projectsFilterResource.ByMilestone,
            milestineHashmark,
            milestoneFilter,
            milestonesForFilter,
            null,
            projectsFilterResource.Select);
        // Responsible

        pushFilterItemPersonMe(typeof common.userInProjectTeam(currentUserId) !== "undefined" && currentProjectId != null && visible,
            filters,
            addProjectFilterId(meTasksResponsibleFilter),
            projectsFilterResource.ByResponsible,
            userIDgroup);

        pushFilterItem(currentProjectId != null && visible,
            filters,
            typePerson,
            addProjectFilterId(tasksResponsibleFilter),
            projectsFilterResource.OtherParticipant,
            projectsFilterResource.ByResponsible + ":",
            projectsFilterResource.ByResponsible,
            personHashmark,
            userIDgroup,
            teamForFilter,
            null,
            projectsFilterResource.Select);

        pushFilterItemPersonMe(currentProjectId == null && visible, filters, meTasksResponsibleFilter, projectsFilterResource.ByResponsible, userIDgroup);

        pushFilterItem(currentProjectId == null && visible,
            filters,
            typePerson,
            tasksResponsibleFilter,
            projectsFilterResource.OtherUsers,
            projectsFilterResource.ByResponsible + ":",
            projectsFilterResource.ByResponsible,
            personHashmark,
            userIDgroup);

        pushFilterItemGroup(visible, filters, projectsFilterResource.ByResponsible, userIDgroup);

        //Payment status
        pushFilterItemsWithFixedOptions(visible, filters,
        [
            { value: notChargeableFilter, title: projectsFilterResource.PaymentStatusNotChargeable },
            { value: notBilledFilter, title: projectsFilterResource.PaymentStatusNotBilled },
            { value: billedFilter, title: projectsFilterResource.PaymentStatusBilled }
        ]);

        //Create date
        pushFilterItem(visible, filters,
            typeDaterange,
            previousweek2Filter,
            projectsFilterResource.PreviousWeek,
            " ",
            projectsFilterResource.TimePeriod,
            periodHashmark,
            periodGroup,
            null,
            { from: startPreviousWeek, to: endPreviousWeek });

        pushFilterItem(visible, filters,
            typeDaterange,
            previousmonth2Filter,
            projectsFilterResource.PreviousMonth,
            " ",
            projectsFilterResource.TimePeriod,
            periodHashmark,
            periodGroup,
            null,
            { from: startPreviousMonth, to: endPreviousMonth });

        pushFilterItem(visible, filters,
            typeDaterange,
            period2Filter,
            projectsFilterResource.CustomPeriod,
            " ",
            projectsFilterResource.TimePeriod,
            periodHashmark,
            periodGroup);

        pushHiddenFilterItems(filters);

        addAdditionalFilterId(filters, startIndex, timeTrackingAdditionalFilterId);

        var sorters = [];

        if (settings && settings.sorters) {
            sorters = settings.sorters;
        }

        pushSorter(visible, sorters, "date", projectsFilterResource.ByDate, sortOrderDescending, true);
        pushSorter(visible, sorters, "hours", projectsFilterResource.ByHours, sortOrderAscending);
        pushSorter(visible, sorters, "note", projectsFilterResource.ByNote, sortOrderAscending);
        pushSorter(visible, sorters, sorterCreateOn, projectsFilterResource.ByCreateDate, sortOrderDescending);

        return {
            filters: filters,
            sorters: sorters
        };
    };

    function createAdvansedFilter(projects, milestones, tasks, discussions, timetracking) {
        currentProjectId = jq.getURLParam('prjID');
        milestonesForFilter = getMilestonesForFilter();
        tagsForFilter = getTagsForFilter();
        teamForFilter = getTeamForFilter();


        var settings = getFilterForTasks(tasks);
        getFilterForMilestones(milestones, settings);
        getFilterForTimeTracking(timetracking, settings);
        getFilterForDiscussion(discussions, settings);
        getFilterForProjects(projects, settings);
        currentSettings = settings;
        return settings;
    }

    var createAdvansedFilterForDiscussion = function (disc) {
        init.call(this, discussionsAdditionalFilterId, disc, createAdvansedFilter(false, false, false, true, false));
    };

    var createAdvansedFilterForMilestones = function (mil) {
        init.call(this, milestonesAdditionalFilterId, mil, createAdvansedFilter(false, true, false, false, false));
    };

    var createAdvansedFilterForProjects = function (prj) {
        init.call(this, projectsAdditionalFilterId, prj, createAdvansedFilter(true, false, false, false, false));
    };

    var createAdvansedFilterForTasks = function (task, cs) {
        customStatuses = cs;
        init.call(this, tasksAdditionalFilterId, task, createAdvansedFilter(false, false, true, false, false));
    };

    var createAdvansedFilterForTimeTracking = function (timeTracking) {
        init.call(this, timeTrackingAdditionalFilterId, timeTracking, createAdvansedFilter(false, false, false, false, true));
    };


    function pushFilterItem(visible, filters, type, id, title, filtertitle, group, hashmark, groupBy, options, bydefault, defaulttitle) {
        var item = {
            type: type,
            id: id,
            title: title,
            filtertitle: filtertitle,
            group: group,
            hashmask: hashmark,
            groupby: groupBy,
            visible: visible,
            enable: visible
        };

        if (typeof options != "undefined" && options != null) {
            item.options = options;
        }

        if (typeof bydefault != "undefined" && bydefault != null) {
            item.bydefault = bydefault;
        }

        if (typeof defaulttitle != "undefined" && defaulttitle != null) {
            item.defaulttitle = defaulttitle;
        }

        filters.push(item);
    }

    function pushFilterItemPersonMe(visible, filters, id, group, groupBy) {
        pushFilterItem(visible, filters,
            currentProjectId ? typeCombobox : typePerson,
            id,
            projectsFilterResource.Me,
            group + ":",
            group,
            personHashmark,
            groupBy,
            currentProjectId ? teamForFilter : null,
            { id: currentUserId });
    }

    function pushFilterItemGroup(visible, filters, group, groupBy) {
        pushFilterItem(visible, filters,
            typeGroup,
            groupFilter,
            projectsFilterResource.Groups,
            projectsFilterResource.Group + ":",
            group,
            groupHashmark,
            groupBy);
    }

    function pushFilterItemsWithFixedOptions(visible, filters, options) {
        var anysub = options.some(function (o) { return typeof o.sub !== "undefined"; });
        for (var i = 0, j = options.length; i < j; i++) {
            var copyOptions = options.map(function (item) { return jq.extend({}, item); });
            copyOptions[i].def = true;

            if (copyOptions[i].sub) {
                pushFilterItem(visible,
                    filters,
                    typeCombobox,
                    copyOptions[i].value,
                    copyOptions[i].title,
                    projectsFilterResource.ByStatus + ":",
                    projectsFilterResource.ByStatus,
                    comboboxHashmark,
                    statusGroup,
                    copyOptions[i].sub);
            } else {
                pushFilterItem(visible,
                    filters,
                    typeCombobox,
                    copyOptions[i].value,
                    copyOptions[i].title,
                    projectsFilterResource.ByStatus + ":",
                    projectsFilterResource.ByStatus,
                    comboboxHashmark,
                    statusGroup,
                    anysub ? [copyOptions[i]] : copyOptions);
            }
        }
    }

    function pushFilterItemsProjects(visible, filters) {
        pushFilterItem((currentProjectId == null) && visible,
            filters,
            typeFlag,
            myprojectsFilter,
            projectsFilterResource.MyProjects,
            '',
            projectsFilterResource.ByProject,
            myprojectsFilter,
            projectGroup);

        pushFilterItem((currentProjectId == null) && visible,
            filters,
            typeCombobox,
            projectFilter,
            projectsFilterResource.OtherProjects,
            projectsFilterResource.ByProject + ":",
            projectsFilterResource.ByProject,
            projectHashmark,
            projectGroup,
            common.getProjectsForFilter(),
            null,
            projectsFilterResource.Select);

        pushFilterItem((currentProjectId == null) && visible,
            filters,
            typeCombobox,
            tagFilter,
            projectsFilterResource.ByTag,
            projectsFilterResource.ByTag + ":",
            projectsFilterResource.ByProject,
            null,
            projectGroup,
            tagsForFilter,
            null,
            projectsFilterResource.Select);

        pushFilterItem((currentProjectId == null) && visible,
            filters,
            typeFlag,
            notagFilter,
            projectsFilterResource.WithoutTag,
            "",
            projectsFilterResource.ByProject,
            notagFilter,
            projectGroup);
    }

    function pushHiddenFilterItems(filters, count) {
        var hiddenStr = "hidden";
        count = count || 1;

        for (var i = 0; i < count; i++) {
            pushFilterItem(false, filters,
                typeFlag,
                hiddenStr + i,
                hiddenStr,
                "",
                hiddenStr + i);
        }
    }

    function pushSorter(visible, sorter, id, title, sortOrder, def) {
        var sorterItem, i, j, finded = false;
        for (i = 0, j = sorter.length; i < j; i++) {
            sorterItem = sorter[i];
            if (sorterItem.id === id) {
                if (visible && !sorterItem.visible) {
                    sorterItem.visible = true;
                }
                sorterItem.def = def;
                finded = true;
                break;
            };
        }
        if (def) {
            for (i = 0; i < j; i++) {
                sorterItem = sorter[i];
                if (sorterItem.id === id) continue;
                if (sorterItem.def && !sorterItem.visible) {
                    sorterItem.def = false;
                }
            }
        }
        if (!finded) {
            sorter.push({ id: id, title: title, sortOrder: sortOrder, def: def, visible: visible });
        }
    }

    function addAdditionalFilterId(filters, startIndex, additionalFilterId) {
        for (var i = startIndex, j = filters.length; i < j; i++) {
            filters[i].id = additionalFilterId + filters[i].id;
            filters[i].groupid = additionalFilterId + filters[i].group;
        }
    }

    function addProjectFilterId(id) {
        return id + "_p";
    }

    function add(name, value, removeParams) {
        var path = jq.changeParamValue(location.hash.substring(1), name, value);

        if (removeParams) {
            for (var i = 0, j = removeParams.length; i < j; i++) {
                path = jq.removeParam(removeParams[i], path);
            }
        }

        ASC.Projects.Common.setHash(path);
        setFilterByUrl();
    }

    function addUser(name, value, removeParams) {
        if (value !== "4a515a15-d4d6-4b8e-828e-e0586f18f3a3") {
            add(name, value, removeParams);
        }
    }

    return {
        add: add,
        addUser: addUser,
        baseFilter: baseFilter,

        resize: resize,

        show: show,
        hide: hide,

        createAdvansedFilterForDiscussion: createAdvansedFilterForDiscussion,
        createAdvansedFilterForMilestones: createAdvansedFilterForMilestones,
        createAdvansedFilterForProjects: createAdvansedFilterForProjects,
        createAdvansedFilterForTasks: createAdvansedFilterForTasks,
        createAdvansedFilterForTimeTracking: createAdvansedFilterForTimeTracking
    };
})();
