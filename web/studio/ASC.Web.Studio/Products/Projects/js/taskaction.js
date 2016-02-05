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


ASC.Projects.TaskAction = (function () {
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

    var isInitData;

    var filterProjectsIds = [];
    var projectItems = [];
    var milestonesItems = [];
    var noneMilestone = { id: 0, title: ASC.Projects.Resources.TasksResource.None, deadline: "" };

    var init = function () {
        if (isInit) {
            return;
        }

        isInit = true;

        initTaskFormElementsAndConstants();

        initEvents();
    };

    function initTaskFormElementsAndConstants () {

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

        if(jq.browser.mobile)
            jq("#ui-datepicker-div").addClass("blockMsg");

        var datePickers = jq(taskDeadlineDate).add(taskStartDate);
        datePickers.mask(ASC.Resources.Master.DatePatternJQ);
        datePickers.datepicker({ selectDefaultDate: false });
        datePickers.on("keydown", onDatePickerKeyDown).on("change", onDatePickerChange);

        getCurrentPage();
    };

    function onDatePickerKeyDown(e) {
        if (e.keyCode === 13) {
            jq(this).blur();
        }
    }

    function onDatePickerChange() {
        jq(this).blur();
    }

    function getCurrentPage () {
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

    function initEvents() {
        taskResponsiblesSelector.on('change', function (evt) {
            var self = jq(this);
            var value = self.val();
            if (value == -1) {
                self.val('-1');
                return;
            }
            var userName = self.find('option[value="' + value + '"]').html();
            if (!listTaskResponsibles.find("div[data-value='" + value + "']").length) {
                listTaskResponsibles.show().append('<div data-value="' + value + '" class="user">' + userName + '</div>');
                jq('.userAdd .combobox-container li.option-item[data-value="' + value + '"]').hide();
                var addedUserCount = listTaskResponsibles.find(".user").length;
                var usersCount = taskResponsiblesSelector.find("option").length - 1;
                if (addedUserCount == usersCount) {
                    taskResponsiblesSelector.tlcombobox(false);
                }
            }
            self.val('-1').change();
            showOrHideNotifyCheckbox();
            evt.stopPropagation();
        });

        taskPopup.on('click', '#fullFormUserList .user', function () {
            var value = jq(this).attr('data-value');
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
            if (jq(this).hasClass("disable")) return;

            clearErrorMessages();
            var data = getTaskData();
            if (checkError(data)) {
                return;
            }

            lockTaskForm();

            var params = {
                 saveAndView: jq(this).attr("id") == "saveTaskAction"
            };

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
                        function () { updateTask({}, currentTask.id, data); },
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

        jq('#addTaskPanel #closeTaskAction').on('click', closeTaskForm);
    };

    function showOrHideNotifyCheckbox() {
        var notifyCheckBox = taskPopup.find('.notify');
        var usersCount = listTaskResponsibles.find(".user").length;
        if (!usersCount || usersCount == listTaskResponsibles.find(".user[data-value=" + currentUserId + "]").length) {
            notifyCheckBox.hide();
            return;
        }
        notifyCheckBox.show();
    };

    function clearErrorMessages() {
        taskPopup.find('.titlePanel').removeClass('requiredFieldError');
        taskPopup.find('.requiredErrorText').html('');
        taskPopup.find('.requiredErrorText.project').hide();
        taskPopup.find(".startDate-error").hide();
        taskStartDate.removeClass("red-border");
        taskDeadlineDate.removeClass("red-border");
    };

    function getTaskData() {
        var self = jq(this);
        var data = {
            title: jq.trim(taskTitle.val()),
            description: taskDescription.val(),
            notify: notifyCheckbox.is(':checked'),
            milestoneId: taskMilestoneSelector.attr("data-id"),
            priority: priorityCheckbox.is(':checked') ? 1 : 0,
            projectId: taskProjectSelector.attr("data-id")
        };

        var responsibles = listTaskResponsibles.find(".user");
        if (responsibles.length) {
            data.responsibles = [];
            responsibles.each(function () {
                data.responsibles.push(jq(this).attr('data-value'));
            });
        }
        try {
            if (taskDeadlineDate.val().length) {
                data.deadline = getTaskDataDate(taskDeadlineDate);
            }

            if (taskStartDate.val().length) {
                data.startDate = getTaskDataDate(taskStartDate);
            }
        } catch(e) {

        } 

        return data;
    };

    function getTaskDataDate(datePicker) {
        var result = datePicker.datepicker('getDate');
        result.setHours(0);
        result.setMinutes(0);
        return Teamlab.serializeTimestamp(result);
    }

    function checkError(data) {
        var isError = false;
        if (!data.title.length) {
            taskPopup.find('.titlePanel').addClass('requiredFieldError');
            taskPopup.find('.requiredErrorText.title').html(taskPopup.find('.requiredErrorText').attr('error'));
            isError = true;
        }

        if (!data.projectId && !updateTaskFlag) {
            taskPopup.find('.requiredErrorText.project').show().html(taskPopup.find('.requiredErrorText.project').attr('error'));
            isError = true;
        }

        if (compareTaskDatesAndShowError()) {
            isError = true;
        } 
        
        return isError;
    };

    function compareTaskDatesAndShowError() {
        var taskStartDateString = taskStartDate.val().trim(),
            taskDeadlineDateString = taskDeadlineDate.val().trim(),
            startDate,
            deadlineDate,
            errorFlag = false;
        
        if (taskStartDateString == "" && taskDeadlineDateString == "") return false;
        
        if (taskStartDateString && !jq.isDateFormat(taskStartDateString)) {
            taskStartDate.addClass("red-border");
            errorFlag = true;
        } else {
            startDate = taskStartDate.datepicker('getDate');
        }
        if (taskDeadlineDateString && !jq.isDateFormat(taskDeadlineDateString)) {
            taskDeadlineDate.addClass("red-border");
            errorFlag = true;
        } else {
            deadlineDate = taskDeadlineDate.datepicker('getDate');
        }

        if (startDate && deadlineDate && startDate > deadlineDate) {
            var errorStartDate = taskPopup.find(".startDate-error");
            errorStartDate.text(errorStartDate.attr("error"));
            errorStartDate.show();
            taskStartDate.addClass("red-border");
            errorFlag = true;
        }

        return errorFlag;
    };

    function initData() {
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

    function setProjectCombobox() {
        projectItems = ASC.Projects.Common.getProjectsForFilter().filter(sortPrj).map(mapPrj);

        taskProjectSelector.projectadvancedSelector(
           {
               itemsChoose: projectItems,
               onechosen: true,
               inPopup: true,
               sortMethod: function () { return 0; }
           }
       );

        taskProjectSelector.on("showList", function (event, item) {
            taskProjectSelector.attr("data-id", item.id).text(item.title).attr("title", item.title);
            taskProjectSelectorOnChange(item);
        });
    };

    function sortPrj(item) {
        return item.canCreateTask;
    }

    function mapPrj(item) {
        return { id: item.value, title: item.title };
    }

    function setMilestonesCombobox() {
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
        });
    };
    
    function taskProjectSelectorOnChange(item) {
        var selectedPrjId = item.id;
        if (selectedPrjId > 0) {
            jq('.popupActionPanel').hide();
            taskPopup.find('.requiredErrorText.project').hide();
            jq('#pm-milestoneBlock, #pm-respBlock').removeClass("display-none");

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

    function lockTaskForm() {
        toggleDisabled([taskTitle, taskDescription, taskStartDate, taskDeadlineDate, notifyCheckbox, priorityCheckbox], true);

        jq(".success-popup, .error-popup").hide();

        LoadingBanner.showLoaderBtn("#addTaskPanel");
    };

    function unlockTaskForm() {
        toggleDisabled([taskTitle, taskDescription, taskStartDate, taskDeadlineDate, notifyCheckbox, priorityCheckbox]);

        LoadingBanner.hideLoaderBtn("#addTaskPanel");
    };

    function toggleDisabled(elements, lock) {
        elements.forEach(function (item) {
            if (lock) {
                item.attr("disabled", "disabled");
            } else {
                item.removeAttr("disabled");
            }
        });
    }

    function addTask (params, projectId, data) {
        Teamlab.addPrjTask(params, projectId, data, { success: onAddTask, error: onTaskError });
    };

    function onAddTask (params, task) {
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
            currentTask = task;
            renderTaskForm(getEmptyTask());
            jq("#addTaskPanel .success-popup").show();
            taskTitle.focus();
        }

        jq("#emptyListTimers .addFirstElement").removeClass("display-none");
    };

    function onTaskError (params, error) {
        var taskErrorBox = jq("#addTaskPanel .error-popup");

        LoadingBanner.hideLoaderBtn("#addTaskPanel");
        taskErrorBox.text(error[0]);
        taskErrorBox.show();

        setTimeout(function () {
            taskErrorBox.hide();
        }, 3000);
        unlockTaskForm();
    };

    function updateTask (params, taskId, data) {
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


    function getMilestones(params, projectId) {
        Teamlab.getPrjMilestones(params, null, { filter: { status: 'open', projectId: projectId }, success: onGetMilestones });
    };

    function onGetMilestones(params, milestones) {
        choosedProjectMilestones = milestones;

        milestonesItems = milestones.map(mapMilestones);
        milestonesItems.unshift(noneMilestone);

        taskMilestoneSelector.projectadvancedSelector("reset");
        taskMilestoneSelector.projectadvancedSelector("rewriteItemList", milestonesItems, []);

        if (currentTask) {
            var taskMile = milestonesItems.find(function(item) {
                return item.id == currentTask.milestoneId;
            });
            taskMilestoneSelector.projectadvancedSelector("reset");
            taskMilestoneSelector.projectadvancedSelector("selectBeforeShow", taskMile || noneMilestone);
        }
    };

    function mapMilestones(item) {
        return { id: item.id, title: '[' + item.displayDateDeadline + '] ' + item.title, deadline: item.deadline };
    }

    function getTeam(params, projectId) {
        Teamlab.getPrjTeam(params, projectId, { success: onGetTeam });
    };

    function onGetTeam(params, team) {
        var noActiveParticipantsTaskNote = jq("#noActiveParticipantsTaskNote");
        var teamWithoutVisitors = ASC.Projects.Common.excludeVisitors(team);
        teamWithoutVisitors = ASC.Projects.Common.removeBlockedUsersFromTeam(teamWithoutVisitors);
        var teamInd = teamWithoutVisitors ? teamWithoutVisitors.length : 0;

        taskResponsiblesSelector.find('option[value!=0][value!=-1]').remove();

        if (teamInd != 0) {
            noActiveParticipantsTaskNote.addClass("display-none");

            for (var i = 0; i < teamInd; i++) {
                var user = teamWithoutVisitors[i];
                taskResponsiblesSelector.append(jq('<option value="' + user.id + '"></option>').html(user.displayName));
            }
            taskResponsiblesSelector.tlcombobox();
            taskResponsiblesSelector.tlcombobox(true);
            listTaskResponsibles.empty();
            setResponsibles();
        } else {
            noActiveParticipantsTaskNote.removeClass("display-none");
            taskResponsiblesSelector.tlcombobox();
            taskResponsiblesSelector.tlcombobox(false);
        }
    };

    function setResponsibles() {
        taskResponsiblesSelector.tlcombobox();
        taskResponsiblesSelector.tlcombobox(true);
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
        if (currentTask && !currentTask.responsibles.length && updateTaskFlag) {
            listTaskResponsibles.empty();
        }
        showOrHideNotifyCheckbox();
    };

    function getEmptyTask () {
        var task = {
            title: "",
            description: "",
            responsibles: [],
            priority: null,
            startDate: null,
            deadline: null,
            milestoneId: null
        };

        if (currentProjectId) {
            task.projectId = currentProjectId;
        }

        return task;
    };

    function isNeedChangeProject (projectId) {
        return projectId && (firstLoadFlag || taskProjectSelector.attr("data-id") != projectId.toString());
    };


    function renderTaskForm (task) {
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
                onGetTeam({}, ASC.Projects.Master.Team);
            }

            if (task.milestoneId) {
                taskMilestoneSelector.projectadvancedSelector("reset");
                var taskMile = milestonesItems.find(function (item) { return item.id == task.milestoneId; });
                taskMilestoneSelector.projectadvancedSelector("selectBeforeShow", taskMile || noneMilestone);
            }

            setResponsibles();
        }

        if (task.deadline) {
            taskDeadlineDate.datepicker('setDate', task.deadline);
            var elemDurationDays = taskDeadlineDate.siblings('.dottedLink');
            elemDurationDays.css('border-bottom', '1px dotted');
            elemDurationDays.css('font-weight', 'normal');
        } else {
            if (updateTaskFlag) taskDeadlineDate.datepicker('setDate', null);
        }

        if (task.startDate) {
            taskStartDate.datepicker('setDate', task.startDate);
        } else {
            if (updateTaskFlag) taskStartDate.datepicker('setDate', null);
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

    function showTaskForm(task) {
        currentTask = task;
        unlockTaskForm();
        renderTaskForm(task);
        StudioBlockUIManager.blockUI(jq("#addTaskPanel"), 550, 550, 0, "absolute");
    };

    function showTaskFormAfterQuestionPopup () {
        unlockTaskForm();
        StudioBlockUIManager.blockUI(jq("#addTaskPanel"), 550, 550, 0, "absolute");
    };

    function closeTaskForm () {
        jq.unblockUI();
    };

    function setDescriptionHeight () {
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
        } else {
            Teamlab.getPrjTask({}, taskId, function (params, targetTask) {
                showTaskForm(targetTask);
            });
        }
    };

    var onAddNewMileston = function (milestone) {
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

    var filterProjectsByIdInCombobox = function (ids) {  // only for gantt chart
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
