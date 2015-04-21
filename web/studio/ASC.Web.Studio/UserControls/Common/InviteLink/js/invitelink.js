/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
            if (typeof (ASC) !== "undefined" &&
                ASC.hasOwnProperty("Resources") &&
                ASC.Resources.hasOwnProperty("Master") &&
                ASC.Resources.Master.hasOwnProperty("ApiResponses_Profiles") &&
                ASC.Resources.Master.ApiResponses_Profiles.hasOwnProperty("response") &&
                ASC.Resources.Master.ApiResponses_Profiles.response.length != 0) {
                var users = ASC.Resources.Master.ApiResponses_Profiles.response;
                for (var i = 0, n = users.length; i < n; i++) {
                    if (users[i].isActivated === true && users[i].isOwner === false) {
                        inviteLinkShow = false;
                        break;
                    }
                }
            }
            if (inviteLinkShow) {
                jq("#menuInviteUsers").show();

                var deviceAgent = navigator.userAgent.toLowerCase(),
                    agentID = deviceAgent.match(/(ipad)/);
                if (jq.browser.mobile && agentID || !jq.browser.flashEnabled()) {
                    jq("#menuInviteUsersBtn").on("click", function () {
                        PopupKeyUpActionProvider.ClearActions();
                        StudioBlockUIManager.blockUI("#inviteLinkContainer", 550, 350, 0);
                        ASC.InvitePanel.bindClipboardEvent();
                    });
                } else {
                    if (typeof ZeroClipboard != 'undefined' && ZeroClipboard.moviePath === 'ZeroClipboard.swf') {
                        ZeroClipboard.setMoviePath(ASC.Resources.Master.ZeroClipboardMoviePath);
                    }

                    var clip = new window.ZeroClipboard.Client();
                    clip.addEventListener("mouseDown",
                        function () {
                            var url = jq("#shareInviteUserLink").val();
                            clip.setText(url);
                        });

                    clip.addEventListener("onComplete",
                        function () {
                            PopupKeyUpActionProvider.ClearActions();
                            StudioBlockUIManager.blockUI("#inviteLinkContainer", 550, 350, 0);
                            ASC.InvitePanel.bindClipboardEvent();

                            if (typeof (window.toastr) !== "undefined") {
                                toastr.success(ASC.Resources.Master.Resource.LinkCopySuccess);
                            } else {
                                jq("#shareInviteUserLink, #shareInviteUserLinkCopy").yellowFade();
                            }
                        });

                    clip.glue("menuInviteUsersBtn", "menuInviteUsersBtn");
                }
            }
        }
    } catch (e) { }
});