/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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

ASC.Projects.Description = (function () {
    var project, teamlab, handler, allProjects;

    function init() {
        teamlab = Teamlab;
        allProjects = ASC.Projects.AllProject;

        if (project) {
            show();
            return;
        }

        handler = teamlab.bind(teamlab.events.getPrjProject, function (params, currentproject) {
            if (!document.location.pathname.endsWith("projects.aspx")) return;
            project = currentproject;
            show();
        });
    }
    function formatDescription(descr) {
        var formatDescr = descr.replace(/</ig, '&lt;').replace(/>/ig, '&gt;').replace(/\n/ig, '<br/>');
        return formatDescr.replace('&amp;', '&');
    };

    function show() {
        var baseProjects = ASC.Projects,
            resources = baseProjects.Resources,
            projectResource = resources.ProjectResource,
            projectsJSResource = resources.ProjectsJSResource;

        baseProjects.ProjectsAdvansedFilter.hide();
        baseProjects.Base.clearTables();

        function cancelChangeStatus() {
            ASC.Projects.DescriptionTab.resetStatus(project.status);
            jq.unblockUI();
        }

        function onChangeStatus(status) {
            if (status === 1 && allProjects.showQuestionWindow(project, cancelChangeStatus)) {
                return;
            }
            teamlab.updatePrjProjectStatus({},
                project.id,
                { id: project.id, status: status },
                {
                    success: function() {
                        location.reload();
                    }
                });

        }

        var tags = project.tags.length ? project.tags.reduce(function (previousValue, currentValue) {
                return previousValue + ", " + currentValue;
            }) : "";

        baseProjects.DescriptionTab
            .init()
            .push(resources.CommonResource.ProjectName, formatDescription(project.title))
            .push(projectResource.ProjectLeader, project.responsible.displayName)
            .push(resources.ProjectsFilterResource.ByCreateDate, project.displayDateCrtdate)
            .push(resources.CommonResource.Description, jq.linksParser(formatDescription(project.description)))
            .push(projectResource.Tags, tags)
            .pushStatus(projectsJSResource.StatusOpenProject, 0, onChangeStatus.bind(null, 0))
            .pushStatus(projectsJSResource.StatusSuspendProject, 2, onChangeStatus.bind(null, 2))
            .pushStatus(projectsJSResource.StatusClosedProject, 1, onChangeStatus.bind(null, 1))
            .setCurrentStatus(project.status)
            .setStatusRight(project.canEdit)
            .tmpl();

        jq("#descriptionTab").show();
    }

    function unbindListEvents() {
        teamlab.unbind(handler);
        project = null;
    }

    return {
        init: init,
        unbindListEvents: unbindListEvents
    };
})();

ASC.Projects.AllProject = (function () {
    var isInit = false,
        projectsAdvansedFilter,
        $projectsTable = null,
        self,
        baseObject,
        resources,
        prjResources,
        $projectsTableBody,
        currentUserId,
        isSimpleView,
        filterProjCount = 0,
        currentListProjects;

    var clickEvent = "click",
        clickProjectsInitEvent = "click.projectsInit",
        projectsTmplName = "projects_projectTmpl",
        teamlab;
    
    var init = function (isSimpleViewFlag) {
        if (isInit === false) {
            isInit = true;

            var res = ASC.Projects.Resources;
            teamlab = Teamlab;

            $projectsTable = jq("#tableListProjects");

            self = this;
            self.showOrHideData = self.showOrHideData.bind(self, {
                $container: $projectsTable.find("tbody"), 
                tmplName: projectsTmplName, 
                baseEmptyScreen: {
                    img: "projects",
                    header: res ? res.ProjectResource.EmptyListProjHeader : "",
                    description: res ? res.ProjectResource.EmptyListProjDescribe : "",
                    button: {
                        title: res ? res.ProjectResource.CreateFirstProject : "",
                        onclick: function () {
                            location.href = "projects.aspx?action=add";
                        },
                        canCreate: function() {
                            return ASC.Projects.Master.CanCreateProject;
                        }
                    }
                },
                filterEmptyScreen: {
                    header: res ? res.CommonResource.Filter_NoProjects : "",
                    description: res ? res.ProjectResource.DescrEmptyListProjFilter : ""
                }
            });

            self.getData = self.getData.bind(self, teamlab.getPrjProjects, onGetListProject);
        }

        isSimpleView = isSimpleViewFlag;
        currentUserId = teamlab.profile.id;
        
        if (!isSimpleView) {
            baseObject = ASC.Projects,
                resources = baseObject.Resources.ProjectsFilterResource,
                prjResources = baseObject.Resources.ProjectsJSResource;

            projectsAdvansedFilter = baseObject.ProjectsAdvansedFilter;
            projectsAdvansedFilter.createAdvansedFilterForProjects(self);

            self.baseInit({
                moduleTitle: prjResources.ProjectsModule,
                elementNotFoundError: prjResources.ProjectNotFound
            },
            {
                pagination: "projectsKeyForPagination"
            },
            {
                handler: changeStatus,
                getItem: getProjectByTarget,
                statuses: [
                    { cssClass: "open", text: prjResources.StatusOpenProject, id: 0 },
                    { cssClass: "paused", text: prjResources.StatusSuspendProject, id: 2 },
                    { cssClass: "closed", text: prjResources.StatusClosedProject, id: 1 }
                ]
            },
            undefined,
            [
                ASC.Projects.Event(teamlab.events.addPrjTask, onAddTask)
            ],
            {
                getItem: getProjectByTarget,
                selector: '.nameProject a',
                getLink: function (item) {
                    return "projects.aspx?prjID=" + item.id + "#";
                }
            });

            $projectsTable.on(clickEvent, ".nameProject a", baseObject.Common.goToWithoutReload);
            $projectsTable.on(clickEvent, ".taskCount a", baseObject.Common.goToWithoutReload);
            $projectsTable.on(clickEvent, ".responsible .participants", baseObject.Common.goToWithoutReload);

            $projectsTable.on(clickEvent, "td.responsible span.userLink", function () {
                var $self = jq(this);
                if ($self.hasClass("not-action")) return;
                var project = getProjectById($self.parents("#tableListProjects tr").attr("id"));
                ASC.Projects.ProjectsAdvansedFilter.addUser('project_manager', project.responsible.id, ['team_member']);
            });
        }
    };

    var unbindListEvents = function () {
        if (!isInit) return;
        $projectsTable.unbind();
        self.unbindEvents();
        jq("body").off(clickProjectsInitEvent);
    };

    var renderListProjects = function (listProjects) {
        onGetListProject({}, listProjects);
    };

    var addProjectsToSimpleList = function (projectItem) {
        $projectsTableBody = $projectsTable.find('tbody');
        projectItem = getProjTmpl(projectItem);
        jq.tmpl(projectsTmplName, projectItem).prependTo($projectsTableBody);
        $projectsTable.show();
    };

    var moduleLocationPath = StudioManager.getLocationPathToModule("projects");

    function getProjTmpl(proj) {
        return jq.extend(proj, {
            projLink: jq.format('{0}tasks.aspx?prjID={1}', moduleLocationPath, proj.id),
            linkMilest: jq.format('{0}milestones.aspx?prjID={1}#sortBy=deadline&sortOrder=ascending&status=open', moduleLocationPath, proj.id),
            linkTasks: jq.format('{0}tasks.aspx?prjID={1}#sortBy=deadline&sortOrder=ascending&status=open', moduleLocationPath, proj.id),
            participants: proj.participantCount ? proj.participantCount - 1 : "",
            linkParticip: jq.format('{0}projectteam.aspx?prjID={1}', moduleLocationPath, proj.id),
            isSimpleView: isSimpleView || false
        });
    };

    function onGetListProject(params, listProj) {
        currentListProjects = listProj;
        $projectsTableBody = $projectsTable.find('tbody');

        if (!isSimpleView) {
            filterProjCount = params.__total != undefined ? params.__total : 0;
        }

        self.showOrHideData(currentListProjects.map(getProjTmpl), filterProjCount);
    };

    function onAddTask(params, task) {
        var project = getProjectById(task.projectOwner.id);
        var $project = getProjectItem(task.projectOwner.id);

        if (!project || !$project.length) return;

        project.taskCount++;
        $project.find("td.taskCount a").text(project.taskCount);
    }

    function changeStatus(id, status) {
        if (status === 1 && showQuestionWindow(id)) {
            return;
        }
        var data = { id: id, status: status };
        teamlab.updatePrjProjectStatus({}, id, data, {
            success: function (params, project) {
                setProject(project);
                changeCboxStatus(project);
            }
        });
    };

    function changeCboxStatus(project) {
        var $project = $projectsTable.find('tr#' + project.id);

        $project.find('span:first-child').attr('class', ASC.Projects.StatusList.getById(project.status).cssClass);
        if (project.isPrivate) {
            $project.find('span:first-child').addClass('private');
        }

        if (project.status !== 0) {
            $project.addClass('noActiveProj');
            $project.find(".linkHeaderMedium").addClass("gray-text");
        } else {
            $project.removeClass('noActiveProj');
            $project.find(".linkHeaderMedium").removeClass("gray-text");
        }
        $project.removeClass("openList");
    };

    function showQuestionWindow(projId, cancel) {
        self = typeof self === "undefined" ? this : self;
        var project = typeof projId === "number" ? getProjectById(projId) : getProjTmpl(projId);

        if (project.taskCount) {
            self.showCommonPopup("projectOpenTaskWarning", function () {
                location.href = project.linkTasks;
            }, cancel);
            return true;
        }
        if (project.milestoneCount) {
            self.showCommonPopup("projectOpenMilestoneWarning", function () {
                location.href = project.linkMilest;
            }, cancel);
            return true;
        }

        return false;
    };

    function getProjectById(id) {
        return currentListProjects.find(function(item) { return item.id == id });
    };

    function getProjectByTarget($targetObject) {
        return getProjectById(jq($targetObject).parents("#tableListProjects tr").attr("id"));
    }

    function getProjectItem(id) {
        return $projectsTable.find(jq.format("tr#{0}", id));
    }

    function setProject(project) {
        for (var i = 0, max = currentListProjects.length; i < max; i++) {
            if (currentListProjects[i].id === project.id) {
                currentListProjects[i] = project;
                break;
            }
        }
    };

    return jq.extend({
        addProjectsToSimpleList: addProjectsToSimpleList,
        basePath: 'sortBy=create_on&sortOrder=ascending',
        init: init,
        renderListProjects: renderListProjects,
        unbindListEvents: unbindListEvents,
        showQuestionWindow: showQuestionWindow
    }, ASC.Projects.Base);
})(jQuery);

