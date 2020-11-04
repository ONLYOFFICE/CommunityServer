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