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


window.ASC.Controls.PasswordSettingsManager = new function () {

    return {

        LoadSettings: function () {

            Teamlab.getPasswordSettings({}, {
                success: function (_, settings) {

                    if (settings == null)
                        return;

                    var onSlide = function (event, ui) {
                        setTimeout(function () {
                            var value = jq("#slider").slider("values").join(" - ");
                            jq("#count").text(value);
                        }, 0);
                    }

                    jq("#slider").slider({
                        range: true,
                        min: settings.limitMinLength,
                        max: settings.limitMaxLength,
                        values: [settings.minLength, settings.maxLength],
                        change: onSlide,
                        slide: onSlide
                    });

                    jq("#chkUpperCase").prop("checked", settings.upperCase);
                    jq("#chkDigits").prop("checked", settings.digits);
                    jq("#chkSpecSymbols").prop("checked", settings.specSymbols);

                    onSlide();
                },
                error: function (_, errors) {
                    LoadingBanner.showMesInfoBtn("#studio_passwordSettings", ASC.Resources.Master.ResourceJS.CommonJSErrorMsg, "error");
                }
            });
        },

        SaveSettings: function () {

            var maxLength = jq("#slider").slider("values", 1),
                minLength = jq("#slider").slider("values", 0),
                upperCase = jq("input#chkUpperCase").is(":checked"),
                digits = jq("input#chkDigits").is(":checked"),
                specSymbols = jq("input#chkSpecSymbols").is(":checked");

            Teamlab.setPasswordSettings(maxLength, minLength, upperCase, digits, specSymbols, {
                success: function () {
                    LoadingBanner.showMesInfoBtn("#studio_passwordSettings", ASC.Resources.Master.ResourceJS.SuccessfullySaveSettingsMessage, "success");
                },
                error: function (_, errors) {
                    LoadingBanner.showMesInfoBtn("#studio_passwordSettings", errors[0], "error");
                },
                before: function () {
                    LoadingBanner.showLoaderBtn("#studio_passwordSettings");
                },
                after: function () {
                    LoadingBanner.hideLoaderBtn("#studio_passwordSettings");
                }
            });
        }
    }
};

jq(function () {

    window.ASC.Controls.PasswordSettingsManager.LoadSettings();

    jq('#savePasswordSettingsBtn').on("click", window.ASC.Controls.PasswordSettingsManager.SaveSettings);
});