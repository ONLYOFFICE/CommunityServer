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


ASC.Projects.MilestoneContainer = (function () {
    var $addTaskContainer,
        $addMilestoneContainer,
        $addMilestone,
        $taskActionPanel,
        $milestoneActionsPanel;

    var init = function () {
        $addTaskContainer = jq('#addTaskContainer'),
        $addMilestoneContainer = jq("#addMilestoneContainer"),
        $addMilestone = jq("#addMilestone"),
        $taskActionPanel = jq('#taskActionPanel'),
        $milestoneActionsPanel = jq("#milestoneActions");

        var href = location.href.toLowerCase();

        if (href.indexOf('projects.aspx') > 0) {
            ASC.Projects.CreateProjectStructure.init(jq.tmpl("projects_choose_responsible")[0].outerHTML);
        }
        
        if (href.indexOf('projecttemplates.aspx') > 0) {
            ASC.Projects.EditMilestoneContainer.init();
        }
        
        jq("#newMilestoneTitle, #newTaskTitle").val('');

        jq(document).on('click', ".task .entity-menu", function () {
            entityMenuOnClick(this, $taskActionPanel, '.task');
            return false;
        });
        jq(document).on('click', ".milestone .mainInfo .entity-menu", function () {
            entityMenuOnClick(this, $milestoneActionsPanel, '.milestone');
            return false;
        });

        function entityMenuOnClick(self, actionPanel, parent) {
            hideAddTaskContainer();
            hideAddMilestoneContainer();
            var target = jq(self).parents(parent).attr('id');
            actionPanel.attr('target', target);
            jq("#" + target).addClass("open");
            showActionsPanel(actionPanel, self);
        }

        jq(document).on('click', ".addTaskContainer .link", function () {
            hideAddMilestoneContainer();
            if ($addTaskContainer.hasClass('edit')) {
                jq('#' + $addTaskContainer.attr('target')).show();
                $addTaskContainer.removeClass('edit');
            }
            jq("#newTaskTitle").val("");
            var target;
            var parent = jq(this).parent().parent();
            if (jq(parent).attr("id") === "noAssignTaskContainer") {
                target = "noAssign";
            } else {
                target = jq(parent).parent().attr("id");
            }
            if ($addTaskContainer.attr("target") !== "") {
                var elem = $addTaskContainer.parent();
                if (jq(elem).attr('id') === "noAssignTaskContainer") {
                    jq("#noAssignTaskContainer .addTaskContainer").appendTo("#noAssignTaskContainer");
                }
                jq(elem).children(".addTaskContainer").show();
            }

            var chooseRespBlock = $addTaskContainer.find(".chooseResponsible");

            if (!chooseRespBlock.length) {
                jq("#addTaskContainer div:first-child").append(jq(".chooseResponsible:first").clone());
            }

            $addTaskContainer.attr("target", target);
            $addTaskContainer.appendTo(parent);
            jq(parent).children(".addTaskContainer").hide();
            $addTaskContainer.show();
            jq("#addTaskContainer #newTaskTitle").focus();
        });

        jq('body').click(function (event) {
            var elt = (event.target) ? event.target : event.srcElement;
            var isHide = true;
            var panelId = "taskActionPanel";
            if (jq(elt).is('[id="' + panelId + '"]') || jq(elt).parent().is('[class*="chooseResponsible"]') || jq(elt).is('[class*="chooseResponsible"]') ||
               (jq(elt).parents().is(".studio-action-panel") && jq(elt).parents(".studio-action-panel").find("input").length)) {
                isHide = false;
            }

            if (isHide) {
                jq(elt).parents().each(function () {
                    if (jq(this).is('[id="' + panelId + '"]')) {
                        isHide = false;
                        return false;
                    }
                    return false;
                });
            }

            if (isHide) {
                jq('.studio-action-panel').hide();
                jq('.milestone').removeClass('open');
                jq('.task').removeClass('open');
                jq(".template").removeClass('open');
            }
        });
    };

    function showActionsPanel ($panel, obj) {
        var x, y;

        jq('.studio-action-panel').hide();

        x = jq(obj).offset().left;
        y = jq(obj).offset().top;

        if ($panel.attr("id") === "projectMemberPanel") {
            x = x - 21;
            y = y + 20;
        } else {
            x = x - $panel.outerWidth() + jq(obj).outerWidth();
            y = y + jq(obj).outerHeight();
        }

        $panel.css("left", x + "px");
        $panel.css("top", y + "px");
        $panel.show();
    };

    var showAction = function (target) {
        if (target === 'noAssign') {
            var listNoAssignTask = jq("#listNoAssignListTask .task");
            if (listNoAssignTask.length > 0) {
                jq("#noAssignTaskContainer .addTaskContainer").appendTo(jq("#noAssignTaskContainer"));
                jq("#noAssignTaskContainer .addTaskContainer").show();
            }
        }
    };

    function hideAddMilestoneContainer() {
        $addMilestoneContainer.hide();
        if ($addMilestoneContainer.hasClass("edit")) {
            var target = $addMilestoneContainer.attr("target");
            jq("#" + target + " .mainInfo").show();
            $addMilestoneContainer.removeClass("edit");
        }
        if ($addMilestoneContainer.hasClass('edit')) {
            jq("#" + $addMilestoneContainer.attr('target')).find(".mainInfo").show();
        }
        jq(".addTaskContainer").show();
        $addMilestoneContainer.find("#newMilestoneTitle").val('');
        $addMilestone.show();
    };

    function hideAddTaskContainer() {
        $addTaskContainer.hide();
        jq('.task').show();
        var target = $addTaskContainer.attr("target");
        if (target === "") return;
        var elem = jq("#" + target);
        var containerTask;
        if (jq(elem).hasClass("milestone")) {
            containerTask = jq(elem).find(".milestoneTasksContainer");
            if (jq(containerTask).find(".task").length === 0) {
                jq(containerTask).hide();
                jq(containerTask).closest(".milestone").find(".addTask").removeClass("hide");
                jq(containerTask).find(".addTaskContainer").hide();
            } else {
                jq(containerTask).find(".addTaskContainer").show();
            }
        } else {
            var container = jq(elem).parent();
            if (target === "noAssign" || jq(container).attr('id') === "listNoAssignListTask") {
                jq("#noAssignTaskContainer .addTaskContainer").show();
            } else {
                containerTask = jq(elem).closest(".milestoneTasksContainer");
                if (jq(containerTask).find(".task").length === 0) {
                    jq(containerTask).hide();
                    jq(containerTask).closest(".milestone").find(".addTask").removeClass("hide");
                    jq(containerTask).find(".addTaskContainer").hide();
                } else {
                    jq(containerTask).find(".addTaskContainer").show();
                }
            }
        }
        $addTaskContainer.removeAttr("target");
        jq("#newTaskTitle").val('');
    };

    return {
        init: init,
        showActionsPanel: showActionsPanel,
        showAction: showAction,
        hideAddMilestoneContainer: hideAddMilestoneContainer,
        hideAddTaskContainer: hideAddTaskContainer
    };
})(jQuery);

ASC.Projects.EditMilestoneContainer = (function () {
    var milestoneCounter = 0,
        taskCounter = 0;
    var $addTaskContainer;
    var $addMilestoneContainer;
    var $addMilestone;

    var $taskActionPanel;
    var $milestoneActionsPanel;
    var milestoneContainer = ASC.Projects.MilestoneContainer;

    var init = function () {
        $addTaskContainer = jq('#addTaskContainer');
        $addMilestoneContainer = jq('#addMilestoneContainer');
        $addMilestone = jq('#addMilestone');

        $taskActionPanel = jq('#taskActionPanel');
        $milestoneActionsPanel = jq("#milestoneActions");

        //milestone
        $addMilestone.find("a").bind('click', function () {
            milestoneContainer.hideAddTaskContainer();
            $addMilestoneContainer.hide();
            if ($addMilestoneContainer.hasClass('edit')) {
                jq("#" + $addMilestoneContainer.attr('target')).find(".mainInfo").show();
                $addMilestoneContainer.removeClass('edit');
            }

            $addMilestone.after($addMilestoneContainer);
            $addMilestone.hide();
            jq("#newMilestoneTitle").val('');

            if (jq("#dueDate").length) {
                var defDate = new Date();
                defDate.setDate(defDate.getDate() + 3);
                jq("#dueDate").datepicker("setDate", defDate);
            }

            $addMilestoneContainer.show();

            jq("#newMilestoneTitle").focus();
        });

        jq("body").on('keydown', '#newMilestoneTitle', function (e) {
            $addMilestoneContainer.removeClass("red-border");
            var targetId = $addMilestoneContainer.attr('target');
            if (e.which === 13) {
                $addMilestoneContainer.find(".button").click();
            } else {
                if (e.which === 27) {
                    jq(this).val("");
                    if ($addMilestoneContainer.hasClass('edit')) {
                        $addMilestoneContainer.hide();
                        jq("#" + targetId + " .mainInfo").show();
                    } else {
                        $addMilestoneContainer.hide();
                        $addMilestone.show();
                    }
                }
            }
        });

        jq("body").on('click', "#addMilestoneContainer .button", function () {
            var targetId = $addMilestoneContainer.attr('target');
            var milestoneTitle = jq("#newMilestoneTitle");

            $addMilestoneContainer.removeClass("red-border");

            var text = jq.trim(milestoneTitle.val());
            if (!text.length) {
                alert(jq("#milestoneError").text());
                return;
            }
            if ($addMilestoneContainer.hasClass('edit')) {

                jq("#" + targetId + " .mainInfo .titleContainerEdit span").text(jq.trim(milestoneTitle.val()));
                var days = $addMilestoneContainer.find("select option:selected").attr('value');
                jq("#" + targetId + " .mainInfo .daysCount span").text(days);
                jq("#" + targetId + " .mainInfo .daysCount").attr('value', days);

                $addMilestoneContainer.hide();
                jq("#" + targetId + " .mainInfo").show();
                $addMilestoneContainer.removeClass('edit');
            } else {
                milestoneCounter++;
                var milestone = {
                    title: jq.trim(milestoneTitle.val()), duration: $addMilestoneContainer.find("select option:selected").attr('value'),
                    tasks: [], number: milestoneCounter
                };

                jq.tmpl("projects_templatesEditMilestoneTmpl", milestone).appendTo("#listAddedMilestone");
                milestoneTitle.val("");
                milestoneTitle.focus();
            }
        });

        //milestone menu
        jq(document).on('click', ".milestone .mainInfo .title, .milestone .mainInfo .daysCount", function () {
            milestoneContainer.hideAddTaskContainer();
            milestoneContainer.hideAddMilestoneContainer();
            $addMilestoneContainer.addClass('edit');
            var target = jq(this).closest('.milestone').attr('id');
            $addMilestoneContainer.attr('target', target);
            var milestone = jq("#" + target + " .mainInfo");
            $addMilestoneContainer.prependTo(jq("#" + target));
            jq(milestone).hide();
            var val = jq(milestone).find(".daysCount").attr('value');
            $addMilestoneContainer.show();
            $addMilestoneContainer.find("#newMilestoneTitle").val(jq(milestone).children(".titleContainerEdit").text());
            $addMilestoneContainer.find("select option[value = '" + val + "']").attr("selected", "selected");
            $addMilestoneContainer.find("#newMilestoneTitle").focus();

        });
        jq(document).on('click', ".milestone .mainInfo .addTask", function () {
            milestoneContainer.hideAddTaskContainer();
            var target = jq(this).closest('.milestone').attr('id');
            var milestTasksCont = jq("#" + target + " .milestoneTasksContainer");
            $addTaskContainer.appendTo(milestTasksCont[0]);
            jq(milestTasksCont).find('.addTaskContainer').hide();
            $addTaskContainer.attr("target", target);
            $addTaskContainer.show();
            jq(milestTasksCont).show();
            jq("#addTaskContainer #newTaskTitle").focus();
        });

        jq("#milestoneActions .actionList #removeMilestone").bind('click', function () {
            milestoneContainer.hideAddTaskContainer();
            $addTaskContainer.appendTo("#noAssignTaskContainer");
            $milestoneActionsPanel.hide();
            var target = jq(this).parents('.studio-action-panel').attr('target');
            jq("#" + target).removeClass("open");
            jq("#" + target).remove();
        });

        jq("#milestoneActions .actionList #addTaskInMilestone").bind('click', function () {
            $milestoneActionsPanel.hide();
            var target = jq(this).parents('.studio-action-panel').attr('target');
            jq("#" + target).removeClass("open");
            var listTasks = jq(".listTasks[milestone='" + target + "']");
            var milestTasksCont = jq(listTasks[0]).closest(".milestoneTasksContainer");
            jq(milestTasksCont).find('.addTaskContainer').hide();
            $addTaskContainer.appendTo(milestTasksCont[0]);
            $addTaskContainer.attr("target", target);
            $addTaskContainer.show();
            jq(milestTasksCont).find(".addTaskContainer").hide();
            jq(milestTasksCont).show();
            jq("#addTaskContainer #newTaskTitle").focus();
        });

        jq("#milestoneActions .actionList #editMilestone").bind('click', function () {
            $milestoneActionsPanel.hide();
            milestoneContainer.hideAddMilestoneContainer();
            $addMilestoneContainer.addClass('edit');
            var target = jq(this).parents('.studio-action-panel').attr('target');
            $addMilestoneContainer.attr('target', target);
            jq("#" + target).removeClass("open");
            var milestone = jq("#" + target + " .mainInfo");
            $addMilestoneContainer.prependTo(jq("#" + target));
            jq(milestone).hide();
            var val = jq(milestone).children(".daysCount").attr('value');
            $addMilestoneContainer.show();
            $addMilestoneContainer.find("#newMilestoneTitle").val(jq(milestone).children(".titleContainerEdit").text());
            $addMilestoneContainer.find("select option[value = '" + val + "']").attr("selected", "selected");
            $addMilestoneContainer.find("#newMilestoneTitle").focus();
        });

        //task

        jq(document).on('click', ".task .title", function () {
            milestoneContainer.hideAddMilestoneContainer();
            $addTaskContainer.hide();
            if ($addTaskContainer.hasClass('edit')) {
                jq('#' + $addTaskContainer.attr('target')).show();
            } else {
                $addTaskContainer.addClass('edit');
                jq('.addTaskContainer').show();
            }

            var target = jq(this).parents('.task');

            $addTaskContainer.attr('target', jq(target).attr("id"));
            $addTaskContainer.insertAfter(target);
            jq(target).hide();
            jq("#addTaskContainer #newTaskTitle").val(jq(target).children(".titleContainer").text());
            $addTaskContainer.show();
            jq("#addTaskContainer #newTaskTitle").focus();
        });

        jq("#newTaskTitle").bind('keydown', function (e) {
            var taskContainer = $addTaskContainer;
            taskContainer.removeClass("red-border");
            var target = taskContainer.attr('target');
            if (e.which === 13) {
                jq("#addTaskContainer .button").click();
            } else {
                if (e.which === 27) {
                    jq(this).val("");
                    if (taskContainer.hasClass('edit')) {
                        milestoneContainer.hideAddTaskContainer();
                        taskContainer.removeClass('edit');
                        jq("#" + target).show();
                    } else {
                        milestoneContainer.hideAddTaskContainer();
                    }
                }
            }
        });

        jq("#addTaskContainer .button").on("click", function () {
            var taskContainer = jq("#addTaskContainer");
            var target = taskContainer.attr('target');
            var taskTitle = jq("#newTaskTitle");
            taskContainer.removeClass("red-border");

            var text = jq.trim(taskTitle.val());
            if (!text.length) {
                alert(jq("#taskError").text());
                return false;
            }
            if (taskContainer.hasClass('edit')) {
                jq("#" + target + " .titleContainer span").text(taskTitle.val());
                taskContainer.removeClass('edit');
                milestoneContainer.hideAddTaskContainer();
                jq("#" + target).show();
            } else {
                taskCounter++;
                var task = { title: jq.trim(taskTitle.val()), number: taskCounter };
                var tElem = target === 'noAssign' ? jq("#listNoAssignListTask") : jq(".listTasks[milestone='" + target + "']");
                jq.tmpl("projects_templatesEditTaskTmpl", task).appendTo(tElem);
                taskTitle.val("");
                taskTitle.focus();
            }

            return true;
        });

        //task menu

        jq("#taskActionPanel .actionList #removeTask").bind('click', function () {
            $taskActionPanel.hide();
            var target = jq(this).parents('.studio-action-panel').attr('target');
            jq("#" + target).removeClass("open");
            var targetParent = jq("#" + target).parent();
            jq("#" + target).remove();
            if (jq(targetParent).hasClass('listTasks')) {
                if (jq(targetParent).children('.task').length === 0) {
                    jq(targetParent).closest('.milestone').find('.milestoneTasksContainer').hide();
                    jq(targetParent).closest(".milestone").find(".addTask").removeClass("hide");
                }
            }
        });

        jq("#taskActionPanel .actionList #editTask").bind('click', function () {
            $taskActionPanel.hide();
            var target = jq(this).parents('.studio-action-panel').attr('target');
            var task = jq("#" + target);
            jq(task).removeClass("open");
            $addTaskContainer.addClass('edit');
            $addTaskContainer.attr('target', target);
            $addTaskContainer.insertAfter(task);
            jq(task).hide();
            jq("#addTaskContainer #newTaskTitle").val(jq(task).children(".titleContainer").text());
            $addTaskContainer.show();
            jq("#addTaskContainer #newTaskTitle").focus();
        });
    };

    var showTmplStructure = function (tmpl) {
        var description = { tasks: [], milestones: [] };
        try {
            description = jq.parseJSON(tmpl.description) || { tasks: [], milestones: [] };
        } catch (e) {

        }
        var i;
        var milestones = description.milestones;
        if (milestones) {
            for (i = 0; i < milestones.length; i++) {
                milestoneCounter++;
                if (milestones[i].duration || milestones[i].duration > 6) {
                    var duration = $addMilestoneContainer.find("select option[duration='" + milestones[i].duration + "']").text();
                    if (duration === "") {
                        duration = milestones[i].duration.toString();
                        duration = duration.replace('.', ',');
                        milestones[i].duration = $addMilestoneContainer.find("select option[duration^='" + duration + "']").text();
                    } else {
                        milestones[i].duration = duration;
                    }
                } else {
                    milestones[i].duration = $addMilestoneContainer.find("select option:first-child").text();
                }
                milestones[i].number = milestoneCounter;
                milestones[i].displayTasks = milestones[i].tasks.length ? true : false;
                jq.tmpl("projects_templatesEditMilestoneTmpl", milestones[i]).appendTo("#listAddedMilestone");
            }
        }
        var noAssignTasks = description.tasks;
        if (noAssignTasks) {
            for (i = 0; i < noAssignTasks.length; i++) {
                taskCounter++;
                noAssignTasks[i].number = taskCounter;
                jq.tmpl("projects_templatesEditTaskTmpl", noAssignTasks[i]).appendTo("#listNoAssignListTask");
            }
            jq("#addTaskContainer").attr("target", 'noAssign');
            milestoneContainer.hideAddTaskContainer();
            milestoneContainer.showAction('noAssign');
        }
    };

    return {
        init: init,
        showTmplStructure: showTmplStructure
    };

})(jQuery);

ASC.Projects.CreateProjectStructure = (function () {
    var prjTitleContainer,
        dueDateContainer,
        teamContainer;
    var $projectMemberPanel;
    var $addTaskContainer;
    var $addMilestoneContainer;

    var $taskActionPanel;
    var $milestoneActionsPanel;

    var $addMilestone;
    var selectedTeam = [];

    var regionalFormatDate, chooseRespStr, showRespCombFlag = false,
        milestoneCounter = 0, taskCounter = 0;
    var pmId = null;
    var milestoneContainer = ASC.Projects.MilestoneContainer;
    var isInit = false;

    var showChooseResponsible = function () {
        if (selectedTeam.length === 0 && !pmId) {
            jq(".chooseResponsible").remove();
            showRespCombFlag = false;
            return;
        }
        if (!showRespCombFlag) {
            if (!jq(".chooseResponsible").length) {
                jq(".milestone .mainInfo .chooseResponsible").removeClass("nobody");
                jq("#addTaskContainer #newTaskTitle").after(chooseRespStr);
            }

            jq(".task, .milestone .mainInfo").find(".entity-menu").each(function () {
                if (!jq(this).siblings(".chooseResponsible").length) {
                    jq(this).after(chooseRespStr);
                }
            });

            var chooseRespBlock = jq("#addMilestoneContainer").find(".chooseResponsible");

            if (!chooseRespBlock.length) {
                jq("#addMilestoneContainer #newMilestoneTitle").after(chooseRespStr);
            }

            showRespCombFlag = true;
        }
        updateProjectMemberPanel();
    };

    function updateProjectMemberPanel() {
        jq("#projectMemberPanel .actionList li").remove();
        var pmName = jq("#projectManagerSelector").attr("data-id") ? jq("#projectManagerSelector").html() : "";

        if (pmId)
            jq("#projectMemberPanel .actionList").append("<li id='" + pmId + "' class='dropdown-item'>" + pmName + "</li>");

        for (var i = 0; i < selectedTeam.length; i++) {
            if (selectedTeam[i].isVisitor) continue;
            jq("#projectMemberPanel .actionList").append("<li id='" + selectedTeam[i].id + "' class='dropdown-item'>" + selectedTeam[i].title + "</li>");
        }
        
        updateMilestoneAndTaskResponsible();
        jq("#projectMemberPanel .actionList").prepend("<li id='nobody' class='dropdown-item'>" + jq("#projectMemberPanel .actionList").attr("nobodyItemText") + "</li>");
    };
    
    var responsibleInTeam = function (entity, team) {
        var oldResp = jq(entity).attr("guid");
        return team.some(function (item) { return item.id === oldResp || (pmId && pmId === oldResp); });
    };
    
    function updateMilestoneAndTaskResponsible() {
        var pmName = jq("#projectManagerSelector").attr("data-id") ? jq("#projectManagerSelector").html() : "";
        var listEntities = jq(".milestone .mainInfo .chooseResponsible .link.dotline, .task .chooseResponsible .link[guid], #addTaskContainer .chooseResponsible .link[guid], #addMilestoneContainer .chooseResponsible .link");

        var check = false;

        for (var i = 0; i < listEntities.length; i++) {
            var $listEntity = jq(listEntities[i]);
            if (!responsibleInTeam($listEntity, selectedTeam)) {
                if (pmId) {
                    $listEntity.attr("guid", pmId);
                    $listEntity.html(pmName);
                    if (jq(listEntities[i]).parents("#addMilestoneContainer").length === 0) {
                        check = true;
                    }
                }
                else {
                    var resp = selectedTeam.find(function (item) { return !item.isVisitor; });
                    if (resp) {
                        jq(listEntities[i]).attr("guid", resp.id);
                        jq(listEntities[i]).html(resp.title);
                    } else {
                        jq(listEntities[i]).closest(".chooseResponsible").remove();
                    }
                }
                jq(listEntities[i]).parent().css("display", "inline-block");
            }
        }

        if (check) {
            jq(document).trigger("chooseResp");
        }
    };
    var $projectManagerSelector,
        $projectTeamSelector;

    var init = function (str) {
        isInit = true;
        chooseRespStr = str;
        prjTitleContainer = jq("input[id*='projectTitle']"),
        dueDateContainer = jq("#dueDate"),
        teamContainer = jq("#Team").length ? jq("#Team") : jq("#projectParticipantsContainer");
        $projectMemberPanel = jq("#projectMemberPanel");
        $addTaskContainer = jq('#addTaskContainer');
        $addMilestoneContainer = jq('#addMilestoneContainer');

        $taskActionPanel = jq('#taskActionPanel');
        $milestoneActionsPanel = jq("#milestoneActions");

        $addMilestone = jq("#addMilestone");
        $projectManagerSelector = jq("#projectManagerSelector"),
            $projectTeamSelector = jq("#projectTeamSelector");

        $projectManagerSelector.useradvancedSelector({
            withGuests: false,
            showGroups: true,
            onechosen: true
        }).on("showList", onChooseProjectManager);

        $projectTeamSelector.useradvancedSelector({
            showGroups: true
        }).on("showList", onChooseProjectTeam);

        //datepicker
        dueDateContainer.val("");
        dueDateContainer.datepicker().mask(ASC.Resources.Master.DatePatternJQ);
        dueDateContainer.datepicker({
            onSelect: function () {
                jq("#newMilestoneTitle").focus();
            }
        });
        regionalFormatDate = jq("#dueDate").datepicker("option", "dateFormat");
       

        jq("body").click(function (event) {
            if (event.target.className !== "userName") {
                if (!jq("#projectManagerSelector").attr("data-id")) {
                    pmId = null;
                    //showChooseResponsible();
                }
            }
        });

        //choose responsible

        function onChooseResponsible(self, itemClass, nobodyDisabled) {
            jq(this).closest(itemClass).addClass('open');
            onChooseResponsibleContainer(self, jq(self).closest(itemClass).attr("id"), nobodyDisabled);
        }

        function onChooseResponsibleContainer(self, target, nobodyDisabled) {
            milestoneContainer.showActionsPanel($projectMemberPanel, self);
            var $nobody = $projectMemberPanel.find("#nobody");
            if (nobodyDisabled) {
                $nobody.hide();
            } else {
                $nobody.show();
            }

            $projectMemberPanel.attr("target", target);
        }

        jq(document).on("click", ".task .chooseResponsible", function () {
            onChooseResponsible(this, ".task");
        });

        jq(document).on("click", ".milestone .mainInfo .chooseResponsible", function () {
            onChooseResponsible(this, ".milestone", true);
        });

        $addTaskContainer.on("click", ".chooseResponsible", function () {
            onChooseResponsibleContainer(this, "newTask");
        });

        $addMilestoneContainer.on("click", ".chooseResponsible", function () {
            onChooseResponsibleContainer(this, "newMilestone", true);
        });

        $projectMemberPanel.on("click", "ul li", function () {
            jq(".studio-action-panel").hide();
            var target = $projectMemberPanel.attr("target");
            jq("#" + target).removeClass("open");
            var type = jq("#" + target).attr("class");
            switch (type) {
                case "milestone":
                    {
                        target = jq("#" + target + " .mainInfo");
                        break;
                    }
                case "task menuButtonContainer with-entity-menu":
                    {
                        target = jq("#" + target);
                        break;
                    }
                default:
                    {
                        if (target === "newTask") {
                            target = $addTaskContainer;
                        } else {
                            target = $addMilestoneContainer;
                        }
                    }
            }

            var guid = jq(this).attr("id");
            var name;
            if (guid === "nobody" || guid === "") {
                name = jq("#projectMemberPanel .actionList").attr("chooseRespText");
                if (type !== "newTask") {
                    jq(target).find(".chooseResponsible").addClass("nobody");
                }
            } else {
                name = jq(this).text();
                jq(target).find(".chooseResponsible").removeClass("nobody");
            }

            var member = jq(target).find(".link.dotline");

            if (guid !== "nobody" && guid !== "") {
                jq(document).trigger("chooseResp");
                jq(member).attr("guid", guid);
            } else {
                jq(member).removeAttr("guid");
            }

            jq(member).text(name);
            jq(target).find("input").last().focus();

        });


        //milestone
        jq(document).on('click', ".milestone .mainInfo .title, .milestone .mainInfo .dueDate", function () {
            milestoneContainer.hideAddMilestoneContainer();
            milestoneContainer.hideAddTaskContainer();
            $addMilestoneContainer.addClass('edit');
            var target = jq(this).closest('.milestone').attr('id');
            $addMilestoneContainer.attr('target', target);
            var milestone = jq("#" + target + " .mainInfo");
            $addMilestoneContainer.prependTo(jq("#" + target));
            jq(milestone).hide();
            var val = jq(milestone).children(".dueDate").text();
            val = jq.datepicker.parseDate(regionalFormatDate, val);
            var chooseRespBlock = jq(milestone).find(".chooseResponsible").clone();

            if (!chooseRespBlock.length) {
                $addMilestoneContainer.find("div:first-child").append(jq(chooseRespBlock));
            }

            var pm = jq("#projectManagerSelector").attr("data-id") ? jq("#projectManagerSelector").html() : "";
            if (pmId) {
                jq(chooseRespBlock).find(".link.dotline").attr("guid", pmId);
                jq(chooseRespBlock).find(".link.dotline").text(pm);
            }

            $addMilestoneContainer.show();
            $addMilestoneContainer.find("#newMilestoneTitle").val(jq(milestone).children(".titleContainer").text());
            $addMilestoneContainer.find("#dueDate").datepicker('setDate', val);
            $addMilestoneContainer.find("#newMilestoneTitle").focus();
        });

        $addMilestone.find("a").bind('click', function () {
            milestoneContainer.hideAddTaskContainer();
            $addMilestoneContainer.hide();
            if ($addMilestoneContainer.hasClass('edit')) {
                jq("#" + $addMilestoneContainer.attr('target')).find(".mainInfo").show();
                $addMilestoneContainer.removeClass('edit');
            }

            $addMilestone.after($addMilestoneContainer);
            $addMilestone.hide();
            jq("#newMilestoneTitle").val('');

            if (jq("#dueDate").length) {
                var defDate = new Date();
                defDate.setDate(defDate.getDate() + 3);
                jq("#dueDate").datepicker("setDate", defDate);
            }
            var chooseRespBlock = $addMilestoneContainer.find(".chooseResponsible");

            if (!chooseRespBlock.length) {
                $addMilestoneContainer.find("div:first-child").append(jq(".chooseResponsible:first").clone());

                var pm = jq("#projectManagerSelector").attr("data-id");
                if (pmId) {
                    jq(chooseRespBlock).find(".link.dotline").attr("guid", pmId);
                    jq(chooseRespBlock).find(".link.dotline").text(pm);
                }
            }
            $addMilestoneContainer.show();
            $addMilestoneContainer.find("#newMilestoneTitle").focus();
        });

        jq("body").on('keydown', '#newMilestoneTitle', function (e) {
            $addMilestoneContainer.removeClass("red-border");
            var targetId = $addMilestoneContainer.attr('target');
            if (e.which === 13) {
                $addMilestoneContainer.find(".button").click();
            } else {
                if (e.which === 27) {
                    jq(this).val("");
                    if ($addMilestoneContainer.hasClass('edit')) {
                        $addMilestoneContainer.hide();
                        jq("#" + targetId + " .mainInfo").show();
                    } else {
                        $addMilestoneContainer.hide();
                        $addMilestone.show();
                    }
                }
            }
        });

        jq("body").on('click', "#addMilestoneContainer .button", function () {
            var milestoneTitle = jq("#newMilestoneTitle");
            var targetId = $addMilestoneContainer.attr('target');

            $addMilestoneContainer.removeClass("red-border");

            var text = jq.trim(milestoneTitle.val());
            if (!text.length) {
                alert(jq("#milestoneError").text());
                return false;
            }
            var milestoneId;
            var date = jq("#dueDate").datepicker("getDate");
            date = jq.datepicker.formatDate(regionalFormatDate, date);
            if ($addMilestoneContainer.hasClass('edit')) {

                jq("#" + targetId + " .mainInfo .titleContainer span").text(jq.trim(milestoneTitle.val()));

                jq("#" + targetId + " .mainInfo .dueDate span").text(date);
                $addMilestoneContainer.hide();
                jq("#" + targetId + " .mainInfo").show();
                $addMilestoneContainer.removeClass('edit');
                milestoneId = targetId;
                jq("#" + targetId + " .mainInfo .chooseResponsible").remove();

                if ($addMilestoneContainer.find(".chooseResponsible").length) {
                    var chooseRespBlock = $addMilestoneContainer.find(".chooseResponsible").clone();
                    jq("#" + milestoneId + " .mainInfo .entity-menu").after(jq(chooseRespBlock));
                    if (jq(chooseRespBlock).attr('guid') !== "") {
                        jq("#" + milestoneId + " .mainInfo .chooseResponsible").removeClass("nobody");
                    } else {
                        jq("#" + milestoneId + " .mainInfo .chooseResponsible").addClass("nobody");
                    }
                }
            } else {
                milestoneCounter++;
                var milestone = {
                    title: jq.trim(milestoneTitle.val()),
                    date: date,
                    tasks: [],
                    number: milestoneCounter,
                    chooseRep: {
                        id: $addMilestoneContainer.find(".link.dotline").attr("guid"),
                        name: $addMilestoneContainer.find(".link.dotline").text()
                    }
                };

                jq.tmpl("projects_templatesCreateMilestoneTmpl", milestone).appendTo("#listAddedMilestone");
                milestoneTitle.val("");
                milestoneTitle.focus();

                if (milestone.chooseRep.id) {
                    jq(document).trigger("chooseResp");
                }
            }
            return true;
        });

        //milestone menu

        jq(document).on('click', ".milestone .mainInfo .addTask", function () {
            milestoneContainer.hideAddMilestoneContainer();
            milestoneContainer.hideAddTaskContainer();
            var target = jq(this).closest('.milestone').attr('id');
            var milestTasksCont = jq("#" + target + " .milestoneTasksContainer");
            jq(milestTasksCont).find('.addTaskContainer').hide();
            $addTaskContainer.appendTo(milestTasksCont[0]);
            $addTaskContainer.attr("target", target);
            $addTaskContainer.removeClass("edit");
            $addTaskContainer.show();
            var chooseRespBlock = $addTaskContainer.find(".chooseResponsible");

            if (!chooseRespBlock.length) {
                jq("#addTaskContainer div:first-child").append(jq(".chooseResponsible:first").clone());
            }
            jq(milestTasksCont).show();
            jq("#addTaskContainer #newTaskTitle").focus();
        });

        jq("#milestoneActions .actionList #removeMilestone").bind('click', function () {
            $addMilestone.after($addMilestoneContainer);
            $addTaskContainer.hide();
            $addTaskContainer.appendTo("#noAssignTaskContainer");
            $milestoneActionsPanel.hide();
            var target = jq(this).parents('.studio-action-panel').attr('target');
            jq("#" + target).removeClass("open");
            jq("#" + target).remove();
            if (jq("#listAddedMilestone .milestone").length == 0 && jq("#listNoAssignListTask .task").length == 0 && selectedTeam.length === 0) {
                jq(document).trigger("unchooseResp");
            }
        });

        jq("#milestoneActions .actionList #addTaskInMilestone").bind('click', function () {
            milestoneContainer.hideAddMilestoneContainer();
            var target = jq(this).parents('.studio-action-panel').attr('target');
            jq("#" + target).removeClass("open");
            var listTasks = jq(".listTasks[milestone='" + target + "']");
            var milestTasksCont = jq(listTasks[0]).parents(".milestoneTasksContainer");
            $addTaskContainer.appendTo(milestTasksCont[0]);
            jq(milestTasksCont).find('.addTaskContainer').hide();
            $addTaskContainer.attr("target", target);
            var chooseRespBlock = $addTaskContainer.find(".chooseResponsible");

            if (!chooseRespBlock.length) {
                jq("#addTaskContainer div:first-child").append(jq(".chooseResponsible:first").clone());
            }

            $addTaskContainer.show();
            jq(milestTasksCont).show();
            jq("#addTaskContainer #newTaskTitle").focus();
        });

        jq("#milestoneActions .actionList #editMilestone").bind('click', function () {
            $milestoneActionsPanel.hide();
            milestoneContainer.hideAddMilestoneContainer();
            $addMilestoneContainer.addClass('edit');
            var target = jq(this).parents('.studio-action-panel').attr('target');
            $addMilestoneContainer.attr('target', target);
            jq("#" + target).removeClass("open");
            var milestone = jq("#" + target + " .mainInfo");
            $addMilestoneContainer.prependTo(jq("#" + target));
            jq(milestone).hide();
            var val = jq(milestone).children(".dueDate").text();
            val = jq.datepicker.parseDate(regionalFormatDate, val);
            var chooseRespBlock = jq(milestone).find(".chooseResponsible").clone();
            if (!chooseRespBlock.length) {
                $addMilestoneContainer.find("div:first-child").append(jq(chooseRespBlock));
            }

            $addMilestoneContainer.show();
            $addMilestoneContainer.find("#newMilestoneTitle").val(jq(milestone).children(".titleContainer").text());
            $addMilestoneContainer.find("#dueDate").datepicker('setDate', val);
            $addMilestoneContainer.find("#newMilestoneTitle").focus();
        });

        //task

        jq(document).on('click', ".task .title", function () {
            milestoneContainer.hideAddMilestoneContainer();
            $addTaskContainer.hide();
            if ($addTaskContainer.hasClass('edit')) {
                jq('#' + $addTaskContainer.attr('target')).show();
            } else {
                $addTaskContainer.addClass('edit');
                jq('.addTaskContainer').show();
            }

            editTask(jq(this).parents('.task'));
        });

        jq("#newTaskTitle").bind('keydown', function (e) {
            $addTaskContainer.removeClass("red-border");
            var target = $addTaskContainer.attr('target');
            if (e.which === 13) {
                jq("#addTaskContainer .button").click();
            } else {
                if (e.which === 27) {
                    jq(this).val("");
                    if ($addTaskContainer.hasClass('edit')) {
                        milestoneContainer.hideAddTaskContainer();
                        $addTaskContainer.removeClass('edit');
                        jq("#" + target).show();
                    } else {
                        milestoneContainer.hideAddTaskContainer();
                    }
                }
            }
        });

        jq("#addTaskContainer .button").on('click', function () {
            var taskContainer = $addTaskContainer;
            var taskTitle = jq("#newTaskTitle");
            var target = taskContainer.attr('target');
            var text = jq.trim(taskTitle.val());

            taskContainer.removeClass("red-border");

            if (!text.length) {
                alert(jq("#taskError").text());
                return;
            }
            var taskId;
            if (taskContainer.hasClass('edit')) {
                jq("#" + target + " .titleContainer span").text(taskTitle.val());
                taskContainer.removeClass('edit');
                milestoneContainer.hideAddTaskContainer();
                jq("#" + target).show();
                taskId = target;
                jq(".task[id='" + taskId + "'] .chooseResponsible").remove();
                if (jq("#addTaskContainer .chooseResponsible").length) {
                    var chooseRespBlock = jq("#addTaskContainer .chooseResponsible").clone();
                    jq(".task[id='" + taskId + "'] .entity-menu").after(jq(chooseRespBlock));
                    var guid = jq(chooseRespBlock).find(".link.dotline").attr('guid');
                    if (guid !== "" & guid !== "nobody" & guid !== undefined) {
                        jq(".task[id='" + taskId + "'] .chooseResponsible").removeClass("nobody");
                    } else {
                        jq(".task[id='" + taskId + "'] .chooseResponsible").addClass("nobody");
                    }
                }
            } else {
                taskCounter++;
                taskId = "t_" + taskCounter;
                var task = {};
                task.title = jq.trim(taskTitle.val());
                task.number = taskCounter;
                if (jq("#projectManagerSelector").attr("data-id") || teamContainer.find("tr").length) {
                    task.selectResp = true;
                }
                if (jq("#addTaskContainer .link.dotline").attr("guid")) {
                    task.chooseRep = { id: jq("#addTaskContainer .link.dotline").attr("guid"), name: jq("#addTaskContainer .link.dotline").text() };
                }
                var tElem;
                if (target === 'noAssign') {
                    tElem = jq("#listNoAssignListTask");
                } else {
                    tElem = jq(".listTasks[milestone='" + target + "']");
                }
                jq.tmpl("projects_templatesCreateTaskTmpl", task).appendTo(tElem);

                taskTitle.val("");
                taskTitle.focus();

                if (task.chooseRep && task.chooseRep.id) {
                    jq(document).trigger("chooseResp");
                }
            }

        });

        //task menu

        jq("#taskActionPanel .actionList #removeTask").bind('click', function () {
            $taskActionPanel.hide();
            var target = jq(this).parents('.studio-action-panel').attr('target');
            jq("#" + target).removeClass("open");
            var targetParent = jq("#" + target).parent();
            jq("#" + target).remove();
            if (jq(targetParent).hasClass('listTasks')) {
                if (jq(targetParent).children('.task').length === 0) {
                    jq(targetParent).parents('.milestone').children('.milestoneTasksContainer').hide();
                    jq(targetParent).closest(".milestone").find(".addTask").removeClass("hide");
                }
            }

            if (jq("#listAddedMilestone .milestone").length == 0 && jq("#listNoAssignListTask .task").length == 0 && selectedTeam.length === 0) {
                jq(document).trigger("unchooseResp");
            }
        });

        jq("#taskActionPanel .actionList #editTask").bind('click', function () {
            $taskActionPanel.hide();
            editTask(jq("#" + jq(this).parents('.studio-action-panel').attr('target')));
        });
    };

    function onResetAction(userId) {
        if (!isInit) return;
        onChooseProjectTeam(null, selectedTeam.filter(function (item) { return item.id !== userId; }));

        var tasksResp = jq(".task .chooseResponsible .link[guid='" + userId + "']");
        var tasks = new Array();
        var i;
        for (i = 0; i < tasksResp.length; i++) {
            tasks.push(jq(tasksResp[i]).closest(".task"));
        }
        jq(tasksResp).closest(".chooseResponsible").remove();
        for (i = 0; i < tasks.length; i++) {
            var button = jq(tasks[i]).find(".entity-menu");
            jq(button).after(chooseRespStr);
        }
    };

    function notOnlyVisitors() {
        return selectedTeam.length === 0 || selectedTeam.some(function(item) {
            return !item.isVisitor;
        });
    }

    function onChooseProjectTeam(e, members) {
        selectedTeam = members.map(function (item) {
            return Object.assign(Object.assign({}, window.UserManager.getUser(item.id)), item);
        });

        if (pmId || notOnlyVisitors()) {
            showChooseResponsible();
        }
    };
    
    function onChooseProjectManager(e, item) {
        if (pmId) {
            $projectTeamSelector.useradvancedSelector("undisable", [pmId]);
        }
        pmId = item.id;
        $projectTeamSelector.useradvancedSelector("disable", [pmId]);
        showChooseResponsible();
    };
    
    var onErrorGetTemplate = function () {
        //window.onbeforeunload = null;
        location.href = "ProjectTemplates.aspx#elementNotFound";
    };

    var onGetTemplate = function (param, templ) {
        //get tmpl

        var val = jq.format(ASC.Projects.Resources.ProjectTemplatesResource.DefaultProjTitle, templ.title);
        prjTitleContainer.val(val);
        showProjStructure(templ);

        jq("#projectDescription").val("");
        jq("#notifyManagerCheckbox").attr("disabled", "disabled");
        prjTitleContainer.focus();
    };

    function editTask (task) {
        jq(task).removeClass("open");
        $addTaskContainer.addClass('edit');
        $addTaskContainer.attr('target', jq(task).attr("id"));
        $addTaskContainer.insertAfter(task);
        jq(task).hide();
        var chooseRespBlock = jq(task).find(".chooseResponsible").clone();
        if (!chooseRespBlock.length) {
            jq("#addTaskContainer div:first-child").append(jq(chooseRespBlock));
        }
        jq("#addTaskContainer #newTaskTitle").val(jq(task).children(".titleContainer").text());
        $addTaskContainer.show();
        jq("#addTaskContainer #newTaskTitle").focus();
    };

    function showProjStructure(tmpl) {
        var description = { tasks: [], milestones: [] };
        try {
            description = jq.parseJSON(tmpl.description) || { tasks: [], milestones: [] };
        } catch (e) {

        }
        jq("#listAddedMilestone").empty();
        jq("#listNoAssignListTask").empty();
        var chooseRep = undefined, i;

        if (jq("#projectManagerSelector").attr("data-id")) {
            chooseRep = {
                id: jq("#projectManagerSelector").attr("data-id"),
                name: jq("#projectManagerSelector").html()
            };
        } else if (teamContainer.find("tr").length) {
            chooseRep = {
                id: teamContainer.find("tr:first").attr('data-partisipantid'),
                name: teamContainer.find("tr:first .user-name").text()
            };
        }

        var milestones = description.milestones;
        if (milestones) {
            for (i = 0; i < milestones.length; i++) {
                milestoneCounter++;
                var duration = parseFloat(milestones[i].duration);
                var date = new Date();
                date.setDate(date.getDate() + duration * 30);
                milestones[i].number = milestoneCounter;
                milestones[i].date = jq.datepicker.formatDate(regionalFormatDate, date);
                milestones[i].displayTasks = milestones[i].tasks.length ? true : false;
                milestones[i].chooseRep = chooseRep;
                jq.tmpl("projects_templatesCreateMilestoneTmpl", milestones[i]).appendTo("#listAddedMilestone");
            }
        }
        var noAssignTasks = description.tasks;
        if (noAssignTasks) {
            for (i = 0; i < noAssignTasks.length; i++) {
                taskCounter++;
                noAssignTasks[i].number = taskCounter;
                noAssignTasks[i].selectResp = chooseRep;
                jq.tmpl("projects_templatesCreateTaskTmpl", noAssignTasks[i]).appendTo("#listNoAssignListTask");
            }
            $addTaskContainer.attr("target", 'noAssign');
        }
        milestoneContainer.showAction('noAssign');
    };

    var getProjMilestones = function () {
        var milestones = new Array();

        var listMilestoneCont = jq("#listAddedMilestone .milestone");
        for (var i = 0; i < listMilestoneCont.length; i++) {
            var deadline = jq.datepicker.parseDate(regionalFormatDate, jq(listMilestoneCont[i]).children(".mainInfo").children('.dueDate').text());
            deadline.setHours(0);
            deadline.setMinutes(0);
            
            var milestone = {
                title: jq(listMilestoneCont[i]).children(".mainInfo").children('.titleContainer').text(),
                deadLine: Teamlab.serializeTimestamp(deadline)
            };

            var mResponsible = jq(listMilestoneCont[i]).find(".mainInfo").find(".link.dotline");
            if (mResponsible.length) {
                var guid = jq(mResponsible).attr("guid");
                if (typeof guid != 'undefined' && guid !== "") {
                    milestone.responsible = guid;
                }
            }

            milestones.push(milestone);
        }

        return milestones;
    };

    var getProjTasks = function () {
        var listNoAssTaskStr = new Array();
        var listNoAssCont = jq('#noAssignTaskContainer .task');
        var listMilestoneCont = jq("#listAddedMilestone .milestone");
        var i, task, guid;
        for (i = 0; i < listMilestoneCont.length; i++) {
            var listTaskCont = jq(listMilestoneCont[i]).children('.milestoneTasksContainer').children(".listTasks").children('.task');
            for (var j = 0; j < listTaskCont.length; j++) {
                task = { title: jq(listTaskCont[j]).children('.titleContainer').text(), milestone: i + 1, responsibles: [] };
                var tResponsible = jq(listTaskCont[j]).find(".link.dotline");
                if (tResponsible.length) {
                    guid = jq(tResponsible).attr("guid");
                    if (typeof guid != 'undefined' && guid !== "") {
                        task.responsibles.push(guid);
                    }
                }
                listNoAssTaskStr.push(task);
            }
        }
        
        for (i = 0; i < listNoAssCont.length; i++) {
            task = { title: jq(listNoAssCont[i]).children('.titleContainer').text() };

            var responsible = jq(listNoAssCont[i]).find(".link.dotline");
            if (responsible.length) {
                guid = jq(responsible).attr("guid");
                if (typeof guid != 'undefined' && guid !== "") {
                    task.responsibles = [guid];
                }
            }

            listNoAssTaskStr.push(task);
        }

        return listNoAssTaskStr;
    };

    return {
        init: init,
        onGetTemplate: onGetTemplate,
        onErrorGetTemplate: onErrorGetTemplate,
        getProjMilestones: getProjMilestones,
        getProjTasks: getProjTasks,
        onResetAction: onResetAction
    };

})(jQuery);

