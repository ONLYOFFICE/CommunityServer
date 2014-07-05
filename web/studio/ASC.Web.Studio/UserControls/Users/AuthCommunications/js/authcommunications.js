/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

var AuthCommunications = new function () {
    this.ShowAdminMessageDialog = function () {
        if (jq("#studio_admMessDialog:visible").length > 0 && jq("#studio_admMessage:visible").length == 0) {
            this.SendAdmMail1stState();
            return;
        }

        jq("#GreetingBlock .help-block-signin .signUpBlock .join").removeClass("opened");
        jq("#GreetingBlock .help-block-signin .signUpBlock .mess").addClass("opened");
        jq("#studio_invJoinDialog").hide();
        jq("#studio_admMessDialog").show();
        jq("#studio_admMessInfo").html("");
        jq("#studio_yourEmail").val("");
        jq("#studio_yourSituation").val("");

        jq("#studio_admMessContent").show();
        jq("#studio_admMessage").hide();

        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.CtrlEnterAction = "AuthCommunications.SendAdminMessage();";
    };

    this.SendAdminMessage = function () {
        AjaxPro.onLoading = function (b) {
            if (b) {
                LoadingBanner.showLoaderBtn("#studio_admMessContent");
            } else {
                LoadingBanner.hideLoaderBtn("#studio_admMessContent");
            }
        };

        AuthCommunicationsController.SendAdmMail(jq("#studio_yourEmail").val(), jq("#studio_yourSituation").val(), function (result) {
            var res = result.value;
            if (res.Status == 1) {
                jq("#studio_admMessage").html(res.Message);
                jq("#studio_admMessContent").hide();
                jq("#studio_admMessage").show();

                setTimeout("AuthCommunications.SendAdmMail1stState();", 3000);
            } else {
                jq("#studio_admMessage").html("<div class=\"errorBox\">" + res.Message + "</div>");
            }
        });
    };

    this.SendAdmMail1stState = function () {
        jq("#GreetingBlock .help-block-signin .signUpBlock .mess").removeClass("opened");
        jq("#studio_admMessDialog").hide();
    };

    this.ShowInviteJoinDialog = function () {
        if (jq("#studio_invJoinDialog:visible").length > 0 && jq("#studio_invJoinMessage:visible").length == 0) {
            this.ShowInviteJoin1stState();
            return;
        }
        jq("#GreetingBlock .help-block-signin .signUpBlock .mess").removeClass("opened");
        jq("#GreetingBlock .help-block-signin .signUpBlock .join").addClass("opened");
        jq("#studio_invJoinInfo").html("");
        jq("#studio_joinEmail").val("");

        jq("#studio_admMessDialog").hide();
        jq("#studio_invJoinDialog").show();

        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.CtrlEnterAction = "AuthCommunications.SendInviteJoinMail();";

        jq("#studio_invJoinContent").show();
        jq("#studio_invJoinMessage").hide();
    };

    this.SendInviteJoinMail = function () {
        AjaxPro.onLoading = function (b) {
            if (b) {
                LoadingBanner.showLoaderBtn("#studio_invJoinDialog");
            } else {
                LoadingBanner.hideLoaderBtn("#studio_invJoinDialog");
            }
        };

        AuthCommunicationsController.SendJoinInviteMail(jq("#studio_joinEmail").val(), function (result) {
            var res = result.value;
            if (res.rs1 == 1) {
                jq("#studio_invJoinMessage").html(res.rs2);
                jq("#studio_invJoinContent").hide();
                jq("#studio_invJoinMessage").show();
                setTimeout("AuthCommunications.ShowInviteJoin1stState();", 3000);
            } else {
                jq("#studio_invJoinInfo").html("<div class=\"errorBox\">" + res.rs2 + "</div>");
            }
        });
    };

    this.ShowInviteJoin1stState = function () {
        jq("#GreetingBlock .help-block-signin .signUpBlock .join").removeClass("opened");
        jq("#studio_invJoinDialog").hide();
    };
};