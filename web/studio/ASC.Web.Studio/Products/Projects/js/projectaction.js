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
        $projectStatusContainer,
        $projectStatus,
        $projectParticipants,
        $notifyManagerCheckbox,
        $followingProjectCheckbox,
        $projectTitle,
        $projectDescription,
        $projectTags,
        $tagsAutocompleteContainer,
        $projectParticipantsContainer,
        $projectPrivacyCheckbox;

    var disabledAttr = "disabled",
        checkedProp = "checked",
        readonlyAttr = "readonly",
        requiredFieldErrorClass = "requiredFieldError",
        requiredInputErrorClass = "requiredInputError",
        itemsDisplayListClass = ".items-display-list",
        tagAutocompleteSelectedItemClass = "tagAutocompleteSelectedItem";

    var teamlab, loadingBanner;

    var init = function () {
        teamlab = Teamlab;
        loadingBanner = LoadingBanner;

        currentUserId = teamlab.profile.id;
        var param = jq.getURLParam("opportunityID");
        opportunityId = param ? param : undefined;
        param = jq.getURLParam("contactID");
        contactId = param ? param : undefined;

        projectId = jq.getURLParam("prjID");
        activeTasksCount = parseInt(jq("#activeTasks").val());
        activeMilestonesCount = parseInt(jq("#activeMilestones").val()),

        $projectManagerSelector = jq("#projectManagerSelector"),
        $projectTeamSelector = jq("#projectTeamSelector");
        $projectStatusContainer = jq("#projectStatusContainer");
        $projectStatus = jq("#projectStatus");
        $projectParticipants = jq("#projectParticipants");
        $notifyManagerCheckbox = jq("#notifyManagerCheckbox");
        $followingProjectCheckbox = jq("#followingProjectCheckbox");
        $projectTitle = jq("[id$=projectTitle]");
        $projectDescription = jq("[id$=projectDescription]");
        $projectTags = jq("[id$=projectTags]");
        $tagsAutocompleteContainer = jq("#tagsAutocompleteContainer");
        $projectParticipantsContainer = jq("#projectParticipantsContainer");
        $projectPrivacyCheckbox = jq("#projectPrivacyCkeckbox");

        if (isProjectsCreatedFromCRM()) {
            ASC.CRM.ListContactView.removeMember = removeContactFromProject;
            getCRMParams();
        } else {
            $projectTitle.focus();
            if (!projectId) {
                clearFields();
            }
        }
        
        if (!projectId && jq("#templateContainer").length) {
            teamlab.getPrjTemplates({}, { success: onGetTemplates });
        }
        // onChoosePM

        var selectedProjectManager = $projectManagerSelector.attr("data-id");
        $projectManagerSelector.useradvancedSelector({
            itemsSelectedIds: selectedProjectManager.length ? [selectedProjectManager] : [],
            withGuests: false,
            showGroups: true,
            onechosen: true
        }).on("showList", onChooseProjectManager);

        projectResponsible = jq("#projectResponsible").val();

        function onChooseProjectManager(e, item) {
            var id = item.id,
                name = item.title;

            $projectManagerSelector.html(name).attr("data-id", id).removeClass("plus");

            if (id == currentUserId) {
                $notifyManagerCheckbox.prop(checkedProp, false).attr(disabledAttr, disabledAttr);
                $followingProjectCheckbox.prop(checkedProp, false).attr(disabledAttr, disabledAttr);
            } else {
                $notifyManagerCheckbox.removeAttr(disabledAttr).prop(checkedProp, true);
                $followingProjectCheckbox.removeAttr(disabledAttr);
            }

            if (projectResponsible) {
                $projectTeamSelector.useradvancedSelector("undisable", [projectResponsible]);
            }
            $projectTeamSelector.useradvancedSelector("disable", [id]);
            $projectParticipantsContainer.find("li[participantId=" + id + "] .reset-action ").addClass("hidden");
            projectResponsible = id;
            $projectParticipantsContainer.find(".items-display-list_i[participantid='" + id + "'] .reset-action").click();
        };
        
        jq("[id$=inputUserName]").keyup(function (eventObject) {
            if (eventObject.which == 13) {
                var id = jq("#login").attr("value");
                if (id != -1) {
                    jq("#projectManagerContainer").removeClass(requiredFieldErrorClass);
                    $projectManagerSelector.find(".borderBase").removeClass(requiredInputErrorClass);
                }
                if (id == currentUserId || (projectResponsible && id == projectResponsible)) {
                    $notifyManagerCheckbox.prop(checkedProp, false).attr(disabledAttr, disabledAttr);
                } else {
                    $notifyManagerCheckbox.removeAttr(disabledAttr).prop(checkedProp, true);
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

            $projectParticipantsContainer.find(itemsDisplayListClass).empty();
            $projectParticipantsContainer.hide();
            displayProjectTeam(items);
            return false;
        };

        if (projectId) {
            var statuses = $projectStatus.attr("data-status").split(";").map(
                function(item) {
                    var keyValuePair = item.split(",");
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
        jq("body").click(function (event) {
            var target = (event.target) ? event.target : event.srcElement;
            var element = jq(target);
            if (!element.is("[id$=projectTags]")) {
                $tagsAutocompleteContainer.empty();
            }
        });

        $projectTags.keydown(function (eventObject) {
            var value = $projectTags.val();
            var titles = value.split(",");

            var code = eventObject.which;
            //enter
            if (code == 13) {
                var str = "";
                for (var i = 0; i < titles.length - 1; i++) {
                    str += jq.trim(titles[i]);
                    str += ", ";
                }

                if (jq(".tagAutocompleteSelectedItem").length != 0) {
                    $projectTags.val(str + jq(".tagAutocompleteSelectedItem").text() + ", ");
                }

                $tagsAutocompleteContainer.html("");
                return false;
            }
            return true;
        });

        $projectTags.keyup(function (eventObject) {
            $tagsAutocompleteContainer.click(function () { return false; });

            var value = $projectTags.val();
            var titles = value.split(",");

            var width = document.getElementById(jq(this).attr("id")).offsetWidth;
            $tagsAutocompleteContainer.css("width", width + "px");

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
            if (jq.trim(tagName) != "") {
                teamlab.getPrjTagsByName({ titles: titles }, tagName, { tagName: tagName }, { success: onGetTags });
            }
        });

        // title
        $projectTitle.keyup(function () {
            if (jq.trim($projectTitle.val()) != "") {
                jq("#projectTitleContainer").removeClass(requiredFieldErrorClass);
            }
        });

        // popup and other button

        jq("#projectActionButton").click(function() {
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

        jq("#projectDeleteButton").click(function () {
            ASC.Projects.Base.showCommonPopup("projectRemoveWarning",
                function() {
                    lockProjectActionPageElements();
                    teamlab.removePrjProject({}, projectId, { success: onDeleteProject, error: unlockProjectActionPageElements });
                });
        });

        jq("#cancelEditProject").click(function () {
            if (jq(this).hasClass("disable")) return;
            window.onbeforeunload = null;
            document.location.href = document.referrer;
        });
        
        $projectParticipantsContainer.on("click", itemsDisplayListClass + " .reset-action", function () {
            var id = jq(this).attr("data-userid");
            deleteItemInArray(projectTeam, id);
            $projectParticipants.attr("value", projectTeam.join());
            if (projectTeam.length == 0) {
                $projectParticipantsContainer.hide();
            }
            if (ASC.Projects.MilestoneContainer) return;
            $projectParticipantsContainer.find("li[participantId=" + id + "]").remove();
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
            if ($projectManagerSelector.attr("data-id") == currentUserId) {
                $projectManagerSelector.useradvancedSelector("selectBeforeShow", { id: currentUserId, title: ASC.Resources.Master.Resource.MeLabel });
            }
            displayProjectTeam(teamPrj);
        }
        if (!projectId && ASC.Projects.MilestoneContainer) {
            ASC.Projects.MilestoneContainer.init();
        }
        jq.confirmBeforeUnload(confirmBeforeUnloadCheck);

    };

    function confirmBeforeUnloadCheck() {
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

    function displayProjectTeam(team) {
        projectTeam = team.map(function (item) { return item.id; });
        var projectManagerId = $projectManagerSelector.attr("data-id");
        $projectParticipants.attr("value", projectTeam.join());
        if (team.length > 0) {
            jq.tmpl("projectParticipant", team).appendTo($projectParticipantsContainer.find(itemsDisplayListClass));
            if (projectManagerId) {
                $projectParticipantsContainer.find("li[participantId=" + projectManagerId + "] .reset-action").addClass("hidden");
            }
            $projectParticipantsContainer.show();
            $projectTeamSelector.useradvancedSelector("select", projectTeam);
        }
    };

    function clearFields() {
        $projectTitle.add($projectDescription).add($projectTags).val("");
        $notifyManagerCheckbox.add($followingProjectCheckbox).prop(checkedProp, false);
    };

    function validateProjectData() {
        jq("#projectTitleContainer , #projectManagerContainer, #projectStatusContainer").removeClass(requiredFieldErrorClass);
        $projectManagerSelector.find(".borderBase").removeClass(requiredInputErrorClass);

        var title = jq.trim($projectTitle.val()),
            responsibleid = $projectManagerSelector.attr("data-id"),
            status = $projectStatus.attr("data-id"),
            isError = false,
            scrollTo = "";

        if (!title) {
            jq("#projectTitleContainer").addClass(requiredFieldErrorClass);
            isError = true;
            scrollTo = "#projectTitleContainer";
        }
        if (!responsibleid) {
            jq("#projectManagerContainer").addClass(requiredFieldErrorClass);
            $projectManagerSelector.find(".borderBase").addClass(requiredInputErrorClass);
            isError = true;
            scrollTo = scrollTo || "#projectManagerContainer";
        }
        if (projectId && status == 1 && showCloseQuestion(status)) {
            $projectStatusContainer.addClass(requiredFieldErrorClass);
            isError = true;
            scrollTo = scrollTo || "#projectStatusContainer";
        }
        if (isError) {
            jq("body").scrollTo(scrollTo);
            return false;
        }
        return true;
    };

    function getProjectData() {
        var project =
        {
            title: jq.trim($projectTitle.val()),
            responsibleid: $projectManagerSelector.attr("data-id"),
            notify: $notifyManagerCheckbox.is(":" + checkedProp),
            description: $projectDescription.val(),
            tags: $projectTags.val(),
            'private': $projectPrivacyCheckbox.is(":" + checkedProp),
            templateProjectId: jq("#SelectedTemplateID").val()
        };
        
        if (ASC.Projects.CreateMilestoneContainer) {
            project.milestones = ASC.Projects.CreateMilestoneContainer.getProjMilestones();
            project.tasks = ASC.Projects.CreateMilestoneContainer.getProjTasks();
        }

        if (projectId) {
            project.status = $projectStatus.attr("data-id");
        }
        if ($projectParticipants.length != 0) {
            var participants = $projectParticipants.attr("value"),
                team = [];
            if (participants) {
                team = participants.split(",");
                if (team.length > 0)
                    project.participants = team;
            }
        }
        return project;
    };

    function removeContactFromProject(contactid) {
        var contactContainer = jq("#contactTable");
        contactContainer.find("tr[id='contactItem_" + contactid + "']").remove();
        if (!contactContainer.find("tr").length) {
            jq("#projectContactsContainer .no-linked-contacts").show();
        }
    };

    function deleteItemInArray(array, item) {
        for (var i = 0; i < array.length; i++) {
            if (array[i] == item) {
                array.splice(i, 1);
                return;
            }
        }
    };

    function isProjectsCreatedFromCRM() {
        if (opportunityId || contactId) {
            return true;
        } else {
            return false;
        }
    };

    function getCRMParams() {
        if (opportunityId) {
            teamlab.getCrmOpportunity({}, opportunityId, { success: onGetCRMOpportunity });
            teamlab.getCrmEntityMembers({}, "opportunity", opportunityId, { success: onGetCRMContacts });
        }
        if (contactId) {
            teamlab.getCrmContact({}, contactId, { success: onGetCRMContact });
        }
    };

    function addUsersToCommand(users) {
        var userIds = [],
            usersCount = users.length;
        if (!usersCount) return;
        for (var i = 0; i < usersCount; i++) {
            userIds.push(users[i].id);
        }
        $projectTeamSelector.useradvancedSelector("select", [userIds]);
    };

    function appendContacts(project) {
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
        teamlab.addCrmContactsForProject({ project: project }, project.id, { contactid: contactsIds }, { success: onAddCRMContacts });
    };

    function checkFollowProj(project) {
        var following = $followingProjectCheckbox.is(":" + checkedProp);

        var isManager = project.responsible.id == currentUserId;
        var isInTeam = getArrayIndex(currentUserId, projectTeam) != -1;
        if (following && !isInTeam & !isManager) {
            teamlab.followingPrjProject({}, project.id, { projectId: project.id }, { success: onFollowingProject });
        } else {
            window.onbeforeunload = null;
            document.location.replace("tasks.aspx?prjID=" + project.id);
        }
    };

    function getArrayIndex(value, array) {
        var index = -1;
        for (var i = 0; i < array.length; i++) {
            if (array[i] === value) {
                index = i;
                break;
            }
        }
        return index;
    };

    function moveSelected(up) {
        if ($tagsAutocompleteContainer.html() == "") return;

        var items = $tagsAutocompleteContainer.find(".tagAutocompleteItem");

        var selected = false;
        items.each(function(idx) {
            if (jq(this).is(".tagAutocompleteSelectedItem")) {
                selected = true;
                if (up && idx > 0) {
                    jq(this).removeClass(tagAutocompleteSelectedItemClass);
                    items.eq(idx - 1).addClass(tagAutocompleteSelectedItemClass);
                    $tagsAutocompleteContainer.scrollTo(items.eq(idx - 1).position().top, 100);
                    return false;
                } else if (!up && idx < items.length - 1) {
                    jq(this).removeClass(tagAutocompleteSelectedItemClass);
                    items.eq(idx + 1).addClass(tagAutocompleteSelectedItemClass);
                    $tagsAutocompleteContainer.scrollTo(items.eq(idx + 1).position().top, 100);
                    return false;
                }
            }
            return true;
        });
        if (!selected) {
            items.eq(0).addClass(tagAutocompleteSelectedItemClass);
        }
    };

    function showCloseQuestion(status) {
        if (status == 1) {
            var moduleLocationPath = StudioManager.getLocationPathToModule("projects");
            if (activeTasksCount > 0) {
                ASC.Projects.Base.showCommonPopup("projectOpenTaskWarning", function () {
                    location.href = jq.format('{0}tasks.aspx?prjID={1}#sortBy=deadline&sortOrder=ascending&status=open', moduleLocationPath, projectId);
                });
                $projectStatusContainer.addClass(requiredFieldErrorClass);
            }
            else if (activeMilestonesCount > 0) {
                ASC.Projects.Base.showCommonPopup("projectOpenMilestoneWarning", function () {
                    location.href = jq.format('{0}milestones.aspx?prjID={1}#sortBy=deadline&sortOrder=ascending&status=open', moduleLocationPath, projectId);
                });
                $projectStatusContainer.addClass(requiredFieldErrorClass);
            }
        } else {
            $projectStatusContainer.removeClass(requiredFieldErrorClass);
            return false;
        }
        return activeTasksCount > 0 || activeMilestonesCount > 0;
    };

    //lock fields

    function lockProjectActionPageElements() {
        loadingBanner.displayLoading();
        $projectTitle.attr(readonlyAttr, readonlyAttr).addClass(disabledAttr);
        $projectDescription.attr(readonlyAttr, readonlyAttr).addClass(disabledAttr);
        jq(".inputUserName").attr(disabledAttr, disabledAttr);
        $projectManagerSelector.find("td:last").css({ 'display': "none" });
        $projectParticipantsContainer.find(itemsDisplayListClass).removeClass("canedit");
        $projectStatus.attr(disabledAttr, disabledAttr).addClass(disabledAttr);
        $projectTags.attr(readonlyAttr, readonlyAttr).addClass(disabledAttr);
        $notifyManagerCheckbox.attr(disabledAttr, disabledAttr);
        $projectPrivacyCheckbox.attr(disabledAttr, disabledAttr);
        $followingProjectCheckbox.attr(disabledAttr, disabledAttr);
        jq("#TemplatesComboboxContainer").hide();
        jq("#projectTeamContainer .headerPanel").off();
    };

    function unlockProjectActionPageElements() {
        $projectTitle.removeAttr(readonlyAttr).removeClass(disabledAttr);
        $projectDescription.removeAttr(readonlyAttr).removeClass(disabledAttr);
        jq(".inputUserName").removeAttr(disabledAttr);
        $projectManagerSelector.find("td:last").css({ 'display': "table-cell" });
        $projectParticipantsContainer.find(itemsDisplayListClass).addClass("canedit");
        $projectStatus.removeAttr(disabledAttr).removeClass(disabledAttr);
        $projectTags.removeAttr(readonlyAttr).removeClass(disabledAttr);
        $notifyManagerCheckbox.removeAttr(disabledAttr);
        $projectPrivacyCheckbox.removeAttr(disabledAttr);
        $followingProjectCheckbox.removeAttr(disabledAttr);
        jq("#TemplatesComboboxContainer").show();
        loadingBanner.hideLoading();
    };

    // actions
    function addProject(project) {
        var params = {};
        if (isProjectsCreatedFromCRM()) {
            params.isProjectsCreatedFromCRM = true;
        }
        teamlab.addPrjProject(params, project, { success: onAddProject, error: unlockProjectActionPageElements });
    };

    function updateProject(projectId, project) {
        teamlab.updatePrjProject({}, projectId, project, { success: onUpdateProject, error: unlockProjectActionPageElements });
    };

    //handlers
    function onAddProject(params) {
        project = this.__responses[0];

        if (params.isProjectsCreatedFromCRM) {
            appendContacts(project);
        } else {
            checkFollowProj(project);
        }
    };

    function onUpdateProject(params, project) {
        window.onbeforeunload = null;
        document.location.replace("tasks.aspx?prjID=" + project.id);
    };

    function onDeleteProject() {
        window.onbeforeunload = null;
        document.location.replace("projects.aspx");
    };

    function onFollowingProject(params, project) {
        window.onbeforeunload = null;
        document.location.replace("projects.aspx?prjID=" + project.id);
    };

    function onGetTags(params, tags) {
        var titles = params.titles;
        $tagsAutocompleteContainer.empty();

        if (tags.length > 0) {
            for (var i = 0; i < tags.length; i++) {
                var container = document.createElement("div");

                jq(container).addClass("tagAutocompleteItem");
                jq(container).text(tags[i]);

                jq(container).on("mouseover", function() {
                    jq("div.tagAutocompleteItem").each(function() {
                        jq(this).removeClass(tagAutocompleteSelectedItemClass);
                    });
                    jq(this).addClass(tagAutocompleteSelectedItemClass);
                });

                jq(container).click(function () {
                    var str = "";
                    for (var j = 0; j < titles.length - 1; j++) {
                        str += jq.trim(titles[j]);
                        str += ", ";
                    }
                    $projectTags.val(str + jq(this).text() + ", ");
                    $tagsAutocompleteContainer.empty();
                });

                $tagsAutocompleteContainer.append(container);
                $projectTags.focus();
            }
        }
    };

    function onGetCRMOpportunity(params, opportunity) {
        $projectTitle.val(opportunity.title);
        $projectDescription.val(opportunity.description);
        $projectManagerSelector.useradvancedSelector("select", [opportunity.responsible.id]);
        $projectManagerSelector.html(Encoder.htmlDecode(opportunity.responsible.displayName)).attr("data-id", opportunity.responsible.id).removeClass("plus");
        if (opportunity.responsible.id == currentUserId) {
            $notifyManagerCheckbox.prop(checkedProp, false).attr(disabledAttr, disabledAttr);
            $followingProjectCheckbox.prop(checkedProp, false).attr(disabledAttr, disabledAttr);
        }
        if (opportunity.isPrivate) {
            $projectPrivacyCheckbox.prop(checkedProp, true);
            addUsersToCommand(opportunity.accessList);
        }
    };

    function onGetCRMContacts(params, contacts) {
        var contactsCount = contacts.length;
        if (contactsCount > 0) {
            ASC.CRM.ListContactView.CallbackMethods.render_simple_content({}, contacts);
        } else {
            jq("#projectContactsContainer .no-linked-contacts").show();
        }

    };

    function onGetCRMContact(params, contact) {
        onGetCRMContacts({}, [contact]);
    };
    
    function onAddCRMContacts (params) {
        checkFollowProj(params.project);
    };

    function onGetTemplates(params, data) {
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
        init: init
    };
})();
