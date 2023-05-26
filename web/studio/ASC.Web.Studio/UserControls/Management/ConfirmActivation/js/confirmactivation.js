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


window.ConfirmManager = new function () {
    var $passwordInput = jq("#studio_confirm_pwd");
    var $passwordMatchInput = jq("#studio_confirm_pwd_match");
    var $passwordHashInput = jq("#passwordHash");
    var $labels = jq("#passwordShowLabel, #passwordShowLabelMatch");
    var $passwordMatchText = jq("#password-match-text");
    var $passwordNotMatchText = jq("#password-do-not-match-text");

    var init = function () {
        $labels.on("click", togglePasswordType);

        if (ASC.Resources.Master.Personal) {
            jq("body").addClass("body-personal-confirm");

            $passwordInput.on("focus", inputFocus).on("focusout", inputFocusOut).on("keyup", inputKeyUp);
            $passwordMatchInput.on("focus", inputFocus).on("focusout", inputFocusOut).on("keyup", inputKeyUp);

            $passwordInput.parents(".confirm-block-field").addClass("focus input-hint-top");

            var $formBlock = jq(".confirm-block-cnt");
            AddPaddingWithoutScrollTo($formBlock, $formBlock);

            jq(window).on("resize", function () {
                AddPaddingWithoutScrollTo($formBlock, $formBlock);
            });
        } else {
            $passwordInput.on("keyup", inputKeyUp);
            $passwordMatchInput.on("keyup", inputKeyUp);
        }
    };

    var togglePasswordType = function () {
        var label = jq(this);
        var input = label.parent().find(".pwdLoginTextbox");

        if (input.attr("type") == "password") {
            input.attr("type", "text");
            label.removeClass("hide-label").addClass("show-label");
        } else {
            input.attr("type", "password");
            label.removeClass("show-label").addClass("hide-label");
        }
    };

    var inputFocus = function () {
        jq(this).parents(".confirm-block-field").addClass("focus").addClass("input-hint-top");
    };

    var inputFocusOut = function () {
        var $input = jq(this);
        var $parent = $input.parents(".confirm-block-field").removeClass("focus");

        if ($input.val().length == 0) {
            $parent.removeClass("input-hint-top");
        }
    }

    var inputKeyUp = function () {
        checkPasswordValid(false, false);
        checkPasswordMatchValid(false);

        var password = $passwordInput.val();
        var passwordMatch = $passwordMatchInput.val();

        if (passwordMatch == "") {
            $passwordMatchText.hide();
            $passwordNotMatchText.hide();
        } else {
            if (password == passwordMatch) {
                $passwordMatchText.show();
                $passwordNotMatchText.hide();
            } else {
                $passwordMatchText.hide();
                $passwordNotMatchText.show();
            }
        }
    };

    var clearInputError = function ($input) {
        if (ASC.Resources.Master.Personal) {
            $input.parents(".confirm-block-field").removeClass("valid").removeClass("error");
        } else {
            $input.removeClass("red-border");
        }
    };

    var toggleInputError = function ($input, show) {
        if (show) {
            if (ASC.Resources.Master.Personal) {
                $input.parents(".confirm-block-field").removeClass("valid").addClass("error");
            } else {
                $input.addClass("red-border");
            }
        } else {
            if (ASC.Resources.Master.Personal) {
                $input.parents(".confirm-block-field").removeClass("error").addClass("valid");
            } else {
                $input.removeClass("red-border");
            }
        }
    };

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

    var confirmActivacion = function () {
        var passwordValid = checkPasswordValid(true, true);
        var passwordMatchValid = checkPasswordMatchValid(true);

        if (!(passwordValid && passwordMatchValid)) {
            return;
        }

        window.hashPassword($passwordInput.val(), function (passwordHash) {
            $passwordHashInput.val(passwordHash);
            window.submitForm();
        });
    };

    return {
        init: init,
        confirmActivacion: confirmActivacion
    };
};

jq(function () {
    window.ConfirmManager.init();
});