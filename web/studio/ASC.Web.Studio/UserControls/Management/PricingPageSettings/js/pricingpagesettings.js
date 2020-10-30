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


PricingPageSettingsManager = new function () {
    this.Init = function () {
        jq("#pricingPageSettingsBtn").click(PricingPageSettingsManager.Save);
    };

    this.Save = function () {
        AjaxPro.onLoading = function (b) {
            if (b)
                LoadingBanner.showLoaderBtn("#studio_pricingPageSettings");
            else
                LoadingBanner.hideLoaderBtn("#studio_pricingPageSettings");
        };

        window.PricingPageSettingsController.Save(jq("#pricingPageSettingsCbx").is(":checked"), function (result) {
            var res = result.value;
            if (res.Status == 1) {
                LoadingBanner.showMesInfoBtn("#studio_pricingPageSettings", res.Message, "success");
            } else {
                LoadingBanner.showMesInfoBtn("#studio_pricingPageSettings", res.Message, "error");
            }
        });
    };
};

jq(document).ready(function () {
    PricingPageSettingsManager.Init();
});