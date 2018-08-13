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


var AuthCommunications = new function () {
    var initialized = false;

    this.ShowAdminMessageDialog = function () {
        if (!initialized) setBindings();

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
        jq("#studio_admMessContent .middle-button-container .button").addClass("disable");

        jq("#studio_admMessContent").show();
        jq("#studio_admMessage").hide();

        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.CtrlEnterAction = "AuthCommunications.SendAdminMessage();";
        
        function setBindings() {
            jq("#studio_admMessContent").on("keyup", "#studio_yourSituation, #studio_yourEmail", checkBtnEnabled);
            jq("#studio_admMessContent").on("paste", "#studio_yourSituation, #studio_yourEmail", function () {
                setTimeout(checkBtnEnabled, 0);
            });
            initialized = true;

            function checkBtnEnabled() {
                if (jq("#studio_yourSituation").val().trim() && jq("#studio_yourEmail").val().trim())
                    jq("#studio_admMessContent .middle-button-container .button").removeClass("disable");
                else
                    jq("#studio_admMessContent .middle-button-container .button").addClass("disable");
            }
        };
    };

    this.SendAdminMessage = function (btnObj) {
        if (jq(btnObj).hasClass("disable"))
            return;

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