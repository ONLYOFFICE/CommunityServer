/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


(function ($) {
    var resources = ASC.Mail.Resources.MailResource;
    var mailboxadvancedSelector = function(element, options) {
        this.$element = $(element);
        this.options = $.extend({}, $.fn.mailboxadvancedSelector.defaults, options);
        this.init();
    };

    mailboxadvancedSelector.prototype = $.extend({}, $.fn.advancedSelector.Constructor.prototype, {
        constructor: mailboxadvancedSelector,
        initAdvSelectorData: function() {
            var that = this;
            that.refrashSelectorData();
            that.$element.unbind('click.once').bind('click.once', function() {
                that.refrashSelectorData();
            });
        },

        refrashSelectorData: function() {
            var that = this;
            if (!that.$advancedSelector.is(":visible")) {
                return;
            }

            var addresses = $.map(that.options.getAddresses(), function(address) {
                return {
                    title: address.email,
                    id: address.id
                };
            });

            if (that.options.getSelectedIds) {
                that.options.itemsSelectedIds = that.options.getSelectedIds();
            }

            that.rewriteObjectItem(addresses);
        },

        rewriteObjectItem: function(data) {
            var that = this;
            that.items = data;
            that.showItemsListAdvSelector.call(that);
        }
    });

    $.fn.mailboxadvancedSelector = function(option) {
        var selfargs = Array.prototype.slice.call(arguments, 1);
        return this.each(function() {
            var $this = $(this),
                data = $this.data('mailboxadvancedSelector'),
                options = $.extend({},
                    $.fn.mailboxadvancedSelector.defaults,
                    $this.data(),
                    typeof option == 'object' && option);
            var container = $this.parent().find('.advanced-selector-container');
            if (!container.length) {
                $this.data('mailboxadvancedSelector', (data = new mailboxadvancedSelector(this, options)));
            }
            if (typeof option == 'string') {
                data[option].apply(data, selfargs);
            }
        });
    };

    $.fn.mailboxadvancedSelector.defaults = $.extend({}, $.fn.advancedSelector.defaults, {
        showme: true,
        noresults: resources.MailBoxSelectorNoResult,
        noitems: resources.MailBoxSelectorNoItems,
        emptylist: resources.MailBoxSelectorEmptyList
    });

})(jQuery, window, document, document.body);