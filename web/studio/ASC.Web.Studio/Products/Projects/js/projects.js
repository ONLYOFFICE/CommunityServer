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
        prjEmptyScreenForFilter = jq("#prjEmptyScreenForFilter"),
        emptyListProjects = jq("#emptyListProjects"),
        projectsAdvansedFilter,
        projectsTable = null,
        describePanel = null,
            self;

    var currentUserId;

    var isSimpleView;
    var filterProjCount = 0;

    // object for list statuses
    var statusListObject = { listId: "projectsStatusList" };

    var initActionPanels = function () {
        if (!describePanel) {
            self.$commonListContainer.append(jq.tmpl("projects_panelFrame", { panelId: "projectDescrPanel" })); // description panel
            describePanel = jq("#projectDescrPanel");
        }
        if (isSimpleView) return;
        var resources = ASC.Projects.Resources.ProjectsJSResource;
        statusListObject.statuses = [
            { cssClass: "open", text: resources.StatusOpenProject },
            { cssClass: "paused", text: resources.StatusSuspendProject },
            { cssClass: "closed", text: resources.StatusClosedProject }
        ];
        jq("#" + statusListObject.listId).remove();
        self.$commonListContainer.append(jq.tmpl("projects_statusChangePanel", statusListObject));
    };
    
    //filter Set
    var init = function (isSimpleViewFlag) {
        self = this;
        if (isInit === false) {
            isInit = true;
            self.cookiePagination = "projectsKeyForPagination";
        }
        
        
        self.isFirstLoad = true;
        self.showLoader();
        projectsTable = jq("#tableListProjects");

        isSimpleView = isSimpleViewFlag;
        currentUserId = Teamlab.profile.id;

        initActionPanels();
        
        if (!isSimpleView) {
            projectsAdvansedFilter = createAdvansedFilter();
            this.setDocumentTitle(ASC.Projects.Resources.ProjectsJSResource.ProjectsModule);
            this.checkElementNotFound(ASC.Projects.Resources.ProjectsJSResource.ProjectNotFound);
            
            ASC.Projects.PageNavigator.init(self);

            projectsTable.on('click', "td.responsible span.userLink", function () {
                var responsibleId = jq(this).attr('id');
                if (responsibleId != "4a515a15-d4d6-4b8e-828e-e0586f18f3a3") {
                    var path = jq.changeParamValue(ASC.Controls.AnchorController.getAnchor(), 'project_manager', responsibleId);
                    path = jq.removeParam('team_member', path);
                    ASC.Controls.AnchorController.move(path);
                }
            });

            // popup handlers
            self.$commonPopupContainer.on("click", ".gray", function () {
                jq.unblockUI();
                return false;
            });
            self.$commonListContainer.on("click", "#statusList .dropdown-item", function () {
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

        self.$commonListContainer.on('click', 'td.action .canEdit', function (event) {
            showListStatus(statusListObject.listId, this);
            return false;
        });        
    };

    var unbindListEvents = function () {
        if (!isInit) return;
        projectsTable.unbind();
        describePanel.unbind();
        self.$commonListContainer.unbind();
        self.$commonPopupContainer.unbind();
    };

    var createAdvansedFilter = function () {
        var resources = ASC.Projects.Resources.ProjectsFilterResource;
        if (typeof self.filters == "undefined") {
            var filters =
            [
                // Project manager
                {
                    type: "person",
                    id: "me_project_manager",
                    title: resources.Me,
                    filtertitle: resources.ProjectMenager + ":",
                    group: resources.ProjectMenager,
                    hashmask: "person/{0}",
                    groupby: "managerid",
                    bydefault: { id: currentUserId }
                },
                {
                    type: "person",
                    id: "project_manager",
                    filtertitle: resources.ProjectMenager + ":",
                    title: resources.OtherUsers,
                    group: resources.ProjectMenager,
                    hashmask: "person/{0}",
                    groupby: "managerid"
                },
                // Team member
                {
                    type: "person",
                    id: "me_team_member",
                    title: resources.Me,
                    filtertitle: resources.TeamMember + ":",
                    group: resources.TeamMember,
                    hashmask: "person/{0}",
                    groupby: "userid",
                    bydefault: { id: currentUserId }
                },
                {
                    type: "person",
                    id: "team_member",
                    filtertitle: resources.TeamMember + ":",
                    title: resources.OtherUsers,
                    group: resources.TeamMember,
                    hashmask: "person/{0}",
                    groupby: "userid"
                },
                {
                    type: "group",
                    id: "group",
                    title: resources.Groups,
                    filtertitle: resources.Group + ":",
                    group: resources.TeamMember,
                    hashmask: "group/{0}",
                    groupby: "userid"
                },
                //Status
                {
                    type: "combobox",
                    id: "open",
                    title: resources.StatusOpenProject,
                    filtertitle: resources.ByStatus + ":",
                    group: resources.ByStatus,
                    hashmask: "combobox/{0}",
                    groupby: "status",
                    options:
                    [
                        { value: "open", title: resources.StatusOpenProject, def: true },
                        { value: "paused", title: resources.StatusSuspend },
                        { value: "closed", title: resources.StatusClosedProject }
                    ]
                },
                {
                    type: "combobox",
                    id: "paused",
                    title: resources.StatusSuspend,
                    filtertitle: resources.ByStatus + ":",
                    group: resources.ByStatus,
                    hashmask: "combobox/{0}",
                    groupby: "status",
                    options:
                    [
                        { value: "open", title: resources.StatusOpenProject },
                        { value: "paused", title: resources.StatusSuspend, def: true },
                        { value: "closed", title: resources.StatusClosedProject }
                    ]
                },
                {
                    type: "combobox",
                    id: "closed",
                    title: resources.StatusClosedProject,
                    filtertitle: resources.ByStatus + ":",
                    group: resources.ByStatus,
                    hashmask: "combobox/{0}",
                    groupby: "status",
                    options:
                    [
                        { value: "open", title: resources.StatusOpenProject },
                        { value: "paused", title: resources.StatusSuspend },
                        { value: "closed", title: resources.StatusClosedProject, def: true }
                    ]
                },
                // Other
                {
                    type: "flag",
                    id: "followed",
                    title: resources.FollowProjects,
                    group: resources.Other,
                    hashmask: "followed"
                }
            ];

            var tags = ASC.Projects.ProjectsAdvansedFilter.getTagsForFilter();

            if (tags.length) {
                filters.push({
                    type: "combobox",
                    id: "tag",
                    title: resources.ByTag,
                    filtertitle: resources.Tag + ":",
                    group: resources.Other,
                    hashmask: "combobox/{0}",
                    options: tags,
                    defaulttitle: resources.Select
                });
            }
            self.filters = filters;
        }

        if (typeof self.sorters == "undefined") {
            var sorters =
            [
                { id: "title", title: resources.ByTitle, sortOrder: "ascending", def: true },
                { id: "create_on", title: resources.ByCreateDate, sortOrder: "descending" }
            ];
            self.sorters = sorters;
        }

        self.colCount = 2;

        return ASC.Projects.ProjectsAdvansedFilter.init(self);
    };


    var renderListProjects = function (listProjects) {
        onGetListProject({}, listProjects);
    };

    var addProjectsToSimpleList = function (projectItem) {
        $projectsTableBody = projectsTable.find('tbody');

        clearTimeout(projDescribeTimeout);
        overProjDescrPanel = false;
        

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
            if (jq(elt).is('[id="#projectDescrPanel"]') || jq(elt).parents('#projectDescrPanel').length) {
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
                describePanel.hide();
            }
        });
    };

    var getData = function () {
        self.showLoader();
        self.currentFilter.Count = ASC.Projects.PageNavigator.entryCountOnPage;
        self.currentFilter.StartIndex = ASC.Projects.PageNavigator.entryCountOnPage * ASC.Projects.PageNavigator.currentPage;
        Teamlab.getPrjProjects({}, { filter: self.currentFilter, success: onGetListProject });
    };

    var onGetListProject = function (params, listProj) {
        $projectsTableBody = projectsTable.find('tbody');

        if (typeof (isSimpleView) != "undefined" && isSimpleView === false) {
            filterProjCount = params.__total != undefined ? params.__total : 0;
        }
        
        clearTimeout(projDescribeTimeout);
        overProjDescrPanel = false;

        if (listProj.length != 0) {
            $projectsTableBody.html(jq.tmpl("projects_projectTmpl", listProj.map(getProjTmpl)));

            prjEmptyScreenForFilter.hide();
            projectsTable.show();
        }
        else {
            ASC.Projects.PageNavigator.hide();
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

        if (typeof (isSimpleView) != "undefined" && isSimpleView == false) {
            ASC.Projects.PageNavigator.update(filterProjCount);
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
    }, ASC.Projects.Base);
})(jQuery);

