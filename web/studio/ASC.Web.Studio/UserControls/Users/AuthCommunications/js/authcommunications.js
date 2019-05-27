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
    var adminMessageInitialized = false,
        joinDialogInitialized = false;

    var $messLink = jq("#GreetingBlock .help-block-signin .signUpBlock .mess"),
        $messDialog = jq("#studio_admMessDialog"),
        $messContent = jq("#studio_admMessContent"),
        $messEmail = jq("#studio_yourEmail"),
        $messSituation = jq("#studio_yourSituation"),
        $messBtn = $messContent.find(".middle-button-container .button"),

        $joinLink = jq("#GreetingBlock .help-block-signin .signUpBlock .join"),
        $joinDialog = jq("#studio_invJoinDialog"),
        $joinContent = jq("#studio_invJoinContent"),
        $joinEmail = jq("#studio_joinEmail"),
        $joinBtn = $joinContent.find(".middle-button-container .button");

    this.ShowAdminMessageDialog = function () {
        if (!adminMessageInitialized) setAdminMessageBindings();

        if ($messDialog.is(":visible")) {
            this.SendAdmMail1stState();
            return;
        }

        $joinLink.removeClass("opened");
        $joinDialog.hide();

        $messLink.addClass("opened");
        $messDialog.show();
        $messSituation.val("");
        $messEmail.val("");
        $messBtn.addClass("disable");

        function setAdminMessageBindings() {
            $messSituation.on("input", checkMessBtnEnabled);
            $messEmail.on("input", checkMessBtnEnabled);

            adminMessageInitialized = true;

            function checkMessBtnEnabled() {
                var enable = true;

                if (!$messSituation.val().trim())
                    enable = false;

                if (!jq.isValidEmail($messEmail.val()))
                    enable = false;

                if (enable)
                    $messBtn.removeClass("disable");
                else
                    $messBtn.addClass("disable");
            }
        };
    };

    this.SendAdminMessage = function () {
        if ($messBtn.hasClass("disable"))
            return;

        AjaxPro.onLoading = function (b) {
            if (b) {
                LoadingBanner.showLoaderBtn($messContent);
            } else {
                LoadingBanner.hideLoaderBtn($messContent);
            }
        };

        window.AuthCommunicationsController.SendAdmMail($messEmail.val().trim(), $messSituation.val().trim(), function (result) {
            var res = result.value;
            LoadingBanner.showMesInfoBtn($messContent, res.Message, res.Status == 1 ? "success" : "error");
        });
    };

    this.SendAdmMail1stState = function () {
        $messLink.removeClass("opened");
        $messDialog.hide();
    };

    this.ShowInviteJoinDialog = function () {
        if (!joinDialogInitialized) setJoinDialogBindings();

        if ($joinDialog.is(":visible")) {
            this.ShowInviteJoin1stState();
            return;
        }

        $messLink.removeClass("opened");
        $messDialog.hide();

        $joinLink.addClass("opened");
        $joinDialog.show();
        $joinEmail.val("");
        $joinBtn.addClass("disable");

        function setJoinDialogBindings() {
            $joinEmail.on("input", checkJoinBtnEnabled);

            joinDialogInitialized = true;

            function checkJoinBtnEnabled() {
                if (jq.isValidEmail($joinEmail.val()))
                    $joinBtn.removeClass("disable");
                else
                    $joinBtn.addClass("disable");
            }
        };
    };

    this.SendInviteJoinMail = function () {
        if ($joinBtn.hasClass("disable"))
            return;

        AjaxPro.onLoading = function (b) {
            if (b) {
                LoadingBanner.showLoaderBtn($joinContent);
            } else {
                LoadingBanner.hideLoaderBtn($joinContent);
            }
        };

        window.AuthCommunicationsController.SendJoinInviteMail($joinEmail.val().trim(), function (result) {
            var res = result.value;
            LoadingBanner.showMesInfoBtn($joinContent, res.Message, res.Status == 1 ? "success" : "error");
        });
    };

    this.ShowInviteJoin1stState = function () {
        $joinLink.removeClass("opened");
        $joinDialog.hide();
    };
};