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


/*******************************************************************************/
if (typeof ASC === "undefined")
    ASC = {};

if (typeof ASC.Projects === "undefined")
    ASC.Projects = {};

ASC.Projects.Common = (function () {
    var initApi = false,
        isInitMobileBanner = false,
        baseObject = ASC.Projects,
        teamlab = Teamlab,
        cPage,
        emptyGuid = "00000000-0000-0000-0000-000000000000",
        projectsForFilter = [],
        projectCache = [],
        emptyScreenShowed = false,
        handlers = [],
        master = baseObject.Master;

    var init = function () {
        window.onpopstate = null;

        initMobileBanner();
        initApiData();
        bindCommonEvents();
        initPages();
        showEmptyScreen();

        setTimeout(function() { window.onpopstate = init; }, 200);
    };

    function initMobileBanner() {
        if (isInitMobileBanner || !jq(".mobileApp-banner").length) return;
        isInitMobileBanner = true;
        jq(".mobileApp-banner_btn.app-store").trackEvent("mobileApp-banner", "action-click", "app-store");
        jq(".mobileApp-banner_btn.google-play").trackEvent("mobileApp-banner", "action-click", "google-play");
    };

    function initApiData() {
        if (initApi) return;

        initApi = true;

        if (typeof (master) != 'undefined') {
            if (typeof (master.Projects) != 'undefined' && master.Projects != null) {
                master.Projects = teamlab.create('prj-projects', null, master.Projects.response);
            }
            
            if (typeof (master.Tags) != 'undefined' && master.Tags != null) {
                master.Tags = master.Tags.response;
            }
            
            if (typeof (master.Team) != 'undefined' && master.Team != null) {
                master.TeamWithBlockedUsers = teamlab.create('prj-projectpersons', null, master.Team.response);
                master.Team = baseObject.Common.removeBlockedUsersFromTeam(master.TeamWithBlockedUsers);
            }
            
            if (typeof (master.Milestones) != 'undefined' && master.Milestones != null) {
                master.Milestones = teamlab.create('prj-milestones', null, master.Milestones.response);
                master.Milestones = master.Milestones.sort(milestoneSort);
            } 
        } else {
            ASC.Projects.Master = {};
        }
    };
    
    function initPages() {
        var action = jq.getURLParam('action'),
            id = jq.getURLParam('id'),
            prjId =jq.getURLParam('prjID'),
            href = location.href;

        var currentPage = document.location.pathname.match(/[^\/]+$/);
        var startModules = baseObject.Resources.StartModules;
        if (Teamlab.profile.isVisitor) {
            startModules = baseObject.Resources.StartModules = startModules.filter(function (item) {
                return item.StartModuleType !== 3; //timetracking
            });
        }

        if (currentPage === null) {
            var startModule = startModules.find(function (item) {
                    return item.StartModuleType === master.StartModuleType;
            }) || startModules[0];

            currentPage = master.Projects.length !== 0 ? startModule.Page : "Projects.aspx";
            href = document.location.pathname + currentPage;
            history.pushState({ href: href }, { href: href }, href);
        } else {
            currentPage = currentPage[0];
        }

        unbindEvents(); // remove events handlers for previos pages

        initControl(baseObject.TaskAction);
        initControl(baseObject.MilestoneAction);

        //init projNavPanel
        href = href.toLowerCase();

        if (jq.getURLParam('prjId') && href.indexOf("ganttchart.aspx") === -1 && href.indexOf("timer.aspx") === -1) {
            baseObject.projectNavPanel.init();
        } else {
            baseObject.projectNavPanel.hide();
        }

        initControl(baseObject.navSidePanel);

        currentPage = currentPage.toLowerCase();

        switch (currentPage) {
            case "tasks.aspx":
                cPage = id ? baseObject.TaskDescriptionPage : baseObject.TasksManager;
                break;
            case "messages.aspx":
                if (action) {
                    cPage = baseObject.DiscussionAction;
                    ckeditorConnector.load(function () {
                        baseObject.Common.ckEditor = jq("#ckEditor").ckeditor({ toolbar: 'PrjMessage', extraPlugins: 'oembed,teamlabcut,codemirror', removePlugins: 'div', filebrowserUploadUrl: 'fckuploader.ashx?newEditor=true&esid=projects_comments' }).editor;
                        baseObject.Common.ckEditor.on("change", cPage.showHidePreview);
                    });
                }
                if (id && action == null) {
                    cPage = baseObject.DiscussionDetails;
                }
                if (id == null && action == null) {
                    cPage = baseObject.Discussions;
                }
                break;
            case "milestones.aspx":
                cPage = baseObject.AllMilestones;
                break;
            case "projects.aspx":
                if (action == null) {
                    if (prjId == null) {
                        cPage = baseObject.AllProject;
                    } else {
                        cPage = baseObject.Description;
                    }
                } else {
                    cPage = baseObject.ProjectAction;
                    jq(".mainPageContent").children(".loader-page").hide();
                }

                jq('#projectTitleContainer .inputTitleContainer').css('width', '100%');
                if (action === "edit") {
                    jq('.dottedHeader').removeClass('dottedHeader');
                    jq('#projectDescriptionContainer').show();
                    jq('#notifyManagerCheckbox').attr('disabled', 'disabled');
                    jq('#projectTagsContainer').show();
                }
                break;
            case "projectteam.aspx":
                cPage = baseObject.ProjectTeam;
                break;
            case "timer.aspx":
                cPage = baseObject.TimeTraking;
                break;
            case "timetracking.aspx":
                baseObject.TimeTrakingEdit.init();
                cPage = baseObject.TimeSpendActionPage;
                break;
            case "projecttemplates.aspx":
                cPage = baseObject.Templates;
                break;
            case "reports.aspx":
                cPage = jq.getURLParam("tmplId") || jq.getURLParam("reportType") ? baseObject.ReportView : baseObject.GeneratedReport;
                break;
            case "contacts.aspx":
                cPage = baseObject.Contacts;
                break;
            case "import.aspx":
                cPage = baseObject.Import;
                break;
            case "ganttchart.aspx":
                cPage = baseObject.GantChartPage;
                break;
            case "settings.aspx":
                cPage = baseObject.SettingsManager;
                break;
        }

        if (cPage && cPage.hasOwnProperty("init")) {
            cPage.init();
        }

        if (id === null && action === null && prjId) {
            var project = projectCache[Number(prjId)];
            if (typeof project !== "undefined") {
                teamlab.call(teamlab.events.getPrjProject, this, [null, project]);
            } else {
                teamlab.bind(teamlab.events.getPrjProject, function(params, prj) {
                     projectCache[prj.id] = prj;
                });
                teamlab.getPrjProject({}, prjId);
            }
        }
    };

    function unbindEvents() {
        if (location.href.toLowerCase().indexOf("ganttchart.aspx") > 0) return;
        if (typeof cPage !== "undefined" && typeof cPage.unbindListEvents === "function") {
            cPage.unbindListEvents();
        }
    };

    function initControl(control) {
        if (control && control.hasOwnProperty('init')) {
            control.init();
        }
    }

    function bindCommonEvents() {
        if (handlers.length) return;
        handlers.push(teamlab.bind(teamlab.events.getException, function (params, errors) {
            if (errors && errors[0] === "unauthorized request") {
                window.location = "/Auth.aspx";
            }
        }));

        handlers.push(teamlab.bind(teamlab.events.updatePrjProjectStatus, function (params, project) {
            projectsForFilter = [];
            if (project.status === 0) {
                if (typeof getProjectById(project.id) === "undefined") {
                    master.Projects.push(project);
                }
            } else {
                master.Projects = master.Projects.filter(function (item) {
                    return item.id !== project.id;
                });
            }

            projectCache[project.id] = project;
        }));

        function onRemoveProjects(params, projects) {
            projectsForFilter = [];

            function filterProject(project) {
                return function(item) {
                    return item.id !== project.id;
                }
            }

            for (var i = 0; i < projects.length; i++) {
                var project = projects[i];

                master.Projects = master.Projects.filter(filterProject(project));

                delete projectCache[project.id];
            }
        }


        handlers.push(teamlab.bind(teamlab.events.removePrjProjects, onRemoveProjects));

        handlers.push(teamlab.bind(teamlab.events.removePrjProject, function (params, project) {
            onRemoveProjects(null, [project]);
        }));
    };
    
    function showEmptyScreen() {
        if (master.ProjectsCount !== 0 || emptyScreenShowed ||
            cPage === baseObject.ProjectAction ||
            cPage === baseObject.Import ||
            cPage === baseObject.GantChartPage ||
            cPage === baseObject.ReportView ||
            cPage === baseObject.Templates 
            ) return;

        function newBlock(image, title, text) {
            return { image: image, title: title, text: text };
        }

        if (!master.CanCreateProject) return;
        var commonResource = baseObject.Resources.CommonResource;

        var tmplObj = {
            blocks: [
                newBlock("design-project-hierarchy.png", commonResource.DashboardDesignProjectHierarchy,
                    [
                        commonResource.DashboardDesignProjectHierarchyFirstLine,
                        commonResource.DashboardDesignProjectHierarchySecondLine,
                        commonResource.DashboardDesignProjectHierarchyThirdLine
                    ]),
                newBlock("track-time-and-progress.png", commonResource.DashboardTrackTimeAndProgress,
                    [
                        commonResource.DashboardTrackTimeAndProgressFirstLine,
                        commonResource.DashboardTrackTimeAndProgressSecondLine,
                        commonResource.DashboardTrackTimeAndProgressThirdLine
                    ]),
                newBlock("manage-access-rights.png", commonResource.DashboardManageAccessRights,
                    [
                        commonResource.DashboardManageAccessRightsFirstLine,
                        commonResource.DashboardManageAccessRightsSecondLine,
                        commonResource.DashboardManageAccessRightsThirdLine
                    ]),
                newBlock("use-more-tools.png", commonResource.DashboardUseMoreTools,
                    [
                        commonResource.DashboardUseMoreToolsFirstLine,
                        commonResource.DashboardUseMoreToolsSecondLine,
                        commonResource.DashboardUseMoreToolsThirdLine
                    ])
            ]
        };
        jq.tmpl("projects_dashboard_empty_screen", tmplObj).appendTo("body");
        var $emptyScreenContainer = jq("#projects_dashboard_empty_screen_container");
        $emptyScreenContainer.on("click", ".close", function () {
            $emptyScreenContainer.remove();
        });

        jq(document).keyup(function (event) {
            var code;

            if (event.keyCode) {
                code = event.keyCode;
            } else if (event.which) {
                code = event.which;
            }

            if (code == 27) {
                $emptyScreenContainer.remove();
            }
        });

        $emptyScreenContainer.find(".slick-carousel").slick({
            slidesToShow: 1,
            slidesToScroll: 1,
            arrows: true,
            dots: true,
            fade: true,
            centerMode: true
        });

        $emptyScreenContainer.find(".slick-next").focus();

        emptyScreenShowed = true;
    }

    var removeBlockedUsersFromTeam = function (team) {
        return team.filter(function(item) {
            return item.status === 1;
        });
    };

    var removeComment = function () {
        cPage.onDeleteComment();
    };

    var showTimer = function (projectId, taskId, userId) {
        var width = 290;
        var height = 660;
        var jqbrowser = jq.browser;

        if (jqbrowser.safari) {
            height = 584;
        } else if (jqbrowser.opera) {
            height = 620;
        }

        if (jqbrowser.msie) {
            width = 290;
            height = 660;
        }
        
        if (navigator.userAgent.indexOf("OPR/") > 0) {
            height = 738;
        }

        var params = "width=" + width + ",height=" + height + ",resizable=yes,scrollbars=yes";
        var windowName = "displayTimerWindow";
        var hWnd = null;
        var isExist;

        try {
            hWnd = window.open('', windowName, params);
        } catch (err) {
        }

        try {
            isExist = !hWnd || typeof hWnd.ASC === 'undefined' ? false : true;
        } catch (err) {
            isExist = true;
        }

        var url = "Timer.aspx";

        if (projectId) {
            url += "?prjID=" + projectId;
            if (taskId) {
                url += "&taskId=" + taskId;

                if (userId) {
                    url += "&userID=" + userId;
                }
            }
        }

        if (!isExist) {
            hWnd = window.open(url, windowName, params);
        }

        try {
            if (hWnd)
                hWnd.focus();
        } catch (err) {
        }
    };

    var excludeVisitors = function (users) {
        return users.filter(function(user) {
            return !user.isVisitor;
        });
    };

    var userInProjectTeam = function (userId) {
        if (typeof master.Team === "undefined") return false;
        return master.Team.find(function (item) {
            return item.id === userId;
        });
    };

    var currentUserIsModuleAdmin = function () {
        return teamlab.profile.isAdmin || master.IsModuleAdmin;
    };

    var linkTypeEnum = {
        start_start: 0,
        end_end: 1,
        start_end: 2,
        end_start: 3
    };

    var getPossibleTypeLink = function (firstTaskStart, firstTaskDeadline, secondTaskStart, secondTaskDeadline, relatedTaskObject) {
        var common = baseObject.Common;
        var possibleTypeLinks = [-1, -1, -1, -1];
        possibleTypeLinks[0] = common.linkTypeEnum.start_start; // possible for all tasks

        if (firstTaskDeadline && secondTaskDeadline) {
            possibleTypeLinks[1] = common.linkTypeEnum.end_end; // possible for tasks with deadline

            if (firstTaskDeadline <= secondTaskStart) {
                possibleTypeLinks[3] = common.linkTypeEnum.end_start;
            } else if (secondTaskDeadline <= firstTaskStart) {
                possibleTypeLinks[2] = common.linkTypeEnum.start_end;
            } else {
                relatedTaskObject.invalidLink = true;
                if (firstTaskStart <= secondTaskStart) {
                    possibleTypeLinks[3] = common.linkTypeEnum.end_start;
                } else {
                    possibleTypeLinks[2] = common.linkTypeEnum.start_end;
                }
            }
        } else {
            if (secondTaskDeadline) {
                possibleTypeLinks[2] = common.linkTypeEnum.start_end;
                if (secondTaskDeadline > firstTaskStart) {
                    relatedTaskObject.invalidLink = true;
                }
            } else if (firstTaskDeadline) {
                possibleTypeLinks[3] = common.linkTypeEnum.end_start;
                if (firstTaskDeadline < secondTaskStart) {
                    relatedTaskObject.invalidLink = true;
                }
            }
        }
        return possibleTypeLinks;
    };

    var displayInfoPanel = function (str, warn) {
        if (str === "" || typeof str === "undefined") {
            return;
        }

        if (warn === true) {
            toastr.error(str);
        } else {
            toastr.success(str);
        }
    };

    var getProjectsForFilter = function () {
        if (projectsForFilter.length) return projectsForFilter;
        var currentUserProjects = [];
        var otherProjects = [];

        var projects = master.Projects;
        if (!projects) return [];
        var projectsCount = projects.length;
        for (var i = 0; i < projectsCount; i++) {
            var prj = { 'value': projects[i].id, 'title': projects[i].title, 'canCreateTask': projects[i].canCreateTask, 'canCreateMilestone': projects[i].canCreateMilestone };
            if (projects[i].isInTeam) {
                currentUserProjects.push(prj);
            } else {
                otherProjects.push(prj);
            }
        }
        if (currentUserProjects.length > 0) {
            currentUserProjects[currentUserProjects.length - 1]['classname'] = 'separator';
        }

        projectsForFilter = currentUserProjects.concat(otherProjects);
        return projectsForFilter;
    };

    var defaultSort = function (a, b, asc) {
        if (asc) return (a < b) ? -1 : (a > b) ? 1 : 0;
        return (a < b) ? 1 : (a > b) ? -1 : 0;
    };

    function milestoneSort(a, b) {
        var deadlineSort = defaultSort(a.deadline, b.deadline);
        return deadlineSort ? deadlineSort : defaultSort(a.title, b.title, true);
    };

    function createActionPanel($container, panelId, actionMenuItems) {
        $container.append(jq.tmpl("projects_panelFrame", { panelId: panelId }));
        var $actionPanel = jq('#' + panelId);
        $actionPanel.find(".panel-content").html(jq.tmpl("projects_actionMenuContent", actionMenuItems));
        return $actionPanel;
    }

    function getProjectById(projectId) {
        for (var i = 0, max = master.Projects.length; i < max; i++) {
            var prj = master.Projects[i];
            if (prj.id == projectId) {
                return prj;
            }
        }

        return undefined;
    }

    function getMilestoneById(milestoneId) {
        for (var i = 0, max = master.Milestones.length; i < max; i++) {
            var ms = master.Milestones[i];
            if (ms.id == milestoneId) {
                return ms;
            }
        }

        return undefined;
    }

    function getProjectByIdFromCache(projectId) {
        return projectCache[projectId];
    };

    function changeTaskCountInProjectsCache(task, action) {
        var project = projectCache[task.projectOwner.id];
        if (project) {
            switch(action) {
                case 0://add
                    project.taskCount++;
                    project.taskCountTotal++;
                    break;
                case 1://update
                    if (task.status === 2) {
                        project.taskCount--;
                    } else {
                        project.taskCount++;
                    }
                    break;
                case 2://remove
                    if (task.status === 1) {
                        project.taskCount--;
                        project.taskCountTotal--;
                    }
                    break;
            }

            baseObject.projectNavPanel.rewriteTaskTab();
        }
    }

    function changeMilestoneCountInProjectsCache(milestone, action) {
        var project = projectCache[milestone.projectId];
        if (project) {
            switch (action) {
                case 0://add
                    project.milestoneCount++;
                    break;
                case 1://update
                    if (milestone.status === 1) {
                        project.milestoneCount--;
                    } else {
                        project.milestoneCount++;
                    }
                    break;
                case 2://remove
                    if (milestone.status === 0) {
                        project.milestoneCount--;
                    }
                    break;
            }
            baseObject.projectNavPanel.rewriteMilestoneTab();
        }
    }



    function chooseMonthNumeralCase(count) {
        var resources = baseObject.Resources;
        return count === 0 ? "" : count + " " +
            chooseNumeralCase(count,
                resources.MonthNominative,
                resources.MonthGenitiveSingular,
                resources.MonthGenitivePlural);
    }

    function chooseNumeralCase(number, nominative, genitiveSingular, genitivePlural) {
        if (number === 0.5) {
            if (ASC.Resources.Master.TwoLetterISOLanguageName === "ru") {
                return genitiveSingular;
            }
        }

        if (number === 1) {
            return nominative;
        }

        return chooseNumeralCaseBase(number, nominative, genitiveSingular, genitivePlural);
    }

    function chooseNumeralCaseBase(number, nominative, genitiveSingular, genitivePlural) {
        if (ASC.Resources.Master.TwoLetterISOLanguageName === "ru") {
            var formsTable = [2, 0, 1, 1, 1, 2, 2, 2, 2, 2];

            number = Math.abs(number);
            var res = formsTable[((((number % 100) / 10) !== 1) ? 1 : 0) * (number % 10)];
            switch (res) {
            case 0:
                return nominative;
            case 1:
                return genitiveSingular;
            default:
                return genitivePlural;
            }
        } else {
            return number === 1 ? nominative : genitivePlural;
        }
    }

    function goToWithoutReload(event) {
        if (event && event.originalEvent && event.originalEvent.ctrlKey) {
            return true;
        }

        var $self = jq(this);
        var href = $self.attr("href");
        goToHrefWithoutReload(href);

        return false;
    }

    function goToHrefWithoutReload(href) {
        history.pushState({ href: href }, { href: href }, href);

        var prjid = jq.getURLParam("prjID");
        if (prjid) {
            teamlab.getPrjTeam({}, prjid,
                function(params, team) {
                    master.TeamWithBlockedUsers = team;
                    master.Team = removeBlockedUsersFromTeam(team);
                    baseObject.Base.clearTables();
                    jq("#filterContainer").hide();
                    init();
                });
        } else {
            baseObject.Base.clearTables();
            jq("#filterContainer").hide();
            init();
        }
        return false;
    }

    function initCustomStatuses(callBack) {
        if (master.customStatuses) {
            callBack();
            return;
        }

        teamlab.getPrjStatuses({
            success: function (params, data) {
                master.customStatuses = data.sort(function (a, b) {
                    if (a.statusType < b.statusType) {
                        return -1;
                    } else if (a.statusType > b.statusType) {
                        return 1;
                    }

                    if (a.order < b.order) {
                        return -1;
                    } else if (a.order > b.order) {
                        return 1;
                    }

                    return 0;
                });

                callBack();
            },
            error: function () {

            }
        });
    }

    function setHash(newHash) {
        var basePath = location.hash === "" ? location.href.replace("#", "") : location.href.substring(0, location.href.indexOf("#"));
        if (newHash.indexOf("#") !== 0) {
            newHash = "#" + newHash;
        }
        var newPath = basePath + newHash;
        history.replaceState({ href: newPath }, { href: newPath }, newPath);
    }

    return {
        chooseMonthNumeralCase: chooseMonthNumeralCase,
        createActionPanel: createActionPanel,
        currentUserIsModuleAdmin: currentUserIsModuleAdmin,
        currentUserIsProjectManager: function (projectId) {
            return master.Projects.some(function (item) {
                return item.id == projectId && item.responsibleId === teamlab.profile.id;
            });
        },

        displayInfoPanel: displayInfoPanel,
        
        emptyGuid: emptyGuid,
        events: { loadTags: "loadTags", loadProjects: "loadProjects", loadTeam: "loadTeam", loadMilestones: "loadMilestones" },
        excludeVisitors: excludeVisitors,

        filterParamsForListProjects: { sortBy: "title", sortOrder: "ascending", status: "open", fields: "id,title,security,isPrivate,status,responsible" },
        filterParamsForListMilestones: { sortBy: "deadline", sortOrder: "descending", status: "open", fields: "id,title,deadline" },
        filterParamsForListTasks: { sortBy: "deadline", sortOrder: "ascending" },
        
        goToWithoutReload: goToWithoutReload,
        goToHrefWithoutReload: goToHrefWithoutReload,
        getPossibleTypeLink: getPossibleTypeLink,
        getProjectsForFilter: getProjectsForFilter,
        getProjectById: getProjectById,
        getMilestoneById: getMilestoneById,
        getProjectByIdFromCache: getProjectByIdFromCache,
        changeTaskCountInProjectsCache: changeTaskCountInProjectsCache,
        changeMilestoneCountInProjectsCache: changeMilestoneCountInProjectsCache,

        baseInit: init,
        
        initCustomStatuses: initCustomStatuses,
        linkTypeEnum: linkTypeEnum,
        milestoneSort: milestoneSort,

        removeBlockedUsersFromTeam: removeBlockedUsersFromTeam,
        removeComment: removeComment,

        showTimer: showTimer,

        setHash: setHash,

        userInProjectTeam: userInProjectTeam
    };

})();

ASC.Projects.ReportGenerator = (function() {
    var resources = ASC.Projects.Resources.ProjectsJSResource,
        teamlab,
        progressDialog,
        isInit = false;

    function init() {
        if (isInit) return;
        isInit = true;

        progressDialog = ProgressDialog;
        teamlab = Teamlab;

        progressDialog.init(
            {
                header: resources.ReportBuilding,
                footer: resources.ReportBuildingInfo.format("<a class='link underline' href='/Products/Files/'>", "</a>"),
                progress: resources.ReportBuildingProgress
            },
            jq("#studioPageContent .mainPageContent"),
            {
                terminate: teamlab.terminateProjectsReport,
                status: teamlab.getProjectsReportStatus,
                generate: teamlab.generateProjectsReport
            });
    }

    function generate(uri) {
        init();
        progressDialog.generate({ uri: uri });
    }

    return {
        generate: generate
    }
}());

jq(document).ready(function () {
    ASC.Projects.Common.baseInit();
});