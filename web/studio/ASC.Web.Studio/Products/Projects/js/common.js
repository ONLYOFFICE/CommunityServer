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

    var init = function () {
        clearTables();

        if (!this.initMobileBanner && jq(".mobileApp-banner").length) {
            initMobileBanner();
            this.initMobileBanner = true;
        }
        bindApi();
        initPages();
        initApiData();
        bindCommonEvents();

        MoveTaskQuestionPopup.init();
    };

    var clearTables = function() {
        jq("#tableListProjects tbody, #milestonesList tbody, .taskList, #discussionsList, #timeSpendsList tbody").empty();
        jq("#tableListProjects, #milestonesList, .taskList, #discussionsList, #timeSpendsList, #tableForNavigation").hide();
        jq("#totalTimeText").remove();
        jq("[id$='EmptyScreenForFilter']").hide();
        jq("[id^='emptyList']").hide();
    };

    var bindApi = function () {
        jq(document).unbind("loadApiData");
        jq(document).unbind("createAdvansedFilter");
        jq(document).unbind(ASC.Projects.Common.events.loadProjects);
        jq(document).unbind(ASC.Projects.Common.events.loadMilestones);
        jq(document).unbind(ASC.Projects.Common.events.loadTags);
        jq(document).unbind(ASC.Projects.Common.events.loadTeam);
        ASC.Projects.Common.loadListProjectsFlag = false;
        ASC.Projects.Common.loadListTagsFlag = false;
        ASC.Projects.Common.loadListMilestonesFlag = false;
        ASC.Projects.Common.loadTeamFlag = false;

        // waiting data from api
        jq(document).bind("loadApiData", function () {
            if (ASC.Projects.Common.loadListProjectsFlag && ASC.Projects.Common.loadListTagsFlag && ASC.Projects.Common.loadListMilestonesFlag && ASC.Projects.Common.loadTeamFlag) {
                jq(document).trigger("createAdvansedFilter");
            }
        });

        ASC.Projects.Common.bind(ASC.Projects.Common.events.loadProjects, function () {
            ASC.Projects.Common.loadListProjectsFlag = true;
            jq(document).trigger("loadApiData");
        });

        ASC.Projects.Common.bind(ASC.Projects.Common.events.loadTags, function () {
            ASC.Projects.Common.loadListTagsFlag = true;
            jq(document).trigger("loadApiData");
        });

        ASC.Projects.Common.bind(ASC.Projects.Common.events.loadMilestones, function () {
            ASC.Projects.Common.loadListMilestonesFlag = true;
            jq(document).trigger("loadApiData");
        });

        if (jq.getURLParam('prjID')) {
            ASC.Projects.Common.bind(ASC.Projects.Common.events.loadTeam, function () {
                ASC.Projects.Common.loadTeamFlag = true;
                jq(document).trigger("loadApiData");
            });
        } else {
            ASC.Projects.Common.loadTeamFlag = true;
            jq(document).trigger("loadApiData");
        }
    };

    var bindCommonEvents = function () {
        jq("body").on("click", ".clearFilterButton", function () {
            jq('#ProjectsAdvansedFilter').advansedFilter(null);
            return false;
        });
    };

    var unbindEvents = function () {
        if (location.href.indexOf("ganttchart.aspx") > 0) return;
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
                CKEDITOR.replace('ckEditor', { toolbar: 'PrjMessage', extraPlugins: 'oembed,teamlabcut', removePlugins: 'div', filebrowserUploadUrl: 'fckuploader.ashx?newEditor=true&esid=discussion' });
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
            if (action == "edit") {
                jq('.dottedHeader').removeClass('dottedHeader');
                jq('#projectTitleContainer .inputTitleContainer').css('width', '100%');
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

        if (location.href.indexOf("tmdocs.aspx") > 0) {
            ZeroClipboard.setMoviePath('/js/flash/zeroclipboard/zeroclipboard10.swf');
        }

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
            jq(document).trigger(ASC.Projects.Common.events.loadProjects);
            jq(document).trigger(ASC.Projects.Common.events.loadTags);
            jq(document).trigger(ASC.Projects.Common.events.loadTeam);
            jq(document).trigger(ASC.Projects.Common.events.loadMilestones);
            return;
        }

        if (typeof (ASC.Projects.Master) != 'undefined') {
            if (typeof (ASC.Projects.Master.Projects) != 'undefined' && ASC.Projects.Master.Projects != null) {
                ASC.Projects.Master.Projects = Teamlab.create('prj-projects', null, ASC.Projects.Master.Projects.response);
                jq(document).trigger(ASC.Projects.Common.events.loadProjects);
            } else {
                Teamlab.getPrjProjects({}, {
                    filter: ASC.Projects.Common.filterParamsForListProjects,
                    success: function (param, projects) {
                        ASC.Projects.Master.Projects = projects;
                        jq(document).trigger(ASC.Projects.Common.events.loadProjects);
                    }
                });
            }
            if (typeof (ASC.Projects.Master.Tags) != 'undefined' && ASC.Projects.Master.Tags != null) {
                ASC.Projects.Master.Tags = ASC.Projects.Master.Tags.response;
                jq(document).trigger(ASC.Projects.Common.events.loadTags);
            } else {
                Teamlab.getPrjTags({}, {
                    success: function (params, tags) {
                        ASC.Projects.Master.Tags = tags;
                        jq(document).trigger(ASC.Projects.Common.events.loadTags);
                    }
                });
            }
            if (typeof (ASC.Projects.Master.Team) != 'undefined' && ASC.Projects.Master.Team != null) {
                ASC.Projects.Master.TeamWithBlockedUsers = Teamlab.create('prj-projectpersons', null, ASC.Projects.Master.Team.response);
                ASC.Projects.Master.Team = ASC.Projects.Common.removeBlockedUsersFromTeam(ASC.Projects.Master.TeamWithBlockedUsers);
                jq(document).trigger(ASC.Projects.Common.events.loadTeam);
            } else {
                ASC.Projects.Common.updateProjectTeam();
            }
            if (typeof (ASC.Projects.Master.Milestones) != 'undefined' && ASC.Projects.Master.Milestones != null) {
                ASC.Projects.Master.Milestones = Teamlab.create('prj-milestones', null, ASC.Projects.Master.Milestones.response);
                ASC.Projects.Master.Milestones = ASC.Projects.Master.Milestones.sort(milestoneSort);
                jq(document).trigger(ASC.Projects.Common.events.loadMilestones);
            } else {
                var filter = ASC.Projects.Common.filterParamsForListMilestones;
                if (prjId) {
                    filter.projectId = prjId;
                }
                Teamlab.getPrjMilestones({}, {
                    filter: filter,
                    success: function (params, milestones) {
                        ASC.Projects.Master.Milestones = milestones;
                        jq(document).trigger(ASC.Projects.Common.events.loadMilestones);
                    }
                });
            }
        } else {
            ASC.Projects.Master = {};
        }
    };

    var initMobileBanner = function () {
        var data = {};
        Teamlab.isMobileAppUser({}, data, {
            success: function (params, isShow) {
                if (!isShow) {
                    jq(".mobileApp-banner").removeClass("display-none");
                }
            }
        });
        jq(".mobileApp-banner_btn.app-store").trackEvent("mobileApp-banner", ga_Actions.actionClick, "app-store");
        jq(".mobileApp-banner_btn.google-play").trackEvent("mobileApp-banner", ga_Actions.actionClick, "google-play");
    };

    var initPageNavigator = function (obj, cookiePaginationKey, small) {
        obj.cookiePaginationKey = cookiePaginationKey;
        obj.entryCountOnPage = getPaginationCookie(cookiePaginationKey);
        obj.currentPage = 0;

        jq("#countOfRows").val(obj.entryCountOnPage).tlCombobox();
        if (small) {
            jq("#tableForNavigation .option-item[data-value=25], .option-item[data-value=50], .option-item[data-value=75], .option-item[data-value=100]").remove();
        } else {
            jq("#tableForNavigation .option-item[data-value=10], .option-item[data-value=20], .option-item[data-value=30], .option-item[data-value=40]").remove();
        }

        window.pgNavigator = new ASC.Controls.PageNavigator.init("pgNavigator", "#divForTaskPager", obj.entryCountOnPage, ASC.Projects.Master.VisiblePageCount, 1,
            ASC.Projects.Resources.ProjectsJSResource.PreviousPage, ASC.Projects.Resources.ProjectsJSResource.NextPage);
        pgNavigator.NavigatorParent = '#divForTaskPager';
        pgNavigator.changePageCallback = function (page) {
            obj.currentPage = page - 1;
            LoadingBanner.displayLoading();
            obj.getData(obj.currentFilter, true);
        };
    };

    var updatePageNavigator = function (obj, filterCount) {
        jq("#totalCount").text(filterCount);
        pgNavigator.drawPageNavigator(obj.currentPage + 1, filterCount);

        setPaginationCookie(obj.entryCountOnPage, obj.cookiePaginationKey);

        jq("#tableForNavigation").show();
        renderSimplePageNavigator();
    };

    var changeCountOfRows = function (obj, newValue) {
        if (isNaN(newValue)) {
            return;
        }
        var newCountOfRows = newValue * 1;
        obj.entryCountOnPage = newCountOfRows;
        pgNavigator.EntryCountOnPage = newCountOfRows;

        LoadingBanner.displayLoading();
        obj.getData(obj.currentFilter, false);
    };

    var renderSimplePageNavigator = function () {
        jq(".simplePageNavigator").html("");
        var $simplePN = jq("<div></div>");
        var lengthOfLinks = 0;
        if (jq("#tableForNavigation .pagerPrevButtonCSSClass").length != 0) {
            lengthOfLinks++;
            jq("#tableForNavigation .pagerPrevButtonCSSClass").clone().appendTo($simplePN);
        }
        if (jq("#tableForNavigation .pagerNextButtonCSSClass").length != 0) {
            lengthOfLinks++;
            if (lengthOfLinks === 2) {
                jq("<span style='padding: 0 8px;'>&nbsp;</span>").clone().appendTo($simplePN);
            }
            jq("#tableForNavigation .pagerNextButtonCSSClass").clone().appendTo($simplePN);
        }
        if ($simplePN.children().length != 0) {
            $simplePN.appendTo(".simplePageNavigator");
            jq(".simplePageNavigator").show();
        }
        else {
            jq(".simplePageNavigator").hide();
        }
    };


    var bind = function (eventName, handler) {
        jq(document).bind(eventName, handler);
    };

    var updateProjectTeam = function () {
        if (prjId) {
            Teamlab.getPrjTeam({}, prjId, {
                success: function (params, team) {
                    ASC.Projects.Master.TeamWithBlockedUsers = team;
                    ASC.Projects.Master.Team = ASC.Projects.Common.removeBlockedUsersFromTeam(ASC.Projects.Master.TeamWithBlockedUsers);
                    jq(document).trigger(ASC.Projects.Common.events.loadTeam);
                }
            });
        }
    };
    
    var removeBlockedUsersFromTeam = function (team) {
        return team.filter(function(item) {
            return item.status == 1;
        });
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
        var hWnd = null;
        var isExist = false;

        try {
            hWnd = window.open('', "displayTimerWindow", "width=" + width + ",height=" + height + ",resizable=yes");
        } catch (err) {
        }
        try {
            isExist = typeof hWnd.ASC === 'undefined' ? false : true;
        } catch (err) {
            isExist = true;
        }

        if (!isExist) {
            hWnd = window.open(url, "displayTimerWindow", "width=" + width + ",height=" + height + ",resizable=yes");
            isExist = true;
        }

        if (!isExist) {
            return undefined;
        }
        try {
            hWnd.focus();
        } catch (err) {
        }
    };

    var excludeVisitors = function (users) {
        return users.filter(function(user) {
            return !user.isVisitor;
        });
    };

    var showCommentBox = function () {
        if (CKEDITOR && !jq.isEmptyObject(CKEDITOR.instances)) {
            CommentsManagerObj.AddNewComment();
        } else {
            setTimeout("ASC.Projects.Common.showCommentBox();", 500);
        }
    };

    var setPaginationCookie = function (countOnPage, cookieKey) {
        if (cookieKey && cookieKey != "") {
            var cookie = {
                countOnPage: countOnPage
            };
            jq.cookies.set(cookieKey, cookie, { path: location.pathname });
        }
    };

    var getPaginationCookie = function (cookieKey) {
        var count = jq.cookies.get(cookieKey);
        if (count != null) {
            return count.countOnPage;
        } else {
            return cookieKey == "discussionsKeyForPagination" ? 10 : ASC.Projects.Master.EntryCountOnPage;
        }
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
    var checkElementNotFound = function (str) {
        if (location.hash.indexOf("elementNotFound") > 0) {
            ASC.Projects.Common.displayInfoPanel(str, true);
        }
    };

    var setDocumentTitle = function (module) {
        document.title = jq.format("{0} - {1}", module, ASC.Projects.Resources.ProjectsJSResource.ProductName);
    };

    var emptyGuid = "00000000-0000-0000-0000-000000000000";
    var defaultPageURL = "projects.aspx";

    var showCommonPopup = function (tmplName, width, height, position) {
        var $commonPopupContainer = jq("#commonPopupContainer");
        $commonPopupContainer.find(".commonPopupContent").empty().append(jq.tmpl(tmplName, {}));
        $commonPopupContainer.find(".commonPopupHeaderTitle").empty().text($commonPopupContainer.find(".hidden-title-text").text());

        StudioBlockUIManager.blockUI(jq('#commonPopupContainer'), width, height, position);
    };

    var getProjectsForFilter = function () {
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

        return currentUserProjects.concat(otherProjects);
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
        
        changeCountOfRows: changeCountOfRows,
        checkElementNotFound: checkElementNotFound,
        clearAdvansedFilter: function () {
            jq("#ProjectsAdvansedFilter").empty();
            jq("#ProjectsAdvansedFilter").removeAttr("class");
            delete window.userSelector;
            ASC.Projects.ProjectsAdvansedFilter.firstload = true;
        },
        clearTables: clearTables,
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
        
        hideAdvansedFilter: function () { jq("#ProjectsAdvansedFilter").hide(); },
        
        init: init,
        initPageNavigator: initPageNavigator,
        
        linkTypeEnum: linkTypeEnum,
        milestoneSort: milestoneSort,

        removeBlockedUsersFromTeam: removeBlockedUsersFromTeam,
        
        setDocumentTitle: setDocumentTitle,
        showAdvansedFilter: function () { jq("#ProjectsAdvansedFilter").show(); },
        showCommentBox: showCommentBox,
        showCommonPopup: showCommonPopup,
        showTimer: showTimer,
        
        
        updatePageNavigator: updatePageNavigator,
        updateProjectTeam: updateProjectTeam,
        userInProjectTeam: userInProjectTeam,

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
    ASC.Projects.Common.init();
});


// Google Analytics const
var ga_Categories = {
    projects: "projects",
    milestones: "projects_milestones",
    tasks: "projects_tasks",
    subtask: "projects_subtask",
    discussions: "projects_discussions",
    timeTrack: "projects_time-track",
    dashboard: "projects_dashboard",
    projectTemplate: "projects_template"
};

var ga_Actions = {
    filterClick: "filter-click",
    createNew: "create-new",
    remove: "remove",
    edit: "edit",
    view: "view",
    changeStatus: "change-status",
    next: "next",
    userClick: "user-click",
    actionClick: "action-click",
    quickAction: "quick-action"
};
// end Google Analytics