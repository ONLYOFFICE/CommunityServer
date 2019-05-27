/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
            window.location.replace("reassigns.aspx?delete=true&user=" + encodeURIComponent(userName));
        });

        StudioBlockUIManager.blockUI("#confirmationDeleteUserPanel", 500, 500, 0);
        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.EnterAction = 'ProfileManager.RemoveUserEnterAction();';
    };
};