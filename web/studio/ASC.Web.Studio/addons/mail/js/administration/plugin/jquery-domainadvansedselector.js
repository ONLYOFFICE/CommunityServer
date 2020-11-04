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


(function($) {
    var domainadvancedSelector = function(element, options) {
        this.$element = $(element);
        this.options = $.extend({}, $.fn.domainadvancedSelector.defaults, options);
        this.init();
    };

    domainadvancedSelector.prototype = $.extend({}, $.fn.advancedSelector.Constructor.prototype, {
        constructor: domainadvancedSelector,
        initAdvSelectorData: function() {
            var that = this,
                data = [];

            var domains = this.options.getDomains();
            for (var i = 0, length = domains.length; i < length; i++) {
                data.push({
                    title: domains[i].name,
                    id: domains[i].id
                });
            }
            if (data.length > 0) {
                that.rewriteObjectItem.call(that, data);
            }

            that.$element.unbind('click.onshow').bind('click.onshow', function() {
                that.refrashSelectorData();
            });
        },

        refrashSelectorData: function() {
            var that = this;
            var needRefrash = false;
            var domains = $.map(that.options.getDomains(), function(domain) {
                return {
                    title: domain.name,
                    id: domain.id
                };
            });
            var newItems = that.items.slice();

            for (var i = 0, length = domains.length; i < length; i++) {
                if (!that.getItemById(domains[i].id, that.items)) {
                    newItems.push(domains[i]);
                    needRefrash = true;
                }
            }

            for (i = 0, length = that.items.length; i < length; i++) {
                if (!that.getItemById(that.items[i].id, domains)) {
                    newItems.splice(i, 1);
                    needRefrash = true;
                }
            }

            if (needRefrash) {
                that.items = newItems;
                that.items = that.items.sort(SortData);
                that.$element.data('items', that.items);
                that.showItemsListAdvSelector.call(that);
            }
        },

        rewriteObjectItem: function(data) {
            var that = this;
            that.items = data;
            that.items = that.items.sort(SortData);
            that.$element.data('items', that.items);
            that.showItemsListAdvSelector.call(that);
        },

        getItemById: function(id, itemList) {
            for (var i = 0, len = itemList.length; i < len; i++) {
                if (id == itemList[i].id) {
                    return itemList[i];
                }
            }
            return undefined;
        }
    });

    $.fn.domainadvancedSelector = function(option) {
        var selfargs = Array.prototype.slice.call(arguments, 1);
        return this.each(function() {
            var $this = $(this),
                data = $this.data('domainadvancedSelector'),
                options = $.extend({},
                    $.fn.domainadvancedSelector.defaults,
                    $this.data(),
                    typeof option == 'object' && option);
            if (!data) {
                $this.data('domainadvancedSelector', (data = new domainadvancedSelector(this, options)));
            }
            if (typeof option == 'string') {
                data[option].apply(data, selfargs);
            }
        });
    };

    $.fn.domainadvancedSelector.defaults = $.extend({}, $.fn.advancedSelector.defaults, {});

})(jQuery, window, document, document.body);