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


ASC.Projects.SubtasksManager = (function () {
    var common = ASC.Projects.Common;

    var currentUserId,
        currentProjectId,
        teamsHash = {},
        $subtaskContainer = null,
        editFlag = false,
        subtaskResponsible = null,
        isInit,
        tasks,
        clickEventName = "click",
        $subtaskNameInput,
        teamlab,
        resources = ASC.Projects.Resources;

    // key codes
    var escKey = 27,
        enterKey = 13;

    var allUsers;

    var init = function () {
        if (isInit) {
            return;
        }

        isInit = true;
        teamlab = Teamlab;
        allUsers = UserManager.getAllUsers(true);

        currentUserId = teamlab.profile.id;
        currentProjectId = jq.getURLParam("prjID");

        if (currentProjectId) {
            teamsHash = {};
            teamsHash[currentProjectId] = ASC.Projects.Master.Team;
        }

        //add subtask panels

        ASC.Projects.DescriptionPanel.init(jq("#CommonListContainer"), null,
        {
            getItem: function (target) {
                var $targetObject = jq(target),
                    $targetParent = $targetObject.parents(".subtask"),
                    subtask = getFilteredSubTaskById($targetParent.attr("id"));

                return subtask;
            },
            selector: '.subtask .taskName span'
        });

        jq(document).on('dblclick', '.subtask .taskName', function () {
            var $self = jq(this);
            if (!$self.is('.canedit') || $self.parent('.subtask').is('.closed')) return false;

            var editedSubtaskId = $self.parent(".subtask").attr('id');
            editSubtask(editedSubtaskId);
            return true;
        });

        jq(document).on(clickEventName, ".subtask-add-button .button", function () {
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
            var self = jq(this).parents(".subtask"),
                subtaskId = self.attr('id'),
                subtask = getFilteredSubTaskById(subtaskId),
                taskId = subtask.taskid;

            if (jq(this).is(':checked')) {
                closeSubTask(self, taskId);
            } else {
                showSubtaskLoader(self.closest(".check"));
                updateSubtaskStatus({}, taskId, subtaskId, { status: 'open' });
            }
        });

        // click events
        jq(document).on(clickEventName, '.quickAddSubTaskLink span', function () {
            showNewSubtaskField(jq(this).parent());
            return false;
        });

        jq(document).on(clickEventName, "#quickAddSubTaskField", function (event) {
            $subtaskNameInput = jq('.subtask-name-input');
            $subtaskNameInput.focus();
        });

        teamlab.bind(teamlab.events.updatePrjTeam, function(params, team) {
            teamsHash[params.projectId] = team;
        });
    };

    function staEditHandler(subtaskid) {
        editSubtask(subtaskid);
        return false;
    };

    function staAcceptHandler(subtask, taskId) {
        var data = {
            title: subtask.title,
            responsible: currentUserId,
            description: ''
        };

        teamlab.updatePrjSubtask({}, taskId, subtask.id, data, { success: onUpdateSubtask });
        return false;
    };

    function staCopyHandler(subtaskid, taskid) {
        $subtaskContainer = jq('.subtasks[taskid=' + taskid + ']');
        teamlab.copyPrjSubtask({ taskid: taskid }, taskid, subtaskid, { success: onAddSubtask });
        return false;
    };

    function staRemoveHandler(subtaskid, taskId) {
        teamlab.removePrjSubtask({ taskId: taskId }, taskId, subtaskid, { success: onRemoveSubtask });
        return false;
    };

    function showEntityMenu(selectedActionCombobox) {
        var subtaskid = selectedActionCombobox.attr("id"),
            subtask = getFilteredSubTaskById(subtaskid),
            taskid = subtask.taskid,
            ActionMenuItem = ASC.Projects.ActionMenuItem;

        var menuItems = [];

        if (subtask.status != 2) {
            if (!subtask.responsible) {
                menuItems.push(new ActionMenuItem("sta_accept", resources.TasksResource.AcceptSubtask, staAcceptHandler.bind(null, subtask, taskid), "accept"));
            }

            if (subtask.canEdit) {
                menuItems.push(new ActionMenuItem("sta_edit", resources.TasksResource.Edit, staEditHandler.bind(null, subtaskid, taskid), "edit"));
                menuItems.push(new ActionMenuItem("sta_copy", resources.CommonResource.Copy, staCopyHandler.bind(null, subtaskid, taskid), "move-or-copy"));
            }
        }

        if (subtask.canEdit) {
            if (menuItems.length == 3) {
                menuItems.push(new ActionMenuItem(null, null, null, null, true));
            }

            menuItems.push(new ActionMenuItem("sta_remove", resources.CommonResource.Delete, staRemoveHandler.bind(null, subtaskid, taskid), "delete"));
        }
        return { menuItems: menuItems };
    }

    function onAddSubtask(params, subtask) {
        if ($subtaskContainer.find('.subtask').length) {
            var elem = $subtaskContainer.find('.quickAddSubTaskLink');
            jq.tmpl("projects_subtaskTemplate", subtask).insertBefore(elem);

        } else {
            jq.tmpl("projects_subtaskTemplate", subtask).prependTo($subtaskContainer);
            $subtaskContainer.show();
        }
        jq(".subtask-loader").remove();

        $subtaskNameInput = jq('.subtask-name-input');
        $subtaskNameInput.removeAttr('disabled');
        $subtaskNameInput.val('');
        $subtaskNameInput.focus();
    };

    function onUpdateSubtask(params, subtask) {
        jq('.subtask[id="' + subtask.id + '"]').replaceWith(jq.tmpl("projects_subtaskTemplate", subtask));
        hideSubtaskFields();
        setFilteredSubTask(subtask);
    };

    function closeSubTask($subtask, taskId) {
        showSubtaskLoader($subtask.find(".check"));
        setTimeout(function () { $subtask.yellowFade(); }, 0);

        var data = {
            title:jq.trim( $subtask.find(".taskName span").text()),
            responsible: currentUserId,
            status: 'closed'
        };

        updateSubtaskStatus({}, taskId, $subtask.attr("id"), data);
    };

    function updateSubtaskStatus(params, taskId, subtaskId, data) {
        teamlab.updatePrjSubtask(params, taskId, subtaskId, data, { success: onUpdateSubtaskStatus });
    };

    function onUpdateSubtaskStatus(params, subtask) {
        var $oldSubtask = getSubtaskElem(subtask.id);
        var $subtaskCont = jq($oldSubtask).closest('.subtasks');

        $oldSubtask.remove();

        if (subtask.status == 1) {
            jq.tmpl("projects_subtaskTemplate", subtask).prependTo($subtaskCont);
        } else {
            jq.tmpl("projects_subtaskTemplate", subtask).appendTo($subtaskCont);
        }

        jq('.subtask-loader').remove();
    };

    function onRemoveSubtask(params, subtask) {
        getSubtaskElem(subtask.id).remove();
    };

    function showSubtaskLoader(inputBox) {
        if (inputBox.hasClass("check")) {
            inputBox.find("input").hide();
        }
        inputBox.prepend('<div class="subtask-loader"></div>');
    };

    function setTeamForResponsibleSelect(projectId) {
        if (teamsHash && teamsHash.hasOwnProperty(projectId)) {
            initResponsibleSelector(teamsHash[projectId]);
        } else {
            teamlab.getPrjTeam({projectId: projectId}, projectId, {
                success: function (params, team) {
                    teamsHash[params.projectId] = team;
                    initResponsibleSelector(team);
                }
            });
        }
    };

    function initResponsibleSelector(team) {
        var validTeamMembers = common.excludeVisitors(team),
            nobody = {
                id: "00000000-0000-0000-0000-000000000000",
                displayName: ASC.Projects.Resources.TasksResource.NoResponsible
            },
            $responsibleSelector = jq(".subtask-responsible-selector");

        validTeamMembers = common.removeBlockedUsersFromTeam(validTeamMembers);
        validTeamMembers = [nobody].concat(validTeamMembers);

        function mapTeamMember(item) {
            return { id: item.id, title: item.displayName };
        }

        var choose = allUsers[subtaskResponsible];
        if (!choose) {
            choose = nobody;
        }

        $responsibleSelector.advancedSelector({
            showSearch: false,
            onechosen: true,
            itemsChoose: validTeamMembers.map(mapTeamMember),
            sortMethod: function () { return 0; },
            inPopup: true,
            $parent: jq(".mainPageContent"),
        }).on("showList", function (event, item) {
            $responsibleSelector.attr("data-id", item.id).html(item.title).attr("title", item.title);
            $subtaskNameInput = jq('.subtask-name-input');
            $subtaskNameInput.focus();
        });

        $responsibleSelector.advancedSelector("selectBeforeShow", mapTeamMember(choose));
    };

    function beforeSubtaskAction() {
        $subtaskNameInput = jq('.subtask-name-input');
        $subtaskContainer = $subtaskNameInput.closest(".subtasks");

        var subtask = {
            title: $subtaskNameInput.val().trim()
        };

        if (subtask.title.length == 0) {
            if (!editFlag) {
                hideSubtaskFields();
            } else {
                $subtaskNameInput.focus();
            }
            return false;
        }

        subtask.responsible = jq(".subtask-responsible-selector").attr("data-id");
        subtask.taskId = $subtaskContainer.attr("taskid");

        var params = {
            taskid: subtask.taskId
        };

        showSubtaskLoader($subtaskNameInput.closest(".subtask-name"));

        if (editFlag) {
            subtask.id = $subtaskNameInput.data("subtaskid");
            teamlab.updatePrjSubtask(params, subtask.taskId, subtask.id, subtask, { success: onUpdateSubtask });
        } else {
            teamlab.addPrjSubtask(params, subtask.taskId, subtask, { success: onAddSubtask });
        }
    };

    var showNewSubtaskField = function ($addLink) {
        hideSubtaskFields();

        var subtask = {
            subtaskid: "-1",
            title: "",
            projectid: "",
            responsible: null
        };

        var task = getFilteredTaskById($addLink.parents(".subtasks").attr('taskid'));
        if (!task) return;
        jq.tmpl("projects_fieldForAddSubtask", subtask).insertAfter($addLink);
        subtaskResponsible = null;
        setTeamForResponsibleSelect(task.projectId);
        $addLink.hide();
        jq("#quickAddSubTaskField").removeClass("display-none");

        $subtaskNameInput = jq('.subtask-name-input');
        $subtaskNameInput.focus();
    };

    function editSubtask(subtaskid) {
        hideSubtaskFields();
        editFlag = true;

        var $subtask = getSubtaskElem(subtaskid);
        var subtask = getFilteredSubTaskById(subtaskid);
        var taskid = subtask.taskid;

        var task = getFilteredTaskById(taskid);

        var data = {
            subtaskid: subtaskid,
            title: subtask.title,
            taskid: subtask.taskid,
            projectid: task.projectId,
            responsible: subtask.responsible == null ? null : subtask.responsible.id

        };

        subtaskResponsible = subtask.responsible == null ? common.emptyGuid : subtask.responsible.id;
        
        jq.tmpl("projects_fieldForAddSubtask", data).insertAfter($subtask);
        setTeamForResponsibleSelect(data.projectid);
        $subtask.find(".taskName").hide();

        var editField = jq("#quickAddSubTaskField");
        editField.addClass("absolute");
        editField.css("top", $subtask.position().top + "px");
        editField.removeClass("display-none");

        $subtaskNameInput = jq('.subtask-name-input');
        $subtaskNameInput.removeAttr('disabled');
        $subtaskNameInput.focus();
    };

    function hideSubtaskFields() {
        jq("#quickAddSubTaskField").remove();
        if (!editFlag) {
            jq(".task:not(.closed)").next().find(".quickAddSubTaskLink").show();
            jq("#subtaskContainer .quickAddSubTaskLink").show();
        }
        jq(".subtask .taskName").show();
        editFlag = false;
    };

    function setTasks(currentTasks) {
        tasks = currentTasks;
    }

    function getFilteredTaskById(taskId) {
        for (var i = 0, max = tasks.length; i < max; i++) {
            if (tasks[i].id == taskId) {
                return tasks[i];
            }
        }
    };

    function getFilteredSubTaskById(subtaskId) {
        for (var i = 0, max = tasks.length; i < max; i++) {
            var subtasks = tasks[i].subtasks;
            for (var j = 0, maxSubtasks = subtasks.length; j < maxSubtasks; j++) {
                if (subtasks[j].id == subtaskId) {
                    return subtasks[j];
                }
            }
        }
    };

    function setFilteredSubTask(subtask) {
        for (var i = 0, max = tasks.length; i < max; i++) {
            var subtasks = tasks[i].subtasks;
            for (var j = 0, maxSubtasks = subtasks.length; j < maxSubtasks; j++) {
                if (subtasks[j].id == subtask.id) {
                    subtasks[j] = subtask;
                    return;
                }
            }
        }
    };

    function getSubtaskElem(subtaskId) {
        return jq(".subtasks div[id=" + subtaskId + "]:visible");
    }

    return {
        init: init,
        hideSubtaskFields: hideSubtaskFields,
        addFirstSubtask: showNewSubtaskField,
        setTasks: setTasks,
        showEntityMenu: showEntityMenu
    };
})(jQuery);