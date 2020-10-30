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


CookieSettingsManager = new function () {
    this.Init = function () {
        jq("#saveCookieSettingsBtn").click(CookieSettingsManager.Save);
        
        jq("#lifeTimeTxt").keydown(function (e) {
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
        
        jq("#cookieSettingsOff").on("click", CookieSettingsManager.HideLifeTimeSettings);
        
        jq("#cookieSettingsOn").on("click", CookieSettingsManager.ShowLifeTimeSettings);
    };

    this.Save = function () {
        AjaxPro.onLoading = function (b) {
            if (b)
                LoadingBanner.showLoaderBtn("#studio_cookieSettings");
            else
                LoadingBanner.hideLoaderBtn("#studio_cookieSettings");
        };

        var callback = function (result) {
            var res = result.value;
            if (res.Status == 1) {
                LoadingBanner.showMesInfoBtn("#studio_cookieSettings", res.Message, "success");
            } else {
                LoadingBanner.showMesInfoBtn("#studio_cookieSettings", res.Message, "error");
            }
        };

        if (jq("#cookieSettingsOn").is(":checked")) {
            var lifeTime = parseInt(jq("#lifeTimeTxt").val().trim());
            if (lifeTime > 0) {
                window.CookieSettingsController.Save(lifeTime, callback);
            }
        } else {
            window.CookieSettingsController.Restore(callback);
        } 
    };

    this.HideLifeTimeSettings = function () {
        jq("#lifeTimeSettings").addClass("display-none");
    };
    
    this.ShowLifeTimeSettings = function () {
        jq("#lifeTimeSettings").removeClass("display-none");
    };
};

jq(document).ready(function () {
    CookieSettingsManager.Init();
});