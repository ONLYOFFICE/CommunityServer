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
                return prj.canCreateMessage && prj.status === 0;
            }

            self.showOrHideData = self.showOrHideData.bind(self, {
                $container: $discussionsList,
                tmplName: "projects_discussionTemplate",
                baseEmptyScreen: {
                    img: "discussions",
                    header: messageResource.DiscussionNotFound_Header,
                    description: messageResource.DiscussionNotFound_Describe,
                    button: {
                        title: messageResource.StartFirstDiscussion,
                        onclick: function () {
                            location.href = jq.format("messages.aspx?action=add{0}", currentProjectId != null ? "&prjID=" + currentProjectId : "");
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

        $discussionsList.on(clickEventName, ".title-list a", function () {
            var $self = jq(this);
            var href = $self.attr("href");
            history.pushState({ href: href }, { href: href }, href);
            ASC.Controls.AnchorController.historyCheck();

            var prjid = jq.getURLParam("prjID");
            Teamlab.getPrjTeam({}, prjid, function (params, team) {
                ASC.Projects.Master.Team = team;
                ASC.Projects.Base.clearTables();
                jq("#filterContainer").hide();
                ASC.Projects.Common.baseInit();
            });


            return false;
        });

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
        jq('.content-list p').filter(function (index) { return jq(this).html() == "&nbsp;"; }).remove();
    };

    function getDiscussionTemplate(discussion) {
        var discussionId = discussion.id;
        var prjId = discussion.projectId;
        var discussionUrl = "messages.aspx?prjID=" + prjId + "&id=" + discussionId;

        var template =
        {
            createdDate: discussion.displayDateCrtdate,
            createdTime: discussion.displayTimeCrtdate,
            title: discussion.title,
            discussionUrl: discussionUrl,
            authorAvatar: discussion.createdBy.avatar,
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
            template.projectUrl = 'projects.aspx?prjID=' + discussion.projectId;
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
    var projectFolderId, projectName, $fileContainer, projectTeam, currentUserId, isCommentEdit = false,
        marginTopClass = "marginTop", itemDisplayListClass = ".items-display-list_i", grayClass = "gray",
        subscribedAttrClass = "subscribed", unsubscribedAttrClass = "unsubscribed";

    var discussionId, projId, discussion, clickEventName = "click";
    var attachments, teamlab, loadingBanner, common, subscribers;
    var $subscribeButton,
        $commentsContainer,
        $discussionCommentsContainer,
        $mainCommentsContainer,
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

        var hash = ASC.Controls.AnchorController.getAnchor();
        var isAddCommentHash = hash === ASC.Projects.Discussions.addCommentHash;

        var messageResource = resources.MessageResource;

        ASC.Projects.DescriptionTab
            .init()
            .push(resources.ProjectResource.Project, discussion.projectOwner.title, "tasks.aspx?prjID=" + discussion.projectOwner.id)
            .push(messageResource.AuthorTitle, discussion.createdBy.displayName)
            .push(resources.ProjectsFilterResource.ByCreateDate, discussion.displayDateTimeCrtdate)
            .push(messageResource.Description, discussion.text)
            .pushStatus(messageResource.OpenDiscussion, 0, daStatusHandler)
            .pushStatus(messageResource.ArchiveDiscussion, 1, daStatusHandler)
            .setCurrentStatus(discussion.status)
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
            isVisibleSelector(),
            isVisibleSelector);
        subscribersTab = new Tab(resources.MessageResource.DiscussionParticipants,
            function() { return subscribers.length; },
            "subscribeModule",
            $discussionParticipantsContainer);
        documentsTab = new Tab(resources.CommonResource.DocsModuleTitle,
            function() { return discussion.files.length; },
            "documentsModule",
            $fileContainer,
            false,
            function() { return discussion.canReadFiles && (discussion.canEditFiles || discussion.files.length) });
        overViewTab = new Tab(resources.ProjectsJSResource.OverviewModule,
            function() { return 0; },
            "overViewModule",
            jq(".tab"),
            !isVisibleSelector());

        var isSubscibed = discussion.subscribers.some(function (item) { return item.id === teamlab.profile.id; });

        var data = {
            uplink: "messages.aspx?prjID=" + discussion.projectId,
            icon: "messages",
            title: discussion.title,
            subscribed: isSubscibed,
            subscribedTitle: isSubscibed ? resources.CommonResource.UnSubscribeOnNewComment : resources.CommonResource.SubscribeOnNewComment
        };

        var tabs = [];
        if (isVisibleSelector()) {
            tabs = [commentsTab, subscribersTab, documentsTab, overViewTab];
        } else {
            tabs = [overViewTab, subscribersTab, documentsTab];
        }

        ASC.Projects.InfoContainer.init(data, showEntityMenu, tabs);

        $subscribeButton = $('#subscribe');

        $subscribeButton.on(clickEventName, function () {
            teamlab.subscribeToPrjDiscussion({}, discussionId, { success: onChangeDiscussionParticipants, error: onDiscussionError });
        });

        jq("#CommonListContainer").show();

        if (!discussion.canEdit) {
            $manageParticipantsSelector.hide();
        }

        jq("#descriptionTab").show();

        if (isAddCommentHash) {
            jq("#add_comment_btn").click();
            location.hash = "";
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

        $manageParticipantsSelector.useradvancedSelector("select", participantIds);

        $manageParticipantsSelector.useradvancedSelector({
            showGroups: true,
            itemsSelectedIds: participantIds

        }).on("additionalClickEvent", function () {
            jq(".project-title .menu-small").removeClass("active");

        }).on("showList", function (event, users) {
            var participants = [];
            var participantsIds = [];

            for (var i = 0; i < users.length; i++) {
                var userId = users[i].id,
                    userName = users[i].title;

                participants.push({ id: userId, displayName: userName, descriptionFlag: true });
                participantsIds.push(userId);
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
        projectName = Encoder.htmlEncode(discussion.projectTitle);

        if (!discussion.canEditFiles) {
            attachments.banOnEditing();
        }

        ProjectDocumentsPopup.init(projectFolderId, projectName);
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
            ProjectDocumentsPopup.reset();
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
                discussion.files = discussion.files.filter(function (item) { return item.id !== fileId });
                if (discussion.files.length === 0) {
                    $fileContainer.css(marginBottomClass, "0px");
                }
                documentsTab.rewrite();
            }, error: onDiscussionError });

        });

        bind(teamlab.events.getDocFolder,
            function() {
                var $filesLists = jq(".fileList li");
                for (var i = 0, j = discussion.files.length; i < j; i++) {
                    $filesLists.find("input#" + discussion.files[i].id).prop("checked", true);
                }
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

        var hash = ASC.Controls.AnchorController.getAnchor();
        if (hash == ASC.Projects.Discussions.addCommentHash && CommentsManagerObj) {
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
        location.href = "messages.aspx?prjID=" + discussion.projectId + "&id=" + discussion.id + "&action=edit";
    };

    function daCreateTaskHandler() {
        jq("body").click();
        showLoading();
        teamlab.addPrjTaskByMessage({}, projId, discussionId, {
            success: function (params, task) {
                window.onbeforeunload = null;
                location.href = "tasks.aspx?prjID=" + projId + "&id=" + task.id;
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

    var onDeleteComment = function () {
        discussion.commentsCount--;
        commentsTab.rewrite();
        
        if (discussion.commentsCount == 0) {
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

        var isSubscibed = subscribers.some(function (item) { return item.id == teamlab.profile.id; });

        if (!isSubscibed) {
            $subscribeButton.removeClass(subscribedAttrClass).addClass(unsubscribedAttrClass);
            $manageParticipantsSelector.useradvancedSelector("disable", [currentUserId]);
        } else {
            $subscribeButton.removeClass(unsubscribedAttrClass).addClass(subscribedAttrClass);
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
                    if ((participant.id == pt.id) && pt.canReadMessages) {
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
            window.location.replace("messages.aspx?prjID=" + discussion.projectId);
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
    var projectId, id, loadListTeamFlag = false,projectFolderId, projectName, currentUserId, privateFlag;
    var newFilesToAttach = [], filesFromProject = [], projectTeam = [], discussionParticipants = [];
    var $fileContainer, $discussionParticipantsContainer, $manageParticipantsSelector, $discussionParticipants,
        $discussionPreviewContainer, $discussionTitleContainer, $discussionTitleContainerInput, $discussionPreviewButton,
        $discussionProjectSelect, $discussionProjectContainer, $discussionTextContainer;
    var common, attachments, teamlab, loadingBanner;
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

        $fileContainer = $("#discussionFilesContainer");
        $discussionProjectSelect = $("#discussionProjectSelect");
        $discussionProjectContainer = $("#discussionProjectContainer");
        $discussionPreviewButton = $("#discussionPreviewButton");
        $discussionParticipantsContainer = $("#discussionParticipantsContainer");
        $manageParticipantsSelector = $("#manageParticipantsSelector");
        $discussionParticipants = $("#discussionParticipants");
        $discussionPreviewContainer = $("#discussionPreviewContainer");
        $discussionTitleContainer = $("#discussionTitleContainer");
        $discussionTextContainer = $("#discussionTextContainer");
        $discussionTitleContainerInput = $discussionTitleContainer.find('input');

        if (id)
            teamlab.getSubscribesToPrjDiscussion({}, id, { success: onGetPrjSubscribers });

        if ($fileContainer.length)
            initAttachmentsControl();

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
            if (action != "edit") {
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
            var userId = jq(this).closest('li').attr('guid');
            if (userId != currentUserId) {
                jq(this).closest('li').remove();
                $manageParticipantsSelector.useradvancedSelector("unselect", [userId]);
            }
        });

        $discussionParticipantsContainer.find(itemsDisplayListClass).each(function () {
            var userId = jq(this).attr('guid');
            if (userId == currentUserId) {
                jq(this).find(resetActionClass).remove();
            }
        });

        jq('#hideDiscussionPreviewButton').click(function () {
            $discussionPreviewContainer.hide();
        });

        $discussionTitleContainer.find('input').keyup(function () {
            if (jq.trim(jq(this).val()) != '') {
                $discussionTitleContainer.removeClass(requiredFieldErrorClass);
            }
        });

        $discussionPreviewButton.click(function () {
            if ($discussionPreviewButton.hasClass(disableClass)) return;

            var takeThis = this;

            teamlab.getPrjDiscussionPreview({ takeThis: takeThis }, common.ckEditor.getData(),
                {
                    before: function () { loader(true); },
                    after: function () { loader(false); },
                    success: function (params, response) {
                        var $this = jq(params.takeThis),
                            discussion =
                                {
                                    title: $discussionTitleContainerInput.val(),
                                    authorName: $this.attr('authorName'),
                                    authorTitle: $this.attr('authorTitle'),
                                    authorPageUrl: $this.attr('authorPageUrl'),
                                    authorAvatarUrl: $this.attr('authorAvatarUrl'),
                                    createOn: formatDate(new Date()),
                                    content: response
                                };

                        $discussionPreviewContainer.find('.discussionContainer').remove();
                        jq.tmpl("projects_discussionActionTemplate", discussion).prependTo($discussionPreviewContainer);
                        $discussionPreviewContainer.show();
                    },
                });
        });

        var $discussionCancelButton = $('#discussionCancelButton');
        $discussionCancelButton.click(function () {
            if ($discussionCancelButton.hasClass(disableClass)) return;
            var projectId = jq.getURLParam('prjID');
            window.onbeforeunload = null;
            if (!!window.history) {
                window.history.go(-1);
            } else {
                window.location.replace('messages.aspx?prjID=' + projectId);
            }
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

            if (title == '') {
                $discussionTitleContainer.addClass(requiredFieldErrorClass);
                isError = true;
            }

            var tmp = document.createElement("DIV");
            tmp.innerHTML = content;

            if (tmp.textContent == "" || tmp.innerText == "") {
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

            var discussion =
                {
                    projectid: projectid,
                    title: title,
                    content: content
                };

            var discussionId = $discussionActionButton.attr('discussionId');
            if (discussionId != -1) {
                discussion.messageid = discussionId;
            }

            var participants = [];
            $discussionParticipantsContainer.find(itemsDisplayListClass).each(function () {
                participants.push(jq(this).attr('guid'));
            });
            discussion.participants = participants.join();

            lockDiscussionActionPageElements();
            if (discussionId == -1) {
                addDiscussion(discussion);
            }
            else {
                updateDiscussion(discussion);
            }
        });

        jq.switcherAction("#switcherParticipantsButton", $discussionParticipantsContainer);
        jq.switcherAction("#switcherFilesButton", $fileContainer);

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

    function onGetPrjSubscribers(params, subscribers) {
        subscribers.forEach(function (item) { item.descriptionFlag = false; });
        $manageParticipantsSelector.useradvancedSelector("select", subscribers.map(function (item) { return item.id; }));
        showDiscussionParticipants(subscribers);
    };

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
        attachFiles(discussion);
    };

    function onError() {
        if (this.__errors[0] == "Access denied.") {
            window.onbeforeunload = null;
            window.location.replace("messages.aspx");
        }
        unlockDiscussionActionPageElements();
    };

    function updateDiscussion(discussion) {
        teamlab.updatePrjDiscussion({}, discussion.messageid, discussion, { success: onUpdateDiscussion, error: onError });
    };

    function onUpdateDiscussion(params, discussion) {
        teamlab.fckeEditCommentComplete({},
            { commentid: discussion.id.toString(), domain: 'discussion', html: discussion.text, isedit: true },
            function () {
                window.onbeforeunload = null;
                window.location.replace('messages.aspx?prjID=' + discussion.projectId + '&id=' + discussion.id);
            });
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
                    if ((participants[i].id == projectTeam[j].id) && projectTeam[j].canReadMessages) {
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
        projectFolderId = parseInt($fileContainer.attr("data-projectfolderid"));
        projectName = $fileContainer.attr("data-projectName").trim();
        if (action == "edit")
            loadAttachmentsForEditingDiscussion();
    };

    function loadAttachmentsForEditingDiscussion() {
        var discussionId = jq.getURLParam("id");
        ProjectDocumentsPopup.init(projectFolderId, projectName);
        attachments.setFolderId(projectFolderId);
        attachments.loadFiles();

        attachments.bind("addFile", function (ev, file) {
            teamlab.addPrjEntityFiles(null, discussionId, entityType, [file.id], function () { });
        });
        attachments.bind("deleteFile", function (ev, fileId) {
            teamlab.removePrjEntityFiles({}, discussionId, entityType, fileId, function () { });
            attachments.deleteFileFromLayout(fileId);
        });
    };

    function attachFiles(discussion) {
        var onComplete = function() {
            window.onbeforeunload = null;
            window.location.replace('messages.aspx?prjID=' + discussion.projectId + '&id=' + discussion.id);
        };

        var filesIds = newFilesToAttach.concat(filesFromProject);
        if (filesIds.length) {
            teamlab.addPrjEntityFiles(null, discussion.id, entityType, filesIds, onComplete);
        } else {
            teamlab.fckeEditCommentComplete({},
            { commentid: discussion.id.toString(), domain: 'discussion', html: discussion.text, isedit: false },
            onComplete);
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
            var existFlag = existParticipants.some(function(item) { return item.attr('guid') == team[i].id; });
            
            if (!existFlag) {
                team[i].descriptionFlag = team[i].id == teamlab.profile.id;
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
        if (this.getData() == "") {
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