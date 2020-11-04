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


window.settingsPanel = (function($) {
    var isInit = false,
        panelContent;

    var init = function() {
        if (isInit === false) {
            isInit = true;
            panelContent = $('#settingsContainer');
        }
    };

    var unmarkSettings = function() {
        var $settings = panelContent.children();

        for (var i = 0, n = $settings.length; i < n; i++) {
            var $item = $($settings[i]);
            if ($item.hasClass('active')) {
                $item.toggleClass('active', false);
            }
        }
    };

    var selectItem = function(id) {
        var $item = (panelContent.find('[id="' + id + '"]')).parent();
        if ($item != undefined) {
            $item.toggleClass('active', true);
            panelContent.parent().toggleClass('open', true);
        }
    };

    return {
        init: init,
        unmarkSettings: unmarkSettings,
        selectItem: selectItem
    };

})(jQuery);