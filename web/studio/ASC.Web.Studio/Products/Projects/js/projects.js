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
            if (!document.location.pathname.toLowerCase().endsWith("projects.aspx")) return;
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

        var statuses = [
            {
                title: projectsJSResource.StatusOpenProject,
                id: 0,
                handler: onChangeStatus.bind(null, 0)
            },
            {
                title: projectsJSResource.StatusSuspendProject,
                id: 2,
                handler: onChangeStatus.bind(null, 2)
            },
            {
                title: projectsJSResource.StatusClosedProject,
                id: 1,
                handler: onChangeStatus.bind(null, 1)
            }
        ];

        var currentStatusId = project.status;
        var currentStatus = statuses.find(function (item) { return item.id === currentStatusId });

        baseProjects.DescriptionTab
            .init()
            .push(resources.CommonResource.ProjectName, formatDescription(project.title))
            .push(projectResource.ProjectLeader, project.responsible.displayName)
            .push(resources.ProjectsFilterResource.ByCreateDate, project.displayDateCrtdate)
            .push(resources.CommonResource.Description, jq.linksParser(formatDescription(project.description)))
            .push(projectResource.Tags, formatDescription(tags))
            .setStatuses(statuses)
            .setCurrentStatus(currentStatus)
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
        currentListProjects,
        loadingBanner;

    var clickEvent = "click",
        clickProjectsInitEvent = "click.projectsInit",
        projectsTmplName = "projects_projectTmpl",
        teamlab;
    
    var init = function (isSimpleViewFlag) {
        isSimpleView = isSimpleViewFlag;

        if (isInit === false) {
            isInit = true;

            var res = ASC.Projects.Resources;
            teamlab = Teamlab;
            loadingBanner = LoadingBanner;

            $projectsTable = jq("#tableListProjects");
            var groupMenu;

            if (!isSimpleView) {
                var actions = [
                    {
                        id: "gaDelete",
                        title: res.CommonResource.Delete,
                        handler: gaRemoveHandler,
                        checker: function(project) {
                            return project.canDelete;
                        }
                    }
                ];
                var projectsFilterResource = res.ProjectsFilterResource;
                groupMenu = {
                    actions: actions,
                    getItemByCheckbox: getProjectByTarget,
                    getLineByCondition: function (condition) {
                        return currentListProjects
                            .filter(condition)
                            .map(function (item) {
                                return $projectsTableBody.find("tr[id=" + item.id + "]");
                            });
                    },
                    multiSelector: [
                    {
                        id: "gasOpen",
                        title: projectsFilterResource.StatusOpenProject,
                        condition: function (item) {
                            return item.status === 0;
                        }
                    },
                    {
                        id: "gasPaused",
                        title: projectsFilterResource.StatusSuspend,
                        condition: function (item) {
                            return item.status === 2;
                        }
                    },
                    {
                        id: "gasClosed",
                        title: projectsFilterResource.StatusClosedProject,
                        condition: function (item) {
                            return item.status === 1;
                        }
                    }]
                }
            }

            self = this;
            self.showOrHideData = self.showOrHideData.bind(self, {
                $container: $projectsTable.find("tbody"), 
                tmplName: projectsTmplName, 
                baseEmptyScreen: {
                    img: "projects",
                    header: res ? res.ProjectResource.EmptyListProjHeader : "",
                    description: res
                                    ? teamlab.profile.isVisitor
                                        ? res.ProjectResource.EmptyListProjDescribeVisitor
                                        : res.ProjectResource.EmptyListProjDescribe
                                    : "",
                    button: {
                        title: res ? res.ProjectResource.CreateFirstProject : "",
                        onclick: function () {
                            location.href = "Projects.aspx?action=add";
                        },
                        canCreate: function() {
                            return ASC.Projects.Master.CanCreateProject;
                        }
                    }
                },
                filterEmptyScreen: {
                    header: res ? res.CommonResource.Filter_NoProjects : "",
                    description: res ? res.ProjectResource.DescrEmptyListProjFilter : ""
                },
                groupMenu: groupMenu
            });

            self.getData = self.getData.bind(self, teamlab.getPrjProjects, onGetListProject);
        }

        
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
                    return "Projects.aspx?prjID=" + item.id + "#";
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

    function gaRemoveHandler(projectids) {
        self.showCommonPopup("projectsRemoveWarning", function () {
            teamlab.removePrjProjects({ projectids: projectids },
            {
                before: function () {
                    loadingBanner.displayLoading();
                },
                success: function (params, data) {
                    for (var i = 0; i < data.length; i++) {
                        onRemovePrj(data[i]);
                    }
                    self.showOrHideData(currentListProjects, filterProjCount);
                    loadingBanner.hideLoading();
                    baseObject.Common.displayInfoPanel(prjResources.ProjectsRemoved);
                }
            });
            jq.unblockUI();
        });
        return false;
    }

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

    var moduleLocationPath = StudioManager.getLocationPathToModule("Projects");

    function getProjTmpl(proj) {
        return jq.extend(proj, {
            projLink: jq.format('{0}Tasks.aspx?prjID={1}', moduleLocationPath, proj.id),
            linkMilest: jq.format('{0}Milestones.aspx?prjID={1}#sortBy=deadline&sortOrder=ascending&status=open', moduleLocationPath, proj.id),
            linkTasks: jq.format('{0}Tasks.aspx?prjID={1}#sortBy=deadline&sortOrder=ascending&status=open', moduleLocationPath, proj.id),
            participants: proj.participantCount ? proj.participantCount - 1 : "",
            linkParticip: jq.format('{0}ProjectTeam.aspx?prjID={1}', moduleLocationPath, proj.id),
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

    function onRemovePrj(project) {
        var projectId = project.id;
        var removedProject = $projectsTableBody.find("tr[id=" + projectId + "]");
        removedProject.yellowFade();
        removedProject.remove();

        filterProjCount--;
        currentListProjects = currentListProjects.filter(function (item) { return item.id !== project.id });
    }

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
                if (status == 1 || status == 2) {
                    teamlab.getPrjTeam({}, id,
                        function (params, team) {
                            teamlab.removeCaldavProjectCalendar(id, jq.map(team, function (user) { return user.id; }));
                        }
                    );
                } else if (status == 0) {
                    teamlab.getCalendarCaldavUrl("Project_" + id);
                }
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
        var project = typeof projId === "number" ? getProjTmpl(getProjectById(projId)) : getProjTmpl(projId);

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

