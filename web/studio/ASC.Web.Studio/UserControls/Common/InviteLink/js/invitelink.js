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


jq(document).ready(function () {
    try {
        var inviteLinkShow = true;

        if (Teamlab.profile.isPortalOwner === true
            && !ASC.Desktop) {
            var users = window.UserManager.getAllUsers(true);
            for (var userId in users) {
                if (!users.hasOwnProperty(userId)) continue;
                var user = users[userId];
                if (user.isActivated === true && user.isOwner === false) {
                    inviteLinkShow = false;
                    break;
                }
            }
            if (inviteLinkShow) {
                jq("#menuInviteUsers").show();

                if (!ASC.Clipboard.enable) {
                    jq("#menuInviteUsersBtn").on("click", function () {
                        PopupKeyUpActionProvider.ClearActions();
                        StudioBlockUIManager.blockUI("#inviteLinkContainer", 550);
                        ASC.InvitePanel.bindClipboardEvent();
                    });
                } else {
                    window.menuInviteUsersClip = ASC.Clipboard.destroy(window.menuInviteUsersClip);

                    var url = jq("#shareInviteUserLink").val();
                    window.menuInviteUsersClip = ASC.Clipboard.create(url, "menuInviteUsersBtn", {
                        onComplete: function () {
                            PopupKeyUpActionProvider.ClearActions();
                            StudioBlockUIManager.blockUI("#inviteLinkContainer", 550);
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