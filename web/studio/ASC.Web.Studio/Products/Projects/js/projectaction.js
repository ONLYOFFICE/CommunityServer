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


ASC.Projects.ProjectAction = (function() {
    var projectId,
        activeTasksCount = 0,
        activeMilestonesCount = 0,
        opportunityId = undefined,
        contactId = undefined,
        teamIds = new Array(),
        currentUserId,
        projectResponsible,
        $projectManagerSelector,
        $projectTeamSelector,
        $projectStatusContainer,
        $projectStatus,
        $projectTag,
        $projectParticipants,
        $notifyManagerCheckbox,
        $followingProjectCheckbox,
        $notifyProjectCheckbox,
        $projectTitle,
        $projectDescription,
        $projectParticipantsContainer,
        $projectPrivacyCheckbox;

    var disabledAttr = "disabled",
        checkedProp = "checked",
        readonlyAttr = "readonly",
        requiredFieldErrorClass = "requiredFieldError",
        requiredInputErrorClass = "requiredInputError",
        itemsDisplayListClass = ".items-display-list";

    var teamlab, loadingBanner, project, team = [], tagsId = [], master;

    var init = function () {
        teamlab = Teamlab;
        loadingBanner = LoadingBanner;
        currentUserId = teamlab.profile.id;
        var param = jq.getURLParam("opportunityID");
        opportunityId = param ? param : undefined;
        param = jq.getURLParam("contactID");
        contactId = param ? param : undefined;
        projectId = jq.getURLParam("prjID");
        master = ASC.Projects.Master;

        if (!projectId) {
            teamlab.getPrjTemplates({}, { success: onGetTemplates });
        } else {
            teamlab.getPrjProject({}, projectId, { success: onGetProject });
        }
    };

    function initObjects() {
        if (project) {
            activeTasksCount = project.taskCount;
            activeMilestonesCount = project.milestoneCount;
        }

        $projectManagerSelector = jq("#projectManagerSelector"),
        $projectTeamSelector = jq("#projectTeamSelector");
        $projectStatusContainer = jq("#projectStatusContainer");
        $projectStatus = jq("#projectStatus");
        $projectTag = jq("#tags");
        $projectParticipants = jq("#projectParticipants");
        $notifyManagerCheckbox = jq("#notifyManagerCheckbox");
        $followingProjectCheckbox = jq("#followingProjectCheckbox");
        $notifyProjectCheckbox = jq("#notifyProjectCheckbox");
        $projectTitle = jq("[id$=projectTitle]");
        $projectDescription = jq("[id$=projectDescription]");
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


        var selectedProjectManager = $projectManagerSelector.attr("data-id");
        $projectManagerSelector.useradvancedSelector({
            itemsSelectedIds: selectedProjectManager.length ? [selectedProjectManager] : [],
            withGuests: false,
            showGroups: true,
            onechosen: true
        }).on("showList", onChooseProjectManager);

        $projectTeamSelector.useradvancedSelector({
            itemsDisabledIds: projectId ? [$projectManagerSelector.attr("data-id")] : [],
            itemsSelectedIds: projectId ? teamIds : [],
            showGroups: true
        }).on("showList", onChooseProjectTeam);

        var itemsSelectedIds = [];

        if (projectId) {
            var findTag = function (tagToFind) {
                return function (item) {
                    return tagToFind === Encoder.htmlDecode(item.title);
                };
            };

            for (var i = 0; i < project.tags.length; i++) {
                var tag = master.Tags.filter(findTag(project.tags[i])).map(function (item) { return item.value; });
                itemsSelectedIds = itemsSelectedIds.concat(tag);
            }
        }

        $projectTag.tagsadvancedSelector({
            canadd: true,
            itemsChoose: master.Tags.map(function (item) { return { id: item.value, title: item.title } }),
            itemsSelectedIds:itemsSelectedIds
        }).on("showList", function (event, items) {
            tagsId = items.map(function(item) {
                return item.id;
            });
            var titles = items.map(function (item) {
                return Encoder.htmlEncode(item.title);
            });
            var selectedTags = titles.length ? titles.reduce(function (previousValue, currentValue) {
                return previousValue + ", " + currentValue;
            }) : "";
            jq("#tagsContainer").html(selectedTags);
        });

        if (project) {
            projectResponsible = project.responsible.id;
            var statuses = ASC.Projects.Resources.ProjectStatus;

            $projectStatus.advancedSelector({
                itemsSelectedIds: project.status,
                onechosen: true,
                showSearch: false,
                itemsChoose: statuses,
                sortMethod: function () {
                    return 0;
                }
            }).on("showList", function (event, item) {
                $projectStatusContainer.find(".selected-before").removeClass("selected-before");
                $projectStatus.attr("data-id", item.id).text(item.title).attr("title", item.title);
                showCloseQuestion(item.id);
            });

            team = master.Team.filter(function (item) {
                return item.id !== projectResponsible;
            });
            if ($projectManagerSelector.attr("data-id") == currentUserId) {
                $projectManagerSelector.useradvancedSelector("selectBeforeShow", { id: currentUserId, title: ASC.Resources.Master.Resource.MeLabel });
            }
            displayProjectTeam();
        }
    }

    function initEvents() {
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

        // title
        $projectTitle.keyup(function () {
            if (jq.trim($projectTitle.val()) != "") {
                jq("#projectTitleContainer").removeClass(requiredFieldErrorClass);
            }
        });

        // popup and other button

        var oldTeam = jq.map(team, function (user) { return user.id; });
        jq("#projectActionButton").click(function () {
            if (!validateProjectData() || jq(this).hasClass("disable")) return;

            jq(this).addClass("disable");
            var project = getProjectData();

            lockProjectActionPageElements();

            if (projectId) {
                var needDelete = oldTeam.filter(function (userId) {
                    return jq.map(team, function (user) { return user.id; }).indexOf(userId) < 0;
                });
                needDelete.forEach(function (user) {
                    teamlab.removeCaldavProjectCalendar(projectId, user);
                });
                updateProject(projectId, project);
            } else {
                addProject(project);
            }
        });

        jq("#projectDeleteButton").click(function () {
            ASC.Projects.Base.showCommonPopup("projectRemoveWarning",
                function () {
                    lockProjectActionPageElements();
                    teamlab.removePrjProject(projectId, { success: onDeleteProject, error: onError });
                });
        });

        jq("#cancelEditProject").click(function () {
            if (jq(this).hasClass("disable")) return;
            window.onbeforeunload = null;
            document.location.replace(location.pathname.substring(0, location.pathname.lastIndexOf("/") + 1));
        });

        $projectParticipantsContainer.on("click", ".reset-action", function () {
            var $tr = jq(this).parents("tr.pm-projectTeam-participantContainer");
            var id = $tr.attr("data-partisipantid");
            deleteItemInArray(teamIds, id);
            team = team.filter(function (item) { return item.id !== id; });

            if (teamIds.length == 0) {
                $projectParticipantsContainer.hide();
            }
            
            $tr.remove();
            $projectTeamSelector.useradvancedSelector("undisable", [id]).useradvancedSelector("unselect", [id]);

            ASC.Projects.CreateProjectStructure.onResetAction(id);
        });

        $projectParticipantsContainer.on("click", ".right-checker", function () {
            var checker = jq(this);
            if (checker.closest("tr").hasClass("disable") || checker.hasClass("no-dotted")) {
                return;
            }

            var onClass = "pm-projectTeam-modulePermissionOn",
                offClass = "pm-projectTeam-modulePermissionOff";

            if (checker.hasClass(offClass)) {
                checker.removeClass(offClass).addClass(onClass);
            } else if (checker.hasClass(onClass)) {
                checker.removeClass(onClass).addClass(offClass);
            }

            var id = checker.parents("tr.pm-projectTeam-participantContainer").attr("data-partisipantid");
            team.find(function(item) { return item.id === id; })[checker.attr("data-flag")] = checker.hasClass(onClass);
        });


        if (!projectId && ASC.Projects.MilestoneContainer) {
            ASC.Projects.MilestoneContainer.init();
            jq(document).bind("chooseResp", function (event, data) {
                showNotifyProjectCheckbox();
            });
            jq(document).bind("unchooseResp", function (event, data) {
                hideNotifyProjectCheckbox();
            });
        }
        jq.confirmBeforeUnload(confirmBeforeUnloadCheck);

        $projectPrivacyCheckbox.on("change", function(e) {
            displayProjectTeam();
        });
    }

    function onChooseProjectTeam(e, items) {
        teamIds = [];

        var findInTeam = function(i) {
            return function(item) { return item.id === items[i].id; };
        };
        var newTeam = [];
        for (var i = 0, j = items.length; i < j; i++) {
            var exist = team.find(findInTeam(i));
            if (exist) {
                newTeam.push(exist);
            } else {
                newTeam.push(items[i]);
            }
        }

        team = newTeam;
        displayProjectTeam();
        return false;
    };

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
        projectResponsible = id;
        $projectParticipantsContainer.find("tr[data-partisipantid='" + id + "'] .reset-action").click();
        $projectTeamSelector.useradvancedSelector("disable", [id]);
    };

    function confirmBeforeUnloadCheck() {
        var project = getProjectData();
        return project.title.length ||
            project.responsibleid ||
            project.notify ||
            project.description.length ||
            tagsId.length ||
            (project.milestones && project.milestones.length) ||
            (project.tasks && project.tasks.length) ||
            (project.participants && project.participants.length);
    };

    function displayProjectTeam() {
        teamIds = team.map(function (item) { return item.id; });

        var projectManagerId = $projectManagerSelector.attr("data-id");

        if (team.length > 0) {
            var teamForTemplate = team.map(function(item) {
                return jq.extend({}, item, window.UserManager.getUser(item.id));
            });

            var data = jq.tmpl('member_template_without_photo',
            {
                team: teamForTemplate.map(mapTeamMember),
                project: {
                    canEditTeam: true,
                    isPrivate: $projectPrivacyCheckbox.is(":" + checkedProp)
                }
            });

            $projectParticipantsContainer.html(data);

            if (projectManagerId) {
                $projectParticipantsContainer.find("li[participantId=" + projectManagerId + "] .reset-action")
                    .addClass("hidden");
            }
            $projectParticipantsContainer.show();
            $projectTeamSelector.useradvancedSelector("select", teamIds);
            if (!project) {
                showNotifyProjectCheckbox();
            }
        } else {
            $projectParticipantsContainer.empty().hide();
        }
    };

    function showNotifyProjectCheckbox() {
        $notifyProjectCheckbox.show();
        $notifyProjectCheckbox.removeAttr(disabledAttr);
    }

    function hideNotifyProjectCheckbox() {
        $notifyProjectCheckbox.attr(disabledAttr, disabledAttr);
    }

    function mapTeamMember(item) {
        var resources = ASC.Projects.Resources;
        item.isManager = projectResponsible === item.id;
        item.isAdmin = item.isAdmin || (item.groups && item.groups.indexOf(master.ProjectsProductID) > -1);
        return jq.extend({
                security: [
                    security(item, "canReadMessages", resources.MessageResource.Messages),
                    security(item, "canReadFiles", resources.ProjectsFileResource.Documents),
                    security(item, "canReadTasks", resources.TasksResource.AllTasks),
                    security(item, "canReadMilestones", resources.MilestoneResource.Milestones),
                    security(item, "canReadContacts", resources.CommonResource.ModuleContacts, item.isVisitor)
                ]
            }, item);
    }

    function security(item, flag, title, defaultDisabled) {
        var result = { check: getPropertyOrDefault(item, flag, defaultDisabled), flag: flag, title: title };
        if (typeof defaultDisabled === "boolean") {
            result.defaultDisabled = defaultDisabled;
        }
        return result;
    }

    function clearFields() {
        $projectTitle.add($projectDescription).val("");
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
            responsibleid: projectResponsible,
            notify: $notifyManagerCheckbox.is(":" + checkedProp),
            description: $projectDescription.val(),
            'private': $projectPrivacyCheckbox.is(":" + checkedProp),
            templateProjectId: jq("#SelectedTemplateID").val()
        };

        var projStructure = ASC.Projects.CreateProjectStructure;
        if (projStructure) {
            project.milestones = projStructure.getProjMilestones();
            project.tasks = projStructure.getProjTasks();
        }

        if (projectId) {
            project.status = $projectStatus.attr("data-id");
        }
        if (team) {
            project.participants = team.map(function (item) {
                return {
                    ID: item.id,
                    CanReadMessages: getPropertyOrDefault(item, "canReadMessages"),
                    CanReadFiles: getPropertyOrDefault(item, "canReadFiles"),
                    CanReadTasks: getPropertyOrDefault(item, "canReadTasks"),
                    CanReadMilestones: getPropertyOrDefault(item, "canReadMilestones"),
                    CanReadContacts: getPropertyOrDefault(item, "canReadContacts")
                };
            });
            project.notifyResponsibles = $notifyProjectCheckbox.is(":" + checkedProp);
        }
        return project;
    };

    function getPropertyOrDefault(item, property, defaultDisabled) {
        return item.hasOwnProperty(property) ? item[property] : !defaultDisabled;
    }

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
        var isInTeam = getArrayIndex(currentUserId, teamIds) != -1;
        if (following && !isInTeam & !isManager) {
            teamlab.followingPrjProject({}, project.id, { projectId: project.id }, { success: onFollowingProject });
        } else {
            window.onbeforeunload = null;
            document.location.replace("Tasks.aspx?prjID=" + project.id + "#sortBy=sort_order&sortOrder=ascending");
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

    function showCloseQuestion(status) {
        if (status == 1) {
            var moduleLocationPath = StudioManager.getLocationPathToModule("Projects");
            if (activeTasksCount > 0) {
                ASC.Projects.Base.showCommonPopup("projectOpenTaskWarning", function () {
                    location.href = jq.format('{0}Tasks.aspx?prjID={1}#sortBy=deadline&sortOrder=ascending&status=open', moduleLocationPath, projectId);
                });
                $projectStatusContainer.addClass(requiredFieldErrorClass);
            }
            else if (activeMilestonesCount > 0) {
                ASC.Projects.Base.showCommonPopup("projectOpenMilestoneWarning", function () {
                    location.href = jq.format('{0}Milestones.aspx?prjID={1}#sortBy=deadline&sortOrder=ascending&status=open', moduleLocationPath, projectId);
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
        $notifyManagerCheckbox.removeAttr(disabledAttr);
        $projectPrivacyCheckbox.removeAttr(disabledAttr);
        $followingProjectCheckbox.removeAttr(disabledAttr);
        jq("#TemplatesComboboxContainer").show();
        loadingBanner.hideLoading();
    };

    function onError(params, error) {
        ASC.Projects.Common.displayInfoPanel(error, true);
        unlockProjectActionPageElements();
    }
    // actions
    function addProject(project) {
        var params = {};
        if (isProjectsCreatedFromCRM()) {
            params.isProjectsCreatedFromCRM = true;
        }
        teamlab.addPrjProject(params, project, { success: onAddProject, error: onError });
    };

    function updateProject(projectId, project) {
        teamlab.updatePrjProject({}, projectId, project, { success: onUpdateProject, error: onError });
    };

    //handlers
    function onAddProject(params) {
        project = this.__responses[0];

        teamlab.updatePrjProjectTags(project.id, { tags: tagsId }, {
            success: function() {
                if (params.isProjectsCreatedFromCRM) {
                    appendContacts(project);
                } else {
                    checkFollowProj(project);
                }
            }
        });
    };

    function onUpdateProject(params, project) {
        teamlab.updatePrjProjectTags(project.id, { tags: tagsId },
            {
                success: function() {
                    window.onbeforeunload = null;
                    document.location.replace("Tasks.aspx?prjID=" + project.id);
                }
            });
    };

    function onDeleteProject() {
        window.onbeforeunload = null;
        document.location.replace("Projects.aspx");
    };

    function onFollowingProject(params, project) {
        window.onbeforeunload = null;
        document.location.replace("Tasks.aspx?prjID=" + project.id + "#sortBy=sort_order&sortOrder=ascending");
    };

    function onGetCRMOpportunity(params, opportunity) {
        $projectTitle.val(opportunity.title);
        $projectDescription.val(opportunity.description);

        if (opportunity.responsible.id !== "4a515a15-d4d6-4b8e-828e-e0586f18f3a3") {
            $projectManagerSelector.useradvancedSelector("select", [opportunity.responsible.id]);
            $projectManagerSelector.html(Encoder.htmlDecode(opportunity.responsible.displayName)).attr("data-id", opportunity.responsible.id).removeClass("plus");
        }

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
        jq("#projectActionPage").html(jq.tmpl("projects_action_create",
        {
            IsProjectCreatedFromCrm: isProjectsCreatedFromCRM(),
            hideChooseTeam: Object.keys(window.UserManager.getAllUsers(true)).length === 1,
            showTemplates: data.length > 0,
            pageTitle: ASC.Projects.Resources.ProjectResource.CreateNewProject,
            IsPrivateDisabled: false,
            project: {
                title: "",
                description: "",
                manager: {
                    id: "",
                    name: ASC.Projects.Resources.ProjectResource.AddProjectManager
                },
                "private": true,
                canDelete: true,
                tags: []
            },
            actionButton: ASC.Projects.Resources.ProjectResource.AddNewProject
        }));
        initObjects();
        initEvents();

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
            ASC.Projects.CreateProjectStructure.onGetTemplate({}, item);
        });
        
        var tmplid = jq.getURLParam("tmplid");
        if (tmplid) {
            var tmpl = data.find(function (item) { return item.id == tmplid; });
            if (tmpl)
                templateSelect.projectadvancedSelector("selectBeforeShow", tmpl);
        }
    };

    function onGetProject(params, data) {
        project = data;
        tagsId = master.Tags
            .filter(function (item) { return data.tags.indexOf(item.title) > -1; })
            .map(function (item) { return item.value });

        jq("#projectActionPage").html(jq.tmpl("projects_action_create",
            {
                IsProjectCreatedFromCrm: isProjectsCreatedFromCRM(),
                hideChooseTeam: Object.keys(window.UserManager.getAllUsers(true)).length === 1,
                showTemplates: false,
                pageTitle: ASC.Projects.Resources.ProjectResource.EditProject,
                IsPrivateDisabled: false,
                project: {
                    title: data.title,
                    description: data.description,
                    manager: {
                        id: data.responsible.id,
                        name: data.responsible.displayName
                    },
                    status: ASC.Projects.Resources.ProjectStatus.find(function(item) {
                        return item.id === data.status;
                    }),
                    "private": data.isPrivate,
                    canDelete: data.canDelete,
                    url: "Tasks.aspx?prjID=" + data.id,
                    tags: data.tags.length ? data.tags.reduce(function (previousValue, currentValue) {
                        return previousValue + ", " + currentValue;
                    }) : ""
                },
                actionButton: ASC.Projects.Resources.ProjectResource.SaveProject
            }));
        initObjects();
        initEvents();
    }

    return {
        init: init
    };
})();
