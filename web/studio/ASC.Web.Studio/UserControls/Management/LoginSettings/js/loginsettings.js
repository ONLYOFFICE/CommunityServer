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


jq(function () {
    LoginSettings.SetEvent();
});

var LoginSettings = new function () {

    this.SetEvent = function () {
        jq("#loginSettingsSaveBtn").on("click", LoginSettings.SaveNewSettings);

        jq("#studio_loginSettings .textEdit").on("keydown", function (e) {
            // Allow: backspace, delete, tab, escape, enter and .
            if (jq.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 190]) !== -1 ||
                // Allow: Ctrl+A, Command+A
                (e.keyCode === 65 && (e.ctrlKey === true || e.metaKey === true)) ||
                // Allow: home, end, left, right, down, up
                (e.keyCode >= 35 && e.keyCode <= 40)) {
                // let it happen, don't do anything
                return;
            }
            // Ensure that it is a number and stop the keypress
            if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
                e.preventDefault();
            }
        });
    }

    this.SaveNewSettings = function () {

        var attemptsCount = jq("#studio_attemptsCount").val().trim();
        var checkPeriod = jq("#studio_checkPeriod").val().trim();
        var blockTime = jq("#studio_blockTime").val().trim();

        Teamlab.updateLoginSettings({}, { attemptsCount: attemptsCount, blockTime: blockTime, checkPeriod: checkPeriod }, {

            before: function () {
                LoadingBanner.showLoaderBtn("#studio_loginSettings");
            },
            after: function () {
                LoadingBanner.hideLoaderBtn("#studio_loginSettings");
            },
            success: function (params, response) {
                LoadingBanner.showMesInfoBtn("#studio_loginSettings", ASC.Resources.Master.ResourceJS.SuccessfullySaveSettingsMessage, "success");
            },
            error: function (params, errors) {
                LoadingBanner.showMesInfoBtn("#studio_loginSettings", errors[0], "error");
            }
        });
    }
}
