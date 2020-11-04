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