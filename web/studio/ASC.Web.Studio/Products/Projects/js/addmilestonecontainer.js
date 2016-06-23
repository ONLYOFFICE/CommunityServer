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


ASC.Projects.MilestoneContainer = (function () {
    var init = function () {
        if (location.href.indexOf('projects.aspx') > 0) {
            ASC.Projects.CreateMilestoneContainer.init("<span class='chooseResponsible nobody'><span class='dottedLink'>" + ASC.Projects.Resources.ProjectsJSResource.ChooseResponsible + "</span></span>");
        }
        
        if (location.href.indexOf('projectTemplates.aspx') > 0) {
            ASC.Projects.EditMilestoneContainer.init();
        }
        
        jq("#newMilestoneTitle, #newTaskTitle").val('');

        jq(document).on('click', ".task .entity-menu", function () {
            hideAddTaskContainer();
            hideAddMilestoneContainer();
            var target = jq(this).parents('.task').attr('id');
            jq("#" + target).addClass("open");
            jq("#taskActionPanel").attr('target', target);
            showActionsPanel("taskActionPanel", this);
            return false;
        });
        jq(document).on('click', ".milestone .mainInfo .entity-menu", function () {
            hideAddTaskContainer();
            hideAddMilestoneContainer();
            var target = jq(this).parents('.milestone').attr('id');
            jq("#milestoneActions").attr('target', target);
            jq("#" + target).addClass("open");
            showActionsPanel("milestoneActions", this);
            return false;
        });

        jq(document).on('click', ".addTaskContainer .link", function () {
            hideAddMilestoneContainer();
            if (jq('#addTaskContainer').hasClass('edit')) {
                jq('#' + jq('#addTaskContainer').attr('target')).show();
                jq('#addTaskContainer').removeClass('edit');
            }
            jq("#newTaskTitle").val("");
            var target;
            var parent = jq(this).parent().parent();
            if (jq(parent).attr("id") == "noAssignTaskContainer") {
                target = "noAssign";
            } else {
                target = jq(parent).parent().attr("id");
            }
            if (jq("#addTaskContainer").attr("target") != "") {
                var elem = jq("#addTaskContainer").parent();
                if (jq(elem).attr('id') == "noAssignTaskContainer") {
                    jq("#noAssignTaskContainer .addTaskContainer").appendTo("#noAssignTaskContainer");
                }
                jq(elem).children(".addTaskContainer").show();
            }

            var chooseRespBlock = jq("#addTaskContainer").find(".chooseResponsible");

            if (!chooseRespBlock.length) {
                jq("#addTaskContainer div:first-child").append(jq(".chooseResponsible:first").clone());
            }

            jq("#addTaskContainer").attr("target", target);
            jq("#addTaskContainer").appendTo(parent);
            jq(parent).children(".addTaskContainer").hide();
            jq("#addTaskContainer").show();
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


    var showActionsPanel = function (panelId, obj) {
        var x, y;

        jq('.studio-action-panel').hide();

        x = jq(obj).offset().left;
        y = jq(obj).offset().top;

        if (panelId == "projectMemberPanel") {
            x = x - 21;
            y = y + 20;
        } else {
            x = x - 164;
            y = y + 29;
        }

        jq('#' + panelId).css("left", x + "px");
        jq('#' + panelId).css("top", y + "px");
        jq('#' + panelId).show();
    };

    var showAction = function (target) {
        if (target == 'noAssign') {
            var listNoAssignTask = jq("#listNoAssignListTask .task");
            if (listNoAssignTask.length > 0) {
                jq("#noAssignTaskContainer .addTaskContainer").appendTo(jq("#noAssignTaskContainer"));
                jq("#noAssignTaskContainer .addTaskContainer").show();
            }
        }
    };

    var hideAddMilestoneContainer = function () {
        jq("#addMilestoneContainer").hide();
        if (jq("#addMilestoneContainer").hasClass("edit")) {
            var target = jq("#addMilestoneContainer").attr("target");
            jq("#" + target + " .mainInfo").show();
            jq("#addMilestoneContainer").removeClass("edit");
        }
        if (jq("#addMilestoneContainer").hasClass('edit')) {
            jq("#" + jq("#addMilestoneContainer").attr('target')).find(".mainInfo").show();
        }
        jq(".addTaskContainer").show();
        jq("#addMilestoneContainer #newMilestoneTitle").val('');
        jq("#addMilestone").show();
    };

    var hideAddTaskContainer = function () {
        jq("#addTaskContainer").hide();
        jq('.task').show();
        var target = jq("#addTaskContainer").attr("target");
        var elem = jq("#" + target);
        var containerTask;
        if (jq(elem).hasClass("milestone")) {
            containerTask = jq(elem).find(".milestoneTasksContainer");
            if (jq(containerTask).find(".task").length == 0) {
                jq(containerTask).hide();
                jq(containerTask).closest(".milestone").find(".addTask").removeClass("hide");
                jq(containerTask).find(".addTaskContainer").hide();
            } else {
                jq(containerTask).find(".addTaskContainer").show();
            }
        } else {
            var container = jq(elem).parent();
            if (target == "noAssign" || jq(container).attr('id') == "listNoAssignListTask") {
                jq("#noAssignTaskContainer .addTaskContainer").show();
            } else {
                containerTask = jq(elem).closest(".milestoneTasksContainer");
                if (jq(containerTask).find(".task").length == 0) {
                    jq(containerTask).hide();
                    jq(containerTask).closest(".milestone").find(".addTask").removeClass("hide");
                    jq(containerTask).find(".addTaskContainer").hide();
                } else {
                    jq(containerTask).find(".addTaskContainer").show();
                }
            }
        }
        jq("#addTaskContainer").removeAttr("target");
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
    var tmplId;

    var init = function () {
        //milestone
        jq("#addMilestone a").bind('click', function () {
            ASC.Projects.MilestoneContainer.hideAddTaskContainer();
            jq("#addMilestoneContainer").hide();
            if (jq("#addMilestoneContainer").hasClass('edit')) {
                jq("#" + jq("#addMilestoneContainer").attr('target')).find(".mainInfo").show();
                jq("#addMilestoneContainer").removeClass('edit');
            }

            jq("#addMilestone").after(jq("#addMilestoneContainer"));
            jq("#addMilestone").hide();
            jq("#newMilestoneTitle").val('');

            if (jq("#dueDate").length) {
                var defDate = new Date();
                defDate.setDate(defDate.getDate() + 3);
                jq("#dueDate").datepicker("setDate", defDate);
            }

            jq("#addMilestoneContainer").show();
            jq("#addMilestoneContainer #newMilestoneTitle").focus();
        });

        jq("#newMilestoneTitle").bind('keydown', function (e) {
            jq("#addMilestoneContainer").removeClass("red-border");
            var targetId = jq("#addMilestoneContainer").attr('target');
            if (e.which == 13) {
                jq("#addMilestoneContainer .button").click();
            } else {
                if (e.which == 27) {
                    jq(this).val("");
                    if (jq("#addMilestoneContainer").hasClass('edit')) {
                        jq("#addMilestoneContainer").hide();
                        jq("#" + targetId + " .mainInfo").show();
                    } else {
                        jq("#addMilestoneContainer").hide();
                        jq("#addMilestone").show();
                    }
                }
            }
        });

        jq("#addMilestoneContainer .button").on('click', function (e) {
            var addMilestoneContainer = jq("#addMilestoneContainer");
            var targetId = addMilestoneContainer.attr('target');
            var milestoneTitle = jq("#newMilestoneTitle");

            addMilestoneContainer.removeClass("red-border");

            var text = jq.trim(milestoneTitle.val());
            if (!text.length) {
                alert(jq("#milestoneError").text());
                return;
            }
            if (addMilestoneContainer.hasClass('edit')) {

                jq("#" + targetId + " .mainInfo .titleContainerEdit span").text(jq.trim(milestoneTitle.val()));
                var days = jq("#addMilestoneContainer select option:selected").attr('value');
                jq("#" + targetId + " .mainInfo .daysCount span").text(days);
                jq("#" + targetId + " .mainInfo .daysCount").attr('value', days);

                addMilestoneContainer.hide();
                jq("#" + targetId + " .mainInfo").show();
                addMilestoneContainer.removeClass('edit');
            } else {
                milestoneCounter++;
                var milestone = {
                    title: jq.trim(milestoneTitle.val()), duration: jq("#addMilestoneContainer select option:selected").attr('value'),
                    tasks: [], number: milestoneCounter
                };

                jq.tmpl("projects_templatesEditMilestoneTmpl", milestone).appendTo("#listAddedMilestone");
                milestoneTitle.val("");
                milestoneTitle.focus();
            }
        });

        //milestone menu
        jq(document).on('click', ".milestone .mainInfo .title, .milestone .mainInfo .daysCount", function () {
            ASC.Projects.MilestoneContainer.hideAddTaskContainer();
            ASC.Projects.MilestoneContainer.hideAddMilestoneContainer();
            jq("#addMilestoneContainer").addClass('edit');
            var target = jq(this).closest('.milestone').attr('id');
            jq("#addMilestoneContainer").attr('target', target);
            var milestone = jq("#" + target + " .mainInfo");
            jq("#addMilestoneContainer").prependTo(jq("#" + target));
            jq(milestone).hide();
            var val = jq(milestone).find(".daysCount").attr('value');
            jq("#addMilestoneContainer").show();
            jq("#addMilestoneContainer #newMilestoneTitle").val(jq(milestone).children(".titleContainerEdit").text());
            jq("#addMilestoneContainer select option[value = '" + val + "']").attr("selected", "selected");

            jq("#addMilestoneContainer #newMilestoneTitle").focus();

        });
        jq(document).on('click', ".milestone .mainInfo .addTask", function () {
            ASC.Projects.MilestoneContainer.hideAddTaskContainer();
            var target = jq(this).closest('.milestone').attr('id');
            var milestTasksCont = jq("#" + target + " .milestoneTasksContainer");
            jq("#addTaskContainer").appendTo(milestTasksCont[0]);
            jq(milestTasksCont).find('.addTaskContainer').hide();
            jq("#addTaskContainer").attr("target", target);
            jq("#addTaskContainer").show();
            jq(milestTasksCont).show();
            jq("#addTaskContainer #newTaskTitle").focus();
        });

        jq("#milestoneActions .actionList #removeMilestone").bind('click', function () {
            ASC.Projects.MilestoneContainer.hideAddTaskContainer();
            jq("#addTaskContainer").appendTo("#noAssignTaskContainer");
            jq("#milestoneActions").hide();
            var target = jq(this).parents('.studio-action-panel').attr('target');
            jq("#" + target).removeClass("open");
            jq("#" + target).remove();
        });

        jq("#milestoneActions .actionList #addTaskInMilestone").bind('click', function () {
            jq("#milestoneActions").hide();
            var target = jq(this).parents('.studio-action-panel').attr('target');
            jq("#" + target).removeClass("open");
            var listTasks = jq(".listTasks[milestone='" + target + "']");
            var milestTasksCont = jq(listTasks[0]).closest(".milestoneTasksContainer");
            jq(milestTasksCont).find('.addTaskContainer').hide();
            jq("#addTaskContainer").appendTo(milestTasksCont[0]);
            jq("#addTaskContainer").attr("target", target);
            jq("#addTaskContainer").show();
            jq(milestTasksCont).find(".addTaskContainer").hide();
            jq(milestTasksCont).show();
            jq("#addTaskContainer #newTaskTitle").focus();
        });

        jq("#milestoneActions .actionList #editMilestone").bind('click', function () {
            jq("#milestoneActions").hide();
            ASC.Projects.MilestoneContainer.hideAddMilestoneContainer();
            jq("#addMilestoneContainer").addClass('edit');
            var target = jq(this).parents('.studio-action-panel').attr('target');
            jq("#addMilestoneContainer").attr('target', target);
            jq("#" + target).removeClass("open");
            var milestone = jq("#" + target + " .mainInfo");
            jq("#addMilestoneContainer").prependTo(jq("#" + target));
            jq(milestone).hide();
            var val = jq(milestone).children(".daysCount").attr('value');
            jq("#addMilestoneContainer").show();
            jq("#addMilestoneContainer #newMilestoneTitle").val(jq(milestone).children(".titleContainerEdit").text());
            jq("#addMilestoneContainer select option[value = '" + val + "']").attr("selected", "selected");

            jq("#addMilestoneContainer #newMilestoneTitle").focus();
        });

        //task

        jq(document).on('click', ".task .title", function () {
            ASC.Projects.MilestoneContainer.hideAddMilestoneContainer();
            jq("#addTaskContainer").hide();
            if (jq("#addTaskContainer").hasClass('edit')) {
                jq('#' + jq("#addTaskContainer").attr('target')).show();
            } else {
                jq("#addTaskContainer").addClass('edit');
                jq('.addTaskContainer').show();
            }

            var target = jq(this).parents('.task');

            jq("#addTaskContainer").attr('target', jq(target).attr("id"));
            jq("#addTaskContainer").insertAfter(target);
            jq(target).hide();
            jq("#addTaskContainer #newTaskTitle").val(jq(target).children(".titleContainer").text());
            jq("#addTaskContainer").show();
            jq("#addTaskContainer #newTaskTitle").focus();
        });

        jq("#newTaskTitle").bind('keydown', function (e) {
            var taskContainer = jq("#addTaskContainer");
            taskContainer.removeClass("red-border");
            var target = taskContainer.attr('target');
            if (e.which == 13) {
                jq("#addTaskContainer .button").click();
            } else {
                if (e.which == 27) {
                    jq(this).val("");
                    if (taskContainer.hasClass('edit')) {
                        ASC.Projects.MilestoneContainer.hideAddTaskContainer();
                        taskContainer.removeClass('edit');
                        jq("#" + target).show();
                    } else {
                        ASC.Projects.MilestoneContainer.hideAddTaskContainer();
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
                ASC.Projects.MilestoneContainer.hideAddTaskContainer();
                jq("#" + target).show();
            } else {
                taskCounter++;
                var task = { title: jq.trim(taskTitle.val()), number: taskCounter };
                var tElem = target == 'noAssign' ? jq("#listNoAssignListTask") : jq(".listTasks[milestone='" + target + "']");
                jq.tmpl("projects_templatesEditTaskTmpl", task).appendTo(tElem);
                taskTitle.val("");
                taskTitle.focus();
            }
        });

        //task menu

        jq("#taskActionPanel .actionList #removeTask").bind('click', function () {
            jq("#taskActionPanel").hide();
            var target = jq(this).parents('.studio-action-panel').attr('target');
            jq("#" + target).removeClass("open");
            var targetParent = jq("#" + target).parent();
            jq("#" + target).remove();
            if (jq(targetParent).hasClass('listTasks')) {
                if (jq(targetParent).children('.task').length == 0) {
                    jq(targetParent).closest('.milestone').find('.milestoneTasksContainer').hide();
                    jq(targetParent).closest(".milestone").find(".addTask").removeClass("hide");
                }
            }
        });

        jq("#taskActionPanel .actionList #editTask").bind('click', function () {
            jq("#taskActionPanel").hide();
            var target = jq(this).parents('.studio-action-panel').attr('target');
            var task = jq("#" + target);
            jq(task).removeClass("open");
            jq("#addTaskContainer").addClass('edit');
            jq("#addTaskContainer").attr('target', target);
            jq("#addTaskContainer").insertAfter(task);
            jq(task).hide();
            jq("#addTaskContainer #newTaskTitle").val(jq(task).children(".titleContainer").text());
            jq("#addTaskContainer").show();
            jq("#addTaskContainer #newTaskTitle").focus();
        });
    };

    var showTmplStructure = function (tmpl) {
        var description = { tasks: [], milestones: [] };
        try {
            description = jQuery.parseJSON(tmpl.description) || { tasks: [], milestones: [] };
        } catch (e) {

        }

        var milestones = description.milestones;
        if (milestones) {
            for (var i = 0; i < milestones.length; i++) {
                milestoneCounter++;
                if (milestones[i].duration || milestones[i].duration > 6) {
                    var duration = jq("#addMilestoneContainer select option[duration='" + milestones[i].duration + "']").text();
                    if (duration == "") {
                        duration = milestones[i].duration.toString();
                        duration = duration.replace('.', ',');
                        milestones[i].duration = jq("#addMilestoneContainer select option[duration^='" + duration + "']").text();
                    } else {
                        milestones[i].duration = duration;
                    }
                } else {
                    milestones[i].duration = jq("#addMilestoneContainer select option:first-child").text();
                }
                milestones[i].number = milestoneCounter;
                milestones[i].displayTasks = milestones[i].tasks.length ? true : false;
                jq.tmpl("projects_templatesEditMilestoneTmpl", milestones[i]).appendTo("#listAddedMilestone");
            }
        }
        var noAssignTasks = description.tasks;
        if (noAssignTasks) {
            for (var i = 0; i < noAssignTasks.length; i++) {
                taskCounter++;
                noAssignTasks[i].number = taskCounter;
                jq.tmpl("projects_templatesEditTaskTmpl", noAssignTasks[i]).appendTo("#listNoAssignListTask");
            }
            jq("#addTaskContainer").attr("target", 'noAssign');
            ASC.Projects.MilestoneContainer.hideAddTaskContainer();
            ASC.Projects.MilestoneContainer.showAction('noAssign');
        }
    };

    return {
        init: init,
        showTmplStructure: showTmplStructure
    };

})(jQuery);

ASC.Projects.CreateMilestoneContainer = (function () {
    var prjTitleContainer = jq("input[id*='projectTitle']"),
        dueDateContainer = jq("#dueDate"),
        teamContainer = jq("#Team").length ? jq("#Team") : jq("#projectParticipantsContainer");
    var regionalFormatDate, chooseRespStr, showRespCombFlag = false,
        milestoneCounter = 0, taskCounter = 0;
    var pmId = null;

    var showChooseResponsible = function () {
        if (selectedTeam.length == 0 && !pmId) {
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

    var updateProjectMemberPanel = function () {
        jq("#projectMemberPanel .actionList li").remove();
        var pmName = jq("#projectManagerSelector").attr("data-id") ? jq("#projectManagerSelector").html() : "";

        if (pmId)
            jq("#projectMemberPanel .actionList").append("<li id='" + pmId + "' class='dropdown-item'>" + pmName + "</li>");

        if (selectedTeam.length) {
            teamContainer.show();
        }

        for (var i = 0; i < selectedTeam.length; i++) {
            if (selectedTeam[i].isVisitor) continue;
            jq("#projectMemberPanel .actionList").append("<li id='" + selectedTeam[i].id + "' class='dropdown-item'>" + selectedTeam[i].title + "</li>");
        }
        
        updateMilestoneAndTaskResponsible();
        jq("#projectMemberPanel .actionList").prepend("<li id='nobody' class='dropdown-item'>" + jq("#projectMemberPanel .actionList").attr("nobodyItemText") + "</li>");
    };
    
    var responsibleInTeam = function (entity, team) {
        var oldResp = jq(entity).attr("guid");
        return team.some(function (item) { return item.id == oldResp || (pmId && pmId == oldResp); });
    };
    
    var updateMilestoneAndTaskResponsible = function () {
        var pmName = jq("#projectManagerSelector").attr("data-id") ? jq("#projectManagerSelector").html() : "";
        var listEntities = jq(".milestone .mainInfo .chooseResponsible .dottedLink, .projects-templates-container .task .dottedLink[guid], #addTaskContainer .chooseResponsible .dottedLink[guid], #addMilestoneContainer .chooseResponsible .dottedLink");

        for (var i = 0; i < listEntities.length; i++) {
            if (!responsibleInTeam(jq(listEntities[i]), selectedTeam)) {
                if (pmId) {
                    jq(listEntities[i]).attr("guid", pmId);
                    jq(listEntities[i]).html(pmName);
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
    };

    var init = function (str) {
        chooseRespStr = str;
        var $projectManagerSelector = jq("#projectManagerSelector"),
           $projectTeamSelector = jq("#projectTeamSelector");

        $projectManagerSelector.useradvancedSelector({
            withGuests: false,
            showGroups: true,
            onechosen: true
        }).on("showList", onChooseProjectManager);

        $projectTeamSelector.useradvancedSelector({
            showGroups: true
        }).on("showList", onChooseProjectTeam);

        //team popup
        $projectTeamSelector.click(function () {
            ASC.Projects.MilestoneContainer.hideAddTaskContainer();
            ASC.Projects.MilestoneContainer.hideAddMilestoneContainer();
            var userIds = [];
            teamContainer.find('span.items-display-list').each(function () {
                userIds.push(jq(this).closest('tr').attr('guid'));
            });
            jq('#projectTeamSelector').useradvancedSelector("select", userIds);
        });

        //datepicker
        dueDateContainer.val("");
        dueDateContainer.datepicker().mask(ASC.Resources.Master.DatePatternJQ);
        dueDateContainer.datepicker({
            onSelect: function () {
                jq("#newMilestoneTitle").focus();
            }
        });
        regionalFormatDate = jq("#dueDate").datepicker("option", "dateFormat");
        
        teamContainer.on('click', ".items-display-list_i .reset-action", function () {
            var userId = jq(this).closest('li').attr('participantid');
            $projectTeamSelector.useradvancedSelector("unselect", [userId]);
            onChooseProjectTeam(null, selectedTeam.filter(function (item) { return item.id != userId; }));

            var tasksResp = jq(".task .chooseResponsible .dottedLink[guid='" + userId + "']");
            var tasks = new Array();
            for (var i = 0; i < tasksResp.length; i++) {
                tasks.push(jq(tasksResp[i]).closest(".task"));
            }
            jq(tasksResp).closest(".chooseResponsible").remove();
            for (var i = 0; i < tasks.length; i++) {
                var button = jq(tasks[i]).find(".entity-menu");
                jq(button).after(chooseRespStr);
            }
            jq(this).closest('li').remove();
        });

        jq("body").click(function (event) {
            if (event.target.className != "userName") {
                if (!jq("#projectManagerSelector").attr("data-id")) {
                    pmId = null;
                    //showChooseResponsible();
                }
            }
        });

        //choose responsible
        jq(document).on("click", ".task .chooseResponsible", function () {
            jq(this).closest(".task").addClass('open');
            ASC.Projects.MilestoneContainer.showActionsPanel("projectMemberPanel", this);
            jq("#projectMemberPanel #nobody").show();
            var target = jq(this).closest(".task").attr("id");
            jq("#projectMemberPanel").attr("target", target);
        });

        jq("#addTaskContainer").on("click", ".chooseResponsible", function () {
            ASC.Projects.MilestoneContainer.showActionsPanel("projectMemberPanel", this);
            jq("#projectMemberPanel #nobody").show();
            jq("#projectMemberPanel").attr("target", "newTask");
        });

        jq("#addMilestoneContainer").on("click", ".chooseResponsible", function () {
            ASC.Projects.MilestoneContainer.showActionsPanel("projectMemberPanel", this);
            jq("#projectMemberPanel #nobody").hide();
            jq("#projectMemberPanel").attr("target", "newMilestone");
        });

        jq(document).on("click", ".milestone .mainInfo .chooseResponsible", function () {
            jq(this).closest(".milestone").addClass('open');
            ASC.Projects.MilestoneContainer.showActionsPanel("projectMemberPanel", this);
            jq("#projectMemberPanel #nobody").hide();
            var target = jq(this).closest(".milestone").attr("id");
            jq("#projectMemberPanel").attr("target", target);
        });

        jq("#projectMemberPanel").on("click", "ul li", function () {
            jq(".studio-action-panel").hide();
            var target = jq("#projectMemberPanel").attr("target");
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
                        if (target == "newTask") {
                            target = jq("#addTaskContainer");
                        } else {
                            target = jq("#addMilestoneContainer");
                        }
                    }
            }

            var guid = jq(this).attr("id");
            var name;
            if (guid == "nobody" || guid == "") {
                name = jq("#projectMemberPanel .actionList").attr("chooseRespText");
                if (type != "newTask") {
                    jq(target).find(".chooseResponsible").addClass("nobody");
                }
            } else {
                name = jq(this).text();
                jq(target).find(".chooseResponsible").removeClass("nobody");
            }

            var member = jq(target).find(".dottedLink");

            if (guid != "nobody" && guid != "") {
                jq(member).attr("guid", guid);
            } else {
                jq(member).removeAttr("guid");
            }

            jq(member).text(name);
            jq(target).find("input").last().focus();

        });


        //milestone
        jq(document).on('click', ".milestone .mainInfo .title, .milestone .mainInfo .dueDate", function () {
            ASC.Projects.MilestoneContainer.hideAddMilestoneContainer();
            ASC.Projects.MilestoneContainer.hideAddTaskContainer();
            jq("#addMilestoneContainer").addClass('edit');
            var target = jq(this).closest('.milestone').attr('id');
            jq("#addMilestoneContainer").attr('target', target);
            var milestone = jq("#" + target + " .mainInfo");
            jq("#addMilestoneContainer").prependTo(jq("#" + target));
            jq(milestone).hide();
            var val = jq(milestone).children(".dueDate").text();
            val = jq.datepicker.parseDate(regionalFormatDate, val);
            var chooseRespBlock = jq(milestone).find(".chooseResponsible").clone();

            if (!chooseRespBlock.length) {
                jq("#addMilestoneContainer div:first-child").append(jq(chooseRespBlock));
            }

            var pm = jq("#projectManagerSelector").attr("data-id") ? jq("#projectManagerSelector").html() : "";
            if (pmId) {
                jq(chooseRespBlock).find(".dottedLink").attr("guid", pmId);
                jq(chooseRespBlock).find(".dottedLink").text(pm);
            }

            jq("#addMilestoneContainer").show();
            jq("#addMilestoneContainer #newMilestoneTitle").val(jq(milestone).children(".titleContainer").text());

            jq("#addMilestoneContainer #dueDate").datepicker('setDate', val);
            jq("#addMilestoneContainer #newMilestoneTitle").focus();
        });

        jq("#addMilestone a").bind('click', function () {
            ASC.Projects.MilestoneContainer.hideAddTaskContainer();
            jq("#addMilestoneContainer").hide();
            if (jq("#addMilestoneContainer").hasClass('edit')) {
                jq("#" + jq("#addMilestoneContainer").attr('target')).find(".mainInfo").show();
                jq("#addMilestoneContainer").removeClass('edit');
            }

            jq("#addMilestone").after(jq("#addMilestoneContainer"));
            jq("#addMilestone").hide();
            jq("#newMilestoneTitle").val('');

            if (jq("#dueDate").length) {
                var defDate = new Date();
                defDate.setDate(defDate.getDate() + 3);
                jq("#dueDate").datepicker("setDate", defDate);
            }
            var chooseRespBlock = jq("#addMilestoneContainer").find(".chooseResponsible");

            if (!chooseRespBlock.length) {
                jq("#addMilestoneContainer div:first-child").append(jq(".chooseResponsible:first").clone());

                var pm = jq("#projectManagerSelector").attr("data-id");
                if (pmId) {
                    jq(chooseRespBlock).find(".dottedLink").attr("guid", pmId);
                    jq(chooseRespBlock).find(".dottedLink").text(pm);
                }
            }
            jq("#addMilestoneContainer").show();
            jq("#addMilestoneContainer #newMilestoneTitle").focus();
        });

        jq("#newMilestoneTitle").bind('keydown', function (e) {
            var milestoneContainer = jq("#addMilestoneContainer");
            milestoneContainer.removeClass("red-border");
            var targetId = milestoneContainer.attr('target');
            if (e.which == 13) {
                jq("#addMilestoneContainer .button").click();
            } else {
                if (e.which == 27) {
                    jq(this).val("");
                    if (milestoneContainer.hasClass('edit')) {
                        milestoneContainer.hide();
                        jq("#" + targetId + " .mainInfo").show();
                    } else {
                        milestoneContainer.hide();
                        jq("#addMilestone").show();
                    }
                }
            }
        });

        jq("#addMilestoneContainer .button").on("click", function () {
            var milestoneContainer = jq("#addMilestoneContainer");
            var milestoneTitle = jq("#newMilestoneTitle");
            var targetId = milestoneContainer.attr('target');

            milestoneContainer.removeClass("red-border");

            var text = jq.trim(milestoneTitle.val());
            if (!text.length) {
                alert(jq("#milestoneError").text());
                return false;
            }
            var milestoneId;
            var date = jq("#dueDate").datepicker("getDate");
            date = jq.datepicker.formatDate(regionalFormatDate, date);
            if (jq("#addMilestoneContainer").hasClass('edit')) {

                jq("#" + targetId + " .mainInfo .titleContainer span").text(jq.trim(milestoneTitle.val()));

                jq("#" + targetId + " .mainInfo .dueDate span").text(date);
                milestoneContainer.hide();
                jq("#" + targetId + " .mainInfo").show();
                milestoneContainer.removeClass('edit');
                milestoneId = targetId;
                jq("#" + targetId + " .mainInfo .chooseResponsible").remove();

                if (jq("#addMilestoneContainer .chooseResponsible").length) {
                    var chooseRespBlock = jq("#addMilestoneContainer .chooseResponsible").clone();
                    jq("#" + milestoneId + " .mainInfo .entity-menu").after(jq(chooseRespBlock));
                    if (jq(chooseRespBlock).attr('guid') != "") {
                        jq("#" + milestoneId + " .mainInfo .chooseResponsible").removeClass("nobody");
                    } else {
                        jq("#" + milestoneId + " .mainInfo .chooseResponsible").addClass("nobody");
                    }
                }
            } else {
                milestoneCounter++;
                milestoneId = "m_" + milestoneCounter;
                var milestone = {};
                milestone.title = jq.trim(milestoneTitle.val());
                milestone.date = date;
                milestone.tasks = [];
                milestone.number = milestoneCounter;
                milestone.chooseRep = { id: jq("#addMilestoneContainer .dottedLink").attr("guid"), name: jq("#addMilestoneContainer .dottedLink").text() };
                jq.tmpl("projects_templatesCreateMilestoneTmpl", milestone).appendTo("#listAddedMilestone");
                milestoneTitle.val("");
                milestoneTitle.focus();
            }

        });

        //milestone menu

        jq(document).on('click', ".milestone .mainInfo .addTask", function () {
            ASC.Projects.MilestoneContainer.hideAddMilestoneContainer();
            ASC.Projects.MilestoneContainer.hideAddTaskContainer();
            var target = jq(this).closest('.milestone').attr('id');
            var milestTasksCont = jq("#" + target + " .milestoneTasksContainer");
            jq(milestTasksCont).find('.addTaskContainer').hide();
            jq("#addTaskContainer").appendTo(milestTasksCont[0]);
            jq("#addTaskContainer").attr("target", target);
            jq("#addTaskContainer").show();
            var chooseRespBlock = jq("#addTaskContainer").find(".chooseResponsible");

            if (!chooseRespBlock.length) {
                jq("#addTaskContainer div:first-child").append(jq(".chooseResponsible:first").clone());
            }
            jq(milestTasksCont).show();
            jq("#addTaskContainer #newTaskTitle").focus();
        });

        jq("#milestoneActions .actionList #removeMilestone").bind('click', function () {
            jq("#addTaskContainer").hide();
            jq("#addTaskContainer").appendTo("#noAssignTaskContainer");
            jq("#milestoneActions").hide();
            var target = jq(this).parents('.studio-action-panel').attr('target');
            jq("#" + target).removeClass("open");
            jq("#" + target).remove();
        });

        jq("#milestoneActions .actionList #addTaskInMilestone").bind('click', function () {
            ASC.Projects.MilestoneContainer.hideAddMilestoneContainer();
            var target = jq(this).parents('.studio-action-panel').attr('target');
            jq("#" + target).removeClass("open");
            var listTasks = jq(".listTasks[milestone='" + target + "']");
            var milestTasksCont = jq(listTasks[0]).parents(".milestoneTasksContainer");
            jq("#addTaskContainer").appendTo(milestTasksCont[0]);
            jq(milestTasksCont).find('.addTaskContainer').hide();
            jq("#addTaskContainer").attr("target", target);
            var chooseRespBlock = jq("#addTaskContainer").find(".chooseResponsible");

            if (!chooseRespBlock.length) {
                jq("#addTaskContainer div:first-child").append(jq(".chooseResponsible:first").clone());
            }

            jq("#addTaskContainer").show();
            jq(milestTasksCont).show();
            jq("#addTaskContainer #newTaskTitle").focus();
        });

        jq("#milestoneActions .actionList #editMilestone").bind('click', function () {
            jq("#milestoneActions").hide();
            ASC.Projects.MilestoneContainer.hideAddMilestoneContainer();
            jq("#addMilestoneContainer").addClass('edit');
            var target = jq(this).parents('.studio-action-panel').attr('target');
            jq("#addMilestoneContainer").attr('target', target);
            jq("#" + target).removeClass("open");
            var milestone = jq("#" + target + " .mainInfo");
            jq("#addMilestoneContainer").prependTo(jq("#" + target));
            jq(milestone).hide();
            var val = jq(milestone).children(".dueDate").text();
            val = jq.datepicker.parseDate(regionalFormatDate, val);
            var chooseRespBlock = jq(milestone).find(".chooseResponsible").clone();
            if (!chooseRespBlock.length) {
                jq("#addMilestoneContainer div:first-child").append(jq(chooseRespBlock));
            }

            jq("#addMilestoneContainer").show();
            jq("#addMilestoneContainer #newMilestoneTitle").val(jq(milestone).children(".titleContainer").text());

            jq("#addMilestoneContainer #dueDate").datepicker('setDate', val);
            jq("#addMilestoneContainer #newMilestoneTitle").focus();
        });

        //task

        jq(document).on('click', ".task .title", function () {
            ASC.Projects.MilestoneContainer.hideAddMilestoneContainer();
            jq("#addTaskContainer").hide();
            if (jq("#addTaskContainer").hasClass('edit')) {
                jq('#' + jq("#addTaskContainer").attr('target')).show();
            } else {
                jq("#addTaskContainer").addClass('edit');
                jq('.addTaskContainer').show();
            }

            editTask(jq(this).parents('.task'));
        });

        jq("#newTaskTitle").bind('keydown', function (e) {
            var taskContainer = jq("#addTaskContainer");
            taskContainer.removeClass("red-border");
            var target = taskContainer.attr('target');
            if (e.which == 13) {
                jq("#addTaskContainer .button").click();
            } else {
                if (e.which == 27) {
                    jq(this).val("");
                    if (taskContainer.hasClass('edit')) {
                        ASC.Projects.MilestoneContainer.hideAddTaskContainer();
                        taskContainer.removeClass('edit');
                        jq("#" + target).show();
                    } else {
                        ASC.Projects.MilestoneContainer.hideAddTaskContainer();
                    }
                }
            }
        });

        jq("#addTaskContainer .button").on('click', function () {
            var taskContainer = jq("#addTaskContainer");
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
                ASC.Projects.MilestoneContainer.hideAddTaskContainer();
                jq("#" + target).show();
                taskId = target;
                jq(".task[id='" + taskId + "'] .chooseResponsible").remove();
                if (jq("#addTaskContainer .chooseResponsible").length) {
                    var chooseRespBlock = jq("#addTaskContainer .chooseResponsible").clone();
                    jq(".task[id='" + taskId + "'] .entity-menu").after(jq(chooseRespBlock));
                    var guid = jq(chooseRespBlock).find(".dottedLink").attr('guid');
                    if (guid != "" & guid != "nobody" & guid !== undefined) {
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
                if (jq("#projectManagerSelector").attr("data-id") || teamContainer.find(".items-display-list_i").length) {
                    task.selectResp = true;
                }
                if (jq("#addTaskContainer .dottedLink").attr("guid")) {
                    task.chooseRep = { id: jq("#addTaskContainer .dottedLink").attr("guid"), name: jq("#addTaskContainer .dottedLink").text() };
                }
                var tElem;
                if (target == 'noAssign') {
                    tElem = jq("#listNoAssignListTask");
                } else {
                    tElem = jq(".listTasks[milestone='" + target + "']");
                }
                jq.tmpl("projects_templatesCreateTaskTmpl", task).appendTo(tElem);

                taskTitle.val("");
                taskTitle.focus();
            }

        });

        //task menu

        jq("#taskActionPanel .actionList #removeTask").bind('click', function () {
            jq("#taskActionPanel").hide();
            var target = jq(this).parents('.studio-action-panel').attr('target');
            jq("#" + target).removeClass("open");
            var targetParent = jq("#" + target).parent();
            jq("#" + target).remove();
            if (jq(targetParent).hasClass('listTasks')) {
                if (jq(targetParent).children('.task').length == 0) {
                    jq(targetParent).parents('.milestone').children('.milestoneTasksContainer').hide();
                    jq(targetParent).closest(".milestone").find(".addTask").removeClass("hide");
                }
            }
        });

        jq("#taskActionPanel .actionList #editTask").bind('click', function () {
            jq("#taskActionPanel").hide();
            editTask(jq("#" + jq(this).parents('.studio-action-panel').attr('target')));
        });
    };

    var selectedTeam = [];
    function notOnlyVisitors() {
        return selectedTeam.some(function(item) {
            return !item.isVisitor;
        });
    }

    var onChooseProjectTeam = function (e, members) {
        selectedTeam = members;
        if (pmId || notOnlyVisitors()) {
            showChooseResponsible();
        }
    };
    
    var onChooseProjectManager = function (e, item) {
        pmId = item.id;
        showChooseResponsible();
    };
    
    var onErrorGetTemplate = function () {
        //window.onbeforeunload = null;
        location.href = "projectTemplates.aspx#elementNotFound";
    };

    var onGetTemplate = function (param, templ) {
        //get tmpl

        var val = jq.format(prjTitleContainer.attr("defText"), templ.title);
        prjTitleContainer.val(val);
        showProjStructure(templ);

        jq("#projectDescription").val("");
        jq("#notifyManagerCheckbox").attr("disabled", "disabled");
        prjTitleContainer.focus();
    };

    var editTask = function (task) {
        jq(task).removeClass("open");
        jq("#addTaskContainer").addClass('edit');
        jq("#addTaskContainer").attr('target', jq(task).attr("id"));
        jq("#addTaskContainer").insertAfter(task);
        jq(task).hide();
        var chooseRespBlock = jq(task).find(".chooseResponsible").clone();
        if (!chooseRespBlock.length) {
            jq("#addTaskContainer div:first-child").append(jq(chooseRespBlock));
        }
        jq("#addTaskContainer #newTaskTitle").val(jq(task).children(".titleContainer").text());
        jq("#addTaskContainer").show();
        jq("#addTaskContainer #newTaskTitle").focus();
    };

    var showProjStructure = function (tmpl) {
        var description = { tasks: [], milestones: [] };
        try {
            description = jQuery.parseJSON(tmpl.description) || { tasks: [], milestones: [] };
        } catch (e) {

        }
        jq("#listAddedMilestone").empty();
        jq("#listNoAssignListTask").empty();
        
        var milestones = description.milestones;
        if (milestones) {
            for (var i = 0; i < milestones.length; i++) {
                milestoneCounter++;
                var duration = parseFloat(milestones[i].duration);
                var date = new Date();
                date.setDate(date.getDate() + duration * 30);
                milestones[i].number = milestoneCounter;
                milestones[i].date = jq.datepicker.formatDate(regionalFormatDate, date);
                milestones[i].displayTasks = milestones[i].tasks.length ? true : false;
                if (jq("#projectManagerSelector").attr("data-id")) {
                    milestones[i].chooseRep = {
                        id: jq("#projectManagerSelector").attr("data-id"),
                        name: jq("#projectManagerSelector").html()
                    };
                } else if (teamContainer.find(".items-display-list_i").length) {
                    milestones[i].chooseRep = {
                        id: teamContainer.find(".items-display-list_i:first").attr('guid'),
                        name: teamContainer.find(".items-display-list_i:first").text()
                    };
                }
                jq.tmpl("projects_templatesCreateMilestoneTmpl", milestones[i]).appendTo("#listAddedMilestone");
            }
        }
        var noAssignTasks = description.tasks;
        if (noAssignTasks) {
            for (var i = 0; i < noAssignTasks.length; i++) {
                taskCounter++;
                noAssignTasks[i].number = taskCounter;
                noAssignTasks[i].chooseRep = jq("#projectManagerSelector").attr("data-id") || teamContainer.find(".items-display-list_i").length;
                jq.tmpl("projects_templatesCreateTaskTmpl", noAssignTasks[i]).appendTo("#listNoAssignListTask");
            }
            jq("#addTaskContainer").attr("target", 'noAssign');
        }
        ASC.Projects.MilestoneContainer.showAction('noAssign');
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

            var mResponsible = jq(listMilestoneCont[i]).find(".mainInfo").find(".dottedLink");
            if (mResponsible.length) {
                var guid = jq(mResponsible).attr("guid");
                if (typeof guid != 'undefined' && guid != "") {
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
        
        for (var i = 0; i < listMilestoneCont.length; i++) {
            var listTaskCont = jq(listMilestoneCont[i]).children('.milestoneTasksContainer').children(".listTasks").children('.task');
            for (var j = 0; j < listTaskCont.length; j++) {
                var task = { title: jq(listTaskCont[j]).children('.titleContainer').text(), milestone: i + 1, responsibles: [] };
                var tResponsible = jq(listTaskCont[j]).find(".dottedLink");
                if (tResponsible.length) {
                    var guid = jq(tResponsible).attr("guid");
                    if (typeof guid != 'undefined' && guid != "") {
                        task.responsibles.push(guid);
                    }
                }
                listNoAssTaskStr.push(task);
            }
        }
        
        for (var i = 0; i < listNoAssCont.length; i++) {
            var task = { title: jq(listNoAssCont[i]).children('.titleContainer').text() };

            var responsible = jq(listNoAssCont[i]).find(".dottedLink");
            if (responsible.length) {
                var guid = jq(responsible).attr("guid");
                if (typeof guid != 'undefined' && guid != "") {
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
        onChooseProjectManager: onChooseProjectManager,
        onChooseProjectTeam: onChooseProjectTeam
    };

})(jQuery);

