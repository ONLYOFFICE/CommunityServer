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

(function ($, win, doc, body) {
    var mailboxadvancedSelector = function (element, options) {
        this.$element = $(element);
        this.options = $.extend({}, $.fn.mailboxadvancedSelector.defaults, options);
        this.init();
    };

    mailboxadvancedSelector.prototype = $.extend({}, $.fn.advancedSelector.Constructor.prototype, {
        constructor: mailboxadvancedSelector,
        initAdvSelectorData: function() {
            var that = this;
            that.refrashSelectorData();
            that.$element.unbind('click.once').bind('click.once', function () {
                that.refrashSelectorData();
            });
        },

        refrashSelectorData: function () {
            var that = this;
            if (!that.$advancedSelector.is(":visible"))
                return;

            var addresses = $.map(that.options.getAddresses(), function(address) {
                return {
                    title: address.email,
                    id: address.id
                };
            });

            if (that.options.getSelectedIds)
                that.options.itemsSelectedIds = that.options.getSelectedIds();

            that.rewriteObjectItem(addresses);
        },

        rewriteObjectItem: function (data) {
            var that = this;
            that.items = data;
            that.showItemsListAdvSelector.call(that);
        }
    });

    $.fn.mailboxadvancedSelector = function (option) {
        var selfargs = Array.prototype.slice.call(arguments, 1);
        return this.each(function () {
            var $this = $(this),
                data = $this.data('mailboxadvancedSelector'),
                options = $.extend({},
                    $.fn.mailboxadvancedSelector.defaults,
                    $this.data(),
                    typeof option == 'object' && option);
            var container = $this.parent().find('.advanced-selector-container');
            if (!container.length) $this.data('mailboxadvancedSelector', (data = new mailboxadvancedSelector(this, options)));
            if (typeof option == 'string') data[option].apply(data, selfargs);
        });
    };

    $.fn.mailboxadvancedSelector.defaults = $.extend({}, $.fn.advancedSelector.defaults, {});

})(jQuery, window, document, document.body);