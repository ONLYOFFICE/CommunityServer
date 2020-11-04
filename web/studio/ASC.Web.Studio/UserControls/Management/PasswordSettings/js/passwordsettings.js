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


jq(function () {
    jq('#savePasswordSettingsBtn').click(PasswordSettingsManager.SaveSettings);
});

jq(document).ready(function () {
    PasswordSettingsManager.LoadSettings();
});

PasswordSettingsManager = new function () {

    this.LoadSettings = function () {
        PasswordSettingsController.LoadPasswordSettings(function (result) {

            var res = result.value;

            if (res == null)
                return;

            var jsonObj = JSON.parse(res);

            var onSlide = function (event, ui) {
                var value = jq("#slider").slider("value");
                jq("#count").html(value);
            };

            jq("#slider").slider({
                range: "max",
                max: (jq("#slider").attr("data-max") | 0) || 30,
                min: (jq("#slider").attr("data-min") | 0) || 1,
                value: jsonObj.MinLength,
                change: onSlide,
                slide: onSlide,
            });

            jq("#chkUpperCase").prop("checked", jsonObj.UpperCase);
            jq("#chkDigits").prop("checked", jsonObj.Digits);
            jq("#chkSpecSymbols").prop("checked", jsonObj.SpecSymbols);

            onSlide();
        });
    };

    this.SaveSettings = function () {

        AjaxPro.onLoading = function (b) {
            if (b)
                LoadingBanner.showLoaderBtn("#studio_passwordSettings");
            else
                LoadingBanner.hideLoaderBtn("#studio_passwordSettings");
        };

        var jsonObj = {
            "MinLength": jq("#slider").slider("value"),
            "UpperCase": jq("input#chkUpperCase").is(":checked"),
            "Digits": jq("input#chkDigits").is(":checked"),
            "SpecSymbols": jq("input#chkSpecSymbols").is(":checked")
        };

        PasswordSettingsController.SavePasswordSettings(JSON.stringify(jsonObj), function (result) {
            var res = result.value;
            LoadingBanner.showMesInfoBtn("#studio_passwordSettings", res.Message, res.Status == 1 ? "success" : "error");
        });
    };
};