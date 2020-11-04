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


ASC.Controls.ColorThemesSettings = function () {
    var isInit = false;

    var init = function() {
        if (isInit) {
            return;
        }
        isInit = true;

        jq("input[name='colorTheme']").on("click", function() {
            var theme = jq(this).val(),
                $preview = jq(".preview-theme-image");
            
            jq("input[name='colorTheme']").each(function() {
                var className = jq(this).val();
                if ($preview.hasClass(className)) {
                    $preview.removeClass(className);
                }
            });
            $preview.addClass(theme);
        });

        jq("#colorThemeBlock .button.blue").on("click", function() {
            saveColorThemeSettings();
        });

        var saveColorThemeSettings = function() {
            var color = jq("input[name='colorTheme']:checked").val();
            Teamlab.setColorTheme({ color: color }, color, {
                success: function (params, response) {
                    jq("body").addClass(params.color);
                    window.location.reload(true);
                }
            });
        };
    };
    return {
        init: init
    };
}();

jq(function () {
    ASC.Controls.ColorThemesSettings.init();
})