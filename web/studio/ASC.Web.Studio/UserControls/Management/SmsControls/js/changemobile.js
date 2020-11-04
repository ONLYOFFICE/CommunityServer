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
window.ASC.Controls.UserMobilePhoneManager = (function () {
    var openDialog = function () {
        jq("#changeMobileContent").show();
        jq("#changeMobileProgress, #changeMobileResult").hide();

        StudioBlockUIManager.blockUI("#studio_mobilePhoneChangeDialog", 400);

        PopupKeyUpActionProvider.EnterAction = "jq(\"#changeMobileSend\").click();";
    };

    var sendNotify = function () {
        jq("#changeMobileContent").hide();
        jq("#changeMobileProgress").show();

        var userInfoId = jq("#hiddenUserInfoId").val();
        AjaxPro.ChangeMobileNumber.SendNotificationToChange(userInfoId, function (result) {
            if (result.error) {
                toastr.error(result.error.Message);
                PopupKeyUpActionProvider.CloseDialog();
            } else {
                if (userInfoId) {
                    jq("#changeMobileProgress").hide();
                    jq("#changeMobileResult").show();
                }

                if (result.value.length) {
                    window.location.href = result.value;
                } else {
                    window.location.reload(true);
                }
            }
        });
    };

    return {
        openDialog: openDialog,
        sendNotify: sendNotify
    };
})();

(function () {
    jq(function () {
        jq("#changeMobileSend").on("click", function () {
            ASC.Controls.UserMobilePhoneManager.sendNotify();
        });
    });
})();