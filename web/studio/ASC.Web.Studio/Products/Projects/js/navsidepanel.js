/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


ASC.Projects.navSidePanel = (function () {
    var currentProjectId, isInit;
    var common = ASC.Projects.Common;
    var $createNewButton,
        $createNewTask,
        $createNewMilestone,
        $createNewTimer,
        $createNewDiscussion,
        $menuCreateNewButton,
        $menuTMDocs,
        $menuMessages,
        $menuTasks,
        $menuMilestones,
        $menuTimeTracking,
        $menuTemplates,
        $menuSettings,
        $menuImport,
        $menuProjects,
        $myProjectsConteiner,
        $pageMenu,
        $menuMyProjects,
        $menuFollowedProjects,
        $menuActiveProjects,
        $menuMyMilestones,
        $menuUpcomingMilestones,
        $menuMyTasks,
        $menuUpcomingTasks,
        $menuMyDiscussions,
        $menuLatestDiscussion;

    var ascProjects = ASC.Projects;

    var openClass = "open",
        activeClass = "active",
        disableClass = "disable",
        currentCategoryClass = "currentCategory",
        openCurrentCategoryClass = openClass + " " + currentCategoryClass,
        activeCurrentCategoryClass = activeClass + " " + currentCategoryClass;

    var projectsPage = "projects.aspx",
        messagesPage = "messages.aspx",
        tasksPage = "tasks.aspx",
        milestonesPage = "milestones.aspx",
        timetrackingPage = "timetracking.aspx";
    var profileID;

    function init() {
        currentProjectId = jq.getURLParam('prjID');

        if (!$createNewDiscussion) {
            $createNewDiscussion = jq("#createNewDiscussion");
        }

        if (currentProjectId !== null) {
            $createNewDiscussion.attr("href", "messages.aspx?action=add&prjID=" + currentProjectId);
        } else {
            $createNewDiscussion.attr("href", "messages.aspx?action=add");
        }

        if (isInit) {
            return;
        }

        Teamlab.bind(Teamlab.events.updatePrjProjectStatus,
            function(params, project) {
                showOrHideCreateNew();
            });

        profileID = Teamlab.profile.id;
        isInit = true;

        $menuCreateNewButton = jq("#menuCreateNewButton");
        $createNewButton = jq("#createNewButton");
        $createNewTask = jq("#createNewTask");
        $createNewMilestone = jq("#createNewMilestone");
        $createNewTimer = jq("#createNewTimer");
        $menuTMDocs = jq("#menuTMDocs");
        $menuMessages = jq("#menuMessages");
        $menuTasks = jq("#menuTasks");
        $menuMilestones = jq("#menuMilestones");
        $menuTimeTracking = jq("#menuTimeTracking");
        $menuTemplates = jq("#menuTemplates");
        $menuSettings = jq("#menuSettings");
        $menuImport = jq("#menuImport");
        $menuProjects = jq("#menuProjects");
        $myProjectsConteiner = jq("#myProjectsConteiner");
        $pageMenu = jq(".page-menu");
        $menuMyProjects = $pageMenu.find("#menuMyProjects");
        $menuFollowedProjects = $pageMenu.find("#menuFollowedProjects");
        $menuActiveProjects = $pageMenu.find("#menuActiveProjects");
        $menuMyMilestones = $pageMenu.find("#menuMyMilestones");
        $menuUpcomingMilestones = $pageMenu.find("#menuUpcomingMilestones");
        $menuMyTasks = $pageMenu.find("#menuMyTasks");
        $menuUpcomingTasks = $pageMenu.find("#menuUpcomingTasks");
        $menuMyDiscussions = $pageMenu.find("#menuMyDiscussions");
        $menuLatestDiscussion = $pageMenu.find("#menuLatestDiscussion");

        highlightMenu();
        initNavMenuItems();
        showOrHideCreateNew();

        $menuCreateNewButton.on("click", function (e) {
            if ($menuCreateNewButton.hasClass(disableClass)) return false;
            return true;
        });

        $createNewTask.click(function () {
            var anchor = ASC.Controls.AnchorController.getAnchor();
            var author = jq.getAnchorParam('responsible_for_milestone', anchor) ||
                jq.getAnchorParam('tasks_responsible', anchor) ||
                jq.getAnchorParam('project_manager', anchor) ||
                jq.getAnchorParam('team_member', anchor) ||
                jq.getAnchorParam('author', anchor);
            var taskParams = {
                projectId: currentProjectId || jq.getAnchorParam('project', anchor),
                milestoneId: jq.getAnchorParam('milestone', anchor)
            };
            if (author) {
                taskParams.responsibles = [{ id: author }];
            }
            ASC.Projects.TaskAction.showCreateNewTaskForm(taskParams);
            $createNewButton.hide();
            return false;
        });

        $createNewMilestone.click(function () {
            ASC.Projects.MilestoneAction.showNewMilestonePopup();
            $createNewButton.hide();
            return false;
        });

        $createNewTimer.click(function () {
            var currentCategory = jq(".menu-list").find(".menu-item." + currentCategoryClass).attr("id");
            var taskId = jq.getURLParam("ID");

            if (currentProjectId) {
                if ((currentCategory === "menuTasks" || document.location.href.indexOf(timetrackingPage) > 0) && taskId) {
                    common.showTimer(currentProjectId, taskId);
                } else {
                    common.showTimer(currentProjectId);
                }
            } else {
                common.showTimer();
            }
            $createNewButton.hide();
            return false;
        });

        $myProjectsConteiner.find(".expander").click(function (event) {
            var menuItem = jq(this).closest(".menu-sub-item");
            if (jq(menuItem).hasClass(openClass)) {
                jq(menuItem).removeClass(openClass);
            } else {
                jq(menuItem).addClass(openClass);
            }
            event.stopPropagation();
        });
    };

    function highlightMenu() {
        jq(".menu-list li.filter, li.menu-item, div.menu-item, li.menu-sub-item").removeClass(activeCurrentCategoryClass);
        var pathnamearr = location.pathname.split('/');
        var currentPage = pathnamearr[pathnamearr.length - 1].toLowerCase();
        currentProjectId = jq.getURLParam("prjID");

        if (currentPage === "tmdocs.aspx") {
            $menuTMDocs.removeClass("none-sub-list");
            $menuTMDocs.addClass("sub-list open");
            if (!currentProjectId) {
                $menuTMDocs.find("a").attr("href", "#");
                $menuTMDocs.addClass(activeCurrentCategoryClass);
            } else {
                $menuTMDocs.find("a").attr("href", "tmdocs.aspx");
            }
        }

        if (currentProjectId) {
            currentPage = projectsPage;
        }
        if (currentPage === messagesPage) {
            $menuMessages.addClass(activeCurrentCategoryClass);
        }

        if (currentPage === projectsPage) {
            if (!$myProjectsConteiner.find("li[id=" + currentProjectId + "]").length) {
                $menuProjects.addClass(activeClass);
            } else {
                $menuProjects.addClass(openClass);
                $myProjectsConteiner.addClass(openCurrentCategoryClass);
                $myProjectsConteiner.find("li[id=" + currentProjectId + "]").addClass(activeCurrentCategoryClass);
            }

            $menuProjects.addClass(currentCategoryClass);
        }

        if (currentPage === milestonesPage) {
            $menuMilestones.addClass(activeCurrentCategoryClass);
        }

        if (currentPage === tasksPage) {
            $menuTasks.addClass(activeCurrentCategoryClass);
        }

        if (currentPage === timetrackingPage) {
            $menuTimeTracking.addClass(activeCurrentCategoryClass);
        }

        if (currentPage === "reports.aspx") {
            jq("#menuReports").addClass(activeClass);
        }

        if (currentPage === "projecttemplates.aspx") {
            $menuTemplates.addClass(activeClass);
        }

        if (currentPage === "settings.aspx") {
            if (!$menuSettings.hasClass("open")) {
                $menuSettings.find(".expander").click();
            }
            $menuSettings.find(" .menu-sub-item:first").addClass(activeClass);
        }



        var currentCategory = jq(".menu-list").find(".menu-item." + activeClass).attr("id");
        var hash = document.location.hash;
        var flag = false;

        var pageProjectsFlag = location.href.indexOf(projectsPage);
        var pageActionFlag = jq.getURLParam('action');

        if ((!currentProjectId && !pageActionFlag) && pageProjectsFlag > 0) {
            if (hash.indexOf("team_member=" + profileID) > 0) {
                if ($myProjectsConteiner.length) {
                    $menuMyProjects.closest("div").addClass(activeClass);
                } else {
                    $menuMyProjects.closest("li").addClass(activeClass);
                }

                flag = true;
            } else if (hash.indexOf("followed=true") > 0) {
                $menuFollowedProjects.parent("li").addClass(activeClass);
                flag = true;
            } else {
                if (hash.indexOf("status=open") > 0) {
                    $menuActiveProjects.parent("li").addClass(activeClass);
                    $menuProjects.addClass(openClass);
                    flag = true;
                }
            }
            if (flag) {
                $menuProjects.removeClass(activeClass);
            }
        }

        if (currentCategory === "menuMilestones") {
            flag = false;
            if (hash.indexOf("user_tasks=" + profileID) > 0 && hash.indexOf("status=open") > 0) {
                $menuMyMilestones.parent("li").addClass(activeClass);
                flag = true;
            } else if (hash.indexOf("user_tasks=" + profileID) > 0 && hash.indexOf("&deadlineStart=") > 0) {
                $menuUpcomingMilestones.parent("li").addClass(activeClass);
                flag = true;
            }
            if (flag) {
                $menuMilestones.removeClass(activeClass);
            }
        }

        if (currentCategory === "menuTasks" && $menuTasks.hasClass(openClass)) {
            flag = false;
            if (hash.indexOf("tasks_responsible=" + profileID) > 0 && hash.indexOf("status=open") > 0) {
                $menuMyTasks.parent("li").addClass(activeClass);
                flag = true;
            } else if (hash.indexOf("tasks_responsible=" + profileID) > 0 && hash.indexOf("&deadlineStart=") > 0) {
                $menuUpcomingTasks.parent("li").addClass(activeClass);
                flag = true;
            }
            if (flag) {
                $menuTasks.removeClass(activeClass);
            }
        }

        if (currentCategory == "menuMessages" && $menuMessages.hasClass(openClass)) {
            flag = false;
            if (hash.indexOf("author=" + profileID) > 0) {
                $menuMyDiscussions.parent("li").addClass(activeClass);
                flag = true;
            } else if (hash.indexOf("createdStart=") > 0) {
                $menuLatestDiscussion.parent("li").addClass(activeClass);
                flag = true;
            }
            if (flag) {
                $menuMessages.removeClass(activeClass);
            }
        }
    };

    function initNavMenuItems() {
        var currentDate = new Date();
        var date = new Date(currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate());
        var deadlineStart = date.getTime();

        date.setDate(date.getDate() + 7);
        var deadlineStop = date.getTime();

        date = new Date(currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate());
        var createdStop = date.getTime();

        date.setDate(date.getDate() - 7);
        var createdStart = date.getTime();

        var categoryWrapper = ".category-wrapper a";

        var menuitems = [
            createProjectItem($menuProjects.find(categoryWrapper)),
            createProjectItem($menuMyProjects, "team_member=" + profileID + "&status=open"),
            createProjectItem($menuFollowedProjects, "status=open&followed=true"),
            createProjectItem($menuActiveProjects, "status=open"),

            createMilestoneItem($menuMilestones.find(categoryWrapper)),
            createMilestoneItem($menuMyMilestones, "user_tasks=" + profileID + "&status=open"),
            createMilestoneItem($menuUpcomingMilestones, "user_tasks=" + profileID + "&deadlineStart=" + deadlineStart + "&deadlineStop=" + deadlineStop),

            createTaskItem($menuTasks.find(categoryWrapper)),
            createTaskItem($menuMyTasks, "tasks_responsible=" + profileID + "&status=open"),
            createTaskItem($menuUpcomingTasks, "tasks_responsible=" + profileID + "&deadlineStart=" + deadlineStart + "&deadlineStop=" + deadlineStop),

            createDiscussionItem($menuMessages.find(categoryWrapper)),
            createDiscussionItem($menuLatestDiscussion, "createdStart=" + createdStart + "&createdStop=" + createdStop),
            createDiscussionItem($menuMyDiscussions, "author=" + profileID),

            createItem($menuTimeTracking.find('a'), timetrackingPage)
        ];

        if (jq.getURLParam("id") == null || (location.href.contains("messages.aspx") && !location.href.contains("action") || location.href.contains("tasks.aspx"))) {
            initMenuItems(menuitems);

            var myprojects = $myProjectsConteiner.find("li a");
            var myProjectsOnClick = function () {
                return function () {
                    if (!checkInit()) return true;
                    ASC.Projects.Common.goToWithoutReload.call(this);
                    highlightMenu();
                    return false;
                }
            }
            for (var i = 0, j = myprojects.length; i < j; i++) {
                jq(myprojects[i]).on("click", myProjectsOnClick(myprojects[i]));
            };
        }
    };

    function createProjectItem(id, hash) {
        return createItem(id, projectsPage, ascProjects.AllProject.basePath, hash);
    }

    function createMilestoneItem(id, hash) {
        return createItem(id, milestonesPage, ascProjects.AllMilestones.basePath, hash);
    }

    function createTaskItem(id, hash) {
        return createItem(id, tasksPage, ascProjects.TasksManager.basePath, hash);
    }

    function createDiscussionItem(id, hash) {
        return createItem(id, messagesPage, ascProjects.Discussions.basePath, hash);
    }

    function createItem(id, basePage, basePath, hash) {
        var href = basePage;
        if (hash) {
            href += "#" + basePath + "&" + hash;
        }

        return { id: id, href: href, onclick: highlightMenu };
    }

    function initMenuItems(items) {
        items.forEach(function (item) {
            item.id.attr('href', item.href);
            item.id.on("click", function(event) {
                    return onClick.call(this, item, event);
                });
        });
    };

    function showOrHideCreateNew() {
        var projects = ASC.Projects.Master.Projects;
        var $createNewProject = jq("#createNewProject"),
            $createProjectTempl = jq("#createProjectTempl");

        var canCreateMilestone = false, canCreateTask = false, canCreateMessage = false, canCreateTimeSpend = false;
        var canCreateProject = ASC.Projects.Master.CanCreateProject;

        for (var i = 0, j = projects.length; i < j; i++) {
            var item = projects[i];
            if (item.status !== 0) continue;

            if (item.canCreateMilestone) canCreateMilestone = true;
            if (item.canCreateTask) canCreateTask = true;
            if (item.canCreateMessage) canCreateMessage = true;
            if (item.canCreateTimeSpend && item.taskCountTotal > 0) canCreateTimeSpend = true;
        }

        if (canCreateMilestone) {
            $createNewMilestone.show();
        } else {
            $createNewMilestone.hide();
        }

        if (canCreateTask) {
            $createNewTask.show();
        } else {
            $createNewTask.hide();
        }

        if (canCreateMessage) {
            $createNewDiscussion.show();
        } else {
            $createNewDiscussion.hide();
        }

        if (canCreateTimeSpend) {
            $createNewTimer.show();
        } else {
            $createNewTimer.hide();
        }

        if (canCreateMilestone || canCreateTask || canCreateMessage || canCreateTimeSpend || canCreateProject) {
            $menuCreateNewButton.removeClass(disableClass);
        } else {
            if ($createNewButton.find(".dropdown-item-seporator").length === 0) {
                $menuCreateNewButton.addClass(disableClass);
            } else {
                $createNewButton.find(".dropdown-item-seporator:first").hide();
            }
        }

        if (Teamlab.profile.isVisitor && window.location.href.indexOf("tmdocs.aspx") > -1) {
            $menuCreateNewButton.addClass(disableClass);
        }

        if (!canCreateProject) {
            $createNewProject.hide();
            $createProjectTempl.hide();
        }
    }

    function checkInit() {
        var currentPage = document.location.pathname.match(/[^\/]+$/)[0];
        return !!(window.history && window.history.replaceState) && !jq.getURLParam('action') &&
            (currentPage === projectsPage ||
            currentPage === messagesPage ||
            currentPage === tasksPage ||
            currentPage === milestonesPage ||
            currentPage === timetrackingPage ||
            currentPage === "projectteam.aspx");
    };
    
    function onClick(item, event) {
        if (!checkInit()) return true;
        if (event.which === 1 && event.ctrlKey || event.which === 2) return true;
        if (location.href.endsWith(item.href)) return false;
        event.stopPropagation();
        history.pushState({ href: item.href }, { href: item.href }, item.href);
        ASC.Controls.AnchorController.historyCheck();
        if (!jq(this).attr("href").contains("#")) {
            location.hash = "";
        }
        common.baseInit();
        if (item.hasOwnProperty('onclick')) {
            item.onclick();
        }
        return false;
    }

    return {
        init: init,
        onClick: onClick
    };
})(jQuery);