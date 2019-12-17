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


ASC.Projects.projectNavPanel = (function() {
    var
        overViewTab,
        taskTab,
        milestoneTab,
        messageTab,
        timeTrakingTab,
        docsTab,
        teamTab,
        ganttChartTab,
        contactsTab;

    var currentProjectId,
        isInit,
        teamlab,
        loadingBanner,
        resources,
        commonResource,
        project,
        handlers = [],
        projectInfoContainerClass = ".project-info-container",
        $projectInfoContainer;

    var init = function () {
        if (currentProjectId === jq.getURLParam("PrjID")) return;

        teamlab = Teamlab;
        loadingBanner = LoadingBanner;
        resources = ASC.Projects.Resources;
        commonResource = resources.CommonResource;
        currentProjectId = jq.getURLParam("PrjID");

        if (!isInit) {
            initTabs();
        }

        isInit = true;
    };

    function initNavigationPanel(params, response) {
        if (!(Number(jq.getURLParam("id")) === 0 && jq.getURLParam("action") == null)) return;
        project = response;

        var resourcesJS = resources.ProjectsJSResource,
            Tab = ASC.Projects.ProjectTab.bind(null, project),
            onClick = ASC.Projects.navSidePanel.onClick;

        overViewTab = new Tab(resourcesJS.OverviewModule,
            function() { return 0; },
            "projects.aspx",
            "overViewModule",
            onClick);
        taskTab = new Tab(resourcesJS.TasksModule,
            function() { return project.taskCount; },
            "tasks.aspx",
            "tasksModule",
            onClick,
            function() { return true });
        milestoneTab = new Tab(resourcesJS.MilestonesModule,
            function() { return project.milestoneCount; },
            "milestones.aspx",
            "milestonesModule",
            onClick,
            function() { return true });
        messageTab = new Tab(resourcesJS.DiscussionsModule,
            function() { return project.discussionCount; },
            "messages.aspx",
            "messagesModule",
            onClick,
            function() { return project.security.canReadMessages });
        timeTrakingTab = new Tab(resourcesJS.TimeTrackingModule,
            function() { return project.timeTrackingTotal; },
            "timetracking.aspx",
            "timetrackingModule",
            onClick,
            function() { return !teamlab.profile.isVisitor });
        docsTab = new Tab(resourcesJS.DocumentsModule,
            function() { return project.documentsCount; },
            "tmdocs.aspx",
            "tmdocsModule",
            null,
            function() { return project.security.canReadFiles });
        contactsTab = new Tab(resourcesJS.ContactsModule,
            function() { return project.contactsCount; },
            "contacts.aspx",
            "contactsModule",
            null,
            function() { return project.security.canReadContacts });
        teamTab = new Tab(resourcesJS.TeamModule,
            function() { return project.participantCount; },
            "projectteam.aspx",
            "projectteamModule",
            onClick);

        ganttChartTab = new Tab(resources.ProjectResource.GanttGart,
            function() { return 0; },
            "ganttchart.aspx",
            "ganttchartModule",
            null,
            function() { return !jq.browser.mobile && project.status === 0 && project.security.canReadTasks && project.security.canReadMilestones; });

        var data = {
            icon: "projects",
            title: project.title,
            private: project.isPrivate
        };

        ASC.Projects.InfoContainer.init(data, showEntityMenu, [overViewTab, taskTab, milestoneTab, messageTab, timeTrakingTab, docsTab, contactsTab, teamTab, ganttChartTab]);

        $projectInfoContainer = jq(projectInfoContainerClass);

        jq("#projectTabs").removeClass("display-none");
        jq("#CommonListContainer").show();
    }


    function showEntityMenu() {
        var menuItems = [];

        if (project.canEdit) {
            menuItems.push(new ASC.Projects.ActionMenuItem("pa_edit", commonResource.Edit, paEditHandler));
        }

        if (project.canDelete) {
            menuItems.push(new ASC.Projects.ActionMenuItem("pa_delete", commonResource.Delete, paDeleteHandler));
        }

        var isInTeam = ASC.Projects.Master.Team.some(function (item) {
            return item.id === Teamlab.profile.id;
        });

        if (!Teamlab.profile.isOutsider && !isInTeam) {
            menuItems.push(new ASC.Projects.ActionMenuItem("pa_follow", project.isFollow ? commonResource.Unfollow : commonResource.Follow, paSubscribeHandler));
        }

        return { menuItems: menuItems };
    }

    function paEditHandler() {
        location.href = "projects.aspx?prjID=" + project.id + "&action=edit";
    }

    function paDeleteHandler() {
        ASC.Projects.Base.showCommonPopup("projectRemoveWarning",
            function() {
                teamlab.removePrjProject(project.id, { success: onDeleteProject, error: onDeleteProjectError });
            });
    };

    function paSubscribeHandler() {
        teamlab.subscribeProject({ followed: !project.isFollow }, currentProjectId, { success: onSubscribeProject });
    }

    function initTabs() {
        var events = teamlab.events;
        unbind();
        bind(events.getPrjProject, initNavigationPanel);
        bind(events.removePrjTask, function (params, task) {
            if (task.status === 2 || task.projectId != currentProjectId) return;
            if (jq.getURLParam("id") !== null) return;

            teamlab.getPrjTimeById({},
                project.id,
                {
                    success: function(params, data) {
                        project.timeTrackingTotal = data;
                        timeTrakingTab.rewrite();
                    }
                });
        });

        bind(events.updatePrjTeam, function (params, team) {
            project.participantCount = team.length;
            teamTab.rewrite();
        });
        bind(events.removePrjTeam, function () {
            project.participantCount--;
            teamTab.rewrite();
        });

        bind(events.getCrmContactsForProject, function (params, response) {
            project.contactsCount = response.length;
            contactsTab.rewrite();
        });

        bind(events.addCrmContactForProject, function (params, response) {
            project.contactsCount++;
            contactsTab.rewrite();
        });

        bind(events.removeCrmContactFromProject, function () {
            project.contactsCount--;
            contactsTab.rewrite();
        });

        bind(events.removePrjTime, function (params, data) {
            if (!project) return;
            var currentTime = parseTime(project.timeTrackingTotal);

            for (var i = data.length; i--;) {
                var time = parseTime(data[i].hours);

                currentTime.hours -= time.hours;
                currentTime.minutes -= time.minutes;
            }

            project.timeTrackingTotal = timeToString(currentTime);

            timeTrakingTab.rewrite();
        });

        bind(events.updatePrjTime, function (params, data) {
            var oldTime = params.oldTime,
                newTime = parseTime(data.hours),
                currentTime = parseTime(project.timeTrackingTotal);

            currentTime.hours -= oldTime.hours;
            currentTime.hours += newTime.hours;

            currentTime.minutes -= oldTime.minutes;
            currentTime.minutes += newTime.minutes;

            project.timeTrackingTotal = timeToString(currentTime);

            timeTrakingTab.rewrite();
        });

        function parseTime(data) {
            var text = jq.trim(data);
            if (typeof data === "number") {
                text = jq.timeFormat(text);
            }
            if (text === "") {
                text = "00:00";
            }
            text = text.split(":");
            return { hours: parseInt(text[0], 10), minutes: parseInt(text[1], 10) };
        }

        function timeToString(data) {
            if (data.minutes > 59) {
                data.hours += 1;
                data.minutes -= 60;
            }
            if (data.minutes < 0) {
                data.hours -= 1;
                data.minutes += 60;
            }

            if (data.hours === 0 && data.minutes === 0) return "";

            return jq.format("{0}:{1}{2}", data.hours, data.minutes < 10 ? "0" : "", data.minutes);
        }
    };

    function bind(event, handler) {
        handlers.push(teamlab.bind(event, handler));
    }

    function unbind() {
        while (handlers.length) {
            var handler = handlers.shift();
            teamlab.unbind(handler);
        }
    }

    function onSubscribeProject(params) {
        project.isFollow = params.followed;
    };


    function onDeleteProject() {
        jq.unblockUI();
        ASC.Projects.Common.goToHrefWithoutReload("/products/projects/");
    };

    function onDeleteProjectError() {
        jq.unblockUI();
    };

    function hide() {
        currentProjectId = undefined;
        if (typeof $projectInfoContainer !== "undefined") {
            $projectInfoContainer.hide();
        }
    }

    function rewriteTaskTab() {
        taskTab.rewrite();
    }

    function rewriteMilestoneTab() {
        milestoneTab.rewrite();
    }

    return {
        init: init,
        hide: hide,
        rewriteTaskTab: rewriteTaskTab,
        rewriteMilestoneTab: rewriteMilestoneTab
    };
})(jQuery);