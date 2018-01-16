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


ASC.Projects.ProjectTeam = (function() {
    var projectId = null,
        managerId = null,
        myGUID = null,
        baseObject,
        master,
        pmProjectTeamModulePermissionOffClass = "pm-projectTeam-modulePermissionOff",
        $teamContainer,
        teamlab,
        loadingBanner,
        clickEventName = "click",
        project,
        handler;

    var init = function () {
        teamlab = Teamlab;
        loadingBanner = LoadingBanner;
        baseObject = ASC.Projects;
        master = baseObject.Master;
        myGUID = teamlab.profile.id;
        projectId = parseInt(jq.getURLParam("prjID"));

        baseObject.ProjectsAdvansedFilter.hide();
        baseObject.Base.clearTables();

        handler = teamlab.bind(teamlab.events.getPrjProject, function (params, prj) {
            managerId = prj.responsibleId;
            var team = baseObject.Master.TeamWithBlockedUsers.sort(function(item1, item2) {
                return item1.isTerminated - item2.isTerminated;
            }).map(mapTeamMember);

            jq(".tab1").html(jq.tmpl("projects_team",
            {
                project: prj,
                team: team,
                manager: prj.responsible
            })).show();
            jq("#descriptionTab").show();
            project = prj;

            // calculate width
            jq(window).resize(calculateWidthBlockUserInfo);


            //--change partisipant security
            $teamContainer = jq("#team_container");

            jq("#CommonListContainer").show();
            ASC.Projects.Base.initActionPanel(showEntityMenu, $teamContainer);

            $teamContainer.on(clickEventName, ".right-checker", function () {
                var cheker = jq(this);
                if (cheker.closest("tr").hasClass("disable") || cheker.hasClass("no-dotted")) {
                    return;
                }
                var data = {
                    userId: jq(this).closest(".pm-projectTeam-participantContainer").attr("data-partisipantId"),
                    security: cheker.attr("data-flag"),
                    visible: cheker.hasClass(pmProjectTeamModulePermissionOffClass)
                };
                teamlab.setTeamSecurity({ partisipant: data.userId, securityFlag: data.security }, projectId, data, { success: onUpdateTeam });
            });

            //--menu actions
            function manageTeam(event, team) {
                var userIDs = new Array();

                jq(team).each(function (i, el) { userIDs.push(el.id); });

                var data = {
                    participants: userIDs,
                    notify: true
                };

                teamlab.updatePrjTeam({ projectId: projectId }, projectId, data,
                    {
                        before: loadingBanner.displayLoading,
                        success: onUpdateTeam,
                        after: loadingBanner.hideLoading
                    });
            };

            calculateWidthBlockUserInfo();

            var teamUserIds = master.TeamWithBlockedUsers.map(function (item) { return item.id; });

            // userselector for the team

            jq("#pm-projectTeam-Selector").useradvancedSelector({
                showGroups: true,
                itemsSelectedIds: teamUserIds,
                itemsDisabledIds: [managerId]
            }).on("showList", manageTeam);

            jq('#PrivateProjectHelp').off("click").on("click", function () {
                jq(this).helper({ BlockHelperID: 'AnswerForPrivateProjectTeam' });
            });
            jq('#RestrictAccessHelp').off("click").on("click", function () {
                jq(this).helper({ BlockHelperID: 'AnswerForRestrictAccessTeam' });
            });
        });
    };

    function showEntityMenu(selectedActionCombobox) {
        var userId = selectedActionCombobox.attr("data-partisipantid"),
            user = master.TeamWithBlockedUsers.find(function (item) { return item.id === userId });
        var ActionMenuItem = ASC.Projects.ActionMenuItem;
        var resources = ASC.Projects.Resources;
        var menuItems = [];

        if (!user.isVisitor){
            if (project.canCreateTask) {
                menuItems.push(new ActionMenuItem("team_task", resources.TasksResource.AddNewTask, teamAddNewTask.bind(null, userId)));
            }
            if (!teamlab.profile.isVisitor) {
                menuItems.push(new ActionMenuItem("team_reportOpen", resources.ReportResource.ReportOpenTasks, teamReportOpenTasksHandler.bind(null, userId)));
                menuItems.push(new ActionMenuItem("team_reportClosed", resources.ReportResource.ReportClosedTasks, teamReportClosedTasksHandler.bind(null, userId)));
            }

            menuItems.push(new ActionMenuItem("team_view", resources.ProjectsJSResource.ViewAllOpenTasks, teamViewOpenTasksHandler.bind(null, userId)));
        }

        if (teamlab.profile.id !== userId) {
            menuItems.push(new ActionMenuItem("team_email", resources.ProjectResource.ClosedProjectTeamWriteMail, teamSendEmailHandler.bind(null, user.email)));
            menuItems.push(new ActionMenuItem("team_jabber", resources.ProjectResource.ClosedProjectTeamWriteInMessenger, teamWriteJabberHandler.bind(null, user.userName)));

            if (project.security.canEditTeam) {
                menuItems.push(new ActionMenuItem("team_remove", resources.CommonResource.RemoveMemberFromTeam, teamRemoveHanlder.bind(null, userId)));
            }
        }

        return { menuItems: menuItems };
    }

    function teamAddNewTask(userId) {
        var user = master.TeamWithBlockedUsers.find(function (item) { return item.id === userId });
        baseObject.TaskAction.showCreateNewTaskForm({ responsibles: [user] });
        return false;
    };

    function teamViewOpenTasksHandler(userId) {
        var url = "tasks.aspx#sortBy=deadline&sortOrder=ascending&tasks_responsible=" + userId;
        window.open(url, "displayOpenUserTasks", "status=yes,toolbar=yes,menubar=yes,scrollbars=yes,resizable=yes,location=yes,directories=yes,menubar=yes,copyhistory=yes");
        return false;
    };

    function teamWriteJabberHandler(userName) {
        ASC.Controls.JabberClient.open(userName);
        return false;
    };

    function teamReportOpenTasksHandler(userId) {
        var url = "generatedreport.aspx?reportType=10&ftime=absolute&fu=" + userId + "&fms=open|closed&fts=open";
        window.open(url, "displayReportWindow", "status=yes,toolbar=yes,menubar=yes,scrollbars=yes,resizable=yes,location=yes,directories=yes,menubar=yes,copyhistory=yes");
        return false;
    };

    function teamReportClosedTasksHandler(userId) {
        var url = "generatedreport.aspx?reportType=10&ftime=absolute&fu=" + userId + "&fms=open|closed&fts=closed";
        window.open(url, "displayReportWindow", "status=yes,toolbar=yes,menubar=yes,scrollbars=yes,resizable=yes,location=yes,directories=yes,menubar=yes,copyhistory=yes");
        return false;
    };

    function teamSendEmailHandler(userEmail) {
        window.location.href = "mailto:" + userEmail;
        return false;
    };

    function teamRemoveHanlder(userId) {
        teamlab.removePrjProjectTeamPerson({ userId: userId }, projectId, { userId: userId }, { success: onRemoveMember });
        return false;
    };

    function onRemoveMember(params, user) {
        jq("tr[data-partisipantid='" + params.userId + "']").remove();

        jq("#pm-projectTeam-Selector").useradvancedSelector("unselect", [params.userId]);

        for (var i = 0, teamLength = master.Team.length; i < teamLength; i++) {
            if (master.Team[i].id == params.userId) {
                master.Team.splice([i], 1);
            }
        }
        baseObject.TaskAction.onUpdateProjectTeam();
    };

    function updateCommonData(team) {
        master.TeamWithBlockedUsers = team;
        master.Team = baseObject.Common.removeBlockedUsersFromTeam(master.TeamWithBlockedUsers);
        baseObject.TaskAction.onUpdateProjectTeam();
    };

    function onUpdateTeam(params, team) {
        displayTeam(team);
        updateCommonData(team);
    };

    function calculateWidthBlockUserInfo(){
        var windowWidth = jq(window).width() - 24 * 2,
            mainBlockWidth = parseInt(jq(".mainPageLayout").css("min-width"), 10),
            newWidth = (windowWidth < mainBlockWidth) ? mainBlockWidth : windowWidth;

        var rightSettingCell = jq(".right-settings");
        if (rightSettingCell.length) {
            newWidth -= rightSettingCell.width();
        }
        $teamContainer.find(".user-info-container").each(
                function() {
                    jq(this).css("max-width", newWidth
                        - 24 * 2 - 24  // padding in blocks
                        - jq(".mainPageTableSidePanel").width()
                        - jq(".menupoint-container").width()
                        - jq(".pm-projectTeam-userPhotoContainer").outerWidth(true)
                        + "px");
                }
        );

        if (jq.browser.msie) {
            $teamContainer.find(".user-info-container").each(
                function() {
                    jq(this).css("width", newWidth + "px");
                }
            );
            $teamContainer.find(".user-info").each(
                function() {
                    jq(this).css("width", newWidth + 50 + "px");
                }
            );
        }
    };

    function displayTeam(team) {
        $teamContainer.html(memberTemplate(team, project));
    };

    function memberTemplate(team, project) {
        return jq.tmpl('memberTemplate',
        {
            team: team.map(mapTeamMember),
            project: project
        });
    }

    function mapTeamMember(item) {
        var resources = ASC.Projects.Resources;
        item.isManager = managerId === item.id;
        return jq.extend({
            security: [
                security(item.canReadMessages, "Messages", resources.MessageResource.Messages),
                security(item.canReadFiles, "Files", resources.ProjectsFileResource.Documents),
                security(item.canReadTasks, "Tasks", resources.TasksResource.AllTasks),
                security(item.canReadMilestones, "Milestone", resources.MilestoneResource.Milestones),
                security(item.canReadContacts, "Contacts", resources.CommonResource.ModuleContacts)
            ]
        }, item);
    }

    function security(check, flag, title) {
        return { check: check, flag: flag, title: title };
    }

    function unbindListEvents() {
        teamlab.unbind(handler);
        $teamContainer.off(clickEventName);
    }

    return {
        init: init,
        unbindListEvents: unbindListEvents,
        mapTeamMember: mapTeamMember
    };
})(jQuery);