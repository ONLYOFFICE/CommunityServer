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


ASC.Projects.TasksManager = (function() {
    var isInit = false,
        currentUserId,
        currentProjectId,
        filteredTasks = [],
        taskDescriptionTimeout = 0,
        overTaskDescriptionPanel = false,
        selectedStatusCombobox = undefined,
        statusListContainer = undefined,
        projectParticipants = undefined,
        filterTaskCount = 0,
        // cache DOM elements
        $taskListContainer = jq('.taskList'),
        taskActionPanel = null,
        taskDescribePanel = null,
        statusListObject = {
            listId: "tasksStatusList",
            statuses: [
                { cssClass: "open", text: ASC.Projects.Resources.TasksResource.Open },
                { cssClass: "closed", text: ASC.Projects.Resources.TasksResource.Closed }
            ]
        },
        actionMenuItems = {
            listId: "taskActionPanel",
            menuItems: [
                { id: "ta_edit", text: ASC.Projects.Resources.TasksResource.Edit },
                { id: "ta_subtask", text: ASC.Projects.Resources.TasksResource.AddSubtask },
                { id: "ta_accept", text: ASC.Projects.Resources.TasksResource.AcceptSubtask },
                { id: "ta_move", text: ASC.Projects.Resources.TasksResource.MoveToMilestone },
                { id: "ta_mesres", text: ASC.Projects.Resources.TasksResource.MessageResponsible },
                { id: "ta_time", text: ASC.Projects.Resources.TasksResource.TrackTime },
                { id: "ta_remove", text: ASC.Projects.Resources.CommonResource.Delete }
            ]
        };
    
    var self;
    var init = function () {
        self = this;
        if (isInit === false) {
            initActionPanels();
            isInit = true;
        }
        self.isFirstLoad = true;
        self.cookiePagination = "tasksKeyForPagination";
        self.setDocumentTitle(ASC.Projects.Resources.ProjectsJSResource.TasksModule);
        self.checkElementNotFound(ASC.Projects.Resources.ProjectsJSResource.TaskNotFound);

        currentUserId = Teamlab.profile.id;
        currentProjectId = jq.getURLParam('prjID');

        ASC.Projects.SubtasksManager.init();
        ASC.Projects.SubtasksManager.onAddSubtaskHandler = onAddSubtask;
        ASC.Projects.SubtasksManager.onRemoveSubtaskHandler = onRemoveSubtask;
        ASC.Projects.SubtasksManager.onChangeTaskStatusHandler = onUpdateSubtaskStatus;

        //page navigator
        ASC.Projects.PageNavigator.init(self);

        self.showLoader();        

        statusListContainer = jq('#' + statusListObject.listId);

        // waiting data from api
        createAdvansedFilter();

        updateMilestonesListForMovePanel(ASC.Projects.Master.Milestones);

        if (currentProjectId) {
            projectParticipants = ASC.Projects.Master.Team;
            taskDescribePanel.find(".project").remove();
        }

        jq('body').on("click.tasksInit", function (event) {
            var elt = (event.target) ? event.target : event.srcElement;
            var isHide = true;
            var $elt = jq(elt);

            if (
              $elt.is('.studio-action-panel') ||
			  $elt.is('#taskName') ||
			  $elt.is('.subtask-name-input') ||
			  $elt.is('.choose') ||
			  $elt.is('.choose > *') ||
			  $elt.is('.combobox-title') ||
			  $elt.is('.combobox-title-inner-text') ||
			  $elt.is('.option-item')
			) {
                isHide = false;
            }

            if (isHide) {
                hideStatusListContainer();
            }
        });

        statusListContainer.on("click", "li", function () {
            if (jq(this).is('.selected')) return;
            var taskid = statusListContainer.attr('taskid'),
                status = jq(this).attr('class').split(" ")[0],
                task = jq('.taskList .task[taskid=' + taskid + ']');

            if (status == task.find(".changeStatusCombobox").find("span").attr('class')) return;
            if (status == 'closed') {
                var subtasks = $taskListContainer.find('.subtask[taskid=' + taskid + ']'),
                    closedSubtasks = subtasks.filter(".closed");
                if (subtasks.length && subtasks.length != closedSubtasks.length) {
                    popupWindow(taskid);
                } else {
                    closeTask(taskid);
                }
            } else {
                task.find(".check div").hide();
                task.find(".check").append('<div class="taskProcess"></div>');
                updateTaskStatus({}, taskid, 1);
            }
        });
        
        jq('#othersListPopup').on('click', '.user', function () {
            var userid = jq(this).attr('userId');
            if (userid != "4a515a15-d4d6-4b8e-828e-e0586f18f3a3") {
                var path = jq.changeParamValue(ASC.Controls.AnchorController.getAnchor(), 'tasks_responsible', userid);
                path = jq.removeParam('noresponsible', path);
                ASC.Controls.AnchorController.move(path);
            }
        });

        self.$commonListContainer.on('click', '.project .value', function () {
            var path = jq.changeParamValue(ASC.Controls.AnchorController.getAnchor(), 'project', jq(this).attr('projectid'));
            path = jq.removeParam('milestone', path);
            path = jq.removeParam('myprojects', path);
            ASC.Controls.AnchorController.move(path);
        });

        self.$commonListContainer.on('click', '.milestone .value', function () {
            var path = jq.changeParamValue(ASC.Controls.AnchorController.getAnchor(), 'milestone', jq(this).attr('milestone'));
            ASC.Controls.AnchorController.move(path);
        });

        jq('.addTask').on('click', function () {
            showNewTaskPopup();
            return false;
        });

        jq("#emptyListTask").on("click", ".addFirstElement", function () {
            showNewTaskPopup();
            return false;
        });

        jq("#moveTaskPanel").on("click", ".blue", function () {
            var data = {},
                taskId = parseInt(jq("#moveTaskPanel").attr("taskid"), 10),
                task = getFilteredTaskById(taskId);

            data.newMilestoneID = jq('#moveTaskPanel .milestonesList input:checked').attr('value');

            if ((task.milestoneId || 0) == parseInt(data.newMilestoneID, 10)) {
                jq.unblockUI();
                return false;
            }

            if (task.links) {
                MoveTaskQuestionPopup.setParametrs("#moveTaskPanel",
                    task.links,
                    function() {
                        Teamlab.updatePrjTask({}, taskId, data, { success: onUpdateTask });
                    },
                    function() {
                        jq.unblockUI();
                        StudioBlockUIManager.blockUI(jq('#moveTaskPanel'), 550, 300, 0);
                    },
                    jq("#removeTaskLinksQuestionPopup"));
                
                MoveTaskQuestionPopup.showDialog();
            } else {
                Teamlab.updatePrjTask({}, taskId, data, { success: onUpdateTask });
                jq.unblockUI();
            }
        });

        taskActionPanel.on('click', "#ta_accept", function () {
            jq('.studio-action-panel').hide();

            var taskId = taskActionPanel.attr('objid');
            var taskRow = jq('.task[taskid=' + taskId + ']');
            var taskLink = taskRow.find(".taskName a");

            var data = {};
            data.title = jq.trim(taskRow.find(".taskName a").text());

            var deadline = taskLink.attr("data-deadline");
            if (deadline) {
                data.deadline = new Date(deadline);
                data.deadline.setHours(0);
                data.deadline.setMinutes(0);
                data.deadline = Teamlab.serializeTimestamp(data.deadline);
            }
            var description = taskLink.attr("description");
            if (description) {
                data.description = description;
            }
            var milestoneId = taskLink.attr("milestoneid");
            if (milestoneId) {
                data.milestoneid = milestoneId;
            }

            data.priority = taskRow.find(".high_priority").length ? 1 : 0;
            data.responsibles = [currentUserId];

            Teamlab.updatePrjTask({}, taskId, data, { success: onUpdateTask });
            return false;
        });

        taskActionPanel.on('click', "#ta_edit", function () {
            jq(".studio-action-panel").hide();
            var taskId = jq('#taskActionPanel').attr('objid');
            ASC.Projects.TaskAction.showUpdateTaskForm(taskId, getFilteredTaskById(taskId));
            return false;
        });

        taskActionPanel.on('click', "#ta_subtask", function () {
            ASC.Projects.SubtasksManager.hideSubtaskFields();
            var taskid = jq('#taskActionPanel').attr('objid');
            var subtaskCont = jq('.subtasks[taskid=' + taskid + ']');

            if (!jq(subtaskCont).is(':visible')) {
                separateSubtasks(taskid);
            }

            ASC.Projects.SubtasksManager.addFirstSubtask(subtaskCont.find(".quickAddSubTaskLink"));

            jq('.studio-action-panel').hide();
            jq('.taskList .task').removeClass('menuopen');
            return false;
        });
        
        taskActionPanel.on('click', "#ta_move", function () {
            jq('.studio-action-panel').hide();
            if (!currentProjectId) {
                getMilestonesForMovePanel({}, jq(this).attr('projectid'));
            } else {
                showMoveToMilestonePanel();
            }
            return false;
        });
        
        taskActionPanel.on('click', "#ta_remove", function () {
            var taskId = jq('#taskActionPanel').attr('objid');
            jq('.studio-action-panel').hide();
            showQuestionWindowTaskRemove(taskId);
            return false;
        });
        
        taskActionPanel.on('click', "#ta_mesres", function () {
            var taskId = jq('#taskActionPanel').attr('objid');
            notifyTaskResponsible({}, taskId);
            jq('.studio-action-panel').hide();
            return false;
        });
        
        taskActionPanel.on('click', "#ta_time", function () {
            jq('.studio-action-panel').hide();
            var taskId = jq('#taskActionPanel').attr('objid');
            var projectId = jq('#taskActionPanel #ta_time').attr('projectid');
            var user = jq('#taskActionPanel #ta_time').attr('userid');
            if (!user) {
                user = currentUserId;
            }
            ASC.Projects.Common.showTimer('timer.aspx?prjID=' + projectId + '&taskId=' + taskId + '&userID=' + user);
            return false;
        });
        
        $taskListContainer.on('click', '.changeStatusCombobox.canEdit', function (event) {
            selectedStatusCombobox = jq(this);
            var status = selectedStatusCombobox.find('span:first').attr('class'),
                current = selectedStatusCombobox.attr('taskid');
            $taskListContainer.find(".changeStatusCombobox").removeClass('selected');

            if (statusListContainer.attr('taskid') !== selectedStatusCombobox.attr('taskid')) {
                statusListContainer.attr('taskid', selectedStatusCombobox.attr('taskid'));
            }

            showStatusListContainer(status);

            return false;
        });

        $taskListContainer.on('click', '.task .user', function (event) {
            var userid = jq(this).attr('userId');
            if (userid != "4a515a15-d4d6-4b8e-828e-e0586f18f3a3") {
                var path;
                if (jq(this).hasClass('not')) {
                    path = jq.changeParamValue(ASC.Controls.AnchorController.getAnchor(), 'noresponsible', true);
                    path = jq.removeParam('tasks_responsible', path);
                } else {
                    path = jq.changeParamValue(ASC.Controls.AnchorController.getAnchor(), 'tasks_responsible', userid);
                    path = jq.removeParam('noresponsible', path);
                }
                path = jq.removeParam('group', path);
                ASC.Controls.AnchorController.move(path);
            }
            event.stopPropagation();
        });
        
        $taskListContainer.on('click', '.task .other', function (event) {
            jq('#othersListPopup').html(jq('.taskList .task .others[taskid="' + jq(this).attr('taskid') + '"]').html());
            showActionsPanel.call(this, 'othersPanel');
            event.stopPropagation();
        });
        
        $taskListContainer.on('mouseenter', '.task .taskName a', function (event) {
            taskDescriptionTimeout = setTimeout(function () {
                var targetObject = event.target,
                    taskDescribePanel = jq('#taskDescrPanel'),
                    panelContent = taskDescribePanel.find(".panel-content");
                    descriptionObj = {};

                    panelContent.empty();

                if (jq(targetObject).attr('status') == 2) {
                    if (typeof jq(targetObject).attr('updated') != 'undefined') {
                        if (jq(targetObject).attr('updated').length) {
                            descriptionObj.closedDate = jq(targetObject).attr('updated').substr(0, 10);
                        }
                        if (jq(targetObject).attr('createdby').length) {
                            descriptionObj.closedBy = Encoder.htmlEncode(jq(targetObject).attr('createdby'));
                        }
                    }
                } else {
                    if (typeof jq(targetObject).attr('created') != 'undefined') {
                        if (jq(targetObject).attr('created').length) {
                            descriptionObj.creationDate = jq(targetObject).attr('created').substr(0, 10);
                        }
                    }
                    if (typeof jq(targetObject).attr('createdby') != 'undefined') {
                        descriptionObj.createdBy = Encoder.htmlEncode(jq(targetObject).attr('createdby'));
                    }
                }
                if (jq(targetObject).attr('data-start').length) {
                    descriptionObj.startDate = jq(targetObject).attr('data-start')
                }
                if (!jq.getURLParam("prjID")) {
                    descriptionObj.project = '<span class="link dotline">' + Encoder.htmlEncode(jq(targetObject).attr('project')) + '</span>';
                    descriptionObj.projectId = jq(targetObject).attr('projectid');
                }
                if (typeof jq(targetObject).attr('milestone') != 'undefined') {
                    descriptionObj.milestone = '<span class="link dotline">' + Encoder.htmlEncode(jq(targetObject).attr('milestone')) + '</span>';
                    descriptionObj.projectId = jq(targetObject).attr('projectid');
                    descriptionObj.milestoneId = jq(targetObject).attr('milestoneid');
                }
                var description = jq(targetObject).attr('description');
                if (jq.trim(description) != '') {
                    descriptionObj.description = jq(targetObject).attr('description');
                    if (description.indexOf("\n") > 2 || description.length > 80) {
                        descriptionObj.readMore = "tasks.aspx?prjID=" + jq(targetObject).attr('projectid') + "&id=" + jq(targetObject).attr('taskid');
                    }
                }

                panelContent.append(jq.tmpl("projects_descriptionPanelContent", descriptionObj));

                showActionsPanel.call(targetObject, 'taskDescrPanel');
                overTaskDescriptionPanel = true;
            }, 400, this);
        });

        $taskListContainer.on('mouseleave', '.task .taskName a', function () {
            clearTimeout(taskDescriptionTimeout);
            overTaskDescriptionPanel = false;
            hideDescriptionPanel();
        });

        function showEntityMenu(event) {
            ASC.Projects.SubtasksManager.hideSubtaskActionPanel();
            ASC.Projects.SubtasksManager.hideSubtaskFields();

            $taskListContainer.find(".task").removeClass('menuopen');
            jq(this).closest(".task").addClass('menuopen');

            var params = undefined;
            if (event)
            {
                params = {
                    x: event.pageX | (event.clientX + event.scrollLeft),
                    y: event.pageY | (event.clientY + event.scrollTop)
                }

            }
            showActionsPanel.call(this, 'taskActionPanel', params);

            return false;
        }

        $taskListContainer.on('click', '.task .entity-menu', function () {
            return showEntityMenu.call(this);
        });

        $taskListContainer.on('contextmenu', '.task', function (event) {
            jq('.studio-action-panel, .filter-list').hide();
            jq(".entity-menu.active").removeClass("active");
            return showEntityMenu.call(this, event);
        });

        $taskListContainer.on('click', '.subtasksCount span.expand', function () {
            hideTaskActionPanel();
            ASC.Projects.SubtasksManager.hideSubtaskFields();

            var taskId = jq(this).attr('taskid');
            if (showOrHideListSubtasks(taskId)) {
                jq(this).attr('class', 'collaps');
            }
            return false;
        });

        $taskListContainer.on('click', '.subtasksCount span.collaps', function () {
            hideTaskActionPanel();
            ASC.Projects.SubtasksManager.hideSubtaskFields();

            var taskId = jq(this).attr('taskid');
            if (showOrHideListSubtasks(taskId)) {
                jq(this).attr('class', 'expand');
            }
            return false;
        });

        $taskListContainer.on('click', '.task', function (event) {
            hideTaskActionPanel();
            ASC.Projects.SubtasksManager.hideSubtaskFields();

            var elt = (event.target) ? event.target : event.srcElement;
            if (jq(elt).is('a')) {
                return undefined;
            }
            var taskid = jq(jq(this).find('.taskName')).attr('taskid');
            jq(this).find(".expand").attr("class", "collaps");
            showOrHideListSubtasks(taskid);

            return false;
        });

        $taskListContainer.on('click', '.subtasksCount span.add', function (event) {
            hideTaskActionPanel();
            ASC.Projects.SubtasksManager.hideSubtaskFields();
            event.stopPropagation();

            var taskid = jq(this).attr('taskid');
            var subtaskCont = jq('.subtasks[taskid=' + taskid + ']');

            showOrHideListSubtasks(taskid, true);

            ASC.Projects.SubtasksManager.addFirstSubtask(subtaskCont.find(".quickAddSubTaskLink"));
        });

        taskDescribePanel.on('mouseenter', function () {
            overTaskDescriptionPanel = true;
        });

        taskDescribePanel.on('mouseleave', function () {
            overTaskDescriptionPanel = false;
            hideDescriptionPanel();
        });

        jq('#moveTaskPanel .gray').on('click', function () {
            jq('.taskList .task').removeClass('menuopen');
            jq.unblockUI();
            return false;
        });
        
        self.$commonPopupContainer.on("click", ".cancel, .ok", function () {
            jq.unblockUI();
            return false;
        });

        self.$commonPopupContainer.on('click', "#popupRemoveTaskButton", function () {
            var taskId = self.$commonPopupContainer.attr('taskId');
            removeTask({ 'taskId': taskId }, taskId);
            return false;
        });

        self.$commonPopupContainer.on("click", ".end", function () {
            closeTask(jq(this).attr('taskid'));
            jq.unblockUI();
            return false;
        });
    };

    var initActionPanels = function () {
        self.$commonListContainer.append(jq.tmpl("projects_panelFrame", { panelId: "taskDescrPanel" }));
        taskDescribePanel = jq("#taskDescrPanel");

        jq("#" + statusListObject.listId).remove();
        self.$commonListContainer.append(jq.tmpl("projects_statusChangePanel", statusListObject));
        //action panel
        self.$commonListContainer.append(jq.tmpl("projects_panelFrame", { panelId: "taskActionPanel" }));
        taskActionPanel = jq("#taskActionPanel");
        taskActionPanel.find(".panel-content").empty().append(jq.tmpl("projects_actionMenuContent", actionMenuItems));

        self.$commonListContainer.append(jq.tmpl("projects_panelFrame", { panelId: "othersPanel" }));
        jq("#othersPanel .panel-content").empty().append(jq.tmpl("projects_actionMenuContent", { menuItems: [] }));
        jq("#othersPanel .dropdown-content").attr("id", "othersListPopup");

        // init move task to milestone popup
        var clonedPopup = self.$commonPopupContainer.clone();
        self.$commonListContainer.append(clonedPopup.attr("id", "moveTaskPanel"));
        jq("#moveTaskPanel .commonPopupContent").append(jq.tmpl("projects_moveTaskPopup", {}));
        jq("#moveTaskPanel .commonPopupHeaderTitle").empty().text(jq("#moveTaskPanel .hidden-title-text").text());
    };

    var createAdvansedFilter = function() {
        var now = new Date(),
            today = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0),
            inWeek = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0);
        inWeek.setDate(inWeek.getDate() + 7);

        var filters = [];

        // Responsible
        if (currentProjectId) {
            if (ASC.Projects.Common.userInProjectTeam(currentUserId)) {
                filters.push({
                    type: "combobox",
                    id: "me_tasks_responsible",
                    title: ASC.Projects.Resources.ProjectsFilterResource.Me,
                    filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible + ":",
                    group: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible,
                    options: ASC.Projects.ProjectsAdvansedFilter.getTeamForFilter(),
                    hashmask: "person/{0}",
                    groupby: "userid",
                    bydefault: { value: currentUserId }
                });
            }
            filters.push({
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
            filters.push({
                type: "person",
                id: "me_tasks_responsible",
                title: ASC.Projects.Resources.ProjectsFilterResource.Me,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible + ":",
                group: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible,
                hashmask: "person/{0}",
                groupby: "userid",
                bydefault: { id: currentUserId }
            });
            filters.push({
                type: "person",
                id: "tasks_responsible",
                title: ASC.Projects.Resources.ProjectsFilterResource.OtherUsers,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible + ":",
                group: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible,
                hashmask: "person/{0}",
                groupby: "userid"
            });
        }
        filters.push({
            type: "group",
            id: "group",
            title: ASC.Projects.Resources.ProjectsFilterResource.Groups,
            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.Group + ":",
            group: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible,
            hashmask: "group/{0}",
            groupby: "userid"
        });
        filters.push({
            type: "flag",
            id: "noresponsible",
            title: ASC.Projects.Resources.ProjectsFilterResource.NoResponsible,
            group: ASC.Projects.Resources.ProjectsFilterResource.ByResponsible,
            hashmask: "noresponsible",
            groupby: "userid"
        });

        // Creator
        filters.push({
            type: "person",
            id: "me_tasks_creator",
            title: ASC.Projects.Resources.ProjectsFilterResource.Me,
            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByCreator + ":",
            group: ASC.Projects.Resources.ProjectsFilterResource.ByCreator,
            hashmask: "person/{0}",
            groupby: "creatorid",
            bydefault: { id: currentUserId }
        });
        filters.push({
            type: "person",
            id: "tasks_creator",
            title: ASC.Projects.Resources.ProjectsFilterResource.OtherUsers,
            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByCreator + ":",
            group: ASC.Projects.Resources.ProjectsFilterResource.ByCreator,
            hashmask: "person/{0}",
            groupby: "creatorid"
        });
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
                options: ASC.Projects.Common.getProjectsForFilter(),
                hashmask: "project/{0}",
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
        //Milestones
        var milestones = ASC.Projects.ProjectsAdvansedFilter.getMilestonesForFilter();
        if (milestones.length > 1) {
            filters.push({
                type: "flag",
                id: "mymilestones",
                title: ASC.Projects.Resources.ProjectsFilterResource.MyMilestones,
                group: ASC.Projects.Resources.ProjectsFilterResource.ByMilestone,
                hashmask: "mymilestones",
                groupby: "milestones"
            });
            filters.push({
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
        // Status
        filters.push({
            type: "combobox",
            id: "open",
            title: ASC.Projects.Resources.ProjectsFilterResource.StatusOpenTask,
            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByStatus + ":",
            group: ASC.Projects.Resources.ProjectsFilterResource.ByStatus,
            hashmask: "combobox/{0}",
            groupby: "status",
            options:
                [
                    { value: "open", title: ASC.Projects.Resources.ProjectsFilterResource.StatusOpenTask, def: true },
                    { value: "closed", title: ASC.Projects.Resources.ProjectsFilterResource.StatusClosedTask }
                ]
        });
        filters.push({
            type: "combobox",
            id: "closed",
            title: ASC.Projects.Resources.ProjectsFilterResource.StatusClosedTask,
            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByStatus + ":",
            group: ASC.Projects.Resources.ProjectsFilterResource.ByStatus,
            hashmask: "combobox/{0}",
            groupby: "status",
            options:
                [
                    { value: "open", title: ASC.Projects.Resources.ProjectsFilterResource.StatusOpenTask },
                    { value: "closed", title: ASC.Projects.Resources.ProjectsFilterResource.StatusClosedTask, def: true }
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
        self.colCount = 2;
        if (!currentProjectId && milestones.length > 1) self.colCount = 3;

        self.sorters =
        [
            { id: "deadline", title: ASC.Projects.Resources.ProjectsFilterResource.ByDeadline, sortOrder: "ascending", def: true },
            { id: "priority", title: ASC.Projects.Resources.ProjectsFilterResource.ByPriority, sortOrder: "descending" },
            { id: "create_on", title: ASC.Projects.Resources.ProjectsFilterResource.ByCreateDate, sortOrder: "descending" },
            { id: "start_date", title: ASC.Projects.Resources.ProjectsFilterResource.ByStartDate, sortOrder: "descending" },
            { id: "title", title: ASC.Projects.Resources.ProjectsFilterResource.ByTitle, sortOrder: "ascending" }
        ];
        
        ASC.Projects.ProjectsAdvansedFilter.init(self);
    };

    var showNewTaskPopup = function() {
        jq('.studio-action-panel').hide();
        ASC.Projects.TaskAction.showCreateNewTaskForm();
    };

    var showOrHideListSubtasks = function(taskId, addFlag) {
        ASC.Projects.SubtasksManager.hideSubtaskActionPanel();
        var taskListContainer = jq("#CommonListContainer .taskList");
        var subtasks = jq('.subtasks[taskid=' + taskId + ']');
        var subtasksCount = subtasks.find(".subtask").length;
        if (jq(subtasks).is(":visible")) {
            taskListContainer.find('.task[taskid=' + taskId + ']').removeClass('borderbott');
            subtasks.hide();
            return true;
        };

        if (!subtasksCount && !addFlag) return false;

        taskListContainer.find('.task[taskid=' + taskId + ']').addClass('borderbott');
        if (subtasksCount) {
            separateSubtasks(taskId);
        }
        subtasks.show();
        return true;
    };

    var getData = function () {
        self.showLoader();
        self.currentFilter.Count = ASC.Projects.PageNavigator.entryCountOnPage;
        self.currentFilter.StartIndex = ASC.Projects.PageNavigator.entryCountOnPage * ASC.Projects.PageNavigator.currentPage;

        Teamlab.getPrjTasks({}, { filter: self.currentFilter, success: onGetTasks });
    };

    var getFilteredTaskById = function (taskId) {
        for (var i = 0, max = filteredTasks.length; i < max; i++){
            if (filteredTasks[i].id == taskId) {
                return filteredTasks[i];
            }
        }
    };
    
    var setFilteredTask = function (task) {
        for (var i = 0, max = filteredTasks.length; i < max; i++) {
            if (filteredTasks[i].id == task.id) {
                filteredTasks[i] = task;
            }
        }
    };
    
    var closeTask = function(taskId) {
        jq('.taskProcess').remove();
        jq('.taskList .task[taskid=' + taskId + '] .check').html('');
        jq('.taskList .task[taskid=' + taskId + '] .check').append('<div class="taskProcess"></div>');

        updateTaskStatus({}, taskId, 2);
    };

    var updateTaskStatus = function(params, taskId, status) {
        Teamlab.updatePrjTask(params, taskId, { "status": status }, {
            success: onUpdateTaskStatus, error: function (filter, response) {
                var task = jq('.taskList .task[taskid=' + taskId + ']');
                task.find(".taskProcess").remove();
                task.find(".check div").show();
                ASC.Projects.Common.displayInfoPanel(response[0], true);
            }
        });
    };

    var notifyTaskResponsible = function(params, taskId) {
        Teamlab.notifyPrjTaskResponsible(params, taskId, { success: showRemindTaskPopup });
    };

    var removeTask = function(params, taskId) {
        Teamlab.removePrjTask(params, taskId, { success: onRemoveTask, error: onErrorRemoveTask });
    };

    var changeCountTaskSubtasks = function(taskid, action) {
        var currentCount;
        var text;
        var task = jq(".task[taskid='" + taskid + "']");
        var subtasksCounterContainer = jq(task).find('.subtasksCount');
        var subtasksCounter = jq(subtasksCounterContainer).find('.dottedNumSubtask');

        if (subtasksCounter.length) {
            text = jq.trim(jq(subtasksCounter).text());
            text = text.substr(1, text.length - 1);
        } else {
            text = "";
        }

        if (text == "") {
            currentCount = 0;
        } else {
            currentCount = parseInt(text);
        }

        if (action == "add") {
            currentCount++;
            if (currentCount == 1) {
                jq(subtasksCounterContainer).find('.add').remove();
                subtasksCounter = '<span class="collaps" taskid="' + taskid + '"><span class="dottedNumSubtask">+' + currentCount + '</span></span>';
                jq(subtasksCounterContainer).append(subtasksCounter);
            } else {
                jq(subtasksCounter).text('+' + currentCount);
            }
        }
        else if (action == "delete") {
            currentCount--;
            if (currentCount != 0) {
                jq(subtasksCounter).text('+' + currentCount);
            }
            else {
                jq(subtasksCounter).remove();
                var hoverText = jq(subtasksCounterContainer).attr('data');
                jq(subtasksCounterContainer).append('<span class="add" taskid="' + taskid + '">+ ' + hoverText + '</span>');
                jq(task).find('.subtasks').hide();
            }
        }
    };

    var updateMilestonesListForMovePanel = function (milestones) {
        milestones = milestones.sort(ASC.Projects.Common.milestoneSort);
        jq('#moveTaskPanel .milestonesList .ms').remove();
        jq.tmpl("projects_milestoneForMoveTaskPanelTmpl", milestones).prependTo("#moveTaskPanel .milestonesList");
    };

    var getMilestonesForMovePanel = function(params, projectId) {
        Teamlab.getPrjMilestones(params, null, { filter: { status: 'open', projectId: projectId }, success: onGetMilestonesForMovePanel });
    };

    var compareDates = function(data) {
        var currentDate = new Date();
        if (currentDate > data) {
            return true;
        }
        else return false;
    };

    var emptyScreenList = function(isItems) {
        var emptyScreen = ASC.Projects.ProjectsAdvansedFilter.baseFilter ? '#emptyListTask' : '#tasksEmptyScreenForFilter';

        if (isItems === undefined) {
            var tasks = jq('.taskList .task');
            if (tasks.length != 0) {
                isItems = true;
            }
        }

        if (isItems) {
            self.$noContentBlock.hide();
            ASC.Projects.ProjectsAdvansedFilter.show();
            ASC.Projects.PageNavigator.show();
            jq(".taskList").show();
        } else {
            if (filterTaskCount == undefined || filterTaskCount == 0) {
                jq(emptyScreen).show();
                ASC.Projects.PageNavigator.hide();
                if (emptyScreen == '#emptyListTask') {
                    ASC.Projects.ProjectsAdvansedFilter.hide();
                    jq('#tasksEmptyScreenForFilter').hide();
                }
            }
            else {
                if (ASC.Projects.PageNavigator.currentPage > 0) {
                    ASC.Projects.PageNavigator.setMaxPage(filterTaskCount);
                    getData();
                }
            }
        }
    };

    var separateSubtasks = function(taskid) {
        var subtasksCont = jq('.subtasks[taskid="' + taskid + '"]');
        var closedSubtasks = jq(subtasksCont).find('.subtask.closed');
        jq(jq(subtasksCont).find('.st_separater')).after(closedSubtasks);
        subtasksCont.show();
    };

    // show popup methods

    var popupWindow = function(taskId) {
        self.showCommonPopup("projects_closedTaskQuestion", 480, 200, 0);
        jq('.commonPopupContent .end').attr('taskid', taskId);
        PopupKeyUpActionProvider.EnterAction = "jq('.commonPopupContent .end').click();";
    };

    var showRemindTaskPopup = function() {
        ASC.Projects.Common.displayInfoPanel(ASC.Projects.Resources.TasksResource.MessageSend);
    };

    var showQuestionWindowTaskRemove = function(taskId) {
        self.showCommonPopup("projects_taskRemoveWarning", 400, 200);
        PopupKeyUpActionProvider.EnterAction = "jq('.commonPopupContent .remove').click();";
        self.$commonPopupContainer.attr('taskId', taskId);
    };

    var showMoveToMilestonePanel = function() {
        var taskId = taskActionPanel.attr('objid');
        jq('#moveTaskPanel').attr('taskid', taskId);

        jq('#moveTaskTitles').text(jq(".taskList .task[taskid=" + taskId + "] .taskName a").text());
        var milestoneid = jq('.taskList .task[taskid=' + taskId + ']').attr('milestoneid');

        if (typeof milestoneid != 'undefined') {
            jq('#moveTaskPanel input#ms_' + milestoneid).prop('checked', true);
        } else {
            jq('#moveTaskPanel input#ms_0').prop('checked', true);
        }

        StudioBlockUIManager.blockUI(jq('#moveTaskPanel'), 550, 300, 0);
        PopupKeyUpActionProvider.EnterAction = "jq('#moveTaskPanel .blue').click();";
    };

    var showActionsPanel = function (panelId, coord) {
        var self = jq(this),
            menuClick = false;
        if (self.is(".entity-menu")) {
            menuClick = true;
        } else if (panelId == "taskActionPanel") {
            self = self.find(".entity-menu");
        }
        var objid = '',
            objidAttr = '';
        var x, y;
        if (typeof self.attr('projectid') != 'undefined') {
            taskActionPanel.find('#ta_move').attr('projectid', self.attr('projectid'));
            taskActionPanel.find('#ta_time').attr('projectid', self.attr('projectid'));
        }
        if (typeof self.attr('userid') != 'undefined') {
            taskActionPanel.find('#ta_time').attr('userid', self.attr('userid'));
        }
        if (panelId == 'taskActionPanel') {
            objid = self.attr('taskid');
        }
        if (objid.length) {
            objidAttr = '[objid=' + objid + ']';
        }
        if (jq('#' + panelId + ':visible' + objidAttr).length && panelId != 'taskDescrPanel' && panelId != 'subTaskDescrPanel') {
            jq('body').off('click');
            jq('.studio-action-panel, .filter-list').hide();
            jq('#' + statusListObject.listId).hide();
            jq('.changeStatusCombobox').removeClass('selected');
            jq(".entity-menu.active").removeClass("active");
        } else {
            jq('.studio-action-panel, .filter-list').hide();
            jq('#' + statusListObject.listId).hide();
            jq('.changeStatusCombobox').removeClass('selected');

            //jq('#' + panelId).show();
            //jq('#' + panelId).hide();
            // remove magic numbers
            if (panelId == 'taskDescrPanel') {
                x = self.offset().left + 10;
                y = self.offset().top + 20;
                jq('#' + panelId).attr('objid', jq(this).attr('taskid'));
            } else if (panelId == 'othersPanel') {
                x = self.offset().left - 133;
                y = self.offset().top + 26;
            } else {
                x = self.offset().left - 110;
                y = self.offset().top + 20;
                jq('#' + panelId).attr('objid', objid);
                taskActionPanel.find('.dropdown-item').show();

                var task = jq('.task[taskid=' + objid + ']');

                var taskUser = jq(task).find(".user");
                if (task.length) { //if it`s tasks menu        
                    if (jq(task).hasClass('closed')) {
                        taskActionPanel.find('.dropdown-item').hide();
                        if (jq(task).find(".entity-menu").data("cancreatetimespend")) {
                            taskActionPanel.find('#ta_time').show();
                        }
                        taskActionPanel.find('#ta_remove').show();
                        
                    } else if (taskUser.length == 1) {
                        if (jq(taskUser).hasClass("not") || jq(taskUser).attr('data-userId') == currentUserId) {
                            taskActionPanel.find('#ta_mesres').hide();
                        }
                        if (!jq(taskUser).hasClass("not")) {
                            taskActionPanel.find('#ta_accept').hide();
                        }
                    } else {
                        taskActionPanel.find('#ta_mesres').show();
                    }
                }

                if (jq('.task[taskid=' + objid + ']').length) {
                    if (self.attr('canDelete') != "true") {
                        taskActionPanel.find('#ta_remove').hide();

                        if (self.attr('canEdit') == "false" || Teamlab.profile.isVisitor) {
                            taskActionPanel.find('#ta_edit').hide();
                            taskActionPanel.find('#ta_move').hide();
                            taskActionPanel.find('#ta_mesres').hide();
                            if (self.attr("data-cancreatesubtask") == "false")
                                taskActionPanel.find("#ta_subtask").hide();
                        }
                    }
                }
            }

            if (typeof y == 'undefined')
                y = self.offset().top + 29;
            
            if (coord) {
                x = coord.x - jq('#' + panelId).outerWidth();
                y = coord.y;
            }

            var panelHeight = jq('#' + panelId).innerHeight(),
                w = jq(window),
                scrScrollTop = w.scrollTop(),
                scrHeight = w.height(),
                correctionY =
                    panelHeight > y
                    ? 0
                    : (scrHeight + scrScrollTop - y > panelHeight ? 0 : panelHeight);

            y = y - correctionY;

            if (menuClick) {
                jq(".entity-menu.active").removeClass("active");
                self.addClass("active");

                y = y - (correctionY == 0 ? 0 : self.outerHeight() + 2);
            }

            jq('#' + panelId).css({ left: x, top: y }).show();

            jq('body')
                .off("click.tasksShowActionsPanel")
                .on("click.tasksShowActionsPanel", function (event) {

                var elt = (event.target) ? event.target : event.srcElement,
                    isHide = true;
                if (jq(elt).is('[id="' + panelId + '"]') || (elt.id == this.id && this.id.length) || jq(elt).is('.entity-menu') || jq(elt).is('.other') || jq(elt).parents('#taskDescrPanel').length) {
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
                    hideTaskActionPanel();
                }
            });
        }
    };

    var hideTaskActionPanel = function () {
        jq('.studio-action-panel').hide();
        jq('.taskList .task').removeClass('menuopen');
        jq(".entity-menu.active").removeClass("active");
    };

    var openedCount = function(items) {
        var c = 0;
        for (var i = 0; i < items.length; i++) {
            if (items[i].status != 2) c++;
        }
        return c;
    };

    var hideDescriptionPanel = function() {
        setTimeout(function() {
            if (!overTaskDescriptionPanel) taskDescribePanel.hide(100);
        }, 200);
    };

    var showStatusListContainer = function(status) {
        selectedStatusCombobox.addClass('selected');
        jq('.studio-action-panel, .filter-list').hide();
        jq('.task.menuopen').removeClass('menuopen');
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

    var hideStatusListContainer = function() {
        if (statusListContainer.is(':visible')) {
            selectedStatusCombobox.removeClass('selected');
        }
        statusListContainer.hide();
    };

    var unbindListEvents = function () {
        if (!isInit) return;
        $taskListContainer.unbind();
        taskActionPanel.unbind();
        self.$commonListContainer.unbind();
        self.$commonPopupContainer.unbind();
        taskDescribePanel.unbind();
        statusListContainer.unbind();
    };


    //api callback
    var onGetTasks = function (params, tasks) {
        self.clearTables();

        filteredTasks = tasks;
        self.$commonListContainer.height('auto');
        jq('#CommonListContainer .taskSaving').hide();
        clearTimeout(taskDescriptionTimeout);
        overTaskDescriptionPanel = false;
        hideDescriptionPanel();

        jq('#CommonListContainer .taskList').height('auto');
        if (tasks.length) {
            jq.tmpl("projects_taskListItemTmpl", tasks).appendTo('.taskList');
            jq(".taskList").show();
        }

        if (!currentProjectId) {
            jq('#CommonListContainer .choose.project span').html(jq('#CommonListContainer .choose.project').attr('choose'));
            jq('#CommonListContainer .choose.project').attr('value', '');
        }

        self.hideLoader();

        filterTaskCount = params.__total != undefined ? params.__total : 0;
        ASC.Projects.PageNavigator.update(filterTaskCount);
        emptyScreenList(tasks.length);
    };

    var onAddTask = function (params, task) {
        filterTaskCount++;
        filteredTasks.push(task);
        self.$noContentBlock.hide();
        jq.tmpl("projects_taskListItemTmpl", task).prependTo(".taskList");
        jq('#CommonListContainer .taskSaving').hide();
        jq('.taskList .task:first').yellowFade();
        ASC.Projects.PageNavigator.update(filterTaskCount);
        emptyScreenList(true);
    };

    var onUpdateTask = function (params, task) {
        var taskId = task.id;
        jq('.taskList .task[taskid=' + taskId + ']:first').remove();
        jq.tmpl("projects_taskListItemTmpl", task).insertBefore('.taskList .subtasks[taskid=' + taskId + ']');
        jq('.taskList .subtasks[taskid=' + taskId + ']:first').remove();
        jq('.taskList .task[taskid=' + taskId + ']').yellowFade();
        if (task.subtasks.length && jq('.taskList .subtasks:visible[taskid=' + taskId + ']').length) {
            jq('.taskList .task[taskid=' + taskId + ']').addClass('borderbott');
        }
        setFilteredTask(task);
        jq.unblockUI();
        ASC.Projects.Common.displayInfoPanel(ASC.Projects.Resources.ProjectsJSResource.TaskUpdated);
    };
    
    var onRemoveTask = function (params, task) {
        var taskId = task.id;
        jq('.taskList .task[taskid=' + taskId + ']').remove();
        jq('.taskList .subtasks[taskid=' + taskId + ']').remove();

        if (currentProjectId == task.projectId && task.status != 2) {
            ASC.Projects.projectNavPanel.changeModuleItemsCount(ASC.Projects.projectNavPanel.projectModulesNames.tasks, "delete");
        }

        filterTaskCount--;
        ASC.Projects.PageNavigator.update(filterTaskCount);
        if (typeof task != 'undefined') {
            emptyScreenList(task.length);
        } else {
            emptyScreenList(0);
        }
        jq('.taskList .task[taskid=' + taskId + ']').html('<div class="taskProcess"></div>');
        self.$commonPopupContainer.removeAttr('taskId');
        jq.unblockUI();
        ASC.Projects.Common.displayInfoPanel(ASC.Projects.Resources.ProjectsJSResource.TaskRemoved);
        
        if (filterTaskCount == 0) {
            jq("#emptyListTimers .addFirstElement").addClass("display-none");
        }
    };

    var onErrorRemoveTask = function (param, error) {
        var removePopupErrorBox = self.$commonPopupContainer.find(".errorBox");
        removePopupErrorBox.text(error[0]);
        removePopupErrorBox.removeClass("display-none");
        self.$commonPopupContainer.find(".middle-button-container").css('marginTop', '8px');
        setTimeout(function () {
            removePopupErrorBox.addClass("display-none");
            self.$commonPopupContainer.find(".middle-button-container").css('marginTop', '32px');
        }, 3000);
    };

    var onAddSubtask = function (params, subtask) {
        changeCountTaskSubtasks(subtask.taskid, 'add');
    };

    var onRemoveSubtask = function (params, subtask) {
        var taskId = params.taskId;
        var subtasksCont = jq('.taskList .subtasks[taskid=' + taskId + ']');
        if (subtask.status == 1) {
            changeCountTaskSubtasks(taskId, 'delete');
        }
        if (!subtasksCont.find('.subtask').length) {
            showOrHideListSubtasks(taskId);
        }
    };

    var onUpdateSubtaskStatus = function (params, subtask) {
        if (subtask.status == 2) {
            changeCountTaskSubtasks(params.taskId, 'delete');
        } else {
            changeCountTaskSubtasks(params.taskId, 'add');
        }
    };
    
    var onUpdateTaskStatus = function (params, task) {
        var status = task.status;
        var taskId = task.id;
        
        if (currentProjectId == task.projectId) {
            ASC.Projects.projectNavPanel.changeModuleItemsCount(ASC.Projects.projectNavPanel.projectModulesNames.tasks, task.status == 2 ? "delete" : "add");
        }
        
        jq('.taskList .subtasks[taskid=' + taskId + ']:first').remove();
        jq('.taskList .task[taskid=' + taskId + ']:first').replaceWith(jq.tmpl("projects_taskListItemTmpl", task));
        
        if (status == 1) {
            setTimeout(function () { jq('.taskList .task[taskid=' + taskId + ']').yellowFade(); }, 0);
        } else {
            jq('.taskList .subtasks[taskid=' + taskId + ']').hide();
            setTimeout(function () { jq('.taskList .task.closed[taskid=' + taskId + ']').yellowFade(); }, 0);
        }
    };
    
    var onGetMilestonesForMovePanel = function (params, milestones) {
        updateMilestonesListForMovePanel(milestones);
        showMoveToMilestonePanel();
    };

    var onAddMilestone = function() {
        updateMilestonesListForMovePanel(ASC.Projects.Master.Milestones);
        jq.unblockUI();
    };

    return jq.extend({
        init: init,
        getData: getData,
        openedCount: openedCount,
        showActionsPanel: showActionsPanel,
        compareDates: compareDates,
        onAddTask: onAddTask,
        onUpdateTask: onUpdateTask,
        onRemoveTask: onRemoveTask,
        onAddMilestone: onAddMilestone,
        unbindListEvents: unbindListEvents,
        basePath: 'sortBy=deadline&sortOrder=ascending'
    }, ASC.Projects.Base);
    
})(jQuery);
