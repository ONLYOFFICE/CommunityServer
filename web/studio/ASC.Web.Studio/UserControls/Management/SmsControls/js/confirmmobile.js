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


;
ASC.Controls.ConfirmMobileManager = function () {
    var init = function () {
        if (!jq("#primaryPhone:visible").length) {
            timerCodeStart();
        }
    };

    var sendAuthCode = function () {
        jq("#errorMobileActivate").hide();

        var mobilePhone = jq("#primaryPhone").val().trim();
        if (!mobilePhone.length) {
            jq("#errorMobileActivate").html(ASC.Resources.Master.Resource.ActivateMobilePhoneEmptyPhoneNumber).show();
            jq("#primaryPhone").val(mobilePhone);
            return;
        }

        jq("#sendPhoneButton").addClass("disable");

        AjaxPro.MobileActivationController.SaveMobilePhone(mobilePhone, sendAuthCodeCallback);
    };

    var sendAuthCodeAgain = function () {
        jq("#errorMobileActivate").hide();
        jq("#getCodeAgainButton, #sendCodeButton").addClass("disable");

        AjaxPro.MobileActivationController.SendSmsCodeAgain(sendAuthCodeCallback);
    };

    var sendAuthCodeCallback = function (result) {
        jq("#sendPhoneButton, #sendCodeButton").removeClass("disable");

        var res = result.value || result.error;
        if (res.phoneNoise) {
            if (res.confirm) {
                jq("#mobileNumberPanel").hide();
                jq("#phoneNoise").html(res.phoneNoise);
                jq("#mobileCodePanel").show();
                jq("#phoneAuthcode").val("").focus();

                timerCodeStart();
            } else {
                location.href = res.RefererURL || "/";
            }
        } else {
            jq("#errorMobileActivate").html(res.Message).show();
        }
    };

    var validateAuthCode = function () {
        jq("#errorMobileActivate").hide();

        var code = jq("#phoneAuthcode").val().trim();
        if (!code.length) {
            jq("#errorMobileActivate").html(ASC.Resources.Master.Resource.ActivateMobilePhoneEmptyCode).show();
            return;
        }

        jq("#sendCodeButton").addClass("disable");

        AjaxPro.MobileActivationController.ValidateSmsCode(code, function (result) {
            var res = result.value || result.error;
            if (typeof res.RefererURL != "undefined") {
                location.href = res.RefererURL || "/";
            } else {
                jq("#sendCodeButton").removeClass("disable");
                jq("#errorMobileActivate").html(res.Message || "Error").show();
            }
        });
    };

    var timerCodeStart = function () {
        timerCode(31);
    };

    var timerCode = function (start) {
        var span = "#getCodeAgainButton span";
        var time = (start || jq(span).data("time")) | 0;

        time -= 1;
        if (time > 0) {
            jq(span).text(" (" + time + ")").data("time", time);

            setTimeout(timerCode, 1000);
        } else {
            jq(span).text("");
            jq("#getCodeAgainButton").removeClass("disable");
        }
    };

    return {
        init: init,

        sendAuthCode: sendAuthCode,
        sendAuthCodeAgain: sendAuthCodeAgain,
        validateAuthCode: validateAuthCode
    };
}();

(function () {
    jq(function () {
        jq(".mobilephone-panel").on("click", "#sendPhoneButton:not(.disable)", function () {
            ASC.Controls.ConfirmMobileManager.sendAuthCode();
            return false;
        });

        if (jq("#primaryPhone").length) {
            var country = jq("#primaryPhone").attr("data-country");
            PhoneController.Init(jq("#primaryPhone"), CountriesManager.countriesList, [country, "US"]);
        }

        jq("#primaryPhone").keyup(function (event) {
            if (!e) {
                var e = event;
            }
            var code = e.keyCode || e.which;
            if (code == 13) {
                ASC.Controls.ConfirmMobileManager.sendAuthCode();
                return false;
            }
        });

        jq(".mobilephone-panel").on("click", "#getCodeAgainButton:not(.disable)", function () {
            ASC.Controls.ConfirmMobileManager.sendAuthCodeAgain();
            return false;
        });

        jq(".mobilephone-panel").on("click", "#sendCodeButton:not(.disable)", function () {
            ASC.Controls.ConfirmMobileManager.validateAuthCode();
            return false;
        });

        jq("#phoneAuthcode").keydown(function (event) {
            if (!e) {
                var e = event;
            }
            var code = e.keyCode || e.which;
            if (code == 13) {
                ASC.Controls.ConfirmMobileManager.validateAuthCode();
                return false;
            }
        });

        ASC.Controls.ConfirmMobileManager.init();
    });
})();