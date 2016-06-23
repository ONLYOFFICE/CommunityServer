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


ASC.Projects.Discussions = (function() {
    var isInit = false;

    var myGuid;
    var currentProjectId;

    var filterDiscCount = 0;

    var discussionsList;
    var self;
    var $emptyScreenForFilter = jq('#discEmptyScreenForFilter');
    var $emptyList = jq('#emptyListDiscussion');

    //init
    var init = function () {
        if (isInit === false) {
            isInit = true;
        }
        self = this;
        self.isFirstLoad = true;
        self.cookiePagination = "discussionsKeyForPagination";
        
        self.setDocumentTitle(ASC.Projects.Resources.ProjectsJSResource.DiscussionsModule);
        self.checkElementNotFound(ASC.Projects.Resources.ProjectsJSResource.DiscussionNotFound);

        myGuid = Teamlab.profile.id;

        self.showLoader();
        
        ASC.Projects.PageNavigator.init(self, true);

        discussionsList = jq('#discussionsList');

        currentProjectId = jq.getURLParam('prjID');

        // waiting data from api
        self.createAdvansedFilter();

        discussionsList.on("click", ".title-list .status", function () {
            var path = jq.changeParamValue(ASC.Controls.AnchorController.getAnchor(), 'status', 'archived');
            ASC.Controls.AnchorController.move(path);
        });
        
        discussionsList.on("click", ".name-list.project", function () {
            var projectId = jq(this).attr('data-projectId');
            var path = jq.changeParamValue(ASC.Controls.AnchorController.getAnchor(), 'project', projectId);
            path = jq.removeParam('tag', path);
            ASC.Controls.AnchorController.move(path);
        });

        discussionsList.on("click", ".name-list.author", function () {
            var authorId = jq(this).attr('data-authorId');
            if (authorId != "4a515a15-d4d6-4b8e-828e-e0586f18f3a3") {
                var path = jq.changeParamValue(ASC.Controls.AnchorController.getAnchor(), 'author', authorId);
                ASC.Controls.AnchorController.move(path);
            }
        });
    };

    var createAdvansedFilter = function () {
        var now = new Date();
        var today = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0);
        var lastWeek = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0);
        lastWeek.setDate(lastWeek.getDate() - 7);

        var filters = [];
        //Author

        if (currentProjectId) {
            if (ASC.Projects.Common.userInProjectTeam(Teamlab.profile.id)) {
                filters.push({
                    type: "combobox",
                    id: "me_author",
                    title: ASC.Projects.Resources.ProjectsFilterResource.MyDiscussions,
                    filtertitle: ASC.Projects.Resources.ProjectsFilterResource.Author + ":",
                    group: ASC.Projects.Resources.ProjectsFilterResource.Author,
                    hashmask: "person/{0}",
                    groupby: "userid",
                    options: ASC.Projects.ProjectsAdvansedFilter.getTeamForFilter(),
                    bydefault: { value: Teamlab.profile.id }
                });
            }
            filters.push({
                type: "combobox",
                id: "author",
                title: ASC.Projects.Resources.ProjectsFilterResource.OtherParticipant,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.Author + ":",
                hashmask: "person/{0}",
                groupby: "userid",
                group: ASC.Projects.Resources.ProjectsFilterResource.ByParticipant,
                options: ASC.Projects.ProjectsAdvansedFilter.getTeamForFilter(),
                defaulttitle: ASC.Projects.Resources.ProjectsFilterResource.Select
            });
        } else {
            filters.push({
                type: "person",
                id: "me_author",
                title: ASC.Projects.Resources.ProjectsFilterResource.MyDiscussions,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.Author + ":",
                group: ASC.Projects.Resources.ProjectsFilterResource.Author,
                hashmask: "person/{0}",
                groupby: "userid",
                bydefault: { id: Teamlab.profile.id }
            });
            filters.push({
                type: "person",
                id: "author",
                title: ASC.Projects.Resources.ProjectsFilterResource.OtherUsers,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.Author + ":",
                group: ASC.Projects.Resources.ProjectsFilterResource.Author,
                hashmask: "person/{0}",
                groupby: "userid"
            });
        }

        //Projects
        if (!currentProjectId) {
            filters.push({
                type: "flag",
                id: "myprojects",
                title: ASC.Projects.Resources.ProjectsFilterResource.MyProjects,
                group: ASC.Projects.Resources.ProjectsFilterResource.ByProject,
                hashmask: "myprojects",
                groupby: "projects"
            });
            filters.push({
                type: "combobox",
                id: "project",
                title: ASC.Projects.Resources.ProjectsFilterResource.OtherProjects,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByProject + ":",
                group: ASC.Projects.Resources.ProjectsFilterResource.ByProject,
                options: ASC.Projects.Common.getProjectsForFilter(),
                groupby: "projects",
                defaulttitle: ASC.Projects.Resources.ProjectsFilterResource.Select
            });
            filters.push({
                type: "combobox",
                id: "tag",
                title: ASC.Projects.Resources.ProjectsFilterResource.ByTag,
                filtertitle: ASC.Projects.Resources.ProjectsFilterResource.Tag + ":",
                group: ASC.Projects.Resources.ProjectsFilterResource.ByProject,
                options: ASC.Projects.ProjectsAdvansedFilter.getTagsForFilter(),
                groupby: "projects",
                defaulttitle: ASC.Projects.Resources.ProjectsFilterResource.Select
            });
        }
        // Status
        filters.push({
            type: "combobox",
            id: "open",
            title: ASC.Projects.Resources.ProjectsFilterResource.StatusOpenDiscussion,
            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByStatus + ":",
            group: ASC.Projects.Resources.ProjectsFilterResource.ByStatus,
            hashmask: "combobox/{0}",
            groupby: "status",
            options:
                [
                    { value: "open", title: ASC.Projects.Resources.ProjectsFilterResource.StatusOpenDiscussion, def: true },
                    { value: "archived", title: ASC.Projects.Resources.ProjectsFilterResource.StatusArchivedDiscussion }
                ]
        });
        filters.push({
            type: "combobox",
            id: "archived",
            title: ASC.Projects.Resources.ProjectsFilterResource.StatusArchivedDiscussion,
            filtertitle: ASC.Projects.Resources.ProjectsFilterResource.ByStatus + ":",
            group: ASC.Projects.Resources.ProjectsFilterResource.ByStatus,
            hashmask: "combobox/{0}",
            groupby: "status",
            options:
                [
                    { value: "open", title: ASC.Projects.Resources.ProjectsFilterResource.StatusOpenDiscussion },
                    { value: "archived", title: ASC.Projects.Resources.ProjectsFilterResource.StatusArchivedDiscussion, def: true }
                ]
        });
        //Creation date
        filters.push({
            type: "daterange",
            id: "today2",
            title: ASC.Projects.Resources.ProjectsFilterResource.Today,
            filtertitle: " ",
            group: ASC.Projects.Resources.ProjectsFilterResource.ByCreateDate,
            hashmask: "created/{0}/{1}",
            groupby: "created",
            bydefault: { from: today.getTime(), to: today.getTime() }
        });
        filters.push({
            type: "daterange",
            id: "recent",
            title: ASC.Projects.Resources.ProjectsFilterResource.Recent,
            filtertitle: " ",
            group: ASC.Projects.Resources.ProjectsFilterResource.ByCreateDate,
            hashmask: "created/{0}/{1}",
            groupby: "created",
            bydefault: { from: lastWeek.getTime(), to: today.getTime() }
        });
        filters.push({
            type: "daterange",
            id: "created",
            title: ASC.Projects.Resources.ProjectsFilterResource.CustomPeriod,
            filtertitle: " ",
            group: ASC.Projects.Resources.ProjectsFilterResource.ByCreateDate,
            hashmask: "created/{0}/{1}",
            groupby: "created"
        });
        //Followed
        filters.push({
            type: "flag",
            id: "followed",
            title: ASC.Projects.Resources.ProjectsFilterResource.FollowDiscussions,
            group: ASC.Projects.Resources.ProjectsFilterResource.Other,
            hashmask: "followed"
        });

        self.filters = filters;
        self.colCount = 3;
        if (currentProjectId) self.colCount = 2;

        self.sorters =
        [
            { id: "comments", title: ASC.Projects.Resources.ProjectsFilterResource.ByComments, sortOrder: "descending", def: true },
            { id: "create_on", title: ASC.Projects.Resources.ProjectsFilterResource.ByCreateDate, sortOrder: "descending" },
            { id: "title", title: ASC.Projects.Resources.ProjectsFilterResource.ByTitle, sortOrder: "ascending" }
        ];

        ASC.Projects.ProjectsAdvansedFilter.init(self);
    };

    var getData = function () {
        self.showLoader();
        self.currentFilter.Count = ASC.Projects.PageNavigator.entryCountOnPage;
        self.currentFilter.StartIndex = ASC.Projects.PageNavigator.entryCountOnPage * ASC.Projects.PageNavigator.currentPage;

        Teamlab.getPrjDiscussions({}, { filter: self.currentFilter, success: onGetDiscussions });
    };

    var showOrHideEmptyScreen = function(discussionCount, total) {
        if (discussionCount) {
            self.$noContentBlock.hide();
            ASC.Projects.ProjectsAdvansedFilter.show();
            return;
        }
        if (ASC.Projects.PageNavigator.currentPage > 0 && filterDiscCount > 0) {
            ASC.Projects.PageNavigator.setMaxPage(filterDiscCount);
            getData(true);
            return;
        }

        ASC.Projects.PageNavigator.hide();
        if (ASC.Projects.ProjectsAdvansedFilter.baseFilter) {
            ASC.Projects.ProjectsAdvansedFilter.hide();
            $emptyScreenForFilter.hide();
            $emptyList.show();
        } else if (filterDiscCount === 0) {
            $emptyList.hide();
            ASC.Projects.ProjectsAdvansedFilter.show();
            $emptyScreenForFilter.show();
        }
    };

    var onGetDiscussions = function (params, discussions) {
        var discussionCount = discussions.length;
        filterDiscCount = params.__total != undefined ? params.__total : 0;

        self.clearTables();
        ASC.Projects.PageNavigator.update(filterDiscCount);

        discussionsList.empty();
        
        showOrHideEmptyScreen(discussionCount);
        if (discussionCount) {
            showDiscussions(discussions);
        }

        self.hideLoader();
    };

    var showDiscussions = function(discussions) {
        var templates = discussions.map(getDiscussionTemplate);

        discussionsList.empty();

        for (var j = 0; j < templates.length; j++) {
            try {
                jq.tmpl("projects_discussionTemplate", templates[j]).appendTo(discussionsList);
            } catch (e) {

            }
        }

        discussionsList.show();

        jq('.content-list p').filter(function(index) { return jq(this).html() == "&nbsp;"; }).remove();

    };

    var getDiscussionTemplate = function(discussion) {
        var discussionId = discussion.id;
        var prjId = discussion.projectId;

        var template =
        {
            createdDate: discussion.displayDateCrtdate,
            createdTime: discussion.displayTimeCrtdate,
            title: discussion.title,
            discussionUrl: 'messages.aspx?prjID=' + prjId + '&id=' + discussionId,
            authorAvatar: discussion.createdBy.avatar,
            authorId: discussion.createdBy.id,
            authorName: discussion.createdBy.displayName,
            authorPost: discussion.createdBy.title,
            status: discussion.status,
            projectId: prjId,
            text: discussion.text,
            hasPreview: discussion.text.search('class="asccut"') > 0,
            commentsCount: discussion.commentsCount,
            commentsUrl: 'messages.aspx?prjID=' + prjId + '&id=' + discussionId + '#comments',
            writeCommentUrl: 'messages.aspx?prjID=' + prjId + '&id=' + discussionId + '#addcomment',
            canComment: !window.Teamlab.profile.isOutsider
        };
        if (!currentProjectId) {
            template.projectTitle = discussion.projectTitle;
            template.projectUrl = 'projects.aspx?prjID=' + discussion.projectId;
        }
        return template;
    };

    var unbindListEvents = function () {
        if (!isInit) return;
        discussionsList.unbind();
    };

    return jq.extend({
        init: init,
        getData: getData,
        createAdvansedFilter: createAdvansedFilter,
        unbindListEvents: unbindListEvents,
        basePath: 'sortBy=create_on&sortOrder=descending'
    }, ASC.Projects.Base);
})();

ASC.Projects.DiscussionDetails = (function () {
    var projectFolderId, projectName, fileContainer, privateFlag, projectTeam;
    var discussionId = jq.getURLParam("id");
    var projId = jq.getURLParam("prjID");
    var currentUserId;
    var participantsCount;
    var isCommentEdit = false;
    var subscribeButton = jq('#changeSubscribeButton');
    var status = jq("#discussionStatus").val();
    
    var init = function () {
        currentUserId = Teamlab.profile.id;
        privateFlag = jq("#discussionParticipantsContainer").attr("data-private") === "True" ? true : false;

        projectTeam = ASC.Projects.Master.Team;
        
        Teamlab.getSubscribesToPrjDiscussion({}, discussionId, { success: onGetPrjSubscribers });
        
        fileContainer = jq("#discussionFilesContainer");
        if (fileContainer.length && window.Attachments) {
            initAttachmentsControl();
        }

        if (jq("#discussionParticipantsContainer [guid=" + currentUserId + "][id!=currentLink]").length) {
            jq("#currentLink").remove();
        }

        var commentsCount = jq("#mainCommentsContainer div[id^=container_] div[id^=comment_] table").length;

        jq("#add_comment_btn").wrap("<span class='addcomment-button icon-link plus'></span>");
        if (commentsCount > 0) {
            jq("#hideCommentsButton").show();
            jq("#commentsContainer").css("marginTop", "15px");
        } else {
            jq("#commentsContainer").css("marginTop", "-6px");
        }
        jq("#commentsContainer").show();

        if (jq('#discussionActions .dropdown-content li a').length == 0 && discussionId != null) {
            jq('.menu-small').addClass("visibility-hidden");
        }

        var hash = ASC.Controls.AnchorController.getAnchor();
        if (hash == "addcomment" && CommentsManagerObj) {
            ckeditorConnector.onReady(CommentsManagerObj.AddNewComment);
        }

        if (status == 1) {
            jq("#manageParticipantsSelector").hide();
        }

        var participantIds = [];
        jq("#discussionParticipantsTable .items-display-list_i").each(function () {
            var userId = jq(this).not(".hidden").attr("guid");
            participantIds.push(userId);
        });

        jq("#manageParticipantsSelector").useradvancedSelector({
            showGroups: true,
            itemsSelectedIds: participantIds

        }).on("additionalClickEvent" , function () {
            jq("#discussionActions").hide();
            jq(".project-title .menu-small").removeClass("active");

        }).on("showList", function (event, users) {
            var participants = [];
            var participantsIds = [];

            for (var i = 0; i < users.length; i++) {
                var userId = users[i].id,
                 userName = users[i].title,
                 userDepartments = [];

                users[i].groups.forEach(function (el) {
                    userDepartments.push(el.name);
                });

                participants.push({ id: userId, displayName: userName, descriptionFlag: true });
                participantsIds.push(userId);
            }

            var data = {};
            data.projectId = jq.getURLParam("prjID");
            data.participants = participantsIds.join();
            data.notify = false;

            Teamlab.updatePrjDiscussion({ participants: participants }, discussionId, data, {
                before: function () { LoadingBanner.displayLoading(); },
                success: onChangeDiscussionParticipants,
                error: onDiscussionError,
                after: function () { LoadingBanner.hideLoading(); }
            });
        });
        
        subscribeButton.click(function () {
            Teamlab.subscribeToPrjDiscussion({}, discussionId, { success: onChangeSubscribe, error: onDiscussionError });
        });

        jq.switcherAction("#switcherDiscussionParticipants", "#discussionParticipantsContainer");
        jq.switcherAction("#switcherFilesButton", "#discussionFilesContainer");
        jq.switcherAction("#switcherCommentsButton", "#discussionCommentsContainer");

        jq("#createTaskOnDiscussion").click(function () {
            jq("body").click();
            LoadingBanner.displayLoading();
            Teamlab.addPrjTaskByMessage({}, projId, discussionId, {
                success: function (params, task) {
                    window.onbeforeunload = null;
                    location.href = "tasks.aspx?prjID=" + projId + "&id=" + task.id;
                }
            });
        });

        jq("#discussionParticipantsTable").on("mouseenter", ".items-display-list_i.gray", function () {
            jq(this).helper({
                BlockHelperID: "hintSubscribersPrivateProject",
                addLeft: 45,
                addTop: 12
            });
        });

        jq("#discussionParticipantsTable").on("mouseleave", ".items-display-list_i.gray", function () {
            jq("#hintSubscribersPrivateProject").hide();
        });

        jq("#addFirstCommentButton").click(function () {
            jq("#commentsContainer").show();
            jq("#add_comment_btn").click();
        });

        jq("#deleteDiscussionButton").click(function () {
            jq("#discussionActions").hide();
            jq(".project-title .menu-small").removeClass("active");
            StudioBlockUIManager.blockUI(jq("#questionWindow"), 400, 400, 0, "absolute");
        });

        jq("#questionWindow .remove").bind("click", function () {
            deleteDiscussion();
            return false;
        });
        jq("#questionWindow .cancel").bind("click", function () {
            jq.unblockUI();
            return false;
        });

        jq(document).on("click", "#btnCancel, #cancel_comment_btn", function () {
            isCommentEdit = false;
        });
        
        jq(document).on("click", "a[id^=edit_]", function () {
            isCommentEdit = true;
        });

        Teamlab.bind(Teamlab.events.addPrjComment, function () {
            if (!isCommentEdit) {
                commentsCount = jq("#mainCommentsContainer div[id^=container_] div[id^=comment_] table").length;
                updateTabTitle("comments", commentsCount);
                jq("#hideCommentsButton").show();
                jq("#commentsContainer").css("marginTop", "15px");
            } else {
                isCommentEdit = false;
            }
        });

        jq("#changeStatus").click(function () {
            Teamlab.updatePrjDiscussionStatus({}, discussionId, { status: jq(this).attr("updateStatus") }, {
                 before: LoadingBanner.displayLoading,
                 success: onUpdateDiscussion,
                 after: LoadingBanner.hideLoading
            });
        });
    };
    
    var onGetPrjSubscribers = function (params, subscribers) {        
        subscribers.forEach(function (item) { item.descriptionFlag = true; });
        jq("#manageParticipantsSelector").useradvancedSelector("select", subscribers.map(function (item) { return item.id; }));
        showDiscussionParticipants(subscribers);
        participantsCount = subscribers.length;
        if (participantsCount > 0) {
            jq("#switcherDiscussionParticipants").show();
        }
        updateTabTitle("participants", participantsCount);
    };
    
    var initAttachmentsControl = function () {

        projectFolderId = parseInt(jq("#discussionFilesContainer").attr("data-projectfolderid"));
        projectName = Encoder.htmlEncode(jq("#discussionFilesContainer").attr("data-projectName").trim());

        var canEditFiles = fileContainer.attr("data-canEdit") === "True" ? true : false;

        if (!canEditFiles) {
            Attachments.banOnEditing();
        }

        ProjectDocumentsPopup.init(projectFolderId, projectName);
        Attachments.init();
        Attachments.setFolderId(projectFolderId);
        Attachments.loadFiles();

        var id = jq.getURLParam("id");

        var filesCount = parseInt(jq('#discussionTabs div[container=discussionFilesContainer]').attr('count'));
        if (filesCount > 0) {
            jq("#hideFilesButton").show();
            jq("#discussionFilesContainer").css("marginBottom", "18px");
        }

        Attachments.bind("addFile", function (ev, file) {
            if (file.attachFromPrjDocFlag || file.isNewFile) {
                Teamlab.addPrjEntityFiles(null, id, "message", [file.id], { error: onDiscussionError });
            }
            filesCount++;
            updateTabTitle('files', filesCount);
            jq("#hideFilesButton").show();
            jq("#discussionFilesContainer").css("marginBottom", "18px");
        });

        Attachments.bind("deleteFile", function (ev, fileId) {
            Teamlab.removePrjEntityFiles({}, id, "message", fileId, { error: onDiscussionError });
            Attachments.deleteFileFromLayout(fileId);
            filesCount--;
            updateTabTitle('files', filesCount);
            if (filesCount == 0) {
                jq("#hideFilesButton").hide();
                jq("#discussionFilesContainer").css("marginBottom", "0px");
            }
        });
    };
    var removeComment = function () {
        var commentsCount = jq("#mainCommentsContainer div[id^=container_] div[id^=comment_] table").length;
        updateTabTitle("comments", commentsCount);
        if (commentsCount == 0) {
            jq("#mainCommentsContainer").empty();
            jq("#mainCommentsContainer").hide();
            jq("#commentsTitle").empty();
            jq("#hideCommentsButton").hide();
            jq("#commentsContainer").css("marginTop", "-6px");
        }
    };

    var onDiscussionError = function () {
        window.onbeforeunload = null;
        window.location.reload();
    };
    var onDeleteDiscussionError = function () {
        if (this.__errors[0] == "Access denied.") {
            window.onbeforeunload = null;
            window.location.replace("messages.aspx");
        }
    };

    var onChangeDiscussionParticipants = function (params, discussion) {
        jq("#discussionParticipantsContainer .items-display-list_i").not(".hidden").remove();

        showDiscussionParticipants(params.participants);

        participantsCount = jq("#discussionParticipantsTable .items-display-list_i").not(".hidden").length;
        updateTabTitle("participants", participantsCount);
        if (participantsCount == 0) {
            jq(".manage-participants-button").show();
            jq("#switcherDiscussionParticipants").hide();
        } else {
            jq("#discussionParticipantsContainer").show();
            jq("#switcherDiscussionParticipants").show();
        }

        var isSubscibed = params.participants.some(function(item) { return item.id == Teamlab.profile.id; });
        var isBeenSubscibed = subscribeButton.attr("subscribed") === "1";
        if (isSubscibed !== isBeenSubscibed) {
            var currentLink = jq("#discussionParticipantsContainer [guid=" + currentUserId + "]");
            if (!currentLink.length) {
                currentLink = jq("#currentLink");
            }
            if (isBeenSubscibed) {
                currentLink.hide();
                currentLink.addClass('hidden');
                subscribeButton.attr('subscribed', '0');
                subscribeButton.removeClass('subscribed').addClass('unsubscribed');
            } else {
                currentLink.show();
                currentLink.removeClass("hidden");
                subscribeButton.attr("subscribed", "1");
                subscribeButton.removeClass('unsubscribed').addClass('subscribed');
            }
        }
    };

    var showDiscussionParticipants = function (participants) {
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
        jq.tmpl("projects_subscribedUser", newListParticipants).appendTo("#discussionParticipantsTable");

        if (notSeePartisipant.length) {
            jq.tmpl("projects_subscribedUser", notSeePartisipant).addClass("gray").appendTo("#discussionParticipantsTable");
        }
    };

    var onChangeSubscribe = function (params, discussion) {
        var subscribed = subscribeButton.attr("subscribed") === "1";
        var currentLink = jq("#discussionParticipantsContainer [guid=" + currentUserId + "]");
        if (!currentLink.length) {
            currentLink = jq("#currentLink");
        }
        if (subscribed) {
            currentLink.hide();
            currentLink.addClass('hidden');
            subscribeButton.attr('subscribed', '0');
            subscribeButton.removeClass('subscribed').addClass('unsubscribed');
            jq("#manageParticipantsSelector").useradvancedSelector("disable", [currentUserId]);

        } else {
            currentLink.show();
            currentLink.removeClass("hidden");
            subscribeButton.attr("subscribed", "1");
            subscribeButton.removeClass('unsubscribed').addClass('subscribed');
            jq("#manageParticipantsSelector").useradvancedSelector("select", [currentUserId]);

        }
        participantsCount = jq("#discussionParticipantsTable .items-display-list_i").not(".hidden").length;
        updateTabTitle("participants", participantsCount);
    };

    var updateTabTitle = function(tabTitle, count) {
        var container;
        switch (tabTitle) {
        case "comments":
            container = "discussionCommentsContainer";
            break;
        case "participants":
            container = "discussionParticipantsContainer";
            break;
        case "files":
            container = "discussionFilesContainer";
            break;
        }
        if (!container) return;

        var tab = jq("#discussionTabs div[container=" + container + "] span:first");
        var oldTitle = tab.text();
        var ind = oldTitle.lastIndexOf("(");
        var newTitle = oldTitle;
        if (ind > -1 && count == 0) {
            newTitle = oldTitle.slice(0, ind);
        } else if (ind > -1 && count != 0) {
            newTitle = oldTitle.slice(0, ind) + "(" + count + ")";
        } else {
            if (count > 0)
                newTitle = oldTitle + " (" + count + ")";
        }
        tab.text(newTitle);
    };

    var deleteDiscussion = function () {
        var params = {};
        Teamlab.removePrjDiscussion(params, discussionId, { success: onDeleteDiscussion, error: ASC.Projects.DiscussionDetails.onDeleteDiscussionError });
    };

    var onDeleteDiscussion = function (params, discussion) {
        Teamlab.fckeRemoveCommentComplete(discussion.id.toString(), 'discussion', function () {
            window.onbeforeunload = null;
            window.location.replace("messages.aspx?prjID=" + discussion.projectId);
        });
    };

    var onUpdateDiscussion = function(params, data) {
        location.reload();
    };

    return {
        init: init,
        removeComment: removeComment
    };
})(jQuery);

ASC.Projects.DiscussionAction = (function () {
    var projectId, id;
    var loadListTeamFlag = false;
    var projectFolderId, projectName, currentUserId, privateFlag;
    var newFilesToAttach = [];
    var filesFromProject = [];
    var projectTeam = [];
    var discussionParticipants = [];

    var init = function () {
        currentUserId = Teamlab.profile.id;
        projectId = jq.getURLParam("prjID");
        id = jq.getURLParam("id");

        if (id)
            Teamlab.getSubscribesToPrjDiscussion({}, id, { success: onGetPrjSubscribers });

        if (jq("#discussionFilesContainer").length)
            initAttachmentsControl();

        jq('[id$=discussionTitle]').focus();

        var participantIds = [];
        jq('#discussionParticipantsContainer .items-display-list_i').each(function () {
            participantIds.push(jq(this).attr('guid'));
        });

        jq("#manageParticipantsSelector").useradvancedSelector({
            showGroups: true,
            itemsSelectedIds: participantIds

        }).on("showList", function (event, participants) {
            jq('#discussionParticipantsContainer .items-display-list_i').remove();

            var partisipantsTmpls = participants.map(function (item) {
                var departemts = [];
                item.groups.forEach(function (el) { departemts.push(el.name) });
                return { id: item.id, displayName: item.title, department: departemts.join(), descriptionFlag: false, profileUrl: item.profileUrl };
            });
            showDiscussionParticipants(partisipantsTmpls);
        });

        if (projectId) {
            loadListTeamFlag = true;
            if (jq.getURLParam("action") != "edit") {
                getTeam({}, projectId);
            } else {
                projectTeam = ASC.Projects.Master.Team;
            }
            privateFlag = ASC.Projects.Master.Projects.some(function (item) { return item.id == projectId && item.isPrivate; });
            jq(".mainPageContent").children(".loader-page").hide();
        } else {
            initProjectsCombobox();
        }

        jq('#discussionParticipantsContainer').on('click', ".items-display-list_i .reset-action", function () {
            var userId = jq(this).closest('li').attr('guid');
            if (userId != currentUserId) {
                jq(this).closest('li').remove();
                jq("#manageParticipantsSelector").useradvancedSelector("unselect", [userId]);
            }
        });

        jq('#discussionParticipantsContainer .items-display-list_i').each(function () {
            var userId = jq(this).attr('guid');
            if (userId == currentUserId) {
                jq(this).find('.reset-action').remove();
            }
        });

        jq('#hideDiscussionPreviewButton').click(function () {
            jq('#discussionPreviewContainer').hide();
        });

        jq('#discussionTitleContainer input').keyup(function () {
            if (jq.trim(jq(this).val()) != '') {
                jq('#discussionTitleContainer').removeClass('requiredFieldError');
            }
        });

        jq('#discussionPreviewButton').click(function () {
            if (jq(this).hasClass("disable")) return;

            var takeThis = this;

            Teamlab.getPrjDiscussionPreview({ takeThis: takeThis }, ASC.Projects.Common.ckEditor.getData(),
                {
                    before: function (params) { LoadingBanner.showLoaderBtn("#discussionActionPage"); },
                    after: function (params) { LoadingBanner.hideLoaderBtn("#discussionActionPage"); },
                    success: function (params, response) {
                        var $this = jq(params.takeThis),
                            discussion =
                                {
                                    title: jq('#discussionTitleContainer input').val(),
                                    authorName: $this.attr('authorName'),
                                    authorTitle: $this.attr('authorTitle'),
                                    authorPageUrl: $this.attr('authorPageUrl'),
                                    authorAvatarUrl: $this.attr('authorAvatarUrl'),
                                    createOn: formatDate(new Date()),
                                    content: response
                                };

                        jq('#discussionPreviewContainer .discussionContainer').remove();
                        jq.tmpl("projects_discussionActionTemplate", discussion).prependTo('#discussionPreviewContainer');
                        jq('#discussionPreviewContainer').show();
                    },
                });
        });

        jq('#discussionCancelButton').click(function () {
            if (jq(this).hasClass("disable")) return;
            var projectId = jq.getURLParam('prjID');
            window.onbeforeunload = null;
            if (!!window.history) {
                window.history.go(-1);
            } else {
                window.location.replace('messages.aspx?prjID=' + projectId);
            }

        });

        jq('#discussionActionButton').click(function () {
            if (jq(this).hasClass("disable")) return;
            jq('#discussionProjectContainer').removeClass('requiredFieldError');
            jq('#discussionTitleContainer').removeClass('requiredFieldError');
            jq('#discussionTextContainer').removeClass('requiredFieldError');

            var projectid = projectId ? projectId : jq('#discussionProjectSelect').attr("data-id");
            var title = jq.trim(jq('#discussionTitleContainer input').val());
            var content = ASC.Projects.Common.ckEditor.getData();

            var isError = false;
            if (!projectid) {
                jq('#discussionProjectContainer').addClass('requiredFieldError');
                isError = true;
            }

            if (title == '') {
                jq('#discussionTitleContainer').addClass('requiredFieldError');
                isError = true;
            }

            var tmp = document.createElement("DIV");
            tmp.innerHTML = content;

            if (tmp.textContent == "" || tmp.innerText == "") {
                jq('#discussionTextContainer').addClass('requiredFieldError');
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

            var discussionId = jq(this).attr('discussionId');
            if (discussionId != -1) {
                discussion.messageid = discussionId;
            }

            var participants = [];
            jq('#discussionParticipantsContainer .items-display-list_i').each(function () {
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

        jq.switcherAction("#switcherParticipantsButton", "#discussionParticipantsContainer");
        jq.switcherAction("#switcherFilesButton", "#discussionFilesContainer");

        jq("#discussionParticipants").on("mouseenter", ".items-display-list_i.gray", function () {
            jq(this).helper({
                BlockHelperID: "hintSubscribersPrivateProject",
                addLeft: 45,
                addTop: 12
            });
        });
        jq("#discussionParticipants").on("mouseleave", ".items-display-list_i.gray", function () {
            jq("#hintSubscribersPrivateProject").hide();
        });
        jq.confirmBeforeUnload(confirmBeforeUnloadCheck);
    };

    var confirmBeforeUnloadCheck = function () {
        return (jq('#discussionProjectSelect').length && jq('#discussionProjectSelect').attr("data-id").length) ||
            jq("[id$=discussionTitle]").val().length ||
            ASC.Projects.Common.ckEditor.getData().length ||
            discussionParticipants.length;
    };

    var onGetPrjSubscribers = function (params, subscribers) {
        subscribers.forEach(function (item) { item.descriptionFlag = false; });
        jq("#manageParticipantsSelector").useradvancedSelector("select", subscribers.map(function (item) {return item.id;}));
        showDiscussionParticipants(subscribers);
    };

    var discussionProjectChange = function(item) {
        jq('#discussionProjectContainer').removeClass('requiredFieldError');
        jq('#discussionParticipantsContainer .items-display-list_i').remove();
        privateFlag = item.isPrivate;

        jq('#errorAllParticipantsProject').hide();

        getTeam({}, item.id);
        jq("#discussionTitleContainer input").focus();
    };

    var initProjectsCombobox = function () {
        var allprojects = ASC.Projects.Master.Projects.filter(function(prj) {
            return prj.canCreateMessage;
        });

        var $discussionAdvancedSelector = jq("#discussionProjectSelect");
        $discussionAdvancedSelector.projectadvancedSelector(
            {
                itemsChoose: allprojects,  
                onechosen: true
            }
        );
        
        $discussionAdvancedSelector.on("showList", function (event, item) {
            jq("#discussionProjectSelect").attr("data-id", item.id).text(item.title).attr("title", item.title);
            discussionProjectChange(item);
        });
    };

    var addDiscussion = function (discussion) {
        var params = {};
        Teamlab.addPrjDiscussion(params, discussion.projectid, discussion, { success: onAddDiscussion, error: onAddDiscussionError });
    };

    var onAddDiscussion = function (params, discussion) {
        ASC.Projects.DiscussionAction.attachFiles(discussion);
    };

    var onAddDiscussionError = function () {
        if (this.__errors[0] == "Access denied.") {
            window.onbeforeunload = null;
            window.location.replace("messages.aspx");
        }
        unlockDiscussionActionPageElements();
    };

    var onUpdateDiscussionError = function () {
        if (this.__errors[0] == "Access denied.") {
            window.onbeforeunload = null;
            window.location.replace("messages.aspx");
        }
        unlockDiscussionActionPageElements();
    };

    var updateDiscussion = function (discussion) {
        var params = {};
        Teamlab.updatePrjDiscussion(params, discussion.messageid, discussion, { success: onUpdateDiscussion, error: onUpdateDiscussionError });
    };

    var onUpdateDiscussion = function (params, discussion) {

        Teamlab.fckeEditCommentComplete({},
            { commentid: discussion.id.toString(), domain: 'discussion', html: discussion.text, isedit: true },
            function () {
                window.onbeforeunload = null;
                window.location.replace('messages.aspx?prjID=' + discussion.projectId + '&id=' + discussion.id);
            });
    };
    
    var lockDiscussionActionPageElements = function() {
        jq('#discussionProjectSelect').attr('disabled', 'disabled').addClass('disabled');
        jq('#discussionTitleContainer input').attr('readonly', 'readonly').addClass('disabled');
        jq('iframe[id^=ctl00_ctl00]').contents().find('iframe').contents().find('#fckbodycontent').attr('readonly', 'readonly').addClass('disabled');
        LoadingBanner.showLoaderBtn("#discussionActionPage");
    };

    var unlockDiscussionActionPageElements = function() {
        jq('#discussionProjectSelect').removeAttr('disabled').removeClass('disabled');
        jq('#discussionTitleContainer input').removeAttr('readonly').removeClass('disabled');
        jq('iframe[id^=ctl00_ctl00]').contents().find('iframe').contents().find('#fckbodycontent').removeAttr('readonly').removeClass('disabled');
        LoadingBanner.hideLoaderBtn("#discussionActionPage");
    };

    var showDiscussionParticipants = function (participants) {
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
        jq.tmpl("projects_subscribedUser", newListParticipants).appendTo("#discussionParticipants");

        if (notSeePartisipant.length) {
            jq.tmpl("projects_subscribedUser", notSeePartisipant).addClass("gray").appendTo("#discussionParticipants");
        }
    };

    var initAttachmentsControl = function () {
        projectFolderId = parseInt(jq("#discussionFilesContainer").attr("data-projectfolderid"));
        projectName = jq("#discussionFilesContainer").attr("data-projectName").trim();
        var action = jq.getURLParam("action");
        if (action == "edit")
            loadAttachmentsForEditingDiscussion();

        //        if (projectId && action == "add")
        //            loadAttachmentsForCreatingDiscussion();
    };

    var loadAttachmentsForEditingDiscussion = function () {
        var discussionId = jq.getURLParam("id");
        ProjectDocumentsPopup.init(projectFolderId, projectName);
        Attachments.setFolderId(projectFolderId);
        Attachments.loadFiles();

        Attachments.bind("addFile", function (ev, file) {
            Teamlab.addPrjEntityFiles(null, discussionId, "message", [file.id], function () { });
        });
        Attachments.bind("deleteFile", function (ev, fileId) {
            Teamlab.removePrjEntityFiles({}, discussionId, "message", fileId, function () { });
            Attachments.deleteFileFromLayout(fileId);
        });
    };

    var loadAttachmentsForCreatingDiscussion = function () {
        var uploadWithAttach = false;
        Attachments.setFolderId(projectFolderId, uploadWithAttach);
        Attachments.setCreateNewEntityFlag(true);

        ProjectDocumentsPopup.init(projectFolderId, projectName);

        Attachments.bind("addFile", function (ev, file) {
            addFileToList(file);
        });
        Attachments.bind("deleteFile", function (ev, fileId) {
            removeFileFromList(fileId);
        });
        window.onbeforeunload = function (evt) {
            for (var i = 0; i < newFilesToAttach.length; i++) {
                Teamlab.removeDocFile({}, newFilesToAttach[i]);
            }
            return;
        };
    };

    var addFileToList = function (file) {
        if (file.fromProjectDocs) {
            filesFromProject.push(file.id);
        } else {
            newFilesToAttach.push(file.id);
        }
    };

    var removeFileFromList = function (fileId) {

        for (var i = 0; i < filesFromProject.length; i++) {
            if (fileId == filesFromProject[i]) {
                filesFromProject.splice(i, 1);
                Attachments.deleteFileFromLayout(fileId);
                break;
            }
        }
        for (var i = 0; i < newFilesToAttach.length; i++) {
            if (fileId == newFilesToAttach[i]) {
                newFilesToAttach.splice(i, 1);
                Teamlab.removeDocFile({}, fileId);
            }
        }

        Attachments.deleteFileFromLayout(fileId);
    };

    var attachFiles = function (discussion) {
        var filesIds = newFilesToAttach.concat(filesFromProject);
        if (filesIds.length) {
            Teamlab.addPrjEntityFiles(null, discussion.id, "message", filesIds, function () {
                window.onbeforeunload = null;
                window.location.replace('messages.aspx?prjID=' + discussion.projectId + '&id=' + discussion.id);
            });
        } else {
            Teamlab.fckeEditCommentComplete({},
            { commentid: discussion.id.toString(), domain: 'discussion', html: discussion.text, isedit: false },
            function () {
                window.onbeforeunload = null;
                window.location.replace('messages.aspx?prjID=' + discussion.projectId + '&id=' + discussion.id);
            });
        }
    };

    var getTeam = function (params, projId) {
        if (loadListTeamFlag) {
            onGetTeam({}, ASC.Projects.Master.Team);
        } else {
            Teamlab.getPrjTeam(params, projId, { before: function () { LoadingBanner.displayLoading(); }, success: onGetTeam, after: function () { LoadingBanner.hideLoading(); } });
        }
    };

    var onGetTeam = function (params, team) {
        projectTeam = team;
        var count = team ? team.length : 0;
        var newParticipants = [];
        if (count <= 0) return;
        
        var existParticipants = jq('#discussionParticipantsContainer .items-display-list_i').length ? jq('#discussionParticipantsContainer .items-display-list_i') : [];

        for (var i = 0; i < count; i++) {
            var existFlag = existParticipants.some(function(item) { return item.attr('guid') == team[i].id; });
            
            if (!existFlag) {
                team[i].descriptionFlag = team[i].id == Teamlab.profile.id;
                newParticipants.push(team[i]);
            }
        }
        
        onGetPrjSubscribers(null, newParticipants);
        
    };

    var formatDate = function (date) {
        var dateArray =
            ['0' + date.getDate(), '0' + (date.getMonth() + 1), '0' + date.getFullYear(), '0' + date.getHours(), '0' + date.getMinutes()];
        for (var i = 0; i < dateArray.length; i++) {
            dateArray[i] = dateArray[i].slice(-2);
        }
        var shortDate = dateArray[0] + '.' + dateArray[1] + '.' + dateArray[2] + ' ' + dateArray[3] + ':' + dateArray[4];
        return shortDate;
    };

    var showHidePreview = function() {
        if (this.getData() == "") {
            jq("#discussionPreviewButton").addClass("disable");
        } else {
            jq("#discussionPreviewButton").removeClass("disable");
        }
    };

    return {
        init: init,
        attachFiles: attachFiles,
        showHidePreview: showHidePreview
    };
})();