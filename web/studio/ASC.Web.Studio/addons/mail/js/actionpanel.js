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

            if ($this.hasClass('menuActionMore') && !$this.hasClass('unlockAction')) {
                return;
            } else {
                $this.addClass('active');
            }

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