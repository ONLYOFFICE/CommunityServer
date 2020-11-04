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


ASC.Projects.GantChartPage = (function () {

    // data for initialize chart

    var currentProjectId = null,
        currentProject = null,
        ganttIndex = null,
        filteredData = false,
        saveOrderFlag = false,
        autoScaleFlag = false,
        stackTasksCount = 0,
        showLoaderFlag = false,
        firstLoad = true,
        readMode = true,

        showOnlyActiveProjects = true,
        haveEditProjects = false,

        filterControl = null,

        localizeStrings = {},

        loadTasks = false,
        loadMilestones = false,
        loadGanttIndex = false,
        loadStatuses = false,

        taskForDiagram = [],
        milestonesForDiagram = [],
        sortedProjectsForFilter = [],
        currentFilteredProjectsIds = [],
        listProjectsOnReceive = [],

        allTaskHash = {},
        milestonesHash = {},
        allProjectsHash = {},

        taskCount = 0,
        milestoneCount = 0,

        taskStatus = { open: 0, closed: 1 },
        showPanelFlag = true,
        menuCoordinates = {},
        menuProject = -1,
        respDummyHide = false,
        refreshData = false,

        closeDialogAction = "ASC.Projects.GantChartPage.enableChartEvents();",

        // chart

        BASE_LAYER = 0,
        OVERLAY_LAYER = 1,

        chart = null,
        zoomBar = null,

        layers = [],
        contexts = [],

        kScaleUnitNormalizeDays = 28,
        kScaleUnitSevenDays = 49,       //  170 + 10
        kScaleUnitOneMonth = 200,       //  300

        // DOM variables

        fullScreenMode, leftPanelMode,
        zoomScale,
        teamMemberFilter,
        ganttCellChecker,
        openTaskOnly,
        fakeBackground,
        saveLinkButton,
        taskSelectContainer,
        statusListContainer,
        statusListTaskContainer,
        parentTaskName, dependentTaskSelector, linkTypeSelector,
        taskContextMenu, milestoneContextMenu,
        setResponsibleMenu,
        taskContextMenuStatus, milestoneContextMenuStatus,
        emptyScreen, ganttHelpPanel, helpActionsPanel,
        undoBtn, redoBtn,
        createNewMenu, blockPanel = null;

    var resources = ASC.Projects.Resources;
    var projectsJsResource = resources.ProjectsJSResource;
    var projectsFilterResource = resources.ProjectsFilterResource;
    var common = ASC.Projects.Common;

    var initLocalizeStrings = function () {
        localizeStrings.statusOpenTask = projectsFilterResource.StatusOpenTask;
        localizeStrings.statusOpenMilestone = projectsFilterResource.StatusOpenMilestone;
        localizeStrings.statusClosedMilestone = projectsFilterResource.StatusClosedMilestone;
        localizeStrings.statusClosedTask = projectsFilterResource.StatusClosedTask;
        localizeStrings.newTask = projectsJsResource.GanttNewTask;
        localizeStrings.newMilestone = projectsJsResource.GanttNewMilestone;
        localizeStrings.taskWithoutMilestones = projectsJsResource.GanttTaskWithoutMilestones;
        localizeStrings.responsibles = projectsJsResource.GanttResponsibles;
        localizeStrings.responsibles.format = jq.format;
        localizeStrings.noResponsible = projectsFilterResource.NoResponsible;
        localizeStrings.taskOverdueText = projectsJsResource.GanttTaskOverdue;
        localizeStrings.overdue = projectsFilterResource.Overdue;
        localizeStrings.taskDescSubtask = projectsJsResource.GanttSubtaskTaskDesc;
        localizeStrings.taskDescSubtasks = projectsJsResource.GanttSubtasksTaskDesc;
        localizeStrings.details = projectsJsResource.GanttTaskDetails;
        localizeStrings.milestones = projectsJsResource.GanttMilestonesPrjDesc;
        localizeStrings.activeTasks = projectsJsResource.GanttActiveTasksPrjDesc;
        localizeStrings.tasks = projectsJsResource.GanttTasksMilDesc;
        localizeStrings.week = projectsJsResource.GanttWeekShort;
        localizeStrings.year = projectsJsResource.GanttYear;
        localizeStrings.toTasksList = projectsJsResource.ToTaskList;
        localizeStrings.day = projectsJsResource.DelayDay;
        localizeStrings.days = projectsJsResource.DelayDays;
        localizeStrings.dayShort = projectsJsResource.GanttDayShort;
        localizeStrings.expand = projectsJsResource.GanttExpand;
        localizeStrings.collapse = projectsJsResource.GanttCollapse;
        localizeStrings.responsibles2 = projectsJsResource.GanttLeftPanelResponsibles;
        localizeStrings.beginDate = projectsJsResource.GanttLeftPanelBeginDate;
        localizeStrings.endDate = projectsJsResource.GanttLeftPanelEndDate;
        localizeStrings.status = projectsJsResource.GanttLeftPanelStatus;
        localizeStrings.priotity = projectsJsResource.GanttLeftPanelPriority;
        localizeStrings.highPriority = projectsJsResource.GanttLeftPanelHighPriority;
        localizeStrings.openStatus = projectsJsResource.GanttLeftPanelOpenStatus;
        localizeStrings.closeStatus = projectsJsResource.GanttLeftPanelCloseStatus;
        localizeStrings.addDate = projectsJsResource.GanttLeftPanelAddDate;
    };
    var initCellChecker = function () {
        var fields = {          // replase on resources strings
            menuItems: [
                { id: "Responsibility", title: projectsJsResource.GanttLeftPanelResponsibles },
                { id: "BeginDate", title: projectsJsResource.GanttLeftPanelBeginDate },
                { id: "EndDate", title: projectsJsResource.GanttLeftPanelEndDate },
                { id: "Status", title: projectsJsResource.GanttLeftPanelStatus },
                { id: "Priority", title: projectsJsResource.GanttLeftPanelPriority }
            ]
        };

        jq("#ganttCanvasContainer").append(jq.tmpl("projects_panelFrame", { panelId: "ganttCellChecker" }));
        ganttCellChecker = jq("#ganttCellChecker");

        // ganttCellChecker.addClass("gantt-context-menu").css("width", "200px");
        ganttCellChecker.addClass("gantt-context-menu");

        ganttCellChecker.find(".panel-content").empty().append(jq.tmpl("projects_linedListWithCheckbox", fields));

        ganttCellChecker.append("<span id='showChoosedFields' class='button small'>" + projectsJsResource.ApplyBtn + "</span>");

        ganttCellChecker.css('padding', '4px 4px 0');
        ganttCellChecker.find('.dropdown-item').each(function () { jq(this).css('border', 'none'); });
        ganttCellChecker.find('.dropdown-content').each(function () { jq(this).css('margin-top', '0px'); });

        ganttCellChecker.on("click", ".dropdown-item", function (e) {
            e.stopPropagation();
        });
    };
    var initProjectsFilter = function () {
        setProjectsFilterFromStorage();

        if (null === filterControl) {
            filterControl = jq("#gantt-filter-projects");
            if (filterControl) {
                jq("#gantt-filter-projects").projectadvancedSelector({
                    itemsChoose: sortedProjectsForFilter,
                    inPopup: true,
                    itemsSelectedIds: currentFilteredProjectsIds
                });

                jq("#gantt-filter-projects").on("showList", getProjectsDataByFilter);
            }
        } else {
            jq("#gantt-filter-projects").projectadvancedSelector("reset");
        }
    };

    var saveCheckedProjectToStorage = function () {
        localStorageManager.setItem("ganttProjects", currentFilteredProjectsIds.join(','));
    };
    var setProjectsFilterFromStorage = function () {
        // project in url - не забыть, что закрытых проектов в списке нет!
        var prjId = currentProjectId, prjCount = 0;

        if (localStorageManager.getItem("ganttProjects") && null === currentProjectId) {
            currentFilteredProjectsIds = localStorageManager.getItem("ganttProjects").split(",");
            listProjectsOnReceive = localStorageManager.getItem("ganttProjects").split(",");
        }
        if (prjId && jq.inArray(prjId, currentFilteredProjectsIds) == -1) {
            currentFilteredProjectsIds.push(prjId);
            listProjectsOnReceive.push(prjId);
        }
        localStorageManager.setItem("ganttProjects", currentFilteredProjectsIds.join(','));

        for (var k = currentFilteredProjectsIds.length - 1; k >= 0; --k) {
            currentFilteredProjectsIds[k] = parseInt(currentFilteredProjectsIds[k]);
        }

        // проект может быть удален или стать недоступен, поэтому надо проверить

        var i, j, project;

        for (i = currentFilteredProjectsIds.length - 1; i >= 0; --i) {
            project = allProjectsHash[currentFilteredProjectsIds[i]];
            if (undefined === project) {
                currentFilteredProjectsIds.splice(i, 1);
            }
        }

        for (i = listProjectsOnReceive.length - 1; i >= 0; --i) {
            project = allProjectsHash[listProjectsOnReceive[i]];
            if (undefined === project) {
                listProjectsOnReceive.splice(i, 1);
            }
        }


        // Отсекаем закрытые или запауженные проекты на просмотр
        // Отсекаем приватные проекты в которых юзер не в команде проекта

        if (showOnlyActiveProjects) {

            var isSiteAdmin = common.currentUserIsModuleAdmin();

            for (i = currentFilteredProjectsIds.length - 1; i >= 0; --i) {
                project = allProjectsHash[currentFilteredProjectsIds[i]];
                if (undefined !== project) {

                    if (!isSiteAdmin) {
                        if (1 === project.status || 2 === project.status) {
                            currentFilteredProjectsIds.splice(i, 1);
                        }
                    }
                }
            }

            for (i = listProjectsOnReceive.length - 1; i >= 0; --i) {
                project = allProjectsHash[listProjectsOnReceive[i]];
                if (undefined !== project) {

                    if (!isSiteAdmin) {
                        if (1 === project.status || 2 === project.status) {
                            listProjectsOnReceive.splice(i, 1);
                        }
                    }
                }
            }
        }

        prjCount = currentFilteredProjectsIds.length;

        showEmptyProjectsScreen(0 === prjCount);

        if (0 === prjCount) {
            if (blockPanel) {
                blockPanel.css({display: 'none'});
            }
        }

        if (prjCount > 0) {
            getProjectData(listProjectsOnReceive.shift());

            if (haveEditProjects) {
                blockCreateMenu(false);
            }

        } else {
            firstLoadSettings();
        }
    };
    var getProjectsDataByFilter = function (event, items) {
        var i = 0,
            ln = items.length,
            receiveFlag = false,
            newCheckedProjects = [],
            prjId;

        for (; i < ln; ++i) {
            prjId = items[i].id;
            newCheckedProjects.push(prjId);
            listProjectsOnReceive.push(prjId);
        }

        haveEditProjects = false;
        readMode = true;

        blockPanel.css({display: ''});

        blockCreateMenu(true);

        currentFilteredProjectsIds = newCheckedProjects.slice(0);
        saveCheckedProjectToStorage();

        chart.modelController().clear();

        showEmptyProjectsScreen(0 === listProjectsOnReceive.length);

        updateZoomElement();

        buildChartFromListProjects();

        if (0 === listProjectsOnReceive.length) {
            blockPanel.css({display: 'none'});
        }
    };
    var buildChartFromListProjects = function () {
        var j, prjCount = listProjectsOnReceive.length;

        for (j = 0; j < prjCount; ++j) {
            prjId = listProjectsOnReceive.shift();

            if (allProjectsHash[prjId].dataLoaded && !refreshData) {
                buildGanttChartStorageForProject(prjId);
                if (j == prjCount - 1) {
                    filterChartData();
                    blockPanel.css({display: 'none'});
                }
            } else {
                getProjectData(prjId);
                break;
            }
        }

        initLeftPanelCells();
    };

    var initResponsibleSelector = function (team, inputType) {
        var i = 0,
            teamWithoutVisitors = common.removeBlockedUsersFromTeam(common.excludeVisitors(team)),
            teamInd = teamWithoutVisitors ? teamWithoutVisitors.length : 0,
            list = setResponsibleMenu.find(".dropdown-content");

        list.empty();

        for (i = 0; i < teamInd; ++i) {
            var user = teamWithoutVisitors[i];
            list.append(jq("<li class='check-value'><input type='" + inputType +
                "' id='" + user.id + "' name='responsibles'/><label for='" + user.id +
                "' title='" + user.displayName + "'>" +
                user.displayName + " </label></li>"));
        }
    };
    var initEventsAndDomVariables = function () {
        //init DOM variables

        fullScreenMode = jq(".full-screen");
        leftPanelMode = jq(".left-panel");
        zoomScale = jq("#zoomScale");
        teamMemberFilter = jq("#teamMemberFilter");
        openTaskOnly = jq("#openTaskOnly");
        openTaskOnly.prop("checked", false);
        fakeBackground = jq(".fake-background");
        parentTaskName = jq("#parentTaskName");
        dependentTaskSelector = jq("#dependentTaskSelect");
        linkTypeSelector = jq("#linkTypeSelector");
        saveLinkButton = jq("#addNewLinkPopup .save");
        taskSelectContainer = jq("#addNewLinkPopup .task-select");
        statusListContainer = jq("#statusListContainer");
        statusListTaskContainer = jq("#statusListTaskContainer");
        taskContextMenu = jq("#taskContextMenu");
        milestoneContextMenu = jq("#milestoneContextMenu");
        taskContextMenuStatus = jq("#taskStatus");
        milestoneContextMenuStatus = jq("#milestoneStatus");
        emptyScreen = jq("#emptyScreenForGanttLayout");
        setResponsibleMenu = jq("#responsiblesContainer");
        ganttHelpPanel = jq("#ganttHelpPanel");
        helpActionsPanel = jq("#ganttActions");
        undoBtn = jq("#undoButton");
        redoBtn = jq("#reduButton");
        createNewMenu = jq("#createNewButton");
		
		helpActionsPanel.append ('<div id="btnCloseHelpActionsPanel" class="cancelButton" style="top:-1px" onclick="">×</div>');

        // default cursor at selection
        document.onselectstart = function () { return chart.isEditTitleMode(); };

        // create new button
        var $dropDownObj = jq("#createNewButton");

        jq("#showGanttHelp").click(function (event) {
            var elt = jq((event.target) ? event.target : event.srcElement);
            if (ganttHelpPanel.is(":visible")) {
                ganttHelpPanel.hide();
            } else {
                showPanelFlag = true;
                event.stopPropagation();
                ganttHelpPanel.show();
            }
        });

        window.onbeforeunload = function (evt) {
            if (!jq("#mainActionButtons").hasClass("disable")) return;
            var message = projectsJsResource.GanttOnBeforeUnloadMessage;
            if (typeof evt == "undefined") {
                evt = window.event;
            }

            if (!haveEditProjects) {
                return;
            }

            if (evt) {
                evt.returnValue = message;
            }

            return message;
        };

        jq("#createNewMilestone").click(function () {
            if (currentFilteredProjectsIds.length) {
                disableChartEvents();
                ASC.Projects.MilestoneAction.showNewMilestonePopup();
                ASC.Projects.MilestoneAction.filterProjectsByIdInCombobox(currentFilteredProjectsIds);
                $dropDownObj.hide();
            }
        });

        jq("#createNewTask").click(function () {
            if (currentFilteredProjectsIds.length) {
                disableChartEvents();
                ASC.Projects.TaskAction.showCreateNewTaskForm();
                ASC.Projects.TaskAction.filterProjectsByIdInCombobox(currentFilteredProjectsIds);
                $dropDownObj.hide();
            }
        });

        // undo/redo buttons
        undoBtn.click(function () {
            if (jq(this).hasClass("disable") || jq("#mainActionButtons").hasClass("disable")) return;
            chart.undoManager().undo();
        });
        redoBtn.click(function () {
            if (jq(this).hasClass("disable") || jq("#mainActionButtons").hasClass("disable")) return;
            chart.undoManager().redo();
        });

        jq(".print").click(function () {
            if (jq(this).parent().hasClass("disable")) return;
            var content = getContentForPrint();
            var printWin = window.open();

            printWin.document.open();
            printWin.document.write(content);
            printWin.document.close();
            printWin.focus();
        });

        jq(".refresh").click(function () {
            // if (jq(this).parent().hasClass("disable")) return;

            refreshChartData();
        });

        // zoom
        jq(".scale-conteiner").on("click", ".tl-combobox-container", function () {
            autoScaleFlag = false;
        });
        zoomScale.change(function () {
            if (autoScaleFlag) {
                autoScaleFlag = false;
                return false;
            }
            setZoomScale();
            localStorageManager.setItem("ganttZoomScale", jq(this).val());
        });

        jq("#todayPreset").click(function () {
            if (chart) {
                chart.viewController().strafeToDay();
                updateZoomElement();
            }
        });

        // questions popup
        jq(".popupContainerClass .cancel").on("click", function () {
            enableChartEvents();
            jq.unblockUI();
            return false;
        });

        jq("#questionWindowDeleteMilestone .remove, #questionWindowTaskWithSubtasks .end").click(function () {
            enableChartEvents();
            chart.modelController().finalize();
            jq.unblockUI();
            return false;
        });

        jq('#questionWindowTaskRemove .remove').click(function () {
            enableChartEvents();
            saveOrderFlag = true;
            removeTask({}, jq(this).data("taskid"));
            chart.modelController().finalize();
            jq.unblockUI();
        });

        jq("#moveTaskOutMilestone .move-all").click(function () {
            enableChartEvents();
            chart.modelController().finalize('SaveConnections');
            jq.unblockUI();
            return false;
        });

        jq("#moveTaskOutMilestone .one-move").click(function () {
            enableChartEvents();
            chart.modelController().finalize();
            jq.unblockUI();
            return false;
        });
        //empty screen
        jq("#addFirstTask").click(function () {
            chart.addNewTask();
            emptyScreen.addClass("display-none");
            return false;
        });
        jq("#addFirstMilestone").click(function () {
            chart.addNewMilestone();
            emptyScreen.addClass("display-none");
            return false;
        });
        jq("#hideEmptyScreen").click(function () {
            emptyScreen.addClass("display-none");
            return false;
        });

        //additional panel
        fullScreenMode.click(function () {
            if (jq(this).hasClass("active")) return;
            checkFullScreen();
        });

        leftPanelMode.click(function () {
            if (jq(this).hasClass("active")) return;
            checkLeftPanel();
        });

        jq("#hideActionsButton").click(function () {
            helpActionsPanel.addClass("display-none");
            localStorageManager.setItem("hideActionHelpPlag", true);
        });
       
		jq("#btnCloseHelpActionsPanel").click(function () {
            helpActionsPanel.addClass("display-none");
            localStorageManager.setItem("hideActionHelpPlag", true);
        });
		
        jq(document).keydown(function (e) {
            var keyCode = e.keyCode ? e.keyCode : e.which ? e.which : e.charCode;

            // Hook CTR + P
            if (80 === keyCode && (e.ctrlKey || e.metaKey)) {
                e.preventDefault();
                jq(".print").click();
                return;
            }

            if (!chart.isEditTitleMode() &&
                !jq('#milestoneActionPanel').is(":visible") &&
                !jq('#addTaskPanel').is(":visible")) {

                if (keyCode == 27) {
                    emptyScreen.addClass("display-none");
                    return true;
                }
                if (keyCode == 72 && !readMode) {
                    if (helpActionsPanel.hasClass("display-none")) {
                        localStorageManager.setItem("hideActionHelpPlag", false);
                        helpActionsPanel.removeClass("display-none").addClass("display-block");
                    } else {
                        localStorageManager.setItem("hideActionHelpPlag", true);
                        helpActionsPanel.removeClass("display-block").addClass("display-none");
                    }
                }
            }
            return true;
        });

        // filter
        jq(".addition").on("click", ".filter-container .combobox-title", function () {
            disableChartEvents();
        });

        teamMemberFilter.change(function () {
            //filterChartData();
        });

        openTaskOnly.change(function () {
            filterChartData();

            if (openTaskOnly.is(":checked")) {
                localStorageManager.setItem("openTaskOnlyFilter", true);
            } else {
                localStorageManager.setItem("openTaskOnlyFilter", false);
            }
        });

        // change status
        statusListContainer.on("click", ".status", function () {
            enableChartEvents();
            statusListContainer.hide();
            if (!jq(this).hasClass("underline")) {
                chart.modelController().finalizeStatus(taskStatus.open);
            }
        });

        jq(".gantt-context-menu").mouseenter(function () {
            disableChartEvents();
        });

        jq(".gantt-context-menu").mouseleave(function () {
            enableChartEvents();
        });

        jq(".gantt-context-menu .dropdown-item").click(function (event) {
            var contextMenuId = jq(this).closest(".gantt-context-menu").attr("id");
            if (contextMenuId == "statusList") {
                return true;
            }
            var action = jq(this).attr("class").split(" ")[0];
            if (action == "responsible") {
                setResponsibleMenu.data("prjid", jq("#" + contextMenuId).data("prjid"));
                if (contextMenuId == "milestoneContextMenu") {
                    showResponsiblePanel("milestone", milestoneContextMenu.data("id"), menuProject);
                } else {
                    showResponsiblePanel("task", taskContextMenu.data("id"));
                }
                event.stopPropagation();
            } else {
                var status = Number(action);
                chart.modelController().finalizeOperation(typeof (status) === "number" && status === status ? status : action);
                jq(".gantt-context-menu").hide();
            }
        });

        jq('body').click(function (event) {
            var elt = jq((event.target) ? event.target : event.srcElement);
            var isHide = true;

            if (jq(".blockPage").length) {
                return true;
            }

            if (elt.closest(".check-value").length) {
                return true;
            }

            if (elt.closest(".addition").length && !elt.hasClass("HelpCenterSwitcher")) {
                if (elt.closest(".tl-combobox").length) {
                    disableChartEvents();
                } else {
                    enableChartEvents();
                }
                isHide = true;
            }

            if (isHide)
                elt.parents().each(function () {
                    if (jq(this).hasClass("gantt-context-menu")) {
                        isHide = true;
                    }
                });

            if (showPanelFlag && !jq(".gantt-context-menu:visible").length) {
                showPanelFlag = false;
                isHide = false;
            } else {
                showPanelFlag = true;
                isHide = true;
            }

            if (isHide) {
                enableChartEvents();
                if (!respDummyHide) {
                    jq(".gantt-context-menu").hide();
                }
                respDummyHide = false;
                showPanelFlag = true;
            }
            return true;
        });

        // add link popup
        jq(document).on("change", "#dependentTaskSelect", function () {
            if (jq(this).val() == "-1") {
                linkTypeSelector.text("");
                return false;
            }
            checkPossibleLinkType();
            enableSaveLinkButton();
        });
        linkTypeSelector.change(function () {
            enableSaveLinkButton();
        });

        saveLinkButton.click(function () {
            if (jq(this).hasClass("disable")) return;
            var link = {};

            link.linkType = parseInt(linkTypeSelector.data("value"));

            if (link.linkType == 2) {
                link.parentTaskId = parseInt(dependentTaskSelector.val(), 10);
                link.dependenceTaskId = parseInt(parentTaskName.data("taskid"), 10);
            } else {
                link.parentTaskId = parseInt(parentTaskName.data("taskid"), 10);
                link.dependenceTaskId = parseInt(dependentTaskSelector.val(), 10);
            }

            if (link.linkType == 3) {
                link.linkType = 2;
            }

            blockChartInterface();
            Teamlab.addPrjTaskLink({ link: link, prjId: dependentTaskSelector.data("prjid") }, link.parentTaskId, link, {
                success: function (params, data) {
                    unblockChartInterface();
                    jq.unblockUI();
                    params.link.projectId = params.prjId;
                    addLinkForTasks(params.link);
                    chart.modelController().addLinkWithIds(params.link);
                }, error: onTaskLinkError
            });

        });
        setResponsibleMenu.on("change", "input", function () {
            setResponsibleMenu.data("changed", "true");
        });

        jq("#setResponsible").click(function (event) {
            if (setResponsibleMenu.data("changed") != "true") {
                setResponsibleMenu.data("id", "");
                setResponsibleMenu.hide();
                return true;
            }
            setResponsibleMenu.data("changed", "");
            var element = {},
                status,
                prjId = setResponsibleMenu.data("prjid"),
                type = setResponsibleMenu.data("type"),
                elementId = setResponsibleMenu.data("id");
            if (type == "task") {
                element = allProjectsHash[prjId].allTaskHash[elementId];
                status = element.status;
                delete element.status;
                element.responsibles = getNewResponsibilities();
                element.updRespFlag = true;
                updateTask(element);
            } else {
                element = allProjectsHash[prjId].milestonesHash[elementId];
                element.responsible = getNewResponsibilities()[0];
                delete element.status;
                element.updRespFlag = true;
                updateMilestone(element);
            }
            jq(".gantt-context-menu").hide();
            event.stopPropagation();
        });
    };
    var initLeftPanelCells = function () {
        if (localStorageManager.getItem("checkedCells")) {
            var cellNames = localStorageManager.getItem("checkedCells").split(",");
            for (var i = 0; i < cellNames.length; i++) {
                ganttCellChecker.find("input[id=" + cellNames[i] + "]").prop("checked", true);
            }

            chart.leftPanelController().addRowsAvailable(cellNames);

            if (localStorageManager.getItem("visibleCells")) {
                var visibleCellsNames = localStorageManager.getItem("visibleCells").split(",");
                chart.leftPanelController().showHiddenRows(visibleCellsNames);
            } else {
                chart.leftPanelController().showHiddenRows([]);
            }
        }
    };

    // refrash chart

    var refreshChartData = function () {
        if (!refreshData) {
            chart.modelController().clear();
            refreshData = true;
            init(true);
        }
    };

    // disable/enable chart events

    var enableChartEvents = function () {
        chart.viewController().disableUserEvents(false);
    };
    var disableChartEvents = function () {
        chart.viewController().disableUserEvents(true);
    };

    // block/unblock interface

    var blockChartInterface = function () {
        showLoaderFlag = true;
        setTimeout(function () { if (showLoaderFlag) { LoadingBanner.displayLoading(); } }, 2000);
        jq(".mode-button-container").addClass("disable");
        jq("#mainActionButtons").addClass("disable");
        createNewMenu.css({ "visibility": "hidden" });
        chart.viewController().disableUserEvents(true);
    };
    var unblockChartInterface = function () {
        jq(".mode-button-container").removeClass("disable");
        jq("#mainActionButtons").removeClass("disable");
        createNewMenu.css({ "visibility": "visible" });
        chart.viewController().disableUserEvents(false);
        showLoaderFlag = false;
        LoadingBanner.hideLoading();
    };
    var blockCreateMenu = function (block) {
        if (!block) {
            jq('#mainActionButtons').removeClass('disable');
            jq("#menuCreateNewButton").removeClass('disable');
            jq('#createNewButton').css({ 'visibility': 'visible' });
        } else {
            jq('#mainActionButtons').addClass('disable');
            jq('#menuCreateNewButton').addClass('disable');
            jq('#createNewButton').css({ 'visibility': 'hidden' });
        }
    };

    // check screen mode

    var checkFullScreen = function () {
        fullScreenMode.addClass("active");
        leftPanelMode.removeClass("active");
        localStorageManager.setItem("ganttFullScreen", true);
        chart.viewController().fullscreen(true);
        fakeBackground.removeClass("gray");
    };
    var checkLeftPanel = function () {
        leftPanelMode.addClass("active");
        fullScreenMode.removeClass("active");
        localStorageManager.setItem("ganttFullScreen", false);
        chart.viewController().fullscreen(false);
        fakeBackground.addClass("gray");
    };

    // new responsibles
    var getNewResponsibilities = function () {
        var checkedElems = setResponsibleMenu.find("input:checked");
        var responsibles = [];
        checkedElems.each(function () {
            responsibles.push(jq(this).attr("id"));
        });
        return responsibles;
    };

    // chart initialization

    var init = function (reload) {
        ASC.Projects.GantChart(window);
        refreshData = true;

        taskForDiagram = [];
        milestonesForDiagram = [];
        sortedProjectsForFilter = [];
        currentFilteredProjectsIds = [];
        listProjectsOnReceive = [];

        allTaskHash = {};
        milestonesHash = {};
        allProjectsHash = {};

        // data
        currentProjectId = jq.getURLParam("PrjID");

        if (!reload) {
            initEventsAndDomVariables();
            startChartInit();
            initCellChecker();

            jq(document).find('.mainPageContent').append('<div id="block-panel" class="blocked-panel"/>');
            blockPanel = jq('#block-panel');
            if (blockPanel) {
                blockPanel.mousemove(function (e) { e.preventDefault(); e.stopImmediatePropagation(); });
                blockPanel.mousedown(function (e) { e.preventDefault(); e.stopImmediatePropagation(); });
                blockPanel.mouseup(function (e) { e.preventDefault(); e.stopImmediatePropagation(); });
                blockPanel.bind("contextmenu",function(e) { return false; });
            }
        }

        blockPanel.css({display: ''});

        blockCreateMenu(true);

        Teamlab.bind(Teamlab.events.addPrjMilestone, function (params, milestone) {
            addMilestoneToChart(milestone, true, true);
        });

        Teamlab.getPrjProjects({ reload: reload }, {
            filter: { sortBy: '', sortOrder: '', status: '', fields: 'id,title,security,isPrivate,status,responsibleId' },
            success: function (param, projects) {
                ASC.Projects.Master.Projects = projects;
                initLoadEvents(param.reload);
            }
        });
    };
    var initLoadEvents = function (reload) {

        // list projects
        var projects = ASC.Projects.Master.Projects,
            projectsCount = projects.length,
            currentUserProjects = [],
            otherProjects = [];

        for (var i = 0; i < projectsCount; i++) {               // добавить проверки на доступ к задачам и вехам
            allProjectsHash[projects[i].id] = projects[i];

            if (showOnlyActiveProjects) {

                // closed, paused pr blocked projects skip
                if (1 === projects[i].status || 2 === projects[i].status)
                    continue;

                if (0 === projects[i].status) {                     // only open project
                    if (projects[i].isInTeam) {
                        currentUserProjects.push(projects[i]);
                    } else {
                        otherProjects.push(projects[i]);
                    }
                }
            }
        }

        if (currentUserProjects.length > 0) {
            currentUserProjects[currentUserProjects.length - 1]['classname'] = 'separator';
        }

        sortedProjectsForFilter = currentUserProjects.concat(otherProjects);

        initProjectsFilter();
        initLeftPanelCells();

        if (!reload) {
            jq(document).bind("loadTasks", function (event, data) {
                loadTasks = true;
                jq(document).trigger("loadData", { prjId: data.prjId });
            });

            jq(document).bind("loadMilestones", function (event, data) {
                loadMilestones = true;
                jq(document).trigger("loadData", { prjId: data.prjId });
            });

            jq(document).bind("loadGanttIndex", function (event, data) {
                loadGanttIndex = true;
                jq(document).trigger("loadData", { prjId: data.prjId });
            });
            jq(document).bind("loadStatuses", function (event, data) {
                loadStatuses = true;
                jq(document).trigger("loadData", { prjId: data.prjId });
            });

            jq(document).bind("loadData", function (event, data) {
                if (loadMilestones && loadTasks && loadGanttIndex && loadStatuses) {

                    if (!data) return;
                    allProjectsHash[data.prjId].dataLoaded = true;

                    buildGanttChartStorageForProject(data.prjId);

                    if (firstLoad) {
                        firstLoadSettings();
                    }

                    if (listProjectsOnReceive.length == 0) {
                        filterChartData();
                        chart.viewController().loadViewState(true);
                        refreshData = false;
                        initLeftPanelCells();

                        blockPanel.css({display: 'none'});
                    } else {
                        buildChartFromListProjects();
                    }
                }
            });
        }
    };
    var startChartInit = function () {
        initLocalizeStrings();

        window["Gantt"]["Teamlab_shortmonths"] = Teamlab.constants.nameCollections.shortmonths;
        window["Gantt"]["Teamlab_months"] = Teamlab.constants.nameCollections.months;
        window["Gantt"]["Localize_strings"] = localizeStrings;

        // chart
        layers.push(document.getElementById('layer0'));
        layers.push(document.getElementById('layer1'));
        if (!layers[BASE_LAYER] || !layers[OVERLAY_LAYER])
            return;

        contexts.push(layers[BASE_LAYER].getContext('2d'));
        contexts.push(layers[OVERLAY_LAYER].getContext('2d'));
        if (!contexts[BASE_LAYER] || !contexts[OVERLAY_LAYER])
            return;

        layers[OVERLAY_LAYER].onselectstart = function () { return false; };
        layers[OVERLAY_LAYER].onmousedown = function () { return false; };
        layers[OVERLAY_LAYER].oncontextmenu = function () { return false; };

        chart = new Gantt.TimeLine(layers[BASE_LAYER], layers[OVERLAY_LAYER]);

        // zoom
        if (localStorageManager.getItem("ganttZoomScale"))
            zoomScale.val(localStorageManager.getItem("ganttZoomScale"));

        zoomScale.tlcombobox();
        setZoomScale();

        updateChartSizes();
        chart.viewController().strafeToDay();
        renderGanttChart();
    };
    var firstLoadSettings = function () {
        if (!localStorageManager.getItem("hideActionHelpPlag") && !readMode) {
            helpActionsPanel.removeClass("display-none").addClass("display-block");
        }
        //checkLeftPanel(); // может быть уже можно удалить эту настройку

        setReadMode();
        chart.userDefaults().setFontFamily(jq("body").css("fontFamily"));

        chart.update();
        chart.viewController().enableSlipAnimation(true);

        renderChartEventHandlers();
        registerChartHandlers();

        if (localStorageManager.getItem("openTaskOnlyFilter")) {
            openTaskOnly.prop("checked", true);
        } else {
            openTaskOnly.prop("checked", false);
        }

        initLeftPanelCells();

        firstLoad = false;
    };

    var setZoomScale = function () {
        var ind = parseInt(zoomScale.val());

        if (ind === 1)
            chart.viewController().scaleTo(kScaleUnitNormalizeDays, undefined, true);
        else if (ind === 7)
            chart.viewController().scaleTo(kScaleUnitSevenDays, undefined, true);
        else if (ind === 4)
            chart.viewController().scaleTo(kScaleUnitOneMonth, undefined, true);
    };
    var setReadMode = function () {
        if (haveEditProjects) {
            jq("#menuCreateNewButton").removeClass("disable");
            readMode = false;
        }
    };

    var showEmptyProjectsScreen = function (show) {
        emptyScreen.css({ 'display': show ? 'block' : 'none' });
        chart.needDrawFlashBackground(show);
    };

    var checkUserRights = function () {
        if (firstLoad) return false;

        var defaultPageURL = "Projects.aspx";
        var prjId = jq.getURLParam("prjID");
        if (prjId) {
            Teamlab.getPrjTeam({}, prjId, {
                success: function (params, team) {
                    ASC.Projects.Master.TeamWithBlockedUsers = team;
                    ASC.Projects.Master.Team = common.removeBlockedUsersFromTeam(ASC.Projects.Master.TeamWithBlockedUsers);
                    var userInTeam = common.userInProjectTeam(Teamlab.profile.id);
                    if (currentProject.isPrivate && !userInTeam) {
                        document.location = defaultPageURL;
                    }
                    if (userInTeam && (!userInTeam.canReadTasks || !userInTeam.canReadMilestones)) {
                        document.location = defaultPageURL;
                    }
                }
            });
        }
    };

    var getProjectData = function (projectId) {
        LoadingBanner.displayLoading();
        // createNewMenu.css({ "visibility": "hidden" });
        loadTasks = false;
        loadMilestones = false;
        loadGanttIndex = false;

        var filter = common.filterParamsForListTasks;
        delete filter.status;

        filter.projectId = projectId;

        Teamlab.getPrjTasksSimpleFilter({ prjId: projectId }, {
            filter: filter,
            success: function (params, tasks) {
                //if (!loadMilestones) {
                //    checkUserRights();
                //}
                allProjectsHash[params.prjId].tasks = tasks;
                jq(document).trigger("loadTasks", [{ prjId: params.prjId }]);
            }
        });

        Teamlab.getPrjMilestones({ prjId: projectId }, {
            filter: filter,
            success: function (params, milestones) {
                // if (!loadTasks) {
                //     checkUserRights();
                // }
                allProjectsHash[params.prjId].milestones = milestones;
                jq(document).trigger("loadMilestones", [{ prjId: params.prjId }]);
            }
        });

        Teamlab.getPrjGanttIndex({ prjId: projectId }, projectId, {
            success: function (params, data) {
                if (typeof data === "string") {
                    allProjectsHash[params.prjId].ganttIndex = jq.parseJSON(data);
                } else {
                    allProjectsHash[params.prjId].ganttIndex = {};
                }
                jq(document).trigger("loadGanttIndex", [{ prjId: params.prjId }]);
            },
            error: function (params, error) {
                allProjectsHash[params.prjId].ganttIndex = {};
                jq(document).trigger("loadGanttIndex", [{ prjId: params.prjId }]);
            }
        });

        if (!loadStatuses) {
            ASC.Projects.Common.initCustomStatuses(function () {
                jq(document).trigger("loadStatuses", [{ prjId: projectId }]);
            });
        }
        if (!currentProjectId || currentProjectId != projectId) {
            Teamlab.getPrjTeam({ prjId: projectId }, projectId, {
                success: function (params, team) {
                    allProjectsHash[params.prjId].team = team;
                }
            });
        } else {
            allProjectsHash[projectId].team = ASC.Projects.Master.Team;
        }
    };
    var filterChartData = function () {
        enableChartEvents();
        var filter = {};

        if (openTaskOnly.prop("checked")) {
            filter.status = true;
        } else {
            filter.status = undefined;
        }
        if (teamMemberFilter.val() != "-1") {
            filter.participant = teamMemberFilter.val();
        } else {
            filter.participant = undefined;
        }

        filteredData = true;

        chart.modelController().setFilter(function (element) {
            var projectId = element.owner();
            if (element.isMilestone()) {
                if (filter.status && element.status() == 1) {
                    element.filter = true;  // hide closed milestone
                } else {
                    element.filter = false;
                }
            } else {
                if (!taskSatisfyFilter(filter, element)) {
                    element.filter = true;
                    allProjectsHash[projectId].allTaskHash[element.id()].visible = false;
                } else {
                    element.filter = false;
                    allProjectsHash[projectId].allTaskHash[element.id()].visible = true;
                }
            }
        });
    };

    var taskSatisfyFilter = function (filter, element) {
        var satisfyStatusFlag = false,
            satisfyRespFlag = false;
        if (!filter.status || (filter.status && element.status() == 1)) {
            satisfyStatusFlag = true;
        }

        if (!filter.participant) {
            satisfyRespFlag = true;
            return satisfyStatusFlag && satisfyRespFlag;
        }

        var responsibles = allProjectsHash[element.owner()].allTaskHash[element.id()].responsibles;
        if (filter.participant && responsibles.length) {
            for (var i = 0; i < responsibles.length; i++) {
                if (filter.participant == responsibles[i].id) {
                    satisfyRespFlag = true;
                    break;
                }
            }
        }
        if (filter.participant == "00000000-0000-0000-0000-000000000000" && responsibles.length == 0) {  // no responsible filter
            satisfyRespFlag = true;
        }
        return satisfyStatusFlag && satisfyRespFlag;
    };

    var renderChartEventHandlers = function () {
        document.onmousemove = function (e) {
            chart.onmousemove(e);
        };
        document.onkeypress = function (e) {
            if (blockPanel && 'none' === blockPanel.css('display')) {
                chart.onkeypress(e);
            }
        };
        document.onkeydown = function (e) {
            if (blockPanel && 'none' === blockPanel.css('display')) {
                chart.onkeydown(e);
            }
        };
        document.onkeyup = function (e) {
            if (blockPanel && 'none' === blockPanel.css('display')) {
                return chart.onkeyup(e);
            }

            return false;
        };
        document.onmouseup = function (e) {
            chart.focus(false, e);
        };
        document.onpaste = function (e) {
            chart.onpaste(e);
        };
        document.oncopy = function (e) {
            chart.oncopy(e);
        };

        window.onresize = function () {
            updateChartSizes();
            updateZoomElement();
            chart.render();
        };

        layers[OVERLAY_LAYER].onmousemove = function (e) {
            chart.onmousemove(e);
        };
        layers[OVERLAY_LAYER].onmousedown = function (e) {
            chart.onmousedown(e);
        };
        layers[OVERLAY_LAYER].onmouseup = function (e) {
            chart.onmouseup(e);
        };
        layers[OVERLAY_LAYER].ondblclick = function (e) {
            chart.ondblclick(e);
        };

        initTouch();

        if (layers[OVERLAY_LAYER].addEventListener) {
            if ('onwheel' in layers[OVERLAY_LAYER]) {
                layers[OVERLAY_LAYER].addEventListener("wheel", onWheel, false);

                layers[OVERLAY_LAYER].addEventListener("wheel", function (e) {
                    e.preventDefault();
                    return false;
                }, false);

            } else if ('onmousewheel' in layers[OVERLAY_LAYER]) {
                layers[OVERLAY_LAYER].addEventListener("mousewheel", onWheel, false);

                layers[OVERLAY_LAYER].addEventListener("mousewheel", function (e) {
                    e.preventDefault();
                    return false;
                }, false);

            } else {
                layers[OVERLAY_LAYER].addEventListener("MozMousePixelScroll", onWheel, false);
            }
        } else { // IE<9
            layers[OVERLAY_LAYER].attachEvent("onmousewheel", onWheel);
            layers[OVERLAY_LAYER].addEventListener("onmousewheel", function (e) { return false }, false);
        }

        jq('.left-panel').bind('mousewheel DOMMouseScroll', function (event, delta) {
            jq(".advanced-selector-container ").first().css({ display: 'none' });
            if (taskContextMenu.is(":visible")) {
                taskContextMenu.hide();
            }
        });

        zoomBar = chart.viewController().buildZoomBar(document.getElementById('ganttChartZoom'));

        updateChartSizes();
        renderGanttChart();
        chart.update();
        chart.viewController().toVisibleElementMove();
        updateZoomElement();
        updateChartSizes();

        window.requestAnimFrame = (function () {
            return window.requestAnimationFrame || window.webkitRequestAnimationFrame || window.mozRequestAnimationFrame || window.oRequestAnimationFrame || window.msRequestAnimationFrame ||
                function (callback, element) {
                    window.setTimeout(callback, 1000 / 15);
                };
        })();

        renderOneFrame();
    };
    var renderOneFrame = function () {
        requestAnimFrame(renderOneFrame);
        renderGanttChart();
    };
    var renderGanttChart = function () {
        chart.render();
    };

    var firstUpdate = true;

    var updateChartSizes = function () {

        var chartWidth = window.innerWidth - 48;
        var topPadding = fakeBackground.position().top;
        var chartHeight = window.innerHeight - topPadding;
        var minWidth = parseInt(jq(".mainPageLayout").first().css("min-width"), 10);

        chartWidth = chartWidth > minWidth ? chartWidth : minWidth;

        if (layers[BASE_LAYER].width != chartWidth || layers[BASE_LAYER].height != chartHeight) {

            layers[BASE_LAYER].width = chartWidth;
            layers[BASE_LAYER].height = chartHeight;

            layers[OVERLAY_LAYER].width = chartWidth;
            layers[OVERLAY_LAYER].height = chartHeight;

            chart.update();
        }

        if (zoomBar && zoomBar.canvas()) {
            if (zoomBar.canvas().width != zoomBar.dom().clientWidth || zoomBar.canvas().height != zoomBar.dom().clientHeight) {
                zoomBar.canvas().width = zoomBar.dom().clientWidth;
                zoomBar.canvas().height = zoomBar.dom().clientHeight;

                zoomBar.needRepaint();
            }
        }

        if (firstUpdate) {
            updateZoomElement();
            firstUpdate = false;
        }
    };
    var updateZoomElement = function () {
        var chartWidth = window.innerWidth - 48;
        var topPadding = fakeBackground.position().top;
        var chartHeight = window.innerHeight - topPadding;
        var minWidth = parseInt(jq(".mainPageLayout").first().css("min-width"), 10);

        chartWidth = chartWidth > minWidth ? chartWidth : minWidth;

        fakeBackground.height(chartHeight);

        // set zoom width
        var marginAndButtonConstWidth = 50 + 32 + 32 + 16 + 15 + 24;
        var width = chartWidth - jq("#mainActionButtons").width() - jq(".zoom-presets").outerWidth() - marginAndButtonConstWidth;

        jq("#ganttChartZoom").width(width);
        if (firstLoad) {
            jq("#ganttChartZoom").css("border", "1px solid #ccc");
            jq("#ganttChartZoom").css("backgroundColor", "#fff");
        }

        // set max-width responsible filter
        marginAndButtonConstWidth = 48 * 4;
        width = jq(".filter-container").width() - jq(".mode-button-container").width() - jq(".task-filter-container:first").outerWidth() - jq(".task-filter-container:last").outerWidth();
        jq(".task-filter-container .tl-combobox").css({ maxWidth: width });

        if (jq("#ZoomBarId").is('canvas')) {
            jq("#ZoomBarId").width(jq("#ganttChartZoom").width());
            if (zoomBar) {
                zoomBar.canvas().width = zoomBar.dom().clientWidth;
                zoomBar.canvas().height = zoomBar.dom().clientHeight;
                zoomBar.needRepaint();
            }
        }
    };

    // data initialisation

    var buildGanttChartStorageForProject = function (projectId) {
        var project = allProjectsHash[projectId];
        readTeamLabProject(project);
        readTeamLabMilestones(project);
        readTeamLabTasks(project);

        chart.modelController().buildWithThroughIndexer();

        LoadingBanner.hideLoading();
        //createNewMenu.css({ "visibility": "visible" });
    };

    var readTeamLabProject = function (project) {
        var title = project.title,
            id = project.id,
            description = "", // need?
            respName = "",
            createdDate = project.created ? new Date(project.created) : new Date(), // нужна ли эта дата вообще?
            ganttIndex = project.ganttIndex;

        var respUser = window.UserManager.getUser(project.responsibleId) || window.UserManager.getRemovedProfile(project.responsibleId);

        if (!respUser) {
            if (undefined !== project.responsible && undefined !== project.responsible.displayName) {
                respName = project.responsible;
            }
        } else {
            project.responsible = respUser;
            respName = project.responsible;
        }

        var canEdit = false;
        if (common.currentUserIsModuleAdmin() || project.responsible.id == Teamlab.profile.id) {
            canEdit = true;
        }
        if (Teamlab.profile.isVisitor) {
            canEdit = false;
        }

        var kReadModeProject = 1000;

        var status = project.status;
        if (!canEdit) { status = kReadModeProject; } // 1000 - readOnly

        if (canEdit) {
            haveEditProjects = true;
            readMode = false;
            blockCreateMenu(false);
        } else {
            var ind = currentFilteredProjectsIds.indexOf(id);
            if (-1 !== ind) {
                currentFilteredProjectsIds.splice(ind, 1);
            }
        }

        chart.modelController().addProject(id, title, description, respName, createdDate, ganttIndex, project.isPrivate, status);
    };

    var resetDateHours = function (date) {
        date.setHours(0);
        date.setMilliseconds(0);
        date.setMinutes(0);
        date.setSeconds(0);

        return date;
    };

    var getFullResponsiblesWrapper = function (respIds) {
        var i, length = respIds.length, fullUsers = [];
        for (i = 0; i < length; ++i) {
            fullUsers.push(window.UserManager.getUser(respIds[i]) || window.UserManager.getRemovedProfile(respIds[i]));
        }
        return fullUsers;
    };

    var addTaskToChart = function (task, update, undo) {
        var taskLinks = [],
            title = task.title,
            description = task.description,
            deadline = task.deadline,
            created = resetDateHours(task.crtdate),
            startDate = task.startDate,
            priority = task.priority,
            customTaskStatus = task.customTaskStatus,
            createdBy = task.createdBy,
            status = task.status,
            ownerId = task.projectOwner,
            subtasks = task.subtasksCount ? task.subtasksCount : 0,
            performer,
            begin,
            end,
            responsibles = [],
            milestone = -1,
            beginFail = false,
            id = task.id;

        if (task.responsibles.length) {
            if ('string' === typeof (task.responsibles[0])) {
                responsibles = getFullResponsiblesWrapper(task.responsibles);
            } else {
                var resp = [];
                for (var j = 0; j < task.responsibles.length; ++j) {
                    resp.push(task.responsibles[j].id);
                }
                responsibles = getFullResponsiblesWrapper(resp);
            }
            performer = Encoder.htmlDecode(task.responsibles[0].displayName);
        }

        if (task.milestoneId) {
            milestone = task.milestoneId;
        }

        end = undefined;
        if (deadline) {
            end = new Date(deadline);
        } else {
            if (status == 2 && task.uptdate) end = new Date(task.uptdate);
        }
        if (end) end = resetDateHours(end);

        begin = undefined;
        if (startDate) {
            begin = resetDateHours(startDate)
        } else {
            if (end && created <= end) {
                begin = resetDateHours(created);
            }
            if (end && created > end) {
                begin = end;
            }

            beginFail = true;
        }
        if (!begin) begin = resetDateHours(created);

        var kLinkBeginEnd = 2;

        taskLinks = [];
        if (task.links) {
            for (var i = 0; i < task.links.length; ++i) {
                if (task.links[i].parentTaskId !== id && kLinkBeginEnd === task.links[i].linkType) {
                    taskLinks.push(task.links[i]);
                }
            }
        }

        if ('number' == typeof (task.projectOwner))
            ownerId = task.projectOwner;
        else if ('object' == typeof (task.projectOwner) && !isNaN(task.projectOwner.id))
            ownerId = task.projectOwner.id;

        chart.modelController().addTask(id, ownerId, title, performer, description, begin, end, status, customTaskStatus, milestone, priority, subtasks, responsibles, taskLinks, undo, beginFail, createdBy);

        if (update) {

            // rebuild & update hash

            allProjectsHash[task.projectId].allTaskHash[task.id] = task;
            allProjectsHash[task.projectId].allTaskHash[task.id].visible = true;
            allProjectsHash[task.projectId].taskCount++;

            setGanttIndex(task.projectId);

            chart.update();
            chart.viewController().disableUserEvents(false);
            chart.viewController().centeringElement(id);
        }
    };
    var addMilestoneToChart = function (milestone, update, undo) {
        if (milestone) {
            var id = milestone.id,
                title = milestone.title,
                description = milestone.description,
                deadline = resetDateHours(milestone.deadline),
                performer = "";
            if (milestone.responsible) {
                performer = Encoder.htmlDecode(milestone.responsible.displayName);
            }
            var ownerId = milestone.projectId;
            var isKey = milestone.isKey;
            var status = milestone.status;

            chart.modelController().addMilestone(id, ownerId, title, description, milestone.responsible, new Date(deadline), status, isKey, undo);
        }

        if (update) {

            // rebuild & update hash

            allProjectsHash[milestone.projectId].milestonesHash[milestone.id] = milestone;
            allProjectsHash[milestone.projectId].milestoneCount++;
            setGanttIndex(milestone.projectId);

            chart.update();
            chart.viewController().disableUserEvents(false);
            chart.viewController().centeringElement(id, true);
        }
    };

    var readTeamLabTasks = function (project) {
        if (0 !== project.status)
            return;

        var task;      // loop iterator
        taskForDiagram = project.tasks.slice(0);
        project.taskCount = taskForDiagram.length;
        if (!project.allTaskHash) {
            project.allTaskHash = {};
        }

        for (var i = 0; i < project.taskCount; i++) {
            task = taskForDiagram[i];
            if (project.allTaskHash[task.id]) {
                task = project.allTaskHash[task.id];
            } else {
                project.allTaskHash[task.id] = task;
                project.allTaskHash[task.id].visible = true;
            }
            addTaskToChart(task);
        }
    };
    var readTeamLabMilestones = function (project) {
        if (0 !== project.status)
            return;

        var milestone;      // loop iterator

        if (!project.milestonesHash) {
            project.milestonesHash = {};
        }

        milestonesForDiagram = project.milestones.slice(0);
        project.milestoneCount = milestonesForDiagram.length;
        for (var i=0; i< project.milestoneCount; i++) {
            milestone = milestonesForDiagram[i];

            if (project.milestonesHash[milestone.id]) {
                milestone = project.milestonesHash[milestone.id];
            } else {
                project.milestonesHash[milestone.id] = milestone;
            }

            addMilestoneToChart(milestone);
        }
    };

    //actions

    var convertToTeamlabTask = function (element, updateFlag) {
        var task = { title: element.title() ? element.title() : "" };
        task.description = element.description() ? element.description() : "";

        if (updateFlag) {
            task.id = element.id();
            task.responsibles = [];

            var taskResp = element.responsibles();
            for (var i = 0; i < taskResp.length; i++) {
                task.responsibles.push(taskResp[i].id);
            }

            task.priority = allProjectsHash[element.owner()].allTaskHash[task.id].priority;
        }
        if (element.beginDate()) task.startDate = Teamlab.serializeTimestamp(element.beginDate());
        if (element.isUndefinedBeginTime()) task.startDate = undefined;

        if (element.isUndefinedEndTime()) {
            task.deadline = undefined;
        } else {
            if (element.endDate()) task.deadline = Teamlab.serializeTimestamp(element.endDate());
        }

        if (element.owner()) task.projectId = element.owner();

        task.milestoneId = element.milestone;

        return task;
    };
    var convertToTeamlabMilestone = function (element, updateFlag) {
        var milestone = { title: element.title() ? element.title() : "" };
        milestone.description = element.description() ? element.description() : "";

        if (updateFlag) {
            milestone.id = element.id();
            milestone.isKey = allProjectsHash[element.owner()].milestonesHash[milestone.id].isKey;
            milestone.responsible = element.responsibles();
        } else {
            milestone.responsible = allProjectsHash[element.owner()].responsibleId;
        }

        milestone.deadline = Teamlab.serializeTimestamp(element.endDate());

        if (element.owner) milestone.projectId = element.owner();

        return milestone;
    };

    // question popups

    var showTaskQuestionPopup = function (task) {
        disableChartEvents();
        if (task.links.length) {
            jq("#noteAboutLinks").removeClass("display-none");
        } else {
            jq("#noteAboutLinks").addClass("display-none");
        }
        PopupKeyUpActionProvider.CloseDialogAction = closeDialogAction;
        StudioBlockUIManager.blockUI(jq("#questionWindowTaskRemove"), 400);
        PopupKeyUpActionProvider.EnterAction = "jq('#questionWindowTaskRemove .remove').click();";

        jq("#questionWindowTaskRemove .remove").data("taskid", task.id());
    };
    var showMilestoneQuestionPopup = function (milestoneId) {
        disableChartEvents();
        PopupKeyUpActionProvider.CloseDialogAction = closeDialogAction;
        StudioBlockUIManager.blockUI(jq("#questionWindowDeleteMilestone"), 400);
        PopupKeyUpActionProvider.EnterAction = "jq('#questionWindowDeleteMilestone .remove').click();";
        jq("#questionWindowDeleteMilestone").attr("milestoneId", milestoneId);
    };
    var showTaskWithSubtasksQuestionPopup = function (taskId) {
        disableChartEvents();
        PopupKeyUpActionProvider.CloseDialogAction = closeDialogAction;
        StudioBlockUIManager.blockUI(jq("#questionWindowTaskWithSubtasks"), "auto");
        PopupKeyUpActionProvider.EnterAction = "jq('#questionWindowTaskWithSubtasks .end').click();";
        jq("#questionWindowTaskWithSubtasks .end").data("taskid", taskId);
    };
    var showMilestoneWithTasksQuestionPopup = function () {
        disableChartEvents();
        PopupKeyUpActionProvider.CloseDialogAction = closeDialogAction;
        StudioBlockUIManager.blockUI(jq("#questionWindowMilestoneTasks"), 400);
        PopupKeyUpActionProvider.EnterAction = "jq('#questionWindowMilestoneTasks .cancel').click();";
    };
    var showMoveTaskOutMilestonePopup = function () {
        disableChartEvents();
        PopupKeyUpActionProvider.CloseDialogAction = closeDialogAction;
        StudioBlockUIManager.blockUI(jq("#moveTaskOutMilestone"), "auto");
        PopupKeyUpActionProvider.EnterAction = "jq('#moveTaskOutMilestone .cancel').click();";
    };
    var showCreateNewLinkPopup = function (task) {
        if (setTaskSelect(task)) {
            disableChartEvents();
            PopupKeyUpActionProvider.CloseDialogAction = closeDialogAction;
            StudioBlockUIManager.blockUI(jq("#addNewLinkPopup"), 400);
        } else {
            common.displayInfoPanel(projectsJsResource.GanttNotAvailableTaskLink, true);
        }
    };

    // link

    var setTaskSelect = function (prTask) {
        var t, isHaveValidLinks = false, taskFirstId = -1,
            project = allProjectsHash[prTask.owner()],
            parentTask = project.allTaskHash[prTask.id()],
            task;

        parentTaskName.text(parentTask.title).data("taskid", parentTask.id);
        var option = dependentTaskSelector.find("option:first");
        dependentTaskSelector.remove();
        taskSelectContainer.find(".tl-combobox-container").remove();
        taskSelectContainer.append("<select id='dependentTaskSelect'></select>");
        dependentTaskSelector = jq("#dependentTaskSelect");

        for (t in project.allTaskHash) {

            task = project.allTaskHash[t];

            if (task.visible && isValidTaskForLink(parentTask, task)) {
                option = document.createElement('option');
                option.setAttribute("value", task.id);
                option.appendChild(document.createTextNode(task.title));
                dependentTaskSelector.append(option);

                if (-1 === taskFirstId) {
                    taskFirstId = task.id;
                    isHaveValidLinks = true;
                }
            }
        }

        if (-1 !== taskFirstId) {
            dependentTaskSelector.data("prjid", project.id);
            dependentTaskSelector.val(taskFirstId + '').tlcombobox();
            linkTypeSelector.text("");

            checkPossibleLinkType();
            enableSaveLinkButton();
        }

        return isHaveValidLinks;
    };
    var isValidTaskForLink = function (parentTask, task) {
        var parentTaskLinksCount = 0;

        if (task.id == parentTask.id) {
            return false;
        }

        if (task.status == 2) {
            return false;
        }

        if (task.milestoneId != parentTask.milestoneId) {
            return false;
        }

        if (!task.deadline && !parentTask.deadline) { // rewrite use possible link types
            return false;
        }

        if (parentTask.links) {
            parentTaskLinksCount = parentTask.links.length;
        }

        if (parentTaskLinksCount > 0) {
            for (var i = 0; i < parentTaskLinksCount; i++) {
                if (parentTask.links[i].parentTaskId == task.id || parentTask.links[i].dependenceTaskId == task.id) {
                    return false;
                }
            }
        }

        return true;
    };
    var checkPossibleLinkType = function () {
        var projectId = dependentTaskSelector.data("prjid"),
            parentTask = allProjectsHash[projectId].allTaskHash[parentTaskName.data("taskid")],
            dependentTask = allProjectsHash[projectId].allTaskHash[dependentTaskSelector.val()];

        var parentStart = parentTask.startDate ? parentTask.startDate : parentTask.crtdate;
        var dependentStart = dependentTask.startDate ? dependentTask.startDate : dependentTask.crtdate;

        var parentDeadline = parentTask.deadline ? parentTask.deadline : undefined;
        var dependentDeadline = dependentTask.deadline ? dependentTask.deadline : undefined;

        var possibleLinkTypes = common.getPossibleTypeLink(parentStart, parentDeadline, dependentStart, dependentDeadline, {});

        if (possibleLinkTypes[2] == common.linkTypeEnum.start_end) {
            linkTypeSelector.text(projectsJsResource.RelatedLinkTypeSE).data("value", common.linkTypeEnum.start_end);
        } else {
            linkTypeSelector.text(projectsJsResource.RelatedLinkTypeES).data("value", common.linkTypeEnum.end_start);
        }
    };
    var enableSaveLinkButton = function () {
        if (dependentTaskSelector.val() != "-1") {
            saveLinkButton.removeClass("disable");
        } else {
            saveLinkButton.addClass("disable");
        }
    };
    var addLinkForTasks = function (link) {
        // add to tasks

        var project = allProjectsHash[link.projectId];

        if (project.allTaskHash[link.parentTaskId].links) {
            project.allTaskHash[link.parentTaskId].links.push(link);
        } else {
            project.allTaskHash[link.parentTaskId].links = [link];
        }
        if (project.allTaskHash[link.dependenceTaskId].links) {
            project.allTaskHash[link.dependenceTaskId].links.push(link);
        } else {
            project.allTaskHash[link.dependenceTaskId].links = [link];
        }
    };
    var compareLinks = function (firstLink, secondLink) {
        if (firstLink.parentTaskId == secondLink.parentTaskId &&
            firstLink.dependenceTaskId == secondLink.dependenceTaskId) {
            return true;
        }

        return (firstLink.parentTaskId == secondLink.dependenceTaskId &&
            firstLink.dependenceTaskId == secondLink.parentTaskId);
    };

    // task

    var addNewTask = function (task) {
        blockChartInterface();
        var params = {};
        Teamlab.addPrjTask(params, task.projectId, task, { success: onAddTask, error: onTaskError });
    };
    var onAddTask = function (params, task) {
        allProjectsHash[task.projectId].allTaskHash[task.id] = task;
        allProjectsHash[task.projectId].allTaskHash[task.id].visible = true;

        chart.modelController().applyNewAddTaskId(task.id);

        allProjectsHash[task.projectId].taskCount++;
        setGanttIndex(task.projectId);
    };
    var updateTasksMilestone = function (milestoneId, tasksIds, links) {
        blockChartInterface();
        for (var i = 0, max = tasksIds.length; i < max; i++) {
            var data = {};
            data.newMilestoneID = milestoneId;

            Teamlab.updatePrjTask({ links: links }, tasksIds[i], data, {
                success: function (params, task) {
                    allProjectsHash[task.projectId].allTaskHash[task.id] = task;
                    setGanttIndex(task.projectId);
                    if (params.links) {
                        for (var i = 0; i < params.links.length; ++i) {
                            addTaskLink(params.links[i].link);
                        }
                    }
                }, error: onTaskError
            });
        }
    };
    var onTaskError = function () {
        unblockChartInterface();
    };
    var updateTask = function (task, params) {
        blockChartInterface();
        params = params || {};
        if (task.updRespFlag) {
            params.updRespFlag = true;
        }
        Teamlab.updatePrjTask(params, task.id, task, { success: onUpdateTask, error: onTaskError });
    };
    var updateTaskStack = function (tasks) {
        var i, count = tasks.length;
        if (!count) {
            return;
        }

        blockChartInterface();
        stackTasksCount = count;

        for (i = 0; i < count; i++) {
            Teamlab.updatePrjTask({}, tasks[i].id, tasks[i], { success: onUpdateTasksStack, error: onTaskError });
        }
    };
    var onUpdateTask = function (params, task) {

        if (params.status) {
            enableChartEvents();
            statusListTaskContainer.hide();
            statusListTaskContainer.data("id", "");
            if (!jq(this).hasClass("underline")) {
                chart.modelController().finalizeStatus(task.status);
            }
        }

        var visible = allProjectsHash[task.projectId].allTaskHash[task.id].visible;
        allProjectsHash[task.projectId].allTaskHash[task.id] = task;
        allProjectsHash[task.projectId].allTaskHash[task.id].visible = visible;

        if (params.updRespFlag) {
            chart.modelController().finalizeOperation('responsible', task.responsibles);
        }

        setGanttIndex(task.projectId);
    };
    var onUpdateTasksStack = function (params, task) {
        stackTasksCount = stackTasksCount - 1;
        allProjectsHash[task.projectId].allTaskHash[task.id] = task;

        if (stackTasksCount == 0) {
            setGanttIndex(task.projectId);
        }
    };
    var removeTask = function (params, taskId) {
        blockChartInterface();
        Teamlab.removePrjTask(params, taskId, { success: onRemoveTask, error: onErrorRemoveTask });
    };
    var onRemoveTask = function (params, task) {
        delete allProjectsHash[task.projectId].allTaskHash[task.id];

        allProjectsHash[task.projectId].taskCount--;
        setGanttIndex(task.projectId);
    };
    var onErrorRemoveTask = function () {


    };

    // milestone

    var addMilestone = function (milestone, tasksIds) {
        blockChartInterface();
        var projectId = milestone.projectId;
        Teamlab.addPrjMilestone({ tasksIds: tasksIds }, projectId, milestone, { success: onAddMilestone, error: onMilestoneError });
    };
    var updateMilestone = function (milestone, tasks) {
        blockChartInterface();
        var data = {},
            params = { tasks: tasks, milestoneId: milestone.id };
        if (tasks || typeof milestone.status === "undefined") {
            data = milestone;
            if (milestone.updRespFlag) {
                params.updRespFlag = true;
            }
        } else {
            data.status = milestone.status;
        }

        Teamlab.updatePrjMilestone(params, milestone.id, data, {
            success: onUpdateMilestone,
            error: onMilestoneError
        });
    };
    var onUpdateMilestone = function (params, milestone) {
        var i, length,
            project = allProjectsHash[milestone.projectId],
            oldMilestone = project.milestonesHash[milestone.id],
            tasks = params.tasks,
            tasksForUpdate = [];

        if (oldMilestone.deadline > milestone.deadline && tasks) {

            length = tasks.length;

            for (i = 0; i < length; ++i) {
                if (project.allTaskHash[tasks[i].id()].deadline > milestone.deadline && project.allTaskHash[tasks[i].id()].status == 1) {
                    tasks[i].milestone = params.milestoneId;
                    tasksForUpdate.push(convertToTeamlabTask(tasks[i], true));
                }
            }

            updateTaskStack(tasksForUpdate);
        }

        if (!tasks || !tasks.length || !tasksForUpdate.length) {
            unblockChartInterface();
        }

        project.milestonesHash[milestone.id] = milestone;

        if (params.updRespFlag) {
            chart.modelController().finalizeOperation('responsible', milestone.responsible)
        }
    };
    var onAddMilestone = function (params, milestone) {
        allProjectsHash[milestone.projectId].milestonesHash[milestone.id] = milestone;
        if (params.tasksIds) {
            updateTasksMilestone(milestone.id, params.tasksIds);
        }
        chart.modelController().applyNewAddMilestoneId(milestone.id, milestone.responsible);

        allProjectsHash[milestone.projectId].milestoneCount++;

        setGanttIndex(milestone.projectId);
    };
    var onMilestoneError = function (/*params, milestone*/) {
        refreshData = true;

        refreshChartData();
        showMilestoneWithTasksQuestionPopup();
        unblockChartInterface();
    };
    var removeMilestone = function (milestoneId) {
        blockChartInterface();
        Teamlab.removePrjMilestone(milestoneId, { success: onRemoveMilestone, error: onMilestoneError });
    };
    var onRemoveMilestone = function (params, milestone) {
        delete milestonesHash[milestone.id];

        milestoneCount--;
        setGanttIndex(milestone.projectId);
    };

    // links

    var addTaskLink = function (link) {
        blockChartInterface();
        Teamlab.addPrjTaskLink({ link: link }, link.parentTaskId, link, { success: onAddLink, error: onTaskLinkError });
    };
    var removeTaskLink = function (link) {
        blockChartInterface();
        var data = { dependenceTaskId: link.dependenceTaskId, parentTaskId: link.parentTaskId };
        Teamlab.removePrjTaskLink({ link: link }, link.dependenceTaskId, data, { success: onRemoveTaskLink, error: onTaskLinkError });
    };
    var onAddLink = function (params /*, data*/) {
        addLinkForTasks(params.link);
        unblockChartInterface();
    };
    var onTaskLinkError = function (params, error) {
        if (error[0] == "link already exist") {
            return;
        }

        refreshData = true;

        refreshChartData();
        ASC.Projects.Base.showCommonPopup("createNewLinkError", enableChartEvents, enableChartEvents);
        unblockChartInterface();
    };
    var onRemoveTaskLink = function (params /*, data*/) {
        var i, max, link = params.link, length,
            parentTask = allProjectsHash[link.projectId].allTaskHash[link.parentTaskId],
            dependentTask = allProjectsHash[link.projectId].allTaskHash[link.dependenceTaskId];

        if (!parentTask.links || !dependentTask.links) {
            return;
        }

        length = parentTask.links.length;

        for (i = 0, max = length; i < max; ++i) { // delete link from parent task
            if (compareLinks(parentTask.links[i], link)) {
                parentTask.links.splice(i, 1);
                break;
            }
        }

        length = dependentTask.links.length;

        for (i = 0, max = length; i < max; ++i) { // delete link from parent task
            if (compareLinks(dependentTask.links[i], link)) {
                dependentTask.links.splice(i, 1);
                break;
            }
        }

        unblockChartInterface();
    };

    // set Gantt index

    var setGanttIndex = function (projectId) {
        if (!saveOrderFlag) {
            unblockChartInterface();
            return;
        }
        allProjectsHash[projectId].ganttIndex = chart.modelController().throughIdsIndexes(projectId);
        var data = {};
        data.id = projectId;
        data.order = JSON.stringify(allProjectsHash[projectId].ganttIndex);
        Teamlab.setPrjGanttIndex({}, projectId, data, {
            success: function (params, data) {
                unblockChartInterface();
            },
            error: function (params, error) {
                unblockChartInterface();
            }
        });
        saveOrderFlag = false;
    };

    // Handlers

    var registerChartHandlers = function () {

        var kHandlerAddTask = '101',
            kHandlerAddMilestone = '102',
            kHandlerAddTaskLink = '140',
            kHandlerDeleteTask = '111',
            kHandlerDeleteMilestone = '112',
            kHandlerDeleteTaskLink = '145',
            kHandlerChangeTitleTask = '121',
            kHandlerChangeTitleMilestone = '122',
            kHandlerChangeTime = '123',
            kHandlerChangeTaskStatus = '124',
            kHandlerChangeMilestoneStatus = '125',
            kHandlerMoveTask = '130',
            kHandlerMoveGroupTasks = '150',
            kHandlerChangeResponsible = '160',

            kHandlerBeforeDeleteTask = '200',
            kHandlerBeforeDeleteMilestone = '220',
            kHandlerBeforeChangeTaskStatus = '240',
            kHandlerBeforeChangeMilestoneStatus = '260',
            kHandlerBeforeAddTaskLink = '270',
            kHandlerBeforeDeleteTaskLink = '280',
            kHandlerBeforeMoveTaskWithLinks = '300',
            kHandlerBeforeMenuAddTaskLink = '350',
            kHandlerBeforeChangeResponsibles = '360',

//          kHanderChangeTaskProperties         = '970',
//          kHanderChangeMilestoneProperties    = '980',

            kTaskSideBegin = -10,
            kTaskSideEnd = 10,

            kLinkBeginBegin = 0,
            kLinkEndEnd = 1,
            kLinkBeginEnd = 2,

            kHanderShowTaskPopUpWindow = '500',
            kHanderShowTaskPopUpCustomWindow = '501',
            kHanderShowEditPopUpMenuWindow = '502',
            kHanderShowRespPopUpMenuWindow = '503',
            kHanderShowEditElemPopUpMenuWindow = '504',

            DaysScale = 40,
            WeekScale = 100,
            MonthScale = 270,
            scaleType = -1,
            CollapseMilestones = 10,
            ExpandMilestones = 20;

        // custom operations
        //        chart.addHandler(kHanderChangeTaskProperties, function(p, m, t, element){
        //
        //        });
        //        chart.addHandler(kHanderChangeMilestoneProperties, function(p, m, element){
        //            updateMilestone(convertToTeamlabMilestone(element, true));
        //        });

        // task add/delete

        chart.addHandler(kHandlerAddTask, function (p, m, t, element, links) {
            saveOrderFlag = true;
            element.milestone = m;
            var task = convertToTeamlabTask(element);
            if (!task.projectId) task.projectId = p;
            addNewTask(task);
        });
        chart.addHandler(kHandlerDeleteTask, function (p, m, t, element, links) {
            saveOrderFlag = true;
            removeTask({}, t);
        });

        // task link add/delete

        chart.addHandler(kHandlerBeforeAddTaskLink, function (link, depTask, parTask) {
            var newLink = {};

            // var owner =  depTask.owner();
            // var ownerParent =  parTask.owner();

            newLink.parentTaskId = link.task.id();
            newLink.dependenceTaskId = link.parent.id();
            newLink.linkType = kLinkBeginEnd;
            newLink.projectId = link.task.owner();

            if (kTaskSideBegin === link.parentSide && kTaskSideEnd === link.side) {
                newLink.linkType = kLinkBeginEnd;
            } else {
                if (kTaskSideBegin === link.parentSide && kTaskSideBegin === link.side) {
                    newLink.linkType = kLinkBeginBegin;
                } else if (kTaskSideEnd === link.parentSide && kTaskSideEnd === link.side) {
                    newLink.linkType = kLinkEndEnd;
                }
            }
            addTaskLink(newLink);
            chart.modelController().finalize();
        });
        chart.addHandler(kHandlerBeforeDeleteTaskLink, function (link, depTask, parTask) {
            link.projectId = depTask.owner();
            removeTaskLink(link);
            chart.modelController().finalize();
        });
        chart.addHandler(kHandlerAddTaskLink, function (link, depTask, parTask) {
            link.projectId = depTask.owner();
            addTaskLink(link);
            chart.modelController().finalize();
        });
        chart.addHandler(kHandlerDeleteTaskLink, function (link, depTask, parTask) {
            link.projectId = depTask.owner();
            removeTaskLink(link);
            chart.modelController().finalize();
        });
        chart.addHandler(kHandlerBeforeMenuAddTaskLink, function (task) {
            showCreateNewLinkPopup(task);
        });

        // milestone add/delete

        chart.addHandler(kHandlerDeleteMilestone, function (p, m, element) {
            saveOrderFlag = true;
            removeMilestone(element.id());
        });
        chart.addHandler(kHandlerAddMilestone, function (p, m, element) {
            saveOrderFlag = true;
            var tasks = element.tasks();
            var tasksIds = [];
            for (var i = 0; i < tasks.length; ++i) {
                tasksIds.push(tasks[i].id());
            }
            var milestone = convertToTeamlabMilestone(element);
            if (!milestone.projectId) milestone.projectId = p;
            addMilestone(milestone, tasksIds);
        });

        // title change

        chart.addHandler(kHandlerChangeTitleTask, function (p, m, t, element) {
            element.milestone = m;
            updateTask(convertToTeamlabTask(element, true));
        });
        chart.addHandler(kHandlerChangeTitleMilestone, function (p, m, element) {
            updateMilestone(convertToTeamlabMilestone(element, true));
        });

        // time change

        chart.addHandler(kHandlerChangeTime, function (p, m, t, element, relaiteditems) {
            if (!element.isMilestone()) {
                var tasksForUpdate = [];
                element.milestone = m;

                if (relaiteditems) {
                    tasksForUpdate.push(convertToTeamlabTask(element, true));
                    for (var index = 0; index < relaiteditems.length; ++index) {
                        if (relaiteditems[index]['change']) {
                            relaiteditems[index].milestone = m;
                            tasksForUpdate.push(convertToTeamlabTask(relaiteditems[index], true));
                        }
                    }
                    updateTaskStack(tasksForUpdate);
                } else {
                    updateTask(convertToTeamlabTask(element, true));
                }

            } else {
                updateMilestone(convertToTeamlabMilestone(element, true), element.tasks());
            }
        });

        // status

        chart.addHandler(kHandlerChangeTaskStatus, function (p, m, t, element) {
            element.milestone = m;
            var task = convertToTeamlabTask(element, true);
            task.status = element.status();
            task.statusId = element.customTaskStatus();
            updateTask(task, { status: true});
        });
        chart.addHandler(kHandlerChangeMilestoneStatus, function (p, m, element) {
            var data = {};
            data.id = m;
            data.status = element.status();
            updateMilestone(data);
        });

        // move task

        chart.addHandler(kHandlerMoveTask, function (p, m, t, pt, mto, element, links, isUndo) {
            saveOrderFlag = true;
            if (!m && !mto) {
                setGanttIndex(p);
                return;
            }
            if (isUndo) {
                updateTasksMilestone(m, [t], links);
            } else {
                if (links) {
                    for (var j = 0; j < links.length; ++j) {
                        removeTaskLink(links[j].link);
                    }
                }
                updateTasksMilestone(mto, [t]);
            }
        });
        chart.addHandler(kHandlerMoveGroupTasks, function (milestone, items) {
            saveOrderFlag = true;
            if (milestone) {
                updateTasksMilestone(milestone, items);
            } else {
                updateTasksMilestone(0, items);
            }
        });
        chart.addHandler(kHandlerBeforeMoveTaskWithLinks, function () {
            showMoveTaskOutMilestonePopup();
        });

        // Undo/Redo UI

        chart.addHandler('UndoRedoUI', function (undo, redo) {
            if (undo)
                undoBtn.removeClass("disable");
            else
                undoBtn.addClass("disable");

            if (redo)
                redoBtn.removeClass("disable");
            else
                redoBtn.addClass("disable");
        });

        // before delete element

        chart.addHandler(kHandlerBeforeDeleteTask, function (p, m, t, element, links) {
            showTaskQuestionPopup(element);
        });
        chart.addHandler(kHandlerBeforeDeleteMilestone, function (p, m, element) {
            showMilestoneQuestionPopup(m);
        });
        chart.addHandler(kHandlerBeforeChangeTaskStatus, function (p, m, t, element, cs) {
            if (element.subtasks().length && element.status() != 2) {
                for (var i = 0; i < element.subtasks().length; i++) {
                    if (element.subtasks()[i].status == 1) {
                        showTaskWithSubtasksQuestionPopup(element);
                        return;
                    }
                }
            }
            chart.modelController().finalize(cs);
        });
        chart.addHandler(kHandlerBeforeChangeMilestoneStatus, function (p, m, element) {
            var tasks = element.tasks();
            if (tasks.length && element.status() != 1) {
                for (var i = 0; i < tasks.length; i++) {
                    if (tasks[i].status() == 1) {
                        showMilestoneWithTasksQuestionPopup();
                        return;
                    }
                }
            }
            chart.modelController().finalize();
        });
        chart.addHandler(kHanderShowTaskPopUpWindow, function (element, coords) {
            var element = element || {};
            showStatusListContainer(element, coords);
        });
        chart.addHandler(kHanderShowTaskPopUpCustomWindow, function (element, coords) {
            var element = element || {};
            showStatusListTaskContainer(element, coords);
        });
        chart.addHandler(kHanderShowEditPopUpMenuWindow, function (coords, element, isTask, project) {
            if (readMode) return;
            menuCoordinates = coords;
            menuProject = project;
            if (isTask) {
                showTaskContextMenu(element, coords);
            } else {
                showMilestoneContextMenu(element, coords);
            }
        });

        // responsibles

        chart.addHandler(kHanderShowRespPopUpMenuWindow, function (element, coords, clickOnLeftPanel) {
            menuCoordinates = coords;
            if (!clickOnLeftPanel) {
                setResponsibleMenu.data("id", "");
            }
            setResponsibleMenu.data("prjid", element.owner());
            if (element.isMilestone()) {
                showResponsiblePanel("milestone", element.id());
            } else {
                showResponsiblePanel("task", element.id());
            }

            (undefined === clickOnLeftPanel) ? respDummyHide = true : respDummyHide = false;
        });

        // responsibles for undo

        chart.addHandler(kHandlerChangeResponsible, function (element) {
            if (element.isMilestone()) {
                updateMilestone(convertToTeamlabMilestone(element, true));
            } else {
                var task = allProjectsHash[element.owner()].allTaskHash[element.id()],
                    taskResp = element.responsibles();
                for (var i = 0; i < taskResp.length; i++) {
                    task.responsibles.push(taskResp[i].id);
                }
                delete task.status;
                updateTask(task);
            }
        });

        //        chart.addHandler(kHanderShowEditElemPopUpMenuWindow, function (element) {
        //
        //            disableChartEvents();
        //
        //            var id = element.id();
        //            if (element.isMilestone()) {
        //
        //                Teamlab.getPrjMilestone ({}, element.id(), function(params, milestoneTarget) {
        //
        //                    var milestone =
        //                    {
        //                        id: milestoneTarget.id,
        //                        project: milestoneTarget.projectId,
        //                        responsible: milestoneTarget.responsible.id,
        //                        deadline: milestoneTarget.deadline,
        //                        title: milestoneTarget.title,
        //                        description: milestoneTarget.description,
        //                        isKey: milestoneTarget.isKey,
        //                        isNotify:  milestoneTarget.isNotify
        //                    };
        //
        //                    ASC.Projects.MilestoneAction.onGetMilestoneBeforeUpdate(milestone);
        //                });
        //
        //            } else {
        //                ASC.Projects.TaskAction.showUpdateTaskForm(element.id());
        //            }
        //
        //            return true;
        //        });

        chart.viewController().onchangetimescale = function (scale) {
            var type = -1;
            if (DaysScale < scale && scale <= WeekScale) {
                type = 7;
            } else if (scale > WeekScale) {
                type = 4;
            } else {
                type = 1;
            }

            if (type !== scaleType) {
                scaleType = type;
                autoScaleCheck(scaleType);
            }
        };
        chart.viewController().onshowhint = function (type, show, coords) {
            if (show) {

                if (CollapseMilestones === type) {
                    showHint("collapse", coords)
                } else if (ExpandMilestones === type) {
                    showHint("expand", coords);
                }
            } else {
                jq(".gantt-hint").hide();
            }
        };

        // LEFT PANEL HANDLERS

        var filedPanelHeight = ganttCellChecker.height();
        var hiddenModeFieldsPanel = false;

        var saveHiddenRows = function (e) {
            if (hiddenModeFieldsPanel) {

                var domElement = e.toElement;
                if (e.currentTarget) domElement = e.currentTarget;
                var elementId = domElement.id;

                var cells = localStorageManager.getItem("checkedCells").split(",");
                cells.splice(cells.indexOf(elementId), 1);
                if (domElement.checked)
                    cells.splice(0, 0, elementId);
                else
                    cells.push(elementId);

                localStorageManager.setItem("checkedCells", cells.join(','));

                var choosedFields = [], checkedInputs = ganttCellChecker.find("input:checked");
                for (var i = 0; i < checkedInputs.length; i++) {
                    choosedFields.push(jq(checkedInputs[i]).attr("id"));
                }

                if (localStorageManager.getItem("visibleCells") && 0 !== localStorageManager.getItem("visibleCells").length)
                    choosedFields.push.apply(choosedFields, localStorageManager.getItem("visibleCells").split(","));

                chart.leftPanelController().addRowsAvailable(cells);
                chart.leftPanelController().showHiddenRows(choosedFields);

                localStorageManager.setItem("visibleCells", choosedFields.join(',') + ',' + localStorageManager.getItem("visibleCells"));

                ganttCellChecker.hide();
            }
        };
        var saveAvailableRows = function (e) {
            if (!hiddenModeFieldsPanel) {

                var i = 0, names = [];

                var availableCells = [];
                var visibleCells = [];

                if (localStorageManager.getItem("checkedCells") && 0 !== localStorageManager.getItem("visibleCells").length)
                    availableCells = localStorageManager.getItem("checkedCells").split(",");
                if (localStorageManager.getItem("visibleCells") && 0 !== localStorageManager.getItem("visibleCells").length)
                    visibleCells = localStorageManager.getItem("visibleCells").split(",");

                var checkedInputs = ganttCellChecker.find("input:checked");

                for (i = 0; i < checkedInputs.length; i++) {
                    names.push(jq(checkedInputs[i]).attr("id"));
                }

                for (i = availableCells.length - 1; i >= 0; --i) {
                    if (-1 == names.indexOf(availableCells[i])) {
                        availableCells.splice(i, 1);
                    }
                }

                for (i = visibleCells.length - 1; i >= 0; --i) {
                    if (-1 == availableCells.indexOf(visibleCells[i])) {
                        visibleCells.splice(i, 1);
                    }
                }

                if (0 === checkedInputs.length) {
                    visibleCells = [];
                    availableCells = [];
                } else {

                    for (i = names.length - 1; i >= 0; --i) {
                        if (-1 !== availableCells.indexOf(names[i])) {
                            names.splice(i, 1);
                        }
                    }

                    availableCells = names.concat(availableCells);
                    visibleCells = names.concat(visibleCells);
                }

                chart.leftPanelController().addRowsAvailable(availableCells);
                chart.leftPanelController().showHiddenRows(visibleCells);

                localStorageManager.setItem("checkedCells", availableCells.join(','));
                localStorageManager.setItem("visibleCells", visibleCells.join(','));

                //console.log('checkedCells: ' + localStorageManager.getItem("checkedCells"));
                //console.log('visibleCells: ' + localStorageManager.getItem("visibleCells"));

                ganttCellChecker.hide();
            }
        };
        var rebuildCellChecker = function (cells) {
            var fields = {          // replase on resources strings
                menuItems: [
                    { id: "Responsibility", title: projectsJsResource.GanttLeftPanelResponsibles },
                    { id: "BeginDate", title: projectsJsResource.GanttLeftPanelBeginDate },
                    { id: "EndDate", title: projectsJsResource.GanttLeftPanelEndDate },
                    { id: "Status", title: projectsJsResource.GanttLeftPanelStatus },
                    { id: "Priority", title: projectsJsResource.GanttLeftPanelPriority }
                ]
            };

            ganttCellChecker = jq("#ganttCellChecker");

            if (cells) {

                var find = false;

                for (var j = 0; j < fields.menuItems.length; ++j) {

                    find = false;

                    for (var i = 0; i < cells.length; ++i) {
                        if (cells[i] === fields.menuItems[j].id) {
                            find = true;
                            break;
                        }
                    }

                    if (!find) { fields.menuItems.splice(j, 1); --j; }
                }
            }

            ganttCellChecker.find(".panel-content").empty().append(jq.tmpl("projects_linedListWithCheckbox", fields));
            ganttCellChecker.find("input").css("display", cells ? "none" : "");
            ganttCellChecker.find('.dropdown-item').each(function () { jq(this).css('border', 'none'); });
            ganttCellChecker.find('.dropdown-content').each(function () { jq(this).css('margin-top', '0px'); });
            ganttCellChecker.addClass("gantt-context-menu").css("width", "auto");

            jq("#Responsibility").click(function (e) { saveHiddenRows(e); });
            jq("#BeginDate").click(function (e) { saveHiddenRows(e); });
            jq("#EndDate").click(function (e) { saveHiddenRows(e); });
            jq("#Status").click(function (e) { saveHiddenRows(e); });
            jq("#Priority").click(function (e) { saveHiddenRows(e); });
        };

        jq("#showChoosedFields").click(function (e) {
            saveAvailableRows(e);
        });

        chart.leftPanelController().onfieldsfilter = function (e, save, left, top, cells) {

            localStorageManager.setItem("checkedCells", cells.join(','));

            if (save) {
                var visibleCells = [];
                if (localStorageManager.getItem("visibleCells") && 0 !== localStorageManager.getItem("visibleCells").length)
                    visibleCells = localStorageManager.getItem("visibleCells").split(",");

                if (visibleCells.length) {
                    for (var j = visibleCells.length - 1; j >= 0; --j) {
                        if (-1 === cells.indexOf(visibleCells[j])) {
                            visibleCells.splice(j, 1);
                        }
                    }

                    localStorageManager.setItem("visibleCells", visibleCells.join(','));
                }

                //console.log('checkedCells: ' + localStorageManager.getItem("checkedCells"));
                //console.log('visibleCells: ' + localStorageManager.getItem("visibleCells"));

                return;
            }

            if (ganttCellChecker.is(":visible") && !hiddenModeFieldsPanel) {
                ganttCellChecker.hide();
                return;
            }

            showPanelFlag = true;
            disableChartEvents();

            if (e.preventDefault) { e.preventDefault() }        // Chrome
            else if (e.returnValue) { e.returnValue = false; }       // IE9+
            if (e.stopPropagation) { e.stopPropagation(); }     // Firefox

            rebuildCellChecker(undefined);

            jq(".gantt-context-menu").hide();
            ganttCellChecker.css({
                overflow: 'hidden', left: left - 20, top: top,
                height: 'auto' // filedPanelHeight
            });

            ganttCellChecker.find("input").prop("checked", false);
            for (var i = 0; i < cells.length; i++) {
                ganttCellChecker.find("input[id=" + cells[i] + "]").prop("checked", true);
            }
            jq("#showChoosedFields").css({ display: '' });
            hiddenModeFieldsPanel = false;

            ganttCellChecker.show();
        };
        chart.leftPanelController().onhiddenfieldsfilter = function (e, save, left, top, cells, visibleCells) {

            localStorageManager.setItem("visibleCells", visibleCells.join(','));
            if (save) return;

            if (ganttCellChecker.is(":visible") && hiddenModeFieldsPanel) {
                ganttCellChecker.hide();
                return;
            }

            showPanelFlag = true;
            disableChartEvents();

            if (e.preventDefault) { e.preventDefault() }        // Chrome
            else if (e.returnValue) { e.returnValue = false; }  // IE9+
            if (e.stopPropagation) { e.stopPropagation(); }     // Firefox

            rebuildCellChecker(cells);

            jq(".gantt-context-menu").hide();
            ganttCellChecker.css({
                overflow: 'hidden', left: left - 20, top: top,
                height: 'auto' // cells.length * 20 + 30
            });
            jq("#showChoosedFields").css({ display: 'none' });
            ganttCellChecker.find("input").prop("checked", false);
            for (var i = 0; i < cells.length; i++) {
                ganttCellChecker.find("input[id=" + cells[i] + "]").prop("checked", false);
            }

            hiddenModeFieldsPanel = true;
            ganttCellChecker.show();
        };
        chart.leftPanelController().onchangetimes = function (coords, element, begin) {
            datePicker(true, coords, element, begin);
        }
    };
    var autoScaleCheck = function (value) {
        var comboText = jq(".zoom-presets .combobox-title-inner-text"),
            currentScale = zoomScale.val(),
            option;

        if (currentScale != value) {
            autoScaleFlag = true;
            zoomScale.val(value).change();
            localStorageManager.setItem("ganttZoomScale", value);
        }
    };
    var showHint = function (type, coords) {
        var hint = jq(".gantt-hint");
        if (!hint.length) {
            jq("body").append("<div class='gantt-hint'></div>");
            hint = jq(".gantt-hint");
        }
        if (type == "expand") {
            hint.text(localizeStrings.expand);
        } else {
            hint.text(localizeStrings.collapse);
        }
        hint.css({ left: coords.left - 10, top: coords.top + 10 });
        hint.show();
    };

    // context menus

    var showStatusListContainer = function (element, coords) {
        if (statusListContainer.data("id") == element.id()) {
            statusListContainer.data("id", "");
            return true;
        }

        statusListContainer.data("id", element.id());
        showContextMenu(statusListContainer, coords.left - 10, coords.top - 1);
    };

    var showStatusListTaskContainer = function (element, coords) {
        if (statusListTaskContainer.data("id") == element.id()) {
            statusListTaskContainer.data("id", "");
            return true;
        }
        if (element.isTask) {
            var master = ASC.Projects.Master;
            var statuses = master.customStatuses.filter(function (item) {
                return typeof item.available !== "undefined" && item.available === false;
            });

            var currentUserId = Teamlab.profile.id;
            for (var i = 0; i < statuses.length; i++) {
                var id = statuses[i].id;
                var $li = statusListContainer.find("li[dataid=" + id + "]");
                if (element.createdBy === currentUserId ||
                    element.project.responsibles().id === currentUserId ||
                    master.isModuleAdmin) {
                    $li.show();
                } else {
                    $li.hide();
                }
            }
        }

        statusListTaskContainer.data("id", element.id());
        showContextMenu(statusListTaskContainer, coords.left - 10, coords.top - 1);
    };
    var showTaskContextMenu = function (element, coords) {
        if (taskContextMenu.is(":visible") && taskContextMenu.data("id") == element.id()) {
            taskContextMenu.hide();
            return true;
        }
        var task = allProjectsHash[element.owner()].allTaskHash[element.id()];
        taskContextMenu.data("id", task.id);
        taskContextMenu.data("prjid", element.owner());
        if (task.status == 2) {
            taskContextMenu.find(".edit").hide();
            taskContextMenu.find(".addlink").hide();
            jq("#taskResponsible").hide();
            taskContextMenuStatus.removeAttr("class").addClass("open dropdown-item");
            taskContextMenuStatus.text(taskContextMenuStatus.data("opentext"));
        } else {
            taskContextMenu.find(".edit").show();
            taskContextMenu.find(".addlink").show();
            jq("#taskResponsible").show();
            taskContextMenuStatus.removeAttr("class").addClass("closed dropdown-item");
            taskContextMenuStatus.text(taskContextMenuStatus.data("closetext"));
        }
        showContextMenu(taskContextMenu, coords.left, coords.top);
    };
    var showMilestoneContextMenu = function (element, coords) {
        if (milestoneContextMenu.is(":visible") && milestoneContextMenu.data("id") == element.id()) {
            milestoneContextMenu.hide();
            return true;
        }

        var milestone = allProjectsHash[element.owner()].milestonesHash[element.id()];
        milestoneContextMenu.data("id", milestone.id);
        milestoneContextMenu.data("prjid", element.owner());
        if (milestone.status == 1) {
            milestoneContextMenu.find(".edit").hide();
            milestoneContextMenu.find(".addMilestoneTask").hide();
            jq("#milestoneResponsible").hide();
            milestoneContextMenuStatus.removeAttr("class").addClass("open dropdown-item");
            milestoneContextMenuStatus.text(taskContextMenuStatus.data("opentext"));
        } else {
            milestoneContextMenu.find(".edit").show();
            milestoneContextMenu.find(".addMilestoneTask").show();
            jq("#milestoneResponsible").show();
            milestoneContextMenuStatus.removeAttr("class").addClass("closed dropdown-item");
            milestoneContextMenuStatus.text(taskContextMenuStatus.data("closetext"));
        }
        showContextMenu(milestoneContextMenu, coords.left, coords.top);
    };
    var showResponsiblePanel = function (type, id) {
        if (setResponsibleMenu.data("id") == id) {
            setResponsibleMenu.data("id", "");
            return true;
        }
        var entityResponsibles = {},
            projectId = setResponsibleMenu.data("prjid");

        setResponsibleMenu.data("id", id);
        setResponsibleMenu.data("type", type);

        if (type == "task") {
            initResponsibleSelector(allProjectsHash[projectId].team, "checkbox");
            entityResponsibles = allProjectsHash[projectId].allTaskHash[id].responsibles;
        } else {
            initResponsibleSelector(allProjectsHash[projectId].team, "radio");
            entityResponsibles = [allProjectsHash[projectId].milestonesHash[id].responsible];
        }
        for (var i = 0; i < entityResponsibles.length; i++) {
            if (entityResponsibles[i].id) {
                jq("#" + entityResponsibles[i].id).prop("checked", true);
            } else {
                jq("#" + entityResponsibles[i]).prop("checked", true);
            }
        }

        showContextMenu(setResponsibleMenu, menuCoordinates.left, menuCoordinates.top);
    };
    var showContextMenu = function (menu, left, top) {
        jq(".gantt-context-menu").hide();

        changeMenuOrientation(menu, left, top);

        showPanelFlag = true;
        document.getElementById(menu.attr("id")).oncontextmenu = function () {
            return false;
        };
        menu.show();
    };
    var changeMenuOrientation = function (menu, left, top) {
        var pageWidth = window.innerWidth,
            pageHeight = window.innerHeight,
            menuWidth = menu.outerWidth(),
            menuHeight = menu.outerHeight();

        // horizontal scroll
        if (pageWidth - left < menuWidth) {
            left = pageWidth - menuWidth - 14;
        }

        // vertical scroll
        if (pageHeight - top < menuHeight) {
            top = top - menuHeight - 16;
        } 

        menu.css({ left: left, top: top });
    };

    var onWheel = function (e) {
        //for status list
        jq(".gantt-context-menu").hide();
        showPanelFlag = true;

        chart.onmousewheel(e);
        chart.update();

        jq(".advanced-selector-container ").first().css({ display: 'none' });
    };

    // touch

    var touchHandler = function (event) {
        var touches = event.changedTouches, first = touches[0], type = "";
        switch (event.type) {
            case "touchstart":
                type = "mousedown";
                break;
            case "touchmove":
                type = "mousemove";
                break;
            case "touchend":
                type = "mouseup";
                break;
            default:
                return;
        }

        chart.setEnableTouch(true);

        var simulatedEvent = document.createEvent("MouseEvent");
        if (simulatedEvent) {
            simulatedEvent.initMouseEvent(type, true, true, window, 1,
                first.screenX, first.screenY,
                first.clientX, first.clientY, false,
                false, false, false, 0/*left*/, null);

            first.target.dispatchEvent(simulatedEvent);
        }
        event.preventDefault();
    };
    var initTouch = function () {
        document.addEventListener("touchstart", touchHandler, true);
        document.addEventListener("touchmove", touchHandler, true);
        document.addEventListener("touchend", touchHandler, true);
        document.addEventListener("touchcancel", touchHandler, true);
    };

    // print

    var getContentForPrint = function () {
        chart.print();
        var dataUrl = layers[0].toDataURL();
        var onloadStr = "javascript:window.print();";
        if (!jq.browser.opera) {
            onloadStr += " window.close();";
        }
        var windowContent = '<!DOCTYPE html>';
        windowContent += '<html>';
        windowContent += '<head>';
        windowContent += '<title>' + document.title + '</title>';
        windowContent += '<style>@media print{img{height:90%;} @page { size: landscape; }}</style>';
        windowContent += '</head>';
        windowContent += '<body style="margin:0;padding:0;" onload="' + onloadStr + '">';

        //        windowContent += '<p>';
        //
        //        var userId = teamMemberFilter.val();
        //        if (userId != "-1") {
        //            var responsible = projectsFilterResource.NoResponsible;
        //            if (userId != common.emptyGuid) {
        //                var user = common.userInProjectTeam(userId);
        //                responsible = user.displayName;
        //            }
        //            windowContent += responsible;
        //        }
        //
        //        if (openTaskOnly.is(":checked")) {
        //            windowContent += '&nbsp;&nbsp;&nbsp';
        //            windowContent += jq(".task-filter-container label").text();
        //        }
        //        windowContent += '</p>';

        windowContent += '<img src="' + dataUrl + '">';
        windowContent += '</body>';
        windowContent += '</html>';
        return windowContent;
    };

    // date picker (proxy)

    var dateControl = null, direction;
    var datePicker = function (show, coords, element, end) {
        direction = end;

        if (null === dateControl) {
            dateControl = document.createElement('input');
            if (dateControl) {

                dateControl.id = 'datepicker-chart';

                dateControl.style.color = 'transparent';
                dateControl.style.border = 'none';
                dateControl.style.position = 'absolute';
                dateControl.style.zIndex = 100;
                dateControl.style.width = '0px';
                dateControl.style.height = '30px';
                dateControl.style.background = 'transparent';
                dateControl.onfocus = 'this.blur()';

                document.body.appendChild(dateControl);

                jq("#datepicker-chart").mask(ASC.Resources.Master.DatePatternJQ);
                jq("#datepicker-chart").datepicker({
                    onSelect: function () {
                        chart.modelController().finalizeOperation('timechanage', {
                            time: jq("#datepicker-chart").datepicker("getDate"),
                            direction: direction
                        });
                    }
                });
            }
        }

        if (dateControl) {
            dateControl.style.left = (coords.left + 10) + 'px';
            dateControl.style.top = (coords.top - (30 - 4)) + 'px';

            (show ? dateControl.style.display = '' : dateControl.style.display = 'none');

            if (element.isMilestone()) {
                jq("#datepicker-chart").datepicker('setDate', element.endDate());
            } else {
                jq("#datepicker-chart").datepicker('setDate', (!direction || element.isUndefinedEndTime()) ? element.beginDate() : element.endDate());
            }

            jq("#datepicker-chart").datepicker(show ? "show" : "hide");
            jq("#ui-datepicker-div").focus();
        }
    };

    return {
        init: init,
        enableChartEvents: enableChartEvents,
        disableChartEvents: disableChartEvents
    };
})(jQuery);