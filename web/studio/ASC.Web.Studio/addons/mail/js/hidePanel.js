/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


(function($) {
    var moreTextWidth = 100;
    var methods = {
        init: function(options) {
            return this.each(function() {

                var $this = $(this);

                $this.empty();

                var maxWidth = $this.width() - moreTextWidth;
                var usedWidth = 0;

                $.each(options.items, function(index, value) {
                    var $html = $(options.item_to_html(value, usedWidth > 0));

                    $html.css({ 'opacity': 0 });
                    $this.append($html);

                    if (usedWidth + $html.outerWidth() < maxWidth) {
                        usedWidth += $html.outerWidth();
                        $html.css({ 'opacity': 1 });
                    } else {
                        $html.remove();
                        var moreText = MailScriptResource.More.replace('%1', options.items.length - index);
                        var more = $('<div class="more_lnk"><span class="gray">' + moreText + '</span></div>');
                        var buttons = [];
                        $.each(options.items, function(index2, val) {
                            if (index2 >= index) {
                                buttons.push({ 'text': val, 'disabled': true });
                            }
                        });
                        more.find('.gray').actionPanel({ 'buttons': buttons });
                        $this.append(more);
                        return false;
                    }
                });
            });
        },

        destroy: function() {

            return this.each(function() {
                var $this = $(this);
                $this.empty();
            });

        }
    };

    $.fn.hidePanel = function(method) {
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        } else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        }
    };

})(jQuery);