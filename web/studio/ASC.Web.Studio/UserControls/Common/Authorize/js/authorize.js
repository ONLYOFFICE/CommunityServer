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


var Authorize = new function () {

    jq(document).ready(function () {

        if (jq("#recaptchaHiddenContainer").is(":visible")) {
            RecaptchaController.InitRecaptcha(jq("#recaptchaHiddenContainer").attr("data-hl"));
        }

        jq(jq("#login").val().length ? "#pwd" : "#login").focus();

        jq("#login,#pwd").keyup(function (event) {
            var code;
            if (!e) {
                var e = event;
            }
            if (e.keyCode) {
                code = e.keyCode;
            } else if (e.which) {
                code = e.which;
            }

            if (code == 13) {
                if (jq(this).is("#login") && !jq("#pwd").val().length) {
                    jq("#pwd").focus();
                    return true;
                }
                if (jq('body').hasClass('desktop') && !!jq("#desktop_agree_to_terms").length && !(jq("#desktop_agree_to_terms").is(':checked'))) {
                    return true;
                }
                Authorize.Submit();
            } else if (code == 27) {
                jq(this).val("");
            }
            return true;
        });

        try {
            var anch = ASC.Controls.AnchorController.getAnchor();
            if (jq.trim(anch) == "passrecovery") {
                PasswordTool.ShowPwdReminderDialog();
            }
        } catch (e) { }

    });

    this.Submit = function () {
        jq("#authMessage").hide();
        jq(".pwdLoginTextbox").removeClass("error");

        var ldapPassword = jq("#ldapPassword");
        if (ldapPassword.length && ldapPassword.prop("checked") == true) {
            var password = jq("#pwd").val();
            jq("#passwordHash").val(password);

            window.submitForm();
        } else {
            login();
        }
    };

    this.SubmitDocs = function () {
        login();
    };

    var login = function () {
        var password = jq("#pwd").val();
        window.hashPassword(password, function (passwordHash) {
            jq("#passwordHash").val(passwordHash);

            window.submitForm();
        });
    };
};