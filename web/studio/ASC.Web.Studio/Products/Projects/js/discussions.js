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


ASC.Projects.Discussions = (function($) {
    var isInit = false, currentProjectId, filterDiscCount = 0,
        $discussionsList, self, teamlab;

    var baseObject = ASC.Projects,
        resources = baseObject.Resources,
        messageResource = resources.MessageResource,
        projectsJsResource = resources.ProjectsJSResource,
        filter = baseObject.ProjectsAdvansedFilter,
        currentProject,
        handler;

    var addCommentHash = "addcomment";
    var clickEventName = "click";

    //init
    var init = function () {
        currentProjectId = $.getURLParam('prjID');
        $discussionsList = $('#discussionsList');
        teamlab = Teamlab;

        if (isInit === false) {
            isInit = true;
            self = this;

            function canCreateDiscussion(prj) {
                return prj.canCreateMessage;
            }

            self.showOrHideData = self.showOrHideData.bind(self, {
                $container: $discussionsList,
                tmplName: "projects_discussionTemplate",
                baseEmptyScreen: {
                    img: "discussions",
                    header: messageResource.DiscussionNotFound_Header,
                    description: teamlab.profile.isVisitor ? messageResource.DiscussionNotFound_DescribeVisitor : messageResource.DiscussionNotFound_Describe,
                    button: {
                        title: messageResource.StartFirstDiscussion,
                        onclick: function () {
                            location.href = jq.format("Messages.aspx?action=add{0}", currentProjectId != null ? "&prjID=" + currentProjectId : "");
                        },
                        canCreate: function () {
                            return currentProjectId ?
                                canCreateDiscussion(currentProject) :
                                ASC.Projects.Master.Projects.some(canCreateDiscussion);
                        }
                    }
                },
                filterEmptyScreen: {
                    header: messageResource.FilterNoDiscussions,
                    description: messageResource.DescrEmptyListMilFilter
                }
            });
            self.getData = self.getData.bind(self, teamlab.getPrjDiscussions, onGetDiscussions);
        }

        self.baseInit({
            elementNotFoundError: projectsJsResource.DiscussionNotFound,
            moduleTitle: projectsJsResource.DiscussionsModule
        },
        {
            pagination: "discussionsKeyForPagination",
            small: true
        });

        handler = teamlab.bind(teamlab.events.getPrjProject, function (params, project) { currentProject = project; });

        filter.createAdvansedFilterForDiscussion(self);

        $discussionsList.on(clickEventName, ".title-list a", baseObject.Common.goToWithoutReload);

        $discussionsList.on(clickEventName, ".title-list .status", function () {
            filter.add('status', 'archived');
        });

        $discussionsList.on(clickEventName, ".name-list.project", function () {
            filter.add('project', $(this).attr('data-projectId'), ['tag']);
        });

        $discussionsList.on(clickEventName, ".name-list.author", function () {
            filter.addUser('author', $(this).attr('data-authorId'));
        });
    };

    function onGetDiscussions (params, discussions) {
        filterDiscCount = params.__total != undefined ? params.__total : 0;

        //$discussionsList.html(jq.tmpl("projects_discussionTemplate", templates));
        self.showOrHideData(discussions.map(getDiscussionTemplate), filterDiscCount);
        jq('.content-list p').filter(function () { return jq(this).html() === "&nbsp;"; }).remove();

        if (jq.cookies.get("is_retina")) {
            var discAuthors = discussions.map(function(item) {
                return item.createdBy.id;
            }).filter(function onlyUnique(value, index, self) { 
                return self.indexOf(value) === index;
            });

            for (var i = 0; i < discAuthors.length; i++) {
                teamlab.getUserPhoto({}, discAuthors[i],
                    {
                        success: onGetUserPhoto(discAuthors[i], discussions)
                    });
            }
        }
    };

    function onGetUserPhoto(author) {
        return function (params, data) {
            if (!data.max || data.max.indexOf("default") > -1) return;

            $discussionsList.find('.avatar-list img[src*="' + author + '"]').each(function () {
                $(this).attr("src", data.max);
            });
        }
    }

    function getDiscussionTemplate(discussion) {
        var discussionId = discussion.id;
        var prjId = discussion.projectId;
        var discussionUrl = "Messages.aspx?prjID=" + prjId + "&id=" + discussionId;

        var template =
        {
            createdDate: discussion.displayDateCrtdate,
            createdTime: discussion.displayTimeCrtdate,
            title: discussion.title,
            discussionUrl: discussionUrl,
            authorAvatar: discussion.createdBy.avatarBig || discussion.createdBy.avatar,
            authorId: discussion.createdBy.id,
            authorName: discussion.createdBy.displayName,
            authorPost: discussion.createdBy.title,
            status: discussion.status,
            projectId: prjId,
            text: discussion.text,
            hasPreview: discussion.text.search('class="asccut"') > 0,
            commentsCount: discussion.commentsCount,
            commentsUrl: discussionUrl + '#comments',
            writeCommentUrl: discussionUrl + '#' + addCommentHash,
            canComment: discussion.canCreateComment
        };
        if (!currentProjectId) {
            template.projectTitle = discussion.projectTitle;
            template.projectUrl = 'Projects.aspx?prjID=' + discussion.projectId;
        }
        return template;
    };

    var unbindListEvents = function () {
        if (!isInit) return;
        $discussionsList.unbind();
        teamlab.unbind(handler);
    };

    return jq.extend({
        init: init,
        unbindListEvents: unbindListEvents,
        basePath: 'sortBy=create_on&sortOrder=descending',
        addCommentHash: addCommentHash
    }, baseObject.Base);
})(jQuery);

ASC.Projects.DiscussionDetails = (function ($) {
    var projectFolderId, $fileContainer, projectTeam, currentUserId, isCommentEdit = false,
        marginTopClass = "marginTop", itemDisplayListClass = ".items-display-list_i", grayClass = "gray";

    var discussionId, projId, discussion, clickEventName = "click";
    var attachments, teamlab, loadingBanner, common, subscribers;
    var $commentsContainer,
        $discussionCommentsContainer,
        $manageParticipantsSelector,
        $discussionParticipantsContainer,
        $discussionParticipantsTable,
        resources = ASC.Projects.Resources;

    var overViewTab, subscribersTab, documentsTab, commentsTab;

    var init = function () {
        teamlab = Teamlab;
        common = ASC.Projects.Common;
        loadingBanner = LoadingBanner;

        discussionId = jq.getURLParam("id");
        projId = jq.getURLParam("prjID");
        currentUserId = teamlab.profile.id;
        $commentsContainer = $("#commonComments");
        $discussionCommentsContainer = $("#commonCommentsContainer");
        $mainCommentsContainer = $("#mainCommentsContainer");
        $fileContainer = $("#filesContainer");
        
        projectTeam = ASC.Projects.Master.Team;

        showLoading();
        ASC.Projects.Base.clearTables();
        jq("#filterContainer").hide();
        teamlab.getPrjDiscussion({}, discussionId,
        {
            success: onGetDiscussion,
            error: function (params, errors) {
                if (errors[0] === "Item not found") {
                    ASC.Projects.Base.setElementNotFound();
                }
            }
        });
    };
    
    function onGetDiscussion(params, response) {
        discussion = response;

        discussion.comments = hideRemovedComments(discussion.comments);

        initSubscribers();

        initAttachmentsControl();

        initCommentsControl();

        var hash = location.hash.substring(1);
        var isAddCommentHash = hash === ASC.Projects.Discussions.addCommentHash;

        var messageResource = resources.MessageResource;

        var statuses = [
            {
                title: messageResource.OpenDiscussion,
                id: 0,
                handler: daStatusHandler
            },
            {
                title: messageResource.ArchiveDiscussion,
                id: 1,
                handler: daStatusHandler
            }
        ];

        var currentStatusId = discussion.status;
        var currentStatus = statuses.find(function (item) { return item.id === currentStatusId });

        ASC.Projects.DescriptionTab
            .init()
            .push(resources.ProjectResource.Project, discussion.projectOwner.title, "Tasks.aspx?prjID=" + discussion.projectOwner.id)
            .push(messageResource.AuthorTitle, discussion.createdBy.displayName)
            .push(resources.ProjectsFilterResource.ByCreateDate, discussion.displayDateTimeCrtdate)
            .push(messageResource.Description, discussion.text)
            .setStatuses(statuses)
            .setCurrentStatus(currentStatus)
            .setStatusRight(discussion.canEdit)
            .tmpl();

        var isVisibleSelector = function() {
            return discussion.status === 0 || discussion.commentsCount;
        };

        var Tab = ASC.Projects.Tab;
        commentsTab = new Tab(resources.MessageResource.Comments,
            function() { return discussion.commentsCount; },
            "commentsModule",
            $discussionCommentsContainer,
            '#comments',
            isVisibleSelector,
            undefined,
            '#comment_');
        subscribersTab = new Tab(resources.MessageResource.DiscussionParticipants,
            function() { return subscribers.length; },
            "subscribeModule",
            $discussionParticipantsContainer,
            '#subscribers');
        documentsTab = new Tab(resources.CommonResource.DocsModuleTitle,
            function() { return discussion.files.length; },
            "documentsModule",
            $fileContainer,
            '#documents',
            function() { return discussion.canReadFiles && (discussion.canEditFiles || discussion.files.length) });
        overViewTab = new Tab(resources.ProjectsJSResource.OverviewModule,
            function() { return 0; },
            "overViewModule",
            jq(".tab"),
            '#');

        var data = {
            icon: "messages",
            title: discussion.title
        };

        var tabs = [];
        if (isVisibleSelector()) {
            tabs = [commentsTab, subscribersTab, documentsTab, overViewTab];
        } else {
            tabs = [overViewTab, subscribersTab, documentsTab];
        }

        ASC.Projects.InfoContainer.init(data, showEntityMenu, tabs);

        jq("#CommonListContainer").show();

        if (!discussion.canEdit) {
            $manageParticipantsSelector.hide();
        }

        jq("#descriptionTab").show();

        if (isAddCommentHash && isVisibleSelector()) {
            jq("#add_comment_btn").click();
        }

        hideLoading();
    }

    function hideRemovedComments(comments) {
        comments.forEach(function (item) {
            if (item.commentList.length) {
                item.commentList = hideRemovedComments(item.commentList);
            }
        });

        comments = comments.filter(function (item) {
            return !(item.inactive && item.commentList.length === 0);
        });

        return comments;
    }

    function initSubscribers() {
        subscribers = discussion.subscribers;
        subscribers.forEach(function (item) { item.descriptionFlag = true; });

        jq(".tab1").html(jq.tmpl("projects_discussionTab", getDiscussionParticipants(subscribers))).show();

        $discussionParticipantsContainer = jq("#discussionParticipantsContainer");
        $manageParticipantsSelector = $("#manageParticipantsSelector");
        $discussionParticipantsTable = $("#discussionParticipantsTable");
        
        if (discussion.status === 1) {
            $manageParticipantsSelector.hide();
        }

        var participantIds = subscribers.map(function(item) {return item.id});

        $manageParticipantsSelector.useradvancedSelector({
            showGroups: true,
            itemsSelectedIds: participantIds,
            itemsDisabledIds: [teamlab.profile.id]
        }).on("additionalClickEvent", function () {
            jq(".project-title .menu-small").removeClass("active");

        }).on("showList", function (event, users) {
            var participantsIds = [];

            for (var i = 0; i < users.length; i++) {
                participantsIds.push(users[i].id);
            }

            function findCurrentUserInSubscribers(item) {
                return item.id === teamlab.profile.id;
            }

            if (subscribers.filter(findCurrentUserInSubscribers).length) {
                participantsIds.push(teamlab.profile.id);
            }

            var data = {
                projectId: projId,
                participants: participantsIds.join(),
                notify: false
            };

            teamlab.updatePrjDiscussion({}, discussionId, data, {
                before: showLoading,
                success: onChangeDiscussionParticipants,
                error: onDiscussionError,
                after: hideLoading
            });
        });

        $manageParticipantsSelector.useradvancedSelector("select", participantIds);

        $discussionParticipantsTable.on("mouseenter", itemDisplayListClass + "." + grayClass, function () {
            jq(this).helper({
                BlockHelperID: "hintSubscribersPrivateProject",
                addLeft: 45,
                addTop: 12
            });
        });

        $discussionParticipantsTable.on("mouseleave", itemDisplayListClass + "." + grayClass, function () {
            jq("#hintSubscribersPrivateProject").hide();
        });
    };
    
    function initAttachmentsControl() {
        if (!$fileContainer.length || !window.Attachments) return;

        var marginBottomClass = "marginBottom", entityType = "message";

        attachments = Attachments;
        projectFolderId = parseInt(discussion.project.projectFolder);

        if (!discussion.canEditFiles) {
            attachments.banOnEditing();
        }

        ProjectDocumentsPopup.init(projectFolderId, attachments.isAddedFile, attachments.appendToListAttachFiles);
        attachments.isLoaded = false;
        attachments.init(entityType, function() { return discussion.id });
        attachments.setFolderId(projectFolderId);
        attachments.loadFiles(discussion.files);
        

        if (discussion.files.length > 0) {
            $fileContainer.css(marginBottomClass, "18px");
        }

        function addFileSuccess(file) {
            discussion.files.push(file);
            documentsTab.rewrite();
        }

        attachments.bind("addFile", function (ev, file) {
            if (file.attachFromPrjDocFlag || file.isNewFile) {
                teamlab.addPrjEntityFiles(null,
                    discussion.id,
                    entityType,
                    [file.id],
                    {
                        success: function() { addFileSuccess(file); },
                        error: onDiscussionError
                    });
            } else {
                addFileSuccess(file);
            }

            $fileContainer.css(marginBottomClass, "18px");
        });

        attachments.bind("deleteFile", function (ev, fileId) {
            teamlab.removePrjEntityFiles({}, discussion.id, entityType, fileId, { success: function() {
                attachments.deleteFileFromLayout(fileId);
                discussion.files = discussion.files.filter(function (item) { return item.id != fileId; });
                if (discussion.files.length === 0) {
                    $fileContainer.css(marginBottomClass, "0");
                }
                documentsTab.rewrite();
            }, error: onDiscussionError });

        });
    };

    function initCommentsControl() {
        CommentsManagerObj.isShowAddCommentBtn = discussion.status === 0 && discussion.canCreateComment;
        CommentsManagerObj.total = discussion.comments.length;
        CommentsManagerObj.isEmpty = discussion.comments.length === 0;
        CommentsManagerObj.moduleName = "projects_Message";
        CommentsManagerObj.comments = jq.base64.encode(JSON.stringify(discussion.comments));
        CommentsManagerObj.objectID = "common";
        CommentsManagerObj.Init();
        CommentsManagerObj.objectID = discussion.id;
        jq("#hdnObjectID").val(discussion.id);

        if (discussion.commentsCount > 0) {
            jq("#hideCommentsButton").show();
            $commentsContainer.css(marginTopClass, "15px");
        } else {
            $commentsContainer.css(marginTopClass, "-6px");
        }
        $commentsContainer.show();

        var hash = location.hash.substring(1);
        if (hash === ASC.Projects.Discussions.addCommentHash && CommentsManagerObj) {
            ckeditorConnector.load(CommentsManagerObj.AddNewComment);
        }

        bind(teamlab.events.addPrjComment, function () {
            if (!isCommentEdit) {
                jq("#hideCommentsButton").show();
                $commentsContainer.css(marginTopClass, "15px");
                discussion.commentsCount++;
                commentsTab.rewrite();
            } else {
                isCommentEdit = false;
            }
        });

        var commentsClick = clickEventName + ".comments";

        jq("#btnCancel").off(commentsClick).on(commentsClick, function () {
            isCommentEdit = false;
            commentsTab.select();
        });

        jq("a[id^=edit_]").off(commentsClick).on(commentsClick, function () {
            isCommentEdit = true;
        });
    }

    function showEntityMenu() {
        var menuItems = [];

        var ActionMenuItem = ASC.Projects.ActionMenuItem;
        if (discussion.canEdit) {
            menuItems.push(new ActionMenuItem("da_edit", resources.MessageResource.EditMessage, daEditHandler));
            menuItems.push(new ActionMenuItem("da_delete", resources.MessageResource.DeleteMessage, daDeleteHandler));
        }

        var project = common.getProjectById(discussion.projectId);
        if (project && project.canCreateTask) {
            menuItems.push(new ActionMenuItem("da_createTask", resources.MessageResource.CreateTaskOnDiscussion, daCreateTaskHandler));
        }

        var isSubscribed = subscribers.some(function (item) { return item.id === teamlab.profile.id; });
        menuItems.push(new ActionMenuItem("da_follow", isSubscribed ? resources.CommonResource.UnSubscribeOnNewComment : resources.CommonResource.SubscribeOnNewComment, daSubscribeHandler));

        return { menuItems: menuItems };
    }

    function daStatusHandler() {
        teamlab.updatePrjDiscussionStatus({}, discussionId, { status: discussion.status === 1 ? 0 : 1 }, {
            before: showLoading,
            success: onUpdateDiscussion,
            after: hideLoading
        });
    };

    function daEditHandler() {
        location.href = "Messages.aspx?prjID=" + discussion.projectId + "&id=" + discussion.id + "&action=edit";
    };

    function daCreateTaskHandler() {
        jq("body").click();
        showLoading();
        teamlab.addPrjTaskByMessage({}, projId, discussionId, {
            success: function (params, task) {
                window.onbeforeunload = null;
                location.href = "Tasks.aspx?prjID=" + projId + "&id=" + task.id;
            }
        });
    };

    function daDeleteHandler() {
        jq(".project-title .menu-small").removeClass("active");

        ASC.Projects.Base.showCommonPopup("discussionRemoveWarning",
            function () {
                teamlab.removePrjDiscussion({}, discussionId, {
                    success: onDeleteDiscussion,
                    error: ASC.Projects.DiscussionDetails.onDeleteDiscussionError
                });
            });
    };

    function daSubscribeHandler() {
        teamlab.subscribeToPrjDiscussion({}, discussionId, { success: onChangeDiscussionParticipants, error: onDiscussionError });
    }

    var onDeleteComment = function () {
        discussion.commentsCount--;
        commentsTab.rewrite();
        
        if (discussion.commentsCount === 0) {
            commentsTab.select();
        }
    };

    function onDiscussionError() {
        window.onbeforeunload = null;
        window.location.reload();
    };

    function onChangeDiscussionParticipants(params, response) {
        subscribers = response.subscribers;

        subscribers.forEach(function (item) { item.descriptionFlag = true; });

        $discussionParticipantsTable.html(jq.tmpl("projects_subscribedUsers", getDiscussionParticipants(subscribers)));

        var isSubscibed = subscribers.some(function (item) { return item.id === teamlab.profile.id; });

        if (!isSubscibed) {
            $manageParticipantsSelector.useradvancedSelector("disable", [currentUserId]);
        } else {
            $manageParticipantsSelector.useradvancedSelector("select", [currentUserId]);
        }
        subscribersTab.rewrite();
    };

    function getDiscussionParticipants(participants) {
        var newListParticipants = [], notSeePartisipant = [], teamLength = projectTeam.length;
        if (discussion.projectOwner.isPrivate) {
            for (var i = 0, pLength = participants.length; i < pLength; i++) {
                var participant = participants[i], addedFlag = false;

                for (var j = 0; j < teamLength; j++) {
                    var pt = projectTeam[j];
                    if ((participant.id === pt.id) && pt.canReadMessages) {
                        newListParticipants.push(participant);
                        addedFlag = true;
                    }
                }

                if (!addedFlag) {
                    participant.hidden = true;
                    notSeePartisipant.push(participant);
                }
                
            }
        } else {
            newListParticipants = participants;
        }

        return {
            newListParticipants: newListParticipants,
            notSeePartisipant: notSeePartisipant
        };
    };

    function onDeleteDiscussion(params, discussion) {
        teamlab.fckeRemoveCommentComplete(discussion.id.toString(), 'discussion', function () {
            window.onbeforeunload = null;
            window.location.replace("Messages.aspx?prjID=" + discussion.projectId);
        });
    };

    function onUpdateDiscussion() {
        location.reload();
    };

    function showLoading() {
        loadingBanner.displayLoading();
    }

    function hideLoading() {
        loadingBanner.hideLoading();
    }

    function unbindListEvents() {
        jq(".project-info-container").hide();
        jq("#descriptionTab").hide();
        attachments.unbind();
        unbind();
    }

    var handlers = [];
    function bind(event, handler) {
        handlers.push(teamlab.bind(event, handler));
    }

    function unbind() {
        while (handlers.length) {
            var handler = handlers.shift();
            teamlab.unbind(handler);
        }
    }

    return {
        init: init,
        onDeleteComment: onDeleteComment,
        unbindListEvents: unbindListEvents
    };
})(jQuery);

ASC.Projects.DiscussionAction = (function ($) {
    var projectId, id, loadListTeamFlag = false,projectFolderId, currentUserId, privateFlag, discussion;
    var projectTeam = [], discussionParticipants = [], filesToAttach = [], filesToDeattach = [];

    var $fileContainer, $discussionParticipantsContainer, $manageParticipantsSelector, $discussionParticipants,
        $discussionPreviewContainer, $discussionTitleContainer, $discussionTitleContainerInput, $discussionPreviewButton,
        $discussionProjectSelect, $discussionProjectContainer, $discussionTextContainer;

    var common, attachments, teamlab, loadingBanner;
    var resources = ASC.Projects.Resources;
    var action, entityType = "message";
    var itemsDisplayListClass = ".items-display-list_i", grayClass = "gray", requiredFieldErrorClass = 'requiredFieldError', 
        disableClass = "disable", disabledClass = "disabled", readonlyAttr = "readonly";

    var init = function () {
        teamlab = Teamlab;
        currentUserId = teamlab.profile.id;
        projectId = jq.getURLParam("prjID");
        id = jq.getURLParam("id");
        action = jq.getURLParam("action");
        common = ASC.Projects.Common;
        if (typeof Attachments !== "undefined") {
            attachments = Attachments;
        }
        loadingBanner = LoadingBanner;

        $fileContainer = $("#filesContainer");
        $discussionProjectSelect = $("#discussionProjectSelect");
        $discussionProjectContainer = $("#discussionProjectContainer");
        $discussionParticipantsContainer = $("#discussionParticipantsContainer");
        $manageParticipantsSelector = $("#manageParticipantsSelector");
        $discussionParticipants = $("#discussionParticipants");
        $discussionTitleContainer = $("#discussionTitleContainer");
        $discussionTextContainer = $("#discussionTextContainer");
        $discussionTitleContainerInput = $discussionTitleContainer.find('input');

        if (id) {
            teamlab.getPrjDiscussion({},
                id,
                {
                    success: onGetDiscussion,
                    error: function(params, errors) {
                        if (errors[0] === "Item not found") {
                            ASC.Projects.Base.setElementNotFound();
                        }
                    }
                });
        } else {
            initButtons();
            if (projectId) {
                projectFolderId = ASC.Projects.Master.projectFolder;
                initAttachmentsControl();
            }
        }

        jq('[id$=discussionTitle]').focus();

        var participantIds = [];
        $discussionParticipantsContainer.find(itemsDisplayListClass).each(function () {
            participantIds.push(jq(this).attr('guid'));
        });

        $manageParticipantsSelector.useradvancedSelector({
            showGroups: true,
            itemsSelectedIds: participantIds

        }).on("showList", function (event, participants) {
            $discussionParticipantsContainer.find(itemsDisplayListClass).remove();

            var partisipantsTmpls = participants.map(function (item) {
                var departemts = [];
                item.groups.forEach(function (el) { departemts.push(el.name) });
                return { id: item.id, displayName: item.title, department: departemts.join(), descriptionFlag: false, profileUrl: item.profileUrl };
            });
            showDiscussionParticipants(partisipantsTmpls);
        });

        if (projectId) {
            loadListTeamFlag = true;
            if (action !== "edit") {
                getTeam({}, projectId);
            } else {
                projectTeam = ASC.Projects.Master.Team;
            }
            privateFlag = ASC.Projects.Master.Projects.some(function (item) { return item.id == projectId && item.isPrivate; });
            jq(".mainPageContent").children(".loader-page").hide();
        } else {
            initProjectsCombobox();
        }

        var resetActionClass = ".reset-action";
        $discussionParticipantsContainer.on('click', itemsDisplayListClass + " " + resetActionClass, function () {
            var $li = jq(this).closest('li');
            $li.remove();
            $manageParticipantsSelector.useradvancedSelector("unselect", [$li.attr('guid')]);
        });

        jq('#hideDiscussionPreviewButton').click(function () {
            $discussionPreviewContainer.hide();
        });

        $discussionTitleContainer.find('input').keyup(function () {
            if (jq.trim(jq(this).val()) !== '') {
                $discussionTitleContainer.removeClass(requiredFieldErrorClass);
            }
        });

        jq.switcherAction("#switcherParticipantsButton", $discussionParticipantsContainer);

        $discussionParticipants.on("mouseenter", itemsDisplayListClass + "." + grayClass, function () {
            jq(this).helper({
                BlockHelperID: "hintSubscribersPrivateProject",
                addLeft: 45,
                addTop: 12
            });
        });
        $discussionParticipants.on("mouseleave", itemsDisplayListClass + "." + grayClass, function () {
            jq("#hintSubscribersPrivateProject").hide();
        });
        jq.confirmBeforeUnload(confirmBeforeUnloadCheck);
    };

    function confirmBeforeUnloadCheck() {
        return ($discussionProjectSelect.length && $discussionProjectSelect.attr("data-id").length) ||
            jq("[id$=discussionTitle]").val().length ||
            common.ckEditor.getData().length ||
            discussionParticipants.length;
    };

    function onGetDiscussion(params, response) {
        discussion = response;

        onGetPrjSubscribers(null, discussion.subscribers);

        projectFolderId = discussion.project.projectFolder;

        initAttachmentsControl();

        initButtons();
    };

    function onGetPrjSubscribers(params, subscribers) {
        subscribers.forEach(function (item) { item.descriptionFlag = false; });
        $manageParticipantsSelector.useradvancedSelector("select", subscribers.map(function (item) { return item.id; }));
        showDiscussionParticipants(subscribers);
    };

    function initButtons() {
        var data = {
            action: discussion ? resources.CommonResource.SaveChanges : resources.MessageResource.AddDiscussion,
            disable: discussion ? "" : "disable"
        };

        jq.tmpl("discussion_buttons", data).appendTo(jq(".mainPageContent"));

        $discussionPreviewButton = $("#discussionPreviewButton");
        $discussionPreviewContainer = $("#discussionPreviewContainer");
        $discussionPreviewButton.click(function () {
            if ($discussionPreviewButton.hasClass(disableClass)) return;

            var takeThis = this;
            var profile = teamlab.profile;

            teamlab.getPrjDiscussionPreview({ takeThis: takeThis }, common.ckEditor.getData(),
                {
                    before: function () { loader(true); },
                    after: function () { loader(false); },
                    success: function (params, response) {
                        var discussion =
                                {
                                    title: $discussionTitleContainerInput.val(),
                                    authorName: profile.displayName,
                                    authorTitle: profile.title,
                                    authorPageUrl: profile.profileUrl,
                                    authorAvatarUrl: profile.avatarBig,
                                    createOn: formatDate(new Date()),
                                    content: response
                                };

                        $discussionPreviewContainer.find('.discussionContainer').remove();
                        jq.tmpl("projects_discussionActionTemplate", discussion).prependTo($discussionPreviewContainer);
                        $discussionPreviewContainer.show();
                    }
                });
        });

        var $discussionCancelButton = $('#discussionCancelButton');
        $discussionCancelButton.click(function () {
            if ($discussionCancelButton.hasClass(disableClass)) return;
            window.onbeforeunload = null;
            document.location.replace(location.pathname.substring(0, location.pathname.lastIndexOf("/") + 1));
        });

        var $discussionActionButton = $('#discussionActionButton');
        $discussionActionButton.click(function () {
            if ($discussionActionButton.hasClass(disableClass)) return;
            $discussionProjectContainer.removeClass(requiredFieldErrorClass);
            $discussionTitleContainer.removeClass(requiredFieldErrorClass);
            $discussionTextContainer.removeClass(requiredFieldErrorClass);

            var projectid = projectId ? projectId : $discussionProjectSelect.attr("data-id");
            var title = jq.trim($discussionTitleContainerInput.val());
            var content = common.ckEditor.getData();

            var isError = false;
            if (!projectid) {
                $discussionProjectContainer.addClass(requiredFieldErrorClass);
                isError = true;
            }

            if (title === '') {
                $discussionTitleContainer.addClass(requiredFieldErrorClass);
                isError = true;
            }

            if (!jq.trim(content).length) {
                $discussionTextContainer.addClass(requiredFieldErrorClass);
                isError = true;
            }

            if (isError) {
                var scroll = jq('#pageHeader').offset().top;
                jq('body, html').animate({
                    scrollTop: scroll
                }, 500);
                return;
            }

            var newDiscussion =
                {
                    projectid: projectid,
                    title: title,
                    content: content
                };

            if (discussion) {
                newDiscussion.messageid = discussion.id;
            }

            var participants = [];
            $discussionParticipantsContainer.find(itemsDisplayListClass).each(function () {
                participants.push(jq(this).attr('guid'));
            });
            newDiscussion.participants = participants.join();

            lockDiscussionActionPageElements();
            if (newDiscussion.messageid) {
                updateDiscussion(newDiscussion);
            }
            else {
                addDiscussion(newDiscussion);
            }
        });
    }

    function discussionProjectChange(item) {
        $discussionProjectContainer.removeClass(requiredFieldErrorClass);
        $discussionParticipantsContainer.find(itemsDisplayListClass).remove();
        privateFlag = item.isPrivate;

        jq('#errorAllParticipantsProject').hide();

        getTeam({}, item.id);
        $discussionTitleContainerInput.focus();
    };

    function initProjectsCombobox() {
        var allprojects = ASC.Projects.Master.Projects.filter(function(prj) {
            return prj.canCreateMessage;
        });

        $discussionProjectSelect.projectadvancedSelector(
            {
                itemsChoose: allprojects,  
                onechosen: true
            }
        );
        
        $discussionProjectSelect.on("showList", function (event, item) {
            $discussionProjectSelect.attr("data-id", item.id).text(item.title).attr("title", item.title);
            discussionProjectChange(item);
        });
    };

    function addDiscussion(discussion) {
        teamlab.addPrjDiscussion({}, discussion.projectid, discussion, { success: onAddDiscussion, error: onError });
    };

    function onAddDiscussion(params, discussion) {
        attachFiles(discussion, false);
    };

    function updateDiscussion(discussion) {
        teamlab.updatePrjDiscussion({}, discussion.messageid, discussion, { success: onUpdateDiscussion, error: onError });
    };

    function onUpdateDiscussion(params, discussion) {
        attachFiles(discussion, true);
    };

    function onError() {
        if (this.__errors[0] === "Access denied.") {
            window.onbeforeunload = null;
            window.location.replace("Messages.aspx");
        }
        unlockDiscussionActionPageElements();
    };

    function lockDiscussionActionPageElements() {
        $discussionProjectSelect.attr(disabledClass, disabledClass).addClass(disabledClass);
        $discussionTitleContainerInput.attr(readonlyAttr, readonlyAttr).addClass(disabledClass);
        jq('iframe[id^=ctl00_ctl00]').contents().find('iframe').contents().find('#fckbodycontent').attr(readonlyAttr, readonlyAttr).addClass(disabledClass);
        loader(true);
    };

    function unlockDiscussionActionPageElements() {
        $discussionProjectSelect.removeAttr(disabledClass).removeClass(disabledClass);
        $discussionTitleContainerInput.removeAttr(readonlyAttr).removeClass(disabledClass);
        jq('iframe[id^=ctl00_ctl00]').contents().find('iframe').contents().find('#fckbodycontent').removeAttr(readonlyAttr).removeClass(disabledClass);
        loader(false);
    };

    function showDiscussionParticipants(participants) {
        discussionParticipants = participants;
        var newListParticipants = [];
        var notSeePartisipant = [];
        if (privateFlag) {
            for (var i = 0; i < participants.length; i++) {
                var addedFlag = false;
                for (var j = 0; j < projectTeam.length; j++) {
                    if ((participants[i].id === projectTeam[j].id) && projectTeam[j].canReadMessages) {
                        newListParticipants.push(participants[i]);
                        addedFlag = true;
                    }
                }
                if (!addedFlag) {
                    participants[i].hidden = true;
                    notSeePartisipant.push(participants[i]);
                }
            }
        } else {
            newListParticipants = participants;
        }
        var tmplName = "projects_subscribedUser";
        jq.tmpl(tmplName, newListParticipants).appendTo($discussionParticipants);

        if (notSeePartisipant.length) {
            jq.tmpl(tmplName, notSeePartisipant).addClass(grayClass).appendTo($discussionParticipants);
        }
    };

    function initAttachmentsControl() {
        ProjectDocumentsPopup.init(projectFolderId, attachments.isAddedFile, attachments.appendToListAttachFiles);

        attachments.isLoaded = false;
        attachments.init(entityType, function () { return discussion ? discussion.id : 0 });
        attachments.setFolderId(projectFolderId, true);
        attachments.loadFiles(discussion ? discussion.files : []);

        attachments.bind("addFile", function (ev, file) {
            if (discussion && discussion.files.find(function (item) { return item.id === file.id })) return;

            filesToAttach.push(file);
        });
        attachments.bind("deleteFile", function (ev, fileId) {
            filesToAttach = filesToAttach.filter(function (item) { return item.id !== fileId; });

            if (discussion && discussion.files) {
                discussion.files.find(function (item) {
                    var finded = item.id === fileId;
                    if (finded) {
                        filesToDeattach.push(fileId);
                    }
                    return !finded;
                });
            }
            attachments.deleteFileFromLayout(fileId);
        });

        var switcherId = "filesButton";

        var fileContainerTitle = {
            id: switcherId,
            title: resources.CommonResource.DocsModuleTitle,
            state: 0
        };

        jq.tmpl("projects_tabs", fileContainerTitle).insertBefore($fileContainer);
        jq.switcherAction("#switcher" + switcherId, $fileContainer);

        jq("#CommonListContainer, #descriptionTab, #filesContainer").show();
    };

    function attachFiles(discussion, isEdit) {
        var onComplete = function() {
            window.onbeforeunload = null;
            window.location.replace('Messages.aspx?prjID=' + discussion.projectId + '&id=' + discussion.id);
        };

        if (filesToDeattach.length || filesToAttach.length) {
            var onCompleteObj = function(cb) {
                return {
                    success: cb,
                    error: cb
                };
            }

            var asyncMethods = [];

            if (filesToAttach.length){
                asyncMethods.push(function(cb) {
                    teamlab.addPrjEntityFiles(null, discussion.id, entityType, filesToAttach.map(function(item) { return item.id; }), onCompleteObj(cb));
                });
            }

            if (filesToDeattach.length) {
                asyncMethods.push(function(cb) {
                    teamlab.removePrjEntityFiles(null, discussion.id, entityType, filesToDeattach, onCompleteObj(cb));
                });
            }

            asyncMethods.push(function(cb) {
                teamlab.fckeEditCommentComplete({},
                {
                    commentid: discussion.id.toString(),
                    domain: 'discussion',
                    html: discussion.text,
                    isedit: isEdit
                },
                onCompleteObj(cb));
            });

            async.parallel(asyncMethods,onComplete);
        } else {
            teamlab.fckeEditCommentComplete({},{ commentid: discussion.id.toString(), domain: 'discussion', html: discussion.text, isedit: false }, onComplete);
        }
    };

    function getTeam(params, projId) {
        if (loadListTeamFlag) {
            onGetTeam({}, ASC.Projects.Master.Team);
        } else {
            teamlab.getPrjTeam(params, projId, {
                before: loadingBanner.displayLoading,
                success: onGetTeam,
                after: loadingBanner.hideLoading
            });
        }
    };

    function onGetTeam(params, team) {
        projectTeam = team;
        var count = team ? team.length : 0;
        var newParticipants = [];
        if (count <= 0) return;
        
        var existParticipants = $discussionParticipantsContainer.find(itemsDisplayListClass).length ? jq('#discussionParticipantsContainer ' + itemsDisplayListClass) : [];

        for (var i = 0; i < count; i++) {
            var existFlag = existParticipants.some(function(item) { return item.attr('guid') === team[i].id; });
            
            if (!existFlag) {
                team[i].descriptionFlag = team[i].id === teamlab.profile.id;
                newParticipants.push(team[i]);
            }
        }
        
        onGetPrjSubscribers(null, newParticipants);
        
    };

    function formatDate(date) {
        var zeroString = '0';
        var dateArray =
            [zeroString + date.getDate(), zeroString + (date.getMonth() + 1), zeroString + date.getFullYear(), zeroString + date.getHours(), zeroString + date.getMinutes()];
        for (var i = 0; i < dateArray.length; i++) {
            dateArray[i] = dateArray[i].slice(-2);
        }
        return dateArray[0] + '.' + dateArray[1] + '.' + dateArray[2] + ' ' + dateArray[3] + ':' + dateArray[4];
    };

    var showHidePreview = function() {
        if (this.getData() === "") {
            $discussionPreviewButton.addClass(disableClass);
        } else {
            $discussionPreviewButton.removeClass(disableClass);
        }
    };

    function loader(show) {
        var containerID = "#discussionActionPage";
        show ? loadingBanner.showLoaderBtn(containerID) : loadingBanner.hideLoaderBtn(containerID);
    }

    return {
        init: init,
        showHidePreview: showHidePreview
    };
})(jQuery);