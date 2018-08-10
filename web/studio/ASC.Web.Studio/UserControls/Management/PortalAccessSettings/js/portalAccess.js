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