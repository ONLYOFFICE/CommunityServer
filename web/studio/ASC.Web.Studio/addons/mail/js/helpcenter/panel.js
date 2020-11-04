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


window.helpPanel = (function($) {
    var isInit = false,
        panelContent;

    var init = function() {
        if (isInit === false) {
            isInit = true;
            panelContent = $('#studio_sidePanel .help-center');
        }
    };

    var unmarkSettings = function() {
        var $sections = $(panelContent).find('.menu-sub-list').children();

        if ($(panelContent).hasClass('active')) {
            $(panelContent).toggleClass('active');
        }

        if ($(panelContent).hasClass('currentCategory')) {
            $(panelContent).toggleClass('currentCategory');
        }


        for (var i = 0, n = $sections.length; i < n; i++) {
            var $item = $($sections[i]);
            if ($item.hasClass('active')) {
                $item.toggleClass('active', false);
            }
        }
    };

    var selectItem = function(number) {
        if (number == 'all') {
            $(panelContent).toggleClass('active', true);
        } else {
            var $sections = $(panelContent).find('.menu-sub-list').children();
            $($sections[number]).toggleClass('active', true);
        }

    };

    return {
        init: init,
        selectItem: selectItem,
        unmarkSettings: unmarkSettings
    };

})(jQuery);