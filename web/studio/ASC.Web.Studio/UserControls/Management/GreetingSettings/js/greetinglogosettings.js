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


var GreetingLogoSettingsManager = new function () {

    var _bindAjaxProOnLoading = function () {
        AjaxPro.onLoading = function (b) {
            if (b) {
                LoadingBanner.showLoaderBtn("#studio_greetingLogoSettings");
            } else {
                LoadingBanner.hideLoaderBtn("#studio_greetingLogoSettings");
            }
        };
    };

    var _saveGreetingLogoOptions = function () {
        _bindAjaxProOnLoading();
        GreetingLogoSettingsController.SaveGreetingLogoSettings(jq('#studio_greetingLogoPath').val(),
                                                function (result) {
                                                    //clean logo path input
                                                    jq('#studio_greetingLogoPath').val('');
                                                    LoadingBanner.showMesInfoBtn("#studio_greetingLogoSettings", result.value.Message, result.value.Status == 1 ? "success" : "error");
                                                });
      };

    var _restoreGreetingLogoOptions = function () {
        _bindAjaxProOnLoading();
        GreetingLogoSettingsController.RestoreGreetingLogoSettings(function (result) {
            //clean logo path input
            jq('#studio_greetingLogoPath').val('');

            if (result.value.Status == 1) {
                jq('#studio_greetingLogo').attr('src', result.value.LogoPath);
            }
            LoadingBanner.showMesInfoBtn("#studio_greetingLogoSettings", result.value.Message, result.value.Status == 1 ? "success" : "error");
        });
    };

    var _uploadCompleteGreetingLogo = function (file, response) {
        LoadingBanner.hideLoaderBtn("#studio_greetingLogoSettings");

        var result = eval("(" + response + ")");

        if (result.Success) {
            jq('#studio_greetingLogo').attr('src', result.Message);
            jq('#studio_greetingLogoPath').val(result.Message);
        } else {
            LoadingBanner.showMesInfoBtn("#studio_greetingLogoSettings", result.Message, "error");
        }
    };

    this.Init = function () {
        jq('#saveGreetLogoSettingsBtn').on("click", _saveGreetingLogoOptions);
        jq('#restoreGreetLogoSettingsBtn').on("click", _restoreGreetingLogoOptions);

        if (jq('#studio_logoUploader').length > 0) {
            new AjaxUpload('studio_logoUploader', {
                action: 'ajaxupload.ashx?type=ASC.Web.Studio.UserControls.Management.LogoUploader,ASC.Web.Studio',
                onChange: function (file, ext) {
                    LoadingBanner.showLoaderBtn("#studio_greetingLogoSettings");
                },
                onComplete: _uploadCompleteGreetingLogo
            });
        }

    }
};


jq(function () {
    GreetingLogoSettingsManager.Init();
});