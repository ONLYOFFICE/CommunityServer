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
    var projectId = null;
    var managerId = null;
    var myGUID = null;
    var canCreateTask = false;


    var init = function() {
        myGUID = Teamlab.profile.id;
        projectId = parseInt(jq.getURLParam("prjID"));
        managerId = jq(".pm-projectTeam-projectLeaderCard").find(".manager-info").attr("data-manager-guid");

        // calculate width
        jq(window).resize(function() {
            calculateWidthBlockUserInfo();
        });


        //--change partisipant security
        $teamContainer = jq("#team_container");
        $actionPanel = jq("#userActionPanel");

        jq.dropdownToggle({
            dropdownID: "userActionPanel",
            switcherSelector: "#team_container .entity-menu",
            addTop: 0,
            addLeft: 10,
            rightPos: true,
            showFunction: function (switcherObj, dropdownItem) {
                var $container = jq(switcherObj).closest('tr'),
                    $openItem = jq("#team_container tr.open"),
                    userId = $container.attr("data-partisipantid"),
                    userName = jq.trim($container.find(".user-name").text()),
                    email = $container.attr("data-email"),
                    jabber = $container.attr("data-user");

                if (jq(switcherObj).attr("data-status") == "2") {
                    jq("#addNewTask, #writeJabber, #sendEmail").hide();
                } else {
                    if (Teamlab.profile.id == userId) {
                        jq("#writeJabber, #sendEmail").hide();
                    } else {
                        jq("#writeJabber, #sendEmail").show();
                    }
                    if (jq(switcherObj).attr("data-isVisitor") == "true") {
                        jq("#addNewTask, #reportOpenTasks, #reportClosedTasks, #viewOpenTasks").hide();
                    } else {
                        jq("#addNewTask, #reportOpenTasks, #reportClosedTasks, #viewOpenTasks").show();
                    }
                }

                dropdownItem.attr("data-userid", userId);
                dropdownItem.attr("data-username", userName);
                dropdownItem.attr("data-email", email);
                dropdownItem.find("#writeJabber").attr("data-username", jabber);
                if (managerId == userId) {
                    dropdownItem.find("#removeFromTeam").hide();
                } else {
                    dropdownItem.find("#removeFromTeam").show();
                }

                $openItem.removeClass("open");
                if (!$openItem.is($container)) {
                    dropdownItem.hide();
                }
                if (dropdownItem.is(":hidden")) {
                    $container.addClass("open");
                }
            },
            hideFunction: function () {
                jq("#team_container tr.open").removeClass("open");
            }
        });


        $teamContainer.on("click", ".right-checker", function() {
            var cheker = jq(this);
            if (cheker.closest("tr").hasClass("disable") || cheker.hasClass("no-dotted")) {
                return;
            }
            var data = {};
            data.userId = jq(this).closest(".pm-projectTeam-participantContainer").attr("data-partisipantId");
            data.security = cheker.attr("data-flag");
            if (cheker.hasClass("pm-projectTeam-modulePermissionOff")) {
                data.visible = true;
            } else {
                data.visible = false;
            }
            Teamlab.setTeamSecurity({ partisipant: data.userId, securityFlag: data.security }, projectId, data, { success: onSetTeamSecurity });
        });

        //--menu actions
        $actionPanel.on("click", "#addNewTask", function () {
            $actionPanel.hide();
            var actionPanel = jq(this).closest("#userActionPanel");
            var userId = jq(actionPanel).attr("data-userid");
            var userName = jq(actionPanel).attr("data-username");
            showAddNewTaskPopup(userId, userName);
            return false;
        });
        $actionPanel.on("click", "#viewOpenTasks", function () {
            $actionPanel.hide();
            var actionPanel = jq(this).closest("#userActionPanel");
            var userId = jq(actionPanel).attr("data-userid");
            var url = "tasks.aspx#sortBy=deadline&sortOrder=ascending&tasks_responsible=" + userId;
            window.open(url, "displayOpenUserTasks", "status=yes,toolbar=yes,menubar=yes,scrollbars=yes,resizable=yes,location=yes,directories=yes,menubar=yes,copyhistory=yes");
            return false;
        });
        $actionPanel.on("click", "#writeJabber", function() {
            $actionPanel.hide();
            jq("#team_container tr.open").removeClass("open");
            var userName = jq(this).attr("data-username");
            ASC.Controls.JabberClient.open(userName);
            return false;
        });
        $actionPanel.on("click", "#reportOpenTasks", function() {
            $actionPanel.hide();
            jq("#team_container tr.open").removeClass("open");
            var actionPanel = jq(this).closest("#userActionPanel");
            var userId = jq(actionPanel).attr("data-userid");
            var url = "generatedreport.aspx?reportType=10&ftime=absolute&fu=" + userId + "&fms=open|closed&fts=open";
            jq("#userActionPanel").hide();
            window.open(url, "displayReportWindow", "status=yes,toolbar=yes,menubar=yes,scrollbars=yes,resizable=yes,location=yes,directories=yes,menubar=yes,copyhistory=yes");
            return false;
        });

        $actionPanel.on("click", "#reportClosedTasks", function() {
            $actionPanel.hide();
            jq("#team_container tr.open").removeClass("open");
            var actionPanel = jq(this).closest("#userActionPanel");
            var userId = jq(actionPanel).attr("data-userid");
            var url = "generatedreport.aspx?reportType=10&ftime=absolute&fu=" + userId + "&fms=open|closed&fts=closed";
            jq("#userActionPanel").hide();
            window.open(url, "displayReportWindow", "status=yes,toolbar=yes,menubar=yes,scrollbars=yes,resizable=yes,location=yes,directories=yes,menubar=yes,copyhistory=yes");
            return false;
        });

        $actionPanel.on("click", "#sendEmail", function() {
            $actionPanel.hide();
            jq("#team_container tr.open").removeClass("open");
            var actionPanel = jq(this).closest("#userActionPanel");
            var userEmail = jq(actionPanel).attr("data-email");
            window.location.href = "mailto:" + userEmail;
            return false;
        });

        $actionPanel.on("click", "#removeFromTeam", function() {
            $actionPanel.hide();
            jq("#team_container tr.open").removeClass("open");
            var actionPanel = jq(this).closest("#userActionPanel");
            var userId = jq(actionPanel).attr("data-userid");
            Teamlab.removePrjProjectTeamPerson({ userId: userId }, projectId, { userId: userId }, { success: onRemoveMember });
            return false;
        });

        function manageTeam(event, team) {
            var userIDs = new Array();

            jq(team).each(function (i, el) { userIDs.push(el.id); });

            var projId = parseInt(jq.getURLParam("prjID"));
            var data = {};
            data.participants = userIDs;
            data.notify = true;

            Teamlab.updatePrjTeam({}, projId, data, { before: function () { LoadingBanner.displayLoading(); }, success: onUpdateTeam, after: function () { LoadingBanner.hideLoading(); } });
        };


        jq("body").click(function(event) {
            var elt = event.target;
            if (!jq(elt).is("#userActionPanel") && !jq(elt).is(".entity-menu")) {
                $actionPanel.hide();
                jq("#team_container tr.open").removeClass("open");
            }
            if (jq(elt).is(".dropdown-item")) {
                $actionPanel.hide();
                jq("#team_container tr.open").removeClass("open");
            }
        });
        
        displayTeam(ASC.Projects.Master.TeamWithBlockedUsers);
        calculateWidthBlockUserInfo();

        var teamUserIds = [];
        jq(ASC.Projects.Master.TeamWithBlockedUsers).each(function (i, el) { teamUserIds.push(el.id) });

        // userselector for the team

        jq("#pm-projectTeam-Selector").useradvancedSelector({
            showGroups: true,
            itemsSelectedIds: teamUserIds,
            itemsDisabledIds: [managerId]
        }).on("showList", manageTeam);
    };

    var onRemoveMember = function (params, user) {
        jq("tr[data-partisipantid='" + params.userId + "']").remove();

        jq("#pm-projectTeam-Selector").useradvancedSelector("unselect", [params.userId]);


        ASC.Projects.projectNavPanel.changeModuleItemsCount(ASC.Projects.projectNavPanel.projectModulesNames.team, "delete");

        for (var i = 0; i < ASC.Projects.Master.Team.length; i++) {
            if (ASC.Projects.Master.Team[i].id == params.userId) {
                ASC.Projects.Master.Team.splice([i], 1);
            }
        }
        ASC.Projects.TaskAction.onUpdateProjectTeam();
    };

    var updateCommonData = function (team) {
        ASC.Projects.Master.TeamWithBlockedUsers = team;
        ASC.Projects.Master.Team = ASC.Projects.Common.removeBlockedUsersFromTeam(ASC.Projects.Master.TeamWithBlockedUsers);
        ASC.Projects.TaskAction.onUpdateProjectTeam();
    };

    var onUpdateTeam = function(params, team) {
        ASC.Projects.projectNavPanel.changeModuleItemsCount(ASC.Projects.projectNavPanel.projectModulesNames.team, team.length);
        displayTeam(team);
        updateCommonData(team);
    };

    var calculateWidthBlockUserInfo = function() {
        var windowWidth = jq(window).width() - 24 * 2,
            mainBlockWidth = parseInt(jq(".mainPageLayout").css("min-width"), 10),
            newWidth = (windowWidth < mainBlockWidth) ? mainBlockWidth : windowWidth;

        var rightSettingCell = jq(".right-settings");
        if (rightSettingCell.length) {
            newWidth -= rightSettingCell.width();
        }
        jq("#team_container .user-info-container").each(
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
            jq("#team_container .user-info-container").each(
                function() {
                    jq(this).css("width", newWidth + "px");
                }
            );
            jq("#team_container .user-info").each(
                function() {
                    jq(this).css("width", newWidth + 50 + "px");
                }
            );
        }
    };

    var displayTeam = function(team) {
        for (var i = 0; i < team.length; i++) {
            team[i].isManager = managerId == team[i].id;
            team[i].title = team[i].title ? team[i].title : "";
            
            if (myGUID == team[i].id) {
                canCreateTask = team[i].canReadTasks;
            }
        }
        jq("#team_container").empty();
        jq('#memberTemplate').tmpl(team).prependTo("#team_container");
    };

    var onSetTeamSecurity = function(params, data) {
        var userId = params.partisipant;
        var flagCont = jq(".pm-projectTeam-participantContainer[data-partisipantId='" + userId + "']").find("span[data-flag='" + params.securityFlag + "']");
        if (flagCont.hasClass("pm-projectTeam-modulePermissionOff")) {
            flagCont.removeClass("pm-projectTeam-modulePermissionOff");
            flagCont.addClass("pm-projectTeam-modulePermissionOn");
        } else {
            flagCont.removeClass("pm-projectTeam-modulePermissionOn");
            flagCont.addClass("pm-projectTeam-modulePermissionOff");
        }
    };
    var showAddNewTaskPopup = function(userId) {
        ASC.Projects.TaskAction.showCreateNewTaskForm();
        jq('#fullFormUserList').show();
        jq('#fullFormUserList div').remove();

        jq("#taskResponsible option[value=" + userId + "]").attr("selected", "selected");
        jq("#taskResponsible").change();

        if (jq("#addTaskPanel #fullFormUserList div.user").length == jq("#addTaskPanel #fullFormUserList div.user[data-value=" + Teamlab.profile.id + "]").length) {
            jq('#addTaskPanel #notify').attr('disabled', 'true');
        } else {
            jq('#addTaskPanel #notify').removeAttr('disabled');
        }

        return false;
    };
    return {
        init: init
    };
})(jQuery);


