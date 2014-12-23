/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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