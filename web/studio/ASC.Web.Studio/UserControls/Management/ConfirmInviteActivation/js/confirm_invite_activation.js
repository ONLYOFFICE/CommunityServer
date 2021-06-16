/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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

jq(function () {
    if (ASC.Resources.Master.Personal) {
        jq("body").addClass("body-personal-confirm");

        jq("#buttonConfirmInvite").on("click", function () {
            jq(".body-personal-confirm input[type='text'], .body-personal-confirm input[type='password']").each(function () {
                var currentInput = jq(this);
                var currentInputValue = currentInput[0].value;
                var currentInputWrapper = currentInput.parents(".property");

                if (currentInputValue.length == 0) {
                    currentInputWrapper.addClass("error");
                }
            });
        });

        jq(".body-personal-confirm input[type='text'], .body-personal-confirm input[type='password']").focus(inputFocus).focusout(inputFocusOut);

        jq(".property").each(function () {
            var $inpuntHint = jq(this).children(".name");
            jq(this).children(".value").append($inpuntHint);
        });

        var $formBlock = jq(".confirmBlock");
        AddPaddingWithoutScrollTo($formBlock, $formBlock);

        jq(window).on("resize", function () {
            AddPaddingWithoutScrollTo($formBlock, $formBlock);
        });

        function inputFocus() {
            var currentInput = jq(this);

            currentInput.parents(".property")
                .removeClass('error')
                .removeClass('valid')
                .addClass('focus')
                .addClass('input-hint-top');
        };

        function inputFocusOut() {
            var currentInput = jq(this);
            var currentInputValue = currentInput[0].value;

            var currentInputId = currentInput[0].id;
            var currentInputWrapper = currentInput.parents(".property");
            currentInputWrapper.removeClass('focus');

            if (currentInputValue.length == 0) {
                currentInputWrapper.removeClass('input-hint-top');
            } else {
                if (currentInputId == "studio_confirm_Email") {
                    if (!jq.isValidEmail(currentInputValue)) {
                        currentInputWrapper.addClass("error");
                    } else if (currentInputValue.length > 0) {
                        currentInputWrapper.addClass('valid');
                    }
                } else if (currentInputId.toLowerCase().indexOf("name") != -1) {
                    var nameRegexp = new XRegExp(ASC.Resources.Master.UserNameRegExpr.Pattern);
                    if (!nameRegexp.test(currentInputValue)) {
                        currentInputWrapper.addClass("error");
                    } else if (currentInputValue.length > 0) {
                        currentInputWrapper.addClass('valid');
                    }
                } else if (currentInputId == "studio_confirm_pwd") {
                    if (!(new XRegExp(jq(this).data("regex"), "ig")).test(currentInputValue)) {
                        currentInputWrapper.addClass("error");
                    } else if (currentInputValue.length > 0) {
                        currentInputWrapper.addClass('valid');
                    }
                }
            }
        };
    }
});