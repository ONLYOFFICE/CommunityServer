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
    jq("input[name='PortalAccess']").click(PortalAccess.SwitchAccessType);
    jq("input[type=radio][name=PortalAccess][checked=checked]").prop("checked", true);
    jq("#cbxRegisterUsers[checked=checked]").prop("checked", true);
    PortalAccess.SwitchAccessType();
});

var PortalAccess = new function () {
    this.SwitchAccessType = function () {
        if (jq("#radioPublicPortal").is(":checked")) {
            jq("#cbxRegisterUsersContainer").show();
        } else {
            jq("#cbxRegisterUsersContainer").hide();
        }
    };

    this.SaveSettings = function (btnObj) {

        if (jq(btnObj).hasClass("disable")) return;

        AjaxPro.onLoading = function (b) {
            if (b) {
                jq("#studio_portalAccessSettings input").prop("disabled", true);
                LoadingBanner.showLoaderBtn("#studio_portalAccessSettings");
            } else {
                jq("#studio_portalAccessSettings input").prop("disabled", false);
                LoadingBanner.hideLoaderBtn("#studio_portalAccessSettings");
            }
        };

        window.PortalAccessController.SaveSettings(jq("#radioPublicPortal").is(":checked"), jq("#cbxRegisterUsers").is(":checked"), function (result) {
            var res = result.value;
            LoadingBanner.showMesInfoBtn("#studio_portalAccessSettings", res.Message, res.Status == 1 ? "success" : "error");
            if (res.Status == 1) {
                window.location.reload();
            }
        });
    };
}