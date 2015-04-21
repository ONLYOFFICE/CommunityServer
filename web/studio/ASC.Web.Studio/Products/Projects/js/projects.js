/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
        prjEmptyScreenForFilter = jq("#prjEmptyScreenForFilter"),
        emptyListProjects = jq("#emptyListProjects"),
        projectsAdvansedFilter,
        filterContainer = jq('#filterContainer'),
        projectsTable = null,
        describePanel = null;

    var currentUserId;

    var isSimpleView;
    var filterProjCount = 0;

    // object for list statuses
    var statusListObject = { listId: "projectsStatusList" };

    var initActionPanels = function () {
        if (!describePanel) {
            commonListContainer.append(jq.tmpl("projects_panelFrame", { panelId: "projectDescrPanel"})); // description panel
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
    
    var self;
    
    //filter Set
    var init = function (isSimpleViewFlag) {
        if (isInit === false) {
            isInit = true;
            Teamlab.bind(Teamlab.events.getPrjProjects, onGetListProject);
        }
        self = this;
        self.isFirstLoad = true;
        jq(".mainPageContent").children(".loader-page").show();

        projectsTable = jq("#tableListProjects");

        isSimpleView = isSimpleViewFlag;
        currentUserId = Teamlab.profile.id;

        initActionPanels();
        
        if (!isSimpleView) {
            projectsAdvansedFilter = createAdvansedFilter();
            this.setDocumentTitle(ASC.Projects.Resources.ProjectsJSResource.ProjectsModule);
            this.checkElementNotFound(ASC.Projects.Resources.ProjectsJSResource.ProjectNotFound);

            self.showLoader();
            //page navigator
            this.initPageNavigator("projectsKeyForPagination");

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
            self.changeCountOfRows(this.value);
        });

        jq('body').on("click.projectsInit", function (event) {
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

        self.filters = filters;
        self.sorters = sorters;
        self.colCount = 2;

        var filter = ASC.Projects.ProjectsAdvansedFilter.init(self);

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
        
        return filter;
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
            if (!overProjDescrPanel) describePanel.hide(100);
        }, 200);
    };

    var getProjTmpl = function (proj) {
        return {
            title: proj.title,
            id: proj.id,
            created: proj.displayDateCrtdate,
            createdBy: proj.createdBy ? proj.createdBy.displayName : "",
            projLink: linkViewProject + proj.id,
            description: proj.description,
            milestones: proj.milestoneCount,
            linkMilest: linkViewMilestones + proj.id + '#sortBy=deadline&sortOrder=ascending&status=open',
            tasks: proj.taskCount,
            linkTasks: linkViewTasks + proj.id + '#sortBy=deadline&sortOrder=ascending&status=open',
            responsible: proj.responsible.displayName,
            responsibleId: proj.responsible.id,
            participants: proj.participantCount ? proj.participantCount - 1 : "",
            linkParticip: linkViewParticipants + proj.id,
            privateProj: proj.isPrivate,
            canEdit: proj.canEdit,
            isSimpleView: isSimpleView,
            canLinkContact: proj.canLinkContact,
            status: proj.status == 0 ? 'open' : (proj.status == 2 ? 'paused' :'closed')
        };
    };

    var showProjDescribePanel = function (targetObject) {
        var x = jq(targetObject).offset().left + 10;
        var y = jq(targetObject).offset().top + 20;
        describePanel.css({ left: x, top: y });
        describePanel.show();

        jq('body')
            .off("click.projectsShowProjDescribePanel")
            .on("click.projectsShowProjDescribePanel", function (event) {
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

    var getData = function () {
        self.showLoader();
        self.currentFilter.Count = self.entryCountOnPage;
        self.currentFilter.StartIndex = self.entryCountOnPage * self.currentPage;

        Teamlab.getPrjProjects({}, { filter: self.currentFilter });
    };

    var onGetListProject = function (params, listProj) {
        $projectsTableBody = projectsTable.find('tbody');
        if (typeof (isSimpleView) != "undefined" && isSimpleView == false) {
            self.clearTables();
            filterProjCount = params.__total != undefined ? params.__total : 0;
        }

        clearTimeout(projDescribeTimeout);
        overProjDescrPanel = false;
        hideDescrPanel();

        $projectsTableBody.empty();

        if (listProj.length != 0) {
            $projectsTableBody.append(jq.tmpl("projects_projectTmpl", listProj.map(getProjTmpl)));

            prjEmptyScreenForFilter.hide();
            projectsTable.show();
        }
        else {
            jq('#tableForNavigation').hide();
            projectsTable.hide();
            if (ASC.Projects.ProjectsAdvansedFilter.baseFilter) {
                prjEmptyScreenForFilter.hide();
                emptyListProjects.show();
                projectsAdvansedFilter.hide();
            } else {
                prjEmptyScreenForFilter.show();
                emptyListProjects.hide();
                projectsAdvansedFilter.show();
            }
        }
        if (typeof(isSimpleView) != "undefined" && isSimpleView == false) {
            self.updatePageNavigator(filterProjCount);
            self.hideLoader();
        }

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
                self.showCommonPopup("projects_projectOpenMilestoneWarning", 400, 200, 0);
                var milUrl = linkViewMilestones + projId + '#sortBy=deadline&sortOrder=ascending&status=open';
                jq('#linkToMilestines').attr('href', milUrl);
            }
            else {
                return false;
            }
        } else {
            self.showCommonPopup("projects_projectOpenTaskWarning", 400, 200, 0);
            var tasksUrl = linkViewTasks + projId + '#sortBy=deadline&sortOrder=ascending&status=open';
            jq('#linkToTasks').attr('href', tasksUrl);
        }
        return true;
    };


    var showListStatus = function (panelId, obj, event) {
        var objid = '';
        var x, y, statusList;
        objid = jq(obj).attr('id');

        x = jq(obj).offset().left;
        y = jq(obj).offset().top + 28;
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

        jq('body').off("click.projectsShowListStatus");
        jq('body').on("click.projectsShowListStatus", function (event) {
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

    return jq.extend({
        init: init,
        renderListProjects: renderListProjects,
        addProjectsToSimpleList: addProjectsToSimpleList,
        getData: getData,
        createAdvansedFilter: createAdvansedFilter,
        unbindListEvents: unbindListEvents,
        basePath: 'sortBy=create_on&sortOrder=ascending'
    }, ASC.Projects.Common);
})(jQuery);

