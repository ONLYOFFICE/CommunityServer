/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
ASC.Projects.TaskDescroptionPage = (function() {
    var isInit = false,
        currentTask = {},
        taskId = undefined,
        projId = undefined,
        currentUserId,
        commentIsEdit = false,
        listResponsibles = [],
        TaskTimeSpend = '0.00';
    var listAllProjectTasks = {};
    var listOpenTasks = [],
        listValidTaskForLink = [],
        linkedTasksIds = [];
    var relatedTasks = {};
    var editedLink = null,
        reloadedTaskListFlag = false,
        overInvalidLinkHint = false;

    var editLinkBox = jq("#editTable"),
        relatedTasksCont = jq("#relatedTasks"),
        taskSelector = jq("#taskSelector"),
        linkTypeSelector = jq("#linkTypeSelector"),
        relatedTaskLoader = jq(".related-task-loader"),
        addTaskLinkButton = jq("#addLink"),
        createAddTaskLinkButton = jq("#addNewLink"),
        hintInvalidLink = jq("#hintInvalidLink");

    var projectFolderId, projectName;

    var initAttachmentsControl = function() {
        projectFolderId = parseInt(jq("#filesContainer").attr("data-projectfolderid"));
        projectName = Encoder.htmlEncode(currentTask.projectTitle);

        ProjectDocumentsPopup.init(projectFolderId, projectName);
        Attachments.init();
        Attachments.setFolderId(projectFolderId);
        Attachments.loadFiles();

        Attachments.bind("addFile", function(ev, file) {
            if (file.attachFromPrjDocFlag || file.isNewFile) {
                Teamlab.addPrjEntityFiles(null, taskId, "task", [file.id], function() { });
            }
            changeCountInTab('add', "filesTab");
            jq("#switcherTaskFilesButton").show();
        });
        Attachments.bind("deleteFile", function(ev, fileId) {
            Teamlab.removePrjEntityFiles({}, taskId, "task", fileId, function() {
            });
            Attachments.deleteFileFromLayout(fileId);
            changeCountInTab('delete', "filesTab");
        });
        Attachments.bind("loadAttachments", function(ev, count) {
            if (count < 1) {
                jq("#filesContainer").show();
            }
            changeCountInTab(count, "filesTab");
        });
    };

    var getProjectTasks = function () {

        var filter = ASC.Projects.Common.filterParamsForListTasks;
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
            Teamlab.getPrjTasksById({}, {taskid: linkedTasksIds}, { success: onGetRelatedTasks });
        }

        Teamlab.getPrjTasks({}, null, null, null, { success: onGetOpenTask, filter: filter });
    };

    var hideGanttLink = function () {
        var isPrivateProj = currentTask.projectOwner.isPrivate;
        if (!isPrivateProj) return;
        var partisipant = ASC.Projects.Common.userInProjectTeam(currentUserId);
        if(!partisipant) return;
        if (!partisipant.canReadTasks || !partisipant.canReadMilestones) {
            jq(".linked-tasks-buttons .chart").remove();
        }
    };

    var init = function() {
        if (isInit === false) {
            isInit = true;
        }
        LoadingBanner.displayLoading();

        currentUserId = Teamlab.profile.id;

        ASC.Projects.SubtasksManager.init();
        ASC.Projects.SubtasksManager.onAddSubtaskHandler = onAddSubtask;
        ASC.Projects.SubtasksManager.onRemoveSubtaskHandler = onRemoveSubtask;
        ASC.Projects.SubtasksManager.onChangeTaskStatusHandler = onUpdateSubtaskStatus;

        initCommentsBlock();

        projId = jq.getURLParam("prjID");

        setTimeSpent();

        var id = jq.getURLParam("id");
        if (id != null) {
            Teamlab.getPrjTask({}, id, { success: onGetTaskDescription });
        } else {
            jq("#followTaskActionTop").remove();
            LoadingBanner.hideLoading();
        }

        jq(function() {
            if (jq('#taskActions .dropdown-content li a').length == 0) {
                jq('.menu-small').addClass("visibility-hidden");
            }
        });

        // task actions
        jq("#taskActions").on("click", "#editTaskAction", function() {
            jq("#taskActions").hide();
            jq(".project-title .menu-small").removeClass("active");
            editTask();
        });

        jq("#taskActions").on("click", "#removeTask", function() {
            jq("#taskActions").hide();
            jq(".project-title .menu-small").removeClass("active");
            showQuestionWindowTaskRemove();
        });

        jq(".commonInfoTaskDescription").on('click', "#closeButton", function() {
            if (jq(".subtasks .subtask").length != jq(".subtasks .subtask.closed").length) {
                showQuestionWindow();
            }
            else {
                closeTask();
            }
        });

        jq(".commonInfoTaskDescription").on('click', "#resumeButton", function() {
            resumeTask();
        });

        jq(".commonInfoTaskDescription").on('click', '#acceptButton', function() {
            var data = {};
            data.title = currentTask.title;
            data.responsibles = [currentUserId];

            data.description = currentTask.description;

            if (currentTask.deadline) {
                data.deadline = currentTask.deadline;
            }

            if (currentTask.milestoneId) {
                data.milestoneid = currentTask.milestoneId;
            }
            data.priority = currentTask.priority;
            Teamlab.updatePrjTask({}, taskId, data, { success: onUpdateTask });

            return false;
        });

        jq("#startTaskTimer").bind('click', function() {
            jq("#taskActions").hide();
            jq(".project-title .menu-small").removeClass("active");
            var taskId = jq(this).attr('taskid'),
            prjid = jq(this).attr('projectid');
            ASC.Projects.Common.showTimer('timer.aspx?prjID=' + prjid + '&taskId=' + taskId);
        });

        //-------------Comments-------------//
        jq("#add_comment_btn").wrap("<span class='addcomment-button icon-link plus'></span>");

        jq("#emptyCommentsPanel .emptyScrBttnPnl").on('click', ".baseLinkAction", function () {
            jq("#emptyCommentsPanel").hide();
            jq("#commentsListWrapper").show();
            jq("#add_comment_btn").click();

            return false;
        });

        jq(document).on('click', "#btnCancel , #cancel_comment_btn", function() {

            var count = jq("#commentContainer #mainContainer div[id^='container_']").length;
            if (count == 0) {
                jq("#commentsListWrapper").hide();
                jq("#add_comment_btn").hide();
                jq("#emptyCommentsPanel").show();
            }
            commentIsEdit = false;
        });
        jq(document).on('click', "#btnAddComment", function() {
            if (!commentIsEdit) {
                if (CKEDITOR.instances.commentEditor.getData()) {
                    changeCountInTab('add', "commentsTab");
                    jq("#switcherTaskCommentsButton").show();
                }
            }
            else {
                commentIsEdit = false;
            }
        });
        jq("#mainContainer div[id^='container_'] a[id^='remove_']").on('click', function() {
            changeCountInTab('delete', "commentsTab");
            var count = jq("#commentContainer #mainContainer div[id^='container_']").length;
            if (count - 1 == 0) {
                jq("#commentsListWrapper").hide();
                jq("#commentContainer #mainContainer").attr('style', '');
                jq("#commentContainer #mainContainer").empty();

                jq("#emptyCommentsPanel").show();
                jq("#switcherTaskCommentsButton").hide();
            }
        });
        jq(document).on('click', "#mainContainer div[id^='container_'] a[id^='edit_']", function() {
            commentIsEdit = true;
        });

        /*---close/remove task----*/
        jq('#questionWindow .end').bind('click', function() {
            closeTask();
            jq.unblockUI();
            return false;
        });
        jq('#questionWindow .cancel, #questionWindowTaskRemove .cancel, #createNewLinkError .cancel').bind('click', function () {
            jq.unblockUI();
            return false;
        });
        jq('#questionWindowTaskRemove .remove').bind('click', function() {
            removeTask();
            return false;
        });

        jq.switcherAction("#switcherSubtasksButton", "#subtaskContainer");
        jq.switcherAction("#switcherTaskFilesButton", "#filesContainer");
        jq.switcherAction("#switcherLinkedTasksButton", "#linkedTasksContainer");
        jq.switcherAction("#switcherTaskCommentsButton", "#commentContainer");

        // related tasks

        relatedTasksCont.on("mouseenter", ".invalid-link", function () {
            overInvalidLinkHint = true;
            var elem = this;
            setTimeout(function () {
                if (!overInvalidLinkHint) return;
            
                jq(elem).helper({
                    BlockHelperID: "hintInvalidLink",
                    addLeft: 8,
                    addTop: 14
                });
           }, 300);
        });

        relatedTasksCont.on("mouseleave", ".invalid-link", function () {
            overInvalidLinkHint = false;
            hideHintInvalidLink();
        });

        hintInvalidLink.mouseenter(function () {
            overInvalidLinkHint = true;
        });

        hintInvalidLink.mouseleave(function () {
            overInvalidLinkHint = false;
            hideHintInvalidLink();
        });

        createAddTaskLinkButton.click(function(){
            if (jq(this).hasClass("disabled")) return;
            showEditLinkBox(false, {});
        });

        relatedTasksCont.on("click", ".entity-menu", function () {
            var actionPanel = jq("#linkedTaskActionPanel");
            if (actionPanel.is(":visible")) {
                actionPanel.hide();
                return;
            }
            actionPanel.data("taskid", jq(this).data("taskid"));
            showActionsPanel("linkedTaskActionPanel", this); 
        });

        addTaskLinkButton.click(function () {
            var link = {};
            var linkType = parseInt(linkTypeSelector.data("value"));//linkTypeSelector.val()
            
            if (linkType == "-1" || taskSelector.val() == "-1") {
                hideEditBox();
                return;
            }

            if (linkType == 2) {
                link = { parentTaskId: taskSelector.val(), dependenceTaskId: currentTask.id, linkType: linkType };
            } else {
                link = { parentTaskId: currentTask.id, dependenceTaskId: taskSelector.val(), linkType: linkType };
            }

            if(linkType == 3){
                link.linkType = 2;
            }

            LoadingBanner.displayLoading();
            if(!editedLink){
                addTaskLink(link);
            } else {
                removeTaskLink(editedLink.targetTaskId, false);
                addTaskLink(link, editedLink.targetTaskId);
            }
        });

        jq("#linkEdit").click(function(){
            jq('.studio-action-panel').hide();
            showEditLinkBox(true, jq("#linkedTaskActionPanel").data("taskid"));
        });

        jq("#linkRemove").click(function(){
            jq('.studio-action-panel').hide();
            removeTaskLink(jq("#linkedTaskActionPanel").data("taskid"), true);
        });

        jq(document).keydown(function(e) {
            if (e.keyCode == 27) {
                hideEditBox(true);
            } 
        });

        jq("body").click(function (event) {
            var element = jq(event.target);
            if (element.attr("id") && element.attr("id") == "addNewLink") return true;
            if (element.closest(".studio-action-panel").length || element.hasClass("studio-action-panel")) {
                return true;
            }
            if (element.closest(".task-links").length || element.hasClass("task-links")) {
                return true;
            }
            if (editLinkBox.is(":visible")) {
                hideEditBox(true);
            }
            return true;
        });

        taskSelector.change(function(){
            var taskId = taskSelector.val();
            if (taskId != "-1") {
                checkValidLinkTypeForTask(taskId);
                //linkTypeSelector.prop("disabled", false);
            }
        });
    };

    var hideHintInvalidLink = function () {
        setTimeout(function () {
            if (!overInvalidLinkHint) hintInvalidLink.hide(100);
        }, 200);
    };

    var initCommentsBlock = function() {
        jq("#commentsTitle").remove();
        jq("#commentContainer #mainContainer").css("width", 100 + "%");
        var count = jq("#commentContainer #mainContainer div[id^='container_']").length;
        if (count != 0) {
            changeCountInTab(count, "commentsTab");
            jq("#add_comment_btn").show();
            jq("#switcherTaskCommentsButton").show();
            jq("#commentsListWrapper").show();
        }
        else {
            jq("#switcherTaskCommentsButton").hide();
            jq("#commentContainer").show();
            jq("#emptyCommentsPanel").show();
            jq("#noComments").hide();
        }
    };

    var setTimeSpent = function() {
        TaskTimeSpend = jq(".commonInfoTaskDescription").attr("data-time-spend");
    };

    var displaySubtasks = function(task) {
        jq('.subtasks').empty();
        if (task.subtasks.length == 0) {
            jq("#switcherSubtasksButton").hide();
            jq("#subtaskContainer").show();
            jq.tmpl("projects_taskDescriptionSubtasksContainerTmpl", task).prependTo('.subtasks');
        }
        else {
            jq.tmpl("projects_taskDescriptionSubtasksContainerTmpl", task).prependTo('.subtasks');
            jq(".st_separater").after(jq('.subtasks .closed'));

            var subtasksCount = 0;
            for (var i = 0, n = task.subtasks.length; i < n; i++) {
                if (task.subtasks[i].status != 2) subtasksCount++;
            }

            changeCountInTab(subtasksCount, "subtaskTab");
            if (task.status == 2) {
                jq(".quickAddSubTaskLink").remove();
                jq(".subtask .check input").attr('disabled', true);
            }
            jq('.subtasks').show();
        }

        jq('.taskTabs').show();
        jq('#tabsContent').show();

    };
    var displayTotalInfo = function(task) {
        var deadline = "", displayDateDeadline = "", milestone = "", closedBy = "", closedDate = "", timeSpend = {};

        if (task.deadline) {
            displayDateDeadline = task.displayDateDeadline;
            deadline = task.deadline;
        }

        var formatedTime = jq.timeFormat(TaskTimeSpend);
        var time = formatedTime.split(":");
        timeSpend.hours = time[0];
        timeSpend.minutes = time[1];

        if (task.status == 2) {
            if (task.updatedBy != undefined)
                closedBy = task.updatedBy.displayName;
            else
                closedBy = task.createdBy.displayName;

            closedDate = task.displayDateUptdate;
        }
        var responsibles = new Array();
        listResponsibles.splice(0, listResponsibles.length);
        if (task.responsibles.length) {
            for (var i = 0; i < task.responsibles.length; i++) {
                var person = { displayName: task.responsibles[i].displayName, id: task.responsibles[i].id };
                listResponsibles.push(task.responsibles[i].id);
                responsibles.push(person);
            }
        }
        if (task.milestone) {
            milestone = task.milestone.title;
            milestone = "[ " + task.milestone.displayDateDeadline + " ] " + milestone;
        }
        var priority = task.priority;
        var descriptionInfo = {
            createdDate: task.displayDateCrtdate, createdBy: task.createdBy.displayName,
            closedDate: closedDate, closedBy: closedBy,
            timeSpend: timeSpend, deadline: deadline, displayDateDeadline: displayDateDeadline, milestone: milestone, description: task.description,
            priority: priority, status: task.status, responsibles: responsibles, taskId: task.id, projId: projId, project: task.projectOwner.title,
            canCreateTimeSpend: task.canCreateTimeSpend, displayDateStart: task.displayDateStart
        };

        jq.tmpl("projects_taskDescriptionTmpl", descriptionInfo).prependTo('.commonInfoTaskDescription');

        if (!task.canEdit) {
            jq("div.buttonContainer").remove();
        }

        if (formatedTime != "0:00" && formatedTime != "") {
            jq(".timeSpend").show();
        }
        jq(".subscribeLink").show();
    };

    var checkValidLinkTypeForTask = function(taskId){
        var task = listAllProjectTasks[taskId];

        var currentTaskStart = currentTask.startDate ? currentTask.startDate : currentTask.crtdate;
        var taskStart = task.startDate ? task.startDate : task.crtdate;

        var currentTaskDeadline = currentTask.deadline ? currentTask.deadline : undefined;
        var taskDeadline = task.deadline ? task.deadline : undefined;

        var validLinkFlag = true;
        var possibleLinkTypes = ASC.Projects.Common.getPossibleTypeLink(currentTaskStart, currentTaskDeadline, taskStart, taskDeadline, validLinkFlag);

        if (possibleLinkTypes[2] == ASC.Projects.Common.linkTypeEnum.start_end) {
            linkTypeSelector.text(ASC.Projects.Resources.ProjectsJSResource.RelatedLinkTypeSE).data("value", ASC.Projects.Common.linkTypeEnum.start_end);
        } else {
            linkTypeSelector.text(ASC.Projects.Resources.ProjectsJSResource.RelatedLinkTypeES).data("value", ASC.Projects.Common.linkTypeEnum.end_start);
        }

        //linkTypeSelector.val("-1");

        //if (possibleLinkTypes[1] == ASC.Projects.Common.linkTypeEnum.end_end) {
        //    linkTypeSelector.find("option[value=" + ASC.Projects.Common.linkTypeEnum.end_end + "]").show().prop("disabled", false);
        //} else {
        //    linkTypeSelector.find("option[value=" + ASC.Projects.Common.linkTypeEnum.end_end + "]").hide().prop("disabled", true);
        //}

        //if (possibleLinkTypes[2] == -1 && possibleLinkTypes[3] == -1) {
        //    linkTypeSelector.find("option[value=" + ASC.Projects.Common.linkTypeEnum.start_end + "]").hide().prop("disabled", true);
        //    linkTypeSelector.find("option[value=" + ASC.Projects.Common.linkTypeEnum.end_start + "]").hide().prop("disabled", true);
        //    return;
        //}
        //if (possibleLinkTypes[2] == ASC.Projects.Common.linkTypeEnum.start_end) {
        //    linkTypeSelector.find("option[value=" + ASC.Projects.Common.linkTypeEnum.end_start + "]").hide().prop("disabled", true);
        //    linkTypeSelector.find("option[value=" + ASC.Projects.Common.linkTypeEnum.start_end + "]").show().prop("disabled", false);
        //} else {
        //    linkTypeSelector.find("option[value=" + ASC.Projects.Common.linkTypeEnum.start_end + "]").hide().prop("disabled", true);
        //    linkTypeSelector.find("option[value=" + ASC.Projects.Common.linkTypeEnum.end_start + "]").show().prop("disabled", false);
        //}
    };

    var getDelay = function(firstDate, secondDate){
        var delay = Math.abs(firstDate.getTime() - secondDate.getTime());
        delay = Math.floor(delay / 1000 / 60 / 60 / 24);
        if(delay == 0) return 0;
        if (delay == 1) {
            return delay + " " + ASC.Projects.Resources.ProjectsJSResource.DelayDay;
        } else {
            return delay + " " + ASC.Projects.Resources.ProjectsJSResource.DelayDays;
        }
    };

    var getRelatedTaskObject = function (firstTask, secondTask, link) {

        var firstTaskStart = firstTask.startDate ? firstTask.startDate : firstTask.crtdate;
        var secondTaskStart = secondTask.startDate ? secondTask.startDate : secondTask.crtdate;
        var firstTaskDeadline = firstTask.deadline ? firstTask.deadline : firstTask.milestone ? firstTask.milestone.deadline : undefined;
        var secondTaskDeadline = secondTask.deadline ? secondTask.deadline : secondTask.milestone ? secondTask.milestone.deadline : undefined;

        var relatedTaskObject = {};
        relatedTaskObject.invalidLink = false;
        relatedTaskObject.delay = 0;
        relatedTaskObject.possibleLinkType = ASC.Projects.Common.getPossibleTypeLink(firstTaskStart, firstTaskDeadline, secondTaskStart, secondTaskDeadline, relatedTaskObject);


        relatedTaskObject.linkType = link.linkType;

        if (link.linkType == 2) {
            if (link.parentTaskId == currentTask.id) {
                relatedTaskObject.linkType = ASC.Projects.Common.linkTypeEnum.end_start;
                if (relatedTaskObject.possibleLinkType[3] != ASC.Projects.Common.linkTypeEnum.end_start) {
                    relatedTaskObject.invalidLink = true;
                }
            } else {
                relatedTaskObject.linkType = ASC.Projects.Common.linkTypeEnum.start_end;
                if (relatedTaskObject.possibleLinkType[2] != ASC.Projects.Common.linkTypeEnum.start_end) {
                    relatedTaskObject.invalidLink = true;
                }
            }
        }

        switch (relatedTaskObject.linkType) {
            case ASC.Projects.Common.linkTypeEnum.start_start:
                relatedTaskObject.linkTypeText = ASC.Projects.Resources.ProjectsJSResource.RelatedLinkTypeSS;
                relatedTaskObject.delay = getDelay(firstTaskStart, secondTaskStart);
                relatedTaskObject.invalidLink = false;
                break;
            case ASC.Projects.Common.linkTypeEnum.end_end:
                relatedTaskObject.linkTypeText = ASC.Projects.Resources.ProjectsJSResource.RelatedLinkTypeEE;
                relatedTaskObject.delay = getDelay(firstTaskDeadline, secondTaskDeadline);
                relatedTaskObject.invalidLink = false;
                break;
            case ASC.Projects.Common.linkTypeEnum.start_end:
                relatedTaskObject.linkTypeText = ASC.Projects.Resources.ProjectsJSResource.RelatedLinkTypeSE;
                relatedTaskObject.delay = getDelay(secondTaskDeadline, firstTaskStart);
                break;
            case ASC.Projects.Common.linkTypeEnum.end_start:
                relatedTaskObject.linkTypeText = ASC.Projects.Resources.ProjectsJSResource.RelatedLinkTypeES;
                relatedTaskObject.delay = getDelay(firstTaskDeadline, secondTaskStart);
                break;
            default: break;
        }

        return relatedTaskObject;
    };

    var getRelatedTaskTmpl = function(taskId, link) {
        var task = listAllProjectTasks[taskId];
        
        if (task) {
            task.relatedTaskObject = getRelatedTaskObject(currentTask, task, link);
            return task;
        } else {
            return null;
        }
    };

    var showEditLinkBox = function(editFlag, taskId) {              
        if (editFlag) {
            var taskCont = relatedTasksCont.find("tr[data-taskid="+taskId+"]");
            editLinkBox.css("position", "absolute");
            var marginTop = taskCont.offset().top - jq("#linkedTasksContainer").offset().top - 17 + "px"
            editLinkBox.css("top", marginTop);
            editLinkBox.attr("data-edit", "edit");
            var option = taskSelector.find("option[value="+taskId+"]");
            option.removeClass("display-none");
            option.prop("selected", true);
            taskSelector.change();
            var link = relatedTasks[taskId];
            editedLink = link;
            editedLink.targetTaskId = taskId;
            var linkType = parseInt(taskCont.find(".link-type").data("type"));
            linkTypeSelector.data("value", linkType);
            //linkTypeSelector.val(linkType);
        } else {
            if (editLinkBox.is(":visible")) {
                hideEditBox(true);
            }
            editLinkBox.addClass("border-top");
            if(!relatedTasksCont.find("tr").length){
                editLinkBox.addClass("border-bottom");
            }
            relatedTasksCont.css("marginTop", "0");
            taskSelector.val("-1");
            linkTypeSelector.text("");
            //linkTypeSelector.prop("disabled", true);
        }
        editLinkBox.removeClass("display-none");
    };

    var hideEditBox = function(escapeFlag){
        if (editedLink) {
            if(escapeFlag){
                taskSelector.find("option[value=" + editedLink.targetTaskId + "]").addClass("display-none");
            }
            editedLink = null;
        }
        taskSelector.val("-1");
        //linkTypeSelector.val("-1");
        //linkTypeSelector.find("option").prop("disabled", false);
        relatedTasksCont.css("marginTop", "16px");
        editLinkBox.css("position", "static");
        editLinkBox.css("marginTop", "16px");    
        editLinkBox.removeClass("border-top");
        editLinkBox.removeClass("border-bottom");
        editLinkBox.addClass("display-none");
    };

    var initTaskLinkSelect = function(){
        var option = taskSelector.find("option:first");
        taskSelector.empty();
        taskSelector.append(option);
        for (var i = 0, max = listValidTaskForLink.length; i < max; i++) {
            option = document.createElement('option');
            option.setAttribute("value", listValidTaskForLink[i].id);
            if (relatedTasks[listValidTaskForLink[i].id]) {
                option.setAttribute("class", "display-none");
            }
            option.appendChild(document.createTextNode(listValidTaskForLink[i].title));
            taskSelector.append(option);
        }
    };
    
    var disableCreateLinkButton = function () {
        if (taskSelector.find(".display-none").length == taskSelector.find("option").length - 1 || currentTask.status == 2) {
            createAddTaskLinkButton.addClass("disabled");
        } else {
            createAddTaskLinkButton.removeClass("disabled");
        }
    };

    var displayRelatedTasks = function() {
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
        relatedTasksCont.empty();
        jq.tmpl("projects_taskLinks", tasksForTmpl).appendTo(relatedTasksCont);
        changeCountInTab(tasksForTmpl.length, "linkedTasksTab");
        hideRelatedTaskLoader();
    };


    var displayNewRelatedTask = function(link, element){
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
            element.after(jq.tmpl("projects_taskLinks", [taskForTmpl]))
        } else {
            jq.tmpl("projects_taskLinks", [taskForTmpl]).prependTo(relatedTasksCont);
        }
        taskSelector.find("option[value=" + linkTaskId + "]").addClass("display-none");
    };

    var displayTaskDescription = function(task) {
        jq(".commonInfoTaskDescription").empty();

        displayTotalInfo(task);
        displaySubtasks(task);
    };

    var showQuestionWindow = function() {
        jq('#questionWindow .end').attr('taskid', taskId);
        jq('#questionWindow .cancel').attr('taskid', taskId);
        StudioBlockUIManager.blockUI(jq("#questionWindow"), 480, 300, 0, "fixed");
    };

    var showQuestionWindowTaskRemove = function() {
        StudioBlockUIManager.blockUI(jq("#questionWindowTaskRemove"), 400, 300, 0, "fixed");
    };

    var showActionsPanel = function (panelId, obj) {
        var objid = '';
        var x, y;

        jq('.studio-action-panel').hide();
        jq('#' + panelId).show();

        if (panelId == 'linkedTaskActionPanel') {
            x = jq(obj).offset().left - (jq("#linkedTaskActionPanel").width() + 6);
            y = jq(obj).offset().top + 17;
        }

        if (typeof y == 'undefined')
            y = jq(obj).offset().top + 26; //TODO

        jq('#' + panelId).css({ left: x, top: y });

        jq('body').click(function(event) {
            var elt = (event.target) ? event.target : event.srcElement;
            var isHide = true;
            if (jq(elt).is('[id="' + panelId + '"]') || (elt.id == obj.id && obj.id.length) || jq(elt).is('.entity-menu')) {
                isHide = false;
            }

            if (isHide)
                jq(elt).parents().each(function() {
                    if (jq(this).is('[id="' + panelId + '"]')) {
                        isHide = false;
                        return false;
                    }
                });

            if (isHide) {
                jq('.studio-action-panel').hide();
                jq('.menuopen').removeClass('menuopen');
            }
        });
    };

    var changeFollowLink = function() {
        var currValue = jq("#followTaskActionTop").attr('title');
        jq("#followTaskActionTop").attr('title', jq("#followTaskActionTop").attr("textValue"));
        jq("#followTaskActionTop").attr('textValue', currValue);
        if (jq("#followTaskActionTop").hasClass("subscribed")) {
            jq("#followTaskActionTop").removeClass("subscribed").addClass("unsubscribed");
        } else {
            jq("#followTaskActionTop").removeClass("unsubscribed").addClass("subscribed");
        }

    };
    var subscribeTask = function() {
        jq("#taskActions").hide();
        jq(".project-title .menu-small").removeClass("active");
        Teamlab.subscribeToPrjTask({}, taskId);
        changeFollowLink();
    };

    var removeTask = function() {
        Teamlab.removePrjTask({}, taskId, { success: onRemoveTask, error: onErrorRemoveTask });
    };

    var addTaskLink = function(link, removedLinkTaskId){
        Teamlab.addPrjTaskLink({link: link, removedLinkTaskId: removedLinkTaskId }, link.parentTaskId, link, { success: onAddTaskLink, error: onAddTaskLinkError });
    };

    var removeTaskLink = function (taskId, removeElemFlag) {
        var link = relatedTasks[taskId];
        Teamlab.removePrjTaskLink({ taskId: taskId, removeElemFlag: removeElemFlag }, link.dependenceTaskId, { dependenceTaskId: link.dependenceTaskId, parentTaskId: link.parentTaskId }, { success: onRemoveTaskLink, error: function (params, error) { } });
    };

    var showRelatedTaskLoader = function () {
        relatedTaskLoader.removeClass("display-none");
    };

    var hideRelatedTaskLoader = function () {
        relatedTaskLoader.addClass("display-none");
    };

    /*--------event handlers--------*/

    var onRemoveTaskLink = function (params, task) {
        if (params.removeElemFlag) {
            relatedTasksCont.find("tr[data-taskid=" + params.taskId + "]").remove();
            LoadingBanner.hideLoading();
            changeCountInTab("delete", "linkedTasksTab");
            if (relatedTasksCont.find("tr").length == 0) {
                jq("#switcherLinkedTasksButton").hide();
            }
        }
        delete relatedTasks[params.taskId];
        editedLink = null;
        taskSelector.find("option[value=" + params.taskId + "]").removeClass("display-none");
        disableCreateLinkButton();
    };

    var onAddTaskLink = function (params, task) {
        hideEditBox();
        
        if (params.removedLinkTaskId) {
            var removeElem = relatedTasksCont.find("tr[data-taskid=" + params.removedLinkTaskId + "]");
            displayNewRelatedTask(params.link, removeElem);
            removeElem.remove();
        } else {
            displayNewRelatedTask(params.link);
            changeCountInTab("add", "linkedTasksTab");            
        }
        jq("#switcherLinkedTasksButton").show();
        disableCreateLinkButton();
        LoadingBanner.hideLoading();
    };

    var onAddTaskLinkError = function (params, error) {
        LoadingBanner.hideLoading();

        StudioBlockUIManager.blockUI(jq("#createNewLinkError"), 480, 300, 0, "fixed");

        hideEditBox();
        reloadedTaskListFlag = true;
        getProjectTasks();
    };

    var onGetTaskDescription = function (params, task) {
        displayTaskDescription(task);
        currentTask = task;
        if (jq("#filesContainer").length && !task.canEdit) {
            Attachments.banOnEditing();
        }
        if (currentTask.projectOwner.status != 0) {
            // remove Edit from task action menu
        }
        if (currentTask.links) {
            showRelatedTaskLoader();
        }
        getProjectTasks();

        taskId = task.id;

        if (jq("#filesContainer").length && window.Attachments) {
            initAttachmentsControl();
            if (jq("#filesTab .count").text().trim() != "")
                jq("#switcherTaskFilesButton").show();
        }
        if (task.canCreateTimeSpend) {
            jq(".project-total-info .menu-small").removeClass("visibility-hidden");
        }
      
        hideGanttLink();

        LoadingBanner.hideLoading();
    };

    var onUpdateTask = function(params, task) {
        currentTask = task;
        if (!task.canEdit) {
            Attachments.banOnEditing();
        }
        jq(".commonInfoTaskDescription").empty();
        displayTaskDescription(task);
        jq("#essenceTitle").text(task.title);
        jq("#essenceTitle").attr("title", task.title);
        jq.unblockUI();
        ASC.Projects.Common.displayInfoPanel(ASC.Projects.Resources.ProjectsJSResource.TaskUpdated);
    };

    var onRemoveTask = function() {
        jq.unblockUI();
        var newUrl = "tasks.aspx?prjID=" + projId;
        window.location.replace(newUrl);
    };

    var onErrorRemoveTask = function(param, error) {
        var removePopupErrorBox = jq("#questionWindowTaskRemove .errorBox");
        removePopupErrorBox.text(error[0]);
        removePopupErrorBox.removeClass("display-none");
        jq("#questionWindowTaskRemove .middle-button-container").css('marginTop', '8px');
        setTimeout(function() {
            removePopupErrorBox.addClass("display-none");
            jq("#questionWindowTaskRemove .middle-button-container").css('marginTop', '32px');
        }, 3000);
    };

    var onRemoveSubtask = function(params, subtask) {
        if (subtask.status == 1) {
            changeCountInTab("delete", "subtaskTab");
        }
        else {
            if (jq(".subtasks").find(".subtask").length == 0) {
                if (currentTask.status == 1) {
                    jq("#subtaskTab").addClass("display-none");
                }
            }
        }
        if (!jq(".subtasks .subtask").length) {
            jq("#switcherSubtasksButton").hide();
        }
    };

    var onAddSubtask = function(params, subtask) {
        jq("#switcherSubtasksButton").show();
        changeCountInTab("add", "subtaskTab");
    };

    var onUpdateSubtask = function(params, subtask) {
        ASC.Projects.SubtasksManager.updateSubtask(subtask);
    };

    var onUpdateSubtaskStatus = function(params, subtask) {
        if (subtask.status == 2) {
            changeCountInTab('delete', 'subtaskTab');
        } else {
            changeCountInTab('add', 'subtaskTab');
        }
    };

    var onChangeTaskStatus = function(params, task) {
        currentTask = task;
        displayTaskDescription(task);
        jq("body").css("cursor", "default");
        var headerStatus = jq(".project-total-info .header-status");
        if (task.status == 2) {
            headerStatus.text(headerStatus.attr("data-text"));
            headerStatus.show();
            jq("#taskActions #editTaskAction").parent("li").hide();
            if (task.subtasks.length == 0) {
                jq("#subtaskTab").addClass("display-none");
                jq("#subtaskContainer").addClass("display-none");
            }
        } else {
            headerStatus.hide();
            jq("#taskActions li").show();
            jq("#subtaskTab").removeClass("display-none");
            jq("#subtaskContainer").removeClass("display-none");
        }
        disableCreateLinkButton();
    };

    var onAddTask = function(params, task) {
        document.location = "tasks.aspx?prjID=" + task.projectOwner.id + "&id=" + task.id;
    };

    var onGetRelatedTasks = function(params, tasks){
        var taskCount = tasks.length;

        if (taskCount) {
            for (var i = 0; i < taskCount; i++) {
                if (tasks[i].status == 2) {
                    listAllProjectTasks[tasks[i].id] = tasks[i];
                }
            }
            displayRelatedTasks();
        }
    };

    var onGetOpenTask = function (params, tasks) {
        var taskCount = tasks.length;
        if (taskCount) {
            for (var i = 0; i < taskCount; i++) {
                if (tasks[i].id == currentTask.id) {
                    currentTask = tasks[i];
                    if (reloadedTaskListFlag) {
                        jq(".commonInfoTaskDescription").empty();
                        displayTotalInfo(currentTask);
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

        if (currentTask.links) {
            displayRelatedTasks();
            jq("#switcherLinkedTasksButton").show();
        }
        initTaskLinkSelect();
        disableCreateLinkButton();
    };

    /*-------tabs-----*/

    var createVisible = function(block) {
        jq(block).toggle();
    };

    var showSubtasks = function() {
        createVisible("#subtaskContainer");
        jq("#commentBox").hide();
        jq("#fckbodycontent").val("");

    };

    var showFiles = function() {
        createVisible("#filesContainer");
        jq("#commentBox").hide();
        jq("#fckbodycontent").val("");

    };

    var showComments = function() {
        createVisible("#commentContainer");
        showEmptyCommentsPanel();
    };

    var showEmptyCommentsPanel = function() {
        if (jq("#commentContainer #mainContainer div[id^='container_']").length == 0) {
            jq("#emptyCommentsPanel").show();
        }
        else {
            jq("#add_comment_btn").show();
        }
    };

    var changeCountInTab = function(actionOrCount, tabAnchorId) {
        var currentCount;
        var text = jq.trim(jq("#" + tabAnchorId).find(".count").text());
        if (text == "") currentCount = 0;
        else {
            text = text.substr(1, text.length - 2);
            currentCount = parseInt(text);
        }

        if (typeof (actionOrCount) == "string") {
            if (actionOrCount == "add") {
                currentCount++;
                jq("#" + tabAnchorId).find(".count").text("(" + currentCount + ")");
            }
            else if (actionOrCount == "delete") {
                currentCount--;
                if (currentCount != 0) {
                    jq("#" + tabAnchorId).find(".count").text("(" + currentCount + ")");
                }
                else {
                    jq("#" + tabAnchorId).find(".count").empty();

                    if (tabAnchorId == "commentsTab") {
                        jq("#commentsListWrapper").hide();
                        jq("#emptyCommentsPanel").show();
                    }
                }
            }
        }
        else if (typeof (actionOrCount) == "number") {
            var count = parseInt(actionOrCount);
            if (count > 0) {
                jq("#" + tabAnchorId).find(".count").text("(" + count + ")");
            } else {
                jq("#" + tabAnchorId).find(".count").empty();
            }
        }
    };
    /*---------actions--------*/
    var closeTask = function() {
        jq("body").css("cursor", "wait");

        Teamlab.updatePrjTask({}, taskId, { 'status': 2 }, { success: onChangeTaskStatus });
    };
    var resumeTask = function() {
        jq("body").css("cursor", "wait");

        Teamlab.updatePrjTask({}, taskId, { 'status': 1 }, { success: onChangeTaskStatus });

        jq(".pm_taskTitleClosedByPanel").hide();
    };

    var editTask = function() {
        jq('.studio-action-panel').hide();
        ASC.Projects.TaskAction.showUpdateTaskForm(currentTask.id, currentTask);
    };

    var formatDescription = function(descr) {
        var formatDescr = descr.replace(/</ig, '&lt;').replace(/>/ig, '&gt;').replace(/\n/ig, '<br/>');
        return formatDescr.replace('&amp;', '&');
    };

    var compareDates = function(data) {
        var currentDate = new Date();
        if (currentDate > data) {
            return true;
        }
        if (currentDate <= data) {
            return false;
        }
    };
    return {
        init: init,
        onAddTask: onAddTask,
        onUpdateTask: onUpdateTask,
        showSubtasks: showSubtasks,
        showFiles: showFiles,
        showComments: showComments,
        changeCountInTab: changeCountInTab,
        subscribeTask: subscribeTask,
        compareDates: compareDates,

        closeTask: closeTask,
        resumeTask: resumeTask,
        showQuestionWindowTaskRemove: showQuestionWindowTaskRemove,
        editTask: editTask,
        removeTask: removeTask,
        formatDescription: formatDescription,
        setTimeSpent: setTimeSpent,
        showActionsPanel: showActionsPanel
    };


})(jQuery);
