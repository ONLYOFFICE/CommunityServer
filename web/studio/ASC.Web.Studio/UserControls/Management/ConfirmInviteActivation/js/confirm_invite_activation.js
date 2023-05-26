/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


window.ConfirmInviteManager = new function () {
    var $registrationForm = jq("#registrationForm");
    var $emailInput = jq("#studio_confirm_Email");
    var $firstNameInput = jq("#studio_confirm_FirstName");
    var $lastNameInput = jq("#studio_confirm_LastName");
    var $passwordInput = jq("#studio_confirm_pwd");
    var $passwordMatchInput = jq("#studio_confirm_pwd_match");
    var $passwordHashInput = jq("#passwordHash");
    var $labels = jq("#passwordShowLabel, #passwordShowLabelMatch");
    var $passwordNotMatchText = jq("#password-do-not-match-text");
    var $buttonConfirmInvite = jq("#buttonConfirmInvite");

    var init = function () {
        var $inputs = $registrationForm.find("input:visible");

        //set default value
        $inputs.each(function () {
            var $input = jq(this);
            $input.val($input.attr("value"));
        });

        $labels.on("click", togglePasswordType);
        $buttonConfirmInvite.on("click", confirmInvite);

        if (ASC.Resources.Master.Personal) {
            jq("body").addClass("body-personal-confirm");

            $inputs.on("focus", inputFocus).on("focusout", inputFocusOut).on("keyup", inputKeyUp);

            $emailInput.trigger("focus").parents(".property").addClass("focus input-hint-top");

            jq(".property").each(function () {
                var $inpuntHint = jq(this).children(".name");
                jq(this).children(".value").append($inpuntHint);
            });

            var $formBlock = jq(".confirmBlock");
            AddPaddingWithoutScrollTo($formBlock, $formBlock);

            jq(window).on("resize", function () {
                AddPaddingWithoutScrollTo($formBlock, $formBlock);
            });
        } else {
            $inputs.on("keyup", inputKeyUp);
        }
    };

    var togglePasswordType = function () {
        var label = jq(this);
        var input = label.parent().find(".textEdit");

        if (input.attr("type") == "password") {
            input.attr("type", "text");
            label.removeClass("hide-label").addClass("show-label");
        } else {
            input.attr("type", "password");
            label.removeClass("show-label").addClass("hide-label");
        }
    };

    var inputFocus = function () {
        jq(this).parents(".property").addClass("focus").addClass("input-hint-top");
    };

    var inputFocusOut = function () {
        var $input = jq(this);
        var $parent = $input.parents(".property").removeClass("focus");

        if ($input.val().length == 0) {
            $parent.removeClass("input-hint-top");
        }
    }

    var inputKeyUp = function (event) {
        checkEmailValid(false);
        checkNameValid($firstNameInput, false);
        checkNameValid($lastNameInput, false);
        checkPasswordValid(false, false);
        checkPasswordMatchValid(false);

        var password = $passwordInput.val();
        var passwordMatch = $passwordMatchInput.val();

        $passwordNotMatchText.toggle(passwordMatch != "" && password != passwordMatch);

        var code = event.keyCode || event.which;
        if (code != 13) {
            return;
        }

        var $input = jq(this);
        if ($input.is($passwordMatchInput)) {
            confirmInvite();
            return;
        }

        var $nextInput = $input.parents(".property").next().find(".value input");
        $nextInput.trigger("focus");

        //set focus to end of text
        var tmpStr = $nextInput.val();
        $nextInput.val("");
        $nextInput.val(tmpStr);
    };

    var clearInputError = function ($input) {
        if (ASC.Resources.Master.Personal) {
            $input.parents(".property").removeClass("valid").removeClass("error");
        } else {
            $input.removeClass("with-error");
        }
    };

    var toggleInputError = function ($input, show) {
        if (show) {
            if (ASC.Resources.Master.Personal) {
                $input.parents(".property").removeClass("valid").addClass("error");
            } else {
                $input.addClass("with-error");
            }
        } else {
            if (ASC.Resources.Master.Personal) {
                $input.parents(".property").removeClass("error").addClass("valid");
            } else {
                $input.removeClass("with-error");
            }
        }
    };

    var checkEmailValid = function (emptyIsError) {
        if (!$emailInput.length) {
            return true;
        }

        var email = $emailInput.val().trim();

        clearInputError($emailInput);

        if (email.length == 0 && !emptyIsError) {
            return true;
        }

        if (!jq.isValidEmail(email)) {
            toggleInputError($emailInput, true);
            return false;
        } else {
            toggleInputError($emailInput, false);
            return true;
        }
    }

    var checkNameValid = function ($input, emptyIsError) {
        var name = $input.val().trim();
        var regexp = new XRegExp(ASC.Resources.Master.UserNameRegExpr.Pattern);

        clearInputError($input);

        if (name.length == 0 && !emptyIsError) {
            return true;
        }

        if (!regexp.test(name)) {
            toggleInputError($input, true);
            return false;
        } else {
            toggleInputError($input, false);
            return true;
        }
    }

    var checkPasswordValid = function (emptyIsError, showToastr) {
        var password = $passwordInput.val();
        var regex = new RegExp($passwordInput.data("regex"), "g");
        var help = $passwordInput.data("help");

        clearInputError($passwordInput);

        if (password.length == 0 && !emptyIsError) {
            return true;
        }

        if (!regex.test(password)) {
            toggleInputError($passwordInput, true);
            if (showToastr) {
                toastr.error(help);
            }
            return false;
        } else {
            toggleInputError($passwordInput, false);
            return true;
        }
    }

    var checkPasswordMatchValid = function (emptyIsError) {
        var password = $passwordInput.val();
        var passwordMatch = $passwordMatchInput.val();

        clearInputError($passwordMatchInput);

        if (passwordMatch.length == 0 && !emptyIsError) {
            return true;
        }

        if (password != passwordMatch) {
            toggleInputError($passwordMatchInput, true);
            return false;
        } else {
            toggleInputError($passwordMatchInput, false);
            return true;
        }
    }

    var confirmInvite = function () {
        var emailValid = checkEmailValid(true);
        var firstNameValid = checkNameValid($firstNameInput, true);
        var lastNameInput = checkNameValid($lastNameInput, true);
        var passwordValid = checkPasswordValid(true, true);
        var passwordMatchValid = checkPasswordMatchValid(true);

        if (!(emailValid && firstNameValid && lastNameInput && passwordValid && passwordMatchValid)) {
            return;
        }

        window.hashPassword($passwordInput.val(), function (passwordHash) {
            $passwordHashInput.val(passwordHash);
            window.submitForm("confirmInvite", "");
        });
    };

    return {
        init: init
    };
};

jq(function () {
    window.ConfirmInviteManager.init();
});