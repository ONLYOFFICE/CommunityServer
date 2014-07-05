/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
(function($) {
    var _more_text_width = 100;
    var methods = {
        init: function(options) {

            return this.each(function() {

                var $this = $(this);

                $this.empty();

                var max_width = $this.width() - _more_text_width;
                var used_width = 0;

                $.each(options.items, function(index, value) {
                    var $html = $(options.item_to_html(value, used_width > 0));

                    $html.css({ 'opacity': 0 });
                    $this.append($html);

                    if (used_width + $html.outerWidth() < max_width) {
                        used_width += $html.outerWidth();
                        $html.css({ 'opacity': 1 });
                    }
                    else {
                        $html.remove();
                        var more_text = MailScriptResource.More.replace('%1', options.items.length - index);
                        var more = $('<div class="more_lnk"><span class="gray">' + more_text + '</span></div>');
                        var buttons = [];
                        $.each(options.items, function(index2, value) {
                            if (index2 >= index)
                                buttons.push({ 'text': value, 'disabled': true });
                        });
                        more.find('.gray').actionPanel({ 'buttons': buttons});
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
            })

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