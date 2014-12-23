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

(function ($) {

    // action panel will be drawn under its parent element and
    // its right corner will be shifted on this value from parent right corner

    var _menu_click = function(e) {
        e.stopPropagation();
    };

    var methods = {

        init: function(options) {

            return this.each(function() {

                var $this = $(this),
                    ap_data = $this.data('actionPanel');

                // If the plugin hasn't been initialized yet
                if (!ap_data) {
                    $(this).data('actionPanel', {
                        target: $this,
                        options: options
                    });

                    ap_data = $this.data('actionPanel');

                    ap_data['hide'] = function () {
                        // remove html if exists
                        ap_data['$html'] && ap_data['$html'].remove();
                        $this.removeClass('active');
                        $this.off('.actionPanel').on('click.actionPanel', methods._show);
                        dropdown.unregHide(ap_data['hide']);
                        dropdown.unregScroll(ap_data['scroll']);
                    };

                    $this.on('click.actionPanel', methods._show);
                    $this.on('remove', methods._destroy);
                }
                else {
                    ap_data['options'] = options;
                }

                if (true === options.show)
                    methods._show.apply(this);
            });
        },

        _destroy: function() {
            var $this = $(this),
                ap_data = $this.data('actionPanel');

            ap_data['hide']();
            $this.off('.actionPanel');
            $this.removeData('actionPanel');
        },

        destroy: function() {
            return this.forEach(methods._destroy);
        },

        _show: function (event) {
            // handle click event if it exists (method was called as callback on click event)
            event && dropdown.onClick(event);

            var $this = $(this);

            if ($this.attr('disabled')) return;

            var ap_data = $this.data('actionPanel');

            $this.off('.actionPanel').on('click.actionPanel', function (e) {
                dropdown.onClick(e);
                ap_data['hide']();
            });
            dropdown.regHide(ap_data['hide']);

            $this.addClass('active');

            var html = '<div class="actionPanel';
            if (ap_data.options.css)
                html += ' ' + ap_data.options.css;
            html += '"><div class="actionPanelContent"></div></div>';
            var $html = $(html);
            $.each(ap_data.options.buttons, function (index, value) {
                var $add = $('<div class="action ' +
                (value.css_class != undefined ? value.css_class : "") +
                (value.disabled != undefined && true == value.disabled ? '' : ' active') +
                '" isActive="' + (value.disabled != undefined ? !value.disabled : "true") +
                '" title="' + (value.title != undefined ? value.title : value.text) + '">' + value.text +
                 (value.explanation != undefined ? ' <span class="explanation">' + value.explanation + '</span>' : '') +
                 '</div>');
                if (!value.disabled) {
                    $add.click(function(e) {
                        ap_data['hide']();
                        return value.handler(e, value);
                    });
                }
                $html.find('.actionPanelContent').append($add);
            });

            $html.data('actionPanel', { target: $this });

            //ToDo: dirty trick - refactore
            var $body = $(document.body);
            $body.append($html);
            $html.css({ opacity: 0 });
            $html.show();

            var offset = $this.offset();

            var x = offset.left;

            $html.css({ left: x, top: methods._getY(ap_data.options.horizontal_target ? $this.find(ap_data.options.horizontal_target) : $this, $html) });
            $html.click(_menu_click);

            var arrow = $this.find('.arrow-down');
            if(0 == arrow.length)
                arrow = $this.find('.down_arrow');

            if (0 != arrow.length) {
                var right = $html[0].offsetWidth - arrow.offset().left - Math.ceil(arrow[0].offsetWidth / 2) + x;
                // right minus magic 1px for some browsers
                if (!$.browser.mozilla)
                    right = right - 1;

            }

            ap_data['$html'] = $html;
            $html.css({ opacity: 1 });

            ap_data['scroll'] = function() {
                $html.css({ top: methods._getY($this, $html) });
            };
            dropdown.regScroll(ap_data['scroll']);
        },
        
        _getY: function($target, $html) {
            var y = $target.offset().top + $target.height();
            if (y + $html[0].offsetHeight > $(document).height()) {
                y = $target.offset.top - $html[0].offsetHeight;
                $html.css('margin', '0 0 4px 0');
            }
            return y;
        },

        hide: function() {
            return this.each(function () {
                var $this = $(this);
                if (!$this.is('.active'))
                    return;
                $this.data('actionPanel')['hide']();
            });
        }
    };

    $.fn.actionPanel = function(method) {

        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        } else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        }

    };

})(jQuery);