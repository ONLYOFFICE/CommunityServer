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


jq(document).ready(function () {
    try {
        var inviteLinkShow = true;

        if (Teamlab.profile.isPortalOwner === true) {
            var users = ASC.Resources.Master.ApiResponses_ActiveProfiles.response;
            for (var i = 0, n = users.length; i < n; i++) {
                if (users[i].isActivated === true && users[i].isOwner === false) {
                    inviteLinkShow = false;
                    break;
                }
            }
            if (inviteLinkShow) {
                jq("#menuInviteUsers").show();

                if (!ASC.Clipboard.enable) {
                    jq("#menuInviteUsersBtn").on("click", function () {
                        PopupKeyUpActionProvider.ClearActions();
                        StudioBlockUIManager.blockUI("#inviteLinkContainer", 550, 350, 0);
                        ASC.InvitePanel.bindClipboardEvent();
                    });
                } else {
                    window.menuInviteUsersClip = ASC.Clipboard.destroy(window.menuInviteUsersClip);

                    var url = jq("#shareInviteUserLink").val();
                    window.menuInviteUsersClip = ASC.Clipboard.create(url, "menuInviteUsersBtn", {
                        onComplete: function () {
                            PopupKeyUpActionProvider.ClearActions();
                            StudioBlockUIManager.blockUI("#inviteLinkContainer", 550, 350, 0);
                            ASC.InvitePanel.bindClipboardEvent();

                            if (typeof (window.toastr) !== "undefined") {
                                toastr.success(ASC.Resources.Master.Resource.LinkCopySuccess);
                            } else {
                                jq("#shareInviteUserLink, #shareInviteUserLinkCopy").yellowFade();
                            }
                        }
                    });
                }
            }
        }
    } catch (e) { }
});