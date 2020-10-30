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


(function ($) {
    var resources = ASC.Resources.Master.Resource,
        teamlab = Teamlab,
        defaultItemsCount = 10,
        minimalItemHeight = 26,
        padding = 8,
        maximalItemsHeight = minimalItemHeight * defaultItemsCount,
        cache = {};

    var mailtemplateadvancedSelector = function (element, options) {
        this.$element = $(element);
        this.options = $.extend({}, $.fn.mailtemplateadvancedSelector.defaults, options);

        this.init();
    };

    mailtemplateadvancedSelector.prototype = $.extend({}, $.fn.advancedSelector.Constructor.prototype, {
        constructor: mailtemplateadvancedSelector,

        initAdvSelectorData: function () {
            var that = this,
                options = {
                    folder: that.options.searchFolder,
                    page_size: that.options.searchPageLimit
                };

            teamlab.getMailFilteredMessages(null, options, {
                success: function (params, data) {
                    cache.data = data;
                    cache.params = params;

                    that.rewriteObjectItem.call(that, params, data);
                },
                error: function (params, errors) {
                    toastr.error(errors);
                }
            });

            that.$advancedSelector.find('.advanced-selector-search-field').on('keyup', onSearchInputKeyup.bind(that));
            that.$advancedSelector.find('.advanced-selector-reset-btn').on('click', onSearchReset.bind(that));
            that.$advancedSelector.off('click').on('click', '.advanced-selector-list-items li', onClickSaveSelectedItems.bind(that));

            function onClickSaveSelectedItems(event) {
                var that = this,
                    $this = $(event.target),
                    result = $this.attr('data-id');

                that.$element.trigger("showList", result);
                that.$advancedSelector.hide();
            }

            function onSearchInputKeyup(event) {
                var that = this,
                    $this = $(event.target),
                    $search = $this.siblings('.advanced-selector-search-btn'),
                    $reset = $this.siblings('.advanced-selector-reset-btn'),
                    itemList = that.$itemsListSelector.find('li:not(.disabled)'),
                    noResult = that.$advancedSelector.find('.advanced-selector-no-results');

                $this.delayAfterInput(function () {
                    if ($this.val() !== '') {
                        $search.trigger('click');
                        $search.hide();
                        $reset.show();
                    } else {
                        noResult.add($reset).hide();
                        itemList.add($search).show();

                        that.options.showall = undefined;

                        that.rewriteObjectItem.call(that, cache.params, cache.data);
                    }
                }, that.options.searchDelay);
            }

            function onSearchReset() {
                var that = this,
                    $resetBtn = that.$advancedSelector.find(".advanced-selector-search .advanced-selector-reset-btn"),
                    $searchBtn = $resetBtn.siblings(".advanced-selector-search-btn"),
                    $itemList = that.$itemsListSelector.find("li:not(.disabled)"),
                    $noResult = that.$advancedSelector.find(".advanced-selector-no-results"),
                    noItems = that.$advancedSelector.find(".advanced-selector-no-items");

                $resetBtn.siblings("input").val("");
                $noResult.hide();
                noItems.hide();
                $itemList.show();
                $resetBtn.hide();
                $searchBtn.show();

                that.rewriteObjectItem.call(that, cache.params, cache.data);
            }

            $.fn.delayAfterInput = (function () {
                var timer = 0;
                return function (callback, ms) {
                    clearTimeout(timer);
                    timer = setTimeout(callback, ms);
                };
            })();
        },

        onSearchItems: function () {
            var that = this,
                $searchField = that.$advancedSelector.find('.advanced-selector-search-field'),
                $moreBtn = that.$itemsListSelector.find('.moreBtn'),
                $listItems = that.$itemsListSelector.find('.advanced-selector-list'),
                options = {
                    folder: that.options.searchFolder,
                    search: $searchField.val().trim(),
                    page_size: that.options.searchPageLimit
                };

            if (options.search === '')
                return;

            teamlab.getMailFilteredMessages(null, options, {
                success: successCallback,
                error: errorCallback
            });

            function successCallback(params, data) {
                var $noResult = that.$advancedSelector.find('.advanced-selector-no-results'),
                    btnAddHeight = that.$itemsListSelector.find('.advanced-selector-btn-cnt').height() || 0,
                    searchHeight = that.$advancedSelector.find('.advanced-selector-search').height();

                $listItems.html('');
                that.options.showall = undefined;

                if (data.length == 0) {
                    $noResult.show();
                    $moreBtn.hide();

                    that.$itemsListSelector.find('.advanced-selector-list').height(minimalItemHeight);
                    that.$advancedSelector.height(searchHeight + minimalItemHeight + btnAddHeight + padding * 4);
                } else {
                    $noResult.hide();

                    if (data.length > defaultItemsCount) {
                        that.rewriteObjectItem.call(that, params, data.slice(0, defaultItemsCount));
                        $moreBtn.show().off('click').on('click', function () { that.showFullTmplList.call(that, params, data); });
                    } else {
                        that.rewriteObjectItem.call(that, params, data);
                        $moreBtn.hide();
                    }
                }
            };

            function errorCallback(params, errors) {
                toastr.error(errors);
            };
        },

        rewriteObjectItem: function (params, data) {
            var that = this;

            that.items = data.map(function (item) {
                return {
                    id: item.id,
                    title: item.subject ? item.subject : item.introduction ? item.introduction : item.title ? item.title : resources.MailNoSubject
                };
            });

            that.showItemsListAdvSelector.call(that);
        },

        showItemsListAdvSelector: function () {
            var that = this,
                itemsDisplay = that.items,
                $listSelector = that.$itemsListSelector,
                $advancedSelector = that.$advancedSelector,
                $emptyList = $advancedSelector.find('.advanced-selector-empty-list'),
                $search = $advancedSelector.find('.advanced-selector-search'),
                $addButton = $listSelector.find('.advanced-selector-btn-cnt'),
                $listItems = $listSelector.find('.advanced-selector-list'),
                $moreButton = $listSelector.find('.moreBtn'),
                $items = [],
                height = (itemsDisplay.length) ? (minimalItemHeight * itemsDisplay.length) : minimalItemHeight,
                searchHeight = $search.height() || 23,
                btnAddHeight = (that.options.showSaveButton ? ($addButton.height()|| 34) : 0) + padding,
                btnMoreHeight = $moreButton.height() || 20;

            if (that.options.showme) {
                var template = {};

                for (var i = 0, length = itemsDisplay.length; i < length; i++) {
                    if (itemsDisplay[i].id == mailBox.currentMessageId) {
                        template = itemsDisplay[i];
                        template.title = template.title + ' (Current)'; // mark current template
                        itemsDisplay.splice(i, 1);
                        itemsDisplay.unshift(template);
                    }
                }
            }

            if (that.options.showall == undefined && height >= maximalItemsHeight) {
                height = maximalItemsHeight;
                $moreButton.show();
            } else {
                $moreButton.hide();
                btnMoreHeight = 0;

                if (that.options.showall) {
                    height = maximalItemsHeight;
                }
            }

            var selectorHeight = height + btnAddHeight + searchHeight + minimalItemHeight + btnMoreHeight;

            if (!that.options.showall) {
                $items.push($.tmpl(that.options.templates.itemList, { Items: itemsDisplay.slice(0, defaultItemsCount) }));
                $moreButton.off('click').on('click', function () { that.showFullTmplList.call(that, {}, itemsDisplay); });
            } else {
                itemsDisplay.chunkArray(10).forEach(function (chunck) {
                    $items.push($.tmpl(that.options.templates.itemList, { Items: chunck }));
                });
            }

            $listSelector.add($listItems).height(height);
            $advancedSelector.height(selectorHeight);

            $listItems.html($items);

            if (!$items[0].length) {
                $emptyList.show();
                $search.hide();
                $advancedSelector.height(minimalItemHeight + btnAddHeight + padding * 2);
            } else {
                $emptyList.hide();
                if (that.options.showSearch) {
                    $search.show();
                }
            }
        },

        showFullTmplList: function (params, data) {
            var that = this,
                $moreButton = that.$itemsListSelector.find('.moreBtn');

            that.options.showall = true;

            that.rewriteObjectItem.call(that, params, data);
            $moreButton.hide();
        }
    });

    $.fn.mailtemplateadvancedSelector = function (option) {
        var selfargs = Array.prototype.slice.call(arguments, 1);
        return this.each(function () {
            var $this = $(this),
                data = $this.data('mailtemplateadvancedSelector'),
                options = $.extend({},
                    $.fn.mailtemplateadvancedSelector.defaults,
                    $this.data(),
                    typeof option == 'object' && option);
            if (!data) $this.data('mailtemplateadvancedSelector', (data = new mailtemplateadvancedSelector(this, options)));
            if (typeof option == 'string') data[option].apply(data, selfargs);
        });
    };

    $.fn.mailtemplateadvancedSelector.defaults = $.extend({}, $.fn.advancedSelector.defaults, {
        onechosen: true,
        showSearch: true,
        showSaveButton: false,
        showme: false,
        searchDelay: 500,
        searchFolder: 7,
        searchPageLimit: 1000,
        noresults: resources.TemplateNotFound,
        emptylist: resources.NotAddedAnyTemplates,
        templates: {
            selectorContainer: 'mail-template-selector-container',
            itemList: 'mail-template-selector-items-list'
        }
    });

})(jQuery);