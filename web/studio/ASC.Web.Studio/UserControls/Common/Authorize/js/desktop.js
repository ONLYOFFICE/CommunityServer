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


window.ASC.Desktop = (function () {
    if (!window["AscDesktopEditor"]) {
        return null;
    }

    var isInit = false;
    var domain = null;

    var init = function () {
        if (isInit === false) {
            isInit = true;

            domain = new RegExp("^http(s)?:\/\/[^\/]+").exec(location)[0];
        }
    };

    var checkpwd = function () {
        var data = {
            domain: domain,
            emailInput: "login",
            pwdInput: "pwd",
        };

        window.AscDesktopEditor.execCommand("portal:checkpwd", JSON.stringify(data));
    };

    return {
        init: init,

        checkpwd: checkpwd,
    };
})();

(function ($) {
    $(function () {
        if (ASC.Desktop) {
            ASC.Desktop.init();
        }
    });
})(jQuery);