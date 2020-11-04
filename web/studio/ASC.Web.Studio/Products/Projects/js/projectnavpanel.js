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
            "Projects.aspx",
            "overViewModule",
            onClick);
        taskTab = new Tab(resourcesJS.TasksModule,
            function() { return project.taskCount; },
            "Tasks.aspx",
            "tasksModule",
            onClick,
            function() { return true });
        milestoneTab = new Tab(resourcesJS.MilestonesModule,
            function() { return project.milestoneCount; },
            "Milestones.aspx",
            "milestonesModule",
            onClick,
            function() { return true });
        messageTab = new Tab(resourcesJS.DiscussionsModule,
            function() { return project.discussionCount; },
            "Messages.aspx",
            "messagesModule",
            onClick,
            function() { return project.security.canReadMessages });
        timeTrakingTab = new Tab(resourcesJS.TimeTrackingModule,
            function() { return project.timeTrackingTotal; },
            "TimeTracking.aspx",
            "timetrackingModule",
            onClick,
            function() { return !teamlab.profile.isVisitor });
        docsTab = new Tab(resourcesJS.DocumentsModule,
            function() { return project.documentsCount; },
            "TMDocs.aspx",
            "tmdocsModule",
            null,
            function() { return project.security.canReadFiles });
        contactsTab = new Tab(resourcesJS.ContactsModule,
            function() { return project.contactsCount; },
            "Contacts.aspx",
            "contactsModule",
            null,
            function() { return project.security.canReadContacts });
        teamTab = new Tab(resourcesJS.TeamModule,
            function() { return project.participantCount; },
            "ProjectTeam.aspx",
            "projectteamModule",
            onClick);

        ganttChartTab = new Tab(resources.ProjectResource.GanttGart,
            function() { return 0; },
            "GanttChart.aspx",
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
        location.href = "Projects.aspx?prjID=" + project.id + "&action=edit";
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
        ASC.Projects.Common.goToHrefWithoutReload("/Products/Projects/");
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