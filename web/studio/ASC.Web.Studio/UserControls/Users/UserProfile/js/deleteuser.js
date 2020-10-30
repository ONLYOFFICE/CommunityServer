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


var ProfileManager = new function () {
    this.RemoveUserEnterAction = function () {
        jq("#confirmationDeleteUserPanel .middle-button-container .remove-btn").click();
    };
    this.RemoveUser = function (userId, displayName, userName, callback) {
        jq("#actionMenu").hide();

        jq("#confirmationDeleteUserPanel .confirmationAction").html(jq.format(ASC.Resources.Master.ConfirmRemoveUser, "<b>" + Encoder.htmlEncode(displayName) + "</b>"));

        jq("#confirmationDeleteUserPanel .middle-button-container .remove-btn").unbind("click").bind("click", function () {
            var dialog = jq("#confirmationDeleteUserPanel");

            Teamlab.removeUser({}, userId, {
                success: function () {
                    jq.unblockUI();

                    if (ASC.People.Resources.PeopleResource.SuccessfullyDeleteUserInfoMessage)
                        toastr.success(ASC.People.Resources.PeopleResource.SuccessfullyDeleteUserInfoMessage);

                    if (callback)
                        callback();
                    else
                        window.location.reload(true);

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
        
        jq("#confirmationDeleteUserPanel .middle-button-container .reassign-btn").unbind("click").bind("click", function () {
            window.location.replace("Reassigns.aspx?delete=true&user=" + encodeURIComponent(userName));
        });

        StudioBlockUIManager.blockUI("#confirmationDeleteUserPanel", 500);
        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.EnterAction = 'ProfileManager.RemoveUserEnterAction();';
    };
};