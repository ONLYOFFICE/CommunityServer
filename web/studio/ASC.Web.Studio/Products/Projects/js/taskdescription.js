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


ASC.Projects.TaskDescriptionPage = (function() {
    var currentTask = {},
        tasks = [],
        taskId = undefined,
        projId = undefined,
        currentUserId,
        commentIsEdit = false,
        listAllProjectTasks = {},
        listOpenTasks = [],
        listValidTaskForLink = [],
        linkedTasksIds = [],
        relatedTasks = {},
        editedLink = null,
        reloadedTaskListFlag = false,
        overInvalidLinkHint = false,
        projectFolderId;

    var
        overViewTab,
        subtaskTab,
        documentsTab,
        linksTab,
        commentsTab,
        ganttTab,
        timeTrackingTab;

    var $editLinkBox,
        $relatedTasksCont,
        $taskSelector,
        $linkTypeSelector,
        $addTaskLinkButton,
        $createAddTaskLinkButton,
        $hintInvalidLink,
        $subtaskContainer,
        $filesContainer,
        $commentContainer,
        $mainCommentContainer,
        $subtasks,
        $linkedTasksContainer,
        $linkedTasksButtons;

    var baseObject = ASC.Projects,
        resources = baseObject.Resources,
        projectsJsResource = resources.ProjectsJSResource,
        tasksResource = resources.TasksResource,
        common = baseObject.Common,
        master = baseObject.Master,
        teamlab,
        attachments,
        loadingBanner,
        handlers = [];

    var displayNoneClass = "display-none",
        clickEventName = "click";

    var init = function () {
        baseObject.Base.clearTables();
        jq("#filterContainer").hide();
        $subtaskContainer = jq("#subtaskContainer");
        $filesContainer = jq("#filesContainer");
        $commentContainer = jq("#commonCommentsContainer");
        $mainCommentContainer = $commentContainer.find("#mainCommentsContainer");
        $subtasks = $subtaskContainer.find(".subtasks");

        jq(".tab1").html(jq.tmpl("projects_taskLinksContainer", { canEdit: false, tasks: [] }));
        $linkedTasksContainer = jq("#linkedTasksContainer");

        teamlab = Teamlab;

        loadingBanner = LoadingBanner;
        loadingBanner.displayLoading();

        currentUserId = Teamlab.profile.id;
        jq("#CommonListContainer").show();

        var events = teamlab.events;
        bind(events.addSubtask, onAddSubtask);
        bind(events.removeSubtask, onRemoveSubtask);
        bind(events.updateSubtask, onUpdateSubtaskStatus);
        //bind(events.addPrjTask, onAddTask);
        bind(events.updatePrjTask, onUpdateTask);
        bind(events.removePrjTask, onRemoveTask);
        bind(events.addPrjComment, function () {
            if (!commentIsEdit) {
                if (CommentsManagerObj.editorInstance.getData()) {
                    currentTask.commentsCount++;
                    commentsTab.rewrite();
                }
            }
            else {
                commentIsEdit = false;
            }
        });

        baseObject.SubtasksManager.init();
        ASC.Projects.Base.initActionPanel(baseObject.SubtasksManager.showEntityMenu, $subtasks);


        projId = jq.getURLParam("prjID");

        var id = jq.getURLParam("id");

        common.initCustomStatuses(function () {
            if (id != null) {
                teamlab.getPrjTask({},
                    id,
                    {
                        success: onGetTaskDescription,
                        error: function (params, errors) {
                            if (errors[0] === "Item not found") {
                                ASC.Projects.Base.setElementNotFound();
                            }
                        }
                    });
            } else {
                loadingBanner.hideLoading();
            }
        });
    };

    function initAttachmentsControl() {
        projectFolderId = currentTask.project.projectFolder;

        var entityType = "task";
        attachments = Attachments;

        if (!currentTask.canEdit) {
            attachments.banOnEditing();
        }

        ProjectDocumentsPopup.init(projectFolderId, attachments.isAddedFile, attachments.appendToListAttachFiles);
        attachments.isLoaded = false;
        attachments.init(entityType, function() { return currentTask.id });
        attachments.setFolderId(projectFolderId);
        attachments.loadFiles(currentTask.files);

        function addFileSuccess(file) {
            currentTask.files.push(file);
            documentsTab.rewrite();
        }

        attachments.bind("addFile", function (ev, file) {
            if (file.attachFromPrjDocFlag || file.isNewFile) {
                teamlab.addPrjEntityFiles(null,
                    taskId,
                    entityType,
                    [file.id],
                    { success: function() { addFileSuccess(file); } });
            } else {
                addFileSuccess(file);
            }
        });

        attachments.bind("deleteFile", function (ev, fileId) {
            teamlab.removePrjEntityFiles({}, taskId, entityType, fileId, {
                success: function () {
                currentTask.files = currentTask.files.filter(function (item) { return item.id != fileId; });
                attachments.deleteFileFromLayout(fileId);
                documentsTab.rewrite();
            }});
        });

        attachments.bind("loadAttachments", function (ev, count) {
            if (count < 1) {
                $filesContainer.show();
            }
        });
    };

    function hideRemovedComments(comments) {
        comments.forEach(function (item) {
            if (item.commentList.length) {
                item.commentList = hideRemovedComments(item.commentList);
            }
        });

        comments = comments.filter(function (item) {
            return !(item.inactive && item.commentList.length === 0);
        });

        return comments;
    }

    function getProjectTasks() {
        linkedTasksIds = [];
        var filter = common.filterParamsForListTasks;
        filter.projectId = projId;
        filter.status = 1;
        filter.milestone = currentTask.milestone ? currentTask.milestoneId : 0;

        if (currentTask.links) {
            for (var i = 0; i < currentTask.links.length; i++) {
                var linkTaskId;
                if (currentTask.links[i].parentTaskId == currentTask.id) {
                    linkTaskId = currentTask.links[i].dependenceTaskId;
                } else {
                    linkTaskId = currentTask.links[i].parentTaskId;
                }
                linkedTasksIds.push(linkTaskId);
                relatedTasks[linkTaskId] = currentTask.links[i];
            }
        }

        teamlab.getPrjTasks({}, { success: onGetOpenTask, filter: filter });
    };

    function hideHintInvalidLink() {
        setTimeout(function () {
            if (!overInvalidLinkHint) $hintInvalidLink.hide(100);
        }, 200);
    };

    function initCommentsBlock(task) {
        CommentsManagerObj.isShowAddCommentBtn = task.canCreateComment;
        CommentsManagerObj.total = task.comments.length;
        CommentsManagerObj.isEmpty = task.comments.length === 0;
        CommentsManagerObj.moduleName = "projects_Task";
        CommentsManagerObj.comments = jq.base64.encode(JSON.stringify(task.comments));
        CommentsManagerObj.objectID = "common";
        CommentsManagerObj.Init();
        CommentsManagerObj.objectID = task.id;
        jq("#hdnObjectID").val(task.id);

        jq("#commentsTitle").remove();
        $mainCommentContainer.css("width", 100 + "%");

        //-------------Comments-------------//
        var commentsClick = clickEventName + ".comments";
        jq("#btnCancel").off(commentsClick).on(commentsClick, function () {
            commentIsEdit = false;
            commentsTab.select();
        });

        jq("#mainCommentsContainer div[id^='container_'] a[id^='edit_']").off(commentsClick).on(commentsClick, function () {
            commentIsEdit = true;
        });


        jq(document).off("keydown.taskdescr").on("keydown.taskdescr", function (e) {
            if (e.keyCode == 27) {
                hideEditBox(true);
                $linkedTasksButtons.show();
            }
        });
    };

    function displaySubtasks(task) {
        $subtasks.empty();
        $subtasks.attr("taskid", task.id);
        var projectsTaskDescriptionSubtasksContainerTmplName = "projects_taskDescriptionSubtasksContainerTmpl";
        if (task.subtasks.length == 0) {
            jq.tmpl(projectsTaskDescriptionSubtasksContainerTmplName, task).prependTo($subtasks);
        }
        else {
            jq.tmpl(projectsTaskDescriptionSubtasksContainerTmplName, task).prependTo($subtasks);
            $subtasks.find(".st_separater").after($subtasks.find('.closed'));

            if (task.status == 2) {
                $subtasks.find(".quickAddSubTaskLink").remove();
                $subtasks.find(".subtask .check input").attr('disabled', true);
            }
            if (subtaskTab.selected) {
                $subtasks.show();
            }
        }

        jq(document).off("keydown.description").on("keydown.description", ".subtask-name-input", function (e) {
            switch (e.which) {
                case 27:
                    subtaskTab.select();
                    break;
            }
        });
    };
    
    function taAcceptHandler() {
        var data = {
            title: currentTask.title,
            responsibles: [currentUserId],
            description: currentTask.description,
            priority: currentTask.priority
        };

        if (currentTask.deadline) {
            data.deadline = currentTask.deadline;
        }

        if (currentTask.milestoneId) {
            data.milestoneid = currentTask.milestoneId;
        }

        teamlab.updatePrjTask({}, taskId, data);

        return false;
    };

    // task actions
    function taCloseHandler() {
        if ($subtasks.find(".subtask").length != $subtasks.find(".subtask.closed").length) {
            showQuestionWindow.call(this);
        }
        else {
            closeTask.call(this);
        }
    };

    function taResumeHandler() {
        jq("body").css("cursor", "wait");

        teamlab.updatePrjTask({}, taskId, { 'status': 1, "statusId": this.id }, { success: onChangeTaskStatus });

        jq(".pm_taskTitleClosedByPanel").hide();
    };

    function taEditHandler() {
        baseObject.TaskAction.showUpdateTaskForm(currentTask);
    };

    function taRemoveHandler() {
        ASC.Projects.Base.showCommonPopup("taskRemoveWarning", function () {
            teamlab.removePrjTask({}, taskId);
        });
    };

    function taStartTimerHandler() {
        common.showTimer(currentTask.projectId, currentTask.id);
    };


    function ltaEditHandler(taskid) {
        showEditLinkBox(true, taskid);
    };

    function ltaRemoveHandler(taskid) {
        removeTaskLink(taskid, true);
    };

    function displayTotalInfo(task) {
        var closedBy = "";

        var time = jq.timeFormat(task.timeSpend).split(":");
        var timeSpend = {
            hours: time[0],
            minutes: time[1]
        };

        if (task.status == 2) {
            if (task.updatedBy != undefined)
                closedBy = task.updatedBy.displayName;
            else
                closedBy = task.createdBy.displayName;
        }

        var statusAvailable = task.createdBy.id === currentUserId ||
            master.IsModuleAdmin ||
            task.project.responsible.id === currentUserId;

        var statuses = master.customStatuses
            .filter(function (item) {
                return statusAvailable || item.available;
            })
            .map(function(item) {
                return {
                    title: item.title,
                    handler: item.statusType === 1 ? taResumeHandler : taCloseHandler,
                    id: (Math.abs(item.id)).toString()
                };
            });

        var currentStatus = master.customStatuses.find(function (item) {
            if (task.customTaskStatus) {
                return item.id === task.customTaskStatus;
            }

            return item.statusType === task.status;
        });
        var descriptionTab = ASC.Projects.DescriptionTab;
        
        descriptionTab.init()
            .push(resources.ProjectResource.Project, formatDescription(task.projectOwner.title), "Tasks.aspx?prjID=" + task.projectOwner.id)
            .push(resources.MilestoneResource.Milestone, task.milestone ? jq.format('[{0}] {1}', task.milestone.displayDateDeadline, task.milestone.title) : '')
            .push(tasksResource.TaskStartDate, task.displayDateStart)
            .push(tasksResource.EndDate, task.displayDateDeadline, undefined, ASC.Projects.TasksManager.compareDates(task.deadline) ? "<span class='deadlineLate'>{0}</span>" : undefined)
            .push(tasksResource.Priority, task.priority === 1 ? tasksResource.HighPriority : undefined, undefined, '<span class="colorPriority high"><span>{0}</span></span>')
            .push(tasksResource.AssignedTo, task.responsibles.length === 0 ? tasksResource.WithoutResponsible : task.responsibles.map(function (item) { return item.displayName }).join(', '))
            .push(resources.CommonResource.SpentTotally, task.canCreateTimeSpend && task.timeSpend ? jq.format("{0} {1}", timeSpend.hours + resources.TimeTrackingResource.ShortHours, timeSpend.minutes + resources.TimeTrackingResource.ShortMinutes) : '', "TimeTracking.aspx?prjID=" + task.projectOwner.id + "&id=" + task.id)
            .push(tasksResource.CreatingDate, task.displayDateCrtdate)
            .push(tasksResource.TaskProducer, task.createdBy.displayName)
            .push(tasksResource.ClosingDate, task.status === 2 ? task.displayDateUptdate : '')
            .push(tasksResource.ClosedBy, closedBy)
            .push(resources.CommonResource.Description, jq.linksParser(formatDescription(task.description)))
            .setStatuses(statuses)
            .setCurrentStatus(currentStatus)
            .setStatusRight(currentTask.canEdit)
            .tmpl();

        if (overViewTab.selected) {
            descriptionTab.show();
        } else {
            descriptionTab.hide();
        }

        if (!task.canEdit) {
            jq("div.buttonContainer").remove();
        }

        if (task.timeSpend) {
            jq(".timeSpend").show();
        }
        jq(".subscribeLink").show();
    };

    function checkValidLinkTypeForTask(taskId){
        var task = listAllProjectTasks[taskId];

        var currentTaskStart = currentTask.startDate ? currentTask.startDate : currentTask.crtdate;
        var taskStart = task.startDate ? task.startDate : task.crtdate;

        var currentTaskDeadline = currentTask.deadline ? currentTask.deadline : undefined;
        var taskDeadline = task.deadline ? task.deadline : undefined;

        var validLinkFlag = true;
        var possibleLinkTypes = common.getPossibleTypeLink(currentTaskStart, currentTaskDeadline, taskStart, taskDeadline, validLinkFlag);

        if (possibleLinkTypes[2] == common.linkTypeEnum.start_end) {
            $linkTypeSelector.text(projectsJsResource.RelatedLinkTypeSE).data("value", common.linkTypeEnum.start_end);
        } else {
            $linkTypeSelector.text(projectsJsResource.RelatedLinkTypeES).data("value", common.linkTypeEnum.end_start);
        }
    };

    function getDelay(firstDate, secondDate){
        var delay = Math.abs(firstDate.getTime() - secondDate.getTime());
        delay = Math.floor(delay / 1000 / 60 / 60 / 24);
        if(delay == 0) return 0;
        if (delay == 1) {
            return delay + " " + projectsJsResource.DelayDay;
        } else {
            return delay + " " + projectsJsResource.DelayDays;
        }
    };

    function getRelatedTaskObject(firstTask, secondTask, link) {

        var firstTaskStart = firstTask.startDate ? firstTask.startDate : firstTask.crtdate;
        var secondTaskStart = secondTask.startDate ? secondTask.startDate : secondTask.crtdate;
        var firstTaskDeadline = firstTask.deadline ? firstTask.deadline : firstTask.milestone ? firstTask.milestone.deadline : undefined;
        var secondTaskDeadline = secondTask.deadline ? secondTask.deadline : secondTask.milestone ? secondTask.milestone.deadline : undefined;

        var relatedTaskObject = {};
        relatedTaskObject.invalidLink = false;
        relatedTaskObject.delay = 0;
        relatedTaskObject.possibleLinkType = common.getPossibleTypeLink(firstTaskStart, firstTaskDeadline, secondTaskStart, secondTaskDeadline, relatedTaskObject);


        relatedTaskObject.linkType = link.linkType;

        if (link.linkType == 2) {
            if (link.parentTaskId == currentTask.id) {
                relatedTaskObject.linkType = common.linkTypeEnum.end_start;
                if (relatedTaskObject.possibleLinkType[3] != common.linkTypeEnum.end_start) {
                    relatedTaskObject.invalidLink = true;
                }
            } else {
                relatedTaskObject.linkType = common.linkTypeEnum.start_end;
                if (relatedTaskObject.possibleLinkType[2] != common.linkTypeEnum.start_end) {
                    relatedTaskObject.invalidLink = true;
                }
            }
        }

        switch (relatedTaskObject.linkType) {
            case common.linkTypeEnum.start_start:
                relatedTaskObject.linkTypeText = projectsJsResource.RelatedLinkTypeSS;
                relatedTaskObject.delay = getDelay(firstTaskStart, secondTaskStart);
                relatedTaskObject.invalidLink = false;
                break;
            case common.linkTypeEnum.end_end:
                relatedTaskObject.linkTypeText = projectsJsResource.RelatedLinkTypeEE;
                relatedTaskObject.delay = getDelay(firstTaskDeadline, secondTaskDeadline);
                relatedTaskObject.invalidLink = false;
                break;
            case common.linkTypeEnum.start_end:
                relatedTaskObject.linkTypeText = projectsJsResource.RelatedLinkTypeSE;
                relatedTaskObject.delay = getDelay(secondTaskDeadline, firstTaskStart);
                break;
            case common.linkTypeEnum.end_start:
                relatedTaskObject.linkTypeText = projectsJsResource.RelatedLinkTypeES;
                relatedTaskObject.delay = getDelay(firstTaskDeadline, secondTaskStart);
                break;
            default: break;
        }

        return relatedTaskObject;
    };

    function getRelatedTaskTmpl(taskId, link) {
        var task = listAllProjectTasks[taskId];
        
        if (task) {
            task.relatedTaskObject = getRelatedTaskObject(currentTask, task, link);
            return task;
        } else {
            return null;
        }
    };

    function showEditLinkBox(editFlag, taskId) {
        if (editFlag) {
            var taskCont = $relatedTasksCont.find("tr[data-taskid="+taskId+"]");
            $editLinkBox.css("position", "absolute");

            var marginTop = taskCont.offset().top - jq("#linkedTasksContainer").offset().top - 1 + "px";
            $editLinkBox.css("top", marginTop);
            $editLinkBox.attr("data-edit", "edit");

            var option = $taskSelector.find("option[value="+taskId+"]");
            option.removeClass(displayNoneClass);
            option.prop("selected", true);
            $taskSelector.change();

            var link = relatedTasks[taskId];
            editedLink = link;
            editedLink.targetTaskId = taskId;

            var linkType = parseInt(taskCont.find(".link-type").data("type"));
            $linkTypeSelector.data("value", linkType);
            //linkTypeSelector.val(linkType);
        } else {
            if ($editLinkBox.is(":visible")) {
                hideEditBox(true);
            }
            $editLinkBox.addClass("border-top");
            $editLinkBox.addClass("border-bottom");
            $relatedTasksCont.css("marginTop", "0");
            $taskSelector.val("-1");
            $linkTypeSelector.text("");
            //linkTypeSelector.prop("disabled", true);
        }
        $editLinkBox.removeClass(displayNoneClass);
    };

    function hideEditBox(escapeFlag){
        if (editedLink) {
            if(escapeFlag){
                $taskSelector.find("option[value=" + editedLink.targetTaskId + "]").addClass(displayNoneClass);
            }
            editedLink = null;
        }
        $taskSelector.val("-1");
        //linkTypeSelector.val("-1");
        //linkTypeSelector.find("option").prop("disabled", false);
        $editLinkBox.css("position", "static");
        $editLinkBox.removeClass("border-top");
        $editLinkBox.removeClass("border-bottom");
        $editLinkBox.addClass(displayNoneClass);

        if (linksTab.selected) {
            linksTab.select();
        }
    };

    function initTaskLinkSelect(){
        var option = $taskSelector.find("option:first");
        $taskSelector.empty();
        $taskSelector.append(option);
        for (var i = 0, max = listValidTaskForLink.length; i < max; i++) {
            option = document.createElement('option');
            option.setAttribute("value", listValidTaskForLink[i].id);
            if (relatedTasks[listValidTaskForLink[i].id]) {
                option.setAttribute("class", displayNoneClass);
            }
            option.appendChild(document.createTextNode(listValidTaskForLink[i].title));
            $taskSelector.append(option);
        }
    };
    
    function disableCreateLinkButton() {
        if ($taskSelector.find("." + displayNoneClass).length == $taskSelector.find("option").length - 1 || currentTask.status == 2) {
            $createAddTaskLinkButton.addClass("disabled");
        } else {
            $createAddTaskLinkButton.removeClass("disabled");
        }
    };

    function displayRelatedTasks() {
        var tasksForTmpl = [];
        var linksCount = currentTask.links.length;
        for (var i = 0; i < linksCount; i++) {          //TODO: rewrite for relatedTasks
            var linkTaskId;
            if (currentTask.links[i].parentTaskId == currentTask.id) {
                linkTaskId = currentTask.links[i].dependenceTaskId;
            } else {
                linkTaskId = currentTask.links[i].parentTaskId;
            }
            tasksForTmpl.push(getRelatedTaskTmpl(linkTaskId, currentTask.links[i]));
        }

        jq(".tab1").html(jq.tmpl("projects_taskLinksContainer", { canEdit: currentTask.canEdit, tasks: tasksForTmpl })).show();
        $linkedTasksContainer = jq("#linkedTasksContainer");
        linksTab.$container = $linkedTasksContainer;

        $editLinkBox = jq("#editTable");
        $relatedTasksCont = jq("#relatedTasks");
        $taskSelector = jq("#taskSelector");
        $linkTypeSelector = jq("#linkTypeSelector");
        $addTaskLinkButton = jq("#addLink");
        $createAddTaskLinkButton = jq("#addNewLink");
        $hintInvalidLink = jq("#hintInvalidLink");
        $linkedTasksButtons = jq(".linked-tasks-buttons");

        ASC.Projects.Base.initActionPanel(showLinkEntityMenu, $linkedTasksContainer);

        $relatedTasksCont.on("mouseenter", ".invalid-link", function () {
            overInvalidLinkHint = true;
            var elem = this;
            setTimeout(function () {
                if (!overInvalidLinkHint) return;
                jq(elem).helper({
                    BlockHelperID: "hintInvalidLink",
                    addLeft: 8,
                    addTop: 14
                });
            }, 1000);
        });

        $relatedTasksCont.on("mouseleave", ".invalid-link", function () {
            overInvalidLinkHint = false;
            hideHintInvalidLink();
        });

        $hintInvalidLink.mouseenter(function () {
            overInvalidLinkHint = true;
        });

        $hintInvalidLink.mouseleave(function () {
            overInvalidLinkHint = false;
            hideHintInvalidLink();
        });

        $createAddTaskLinkButton.on(clickEventName, function () {
            if ($createAddTaskLinkButton.hasClass("disabled")) return;
            $linkedTasksButtons.hide();
            showEditLinkBox(false, {});
        });

        $addTaskLinkButton.on(clickEventName, function () {
            var link = {};
            var linkType = parseInt($linkTypeSelector.data("value"));//linkTypeSelector.val()

            if (linkType == "-1" || $taskSelector.val() == "-1") {
                hideEditBox();
                $linkedTasksButtons.show();
                return;
            }

            if (linkType == 2) {
                link = { parentTaskId: $taskSelector.val(), dependenceTaskId: currentTask.id, linkType: linkType };
            } else {
                link = { parentTaskId: currentTask.id, dependenceTaskId: $taskSelector.val(), linkType: linkType };
            }

            if (linkType == 3) {
                link.linkType = 2;
            }

            loadingBanner.displayLoading();
            if (!editedLink) {
                addTaskLink(link);
            } else {
                removeTaskLink(editedLink.targetTaskId, false);
                addTaskLink(link, editedLink.targetTaskId);
            }
        });
        $taskSelector.change(function () {
            var taskId = $taskSelector.val();
            if (taskId != "-1") {
                checkValidLinkTypeForTask(taskId);
                //linkTypeSelector.prop("disabled", false);
            }
        });

        if (linksTab.selected) {
            linksTab.select();
        }
    }


    function displayNewRelatedTask(link, element){
        if (currentTask.links) {
            currentTask.links.push(link);
        } else {
            currentTask.links = [];
            currentTask.links.push(link);
        }
        var linkTaskId;
        if (link.parentTaskId == currentTask.id) {
            linkTaskId = link.dependenceTaskId;
        } else {
            linkTaskId = link.parentTaskId;
        }
        relatedTasks[linkTaskId] = link;
        var taskForTmpl = getRelatedTaskTmpl(linkTaskId, link);
        if (element) {
            element.after(jq.tmpl("projects_taskLinks", [taskForTmpl]));
        } else {
            jq.tmpl("projects_taskLinks", [taskForTmpl]).prependTo($relatedTasksCont);
        }
        $taskSelector.find("option[value=" + linkTaskId + "]").addClass(displayNoneClass);
    }

    function displayTaskDescription(task) {
        displayTotalInfo(task);
        displaySubtasks(task);
        initCommentsBlock(task);
    }

    function showQuestionWindow() {
        ASC.Projects.Base.showCommonPopup("closedTaskQuestion", closeTask.bind(this), cancelCloseTask);
    }

    function subscribeTask() {
        teamlab.subscribeToPrjTask({}, taskId, { success: function() {
            currentTask.isSubscribed = !currentTask.isSubscribed;
        }});
    };

    function addTaskLink(link, removedLinkTaskId){
        teamlab.addPrjTaskLink({link: link, removedLinkTaskId: removedLinkTaskId }, link.parentTaskId, link, { success: onAddTaskLink, error: onAddTaskLinkError });
    };

    function removeTaskLink(taskId, removeElemFlag) {
        var link = relatedTasks[taskId];
        teamlab.removePrjTaskLink({ taskId: taskId, removeElemFlag: removeElemFlag }, link.dependenceTaskId, { dependenceTaskId: link.dependenceTaskId, parentTaskId: link.parentTaskId }, { success: onRemoveTaskLink, error: function (params, error) { } });
    };

    function showEntityMenu() {
        var menuItems = [],
            ActionMenuItem = ASC.Projects.ActionMenuItem;

        if (currentTask.status !== 2 && currentTask.canEdit) {
            if (currentTask.responsibles.length === 0) {
                menuItems.push(new ActionMenuItem("ta_accept", tasksResource.Accept, taAcceptHandler));
            }

            menuItems.push(new ActionMenuItem("ta_edit", tasksResource.EditTask, taEditHandler));
        }

        if (currentTask.canDelete) {
            menuItems.push(new ActionMenuItem("ta_remove", tasksResource.RemoveTask, taRemoveHandler));
        }

        if (currentTask.canCreateTimeSpend) {
            menuItems.push(new ActionMenuItem("ta_startTimer", resources.CommonResource.AutoTimer, taStartTimerHandler));
        }

        if (typeof (currentTask.isSubscribed) !== "undefined") {
            menuItems.push(new ActionMenuItem("ta_follow", currentTask.isSubscribed ? tasksResource.UnfollowTask : tasksResource.FollowTask, subscribeTask));
        }

        return { menuItems: menuItems };
    }

    function showLinkEntityMenu(selectedActionCombobox) {
        var taskid = selectedActionCombobox.data("taskid");

        var menuItems = [
            new ASC.Projects.ActionMenuItem("lta_edit", tasksResource.Edit, ltaEditHandler.bind(null, taskid), "edit"),
            new ASC.Projects.ActionMenuItem("lta_remove", resources.CommonResource.Delete, ltaRemoveHandler.bind(null, taskid), "delete")
        ];

        return { menuItems: menuItems };
    }

    /*--------event handlers--------*/

    function onRemoveTaskLink(params, task) {
        if (params.removeElemFlag) {
            $relatedTasksCont.find("tr[data-taskid=" + params.taskId + "]").remove();
            loadingBanner.hideLoading();
        }

        currentTask.links = currentTask.links.filter(function(item) {
            return item.dependenceTaskId != params.taskId && item.parentTaskId != params.taskId;
        });
        if (params.removeElemFlag) {
            linksTab.select();
        }

        delete relatedTasks[params.taskId];
        editedLink = null;
        $taskSelector.find("option[value=" + params.taskId + "]").removeClass(displayNoneClass);
        disableCreateLinkButton();
    };

    function onAddTaskLink(params, task) {
        hideEditBox();
        
        if (params.removedLinkTaskId) {
            var removeElem = $relatedTasksCont.find("tr[data-taskid=" + params.removedLinkTaskId + "]");
            displayNewRelatedTask(params.link, removeElem);
            removeElem.remove();
        } else {
            displayNewRelatedTask(params.link);
        }

        $linkedTasksButtons.show();
        disableCreateLinkButton();
        linksTab.select();
        loadingBanner.hideLoading();
    };

    function onAddTaskLinkError(params, error) {
        loadingBanner.hideLoading();

        ASC.Projects.Base.showCommonPopup("createNewLinkError");

        hideEditBox();
        reloadedTaskListFlag = true;
        getProjectTasks();
    };

    function onGetTaskDescription(params, task) {
        currentTask = task;

        getProjectTasks();

        taskId = task.id;

        initAttachmentsControl();

        task.comments = hideRemovedComments(task.comments);

        var subtasksEmpty = 
        {
            img: "subtasks",
            header: tasksResource.SubtasksEmptyScreen_Header,
            description: tasksResource.SubtasksEmptyScreen_Describe,
            button: {
                title: tasksResource.AddNewSubtask,
                onclick: function () {
                    $subtaskContainer.find(".quickAddSubTaskLink .link").click();
                },
                canCreate: function () {
                    return currentTask.status !== 2 && currentTask.canCreateSubtask;
                }
            }
        };

        var linksEmpty = 
        {
            img: "relatedtasks",
            header: tasksResource.RelatedTasksEmptyScreen_Header,
            description: tasksResource.RelatedTasksEmptyScreen_Describe,
            button: {
                title: tasksResource.CreateNewLink,
                onclick: function () {
                    if (!$editLinkBox.is(":visible")) {
                        $createAddTaskLinkButton.click();
                    }
                },
                canCreate: function () {
                    return currentTask.status === 1 && currentTask.canEdit && listValidTaskForLink.length;
                }
            }
        };

        var commentsEmpty = 
        {
            img: "comments",
            header: tasksResource.CommentsEmptyScreen_Header,
            description: tasksResource.CommentsEmptyScreen_Describe,
            button: {
                title: ASC.Resources.Master.TemplateResource.AddNewCommentButton,
                onclick: function () {
                    jq("#add_comment_btn").click();
                },
                canCreate: function () {
                    return currentTask.canCreateComment;
                }
            }
        };

        var Tab = ASC.Projects.Tab;
        overViewTab = new Tab(resources.ProjectsJSResource.OverviewModule,
            function() { return 0; },
            "overViewModule",
            jq(".tab"),
            '#');
        subtaskTab = new Tab(tasksResource.Subtasks,
            function() { return currentTask.subtasks.length; },
            "subtasksModule",
            $subtaskContainer,
            '#subtasks',
            function() { return currentTask.canEdit || currentTask.subtasks.length },
            subtasksEmpty);
        documentsTab = new Tab(ASC.Projects.Resources.CommonResource.DocsModuleTitle,
            function() { return currentTask.files.length; },
            "documentsModule",
            $filesContainer,
            '#documents',
            function() { return currentTask.canReadFiles && (currentTask.canEditFiles || currentTask.files.length) });
        linksTab = new Tab(tasksResource.RelatedTask,
            function() { return currentTask.links.length; },
            "linksModule",
            $linkedTasksContainer,
            '#links',
            function() { return currentTask.canEdit || currentTask.links.length },
            linksEmpty);
        commentsTab = new Tab(resources.MessageResource.Comments,
            function() { return currentTask.commentsCount; },
            "commentsModule",
            $commentContainer,
            '#comments',
            function() { return true },
            commentsEmpty,
            '#comment_');
        ganttTab = new Tab(resources.ProjectResource.GanttGart,
            function () { return 0; },
            "ganttchartModule",
            null,
            "GanttChart.aspx",
            function () { return !jq.browser.mobile && task.projectOwner.status === 0 });

        ganttTab.href = "GanttChart.aspx?prjID=" + task.projectId;

        timeTrackingTab = new Tab(resources.ProjectsJSResource.TimeTrackingModule,
            function () {
                var time = jq.timeFormat(task.timeSpend).split(":");
                var timeSpend = {
                    hours: time[0],
                    minutes: time[1]
                };
                 return jq.format("{0}:{1}", timeSpend.hours, timeSpend.minutes);
            },
            "timetrackingModule",
            null,
            "TimeTracking.aspx",
            function () { return task.canCreateTimeSpend && task.timeSpend; });

        timeTrackingTab.href = "TimeTracking.aspx?prjID=" + task.projectOwner.id + "&id=" + task.id;

        var data = {
            icon: "tasks",
            title: task.title
        };

        baseObject.InfoContainer.init(data, showEntityMenu, [overViewTab, subtaskTab, documentsTab, linksTab, commentsTab, timeTrackingTab, ganttTab]);

        jq("#descriptionTab").show();
        loadingBanner.hideLoading();

        tasks = [currentTask];
        baseObject.SubtasksManager.setTasks(tasks);

        displayTaskDescription(task);
    };

    function onUpdateTask(params, task) {
        currentTask = task;
        if (!task.canEdit && attachments) {
            attachments.banOnEditing();
        }

        baseObject.InfoContainer.updateTitle(task.title);
        displayTaskDescription(task);

        jq.unblockUI();
        common.displayInfoPanel(projectsJsResource.TaskUpdated);
    };

    function onRemoveTask() {
        jq.unblockUI();
        var newUrl = "Tasks.aspx?prjID=" + projId;
        window.location.replace(newUrl);
    };

    function onRemoveSubtask(params, subtask) {
        tasks[0].subtasks = tasks[0].subtasks.filter(function (t) { return t.id !== subtask.id });
        subtaskTab.select();
    };

    function onAddSubtask(params, subtask) {
        tasks[0].subtasks.push(subtask);
        subtaskTab.rewrite();
    };

    function onUpdateSubtaskStatus(params, subtask) {
        for (var i = 0, max = tasks[0].subtasks.length; i < max; i++) {
            if (tasks[0].subtasks[i].id === subtask.id) {
                tasks[0].subtasks[i] = subtask;
                break;
            }
        }
        subtaskTab.rewrite();
    };

    function onChangeTaskStatus(params, task) {
        currentTask = task;
        tasks[0].subtasks = currentTask.subtasks;
        subtaskTab.rewrite();

        displayTaskDescription(task);

        jq("body").css("cursor", "default");
        disableCreateLinkButton();
        jq.unblockUI();
    }

    function onGetRelatedTasks(params, tasks){
        var taskCount = tasks.length;

        if (taskCount) {
            for (var i = 0; i < taskCount; i++) {
                if (tasks[i].status == 2) {
                    listAllProjectTasks[tasks[i].id] = tasks[i];
                }
            }
        }

        displayRelatedTasks();
        initTaskLinkSelect();
        disableCreateLinkButton();
    };

    function onGetOpenTask(params, tasks) {
        listOpenTasks = [];
        listValidTaskForLink = [];
        var taskCount = tasks.length;
        if (taskCount) {
            for (var i = 0; i < taskCount; i++) {
                if (tasks[i].id == currentTask.id) {
                    if (reloadedTaskListFlag) {
                        //displayTotalInfo(currentTask);
                        reloadedTaskListFlag = false;
                    }
                }
                if (tasks[i].status == 1 && tasks[i].id != currentTask.id) {
                    listOpenTasks.push(tasks[i]);
                    if (tasks[i].deadline || currentTask.deadline) { // rewrite use possible link types
                        listValidTaskForLink.push(tasks[i]);
                    }
                }                
                listAllProjectTasks[tasks[i].id] = tasks[i];
            }
        }

        teamlab.getPrjTasksById({}, { taskid: linkedTasksIds }, { success: onGetRelatedTasks });
    };

    function onDeleteComment() {
        currentTask.commentsCount--;
        commentsTab.select();

        if (currentTask.commentsCount === 0) {
            commentsTab.select();
        }
    };

    /*---------actions--------*/
    function closeTask() {
        jq("body").css("cursor", "wait");

        teamlab.updatePrjTask({}, taskId, { 'status': 2, 'statusId': this.id }, { success: onChangeTaskStatus });
    };

    function cancelCloseTask() {
        ASC.Projects.DescriptionTab.resetStatus(currentTask.status);
        jq.unblockUI();
    }

    function formatDescription(descr) {
        var formatDescr = descr.replace(/</ig, '&lt;').replace(/>/ig, '&gt;').replace(/\n/ig, '<br/>');
        return formatDescr.replace('&amp;', '&');
    };

    function compareDates(data) {
        var currentDate = new Date();
        if (currentDate > data) {
            return true;
        }
        if (currentDate <= data) {
            return false;
        }
    };

    function unbindListEvents() {
        jq(".project-info-container").hide();
        jq("#descriptionTab").hide();
        jq(document).off("keydown.taskdescr");
        jq(document).off("keydown.description");
        attachments.unbind();
        unbind();
    }

    function bind(event, handler) {
        handlers.push(teamlab.bind(event, handler));
    }

    function unbind() {
        while (handlers.length) {
            var handler = handlers.shift();
            teamlab.unbind(handler);
        }
    }

    return {
        init: init,
        onDeleteComment: onDeleteComment,
        compareDates: compareDates,
        unbindListEvents: unbindListEvents
    };
})(jQuery);
