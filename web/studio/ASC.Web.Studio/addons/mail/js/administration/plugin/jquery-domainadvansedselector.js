/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/


/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/

(function ($, win, doc, body) {
    var domainadvancedSelector = function (element, options) {
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
            
            that.$element.unbind('click.onshow').bind('click.onshow', function () {
                that.refrashSelectorData();
            });
        },
        
        refrashSelectorData: function() {
            var that = this;
            var need_refrash = false;
            var domains = $.map(that.options.getDomains(), function(domain) {
                return {
                    title: domain.name,
                    id: domain.id
                };
            });
            var new_items = that.items.slice();
            
            for (var i = 0, length = domains.length; i < length; i++) {
                if (!that.getItemById(domains[i].id, that.items)) {
                    new_items.push(domains[i]);
                    need_refrash = true;
                }
            }

            for (i = 0, length = that.items.length; i < length; i++) {
                if (!that.getItemById(that.items[i].id, domains)) {
                    new_items.splice(i, 1);
                    need_refrash = true;
                }
            }

            if (need_refrash) {
                that.items = new_items;
                that.items = that.items.sort(SortData);
                that.$element.data('items', that.items);
                that.showItemsListAdvSelector.call(that);
            }
        },

        rewriteObjectItem: function (data) {
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

    $.fn.domainadvancedSelector = function (option) {
        var selfargs = Array.prototype.slice.call(arguments, 1);
        return this.each(function () {
            var $this = $(this),
                data = $this.data('domainadvancedSelector'),
                options = $.extend({},
                    $.fn.domainadvancedSelector.defaults,
                    $this.data(),
                    typeof option == 'object' && option);
            if (!data) $this.data('domainadvancedSelector', (data = new domainadvancedSelector(this, options)));
            if (typeof option == 'string') data[option].apply(data, selfargs);
        });
    };

    $.fn.domainadvancedSelector.defaults = $.extend({}, $.fn.advancedSelector.defaults, {});

})(jQuery, window, document, document.body);