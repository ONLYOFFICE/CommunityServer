
/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
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