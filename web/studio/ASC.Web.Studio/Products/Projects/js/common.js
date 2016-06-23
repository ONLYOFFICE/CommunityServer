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


/*******************************************************************************/
if (typeof ASC === "undefined")
    ASC = {};

if (typeof ASC.Projects === "undefined")
    ASC.Projects = {};

ASC.Projects.Common = (function () {
    this.prjId = jq.getURLParam("prjID");
    this.allUsers = [];
    this.allUsersHash = { allUsersCount: 0 };
    this.initApi = false;
    this.initMobileBanner = false;
    this.ckEditor = null;

    var init = function () {
        if (ASC.Projects.Base) {
            ASC.Projects.Base.clearTables();
        }

        if (!this.initMobileBanner && jq(".mobileApp-banner").length) {
            initMobileBanner();
            this.initMobileBanner = true;
        }

        bindApi();
        initApiData();
        initPages();
        bindCommonEvents();
        MoveTaskQuestionPopup.init();
    };



    var bindApi = function () {
        jq('body').off('click.milestonesInit');
        jq('body').off('click.projectsInit');
        jq('body').off('click.tasksInit');
    };

    var bindCommonEvents = function () {
        if (ASC.Projects.ProjectsAdvansedFilter) {
            ASC.Projects.ProjectsAdvansedFilter.bindEvents();
        }
        Teamlab.unbind(Teamlab.events.getException);
        Teamlab.bind(Teamlab.events.getException, function(params, errors) {
            if (errors && errors[0] == "unauthorized request") {
                window.location = "/auth.aspx";
            }
        });
    };

    var unbindEvents = function () {
        if (location.href.indexOf("ganttchart.aspx") > 0) return;
        ASC.Projects.ProjectsAdvansedFilter.unbindEvents();
        ASC.Projects.PageNavigator.unbindListEvents();
        ASC.Projects.AllProject.unbindListEvents();
        ASC.Projects.AllMilestones.unbindListEvents();
        ASC.Projects.Discussions.unbindListEvents();
        ASC.Projects.TasksManager.unbindListEvents();
        ASC.Projects.TimeSpendActionPage.unbindListEvents();
    };

    var initPages = function () {
        var action = jq.getURLParam('action');
        var id = jq.getURLParam('id');

        unbindEvents(); // remove events handlers for previos pages

        if (typeof ASC.Projects.TaskAction != "undefined") {
            ASC.Projects.TaskAction.init();
        }

        if (typeof ASC.Projects.MilestoneAction != "undefined") {
            ASC.Projects.MilestoneAction.init();
        }

        if (typeof ASC.Projects.navSidePanel != "undefined") {
            ASC.Projects.navSidePanel.init();
        }

        //init projNavPanel
        if (jq.getURLParam('prjId') && location.href.indexOf("ganttchart.aspx") == -1) {
            ASC.Projects.projectNavPanel.init();
        }

        if (location.href.indexOf("tasks.aspx") > 0) {
            if (id) {
                ASC.Projects.TaskDescroptionPage.init();
            } else {
                ASC.Projects.TasksManager.init();
            }
        }

        if (location.href.indexOf("messages.aspx") > 0) {
            if (action) {
                ASC.Projects.DiscussionAction.init();
                ckeditorConnector.onReady(function () {
                    ASC.Projects.Common.ckEditor = jq("#ckEditor").ckeditor({ toolbar: 'PrjMessage', extraPlugins: 'oembed,teamlabcut,codemirror', removePlugins: 'div', filebrowserUploadUrl: 'fckuploader.ashx?newEditor=true&esid=projects_comments' }).editor;
                    ASC.Projects.Common.ckEditor.on("change", ASC.Projects.DiscussionAction.showHidePreview);
                });
            }
            if (id && action == null) {
                ASC.Projects.DiscussionDetails.init();
            }
            if (id == null && action == null) {
                ASC.Projects.Discussions.init();
            }
        }

        if (location.href.indexOf("milestones.aspx") > 0) {
            ASC.Projects.AllMilestones.init();
        }

        if (location.href.indexOf("projects.aspx") > 0) {
            if (action == null) {
                ASC.Projects.AllProject.init(false);
            }
            jq('#projectTitleContainer .inputTitleContainer').css('width', '100%');
            if (action == "edit") {
                jq('.dottedHeader').removeClass('dottedHeader');
                jq('#projectDescriptionContainer').show();
                jq('#notifyManagerCheckbox').attr('disabled', 'disabled');
                jq('#projectTagsContainer').show();
            }
        }

        if (location.href.toLowerCase().indexOf("projectteam.aspx") > 0) {
            ASC.Projects.ProjectTeam.init();
            jq('#PrivateProjectHelp').click(function () {
                jq(this).helper({ BlockHelperID: 'AnswerForPrivateProjectTeam' });
            });
            jq('#RestrictAccessHelp').click(function () {
                jq(this).helper({ BlockHelperID: 'AnswerForRestrictAccessTeam' });
            });
        }

        if (location.href.indexOf("timer.aspx") > 0) {
            ASC.Projects.TimeTraking.init();
        }

        if (location.href.toLowerCase().indexOf("timetracking.aspx") > 0) {
            ASC.Projects.TimeTrakingEdit.init();
            ASC.Projects.TimeSpendActionPage.init();
        }

        if (location.href.toLowerCase().indexOf("projecttemplates.aspx") > 0) {
            ASC.Projects.Templates.init();
        }

        jq(function () {
            jq('#content .close').on('click', function () {
                jq('[blank-page]').remove();
            });
        });

        if (location.href.indexOf("generatedreport.aspx") > 0) {
            ASC.Projects.GeneratedReportView.init();
            ASC.Projects.ReportView.init();
        }

        if (location.href.indexOf("reports.aspx") > 0) {
            ASC.Projects.ReportView.init();
        }
        
        if (location.href.indexOf("contacts.aspx") > 0) {
            ASC.Projects.Contacts.init();
        }
    };

    var initApiData = function () {
        if (!this.initApi) {
            this.initApi = true;
        } else {
            return;
        }

        if (typeof (ASC.Projects.Master) != 'undefined') {
            if (typeof (ASC.Projects.Master.Projects) != 'undefined' && ASC.Projects.Master.Projects != null) {
                ASC.Projects.Master.Projects = Teamlab.create('prj-projects', null, ASC.Projects.Master.Projects.response);
            }
            
            if (typeof (ASC.Projects.Master.Tags) != 'undefined' && ASC.Projects.Master.Tags != null) {
                ASC.Projects.Master.Tags = ASC.Projects.Master.Tags.response;
                
            }
            
            if (typeof (ASC.Projects.Master.Team) != 'undefined' && ASC.Projects.Master.Team != null) {
                ASC.Projects.Master.TeamWithBlockedUsers = Teamlab.create('prj-projectpersons', null, ASC.Projects.Master.Team.response);
                ASC.Projects.Master.Team = ASC.Projects.Common.removeBlockedUsersFromTeam(ASC.Projects.Master.TeamWithBlockedUsers);
            }
            
            if (typeof (ASC.Projects.Master.Milestones) != 'undefined' && ASC.Projects.Master.Milestones != null) {
                ASC.Projects.Master.Milestones = Teamlab.create('prj-milestones', null, ASC.Projects.Master.Milestones.response);
                ASC.Projects.Master.Milestones = ASC.Projects.Master.Milestones.sort(milestoneSort);
            } 
        } else {
            ASC.Projects.Master = {};
        }
    };

    var initMobileBanner = function () {
        jq(".mobileApp-banner_btn.app-store").trackEvent("mobileApp-banner", "action-click", "app-store");
        jq(".mobileApp-banner_btn.google-play").trackEvent("mobileApp-banner", "action-click", "google-play");
    };

    var bind = function (eventName, handler) {
        jq(document).bind(eventName, handler);
    };
    
    var removeBlockedUsersFromTeam = function (team) {
        return team.filter(function(item) {
            return item.status == 1;
        });
    };
    var removeComment = function () {
        if (location.href.toLowerCase().indexOf("tasks.aspx") > 0) {
            ASC.Projects.TaskDescroptionPage.onDeleteComment();
        }

        if (location.href.toLowerCase().indexOf("messages.aspx") > 0) {
            ASC.Projects.DiscussionDetails.removeComment();
        }
    };

    var showTimer = function (url) {
        var width = 288;
        var height = 638;

        if (jq.browser.safari) {
            height = 584;
        } else if (jq.browser.opera) {
            height = 620;
        }

        if (jq.browser.msie) {
            width = 284;
            height = 614;
        }
        
        if (jq.browser.chrome) {
            height = 658;
        }

        var params = "width=" + width + ",height=" + height + ",resizable=yes";
        var hWnd = null;
        var isExist = false;

        try {
            hWnd = window.open('', "displayTimerWindow", params);
        } catch (err) {
        }

        try {
            isExist = !hWnd || typeof hWnd.ASC === 'undefined' ? false : true;
        } catch (err) {
            isExist = true;
        }

        if (!isExist) {
            hWnd = window.open(url, "displayTimerWindow", params);
            isExist = true;
        }

        if (!isExist) {
            return undefined;
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
        return ASC.Projects.Master.Team.find(function(item) {
            return item.id == userId;
        });
    };

    var getUserById = function (id) {
        var user;
        if (!allUsers.length) {
            allUsers = ASC.Resources.Master.ApiResponses_Profiles.response;
            allUsersHash.allUsersCount = allUsers.length;
        }
        if (allUsersHash && allUsersHash[id]) return allUsersHash[id];
        for (var i = 0; i < allUsersHash.allUsersCount; i++) {
            user = allUsers[i];
            if (id == user.id) {
                allUsersHash[id] = user;
                return user;
            }
        }
        return "User not found";
    };

    var currentUserIsModuleAdmin = function () {
        return Teamlab.profile.isAdmin || ASC.Projects.Master.IsModuleAdmin;
    };

    var linkTypeEnum = {
        start_start: 0,
        end_end: 1,
        start_end: 2,
        end_start: 3
    };

    var getPossibleTypeLink = function (firstTaskStart, firstTaskDeadline, secondTaskStart, secondTaskDeadline, relatedTaskObject) {
        var possibleTypeLinks = [-1, -1, -1, -1];
        possibleTypeLinks[0] = ASC.Projects.Common.linkTypeEnum.start_start; // possible for all tasks

        if (firstTaskDeadline && secondTaskDeadline) {
            possibleTypeLinks[1] = ASC.Projects.Common.linkTypeEnum.end_end; // possible for tasks with deadline

            if (firstTaskDeadline <= secondTaskStart) {
                possibleTypeLinks[3] = ASC.Projects.Common.linkTypeEnum.end_start;
            } else if (secondTaskDeadline <= firstTaskStart) {
                possibleTypeLinks[2] = ASC.Projects.Common.linkTypeEnum.start_end;
            } else {
                relatedTaskObject.invalidLink = true;
                if (firstTaskStart <= secondTaskStart) {
                    possibleTypeLinks[3] = ASC.Projects.Common.linkTypeEnum.end_start;
                } else {
                    possibleTypeLinks[2] = ASC.Projects.Common.linkTypeEnum.start_end;
                }
            }
        } else {
            if (secondTaskDeadline) {
                possibleTypeLinks[2] = ASC.Projects.Common.linkTypeEnum.start_end;
                if (secondTaskDeadline > firstTaskStart) {
                    relatedTaskObject.invalidLink = true;
                }
            } else if (firstTaskDeadline) {
                possibleTypeLinks[3] = ASC.Projects.Common.linkTypeEnum.end_start;
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

    var emptyGuid = "00000000-0000-0000-0000-000000000000";
    var defaultPageURL = "projects.aspx";

    var projectsForFilter;
    var getProjectsForFilter = function () {
        if (projectsForFilter) return projectsForFilter;
        var currentUserProjects = [];
        var otherProjects = [];

        var projects = ASC.Projects.Master.Projects;
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

    var milestoneSort = function (a, b) {
        var deadlineSort = defaultSort(a.deadline, b.deadline);
        return deadlineSort ? deadlineSort : defaultSort(a.title, b.title, true);
    };

    var defaultSort = function (a, b, asc) {
        if (asc) return (a < b) ? -1 : (a > b) ? 1 : 0;
        return (a < b) ? 1 : (a > b) ? -1 : 0;
    };
    
    return {
        bind: bind,

        currentUserIsModuleAdmin: currentUserIsModuleAdmin,
        currentUserIsProjectManager: function (projectId) {
            return ASC.Projects.Master.Projects.some(function(item) {
                return item.id == projectId && item.responsibleId === Teamlab.profile.id;
            });
        },

        defaultPageURL: defaultPageURL,
        displayInfoPanel: displayInfoPanel,
        
        emptyGuid: emptyGuid,
        events: { loadTags: "loadTags", loadProjects: "loadProjects", loadTeam: "loadTeam", loadMilestones: "loadMilestones" },
        excludeVisitors: excludeVisitors,

        filterParamsForListProjects: { sortBy: "title", sortOrder: "ascending", status: "open", fields: "id,title,security,isPrivate,status,responsible" },
        filterParamsForListMilestones: { sortBy: "deadline", sortOrder: "descending", status: "open", fields: "id,title,deadline" },
        filterParamsForListTasks: { sortBy: "deadline", sortOrder: "ascending" },
        
        getPossibleTypeLink: getPossibleTypeLink,
        getProjectsForFilter: getProjectsForFilter,
        getUserById: getUserById,
        
        baseInit: init,
        
        linkTypeEnum: linkTypeEnum,
        milestoneSort: milestoneSort,

        removeBlockedUsersFromTeam: removeBlockedUsersFromTeam,
        removeComment: removeComment,

        showTimer: showTimer,
        
        userInProjectTeam: userInProjectTeam
    };

})();

var MoveTaskQuestionPopup = (function () {
    var isInit = false,
        links = [],
        firstBlockId = "",
        blockId;
    var successFunc = function () { };
    var cancelFunc = function () { };

    var init = function () {
        if (isInit) return;
        isInit = true;

        jq("#removeTaskLinksQuestionPopup .one-move, #removeTaskLinksQuestionPopupDeadLine .one-move").click(function () {
            for (var j = 0; j < links.length; ++j) {
                var data = { dependenceTaskId: links[j].dependenceTaskId, parentTaskId: links[j].parentTaskId };
                Teamlab.removePrjTaskLink({}, links[j].dependenceTaskId, data, { success: function () { } });
            }
            successFunc();
        });
        jq("#removeTaskLinksQuestionPopup .cancel, #removeTaskLinksQuestionPopupDeadLine .cancel").click(function () {
            jq.unblockUI();
            cancelFunc();
        });
    };

    var setParametrs = function (firstBlockId, tasklinks, success, cancel, block) {
        links = tasklinks;
        firstBlockId = firstBlockId;
        successFunc = success;
        cancelFunc = cancel;
        blockId = block;
    };

    var showDialog = function () {
        jq.unblockUI();
        StudioBlockUIManager.blockUI(blockId, "auto", 200, 0, "absolute");
    };

    return {
        init: init,
        setParametrs: setParametrs,
        showDialog: showDialog
    };
})();

jq(document).ready(function () {
    ASC.Projects.Common.baseInit();
});