(function($) {
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

    $.fn.mailboxadvancedSelector.defaults = $.extend({}, $.fn.advancedSelector.defaults, {});

})(jQuery, window, document, document.body);