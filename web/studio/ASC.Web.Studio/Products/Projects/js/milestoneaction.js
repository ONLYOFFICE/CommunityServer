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


ASC.Projects.MilestoneAction = (function() {
    var isInit, isInitData, isAdmin, myGuid, currentProjectId, isInitMilestoneForm;
    var filterProjectsIds = [], projectItems = [];

    // DOM variables
    var $milestoneProject,
        $milestoneResponsible,
        $milestoneDeadlineInputBox,
        $milestoneResponsibleContainer,
        $notifyResponsibleContainer,
        $milestoneDeadlineContainer,
        $milestoneProjectContainer,
        $milestoneTitleContainer,
        $milestoneTitleInputBox,
        $milestoneActionPanel,
        $milestoneDeadlineLeft,
        $milestoneDescriptionInputBox,
        $milestoneKeyCheckBox,
        $milestoneNotifyManagerCheckBox,
        $milestoneActionButton,
        $notifyResponsibleCheckbox;

    var requiredFieldErrorClass = 'requiredFieldError',
        disabledAttr = 'disabled',
        displayNoneClass = "display-none",
        disableClass = "disable",
        ganttChartPage = "ganttchart.aspx";

    var resources = ASC.Projects.Resources,
        projectsJsResource = resources.ProjectsJSResource,
        common = ASC.Projects.Common;

    var teamlab, loadingBanner, selectedPrjId;

    var init = function () {
        if (isInit) {
            return;
        }

        teamlab = Teamlab;
        loadingBanner = LoadingBanner;

        isInit = true;
    };

    function initMilestoneFormElementsAndConstants() {
        if (isInitMilestoneForm) return;
        isInitMilestoneForm = true;
        var milesoneResource = resources.MilestoneResource;

        jq("#milestoneActionPanel")
            .html(jq.tmpl("common_containerTmpl",
            {
                options: {
                    PopupContainerCssClass: "popupContainerClass",
                    OnCancelButtonClick: "PopupKeyUpActionProvider.CloseDialog();",
                    IsPopup: true
                },
                header: {
                    data: { title: resources.ProjectResource.AddMilestone },
                    title: "projects_common_popup_header"
                },
                body: {
                    title: "projects_milestone_action",
                    data: {
                        title: {
                            error: milesoneResource.NoTitleMessage,
                            header: milesoneResource.Title
                        },
                        description: milesoneResource.Description,
                        project: {
                            header: resources.ProjectResource.Project,
                            error: milesoneResource.ChooseProject
                        }
                    }
                }
            }));

        // init DOM variables
        $milestoneActionPanel = jq("#milestoneActionPanel");
        $milestoneResponsibleContainer = jq("#milestoneResponsibleContainer");;
        $milestoneResponsible = $milestoneResponsibleContainer.find("#milestoneResponsible");
        $notifyResponsibleContainer = $milestoneResponsibleContainer.find(".notifyResponsibleContainer");
        $milestoneDeadlineInputBox = jq("#milestoneDeadlineInputBox");
        $milestoneDeadlineContainer = jq("#milestoneDeadlineContainer");
        $milestoneProjectContainer = $milestoneActionPanel.find(".projectContaner");
        $milestoneProject = $milestoneProjectContainer.find(".advansed-select-container");
        $milestoneTitleContainer = $milestoneActionPanel.find(".titlePanel");
        $milestoneTitleInputBox = $milestoneActionPanel.find(".textEdit");
        $milestoneDescriptionInputBox = $milestoneActionPanel.find("textarea");
        $milestoneKeyCheckBox = jq("#milestoneKeyCheckBox");
        $milestoneNotifyManagerCheckBox = jq("#milestoneNotifyManagerCheckBox");
        $milestoneActionButton = jq('#milestoneActionButton');
        $milestoneDeadlineLeft = $milestoneActionPanel.find(".deadlineLeft");
        $notifyResponsibleCheckbox = jq("#notifyResponsibleCheckbox");

        myGuid = teamlab.profile.id;
        currentProjectId = jq.getURLParam('prjID');

        if (teamlab.profile.isAdmin) {
            isAdmin = true;
        }

        $milestoneDeadlineInputBox.on("change", function () {
            if (jq.trim($milestoneDeadlineInputBox.val()) != '') {
                $milestoneDeadlineContainer.removeClass(requiredFieldErrorClass);
            }
            var date = getMilestoneDate();
            var dateNow = new Date();
            var week = teamlab.serializeTimestamp(new Date(dateNow.getFullYear(), dateNow.getMonth(), dateNow.getDate() + 7, 0, 0, 0));
            var month = teamlab.serializeTimestamp(new Date(dateNow.getFullYear(), dateNow.getMonth() + 1, dateNow.getDate(), 0, 0, 0));
            var twoMonth = teamlab.serializeTimestamp(new Date(dateNow.getFullYear(), dateNow.getMonth() + 2, dateNow.getDate(), 0, 0, 0));

            if (date === week) {
                boldDeadlineLeft(7);
            } else if (date === month) {
                boldDeadlineLeft(1);
            } else if (date === twoMonth) {
                boldDeadlineLeft(2);
            } else {
                boldDeadlineLeft(-1);
            }
        });
        $milestoneTitleInputBox.keyup(function () {
            if (jq.trim($milestoneTitleInputBox.val()) != '') {
                $milestoneTitleContainer.removeClass(requiredFieldErrorClass);
            }
        });

        $milestoneDeadlineLeft.click(function () {
            var period = parseInt(jq(this).attr('data-value'));
            boldDeadlineLeft(period);
            var date = new Date();
            if (period == 7) {
                date.setDate(date.getDate() + period);
            } else {
                date.setMonth(date.getMonth() + period);
            }
            $milestoneDeadlineInputBox.datepicker('setDate', date);
        });

        $milestoneActionButton.on('click', function () {
            if ($milestoneActionButton.hasClass(disableClass)) return;
            $milestoneProjectContainer.removeClass(requiredFieldErrorClass);
            $milestoneResponsibleContainer.removeClass(requiredFieldErrorClass);
            $milestoneDeadlineContainer.removeClass(requiredFieldErrorClass);
            $milestoneTitleContainer.removeClass(requiredFieldErrorClass);

            currentProjectId = jq.getURLParam('prjID');
            var milestoneId = $milestoneActionPanel.attr('milestoneId');
            var data = {
                projectId: $milestoneProject.attr("data-id") || currentProjectId,
                responsible: $milestoneResponsible.attr("data-id"),
                notifyResponsible: $notifyResponsibleCheckbox.is(':checked'),
                title: jq.trim($milestoneTitleInputBox.val()),
                description: $milestoneDescriptionInputBox.val(),
                isKey: $milestoneKeyCheckBox.is(':checked'),
                isNotify: $milestoneNotifyManagerCheckBox.is(':checked')
            };

            if ($milestoneDeadlineInputBox.val().length && jq.isDateFormat($milestoneDeadlineInputBox.val())) {
                data.deadline = getMilestoneDate();
            }

            var isError = false;
            if (!data.projectId) {
                $milestoneProjectContainer.addClass(requiredFieldErrorClass);
                isError = true;
            }
            if (!data.responsible) {
                $notifyResponsibleContainer.hide();
                $milestoneResponsibleContainer.addClass(requiredFieldErrorClass);
                isError = true;
            }
            if (!data.deadline) {
                $milestoneDeadlineContainer.addClass(requiredFieldErrorClass);
                isError = true;
            }
            if (!data.title.length) {
                $milestoneTitleContainer.addClass(requiredFieldErrorClass);
                isError = true;
            }
            if (!data.projectId) {
                $milestoneProjectContainer.addClass(requiredFieldErrorClass);
                isError = true;
            }
            if (isError) { return false; }

            loadingBanner.showLoaderBtn($milestoneActionPanel);

            if ($milestoneActionPanel.attr('type') == 'update') {
                updateMilestone(milestoneId, data);
            }
            else {
                addMilestone(data);
            }
            return false;
        });
        jq('#milestoneActionCancelButton').click(function () {
            if (location.href.toLowerCase().indexOf(ganttChartPage) > 0) {
                ASC.Projects.GantChartPage.enableChartEvents();
            }
        });

        teamlab.bind(teamlab.events.updatePrjProjectStatus, function (params, project) {
            prjOnChange(function () {
                return selectedPrjId === project.id && project.status === 1;
            });
        });

        teamlab.bind(teamlab.events.removePrjProjects, function (params, projects) {
            prjOnChange(function() {
                var remPrj = projects.find(function (item) { return item.id === selectedPrjId; });
                return typeof remPrj !== "undefined";
            });
        });

        teamlab.bind(teamlab.events.removePrjProject, function (params, project) {
            prjOnChange(function() {
                return project.id === selectedPrjId;
            });
        });

        function prjOnChange(condition) {
            if (!isInitData) return;
            var sortedProjects = common.getProjectsForFilter().filter(sortPrj).map(mapPrj);
            $milestoneProject.projectadvancedSelector("rewriteItemList", sortedProjects, []);

            if (condition() && sortedProjects.length) {
                $milestoneProject.projectadvancedSelector("reset");
                $milestoneProject.projectadvancedSelector("selectBeforeShow", sortedProjects[0]);
            }
        }
    }

    function getMilestoneDate() {
        var result = $milestoneDeadlineInputBox.datepicker('getDate');
        if (!result) return "";
        result.setHours(0);
        result.setMinutes(0);
        result.setSeconds(0);
        return teamlab.serializeTimestamp(result);
    }

    var setProjectCombobox = function (milestone) {
        jq($milestoneProject, $milestoneResponsible).css('max-width', 300);

        projectItems = common.getProjectsForFilter().filter(sortPrj).map(mapPrj);
        
        $milestoneProject.projectadvancedSelector(
            {
                itemsChoose: projectItems,
                onechosen: true,
                inPopup: true,
                sortMethod: function() { return 0; }
            }
        );

        $milestoneProject.on("showList", function (event, item, milestoneResponsibleId) {
            $milestoneProject.attr("data-id", item.id).text(item.title).attr("title", item.title);
            selectedPrjId = item.id;
            milestoneProjectOnChange(item, milestoneResponsibleId);
        });
    };

    function sortPrj(item) {
        return item.canCreateMilestone;
    }

    function mapPrj(item) {
        return { id: item.value, title: item.title };
    }

    var setResponsibleCombobox = function () {
        var selectorObj = {
            onechosen: true,
            inPopup: true,
            noresults: ASC.Resources.Master.Resource.UserSelectorNoResults
        };
        currentProjectId = jq.getURLParam('prjID');
        if (currentProjectId) {
            selectorObj.itemsChoose = common.excludeVisitors(ASC.Projects.Master.Team).map(function(item) {
                return { id: item.id, title: item.displayName };
            });
        }
        
        $milestoneResponsible.projectadvancedSelector(selectorObj);

        $milestoneResponsible.on("showList", function (event, item) {
            $milestoneResponsible.attr("data-id", item.id).html(item.title).attr("title", item.title);
            milestoneResponsibleOnChange(item);
        });
        
        $milestoneResponsibleContainer.show();
    };

    var initData = function (milestone) {
        if (!isInitData) {
            isInitData = true;
            setProjectCombobox(milestone);
            setResponsibleCombobox(milestone);
        }
        currentProjectId = jq.getURLParam('prjID');
        var anchor = location.hash.substring(1);
        var curPrjId = currentProjectId || jq.getAnchorParam('project', anchor);
        var selectedPrj = projectItems.find(function (item) { return item.id == curPrjId; });
        
        
        var responsible = jq.getAnchorParam('responsible_for_milestone', anchor) || 
            jq.getAnchorParam('tasks_responsible', anchor) ||
            jq.getAnchorParam('project_manager', anchor) ||
            jq.getAnchorParam('team_member', anchor) ||
            jq.getAnchorParam('author', anchor);
        
        if (selectedPrj) $milestoneProject.projectadvancedSelector("selectBeforeShow", selectedPrj, milestone ? milestone.resposible : responsible);
    };


    function milestoneProjectOnChange(item, milestoneResponsibleid) {
        $milestoneProjectContainer.removeClass(requiredFieldErrorClass);
        $milestoneResponsibleContainer.show();
        $notifyResponsibleContainer.hide();
        currentProjectId = jq.getURLParam('prjID');
        if (currentProjectId == item.id) {
            onGetProjectParticipants({ serverData: true, responsible: milestoneResponsibleid });
        } else {
            getProjectParticipants(item.id, { responsible: milestoneResponsibleid });
        }
    };
    
    var milestoneResponsibleOnChange = function (item) {
        $milestoneResponsibleContainer.removeClass(requiredFieldErrorClass);
        
        if (item.id && item.id != myGuid) {
            $notifyResponsibleContainer.show();
            $notifyResponsibleCheckbox.attr('checked', true);
        } else {
            $notifyResponsibleContainer.hide();
        }
    };
    
    var addMilestone = function(milestone) {
        lockMilestoneActionPage();

        var params = { projectId: milestone.projectId };
        teamlab.addPrjMilestone(params, milestone.projectId, milestone, { success: onAddMilestone, error: onAddMilestoneError });
    };

    var updateMilestone = function(milestoneId, milestone) {
        lockMilestoneActionPage();

        var params = {};
        teamlab.updatePrjMilestone(params, milestoneId, milestone,
            {
                success: function (params, milestone) {
                    updateCaldavMilestone(milestoneId, milestone.projectId, 0);
                    for (var i = 0; i < ASC.Projects.Master.Milestones.length; i++) {
                        if (ASC.Projects.Master.Milestones[i].id == milestone.id) {
                            ASC.Projects.Master.Milestones[i] = milestone;
                            break;
                        }
                    }
                }
            });
    };

    var lockMilestoneActionPage = function() {
        $milestoneDeadlineInputBox.attr(disabledAttr, true);
        $milestoneTitleInputBox.attr(disabledAttr, true);
        $milestoneDescriptionInputBox.attr(disabledAttr, true);
        $milestoneKeyCheckBox.attr(disabledAttr, true);
        $milestoneNotifyManagerCheckBox.attr(disabledAttr, true);
    };

    var unlockMilestoneActionPage = function () {
        if (!isInitMilestoneForm) return;
        $milestoneDeadlineInputBox.removeAttr(disabledAttr);
        $milestoneTitleInputBox.removeAttr(disabledAttr).val('');
        $milestoneDescriptionInputBox.removeAttr(disabledAttr).val('');
        $milestoneKeyCheckBox.removeAttr(disabledAttr).removeAttr("checked");
        $milestoneNotifyManagerCheckBox.removeAttr(disabledAttr);
        loadingBanner.hideLoaderBtn($milestoneActionPanel);
    };

    var clearPanel = function() {
        $milestoneActionPanel.removeAttr('type');
        $notifyResponsibleCheckbox.attr('checked', true);

        if (!$milestoneProject.attr("data-id")) {
            $milestoneResponsibleContainer.hide();
            $notifyResponsibleContainer.hide();
        }

        $milestoneProjectContainer.removeClass(requiredFieldErrorClass);
        $milestoneDeadlineContainer.removeClass(requiredFieldErrorClass);

        $milestoneDeadlineInputBox.val('');
        $milestoneDeadlineInputBox.datepicker({ popupContainer: '#milestoneActionPanel', selectDefaultDate: true });
        $milestoneDeadlineInputBox.mask(ASC.Resources.Master.DatePatternJQ);
        $milestoneDeadlineInputBox.on("keydown", onDatePickerKeyDown).on("change", onDatePickerChange);

        if (jq.browser.mobile)
            jq("#ui-datepicker-div").addClass("blockMsg");

        var date = new Date();
        date.setDate(date.getDate() + 7);
        $milestoneDeadlineInputBox.datepicker('setDate', date);
        boldDeadlineLeft(7);


        $milestoneResponsibleContainer.removeClass(requiredFieldErrorClass);
        $milestoneResponsible.projectadvancedSelector("reset");

        $milestoneTitleContainer.removeClass(requiredFieldErrorClass);
        $milestoneTitleInputBox.val('');

        $milestoneDescriptionInputBox.val('');

        $milestoneKeyCheckBox.removeAttr('checked');

        $milestoneNotifyManagerCheckBox.removeAttr('checked');
        loadingBanner.hideLoaderBtn($milestoneActionPanel);
    };

    function onDatePickerKeyDown(e) {
        if (e.keyCode === 13) {
            onDatePickerChange(e);
        }
    }

    function onDatePickerChange(e) {
        var obj = jq(e.target);
        var date = obj.datepicker("getDate");
        obj.unmask().blur().mask(ASC.Resources.Master.DatePatternJQ);
        obj.datepicker("setDate", date);
    }

    function boldDeadlineLeft(dataValue) {
        var dotline = "dotline", bold = "bold";
        $milestoneActionPanel.find('.deadlineLeft').removeClass(bold).removeClass(dotline).addClass(dotline);
        $milestoneActionPanel.find("[data-value=" + dataValue + "]").removeClass(dotline).addClass(bold);
    }

    function onAddMilestone(params, milestone) {
        unlockMilestoneActionPage();
        currentProjectId = jq.getURLParam('prjID');
        ASC.Projects.Master.Milestones.push(milestone);
        ASC.Projects.Master.Milestones = ASC.Projects.Master.Milestones.sort(common.milestoneSort);

        jq.unblockUI();
        common.displayInfoPanel(projectsJsResource.MilestoneAdded);
        $milestoneDeadlineInputBox.unmask();

        common.changeMilestoneCountInProjectsCache(milestone, 0);
        updateCaldavMilestone(milestone.id, milestone.projectId, 0);
    };

    var onAddMilestoneError = function(params, error) {
        if (location.href.toLowerCase().indexOf(ganttChartPage) > 0) {
            ASC.Projects.GantChartPage.enableChartEvents();
        }

        var errorBox = $milestoneActionPanel.find(".errorBox");
        var actionContainer = jq("#milestoneActionButtonsContainer");
        errorBox.text(error[0]);
        errorBox.removeClass(displayNoneClass);
        $milestoneActionButton.addClass(disableClass);
        loadingBanner.hideLoaderBtn($milestoneActionPanel);
        actionContainer.css('marginTop', '8px');
        actionContainer.show();


        setTimeout(function() {
            errorBox.addClass(displayNoneClass);
            actionContainer.css('marginTop', '43px');

            $milestoneDeadlineInputBox.removeAttr(disabledAttr);
            $milestoneTitleInputBox.removeAttr(disabledAttr);
            $milestoneDescriptionInputBox.removeAttr(disabledAttr);
            $milestoneKeyCheckBox.removeAttr(disabledAttr);
            $milestoneNotifyManagerCheckBox.removeAttr(disabledAttr);
        }, 3000);
        currentProjectId = jq.getURLParam('prjID');
        if (location.href.toLowerCase().indexOf("milestones.aspx") > 0 && (currentProjectId == params.projectId)) {
            ASC.Projects.AllMilestones.removeMilestonesActionsForManager();
        }
        for (var i = 0; i < ASC.Projects.Master.Projects.length; i++) {
            if (params.projectId == ASC.Projects.Master.Projects[i].id) {
                ASC.Projects.Master.Projects[i].canCreateMilestone = false;
                break;
            }
        }
    };

    var onGetProjectParticipants = function (params, participants) {
        if (params.serverData && !participants) {
            participants = ASC.Projects.Master.Team;
        }
        
        participants = common.excludeVisitors(participants);
        participants = common.removeBlockedUsersFromTeam(participants);
        participants = participants
            .filter(function (item) {
                return !item.isVisitor;
            })
            .map(function (item) {
                return { id: item.id, title: item.id == teamlab.profile.id ? ASC.Resources.Master.Resource.MeLabel : item.displayName };
            });
        
        var mileResp = participants.find(function(item) {
            return item.id == params.responsible;
        });

        var respSelected;
        
        if (mileResp) {
            respSelected = mileResp;
        } else {
            var $noActiveParticipantsMilNote = jq("#noActiveParticipantsMilNote");
            if (!participants.length) {
                $noActiveParticipantsMilNote.removeClass(displayNoneClass);
                $milestoneActionButton.addClass(disableClass);
                respSelected = [{ id: "", title: resources.CommonResource.Select }];
            } else {
                var currentProject = common.getProjectById(selectedPrjId);
                $noActiveParticipantsMilNote.addClass(displayNoneClass);
                $milestoneActionButton.removeClass(disableClass);
                respSelected = participants.find(function (item) { return item.id === currentProject.responsibleId; }) || participants[0];
            }
        }
        
        $milestoneResponsible.projectadvancedSelector("rewriteItemList", participants, [respSelected.id]);
        $milestoneResponsible.projectadvancedSelector("selectBeforeShow", respSelected);

        if ($milestoneProjectContainer.is(":visible")) {
            $milestoneResponsibleContainer.show();
        }
    };
    var updateCaldavMilestone = function (id, prjId, action) {
        teamlab.getPrjTeam({}, prjId, function (p, team) {
            var url = ASC.Resources.Master.ApiPath + "calendar/caldavevent.json";
            var postData = {
                calendarId: "Project_" + prjId,
                uid: "Milestone_" + id,
                responsibles: jq.map(team, function (user) { return user.id; })
            };
            jq.ajax({
                type: action === 0 || action === 1 ? 'put' : 'delete',
                url: url,
                data: postData,
                complete: function (d) { }
            });
        });
    };
    var onGetMilestoneBeforeUpdate = function (milestone) {
        initMilestoneFormElementsAndConstants();
        initData(milestone);
        clearPanel();
        
        $milestoneActionPanel.attr('type', 'update');
        $milestoneActionPanel.attr('milestoneId', milestone.id);

        $milestoneActionButton.html($milestoneActionButton.attr('update'));

        var milestoneActionHeader = $milestoneActionPanel.find(".containerHeaderBlock table td:first");
        milestoneActionHeader.html(projectsJsResource.EditMilestone);

        if (milestone.deadline) {
            $milestoneDeadlineInputBox.datepicker("setDate", milestone.deadline);
            $milestoneDeadlineInputBox.change();
        }

        $milestoneTitleInputBox.val(milestone.title);
        $milestoneDescriptionInputBox.val(milestone.description);

        $milestoneProjectContainer.hide();

        if (!common.currentUserIsModuleAdmin() && !common.currentUserIsProjectManager(milestone.project)) {
            $milestoneResponsibleContainer.hide();
        }

        $milestoneKeyCheckBox.prop("checked", milestone.isKey);
        $milestoneNotifyManagerCheckBox.prop("checked", milestone.isNotify);

        currentProjectId = jq.getURLParam('prjID');
        if (currentProjectId == milestone.project && $milestoneProject.attr("data-id") == currentProjectId) {
            onGetProjectParticipants({ serverData: true, responsible: milestone.responsible });
        } else {
            var milePrj = projectItems.find(function(item) { return item.id == milestone.project; });
            if (milePrj) {
                $milestoneProject.trigger("showList", [milePrj, milestone.responsible]);
            } else {
                $milestoneProject.attr("data-id", milestone.project);
                getProjectParticipants(milestone.project, { responsible: milestone.responsible });
            }
        }
        showMilestoneActionPanel();
    };

    function getProjectParticipants(projectId, params) {
        teamlab.getPrjProjectTeamPersons(params, projectId, { success: onGetProjectParticipants });
    };

    var showNewMilestonePopup = function () {
        initMilestoneFormElementsAndConstants();
        initData();
        clearPanel();

        $milestoneActionButton.html($milestoneActionButton.attr('add'));

        var milestoneActionHeader = $milestoneActionPanel.find(".containerHeaderBlock table td:first");
        milestoneActionHeader.html(projectsJsResource.AddMilestone);

        $milestoneProjectContainer.show();

        if ($milestoneProject.attr("data-id")) {
            $milestoneResponsibleContainer.show();
        }

        showMilestoneActionPanel();
        return false;
    };

    function showMilestoneActionPanel() {
        StudioBlockUIManager.blockUI($milestoneActionPanel, 550);
        $milestoneTitleInputBox.focus();
    };

    var filterProjectsByIdInCombobox = function(ids) {  // only for gantt chart
        if (ids.length === filterProjectsIds.length) {

            var isEqual = true;
            for (var i = ids.length - 1; i >= 0; --i) {
                if (filterProjectsIds[i] !== ids[i]) {
                    isEqual = false;
                    break;
                }
            }

            if (isEqual) return;
        }

        filterProjectsIds = ids;


        var length = filterProjectsIds.length;

        var sortedProjects = common.getProjectsForFilter();

        function sortPrj(item) {
            return item.canCreateMilestone && (!length || filterProjectsIds.some(function (fitem) {
                return item.value == fitem;
            }));
        }
        
        function mapPrj(item) {
            return { id: item.value, title: item.title };
        }

        $milestoneProject.projectadvancedSelector("rewriteItemList", sortedProjects.filter(sortPrj).map(mapPrj), []);
    };

    return {
        init: init,
        showNewMilestonePopup: showNewMilestonePopup,
        onGetMilestoneBeforeUpdate: onGetMilestoneBeforeUpdate,
        updateCaldavMilestone: updateCaldavMilestone,
        unlockMilestoneActionPage: unlockMilestoneActionPage,
        filterProjectsByIdInCombobox: filterProjectsByIdInCombobox
    };
})(jQuery);

jq(document).ready(function() {
    autosize(jq('textarea'));
});
