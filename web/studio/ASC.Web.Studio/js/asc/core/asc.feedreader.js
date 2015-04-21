/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


window.FeedProductsColection = {
    community: ASC.Resources.Master.FeedResource.CommunityProduct,
    projects: ASC.Resources.Master.FeedResource.ProjectsProduct,
    crm: ASC.Resources.Master.FeedResource.CrmProduct,
    documents: ASC.Resources.Master.FeedResource.DocumentsProduct
};

window.FeedTextsColection = {
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

var FeedReader = function() {
    var guestId = '712d9ec3-5d2b-4b13-824f-71f00191dcca';

    var newFeedsVal;

    function newFeeds(feeds) {
        if (feeds !== undefined) {
            return newFeedsVal = feeds;
        } else {
            return newFeedsVal;
        }
    }

    var newsReadedVal = false;

    function newsReaded(readed) {
        if (readed !== undefined) {
            return newsReadedVal = readed;
        } else {
            return newsReadedVal;
        }
    }

    var newsRequestedVal = false;

    function newsRequested(requested) {
        if (requested !== undefined) {
            return newsRequestedVal = requested;
        } else {
            return newsRequestedVal;
        }
    }

    function getNewFeedsCount() {
        Teamlab.getNewFeedsCount({}, { filter: {}, success: onGetNewFeedsCount });
    }

    function onGetNewFeedsCount(params, newsCount) {
        var $feedLink = jq('.top-item-box.feed');
        if (newsCount == 0) {
            $feedLink.removeClass('has-led');
            $feedLink.find('.inner-label').text(newsCount);
        } else {
            if (newsCount > 100) {
                newsCount = '>100';
            }
            $feedLink.addClass('has-led');
            $feedLink.find('.inner-label').text(newsCount);
            newFeedsVal = true;
        }
    }

    function getDropFeeds() {
        var filter = {
            onlyNew: true
        };

        Teamlab.getFeeds({}, { filter: filter, success: onGetDropFeeds });
    }

    function onGetDropFeeds(params, response) {
        var feeds = response.feeds,
            $dropFeedsBox = jq('#drop-feeds-box'),
            $loader = $dropFeedsBox.find('.loader-text-block'),
            $feedsReadedMsg = $dropFeedsBox.find('.feeds-readed-msg'),
            $seeAllBtn = $dropFeedsBox.find('.see-all-btn'),
            dropFeedsList = $dropFeedsBox.find('.list');

        if (!feeds.length) {
            $loader.hide();
            $feedsReadedMsg.show();
            $seeAllBtn.css('display', 'inline-block');
            return;
        }

        dropFeedsList.empty();

        for (var i = 0; i < feeds.length; i++) {
            try {
                var template = getFeedTemplate(feeds[i]);
                jq.tmpl('dropFeedTmpl', template).appendTo(dropFeedsList);
            } catch(e) {
                console.log(e);
            }
        }

        $loader.hide();
        jq(dropFeedsList).removeClass('display-none');
        $seeAllBtn.css('display', 'inline-block');
    }

    function getFeedTemplate(feed) {
        var template = feed;

        var authorId = getFeedAuthor(feed);
        template.author = getUser(authorId);

        template.isGuest = template.author == null || template.authorId == guestId;
        template.productText = getFeedProductText(template);

        if (!template.location) {
            template.location = getFeedLocation(template);
        }

        resolveAdditionalFeedData(template);
        template.actionText = getFeedActionText(template);

        return template;
    }

    function getFeedAuthor(feed) {
        var authorId;
        if (feed.comments && feed.comments.length) {
            authorId = feed.comments[feed.comments.length - 1].authorId;
        } else {
            authorId = feed.authorId;
        }

        return authorId;
    }

    function getUser(id) {
        if (!id) {
            return null;
        }

        var users = ASC.Resources.Master.ApiResponses_Profiles.response;
        if (!users) {
            return null;
        }

        for (var j = 0; j < users.length; j++) {
            if (users[j].id == id) {
                return users[j];
            }
        }

        return null;
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
            case 'blog':
                template.itemClass = 'blogs';
                break;
            case 'news':
            case 'order':
            case 'advert':
            case 'poll':
                template.itemClass = 'events';
                break;
            case 'forum':
            case 'forumPoll':
                template.itemClass = 'forum';
                break;
            case 'bookmark':
                template.itemClass = 'bookmarks';
                break;
            case 'company':
            case 'person':
                template.itemClass = 'group';
                break;
            case 'crmTask':
                template.itemClass = 'tasks';
                break;
            case 'deal':
                template.itemClass = 'opportunities';
                break;
            case 'cases':
                template.itemClass = 'cases';
                break;
            case 'project':
                template.itemClass = 'projects';
                break;
            case 'participant':
                template.itemClass = 'projects';
                break;
            case 'milestone':
                template.itemClass = 'milestones';
                break;
            case 'task':
                template.itemClass = 'tasks';
                break;
            case 'discussion':
                template.itemClass = 'discussions';
                break;
            case 'file':
            case 'sharedFile':
                template.itemClass = 'documents';
                break;
            case 'folder':
            case 'sharedFolder':
                template.itemClass = 'documents';
                break;
        }
    }

    return {
        getNewFeedsCount: getNewFeedsCount,
        onGetNewFeedsCount: onGetNewFeedsCount,
        getDropFeeds: getDropFeeds,

        newFeeds: newFeeds,
        newsRequested: newsRequested,
        newsReaded: newsReaded
    };
}();

jq(document).ready(function() {
    jq.dropdownToggle({
        switcherSelector: '.studio-top-panel .feedActiveBox',
        dropdownID: 'studio_dropFeedsPopupPanel',
        addTop: 5,
        addLeft: -392
    });

    jq('.studio-top-panel .feedActiveBox').on('mouseup', function(event) {
        if (event.which == 2 && FeedReader.newFeeds()) {
            FeedReader.newFeeds(false);
            jq('.top-item-box.feed').removeClass('has-led');
        }
    });

    jq('.studio-top-panel .feedActiveBox').on('click', function(event) {
        if (FeedReader.newsRequested()) {
            return false;
        }

        if (event.which == 2 && FeedReader.newFeeds()) {
            FeedReader.newFeeds(false);
            jq('.top-item-box.feed').removeClass('has-led');
            return true;
        }
        if (event.which != 1) {
            return true;
        }

        if (FeedReader.newFeeds()) {
            if (!FeedReader.newsReaded()) {
                FeedReader.newsRequested(true);

                FeedReader.getDropFeeds();
                Teamlab.readFeeds({}, {
                    filter: {},
                    success: function (params, readed) {
                        try {
                            if (ASC.Resources.Master.Hub.Url) {
                                // sendFeedsCount
                                jq.connection.ch.server.sfc();
                            }
                        } catch (e) {
                            console.error(e.message);
                        }
                        if (readed) {
                            FeedReader.newsRequested(false);
                            FeedReader.newsReaded(true);
                            jq('.top-item-box.feed').removeClass('has-led');
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
    if (!ASC.Resources.Master.Hub.Url) {
        setTimeout(function () {
            FeedReader.getNewFeedsCount();
        }, 3000);
    }
});