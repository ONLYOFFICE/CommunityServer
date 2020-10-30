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


;
ASC.Controls.ConfirmTfaManager = (function () {
    var $sendCodeButton;
    var $errorTfaActivate;
    var $tfaAuthCode;
    var isSendCodeDisabled = false;

    var init = function () {
        $sendCodeButton = jq("#sendCodeButton");
        $errorTfaActivate = jq("#errorTfaActivate");
        $tfaAuthCode = jq("#tfaAuthcode");

        $sendCodeButton.on("click", function () {
            if (!isSendCodeDisabled)
                validateAuthCode();
            return false;
        });

        $tfaAuthCode.keydown(function (event) {
            if (!e) {
                var e = event;
            }
            var code = e.keyCode || e.which;
            if (code == 13) {
                validateAuthCode();
                return false;
            }
        });
    };

    var validateAuthCode = function () {
        $errorTfaActivate.hide();

        var code = $tfaAuthCode.val().trim();
        if (!code.length) {
            $errorTfaActivate.html(ASC.Resources.Master.Resource.ActivateTfaAppEmptyCode).show();
            return;
        }

        $sendCodeButton.addClass("disable");
        isSendCodeDisabled = true;

        AjaxPro.TfaActivationController.ValidateTfaCode(location.search.substring(1), code, function (result) {
            var res = result.value || result.error;
            if (typeof res.RefererURL != "undefined") {
                location.href = res.RefererURL || "/";
            } else {
                $sendCodeButton.removeClass("disable");
                isSendCodeDisabled = false;
                $errorTfaActivate.html(res.Message || "Error").show();
            }
        });
    };

    return {
        init: init
    };
}());


jq(document).ready(function () {
    ASC.Controls.ConfirmTfaManager.init();
});