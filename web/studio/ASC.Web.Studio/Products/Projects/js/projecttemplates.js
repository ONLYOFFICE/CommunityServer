/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

ASC.Projects.Templates = (function () {
    var init = function () {
        var tmplId = jq.getURLParam("id"),
            action = jq.getURLParam("action");

        if ((tmplId == null && action == "add") || (tmplId && action == "edit")) {
            ASC.Projects.EditProjectTemplates.init();
        }
        if (tmplId == null && action == null) {
            ASC.Projects.ListProjectsTemplates.init();
        }
    };

    return {
        init: init
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

        jq.dropdownToggle({
            dropdownID: "templateActionPanel",
            switcherSelector: "#listTemplates .entity-menu",
            addTop: 0,
            addLeft: 10,
            rightPos: true,
            showFunction: function (switcherObj, dropdownItem) {
                var $tmpl = jq(switcherObj).closest('.template'),
                    $openItem = jq("#listTemplates .template.open");
                dropdownItem.attr('target', $tmpl.attr('id'));
                $openItem.removeClass("open");
                if (!$openItem.is($tmpl)) {
                    dropdownItem.hide();
                }
                if (dropdownItem.is(":hidden")) {
                    $tmpl.addClass("open");
                }
            },
            hideFunction: function () {
                jq("#listTemplates .template.open").removeClass("open");
            }
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
            StudioBlockUIManager.blockUI(jq('#questionWindow'), 400, 400, 0, "absolute");
        });
        jq("#templateActionPanel #createProj").bind('click', function () {
            var tmplId = jq("#templateActionPanel").attr('target');
            jq(".studio-action-panel").hide();
            jq(".template").removeClass('open');
            window.onbeforeunload = null;
            window.location.replace('projects.aspx?tmplid=' + tmplId + '&action=add');
        });
    };
    
    return {
        init: init
    };

})(jQuery);

ASC.Projects.EditProjectTemplates = (function() {
    var tmplId = null;

    var action = "";

    var init = function() {
        ASC.Projects.MilestoneContainer.init();
        tmplId = jq.getURLParam('id');
        if (tmplId) {
            Teamlab.getPrjTemplate({}, tmplId, { success: onGetTemplate, before: LoadingBanner.displayLoading, after: LoadingBanner.hideLoading });
        }

        jq('#templateTitle').focus();

        jq("#saveTemplate").bind("click", function () {
            generateAndSaveTemplate.call(this, 'save');
            return false;
        });

        jq('#createProject').bind('click', function () {
            generateAndSaveTemplate.call(this, 'saveAndCreateProj');
            return false;
        });

        jq("#cancelCreateProjectTemplate").on("click", function() {
            window.onbeforeunload = null;
            window.location.replace('projectTemplates.aspx');
        });
        jq.confirmBeforeUnload();
    };

    var onGetTemplate = function(params, tmpl) {
        tmplId = tmpl.id;
        ASC.Projects.EditMilestoneContainer.showTmplStructure(tmpl);
        jq("#templateTitle").val(tmpl.title);
    };

    var generateAndSaveTemplate = function (mode) {
        if (jq(this).hasClass("disable")) return;
        jq(".requiredFieldError").removeClass("requiredFieldError");

        if (jq.trim(jq("#templateTitle").val()) == "") {
            jq("#templateTitleContainer").addClass("requiredFieldError");
            jq.scrollTo("#templateTitleContainer");
            jq("#templateTitle").focus();
            return;
        }
        jq(this).addClass("disable");
        ASC.Projects.MilestoneContainer.hideAddTaskContainer();
        ASC.Projects.MilestoneContainer.hideAddMilestoneContainer();
        
        var description = { tasks: new Array(), milestones: new Array() };

        var listNoAssCont = jq('#noAssignTaskContainer .task');
        for (var i = 0; i < listNoAssCont.length; i++) {
            description.tasks.push({ title: jq(listNoAssCont[i]).children('.titleContainer').text() });
        }


        var listMilestoneCont = jq("#listAddedMilestone .milestone");
        for (var i = 0; i < listMilestoneCont.length; i++) {
            var duration = jq(listMilestoneCont[i]).children(".mainInfo").children('.daysCount').attr('value');
            duration = duration.replace(',', '.');
            duration = parseFloat(duration);
            var milestone = {
                title: jq(listMilestoneCont[i]).children(".mainInfo").children('.titleContainerEdit').text(),
                duration: duration,
                tasks: new Array()
            };

            var listTaskCont = jq(listMilestoneCont[i]).children('.milestoneTasksContainer').children(".listTasks").children('.task');
            for (var j = 0; j < listTaskCont.length; j++) {
                milestone.tasks.push({ title: jq(listTaskCont[j]).children('.titleContainer').text() });
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
            document.location.replace("projects.aspx?tmplid=" + tmpl.id + "&action=add");
        }
    };
    
    return {
        init: init
    };

})(jQuery);