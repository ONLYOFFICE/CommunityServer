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


ASC.Projects.AllMilestones = (function () {
    var baseObject = ASC.Projects,
        resources = baseObject.Resources,
        projectsJsResource = resources.ProjectsJSResource,
        filter = baseObject.ProjectsAdvansedFilter,
        pageNavigator = baseObject.PageNavigator,
        common = baseObject.Common,
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
                return prj.canCreateMilestone && prj.status === 0;
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
                    description: resources.MilestoneResource.MilestonesMarkMajorTimestamps,
                    button: {
                        title: resources.MilestoneResource.PlanFirstMilestone,
                        onclick: ASC.Projects.MilestoneAction.showNewMilestonePopup,
                        canCreate: function () {
                            return currentProjectId ?
                                canCreateMilestone(currentProject) :
                                ASC.Projects.Master.Projects.some(canCreateMilestone);
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
            var milestone = getMilestoneByTarget(jq(this));
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

        ASC.Projects.MilestoneAction.onGetMilestoneBeforeUpdate(milestoneForUpdate);
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
        self.showCommonPopup("milestoneRemoveWarning", function () {
            loadingBanner.displayLoading();
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
        var link = 'tasks.aspx?prjID=' + prjId + '#milestone=' + milestoneId + '&status=' + status;

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
            location.href = 'tasks.aspx?prjID=' + milestone.projectId + '#milestone=' + milestoneId + '&status=open';
        });
    };

    function showEntityMenu(selectedActionCombobox) {
        var milestoneId = selectedActionCombobox.attr("id");
        var milestone = getMilestoneById(milestoneId);

        var menuItems = [],
            ActionMenuItem = ASC.Projects.ActionMenuItem;

        if (milestone.status !== 'closed') {
            menuItems.push(new ActionMenuItem("updateMilestoneButton", resources.TasksResource.Edit, updateMilestoneActionHandler.bind(null, milestoneId)));
            menuItems.push(new ActionMenuItem("addMilestoneTaskButton", resources.TasksResource.AddTask, addMilestoneTaskActionHandler.bind(null, milestoneId)));
        }

        if (milestone.canDelete) {
            menuItems.push(new ActionMenuItem("removeMilestoneButton", resources.CommonResource.Delete, maRemoveHandler.bind(null, milestoneId)));
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

        ASC.Projects.MilestoneAction.unlockMilestoneActionPage();
        jq.unblockUI();
    };
    function onUpdateMilestone(params, milestone) {
        var milestoneTemplate = getMilestoneTemplate(milestone);

        currentMilestonesList = currentMilestonesList.filter(function(item) { return item.id !== milestone.id });
        currentMilestonesList.push(milestoneTemplate);

        var $updatedMilestone = $milestoneList.find("#" + milestone.id);
        var newMilestone = jq.tmpl(projectsMilestoneTemplateName, milestoneTemplate);

        $updatedMilestone.replaceWith(newMilestone);
        newMilestone.yellowFade();
        ASC.Projects.MilestoneAction.unlockMilestoneActionPage();
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