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


var PasswordTool = new function () {
    this.ShowPwdReminderDialog = function (pswdRecoveryType, userEmail) {
        //pswdRecoveryType can be
        //1 if the password need for change
        //0 if the password need for recovery
        //if pswdRecoveryType == 1 => userEmail must be setted

        if (pswdRecoveryType == 1) {
            jq("#pswdRecoveryDialogPopupHeader").hide();
            jq("#pswdRecoveryDialogText").hide();
            jq("#pswdChangeDialogPopupHeader").show();
            jq("#pswdChangeDialogText").show();

            jq("#studio_emailPwdReminder").val(userEmail);
            jq("#studio_pwdReminderDialog [name='userEmail']").attr("href", "../../addons/mail/#composeto/email=" + userEmail).html(userEmail);
        } else {
            jq("#pswdRecoveryDialogPopupHeader").show();
            jq("#pswdRecoveryDialogText").show();
            jq("#pswdChangeDialogPopupHeader").hide();
            jq("#pswdChangeDialogText").hide();
        }

        jq("#" + jq("#studio_pwdReminderInfoID").val()).html("<div></div>");
        jq("#" + jq("#studio_pwdReminderInfoID").val()).hide();

        StudioBlockUIManager.blockUI("#studio_pwdReminderDialog", 350);

        PopupKeyUpActionProvider.EnterAction = "PasswordTool.RemindPwd();";

        var loginEmail = jq("#login").val();
        if (loginEmail != null && loginEmail != undefined && jq.isValidEmail(loginEmail)) {
            jq("#studio_emailPwdReminder").val(loginEmail);
        }
    };

    this.RemindPwd = function () {
        Teamlab.remindPwd({}, jq("#studio_emailPwdReminder").val().trim(),
            {
                before: function (params) {
                    LoadingBanner.showLoaderBtn("#studio_pwdReminderDialog");
                },
                after: function (params) {
                    LoadingBanner.hideLoaderBtn("#studio_pwdReminderDialog");
                },
                success: function(params, response) {
                    jq.unblockUI();
                    toastr.success(response);
                },
                error: function (params, errors) {
                    toastr.error(errors[0]);
                }
            });

        PopupKeyUpActionProvider.ClearActions();
    };
};