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
ASC.Projects.MilestoneAction = (function() {
    var isInit, isInitData;
    var isAdmin;
    var myGuid;
    var currentProjectId;
    var filterProjectsIds = [];
    var projectItems = [];
    // DOM variables
    var milestoneProject;
    var milestoneResponsible;

    var setProjectCombobox = function (milestone) {
        jq(milestoneProject, milestoneResponsible).css('max-width', 300);

        function sortPrj(item) {
            return item.canCreateMilestone;
        }

        function mapPrj(item) {
            return { id: item.value, title: item.title };
        }
        
        projectItems = ASC.Projects.Common.getProjectsForFilter().filter(sortPrj).map(mapPrj);
        
        milestoneProject.projectadvancedSelector(
            {
                itemsChoose: projectItems,
                onechosen: true,
                inPopup: true,
                sortMethod: function() { return 0; }
            }
        );

        milestoneProject.on("showList", function (event, item, milestoneResponsibleId) {
            jq("#milestoneProject").attr("data-id", item.id).text(item.title).attr("title", item.title);
            milestoneProjectOnChange(item, milestoneResponsibleId);
        });
        
        var curPrjId = currentProjectId || jq.getAnchorParam('project', ASC.Controls.AnchorController.getAnchor());

        var selectedPrj = projectItems.find(function(item) { return item.id == curPrjId; });
        
        if (selectedPrj) milestoneProject.projectadvancedSelector("selectBeforeShow", selectedPrj, milestone ? milestone.resposible : undefined);
    };

    var setResponsibleCombobox = function () {
        var selectorObj = {
            onechosen: true,
            inPopup: true,
            noresults: ASC.Resources.Master.Resource.UserSelectorNoResults
        };
        
        if (currentProjectId) {
            selectorObj.itemsChoose = ASC.Projects.Common.excludeVisitors(ASC.Projects.Master.Team).map(function(item) {
                return { id: item.id, title: item.displayName };
            });
        }
        
        milestoneResponsible.projectadvancedSelector(selectorObj);

        milestoneResponsible.on("showList", function (event, item) {
            milestoneResponsible.attr("data-id", item.id).html(item.title).attr("title", item.title);
            milestoneResponsibleOnChange(item);
        });
        
        jq("#milestoneResponsibleContainer").show();
    };

    var initData = function(milestone) {
        if (isInitData) return;
        isInitData = true;

        setProjectCombobox(milestone);
        setResponsibleCombobox(milestone);
    };

    var init = function() {
        if (isInit) {
            return;
        }

        isInit = true;

        // init DOM variables
        milestoneProject = jq('#milestoneProject');
        milestoneResponsible = jq('#milestoneResponsibleContainer #milestoneResponsible');;

        myGuid = Teamlab.profile.id;
        currentProjectId = jq.getURLParam('prjID');

        if (Teamlab.profile.isAdmin) {
            isAdmin = true;
        }

        jq("#milestoneDeadlineInputBox").mask(ASC.Resources.Master.DatePatternJQ);
        
        jq('#milestoneDeadlineInputBox').on("change",function () {
            if (jq.trim(jq(this).val()) != '') {
                jq('#milestoneDeadlineContainer').removeClass('requiredFieldError');
            }
        });
        
        jq('#milestoneTitleInputBox').keyup(function() {
            if (jq.trim(jq(this).val()) != '') {
                jq('#milestoneTitleContainer').removeClass('requiredFieldError');
            }
        });

        jq('#milestoneActionPanel .deadlineLeft').on('click', function() {
            jq('#milestoneActionPanel .deadlineLeft').css('border-bottom', '1px dotted').css('font-weight', 'normal');
            jq(this).css('border-bottom', 'none').css('font-weight', 'bold');
            var period = parseInt(jq(this).attr('data-value'));
            var date = new Date();
            if (period == 7) {
                date.setDate(date.getDate() + period);
            } else {
                date.setMonth(date.getMonth() + period);
            }
            jq("#milestoneDeadlineInputBox").datepicker('setDate', date);
        });

        jq('#milestoneActionButton').on('click', function () {
            if (jq(this).hasClass("disable")) return;
            jq('#milestoneProjectContainer').removeClass('requiredFieldError');
            jq('#milestoneResponsibleContainer').removeClass('requiredFieldError');
            jq('#milestoneDeadlineContainer').removeClass('requiredFieldError');
            jq('#milestoneTitleContainer').removeClass('requiredFieldError');

            var data = {};
            var milestoneId = jq('#milestoneActionPanel').attr('milestoneId');
            data.projectId = milestoneProject.attr("data-id") || currentProjectId;
            data.responsible = milestoneResponsible.attr("data-id");
            data.notifyResponsible = jq('#notifyResponsibleCheckbox').is(':checked');

            if (jq('#milestoneDeadlineInputBox').val().length) {
                data.deadline = jq('#milestoneDeadlineInputBox').datepicker('getDate');
                data.deadline.setHours(0);
                data.deadline.setMinutes(0);
                data.deadline = Teamlab.serializeTimestamp(data.deadline);
            }

            data.title = jq.trim(jq('#milestoneTitleInputBox').val());
            data.description = jq('#milestoneDescriptionInputBox').val();

            data.isKey = jq('#milestoneKeyCheckBox').is(':checked');
            data.isNotify = jq('#milestoneNotifyManagerCheckBox').is(':checked');

            var isError = false;
            if (!data.projectId) {
                jq('#milestoneProjectContainer').addClass('requiredFieldError');
                isError = true;
            }
            if (!data.responsible) {
                jq('#milestoneResponsibleContainer .notifyResponsibleContainer').hide();
                jq('#milestoneResponsibleContainer').addClass('requiredFieldError');
                isError = true;
            }
            if (!data.deadline) {
                jq('#milestoneDeadlineContainer').addClass('requiredFieldError');
                isError = true;
            }
            if (!data.title.length) {
                jq('#milestoneTitleContainer').addClass('requiredFieldError');
                isError = true;
            }
            if (!data.projectId) {
                jq('#milestoneProjectContainer').addClass('requiredFieldError');
                isError = true;
            }
            if (isError) { return false; }

            LoadingBanner.showLoaderBtn("#milestoneActionPanel");

            if (jq('#milestoneActionPanel').attr('type') == 'update') {
                updateMilestone(milestoneId, data);
            }
            else {
                addMilestone(data);
            }
            return false;
        });
        jq('#milestoneActionCancelButton').on('click', function() {
            if (location.href.indexOf("ganttchart.aspx") > 0) {
                ASC.Projects.GantChartPage.enableChartEvents();
            }
        });
    };


    var milestoneProjectOnChange = function (item, milestoneResponsibleid) {
        jq('#milestoneProjectContainer').removeClass('requiredFieldError');
        jq('#milestoneResponsibleContainer').show();
        jq('#milestoneResponsibleContainer .notifyResponsibleContainer').hide();
        getProjectParticipants(item.id, { responsible: milestoneResponsibleid });
    };
    
    var milestoneResponsibleOnChange = function (item) {
        jq('#milestoneResponsibleContainer').removeClass('requiredFieldError');
        
        if (item.id && item.id != myGuid) {
            jq('#milestoneResponsibleContainer .notifyResponsibleContainer').show();
            jq('#notifyResponsibleCheckbox').attr('checked', true);
        } else {
            jq('#milestoneResponsibleContainer .notifyResponsibleContainer').hide();
        }
    };
    
    var addMilestone = function(milestone) {
        lockMilestoneActionPage();

        var params = { projectId: milestone.projectId };
        Teamlab.addPrjMilestone(params, milestone.projectId, milestone, { success: onAddMilestone, error: onAddMilestoneError });
    };

    var updateMilestone = function(milestoneId, milestone) {
        lockMilestoneActionPage();

        var params = {};
        Teamlab.updatePrjMilestone(params, milestoneId, milestone,
            {
                success: function (params, milestone) {
                    for (var i = 0; i < ASC.Projects.Master.Milestones.length; i++) {
                        if (ASC.Projects.Master.Milestones[i].id == milestone.id) {
                            ASC.Projects.Master.Milestones[i] = milestone;
                            break;
                        }
                    }
                    ASC.Projects.AllMilestones.onUpdateMilestone(params, milestone);
                },
                error: ASC.Projects.AllMilestones.onUpdateMilestoneError
            });
    };

    var lockMilestoneActionPage = function() {
        jq('#milestoneDeadlineInputBox').attr('disabled', true);
        jq('#milestoneTitleInputBox').attr('disabled', true);
        jq('#milestoneDescriptionInputBox').attr('disabled', true);
        jq('#milestoneKeyCheckBox').attr('disabled', true);
        jq('#milestoneNotifyManagerCheckBox').attr('disabled', true);
    };

    var unlockMilestoneActionPage = function() {
        jq('#milestoneDeadlineInputBox').removeAttr('disabled');
        jq('#milestoneTitleInputBox').removeAttr('disabled').val('');
        jq('#milestoneDescriptionInputBox').removeAttr('disabled').val('');
        jq('#milestoneKeyCheckBox').removeAttr('disabled').removeAttr("checked");
        jq('#milestoneNotifyManagerCheckBox').removeAttr('disabled');
        LoadingBanner.hideLoaderBtn("#milestoneActionPanel");
    };

    var clearPanel = function() {
        jq('#milestoneActionPanel').removeAttr('type');
        jq('#notifyResponsibleCheckbox').attr('checked', true);

        if (!milestoneProject.attr("data-id")) {
            jq('#milestoneResponsibleContainer').hide();
            jq('#milestoneResponsibleContainer .notifyResponsibleContainer').hide();
        }

        jq('#milestoneProjectContainer').removeClass('requiredFieldError');

        jq('#milestoneDeadlineContainer').removeClass('requiredFieldError');

        jq('#milestoneActionPanel .deadlineLeft').css('border-bottom', '1px dotted').css('font-weight', 'normal');

        var milestoneDeadline = jq('#milestoneDeadlineInputBox');
        milestoneDeadline.datepicker({ popupContainer: '#milestoneActionPanel', selectDefaultDate: true });
        jq(milestoneDeadline).on("keydown", function (e) { if (e.keyCode == 13) { milestoneDeadline.blur(); } });
        jq(milestoneDeadline).on("change", function () { milestoneDeadline.blur(); });
        
        var date = new Date();
        date.setDate(date.getDate() + 7);
        milestoneDeadline.datepicker('setDate', date);
        var elemDuration3Days = jq(milestoneDeadline).siblings(".dottedLink[data-value=7]");
        jq(elemDuration3Days).css("border-bottom", "medium none");
        jq(elemDuration3Days).css("font-weight", "bold");


        jq('#milestoneResponsibleContainer').removeClass('requiredFieldError');
        milestoneResponsible.projectadvancedSelector("reset");

        jq('#milestoneTitleContainer').removeClass('requiredFieldError');
        jq('#milestoneTitleInputBox').val('');

        jq('#milestoneDescriptionInputBox').val('');

        jq('#milestoneKeyCheckBox').removeAttr('checked');

        jq('#milestoneNotifyManagerCheckBox').removeAttr('checked');
        LoadingBanner.hideLoaderBtn("#milestoneActionPanel");
    };

    var onAddMilestone = function(params, milestone) {
        unlockMilestoneActionPage();
        if (location.href.indexOf("ganttchart.aspx") > 0) {
            ASC.Projects.GantChartPage.addMilestoneToChart(milestone, true, true);
        }
        
        if (ASC.Projects.projectNavPanel && currentProjectId == milestone.projectId) {
            ASC.Projects.projectNavPanel.changeModuleItemsCount(ASC.Projects.projectNavPanel.projectModulesNames.milestones, "add");
            ASC.Projects.Master.Milestones.push(milestone);
            ASC.Projects.Master.Milestones = ASC.Projects.Master.Milestones.sort(ASC.Projects.Common.milestoneSort);
        }
        if (location.href.indexOf("milestones.aspx") > 0 && (currentProjectId == null || milestone.projectId == currentProjectId)) {
            ASC.Projects.AllMilestones.onAddMilestone(params, milestone);
        } else if (location.href.indexOf("tasks.aspx") > 0 && currentProjectId && milestone.projectId == currentProjectId) {
            ASC.Projects.TasksManager.onAddMilestone();
        } else {
            jq.unblockUI();
        }
        ASC.Projects.TaskAction.onAddNewMileston(milestone);
        ASC.Projects.Common.displayInfoPanel(ASC.Projects.Resources.ProjectsJSResource.MilestoneAdded);
    };

    var onAddMilestoneError = function(params, error) {
        if (location.href.indexOf("ganttchart.aspx") > 0) {
            ASC.Projects.GantChartPage.enableChartEvents();
        }

        var errorBox = jq("#milestoneActionPanel .errorBox");
        var actionContainer = jq("#milestoneActionButtonsContainer");
        errorBox.text(error[0]);
        errorBox.removeClass("display-none");
        jq("#milestoneActionButton").addClass("disable");
        LoadingBanner.hideLoaderBtn("#milestoneActionPanel");
        actionContainer.css('marginTop', '8px');
        actionContainer.show();


        setTimeout(function() {
            errorBox.addClass("display-none");
            actionContainer.css('marginTop', '43px');

            jq('#milestoneDeadlineInputBox').removeAttr('disabled');
            jq('#milestoneTitleInputBox').removeAttr('disabled');
            jq('#milestoneDescriptionInputBox').removeAttr('disabled');
            jq('#milestoneKeyCheckBox').removeAttr('disabled');
            jq('#milestoneNotifyManagerCheckBox').removeAttr('disabled');
        }, 3000);

        if (location.href.indexOf("milestones.aspx") > 0 && (currentProjectId == params.projectId)) {
            ASC.Projects.AllMilestones.removeMilestonesActionsForManager();
        }
        for (var i = 0; i < ASC.Projects.Master.Projects.length; i++) {
            if (params.projectId == ASC.Projects.Master.Projects[i].id) {
                ASC.Projects.Master.Projects[i].canCreateMilestone = false;
                break;
            }
        }
    };

    var onGetProjectParticipants = function(params, participants) {
        if (params.serverData && !participants) {
            participants = ASC.Projects.Master.Team;
        }
        
        participants = ASC.Projects.Common.excludeVisitors(participants);
        participants = ASC.Projects.Common.removeBlockedUsersFromTeam(participants);
        participants = participants
            .filter(function (item) {
                return !item.isVisitor;
            })
            .map(function (item) {
                return { id: item.id, title: item.displayName };
            });
        
        var mileResp = participants.find(function(item) {
            return item.id == params.responsible;
        });

        var respSelected;
        
        if (mileResp) {
            respSelected = mileResp;
        } else {
            if (!participants.length) {
                jq("#noActiveParticipantsMilNote").removeClass("display-none");
                jq("#milestoneActionButton").addClass("disable");
                respSelected = [{ id: "", title: ASC.Projects.Resources.CommonResource.Select}];
            } else {
                jq("#noActiveParticipantsMilNote").addClass("display-none");
                jq("#milestoneActionButton").removeClass("disable");
                respSelected = participants[0];
            }
        }
        
        milestoneResponsible.projectadvancedSelector("rewriteItemList", participants, [respSelected.id]);
        milestoneResponsible.projectadvancedSelector("selectBeforeShow", respSelected);

        if (jq("#milestoneProjectContainer").is(":visible")) {
            jq("#milestoneResponsibleContainer").show();
        }
        showMilestoneActionPanel();
    };

    var onGetMilestoneBeforeUpdate = function(milestone) {
        initData(milestone);
        clearPanel();
        
        jq('#milestoneActionPanel').attr('type', 'update');
        jq('#milestoneActionPanel').attr('milestoneId', milestone.id);

        var milestoneActionButton = jq('#milestoneActionButton');
        milestoneActionButton.html(milestoneActionButton.attr('update'));

        var milestoneActionHeader = jq('#milestoneActionPanel .containerHeaderBlock table td:first');
        milestoneActionHeader.html(ASC.Projects.Resources.ProjectsJSResource.EditMilestone);

        if (milestone.deadline) {
            jq('#milestoneDeadlineInputBox').datepicker("setDate", milestone.deadline);
            var elemDurationDays = jq("#milestoneDeadlineInputBox").siblings(".dottedLink");
            jq(elemDurationDays).css("border-bottom", "1px dotted");
            jq(elemDurationDays).css("font-weight", "normal");
        }

        jq('#milestoneTitleInputBox').val(milestone.title);
        jq('#milestoneDescriptionInputBox').val(milestone.description);

        jq('#milestoneProjectContainer').hide();

        if (!ASC.Projects.Common.currentUserIsModuleAdmin() && !ASC.Projects.Common.currentUserIsProjectManager(milestone.project)) {
            jq('#milestoneResponsibleContainer').hide();
        }

        if (milestone.isKey == 'true') {
            jq('#milestoneKeyCheckBox').prop("checked", true);
        }

        if (milestone.isNotify == 'true') {
            jq('#milestoneNotifyManagerCheckBox').prop("checked", true);
        }

        if (currentProjectId == milestone.project && milestoneProject.attr("data-id") == currentProjectId) {
            onGetProjectParticipants({ serverData: true, responsible: milestone.responsible });
        } else {
            var milePrj = projectItems.find(function(item) { return item.id == milestone.project; });
            if (milePrj) {
                milestoneProject.projectadvancedSelector("selectBeforeShow", milePrj, milestone.responsible);
            } else {
                milestoneProject.attr("data-id", milestone.project);
                getProjectParticipants(milestone.project, { responsible: milestone.responsible });
            }
        }
    };

    var getProjectParticipants = function(projectId, params) {
        Teamlab.getPrjProjectTeamPersons(params, projectId, { success: onGetProjectParticipants });
    };

    var showNewMilestonePopup = function() {
        initData();
        ASC.Projects.MilestoneAction.clearPanel();
        var milestoneActionButton = jq('#milestoneActionButton');
        milestoneActionButton.html(milestoneActionButton.attr('add'));

        var milestoneActionHeader = jq('#milestoneActionPanel .containerHeaderBlock table td:first');
        milestoneActionHeader.html(ASC.Projects.Resources.ProjectsJSResource.AddMilestone);

        jq('#milestoneProjectContainer').show();

        if (jq("#milestoneProject").attr("data-id")) {
            jq('#milestoneResponsibleContainer').show();
        }

        showMilestoneActionPanel();
        return false;
    };

    var showMilestoneActionPanel = function () {
        StudioBlockUIManager.blockUI(jq("#milestoneActionPanel"), 550, 550, 0);
        jq('#milestoneTitleInputBox').focus();
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

        var sortedProjects = ASC.Projects.Common.getProjectsForFilter();

        function sortPrj(item) {
            return item.canCreateMilestone && (!length || filterProjectsIds.some(function (fitem) {
                return item.value == fitem;
            }));
        }
        
        function mapPrj(item) {
            return { id: item.value, title: item.title };
        }

        milestoneProject.projectadvancedSelector("rewriteItemList", sortedProjects.filter(sortPrj).map(mapPrj), []);
    };
    
    return {
        init: init,
        showNewMilestonePopup: showNewMilestonePopup,
        updateMilestone: updateMilestone,
        onGetProjectParticipants: onGetProjectParticipants,
        onGetMilestoneBeforeUpdate: onGetMilestoneBeforeUpdate,
        clearPanel: clearPanel,
        unlockMilestoneActionPage: unlockMilestoneActionPage,
        filterProjectsByIdInCombobox: filterProjectsByIdInCombobox
    };
})(jQuery);

jq(document).ready(function() {
    jq('textarea').autosize();
});
