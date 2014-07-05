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


;window.TeamlabMobile = (function(TeamlabMobile) {
    if (!TeamlabMobile) {
        console.log('Teamlab.default: has no TeamlabMobile');
        return DefaultMobile;
    }

    var 
    loadedCommItems = { blogs: 0, forums: 0, events: 0, bookmarks: 0 },
    templateData = {
        modules: [
        {
            id: null,
            type: 'community-page',
            title: ASC.Resources.BtnAll,
            link: 'community',
            classname: 'filter-item all',
            enabled: true
        },
        {
            id: '6a598c74-91ae-437d-a5f4-ad339bd11bb2',
            type: 'community-page-blogs',
            title: ASC.Resources.BtnBlogs,
            link: 'community/blogs',
            classname: 'filter-item blogs',
            enabled: true
        },
        {
            id: '853b6eb9-73ee-438d-9b09-8ffeedf36234',
            type: 'community-page-forums',
            title: ASC.Resources.BtnForums,
            link: 'community/forums',
            classname: 'filter-item forums',
            enabled: true
        },
        {
            id: '28b10049-dd20-4f54-b986-873bc14ccfc7',
            type: 'community-page-bookmarks',
            title: ASC.Resources.BtnBookmarks,
            link: 'community/bookmarks',
            classname: 'filter-item bookmarks',
            enabled: true
        },
        {
            id: '3cfd481b-46f2-4a4a-b55c-b8c0c9def02c',
            type: 'community-page-events',
            title: ASC.Resources.BtnEvents,
            link: 'community/events',
            classname: 'filter-item events',
            enabled: true
        }
      ]
    },
    templateIds = {
        lbnewcommitems: 'template-comm-timeline',
        pgcommunity: 'template-page-community',
        pgcommblog: 'template-page-community-blog',
        pgcommforum: 'template-page-community-forum',
        pgcommevent: 'template-page-community-event',
        pgcommbookmark: 'template-page-community-bookmark',
        pgcommaddblog: 'template-community-addblog',
        pgcommaddforum: 'template-community-addforum',
        pgcommaddevent: 'template-community-addevent',
        pgcommaddbookmark: 'template-community-addbookmark'
    },
    staticAnchors = {
        blog: 'community/blog/',
        forum: 'community/forum/',
        event: 'community/event/',
        bookmark: 'community/bookmark/',
        blogs: 'community/blogs',
        forums: 'community/forums',
        events: 'community/events',
        bookmarks: 'community/bookmarks'
    },
    anchorRegExp = {
        
        community: /^community[\/]*$/,
        cmt_blog: /^community\/blog\/([\w\d-]+)$/,
        cmt_forum: /^community\/forum\/([\w\d-]+)$/,
        cmt_event: /^community\/event\/([\w\d-]+)$/,
        cmt_bookmark: /^community\/bookmark\/([\w\d-]+)$/,
        cmt_blogs: /^community\/blogs[\/]*$/,
        cmt_forums: /^community\/forums[\/]*$/,
        cmt_events: /^community\/events[\/]*$/,
        cmt_bookmarks: /^community\/bookmarks[\/]*$/,
        cmt_addblog: /^community\/blogs\/add[\/]*$/,
        cmt_addforum: /^community\/forums\/add[\/]*$/,
        cmt_addevent: /^community\/events\/add[\/]*$/,
        cmt_addbookmark: /^community\/bookmarks\/add[\/]*$/,
        events: /^community\/event/,
        bookmarks: /^community\/bookmark/,
        blogs: /^community\/blog/,
        forums: /^community\/forum/
    },
    customEvents = {
        changePage: 'onchangepage',
        addComment: 'onaddcomment',
        loadComments: 'onloadcomments',
        
        addCommunityComment: 'onaddcommunitycomment',
        communityPage: 'oncommunitypage',
        blogPage: 'onblogpage',
        forumPage: 'onforumpage',
        eventPage: 'oneventpage',
        bookmarkPage: 'onbookmarkpage',
        addBlogPage: 'onaddblogpage',
        addForumPage: 'onaddforumpage',
        addEventPage: 'onaddeventpage',
        addBookmarkPage: 'onaddbookmarkpage',
        blogsPage: 'onblogspage',
        forumsPage: 'onforumspage',
        eventsPage: 'oneventspage',
        bookmarksPage: 'onbookmarkspage',
        threadsPage: 'onthreadspage',
        loadTimelineCommItems: 'onloadtimelinecommitems',
        addBlog: 'onaddblog',
        addForum: 'onaddforum',
        addEvent: 'onaddevent',
        addBookmark: 'onaddbookmark',
        getThreads: 'ongetthreads'
    },
    eventManager = TeamlabMobile.extendEventManager(customEvents);

    TeamlabMobile.extendModule(templateIds, anchorRegExp, staticAnchors);

    
    ASC.Controls.AnchorController.bind(anchorRegExp.forums, onSecurityCallback);
    ASC.Controls.AnchorController.bind(anchorRegExp.events, onSecurityCallback);
    ASC.Controls.AnchorController.bind(anchorRegExp.blogs, onSecurityCallback);
    ASC.Controls.AnchorController.bind(anchorRegExp.bookmarks, onSecurityCallback);
    ASC.Controls.AnchorController.bind(anchorRegExp.community, onCommunityAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.cmt_blogs, onBlogsAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.cmt_forums, onForumsAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.cmt_events, onEventsAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.cmt_bookmarks, onBookmarksAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.cmt_blog, onBlogAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.cmt_forum, onForumAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.cmt_event, onEventAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.cmt_bookmark, onBookmarkAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.cmt_addblog, onAddBlogAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.cmt_addforum, onAddForumAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.cmt_addevent, onAddEventAnch);
    ASC.Controls.AnchorController.bind(anchorRegExp.cmt_addbookmark, onAddBookmarkAnch);


    
    function getModulesSecurityInfo(items) {
        var modules = {
            all: templateData.modules[0],
            blogs: templateData.modules[1],
            forums: templateData.modules[2],
            bookmarks: templateData.modules[3],
            events: templateData.modules[4]
        };

        for (var fld in modules) {
            if (modules.hasOwnProperty(fld)) {
                var id = modules[fld].id;
                var itemsInd = items.length;
                while (itemsInd--) {
                    if (items[itemsInd].webItemId == id) {
                        modules[fld].enabled = items[itemsInd].enabled === true;
                    }
                }
            }
        }
        return modules;
    }


    
    function onCommunityAnch(params) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
            return undefined;
        }
        eventManager.call(customEvents.changePage, window, []);

        TeamlabMobile.getCommItems('all');
    }

    function onBlogsAnch(params) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
            return undefined;
        }
        eventManager.call(customEvents.changePage, window, []);

        TeamlabMobile.getCommItems('blog');
    }

    function onForumsAnch(params) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
            return undefined;
        }
        eventManager.call(customEvents.changePage, window, []);

        TeamlabMobile.getCommItems('forum');
    }

    function onEventsAnch(params) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
            return undefined;
        }
        eventManager.call(customEvents.changePage, window, []);

        TeamlabMobile.getCommItems('event');
    }

    function onBookmarksAnch(params) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
            return undefined;
        }
        eventManager.call(customEvents.changePage, window, []);

        TeamlabMobile.getCommItems('bookmark');
    }

    function onBlogAnch(params, id) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
            return undefined;
        }
        if (id) {
            eventManager.call(customEvents.changePage, window, []);
            Teamlab.joint()
        .getCmtBlog(null, id)
        .getCmtBlogComments(null, id)
        .start(null, onGetBlog);
        }
    }

    function onForumAnch(params, id) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
            return undefined;
        }
        if (id) {
            eventManager.call(customEvents.changePage, window, []);
            Teamlab.getCmtForumTopic(null, id, onGetForum);
        }
    }

    function onEventAnch(params, id) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
            return undefined;
        }
        if (id) {
            eventManager.call(customEvents.changePage, window, []);
            Teamlab.joint()
        .getCmtEvent(null, id)
        .getCmtEventComments(null, id)
        .start(null, onGetEvent);
        }
    }

    function onBookmarkAnch(params, id) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
            return undefined;
        }
        if (id) {
            params = params || {};
            eventManager.call(customEvents.changePage, window, []);
            Teamlab.joint()
        .getCmtBookmark(null, id)
        .getCmtBookmarkComments(null, id)
        .start(null, onGetBookmark);
        }
    }

    function onAddBlogAnch(params) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
            return undefined;
        }
        eventManager.call(customEvents.changePage, window, []);
        eventManager.call(customEvents.addBlogPage, window, []);
    }

    function onAddForumAnch(params) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
            return undefined;
        }
        eventManager.call(customEvents.changePage, window, []);
        eventManager.call(customEvents.addForumPage, window, []);
    }

    function onAddEventAnch(params) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
            return undefined;
        }
        eventManager.call(customEvents.changePage, window, []);
        eventManager.call(customEvents.addEventPage, window, []);
    }

    function onAddBookmarkAnch(params) {
        if (!TeamlabMobile.validSession()) {
            ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
            return undefined;
        }
        eventManager.call(customEvents.changePage, window, []);
        eventManager.call(customEvents.addBookmarkPage, window, []);
    }

    function onGetCommForumItems(params, securityitems, forums, categories) {
        var 
      items = [],
      enabledmodules = [],
      modules = getModulesSecurityInfo(securityitems);

        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.forums) && modules.forums.enabled === false) {
            document.location.href = "./";
        }

        items = items = modules.forums.enabled === true && forums ? items.concat(forums) : items;
        items.sort(TeamlabMobile.dscSortByDate);

        var allLoaded = items.length <= TeamlabMobile.constants.pageItems;
        if (items.length > TeamlabMobile.constants.pageItems) {
            items.splice(TeamlabMobile.constants.pageItems, items.length);
        }

        var hasThreads = false;
        if (categories.length > 0) {
            var categoriesInd = categories.length;
            while (categoriesInd--) {
                if (categories[categoriesInd].threads && categories[categoriesInd].threads.length > 0) {
                    hasThreads = true;
                    break;
                }
            }
        }

        var 
      item = null,
      itemsInd = items.length;
        while (itemsInd--) {
            item = items[itemsInd];
            item.classname = item.type;
            item.href = 'community/' + item.type + '/' + item.id;
        }

        loadedCommItems.forums = items.length;

        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.cmt_forums)) {
            var activemodules = [].concat(templateData.modules);

            for (var fld in modules) {
                if (modules.hasOwnProperty(fld)) {
                    var moduleid = modules[fld].id;
                    var activemodulesInd = activemodules.length;
                    while (activemodulesInd--) {
                        if (activemodules[activemodulesInd].id == moduleid) {
                            if (modules[fld].enabled !== true) {
                                activemodules.splice(activemodulesInd, 1);
                            }
                            break;
                        }
                    }
                }
            }

            eventManager.call(customEvents.forumsPage, window, [items, activemodules, allLoaded, hasThreads]);
        }
    }

    function onGetCommItems(params, securityitems, blogs, forums, events, bookmarks) {
        var 
      items = [],
      enabledmodules = [],
      redirect,
      modules = getModulesSecurityInfo(securityitems);

        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.events) && modules.events.enabled === false) {
            redirect = true;
        }
        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.forums) && modules.forums.enabled === false) {
            redirect = true;
        }
        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.bookmarks) && modules.bookmarks.enabled === false) {
            redirect = true;
        }
        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.blogs) && modules.blogs.enabled === false) {
            redirect = true;
        }

        if (redirect) {
            document.location.href = "./";
        }

        items = modules.blogs.enabled === true && blogs ? items.concat(blogs) : items;
        items = modules.forums.enabled === true && forums ? items.concat(forums) : items;
        items = modules.events.enabled === true && events ? items.concat(events) : items;
        items = modules.bookmarks.enabled === true && bookmarks ? items.concat(bookmarks) : items;
        items.sort(TeamlabMobile.dscSortByDate);

        var allLoaded = items.length <= TeamlabMobile.constants.pageItems;
        if (items.length > TeamlabMobile.constants.pageItems) {
            items.splice(TeamlabMobile.constants.pageItems, items.length);
        }

        loadedCommItems.blogs = 0;
        loadedCommItems.forums = 0;
        loadedCommItems.events = 0;
        loadedCommItems.bookmarks = 0;

        params = (params && params.length ? params[params.length - 1] : {});

        var 
      customEvent = params.hasOwnProperty('customEvent') ? params.customEvent : null,
      type = params.hasOwnProperty('type') ? params.type : null,
      item = null,
      itemsInd = items.length;
        while (itemsInd--) {
            item = items[itemsInd];
            item.classname = item.type;
            item.href = 'community/' + item.type + '/' + item.id;

            switch (item.type) {
                case 'blog': loadedCommItems.blogs += 1; break;
                case 'forum': loadedCommItems.forums += 1; break;
                case 'event': loadedCommItems.events += 1; break;
                case 'bookmark': loadedCommItems.bookmarks += 1; break;
            }
        }

        var isCorrectAnchor = false;
        switch (customEvent) {
            case customEvents.communityPage:
                isCorrectAnchor = ASC.Controls.AnchorController.testAnchor(anchorRegExp.community);
                break;
            case customEvents.blogsPage:
                isCorrectAnchor = ASC.Controls.AnchorController.testAnchor(anchorRegExp.cmt_blogs);
                break;
            case customEvents.forumsPage:
                isCorrectAnchor = ASC.Controls.AnchorController.testAnchor(anchorRegExp.cmt_forums);
                break;
            case customEvents.eventsPage:
                isCorrectAnchor = ASC.Controls.AnchorController.testAnchor(anchorRegExp.cmt_events);
                break;
            case customEvents.bookmarksPage:
                isCorrectAnchor = ASC.Controls.AnchorController.testAnchor(anchorRegExp.cmt_bookmarks);
                break;
        }

        if (isCorrectAnchor) {
            var activemodules = [].concat(templateData.modules);

            for (var fld in modules) {
                if (modules.hasOwnProperty(fld)) {
                    var moduleid = modules[fld].id;
                    var activemodulesInd = activemodules.length;
                    while (activemodulesInd--) {
                        if (activemodules[activemodulesInd].id == moduleid) {
                            if (modules[fld].enabled !== true) {
                                activemodules.splice(activemodulesInd, 1);
                            }
                            break;
                        }
                    }
                }
            }

            eventManager.call(customEvent, window, [items, activemodules, allLoaded]);
        }
    }

    function onGetMoreCommItems(params, securityitems, blogs, forums, events, bookmarks) {
        var 
        items = [],
        modules = getModulesSecurityInfo(securityitems);

        items = modules.blogs.enabled === true && blogs ? items.concat(blogs) : items;
        items = modules.forums.enabled === true && forums ? items.concat(forums) : items;
        items = modules.events.enabled === true && events ? items.concat(events) : items;
        items = modules.bookmarks.enabled === true && bookmarks ? items.concat(bookmarks) : items;
        items.sort(TeamlabMobile.dscSortByDate);

        var allLoaded = items.length <= TeamlabMobile.constants.pageItems;
        if (items.length > TeamlabMobile.constants.pageItems) {
            items.splice(TeamlabMobile.constants.pageItems, items.length);
        }

        params = (params && params.length ? params[params.length - 1] : {});

        var 
      type = params.hasOwnProperty('type') ? params.type : null,
      item = null,
      itemsInd = items.length;

        while (itemsInd--) {
            item = items[itemsInd];
            item.classname = item.type;
            item.href = 'community/' + item.type + '/' + item.id;

            switch (item.type) {
                case 'blog': loadedCommItems.blogs += 1; break;
                case 'forum': loadedCommItems.forums += 1; break;
                case 'event': loadedCommItems.events += 1; break;
                case 'bookmark': loadedCommItems.bookmarks += 1; break;
            }
        }

        eventManager.call(customEvents.loadTimelineCommItems, window, [type, items, allLoaded]);
    }

    function onGetForumCategories(params, items) {
        var threads = [];
        for (var i = 0, n = items.length; i < n; i++) {
            threads = threads.concat(items[i].threads);
        }

        items = threads;
        var 
      item = null,
      itemsInd = items.length;
        while (itemsInd--) {
            item = items[itemsInd];
            item.classname = item.type;
            item.href = 'community/' + item.type + '/' + item.id;
        }

        eventManager.call(customEvents.getThreads, window, [items]);
    }

    function onAddBlog(params, item) {
        item.classname = item.type;

        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.cmt_addblog)) {
            ASC.Controls.AnchorController.lazymove({ back: TeamlabMobile.anchors.blogs }, TeamlabMobile.anchors.blog + item.id);
        }
    }

    function onGetBlog(params, item, comments) {
        item.classname = item.type;
        item.comments = comments;

        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.cmt_blog)) {
            params.back = TeamlabMobile.regexps.community.test(ASC.Controls.AnchorController.getAnchor()) ? ASC.Controls.AnchorController.getAnchor() : null;
            eventManager.call(customEvents.blogPage, window, [item, params]);
        }
    }

    function onAddForum(params, item) {
        item.classname = item.type;

        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.cmt_addforum)) {
            ASC.Controls.AnchorController.lazymove({ back: TeamlabMobile.anchors.forums }, TeamlabMobile.anchors.forum + item.id);
        }
    }

    function onGetForum(params, item) {
        item.classname = item.type;

        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.cmt_forum)) {
            params.back = TeamlabMobile.regexps.community.test(ASC.Controls.AnchorController.getAnchor()) ? ASC.Controls.AnchorController.getAnchor() : null;
            eventManager.call(customEvents.forumPage, window, [item, params]);
        }
    }

    function onAddEvent(params, item) {
        item.classname = item.type;

        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.cmt_addevent)) {
            ASC.Controls.AnchorController.lazymove({ back: TeamlabMobile.anchors.events }, TeamlabMobile.anchors.event + item.id);
        }
    }

    function onGetEvent(params, item, comments) {
        item.classname = item.type;
        item.comments = comments;

        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.cmt_event)) {
            params.back = TeamlabMobile.regexps.community.test(ASC.Controls.AnchorController.getAnchor()) ? ASC.Controls.AnchorController.getAnchor() : null;
            eventManager.call(customEvents.eventPage, window, [item, params]);
        }
    }

    function onAddBookmark(params, item) {
        item.classname = item.type;

        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.cmt_addbookmark)) {
            ASC.Controls.AnchorController.lazymove({ back: TeamlabMobile.anchors.bookmarks }, TeamlabMobile.anchors.bookmark + item.id);
        }
    }

    function onGetBookmark(params, item, comments) {
        item.classname = item.type;
        item.comments = comments;

        if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.cmt_bookmark)) {
            params.back = TeamlabMobile.regexps.community.test(ASC.Controls.AnchorController.getAnchor()) ? ASC.Controls.AnchorController.getAnchor() : null;
            eventManager.call(customEvents.bookmarkPage, window, [item, params]);
        }
    }

    function onAddComment(params, comment) {
        eventManager.call(customEvents.addCommunityComment, window, [comment, params]);
        eventManager.call(customEvents.addComment, window, [comment, params]);
    }

    
    TeamlabMobile.getThreads = function() {
        Teamlab.getCmtForumCategories(null, onGetForumCategories);
    };

    TeamlabMobile.hasMoreCommItems = function(type) {
        return true;
    };

    TeamlabMobile.getCommItems = function(type) {

        switch (type) {
            case 'all':
                Teamlab.joint()
          .getWebItemSecurityInfo(null, null)
          .getCmtBlogs(null, { filter: { startIndex: 0, count: TeamlabMobile.constants.pageItems + 1} })
          .getCmtForumTopics(null, { filter: { startIndex: 0, count: TeamlabMobile.constants.pageItems + 1} })
          .getCmtEvents(null, { filter: { startIndex: 0, count: TeamlabMobile.constants.pageItems + 1} })
          .getCmtBookmarks(null, { filter: { startIndex: 0, count: TeamlabMobile.constants.pageItems + 1} })
          .start({ type: type, customEvent: customEvents.communityPage }, onGetCommItems);
                break;
            case 'blog':
                Teamlab.joint()
          .getWebItemSecurityInfo(null, null)
          .getCmtBlogs(null, { filter: { startIndex: 0, count: TeamlabMobile.constants.pageItems + 1} })
          .start({ type: type, customEvent: customEvents.blogsPage }, onGetCommItems);
                break;
            case 'forum':
                Teamlab.joint()
          .getWebItemSecurityInfo(null, null)
          .getCmtForumTopics(null, { filter: { startIndex: 0, count: TeamlabMobile.constants.pageItems + 1} })
          .getCmtForumCategories(null)
          .start({ type: type, customEvent: customEvents.forumsPage }, onGetCommForumItems);
                break;
            case 'event':
                Teamlab.joint()
          .getWebItemSecurityInfo(null, null)
          .getCmtEvents(null, { filter: { startIndex: 0, count: TeamlabMobile.constants.pageItems + 1} })
          .start({ type: type, customEvent: customEvents.eventsPage }, onGetCommItems);
                break;
            case 'bookmark':
                Teamlab.joint()
          .getWebItemSecurityInfo(null, null)
          .getCmtBookmarks(null, { filter: { startIndex: 0, count: TeamlabMobile.constants.pageItems + 1} })
          .start({ type: type, customEvent: customEvents.bookmarksPage }, onGetCommItems);
                break;
        }
    };

    TeamlabMobile.getMoreCommItems = function(type) {
        var index = -1, customEvent = null, fn = null;
        switch (type) {
            case 'all':
                Teamlab.joint()
          .getWebItemSecurityInfo(null, null)
          .getCmtBlogs(null, { filter: { startIndex: loadedCommItems.blogs, count: TeamlabMobile.constants.pageItems + 1} })
          .getCmtForumTopics(null, { filter: { startIndex: loadedCommItems.forums, count: TeamlabMobile.constants.pageItems + 1} })
          .getCmtEvents(null, { filter: { startIndex: loadedCommItems.events, count: TeamlabMobile.constants.pageItems + 1} })
          .getCmtBookmarks(null, { filter: { startIndex: loadedCommItems.bookmarks, count: TeamlabMobile.constants.pageItems + 1} })
          .start({ type: type, customEvent: customEvents.loadTimelineCommItems }, onGetMoreCommItems);
                break;
            case 'blog':
                fn = Teamlab.getCmtBlogs;
                index = loadedCommItems.blogs;
                customEvent = customEvents.loadTimelineCommItems;
                break;
            case 'forum':
                fn = Teamlab.getCmtForumTopics;
                index = loadedCommItems.forums;
                customEvent = customEvents.loadTimelineCommItems;
                break;
            case 'event':
                fn = Teamlab.getCmtEvents;
                index = loadedCommItems.events;
                customEvent = customEvents.loadTimelineCommItems;
                break;
            case 'bookmark':
                fn = Teamlab.getCmtBlogs;
                index = loadedCommItems.bookmarks;
                customEvent = customEvents.loadTimelineCommItems;
                break;
        }

        if (index !== -1 && fn && customEvent) {
            Teamlab.joint();
            Teamlab.getWebItemSecurityInfo(null, null);
            fn(null, { filter: { startIndex: index, count: TeamlabMobile.constants.pageItems + 1} });
            Teamlab.start({ type: type, customEvent: customEvent }, onGetMoreCommItems);
        }
    };

    TeamlabMobile.addCommunityComment = function(type, id, data) {
        var fn = null, callback = null;
        switch (type) {
            case 'blog':
                fn = Teamlab.addCmtBlogComment;
                callback = onAddComment;
                break;
            case 'forum':
                fn = Teamlab.addCmtForumTopicPost;
                callback = onAddComment;
                break;
            case 'event':
                fn = Teamlab.addCmtEventComment;
                callback = onAddComment;
                break;
            case 'bookmark':
                fn = Teamlab.addCmtBookmarkComment;
                callback = onAddComment;
                break;
        }

        if (fn && callback) {
            return fn({ type: type }, id, data, callback);
        }
        return false;
    };

    TeamlabMobile.addCommunityItem = function(type, data) {
        var fn = null, callback = null;
        switch (type) {
            case 'blog':
                fn = Teamlab.addCmtBlog;
                callback = onAddBlog;
                break;
            case 'forum':
                fn = Teamlab.addCmtForumTopic;
                callback = onAddForum;
                break;
            case 'event':
                fn = Teamlab.addCmtEvent;
                callback = onAddEvent;
                break;
            case 'bookmark':
                fn = Teamlab.addCmtBookmark;
                callback = onAddBookmark;
                break;
        }

        if (fn && callback) {
            return fn(null, data, callback);
        }
        return false;
    };
    
    function onSecurityCallback(params) {
        var mdls = templateData.modules,
            mdlsInd = mdls.length,
            mdlsIds = [],
            redirect;
        while (mdlsInd--) {
            if (mdls[mdlsInd].id !== null) {
                mdlsIds.unshift(mdls[mdlsInd].id);
                continue;
            }
        }
        Teamlab.getWebItemSecurityInfo({}, mdlsIds, { success: function(params, modules) {
            for (var i = 0; i < mdls.length; i++) {
                for (var j = 0; j < modules.length; j++) {
                    if (mdls[i].id == modules[j].webItemId && modules[j].enabled === false) {
                        var name = mdls[i].link;
                        switch (name) {
                            case "community/blogs":
                                if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.blogs)) {
                                    redirect = true;
                                }
                                break;
                            case "community/forums":
                                if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.forums)) {
                                    redirect = true;
                                }
                                break;
                            case "community/bookmarks":
                                if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.bookmarks)) {
                                    redirect = true;
                                }
                                break;
                            case "community/events":
                                if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.events)) {
                                    redirect = true;
                                }
                                break;                            
                        }
                        if (redirect) {
                            document.location.href = "./";
                        }                        
                    }
                }
            }
        }
        });
    }


    return TeamlabMobile;
})(TeamlabMobile);
