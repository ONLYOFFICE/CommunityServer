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


;window.DefaultMobile = (function (DefaultMobile) {
  if (!DefaultMobile) {
    console.log('Default.community: has no DefaultMobile');
    return DefaultMobile;
  }

  function getCommentData ($page) {
    var 
      data = {
        commenttype : $page.find('input.comment-type:first').removeClass('error-field').val(),
        id          : $page.find('input.comment-id:first').removeClass('error-field').val(),
        parentid    : $page.find('input.comment-parentid:first').removeClass('error-field').val(),
        subject     : $page.find('input.comment-subject:first').removeClass('error-field').val(),
        content     : $page.find('textarea.comment-content:first').removeClass('error-field').val()
      };

    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        switch (fld) {
          case 'subject':
            continue;
        }
        data[fld] = TeamlabMobile.verificationValue(data[fld]);
      }
    }

    data.parentid = data.parentid ? data.parentid : '00000000-0000-0000-0000-000000000000';

    var errors = [];
    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        if (data[fld] !== null) {
          continue;
        }
        switch (fld) {
          case 'commenttype'  : errors.push($page.find('input.comment-type:first').addClass('error-field')); break;
          case 'id'           : errors.push($page.find('input.comment-id:first').addClass('error-field')); break;
          case 'content'      : errors.push($page.find('textarea.comment-content:first').addClass('error-field')); break;
        }
      }
    }

    if (errors.length === 0) {
      return data;
    }

    ASC.Controls.messages.show(Base64.encode(ASC.Resources.ErrEmpyField), 'error', ASC.Resources.ErrEmpyField);
    return null;
  }

  function getBlogData ($page) {
    var data = {
      title   : $page.find('input.blog-title:first').removeClass('error-field').val(),
      content : $page.find('textarea.blog-description:first').removeClass('error-field').val() || '',
      tags    : $page.find('input.blog-tags:first').removeClass('error-field').val() || ''
    };

    data.tags = data.tags.split(/\s*,\s*/).join(',');

    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        switch (fld) {
          case 'tags':
              continue;
        }
        data[fld] = TeamlabMobile.verificationValue(data[fld]);
      }
    }

    var errors = [];
    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        if (data[fld] !== null) {
          continue;
        }
        switch (fld) {
          case 'title'    : errors.push($page.find('input.blog-title:first').addClass('error-field')); break;
          case 'content'  : errors.push($page.find('textarea.blog-description:first').addClass('error-field')); break;
        }
      }
    }

    if (errors.length === 0) {
      return data;
    }

    ASC.Controls.messages.show(Base64.encode(ASC.Resources.ErrEmpyField), 'error', ASC.Resources.ErrEmpyField);
    return null;
  }

  function getForumData ($page) {
    var data = {
      threadid  : $page.find('select.forum-threadid:first').removeClass('error-field').val(),
      subject   : $page.find('input.forum-title:first').removeClass('error-field').val(),
      content   : $page.find('textarea.forum-description:first').removeClass('error-field').val() || ''
    };

    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        data[fld] = TeamlabMobile.verificationValue(data[fld]);
      }
    }

    var errors = [];
    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        if (data[fld] !== null) {
          continue;
        }
        switch (fld) {
          case 'threadid': errors.push($page.find('select.forum-threadid:first').addClass('error-field')); break;
          case 'subject': errors.push($page.find('input.forum-title:first').addClass('error-field')); break;
          case 'content': errors.push($page.find('textarea.forum-description:first').addClass('error-field')); break;
        }
      }
    }

    if (errors.length === 0) {
      return data;
    }

    ASC.Controls.messages.show(Base64.encode(ASC.Resources.ErrEmpyField), 'error', ASC.Resources.ErrEmpyField);
    return null;
  }

  function getEventData ($page) {
    return null;
  }

  function getBookmarkData ($page) {
    var data = {
      url         : $page.find('input.bookmark-url:first').removeClass('error-field').val(),
      title       : $page.find('input.bookmark-title:first').removeClass('error-field').val(),
      description : $page.find('textarea.bookmark-description:first').removeClass('error-field').val() || '',
      tags        : $page.find('input.bookmark-tags:first').removeClass('error-field').val() || ''
    };

    data.tags = data.tags.split(/\s*,\s*/).join(',');

    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        switch (fld) {
          case 'description':
          case 'tags':
            continue;
        }
        data[fld] = TeamlabMobile.verificationValue(data[fld]);
      }
    }

    var errors = [];
    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        if (data[fld] !== null) {
          continue;
        }
        switch (fld) {
          case 'url'    : errors.push($page.find('input.bookmark-url:first').addClass('error-field')); break;
          case 'title'  : errors.push($page.find('input.bookmark-title:first').addClass('error-field')); break;
        }
      }
    }

    if (data.hasOwnProperty('url') && data.url) {
      data.url = data.url.indexOf('://') === -1 ? 'http://' + data.url : data.url;
    }

    if (errors.length === 0) {
      return data;
    }

    ASC.Controls.messages.show(Base64.encode(ASC.Resources.ErrEmpyField), 'error', ASC.Resources.ErrEmpyField);
    return null;
  }

  DefaultMobile.add_community_blog_comment = function (evt, $page, $button) {
    var data = getCommentData($page);
    if (data && TeamlabMobile.addCommunityComment('blog', data.id, data)) {
      jQuery(document).trigger('changepage');
      $button.addClass('disabled');
    }
  };

  DefaultMobile.add_community_forum_post = function (evt, $page, $button) {
    var data = getCommentData($page);
    if (data && TeamlabMobile.addCommunityComment('forum', data.id, data)) {
      jQuery(document).trigger('changepage');
      $button.addClass('disabled');
    }
  };

  DefaultMobile.add_community_event_comment = function (evt, $page, $button) {
    var data = getCommentData($page);
    //HACK: Fast fix problem with parentid in events is LONG not GUID !!!
    if (data.parentid === "00000000-0000-0000-0000-000000000000") {
          data.parentid = 0;
    }
    if (data && TeamlabMobile.addCommunityComment('event', data.id, data)) {
      jQuery(document).trigger('changepage');
      $button.addClass('disabled');
    }
  };

  DefaultMobile.add_community_bookmark_comment = function (evt, $page, $button) {
    var data = getCommentData($page);
    if (data && TeamlabMobile.addCommunityComment('bookmark', data.id, data)) {
      jQuery(document).trigger('changepage');
      $button.addClass('disabled');
    }
  };

  DefaultMobile.create_community_blog = function (evt, $page, $button) {
    var data = getBlogData($page);
    if (data && TeamlabMobile.addCommunityItem('blog', data)) {
      jQuery(document).trigger('changepage');
      $button.addClass('disabled');
    }
  };

  DefaultMobile.create_community_forum = function (evt, $page, $button) {
    var data = getForumData($page);
    if (data && TeamlabMobile.addCommunityItem('forum', data)) {
      jQuery(document).trigger('changepage');
      $button.addClass('disabled');
    }
  };

  DefaultMobile.create_community_event = function (evt, $page, $button) {
    var data = getEventData($page);
    if (data && TeamlabMobile.addCommunityItem('event', data)) {
      jQuery(document).trigger('changepage');
      $button.addClass('disabled');
    }
  };

  DefaultMobile.create_community_bookmark = function (evt, $page, $button) {
    var data = getBookmarkData($page);
    if (data && TeamlabMobile.addCommunityItem('bookmark', data)) {
      jQuery(document).trigger('changepage');
      $button.addClass('disabled');
    }
  };

  DefaultMobile.load_more_community_items = function (evt, $page, $button) {
    var types = [
      {type : 'all', classname : 'filter-none'},
      {type : 'blog', classname : 'filter-blogs'},
      {type : 'forum', classname : 'filter-forums'},
      {type : 'event', classname : 'filter-events'},
      {type : 'bookmark', classname : 'filter-bookmarks'}
    ];

    var typesInd = types.length;
    while (typesInd--) {
      if ($page.hasClass(types[typesInd].classname)) {
        TeamlabMobile.getMoreCommItems(types[typesInd].type);
        $page.addClass('loading-items');
      }
    }
  };

  return DefaultMobile;
})(DefaultMobile);

;(function($) {
    TeamlabMobile.bind(TeamlabMobile.events.communityPage, onCommunityPage);
    TeamlabMobile.bind(TeamlabMobile.events.loadTimelineCommItems, onLoadTimelineCommItems);
    TeamlabMobile.bind(TeamlabMobile.events.blogsPage, onBlogsPage);
    TeamlabMobile.bind(TeamlabMobile.events.forumsPage, onForumsPage);
    TeamlabMobile.bind(TeamlabMobile.events.eventsPage, onEventsPage);
    TeamlabMobile.bind(TeamlabMobile.events.bookmarksPage, onBookmarksPage);
    TeamlabMobile.bind(TeamlabMobile.events.blogPage, onBlogPage);
    TeamlabMobile.bind(TeamlabMobile.events.forumPage, onForumPage);
    TeamlabMobile.bind(TeamlabMobile.events.eventPage, onEventPage);
    TeamlabMobile.bind(TeamlabMobile.events.bookmarkPage, onBookmarkPage);
    TeamlabMobile.bind(TeamlabMobile.events.addBlogPage, onAddBlogPage);
    TeamlabMobile.bind(TeamlabMobile.events.addForumPage, onAddForumPage);
    TeamlabMobile.bind(TeamlabMobile.events.addEventPage, onAddEventPage);
    TeamlabMobile.bind(TeamlabMobile.events.addBookmarkPage, onAddBookmarkPage);
    TeamlabMobile.bind(TeamlabMobile.events.getThreads, onGetThreads);

    TeamlabMobile.bind(TeamlabMobile.events.addCommunityComment, onAddComment);

    function onCommunityPage(data, enabledModules, allLoaded) {
        data = { pagetitle: ASC.Resources.LblCommunityTitle, type: 'community-page', items: data, modules: enabledModules, allLoaded: allLoaded };

        DefaultMobile.renderPage('community-page', 'page-community', 'community', ASC.Resources.LblCommunityTitle, data).addClass('filter-none');
    }

    function onBlogsPage(data, enabledModules, allLoaded) {
        data = { pagetitle: ASC.Resources.LblBlogs, type: 'community-page-blogs', items: data, modules: enabledModules, allLoaded: allLoaded };

        var $page = DefaultMobile.renderPage('community-blogs-page', 'page-community-blogs', 'community-blogs', ASC.Resources.LblBlogs, data).addClass('filter-blogs');

        $page.find('a.ui-btn-addblog').removeClass('ui-btn-disabled');
    }

    function onForumsPage(data, enabledModules, allLoaded, hasThreads) {
        data = { pagetitle: ASC.Resources.LblForums, type: 'community-page-forums', items: data, modules: enabledModules, allLoaded: allLoaded, hasThreads: hasThreads };

        var $page = DefaultMobile.renderPage('community-forums-page', 'page-community-forums', 'community-forums', ASC.Resources.LblForums, data).addClass('filter-forums');

        if (hasThreads) {
            $page.find('a.ui-btn-addforum').removeClass('ui-btn-disabled');
        }
    }

    function onEventsPage(data, enabledModules, allLoaded) {
        data = { pagetitle: ASC.Resources.LblEvents, type: 'community-page-events', items: data, modules: enabledModules, allLoaded: allLoaded };

        var $page = DefaultMobile.renderPage('community-events-page', 'page-community-events', 'community-events', ASC.Resources.LblEvents, data).addClass('filter-events');

        $page.find('a.ui-btn-addevent').removeClass('ui-btn-disabled');
    }

    function onBookmarksPage(data, enabledModules, allLoaded) {
        data = { pagetitle: ASC.Resources.LblBookmarks, type: 'community-page-bookmarks', items: data, modules: enabledModules, allLoaded: allLoaded };

        var $page = DefaultMobile.renderPage('community-bookmarks-page', 'page-community-bookmarks', 'community-bookmarks', ASC.Resources.LblBookmarks, data).addClass('filter-bookmarks');

        $page.find('a.ui-btn-addbookmark').removeClass('ui-btn-disabled');
    }

    function onLoadTimelineCommItems(type, data, allLoaded) {
        data = { items: data };

        var $page = null;

        switch (type) {
            case 'all': $page = $('div.page-community.filter-none'); break;
            case 'blog': $page = $('div.page-community.filter-blogs'); break;
            case 'forum': $page = $('div.page-community.filter-forums'); break;
            case 'event': $page = $('div.page-community.filter-events'); break;
            case 'bookmark': $page = $('div.page-community.filter-bookmarks'); break;
        }

        if ($page !== null) {
            $page.removeClass('loading-items').find('ul.ui-timeline:first').append(DefaultMobile.processTemplate(TeamlabMobile.templates.lbnewcommitems, data));

            DefaultMobile.renderVectorImages($page);

            if (allLoaded === true) {
                $page.addClass('loaded-items');
            }
            jQuery(document).trigger('updatepage');
        }
    }

    function onBlogPage(data, params) {
        data = { pagetitle: ASC.Resources.LblCommunityTitle, title: ASC.Resources.LblCommunityTitle, id: data.id, parentid: null, back: null, item: data };

        if (params.hasOwnProperty('back')) {
            data.back = params.back;
        }

        var $page = DefaultMobile.renderPage('community-blog-page', 'page-community-blog', 'community-blog-' + data.id, ASC.Resources.LblBlog, data);
        $('.ui-blog-content').find('a').addClass('target-blank');
    }

    function onForumPage(data, params) {
        data = { pagetitle: ASC.Resources.LblCommunityTitle, title: ASC.Resources.LblCommunityTitle, id: data.id, parentid: null, back: null, item: data };

        if (params.hasOwnProperty('back')) {
            data.back = params.back;
        }

        var $page = DefaultMobile.renderPage('community-forum-page', 'page-community-forum', 'community-forum-' + data.id, ASC.Resources.LblForum, data).addClass('loaded-comments');
    }

    function onEventPage(data, params) {
        data = { pagetitle: ASC.Resources.LblCommunityTitle, title: ASC.Resources.LblCommunityTitle, id: data.id, parentid: null, back: null, item: data };

        if (params.hasOwnProperty('back')) {
            data.back = params.back;
        }

        var $page = DefaultMobile.renderPage('community-event-page', 'page-community-event', 'community-event-' + data.id, ASC.Resources.LblEvent, data);
    }

    function onBookmarkPage(data, params) {
        data = { pagetitle: ASC.Resources.LblCommunityTitle, title: ASC.Resources.LblCommunityTitle, id: data.id, parentid: null, back: null, item: data };

        if (params.hasOwnProperty('back')) {
            data.back = params.back;
        }

        var $page = DefaultMobile.renderPage('community-bookmark-page', 'page-community-bookmark', 'community-bookmark-' + data.id, ASC.Resources.LblBookmark, data);
    }

    function onAddBlogPage(data, params) {
        data = { pagetitle: ASC.Resources.LblAddBlogTitle, title: ' ', type: 'community-addblog' };

        var $page = DefaultMobile.renderPage('community-addblog-page', 'page-community-addblog', 'community-addblog' + Math.floor(Math.random() * 1000000), ' ', data);
    }

    function onAddForumPage(data, params) {
        data = { pagetitle: ASC.Resources.LblAddForumTitle, title: ' ', type: 'community-addforum' };

        var $page = DefaultMobile.renderPage('community-addforum-page', 'page-community-addforum', 'community-addforum' + Math.floor(Math.random() * 1000000), ' ', data);

        $page.addClass('threads-loading');
        TeamlabMobile.getThreads();
    }

    function onAddEventPage(data, params) {
        data = { pagetitle: ASC.Resources.LblAddEventTitle, title: ' ', type: 'community-addevent' };

        var $page = DefaultMobile.renderPage('community-addevent-page', 'page-community-addevent', 'community-addevent' + Math.floor(Math.random() * 1000000), 'title', data);
    }

    function onAddBookmarkPage(data, params) {
        data = { pagetitle: ASC.Resources.LblAddBookmarkTitle, title: ' ', type: 'community-addbookmark' };

        var $page = DefaultMobile.renderPage('community-addbookmark-page', 'page-community-addbookmark', 'community-addbookmark' + Math.floor(Math.random() * 1000000), ' ', data);
    }

    function onAddComment(data, params) {
        var $page = $();
        var comment = data;

        if (params.hasOwnProperty('type')) {
            switch (params.type) {
                case 'blog':
                    $page = $('div.page-community-blog.ui-page-active:first');
                    break;
                case 'forum':
                    $page = $('div.page-community-forum.ui-page-active:first');
                    break;
                case 'event':
                    $page = $('div.page-community-event.ui-page-active:first');
                    break;
                case 'bookmark':
                    $page = $('div.page-community-bookmark.ui-page-active:first');
                    break;
            }
        }

        if ($page.length === 0) {
            return undefined;
        }

        data = { comments: [data] };

        var $comments = $page.find('ul.ui-item-comments:first');
        if (comment.parentId) {
            $comments.find('li.item-comment[data-commentid="' + comment.parentId + '"]:first ul.inline-comments:first').append(DefaultMobile.processTemplate(TeamlabMobile.templates.lbcomments, data));
        } else {
            $comments.append(DefaultMobile.processTemplate(TeamlabMobile.templates.lbcomments, data));
        }

        jQuery(document).trigger('updatepage');
    }

    function onGetThreads(data, params) {
        data = { items: data };

        var $page = $('div.ui-page-active:first').removeClass('threads-loading');

        $page.find('select.forum-threadid').remove();
        $page.find('div.item-threadid:first').html(DefaultMobile.processTemplate(TeamlabMobile.templates.lbcommthreads, data));
        DefaultMobile.renderCustomSelects($page);
    }
})(jQuery);
