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

        AjaxPro.MobileActivationController.SaveMobilePhone(location.search.substring(1), mobilePhone, sendAuthCodeCallback);
    };

    var sendAuthCodeAgain = function () {
        jq("#errorMobileActivate").hide();
        jq("#getCodeAgainButton, #sendCodeButton").addClass("disable");

        AjaxPro.MobileActivationController.SendSmsCodeAgain(location.search.substring(1), sendAuthCodeCallback);
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

        AjaxPro.MobileActivationController.ValidateSmsCode(location.search.substring(1), code, function (result) {
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