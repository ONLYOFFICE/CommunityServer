/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


window.AdminHelperSettings = new function () {
    var $adminHelperSettings = jq("#adminHelperSettings");
    var $buttonAdminHelperWasClosed = jq("#adminHelperCancel");
    var $buttonDoNotShowItAgain = jq("#doNotShowItAgain");

    var init = function () {
        if (sessionStorage && sessionStorage.getItem("adminHelperClosed") != "true") {
            $adminHelperSettings.show();
        } else {
            $adminHelperSettings.hide();
        }

        $buttonAdminHelperWasClosed.click(closeAdminHelper);
        $buttonDoNotShowItAgain.click(removeAdminHelper);
    };

    var closeAdminHelper = function () {
        sessionStorage.setItem("adminHelperClosed", "true");
        $adminHelperSettings.remove();
    };

    var removeAdminHelper = function closeAdminSettingsHelper() {
        Teamlab.closeAdminHelper({
            success: function () {
                $adminHelperSettings.remove();
            }
        });
    };

    return {
        init: init
    };
}

jq(function () {
    window.AdminHelperSettings.init();
});

