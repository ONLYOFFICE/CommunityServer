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


var ImpersonateManager = new function () {

    this.LoginAsUserEnterAction = function () {
        jq("#confirmationImpersonateUserPanel .middle-button-container .impersonate-btn").trigger("click");
    };

    this.LoginAsUser = function (userId, displayName) {
        jq("#actionMenu").hide();

        jq("#confirmationImpersonateUserPanel .confirmationAction").html(ASC.Resources.Master.ResourceJS.ImpersonateUserConfirmationInfo.format("<b>" + Encoder.htmlEncode(displayName) + "</b>"));

        jq("#confirmationImpersonateUserPanel .middle-button-container .impersonate-btn").off("click").on("click", function () {
            if (jq(this).hasClass("disable")) {
                return;
            }

            var dialog = jq("#confirmationImpersonateUserPanel");

            Teamlab.impersonateUser({}, userId, {
                success: function () {
                    jq.unblockUI();

                    var location = window.location;

                    jq.cookies.set(`reverse_address`, location.href, { path: '/' });
                    jq.cookies.set(`showImpersonateLoginMessage`, true, { path: '/' });
                    location.replace(location.origin);
                },
                error: function (params, errors) {
                    toastr.error(errors[0]);
                },
                after: function () {
                    LoadingBanner.hideLoaderBtn(dialog);
                },
                before: function () {
                    LoadingBanner.showLoaderBtn(dialog);
                }
            });
        });

        StudioBlockUIManager.blockUI("#confirmationImpersonateUserPanel", 500);
        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.EnterAction = 'ImpersonateManager.LoginAsUserEnterAction();';
    };
};