/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

;
ASC.Controls.ConfirmMobileManager = function () {
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
        jq("#sendPhoneButton, #getCodeAgainButton, #sendCodeButton").removeClass("disable");

        var res = result.value || result.error;
        if (res.phoneNoise) {
            if (res.confirm) {
                jq("#mobileNumberPanel").hide();
                jq("#phoneNoise").html(res.phoneNoise);
                jq("#mobileCodePanel").show();
                jq("#phoneAuthcode").val("").focus();
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

        jq("#getCodeAgainButton, #sendCodeButton").addClass("disable");

        AjaxPro.MobileActivationController.ValidateSmsCode(code, function (result) {
            var res = result.value || result.error;
            if (typeof res.RefererURL != "undefined") {
                location.href = res.RefererURL || "/";
            } else {
                jq("#getCodeAgainButton, #sendCodeButton").removeClass("disable");
                jq("#errorMobileActivate").html(res.Message || "Error").show();
            }
        });
    };

    return {
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

        jq("#phoneAuthcode").keyup(function (event) {
            if (!e) {
                var e = event;
            }
            var code = e.keyCode || e.which;
            if (code == 13) {
                ASC.Controls.ConfirmMobileManager.validateAuthCode();
                return false;
            }
        });
    });
})();