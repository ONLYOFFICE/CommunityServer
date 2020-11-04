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


ASC.Projects.TasksManager = (function () {
    var baseObject = ASC.Projects,
        resources = baseObject.Resources,
        tasksResource = resources.TasksResource,
        projectsJsResource = resources.ProjectsJSResource,
        filter = baseObject.ProjectsAdvansedFilter,
        common = baseObject.Common,
        subtaskManager = baseObject.SubtasksManager,
        master = baseObject.Master,
        groupActionPanel = baseObject.GroupActionPanel,
        statusList = baseObject.StatusList;

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
        currentProject,
        loadingBanner;
    
    var self, teamlab;

    var clickEventName = "click",
        projectsTaskListItemTmplName = "projects_taskListItemTmpl",
        taskidAttr = 'taskid',
        divTaskProcessList = '<div class="taskProcess"></div>';

    var showMoveToMilestonePanelTaskItems, statuses = [];

    function init() {
        teamlab = Teamlab;

        if (isInit === false) {
            self = this;

            common.initCustomStatuses(function() {
                if (master.customStatuses.length) {
                    var styleNode = document.createElement("style");
                    var html = "";
                    for (var i = 0; i < master.customStatuses.length; i++) {
                        var status = master.customStatuses[i];
                        var cssClass = "custom-task-status-" + status.id;
                        html += jq.format("#CommonListContainer .{0} { background: url('data:{1};base64,{2}') }",
                            cssClass,
                            status.imageType,
                            status.image);
                        statuses.push({
                            cssClass: cssClass,
                            text: status.title,
                            id: status.id,
                            statusType: status.statusType,
                            isDefault: status.isDefault,
                            available: status.available
                        });
                    }
                    styleNode.innerHTML = html;
                    document.body.appendChild(styleNode);
                }

                initAfterSettings();
            });
        } else {
            initAfterSettings();
        }
    }

    function initAfterSettings() {
        currentUserId = teamlab.profile.id;
        currentProjectId = jq.getURLParam('prjID');
        loadingBanner = LoadingBanner;

        if (isInit === false) {
            isInit = true;

            function canCreateTask(prj) {
                return prj.canCreateTask;
            }

            var actions = [
                {
                    id: "gaChangeStatus",
                    title: tasksResource.ChangeStatus,
                    multi: master.customStatuses.map(function(item){
                            return {
                                id: "ga" + item.id,
                                title: item.title,
                                handler: gaChangeStatusHandler.bind(null, item),
                                checker: function (task) {
                                    return task.canEdit && statusList.getByData(task).id !== item.id;
                                }
                            }
                        })
                },
                {
                    id: "gaMove",
                    title: projectsJsResource.Move,
                    handler: gaMoveHandler,
                    checker: function (task, index, array) {
                        if (index > 0) {
                            var prevTask = array[index - 1];
                            if (prevTask && prevTask.projectId !== task.projectId) return false;
                        }
                        
                        return task.canEdit && task.status !== 2;
                    }
                },
                {
                    id: "gaDelete",
                    title: resources.CommonResource.Delete,
                    handler: gaRemoveHandler,
                    checker: function (task) {
                        return task.canDelete;
                    }
                }
            ];

            self.showOrHideData = self.showOrHideData.bind(self, {
                $container: $taskListContainer,
                tmplName: projectsTaskListItemTmplName,
                baseEmptyScreen: {
                    img: "tasks",
                    header: tasksResource.NoTasksCreated,
                    description: jq.format(teamlab.profile.isVisitor ? tasksResource.TasksHelpTheManageVisitor : tasksResource.TasksHelpTheManage, tasksResource.DescrEmptyListTaskFilter),
                    button: {
                        title: tasksResource.AddFirstTask,
                        onclick: showNewTaskPopup,
                        canCreate: function () {
                            if (currentProjectId && !currentProject) {
                                currentProject = common.getProjectByIdFromCache(currentProjectId);
                            }

                            return currentProjectId ?
                                canCreateTask(currentProject) :
                                master.Projects.some(canCreateTask);
                        }
                    }
                },
                filterEmptyScreen: {
                    header: tasksResource.NoTasks, 
                    description: tasksResource.DescrEmptyListTaskFilter
                },
                groupMenu: {
                    actions: actions,
                    getItemByCheckbox: getFilteredTaskByTarget,
                    getLineByCondition: function (condition) {
                        return filteredTasks
                            .filter(condition)
                            .map(function (item) {
                                return $taskListContainer.find("[taskid=" + item.id + "]");
                            });
                    },
                    multiSelector: master.customStatuses.map(function (item) {
                        return {
                            id: "gas" + item.id,
                            title: item.title,
                            condition: function (taskItem) {
                                return statusList.getByData(taskItem).id === item.id;
                            }
                        };
                    })
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
                getItem: getFilteredTaskByTarget,
                statuses: statuses
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
            ],
            {
                getItem: getFilteredTaskByTarget,
                selector: '.task .taskName a',
                getLink: function(item) {
                    return "Tasks.aspx?prjID=" + item.projectOwner.id + "&id=" + item.id;
                }
            });

        subtaskManager.init();

        // waiting data from api
        filter.createAdvansedFilterForTasks(self, master.customStatuses);

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
            var $self = jq(this);
            if ($self.find("span.not-action").length) return;
            var taskid = $self.parent().attr(taskidAttr);
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
            $othersListPopup.html(jq.tmpl("projects_taskListResponsibles", getFilteredTaskByTarget(event.target)));
            showActionsPanel.call(this);
            event.stopPropagation();
        });

        $taskListContainer.on(clickEventName, '.task', function (event) {
            var $elt = jq((event.target) ? event.target : event.srcElement);

            if ($elt.is('a')) {
                return;
            }

            if ($elt.is(".changeStatusCombobox.canEdit") ||
                $elt.parent().is(".changeStatusCombobox.canEdit") ||
                $elt.is(".entity-menu") ||
                $elt.is("input") ||
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
                menuItems.push(new ActionMenuItem("ta_edit", tasksResource.Edit, taEditHandler.bind(null, task), "edit"));
            }

            if (task.canCreateSubtask) {
                menuItems.push(new ActionMenuItem("ta_subtask", tasksResource.AddSubtask, taSubtaskHandler.bind(null, taskid), "subtask-v2"));
            }

            if (!task.responsible) {
                menuItems.push(new ActionMenuItem("ta_accept", tasksResource.AcceptSubtask, taAcceptHandler.bind(null, task), "accept"));
            }

            if (menuItems.length) {
                menuItems.push(new ActionMenuItem(null, null, null, null, true));
            }

            if (task.canEdit) {
                if (task.responsibles && task.responsibles.some(function (item) { return item.id != currentUserId; })) {
                    menuItems.push(new ActionMenuItem("ta_mesres", tasksResource.MessageResponsible, taMesresHandler.bind(null, taskid), "notify-responsible"));
                } 
            }
        }

        if (task.canCreateTimeSpend) {
            menuItems.push(new ActionMenuItem("ta_time", tasksResource.TrackTime, taTimeHandler.bind(null, task), "track-time"));
        }

        if (task.status !== 2) {
            if (task.canEdit) {
                menuItems.push(new ActionMenuItem("ta_move", tasksResource.MoveToMilestone, taMoveHandler.bind(null, task), "move-to-milestone"));
            }

            var project = common.getProjectById(task.projectId);
            if (project && project.canCreateTask) {
                menuItems.push(new ActionMenuItem("ta_copy", resources.CommonResource.Copy, taCopyHandler.bind(null, task), "move-or-copy"));
            } 
        }

        if (task.canDelete) {
            if (menuItems.length >= 3) {
                menuItems.push(new ActionMenuItem(null, null, null, null, true));
            }

            menuItems.push(new ActionMenuItem("ta_remove", resources.CommonResource.Delete, taRemoveHandler.bind(null, taskid), "delete"));
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
        showMoveToMilestonePanel([task]);
        return false;
    };

    function taRemoveHandler(taskid) {
        self.showCommonPopup("taskRemoveWarning", function () {
            teamlab.removePrjTask({ 'taskId': taskid }, taskid);
        });
        return false;
    };

    function gaChangeStatusHandler(status, taskids) {
        var tasks = taskids.map(getFilteredTaskById),
            openedSubtasks = false;

        for (var i = 0; i < tasks.length; i++) {
            var task = tasks[i];

            openedSubtasks = task.subtasks.some(function (item) {
                return item.status === 1;
            });

            if (openedSubtasks) break;
        }

        if (status.statusType === 2 && openedSubtasks) {
            self.showCommonPopup("closedTasksQuestion", function () {
                changeStatusMultipleTasks(status, taskids);
                jq.unblockUI();
            });
        } else {
            changeStatusMultipleTasks(status, taskids);
        }


        return false;
    };

    function changeStatusMultipleTasks(status, taskids) {
        teamlab.updatePrjTasksStatus({ taskids: taskids,  status: status.statusType, statusId: status.id },
        {
            before: function () {
                loadingBanner.displayLoading();
            },
            after: function () {
                loadingBanner.hideLoading();
            },
            success: function(params, data) {
                for (var i = 0; i < data.length; i++) {
                    teamlab.call(teamlab.events.updatePrjTaskStatus, this, [{ disableMessage: true }, data[i]]);
                }

                groupActionPanel.deselectAll();
                common.displayInfoPanel(projectsJsResource.TasksUpdated);
            }
        });
    }

    function gaMoveHandler(taskids) {
        showMoveToMilestonePanel(taskids.map(getFilteredTaskById));
        return false;
    };

    function gaRemoveHandler(taskids) {
        self.showCommonPopup("tasksRemoveWarning", function () {
            teamlab.removePrjTasks({ taskids: taskids },
            {
                before: function () {
                    loadingBanner.displayLoading();
                },
                after: function () {
                    loadingBanner.hideLoading();
                },
                success: function (params, data) {
                    for (var i = 0; i < data.length; i++) {
                        teamlab.call(teamlab.events.removePrjTask, this, [{ disableMessage: true }, data[i]]);
                    }

                    common.displayInfoPanel(projectsJsResource.TasksRemoved);
                }
            });
            jq.unblockUI();
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

        $moveTaskPanel
            .on(clickEventName, ".blue", function() {
                var tasks = showMoveToMilestonePanelTaskItems,
                    tasksIds = tasks.map(function(item) { return item.id; }),
                    links = [];

                var data = {
                    milestoneid: $moveTaskPanel.find(".milestonesList input:checked").attr('value'),
                    taskids: tasksIds
                };

                for (var i = 0; i < tasks.length; i++) {
                    var task = tasks[i];

                    if (task.links && task.links.length) {
                        links = links.concat(task.links);
                    }
                }

                function update() {
                    teamlab.updatePrjTasksMilestone(data, {
                        error: function() {
                            jq.unblockUI();
                        },
                        before: function() {
                            loadingBanner.displayLoading();
                        },
                        after: function () {
                            loadingBanner.hideLoading();
                        },
                        success: function (params, resp) {
                            for (var i = 0; i < resp.length; i++) {
                                teamlab.call(teamlab.events.updatePrjTask, this, [{ disableMessage: true }, resp[i]]);
                            }
                            groupActionPanel.deselectAll();
                            common.displayInfoPanel(projectsJsResource.TasksUpdated);
                        }
                    });
                }

                if (links.length) {
                    ASC.Projects.Base.showCommonPopup("taskLinksRemoveWarning",
                        function() {
                            for (var j = 0; j < links.length; ++j) {
                                var dataLink = {
                                    dependenceTaskId: links[j].dependenceTaskId,
                                    parentTaskId: links[j].parentTaskId
                                };
                                teamlab.removePrjTaskLink({},
                                    links[j].dependenceTaskId,
                                    dataLink,
                                    { success: function() {} });
                            }
                            update();
                        },
                        function() {
                            jq.unblockUI();
                            StudioBlockUIManager.blockUI($moveTaskPanel, 550);
                        });
                } else {
                    update();
                }
            })
            .on(clickEventName, ".gray", function() {
                jq.unblockUI();
                return false;
            });
    };

    function changeStatusHandler(id, statusId, status) {
        if (status === 2) {
            var task = getFilteredTaskById(id);
            var openedSubtasks = task.subtasks.some(function(item) {
                    return item.status === 1;
                });
            if (openedSubtasks) {
                popupWindow(id, statusId);
            } else {
                closeTask(id, statusId);
            }
        } else {
            var $task = getTaskItem(id);
            $task.find(".check div").hide();
            $task.find(".check").append(divTaskProcessList);
            updateTaskStatus({}, id, 1, statusId);
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

    function getFilteredTaskByTarget(target) {
        return getFilteredTaskById(jq(target).parents(".task").attr(taskidAttr));
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
    
    function closeTask(taskId, statusId) {
        jq('.taskProcess').remove();
        getTaskItem(taskId).find(".check").html(divTaskProcessList);

        updateTaskStatus({}, taskId, 2, statusId);
    };

    function updateTaskStatus(params, taskId, status, statusId) {
        teamlab.updatePrjTask(params, taskId, { "status": status, "statusId": statusId }, {
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

    function popupWindow(taskId, statusId) {
        self.showCommonPopup("closedTaskQuestion", function () {
            closeTask(taskId, statusId);
            jq.unblockUI();
        });
    };

    function showMoveToMilestonePanel(tasks) {
        var milestones = master.Milestones.filter(function(item) {
                return item.status === 0 && item.projectId == tasks[0].projectId;
            })
            .concat([
                {
                    id: 0,
                    title: tasksResource.None
                }
            ]);

        showMoveToMilestonePanelTaskItems = tasks;

        milestones.forEach(function (item) {
            item.checked = tasks.some(function(task) {
                return (task.milestoneId || 0) == item.id;
            });
        });

        var header, describe;

        if (tasks.length === 1) {
            header = tasksResource.MoveTaskToAnotherMilestone;
            describe = { title: tasksResource.Task, description: tasks[0].title, moveToMilestone: tasksResource.MoveToMilestone };
        } else {
            header = tasksResource.MoveTasksToAnotherMilestone;
            describe = { title: tasksResource.TasksCount, description: tasks.length, moveToMilestone: tasksResource.MoveTasksToMilestone };
        }

        $moveTaskPanel.html(jq.tmpl("common_containerTmpl",
        {
            options: {
                PopupContainerCssClass: "popupContainerClass",
                OnCancelButtonClick: "PopupKeyUpActionProvider.CloseDialog();",
                IsPopup: true
            },
            header: {
                data: { title: header },
                title: "projects_common_popup_header"
            },
            body: {
                title: "projects_move_task_panel",
                data: {
                    milestones: milestones.sort(common.milestoneSort),
                    describe: describe
                }
            }
        }));

        StudioBlockUIManager.blockUI($moveTaskPanel, 550);
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

    function updateCaldavEvent(task, action) {
        var url = ASC.Resources.Master.ApiPath + "calendar/caldavevent.json";
        var postData = {
            calendarId: "Project_" + task.projectId,
            uid: "Task_" + task.id,
            responsibles: jq.map(task.responsibles, function (user) { return user.id; })
        };
        jq.ajax({
            type: action === 0 || action === 1 ? 'put' : 'delete',
            url: url,
            data: postData,
            complete: function (d) {}
        });
        if (action === 0 || action === 1) {
            Teamlab.getPrjTeam({}, task.projectId, function (p, t) {
                var team = jq.map(t, function (user) { return user.id; });
                var responsibles = jq.map(task.responsibles, function (user) { return user.id; });

                var needDelete = team.filter(function (userId) {
                    return responsibles.indexOf(userId) < 0;
                });

                if (needDelete.length > 0) {
                    var deleteData = {
                        calendarId: "Project_" + task.projectId,
                        uid: "Task_" + task.id,
                        responsibles: needDelete
                    };
                    jq.ajax({
                        type: 'delete',
                        url: url,
                        data: deleteData,
                        complete: function (d) { }
                    });
                }
            });
        }
        
    }

    //api callback
    function onGetTasks(params, tasks) {
        filteredTasks = tasks;
        self.$commonListContainer.height('auto');
        $taskListContainer.height('auto');

        filterTaskCount = params.__total != undefined ? params.__total : 0;

        self.showOrHideData(tasks, filterTaskCount);
        subtaskManager.setTasks(tasks);

        $taskListContainer.off("click.taskItem").on("click.taskItem", ".taskName a", baseObject.Common.goToWithoutReload);
    };

    function onAddTask(params, task) {
        updateCaldavEvent(task, 0);
        currentProjectId = jq.getURLParam("prjId");

        if (currentProjectId && currentProjectId != task.projectId || jq.getURLParam("id") !== null) return;

        filterTaskCount++;
        filteredTasks.unshift(task);

        self.showOrHideData(filteredTasks, filterTaskCount);
        $taskListContainer.find(".task:first").yellowFade();
    };

    function onUpdateTask(params, task) {
        updateCaldavEvent(task, task.status);
        var taskId = task.id;
        getSubTasksItem(taskId).remove();
        getTaskItem(taskId).replaceWith(jq.tmpl(projectsTaskListItemTmplName, task));
        getTaskItem(taskId).yellowFade();
        setFilteredTask(task);
        jq.unblockUI();
        if (!params || !params.disableMessage) {
            common.displayInfoPanel(projectsJsResource.TaskUpdated);
        }
    };
    
    function onRemoveTask(params, task) {
        updateCaldavEvent(task, 2);
        var taskId = task.id;
        getTaskItem(taskId).remove();
        getSubTasksItem(taskId).remove();

        filterTaskCount--;
        filteredTasks = filteredTasks.filter(function(t) { return t.id !== taskId });

        self.showOrHideData(filteredTasks, filterTaskCount);

        getTaskItem(taskId).html(divTaskProcessList);
        jq.unblockUI();

        if (!params || !params.disableMessage) {
            common.displayInfoPanel(projectsJsResource.TaskRemoved);
        }

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
        var oldTask = getFilteredTaskById(task.id);
        if (oldTask.status !== task.status) {
            common.changeTaskCountInProjectsCache(task, 1);
        }
        groupActionPanel.deselectAll();
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
