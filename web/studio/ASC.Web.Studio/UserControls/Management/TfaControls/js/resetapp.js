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


;
window.ASC.Controls.TfaAppResetApp = (function () {
    var $content = jq("#resetAppContent");
    var $errorBox = jq("#errorResetApp");

    var init = function () {
        jq("#resetAppButton").on("click", function () {
            $errorBox.hide();
            Teamlab.tfaAppNewApp(jq("#tfaHiddenUserInfoId")[0].value, {
                success: function () {
                    toastr.success(ASC.Resources.Master.Resource.SavedTitle);
                    PopupKeyUpActionProvider.CloseDialog();
                    window.location.reload(true);
                    return false;
                },
                error: function (params, error) {
                    $errorBox.html(error[0] || "Error").show();
                }
            });
        });

        jq("#resetAppClose").on("click", function () {
            PopupKeyUpActionProvider.CloseDialog();
            return false;
        });

        var openDialog = function () {
            $content.show();
            StudioBlockUIManager.blockUI("#studio_resetAppDialog", 400);
        };

        jq("#tfaAppResetApp").on("click", openDialog);
    };

    return {
        init: init
    };
})();

jq(document).ready(function () {
    window.ASC.Controls.TfaAppResetApp.init();
});