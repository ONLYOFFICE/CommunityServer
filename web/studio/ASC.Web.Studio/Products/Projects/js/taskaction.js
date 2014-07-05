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
ASC.Projects.TaskAction = (function() {
    var isInit;
    var updateTaskFlag = false;
    var firstLoadFlag = true;
    var currentTask = null;

    var taskPopup = null;
    /*---task fields----*/

    //text
    var taskTitle = null;
    var taskDescription = null;

    //selectors
    var taskProjectSelector = null;
    var taskMilestoneSelector = null;
    var taskResponsiblesSelector = null;

    //dates
    var taskStartDate = null;
    var taskDeadlineDate = null;

    //other
    var notifyCheckbox = null;
    var priorityCheckbox = null;
    var listTaskResponsibles = null;
    /*---*/

    var currentPage = null;
    var currentUserId;
    var currentProjectId;
    var choosedProjectMilestones = [];
    var choosedMilestone = null;

    var isInitData;

    var filterProjectsIds = [];
    var projectItems = [];
    var milestonesItems = [];
    var noneMilestone = { id: 0, title: ASC.Projects.Resources.TasksResource.None, deadline: "" };
    function mapPrj(item) {
        return { id: item.value, title: item.title };
    }
    
    var init = function () {
        if (isInit) {
            return;
        }

        isInit = true;

        initTaskFormElementsAndConstants();

        taskResponsiblesSelector.on('change', function (evt) {
            var value = jq(this).val();
            if (value == -1) {
                jq(this).val('-1');
                return;
            }
            var userName = jq(this).find('option[value="' + value + '"]').html();
            if (!listTaskResponsibles.find("div[data-value='" + value + "']").length) {
                listTaskResponsibles.show().append('<div data-value="' + value + '" class="user">' + userName + '</div>');
                jq('.userAdd .combobox-container li.option-item[data-value="' + value + '"]').hide();
                var addedUserCount = listTaskResponsibles.find(".user").length;
                var usersCount = taskResponsiblesSelector.find("option").length - 1;
                if (addedUserCount == usersCount) {
                    taskResponsiblesSelector.tlcombobox(false);
                }
            }
            jq(this).val('-1').change();
            showOrHideNotifyCheckbox();
            evt.stopPropagation();
        });

        taskPopup.on('click', '#fullFormUserList .user', function () {
            value = jq(this).attr('data-value');
            jq(this).remove();
            taskResponsiblesSelector.tlcombobox(true);
            jq('.userAdd .combobox-container li.option-item[data-value="' + value + '"]').show();
            if (!listTaskResponsibles.find(".user").length) {
                listTaskResponsibles.hide();
            }
            showOrHideNotifyCheckbox();
        });

        taskPopup.on('click', '.deadline_left', function () {
            taskPopup.find('.deadline_left').css('border-bottom', '1px dotted').css('font-weight', 'normal');
            jq(this).css('border-bottom', 'none').css('font-weight', 'bold');
            var daysCount = parseInt(jq(this).attr('data-value'), 10);
            var date = new Date();
            date.setDate(date.getDate() + daysCount);
            taskDeadlineDate.datepicker('setDate', date);
        });

        jq('#saveTaskAction, #createTaskAndCreateNew').on('click', function () {
            clearErrorMessages();
            var data = {};
            data.title = jq.trim(taskTitle.val());
            data.description = taskDescription.val();
            var responsibles = listTaskResponsibles.find(".user");
            if (responsibles.length) {
                data.responsibles = [];
                responsibles.each(function () {
                    data.responsibles.push(jq(this).attr('data-value'));
                });
            }

            if (notifyCheckbox.is(':checked')) {
                data.notify = true;
            }

            data.milestoneId = taskMilestoneSelector.attr("data-id");
            data.priority = priorityCheckbox.is(':checked') ? 1 : 0;

            var isError = false;
            if (!data.title.length) {
                taskPopup.find('.titlePanel').addClass('requiredFieldError');
                taskPopup.find('.requiredErrorText.title').html(taskPopup.find('.requiredErrorText').attr('error'));
                isError = true;
            }

            data.projectId = taskProjectSelector.attr("data-id");
            if (!data.projectId && !updateTaskFlag) {
                taskPopup.find('.requiredErrorText.project').show().html(taskPopup.find('.requiredErrorText.project').attr('error'));
                isError = true;
            }

            if (compareTaskDatesAndShowError()) {
                isError = true;
            } else {
                if (taskDeadlineDate.val().length) {
                    data.deadline = taskDeadlineDate.datepicker('getDate');
                    data.deadline.setHours(0);
                    data.deadline.setMinutes(0);
                    data.deadline = Teamlab.serializeTimestamp(data.deadline);
                }

                if (taskStartDate.val().length) {
                    data.startDate = taskStartDate.datepicker('getDate');
                    data.startDate.setHours(0);
                    data.startDate.setMinutes(0);
                    data.startDate = Teamlab.serializeTimestamp(data.startDate);
                }
            }

            if (isError) {
                return;
            }

            lockTaskForm();

            var params = { saveAndView: false };
            if (jq(this).attr("id") == "saveTaskAction") {
                params.saveAndView = true;
            }

            if (updateTaskFlag) {
                var block;
                if ((currentTask.milestoneId || 0) != parseInt(data.milestoneId, 10)) {
                    block = jq("#removeTaskLinksQuestionPopup");
                }
                
                var endStartLinks;
                
                if (currentTask.links) {
                    endStartLinks = currentTask.links.some(function (item) {
                        return item.linkType == 2 && item.parentTaskId == currentTask.id;
                    });
                }

                if (!data.deadline && endStartLinks) {
                    block = jq("#removeTaskLinksQuestionPopupDeadLine");
                }
                
                if (currentTask.links && block) {
                    MoveTaskQuestionPopup.setParametrs("#addTaskPanel", currentTask.links,
                        function() { updateTask({}, currentTask.id, data); },
                        showTaskFormAfterQuestionPopup, block);
                    MoveTaskQuestionPopup.showDialog();

                } else {
                    updateTask({}, currentTask.id, data);
                }
            } else {
                var project = taskProjectSelector.attr("data-id");
                addTask(params, project, data);
            }
        });

        jq('#addTaskPanel #closeTaskAction').on('click', function () {
            closeTaskForm();
        });
    };
    
    var initData = function () {
        if (isInitData) {
            return;
        }

        isInitData = true;
        setProjectCombobox();
        setMilestonesCombobox();
        taskResponsiblesSelector.tlcombobox();
        
        if (currentProjectId) {
            onGetTeam({}, ASC.Projects.Master.Team);
            onGetMilestones({}, ASC.Projects.Master.Milestones);
        }
    };
    
    var setProjectCombobox = function () {
        function sortPrj(item) {
            return item.canCreateTask;
        }
        
        projectItems = ASC.Projects.Common.getProjectsForFilter().filter(sortPrj).map(mapPrj);
        
        taskProjectSelector.projectadvancedSelector(
           {
               itemsChoose: projectItems,
               onechosen: true,
               inPopup: true,
               sortMethod: function () { return 0; }
           }
       );

        taskProjectSelector.on("showList", function (event, item, milestoneResponsibleId) {
            jq("#taskProject").attr("data-id", item.id).text(item.title).attr("title", item.title);
            taskProjectSelectorOnChange(item, milestoneResponsibleId);
        });
    };
    
    var setMilestonesCombobox = function () {
        var selectorObj = {
            onechosen: true,
            inPopup: true,
            sortMethod: ASC.Projects.Common.milestoneSort,
            noresults: ASC.Resources.Master.Resource.MilestoneSelectorNoResult,
            noitems: ASC.Resources.Master.Resource.MilestoneSelectorNoItems
        };

        taskMilestoneSelector.projectadvancedSelector(selectorObj);

        taskMilestoneSelector.on("showList", function (event, item) {
            taskMilestoneSelector.attr("data-id", item.id).text(item.title).attr("title", item.title);
            taskMilestoneSelectorOnChange(item);
        });
    };
    
    // events handlers
    var taskProjectSelectorOnChange =  function (item) {
        var selectedPrjId = item.id;
        if (selectedPrjId > 0) {
            jq('.popupActionPanel').hide();
            taskPopup.find('.requiredErrorText.project').hide();
            jq('#pm-milestoneBlock').removeClass("display-none");
            jq('#pm-respBlock').removeClass("display-none");

            listTaskResponsibles.html('').hide();

            jq(taskResponsiblesSelector).tlcombobox(false);

            if (selectedPrjId == currentProjectId) {
                onGetMilestones({}, ASC.Projects.Master.Milestones);
                onGetTeam({}, ASC.Projects.Master.Team);
            } else {
                getMilestones({}, selectedPrjId);
                getTeam({}, selectedPrjId);
            }
        }
    };
    
    var taskMilestoneSelectorOnChange = function (item) {
        var milestoneId = item.id;
        if (milestoneId == "0") {
            choosedMilestone = null;
        } else {
            var milestone = getMilestoneById(parseInt(milestoneId), 10);
            choosedMilestone = milestone;
        }
    };

    var getCurrentPage = function() {
        var url = document.location.href;
        if (url.indexOf("tasks.aspx") > 0) {
            currentPage = "tasks";
            var projId = jq.getURLParam("prjID");
            var id = jq.getURLParam("id");
            if (projId && id)
                currentPage = "taskdescription";
        }

        if (url.indexOf("milestones.aspx") > 0)
            currentPage = "milestones";
        
        if (url.indexOf("ganttchart.aspx") > 0)
            currentPage = "ganttchart";
    };

    var initTaskFormElementsAndConstants = function() {

        currentUserId = Teamlab.profile.id;
        currentProjectId = jq.getURLParam('prjID');

        taskPopup = jq('#addTaskPanel');

        //text
        taskTitle = jq("#addtask_title");
        taskDescription = jq("#addtask_description");

        //selectors
        taskProjectSelector = jq("#taskProject");
        taskMilestoneSelector = jq("#taskMilestone");
        taskResponsiblesSelector = jq("select#taskResponsible");

        //dates
        taskStartDate = jq("#taskStartDate");
        taskDeadlineDate = jq("#taskDeadline");

        //others
        notifyCheckbox = jq("#notify");
        priorityCheckbox = jq("#priority");
        listTaskResponsibles = jq("#fullFormUserList");

        taskDescription.autosize();
        jq(taskProjectSelector, taskMilestoneSelector, taskResponsiblesSelector).css('max-width', 300);

        taskStartDate.mask(ASC.Resources.Master.DatePatternJQ);
        taskDeadlineDate.mask(ASC.Resources.Master.DatePatternJQ);
        taskDeadlineDate.datepicker({ selectDefaultDate: false });
        taskStartDate.datepicker({ selectDefaultDate: false });

        jq(taskDeadlineDate).on("keydown", function (e) { if (e.keyCode == 13) {taskDeadlineDate.blur();}});
        jq(taskDeadlineDate).on("change", function () { taskDeadlineDate.blur(); });
        
        jq(taskStartDate).on("keydown", function (e) { if (e.keyCode == 13) { taskStartDate.blur();}});
        jq(taskStartDate).on("change", function (e) { taskStartDate.blur();});

        getCurrentPage();
    };


    var compareTaskDatesAndShowError = function() {
        if (taskStartDate.val().trim() == "" && taskDeadlineDate.val().trim() == "") return false;

        var startDate = taskStartDate.datepicker('getDate');
        var deadlineDate = taskDeadlineDate.datepicker('getDate');
        var milestoneDeadline = choosedMilestone ? choosedMilestone.deadline : null;

        var errorFlag = false;

        if ((startDate && deadlineDate) && (startDate > deadlineDate)) {
            var errorStartDate = taskPopup.find(".startDate-error");
            errorStartDate.text(errorStartDate.attr("error"));
            errorStartDate.show();
            taskStartDate.addClass("red-border");
            errorFlag = true;
        }

        return errorFlag;
    };

    var clearErrorMessages = function() {
        taskPopup.find('.titlePanel').removeClass('requiredFieldError');
        taskPopup.find('.requiredErrorText').html('');
        taskPopup.find('.requiredErrorText.project').hide();
        taskPopup.find(".startDate-error").hide();
        taskStartDate.removeClass("red-border");
        taskDeadlineDate.removeClass("red-border");
    };

    var showOrHideNotifyCheckbox = function() {
        var usersCount = listTaskResponsibles.find(".user").length;
        if (!usersCount || usersCount == listTaskResponsibles.find(".user[data-value=" + currentUserId + "]").length) {
            taskPopup.find('.notify').hide();
            return;
        }
        taskPopup.find('.notify').show();
    };

    var lockTaskForm = function() {
        taskTitle.attr("disabled", "disabled");
        taskDescription.attr("disabled", "disabled");
        taskStartDate.attr("disabled", "disabled");
        taskDeadlineDate.attr("disabled", "disabled");
        notifyCheckbox.attr("disabled", "disabled");
        priorityCheckbox.attr("disabled", "disabled");

        jq(".success-popup, .error-popup").hide();

        LoadingBanner.showLoaderBtn("#addTaskPanel");
    };

    var unlockTaskForm = function() {
        taskTitle.removeAttr("disabled");
        taskDescription.removeAttr("disabled");
        taskStartDate.removeAttr("disabled");
        taskDeadlineDate.removeAttr("disabled");
        notifyCheckbox.removeAttr("disabled");
        priorityCheckbox.removeAttr("disabled");

        
        LoadingBanner.hideLoaderBtn("#addTaskPanel");
    };

    var addTask = function(params, projectId, data) {
        Teamlab.addPrjTask(params, projectId, data, { success: onAddTask, error: onTaskError });
    };

    var updateTask = function (params, taskId, data) {
        getCurrentPage();
        var success;
        switch (currentPage) {
            case "tasks":
                success = ASC.Projects.TasksManager.onUpdateTask;
                break;
            case "taskdescription":
                success = ASC.Projects.TaskDescroptionPage.onUpdateTask;
                break;
            default:
                success = null;
        }
        Teamlab.updatePrjTask(params, taskId, data, { success: success, error: onTaskError });
    };

    var onAddTask = function (params, task) {
        getCurrentPage();
        
        if (currentPage == "ganttchart") {
            ASC.Projects.GantChartPage.addTaskToChart(task, true, true);
        }

        if (currentPage == "tasks" && (currentProjectId == null || task.projectOwner.id == currentProjectId)) {
            ASC.Projects.TasksManager.onAddTask(params, task);
        }

        if (currentPage == "milestones" && (currentProjectId == null || task.projectOwner.id == currentProjectId)) {
            ASC.Projects.AllMilestones.onAddTask(params, task);
        }

        if (currentProjectId == task.projectId && ASC.Projects.projectNavPanel) {
            ASC.Projects.projectNavPanel.changeModuleItemsCount(ASC.Projects.projectNavPanel.projectModulesNames.tasks, "add");
        }

        if (params.saveAndView) {
            closeTaskForm();
            ASC.Projects.Common.displayInfoPanel(ASC.Projects.Resources.ProjectsJSResource.TaskAdded);
        }
        else {
            taskPopup.find('#taskLink').attr("href", "tasks.aspx?prjID=" + task.projectId + "&id=" + task.id);
            unlockTaskForm();
            renderTaskForm(getEmptyTask());
            jq("#addTaskPanel .success-popup").show();
            taskTitle.focus();
        }

        jq("#emptyListTimers .addFirstElement").removeClass("display-none");
    };

    var onTaskError = function(params, error) {
        var taskErrorBox = jq("#addTaskPanel .error-popup");

        LoadingBanner.hideLoaderBtn("#addTaskPanel");
        taskErrorBox.text(error[0]);
        taskErrorBox.show();

        setTimeout(function() {
            taskErrorBox.hide();
        }, 3000);
        unlockTaskForm();
    };

    var getMilestones = function(params, projectId) {
        Teamlab.getPrjMilestones(params, null, { filter: { status: 'open', projectId: projectId }, success: onGetMilestones });
    };

    var onGetMilestones = function (params, milestones) {
        taskMilestoneSelector.projectadvancedSelector("reset");
        
        milestonesItems = milestones.map(function (item) {
            return { id: item.id, title: '[' + item.displayDateDeadline + '] ' + item.title, deadline: item.deadline };
        });

        milestonesItems.unshift(noneMilestone);
        choosedProjectMilestones = milestones;
        
        taskMilestoneSelector.projectadvancedSelector("rewriteItemList", milestonesItems, []);
        

        if (currentTask) {
            var taskMile = milestonesItems.find(function (item) {
                return item.id == currentTask.milestoneId;
            });
            taskMilestoneSelector.projectadvancedSelector("selectBeforeShow", taskMile || noneMilestone);
        }
    };

    var getTeam = function(params, projectId) {
        Teamlab.getPrjTeam(params, projectId, { success: onGetTeam });
    };

    var onGetTeam = function(params, team) {
        var teamWithoutVisitors = ASC.Projects.Common.excludeVisitors(team);
        teamWithoutVisitors = ASC.Projects.Common.removeBlockedUsersFromTeam(teamWithoutVisitors);
        var teamInd = teamWithoutVisitors ? teamWithoutVisitors.length : 0;

        taskResponsiblesSelector.find('option[value!=0][value!=-1]').remove();

        if (teamInd != 0) {
            jq("#noActiveParticipantsTaskNote").addClass("display-none");

            for (var i = 0; i < teamInd; i++) {
                var user = teamWithoutVisitors[i];
                taskResponsiblesSelector.append(jq('<option value="' + user.id + '"></option>').html(user.displayName));
            }
            taskResponsiblesSelector.tlcombobox();
            listTaskResponsibles.empty();
            setResponsibles();
        } else {
            jq("#noActiveParticipantsTaskNote").removeClass("display-none");
            taskResponsiblesSelector.tlcombobox();
            taskResponsiblesSelector.tlcombobox(false);
        }
    };

    var setResponsibles = function() {
        if (currentTask && currentTask.responsibles.length) {
            listTaskResponsibles.empty();
            jQuery.each(currentTask.responsibles, function() {
                var elem = jq('.userAdd .combobox-container li.option-item[data-value="' + this.id + '"]');
                if (elem.length) {
                    var name = elem.text();
                    listTaskResponsibles.append('<div data-value="' + this.id + '" class="user">' + Encoder.htmlEncode(name) + '</div>');
                    elem.hide();
                }
            });
            var users = listTaskResponsibles.find(".user");
            if (jq('.userAdd .combobox-container li.option-item').length - 1 == users.length) {
                taskResponsiblesSelector.tlcombobox(false);
            }
            if (users.length) {
                listTaskResponsibles.show();
            }
        }
        if (currentTask && !currentTask.responsibles.length && updateTaskFlag){
            listTaskResponsibles.empty();
        }
        showOrHideNotifyCheckbox();
    };

    var getMilestoneById = function(id) {
        var milestonesInd = choosedProjectMilestones ? choosedProjectMilestones.length : 0;
        while (milestonesInd--) {
            if (id == choosedProjectMilestones[milestonesInd].id) return choosedProjectMilestones[milestonesInd];
        }
    };

    var getEmptyTask = function() {
        var task = {};
        task.title = "";
        task.description = "";
        if (currentProjectId) {
            task.projectId = currentProjectId;
        }
        task.responsibles = [];
        task.priority = null;
        task.startDate = null;
        task.deadline = null;
        task.milestoneId = null;

        return task;
    };

    var isNeedChangeProject = function(projectId) {
        if (projectId && (firstLoadFlag || taskProjectSelector.attr("data-id") != projectId.toString())) return true;
        return false;
    };

    var renderTaskForm = function(task) {
        clearErrorMessages();
        taskPopup.find('.success-popup, .error-popup').hide();
        // task body
        taskTitle.val(task.title);
        taskDescription.val(task.description);

        if (isNeedChangeProject(task.projectId)) {
            var taskProject = projectItems.find(function (item) { return item.id == task.projectId; }) || task.projectOwner;
            if (taskProject) {
                taskProjectSelector.projectadvancedSelector("selectBeforeShow", taskProject);
            } else {
                jq("#pm-milestoneBlock").hide();
                getMilestones({}, task.projectId);
                getTeam({}, task.projectId);
            }
        } else {
            if (currentProjectId) {
                onGetMilestones({}, ASC.Projects.Master.Milestones);
            }

            if (task.milestoneId) {
                var taskMile = milestonesItems.find(function(item) { return item.id == task.milestoneId; });
                taskMilestoneSelector.projectadvancedSelector("selectBeforeShow", taskMile || noneMilestone);
            }
            
            setResponsibles();
        }

        if (task.deadline) {
            taskDeadlineDate.datepicker('setDate', task.deadline);
            var elemDurationDays = taskDeadlineDate.siblings('.dottedLink');
            elemDurationDays.css('border-bottom', '1px dotted');
            elemDurationDays.css('font-weight', 'normal');
        }else{
            if(updateTaskFlag) taskDeadlineDate.datepicker('setDate', null);
        }

        if (task.startDate) {
            taskStartDate.datepicker('setDate', task.startDate);
        }else{
            if(updateTaskFlag) taskStartDate.datepicker('setDate', null);
        }

        if (task.priority) {
            priorityCheckbox.prop("checked", true);
        } else {
            priorityCheckbox.prop("checked", false);
        }

        if (updateTaskFlag) {
            setDescriptionHeight();
            jq("#pm-projectBlock").hide();
        } else {
            taskDescription.height(60);
            jq('#pm-projectBlock').show();
        }

        //buttons and title
        var saveButton = taskPopup.find('#saveTaskAction');
        var createNewButton = jq("#createTaskAndCreateNew");

        if (updateTaskFlag) {
            createNewButton.hide();
            createNewButton.siblings(".splitter-buttons").first().hide();
            saveButton.html(saveButton.attr('update'));
            taskPopup.find('.containerHeaderBlock table td:first').html(ASC.Projects.Resources.ProjectsJSResource.EditThisTask);
        } else {
            createNewButton.show();
            createNewButton.siblings(".splitter-buttons").show();

            saveButton.html(saveButton.attr('add'));
            taskPopup.find('.containerHeaderBlock table td:first').html(ASC.Projects.Resources.ProjectsJSResource.CreateNewTask);
        }

        if (firstLoadFlag) {
            firstLoadFlag = false;
        }
        LoadingBanner.hideLoading();
    };

    var showTaskForm = function(task) {
        currentTask = task;
        unlockTaskForm();
        renderTaskForm(task);
        StudioBlockUIManager.blockUI(jq("#addTaskPanel"), 550, 550, 0, "absolute");
    };

    var showTaskFormAfterQuestionPopup = function () {
        unlockTaskForm();
        StudioBlockUIManager.blockUI(jq("#addTaskPanel"), 550, 550, 0, "absolute");
    };

    var closeTaskForm = function() {
        jq.unblockUI();
    };

    var setDescriptionHeight = function() {
        var description = jq("#addtask_description");

        //default
        description.height(60);
        description.css("overflowY", "auto");

        var colsCount = parseInt(description.attr("cols"), 10);
        var text = description.val();
        var countStr = text.split("\n").length;
        if (countStr > 4) {
            if (countStr > 26) {
                description.height(400);
                description.css("overflowY", "scroll");
                return;
            }
            description.height(countStr * 15);
        }
        var signsCount = text.length;
        if (signsCount > colsCount * 6) {
            var rows = Math.floor(signsCount / (colsCount * 2.5));
            if (rows > 27) {
                description.height(400);
                description.css("overflowY", "scroll");
            } else {
                if (rows > countStr) {
                    description.height(rows * 15);
                }
            }
        }
    };

    var showCreateNewTaskForm = function (taskParams) {
        initData();
        
        updateTaskFlag = false;

        var task = getEmptyTask();
        if (taskParams) {
            if (taskParams.projectId) {
                task.projectId = taskParams.projectId;
            }
            if (taskParams.milestoneId) {
                task.milestoneId = taskParams.milestoneId;
            }
            if (taskParams.responsibles) {
                task.responsibles = taskParams.responsibles;
            }
        }

        showTaskForm(task);
    };

    var showUpdateTaskForm = function (taskId, task) {
        initData();
        LoadingBanner.displayLoading();
        updateTaskFlag = true;
        if (task) {
            showTaskForm(task);
            setUpdatedTaskAttr(task);
        } else {
            Teamlab.getPrjTask({}, taskId, function(params, targetTask) {
                showTaskForm(targetTask);
                setUpdatedTaskAttr(targetTask);
            });
        }
    };

    var setUpdatedTaskAttr = function (task) {
        var taskProject = projectItems.find(function(item) { return item.id == task.projectId; }) || task.projectOwner;
        var taskMilestone = milestonesItems.find(function (item) { return item.id == task.milestoneId; });
        
        taskProjectSelector.projectadvancedSelector("selectBeforeShow", taskProject);
        taskMilestoneSelector.projectadvancedSelector("selectBeforeShow", taskMilestone || noneMilestone);
    };

    var onAddNewMileston = function(milestone) {
        if (taskProjectSelector.attr("data-id") == milestone.projectId) {
            milestonesItems.push({ id: milestone.id, title: '[' + milestone.displayDateDeadline + '] ' + milestone.title, deadline: milestone.deadline });
            taskMilestoneSelector.projectadvancedSelector("rewriteItemList", milestonesItems, []);
        }
    };

    var onUpdateTeam = function () {
        if (currentProjectId == taskProjectSelector.attr("data-id")) {
            onGetTeam({}, ASC.Projects.Master.Team);
        }
    };

    var filterProjectsByIdInCombobox = function(ids) {  // only for gantt chart
        if (ids.length === filterProjectsIds.length) {

            var isEqual = true;
            for (var i = ids.length - 1; i >= 0; --i) {
                if (filterProjectsIds[i] !== ids[i]) {
                    isEqual = false;
                    break;
                }
            }

            if (isEqual) {
                return;
            }
        }

        filterProjectsIds = ids;

        var length = filterProjectsIds.length;

        var sortedProjects = ASC.Projects.Common.getProjectsForFilter();

        function sortPrj(item) {
            return item.canCreateTask && (!length || filterProjectsIds.some(function (fitem) {
                return item.value == fitem;
            }));
        }

        taskProjectSelector.projectadvancedSelector("rewriteItemList", sortedProjects.filter(sortPrj).map(mapPrj), []);
    };

    return {
        init: init,
        showUpdateTaskForm: showUpdateTaskForm,
        showCreateNewTaskForm: showCreateNewTaskForm,
        onUpdateProjectTeam: onUpdateTeam,
        onAddNewMileston: onAddNewMileston,
        filterProjectsByIdInCombobox: filterProjectsByIdInCombobox
    };
})(jQuery);
