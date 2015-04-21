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


ASC.Projects.SubtasksManager = (function () {

    var currentUserId,
        currentProjectId,
        teamsHash = {},
        parentTask = null;

    var subtaskContainer = null,
        subtaskActionPanel = null;

    var editFlag = false,
        editedSubtaskId = null,
        subtaskResponsible = null;

    // key codes
    var escKey = 27,
        enterKey = 13,
        tabKey = 9;
    
    var isInit;
    
    var showActionsPanel = function (panelId, obj) {
        var objid = jq(obj).attr('subtaskid'),
            taskId = jq(obj).attr('taskid'),
            panel = jq('#' + panelId);
        var x, y;
        var subtask = jq('#subtask_' + objid);

        jq('.studio-action-panel').hide();
        jq('.menuopen').removeClass('menuopen');
        panel.show();

        if (panelId == 'subtaskActionPanel') {
            x = jq(obj).offset().left - 110;
            y = jq(obj).offset().top + 20;
            subtask.addClass('menuopen');
            if (subtask.hasClass('closed')) {
                panel.find(".dropdown-item").hide();
                jq("#sta_remove").show();
            } else {
                panel.find(".dropdown-item").show();
                if (!subtask.find(".user").hasClass("not")) { // todo: add check for team
                    jq("#sta_accept").hide();
                }
            }

            panel.data('subtaskid', objid);
            panel.data('taskid', taskId);
        }

        if (panelId == 'subTaskDescrPanel') {
            x = obj.offset().left; 
            y = obj.offset().top + 25;
            panel.attr('objid', jq(obj).attr('taskid'));
        }

        panel.css({ left: x, top: y });
    };

    var init = function () {
        if (isInit) {
            return;
        }

        isInit = true;
        
        currentUserId = Teamlab.profile.id;
        currentProjectId = jq.getURLParam("prjID");

        if (currentProjectId) {
            teamsHash = {};
            teamsHash[currentProjectId] = ASC.Projects.Master.Team;
        }

        //add subtask panels
        jq.tmpl("projects_subtaskActionPanelTmpl", {}).appendTo("body");
        jq("body").append(jq.tmpl("projects_panelFrame", {panelId: "subTaskDescrPanel"}));

        subtaskActionPanel = jq("#subtaskActionPanel");

        jq(document).on('mouseenter', '.subtask .taskName span', function (event) {
            showDescribePanel(event);
        });
        jq(document).on('mouseleave', '.subtask .taskName span', function () {
            hideDescribePanel();
        });

        jq(document).on('click', '.subtask .entity-menu', function () {
            hideSubtaskFields();
            if (subtaskActionPanel.is(":visible")) {
                subtaskActionPanel.hide();
                jq('.menuopen').removeClass('menuopen');
                return false;
            } else {
                jq(this).closest(".subtask").addClass('menuopen');
            }
            showActionsPanel('subtaskActionPanel', this);
            return false;
        });

        jq(document).on('dblclick', '.subtask .taskName', function () {
            editedSubtaskId = jq(this).attr('subtaskid');
            if (!jq(this).is('.canedit') || jq(this).parents('.subtask').is('.closed')) return false;
            editSubtask(editedSubtaskId);
            return true;
        });

        jq(document).on("click", ".subtask-add-button .button", function () {
            beforeSubtaskAction();
            return false;
        });

        jq(document).on("keydown", ".subtask-name-input", function (e) {
            switch(e.which){
                case escKey:
                    hideSubtaskFields();
                    break;
                case enterKey:
                    jq(this).prop("disabled", true);
                    beforeSubtaskAction();
                    break;
                default:
            }
        });

        jq(document).on('change', '.subtask .check input', function () {
            var data = {};
            var taskId = jq(this).attr('taskid');
            var subtaskId = jq(this).attr('subtaskid');
            if (jq(this).is(':checked')) {
                closeSubTask(taskId, subtaskId);
            } else {
                data.status = 'open';
                showSubtaskLoader(jq(this).closest(".check"));
                updateSubtaskStatus({}, taskId, subtaskId, data);
            }
        });

        jq(document).on("change", ".subtask-responsible-selector", function () {
            jq(".subtask-name-input").focus();
        });

        // click events
        jq(document).on('click', '.quickAddSubTaskLink span', function () {
            showNewSubtaskField(jq(this).parent());
            return false;
        });

        jq(document).on("click", "#quickAddSubTaskField", function (event) {
            if (jq(event.target).hasClass("subtask-responsible-selector")) {
                return true;
            }
            jq(".subtask-name-input").focus();
        });

        // subtask menu

        jq(document).on('click', "#sta_edit", function() {
            editSubtask(subtaskActionPanel.data('subtaskid'));
            jq('.studio-action-panel').hide();
            return false;
        });

        jq(document).on('click', "#sta_accept", function () {
            subtaskActionPanel.hide();

            var data = {},
                taskId = subtaskActionPanel.data("taskid"),
                subtaskid = subtaskActionPanel.data('subtaskid'),
                projectid = jq('.subtask[subtaskid=' + subtaskid + ']').attr('projectid');

            data.title = jq.trim(jq('.subtask[subtaskid=' + subtaskid + '] .taskName').text());
            data.responsible = currentUserId;
            data.description = '';

            Teamlab.updatePrjSubtask({ projectid: projectid }, taskId, subtaskid, data, { success: onUpdateSubtask });

            return false;
        });

        jq(document).on('click', "#sta_remove", function () {
            jq('.studio-action-panel').hide();
            var subtaskid = subtaskActionPanel.data("subtaskid");
            var taskId = subtaskActionPanel.data("taskid");
            removeSubtask({taskId: taskId}, taskId, subtaskid)
            return false;
        });

        jq('body').click(function (event) {
            var element = jq(event.target);

            if (subtaskActionPanel.is(":visible") && (!element.closest(".dropdown-item") || element.hasClass("entity-menu"))) {
                event.stopPropagation();
            }
            subtaskActionPanel.hide();
            jq('.menuopen').removeClass('menuopen');
        });

        //ga-track-events
        jq(".quickAddSubTaskLink").trackEvent(ga_Categories.subtask, ga_Actions.createNew, "create-new-subtask");
        jq(".subtask[status=1] .check input").trackEvent(ga_Categories.subtask, ga_Actions.changeStatus, "close");
        jq(".subtask[status=2] .check input").trackEvent(ga_Categories.subtask, ga_Actions.changeStatus, "open");
        //end ga-track-events

    };

    var addSubtask = function (params, taskId, data) {
        Teamlab.addPrjSubtask(params, taskId, data, { success: onAddSubtask });
    };

    var onAddSubtask = function (params, subtask) {
        subtask.projectid = params.projectId;
        showNewSubtask(params, subtask);
        ASC.Projects.SubtasksManager.onAddSubtaskHandler(params, subtask)
    };

    var updateSubtask = function (params, taskId, subtaskId, data) {
        Teamlab.updatePrjSubtask(params, taskId, subtaskId, data, { success: onUpdateSubtask });
    };

    var onUpdateSubtask = function (params, subtask) {
        subtask.projectid = params.projectid;
        replaceSubtask(subtask);
    };

    var closeSubTask = function (taskId, subtaskId) {
        var data = {};
        var subtask = jq("#subtask_" + subtaskId);
        showSubtaskLoader(subtask.find(".check"));

        data.title = jq.trim( subtask.find(".taskName span").text());
        data.responsible = currentUserId;
        setTimeout(function () { subtask.yellowFade(); }, 0);
        data.status = 'closed';

        updateSubtaskStatus({}, taskId, subtaskId, data);
    };

    var updateSubtaskStatus = function (params, taskId, subtaskId, data) {
        params.taskId = taskId;
        Teamlab.updatePrjSubtask(params, taskId, subtaskId, data, { success: onUpdateSubtaskStatus });
    };

    var onUpdateSubtaskStatus = function (params, subtask) {
        var subtaskid = subtask.id;
        var newstatus = subtask.status;
        var oldSubtask = jq("#subtask_" + subtaskid);
        var subtaskCont = jq(oldSubtask).closest('.subtasks');
        if (newstatus == 1) {
            jq(oldSubtask).prependTo(subtaskCont);
            jq(oldSubtask).removeClass('closed');
            jq(oldSubtask).find('.check input').removeAttr('checked');

        } else {
            jq(oldSubtask).addClass('closed');
            var user = jq(oldSubtask).find(".user");
            if (jq(user).hasClass("not")) {
                var displayName = subtask.responsible.displayName;
                if (currentUserId == subtask.responsible.id) {
                    displayName = Teamlab.profile.displayName;
                    jq(user).removeClass("not");
                }
                jq(user).attr('data-userId', subtask.responsible.id);
                jq(user).html(displayName);
            }
            jq(oldSubtask).find('.check input').prop('checked', true);
            jq(oldSubtask).appendTo(subtaskCont);
        }
        changeSubtaskAtributes(oldSubtask, subtask);
        jq('.subtask-loader').remove();
        jq(oldSubtask).find('.check input').show();

        ASC.Projects.SubtasksManager.onChangeTaskStatusHandler(params, subtask);
    };

    var removeSubtask = function (params, taskId, subtaskId) {
        Teamlab.removePrjSubtask(params, taskId, subtaskId, { success: onRemoveSubtask });
    };

    var onRemoveSubtask = function (params, subtask) {
        jq('#subtask_' + subtask.id).remove();
        ASC.Projects.SubtasksManager.onRemoveSubtaskHandler(params, subtask);
    };

    var showDescribePanel = function (event) {      // todo: rewrite!!!
        subtaskDescrTimeout = setTimeout(function () {
            var targetObject,
                taskDescribePanel = jq('#subTaskDescrPanel'),
                panelContent = taskDescribePanel.find(".panel-content");
                descriptionObj = {};

            panelContent.empty();

            if (jq(event.target).is('a')) {
                targetObject = jq(event.target).parent();
            } else {
                targetObject = event.target;
            }

            var taskName = jq(targetObject).closest('.taskName');
            if (jq(taskName).attr('status') == 2) {
                if (typeof jq(targetObject).parent().attr('updated') != 'undefined') {
                    descriptionObj.closedDate = jq(targetObject).parent().attr('updated').substr(0, 10);
                }
                if (typeof jq(targetObject).parent().attr('createdby') != 'undefined') {
                    descriptionObj.closedBy = jq.htmlEncodeLight(jq(targetObject).parent().attr('createdby'));
                }
            } else {
                if (typeof jq(targetObject).parent().attr('created') != 'undefined') {
                    descriptionObj.creationDate = jq(targetObject).parent().attr('created').substr(0, 10);
                }
                if (typeof jq(targetObject).parent().attr('createdby') != 'undefined') {
                    descriptionObj.createdBy = jq.htmlEncodeLight(jq(targetObject).parent().attr('createdby'));
                }
            }
            descriptionObj.startDate = null; // broken magic
            panelContent.append(jq.tmpl("projects_descriptionPanelContent", descriptionObj));
            showActionsPanel('subTaskDescrPanel', jq(event.target).parent());
            overSubtaskDescrPanel = true;
        }, 400, this);
    };

    var hideDescribePanel = function () {
        clearTimeout(subtaskDescrTimeout);
        overSubtaskDescrPanel = false;
        jq('#subTaskDescrPanel').hide();
    };

    var showSubtaskLoader = function (inputBox) {
        if (inputBox.hasClass("check")) {
            inputBox.find("input").hide();
        }
        inputBox.prepend('<div class="subtask-loader"></div>');
    };

    var getEmptySubtask = function () {
        var subtask = { subtaskid: "-1", title: "", projectid: "", taskid: "-1" };
        subtask.responsible = null;
        return subtask;
    };

    var setTeamForResponsibleSelect = function (projectId) {
        if (teamsHash && teamsHash.hasOwnProperty(projectId)) {
            initResponsibleSelector(teamsHash[projectId]);
        } else {
            Teamlab.getPrjTeam({projectId: projectId}, projectId, {
                success: function (params, team) {
                    teamsHash[params.projectId] = team;
                    initResponsibleSelector(team);
                }
            });
        }
    };

    var initResponsibleSelector = function (team) {
        var validTeamMembers = ASC.Projects.Common.excludeVisitors(team);
        validTeamMembers = ASC.Projects.Common.removeBlockedUsersFromTeam(validTeamMembers);
        var responsibleSelector = jq(".subtask-responsible-selector");
        for (var i = 0; i < validTeamMembers.length; i++) {
            responsibleSelector.append(jq('<option value="' + validTeamMembers[i].id + '"></option>').html(validTeamMembers[i].displayName));
        }
        if (subtaskResponsible) {
            responsibleSelector.find("option[value=" + subtaskResponsible + "]").prop("selected", true);
        }
        responsibleSelector.tlcombobox();
    };

    var beforeSubtaskAction = function () {
        var subtask = {},
            params = {};
        var field = jq(".subtask-name-input");
        subtaskContainer = field.closest(".subtasks");
        subtask.title = field.val().trim();
        if (subtask.title.length == 0) {
            if (!editFlag) {
                hideSubtaskFields();
            } else {
                field.focus();
            }
            return false;
        }
        subtask.responsible = jq(".tl-combobox.subtask-responsible-selector").attr("data-value") || jq(".subtask-responsible-selector").val();
        if (subtask.responsible == "-1") {
            subtask.responsible = ASC.Projects.Common.emptyGuid;
        }
        subtask.taskId = field.data("taskid");

        params.projectid = field.data("projectid");
        params.taskid = subtask.taskId;

        showSubtaskLoader(field.closest(".subtask-name"));

        if (editFlag) {
            subtask.id = field.data("subtaskid");
            updateSubtask(params, subtask.taskId, subtask.id, subtask);
        } else {
            addSubtask(params, subtask.taskId, subtask);
        }
    };

    var showNewSubtaskField = function (addLink) {
        hideSubtaskFields();
        var subtask = getEmptySubtask();
        subtask.taskid = addLink.attr('taskid');
        subtask.projectid = addLink.attr("projectid");
        jq.tmpl("projects_fieldForAddSubtask", subtask).insertAfter(addLink);
        subtaskResponsible = null;
        setTeamForResponsibleSelect(subtask.projectid);
        addLink.hide();
        jq("#quickAddSubTaskField").removeClass("display-none");
        jq(".subtask-name-input").focus();
    };

    var editSubtask = function (subtaskid) {
        hideSubtaskFields();
        editFlag = true;
        var subtask = jq("#subtask_" + subtaskid);
        jq(subtask).removeClass('menuopen');

        var data = {};
        data.subtaskid = subtaskid;
        data.title = subtask.find(".taskName").text();
        var responsible = subtask.find(".user");
        if (responsible.length && !responsible.hasClass("not")) {
            data.responsible = responsible.attr('data-userId');
            subtaskResponsible = data.responsible;
        }else{
            data.responsible = null;
            subtaskResponsible = ASC.Projects.Common.emptyGuid;
        }
        data.taskid = subtask.attr('taskid');
        data.projectid = subtask.attr("projectid");
        
        jq.tmpl("projects_fieldForAddSubtask", data).insertAfter(subtask);
        setTeamForResponsibleSelect(data.projectid);
        var editField = jq("#quickAddSubTaskField");
        subtask.find(".taskName").hide();
        editField.addClass("absolute");
        var marginTop = subtask.position().top + "px"
        editField.css("top", marginTop);
        editField.removeClass("display-none");
        jq('.subtask-name-input').removeAttr('disabled');
        jq(".subtask-name-input").focus();
    };

    var showNewSubtask = function (params, subtask) {
        subtask.taskid = params.taskid;
        subtask.projectid = params.projectid;
        if (subtaskContainer.find('.subtask').length) {
            var elem = subtaskContainer.find('.quickAddSubTaskLink');
            jq.tmpl("projects_newSubtaskTemplate", subtask).insertBefore(elem);

        } else {
            jq.tmpl("projects_newSubtaskTemplate", subtask).prependTo(subtaskContainer);
            subtaskContainer.show();
        }
        jq(".subtask-loader").remove();
        var subtaskInput = jq('.subtask-name-input');
        jq(subtaskInput).removeAttr('disabled');
        jq(subtaskInput).val('');
        jq(subtaskInput).focus();
    };

    var replaceSubtask = function (subtask) {
        var subtaskid = subtask.id;
        var oldSubtask = jq('.subtask[subtaskid="' + subtaskid + '"]');
        subtask.taskid = jq(oldSubtask).attr('taskid');
        jq(oldSubtask).hide();
        jq.tmpl("projects_newSubtaskTemplate", subtask).insertBefore(oldSubtask);
        jq(oldSubtask).remove();
        hideSubtaskFields();
    };

    var changeSubtaskAtributes = function (oldSubtask, subtask) {
        var taskName = jq(oldSubtask).find('.taskName');
        jq(taskName).attr('created', subtask.displayDateCrtdate);
        if (subtask.createdBy) {
            jq(taskName).attr('createdBy', subtask.createdBy.displayName);
        }
        if (subtask.updatedBy) {
            jq(taskName).attr('updatedBy', subtask.updatedBy.displayName);
        }
        if (subtask.updated) {
            jq(taskName).attr('updated', subtask.displayDateUptdate);
        }
        jq(taskName).attr('status', subtask.status);
    };

    var hideSubtaskFields = function () {
        jq("#quickAddSubTaskField").remove();
        if (!editFlag) {
            jq(".task:not(.closed)").next().find(".quickAddSubTaskLink").show();
            jq("#subtaskContainer .quickAddSubTaskLink").show();
        }
        jq(".subtask .taskName").show();
        editedSubtaskId = null;
        editFlag = false;
    };

    var hideSubtaskActionPanel = function () {
        if (subtaskActionPanel.is(":visible")) {
            jq('.subtask[subtaskid=' + subtaskActionPanel.data("subtaskid") + ']').removeClass('menuopen');
            subtaskActionPanel.hide();
        }
    };

    return {
        init: init,
        hideSubtaskActionPanel: hideSubtaskActionPanel,
        hideSubtaskFields: hideSubtaskFields,
        addFirstSubtask: showNewSubtaskField,
        onAddSubtaskHandler: function () {
        },
        onRemoveSubtaskHandler: function () {
        },
        onChangeTaskStatusHandler: function () {
        }        
    };
})(jQuery);