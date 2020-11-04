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


ASC.Projects.AllMilestones = (function () {
    var baseObject = ASC.Projects,
        resources = baseObject.Resources,
        projectsJsResource = resources.ProjectsJSResource,
        filter = baseObject.ProjectsAdvansedFilter,
        pageNavigator = baseObject.PageNavigator,
        common = baseObject.Common,
        milestoneAction = baseObject.MilestoneAction,
        currentProject;


    var // cache DOM elements
        $milestoneList,
        $milestoneListBody,
        $emptyListMilestone;

    var isInit = false,
        currentProjectId,
        filterMilestoneCount = 0,
        currentUserId,
        self,
        currentMilestonesList = [];

    var clickEventName = "click",
        clickMilestoneInit = clickEventName + ".milestonesInit",
        projectsMilestoneTemplateName = "projects_milestoneTemplate",
        entityMenuClassName = ".entity-menu";
    var teamlab, loadingBanner;

    var init = function () {
        teamlab = Teamlab;
        loadingBanner = LoadingBanner;
        currentUserId = teamlab.profile.id;
        $milestoneList = jq('#milestonesList');
        $milestoneListBody = $milestoneList.find("tbody");
        $emptyListMilestone = jq("#emptyListMilestone");
        currentProjectId = jq.getURLParam('prjID');

        if (isInit === false) {
            isInit = true;
            self = this;

            function canCreateMilestone(prj) {
                return prj.canCreateMilestone;
            }

            var actions = [
                {
                    id: "gaDelete",
                    title: resources.CommonResource.Delete,
                    handler: gaRemoveHandler,
                    checker: function(milestone) {
                        return milestone.canDelete;
                    }
                }
            ];

            self.showOrHideData = self.showOrHideData.bind(self, {
                $container: $milestoneList.find("tbody"),
                tmplName: projectsMilestoneTemplateName,
                baseEmptyScreen: {
                    img: "milestones",
                    header: resources.MilestoneResource.MilestoneNotFound_Header,
                    description: teamlab.profile.isVisitor
                                    ? resources.MilestoneResource.MilestonesMarkMajorTimestampsVisitor
                                    : resources.MilestoneResource.MilestonesMarkMajorTimestamps,
                    button: {
                        title: resources.MilestoneResource.PlanFirstMilestone,
                        onclick: milestoneAction.showNewMilestonePopup,
                        canCreate: function () {
                            return currentProjectId ?
                                canCreateMilestone(currentProject) :
                                baseObject.Master.Projects.some(canCreateMilestone);
                        }
                    }
                },
                filterEmptyScreen: {
                    header: resources.MilestoneResource.FilterNoMilestones,
                    description: resources.MilestoneResource.DescrEmptyListMilFilter
                },
                groupMenu: {
                    actions: actions,
                    getItemByCheckbox: getMilestoneByTarget
                }
            });
            self.getData = self.getData.bind(self, teamlab.getPrjMilestones, onGetMilestones);
        }
        var eventConstructor = ASC.Projects.Event,
            events = teamlab.events;

        self.baseInit(
            {
                moduleTitle: projectsJsResource.MilestonesModule
            },
            {
                pagination: "milestonesKeyForPagination"
            },
            {
                handler: changeStatusHandler,
                getItem: getMilestoneByTarget,
                statuses: [
                    { cssClass: "open", text: projectsJsResource.StatusOpenMilestone, id: 0 },
                    { cssClass: "closed", text: projectsJsResource.StatusClosedMilestone, id: 1 }
                ]
            },
            showEntityMenu,
            [
                eventConstructor(events.addPrjTask, onAddTask),
                eventConstructor(events.addPrjMilestone, onAddMilestone),
                eventConstructor(events.updatePrjMilestone, onUpdateMilestone),
                eventConstructor(events.removePrjMilestone, onDeleteMilestone),
                eventConstructor(events.updatePrjMilestoneStatus, onUpdateMilestoneStatus),
                eventConstructor(events.getPrjProject, function (params, project) { currentProject = project; })
            ],
            {
                getItem: getMilestoneByTarget,
                selector: 'tr .title a'
            }
        );

        filter.createAdvansedFilterForMilestones(self);

        // Events

        $milestoneList.on(clickEventName, "td.responsible span", function () {
            var $self = jq(this);
            if ($self.hasClass("not-action")) return;
            var milestone = getMilestoneByTarget($self);
            filter.addUser('responsible_for_milestone', milestone.responsibleId, ['user_tasks']);
        });

        $milestoneList.on(clickEventName, "td.activeTasksCount a, td.closedTasksCount a", baseObject.Common.goToWithoutReload);
    };

    function changeStatusHandler(id, status) {
        if (status === 1) {
            var milestone = getMilestoneById(id);
            if (milestone && milestone.activeTasksCount) {
                showQuestionWindow(id);
                return;
            }
        }

        teamlab.updatePrjMilestone(
            { milestoneId: id },
            id,
            { status: status },
            { error: onChangeStatusError });
    };

    function updateMilestoneActionHandler(milestoneId) {
        var milestone = getMilestoneById(milestoneId);
        var milestoneForUpdate =
        {
            id: milestone.id,
            project: milestone.projectId,
            responsible: milestone.responsibleId,
            deadline: milestone.deadline,
            title: milestone.title,
            description: milestone.description,
            isKey: milestone.isKey,
            isNotify: milestone.isNotify
        };

        milestoneAction.onGetMilestoneBeforeUpdate(milestoneForUpdate);
    }

    function addMilestoneTaskActionHandler(milestoneId) {
        var milestone = getMilestoneById(milestoneId);
        var taskParams = {
            milestoneId: milestoneId,
            projectId: milestone.projectId
        };

        ASC.Projects.TaskAction.showCreateNewTaskForm(taskParams);
    }

    function maRemoveHandler(milestoneId) {
        var milestone = getMilestoneById(milestoneId);
        self.showCommonPopup("milestoneRemoveWarning", function () {
            loadingBanner.displayLoading();

            milestoneAction.updateCaldavMilestone(milestone.id, milestone.projectId, 2);
            teamlab.removePrjMilestone(milestoneId);
            jq.unblockUI();
        });
    }

    function gaRemoveHandler(milestoneids) {
        self.showCommonPopup("milestonesRemoveWarning", function () {
            loadingBanner.displayLoading();
            teamlab.removePrjMilestones({ ids: milestoneids }, {
                success: function (params, data) {
                    for (var i = 0; i < data.length; i++) {
                        milestoneAction.updateCaldavMilestone(data[i].id, data[i].projectId, 2);
                        teamlab.call(teamlab.events.removePrjMilestone, this, [{ disableMessage: true }, data[i]]);
                    }
                    loadingBanner.hideLoading();
                    common.displayInfoPanel(projectsJsResource.MilestonesRemoved);
                }
            });
            jq.unblockUI();
        });
    }

    var getMilestoneTasksLink = function (prjId, milestoneId, status) {
        var link = 'Tasks.aspx?prjID=' + prjId + '#milestone=' + milestoneId + '&status=' + status;

        if (location.hash.indexOf('user_tasks') > 0)
            link += '&tasks_responsible=' + jq.getAnchorParam('user_tasks', location.href);

        return link;
    };

    function onGetMilestones(params, milestones) {
        $milestoneListBody = $milestoneList.find("tbody");
        filterMilestoneCount = params.__total != undefined ? params.__total : 0;
        currentMilestonesList = milestones.map(getMilestoneTemplate);

        self.showOrHideData(currentMilestonesList, filterMilestoneCount);
    };

    function getMilestoneTemplate(milestone) {
        var id = milestone.id;
        var prjId = milestone.projectId;
        var template = {
            id: id,
            isKey: milestone.isKey,
            isNotify: milestone.isNotify,
            title: milestone.title,
            activeTasksCount: milestone.activeTaskCount || 0,
            activeTasksLink: getMilestoneTasksLink(prjId, id, 'open'),
            closedTasksCount: milestone.closedTaskCount || 0,
            closedTasksLink: getMilestoneTasksLink(prjId, id, 'closed'),
            canEdit: milestone.canEdit,
            canDelete: milestone.canDelete,
            projectId: prjId,
            projectTitle: milestone.projectTitle,
            createdById: milestone.createdBy.id,
            createdBy: milestone.createdBy.displayName,
            description: milestone.description,
            created: milestone.displayDateCrtdate
        };

        if (milestone.responsible) {
            template.responsible = milestone.responsible.displayName;
            template.responsibleId = milestone.responsible.id;
            template.isTerminated = milestone.responsible.isTerminated;
        } else {
            template.responsible = null;
            template.responsibleId = null;
        }

        var today = new Date();
        template.overdue = milestone.status === 0 && today >= milestone.deadline;
        template.status = milestone.status;
        template.deadline = milestone.displayDateDeadline;

        return template;
    };

    function showQuestionWindow(milestoneId) {
        self.showCommonPopup("closeMilestoneWithOpenTasks", function () {
            var milestone = getMilestoneById(milestoneId);
            location.href = 'Tasks.aspx?prjID=' + milestone.projectId + '#milestone=' + milestoneId + '&status=open';
        });
    };

    function showEntityMenu(selectedActionCombobox) {
        var milestoneId = selectedActionCombobox.attr("id");
        var milestone = getMilestoneById(milestoneId);

        var menuItems = [],
            ActionMenuItem = ASC.Projects.ActionMenuItem;

        if (milestone.status !== 'closed') {
            menuItems.push(new ActionMenuItem("updateMilestoneButton", resources.TasksResource.Edit, updateMilestoneActionHandler.bind(null, milestoneId), "edit"));
            menuItems.push(new ActionMenuItem("addMilestoneTaskButton", resources.TasksResource.AddTask, addMilestoneTaskActionHandler.bind(null, milestoneId), "new-task"));
        }

        if (milestone.canDelete) {
            menuItems.push(new ActionMenuItem("removeMilestoneButton", resources.CommonResource.Delete, maRemoveHandler.bind(null, milestoneId), "delete"));
        }

        return { menuItems: menuItems };
    }

    var removeMilestonesActionsForManager = function () {
        var milestones = currentMilestonesList;
        for (var i = 0; i < milestones.length; i++) {
            if (milestones[i].responsibleId != currentUserId) {
                jq("tr#" + milestones[i].id).find(entityMenuClassName).remove();
            }
        }
        $emptyListMilestone.find(".emptyScrBttnPnl").remove();
    };

    function onAddMilestone(params, milestone) {
        currentProjectId = jq.getURLParam("prjId");

        if (currentProjectId && currentProjectId != milestone.projectId) return;

        filterMilestoneCount++;
        currentMilestonesList.unshift(getMilestoneTemplate(milestone));

        self.showOrHideData(currentMilestonesList, filterMilestoneCount);
        $milestoneListBody.find("tr:first").yellowFade();

        milestoneAction.unlockMilestoneActionPage();
        jq.unblockUI();
    };
    function onUpdateMilestone(params, milestone) {

        if (milestone.status == 0)
            milestoneAction.updateCaldavMilestone(milestone.id, milestone.projectId, 1)
        else if (milestone.status == 1)
            milestoneAction.updateCaldavMilestone(milestone.id, milestone.projectId, 2)

        var milestoneTemplate = getMilestoneTemplate(milestone);

        currentMilestonesList = currentMilestonesList.filter(function(item) { return item.id !== milestone.id });
        currentMilestonesList.push(milestoneTemplate);

        var $updatedMilestone = $milestoneList.find("#" + milestone.id);
        var newMilestone = jq.tmpl(projectsMilestoneTemplateName, milestoneTemplate);

        $updatedMilestone.replaceWith(newMilestone);
        newMilestone.yellowFade();
        milestoneAction.unlockMilestoneActionPage();
        jq.unblockUI();
        common.displayInfoPanel(projectsJsResource.MilestoneUpdated);
    };

    function onUpdateMilestoneStatus(params, milestone) {
        baseObject.GroupActionPanel.deselectAll();
        common.changeMilestoneCountInProjectsCache(milestone, 1);
        onUpdateMilestone(params, milestone);
    }

    function onChangeStatusError(params, error) {
        if (error[0] === "Can not close a milestone with open tasks") {
            showQuestionWindow(params.milestoneId);
        } else {
            common.displayInfoPanel(error[0], true);
        }
    };

    function onDeleteMilestone(params, milestone) {
        var milestoneId = milestone.id;
        var removedMilestone = $milestoneList.find("#" + milestoneId);
        removedMilestone.yellowFade();
        removedMilestone.remove();

        filterMilestoneCount--;
        currentMilestonesList = currentMilestonesList.filter(function (item) { return item.id !== milestone.id });
        pageNavigator.update(filterMilestoneCount);

        if ($milestoneListBody.children("tr").length === 0) {
            pageNavigator.hide();

            self.showOrHideData([], filterMilestoneCount);
        }
        ASC.Projects.Master.Milestones = ASC.Projects.Master.Milestones.filter(function (item) {
            return item.id !== milestone.id;
        });

        loadingBanner.hideLoading();
        if (!params || !params.disableMessage) {
            common.displayInfoPanel(projectsJsResource.MilestoneRemoved);
        }
        common.changeMilestoneCountInProjectsCache(milestone, 2);
    };

    function onAddTask(params, task) {
        if (task.milestone == null) return;
        var milestone = getMilestoneById(task.milestone.id);
        var $updatedMilestone = $milestoneListBody.find("#" + task.milestone.id);
        if (!milestone || !$updatedMilestone.length) return;

        milestone.activeTasksCount++;

        var newMilestone = jq.tmpl(projectsMilestoneTemplateName, milestone);
        $updatedMilestone.replaceWith(newMilestone);
        newMilestone.yellowFade();
    };

    var unbindListEvents = function () {
        if (!isInit) return;
        $milestoneList.unbind();
        self.unbindEvents();
        jq("body").off(clickMilestoneInit);
    };

    function getMilestoneByTarget($targetObject) {
        return getMilestoneById(jq($targetObject).parents("#milestonesList tr").attr("id"));
    }

    function getMilestoneById(id) {
        return currentMilestonesList.find(function (item) { return item.id == id });
    }

    return jq.extend({
        init: init,
        removeMilestonesActionsForManager: removeMilestonesActionsForManager,
        unbindListEvents: unbindListEvents,
        basePath: 'sortBy=deadline&sortOrder=ascending'
    }, ASC.Projects.Base);
})(jQuery);