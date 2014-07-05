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

ASC.Projects.Templates = (function () {
    var init = function () {
        var tmplId = jq.getURLParam("id");
        var action = jq.getURLParam("action");
        if ((tmplId == null && action == "add") || (tmplId && action == "edit")) {
            ASC.Projects.EditProjectTemplates.init();
        }
        if (tmplId == null && action == null) {
            ASC.Projects.ListProjectsTemplates.init();
        }
        if (action == 'add' && tmplId) {
            ASC.Projects.CreateProjectFromTemplate.init("<span class='chooseResponsible nobody'><span class='dottedLink'>" + ASC.Projects.Resources.ProjectsJSResource.ChooseResponsible + "</span></span>");
        };
        jq("#newMilestoneTitle, #newTaskTitle").val('');

        jq("#attentionPopup .cancel").click(function () {
            jq.unblockUI();
        });

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

        jq(document).on('click', ".addTaskContainer .baseLinkAction", function () {
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
            if (jq(elt).is('[id="' + panelId + '"]') || jq(elt).parent().is('[class*="chooseResponsible"]') ||
               (jq(elt).parents().is(".studio-action-panel") && jq(elt).parents(".studio-action-panel").find("input").length)) {
                isHide = false;
            }

            if (isHide) {
                jq(elt).parents().each(function() {
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

    var showActionsPanel = function(panelId, obj) {
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

    var showAction = function(target) {
        if (target == 'noAssign') {
            var listNoAssignTask = jq("#listNoAssignListTask .task");
            if (listNoAssignTask.length > 0) {
                jq("#noAssignTaskContainer .addTaskContainer").appendTo(jq("#noAssignTaskContainer"));
                jq("#noAssignTaskContainer .addTaskContainer").show();
            }
        }
    };

    var hideAddMilestoneContainer = function() {
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

    var hideAddTaskContainer = function() {
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

    var checkUnsavedData = function () {
        jq("#addMilestoneContainer, #addTaskContainer").removeClass("red-border");
        var newMilestone = jq("#newMilestoneTitle");
        if (newMilestone.is(":visible") && newMilestone.val().trim() != "") {
            StudioBlockUIManager.blockUI(jq("#attentionPopup"), 400, 200, 0);
            jq.scrollTo("#addMilestoneContainer");
            jq("#addMilestoneContainer").addClass("red-border");
            return true;
        } else {
            var newTask = jq("#newTaskTitle");
            if (newTask.is(":visible") && newTask.val().trim() != "") {
                StudioBlockUIManager.blockUI(jq("#attentionPopup"), 400, 200, 0);
                jq.scrollTo("#addTaskContainer");
                jq("#addTaskContainer").addClass("red-border");
                return true;
            }
        }
        return false;
    };

    return {
        init: init,
        showActionsPanel: showActionsPanel,
        showAction: showAction,
        hideAddMilestoneContainer: hideAddMilestoneContainer,
        hideAddTaskContainer: hideAddTaskContainer,
        checkUnsavedData: checkUnsavedData
    };
})(jQuery);

ASC.Projects.ListProjectsTemplates = (function () {
    var idDeleteTempl;
    var init = function () {
        Teamlab.getPrjTemplates({}, { success: displayListTemplates, before: LoadingBanner.displayLoading, after: LoadingBanner.hideLoading });

        jq('#questionWindow .cancel').bind('click', function() {
            jq.unblockUI();
            idDeleteTempl = 0;
            return false;
        });
        jq('#questionWindow .remove').bind('click', function() {
            removeTemplate();
            jq.unblockUI();
            return false;
        });
    };

    var onDeleteTemplate = function(params, data) {
        jq("#" + idDeleteTempl).remove();
        idDeleteTempl = 0;
        var list = jq("#listTemplates").find(".template");
        if (!list.length) {
            jq("#listTemplates").hide();
            jq("#emptyListTemplates").show();
        }
    };
    var removeTemplate = function() {
        Teamlab.removePrjTemplate({ tmplId: idDeleteTempl }, idDeleteTempl, { success: onDeleteTemplate });
    };
    var createTemplateTmpl = function(template) {
        var mCount = 0, tCount = 0;

        var description = { tasks: [], milestones: [] };
        try {
            description = jQuery.parseJSON(template.description) || {tasks:[], milestones:[] };
        } catch (e) {

        }
        var milestones = description.milestones;

        var tasks = description.tasks;
        if (tasks) tCount = tasks.length;
        if (milestones) {
            mCount = milestones.length;
            for (var i = 0; i < milestones.length; i++) {
                var mTasks = milestones[i].tasks;
                if (mTasks) {
                    tCount += mTasks.length;
                }
            }
        }
        return { title: template.title, id: template.id, milestones: mCount, tasks: tCount };
    };

    var displayListTemplates = function (params, templates) {
        if (templates.length) {
            for (var i = 0; i < templates.length; i++) {
                var tmpl = createTemplateTmpl(templates[i]);
                jq.tmpl("projects_templateTmpl", tmpl).appendTo("#listTemplates");
            }
        } else {
            jq("#emptyListTemplates").show();
        }
        jq(".template").on('click', ".entity-menu", function () {
            var tmplId = jq(this).closest('.template').attr('id');
            jq("#templateActionPanel").attr('target', tmplId);
            jq(this).closest('.template').addClass('open');
            ASC.Projects.Templates.showActionsPanel("templateActionPanel", this);
            return false;
        });
        jq("#templateActionPanel #editTmpl").bind('click', function () {
            var tmplId = jq("#templateActionPanel").attr('target');
            jq(".studio-action-panel").hide();
            jq(".template").removeClass('open');
            window.onbeforeunload = null;
            window.location.replace('projectTemplates.aspx?id=' + tmplId + '&action=edit');
        });
        jq("#templateActionPanel #deleteTmpl").bind('click', function () {
            idDeleteTempl = parseInt(jq("#templateActionPanel").attr('target'));
            jq(".studio-action-panel").hide();
            jq(".template").removeClass('open');
            showQuestionWindow();
        });
        jq("#templateActionPanel #createProj").bind('click', function () {
            var tmplId = jq("#templateActionPanel").attr('target');
            jq(".studio-action-panel").hide();
            jq(".template").removeClass('open');
            window.onbeforeunload = null;
            window.location.replace('projectTemplates.aspx?id=' + tmplId + '&action=add');
        });
    };
    var showQuestionWindow = function() {
        var margintop = jq(window).scrollTop();
        margintop = margintop + 'px';
        jq.blockUI({ message: jq("#questionWindow"),
            css: {
                left: '50%',
                top: '25%',
                opacity: '1',
                border: 'none',
                padding: '0px',
                width: '400px',

                cursor: 'default',
                textAlign: 'left',
                position: 'absolute',
                'margin-left': '-200px',
                'margin-top': margintop,
                'background-color': 'White'
            },
            overlayCSS: {
                backgroundColor: '#AAA',
                cursor: 'default',
                opacity: '0.3'
            },
            focusInput: false,
            baseZ: 666,

            fadeIn: 0,
            fadeOut: 0,

            onBlock: function() {
            }
        });
    };
    return {
        init: init
    };

})(jQuery);

ASC.Projects.EditProjectTemplates = (function() {
    var milestoneCounter = 0,
        taskCounter = 0;
    var tmplId = null;

    var action = "";

    var init = function() {

        tmplId = jq.getURLParam('id');
        if (tmplId) {
            Teamlab.getPrjTemplate({}, tmplId, { success: onGetTemplate, before: LoadingBanner.displayLoading, after: LoadingBanner.hideLoading });
        }

        jq('#templateTitle').focus();

        //
        jq("#attentionPopup .continue").click(function() {
            generateAndSaveTemplate(action);
            jq.unblockUI();
        });
        //milestone
        jq("#addMilestone a").bind('click', function() {
            ASC.Projects.Templates.hideAddTaskContainer();
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
        
        jq("#newMilestoneTitle").bind('keydown', function(e) {
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

                jq("#" + targetId + " .mainInfo .title").text(jq.trim(milestoneTitle.val()));
                var days = jq("#addMilestoneContainer select option:selected").attr('value');
                jq("#" + targetId + " .mainInfo .daysCount span").text(days);
                jq("#" + targetId + " .mainInfo .daysCount").attr('value', days);

                addMilestoneContainer.hide();
                jq("#" + targetId + " .mainInfo").show();
                addMilestoneContainer.removeClass('edit');
            } else {
                milestoneCounter++;
                var milestone = {};
                milestone.title = jq.trim(milestoneTitle.val());
                milestone.duration = jq("#addMilestoneContainer select option:selected").attr('value');
                milestone.tasks = [];
                milestone.number = milestoneCounter;
                jq.tmpl("projects_templatesEditMilestoneTmpl", milestone).appendTo("#listAddedMilestone");
                milestoneTitle.val("");
                milestoneTitle.focus();
            }
        });
        
        //milestone menu
        jq(document).on('click', ".milestone .mainInfo .title, .milestone .mainInfo .daysCount", function() {
            ASC.Projects.Templates.hideAddTaskContainer();
            ASC.Projects.Templates.hideAddMilestoneContainer();
            jq("#addMilestoneContainer").addClass('edit');
            var target = jq(this).closest('.milestone').attr('id');
            jq("#addMilestoneContainer").attr('target', target);
            var milestone = jq("#" + target + " .mainInfo");
            jq("#addMilestoneContainer").prependTo(jq("#" + target));
            jq(milestone).hide();
            var val = jq(milestone).find(".daysCount").attr('value');
            jq("#addMilestoneContainer").show();
            jq("#addMilestoneContainer #newMilestoneTitle").val(jq(milestone).children(".title").text());
            jq("#addMilestoneContainer select option[value = '" + val + "']").attr("selected", "selected");

            jq("#addMilestoneContainer #newMilestoneTitle").focus();

        });
        jq(document).on('click', ".milestone .mainInfo .addTask", function() {
            ASC.Projects.Templates.hideAddTaskContainer();
            var target = jq(this).closest('.milestone').attr('id');
            var milestTasksCont = jq("#" + target + " .milestoneTasksContainer");
            jq("#addTaskContainer").appendTo(milestTasksCont[0]);
            jq(milestTasksCont).find('.addTaskContainer').hide();
            jq("#addTaskContainer").attr("target", target);
            jq("#addTaskContainer").show();
            jq(milestTasksCont).show();
            jq("#addTaskContainer #newTaskTitle").focus();
        });

        jq("#milestoneActions .actionList #removeMilestone").bind('click', function() {
            ASC.Projects.Templates.hideAddTaskContainer();
            jq("#addTaskContainer").appendTo("#noAssignTaskContainer");
            jq("#milestoneActions").hide();
            var target = jq(this).parents('.studio-action-panel').attr('target');
            jq("#" + target).removeClass("open");
            jq("#" + target).remove();
        });

        jq("#milestoneActions .actionList #addTaskInMilestone").bind('click', function() {
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

        jq("#milestoneActions .actionList #editMilestone").bind('click', function() {
            jq("#milestoneActions").hide();
            ASC.Projects.Templates.hideAddMilestoneContainer();
            jq("#addMilestoneContainer").addClass('edit');
            var target = jq(this).parents('.studio-action-panel').attr('target');
            jq("#addMilestoneContainer").attr('target', target);
            jq("#" + target).removeClass("open");
            var milestone = jq("#" + target + " .mainInfo");
            jq("#addMilestoneContainer").prependTo(jq("#" + target));
            jq(milestone).hide();
            var val = jq(milestone).children(".daysCount").attr('value');
            jq("#addMilestoneContainer").show();
            jq("#addMilestoneContainer #newMilestoneTitle").val(jq(milestone).children(".title").text());
            jq("#addMilestoneContainer select option[value = '" + val + "']").attr("selected", "selected");

            jq("#addMilestoneContainer #newMilestoneTitle").focus();
        });

        //task

        jq(document).on('click', ".task .title" , function () {
            ASC.Projects.Templates.hideAddMilestoneContainer();
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
            jq("#addTaskContainer #newTaskTitle").val(jq(target).children(".title").text());
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
                        ASC.Projects.Templates.hideAddTaskContainer();
                        taskContainer.removeClass('edit');
                        jq("#" + target).show();
                    } else {
                        ASC.Projects.Templates.hideAddTaskContainer();
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
                jq("#" + target + " .title").text(taskTitle.val());
                taskContainer.removeClass('edit');
                ASC.Projects.Templates.hideAddTaskContainer();
                jq("#" + target).show();
            } else {
                taskCounter++;
                var task = {};
                task.title = jq.trim(taskTitle.val());
                task.number = taskCounter;
                var tElem;
                if (target == 'noAssign') {
                    tElem = jq("#listNoAssignListTask");
                }
                else {
                    tElem = jq(".listTasks[milestone='" + target + "']");
                    jq("#" + target).find(".addTask").addClass("hide");
                }
                jq.tmpl("projects_templatesEditTaskTmpl", task).appendTo(tElem);
                taskTitle.val("");
                taskTitle.focus();
            }
        });
        
        //task menu

        jq("#taskActionPanel .actionList #removeTask").bind('click', function() {
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

        jq("#taskActionPanel .actionList #editTask").bind('click', function() {
            jq("#taskActionPanel").hide();
            var target = jq(this).parents('.studio-action-panel').attr('target');
            var task = jq("#" + target);
            jq(task).removeClass("open");
            jq("#addTaskContainer").addClass('edit');
            jq("#addTaskContainer").attr('target', target);
            jq("#addTaskContainer").insertAfter(task);
            jq(task).hide();
            jq("#addTaskContainer #newTaskTitle").val(jq(task).children(".title").text());
            jq("#addTaskContainer").show();
            jq("#addTaskContainer #newTaskTitle").focus();
        });

        jq("#saveTemplate").bind("click", function() {
            jq(".requiredFieldError").removeClass("requiredFieldError");

            if (jq.trim(jq("#templateTitle").val()) == "") {
                jq("#templateTitleContainer").addClass("requiredFieldError");
                jq.scrollTo("#templateTitleContainer");
                jq("#templateTitle").focus();
                return false;
            }
            action = "save";
            if (ASC.Projects.Templates.checkUnsavedData()) {
                return false;
            }
            ASC.Projects.Templates.hideAddTaskContainer();
            ASC.Projects.Templates.hideAddMilestoneContainer();
            generateAndSaveTemplate('save');
            return false;
        });

        jq('#createProject').bind('click', function() {
            jq(".requiredFieldError").removeClass("requiredFieldError");

            if (jq.trim(jq("#templateTitle").val()) == "") {
                jq("#templateTitleContainer").addClass("requiredFieldError");
                jq.scrollTo("#templateTitleContainer");
                jq("#templateTitle").focus();
                return false;
            }
            action = "saveAndCreateProj";
            if (ASC.Projects.Templates.checkUnsavedData()) {
                return false;
            }
            ASC.Projects.Templates.hideAddTaskContainer();
            ASC.Projects.Templates.hideAddMilestoneContainer();
            generateAndSaveTemplate('saveAndCreateProj');
            return false;
        });

        jq("#cancelCreateProjectTemplate").on("click", function () {
            window.onbeforeunload = null;
            window.location.replace('projectTemplates.aspx');
        })
        jq.confirmBeforeUnload();
    };

    var onGetTemplate = function(params, tmpl) {
        tmplId = tmpl.id;
        showTmplStructure(tmpl);
        jq("#templateTitle").val(tmpl.title);
        jq("#createProject").attr("href", "projectTemplates.aspx?id=" + tmpl.id + "&action=add");
    };

    var showTmplStructure = function(tmpl) {
        var description = { tasks: [], milestones: [] };
        try {
            description = jQuery.parseJSON(tmpl.description) || {tasks:[], milestones:[] };
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
            ASC.Projects.Templates.hideAddTaskContainer();
            ASC.Projects.Templates.showAction('noAssign');
        }
    };

    var generateAndSaveTemplate = function(mode) {
        var description = { tasks: new Array(), milestones: new Array() };

        var listNoAssCont = jq('#noAssignTaskContainer .task');
        for (var i = 0; i < listNoAssCont.length; i++) {
            description.tasks.push({ title: jq(listNoAssCont[i]).children('.title').text() });
        }


        var listMilestoneCont = jq("#listAddedMilestone .milestone");
        for (var i = 0; i < listMilestoneCont.length; i++) {
            var duration = jq(listMilestoneCont[i]).children(".mainInfo").children('.daysCount').attr('value');
            duration = duration.replace(',', '.');
            duration = parseFloat(duration);
            var milestone = { title: jq(listMilestoneCont[i]).children(".mainInfo").children('.title').text(),
                duration: duration,
                tasks: new Array()
            };

            var listTaskCont = jq(listMilestoneCont[i]).children('.milestoneTasksContainer').children(".listTasks").children('.task');
            for (var j = 0; j < listTaskCont.length; j++) {
                milestone.tasks.push({ title: jq(listTaskCont[j]).children('.title').text() });
            }

            description.milestones.push(milestone);
        }
        var data = {};

        data.title = jq.trim(jq("#templateTitle").val());
        data.description = JSON.stringify(description);

        var success = onSave;

        if (mode == 'saveAndCreateProj') {
            success = onSaveAndCreate;
        }

        if (tmplId) {
            data.id = tmplId;
            Teamlab.updatePrjTemplate({}, data.id, data, { success: success, before: LoadingBanner.displayLoading, after: LoadingBanner.hideLoading });
        }
        else {
            Teamlab.createPrjTemplate({}, data, { success: success, before: LoadingBanner.displayLoading, after: LoadingBanner.hideLoading });
        }
    };

    var onSave = function () {
        window.onbeforeunload = null;
        document.location.replace("projectTemplates.aspx");
    };
    var onSaveAndCreate = function(params, tmpl) {
        if (tmpl.id) {
            window.onbeforeunload = null;
            document.location.replace("projectTemplates.aspx?id=" + tmpl.id + "&action=add");
        }
    };
    return {
        init: init
    };

})(jQuery);

ASC.Projects.CreateProjectFromTemplate = (function() {
    var templId, regionalFormatDate, chooseRespStr, showRespCombFlag = false,
        milestoneCounter = 0, taskCounter = 0;
    var pmId = null;
    var continueFlag = false;

    var showChooseResponsible = function () {
        var projectMembers = jq("#Team").find(".projectMember");
        if (projectMembers.length == 0 && !pmId) {
            jq(".chooseResponsible").remove();
            showRespCombFlag = false;
            return;
        }
        if (!showRespCombFlag) {
            if (!jq(".chooseResponsible").length) {
                jq(".milestone .mainInfo .chooseResponsible").removeClass("nobody");
                jq("#addTaskContainer #newTaskTitle").after(chooseRespStr);
                jq("#addMilestoneContainer #newMilestoneTitle").after(chooseRespStr);
            }
            
            jq(".task, .milestone .mainInfo").find(".entity-menu").each(function () {
                jq(this).after(chooseRespStr);
            });

            showRespCombFlag = true;
        }
        updateProjectMemberPanel();
    };

    var updateProjectMemberPanel = function() {
        jq("#projectMemberPanel .actionList li").remove();
        var projectMembers = jq("#Team").find(".projectMember");
        var pmName = projectManagerSelector.SelectedUserName;

        if (pmId)
            jq("#projectMemberPanel .actionList").append("<li id='" + pmId + "' class='dropdown-item'>" + pmName + "</li>");

        if (projectMembers.length) {
            for (var i = 0; i < projectMembers.length; i++) {
                var guid = jq(projectMembers[i]).attr('guid');
                var name = jq.trim(jq(projectMembers[i]).find(".name").text());
                jq("#projectMemberPanel .actionList").append("<li id='" + guid + "' class='dropdown-item'>" + name + "</li>");
            }
        }
        updateMilestoneAndTaskResponsible();
        jq("#projectMemberPanel .actionList").prepend("<li id='nobody' class='dropdown-item'>" + jq("#projectMemberPanel .actionList").attr("nobodyItemText") + "</li>");
    };
    var responsibleInTeam = function(entity, team) {
        var oldResp = jq(entity).attr("guid");
        var inTeam = false;
        for (var i = 0; i < team.length; i++) {
            if (jq(team[i]).attr("guid") == oldResp || pmId == oldResp) {
                inTeam = true;
                break;
            }
        }
        return inTeam;
    };
    var updateMilestoneAndTaskResponsible = function() {
        var team = jq("#Team .userLink");
        var pmName = projectManagerSelector.SelectedUserName;
        var listEntities = jq(".milestone .mainInfo .chooseResponsible .dottedLink, .projects-templates-container .task .dottedLink[guid], #addTaskContainer .chooseResponsible .dottedLink[guid], #addMilestoneContainer .chooseResponsible .dottedLink");

        for (var i = 0; i < listEntities.length; i++) {
            if (!responsibleInTeam(jq(listEntities[i]), team)) {
                if (pmId) {
                    jq(listEntities[i]).attr("guid", pmId);
                    jq(listEntities[i]).text(pmName);
                }
                else {
                    jq(listEntities[i]).attr("guid", jq("#Team .userLink:last").attr("guid"));
                    jq(listEntities[i]).text(jq("#Team .userLink:last").text());
                }
                jq(listEntities[i]).parent().css("display", "inline-block");
            }
        }
    };

    var init = function (str) {
        chooseRespStr = str;
        templId = jq.getURLParam('id');
        Teamlab.getPrjTemplate({}, templId, { success: onGetTemplate, error: onErrorGetTemplate, before: LoadingBanner.displayLoading, after: LoadingBanner.hideLoading });
        jq.confirmBeforeUnload();
    };

    var onErrorGetTemplate = function () {
        window.onbeforeunload = null;
        location.href = "projectTemplates.aspx#elementNotFound";
    };

    var onGetTemplate = function (param, templ) {
        
        //datepicker
        jq("#dueDate").val("");
        jq("#dueDate").datepicker().mask(ASC.Resources.Master.DatePatternJQ);
        jq("#dueDate").datepicker({
            onSelect: function() {
                jq("#newMilestoneTitle").focus();
            }
        });
        regionalFormatDate = jq("#dueDate").datepicker("option", "dateFormat");

        //
        jq("#attentionPopup .continue").click(function() {
            continueFlag = true;
            createProject();
            jq.unblockUI();
        });

        //get tmpl
        
        var val = jq.format(jq("#projectTitle").attr("defText"), templ.title);
        jq("#projectTitle").val(val);
        showProjStructure(templ);
        
        jq("#projectDescription").val("");
        jq("#notifyManagerCheckbox").attr("disabled", "disabled");
        jq('#projectTitle').focus();

        //team popup
        jq('#manageTeamButton').click(function () {
            ASC.Projects.Templates.hideAddTaskContainer();
            ASC.Projects.Templates.hideAddMilestoneContainer();
            
            jq('#Team span.userLink').each(function() {
                var userId = jq(this).closest('tr').attr('guid');
                projectTeamSelector.SelectUser(userId);
            });

            projectTeamSelector.IsFirstVisit = true;
            projectTeamSelector.ShowDialog();
        });

        jq('#Team').on('click', ".projectMember td.delMember span", function() {
            var userId = jq(this).closest('tr').attr('guid');
            jq(this).closest('tr').remove();

            jq('#usrdialog_' + projectTeamSelector.ID + '_usr_' + userId).attr('checked', 'checked');
            projectTeamSelector.PreSelectUser(userId);
            projectTeamSelector.SelectUser(userId);
            projectTeamSelector.Unselect();

            var projectMembers = jq("#Team").find(".projectMember");
            if (projectMembers.length) {
                updateProjectMemberPanel();
            } else {
                showChooseResponsible();
            }
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
        });

        jq("body").click(function(event) {
            if (event.target.className != "userName") {
                if (!projectManagerSelector.SelectedUserId) {
                    pmId = null;
                    showChooseResponsible();
                }
            }
        });

        projectTeamSelector.OnOkButtonClick = function() {
            jq('#Team table').empty();

            var members = projectTeamSelector.GetSelectedUsers();
            for (var i = 0; i < members.length; i++) {
                var pid = members[i].ID;
                var pname = members[i].Name;
                var pdepartment = members[i].Group.Name;
                var ptitle = members[i].Title;
                var newMember = ('<tr class="projectMember" guid=' + pid + '><td class="name"><span class="userLink" guid=' + pid + '>' + jq.htmlEncodeLight(pname)
                    + '</span></td><td class="department"><span>' + jq.htmlEncodeLight(pdepartment) + '</span></td><td class="title"><span>'
                    + jq.htmlEncodeLight(ptitle) + '</span></td><td class="delMember"><span></span></td></tr>');
                jq(newMember).appendTo("#Team table");
            }
            if (jq(".chooseResponsible").length) {
                updateProjectMemberPanel();
            } else {
                showChooseResponsible();
            }
        };
        //choose responsible
        jq(document).on("click", ".task .chooseResponsible", function() {
            jq(this).closest(".task").addClass('open');
            ASC.Projects.Templates.showActionsPanel("projectMemberPanel", this);
            jq("#projectMemberPanel #nobody").show();
            var target = jq(this).closest(".task").attr("id");
            jq("#projectMemberPanel").attr("target", target);
        });

        jq("#addTaskContainer").on("click", ".chooseResponsible", function() {
            ASC.Projects.Templates.showActionsPanel("projectMemberPanel", this);
            jq("#projectMemberPanel #nobody").show();
            jq("#projectMemberPanel").attr("target", "newTask");
        });

        jq("#addMilestoneContainer").on("click", ".chooseResponsible", function() {
            ASC.Projects.Templates.showActionsPanel("projectMemberPanel", this);
            jq("#projectMemberPanel #nobody").hide();
            jq("#projectMemberPanel").attr("target", "newMilestone");
        });

        jq(document).on("click", ".milestone .mainInfo .chooseResponsible", function() {
            jq(this).closest(".milestone").addClass('open');
            ASC.Projects.Templates.showActionsPanel("projectMemberPanel", this);
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

        // onChoosePM
        projectManagerSelector.AdditionalFunction = function () {
            projectTeamSelector.EnableUser(pmId);

            pmId = projectManagerSelector.SelectedUserId;

            if (pmId != Teamlab.profile.id) {
                jq("#notifyManagerCheckbox").removeAttr("disabled");
            } else {
                jq("#notifyManagerCheckbox").attr("disabled", "disabled");
                jq("#notifyManagerCheckbox").removeAttr("checked");
            }

            projectTeamSelector.DisableUser(pmId);

            var projectMembers = jq("#Team").find(".projectMember");
            if (projectMembers.length != 0 || showRespCombFlag) {
                updateProjectMemberPanel();
            } else {
                showChooseResponsible();
            }
        };


        //milestone
        jq(document).on('click', ".milestone .mainInfo .title, .milestone .mainInfo .dueDate", function() {
            ASC.Projects.Templates.hideAddMilestoneContainer();
            ASC.Projects.Templates.hideAddTaskContainer();
            jq("#addMilestoneContainer").addClass('edit');
            var target = jq(this).closest('.milestone').attr('id');
            jq("#addMilestoneContainer").attr('target', target);
            var milestone = jq("#" + target + " .mainInfo");
            jq("#addMilestoneContainer").prependTo(jq("#" + target));
            jq(milestone).hide();
            var val = jq(milestone).children(".dueDate").text();
            val = jq.datepicker.parseDate(regionalFormatDate, val);

            var chooseRespBlock = jq(milestone).find(".chooseResponsible").clone();
            jq("#addMilestoneContainer .chooseResponsible").remove();
            jq("#addMilestoneContainer").append(jq(chooseRespBlock));

            var pm = projectManagerSelector.SelectedUserName;
            if (pmId) {
                jq(chooseRespBlock).find(".dottedLink").attr("guid", pmId);
                jq(chooseRespBlock).find(".dottedLink").text(pm);
            }

            jq("#addMilestoneContainer").show();
            jq("#addMilestoneContainer #newMilestoneTitle").val(jq(milestone).children(".title").text());

            jq("#addMilestoneContainer #dueDate").datepicker('setDate', val);
            jq("#addMilestoneContainer #newMilestoneTitle").focus();
        });

        jq("#addMilestone a").bind('click', function () {
            ASC.Projects.Templates.hideAddTaskContainer();
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

                var pm = projectManagerSelector.SelectedUserName;
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

                jq("#" + targetId + " .mainInfo .title").text(jq.trim(milestoneTitle.val()));

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

        jq(document).on('click', ".milestone .mainInfo .addTask", function() {
            ASC.Projects.Templates.hideAddMilestoneContainer();
            ASC.Projects.Templates.hideAddTaskContainer();
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

        jq("#milestoneActions .actionList #removeMilestone").bind('click', function() {
            jq("#addTaskContainer").hide();
            jq("#addTaskContainer").appendTo("#noAssignTaskContainer");
            jq("#milestoneActions").hide();
            var target = jq(this).parents('.studio-action-panel').attr('target');
            jq("#" + target).removeClass("open");
            jq("#" + target).remove();
        });

        jq("#milestoneActions .actionList #addTaskInMilestone").bind('click', function() {
            ASC.Projects.Templates.hideAddMilestoneContainer();
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

        jq("#milestoneActions .actionList #editMilestone").bind('click', function() {
            jq("#milestoneActions").hide();
            ASC.Projects.Templates.hideAddMilestoneContainer();
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
            jq("#addMilestoneContainer .chooseResponsible").remove();
            jq("#addMilestoneContainer div:first-child").append(jq(chooseRespBlock));

            jq("#addMilestoneContainer").show();
            jq("#addMilestoneContainer #newMilestoneTitle").val(jq(milestone).children(".title").text());

            jq("#addMilestoneContainer #dueDate").datepicker('setDate', val);
            jq("#addMilestoneContainer #newMilestoneTitle").focus();
        });

        //task

        jq(document).on('click', ".task .title", function() {
            ASC.Projects.Templates.hideAddMilestoneContainer();
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
                        ASC.Projects.Templates.hideAddTaskContainer();
                        taskContainer.removeClass('edit');
                        jq("#" + target).show();
                    } else {
                        ASC.Projects.Templates.hideAddTaskContainer();
                    }
                }
            }
        });
        
        jq("#addTaskContainer .button").on('click', function() {
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
                jq("#" + target + " .title").text(taskTitle.val());
                taskContainer.removeClass('edit');
                ASC.Projects.Templates.hideAddTaskContainer();
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
                if (projectManagerSelector.SelectedUserId || jq("#Team").find(".projectMember").length) {
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
                    jq("#" + target).find(".addTask").addClass("hide");
                }
                jq.tmpl("projects_templatesCreateTaskTmpl", task).appendTo(tElem);

                taskTitle.val("");
                taskTitle.focus();
            }

        });

        //task menu

        jq("#taskActionPanel .actionList #removeTask").bind('click', function() {
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

        jq("#taskActionPanel .actionList #editTask").bind('click', function() {
            jq("#taskActionPanel").hide();
            editTask(jq("#" + jq(this).parents('.studio-action-panel').attr('target')));
        });

        jq("#createProject").click(function() {
            jq(".requiredFieldError").removeClass("requiredFieldError");

            if (jq.trim(jq("#projectTitle").val()) == "") {
                jq("#projectTitleContainer").addClass("requiredFieldError");
                jq.scrollTo("#projectTitleContainer");
                jq("#projectTitle").focus();
                return false;
            }

            if (projectManagerSelector.SelectedUserName == "") {
                jq("#pmContainer").addClass("requiredFieldError");
                jq.scrollTo("#pmContainer");
                return false;
            }
            if (ASC.Projects.Templates.checkUnsavedData()) {
                return false;
            }
            createProject();
        });

        jq("#cancelCreateProjectTemplate").on("click", function() {
            window.onbeforeunload = null;
            window.location.replace('projectTemplates.aspx');
        });
    };

    var editTask = function (task) {
        jq(task).removeClass("open");
        jq("#addTaskContainer").addClass('edit');
        jq("#addTaskContainer").attr('target', jq(task).attr("id"));
        jq("#addTaskContainer").insertAfter(task);
        jq(task).hide();

        var chooseRespBlock = jq(task).find(".chooseResponsible").clone();
        jq("#addTaskContainer .chooseResponsible").remove();
        jq("#addTaskContainer div:first-child").append(jq(chooseRespBlock));
        jq("#addTaskContainer #newTaskTitle").val(jq(task).children(".title").text());
        jq("#addTaskContainer").show();
        jq("#addTaskContainer #newTaskTitle").focus();
    };
    
    var showProjStructure = function(tmpl) {
        var description = { tasks: [], milestones: [] };
        try {
            description = jQuery.parseJSON(tmpl.description) || {tasks:[], milestones:[] };
        } catch(e) {

        } 

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
                if (projectManagerSelector.SelectedUserId) {
                    milestones[i].chooseRep = { id: projectManagerSelector.SelectedUserId, name: projectManagerSelector.SelectedUserName };
                } else if (jq("#Team").find(".projectMember .userLink").length) {
                    milestones[i].chooseRep = { id: jq("#Team").find(".projectMember .userLink:first").attr('guid'), name: jq("#Team").find(".projectMember .userLink:first").text() };
                }
                jq.tmpl("projects_templatesCreateMilestoneTmpl", milestones[i]).appendTo("#listAddedMilestone");
            }
        }
        var noAssignTasks = description.tasks;
        if (noAssignTasks) {
            for (var i = 0; i < noAssignTasks.length; i++) {
                taskCounter++;
                noAssignTasks[i].number = taskCounter;
                noAssignTasks[i].chooseRep = projectManagerSelector.SelectedUserId || jq("#Team").find(".projectMember").length;
                jq.tmpl("projects_templatesCreateTaskTmpl", noAssignTasks[i]).appendTo("#listNoAssignListTask");
            }
            jq("#addTaskContainer").attr("target", 'noAssign');
        }
        ASC.Projects.Templates.showAction('noAssign');
    };

    var getProjMilestones = function() {
        var milestones = new Array();

        var listMilestoneCont = jq("#listAddedMilestone .milestone");
        for (var i = 0; i < listMilestoneCont.length; i++) {
            var deadline = jq.datepicker.parseDate(regionalFormatDate, jq(listMilestoneCont[i]).children(".mainInfo").children('.dueDate').text());
            var milestone = { Title: jq(listMilestoneCont[i]).children(".mainInfo").children('.title').text(),
                DeadLine: '/Date(' + deadline.getTime() + ')/',
                Tasks: new Array()
            };

            var mResponsible = jq(listMilestoneCont[i]).find(".mainInfo").find(".dottedLink");
            if (mResponsible.length) {
                var guid = jq(mResponsible).attr("guid");
                if (typeof guid != 'undefined' && guid != "") {
                    milestone.Responsible = guid;
                }
            }

            var listTaskCont = jq(listMilestoneCont[i]).children('.milestoneTasksContainer').children(".listTasks").children('.task');
            for (var j = 0; j < listTaskCont.length; j++) {
                var task = { Title: jq(listTaskCont[j]).children('.title').text() };
                var tResponsible = jq(listTaskCont[j]).find(".dottedLink");
                if (tResponsible.length) {
                    var guid = jq(tResponsible).attr("guid");
                    if (typeof guid != 'undefined' && guid != "") {
                        task.Responsibles = [guid];
                    }
                }
                milestone.Tasks.push(task);
            }

            milestones.push(milestone);
        }

        return milestones;
    };

    var getNoAssignTasks = function() {
        var listNoAssTaskStr = new Array();
        var listNoAssCont = jq('#noAssignTaskContainer .task');

        for (var i = 0; i < listNoAssCont.length; i++) {
            var task = { Title: jq(listNoAssCont[i]).children('.title').text() };

            var responsible = jq(listNoAssCont[i]).find(".dottedLink");
            if (responsible.length) {
                var guid = jq(responsible).attr("guid");
                if (typeof guid != 'undefined' && guid != "") {
                    task.Responsibles = [guid];
                }
            }

            listNoAssTaskStr.push(task);
        }

        return listNoAssTaskStr;
    };

    var createProject = function() {
        if (ASC.Projects.Templates.checkUnsavedData() && !continueFlag) {
            continueFlag = false;
            return;
        }
        var project = {
            Title: jq("#projectTitle").val(),
            Description: jq("#projectDescription").val(),
            Responsible: projectManagerSelector.SelectedUserId,
            Private: jq("#projectPrivacyCkeckbox").is(':checked')
        };

        var team = [];
        var participants = jq("#Team .projectMember .userLink");
        for (var i = 0; i < participants.length; i++) {
            team.push(jq(participants[i]).closest('tr').attr('guid'));
        }

        var milestones = getProjMilestones();
        var noAssignTasks = getNoAssignTasks();
        var notifyManager = jq("#notifyManagerCheckbox").is(":checked");
        var notifyResponsibles = jq('#notifyResponsibles').is(':checked');

        jq.ajax({
            type: "POST",
            async: false,
            data: { project: JSON.stringify(project), team: JSON.stringify(team), milestones: JSON.stringify(milestones), noAssignTasks: JSON.stringify(noAssignTasks), notifyManager: notifyManager, notifyResponsibles: notifyResponsibles },
            success: onCreate
        });
    };

    var onCreate = function (response) {
        window.onbeforeunload = null;
        document.location.replace("projects.aspx?prjID=" + response);
    };

    return {
        init: init
    };

})(jQuery);