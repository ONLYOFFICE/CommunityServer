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


ASC.Projects.TaskAction = (function () {
    var updateTaskFlagEnum = {
        add: 0,
        update: 1,
        copy: 2
    };

    var isInit,
        updateTaskFlag = updateTaskFlagEnum.add,
        firstLoadFlag = true,
        currentTask = null,

        $taskPopup = null,
        $taskTitle = null,
        $taskDescription = null,

        $taskProjectSelector = null,
        $taskMilestoneSelector = null,
        $taskResponsiblesSelector = null,

        //dates
        $taskStartDate = null,
        $taskDeadlineDate = null,

        //other
        $notifyCheckbox = null,
        $priorityCheckbox = null,
        $copySubtasksCheckbox = null,
        $copyFilesCheckbox = null,
        $copyContainer = null,
        $copyFilesContainerCheckbox = null,
        $listTaskResponsibles = null,
        $projectContaner,
        /*---*/

        currentUserId,
        currentProjectId,
        choosedProjectMilestones = [],

        isInitData,

        filterProjectsIds = [],
        projectItems = [],
        milestonesItems = [],
        baseObject = ASC.Projects,
        master = baseObject.Master,
        resources = baseObject.Resources,
        projectsJSResource = resources.ProjectsJSResource,
        noneMilestone = { id: 0, title: resources.TasksResource.None, deadline: "" },
        common = baseObject.Common,
        loadingBanner = LoadingBanner,
        studioBlockUIManager = StudioBlockUIManager,
        teamWithoutVisitors;

    var clickEventName = "click",
        redBorderClass = "red-border",
        displayNone = "display-none",
        requiredFieldErrorClass = "requiredFieldError";

    var teamlab;
    var selectedPrjId;

    var init = function () {
        if (isInit) {
            return;
        }

        isInit = true;
        teamlab = Teamlab;
    };

    function initTaskFormElementsAndConstants () {

        currentUserId = teamlab.profile.id;
        currentProjectId = jq.getURLParam('prjID');

        $taskPopup = jq('#addTaskPanel');

        //text
        $taskTitle = $taskPopup.find("input.textEdit");
        $taskDescription = $taskPopup.find("textarea");

        //selectors
        $projectContaner = $taskPopup.find(".projectContaner");
        $taskProjectSelector = $projectContaner.find(".advansed-select-container");
        $taskMilestoneSelector = jq("#taskMilestone");
        $taskResponsiblesSelector = jq("#taskResponsible");

        //dates
        $taskStartDate = jq("#taskStartDate");
        $taskDeadlineDate = jq("#taskDeadline");
        $taskDeadlineDate.val("");

        //others
        $notifyCheckbox = jq("#notify");
        $priorityCheckbox = jq("#priority");
        $copySubtasksCheckbox = jq("#copySubtasks");
        $copyContainer = jq("#copySubtasksContainer");
        $copyFilesCheckbox = jq("#copyFiles");
        $copyFilesContainerCheckbox = $copyContainer.find(".copy.files");
        $listTaskResponsibles = jq("#fullFormUserList");
        
        jq($taskProjectSelector, $taskMilestoneSelector, $taskResponsiblesSelector).css('max-width', 300);

        if (jq.browser.mobile) {
            jq("#ui-datepicker-div").addClass("blockMsg");
        }

        
        var datePickers = jq($taskDeadlineDate).add($taskStartDate);
        datePickers.datepicker({ selectDefaultDate: false });
        datePickers.on("keydown", onDatePickerKeyDown).off("change").on("change", onDatePickerChange);
        $taskDeadlineDate.on("change",
            function () {
                var date = getTaskDataDate($taskDeadlineDate);
                var dateNow = new Date();
                dateNow.setHours(0);
                dateNow.setMinutes(0);
                dateNow.setSeconds(0);
                var now = teamlab.serializeTimestamp(dateNow);
                var threedays = teamlab.serializeTimestamp(new Date(dateNow.getFullYear(), dateNow.getMonth(), dateNow.getDate() + 3, 0, 0, 0));
                var week = teamlab.serializeTimestamp(new Date(dateNow.getFullYear(), dateNow.getMonth(), dateNow.getDate() + 7, 0, 0, 0));
                if (date === now) {
                    boldDeadlineLeft(0);
                }else if (date === threedays) {
                    boldDeadlineLeft(3);
                } else if (date === week) {
                    boldDeadlineLeft(7);
                } else {
                    boldDeadlineLeft(-1);
                }
            });
    };

    function onDatePickerKeyDown(e) {
        if (e.keyCode === 13) {
            onDatePickerChange(e);
        }
    }

    function onDatePickerChange(e) {
        jq(e.target).unmask().blur().mask(ASC.Resources.Master.DatePatternJQ);
    }

    function boldDeadlineLeft(dataValue) {
        var dotline = "dotline", bold = "bold";
        $taskPopup.find('.deadline_left').removeClass(bold).removeClass(dotline).addClass(dotline);
        $taskPopup.find("[data-value=" + dataValue + "]").removeClass(dotline).addClass(bold);
    }

    function initEvents() {
        $taskPopup.on(clickEventName, '#fullFormUserList .user', function () {
            jq(this).remove();
            $taskResponsiblesSelector.advancedSelector("unselect", [jq(this).attr("data-value")]);
            if (!$listTaskResponsibles.find(".user").length) {
                $listTaskResponsibles.hide();
            }
            showOrHideNotifyCheckbox();
        });

        $taskPopup.on(clickEventName, '.deadline_left', function () {
            var daysCount = parseInt(jq(this).attr('data-value'), 10);
            var date = new Date();
            date.setDate(date.getDate() + daysCount);
            $taskDeadlineDate.datepicker('setDate', date);
            boldDeadlineLeft(daysCount);
        });

        jq('#saveTaskAction, #createTaskAndCreateNew').on(clickEventName, function () {
            if (jq(this).hasClass("disable")) return;

            clearErrorMessages();
            var data = getTaskData();
            if (checkError(data)) {
                return;
            }

            lockTaskForm();

            if (updateTaskFlag === updateTaskFlagEnum.update) {
                var block;
                if ((currentTask.milestoneId || 0) != parseInt(data.milestoneId, 10)) {
                    block = "taskLinksRemoveWarning";
                }

                var endStartLinks;

                if (currentTask.links.length) {
                    endStartLinks = currentTask.links.some(function (item) {
                        return item.linkType == 2 && item.parentTaskId == currentTask.id;
                    });
                }

                if (!data.deadline && endStartLinks) {
                    block = "taskLinksRemoveDeadlineWarning";
                }

                if (currentTask.links.length && block) {
                    ASC.Projects.Base.showCommonPopup(block,
                        function () {
                            var links = currentTask.links;
                            for (var j = 0; j < links.length; ++j) {
                                var dataLink = { dependenceTaskId: links[j].dependenceTaskId, parentTaskId: links[j].parentTaskId };
                                Teamlab.removePrjTaskLink({}, links[j].dependenceTaskId, dataLink, { success: function () { } });
                            }
                            updateTask({}, currentTask.id, data);
                        },
                        showTaskFormAfterQuestionPopup);

                } else {
                    updateTask({}, currentTask.id, data);
                }
            } else if (updateTaskFlag === updateTaskFlagEnum.add) {
                var project = $taskProjectSelector.attr("data-id");
                var params = { saveAndView: jq(this).attr("id") == "saveTaskAction" };
                teamlab.addPrjTask(params, project, data, { error: onTaskError });
            } else if (updateTaskFlag === updateTaskFlagEnum.copy) {
                var project = $taskProjectSelector.attr("data-id");
                data.copyFrom = currentTask.id;
                data.copySubtasks = $copySubtasksCheckbox.is(':checked');
                data.copyFiles = $copyFilesCheckbox.is(':checked');
                data.removeOld = jq(this).attr("id") == "createTaskAndCreateNew";

                var params = { saveAndView: true, removeOld: data.removeOld };

                teamlab.copyPrjTask(params, project, data, { error: onTaskError });
            }
        });

        $taskPopup.find("#closeTaskAction").on(clickEventName, closeTaskForm);

        var events = teamlab.events;

        teamlab.bind(events.addPrjMilestone, onAddNewMileston);
        teamlab.bind(events.removePrjMilestone, function (params, milestone) {
            if (selectedPrjId == milestone.projectId) {
                getMilestones(milestone.projectId);
            }
        });
        teamlab.bind(events.addPrjTask, onAddTask);
        teamlab.bind(events.copyPrjTask, function (params, task) {
            teamlab.call(events.addPrjTask, this, [params, task]);
            if (params.removeOld) {
                teamlab.call(events.removePrjTask, this, [params, currentTask]);
            }
        });

        teamlab.bind(events.updatePrjProjectStatus, function (params, project) {
            if (!isInitData) return;
            var sortedProjects = common.getProjectsForFilter().filter(sortPrj).map(mapPrj);
            $taskProjectSelector.projectadvancedSelector("rewriteItemList", sortedProjects, []);

            if (selectedPrjId === project.id && project.status === 1 && sortedProjects.length) {
                $taskProjectSelector.projectadvancedSelector("reset");
                $taskProjectSelector.projectadvancedSelector("selectBeforeShow", sortedProjects[0]);
            }
        });
    };

    function showOrHideNotifyCheckbox() {
        var notifyCheckBox = $taskPopup.find('.notify');
        var usersCount = $listTaskResponsibles.find(".user").length;
        if (!usersCount || usersCount == $listTaskResponsibles.find(".user[data-value=" + currentUserId + "]").length) {
            notifyCheckBox.hide();
            return;
        }
        notifyCheckBox.show();
    };

    function clearErrorMessages() {
        $taskPopup.find('.titlePanel').removeClass(requiredFieldErrorClass);
        $projectContaner.removeClass(requiredFieldErrorClass);
        $taskPopup.find(".startDate-error").hide();
        $taskStartDate.removeClass(redBorderClass);
        $taskDeadlineDate.removeClass(redBorderClass);
    };

    function getTaskData() {
        var data = {
            title: jq.trim($taskTitle.val()),
            description: $taskDescription.val(),
            notify: $notifyCheckbox.is(':checked'),
            milestoneId: $taskMilestoneSelector.attr("data-id"),
            priority: $priorityCheckbox.is(':checked') ? 1 : 0,
            projectId: $taskProjectSelector.attr("data-id")
        };

        var responsibles = $listTaskResponsibles.find(".user");
        if (responsibles.length) {
            data.responsibles = [];
            responsibles.each(function () {
                data.responsibles.push(jq(this).attr('data-value'));
            });
        }
        try {
            if ($taskDeadlineDate.val().length) {
                data.deadline = getTaskDataDate($taskDeadlineDate);
            }

            if ($taskStartDate.val().length) {
                data.startDate = getTaskDataDate($taskStartDate);
            }
        } catch(e) {

        } 

        return data;
    };

    function getTaskDataDate(datePicker) {
        var result = datePicker.datepicker('getDate');
        if (!result) return "";
        result.setHours(0);
        result.setMinutes(0);
        result.setSeconds(0);
        return teamlab.serializeTimestamp(result);
    }

    function checkError(data) {
        var isError = false;
        if (!data.title.length) {
            $taskPopup.find('.titlePanel').addClass(requiredFieldErrorClass);
            isError = true;
        }

        if (!data.projectId && updateTaskFlag !== updateTaskFlagEnum.update) {
            $projectContaner.addClass(requiredFieldErrorClass);
            isError = true;
        }

        if (compareTaskDatesAndShowError()) {
            isError = true;
        } 
        
        return isError;
    };

    function compareTaskDatesAndShowError() {
        var taskStartDateString = $taskStartDate.val().trim(),
            taskDeadlineDateString = $taskDeadlineDate.val().trim(),
            startDate,
            deadlineDate,
            errorFlag = false;
        
        if (taskStartDateString == "" && taskDeadlineDateString == "") return false;
        
        if (taskStartDateString && !jq.isDateFormat(taskStartDateString)) {
            $taskStartDate.addClass(redBorderClass);
            errorFlag = true;
        } else {
            startDate = $taskStartDate.datepicker('getDate');
        }
        if (taskDeadlineDateString && !jq.isDateFormat(taskDeadlineDateString)) {
            $taskDeadlineDate.addClass(redBorderClass);
            errorFlag = true;
        } else {
            deadlineDate = $taskDeadlineDate.datepicker('getDate');
        }

        if (startDate && deadlineDate && startDate > deadlineDate) {
            var errorStartDate = $taskPopup.find(".startDate-error");
            errorStartDate.text(errorStartDate.attr("error"));
            errorStartDate.show();
            $taskStartDate.addClass(redBorderClass);
            errorFlag = true;
        }

        return errorFlag;
    };

    function initData() {
        setProjectCombobox();
        setMilestonesCombobox();
        setResponsibleCombobox();

        if (currentProjectId) {
            onGetTeam({}, master.Team);
        }
    };

    function setProjectCombobox() {
        projectItems = common.getProjectsForFilter().filter(sortPrj).map(mapPrj);

        $taskProjectSelector.projectadvancedSelector(
           {
               itemsChoose: projectItems,
               onechosen: true,
               inPopup: true,
               sortMethod: function () { return 0; }
           }
       );

        $taskProjectSelector.on("showList", function (event, item) {
            $taskProjectSelector.attr("data-id", item.id).text(item.title).attr("title", item.title);
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
            sortMethod: common.milestoneSort,
            noresults: ASC.Resources.Master.Resource.MilestoneSelectorNoResult,
            noitems: ASC.Resources.Master.Resource.MilestoneSelectorNoItems
        };

        $taskMilestoneSelector.projectadvancedSelector(selectorObj);

        $taskMilestoneSelector.on("showList", function (event, item) {
            $taskMilestoneSelector.attr("data-id", item.id).text(item.title).attr("title", item.title);
        });
    };
    
    function setResponsibleCombobox() {
        $taskResponsiblesSelector.advancedSelector({
            showSearch: true,
            inPopup: true,
            noresults: projectsJSResource.NoActiveParticipantsNote
        }).on("showList", function (event, items) {
            $listTaskResponsibles.empty();
            for (var i = 0, j = items.length; i < j; i++) {
                var item = items[i];
                if (!$listTaskResponsibles.find("div[data-value='" + item.id + "']").length) {
                    $listTaskResponsibles.show().append('<div data-value="' + item.id + '" class="user">' + item.title + '</div>');
                }
            }
            showOrHideNotifyCheckbox();
            event.stopPropagation();
        });
    }

    function taskProjectSelectorOnChange(item) {
        selectedPrjId = item.id;
        if (selectedPrjId > 0) {
            jq('.popupActionPanel').hide();
            $taskPopup.find('.requiredErrorText.project').hide();
            jq('#pm-milestoneBlock, #pm-respBlock').removeClass(displayNone);

            $listTaskResponsibles.html('').hide();

            getMilestones(selectedPrjId);

            if (selectedPrjId == currentProjectId) {
                onGetTeam({}, master.Team);
            } else {
                getTeam({}, selectedPrjId);
            }

            if (updateTaskFlag === updateTaskFlagEnum.copy && currentTask) {
                $copyFilesCheckbox.prop("checked", false);

                if (selectedPrjId !== currentTask.projectId) {
                    $copyFilesContainerCheckbox.hide();
                } else {
                    $copyFilesContainerCheckbox.show();
                }
            }
        }
    };

    function lockTaskForm() {
        toggleDisabled([$taskTitle, $taskDescription, $taskStartDate, $taskDeadlineDate, $notifyCheckbox, $priorityCheckbox, $copySubtasksCheckbox, $copyFilesCheckbox], true);

        jq(".success-popup, .error-popup").hide();

        loadingBanner.showLoaderBtn($taskPopup);
    };

    function unlockTaskForm() {
        toggleDisabled([$taskTitle, $taskDescription, $taskStartDate, $taskDeadlineDate, $notifyCheckbox, $priorityCheckbox, $copySubtasksCheckbox, $copyFilesCheckbox]);

        loadingBanner.hideLoaderBtn($taskPopup);
    };

    function toggleDisabled(elements, lock) {
        var disabledAttr = "disabled";
        elements.forEach(function (item) {
            if (lock) {
                item.attr(disabledAttr, disabledAttr);
            } else {
                item.removeAttr(disabledAttr);
            }
        });
    }

    function onAddTask (params, task) {
        if (params.saveAndView) {
            closeTaskForm();
            common.displayInfoPanel(projectsJSResource.TaskAdded);
        }
        else {
            $taskPopup.find('#taskLink').attr("href", "tasks.aspx?prjID=" + task.projectId + "&id=" + task.id);
            unlockTaskForm();
            currentTask = task;
            renderTaskForm(getEmptyTask());
            $taskPopup.find(".success-popup").show();
            $taskTitle.focus();
        }

        var project = common.getProjectById(task.projectOwner.id);
        if (project) {
            project.taskCountTotal++;
        }

        common.changeTaskCountInProjectsCache(task, 0);
    };

    function onTaskError (params, error) {
        var taskErrorBox = $taskPopup.find(".error-popup");

        loadingBanner.hideLoaderBtn($taskPopup);
        taskErrorBox.text(error[0]);
        taskErrorBox.show();

        setTimeout(taskErrorBox.hide, 3000);
        unlockTaskForm();
    };

    function updateTask (params, taskId, data) {
        teamlab.updatePrjTask(params, taskId, data, { error: onTaskError });
    };

    function getMilestones(projectId) {
        var milestones = master.Milestones.filter(function (item) {
            return item.status === 0 && item.projectId == projectId;
        });

        choosedProjectMilestones = milestones;

        milestonesItems = milestones.map(mapMilestones);
        milestonesItems.unshift(noneMilestone);

        $taskMilestoneSelector.projectadvancedSelector("reset");
        $taskMilestoneSelector.projectadvancedSelector("rewriteItemList", milestonesItems, []);

        if (currentTask) {
            var taskMile = milestonesItems.find(function(item) {
                return item.id == currentTask.milestoneId;
            });
            $taskMilestoneSelector.projectadvancedSelector("reset");
            $taskMilestoneSelector.projectadvancedSelector("selectBeforeShow", taskMile || noneMilestone);
        }
    };

    function mapMilestones(item) {
        return { id: item.id, title: '[' + item.displayDateDeadline + '] ' + item.title, deadline: item.deadline };
    }

    function getTeam(params, projectId) {
        teamlab.getPrjTeam(params, projectId, { success: onGetTeam });
    };

    function onGetTeam(params, team) {
        teamWithoutVisitors = common.excludeVisitors(team);
        teamWithoutVisitors = common.removeBlockedUsersFromTeam(teamWithoutVisitors);

        $listTaskResponsibles.empty();
        $taskResponsiblesSelector.advancedSelector("reset");
        $taskResponsiblesSelector.advancedSelector("rewriteItemList", teamWithoutVisitors.map(
            function (item) {
                return { id: item.id, title: item.displayName }
            }), []);

        updateTaskResponsibleSelector();
    };

    function updateTaskResponsibleSelector() {
        if (currentTask && currentTask.responsibles.length && teamWithoutVisitors) {
            $listTaskResponsibles.empty();
            jQuery.each(currentTask.responsibles, function () {
                var self = this;
                var elem = teamWithoutVisitors.find(function (item) { return item.id === self.id });
                if (typeof elem != "undefined") {
                    $listTaskResponsibles.show().append('<div data-value="' + elem.id + '" class="user">' + elem.displayName + '</div>');
                }
            });
            $taskResponsiblesSelector.advancedSelector("select", currentTask.responsibles.map(function(item) { return item.id; }));
        }
        if (currentTask && !currentTask.responsibles.length && updateTaskFlag !== updateTaskFlagEnum.add) {
            $listTaskResponsibles.empty();
        }
        showOrHideNotifyCheckbox();
    }

    function getEmptyTask () {
        var task = {
            title: "",
            description: "",
            responsibles: [],
            priority: null,
            startDate: null,
            deadline: null
        };

        currentProjectId = jq.getURLParam("prjID");
        if (currentProjectId) {
            task.projectId = currentProjectId;
        }

        return task;
    };

    function isNeedChangeProject (projectId) {
        return projectId && (firstLoadFlag || $taskProjectSelector.attr("data-id") != projectId.toString());
    };


    function renderTaskForm (task) {
        clearErrorMessages();
        $taskPopup.find('.success-popup, .error-popup').hide();
        $taskPopup.find('.notify');
        // task body
        $taskTitle.val(task.title);
        $taskDescription.val(task.description);

        if (isNeedChangeProject(task.projectId)) {
            var taskProject = projectItems.find(function (item) { return item.id == task.projectId; }) || task.projectOwner;
            if (taskProject) {
                $taskProjectSelector.projectadvancedSelector("selectBeforeShow", taskProject);
            } else {
                jq("#pm-milestoneBlock").hide();
                getMilestones(task.projectId);
                getTeam({}, task.projectId);
            }
        } else {
            if (currentProjectId) {
                getMilestones(task.projectId);
                onGetTeam({}, master.Team);
            }

            if (task.milestoneId) {
                $taskMilestoneSelector.projectadvancedSelector("reset");
                var taskMile = milestonesItems.find(function (item) { return item.id == task.milestoneId; });
                $taskMilestoneSelector.projectadvancedSelector("selectBeforeShow", taskMile || noneMilestone);
            }
            updateTaskResponsibleSelector();
        }

        var setDateAction = "setDate";
        if (task.deadline) {
            $taskDeadlineDate.datepicker(setDateAction, task.deadline);
            $taskDeadlineDate.change();
        } else {
            if (updateTaskFlag !== updateTaskFlagEnum.add) {
                $taskDeadlineDate.datepicker(setDateAction, null);
                boldDeadlineLeft(-1);
            }
        }

        if (task.startDate) {
            $taskStartDate.datepicker(setDateAction, task.startDate);
        } else {
            if (updateTaskFlag !== updateTaskFlagEnum.add) $taskStartDate.datepicker(setDateAction, null);
        }

        if (task.priority) {
            $priorityCheckbox.prop("checked", true);
        } else {
            $priorityCheckbox.prop("checked", false);
        }

        if (updateTaskFlag === updateTaskFlagEnum.update) {
            setDescriptionHeight();
            $projectContaner.hide();
        } else {
            $taskDescription.height(60);
            $projectContaner.show();
        }

        //buttons and title
        var saveButton = $taskPopup.find('#saveTaskAction');
        var createNewButton = jq("#createTaskAndCreateNew");
        var commonResource = resources.CommonResource;

        if (updateTaskFlag === updateTaskFlagEnum.update) {
            createNewButton.hide();
            createNewButton.siblings(".splitter-buttons").first().hide();

            $copyContainer.hide();

            saveButton.html(commonResource.SaveChanges);
            $taskPopup.find('.containerHeaderBlock table td:first').html(projectsJSResource.EditThisTask);
        } else if (updateTaskFlag === updateTaskFlagEnum.add) {
            createNewButton.show();
            createNewButton.siblings(".splitter-buttons").show();

            $copyContainer.hide();

            saveButton.html(commonResource.Save);
            $taskPopup.find('.containerHeaderBlock table td:first').html(projectsJSResource.CreateNewTask);
        } else if (updateTaskFlag === updateTaskFlagEnum.copy) {
            createNewButton.html(commonResource.Replace);
            createNewButton.show();
            createNewButton.siblings(".splitter-buttons").show();

            $copySubtasksCheckbox.prop("checked", false);
            $copyFilesCheckbox.prop("checked", false);
            $copyContainer.show();

            saveButton.html(commonResource.Copy);
            $taskPopup.find('.containerHeaderBlock table td:first').html(resources.TasksResource.CopyTaskHeader);
        }

        if (firstLoadFlag) {
            firstLoadFlag = false;
        }
        loadingBanner.hideLoading();
    };

    function showTaskForm(task) {
        currentTask = task;
        unlockTaskForm();
        renderTaskForm(task);
        studioBlockUIManager.blockUI($taskPopup, 550, 550, 0, "absolute");
        var datePickers = jq($taskDeadlineDate).add($taskStartDate);
        datePickers.mask(ASC.Resources.Master.DatePatternJQ);
    };

    function showTaskFormAfterQuestionPopup () {
        unlockTaskForm();
        studioBlockUIManager.blockUI($taskPopup, 550, 550, 0, "absolute");
    };

    function closeTaskForm() {
        var datePickers = jq($taskDeadlineDate).add($taskStartDate);
        datePickers.unmask();
        jq.unblockUI();
    };

    function setDescriptionHeight () {
        var overflowYClass = "overflowY";

        //default
        $taskDescription.height(60);
        $taskDescription.css(overflowYClass, "auto");

        var colsCount = parseInt($taskDescription.attr("cols"), 10),
            text = $taskDescription.val(),
            countStr = text.split("\n").length;

        if (countStr > 4) {
            if (countStr > 26) {
                $taskDescription.height(400);
                $taskDescription.css(overflowYClass, "scroll");
                return;
            }
            $taskDescription.height(countStr * 15);
        }
        var signsCount = text.length;
        if (signsCount > colsCount * 6) {
            var rows = Math.floor(signsCount / (colsCount * 2.5));
            if (rows > 27) {
                $taskDescription.height(400);
                $taskDescription.css(overflowYClass, "scroll");
            } else {
                if (rows > countStr) {
                    $taskDescription.height(rows * 15);
                }
            }
        }
    };

    function show(task, updateTaskFlagVal) {
        if (!isInitData) {
            var tasksResource = resources.TasksResource;

            jq("#addTaskPanel")
                .html(jq.tmpl("common_containerTmpl",
                {
                    options: {
                        PopupContainerCssClass: "popupContainerClass",
                        OnCancelButtonClick: "PopupKeyUpActionProvider.CloseDialog();",
                        IsPopup: true
                    },
                    header: {
                        data: { title: tasksResource.AddTask },
                        title: "projects_common_popup_header"
                    },
                    body: {
                        title: "projects_task_action",
                        data: {
                            title: {
                                error: tasksResource.EachTaskMustHaveTitle,
                                header: tasksResource.TaskTitle
                            },
                            description: tasksResource.TaskDescription,
                            project: {
                                error: tasksResource.ChooseProject,
                                header: resources.ProjectResource.Project
                            }
                        }
                    }
                }));

            initTaskFormElementsAndConstants();
            initEvents();
            initData();

            isInitData = true;
        }

        loadingBanner.displayLoading();
        updateTaskFlag = updateTaskFlagVal;
        showTaskForm(task);
    }

    var showCreateNewTaskForm = function (taskParams) {
        show(jq.extend(getEmptyTask(), taskParams), updateTaskFlagEnum.add);
    };

    var showUpdateTaskForm = function (task) {
        show(task, updateTaskFlagEnum.update);
    };

    var showCopyTaskForm = function (task) {
        show(task, updateTaskFlagEnum.copy);
    };

    function onAddNewMileston(params, milestone) {
        if ($taskProjectSelector.attr("data-id") == milestone.projectId) {
            milestonesItems.push({ id: milestone.id, title: '[' + milestone.displayDateDeadline + '] ' + milestone.title, deadline: milestone.deadline });
            $taskMilestoneSelector.projectadvancedSelector("rewriteItemList", milestonesItems, []);
        }
    };

    var onUpdateTeam = function () {
        if (!isInitData) return;
        if (currentProjectId == $taskProjectSelector.attr("data-id")) {
            onGetTeam({}, master.Team); 
        }
    };

    var filterProjectsByIdInCombobox = function (ids) {  // only for gantt chart
        if (!isInitData) return;

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

        var sortedProjects = common.getProjectsForFilter();

        function sortPrj(item) {
            return item.canCreateTask && (!length || filterProjectsIds.some(function (fitem) {
                return item.value == fitem;
            }));
        }

        $taskProjectSelector.projectadvancedSelector("rewriteItemList", sortedProjects.filter(sortPrj).map(mapPrj), []);
    };

    return {
        init: init,
        showUpdateTaskForm: showUpdateTaskForm,
        showCreateNewTaskForm: showCreateNewTaskForm,
        showCopyTaskForm: showCopyTaskForm,
        onUpdateProjectTeam: onUpdateTeam,
        filterProjectsByIdInCombobox: filterProjectsByIdInCombobox
    };
})(jQuery);
