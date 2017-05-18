/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

    // action panel will be drawn under its parent element and
    // its right corner will be shifted on this value from parent right corner

    var menuClick = function(e) {
        e.stopPropagation();
    };

    var methods = {
        init: function(options) {

            return this.each(function() {

                var $this = $(this),
                    apData = $this.data('actionPanel');

                // If the plugin hasn't been initialized yet
                if (!apData) {
                    $(this).data('actionPanel', {
                        target: $this,
                        options: options
                    });

                    apData = $this.data('actionPanel');

                    apData['hide'] = function() {
                        // remove html if exists
                        apData['$html'] && apData['$html'].remove();
                        $this.removeClass('active');
                        $this.off('.actionPanel').on('click.actionPanel', methods._show);
                        dropdown.unregHide(apData['hide']);
                        dropdown.unregScroll(apData['scroll']);
                    };

                    $this.on('click.actionPanel', methods._show);
                    $this.on('remove', methods._destroy);
                } else {
                    apData['options'] = options;
                }

                if (true === options.show) {
                    methods._show.apply(this);
                }
            });
        },

        _destroy: function() {
            var $this = $(this),
                apData = $this.data('actionPanel');

            apData['hide']();
            $this.off('.actionPanel');
            $this.removeData('actionPanel');
        },

        destroy: function() {
            return this.forEach(methods._destroy);
        },

        _show: function(event) {
            // handle click event if it exists (method was called as callback on click event)
            event && dropdown.onClick(event);

            var $this = $(this);

            if ($this.attr('disabled')) {
                return;
            }

            var apData = $this.data('actionPanel');

            $this.off('.actionPanel').on('click.actionPanel', function(e) {
                dropdown.onClick(e);
                apData['hide']();
            });
            dropdown.regHide(apData['hide']);

            $this.addClass('active');

            var html = $.tmpl("actionPanelTmpl", apData.options);
            var $html = $(html);
            
            $.each(apData.options.buttons, function(index, value) {
                var $add = $.tmpl("actionPanelItemTmpl", value);
                if (!value.disabled) {
                    $add.click(function(e) {
                        apData['hide']();
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

            $html.css({ left: x, top: methods._getY(apData.options.horizontal_target ? $this.find(apData.options.horizontal_target) : $this, $html) });
            $html.click(menuClick);

            apData['$html'] = $html;
            $html.css({ opacity: 1 });

            apData['scroll'] = function() {
                $html.css({ top: methods._getY($this, $html) });
            };
            dropdown.regScroll(apData['scroll']);
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
            return this.each(function() {
                var $this = $(this);
                if (!$this.is('.active')) {
                    return;
                }
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