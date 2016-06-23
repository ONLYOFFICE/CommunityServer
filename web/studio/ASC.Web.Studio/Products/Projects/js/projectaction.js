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


ASC.Projects.ProjectAction = (function() {
    var projectId,
        activeTasksCount,
        activeMilestonesCount,
        opportunityId = undefined,
        contactId = undefined,
        projectTeam = new Array(),
        currentUserId,
        projectResponsible,
        $projectManagerSelector,
        $projectTeamSelector,
        $projectStatusContainer;

    var $projectStatus = jq("#projectStatus");

    var init = function() {
        currentUserId = Teamlab.profile.id;
        var param = jq.getURLParam("opportunityID");
        opportunityId = param ? param : undefined;
        param = jq.getURLParam("contactID");
        contactId = param ? param : undefined;

        projectId = jq.getURLParam('prjID');
        activeTasksCount = parseInt(jq('#activeTasks').val());
        activeMilestonesCount = parseInt(jq('#activeMilestones').val()),
        $projectManagerSelector = jq("#projectManagerSelector"),
        $projectTeamSelector = jq("#projectTeamSelector");
        $projectStatusContainer = jq("#projectStatusContainer");

        if (isProjectsCreatedFromCRM()) {
            ASC.CRM.ListContactView.removeMember = removeContactFromProject;
            getCRMParams();
        } else {
            jq('[id$=projectTitle]').focus();
            if (!projectId) {
                clearFields();
            }
        }
        
        if (!projectId && jq("#templateContainer").length) {
            Teamlab.getPrjTemplates({}, { success: onGetTemplates });
        }
        // onChoosePM

        var selectedProjectManager = $projectManagerSelector.attr("data-id");
        $projectManagerSelector.useradvancedSelector({
            itemsSelectedIds: selectedProjectManager.length ? [selectedProjectManager] : [],
            withGuests: false,
            showGroups: true,
            onechosen: true
        }).on("showList", onChooseProjectManager);

        projectResponsible = jq('#projectResponsible').val();

        function onChooseProjectManager(e, item) {
            var id = item.id,
                name = item.title;

            $projectManagerSelector.html(name).attr("data-id", id).removeClass("plus");

            if (id == currentUserId) {
                jq('#notifyManagerCheckbox').prop("checked", false).attr('disabled', 'disabled');
                jq('#followingProjectCheckbox').prop("checked", false).attr('disabled', 'disabled');
            } else {
                jq('#notifyManagerCheckbox').removeAttr('disabled').prop("checked", true);
                jq('#followingProjectCheckbox').removeAttr('disabled');
            }

            if (projectResponsible) {
                $projectTeamSelector.useradvancedSelector("undisable", [projectResponsible]);
            }
            $projectTeamSelector.useradvancedSelector("disable", [id]);
            jq("#projectParticipantsContainer li[participantId=" + id + "] .reset-action ").addClass("hidden");
            projectResponsible = id;
            jq("#projectParticipantsContainer .items-display-list_i[participantid='" + id + "'] .reset-action").click();
        };
        
        jq('[id$=inputUserName]').keyup(function (eventObject) {
            if (eventObject.which == 13) {
                var id = jq('#login').attr('value');
                if (id != -1) {
                    jq('#projectManagerContainer').removeClass('requiredFieldError');
                    jq('#projectManagerSelector .borderBase').removeClass('requiredInputError');
                }
                if (id == currentUserId || (projectResponsible && id == projectResponsible)) {
                    jq('#notifyManagerCheckbox').prop('checked', false).attr('disabled', 'disabled');
                } else {
                    jq('#notifyManagerCheckbox').removeAttr('disabled').prop('checked', true);
                }
            }
        });

        $projectTeamSelector.useradvancedSelector({
            itemsDisabledIds: projectId ? [$projectManagerSelector.attr("data-id")] : [],
            itemsSelectedIds: projectId ? projectTeam : [],
            showGroups: true
        }).on("showList", onChooseProjectTeam);

        function onChooseProjectTeam(e, items) {
            projectTeam = [];

            jq('#projectParticipantsContainer .items-display-list').empty();
            jq('#projectParticipantsContainer').hide();
            displayProjectTeam(items);
            return false;
        };

        if (projectId) {
            var statuses = $projectStatus.attr("data-status").split(";").map(
                function(item) {
                    var keyValuePair = item.split(',');
                    return { id: keyValuePair[0], title: keyValuePair[1] }
                });

            $projectStatus.advancedSelector({
                itemsSelectedIds: $projectStatus.attr("data-id"),
                onechosen: true,
                showSearch: false,
                itemsChoose: statuses,
                sortMethod: function() {
                    return 0;
                }
            }).on("showList", function(event, item) {
                $projectStatusContainer.find(".selected-before").removeClass("selected-before");
                $projectStatus.attr("data-id", item.id).text(item.title).attr("title", item.title);
                showCloseQuestion(item.id);
            });
        }

        // tags
        jq('body').on('click', function(event) {
            var target = (event.target) ? event.target : event.srcElement;
            var element = jq(target);
            if (!element.is('[id$=projectTags]')) {
                jq('#tagsAutocompleteContainer').empty();
            }
        });

        jq('[id$=projectTags]').keydown(function(eventObject) {
            var value = jq('[id$=projectTags]').val();
            var titles = value.split(',');

            var code = eventObject.which;
            //enter
            if (code == 13) {
                var str = '';
                for (var i = 0; i < titles.length - 1; i++) {
                    str += jq.trim(titles[i]);
                    str += ', ';
                }

                if (jq('.tagAutocompleteSelectedItem').length != 0) {
                    jq('[id$=projectTags]').val(str + jq('.tagAutocompleteSelectedItem').text() + ', ');
                }

                jq('#tagsAutocompleteContainer').html('');
                return false;
            }
            return true;
        });

        jq('[id$=projectTags]').keyup(function(eventObject) {
            jq('#tagsAutocompleteContainer').on('click', function() { return false; });

            var value = jq('[id$=projectTags]').val();
            var titles = value.split(',');

            var width = document.getElementById(jq(this).attr('id')).offsetWidth;
            jq('#tagsAutocompleteContainer').css('width', width + 'px');

            var code = eventObject.which;
            //left
            if (code == 37) { return; }
            //up
            else if (code == 38) { moveSelected(true); return; }
            //right
            else if (code == 39) { return; }
            //down
            else if (code == 40) { moveSelected(false); return; }

            var tagName = titles[titles.length - 1];
            if (jq.trim(tagName) != '') {
                Teamlab.getPrjTagsByName({ titles: titles }, tagName, { tagName: tagName }, { success: onGetTags });
            }
        });

        // title
        jq('[id$=projectTitle]').keyup(function() {
            if (jq.trim(jq(this).val()) != '') {
                jq('#projectTitleContainer').removeClass('requiredFieldError');
            }
        });

        // popup and other button

        jq('#projectActionButton').click(function() {
            if (!validateProjectData() || jq(this).hasClass("disable")) return;

            jq(this).addClass("disable");
            var project = getProjectData();

            lockProjectActionPageElements();

            if (projectId) {
                updateProject(projectId, project);
            } else {
                addProject(project);
            }
        });

        jq('#projectDeleteButton').click(function () {
            StudioBlockUIManager.blockUI(jq('#questionWindowDeleteProject'), 400, 400, 0, "absolute");
        });

        jq("#cancelEditProject").click(function () {
            if (jq(this).hasClass("disable")) return;
            window.onbeforeunload = null;
            document.location.href = document.referrer;
        });

        jq('#questionWindowDeleteProject .remove').on('click', function() {
            lockProjectActionPageElements();
            deleteProject(projectId);
            return false;
        });

        jq('#questionWindowDeleteProject .cancel, #questionWindowActiveTasks .cancel, #questionWindowActiveMilestones .cancel').on('click', function() {
            jq.unblockUI();
            return false;
        });
        
        jq('#projectParticipantsContainer').on('click', '.items-display-list .reset-action', function () {
            var id = jq(this).attr('data-userid');
            deleteItemInArray(projectTeam, id);
            jq('#projectParticipants').attr('value', projectTeam.join());
            if (projectTeam.length == 0) {
                jq('#projectParticipantsContainer').hide();
            }
            if (ASC.Projects.MilestoneContainer) return;
            jq('#projectParticipantsContainer li[participantId=' + id + ']').remove();
            $projectTeamSelector.useradvancedSelector("undisable", [id]).useradvancedSelector("unselect", [id]);
        });

        if (projectId) {
            var teamPrj = ASC.Projects.Master.Team.map(function(el) {
                return {
                    id: el.id,
                    title: el.displayName,
                    profileUrl: el.profileUrl
                };
            });
            if ($projectManagerSelector.attr("data-id") == Teamlab.profile.id) {
                $projectManagerSelector.useradvancedSelector("selectBeforeShow", { id: Teamlab.profile.id, title: ASC.Resources.Master.Resource.MeLabel });
            }
            displayProjectTeam(teamPrj);
        }
        if (!projectId && ASC.Projects.MilestoneContainer) {
            ASC.Projects.MilestoneContainer.init();
        }
        jq.confirmBeforeUnload(confirmBeforeUnloadCheck);

    };

    var confirmBeforeUnloadCheck = function () {
        var project = getProjectData();
        return project.title.length ||
            project.responsibleid ||
            project.notify ||
            project.description.length ||
            project.tags.length ||
            (project.milestones && project.milestones.length) ||
            (project.tasks && project.tasks.length) ||
            (project.participants && project.participants.length);
    };

    var displayProjectTeam = function (team) {
        projectTeam = team.map(function (item) { return item.id; });
        var projectManagerId = jq("#projectManagerSelector").attr("data-id");
        jq('#projectParticipants').attr('value', projectTeam.join());
        if (team.length > 0) {
            jq('#projectParticipant').tmpl(team).appendTo('#projectParticipantsContainer .items-display-list');
            if (projectManagerId)
                jq("#projectParticipantsContainer li[participantId=" + projectManagerId + "] .reset-action").addClass("hidden");
            jq('#projectParticipantsContainer').show();
            $projectTeamSelector.useradvancedSelector("select", projectTeam);
        }
    };

    var clearFields = function() {
        jq('[id$=projectTitle], [id$=projectDescription], [id$=projectTags]').val("");
        jq('#notifyManagerCheckbox , #followingProjectCheckbox').prop("checked", false);
    };

    var validateProjectData = function () {
        jq('#projectTitleContainer , #projectManagerContainer, #projectStatusContainer').removeClass('requiredFieldError');
        jq('#projectManagerSelector .borderBase').removeClass('requiredInputError');

        var title = jq.trim(jq('[id$=projectTitle]').val()),
            responsibleid = jq("#projectManagerSelector").attr("data-id"),
            status = $projectStatus.attr("data-id"),
            isError = false,
            scrollTo = '';

        if (!title) {
            jq('#projectTitleContainer').addClass('requiredFieldError');
            isError = true;
            scrollTo = '#projectTitleContainer';
        }
        if (!responsibleid) {
            jq('#projectManagerContainer').addClass('requiredFieldError');
            jq('#projectManagerSelector .borderBase').addClass('requiredInputError');
            isError = true;
            scrollTo = scrollTo || '#projectManagerContainer';
        }
        if (projectId && status == 1 && showCloseQuestion(status)) {
            $projectStatusContainer.addClass('requiredFieldError');
            isError = true;
            scrollTo = scrollTo || '#projectStatusContainer';
        }
        if (isError) {
            jq('body').scrollTo(scrollTo);
            return false;
        }
        return true;
    };

    var getProjectData = function() {
        var project =
        {
            title: jq.trim(jq('[id$=projectTitle]').val()),
            responsibleid: jq("#projectManagerSelector").attr("data-id"),
            notify: jq('#notifyManagerCheckbox').is(':checked'),
            description: jq('[id$=projectDescription]').val(),
            tags: jq('[id$=projectTags]').val(),
            'private': jq('#projectPrivacyCkeckbox').is(':checked'),
            templateProjectId: jq('#SelectedTemplateID').val()
        };
        
        if (ASC.Projects.CreateMilestoneContainer) {
            project.milestones = ASC.Projects.CreateMilestoneContainer.getProjMilestones();
            project.tasks = ASC.Projects.CreateMilestoneContainer.getProjTasks();
        }

        if (jq.getURLParam("prjID")) {
            project.status = $projectStatus.attr("data-id");
        }
        if (jq('#projectParticipants').length != 0) {
            var participants = jq('#projectParticipants').attr('value'),
                team = [];
            if (participants) {
                team = participants.split(',');
                if (team.length > 0)
                    project.participants = team;
            }
        }
        return project;
    };

    var removeContactFromProject = function(contactid) {
        var contactContainer = jq("#contactTable");
        contactContainer.find("tr[id='contactItem_" + contactid + "']").remove();
        if (!contactContainer.find("tr").length) {
            jq("#projectContactsContainer .no-linked-contacts").show();
        }
    };

    var deleteItemInArray = function(array, item) {
        for (var i = 0; i < array.length; i++) {
            if (array[i] == item) {
                array.splice(i, 1);
                return;
            }
        }
    };

    var getActiveTasksCount = function() {
        return activeTasksCount;
    };

    var getActiveMilestonesCount = function() {
        return activeMilestonesCount;
    };

    var isProjectsCreatedFromCRM = function() {
        if (opportunityId || contactId) {
            return true;
        } else {
            return false;
        }
    };

    var getCRMParams = function() {
        if (opportunityId) {
            Teamlab.getCrmOpportunity({}, opportunityId, { success: onGetCRMOpportunity });
            Teamlab.getCrmEntityMembers({}, "opportunity", opportunityId, { success: onGetCRMContacts });
        }
        if (contactId) {
            Teamlab.getCrmContact({}, contactId, { success: onGetCRMContact });
        }
    };

    var clearSelectedTemplate = function() {
        jq('#SelectedTemplateID').val(0);

        jq('[id$="_TemplatesDropdownContainer"]').show();
        jq('#SelectedTemplateTitle').hide();
        jq('#ClearSelectedTemplate').hide();

        jq('label[for=notify]').text(ASC.Projects.Resources.ProjectsJSResource.NotifyProjectLeader);
    };

    var addUsersToCommand = function(users) {
        var userIds = [],
            usersCount = users.length;
        if (!usersCount) return;
        for (var i = 0; i < usersCount; i++) {
            userIds.push(users[i].id);
        }
        jq("#projectTeamSelector").useradvancedSelector("select", [userIds]);
    };

    var appendContacts = function(project) {
        var contacts = jq("#contactTable tr");
        var contactsCount = contacts.length;
        if (contactsCount == 0) {
            checkFollowProj(project);
            return;
        };
        var contactsIds = [];
        for (var i = 0; i < contactsCount; i++) {
            var id = jq(contacts[i]).attr("id").split("_")[1];
            contactsIds.push(parseInt(id));
        }
        Teamlab.addCrmContactsForProject({ project: project }, project.id, { contactid: contactsIds }, { success: onAddCRMContacts });
    };

    var checkFollowProj = function(project) {
        var following = jq('#followingProjectCheckbox').is(':checked');

        var isManager = project.responsible.id == currentUserId;
        var isInTeam = getArrayIndex(currentUserId, projectTeam) != -1;
        if (following && !isInTeam & !isManager) {
            Teamlab.followingPrjProject({}, project.id, { projectId: project.id }, { success: onFollowingProject });
        } else {
            window.onbeforeunload = null;
            document.location.replace('tasks.aspx?prjID=' + project.id);
        }
    };

    var getArrayIndex = function(value, array) {
        var index = -1;
        for (var i = 0; i < array.length; i++) {
            if (array[i] === value) {
                index = i;
                break;
            }
        }
        return index;
    };

    var moveSelected = function(up) {
        if (jq('#tagsAutocompleteContainer').html() == '') return;

        var items = jq('#tagsAutocompleteContainer .tagAutocompleteItem');

        var selected = false;
        items.each(function(idx) {
            if (jq(this).is('.tagAutocompleteSelectedItem')) {
                selected = true;
                if (up && idx > 0) {
                    jq(this).removeClass('tagAutocompleteSelectedItem');
                    items.eq(idx - 1).addClass('tagAutocompleteSelectedItem');
                    jq('#tagsAutocompleteContainer').scrollTo(items.eq(idx - 1).position().top, 100);
                    return false;
                } else if (!up && idx < items.length - 1) {
                    jq(this).removeClass('tagAutocompleteSelectedItem');
                    items.eq(idx + 1).addClass('tagAutocompleteSelectedItem');
                    jq('#tagsAutocompleteContainer').scrollTo(items.eq(idx + 1).position().top, 100);
                    return false;
                }
            }
            return true;
        });
        if (!selected) {
            items.eq(0).addClass('tagAutocompleteSelectedItem');
        }
    };

    function showCloseQuestion(status) {
        var activeTasksCount = ASC.Projects.ProjectAction.getActiveTasksCount();
        var activeMilestonesCount = ASC.Projects.ProjectAction.getActiveMilestonesCount();
        if (status == 1) {
            if (activeTasksCount > 0) {
                StudioBlockUIManager.blockUI(jq('#questionWindowActiveTasks'), 400, 400, 0, "absolute");
                $projectStatusContainer.addClass('requiredFieldError');
            }
            else if (activeMilestonesCount > 0) {
                StudioBlockUIManager.blockUI(jq('#questionWindowActiveMilestones'), 400, 400, 0, "absolute");
                $projectStatusContainer.addClass('requiredFieldError');
            }
        } else {
            $projectStatusContainer.removeClass('requiredFieldError');
            return false;
        }
        return activeTasksCount > 0 || activeMilestonesCount > 0;
    };

    //lock fields

    var lockProjectActionPageElements = function () {
        LoadingBanner.displayLoading();
        jq('[id$=projectTitle]').attr('readonly', 'readonly').addClass('disabled');
        jq('[id$=projectDescription]').attr('readonly', 'readonly').addClass('disabled');
        jq('.inputUserName').attr('disabled', 'disabled');
        jq('#projectManagerSelector td:last').css({ 'display': 'none' });
        jq('#projectParticipantsContainer .items-display-list').removeClass('canedit');
        jq('#projectStatus').attr('disabled', 'disabled').addClass('disabled');
        jq('[id$=projectTags]').attr('readonly', 'readonly').addClass('disabled');
        jq('#notifyManagerCheckbox').attr('disabled', 'disabled');
        jq('#projectPrivacyCkeckbox').attr('disabled', 'disabled');
        jq('#followingProjectCheckbox').attr('disabled', 'disabled');
        jq('#TemplatesComboboxContainer').hide();
        jq('#projectTeamContainer .headerPanel').off();
    };

    var unlockProjectActionPageElements = function () {
        jq('[id$=projectTitle]').removeAttr('readonly').removeClass('disabled');
        jq('[id$=projectDescription]').removeAttr('readonly').removeClass('disabled');
        jq('.inputUserName').removeAttr('disabled');
        jq('#projectManagerSelector td:last').css({ 'display': 'table-cell' });
        jq('#projectParticipantsContainer .items-display-list').addClass('canedit');
        jq('#projectStatus').removeAttr('disabled').removeClass('disabled');
        jq('[id$=projectTags]').removeAttr('readonly').removeClass('disabled');
        jq('#notifyManagerCheckbox').removeAttr('disabled');
        jq('#projectPrivacyCkeckbox').removeAttr('disabled');
        jq('#followingProjectCheckbox').removeAttr('disabled');
        jq('#TemplatesComboboxContainer').show();
        LoadingBanner.hideLoading();
    };

    // actions
    var addProject = function(project) {
        var params = {};
        if (ASC.Projects.ProjectAction.isProjectsCreatedFromCRM()) {
            params.isProjectsCreatedFromCRM = true;
        }
        Teamlab.addPrjProject(params, project, { success: onAddProject, error: onAddProjectError });
    };

    var updateProject = function(projectId, project) {
        Teamlab.updatePrjProject({}, projectId, project, { success: onUpdateProject, error: onUpdateProjectError });
    };

    var deleteProject = function(projectId) {
        Teamlab.removePrjProject({}, projectId, { success: onDeleteProject, error: onDeleteProjectError });
    };

    //handlers
    var onAddProject = function(params, project) {
        project = this.__responses[0];

        if (params.isProjectsCreatedFromCRM) {
            ASC.Projects.ProjectAction.appendContacts(project);
        } else {
            ASC.Projects.ProjectAction.checkFollowProj(project);
        }
    };

    var onUpdateProject = function (params, project) {
        window.onbeforeunload = null;
        document.location.replace('tasks.aspx?prjID=' + project.id);
    };

    var onDeleteProject = function () {
        window.onbeforeunload = null;
        document.location.replace('projects.aspx');
    };

    var onAddProjectError = function() {
        unlockProjectActionPageElements();
    };

    var onUpdateProjectError = function() {
        unlockProjectActionPageElements();
    };

    var onDeleteProjectError = function() {
        unlockProjectActionPageElements();
    };

    var onFollowingProject = function (params, project) {
        window.onbeforeunload = null;
        document.location.replace('projects.aspx?prjID=' + project.id);
    };

    var onGetTags = function(params, tags) {
        var titles = params.titles;
        jq('#tagsAutocompleteContainer').empty();

        if (tags.length > 0) {
            for (var i = 0; i < tags.length; i++) {
                var container = document.createElement('div');

                jq(container).addClass('tagAutocompleteItem');
                jq(container).text(tags[i]);

                jq(container).on('mouseover', function() {
                    jq('div.tagAutocompleteItem').each(function() {
                        jq(this).removeClass('tagAutocompleteSelectedItem');
                    });
                    jq(this).addClass('tagAutocompleteSelectedItem');
                });

                jq(container).on('click', function() {
                    var str = '';
                    for (var j = 0; j < titles.length - 1; j++) {
                        str += jq.trim(titles[j]);
                        str += ', ';
                    }
                    jq('[id$=projectTags]').val(str + jq(this).text() + ', ');
                    jq('#tagsAutocompleteContainer').empty();
                });

                jq('#tagsAutocompleteContainer').append(container);
                jq('[id$=projectTags]').focus();
            }
        }
    };

    var onGetCRMOpportunity = function (params, opportunity) {
        jq("[id$=projectTitle]").val(opportunity.title);
        jq("[id$=projectDescription]").val(opportunity.description);
        jq("#projectManagerSelector").useradvancedSelector("select", [opportunity.responsible.id]);
        jq("#projectManagerSelector").html(Encoder.htmlDecode(opportunity.responsible.displayName)).attr("data-id", opportunity.responsible.id).removeClass("plus");
        if (opportunity.responsible.id == Teamlab.profile.id) {
            jq('#notifyManagerCheckbox').prop("checked", false).attr('disabled', 'disabled');
            jq('#followingProjectCheckbox').prop("checked", false).attr('disabled', 'disabled');
        }
        if (opportunity.isPrivate) {
            jq("#projectPrivacyCkeckbox").prop("checked", true);
            addUsersToCommand(opportunity.accessList);
        }
    };

    var onGetCRMContacts = function (params, contacts) {
        var contactsCount = contacts.length;
        if (contactsCount > 0) {
            ASC.CRM.ListContactView.CallbackMethods.render_simple_content({}, contacts);
        } else {
            jq("#projectContactsContainer .no-linked-contacts").show();
        }

    };

    var onGetCRMContact = function(params, contact) {
        onGetCRMContacts({}, [contact]);
    };
    
    var onAddCRMContacts = function(params, data) {
        checkFollowProj(params.project);
    };

    var onGetTemplates = function (params, data) {
        jq("#templateContainer").removeClass("display-none");
        var templateSelect = jq("#templateSelect");
        templateSelect.projectadvancedSelector(
            {
                itemsChoose: data,
                onechosen: true
            }
        );
        
        templateSelect.on("showList", function (event, item) {
            templateSelect.text(item.title).attr("title", item.title);
            ASC.Projects.CreateMilestoneContainer.onGetTemplate({}, item);
        });
        
        var tmplid = jq.getURLParam("tmplid");
        if (tmplid) {
            var tmpl = data.find(function (item) { return item.id == tmplid; });
            if (tmpl)
                templateSelect.projectadvancedSelector("selectBeforeShow", tmpl);
        }
    };
    
    return {
        init: init,
        getActiveTasksCount: getActiveTasksCount,
        getActiveMilestonesCount: getActiveMilestonesCount,
        clearSelectedTemplate: clearSelectedTemplate,
        isProjectsCreatedFromCRM: isProjectsCreatedFromCRM,
        appendContacts: appendContacts,
        projectTeam: projectTeam,
        checkFollowProj: checkFollowProj
    };

})();

jq(document).ready(function() {
    var url = location.href;
    if (jq.getURLParam("action") && url.indexOf("projects.aspx") > 0) {
        ASC.Projects.ProjectAction.init();
    }
    jq(".mainPageContent").children(".loader-page").hide();
});
