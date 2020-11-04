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


jq(document).on("keyup",
    "#studio_confirm_Email," +
        "#studio_confirm_FirstName," +
        "#studio_confirm_LastName," +
        "#studio_confirm_pwd",
    function (event) {
        var code;
        if (!e) {
            var e = event;
        }
        if (e.keyCode) {
            code = e.keyCode;
        } else if (e.which) {
            code = e.which;
        }

        if (code != 13) {
            return;
        }

        if (jq(this).is("#studio_confirm_pwd")) {
            //do postback
            jq("#buttonConfirmInvite").click();
            return;
        }

        var input = jq(this).parents(".property").next().find(".value input");
        input.focus();
        //set focus to end of text
        var tmpStr = input.val();
        input.val("");
        input.val(tmpStr);
    });

jq(document).on("click", "#buttonConfirmInvite", function () {
    var requireFields = {
            email: jq("#studio_confirm_Email"),
            firstname: jq("#studio_confirm_FirstName"),
            lastname: jq("#studio_confirm_LastName"),
            psw: jq("#studio_confirm_pwd")
        },
        error = 0;

    jq("#registrationForm input").removeClass("with-error");

    for (var item in requireFields) {
        
        if (requireFields[item].is(":visible")) requireFields[item].val(requireFields[item].val().trim())

        if (requireFields[item].is(":visible") && !requireFields[item].val()) {
            requireFields[item].addClass("with-error");
            error++;
        }
    }

    if (requireFields.email.is(":visible") && !jq.isValidEmail(requireFields.email.val())) {
        requireFields.email.addClass("with-error");
        error++;
    }

    var regexp = new XRegExp(ASC.Resources.Master.UserNameRegExpr.Pattern);

    if (requireFields.firstname.is(":visible") && !regexp.test(requireFields.firstname.val())) {
        requireFields.firstname.addClass("with-error");
        error++;
    }

    if (requireFields.lastname.is(":visible") && !regexp.test(requireFields.lastname.val())) {
        requireFields.lastname.addClass("with-error");
        error++;
    }

    var password = requireFields.psw.val();
    if (!(new XRegExp(requireFields.psw.data("regex"), "ig")).test(password)) {
        requireFields.psw.addClass("with-error");

        toastr.error(requireFields.psw.data("help"));
        error++;
    }

    if (error == 0) {
        var password = jq("#studio_confirm_pwd").val();
        window.hashPassword(password, function (passwordHash) {
            jq("#passwordHash").val(passwordHash);

            window.submitForm("confirmInvite", "");
        });
    }
});