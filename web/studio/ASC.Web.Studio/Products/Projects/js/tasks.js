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


ASC.Projects.TasksManager = (function () {
    var baseObject = ASC.Projects,
        resources = baseObject.Resources,
        tasksResource = resources.TasksResource,
        projectsJsResource = resources.ProjectsJSResource,
        filter = baseObject.ProjectsAdvansedFilter,
        common = baseObject.Common,
        subtaskManager = baseObject.SubtasksManager,
        master = baseObject.Master;

    var isInit = false,
        currentUserId,
        currentProjectId,
        filteredTasks = [],
        projectParticipants = undefined,
        filterTaskCount = 0,
        // cache DOM elements
        $taskListContainer = jq('.taskList'),
        $moveTaskPanel,
        $othersListPopup,
        currentProject;
    
    var self, teamlab;

    var clickEventName = "click",
        projectsTaskListItemTmplName = "projects_taskListItemTmpl",
        taskidAttr = 'taskid',
        divTaskProcessList = '<div class="taskProcess"></div>';

    function init() {
        teamlab = Teamlab;
        currentUserId = teamlab.profile.id;
        currentProjectId = jq.getURLParam('prjID');

        if (isInit === false) {
            self = this;
            isInit = true;

            function canCreateTask(prj) {
                return prj.canCreateTask && prj.status === 0;
            }

            self.showOrHideData = self.showOrHideData.bind(self, {
                $container: $taskListContainer,
                tmplName: projectsTaskListItemTmplName,
                baseEmptyScreen: {
                    img: "tasks",
                    header: tasksResource.NoTasksCreated,
                    description: jq.format(tasksResource.TasksHelpTheManage, tasksResource.DescrEmptyListTaskFilter),
                    button: {
                        title: tasksResource.AddFirstTask,
                        onclick: showNewTaskPopup,
                        canCreate: function () {
                            return currentProjectId ?
                                canCreateTask(currentProject) :
                                master.Projects.some(canCreateTask);
                        }
                    }
                },
                filterEmptyScreen: {
                    header: tasksResource.NoTasks, 
                    description: tasksResource.DescrEmptyListTaskFilter
                }
            });

            self.getData = self.getData.bind(self, teamlab.getPrjTasks, onGetTasks);
            initActionPanels();
        }
        var eventConstructor = ASC.Projects.Event,
            events = teamlab.events;
        self.baseInit(
            {
                moduleTitle: projectsJsResource.TasksModule,
                elementNotFoundError: projectsJsResource.TaskNotFound
            },
            {
                pagination: "tasksKeyForPagination"
            },
            {
                handler: changeStatusHandler,
                statuses: [
                    { cssClass: "open", text: tasksResource.Open },
                    { cssClass: "closed", text: tasksResource.Closed }
                ]
            },
            showEntityMenu,
            [
                eventConstructor(events.addPrjTask, onAddTask),
                eventConstructor(events.updatePrjTask, onUpdateTask),
                eventConstructor(events.updatePrjTaskStatus, onUpdateTaskStatus),
                eventConstructor(events.removePrjTask, onRemoveTask),
                eventConstructor(events.addSubtask, onAddSubtask),
                eventConstructor(events.removeSubtask, onRemoveSubtask),
                eventConstructor(events.updateSubtask, onUpdateSubtaskStatus),
                eventConstructor(events.getPrjProject, function (params, project) { currentProject = project; })
            ]);

        subtaskManager.init();

        // waiting data from api
        filter.createAdvansedFilterForTasks(self);

        if (currentProjectId) {
            projectParticipants = master.Team;
        }
        
        $othersListPopup.on(clickEventName, '.user', function () {
            filter.addUser('tasks_responsible', jq(this).attr('userId'), ['noresponsible']);
        });

        jq('.addTask').on(clickEventName, function () {
            showNewTaskPopup();
            return false;
        });

        $taskListContainer.on(clickEventName, '.task .user', function (event) {
            var taskid = jq(this).parent().attr(taskidAttr);
            var task = getFilteredTaskById(taskid);
            var userid = task.responsible ? task.responsible.id : null;

            if (userid == null) {
                filter.add('noresponsible', true, ['tasks_responsible', 'group']);
            } else {
                filter.addUser('tasks_responsible', userid, ['noresponsible', 'group']);
            }

            event.stopPropagation();
        });

        $taskListContainer.on(clickEventName, '.task .other', function (event) {
            jq(".studio-action-panel").hide();
            $othersListPopup.html(jq.tmpl("projects_taskListResponsibles", getFilteredTaskById(jq(event.target).parents(".task").attr(taskidAttr))));
            showActionsPanel.call(this);
            event.stopPropagation();
        });

        $taskListContainer.on('mouseenter', '.task .taskName a', function(event) {
            var $targetObject = jq(event.target);
            var task = getFilteredTaskById($targetObject.parents(".task").attr(taskidAttr));
            self.showDescPanel(task, $targetObject, "tasks.aspx?prjID=" + task.projectOwner.id + "&id=" + task.id);
        });

        $taskListContainer.on('mouseleave', '.task .taskName a', function () {
            self.hideDescrPanel(false);
        });

        $taskListContainer.on(clickEventName, '.task', function (event) {
            var $elt = jq((event.target) ? event.target : event.srcElement);

            if ($elt.is('a')) {
                return;
            }

            if ($elt.is(".changeStatusCombobox.canEdit") ||
                $elt.parent().is(".changeStatusCombobox.canEdit") ||
                $elt.is(".entity-menu") ||
                $elt.parent().is(".entity-menu") ||
                $elt.is(".noSubtasks:not(.canedit)")) {
                return;
            }


            subtaskManager.hideSubtaskFields();

            var taskid = jq(this).attr(taskidAttr), 
                task = getFilteredTaskById(taskid),
                emptyOrAllClosed = !task.subtasks || task.subtasks.every(function (item) {
                    return item.status === 2;
                });

            showOrHideListSubtasks(task, task.status === 1 && emptyOrAllClosed);
        });
    };

    function showEntityMenu(selectedActionCombobox) {
        subtaskManager.hideSubtaskFields();

        if (selectedActionCombobox.is(".subtask")) {
            return subtaskManager.showEntityMenu(selectedActionCombobox);
        }

        var taskid = selectedActionCombobox.attr(taskidAttr),
            task = getFilteredTaskById(taskid);

        var menuItems = [],
            ActionMenuItem = ASC.Projects.ActionMenuItem;

        if (task.status !== 2) {
            if (task.canEdit) {
                menuItems.push(new ActionMenuItem("ta_edit", tasksResource.Edit, taEditHandler.bind(null, task)));
                menuItems.push(new ActionMenuItem("ta_move", tasksResource.MoveToMilestone, taMoveHandler.bind(null, task)));
                menuItems.push(new ActionMenuItem("ta_mesres", tasksResource.MessageResponsible, taMesresHandler.bind(null, taskid)));
            }

            if (!task.responsible) {
                menuItems.push(new ActionMenuItem("ta_accept", tasksResource.AcceptSubtask, taAcceptHandler.bind(null, task)));
            }

            if (task.canCreateSubtask)
                menuItems.push(new ActionMenuItem("ta_subtask", tasksResource.AddSubtask, taSubtaskHandler.bind(null, taskid)));

            var project = common.getProjectById(task.projectId);
            if (project && project.canCreateTask) {
                menuItems.push(new ActionMenuItem("ta_copy", resources.CommonResource.Copy, taCopyHandler.bind(null, task)));
            }
        }

        if (task.canCreateTimeSpend) {
            menuItems.push(new ActionMenuItem("ta_time", tasksResource.TrackTime, taTimeHandler.bind(null, task)));
        }

        if (task.canDelete) {
            menuItems.push(new ActionMenuItem("ta_remove", resources.CommonResource.Delete, taRemoveHandler.bind(null, taskid)));
        }

        return { menuItems: menuItems };
    }

    function taAcceptHandler(task) {
        var data = {
            title: jq.trim(task.title),
            priority: task.priority,
            responsibles: [currentUserId]
        };

        var deadline = task.deadline;
        if (deadline) {
            data.deadline = new Date(deadline);
            data.deadline.setHours(0);
            data.deadline.setMinutes(0);
            data.deadline = teamlab.serializeTimestamp(data.deadline);
        }
        var description = task.description;
        if (description) {
            data.description = description;
        }

        if (typeof task.milestone != "undefined" && task.milestone != null) {
            data.milestoneid = task.milestone.id;
        }

        teamlab.updatePrjTask({}, task.id, data);
        return false;
    };

    function taEditHandler(task) {
        baseObject.TaskAction.showUpdateTaskForm(task);
        return false;
    };

    function taSubtaskHandler(taskid) {
        subtaskManager.hideSubtaskFields();
        var $subtaskCont = getSubTasksItem(taskid);

        if (!$subtaskCont.is(':visible')) {
            separateSubtasks($subtaskCont);
        }

        subtaskManager.addFirstSubtask($subtaskCont.find(".quickAddSubTaskLink"));
        return false;
    };

    function taMoveHandler(task) {
        var milestones = master.Milestones.filter(function (item) {
            return item.status === 0 && item.projectId == task.projectId;
        });
        showMoveToMilestonePanel(task.id, milestones);
        return false;
    };

    function taRemoveHandler(taskid) {
        self.showCommonPopup("taskRemoveWarning", function () {
            teamlab.removePrjTask({ 'taskId': taskid }, taskid);
        });
        return false;
    };
    function taCopyHandler(task) {
        baseObject.TaskAction.showCopyTaskForm(task);
        return false;
    };

    function taMesresHandler(taskid) {
        notifyTaskResponsible({}, taskid);
        return false;
    };

    function taTimeHandler(task) {
        common.showTimer(task.projectId, task.id, task.responsible || currentUserId);
        return false;
    };

    function initActionPanels() {
        common.createActionPanel(self.$commonListContainer, "othersPanel", { menuItems: [] });
        jq("#othersPanel .dropdown-content").attr("id", "othersListPopup");

        $othersListPopup = jq("#othersListPopup");
        $moveTaskPanel = jq("#moveTaskPanel");
        
        $moveTaskPanel.on(clickEventName, ".blue", function () {
            var data = {},
                taskId = parseInt($moveTaskPanel.attr(taskidAttr), 10),
                task = getFilteredTaskById(taskId);

            data.newMilestoneID = $moveTaskPanel.find(".milestonesList input:checked").attr('value');

            if ((task.milestoneId || 0) == parseInt(data.newMilestoneID, 10)) {
                jq.unblockUI();
                return false;
            }

            if (task.links && task.links.length) {
                ASC.Projects.Base.showCommonPopup("taskLinksRemoveWarning",
                    function () {
                        var links = task.links;
                        for (var j = 0; j < links.length; ++j) {
                            var dataLink = { dependenceTaskId: links[j].dependenceTaskId, parentTaskId: links[j].parentTaskId };
                            teamlab.removePrjTaskLink({}, links[j].dependenceTaskId, dataLink, { success: function () { } });
                        }
                        teamlab.updatePrjTask({}, taskId, data);
                    },
                    function () {
                        jq.unblockUI();
                        StudioBlockUIManager.blockUI($moveTaskPanel, 550, 300, 0);
                    });
            } else {
                teamlab.updatePrjTask({}, taskId, data);
                jq.unblockUI();
            }
        })
        .on(clickEventName, ".gray", function () {
            jq.unblockUI();
            return false;
        });
    };

    function changeStatusHandler(id, status) {
        if (status == 'closed') {
            var task = getFilteredTaskById(id);
            var openedSubtasks = task.subtasks.some(function(item) {
                    return item.status === 1;
                });
            if (openedSubtasks) {
                popupWindow(id);
            } else {
                closeTask(id);
            }
        } else {
            var $task = getTaskItem(id);
            $task.find(".check div").hide();
            $task.find(".check").append(divTaskProcessList);
            updateTaskStatus({}, id, 1);
        }
    };

    function showNewTaskPopup() {
        baseObject.TaskAction.showCreateNewTaskForm();
    };

    function showOrHideListSubtasks(task, addFlag) {
        var $subtasks = getSubTasksItem(task.id);

        if (addFlag) {
            subtaskManager.addFirstSubtask($subtasks.find(".quickAddSubTaskLink"));
        }

        var subtasksCount = task.subtasks.length;
        if ($subtasks.is(":visible")) {
            $subtasks.hide();
            return true;
        };

        if (!subtasksCount && !addFlag) return false;

        if (subtasksCount) {
            separateSubtasks($subtasks);
        }
        $subtasks.show();
        return true;
    };

    function getFilteredTaskById(taskId) {
        for (var i = 0, max = filteredTasks.length; i < max; i++){
            if (filteredTasks[i].id == taskId) {
                return filteredTasks[i];
            }
        }
    };
    
    function setFilteredTask(task) {
        for (var i = 0, max = filteredTasks.length; i < max; i++) {
            if (filteredTasks[i].id == task.id) {
                filteredTasks[i] = task;
                break;
            }
        }
    };

    function updateFilteredTaskSubtasks(task, subtask) {
        for (var i = 0, max = task.subtasks.length; i < max; i++) {
            if (task.subtasks[i].id === subtask.id) {
                task.subtasks[i] = subtask;
                break;
            }
        }
    };
    
    function closeTask(taskId) {
        jq('.taskProcess').remove();
        getTaskItem(taskId).find(".check").html(divTaskProcessList);

        updateTaskStatus({}, taskId, 2);
    };

    function updateTaskStatus(params, taskId, status) {
        teamlab.updatePrjTask(params, taskId, { "status": status }, {
            error: function(filter, response) {
                var task = getTaskItem(taskId);
                task.find(".taskProcess").remove();
                task.find(".check div").show();
                common.displayInfoPanel(response[0], true);
            }
        });
    };

    function notifyTaskResponsible(params, taskId) {
        teamlab.notifyPrjTaskResponsible(params, taskId, { success: common.displayInfoPanel.bind(null, tasksResource.MessageSend) });
    };


    function changeCountTaskSubtasks(task) {
        var taskid = task.id,
            $task = getTaskItem(taskid),
            $subtasksCounterContainer = $task.find('.subtasksCount');

        $subtasksCounterContainer.html(jq.tmpl('projects_numSubtasksTmpl', task));
    };

    function compareDates(data) {
        return new Date() > data;
    };

    function separateSubtasks($subtasksCont) {
        var closedSubtasks = $subtasksCont.find('.subtask.closed');
        $subtasksCont.find('.st_separater').after(closedSubtasks);
        $subtasksCont.show();
    };

    // show popup methods

    function popupWindow(taskId) {
        self.showCommonPopup("closedTaskQuestion", function () {
            closeTask(taskId);
            jq.unblockUI();
        });
    };

    function showMoveToMilestonePanel(taskId, milestones) {
        $moveTaskPanel.attr(taskidAttr, taskId);
        $moveTaskPanel.html(jq.tmpl("common_containerTmpl",
        {
            options: {
                PopupContainerCssClass: "popupContainerClass",
                OnCancelButtonClick: "PopupKeyUpActionProvider.CloseDialog();",
                IsPopup: true
            },
            header: {
                data: { title: tasksResource.MoveTaskToAnotherMilestone },
                title: "projects_common_popup_header"
            },
            body: {
                title: "projects_move_task_panel",
                data: {
                    milestones: milestones.sort(common.milestoneSort)
                }
            }
        }));

        jq('#moveTaskTitles').text(getTaskItem(taskId).find(".taskName a").text());
        var milestoneid = getFilteredTaskById(taskId).milestoneId || 0;
        $moveTaskPanel.find("input#ms_" + milestoneid).prop('checked', true);

        StudioBlockUIManager.blockUI($moveTaskPanel, 550, 300, 0);
        PopupKeyUpActionProvider.EnterAction = "$moveTaskPanel.find('.blue').click();";
    };

    function showActionsPanel() {
        var $this = jq(this),
            offset = $this.offset(),
            x = offset.left,
            y = offset.top + $this.outerHeight(),
            $panel = jq('#othersPanel');

        var panelHeight = $panel.innerHeight(),
            w = jq(window),
            scrScrollTop = w.scrollTop(),
            scrHeight = w.height();

        if (panelHeight < y && scrHeight + scrScrollTop - panelHeight <= y) {
            y = y - panelHeight;
        }

        $panel.css({ left: x, top: y }).show();

        jq('body').off("click.body2").on("click.body2", function () {
            setTimeout(function () {
                if ($panel) {
                    $panel.hide();
                }
            }, 1);
        });
    };

    function openedCount(items) {
        var c = 0;
        for (var i = 0; i < items.length; i++) {
            if (items[i].status != 2) c++;
        }
        return c;
    };

    function unbindListEvents() {
        if (!isInit) return;

        self.unbindEvents();
        $taskListContainer.unbind();
    };


    //api callback
    function onGetTasks(params, tasks) {
        filteredTasks = tasks;
        self.$commonListContainer.height('auto');
        $taskListContainer.height('auto');

        filterTaskCount = params.__total != undefined ? params.__total : 0;

        self.showOrHideData(tasks, filterTaskCount);
        subtaskManager.setTasks(tasks);

        $taskListContainer.off("click.taskItem").on("click.taskItem", ".taskName a", function () {
            var $self = jq(this);
            var href = $self.attr("href");
            history.pushState({ href: href }, { href: href }, href);
            ASC.Controls.AnchorController.historyCheck();

            var prjid = jq.getURLParam("prjID");

            Teamlab.getPrjTeam({}, prjid, function (params, team) {
                ASC.Projects.Master.Team = team;
                ASC.Projects.Base.clearTables();
                jq("#filterContainer").hide();
                ASC.Projects.Common.baseInit();
            });


            return false;
        });
    };

    function onAddTask(params, task) {
        currentProjectId = jq.getURLParam("prjId");

        if (currentProjectId && currentProjectId != task.projectId || jq.getURLParam("id") !== null) return;

        filterTaskCount++;
        filteredTasks.unshift(task);

        self.showOrHideData(filteredTasks, filterTaskCount);
        $taskListContainer.find(".task:first").yellowFade();
    };

    function onUpdateTask(params, task) {
        var taskId = task.id;
        getSubTasksItem(taskId).remove();
        getTaskItem(taskId).replaceWith(jq.tmpl(projectsTaskListItemTmplName, task));
        getTaskItem(taskId).yellowFade();
        setFilteredTask(task);
        jq.unblockUI();
        common.displayInfoPanel(projectsJsResource.TaskUpdated);
    };
    
    function onRemoveTask(params, task) {
        var taskId = task.id;
        getTaskItem(taskId).remove();
        getSubTasksItem(taskId).remove();

        filterTaskCount--;
        filteredTasks = filteredTasks.filter(function(t) { return t.id !== taskId });

        self.showOrHideData(filteredTasks, filterTaskCount);

        getTaskItem(taskId).html(divTaskProcessList);
        jq.unblockUI();
        common.displayInfoPanel(projectsJsResource.TaskRemoved);

        var project = common.getProjectById(task.projectOwner.id);
        if (project) {
            project.taskCountTotal--;
        }
        common.changeTaskCountInProjectsCache(task, 2);
    };

    function onAddSubtask(params, subtask) {
        var task = getFilteredTaskById(subtask.taskid);
        task.subtasks.push(subtask);

        changeCountTaskSubtasks(task);
    };

    function onRemoveSubtask(params, subtask) {
        var taskId = subtask.taskid,
            task = getFilteredTaskById(taskId);
        task.subtasks = task.subtasks.filter(function (t) { return t.id !== subtask.id });

        if (subtask.status == 1) {
            changeCountTaskSubtasks(task);
        }
        if (!getSubTasksItem(taskId).find('.subtask').length) {
            showOrHideListSubtasks(task);
        }
    };

    function onUpdateSubtaskStatus(params, subtask) {
        var task = getFilteredTaskById(subtask.taskid);
        updateFilteredTaskSubtasks(task, subtask);
        changeCountTaskSubtasks(task);
    };
    
    function onUpdateTaskStatus(params, task) {
        common.changeTaskCountInProjectsCache(task, 1);
        onUpdateTask(params, task);
        setTimeout(function () { getTaskItem(task.Id).yellowFade(); }, 0);
    };

    function getTaskItem(taskid, first) {
        return $taskListContainer.find(jq.format(".task[{0}={1}]{2}", taskidAttr, taskid, first ? ":first" : ""));
    }
    function getSubTasksItem(taskid, first) {
        return $taskListContainer.find(jq.format(".subtasks[{0}={1}]{2}", taskidAttr, taskid, first ? ":first" : ""));
    }

    return jq.extend({
        init: init,
        openedCount: openedCount,
        compareDates: compareDates,
        unbindListEvents: unbindListEvents,
        basePath: 'sortBy=deadline&sortOrder=ascending'
    }, ASC.Projects.Base);
    
})(jQuery);
