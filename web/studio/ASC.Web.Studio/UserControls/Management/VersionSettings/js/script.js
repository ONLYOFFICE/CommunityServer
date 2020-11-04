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


var StudioVersionManagement = new function () {

    this.SwitchVersion = function () {
        AjaxPro.onLoading = function (b) {
            if (b) {
                LoadingBanner.showLoaderBtn("#studio_versionSetting");
            } else {
                LoadingBanner.hideLoaderBtn("#studio_versionSetting");
            }
        };
        VersionSettingsController.SwitchVersion(jq('#versionSelector  input:radio[name=version]:checked').val(), function (res) {
            if (res.value.Status == '1') {
                LoadingBanner.showLoaderBtn("#studio_versionSetting");
                setTimeout(function () {
                    window.location.reload(true);
                }, 10000);
            } else {
                LoadingBanner.showMesInfoBtn("#studio_versionSetting", res.value.Message, "error");
            }
        });
    };
};