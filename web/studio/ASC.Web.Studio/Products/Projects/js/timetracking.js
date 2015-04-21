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


ASC.Projects.TimeSpendActionPage = (function() {
    var basePath = 'sortBy=date&sortOrder=descending',
        isTask = false,
        isInit = false;

    var overTimeDescrPanel = false;
    var timeDescribeTimeout = 0;

    var currentProjectId;
    var currentUserId;

    var currentFilter;
    var currentTimesCount;
    var filterTimesCount;
    var lastTimerId = null;

    var totalTimeContainer,
        selectedStatusCombobox,
        timerList = jq("#timeSpendsList"),
        commonListContainer = jq("#CommonListContainer"),
        describePanel = null,
        timeActionPanel = null,
        groupeMenu = "";

    var listActionButtons = [];
    var counterSelectedItems;

    var showCheckboxFlag = false;

    // object for list statuses
    var statusListObject = { listId: "statusListContainer" };
    statusListObject.statuses = [
        { cssClass: "not-chargeable", text: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatusNotChargeable },
        { cssClass: "not-billed", text: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatusNotBilled },
        { cssClass: "billed", text: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatusBilled }
    ];

    var actionMenuItems = { listId: "timeActionPanel" };
    actionMenuItems.menuItems = [
        { id: "ta_edit", text: ASC.Projects.Resources.TasksResource.Edit },
        { id: "ta_remove", text: ASC.Projects.Resources.CommonResource.Delete }
    ];

    var createAdvansedFilter = function() {
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

        var ttfilters = [];
        //Projects
        if (!currentProjectId) {
            ttfilters.push({
                type: "flag",
                id: "myprojects",
                title: ASC.Projects.Resources.ProjectsFilterResource.MyProjects,
                group: ASC.Projects.Resources.ProjectsFilterResource.ByProject,
                hashmask: "myprojects",
                groupby: "projects"
            });
            ttfilters.push({
                type: "combobox",
                id: "project",
                title: ASC.Projects.Resources.ProjectsFilterResource.OtherProjects,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByProject + ":",
                group: ASC.Projects.Resources.ProjectsFilterResource.ByProject,
                options: ASC.Projects.Common.getProjectsForFilter(),
                hashmask: "project/{0}",
                groupby: "projects",
                defaulttitle: ASC.Projects.Resources.ProjectsFilterResource.Select
            });
            ttfilters.push({
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
        //Milestones
        var milestones = ASC.Projects.ProjectsAdvansedFilter.getMilestonesForFilter();
        if (milestones.length > 1) {
            ttfilters.push({
                type: "flag",
                id: "mymilestones",
                title: ASC.Projects.Resources.ProjectsFilterResource.MyMilestones,
                group: ASC.Projects.Resources.ProjectsFilterResource.ByMilestone,
                hashmask: "mymilestones",
                groupby: "milestones"
            });
            ttfilters.push({
                type: "combobox",
                id: "milestone",
                title: ASC.Projects.Resources.ProjectsFilterResource.OtherMilestones,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByMilestone + ":",
                group: ASC.Projects.Resources.ProjectsFilterResource.ByMilestone,
                hashmask: "milestone/{0}",
                groupby: "milestones",
                options: milestones,
                defaulttitle: ASC.Projects.Resources.ProjectsFilterResource.Select
            });
        }
        // Responsible

        if (currentProjectId) {
            if (self.userInProjectTeam(Teamlab.profile.id)) {
                ttfilters.push({
                    type: "combobox",
                    id: "me_tasks_responsible",
                    title: ASC.Projects.Resources.ProjectsFilterResource.Me,
                    filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible + ":",
                    group: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible,
                    hashmask: "person/{0}",
                    groupby: "userid",
                    options: ASC.Projects.ProjectsAdvansedFilter.getTeamForFilter(),
                    bydefault: { value: currentUserId }
                });
            }
            ttfilters.push({
                type: "combobox",
                id: "tasks_responsible",
                title: ASC.Projects.Resources.ProjectsFilterResource.OtherParticipant,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible + ":",
                hashmask: "person/{0}",
                groupby: "userid",
                group: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible,
                options: ASC.Projects.ProjectsAdvansedFilter.getTeamForFilter(),
                defaulttitle: ASC.Projects.Resources.ProjectsFilterResource.Select
            });
        } else {
            ttfilters.push({
                type: "person",
                id: "me_tasks_responsible",
                title: ASC.Projects.Resources.ProjectsFilterResource.Me,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible + ":",
                group: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible,
                hashmask: "person/{0}",
                groupby: "userid",
                bydefault: { id: currentUserId }
            });
            ttfilters.push({
                type: "person",
                id: "tasks_responsible",
                title: ASC.Projects.Resources.ProjectsFilterResource.OtherUsers,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible + ":",
                group: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible,
                hashmask: "person/{0}",
                groupby: "userid"
            });
        }

        ttfilters.push({
            type: "group",
            id: "group",
            title: ASC.Projects.Resources.ProjectsFilterResource.Groups,
            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.Group + ":",
            group: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible,
            hashmask: "group/{0}",
            groupby: "userid"
        });
        //Payment status
        ttfilters.push({
            type: "combobox",
            id: "notChargeable",
            title: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatusNotChargeable,
            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatus + ":",
            group: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatus,
            hashmask: "combobox/{0}",
            groupby: "payment_status",
            options:
                [
                    { value: "notChargeable", title: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatusNotChargeable, def: true },
                    { value: "notBilled", title: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatusNotBilled },
                    { value: "billed", title: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatusBilled }
                ]
        },
        {
            type: "combobox",
            id: "notBilled",
            title: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatusNotBilled,
            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatus + ":",
            group: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatus,
            hashmask: "combobox/{0}",
            groupby: "payment_status",
            options:
                [
                    { value: "notChargeable", title: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatusNotChargeable },
                    { value: "notBilled", title: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatusNotBilled, def: true },
                    { value: "billed", title: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatusBilled }
                ]
        },
        {
            type: "combobox",
            id: "billed",
            title: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatusBilled,
            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatus + ":",
            group: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatus,
            hashmask: "combobox/{0}",
            groupby: "payment_status",
            options:
                [
                    { value: "notChargeable", title: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatusNotChargeable },
                    { value: "notBilled", title: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatusNotBilled },
                    { value: "billed", title: ASC.Projects.Resources.ProjectsFilterResource.PaymentStatusBilled, def: true }
                ]
        });
        //Create date
        ttfilters.push({
            type: "daterange",
            id: "previousweek2",
            title: ASC.Projects.Resources.ProjectsFilterResource.PreviousWeek,
            filtertitle: " ",
            group: ASC.Projects.Resources.ProjectsFilterResource.TimePeriod,
            hashmask: "period/{0}/{1}",
            groupby: "period",
            bydefault: { from: startPreviousWeek, to: endPreviousWeek }
        });
        ttfilters.push({
            type: "daterange",
            id: "previousmonth2",
            title: ASC.Projects.Resources.ProjectsFilterResource.PreviousMonth,
            filtertitle: " ",
            group: ASC.Projects.Resources.ProjectsFilterResource.TimePeriod,
            hashmask: "period/{0}/{1}",
            groupby: "period",
            bydefault: { from: startPreviousMonth, to: endPreviousMonth }
        });
        ttfilters.push({
            type: "daterange",
            id: "period2",
            title: ASC.Projects.Resources.ProjectsFilterResource.CustomPeriod,
            filtertitle: " ",
            group: ASC.Projects.Resources.ProjectsFilterResource.TimePeriod,
            hashmask: "period/{0}/{1}",
            groupby: "period"
        });
        
        self.basePath = 'sortBy=date&sortOrder=descending';
        self.filters = ttfilters;
        self.colCount = 2;
        if (currentProjectId) self.colCount = 3;

        self.sorters =
        [
            { id: "date", title: ASC.Projects.Resources.ProjectsFilterResource.ByDate, sortOrder: "descending", def: true },
            { id: "hours", title: ASC.Projects.Resources.ProjectsFilterResource.ByHours, sortOrder: "ascending" },
            { id: "note", title: ASC.Projects.Resources.ProjectsFilterResource.ByNote, sortOrder: "ascending" }
        ];

        ASC.Projects.ProjectsAdvansedFilter.init(self);
        
        //filter
        ASC.Projects.ProjectsAdvansedFilter.filter.one("adv-ready", function () {
            var projectAdvansedFilterContainer = jq("#ProjectsAdvansedFilter .advansed-filter-list");
            projectAdvansedFilterContainer.find("li[data-id='myprojects'] .inner-text").trackEvent(ga_Categories.timeTrack, ga_Actions.filterClick, 'my_projects');
            projectAdvansedFilterContainer.find("li[data-id='project'] .inner-text").trackEvent(ga_Categories.timeTrack, ga_Actions.filterClick, 'other_projects');
            projectAdvansedFilterContainer.find("li[data-id='tag'] .inner-text").trackEvent(ga_Categories.timeTrack, ga_Actions.filterClick, 'with_tag');
            projectAdvansedFilterContainer.find("li[data-id='notChargeable'] .inner-text").trackEvent(ga_Categories.timeTrack, ga_Actions.filterClick, 'not_chargeable');
            projectAdvansedFilterContainer.find("li[data-id='notBilled'] .inner-text").trackEvent(ga_Categories.timeTrack, ga_Actions.filterClick, 'not_billed');
            projectAdvansedFilterContainer.find("li[data-id='billed'] .inner-text").trackEvent(ga_Categories.timeTrack, ga_Actions.filterClick, 'billed');
            projectAdvansedFilterContainer.find("li[data-id='mymilestones'] .inner-text").trackEvent(ga_Categories.timeTrack, ga_Actions.filterClick, 'my_milestones');
            projectAdvansedFilterContainer.find("li[data-id='milestone'] .inner-text").trackEvent(ga_Categories.timeTrack, ga_Actions.filterClick, 'other_milestones');
            projectAdvansedFilterContainer.find("li[data-id='previousweek2'] .inner-text").trackEvent(ga_Categories.timeTrack, ga_Actions.filterClick, 'previous_week');
            projectAdvansedFilterContainer.find("li[data-id='previousmonth2'] .inner-text").trackEvent(ga_Categories.timeTrack, ga_Actions.filterClick, 'previous_month');
            projectAdvansedFilterContainer.find("li[data-id='period2'] .inner-text").trackEvent(ga_Categories.timeTrack, ga_Actions.filterClick, 'custom_period');
            projectAdvansedFilterContainer.find("li[data-id='me_tasks_responsible'] .inner-text").trackEvent(ga_Categories.timeTrack, ga_Actions.filterClick, 'me-tasks-responsible');
            projectAdvansedFilterContainer.find("li[data-id='tasks_responsible'] .inner-text").trackEvent(ga_Categories.timeTrack, ga_Actions.filterClick, 'tasks-responsible');
            projectAdvansedFilterContainer.find("li[data-id='group'] .inner-text").trackEvent(ga_Categories.timeTrack, ga_Actions.filterClick, 'group');

            jq("#ProjectsAdvansedFilter .btn-toggle-sorter").trackEvent(ga_Categories.timeTrack, ga_Actions.filterClick, 'sort');
            jq("#ProjectsAdvansedFilter .advansed-filter-input").trackEvent(ga_Categories.timeTrack, ga_Actions.filterClick, "search_text", "enter");
        });
    };

    var initPanelsAndPopups = function () {
        if (!describePanel) {
            commonListContainer.append(jq.tmpl("projects_panelFrame", { panelId: "timeTrackingDescrPanel" })); // description panel
            describePanel = jq("#timeTrackingDescrPanel");
        }
        //status list
        jq("#" + statusListObject.listId).remove();
        commonListContainer.append(jq.tmpl("projects_statusChangePanel", statusListObject));
        //action panel
        commonListContainer.append(jq.tmpl("projects_panelFrame", { panelId: "timeActionPanel" }));
        timeActionPanel = jq("#timeActionPanel");
        timeActionPanel.find(".panel-content").empty().append(jq.tmpl("projects_actionMenuContent", actionMenuItems));
        // group action panel
        var userIsManager = jq("#managerNameInfo").data("managerid") == currentUserId;
        if (Teamlab.profile.isAdmin || (currentProjectId && userIsManager)) {
            commonListContainer.append(jq.tmpl("projects_timeTrakingGroupActionMenu", {}));
            groupeMenu = jq("#timeTrakingGroupActionMenu");
        }

        if (!commonListContainer.find("#totalTimeText").length) {
            totalTimeContainer = commonListContainer.prepend("<div class='total-time-forFilter' id='totalTimeText'></div>").find("#totalTimeText");
            //var content = 
            totalTimeContainer.append(jq.tmpl("projects_totalTimeText", {}));
        }
    };
    
    var self;
    var init = function () {
        if (isInit === false) {
            isInit = true;
        }
        self = this;
        self.isFirstLoad = true;
        jq(".mainPageContent").children(".loader-page").show();

        self.setDocumentTitle(ASC.Projects.Resources.CommonResource.TimeTracking);

        currentProjectId = jq.getURLParam("prjID");
        currentUserId = Teamlab.profile.id;

        initPanelsAndPopups();
        jq("#groupActionContainer").append(groupeMenu);
        jq("#groupActionContainer").show();
        
        showCheckboxFlag = groupeMenu.length ? true : false;

        listActionButtons = groupeMenu ? groupeMenu.find(".menuAction") : [];
        listActionButtons.splice(0, 1);

        counterSelectedItems = groupeMenu ? groupeMenu.find(".menu-action-checked-count") : [];

        Teamlab.bind(Teamlab.events.removePrjTime, onRemoveTime);
        Teamlab.bind(Teamlab.events.updatePrjTime, onUpdateTime);

        var taskid = jq.getURLParam("id"),
            textSpan = totalTimeContainer.find("span").first(),
            totalTimeText = "";

        if (taskid != null && typeof taskid != 'undefined') {
            isTask = true;
            Teamlab.getPrjTime({}, taskid, {
                success: function (data, times) {
                    self.hideLoader();
                    if(self.isFirstLoad) {
                        
                    }
                    if (times.length) {
                        jq.each(times, function(i, time) {
                            times[i].showCheckbox = showCheckboxFlag;
                        });
                        jq.tmpl("projects_timeTrackingTmpl", times).appendTo('#timeSpendsList tbody');
                        timerList.show();
                        showTotalCountForTask(times);
                    }
                    showEmptyScreen(times.length);
                }
            });
            totalTimeText = textSpan.data("tasktext");
        } else {
            isTask = false;
            self.initPageNavigator("timeKeyForPagination");

            self.showLoader();

            // waiting data from api
            createAdvansedFilter();
            
            jq("#countOfRows").change(function (evt) {
                self.changeCountOfRows(this.value);
            });
            
            timerList.addClass("forProject");
            totalTimeText = textSpan.data("listtext");
        }
        textSpan.text(totalTimeText);

        // discribe panel
        timerList.on("mouseenter", ".pm-ts-noteColumn a", function(event) {
            timeDescribeTimeout = setTimeout(function() {
                var targetObject = event.target,
                    panelContent = describePanel.find(".panel-content"),
                    createdAttr = jq(targetObject).attr('created'),
                    createdAttrBy = jq(targetObject).attr('createdby'),
                    descriptionObj = {};

                panelContent.empty();

                if (typeof createdAttr != 'undefined' && jq.trim(createdAttr) != "") {
                    descriptionObj.creationDate = createdAttr;
                }

                if (typeof createdAttrBy != 'undefined') {
                    descriptionObj.createdBy = Encoder.htmlEncode(createdAttrBy);
                }

                if (createdAttr.length || createdAttrBy.length) {
                    panelContent.append(jq.tmpl("projects_descriptionPanelContent", descriptionObj));
                    showTimeDescribePanel(targetObject);
                    overTimeDescrPanel = true;
                }                
            }, 500, this);
        });

        timerList.on('mouseleave', '.pm-ts-noteColumn a', function() {
            clearTimeout(timeDescribeTimeout);
            overTimeDescrPanel = false;
            hideDescrPanel();
        });
        describePanel.on('mouseenter', function() {
            overTimeDescrPanel = true;
        });

        describePanel.on('mouseleave', function() {
            overTimeDescrPanel = false;
            hideDescrPanel();
        });

        function showEntityMenu() {
            var self = jq(this);
            if (!self.is(".entity-menu")) self = self.find(".entity-menu");
            
            jq('#timeSpendsList .entity-menu').removeClass('show');
            jq('#timeSpendsList .menuopen').removeClass("menuopen");
            if (jq('.studio-action-panel:visible').length) {
                self.removeClass('show');
            } else {
                self.addClass('show');
            }
            
            self.parents("tr").addClass("menuopen");
        }

        timerList.on('click', ".entity-menu", function() {
            showEntityMenu.call(this);
            showActionsPanel.call(this, 'timeActionPanel');
            return false;
        });
        
        timerList.on('contextmenu', ".timeSpendRecord", function (event) {
            showEntityMenu.call(this);
            showActionsPanel.call(this, 'timeActionPanel', { x: event.pageX | (event.clientX + event.scrollLeft), y: event.pageY | (event.clientY + event.scrollTop) });
            return false;
        });

        jq('#emptyListTimers .addFirstElement').click(function () {
            if (isTask) {
                var taskId = jq.getURLParam("ID");
                self.showTimer('timer.aspx?prjID=' + currentProjectId + '&ID=' + taskId);
            } else {
                if (currentProjectId != null) {
                    self.showTimer('timer.aspx?prjID=' + currentProjectId);
                } else {
                    self.showTimer('timer.aspx');
                }
            }
        });

        timeActionPanel.on("click", "#ta_edit", function() {
            var id = jq(this).attr('timeid');
            var prjId = jq(this).attr('prjid');
            var record = jq('.timeSpendRecord[timeid=' + id + ']');
            var taskId = jq(record).attr('taskid');
            var taskTitle = jq(record).find('.pm-ts-noteColumn').find('a').text();
            var recordNote = jq(record).find('.pm-ts-noteColumn').find('span').text();
            var date = jq(record).attr('date');
            var separateTime = jq.trim(jq(record).find('.pm-ts-hoursColumn').text()).split(' ');
            var responsible = jq(record).find('.pm-ts-personColumn').find('span').attr('userid');
            timeActionPanel.hide();

            ASC.Projects.TimeTrakingEdit.showPopup(prjId, taskId, taskTitle, id, { hours: parseInt(separateTime[0], 10), minutes: parseInt(separateTime[1], 10) }, date, recordNote, responsible);
        });

        timeActionPanel.on("click", "#ta_remove", function() {
            var id = jq(this).attr('timeid');
            timeActionPanel.hide();
            Teamlab.removePrjTime({ timeid: id }, { timeids: [id] }, { error: onUpdatePrjTimeError });
        });

        timerList.on("click", ".pm-ts-personColumn span", function() {
            var userid = jq(this).attr('userid');
            if (!isTask && userid != "4a515a15-d4d6-4b8e-828e-e0586f18f3a3") {
                var path = jq.changeParamValue(ASC.Controls.AnchorController.getAnchor(), 'tasks_responsible', userid);
                path = jq.removeParam('noresponsible', path);
                ASC.Controls.AnchorController.move(path);
            }
        });
        jq('#timeEmptyScreenForFilter').on('click', '.clearFilterButton', function () {
            jq('#ProjectsAdvansedFilter').advansedFilter(null);
            return false;
        });

        timerList.on('click', '.changeStatusCombobox.canEdit', function (event) {
            var visible = jq("#statusListContainer").is(":visible");
            var status = jq(this).find('span:first').attr('class');

            if (selectedStatusCombobox)
                selectedStatusCombobox.removeClass('selected');
            selectedStatusCombobox = jq(this);

            jq('#statusListContainer').attr('data-timeid', jq(this).attr('data-timeid'));
            jq('.studio-action-panel').hide();
            jq('.entity-menu').removeClass('show');
            if (!visible) {
                showStatusListContainer(status);
            }

            return false;
        });

        jq('#statusListContainer li').on('click', function() {
            if (jq(this).is('.selected')) return;
            var timeId = jq('#statusListContainer').attr('data-timeid');
            var status = jq(this).attr('class').split(" ")[0];

            if (status == jq('.timeSpendRecord[timeid=' + timeId + '] .changeStatusCombobox span').attr('class')) return;

            changePaymentStatus(status, timeId);
        });

        jq('body').on("click.timeTracking", function (event) {
            var elt = (event.target) ? event.target : event.srcElement;
            var isHide = true;
            if (jq(elt).is(".studio-action-panel") || jq(elt).is('.entity-menu')) {
                isHide = false;
            }

            if (isHide) {
                jq('#timeSpendsList .menuopen').removeClass("menuopen");
                jq('.studio-action-panel').hide();
                jq('.entity-menu').removeClass('show');
                if (selectedStatusCombobox)
                    selectedStatusCombobox.removeClass('selected');
            }
        });

        // group menu

        jq("#commonPopupContainer").on("click", "#deleteTimersButton", function() {
            jq.unblockUI();
            removeChackedTimers();
            return false;
        });

        jq("#selectAllTimers").change(function() {
            var checkboxes = jq("#timeSpendsList input");
            var rows = jq("#timeSpendsList tr");

            if (jq(this).is(":checked")) {
                checkboxes.each(function (id, item) { item.checked = true; });
                rows.addClass("checked-row");
                unlockActionButtons();
            } else {
                checkboxes.each(function (id, item) { item.checked = false; });
                rows.removeClass("checked-row");
                lockActionButtons();
            }
        });

        jq("#commonPopupContainer").on("click", ".cancel", function () {
            jq.unblockUI();
            return false;
        });

        if (groupeMenu) {
            groupeMenu.on("click", ".unlockAction", function () {
                var actionType = jq(this).attr("data-status");
                var status;
                switch (actionType) {
                    case "not-chargeable":
                        status = 0;
                        break;
                    case "not-billed":
                        status = 1;
                        break;
                    case "billed":
                        status = 2;
                        break;
                    default:
                        showQuestionWindow();
                        return;
                }
                changePaymentSatusByCheckedTimers(actionType, status);
            });

            var options = {
                menuSelector: "#timeTrakingGroupActionMenu",
                menuAnchorSelector: "#selectAllTimers",
                menuSpacerSelector: "#CommonListContainer .header-menu-spacer",
                userFuncInTop: function() { groupeMenu.find(".menu-action-on-top").hide(); },
                userFuncNotInTop: function() { groupeMenu.find(".menu-action-on-top").show(); }
            };
            ScrolledGroupMenu.init(options);
        }

        timerList.on("change", "input", function() {
            var input = jq(this);
            var timerId = input.attr("data-timeId");

            if (input.is(":checked")) {
                jq("tr[timeid=" + timerId + "]").addClass("checked-row");
            } else {
                jq("tr[timeid=" + timerId + "]").removeClass("checked-row");
                jq("#selectAllTimers").removeAttr("checked");
            }

            var countCheckedTimers = getCountCheckedTimers();

            if (countCheckedTimers > 0) {
                if (countCheckedTimers == 1) {
                    unlockActionButtons();
                } else {
                    changeSelectedItemsCounter();
                }
            } else {
                lockActionButtons();
            }
        });

        jq("#deselectAllTimers").click(function() {
            var checkboxes = jq("#timeSpendsList input");
            var rows = jq("#timeSpendsList tr");

            jq("#selectAllTimers").removeAttr("checked");
            checkboxes.removeAttr("checked");
            rows.removeClass("checked-row");
            lockActionButtons();
        });

        //ga-track-events
        //show next
        //responsible
        jq("td[id^=person_]").trackEvent(ga_Categories.timeTrack, ga_Actions.userClick, "tasks-responsible");

        //end ga-track-events
    };
    var getCountCheckedTimers = function() {
        return timerList.find("input:checked").length;
    };

    var getTimersIds = function() {
        var timers = timerList.find("input:checked");

        var timersIds = [];

        for (var i = 0; i < timers.length; i++) {
            timersIds.push(jq(timers[i]).attr("data-timeId"));
        }
        return timersIds;
    };

    var changeSelectedItemsCounter = function() {
        var checkedInputCount = getCountCheckedTimers();
        counterSelectedItems.find("span").text(checkedInputCount + " " + ASC.Projects.Resources.ProjectsJSResource.GroupMenuSelectedItems);
    };

    var unlockActionButtons = function() {
        jq(listActionButtons).addClass("unlockAction");
        changeSelectedItemsCounter();
        counterSelectedItems.show();
    };

    var lockActionButtons = function() {
        jq(listActionButtons).removeClass("unlockAction");
        counterSelectedItems.hide();
    };

    var showQuestionWindow = function () {
        self.showCommonPopup("projects_ttakingRemoveWarning", 400, 300, 0);
        PopupKeyUpActionProvider.EnterAction = "jq('#deleteTimersButton').click();";
    };

    var removeChackedTimers = function() {
        var timersIds = getTimersIds();

        LoadingBanner.displayLoading();
        Teamlab.removePrjTime({}, { timeids: timersIds });
    };

    var changePaymentSatusByCheckedTimers = function(textStatus, status) {
        var timersIds = getTimersIds();

        LoadingBanner.displayLoading();
        Teamlab.changePaymentStatus({ textStatus: textStatus }, { status: status, timeIds: timersIds }, onChangePaymentStatus);
    };

    var changePaymentStatus = function(textStatus, timeId) {
        var status = 0;
        switch (textStatus) {
            case "not-billed":
                status = 1;
                break;
            case "billed":
                status = 2;
                break;
        }
        LoadingBanner.displayLoading();
        Teamlab.changePaymentStatus({ textStatus: textStatus }, { status: status, timeIds: [timeId] }, onChangePaymentStatus);
    };

    var onChangePaymentStatus = function(params, times) {
        for (var i = 0; i < times.length; i++) {
            jq('.timeSpendRecord[timeid=' + times[i].id + '] .changeStatusCombobox span:first').attr('class', params.textStatus);
        }
        LoadingBanner.hideLoading();
    };

    var showStatusListContainer = function(status) {
        var statusListContainer = jq("#statusListContainer");
        selectedStatusCombobox.addClass('selected');

        var top = selectedStatusCombobox.offset().top + 29;
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

    var hideDescrPanel = function() {
        setTimeout(function() {
            if (!overTimeDescrPanel) jq('#timeTrackingDescrPanel').hide(100);
        }, 200);
    };

    var showTotalCountForTask = function(times) {
        var timesCount = times.length;
        if (!timesCount) return;
        var totalTime = 0;
        var billedTime = 0;
        for (var i = 0; i < timesCount; i++) {
            totalTime += times[i].hours;

            if (times[i].paymentStatus == 2) {
                billedTime += times[i].hours;
            }
        }

        var taskTotalTime = jq.timeFormat(totalTime).split(":");
        totalTimeContainer.find(".total-count .hours").text(taskTotalTime[0]);
        totalTimeContainer.find(".total-count .minutes").text(taskTotalTime[1]);

        var taskBilledTime = jq.timeFormat(billedTime).split(":");
        totalTimeContainer.find(".billed-count .hours").text(taskBilledTime[0]);
        totalTimeContainer.find(".billed-count .minutes").text(taskBilledTime[1]);

        totalTimeContainer.addClass("float-none");
        totalTimeContainer.css("visibility", "visible");
    };

    var showTimeDescribePanel = function(targetObject) {
        var x = jq(targetObject).offset().left + 10;
        var y = jq(targetObject).offset().top + 20;
        describePanel.css({ left: x, top: y });
        describePanel.show();

        jq('body').off("click.timeShowTimeDescribePanel");
        jq('body').on("click.timeShowTimeDescribePanel", function (event) {
            var elt = (event.target) ? event.target : event.srcElement;
            var isHide = true;
            if (jq(elt).is('[id="#timeTrackingDescrPanel"]')) {
                isHide = false;
            }

            if (isHide)
                jq(elt).parents().each(function() {
                    if (jq(this).is('[id="#timeTrackingDescrPanel"]')) {
                        isHide = false;
                        return false;
                    }
                });

            if (isHide) {
                jq('#timeSpendsList .menuopen').removeClass("menuopen");
                jq('.studio-action-panel').hide();
            }
        });
    };

    var showEmptyScreen = function (isItems) {
        var emptyScreen = ASC.Projects.ProjectsAdvansedFilter.baseFilter ? '#emptyListTimers' : '#timeEmptyScreenForFilter';
        if (isTask) {
            if (isItems) {
                jq('#emptyListTimers').hide();
                jq("#timeTrakingGroupActionMenu").show();
            } else {
                jq('#emptyListTimers').show();
                jq("#timeTrakingGroupActionMenu").hide();
                totalTimeContainer.css("visibility", "hidden");
            }
        } else {
            if (isItems) {
                jq('.noContentBlock').hide();
                jq('#ProjectsAdvansedFilter').show();
                jq('#tableForNavigation').show();
                jq("#timeTrakingGroupActionMenu").show();
            } else {
                if (filterTimesCount == undefined || filterTimesCount == 0) {
                    jq("#timeTrakingGroupActionMenu").hide();
                    jq(emptyScreen).show();
                    jq('#tableForNavigation').hide();
                    if (emptyScreen == '#emptyListTimers') {
                        jq('#ProjectsAdvansedFilter').hide();
                        jq('#timeEmptyScreenForFilter').hide();
                    }
                }
                else {
                    if (self.currentPage > 0) {
                        self.currentPage--;
                        self.getData(false);
                    }
                }
            }
        }
    };

    var getData = function () {
        self.showLoader();
        self.currentFilter.Count = self.entryCountOnPage;
        self.currentFilter.StartIndex = self.entryCountOnPage * self.currentPage;

        Teamlab.getPrjTime({}, null, { filter: self.currentFilter, success: onGetTimes });
        
    };

    var getTotalTimeByFilter = function(params, filter) {
        if (params.mode != 'next') {
            currentTimesCount = 0;
        }
        filter.Count = 31;
        filter.StartIndex = currentTimesCount;

        Teamlab.getTotalTimeByFilter(params, { filter: filter, success: onGetTotalTime });

        filter.status = 2;
        Teamlab.getTotalTimeByFilter(params, { filter: filter, success: onGetTotalBilledTime });
    };

    var onGetTimes = function (params, data) {
        self.clearTables();

        if (Object.keys(params.__filter).length > 4) {
            totalTimeContainer = commonListContainer.prepend("<div class='total-time-forFilter' id='totalTimeText'></div>").find("#totalTimeText");
            totalTimeContainer.append(jq.tmpl("projects_totalTimeText", {}));
            getTotalTimeByFilter({}, params.__filter);

            var textSpan = totalTimeContainer.find("span").first(), totalTimeText = isTask ? textSpan.data("tasktext") : textSpan.data("listtext");
            
            textSpan.text(totalTimeText);
        }

        var count = data.length;
        currentTimesCount += count;
        clearTimeout(timeDescribeTimeout);
        overTimeDescrPanel = false;
        hideDescrPanel();

        if (params.mode != 'next') {
            jq('#timeSpendsList tbody').html('');
        }

        jq.each(data, function(i, time) {
            data[i].showCheckbox = showCheckboxFlag;
        });

        jq.tmpl("projects_timeTrackingTmpl", data).appendTo('#timeSpendsList tbody');
        timerList.show();

        filterTimesCount = params.__total != undefined ? params.__total : 0;
        self.updatePageNavigator(filterTimesCount, jq(".menu-action-simple-pagenav"));
        showEmptyScreen(count);
        self.hideLoader("#timeTrakingGroupActionMenu");
    };

    var onGetTotalTime = function(params, time) {
        if (time != 0) {
            var fotmatedTime = jq.timeFormat(time).split(":");
            totalTimeContainer.find(".total-count .hours").text(fotmatedTime[0]);
            totalTimeContainer.find(".total-count .minutes").text(fotmatedTime[1]);

            totalTimeContainer.find(".billed-count .hours").text("0");
            totalTimeContainer.find(".billed-count .minutes").text("00");

            totalTimeContainer.css("visibility", "visible");
        } else {
            totalTimeContainer.css("visibility", "hidden");
        }
    };

    var onGetTotalBilledTime = function(params, time) {
        var billedTimeContainer = totalTimeContainer.find(".billed-count");
        if (time != 0) {
            var fotmatedTime = jq.timeFormat(time).split(":");
            billedTimeContainer.find(".hours").text(fotmatedTime[0]);
            billedTimeContainer.find(".minutes").text(fotmatedTime[1]);
        } else {
            billedTimeContainer.find(".hours").text("0");
            billedTimeContainer.find(".minutes").text("00");
        }
    };

    var onUpdateTime = function (params, time) {
        self.displayInfoPanel(ASC.Projects.Resources.ProjectsJSResource.TimeUpdated);
        time.showCheckbox = showCheckboxFlag;
        jq('#timeSpendsList .timeSpendRecord[timeid=' + time.id + ']').replaceWith(jq.tmpl("projects_timeTrackingTmpl", time));
        if (!params.oldTime || !currentProjectId) return;

        var updatedTimeText = jq.timeFormat(time.hours).split(":");
        var updatedTime = { hours: parseInt(updatedTimeText[0], 10), minutes: parseInt(updatedTimeText[1], 10) };

        var difference = { hours: updatedTime.hours - params.oldTime.hours, minutes: updatedTime.minutes - params.oldTime.minutes };

        ASC.Projects.projectNavPanel.changeCommonProjectTime(difference);
        

    };

    var onRemoveTime = function(params, data) {
        var totalTime = 0;
        for (var i = 0; i < data.length; i++) {
            var timer = jq("#timeSpendRecord" + data[i].id);
            totalTime += data[i].hours;
            timer.animate({ opacity: "hide" }, 500);
            filterTimesCount--;
            timer.remove();
            showEmptyScreen(jq("#timeSpendsList .timeSpendRecord").length);
        }
        var timeText = jq.timeFormat(totalTime).split(":");
        ASC.Projects.projectNavPanel.changeCommonProjectTime({ hours: -parseInt(timeText[0], 10), minutes: -parseInt(timeText[1], 10) });

        LoadingBanner.hideLoading();
        self.displayInfoPanel(ASC.Projects.Resources.ProjectsJSResource.TimeRemoved);
    };
    
    var onUpdatePrjTimeError = function(params, data) {
        jq("div.entity-menu[timeid=" + params.timeid + "]").hide();
    };
    
    var showActionsPanel = function (panelId, coord) {
        var self = jq(this);
        var objid = '',
            objidAttr = '',
            x = 0,
            y = 0,
            panel = jq('#' + panelId);
        if (typeof self.attr('timeid') != 'undefined') {
            timeActionPanel.find('.dropdown-item').attr('timeid', self.attr('timeid')).attr('prjid', self.attr('prjid')).attr('userid', self.attr('userid'));
        }
        if (panelId == 'timeActionPanel') objid = self.attr('timeid');
        if (objid.length) objidAttr = '[objid=' + objid + ']';
        if (jq('#' + panelId + ':visible' + objidAttr).length) {
            jq("body").unbind("click");
            jq('.studio-action-panel').hide();
            jq('#timeSpendsList .menuopen').removeClass("menuopen");
        } else {
            if (coord) {
                x = coord.x - panel.outerWidth();
                y = coord.y;
            } else {
                x = self.offset().left - 110;
                y = self.offset().top + 20;
            }


            jq('.studio-action-panel').hide();
            panel.show();
            panel.attr('objid', objid);
            panel.css({ left: x, top: y });

            jq('body').off("click.timeShowActionsPanel");
            jq('body').on("click.timeShowActionsPanel", function (event) {
                var elt = (event.target) ? event.target : event.srcElement;
                var isHide = true;
                if (jq(elt).is('[id="' + panelId + '"]') || (elt.id == this.id && this.id.length) || jq(elt).is('.entity-menu')) {
                    isHide = false;
                }

                if (isHide)
                    jq(elt).parents().each(function() {
                        if (self.is('[id="' + panelId + '"]')) {
                            isHide = false;
                            return false;
                        }
                    });

                if (isHide) {
                    jq('#timeSpendsList .menuopen').removeClass("menuopen");
                    jq('.studio-action-panel').hide();
                    jq('.entity-menu').removeClass('show');
                }
            });

        }
    };

    var unbindListEvents = function () {
        if (!isInit) return;
        jq("#countOfRows").unbind();
        timerList.unbind();
        commonListContainer.unbind();
        jq("#commonPopupContainer").unbind();
        describePanel.unbind();
        jq("#groupActionContainer").hide();
    };

    return jq.extend({
        init: init,
        getData: getData,
        unbindListEvents: unbindListEvents
    }, ASC.Projects.Common);
})(jQuery);
