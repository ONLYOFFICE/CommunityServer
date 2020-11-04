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
    window.dropdown = {
        unregHide: function(hide) {
            $(window).unbind('.dropdown', hide);
            $('iframe').each(function() {
                try {
                    if ($(this)[0].contentWindow.document) {
                        $($(this)[0].contentWindow.document).off(".dropdown", hide);
                    }
                } catch(e) {
                }
            });
        },

        regHide: function(hide) {
            dropdown.unregHide(hide);
            setImmediate(function() {
                $(window).on("contextmenu.dropdown click.dropdown resize.dropdown", hide);
                $('iframe').each(function() {
                    try {
                        if ($(this)[0].contentWindow.document) {
                            $($(this)[0].contentWindow.document).on("contextmenu.dropdown click.dropdown", hide);
                        }
                    } catch(e) {
                    }
                });
            });
        },

        unregScroll: function(scroll) {
            $(window).off('.dropdown', scroll);
        },

        regScroll: function(scroll) {
            $(window).on('scroll.dropdown', scroll);
        },

        onClick: function(event) {
            event.preventDefault();
            event.stopPropagation();
            // initiate global event for other dropdowns close
            $(window).click();
        }
    };
})(jQuery);