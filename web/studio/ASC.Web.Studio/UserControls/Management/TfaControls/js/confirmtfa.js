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