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

ASC.Projects.AllMilestones = (function () {
    var isInit = false;

    var currentUserId;

    var statusListContainer,
        milestoneActionContainer,
        milestoneDescribePanel,
        // cache DOM elements
        commonListContainer = jq("#CommonListContainer"),
        milestoneList = jq('#milestonesList'),
        milestoneListBody = milestoneList.find("tbody"),
        commonPopupContainer = jq("#commonPopupContainer");

    var currentProjectId;

    var filterMilestoneCount = 0;

    var isFirstLoad = true;

    var selectedStatusCombobox;
    var selectedActionCombobox;

    var descriptionTimeout;
    var overDescriptionPanel = false;

    var hideFirstLoader = function () {
        isFirstLoad = false;
        jq(".mainPageContent").children(".loader-page").hide();
        jq("#filterContainer, #CommonListContainer").show();
        jq('#ProjectsAdvansedFilter').advansedFilter("resize");
    };

    var getMilestoneTasksLink = function (prjId, milestoneId, status) {
        var link = 'tasks.aspx?prjID=' + prjId + '#milestone=' + milestoneId + '&status=' + status;

        if (location.hash.indexOf('user_tasks') > 0)
            link += '&tasks_responsible=' + jq.getAnchorParam('user_tasks', location.href);

        return link;
    };


    var getCurrentProjectId = function () {
        return currentProjectId;
    };

    var setCurrentFilter = function (filter) {
        self.currentFilter = filter;
    };

    var showNewMilestoneButton = function () {
        jq(".addNewButton").removeClass("display-none");
    };

    var hideNewMilestoneButton = function () {
        jq(".addNewButton").addClass("display-none");
    };

    // object for list statuses
    var statusListObject = { listId: "milestonesStatusList" };
    statusListObject.statuses = [
        { cssClass: "open", text: ASC.Projects.Resources.ProjectsJSResource.StatusOpenMilestone },
        { cssClass: "closed", text: ASC.Projects.Resources.ProjectsJSResource.StatusClosedMilestone }
    ];

    var actionMenuItems = { listId: "milestoneActionContainer" };
    actionMenuItems.menuItems = [
        { id: "updateMilestoneButton", text: ASC.Projects.Resources.TasksResource.Edit },
        { id: "addMilestoneTaskButton", text: ASC.Projects.Resources.TasksResource.AddTask },
        { id: "removeMilestoneButton", text: ASC.Projects.Resources.CommonResource.Delete }
    ];

    var initActionPanels = function () {
        if(!milestoneDescribePanel){
            commonListContainer.append(jq.tmpl("projects_panelFrame", { panelId: "milestoneDescrPanel"})); // description panel
            milestoneDescribePanel = jq("#milestoneDescrPanel");
        }

        jq("#" + statusListObject.listId).remove();
        commonListContainer.append(jq.tmpl("projects_statusChangePanel", statusListObject));
        //action panel
        commonListContainer.append(jq.tmpl("projects_panelFrame", { panelId: "milestoneActionContainer"}));
        jq("#milestoneActionContainer .panel-content").empty().append(jq.tmpl("projects_actionMenuContent", actionMenuItems));

    };

    var self;
    
    var init = function () {
        currentUserId = Teamlab.profile.id;

        if (isInit === false) {
            isInit = true;
        }
        
        self = this;
        isFirstLoad = true;
        jq(".mainPageContent").children(".loader-page").show();
        self.setDocumentTitle(ASC.Projects.Resources.ProjectsJSResource.MilestonesModule);

        currentProjectId = jq.getURLParam('prjID');
        //page navigator
        self.initPageNavigator("milestonesKeyForPagination");

        if (!isFirstLoad) {
            LoadingBanner.displayLoading();
            jq("#filterContainer, #CommonListContainer").show();
            jq('#ProjectsAdvansedFilter').advansedFilter("resize");
        } else {
            jq("#filterContainer, #CommonListContainer").hide();
        }

        createAdvansedFilter();
        initActionPanels();
        statusListContainer = jq('#' + statusListObject.listId);
        milestoneActionContainer = jq('#milestoneActionContainer');



        // Events

        jq("#countOfRows").change(function (evt) {
            self.changeCountOfRows(this.value);
        });

        jq('#emptyListMilestone').on('click', '.addFirstElement', function () {
            ASC.Projects.MilestoneAction.showNewMilestonePopup();
        });

        // popup
        commonPopupContainer.on("click", ".remove", function () {
            LoadingBanner.displayLoading();
            var milestoneId = jq("#commonPopupContainer").attr("milestoneId");
            deleteMilestone(milestoneId);
            jq("#commonPopupContainer").removeAttr("milestoneId");
            jq.unblockUI();
            return false;
        });
        commonPopupContainer.on("click", ".gray", function () {
            jq.unblockUI();
            return false;
        });

        jq('body').on('click.milestonesInit', function (event) {
            var target = (event.target) ? event.target : event.srcElement;
            var element = jq(target);
            if (!element.is('.entity-menu')) {
                hideMilestoneActionContainer();
            }
            if (!(element.is('span.overdue') || element.is('span.active') || element.is('span.closed'))) {
                hideStatusListContainer();
            }
        });

        milestoneList.on('click', 'td.status .changeStatusCombobox.canEdit', function (event) {
            hideMilestoneActionContainer();
            var element = (event.target) ? event.target : event.srcElement;
            var status = jq(element).attr('class');
            var currentMilestone = selectedStatusCombobox !== undefined ? selectedStatusCombobox.attr('milestoneId') : -1;
            jq('#milestonesList tr#' + currentMilestone + ' td.status .changeStatusCombobox').removeClass('selected');
            selectedStatusCombobox = jq(this);

            if (statusListContainer.attr('milestoneId') !== selectedStatusCombobox.attr('milestoneId')) {
                statusListContainer.attr('milestoneId', selectedStatusCombobox.attr('milestoneId'));
                showStatusListContainer(status);
            } else {
                toggleStatusListContainer(status);
            }
            return false;
        });

        commonListContainer.find('#' + statusListObject.listId).on('click', 'li', function () {
            if (jq(this).is('.selected')) return;
            var milestoneId = jq('#' + statusListObject.listId).attr('milestoneId');
            var status = jq(this).attr('class').split(" ")[0];
            if (status == 'closed') {
                var text = jq.trim(jq('#' + milestoneId + ' td.activeTasksCount').text());
                if (text != '' && text != '0') {
                    showQuestionWindow(milestoneId);
                    return;
                }
            }

            Teamlab.updatePrjMilestone({ milestoneId: milestoneId }, milestoneId, { status: status }, { success: onUpdateMilestone, error: onChangeStatusError });
        });

        milestoneList.on('mouseenter', 'tr .title a', function (event) {
            descriptionTimeout = setTimeout(function () {
                var targetObject = event.target,
                    panelContent = milestoneDescribePanel.find(".panel-content"),
                    descriptionObj = {};

                panelContent.empty();

                if (!jq.getURLParam("prjID")) {
                    descriptionObj.project = '<span class="link dotline">' + jq.htmlEncodeLight(jq(targetObject).attr('projectTitle')) + '</span>';
                    descriptionObj.projectId = jq(targetObject).attr('projectId');
                }
                if (typeof jq(targetObject).attr('description') != 'undefined') {
                    var description = jq(targetObject).attr('description');
                    if (description != '') {
                        descriptionObj.description = description;
                    }
                }
                if (typeof jq(targetObject).attr('created') != 'undefined') {
                    descriptionObj.creationDate = jq(targetObject).attr('created');
                }
                if (typeof jq(targetObject).attr('createdBy') != 'undefined') {
                    descriptionObj.createdBy = Encoder.htmlEncode(jq(targetObject).attr('createdBy'));
                }
                panelContent.append(jq.tmpl("projects_descriptionPanelContent", descriptionObj));

                panelContent.find(".descrValue").css("maxHeight", "200px");

                showDescriptionPanel(targetObject);
                overDescriptionPanel = true;
            }, 400, this);

        });

        milestoneList.on('mouseleave', 'tr .title a', function () {
            clearTimeout(descriptionTimeout);
            overDescriptionPanel = false;
            hideDescriptionPanel();
        });

        milestoneDescribePanel.on('mouseenter', function () {
            overDescriptionPanel = true;
        });

        milestoneDescribePanel.on('mouseleave', function () {
            overDescriptionPanel = false;
            hideDescriptionPanel();
        });

        commonListContainer.on('click', '.project .value', function () {
            overDescriptionPanel = false;
            hideDescriptionPanel();
            var projectId = jq(this).attr('projectId');
            var path = jq.changeParamValue(ASC.Controls.AnchorController.getAnchor(), 'project', projectId);
            path = jq.removeParam('tag', path);
            ASC.Controls.AnchorController.move(path);
        });

        milestoneList.on('click', 'td.responsible span', function () {
            var responsibleId = jq(this).attr('responsibleId');
            if (responsibleId != "4a515a15-d4d6-4b8e-828e-e0586f18f3a3") {
                var path = jq.changeParamValue(ASC.Controls.AnchorController.getAnchor(), 'responsible_for_milestone', responsibleId);
                path = jq.removeParam('user_tasks', path);
                ASC.Controls.AnchorController.move(path);
            }
        });

        function showEntityMenu() {
            hideStatusListContainer();
            var currentMilestone = selectedActionCombobox !== undefined ? selectedActionCombobox.attr('milestoneId') : -1;
            milestoneList.find("#" + currentMilestone).find(".entity-menu").removeClass('selected');
            selectedActionCombobox = jq(this);

            if (!selectedActionCombobox.is(".entity-menu")) selectedActionCombobox = selectedActionCombobox.siblings(".actions").find(".entity-menu");

            if (selectedActionCombobox.attr('milestoneId') !== milestoneActionContainer.attr('milestoneId')) {
                milestoneActionContainer.attr('milestoneId', selectedActionCombobox.attr('milestoneId'));
                milestoneActionContainer.attr('projectId', selectedActionCombobox.attr('projectId'));
            }
            milestoneList.find(".menuopen").removeClass("menuopen");
            milestoneList.find("#" + selectedActionCombobox.attr('milestoneId')).addClass("menuopen");

            // ga-track
            trackingGoogleAnalitics(ga_Categories.milestones, ga_Actions.actionClick, "milestone-menu");
        }

        milestoneList.on('click', 'td.actions .entity-menu', function () {
            showEntityMenu.call(this);
            showMilestoneActionContainer(selectedActionCombobox);
            return false;
        });
        
        milestoneList.on('contextmenu', 'td.title', function (event) {
            showEntityMenu.call(this);
            
            var top = (event.pageY | (event.clientY + event.scrollTop));
            var left = (event.pageX | (event.clientX + event.scrollLeft)) - milestoneActionContainer.outerWidth();
            milestoneActionContainer.css({ 'top': top, 'left': left });
            milestoneActionContainer.show();

            return false;
        });

        jq('#removeMilestoneButton').on('click', function () {
            var milestoneId = milestoneActionContainer.attr('milestoneId');
            milestoneActionContainer.hide();
            showQuestionWindowMilestoneRemove(milestoneId);
        });

        jq('#addMilestoneTaskButton').on('click', function () {
            milestoneActionContainer.hide();

            var taskParams = {};
            taskParams.milestoneId = parseInt(milestoneActionContainer.attr('milestoneId'));
            taskParams.projectId = parseInt(milestoneActionContainer.attr('projectId'));

            ASC.Projects.TaskAction.showCreateNewTaskForm(taskParams);
        });

        jq('#updateMilestoneButton').on('click', function () {
            var milestoneId = milestoneActionContainer.attr('milestoneId');
            milestoneActionContainer.hide();

            var milestoneRow = milestoneList.find("#" + milestoneId);

            var milestone =
            {
                id: milestoneRow.attr('id'),
                project: milestoneRow.find('.title a').attr('projectId'),
                responsible: milestoneRow.find('.responsible span').attr('responsibleId'),
                deadline: milestoneRow.find('.deadline span').text(),
                title: milestoneRow.find('.title a').text(),
                description: milestoneRow.find('.title a').attr('description'),
                isKey: milestoneRow.attr('isKey'),
                isNotify: milestoneRow.attr('isNotify')
            };

            ASC.Projects.MilestoneAction.onGetMilestoneBeforeUpdate(milestone);
        });

        // ga-track-events
        //change status
        jq("#statusListContainer .open").trackEvent(ga_Categories.milestones, ga_Actions.changeStatus, "open");
        jq("#statusListContainer .closed").trackEvent(ga_Categories.milestones, ga_Actions.changeStatus, "closed");

        //responsible
        jq(".responsible span").trackEvent(ga_Categories.milestones, ga_Actions.userClick, "milestone-responsible");

        //actions
        jq("#addMilestoneTaskButton").trackEvent(ga_Categories.milestones, ga_Actions.actionClick, "add-task-in-milestone");

        //end ga-track-events
    };

    var createAdvansedFilter = function () {
        var now = new Date();
        var today = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0);
        var inWeek = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0);
        inWeek.setDate(inWeek.getDate() + 7);

        var filters = [];

        // Responsible

        if (currentProjectId) {
            if (self.userInProjectTeam(currentUserId)) {
                filters.push({
                    type: "combobox",
                    id: "me_responsible_for_milestone",
                    title: ASC.Projects.Resources.ProjectsFilterResource.Me,
                    filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible + ":",
                    group: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible,
                    options: ASC.Projects.ProjectsAdvansedFilter.getTeamForFilter(),
                    hashmask: "person/{0}",
                    groupby: "userid",
                    bydefault: { value: currentUserId }
                });
                //Tasks
                filters.push({
                    type: "combobox",
                    id: "me_tasks",
                    title: ASC.Projects.Resources.ProjectsFilterResource.MyTasks,
                    filtertitle: ASC.Projects.Resources.ProjectsFilterResource.Tasks + ":",
                    group: ASC.Projects.Resources.ProjectsFilterResource.Tasks,
                    hashmask: "person/{0}",
                    groupby: "taskuserid",
                    options: ASC.Projects.ProjectsAdvansedFilter.getTeamForFilter(),
                    bydefault: { value: currentUserId }
                });
            }
            filters.push({
                type: "combobox",
                id: "responsible_for_milestone",
                title: ASC.Projects.Resources.ProjectsFilterResource.OtherParticipant,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible + ":",
                hashmask: "person/{0}",
                groupby: "userid",
                group: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible,
                options: ASC.Projects.ProjectsAdvansedFilter.getTeamForFilter(),
                defaulttitle: ASC.Projects.Resources.ProjectsFilterResource.Select
            });
            filters.push({
                type: "combobox",
                id: "user_tasks",
                title: ASC.Projects.Resources.ProjectsFilterResource.OtherParticipant,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible + ":",
                hashmask: "person/{0}",
                groupby: "userid",
                group: ASC.Projects.Resources.ProjectsFilterResource.Tasks,
                options: ASC.Projects.ProjectsAdvansedFilter.getTeamForFilter(),
                defaulttitle: ASC.Projects.Resources.ProjectsFilterResource.Select
            });
        } else {
            filters.push({
                type: "person",
                id: "me_responsible_for_milestone",
                title: ASC.Projects.Resources.ProjectsFilterResource.Me,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible + ":",
                group: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible,
                hashmask: "person/{0}",
                groupby: "userid",
                bydefault: { id: currentUserId }
            });
            filters.push({
                type: "person",
                id: "responsible_for_milestone",
                title: ASC.Projects.Resources.ProjectsFilterResource.OtherUsers,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible + ":",
                group: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible,
                hashmask: "person/{0}",
                groupby: "userid"
            });
            //Tasks
            filters.push({
                type: "person",
                id: "me_tasks",
                title: ASC.Projects.Resources.ProjectsFilterResource.MyTasks,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.Tasks + ":",
                group: ASC.Projects.Resources.ProjectsFilterResource.Tasks,
                hashmask: "person/{0}",
                groupby: "taskuserid",
                bydefault: { id: currentUserId }
            });
            filters.push({
                type: "person",
                id: "user_tasks",
                title: ASC.Projects.Resources.ProjectsFilterResource.OtherUsers,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.Tasks + ":",
                group: ASC.Projects.Resources.ProjectsFilterResource.Tasks,
                hashmask: "person/{0}",
                groupby: "taskuserid"
            });
        }

        //Projects
        if (!currentProjectId) {
            filters.push({
                type: "flag",
                id: "myprojects",
                title: ASC.Projects.Resources.ProjectsFilterResource.MyProjects,
                group: ASC.Projects.Resources.ProjectsFilterResource.ByProject,
                hashmask: "myprojects",
                groupby: "projects"
            });
            filters.push({
                type: "combobox",
                id: "project",
                title: ASC.Projects.Resources.ProjectsFilterResource.OtherProjects,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByProject + ":",
                group: ASC.Projects.Resources.ProjectsFilterResource.ByProject,
                options: self.getProjectsForFilter(),
                groupby: "projects",
                defaulttitle: ASC.Projects.Resources.ProjectsFilterResource.Select
            });
            filters.push({
                type: "combobox",
                id: "tag",
                title: ASC.Projects.Resources.ProjectsFilterResource.ByTag,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.Tag + ":",
                group: ASC.Projects.Resources.ProjectsFilterResource.ByProject,
                options: ASC.Projects.ProjectsAdvansedFilter.getTagsForFilter(),
                groupby: "projects",
                defaulttitle: ASC.Projects.Resources.ProjectsFilterResource.Select
            });
        }
        // Status
        filters.push({
            type: "combobox",
            id: "open",
            title: ASC.Projects.Resources.ProjectsFilterResource.StatusOpenMilestone,
            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByStatus + ":",
            group: ASC.Projects.Resources.ProjectsFilterResource.ByStatus,
            hashmask: "combobox/{0}",
            groupby: "status",
            options:
                [
                    { value: "open", title: ASC.Projects.Resources.ProjectsFilterResource.StatusOpenMilestone, def: true },
                    { value: "closed", title: ASC.Projects.Resources.ProjectsFilterResource.StatusClosedMilestone }
                ]
        });
        filters.push({
            type: "combobox",
            id: "closed",
            title: ASC.Projects.Resources.ProjectsFilterResource.StatusClosedMilestone,
            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByStatus + ":",
            group: ASC.Projects.Resources.ProjectsFilterResource.ByStatus,
            hashmask: "combobox/{0}",
            groupby: "status",
            options:
                [
                    { value: "open", title: ASC.Projects.Resources.ProjectsFilterResource.StatusOpenMilestone },
                    { value: "closed", title: ASC.Projects.Resources.ProjectsFilterResource.StatusClosedMilestone, def: true }
                ]
        });
        //Due date
        filters.push({
            type: "flag",
            id: "overdue",
            title: ASC.Projects.Resources.ProjectsFilterResource.Overdue,
            group: ASC.Projects.Resources.ProjectsFilterResource.DueDate,
            hashmask: "overdue",
            groupby: "deadline"
        });
        filters.push({
            type: "daterange",
            id: "today",
            title: ASC.Projects.Resources.ProjectsFilterResource.Today,
            filtertitle: " ",
            group: ASC.Projects.Resources.ProjectsFilterResource.DueDate,
            hashmask: "deadline/{0}/{1}",
            groupby: "deadline",
            bydefault: { from: today.getTime(), to: today.getTime() }
        });
        filters.push({
            type: "daterange",
            id: "upcoming",
            title: ASC.Projects.Resources.ProjectsFilterResource.UpcomingMilestones,
            filtertitle: " ",
            group: ASC.Projects.Resources.ProjectsFilterResource.DueDate,
            hashmask: "deadline/{0}/{1}",
            groupby: "deadline",
            bydefault: { from: today.getTime(), to: inWeek.getTime() }
        });
        filters.push({
            type: "daterange",
            id: "deadline",
            title: ASC.Projects.Resources.ProjectsFilterResource.CustomPeriod,
            filtertitle: " ",
            group: ASC.Projects.Resources.ProjectsFilterResource.DueDate,
            hashmask: "deadline/{0}/{1}",
            groupby: "deadline"
        });

        self.filters = filters;
        self.colCount = 3;
        if (currentProjectId) self.colCount = 2;

        self.sorters =
        [
            { id: "deadline", title: ASC.Projects.Resources.ProjectsFilterResource.ByDeadline, sortOrder: "ascending", def: true },
            { id: "create_on", title: ASC.Projects.Resources.ProjectsFilterResource.ByCreateDate, sortOrder: "descending" },
            { id: "title", title: ASC.Projects.Resources.ProjectsFilterResource.ByTitle, sortOrder: "ascending" }
        ];

        ASC.Projects.ProjectsAdvansedFilter.init(self);

        // ga-track-events

        //filter
        ASC.Projects.ProjectsAdvansedFilter.filter.one("adv-ready", function () {
            var projectAdvansedFilterContainer = jq("#ProjectsAdvansedFilter .advansed-filter-list");
            projectAdvansedFilterContainer.find("li[data-id='me_responsible_for_milestone'] .inner-text").trackEvent(ga_Categories.milestones, ga_Actions.filterClick, 'me_responsible_for_milestone');
            projectAdvansedFilterContainer.find("li[data-id='responsible_for_milestone'] .inner-text").trackEvent(ga_Categories.milestones, ga_Actions.filterClick, 'responsible_for_milestone');
            projectAdvansedFilterContainer.find("li[data-id='me_tasks'] .inner-text").trackEvent(ga_Categories.milestones, ga_Actions.filterClick, 'me_tasks');
            projectAdvansedFilterContainer.find("li[data-id='user_tasks'] .inner-text").trackEvent(ga_Categories.milestones, ga_Actions.filterClick, 'user_tasks');
            projectAdvansedFilterContainer.find("li[data-id='open'] .inner-text").trackEvent(ga_Categories.milestones, ga_Actions.filterClick, 'open');
            projectAdvansedFilterContainer.find("li[data-id='closed'] .inner-text").trackEvent(ga_Categories.milestones, ga_Actions.filterClick, 'closed');
            projectAdvansedFilterContainer.find("li[data-id='myprojects'] .inner-text").trackEvent(ga_Categories.milestones, ga_Actions.filterClick, 'myprojects');
            projectAdvansedFilterContainer.find("li[data-id='project'] .inner-text").trackEvent(ga_Categories.milestones, ga_Actions.filterClick, 'project');
            projectAdvansedFilterContainer.find("li[data-id='tag'] .inner-text").trackEvent(ga_Categories.milestones, ga_Actions.filterClick, 'tag');

            projectAdvansedFilterContainer.find("li[data-id='overdue'] .inner-text").trackEvent(ga_Categories.milestones, ga_Actions.filterClick, 'overdue');
            projectAdvansedFilterContainer.find("li[data-id='today'] .inner-text").trackEvent(ga_Categories.milestones, ga_Actions.filterClick, 'today');
            projectAdvansedFilterContainer.find("li[data-id='upcoming'] .inner-text").trackEvent(ga_Categories.milestones, ga_Actions.filterClick, 'upcoming');
            projectAdvansedFilterContainer.find("li[data-id='deadline'] .inner-text").trackEvent(ga_Categories.milestones, ga_Actions.filterClick, 'user-period');

            jq("#ProjectsAdvansedFilter .btn-toggle-sorter").trackEvent(ga_Categories.milestones, ga_Actions.filterClick, 'sort');
            jq("#ProjectsAdvansedFilter .advansed-filter-input").trackEvent(ga_Categories.milestones, ga_Actions.filterClick, "search_text", "enter");
        });
    };

    var getData = function () {
        self.currentFilter.Count = self.entryCountOnPage;
        self.currentFilter.StartIndex = self.entryCountOnPage * self.currentPage;

        Teamlab.getPrjMilestones({}, { filter: self.currentFilter, success: onGetMilestones });
    };

    var showOrHideEmptyScreen = function (milestonesCount) {
        if (milestonesCount) {
            self.showAdvansedFilter();
            showNewMilestoneButton();
            jq('.noContentBlock').hide();
            milestoneList.show();
        } else {
            jq("#tableForNavigation").hide();
            if (ASC.Projects.ProjectsAdvansedFilter.baseFilter) {
                jq('#mileEmptyScreenForFilter').hide();
                jq('#emptyListMilestone').show();
                hideNewMilestoneButton();
                self.hideAdvansedFilter();
            } else {
                jq('#emptyListMilestone').hide();
                self.showAdvansedFilter();
                jq('#mileEmptyScreenForFilter').show();
            }
        }
    };

    var onGetMilestones = function (params, milestones) {
        self.clearTables();
        var tmplMile, listTmplMiles = new Array(),
            milestonesCount = milestones.length;

        clearTimeout(descriptionTimeout);

        milestoneListBody.empty();
        filterMilestoneCount = params.__total != undefined ? params.__total : 0;

        LoadingBanner.hideLoading();
        showOrHideEmptyScreen(milestonesCount);

        if (milestonesCount) {
            for (var i = 0; i < milestones.length; i++) {
                tmplMile = getMilestoneTemplate(milestones[i]);
                listTmplMiles.push(tmplMile);
            }
            milestoneListBody.append(jq.tmpl("projects_milestoneTemplate", listTmplMiles));
            milestoneList.show();
        }
        self.updatePageNavigator(filterMilestoneCount);
        isFirstLoad ? hideFirstLoader() : LoadingBanner.hideLoading();
    };

    var getMilestoneTemplate = function (milestone) {
        var id = milestone.id;
        var prjId = milestone.projectId;
        var template = {
            id: id,
            isKey: milestone.isKey,
            isNotify: milestone.isNotify,
            title: milestone.title,
            activeTasksCount: milestone.activeTaskCount,
            activeTasksLink: getMilestoneTasksLink(prjId, id, 'open'),
            closedTasksCount: milestone.closedTaskCount,
            closedTasksLink: getMilestoneTasksLink(prjId, id, 'closed'),
            canEdit: milestone.canEdit,
            canDelete: milestone.canDelete,
            projectId: prjId,
            projectTitle: milestone.projectTitle,
            createdById: milestone.createdBy.id,
            createdBy: milestone.createdBy.displayName,
            description: milestone.description,
            created: milestone.displayDateCrtdate
        };

        if (milestone.responsible) {
            template.responsible = milestone.responsible.displayName;
            template.responsibleId = milestone.responsible.id;
        } else {
            template.responsible = null;
            template.responsibleId = null;
        }

        var today = new Date();
        var status = milestone.status == 0
            ? today < milestone.deadline
                ? 'active'
                : 'overdue'
            : 'closed';

        template.status = status;
        template.deadline = milestone.displayDateDeadline;

        return template;
    };

    var showStatusListContainer = function (status) {
        selectedStatusCombobox.addClass('selected');

        var top = selectedStatusCombobox.offset().top + 28;
        var left = selectedStatusCombobox.offset().left;
        statusListContainer.css({ left: left, top: top });

        if (status == 'overdue' || status == 'active') {
            status = 'open';
        }
        var currentStatus = statusListContainer.find('li.' + status);
        currentStatus.addClass('selected');
        currentStatus.siblings().removeClass('selected');

        statusListContainer.show();
    };

    var toggleStatusListContainer = function (status) {
        if (statusListContainer.is(':visible')) {
            selectedStatusCombobox.removeClass('selected');
        } else {
            selectedStatusCombobox.addClass('selected');
        }

        if (status == 'overdue' || status == 'active') {
            status = 'open';
        }
        var currentStatus = statusListContainer.find('li.' + status);
        currentStatus.addClass('selected');
        currentStatus.siblings().removeClass('selected');

        statusListContainer.toggle();
    };

    var hideStatusListContainer = function () {
        if (statusListContainer.is(':visible')) {
            selectedStatusCombobox.removeClass('selected');
        }
        statusListContainer.hide();
        jq("#projectActions").hide();
        jq(".project-title .menu-small").removeClass("active");
    };

    var showQuestionWindow = function (milestoneId) {
        self.showCommonPopup("projects_closeMilestoneWithOpenTasks", 400, 200, 0);
        var proj = jq("tr#" + milestoneId + " td.title").find("a").attr("projectid");
        jq("#linkToTasksPage").attr("href", 'tasks.aspx?prjID=' + proj + '#milestone=' + milestoneId + '&status=open');
    };

    var showQuestionWindowMilestoneRemove = function (milestoneId) {
        self.showCommonPopup("projects_milestoneRemoveWarning", 400, 200, 0);
        commonPopupContainer.attr("milestoneId", milestoneId);
    };

    var showDescriptionPanel = function (obj) {
        var x, y;
        milestoneDescribePanel.show();

        x = jq(obj).offset().left;
        y = jq(obj).offset().top + 20;

        milestoneDescribePanel.find(".dropdown-item").show();

        milestoneDescribePanel.css({ left: x, top: y });
    };

    var hideDescriptionPanel = function () {
        setTimeout(function () {
            if (!overDescriptionPanel) {
                milestoneDescribePanel.hide(100);
            }
        }, 200);
    };

    var showMilestoneActionContainer = function () {
        selectedActionCombobox.addClass('selected');


        var currentStatus = selectedActionCombobox.attr('status');
        if (currentStatus == 'closed') {
            jq('#updateMilestoneButton').hide();
            jq('#addMilestoneTaskButton').hide();
        }
        else {
            jq('#updateMilestoneButton').show();
            jq('#addMilestoneTaskButton').show();
        }
        
        if (selectedActionCombobox.attr('candelete') == "false") {
            jq('#removeMilestoneButton').hide();
        }

        var top = selectedActionCombobox.offset().top + selectedActionCombobox.innerHeight();
        var left = selectedActionCombobox.offset().left - milestoneActionContainer.innerWidth() + 29;

        if (milestoneActionContainer.position().top == top && (milestoneActionContainer.position().left == left)) {
            milestoneList.find("#" + selectedActionCombobox.attr('milestoneId')).removeClass("menuopen");
            toggleMilestoneActionContainer();
            return;
        }
        
        milestoneActionContainer.css({ 'top': top, 'left': left });
        milestoneActionContainer.show();
    };

    var toggleMilestoneActionContainer = function () {
        if (milestoneActionContainer.is(':visible')) {
            selectedActionCombobox.removeClass('selected');
        } else {
            selectedActionCombobox.addClass('selected');
        }

        var currentStatus = selectedActionCombobox.attr('status');
        if (currentStatus == 'closed') {
            jq('#updateMilestoneButton').hide();
            jq('#addMilestoneTaskButton').hide();
        }
        else {
            jq('#updateMilestoneButton').show();
            jq('#addMilestoneTaskButton').show();
        }

        milestoneActionContainer.toggle();
    };

    var hideMilestoneActionContainer = function () {
        if (selectedActionCombobox) {
            selectedActionCombobox.removeClass('selected');
            milestoneList.find(".menuopen").removeClass("menuopen");
        }
        milestoneActionContainer.hide();
        jq("#projectActions").hide();
        jq(".project-title .menu-small").removeClass("active");
    };

    var removeMilestonesActionsForManager = function () {
        var milestones = milestoneList.find("tr");
        for (var i = 0; i < milestones.length; i++) {
            var responsibleId = jq(milestones[i]).find(".responsible span").attr("responsibleid");
            if (responsibleId != currentUserId) {
                jq(milestones[i]).find(".entity-menu").remove();
            }
        }
        jq("#emptyListMilestone .emptyScrBttnPnl").remove();
    };

    var onAddMilestone = function (params, milestone) {
        filterMilestoneCount++;
        self.updatePageNavigator(filterMilestoneCount);

        var milestoneTemplate = getMilestoneTemplate(milestone);

        var firstMilestone = milestoneListBody.find("tr:first"),
            addedMilestone = jq.tmpl("projects_milestoneTemplate", milestoneTemplate);

        if (firstMilestone.length == 0) {
            showOrHideEmptyScreen(1);
            milestoneListBody.append(addedMilestone);
            ASC.Projects.MilestoneAction.unlockMilestoneActionPage();
            jq.unblockUI();
            return;
        }

        addedMilestone.insertBefore(firstMilestone);
        addedMilestone.yellowFade();
        ASC.Projects.MilestoneAction.unlockMilestoneActionPage();
        jq.unblockUI();
    };
    var onUpdateMilestone = function (params, milestone) {
        var milestoneTemplate = getMilestoneTemplate(milestone);

        var updatedMilestone = milestoneList.find("#" + milestone.id);

        var newMilestone = jq.tmpl("projects_milestoneTemplate", milestoneTemplate);

        updatedMilestone.replaceWith(newMilestone);
        newMilestone.yellowFade();
        ASC.Projects.MilestoneAction.unlockMilestoneActionPage();
        jq.unblockUI();
        self.displayInfoPanel(ASC.Projects.Resources.ProjectsJSResource.MilestoneUpdated);
    };

    var onUpdateMilestoneError = function (params, error) {
        ASC.Projects.MilestoneAction.unlockMilestoneActionPage();
        jq.unblockUI();
    };

    var onChangeStatusError = function (params, error) {
        if (error[0] == "Can not close a milestone with open tasks") {
            showQuestionWindow(params.milestoneId);
        } else {
            self.displayInfoPanel(error[0], true);
        }
    };

    var deleteMilestone = function (milestoneId) {
        var params = {};
        Teamlab.removePrjMilestone(params, milestoneId, { success: onDeleteMilestone });
    };

    var onDeleteMilestone = function (params, milestone) {
        var milestoneId = milestone.id;
        var removedMilestone = milestoneList.find("#" + milestoneId);
        removedMilestone.yellowFade();
        removedMilestone.remove();

        if (currentProjectId == milestone.projectId) {
            ASC.Projects.projectNavPanel.changeModuleItemsCount(ASC.Projects.projectNavPanel.projectModulesNames.milestones, "delete");
        }

        filterMilestoneCount--;
        self.updatePageNavigator(filterMilestoneCount);

        if (milestoneListBody.children("tr").length == 0) {
            clearTimeout(descriptionTimeout);
            milestoneDescribePanel.hide();
            jq('#tableForNavigation').hide();

            if (filterMilestoneCount == 0) {
                showOrHideEmptyScreen(0);
            }
            else {
                self.currentPage--;
                getData(true);
            }
        }
        
        LoadingBanner.hideLoading();
        self.displayInfoPanel(ASC.Projects.Resources.ProjectsJSResource.MilestoneRemoved);
    };

    var onAddTask = function (params, task) {
        if (task.milestone == null) return;
        var milestoneId = task.milestone.id;
        Teamlab.getPrjMilestone({}, milestoneId, { success: onGetMilestoneAfterAddTask });
    };

    var onGetMilestoneAfterAddTask = function (params, milestone) {
        var milestoneTemplate = getMilestoneTemplate(milestone);

        var updatedMilestone = milestoneListBody.find("#" + milestoneTemplate.id);
        var newMilestone = jq.tmpl("projects_milestoneTemplate", milestoneTemplate);

        if (updatedMilestone.length)
            updatedMilestone.replaceWith(newMilestone);
        newMilestone.yellowFade();
    };

    var unbindListEvents = function () {
        if (!isInit) return;
        jq("#countOfRows").unbind();
        milestoneList.unbind();
        commonListContainer.unbind();
        milestoneDescribePanel.unbind();
        commonPopupContainer.unbind();
        jq('#' + statusListObject.listId).unbind();
    };

    return jq.extend({
        init: init,
        getCurrentProjectId: getCurrentProjectId,
        setCurrentFilter: setCurrentFilter,
        onAddMilestone: onAddMilestone,
        onUpdateMilestone: onUpdateMilestone,
        onUpdateMilestoneError: onUpdateMilestoneError,
        onDeleteMilestone: onDeleteMilestone,
        getMilestoneTemplate: getMilestoneTemplate,
        removeMilestonesActionsForManager: removeMilestonesActionsForManager,
        onAddTask: onAddTask,
        getData: getData,
        unbindListEvents: unbindListEvents,
        basePath: 'sortBy=deadline&sortOrder=ascending'
    }, ASC.Projects.Common);
})(jQuery);