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

if (typeof (ASC) === 'undefined') {
     ASC = {};
}

if (typeof (ASC.Feed) === 'undefined') {
    ASC.Feed = {};
}

ASC.Feed = (function () {
    var feedResource = ASC.Resources.Master.FeedResource;

    function createTextsObj(createdText, location, commentedText) {
        var result = {
            createdText: createdText,
            location: location
        }

        if (typeof commentedText !== "undefined") {
            result.commentedText = commentedText;
        }

        return result;
    }

    return jq.extend({
            Products: {
                community: feedResource.CommunityProduct,
                projects: feedResource.ProjectsProduct,
                crm: feedResource.CrmProduct,
                documents: feedResource.DocumentsProduct
            },
            Texts: {
                blog: createTextsObj(feedResource.BlogCreatedText, feedResource.BlogsModule, feedResource.BlogCommentedText),
                bookmark: createTextsObj(feedResource.BookmarkCreatedText, feedResource.BookmarksModule, feedResource.BookmarkCommentedText),
                news: createTextsObj(feedResource.NewsCreatedText, feedResource.EventsModule, feedResource.NewsCommentedText),
                order: createTextsObj(feedResource.OrderCreatedText, feedResource.EventsModule, feedResource.OrderCommentedText),
                advert: createTextsObj(feedResource.AdvertCreatedText, feedResource.EventsModule, feedResource.AdvertCommentedText),
                poll: createTextsObj(feedResource.PollCreatedText, feedResource.EventsModule, feedResource.PollCommentedText),
                forumPost: createTextsObj(feedResource.ForumPostCreatedText, feedResource.ForumsModule),
                forumTopic: createTextsObj(feedResource.ForumTopicCreatedText, feedResource.ForumsModule),
                forumPoll: createTextsObj(feedResource.ForumPollCreatedText, feedResource.ForumsModule),
                company: createTextsObj(feedResource.CompanyCreatedText, feedResource.ContactsModule),
                person: createTextsObj(feedResource.PersonCreatedText, feedResource.ContactsModule),
                crmTask: {
                    createdText: feedResource.CrmTaskCreatedText,
                    extraText: feedResource.Responsible,
                    extraText2: feedResource.Contact,
                    location: feedResource.CrmTaskModule
                },
                deal: {
                    createdText: feedResource.DealCreatedText,
                    extraText2: feedResource.Contact,
                    location: feedResource.OpportunitiesModule
                },
                cases: createTextsObj(feedResource.CaseCreatedText, feedResource.CasesModule),
                project: {
                    createdText: feedResource.ProjectCreatedText,
                    extraText: feedResource.ProjectManager
                },
                participant: {
                    createdText: feedResource.ParticipantCreatedText
                },
                milestone: {
                    createdText: feedResource.MilestoneCreatedText,
                    extraText: feedResource.Project,
                    location: feedResource.MilestonesModule
                },
                task: {
                    createdText: feedResource.TaskCreatedText,
                    commentedText: feedResource.TaskCommentedText,
                    extraText: feedResource.Project,
                    extraText2: feedResource.Responsibles,
                    location: feedResource.TasksModule
                },
                discussion: {
                    createdText: feedResource.DiscussionCreatedText,
                    commentedText: feedResource.DiscussionCommentedText,
                    location: feedResource.DiscussionsModule
                },
                file: {
                    createdText: feedResource.FileCreatedText,
                    createdExtraText: feedResource.FileCreatedInFolderText,
                    updatedText: feedResource.FileUpdatedText,
                    updatedExtraText: feedResource.FileUpdatedInFolderText,
                    extraText: feedResource.Size,
                    location: feedResource.FilesModule
                },
                sharedFile: {
                    createdText: feedResource.SharedFileCreatedText,
                    createdExtraText: feedResource.SharedFileCreatedInFolderText,
                    extraText: feedResource.Size,
                    location: feedResource.FilesModule
                },
                folder: {
                    createdText: feedResource.FolderCreatedText,
                    createdExtraText: feedResource.FolderCreatedInFolderText,
                    location: feedResource.FoldersModule
                },
                sharedFolder: createTextsObj(feedResource.SharedFolderCreatedText, feedResource.FoldersModule)
            }
        }, ASC.Feed);
})();


ASC.Feed.Reader = (function() {
    var guestId = '712d9ec3-5d2b-4b13-824f-71f00191dcca';

    var newFeedsVal;

    var socket;

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
        Teamlab.getNewFeedsCount({}, { filter: {}, success: onGetNewFeedsCount, async: true });
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
        template.author = window.UserManager.getUser(authorId);

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

    function getFeedProductText(template) {
        var productsCollection = ASC.Feed.Products;
        if (!productsCollection) {
            return null;
        }

        return productsCollection[template.product];
    }

    function getFeedLocation(template) {
        var textsColection = ASC.Feed.Texts;
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
        var textsCollection = ASC.Feed.Texts;
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

    function init() {
        if (jq("#studioPageContent li.feed a.feedActiveBox").length == 0)
            return;

        if (ASC.SocketIO && !ASC.SocketIO.disabled()) {
            socket = ASC.SocketIO.Factory.counters
                .reconnect_failed(function () {
                   getNewFeedsCount();
                })
                .on('sendFeedsCount', function (counts) {
                    var currentCounts = Number(jq('.top-item-box.feed').find('.inner-label').text());
                    if (!newsReadedVal && currentCounts) {
                        counts = currentCounts + counts;
                    }
                    newsReadedVal = false;
                    onGetNewFeedsCount(null, counts);
                })
                .on('getNewMessagesCount', function (counts) {
                    onGetNewFeedsCount(null, counts.f);
                });
        }

        jq.dropdownToggle({
            switcherSelector: '.studio-top-panel .feedActiveBox',
            dropdownID: 'studio_dropFeedsPopupPanel',
            addTop: 5,
            addLeft: -392
        });

        jq('.studio-top-panel .feedActiveBox').on('mouseup', function (event) {
            if (event.which == 2 && newFeeds()) {
                newFeeds(false);
                jq('.top-item-box.feed').removeClass('has-led');
            }
        });

        jq('.studio-top-panel .feedActiveBox').on('click', function (event) {
            if (newsRequested()) {
                return false;
            }

            if (event.which == 2 && newFeeds()) {
                newFeeds(false);
                jq('.top-item-box.feed').removeClass('has-led');
                return true;
            }
            if (event.which != 1) {
                return true;
            }

            if (newFeeds()) {
                if (!newsReaded()) {
                    newsRequested(true);

                    getDropFeeds();
                    Teamlab.readFeeds({}, {
                        filter: {},
                        success: function (params, readed) {
                            try {
                                if (socket) {
                                    socket.emit('sendFeedsCount');
                                }
                            } catch (e) {
                                console.error(e.message);
                            }
                            if (readed) {
                                newsRequested(false);
                                newsReaded(true);
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
            StudioManager.addPendingRequest(getNewFeedsCount);
        }
    }

    return {
        init: init
    };
})();

jq(document).ready(function () {
    if (jq("#studioPageContent li.feed a.feedActiveBox").length == 0)
        return;

    ASC.Feed.Reader.init();
});