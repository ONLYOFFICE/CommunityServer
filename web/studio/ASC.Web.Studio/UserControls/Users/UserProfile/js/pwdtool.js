/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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

        StudioBlockUIManager.blockUI("#studio_pwdReminderDialog", 350, 400, 0);

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