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


var PromoCodeManagement = new function () {

    this.ActivateKey = function () {
        AjaxPro.onLoading = function (b) {
            if (b)
                LoadingBanner.showLoaderBtn("#promoCodeSettings");
            else
                LoadingBanner.hideLoaderBtn("#promoCodeSettings");
        };

        var promocode = jq('#promoCodeSettings_input').val();
        if (promocode && promocode.trim().length !== 0) {
            PromoCodeController.ActivateKey(promocode, function (result) {
                if (result.value.Status == '1') {
                    LoadingBanner.showMesInfoBtn("#promoCodeSettings", result.value.Message, "success");
                    setTimeout(function () { window.location.reload(true) }, 3000);
                }
                else {
                    LoadingBanner.showMesInfoBtn("#promoCodeSettings", result.value.Message, "error");
                }
            });
        }
    };
};