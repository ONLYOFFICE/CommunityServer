/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
ASC.Projects.projectNavPanel = (function() {
    var projectModulesNames = {
        tasks: "tasksModule",
        milestones: "milestonesModule",
        messages: "messagesModule",
        timeTraking: "timetrackingModule",
        docs: "tmdocsModule",
        team: "projectteamModule",
        contacts: "contactsModule"
    };
    var currentProjectId;
    var isInit;
    
    var init = function () {
        if (isInit) {
            return;
        }

        isInit = true;
        
        currentProjectId = jq.getURLParam("PrjID");

        if (Number(jq.getURLParam("id")) === 0) {
            Teamlab.getPrjProject({}, currentProjectId, { success: initTabs });
        }

        jq('#questionWindowDelProj .remove').bind('click', function() {
            var projectId = jq.getURLParam("prjID");
            deleteProject(projectId);
            return false;
        });
        jq('#questionWindowDelProj .cancel, .projectDescriptionPopup .cancel').bind('click', function() {
            jq.unblockUI();
            return false;
        });
        jq('#deleteProject').click(function() {
            jq("#projectActions").hide();
            jq(".project-title .menu-small").removeClass("active");
            showQuestionWindow();
            return false;
        });
        jq("#viewDescription").click(function() {
            jq("#projectActions").hide();
            jq(".project-title .menu-small").removeClass("active");
            showProjectDescription();
            return false;
        });
        jq("#followProject").click(function() {
            if (!jq(this).attr("data-followed")) {
                Teamlab.subscribeProject({ followed: true }, currentProjectId, { success: onSubscribeProject });
            } else {
                Teamlab.subscribeProject({ followed: false }, currentProjectId, { success: onSubscribeProject });
            }
            return false;
        });
        
        jq("body").click(function () {
            jq(".project-title .menu-small").removeClass("active");
        });
        jq(".gant-chart-link").trackEvent("gantt-chart", ga_Actions.actionClick, "gantt-btn");
        
        var dropdownId = "";
        var id = jq.getURLParam('id');
        
        if (id == null) {
            dropdownId = "projectActions";
        } else {
            if (location.href.indexOf("tasks.aspx") > 0) dropdownId = "taskActions";
            else if (location.href.indexOf("timetracking.aspx") > 0) dropdownId = "projectActions";
            else if (location.href.indexOf("messages.aspx") > 0) dropdownId = "discussionActions";
        }
        jq.dropdownToggle({
            switcherSelector: '.project-title .menu-small',
            dropdownID: dropdownId,
            addTop: -4,
            addLeft: -11,
            showFunction: function (switcherObj, dropdownItem) {
                jq('.project-title .menu-small').removeClass('active');
                if (dropdownItem.is(':hidden')) {
                    switcherObj.addClass('active');
                }
            },
            hideFunction: function () {
                jq('.project-title .menu-small').removeClass('active');
            }
        });
    };

    var initTabs = function(params, project) {
        var tabs = [];

        tabs.push(
            {
                title: createTitle(ASC.Projects.Resources.ProjectsJSResource.TasksModule, project.taskCount),
                selected: location.href.indexOf('tasks.aspx') > 0,
                href: "tasks.aspx?prjID=" + project.id,
                divID: projectModulesNames.tasks
            });
        tabs.push(
            {
                title: createTitle(ASC.Projects.Resources.ProjectsJSResource.MilestonesModule, project.milestoneCount),
                selected: location.href.indexOf('milestones.aspx') > 0,
                href: "milestones.aspx?prjID=" + project.id,
                divID: projectModulesNames.milestones
            });

        if (project.security.canReadMessages) {
            tabs.push(
                {
                    title: createTitle(ASC.Projects.Resources.ProjectsJSResource.DiscussionsModule, project.discussionCount),
                    selected: location.href.indexOf('messages.aspx') > 0,
                    href: "messages.aspx?prjID=" + project.id,
                    divID: projectModulesNames.messages
                });
        }

        if (!Teamlab.profile.isVisitor) {
            tabs.push(
                {
                    title: createTitle(ASC.Projects.Resources.ProjectsJSResource.TimeTrackingModule, project.timeTrackingTotal),
                    selected: location.href.toLowerCase().indexOf('timetracking.aspx') > 0,
                    href: "timetracking.aspx?prjID=" + project.id,
                    divID: projectModulesNames.timeTraking
                });
        }

        if (project.security.canReadFiles) {
            tabs.push(
                {
                    title: createTitle(ASC.Projects.Resources.ProjectsJSResource.DocumentsModule, project.documentsCount),
                    selected: location.href.indexOf('tmdocs.aspx') > 0,
                    href: "tmdocs.aspx?prjID=" + project.id,
                    divID: projectModulesNames.docs
                });
        }
        
        if (project.security.canReadContacts) {
            tabs.push(
                {
                    title: ASC.Projects.Resources.ProjectsJSResource.ContactsModule,
                    selected: location.href.indexOf('contacts.aspx') > 0,
                    href: "contacts.aspx?prjID=" + project.id,
                    divID: projectModulesNames.contacts
                });
        }
        
        tabs.push(
            {
                title: createTitle(ASC.Projects.Resources.ProjectsJSResource.TeamModule, project.participantCount),
                selected: location.href.toLowerCase().indexOf('projectteam.aspx') > 0,
                href: "projectteam.aspx?prjID=" + project.id,
                divID: projectModulesNames.team
            });

        window.ASC.Controls.ClientTabsNavigator.init("projectTabs", {
            tabs: tabs
        });
        
        ASC.Projects.navSidePanel.initMenuItems([
            { id: '#messagesModule_tab', href: 'messages.aspx?prjID=' + project.id, onclick: highlightMenu },
            { id: '#milestonesModule_tab', href: 'milestones.aspx?prjID=' + project.id, onclick: highlightMenu },
            { id: '#tasksModule_tab', href: 'tasks.aspx?prjID=' + project.id, onclick: highlightMenu },
            { id: '#timetrackingModule_tab', href: 'timetracking.aspx?prjID=' + project.id, onclick: highlightMenu }]);
       
        jq("#projectTabs").removeClass("display-none");
    };

    var highlightMenu = function () {
        jq("#projectTabs a").removeClass("selectedTab");
        jq(this.id).addClass("selectedTab");
    };

    var createTitle = function(moduleName, count) {
        var result = moduleName;
        if (count) result += "<span class='count'> (" + count + ")</span>";
        return result;
    };

    var changeModuleItemsCount = function(moduleName, actionOrCount) {
        var currentCount;
        var countContainer = jq("#" + moduleName + "_tab .count");
        if (!countContainer.length) {
            jq("#" + moduleName + "_tab").append("<span class='count'></span>");
            countContainer = jq("#" + moduleName + "_tab .count");
        }

        var text = jq.trim(countContainer.text());
        if (text == "") currentCount = 0;
        else {
            text = text.substr(1, text.length - 2);
            currentCount = parseInt(text);
        }

        if (typeof (actionOrCount) == "string") {
            if (actionOrCount == "add") {
                currentCount++;
                countContainer.text(" (" + currentCount + ")");
            } else if (actionOrCount == "delete") {
                currentCount--;
                if (currentCount != 0) {
                    countContainer.text(" (" + currentCount + ")");
                } else {
                    countContainer.empty();
                }
            }
        } else {
            countContainer.text(" (" + actionOrCount + ")");
        }
    };

    var changeCommonProjectTime = function(time) {
        var countContainer = jq("#" + projectModulesNames.timeTraking + "_tab .count");
        if (!countContainer) return;

        var currentTime = { hours: 0, minutes: 0 };
        var text = jq.trim(countContainer.text());
        if (text != "") {
            text = text.substr(1, text.length - 2);
            text = text.split(":");
            currentTime.hours = parseInt(text[0], 10);
            currentTime.minutes = parseInt(text[1], 10);
        }

        currentTime.hours += time.hours;
        currentTime.minutes += time.minutes;

        if (currentTime.minutes > 59) {
            currentTime.hours += 1;
            currentTime.minutes -= 60;
        }
        if (currentTime.minutes < 0) {
            currentTime.hours -= 1;
            currentTime.minutes += 60;
        }

        if (currentTime.hours == 0 && currentTime.minutes == 0) countContainer.empty();

        if (currentTime.minutes < 10) {
            countContainer.text(" (" + currentTime.hours + ":0" + currentTime.minutes + ")");
        } else {
            countContainer.text(" (" + currentTime.hours + ":" + currentTime.minutes + ")");
        }

    };

    var onSubscribeProject = function(params) {
        jq("#projectActions").hide();
        jq(".project-title .menu-small").removeClass("active");
        var followLink = jq("#followProject");
        var currentText = jq.trim(followLink.attr('title'));
        var newText = followLink.attr("data-text");
        followLink.attr('title', newText);
        followLink.attr("data-text", currentText);
        if (params.followed) {
            followLink.attr("data-followed", "followed");
            followLink.removeClass("unsubscribed").addClass("subscribed");
        } else {
            followLink.removeAttr("data-followed");
            followLink.removeClass("subscribed").addClass("unsubscribed");
        }
    };

    var deleteProject = function(projectId) {
        var params = {};
        LoadingBanner.showLoaderBtn("#questionWindowDelProj");
        Teamlab.removePrjProject(params, projectId, { success: onDeleteProject, error: onDeleteProjectError });
    };

    var onDeleteProject = function() {
        document.location.replace("projects.aspx");
    };

    var onDeleteProjectError = function () {
        LoadingBanner.hideLoaderBtn("#questionWindowDelProj");
        jq.unblockUI();
    };
    var showQuestionWindow = function() {
        StudioBlockUIManager.blockUI(jq("#questionWindowDelProj"), 400, 300, 0);
    };
    var showProjectDescription = function() {     
        var prjInfoDescr = jq("#prjInfoDescription");
        if (prjInfoDescr.length) {
            var description = prjInfoDescr.attr("data-description").trim();
            if (description.length) {
                var newText = jq.linksParser(description.replace(/</ig, '&lt;').replace(/>/ig, '&gt;').replace(/\n/ig, '<br/>').replace('&amp;', '&'));
                jq("#prjInfoDescription").html(newText);
            }
        }
        StudioBlockUIManager.blockUI(jq('.projectDescriptionPopup'), 560, 300, 0);
    };
    return {
        init: init,
        changeModuleItemsCount: changeModuleItemsCount,
        projectModulesNames: projectModulesNames,
        changeCommonProjectTime: changeCommonProjectTime
    };
})(jQuery);