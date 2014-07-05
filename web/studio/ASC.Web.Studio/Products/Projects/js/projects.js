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
if (typeof ASC === "undefined")
    ASC = {};
if (typeof ASC.Projects === "undefined")
    ASC.Projects = {};

ASC.Projects.AllProject = (function () {
    var isInit = false,
        overProjDescrPanel = false,
        projDescribeTimeout = 0,
        moduleLocationPath = StudioManager.getLocationPathToModule("projects"),
        linkViewProject = moduleLocationPath + 'tasks.aspx?prjID=',
        linkViewMilestones = moduleLocationPath + 'milestones.aspx?prjID=',
        linkViewTasks = moduleLocationPath + 'tasks.aspx?prjID=',
        linkViewParticipants = moduleLocationPath + 'projectTeam.aspx?prjID=',
        commonListContainer = jq("#CommonListContainer"),
        projectsTable = null,
        describePanel = null;

    var currentUserId;

    var isSimpleView;
    var filterProjCount = 0;

    // object for list statuses
    var statusListObject = { listId: "projectsStatusList" };


    var initActionPanels = function () {
        if (!describePanel) {
            commonListContainer.append(jq.tmpl("projects_panelFrame", { panelId: "projectDescrPanel", cornerPosition: "left" })); // description panel
            describePanel = jq("#projectDescrPanel");
        }
        if (isSimpleView) return;

        statusListObject.statuses = [
            { cssClass: "open", text: ASC.Projects.Resources.ProjectsJSResource.StatusOpenProject },
            { cssClass: "paused", text: ASC.Projects.Resources.ProjectsJSResource.StatusSuspendProject },
            { cssClass: "closed", text: ASC.Projects.Resources.ProjectsJSResource.StatusClosedProject }
        ];
        jq("#" + statusListObject.listId).remove();
        commonListContainer.append(jq.tmpl("projects_statusChangePanel", statusListObject));
    };

    //filter Set
    var init = function (isSimpleViewFlag) {
        if (isInit === false) {
            isInit = true;
            Teamlab.bind(Teamlab.events.getPrjProjects, onGetListProject);
        }
        projectsTable = jq("#tableListProjects");

        isSimpleView = isSimpleViewFlag;
        currentUserId = Teamlab.profile.id;

        initActionPanels();

        if (!isSimpleView) {
            ASC.Projects.Common.setDocumentTitle(ASC.Projects.Resources.ProjectsJSResource.ProjectsModule);
            ASC.Projects.Common.checkElementNotFound(ASC.Projects.Resources.ProjectsJSResource.ProjectNotFound);
            LoadingBanner.displayLoading();
            //page navigator
            ASC.Projects.Common.initPageNavigator(this, "projectsKeyForPagination");

            // waiting data from api
            jq(document).bind("createAdvansedFilter", function () {
                createAdvansedFilter();
            });
        }
        if (!isSimpleView) {
            projectsTable.on('click', "td.responsible span.userLink", function () {
                var responsibleId = jq(this).attr('id');
                if (responsibleId != "4a515a15-d4d6-4b8e-828e-e0586f18f3a3") {
                    var path = jq.changeParamValue(ASC.Controls.AnchorController.getAnchor(), 'project_manager', responsibleId);
                    path = jq.removeParam('team_member', path);
                    ASC.Controls.AnchorController.move(path);
                }
            });

            // popup handlers
            jq("#commonPopupContainer").on("click", ".gray", function () {
                jq.unblockUI();
                return false;
            });
            commonListContainer.on("click", "#statusList .dropdown-item", function () {
                changeStatus(this);
            });
        }
        // discribe panel
        projectsTable.on("mouseenter", ".nameProject a", function (event) {
            projDescribeTimeout = setTimeout(function () {
                var targetObject = event.target,
                    panelContent = describePanel.find(".panel-content"),
                    createdAttr = jq(targetObject).attr('created'),
                    description = jq(targetObject).siblings('.description').text(),
                    descriptionObj = {};

                panelContent.empty();

                if (typeof createdAttr != 'undefined' && jq.trim(createdAttr) != "") {
                    descriptionObj.creationDate = createdAttr;
                }

                if (jq.trim(description) != '') {
                    descriptionObj.description = description;
                    if (description.indexOf("\n") > 2 || description.length > 80) {
                        descriptionObj.readMore = "projects.aspx?prjID=" + jq(targetObject).attr('projectid');
                    }
                }
                if (descriptionObj.creationDate || descriptionObj.description) {
                    panelContent.append(jq.tmpl("projects_descriptionPanelContent", descriptionObj));
                    showProjDescribePanel(targetObject);
                    overProjDescrPanel = true;
                }
            }, 500, this);
        });
        projectsTable.on('mouseleave', '.nameProject a', function () {
            clearTimeout(projDescribeTimeout);
            overProjDescrPanel = false;
            hideDescrPanel();
        });

        describePanel.on('mouseenter', function () {
            overProjDescrPanel = true;
        });

        describePanel.on('mouseleave', function () {
            overProjDescrPanel = false;
            hideDescrPanel();
        });

        /*--------events--------*/

        jq("#countOfRows").change(function (evt) {
            ASC.Projects.Common.changeCountOfRows(ASC.Projects.AllProject, this.value);
        });

        jq('body').click(function (event) {
            var elt = (event.target) ? event.target : event.srcElement;
            var isHide = true;
            var $elt = jq(elt);

            if ($elt.is('#' + statusListObject.listId)) isHide = false;
            if (isHide) {
                jq('#' + statusListObject.listId).hide();
                jq('.statusContainer').removeClass('openList');
            }
        });

        commonListContainer.on('click', 'td.action .canEdit', function (event) {
            showListStatus(statusListObject.listId, this);
            return false;
        });

        // ga-track-events
        if (!isSimpleView) {
            //change status
            jq("#statusList .open").trackEvent(ga_Categories.projects, ga_Actions.changeStatus, "open");
            jq("#statusList .closed").trackEvent(ga_Categories.projects, ga_Actions.changeStatus, "closed");
            jq("#statusList .paused").trackEvent(ga_Categories.projects, ga_Actions.changeStatus, "paused");

            //PM
            jq(".responsible .userLink").trackEvent(ga_Categories.projects, ga_Actions.userClick, "project-manager");

            //end ga-track-events
        }
    };

    var unbindListEvents = function () {
        if (!isInit) return;
        jq("#countOfRows").unbind();
        projectsTable.unbind();
        describePanel.unbind();
        commonListContainer.unbind();
        jq("#commonPopupContainer").unbind();
    };

    var createAdvansedFilter = function () {
        var filters =
                    [
                // Team member
                        {
                            type: "person",
                            id: "me_team_member",
                            title: ASC.Projects.Resources.ProjectsFilterResource.Me,
                            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.TeamMember + ":",
                            group: ASC.Projects.Resources.ProjectsFilterResource.TeamMember,
                            hashmask: "person/{0}",
                            groupby: "userid",
                            bydefault: { id: currentUserId }
                        },
                        {
                            type: "person",
                            id: "team_member",
                            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.TeamMember + ":",
                            title: ASC.Projects.Resources.ProjectsFilterResource.OtherUsers,
                            group: ASC.Projects.Resources.ProjectsFilterResource.TeamMember,
                            hashmask: "person/{0}",
                            groupby: "userid"
                        },
                // Project manager
                        {
                            type: "person",
                            id: "me_project_manager",
                            title: ASC.Projects.Resources.ProjectsFilterResource.Me,
                            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ProjectMenager + ":",
                            group: ASC.Projects.Resources.ProjectsFilterResource.ProjectMenager,
                            hashmask: "person/{0}",
                            groupby: "managerid",
                            bydefault: { id: currentUserId }
                        },
                        {
                            type: "person",
                            id: "project_manager",
                            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ProjectMenager + ":",
                            title: ASC.Projects.Resources.ProjectsFilterResource.OtherUsers,
                            group: ASC.Projects.Resources.ProjectsFilterResource.ProjectMenager,
                            hashmask: "person/{0}",
                            groupby: "managerid"
                        },
                //Status
                        {
                            type: "combobox",
                            id: "open",
                            title: ASC.Projects.Resources.ProjectsFilterResource.StatusOpenProject,
                            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByStatus + ":",
                            group: ASC.Projects.Resources.ProjectsFilterResource.ByStatus,
                            hashmask: "combobox/{0}",
                            groupby: "status",
                            options:
                                    [
                                        { value: "open", title: ASC.Projects.Resources.ProjectsFilterResource.StatusOpenProject, def: true },
                                        { value: "paused", title: ASC.Projects.Resources.ProjectsFilterResource.StatusSuspend },
                                        { value: "closed", title: ASC.Projects.Resources.ProjectsFilterResource.StatusClosedProject }
                                    ]
                        },
                        {
                            type: "combobox",
                            id: "paused",
                            title: ASC.Projects.Resources.ProjectsFilterResource.StatusSuspend,
                            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByStatus + ":",
                            group: ASC.Projects.Resources.ProjectsFilterResource.ByStatus,
                            hashmask: "combobox/{0}",
                            groupby: "status",
                            options:
                                [
                                    { value: "open", title: ASC.Projects.Resources.ProjectsFilterResource.StatusOpenProject },
                                    { value: "paused", title: ASC.Projects.Resources.ProjectsFilterResource.StatusSuspend, def: true },
                                    { value: "closed", title: ASC.Projects.Resources.ProjectsFilterResource.StatusClosedProject }
                                ]
                        },
                        {
                            type: "combobox",
                            id: "closed",
                            title: ASC.Projects.Resources.ProjectsFilterResource.StatusClosedProject,
                            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByStatus + ":",
                            group: ASC.Projects.Resources.ProjectsFilterResource.ByStatus,
                            hashmask: "combobox/{0}",
                            groupby: "status",
                            options:
                                [
                                    { value: "open", title: ASC.Projects.Resources.ProjectsFilterResource.StatusOpenProject },
                                    { value: "paused", title: ASC.Projects.Resources.ProjectsFilterResource.StatusSuspend },
                                    { value: "closed", title: ASC.Projects.Resources.ProjectsFilterResource.StatusClosedProject, def: true }
                                ]
                        },
                // Other
                        {
                            type: "flag",
                            id: "followed",
                            title: ASC.Projects.Resources.ProjectsFilterResource.FollowProjects,
                            group: ASC.Projects.Resources.ProjectsFilterResource.Other,
                            hashmask: "followed"
                        }
                    ],
                sorters =
                [
                    { id: "title", title: ASC.Projects.Resources.ProjectsFilterResource.ByTitle, sortOrder: "ascending", def: true },
                    { id: "create_on", title: ASC.Projects.Resources.ProjectsFilterResource.ByCreateDate, sortOrder: "descending" }
                ];
        var tags = ASC.Projects.ProjectsAdvansedFilter.getTagsForFilter();
        
        if (tags.length) {
            filters.push({
                type: "combobox",
                id: "tag",
                title: ASC.Projects.Resources.ProjectsFilterResource.ByTag,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.Tag + ":",
                group: ASC.Projects.Resources.ProjectsFilterResource.Other,
                hashmask: "combobox/{0}",
                options: ASC.Projects.ProjectsAdvansedFilter.getTagsForFilter(),
                defaulttitle: ASC.Projects.Resources.ProjectsFilterResource.Select
            });
        }

        ASC.Projects.AllProject.filters = filters;
        ASC.Projects.AllProject.sorters = sorters;
        ASC.Projects.AllProject.colCount = 2;

        ASC.Projects.ProjectsAdvansedFilter.init(ASC.Projects.AllProject);

        if (!isSimpleView) {
            //filter
            ASC.Projects.ProjectsAdvansedFilter.filter.one("adv-ready", function () {
                var projectAdvansedFilterContainer = jq("#ProjectsAdvansedFilter .advansed-filter-list");
                projectAdvansedFilterContainer.find("li[data-id='me_team_member'] .inner-text").trackEvent(ga_Categories.projects, ga_Actions.filterClick, 'me_team_member');
                projectAdvansedFilterContainer.find("li[data-id='team_member'] .inner-text").trackEvent(ga_Categories.projects, ga_Actions.filterClick, 'team_member');
                projectAdvansedFilterContainer.find("li[data-id='me_project_manager'] .inner-text").trackEvent(ga_Categories.projects, ga_Actions.filterClick, 'me_project_manager');
                projectAdvansedFilterContainer.find("li[data-id='project_manager'] .inner-text").trackEvent(ga_Categories.projects, ga_Actions.filterClick, 'project_manager');
                projectAdvansedFilterContainer.find("li[data-id='open'] .inner-text").trackEvent(ga_Categories.projects, ga_Actions.filterClick, 'open');
                projectAdvansedFilterContainer.find("li[data-id='closed'] .inner-text").trackEvent(ga_Categories.projects, ga_Actions.filterClick, 'closed');
                projectAdvansedFilterContainer.find("li[data-id='paused'] .inner-text").trackEvent(ga_Categories.projects, ga_Actions.filterClick, 'paused');
                projectAdvansedFilterContainer.find("li[data-id='followed'] .inner-text").trackEvent(ga_Categories.projects, ga_Actions.filterClick, 'followed');
                projectAdvansedFilterContainer.find("li[data-id='tag'] .inner-text").trackEvent(ga_Categories.projects, ga_Actions.filterClick, 'tag');
                jq("#ProjectsAdvansedFilter .btn-toggle-sorter").trackEvent(ga_Categories.projects, ga_Actions.filterClick, 'sort');
                jq("#ProjectsAdvansedFilter .advansed-filter-input").trackEvent(ga_Categories.projects, ga_Actions.filterClick, "search_text", "enter");
            });
        }
    };


    var renderListProjects = function (listProjects) {
        onGetListProject({}, listProjects);
    };

    var addProjectsToSimpleList = function (projectItem) {
        $projectsTableBody = projectsTable.find('tbody');

        clearTimeout(projDescribeTimeout);
        overProjDescrPanel = false;
        hideDescrPanel();

        projectItem = getProjTmpl(projectItem);
        jq.tmpl("projects_projectTmpl", projectItem).prependTo($projectsTableBody);
        projectsTable.show();
    };


    var hideDescrPanel = function () {
        setTimeout(function () {
            if (!overProjDescrPanel) jq('#projectDescrPanel').hide(100);
        }, 200);
    };

    var getProjTmpl = function (proj) {
        var projTmpl = {};
        projTmpl.title = proj.title;
        projTmpl.id = proj.id;
        projTmpl.created = proj.displayDateCrtdate;
        projTmpl.createdBy = proj.createdBy ? proj.createdBy.displayName : "";
        projTmpl.projLink = linkViewProject + projTmpl.id;
        projTmpl.description = proj.description;
        projTmpl.milestones = proj.milestoneCount;
        projTmpl.linkMilest = linkViewMilestones + projTmpl.id + '#sortBy=deadline&sortOrder=ascending&status=open';
        projTmpl.tasks = proj.taskCount;
        projTmpl.linkTasks = linkViewTasks + projTmpl.id + '#sortBy=deadline&sortOrder=ascending&status=open';
        projTmpl.responsible = proj.responsible.displayName;
        projTmpl.responsibleId = proj.responsible.id;
        projTmpl.participants = proj.participantCount ? proj.participantCount - 1 : "";
        projTmpl.linkParticip = linkViewParticipants + projTmpl.id;
        projTmpl.privateProj = proj.isPrivate;
        projTmpl.canEdit = proj.canEdit;
        projTmpl.isSimpleView = isSimpleView;
        projTmpl.canLinkContact = proj.canLinkContact;

        if (proj.status == 0) {
            projTmpl.status = 'open';
        }
        else {
            projTmpl.status = 'closed';
        }
        if (proj.status == 2) projTmpl.status = 'paused';

        return projTmpl;
    };

    var showProjDescribePanel = function (targetObject) {
        var x = jq(targetObject).offset().left + 10;
        var y = jq(targetObject).offset().top + 20;
        jq('#projectDescrPanel').css({ left: x, top: y });
        jq('#projectDescrPanel').show();

        jq('body').click(function (event) {
            var elt = (event.target) ? event.target : event.srcElement;
            var isHide = true;
            if (jq(elt).is('[id="#projectDescrPanel"]')) {
                isHide = false;
            }

            if (isHide)
                jq(elt).parents().each(function () {
                    if (jq(this).is('[id="#projectDescrPanel"]')) {
                        isHide = false;
                        return false;
                    }
                });

            if (isHide) {
                jq('.studio-action-panel').hide();
            }
        });
    };

    var getData = function (filter) {
        filter.Count = ASC.Projects.AllProject.entryCountOnPage;
        filter.StartIndex = ASC.Projects.AllProject.entryCountOnPage * ASC.Projects.AllProject.currentPage;

        if (filter.StartIndex > filterProjCount) {
            filter.StartIndex = 0;
            ASC.Projects.AllProject.currentPage = 1;
        }

        Teamlab.getPrjProjects({}, { filter: filter });
    };

    var onGetListProject = function (params, listProj) {
        $projectsTableBody = projectsTable.find('tbody');
        if (typeof (isSimpleView) != "undefined" && isSimpleView == false) {
            ASC.Projects.Common.clearTables();
            filterProjCount = params.__total != undefined ? params.__total : 0;
            ASC.Projects.Common.updatePageNavigator(ASC.Projects.AllProject, filterProjCount);
        }

        clearTimeout(projDescribeTimeout);
        overProjDescrPanel = false;
        hideDescrPanel();

        var listTmplProj = new Array(),
            projTmpl;

        $projectsTableBody.empty();

        if (listProj.length != 0) {
            for (var i = 0; i < listProj.length; i++) {
                projTmpl = getProjTmpl(listProj[i]);
                listTmplProj.push(projTmpl);
            }
            $projectsTableBody.append(jq.tmpl("projects_projectTmpl", listTmplProj));

            jq("#prjEmptyScreenForFilter").hide();
            projectsTable.show();
        }
        else {
            jq('#tableForNavigation').hide();
            projectsTable.hide();
            if (ASC.Projects.ProjectsAdvansedFilter.baseFilter) {
                jq("#prjEmptyScreenForFilter").hide();
                jq("#emptyListProjects").show();
                jq('#ProjectsAdvansedFilter').hide();
            } else {
                jq("#prjEmptyScreenForFilter").show();
                jq("#emptyListProjects").hide();
                jq('#ProjectsAdvansedFilter').show();
            }
        }
        LoadingBanner.hideLoading();
    };

    var changeStatus = function (item) {
        if (!jq(item).hasClass('current')) {
            var projId = jq(item).parents('#' + statusListObject.listId).attr('objid').split('_')[1];
            var newStatus = jq(item).attr('class').split(" ")[0];
            if (newStatus == 'closed') {
                var flag = showQuestionWindow(projId);
                if (flag) return;
            }
            var newtitle = jq(item).text().trim();
            var data = { id: projId, status: newStatus };
            Teamlab.updatePrjProjectStatus({}, projId, data);

            changeCboxStatus(newStatus, projId, newtitle);
        }
    };

    var changeCboxStatus = function (status, projId, title) {
        jq('#statusCombobox_' + projId + ' span:first-child').attr('class', status).attr('title', title);
        if (status != 'open') {
            jq('tr#' + projId).addClass('noActiveProj');
        } else {
            jq('tr#' + projId).removeClass('noActiveProj');
        }
        projectsTable.find("tr").removeClass("openList");
    };

    var showQuestionWindow = function (projId) {
        var proj = jq('#tableListProjects tr#' + projId),
            tasks = proj.find('td.taskCount').text().trim();

        if (!tasks.length) {
            var milestones = jq.trim(proj.find('td.taskCount').data("milestones"));
            if (milestones.length && milestones != 0) {
                ASC.Projects.Common.showCommonPopup("projects_projectOpenMilestoneWarning", 400, 200, 0);
                var milUrl = linkViewMilestones + projId + '#sortBy=deadline&sortOrder=ascending&status=open';
                jq('#linkToMilestines').attr('href', milUrl);
            }
            else {
                return false;
            }
        } else {
            ASC.Projects.Common.showCommonPopup("projects_projectOpenTaskWarning", 400, 200, 0);
            var tasksUrl = linkViewTasks + projId + '#sortBy=deadline&sortOrder=ascending&status=open';
            jq('#linkToTasks').attr('href', tasksUrl);
        }
        return true;
    };


    var showListStatus = function (panelId, obj, event) {
        var objid = '';
        var x, y, statusList;
        objid = jq(obj).attr('id');

        x = jq(obj).offset().left + 9;
        y = jq(obj).offset().top + 25;
        statusList = jq('#' + statusListObject.listId);

        statusList.attr('objid', objid);
        jq(obj).parents('tr').addClass('openList');

        statusList.css({ left: x, top: y });

        var status = jq(obj).children('span').attr('class');
        statusList.find('li').show().removeClass('current');
        switch (status) {
            case 'closed':
                {
                    statusList.find('.closed').addClass('current');
                    statusList.find('.paused').hide();
                    break;
                }
            case 'paused':
                {
                    statusList.find('.paused').addClass('current');
                    break;
                }
            default:
                {
                    statusList.find('.open').addClass('current');
                    break;
                }
        }

        statusList.show();

        jq('body').click(function (event) {
            var elt = (event.target) ? event.target : event.srcElement;
            var isHide = true;
            if (jq(elt).is('[id="' + statusListObject.listId + '"]')) {
                isHide = false;
            }

            if (isHide)
                jq(elt).parents().each(function () {
                    if (jq(this).is('[id="' + statusListObject.listId + '"]')) {
                        isHide = false;
                        return false;
                    }
                });

            if (isHide) {
                statusList.hide();
                projectsTable.find("tr").removeClass('openList');
            }
        });
    };

    return {
        init: init,
        renderListProjects: renderListProjects,
        addProjectsToSimpleList: addProjectsToSimpleList,
        getData: getData,
        createAdvansedFilter: createAdvansedFilter,
        unbindListEvents: unbindListEvents,
        basePath: 'sortBy=create_on&sortOrder=ascending'
    };
})(jQuery);

