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
window.ASC.Controls.SmsValidationSettings = (function () {
    return {

        SaveSmsValidationSettings: function () {


            AjaxPro.onLoading = function (loading) {
                if (loading) {
                    LoadingBanner.showLoaderBtn("#sms-validation-settings");
                } else {
                    LoadingBanner.hideLoaderBtn("#sms-validation-settings");
                }
            };

            var smsEnable = jq("#chk2FactorAuthEnable").is(":checked");
            var tfaAppEnable = jq("#chk2FactorAppAuthEnable").is(":checked");

            var callback = {
                success: function (_, data) {
                    LoadingBanner.showMesInfoBtn("#studio_smsValidationSettings", ASC.Resources.Master.Resource.SuccessfullySaveSettingsMessage, "success");
                    if (data) {
                        window.location.reload(true);
                    }
                },
                error: function (params, error) {
                    LoadingBanner.showMesInfoBtn("#studio_smsValidationSettings", error[0], "error");
                }
            };

            if (smsEnable) {
                Teamlab.tfaAppAuthSettings("sms", callback);
            } else if (tfaAppEnable) {
                Teamlab.tfaAppAuthSettings("app", callback);
            } else {
                Teamlab.tfaAppAuthSettings("none", callback);
            }
        }
    };
})();

(function () {
    jq(function () {
        jq("#studio_smsValidationSettings").on("click", "#chk2FactorAuthSave:not(.disable)", ASC.Controls.SmsValidationSettings.SaveSmsValidationSettings);

        jq("input[name=\"chk2FactorAuth\"]").change(function () {
            jq("#chk2FactorAuthSave").removeClass("disable");
        });
    });
})();