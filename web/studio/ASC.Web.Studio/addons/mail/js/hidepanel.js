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
                        var more = $.tmpl('moreLinkTmpl', { moreText: moreText });
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