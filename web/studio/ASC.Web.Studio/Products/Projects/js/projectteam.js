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
        handler,
        mailModuleEnabled;

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

            var isRetina = jq.cookies.get("is_retina");
            jq(".tab1").html(jq.tmpl("projects_team",
            {
                project: prj,
                team: team,
                manager: prj.responsible,
                isRetina: isRetina
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

            var teamUserIds = master.TeamWithBlockedUsers
                .map(function (item) {
                    return item.id;
                })
                .filter(function (item) {
                    return item !== managerId;
                });

            if (teamUserIds.indexOf(managerId) < 0) {
                teamUserIds.push(managerId);
            }

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

            if (isRetina) {
                teamlab.getUserPhoto({}, prj.responsible.id,
                    {
                        success: function(params, data) {
                            if (data && typeof(data.max) === "string" && data.max.indexOf("default") === -1) {
                                jq(".managerAvatar").attr("src", data.max);
                            }
                        }
                    });
            }
        });

        teamlab.getWebItemSecurityInfo({}, "2A923037-8B2D-487b-9A22-5AC0918ACF3F",
        {
            success: function(params, data) {
                mailModuleEnabled = data[0].enabled;
            }
        });
    };

    function showEntityMenu(selectedActionCombobox) {
        var userId = selectedActionCombobox.attr("data-partisipantid"),
            user = master.TeamWithBlockedUsers.find(function (item) { return item.id === userId });
        var ActionMenuItem = ASC.Projects.ActionMenuItem;
        var resources = ASC.Projects.Resources;
        var menuItems = [];

        if (!user.isVisitor) {
            if (project.canCreateTask && !user.isTerminated) {
                menuItems.push(new ActionMenuItem("team_task", resources.TasksResource.AddNewTask, teamAddNewTask.bind(null, userId), "new-task"));
            }
            if (!teamlab.profile.isVisitor) {
                menuItems.push(new ActionMenuItem("team_reportOpen", resources.ReportResource.ReportOpenTasks, teamReportOpenTasksHandler.bind(null, userId), "open-tasks-report"));
                menuItems.push(new ActionMenuItem("team_reportClosed", resources.ReportResource.ReportClosedTasks, teamReportClosedTasksHandler.bind(null, userId), "closed-tasks-report"));
            }

            if (menuItems.length) {
                menuItems.push(new ActionMenuItem(null, null, null, null, true));
            }

            menuItems.push(new ActionMenuItem("team_view", resources.ProjectsJSResource.ViewAllOpenTasks, teamViewOpenTasksHandler.bind(null, userId), "preview"));
        }

        if (teamlab.profile.id !== userId) {
            if (user.email) {
                menuItems.push(new ActionMenuItem("team_email", resources.ProjectResource.ClosedProjectTeamWriteMail, teamSendEmailHandler.bind(null, user.email), "email"));
            }

            menuItems.push(new ActionMenuItem("team_jabber", resources.ProjectResource.ClosedProjectTeamWriteInMessenger, teamWriteJabberHandler.bind(null, user.userName), "chat"));

            if (project.security.canEditTeam && userId !== project.responsibleId) {
                if (menuItems.length >= 3) {
                    menuItems.push(new ActionMenuItem(null, null, null, null, true));
                }

                menuItems.push(new ActionMenuItem("team_remove", resources.CommonResource.RemoveMemberFromTeam, teamRemoveHanlder.bind(null, userId), "user"));
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
        var url = "Tasks.aspx#sortBy=deadline&sortOrder=ascending&tasks_responsible=" + userId;
        window.open(url, "displayOpenUserTasks", "status=yes,toolbar=yes,menubar=yes,scrollbars=yes,resizable=yes,location=yes,directories=yes,menubar=yes,copyhistory=yes");
        return false;
    };

    function teamWriteJabberHandler(userName) {
        ASC.Controls.JabberClient.open(userName);
        return false;
    };

    function teamReportOpenTasksHandler(userId) {
        ASC.Projects.ReportGenerator.generate("generatedreport.aspx?reportType=10&ftime=absolute&fu=" + userId + "&fms=open|closed&fts=open");
    };

    function teamReportClosedTasksHandler(userId) {
        ASC.Projects.ReportGenerator.generate("generatedreport.aspx?reportType=10&ftime=absolute&fu=" + userId + "&fms=open|closed&fts=closed");
    };

    function teamSendEmailHandler(userEmail) {
        if (mailModuleEnabled && !teamlab.profile.isVisitor) {
            window.open('../../addons/mail/#composeto/email=' + userEmail, "_blank");
        } else {
            window.location.href = "mailto:" + userEmail;
        }
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
                break;
            }
        }

        for (var i = 0, teamLength = master.TeamWithBlockedUsers.length; i < teamLength; i++) {
            if (master.TeamWithBlockedUsers[i].id == params.userId) {
                master.TeamWithBlockedUsers.splice([i], 1);
                break;
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
        team = teamlab.create('prj-projectpersons', null, team);
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
            project: project,
            isRetina: jq.cookies.get("is_retina")
        });
    }

    function mapTeamMember(item) {
        var resources = ASC.Projects.Resources;
        item = jq.extend({}, item, window.UserManager.getUser(item.id));
        item.isManager = managerId === item.id;
        item.isProjectAdmin = item.groups && item.groups.indexOf(master.ProjectsProductID) > -1;
        return jq.extend({
            security: [
                security(item.canReadMessages, "Messages", resources.MessageResource.Messages),
                security(item.canReadFiles, "Files", resources.ProjectsFileResource.Documents),
                security(item.canReadTasks, "Tasks", resources.TasksResource.AllTasks),
                security(item.canReadMilestones, "Milestone", resources.MilestoneResource.Milestones),
                security(item.canReadContacts, "Contacts", resources.CommonResource.ModuleContacts, item.isVisitor)
            ]
        }, item);
    }

    function security(check, flag, title, defaultDisabled) {
        var result = { check: check, flag: flag, title: title };
        if (typeof defaultDisabled === "boolean") {
            result.defaultDisabled = defaultDisabled;
        }
        return result;
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