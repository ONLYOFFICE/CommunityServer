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
var FeedProductsColection = {
    community: ASC.Resources.Master.FeedResource.CommunityProduct,
    projects: ASC.Resources.Master.FeedResource.ProjectsProduct,
    crm: ASC.Resources.Master.FeedResource.CrmProduct,
    documents: ASC.Resources.Master.FeedResource.DocumentsProduct
};

var FeedTextsColection = {
    blog: {
        createdText: ASC.Resources.Master.FeedResource.BlogCreatedText,
        commentedText: ASC.Resources.Master.FeedResource.BlogCommentedText,
        location: ASC.Resources.Master.FeedResource.BlogsModule
    },
    bookmark: {
        createdText: ASC.Resources.Master.FeedResource.BookmarkCreatedText,
        commentedText: ASC.Resources.Master.FeedResource.BookmarkCommentedText,
        location: ASC.Resources.Master.FeedResource.BookmarksModule
    },
    news: {
        createdText: ASC.Resources.Master.FeedResource.NewsCreatedText,
        commentedText: ASC.Resources.Master.FeedResource.NewsCommentedText,
        location: ASC.Resources.Master.FeedResource.EventsModule
    },
    order: {
        createdText: ASC.Resources.Master.FeedResource.OrderCreatedText,
        commentedText: ASC.Resources.Master.FeedResource.OrderCommentedText,
        location: ASC.Resources.Master.FeedResource.EventsModule
    },
    advert: {
        createdText: ASC.Resources.Master.FeedResource.AdvertCreatedText,
        commentedText: ASC.Resources.Master.FeedResource.AdvertCommentedText,
        location: ASC.Resources.Master.FeedResource.EventsModule
    },
    poll: {
        createdText: ASC.Resources.Master.FeedResource.PollCreatedText,
        commentedText: ASC.Resources.Master.FeedResource.PollCommentedText,
        location: ASC.Resources.Master.FeedResource.EventsModule
    },
    forumPost: {
        createdText: ASC.Resources.Master.FeedResource.ForumPostCreatedText,
        location: ASC.Resources.Master.FeedResource.ForumsModule
    },
    forumTopic: {
        createdText: ASC.Resources.Master.FeedResource.ForumTopicCreatedText,
        location: ASC.Resources.Master.FeedResource.ForumsModule
    },
    forumPoll: {
        createdText: ASC.Resources.Master.FeedResource.ForumPollCreatedText,
        location: ASC.Resources.Master.FeedResource.ForumsModule
    },
    company: {
        createdText: ASC.Resources.Master.FeedResource.CompanyCreatedText,
        location: ASC.Resources.Master.FeedResource.ContactsModule
    },
    person: {
        createdText: ASC.Resources.Master.FeedResource.PersonCreatedText,
        location: ASC.Resources.Master.FeedResource.ContactsModule
    },
    crmTask: {
        createdText: ASC.Resources.Master.FeedResource.CrmTaskCreatedText,
        extraText: ASC.Resources.Master.FeedResource.Responsible,
        extraText2: ASC.Resources.Master.FeedResource.Contact,
        location: ASC.Resources.Master.FeedResource.CrmTaskModule
    },
    deal: {
        createdText: ASC.Resources.Master.FeedResource.DealCreatedText,
        extraText2: ASC.Resources.Master.FeedResource.Contact,
        location: ASC.Resources.Master.FeedResource.OpportunitiesModule
    },
    cases: {
        createdText: ASC.Resources.Master.FeedResource.CaseCreatedText,
        location: ASC.Resources.Master.FeedResource.CasesModule
    },
    project: {
        createdText: ASC.Resources.Master.FeedResource.ProjectCreatedText,
        extraText: ASC.Resources.Master.FeedResource.ProjectManager
    },
    participant: {
        createdText: ASC.Resources.Master.FeedResource.ParticipantCreatedText
    },
    milestone: {
        createdText: ASC.Resources.Master.FeedResource.MilestoneCreatedText,
        extraText: ASC.Resources.Master.FeedResource.Project,
        location: ASC.Resources.Master.FeedResource.MilestonesModule
    },
    task: {
        createdText: ASC.Resources.Master.FeedResource.TaskCreatedText,
        commentedText: ASC.Resources.Master.FeedResource.TaskCommentedText,
        extraText: ASC.Resources.Master.FeedResource.Project,
        extraText2: ASC.Resources.Master.FeedResource.Responsibles,
        location: ASC.Resources.Master.FeedResource.TasksModule
    },
    discussion: {
        createdText: ASC.Resources.Master.FeedResource.DiscussionCreatedText,
        commentedText: ASC.Resources.Master.FeedResource.DiscussionCommentedText,
        location: ASC.Resources.Master.FeedResource.DiscussionsModule
    },
    file: {
        createdText: ASC.Resources.Master.FeedResource.FileCreatedText,
        createdExtraText: ASC.Resources.Master.FeedResource.FileCreatedInFolderText,
        updatedText: ASC.Resources.Master.FeedResource.FileUpdatedText,
        updatedExtraText: ASC.Resources.Master.FeedResource.FileUpdatedInFolderText,
        extraText: ASC.Resources.Master.FeedResource.Size,
        location: ASC.Resources.Master.FeedResource.FilesModule
    },
    sharedFile: {
        createdText: ASC.Resources.Master.FeedResource.SharedFileCreatedText,
        createdExtraText: ASC.Resources.Master.FeedResource.SharedFileCreatedInFolderText,
        extraText: ASC.Resources.Master.FeedResource.Size,
        location: ASC.Resources.Master.FeedResource.FilesModule
    },
    folder: {
        createdText: ASC.Resources.Master.FeedResource.FolderCreatedText,
        createdExtraText: ASC.Resources.Master.FeedResource.FolderCreatedInFolderText,
        location: ASC.Resources.Master.FeedResource.FoldersModule
    },
    sharedFolder: {
        createdText: ASC.Resources.Master.FeedResource.SharedFolderCreatedText,
        location: ASC.Resources.Master.FeedResource.FoldersModule
    }
};

var FeedReader = new function() {
    var guestId = "712d9ec3-5d2b-4b13-824f-71f00191dcca";
    // for develop set "/asc"
    var developUrlPrefix = "";

    this.HasNewFeeds = false;
    this.DataReaded = false;

    this.NewsRequested = false;

    this.GetNewFeedsCount = function() {
        Teamlab.getNewFeedsCount({}, { filter: {}, success: FeedReader.OnGetNewFeedsCount });
    };

    this.OnGetNewFeedsCount = function(params, newsCount) {
        if (!newsCount) {
            return;
        }
        if (newsCount > 100) {
            newsCount = ">100";
        }

        var $feedLink = jq(".top-item-box.feed");
        $feedLink.addClass("has-led");
        $feedLink.find(".inner-label").text(newsCount);

        FeedReader.HasNewFeeds = true;
    };

    this.GetDropFeeds = function() {
        var filter = {
            onlyNew: true,
        };

        Teamlab.getFeeds({}, { filter: filter, success: FeedReader.OnGetDropFeeds });
    };

    this.OnGetDropFeeds = function(params, response) {
        var feeds = response.feeds,
            $dropFeedsBox = jq("#drop-feeds-box"),
            $loader = $dropFeedsBox.find(".loader-text-block"),
            $feedsReadedMsg = $dropFeedsBox.find(".feeds-readed-msg"),
            $seeAllBtn = $dropFeedsBox.find(".see-all-btn"),
            dropFeedsList = $dropFeedsBox.find(".list");

        if (!feeds.length) {
            $loader.hide();
            $feedsReadedMsg.show();
            $seeAllBtn.css("display", "inline-block");
            return;
        }

        dropFeedsList.empty();

        for (var i = 0; i < feeds.length; i++) {
            try {
                var template = getFeedTemplate(feeds[i]);
                jq.tmpl("dropFeedTmpl", template).appendTo(dropFeedsList);
            } catch(e) {
                console.log(e);
            }
        }

        $loader.hide();
        jq(dropFeedsList).removeClass("display-none");
        $seeAllBtn.css("display", "inline-block");
    };

    var getFeedTemplate = function(feed) {
        var template = feed;

        template.author = getAuthor(feed);

        template.byGuest = template.author.UserInfo.ID == guestId;
        template.productText = getFeedProductText(template);

        if (!template.location) {
            template.location = getFeedLocation(template);
        }

        resolveAdditionalFeedData(template);
        template.actionText = getFeedActionText(template);

        template.itemUrl = developUrlPrefix + template.itemUrl;

        return template;
    };
    
    function getAuthor(feed) {
        var author;
        if (feed.comments && feed.comments.length) {
            author = feed.comments[feed.comments.length - 1].author;
            author.AvatarUrl = feed.comments[feed.comments.length - 1].authorAvatar;
        } else {
            author = feed.author;
            author.AvatarUrl = feed.authorAvatar;
        }
        
        author.ProfileUrl = developUrlPrefix + author.ProfileUrl;

        return author;
    }

    function getFeedProductText(template) {
        var productsCollection = FeedProductsColection;
        if (!productsCollection) {
            return null;
        }

        return productsCollection[template.product];
    }

    function getFeedLocation(template) {
        var textsColection = FeedTextsColection;
        if (!textsColection) {
            return null;
        }

        var itemTexts = textsColection[template.item];
        if (!itemTexts) {
            return null;
        }

        return itemTexts.location;
    }

    function getFeedActionText(template) {
        var textsCollection = FeedTextsColection;
        if (!textsCollection) {
            return null;
        }

        var itemTexts = textsCollection[template.item];
        if (!itemTexts) {
            return null;
        }

        switch (template.action) {
            case 0:
                return itemTexts.createdText;
            case 1:
                return itemTexts.updatedText;
            case 2:
                return itemTexts.commentedText;
            default:
                return itemTexts.createdText;
        }
    }

    function resolveAdditionalFeedData(template) {
        switch (template.item) {
            case "blog":
                template.itemClass = "blogs";
                break;
            case "news":
            case "order":
            case "advert":
            case "poll":
                template.itemClass = "events";
                break;
            case "forum":
            case "forumPoll":
                template.itemClass = "forum";
                break;
            case "bookmark":
                template.itemClass = "bookmarks";
                break;
            case "company":
            case "person":
                template.itemClass = "group";
                break;
            case "crmTask":
                template.itemClass = "tasks";
                break;
            case "deal":
                template.itemClass = "opportunities";
                break;
            case "cases":
                template.itemClass = "cases";
                break;
            case "project":
                template.itemClass = "projects";
                break;
            case "participant":
                template.itemClass = "projects";
                break;
            case "milestone":
                template.itemClass = "milestones";
                break;
            case "task":
                template.itemClass = "tasks";
                break;
            case "discussion":
                template.itemClass = "discussions";
                break;
            case "file":
            case "sharedFile":
                template.itemClass = "documents";
                break;
            case "folder":
            case "sharedFolder":
                template.itemClass = "documents";
                break;
        }
    }
};

jq(document).ready(function() {
    jq.dropdownToggle({
        switcherSelector: ".studio-top-panel .feedActiveBox",
        dropdownID: "studio_dropFeedsPopupPanel",
        addTop: 5,
        addLeft: -405
    });

    jq(".studio-top-panel .feedActiveBox").on("mouseup", function(event) {
        if (event.which == 2 && FeedReader.HasNewFeeds) {
            FeedReader.HasNewFeeds = false;
            jq(".studio-top-panel .feedActiveBox .inner-label").remove();
        }
    });

    jq(".studio-top-panel .feedActiveBox").on("click", function(event) {
        if (FeedReader.NewsRequested) {
            return false;
        }

        if (event.which == 2 && FeedReader.HasNewFeeds) {
            FeedReader.HasNewFeeds = false;
            jq(".studio-top-panel .feedActiveBox .inner-label").remove();
            return true;
        }
        if (event.which != 1) {
            return true;
        }

        if (FeedReader.HasNewFeeds) {
            if (!FeedReader.DataReaded) {
                FeedReader.NewsRequested = true;

                FeedReader.GetDropFeeds();
                Teamlab.readFeeds({}, {
                    filter: {},
                    success: function(params, readed) {
                        if (readed) {
                            FeedReader.NewsRequested = false;

                            FeedReader.DataReaded = true;
                            jq(".studio-top-panel .feedActiveBox .inner-label").remove();
                        }
                    }
                });
            }
            event.preventDefault();
        } else {
            event.stopPropagation();
        }

        return true;
    });

    setTimeout(function() {
        FeedReader.GetNewFeedsCount();
    }, 3000);
});