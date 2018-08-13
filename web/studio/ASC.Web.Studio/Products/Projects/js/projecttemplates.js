/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
    var clickEventName = "click", targetAttr = "target", openClass = "open", templateClass = ".template";
    var $listTemplates, $templateActionPanel;
    var teamlab, loadingBanner, resources = ASC.Projects.Resources;

    var init = function () {
        teamlab = Teamlab;
        loadingBanner = LoadingBanner;
        $listTemplates = jq("#listTemplates");

        var resources = ASC.Projects.Resources.ProjectTemplatesResource,
            actionMenuItems = [
                { id: "editTmpl", text: resources.Edit },
                { id: "createProj", text: resources.CreateProject },
                { id: "deleteTmpl", text: resources.Delete }
            ];

        $templateActionPanel = ASC.Projects.Common.createActionPanel($listTemplates, "templateActionPanel", { menuItems: actionMenuItems });

        $templateActionPanel.find("#editTmpl").on(clickEventName, function () {
            var tmplId = buttonOnClick(true);
            window.location.replace('projectTemplates.aspx?id=' + tmplId + '&action=edit');
        });

        $templateActionPanel.find("#deleteTmpl").on(clickEventName, function () {
            idDeleteTempl = parseInt(buttonOnClick());
            ASC.Projects.Base.showCommonPopup("projectTemplateRemoveWarning",
                function () {
                    teamlab.removePrjTemplate({ tmplId: idDeleteTempl }, idDeleteTempl, { success: onDeleteTemplate });
                },
                function () {
                    idDeleteTempl = 0;
                });
        });

        $templateActionPanel.find("#createProj").on(clickEventName, function () {
            var tmplId = buttonOnClick(true);
            window.location.replace('projects.aspx?tmplid=' + tmplId + '&action=add');
        });

        teamlab.getPrjTemplates({}, { success: displayListTemplates, before: loadingBanner.displayLoading, after: loadingBanner.hideLoading });
    };

    function onDeleteTemplate() {
        jq("#" + idDeleteTempl).remove();
        idDeleteTempl = 0;
        var list = $listTemplates.find(templateClass);
        if (!list.length) {
            $listTemplates.hide();
            showEmptyScreen();
        }
        jq.unblockUI();
    };

    function showEmptyScreen() {
        var emptyScreen = {
            img: "templates",
            header: resources.ProjectTemplatesResource.EmptyListTemplateHeader,
            description: resources.ProjectTemplatesResource.EmptyListTemplateDescr,
            button: {
                title: resources.ProjectTemplatesResource.EmptyListTemplateButton,
                onclick: function () {
                    location.href = "projectTemplates.aspx?action=add";
                },
                canCreate: function() {
                    return true;
                }
            }
        };

        jq("#emptyScrCtrlPrj").html(jq.tmpl("projects_emptyScreen", emptyScreen)).show();
        jq("#emptyScrCtrlPrj .addFirstElement").off("click").on("click", emptyScreen.button.onclick);
    }

    function createTemplateTmpl(template) {
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

    function displayListTemplates(params, templates) {
        if (templates.length) {
            for (var i = 0; i < templates.length; i++) {
                var tmpl = createTemplateTmpl(templates[i]);
                jq.tmpl("projects_templateTmpl", tmpl).appendTo($listTemplates);
            }
        } else {
            showEmptyScreen();
        }

        jq.dropdownToggle({
            dropdownID: "templateActionPanel",
            switcherSelector: "#listTemplates .entity-menu",
            addTop: 0,
            addLeft: 10,
            rightPos: true,
            showFunction: function (switcherObj, dropdownItem) {
                var $tmpl = jq(switcherObj).closest(templateClass),
                    $openItem = $listTemplates.find(templateClass + "." + openClass);
                dropdownItem.attr(targetAttr, $tmpl.attr('id'));
                $openItem.removeClass(openClass);
                if (!$openItem.is($tmpl)) {
                    dropdownItem.hide();
                }
                if (dropdownItem.is(":hidden")) {
                    $tmpl.addClass(openClass);
                }
            },
            hideFunction: function () {
                $listTemplates.find(templateClass).removeClass(openClass);
            }
        });
    };
    
    function buttonOnClick(clearOnbeforeunload) {
        jq(".studio-action-panel").hide();
        jq(templateClass).removeClass(openClass);
        if (clearOnbeforeunload) {
            window.onbeforeunload = null;
        }

        return $templateActionPanel.attr(targetAttr);
    }

    return {
        init: init
    };

})(jQuery);

ASC.Projects.EditProjectTemplates = (function() {
    var tmplId = null;
    var clickEventName = "click", requiredFieldErrorClass = "requiredFieldError";
    var $templateTitle, $templateTitleContainer;
    var teamlab, loadingBanner;

    var init = function () {
        teamlab = Teamlab;
        loadingBanner = LoadingBanner;
        $templateTitle = jq("#templateTitle");
        $templateTitleContainer = jq("#templateTitleContainer");

        var month = [];

        for (var i = 1; i <= 12; i = i + 0.5) {
            month.push(i);
        }

        jq("#amContainer")
            .html(jq.tmpl("projects_action_template",
            {
                edit: true,
                month: month
            }));
        ASC.Projects.MilestoneContainer.init();
        tmplId = jq.getURLParam('id');
        if (tmplId) {
            teamlab.getPrjTemplate({}, tmplId, { success: onGetTemplate, before: loadingBanner.displayLoading, after: loadingBanner.hideLoading });
        }

        $templateTitle.focus();

        jq("#saveTemplate").on(clickEventName, function () {
            generateAndSaveTemplate.call(this, 'save');
            return false;
        });

        jq('#createProject').on(clickEventName, function () {
            generateAndSaveTemplate.call(this, 'saveAndCreateProj');
            return false;
        });

        jq("#cancelCreateProjectTemplate").on(clickEventName, function () {
            window.onbeforeunload = null;
            window.location.replace('projectTemplates.aspx');
        });

        jq.confirmBeforeUnload(confirmBeforeUnloadCheck);
    };

    function confirmBeforeUnloadCheck() {
        return $templateTitle.val().length ||
            jq("#listAddedMilestone .milestone").length ||
            jq('#noAssignTaskContainer .task').length;
    };

    function onGetTemplate(params, tmpl) {
        tmplId = tmpl.id;
        ASC.Projects.EditMilestoneContainer.showTmplStructure(tmpl);
        $templateTitle.val(tmpl.title);
    };

    function generateAndSaveTemplate(mode) {
        if (jq(this).hasClass("disable")) return;
        jq("." + requiredFieldErrorClass).removeClass(requiredFieldErrorClass);

        if (jq.trim($templateTitle.val()) == "") {
            $templateTitleContainer.addClass(requiredFieldErrorClass);
            jq.scrollTo($templateTitleContainer);
            $templateTitle.focus();
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
        var data = {
            title: jq.trim($templateTitle.val()),
            description: JSON.stringify(description)
        };

        var success = onSave;

        if (mode == 'saveAndCreateProj') {
            success = onSaveAndCreate;
        }

        if (tmplId) {
            data.id = tmplId;
            teamlab.updatePrjTemplate({}, data.id, data, { success: success, before: loadingBanner.displayLoading, after: loadingBanner.hideLoading });
        }
        else {
            teamlab.createPrjTemplate({}, data, { success: success, before: loadingBanner.displayLoading, after: loadingBanner.hideLoading });
        }
    };

    function onSave() {
        window.onbeforeunload = null;
        document.location.replace("projectTemplates.aspx");
    };
    
    function onSaveAndCreate(params, tmpl) {
        if (tmpl.id) {
            window.onbeforeunload = null;
            document.location.replace("projects.aspx?tmplid=" + tmpl.id + "&action=add");
        }
    };
    
    return {
        init: init
    };

})(jQuery);