/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


var AdmMess = new function() {
    this.SaveSettings = function (btnObj) {

        if (jq(btnObj).hasClass("disable")) return;

        AjaxPro.onLoading = function(b) {
            if (b)
                LoadingBanner.showLoaderBtn("#studio_admMessSettings");
            else
                LoadingBanner.hideLoaderBtn("#studio_admMessSettings");
        };

        AdmMessController.SaveSettings(jq("#chk_studio_admMess").is(":checked"), function(result) {
            var res = result.value;
            LoadingBanner.showMesInfoBtn("#studio_admMessSettings", res.Message, res.Status == 1 ? "success" : "error");
        });
    }
}