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


var GreetingSettingsManager = new function () {

    var _bindAjaxProOnLoading = function () {
        AjaxPro.onLoading = function (b) {
            if (b) {
                LoadingBanner.showLoaderBtn("#studio_greetingSettings");
            } else {
                LoadingBanner.hideLoaderBtn("#studio_greetingSettings");
            }
        };
    };

    var _saveGreetingOptions = function () {
        _bindAjaxProOnLoading();
        GreetingSettingsController.SaveGreetingSettings(jq('#studio_greetingHeader').val(),
                                                function (result) {
                                                    LoadingBanner.showMesInfoBtn("#studio_greetingSettings", result.value.Message, result.value.Status == 1 ? "success" : "error");
                                                });
      };

    var _restoreGreetingOptions = function () {
        _bindAjaxProOnLoading();

        GreetingSettingsController.RestoreGreetingSettings(function (result) {
            if (result.value.Status == 1) {
                jq('#studio_greetingHeader').val(result.value.CompanyName);
            }
            LoadingBanner.showMesInfoBtn("#studio_greetingSettings", result.value.Message, result.value.Status == 1 ? "success" : "error");

        });
    };

    var _uploadCompleteGreetingLogo = function (file, response) {
        LoadingBanner.hideLoaderBtn("#studio_greetingSettings");

        var result = eval("(" + response + ")");

        if (!result.Success) {
            LoadingBanner.showMesInfoBtn("#studio_greetingSettings", result.Message, "error");
        }
    };

    this.Init = function () {
        jq('#saveGreetSettingsBtn').on("click", _saveGreetingOptions);
        jq('#restoreGreetSettingsBtn').on("click", _restoreGreetingOptions);
    }
};


jq(function () {
    GreetingSettingsManager.Init();
});