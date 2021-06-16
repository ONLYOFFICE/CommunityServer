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

jq(function () {
    if (jq(".confirm-block-page").hasClass("confirm-personal")) {
        jq("body").addClass("body-personal-confirm");
        var $inputPassword = jq(".body-personal-confirm input[type='password']")
        $inputPassword.focus(inputFocus).focusout(inputFocusOut);
        if ($inputPassword.attr("autofocus")) {
            $inputPassword.parents(".confirm-block-field").addClass('focus input-hint-top');
        }

        jq("input[type='submit']").on("click", function () {
            var $inputPwd = jq("#studio_confirm_pwd");
            if ($inputPwd.val().length == 0) {
                $inputPwd.parents(".confirm-block-field").addClass("error");
            }
        });

        jq(".confirm-block-field").each(function () {
            var $inpuntHint = jq(this).children(".default-personal-popup_label");
            jq(this).append($inpuntHint);
        });

        var $formBlock = jq(".confirm-block-cnt");
        AddPaddingWithoutScrollTo($formBlock, $formBlock);

        jq(window).on("resize", function () {
            AddPaddingWithoutScrollTo($formBlock, $formBlock);
        });

        function inputFocus() {
            var currentInput = jq(this);

            currentInput.parents(".confirm-block-field")
                .removeClass('error')
                .removeClass('valid')
                .addClass('focus')
                .addClass('input-hint-top');
        };

        function inputFocusOut() {
            var currentInput = jq(this);
            var currentInputValue = currentInput[0].value;

            var currentInputWrapper = currentInput.parents(".confirm-block-field");
            currentInputWrapper.removeClass('focus');

            if (currentInputValue.length == 0) {
                currentInputWrapper.removeClass('input-hint-top');
            } else {
                if (!(new XRegExp(jq(this).data("regex"), "ig")).test(currentInputValue)) {
                    currentInputWrapper.addClass("error");
                } else if (currentInputValue.length > 0) {
                    currentInputWrapper.addClass('valid');
                }
            };
        };
    };
});

var ConfirmActivacion = function () {
    var password = jq("#studio_confirm_pwd").val();
    if (!(new XRegExp(jq("#studio_confirm_pwd").data("regex"), "ig")).test(password)) {
        jq("#studio_confirm_pwd").css("border-color", "#DF1B1B");

        toastr.error(jq("#studio_confirm_pwd").data("help"));
        return;
    }

    window.hashPassword(password, function (passwordHash) {
        jq("#passwordHash").val(passwordHash);

        window.submitForm();
    });
};