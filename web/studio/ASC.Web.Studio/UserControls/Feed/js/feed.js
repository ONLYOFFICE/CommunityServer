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


ASC.Feed = (function() {
    var $ = jq;

    var userId = Teamlab.profile.id,
        basePath = '',
        readedDate,
        feedChunk = 30,
        currentFeedsCount = 0,
        currentGroupFeedsCount = 0,
        guestId = '712d9ec3-5d2b-4b13-824f-71f00191dcca',
        firstLoad = true,
        $view = $('#feed-view'),
        $emptyScreen = $view.find('#emptyFeedScr'),
        $emptyFilterScreen = $view.find('#emptyFeedFilterScr'),
        $managerEmptyScreen = $view.find('#manager-empty-screen'),

        $communityEmptyScreen = $view.find('#emptyListCommunity'),
        $crmEmptyScreen = $view.find('#emptyListCrm'),
        $projectsEmptyScreen = $view.find('#emptyListProjects'),
        $documentsEmptyScreen = $view.find('#emptyListDocuments'),

        $firstLoader = $view.find('.loader-page'),

        $pageMenu = $('#feed-page-menu'),

        $list = $('#feed-list'),

        $showNextBtn = $('#show-next-feeds-btn'),
        $showNextLoader = $('#show-next-feeds-loader'),
        filter,
        feedTemplateId = 'feedTmpl',
        feedCommentTemplateId = 'feedCommentTmpl',
        productsAccessRights;

    function init(productsAccessRightsParam) {
        filter = new FeedFilter();
        filter.onSetFilter = onSetFilter;
        filter.onResetFilter = onResetFilter;

        productsAccessRights = productsAccessRightsParam.split(',');
        for (var i = 0; i < productsAccessRights.length; i++) {
            productsAccessRights[i] = productsAccessRights[i].toLowerCase() == 'true';
        }
        initFilter();
        bindEvents();
    }

    function initFilter() {
        var now = new Date();

        var today = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0);
        var tomorrow = new Date(now.getFullYear(), now.getMonth(), now.getDate() + 1, 0, 0, 0, 0);

        var startWeek = new Date(today);
        startWeek.setDate(today.getDate() - today.getDay() + 1);

        var endWeek = new Date(today);
        endWeek.setDate(today.getDate() - today.getDay() + 7);

        var startMonth = new Date(today);
        startMonth.setDate(1);

        var endMonth = new Date(startMonth);
        endMonth.setMonth(startMonth.getMonth() + 1);
        endMonth.setDate(endMonth.getDate() - 1);

        today = today.getTime();
        tomorrow = tomorrow.getTime();
        startWeek = startWeek.getTime();
        endWeek = endWeek.getTime();
        startMonth = startMonth.getTime();
        endMonth = endMonth.getTime();

        var filters = [];
        if (productsAccessRights[0]) {
            filters.push({
                type: 'combobox',
                id: 'community',
                title: ASC.Resources.Master.FeedResource.CommunityProduct,
                filtertitle: ASC.Resources.Master.FeedResource.Product,
                group: ASC.Resources.Master.FeedResource.Product,
                hashmask: 'product/{0}',
                groupby: 'product',
                options: getProductFilterOptions(productsAccessRights, 'community')
            });
        }
        if (productsAccessRights[1]) {
            filters.push({
                type: 'combobox',
                id: 'crm',
                title: ASC.Resources.Master.FeedResource.CrmProduct,
                filtertitle: ASC.Resources.Master.FeedResource.Product + ':',
                group: ASC.Resources.Master.FeedResource.Product,
                hashmask: 'product/{0}',
                groupby: 'product',
                options: getProductFilterOptions(productsAccessRights, 'crm')
            });
        }
        if (productsAccessRights[2]) {
            filters.push({
                type: 'combobox',
                id: 'projects',
                title: ASC.Resources.Master.FeedResource.ProjectsProduct,
                filtertitle: ASC.Resources.Master.FeedResource.Product,
                group: ASC.Resources.Master.FeedResource.Product,
                hashmask: 'product/{0}',
                groupby: 'product',
                options: getProductFilterOptions(productsAccessRights, 'projects')
            });
        }
        if (productsAccessRights[3]) {
            filters.push({
                type: 'combobox',
                id: 'documents',
                title: ASC.Resources.Master.FeedResource.DocumentsProduct,
                filtertitle: ASC.Resources.Master.FeedResource.Product + ':',
                group: ASC.Resources.Master.FeedResource.Product,
                hashmask: 'product/{0}',
                groupby: 'product',
                options: getProductFilterOptions(productsAccessRights, 'documents')
            });
        }

        $('#feed-filter').advansedFilter(
            {
                store: false,
                anykey: true,
                colcount: 2,
                anykeytimeout: 1000,
                filters: filters.concat(
                    [
                        // Distance
                        {
                            type: 'daterange',
                            id: 'today',
                            title: ASC.Resources.Master.FeedResource.Today,
                            filtertitle: ' ',
                            group: ASC.Resources.Master.FeedResource.TimeDistance,
                            hashmask: 'distance/{0}/{1}',
                            groupby: 'distance',
                            bydefault: { from: today, to: today }
                        },
                        {
                            type: 'daterange',
                            id: 'currentweek',
                            title: ASC.Resources.Master.FeedResource.CurrentWeek,
                            filtertitle: ' ',
                            group: ASC.Resources.Master.FeedResource.TimeDistance,
                            hashmask: 'distance/{0}/{1}',
                            groupby: 'distance',
                            bydefault: { from: startWeek, to: endWeek }
                        },
                        {
                            type: 'daterange',
                            id: 'currentmonth',
                            title: ASC.Resources.Master.FeedResource.CurrentMonth,
                            filtertitle: ' ',
                            group: ASC.Resources.Master.FeedResource.TimeDistance,
                            hashmask: 'distance/{0}/{1}',
                            groupby: 'distance',
                            bydefault: { from: startMonth, to: endMonth }
                        },
                        {
                            type: 'daterange',
                            id: 'distance',
                            title: ASC.Resources.Master.FeedResource.CustomPeriod,
                            filtertitle: ' ',
                            group: ASC.Resources.Master.FeedResource.TimeDistance,
                            hashmask: 'distance/{0}/{1}',
                            groupby: 'distance'
                        },
                        // Author
                        {
                            type: 'person',
                            id: 'author',
                            title: ASC.Resources.Master.FeedResource.OtherUsers,
                            filtertitle: ASC.Resources.Master.FeedResource.ByUser + ':',
                            group: ASC.Resources.Master.FeedResource.ByUser,
                            hashmask: 'author/{0}',
                            groupby: 'authorid',
                            showme: false
                        }
                    ]),
                sorters: []
            })
            .bind('setfilter', filter.onSetFilter)
            .bind('resetfilter', filter.onResetFilter)
            .advansedFilter('sort', false);

        function getProductFilterOptions(productsRights, product) {
            var options = [];
            var elem;
            if (productsRights[0]) {
                elem = { value: 'community', title: ASC.Resources.Master.FeedResource.CommunityProduct };
                if (product == 'community') {
                    elem.def = true;
                }
                options.push(elem);
            }
            if (productsRights[1]) {
                elem = { value: 'crm', title: ASC.Resources.Master.FeedResource.CrmProduct };
                if (product == 'crm') {
                    elem.def = true;
                }
                options.push(elem);
            }
            if (productsRights[2]) {
                elem = { value: 'projects', title: ASC.Resources.Master.FeedResource.ProjectsProduct };
                if (product == 'projects') {
                    elem.def = true;
                }
                options.push(elem);
            }
            if (productsRights[3]) {
                elem = { value: 'documents', title: ASC.Resources.Master.FeedResource.DocumentsProduct };
                if (product == 'documents') {
                    elem.def = true;
                }
                options.push(elem);
            }
            return options;
        }
    }

    function onSetFilter(evt, $fltr) {
        if (filter.resolveFirstLoad()) {
            return;
        }

        filter.syncHashWithFilter($fltr);
        getNews({}, filter.get($fltr));
    }

    function onResetFilter(evt, $fltr) {
        filter.syncHashWithFilter($fltr);
        getNews({}, filter.get($fltr));
    }

    var currentFilter = {};

    function getNews(options, filterObj) {
        currentFilter = filterObj;
        if (!options.showNext) {
            currentFeedsCount = 0;
            currentGroupFeedsCount = 0;
        }
        filterObj.StartIndex = currentGroupFeedsCount;
        filterObj.Count = feedChunk;

        if (readedDate) {
            filterObj.timeReaded = readedDate;
        }

        showLoader();
        Teamlab.getFeeds(options, {
            filter: filterObj,
            success: function(params, data) {
                hideLoader();
                firstLoad = false;
                onGetNews(params, data);
            },
            error: function() {
                hideLoader();
                showErrorMessage();
            }
        });
    }

    function onGetNews(params, response) {
        var feeds = response.feeds;
        //feeds = [];

        hideEmptyScreens();
        if (feeds.length) {
            readedDate = response.readedDate;
            showFilter();

            if (!params.showNext) {
                $list.empty();
            }

            var feedsCount = feeds.length;
            for (var j = 0; j < feeds.length; j++) {
                try {
                    var template = getFeedTemplate(feeds[j]);
                    currentGroupFeedsCount += template.groupedFeeds.length;
                    if (params.showNext) {
                        $.tmpl(feedTemplateId, template).insertAfter($list.find('.item:last'));
                    } else {
                        $.tmpl(feedTemplateId, template).appendTo($list);
                    }
                } catch(e) {
                    console.log(e);
                }
            }

            currentFeedsCount += feedsCount;
            currentGroupFeedsCount += feedsCount;

            $list.show();

            $showNextLoader.hide();
            if (feedsCount == feedChunk) {
                $showNextBtn.show();
            } else {
                $showNextBtn.hide();
            }
        } else {
            $showNextBtn.hide();
            if (currentFeedsCount == 0) {
                hideFilter();
                $list.empty();
                showEmptyScreen();
            }
        }

        $showNextLoader.hide();
        jq('#feed-filter').advansedFilter('resize');
    }

    function showEmptyScreen() {
        var hash = window.location.hash;
        if (hash == basePath) {
            if (true) {
                $managerEmptyScreen.show();
            } else {
                $emptyScreen.show();
            }
        } else {
            if (~hash.indexOf('&')) {
                showFilter();
                $emptyFilterScreen.show();
            } else if (~hash.indexOf('community')) {
                $communityEmptyScreen.show();
            } else if (~hash.indexOf('crm')) {
                $crmEmptyScreen.show();
            } else if (~hash.indexOf('projects')) {
                $projectsEmptyScreen.show();
            } else if (~hash.indexOf('documents')) {
                $documentsEmptyScreen.show();
            } else {
                showFilter();
                $emptyFilterScreen.show();
            }
        }
    }

    function hideEmptyScreens() {
        $emptyScreen.hide();
        $emptyFilterScreen.hide();
        $managerEmptyScreen.hide();
        $communityEmptyScreen.hide();
        $crmEmptyScreen.hide();
        $projectsEmptyScreen.hide();
        $documentsEmptyScreen.hide();
    }

    function getFeedTemplate(feed) {
        if (!feed.item) {
            return null;
        }

        var template = feed;

        template.author = window.UserManager.getUser(template.authorId);

        template.isGuest = template.author == null || template.authorId == guestId;
        template.isNew = checkNew(template);

        template.productText = getFeedProductText(template);
        if (!template.location) {
            template.location = getFeedLocation(template);
        }

        resolveAdditionalFeedData(template);
        template.actionText = getFeedActionText(template);

        if (template.comments) {
            for (var j = 0; j < template.comments.length; j++) {
                template.comments[j].author = window.UserManager.getUser(template.comments[j].authorId) || window.UserManager.getRemovedProfile();
            }
        }

        return template;
    }

    function checkNew(template) {
        if (template.aggregatedDate <= readedDate) {
            return false;
        }

        if (template.action != 2) {
            return template.authorId != userId;
        } else {
            for (var j = 0; j < template.comments.length; j++) {
                var comment = template.comments[j];
                if (comment.authorId != userId) {
                    return true;
                }
            }
            return false;
        }
    }

    function getFeedProductText(template) {
        var productsCollection = ASC.Feed.Products;
        if (!productsCollection) {
            return null;
        }

        return productsCollection[template.product];
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

    function resolveAdditionalFeedData(template) {
        template.excludeAuthorBox = false;

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
            case 'forumTopic':
            case 'forumPoll':
            case 'forumPost':
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
                /*var crmTaskResponsible = getUser(template.additionalInfo);
                template.additionalInfo = crmTaskResponsible ? crmTaskResponsible.displayName : '';*/

                template.itemClass = 'tasks';
                template.hintName = 'responsible';
                template.hintName2 = 'contact';
                break;
            case 'deal':
                template.itemClass = 'opportunities';
                template.hintName = 'contact';
                break;
            case 'cases':
                template.itemClass = 'cases';
                template.hintName = 'members';
                break;
            case 'project':
                /*var projectManager = getUser(template.additionalInfo);
                template.additionalInfo = projectManager ? projectManager.displayName : '';*/

                template.itemClass = 'projects';
                template.hintName = 'manager';
                break;
            case 'participant':
                template.itemClass = 'projects';

                template.title = template.author.displayName;

                for (var j = 0; j < template.groupedFeeds.length; j++) {
                    var g = template.groupedFeeds[j];
                    var author = window.UserManager.getUser(g.AuthorId);

                    g.Title = author ? author.displayName : null;
                    g.ItemUrl = author ? author.profileUrl : null;
                }
                template.excludeAuthorBox = true;

                break;
            case 'milestone':
                /*var milestoneResponsible = getUser(template.additionalInfo);
                template.additionalInfo = milestoneResponsible ? milestoneResponsible.displayName : '';*/

                template.itemClass = 'milestones';
                template.hintName = 'responsible';
                template.hintName2 = 'deadline';
                break;
            case 'task':
                /*var responsibles = template.additionalInfo ? template.additionalInfo.split(',') : null;
                var taskResponsible = getUsers(responsibles);
                template.additionalInfo = taskResponsible ? taskResponsible.map(function(elem) {
                    return elem.displayName;
                }).join(', ') : '';*/

                template.itemClass = 'tasks';
                template.hintName = 'responsibles';
                break;
            case 'discussion':
                template.itemClass = 'messages';
                break;
            case 'file':
            case 'sharedFile':
                template.itemClass = 'documents';
                template.hintName = 'size';
                break;
            case 'folder':
            case 'sharedFolder':
                template.itemClass = 'documents';
                break;
        }
    }

    function bindEvents() {
        $emptyFilterScreen.on('click', '.clearFilterButton', function() {
            $('.advansed-filter .btn-reset-filter').click();
        });

        // #region page menu

        $pageMenu.on('click', '.filter', function() {
            $(this).find('.menu-item-label').click();
        });

        $pageMenu.on('click', '.filter .menu-item-label', function() {
            $('.page-menu .active').removeClass('active');
            $(this).parent().addClass('active');
        });

        $pageMenu.on('click', '#feed-all-products-nav', function() {
            filter.changeHash('product', '');
            $('#feed-filter .advansed-filter-input').removeClass('has-value');
            return false;
        });

        $pageMenu.on('click', '#feed-community-product-nav', function() {
            filter.changeHash('product', 'community');
            return false;
        });

        $pageMenu.on('click', '#feed-crm-product-nav', function() {
            filter.changeHash('product', 'crm');
            return false;
        });

        $pageMenu.on('click', '#feed-projects-product-nav', function() {
            filter.changeHash('product', 'projects');
            return false;
        });

        $pageMenu.on('click', '#feed-documents-product-nav', function() {
            filter.changeHash('product', 'documents');
            return false;
        });

        // #endregion

        // #region list

        $list.on('click', '.show-all-btn', function() {
            var $this = $(this);

            var $description = $this.parents('.body');
            $description.find('.asccut').each(function() {
                $(this).removeClass('asccut');
            });

            $this.remove();
            return false;
        });

        $showNextBtn.on('click', function() {
            $showNextBtn.hide();
            $showNextLoader.show();
            getNews({ showNext: true }, currentFilter, true);
            return false;
        });


        $list.on('mouseenter', '.feed-item .header .title, .feed-item .grouped-feeds-box .title', function() {
            var elem = $(this);
            hintPanel = setTimeout(function() {
                $('#hintPanel .feed-params-hint span, #hintPanel .feed-values-hint span').hide();
                var isDescEmpty = true;
                var extra = elem.attr('data-extra');
                if (extra != '') {
                    isDescEmpty = false;
                    var hintName = elem.attr('data-hintName');
                    $('#hintPanel .feed-values-hint .feed-hint-' + hintName).html(htmlEncode(extra));
                    $('#hintPanel .feed-hint-' + hintName).show();
                }

                var extra2 = elem.attr('data-extra2');
                if (extra2 != '') {
                    isDescEmpty = false;
                    var hintName2 = elem.attr('data-hintName2');
                    $('#hintPanel .feed-values-hint .feed-hint-' + hintName2).html(htmlEncode(extra2));
                    $('#hintPanel .feed-hint-' + hintName2).show();
                }

                var extra3 = elem.attr('data-extra3');
                if (extra3 != '') {
                    isDescEmpty = false;
                    var hintName3 = elem.attr('data-hintName3');
                    $('#hintPanel .feed-values-hint .feed-hint-' + hintName3).html(htmlEncode(extra3));
                    $('#hintPanel .feed-hint-' + hintName3).show();
                }

                var extra4 = elem.attr('data-extra4');
                if (extra4 != '') {
                    isDescEmpty = false;
                    var hintName4 = elem.attr('data-hintName4');
                    $('#hintPanel .feed-values-hint .feed-hint-' + hintName4).html(htmlEncode(extra4));
                    $('#hintPanel .feed-hint-' + hintName4).show();
                }

                if (isDescEmpty) {
                    return;
                }

                showHintPanel(elem);
                overHintPanel = true;
            }, 400, this);

            function htmlEncode(value) {
                return $('<div/>').text(value).html().replace(/\n/ig, '<br/>');
            }
        });

        $list.on('mouseleave', '.feed-item .header .title, .feed-item .grouped-feeds-box .title', function() {
            clearTimeout(hintPanel);
            overHintPanel = false;
            hideHintPanel();
        });

        $('#hintPanel').on('mouseenter', function() {
            overHintPanel = true;
        });

        $('#hintPanel').on('mouseleave', function() {
            overHintPanel = false;
            hideHintPanel();
        });

        function showHintPanel(obj) {
            var x, y;
            $('#hintPanel').show();

            x = obj.offset().left;
            y = obj.offset().top + 20;

            $('#hintPanel .dropdown-item').show();
            $('#hintPanel').css({ left: x, top: y });
        }

        function hideHintPanel() {
            setTimeout(function() {
                if (!overHintPanel) {
                    $('#hintPanel').hide(100);
                }
            }, 200);
        }

        // #endregion

        // #region comments

        $list.on('click', '.comments-box .control-btn', function() {
            var $this = $(this);

            if ($this.attr('data-state') == '0') {
                $this.closest('.comments-box').find('.extra-comments-box').show();
                $this.attr('data-state', '1');
                $this.text($this.attr('data-hide-text'));
            } else {
                $this.closest('.comments-box').find('.extra-comments-box').hide();
                $this.attr('data-state', '0');
                $this.text($this.attr('data-show-text'));
            }

            return false;
        });

        $list.on('click', '.write-comment-btn', function() {
            var $this = $(this);

            var $commentForm = $(this).closest('.content-box').find('.comment-form');
            var $publishBtn = $commentForm.find('.publish-comment-btn');
            $publishBtn.attr('data-parent-comment', '');
            $commentForm.insertAfter($this);

            $this.siblings('.comments-box').find('.reply-comment-btn.closed').show().removeClass('closed');
            $this.hide();

            $commentForm.show();
            $commentForm.find('textarea').focus();

            return false;
        });

        $list.on('keypress', '.comment-form textarea', function(e) {
            if ((e.keyCode == 10 || e.keyCode == 13) && e.ctrlKey) {
                var $commentForm = $(this).closest('.comment-form');
                var $publishBtn = $commentForm.find('.publish-comment-btn');

                $publishBtn.click();
                return;
            }

            if (e.which !== 0) {
                $(this).removeClass("error");
            }
        });

        $list.on('click', '.publish-comment-btn', function(event) {
            var $this = $(this);

            var $commentForm = $this.closest('.comment-form');
            var $writeCommentBtn = $commentForm.siblings('.write-comment-btn');

            var commentText = $this.siblings('textarea').val().trim();
            if (!commentText) {
                $this.siblings('textarea').addClass("error").focus();
                event.preventDefault();
                return;
            }

            commentText = Encoder.htmlEncode(commentText).replace(/&#10;/g, '<br />');

            var itemId = $(this).attr('data-id');
            var entity = $(this).attr('data-entity');
            var entityId = $(this).attr('data-entityid');
            var parentCommentId = $(this).attr('data-parent-comment');

            var commentData = getApiCommentData(entity, entityId, commentText, itemId, parentCommentId);

            var commentApiUrl = $(this).attr('data-commentapiurl');

            commentRequest(commentData, commentApiUrl).then(commentCallback).fail(commentFail);

            event.preventDefault();

            function commentCallback(data) {
                if (data.response) {
                    var response = data.response;
                    var template = getCommentTemplateData(entity, response);

                    var $feed = $('#feed_' + itemId);
                    var $commentsBox = $feed.find('.comments-box');

                    $feed.find('.comment-error-msg-box').hide();

                    if (parentCommentId) {
                        var $parentComment = $commentForm.closest('.comment');
                        $.tmpl(feedCommentTemplateId, template).insertAfter($parentComment);
                    } else {
                        var $lastComment = $commentsBox.find('.main-comments-box .comment:last');
                        if ($lastComment.length) {
                            $.tmpl(feedCommentTemplateId, template).insertAfter($lastComment);
                        } else {
                            $('<div style="margin-top: 5px;"></div>').insertBefore($commentsBox.find('.main-comments-box'));
                            $.tmpl(feedCommentTemplateId, template).appendTo($commentsBox);
                        }
                    }

                    $commentForm.hide();
                    $commentForm.find('textarea').val('').removeClass("error");

                    $commentsBox.find('.reply-comment-btn.closed').show().removeClass('closed');

                    $writeCommentBtn.show();
                }
            }

            function commentFail() {
                $('#feed_' + itemId + ' .comment-error-msg-box').show().prev().addClass("error");
            }
        });

        function commentRequest(data, apiUrl) {
            return $.ajax({
                url: apiUrl,
                type: 'POST',
                data: data,
                dataType: 'json',
                traditional: true
            });
        }

        function getApiCommentData(entity, entityId, commentText, itemId, parentCommentId) {
            switch (entity) {
                case 'bookmark':
                    return {
                        id: entityId,
                        content: commentText,
                        parentId: parentCommentId
                    };
                case 'blog':
                    return {
                        postid: entityId,
                        content: commentText,
                        parentId: parentCommentId
                    };
                case 'forum':
                case 'forumPoll':
                    return {
                        topicid: entityId,
                        parentPostId: parentCommentId,
                        subject: $('#feed-list #feed_' + itemId + ' .title a').text(),
                        content: commentText
                    };
                case 'order':
                case 'poll':
                case 'advert':
                case 'news':
                    return {
                        feedid: entityId,
                        content: commentText,
                        parentId: parentCommentId
                    };
                case 'task':
                    return {
                        taskid: entityId,
                        content: commentText,
                        parentid: parentCommentId
                    };
                case 'discussion':
                    return {
                        messageid: entityId,
                        content: commentText,
                        parentId: parentCommentId
                    };
            }
            return {};
        }

        function getCommentTemplateData(entity, response) {
            switch (entity) {
                case 'bookmark':
                case 'blog':
                case 'forum':
                case 'forumPoll':
                case 'order':
                case 'poll':
                case 'advert':
                case 'news':
                case 'task':
                case 'discussion':
                    return {
                        id: response.id,
                        author: {
                            displayName: response.createdBy.displayName,
                            profileUrl: response.createdBy.profileUrl,
                            avatarBig: ASC.Resources.Master.UserPhotoHandlerUrl + '?userId=' + response.createdBy.id,
                        },
                        date: ServiceFactory.getDisplayDatetime(ServiceFactory.serializeDate(response.created)),
                        description: response.text
                    };
            }
            return {};
        }

        $list.on('click', '.cancel-comment-btn', function(event) {
            var $this = $(this);

            var $contentBox = $this.closest('.content-box');
            var $commentForm = $contentBox.find('.comment-form');
            var $commentErrorMsg = $commentForm.find('.comment-error-msg-box');
            var $writeCommentBtn = $contentBox.find('.write-comment-btn');

            $commentForm.hide();
            $commentForm.find('textarea').val('').removeClass("error");

            $writeCommentBtn.show();

            $contentBox.find('.reply-comment-btn.closed').show().removeClass('closed');

            $commentErrorMsg.hide();

            event.preventDefault();
        });

        $list.on('click', '.reply-comment-btn', function() {
            var $this = $(this);

            var $contentBox = $this.closest('.content-box');
            var $commentForm = $contentBox.find('.comment-form');
            var $publishBtn = $commentForm.find('.publish-comment-btn');
            $publishBtn.attr('data-parent-comment', $this.attr('data-commentid'));
            $commentForm.insertAfter($this);

            $contentBox.find('.reply-comment-btn.closed').show().removeClass('closed');

            $this.hide();
            $this.addClass('closed');

            $commentForm.show();
            $commentForm.find('textarea').focus();

            return false;
        });

        // #endregion

        // #region grouped feeds

        $list.on('click', '.grouped-feeds-count', function() {
            var hideBtn = $(this).closest('.header').siblings('.hide-grouped-feeds-btn');
            var showBtn = $(this).closest('.header').siblings('.show-grouped-feeds-btn');

            if (hideBtn.is(':visible')) {
                hideBtn.click();
            } else {
                showBtn.click();
            }
        });

        $list.on('click', '.show-grouped-feeds-btn', function() {
            $(this).hide().siblings('.hide-grouped-feeds-btn').css('display', 'inline-block');
            $(this).hide().siblings('.grouped-feeds-box').show();
        });

        $list.on('click', '.hide-grouped-feeds-btn', function() {
            $(this).hide().siblings('.show-grouped-feeds-btn').css('display', 'inline-block');
            $(this).hide().siblings('.grouped-feeds-box').hide();
        });

        // #endregion
    }

    var hintPanel;
    var overHintPanel = false;

    function showLoader() {
        if (firstLoad) {
            $firstLoader.css({
                top: $(window).height() / 2 + 'px'
            });
        } else {
            LoadingBanner.displayLoading();
        }
    }

    function hideLoader() {
        if (firstLoad) {
            $firstLoader.remove();
        } else {
            LoadingBanner.hideLoading();
        }
    }

    function showFilter() {
        $('#feed-filter').show(); //.css('visibility', 'visible');
    }

    function hideFilter() {
        $('#feed-filter').hide(); //('visibility', 'hidden');
    }

    function showErrorMessage() {
        toastr.error(ASC.Resources.Master.Resource.CommonJSErrorMsg);
    }

    return jq.extend({
        init: init
    }, ASC.Feed);
})();