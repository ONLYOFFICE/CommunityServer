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


jq(function () {
    var recaptchaEmail = null;
    var recaptchaLogin = null;
    var createAccountShown = false;
    var loginPopupShown = false;
    var passwordRecoveryShown = false;
    var successRegisteringShown = false;

    var $loginEmail = jq("#login");
    $loginEmail.trigger("blur");
    var loginEmailVal = $loginEmail.val();
    if (loginEmailVal != "") {
        $loginEmail.parent().addClass("focus");
    }
    
    if (jq.cookies.get('onluoffice_personal_cookie') == null || jq.cookies.get('onluoffice_personal_cookie') == false) {
        jq('.cookieMess').show();
    }

    if (jq("#agree_to_terms").prop("checked")) {
        bindConfirmEmailBtm();
        jq('.auth-form-with_btns_social .account-links a')
                    .removeClass('disabled')
                    .off('click');
    } else {
        jq('.auth-form-with_btns_social .account-links a')
            .addClass('disabled');
    }

    jq('.auth-form-with_form_btn')
        .on("click", function () {
            return false;
        });

    function bindConfirmEmailBtm() {

        jq('.auth-form-with_btns_social .account-links a').removeClass('disabled');

        if (jq("#agree_to_terms").prop("checked")) {
            jq("#confirmEmailBtn").removeClass('disabled');
        }

        jq("#confirmEmailBtn").on("click", function () {
                var $email = jq("#confirmEmail"),
                    email = $email.val().trim(),
                    spam = jq("#spam").prop("checked"),
                    $error = jq("#confirmEmailError"),
                    errorText = "",
                    isError = false;

                $email.removeClass("error");

                if (!email) {
                    errorText = ASC.Resources.Master.ResourceJS.ErrorEmptyEmail;
                    isError = true;
                } else if (!jq.isValidEmail(email)) {
                    errorText = ASC.Resources.Master.ResourceJS.ErrorNotCorrectEmail;
                    isError = true;
                }

                if (isError) {
                    $error.html(errorText).show();
                    $email.addClass("error").parent().removeClass("valid").addClass("error");
                    return;
                }

                var data = {
                    "email": email,
                    "lang": jq(".personal-languages_select").attr("data-lang"),
                    "campaign": jq("#confirmEmailBtn").attr("data-campaign") ? !!(jq("#confirmEmailBtn").attr("data-campaign").length) : false,
                    "spam": spam,
                    "recaptchaResponse": recaptchaEmail != null ? window.grecaptcha.getResponse(recaptchaEmail) : ""
                };

                var onError = function (error) {
                    $error.html(error).show();
                    $email.addClass("error").parent().removeClass("valid").addClass("error");

                    if (recaptchaEmail != null) {
                        window.grecaptcha.reset(recaptchaEmail);
                    }
                };

                Teamlab.registerUserOnPersonal(data, {
                    success: function (arg, res) {
                        if (res && res.length) {
                            onError(res);
                            return;
                        }
                        $error.empty();
                        jq("#activationEmail").html(email);
                        var $authFormWithForm = jq(".auth-form-with_form");
                        if ($authFormWithForm.parents(".first-screen-content").length) {
                            jq(".auth-form-container").hide();
                        } else {
                            $authFormWithForm.hide();
                        }
                        jq("body").addClass("auth-maincontent-hidden");
                        jq("#sendEmailSuccessPopup").show();
                        successRegisteringShown = true;
                        AddPaddingWithoutScrollTo(jq("#sendEmailSuccessPopup"), jq("#loginPopup"));
                    },
                    error: function (params, errors) {
                        onError(errors[0]);
                    }
            });
        });
    }
    
    function bindEvents () {
        jq(".back-to-login").on("click", function () {
            jq("#passwordRecovery").hide();
            jq("#loginPopup").show();
            jq("#personalCreateNow").addClass("web-shown");
            positionPersonalCreateNowForLogin();
            createAccountShown = false;
            loginPopupShown = true;
            passwordRecoveryShown = false;
        });

        // close cookie mess
        jq("#personalcookie").on("click", function () {
            jq.cookies.set('onluoffice_personal_cookie', true);
            closeCookieMess();
        });
        function closeCookieMess() {
            jq('.cookieMess').hide();
        }
        // confirm the email
        jq(document).on("keypress", "#confirmEmail", function (evt) {
            jq(this).removeClass("error");
            if (evt.keyCode == 13) {
                evt.preventDefault();
                jq("#confirmEmailBtn").trigger("click");
            }
        });
        jq('.account-links a').on("click", function () {
            return false;
        });
        // change in consent to terms

        function showAccountLinks() {
            jq('.account-links a')
                   .removeClass('disabled')
                   .off('click');
            bindConfirmEmailBtm();
        }
        function hideAccountLinks() {
            if (!jq("#agree_to_terms")[0].checked) {
                jq('.auth-form-with_btns_social .account-links a')
                    .on("click", function () { return false; })
                    .addClass('disabled');
                jq("#confirmEmailBtn")
                    .addClass('disabled')
                    .off('click');
            }
        }
        jq("#agree_to_terms").on("change", function () {
            if (this.checked) {
                showAccountLinks();
            } else {
                hideAccountLinks();
            }
        });
        jq("#desktop_agree_to_terms").on("change", function () {
            var btn = jq("#loginBtn");
            if (this.checked) {
                btn.removeClass('disabled').off('click');
            } else {
                btn.addClass('disabled').on("click", function() {
                    return false;
                });
            }
        });

        jq(document).on("click", "#loginBtn:not(.disabled)", function () {
            Authorize.SubmitDocs();
            return false;
        });
        
        jq(document).on("keyup", function (event) {
            var code;
            if (!e) {
                var e = event;
            }
            if (e.keyCode) {
                code = e.keyCode;
            } else if (e.which) {
                code = e.which;
            }


            if (code == 27 && !jq('body').hasClass('desktop')) {
                jq(".default-personal-popup").fadeOut(200, function () {
                    enableScroll();
                    hideAccountLinks();
                });
            }
        });

        
        var $body = jq(window.document.body);
        var marginRight;
        function disableScroll() {
            var bodyWidth = $body.innerWidth();
            $body.css('overflow-y', 'hidden');
            marginRight = $body.innerWidth() - bodyWidth;
            $body.css('marginRight', ($body.css('marginRight') ? '+=' : '') + marginRight);
        }

        function enableScroll() {
            if (parseInt($body.css('marginRight')) >= marginRight) {
                $body.css('marginRight', '-=' + marginRight);
            }
            $body.css('overflow-y', 'auto');
        }

        // Login
        jq("#personalLogin a").on("click", function () {
            jq(".auth-form-container").hide();
            jq("#passwordRecovery").hide();
            showAccountLinks();
            jq("#loginPopup").show();
            jq("#personalCreateNow").addClass("web-shown");
            createAccountShown = false;
            loginPopupShown = true;
            passwordRecoveryShown = false;

            jq('#login').trigger("focus");
            jq('#pwd').trigger("blur");
            jq("body").addClass("auth-maincontent-hidden");
            positionPersonalCreateNowForLogin();

            if (jq("#recaptchaLogin").length) {
                if (recaptchaLogin != null) {
                    window.grecaptcha.reset(recaptchaLogin);
                } else {
                    var recaptchaLoginRender = function () {
                        recaptchaLogin = window.grecaptcha.render("recaptchaLogin", {"sitekey": jq("#recaptchaData").val()});
                    };
                    
                    if (window.grecaptcha && window.grecaptcha.render) {
                        recaptchaLoginRender();
                    } else {
                        jq(document).ready(recaptchaLoginRender);
                    }
                }
            }
        });

        jq("#personalCreateNow a").on("click", function () {
            jq(".auth-form-container").after(jq(".auth-form-with_form_w").addClass("separate-window").show());
            jq("div:not(.first-screen-content) .auth-form-with_form").show();
            jq(".auth-form-with_form > .auth-form-settings").append(jq("#confirmEmailBtn"));
            positionPersonalAccountLogin();
            jq("#loginPopup").hide().prepend(jq("#personalCreateNow"));
            jq("#personalCreateNow").removeClass("web-shown");
            jq("#passwordRecovery").hide();
            jq("#personalAccountLogin").addClass("web-shown");
            createAccountShown = true;
            loginPopupShown = false;
            passwordRecoveryShown = false;
        });

        jq("#personalAccountLogin a").on("click", function () {
            jq("#loginPopup").show();
            jq("#personalCreateNow").addClass("web-shown");
            jq(".auth-form-with_form_w").hide();
            jq("#personalAccountLogin").removeClass("web-shown");
            positionPersonalCreateNowForLogin();
            createAccountShown = false;
            loginPopupShown = true;
            passwordRecoveryShown = false;
        });

        jq(".login_forget-psw").on("click", function () {
            jq("#pswdRecoveryDialogPopupHeader").removeClass("display-none");
            jq("#pswdRecoveryDialogText").show();
            jq("#pswdChangeDialogPopupHeader").hide();
            jq("#pswdChangeDialogText").hide();

            jq("#" + jq("#studio_pwdReminderInfoID").val()).html("<div></div>");
            jq("#" + jq("#studio_pwdReminderInfoID").val()).hide();

            jq("#passwordRecovery").append(jq("#studio_pwdReminderDialog")).show();
            jq("#personalCreateNow").addClass("web-shown");
            jq(".popupContainerClass").append(jq("#passwordRecovery .link-as-btn"));
            positionPersonalCreateNowForPasswordRecovery();

            PopupKeyUpActionProvider.EnterAction = "PasswordTool.RemindPwd();";

            var loginEmail = jq("#login").val();
            if (loginEmail != null && loginEmail != undefined && jq.isValidEmail(loginEmail)) {
                jq("#studio_emailPwdReminder").val(loginEmail).parent().addClass("focus");
            }

            jq(".auth-form-container").hide();
            jq("#loginPopup").hide();
            createAccountShown = false;
            loginPopupShown = false;
            passwordRecoveryShown = true;
        });

        jq("#goToMainPage").on("click", function () {
            jq("#sendEmailSuccessPopup").hide();
            jq(".first-screen-content").append(jq(".auth-form-with_form_w").removeClass("separate-window"));
            jq(".auth-form-with_form").css({ "paddingTop": "", "paddingBottom": "" });
            jq(".auth-form-with_form").show();
            positionConfirmEmailBtn();
            jq(".auth-form-container").fadeIn(1000);
            jq("body").removeClass("auth-maincontent-hidden");
            jq("#personalCreateNow").removeClass("web-shown");
            jq("#personalAccountLogin").removeClass("web-shown");
            jq("#confirmEmail").val("").parent().removeClass("valid");
            successRegisteringShown = false;
        });

        jq("#studio_emailPwdReminder").wrap('<div class="auth-input-wrapper"></div>');
        jq("#pswdRecoveryDialogText .auth-input-wrapper").append(jq("label[for='studio_emailPwdReminder']"));

        var loginMessage = jq(".login-message[value!='']").val();
        if (loginMessage && loginMessage.length) {
            jq("#personalLogin a").trigger("click");

            var type = jq(".login-message[value!='']").attr("data-type");
            if (type | 0) {
                toastr.success(loginMessage);
            } else {
                toastr.error(loginMessage);
            }
        }

        try {
            var anch = ASC.Controls.AnchorController.getAnchor();
            if (anch.trim() == "passrecovery") {
                PasswordTool.ShowPwdReminderDialog();
            }
        } catch (e) {
        }
        if (jq('body').hasClass('desktop')) {
            showAccountLinks();
        }
    }

    jq("input[type='email'], input[type='password']").on("focus", inputFocus).on("focusout", inputFocusOut);

    function inputFocus() {
        var currentInput = jq(this);

        currentInput.parent()
            .removeClass('error')
            .removeClass('valid')
            .addClass('focus');
    };

    function inputFocusOut() {
        var currentInput = jq(this);
        var currentInputValue = currentInput[0].value;

        var currentInputId = currentInput[0].id;
        var currentInputWrapper = currentInput.parent();
        currentInputWrapper.removeClass('focus');

        if (currentInput.attr("type") == "email") {
            if (jq.isValidEmail(currentInputValue)) {
                currentInput.removeClass("error");
                currentInputWrapper.addClass('valid').next("[class*='error']").hide();
            } else {
                if (currentInputValue.length > 0) {
                    currentInput.parent().addClass("error");
                } else {
                    currentInputWrapper.next("[class*='error']").hide();
                }
            }
        } else if (currentInputValue.length > 0) {
            currentInput.parent().addClass('valid');
        }
    };

    jq("span[data-span-for]").on("click", function () {
        jq("input[id=" + jq(this).attr("data-span-for") + "]").trigger('click');
    });

    jq.fn.duplicate = function (count, cloneEvents) {
        var tmp = [];
        for (var i = 0; i < count; i++) {
            jq.merge(tmp, this.clone(cloneEvents).get());
        }
        return this.pushStack(tmp);
    };
    
    jq('.create-carousel').slick({
        slidesToShow: 1,
        slidesToScroll: 1,
        arrows: false,
        fade: true,
        asNavFor: '.slick-carousel'
    });
    jq('.slick-carousel').slick({
        slidesToShow: 1,
        arrows: true,
        centerMode: true,
        asNavFor: '.create-carousel'
    });

    bindEvents();
    
    reviewBuilder(reviewsDataObject, reviewsDataObjectLocale, "#heartheweb-reviews-template-list-item");
    maxCreateFormWidth();
    positionConfirmEmailBtn();
    AddPaddingWithoutScrollTo(jq("#loginPopup"), jq("#loginPopup"));

    jq(window).on("resize", function () {
        maxCreateFormWidth();
        if (loginPopupShown) {
            positionPersonalCreateNowForLogin();
            AddPaddingWithoutScrollTo(jq("#loginPopup"), jq("#loginPopup"));
        }
        if (passwordRecoveryShown) {
            positionPersonalCreateNowForPasswordRecovery();
            AddPaddingWithoutScrollTo(jq("#passwordRecovery"), jq("#loginPopup"));
        }
        if (createAccountShown) {
            positionConfirmEmailBtn();
            positionPersonalAccountLogin();
            AddPaddingWithoutScrollTo(jq(".auth-form-with_form_w.separate-window .auth-form-with_form"), jq("#loginPopup"));
        }
        if (!createAccountShown && !loginPopupShown && !passwordRecoveryShown) {
            positionConfirmEmailBtn();
        };
    });

    function maxCreateFormWidth() {
        var wInnerWidth = jq(window).innerWidth();
        jq(".auth-form-with_form_w").css("maxWidth", wInnerWidth);
    };

    function positionConfirmEmailBtn() {
        if (jq(window).outerWidth() <= 592) {
            jq(".first-screen-content .auth-form-with_form > .auth-form-settings").append(jq(".first-screen-content #confirmEmailBtn"));
        } else {
            jq(".first-screen-content .auth-input-row").append(jq(".first-screen-content #confirmEmailBtn"));
        }
    };

    function positionPersonalAccountLogin() {
        if (jq(window).width() < 768) {
            jq(".auth-form-with_form").append(jq("#personalAccountLogin"));
        } else {
            jq("#personalLogin").before(jq("#personalAccountLogin"));
        }
    };

    function positionPersonalCreateNowForLogin() {
        if (jq(window).width() < 768) {
            jq("#loginPopup").append(jq("#personalCreateNow"));
        } else {
            jq("#personalLogin").before(jq("#personalCreateNow"));
        }
    };

    function positionPersonalCreateNowForPasswordRecovery() {
        if (jq(window).width() < 768) {
            jq(".popupContainerClass").append(jq("#personalCreateNow"));
        } else {
            jq("#personalLogin").before(jq("#personalCreateNow"));
        }
    };
});

/*Call from another .js file.*/
function onSuccessRemindPwd() {
    jq("#passwordRecovery").hide();
    jq("#personalCreateNow").removeClass("web-shown");
    if (jq('body').hasClass('desktop')) {
        jq("#loginPopup").fadeIn(1000);
    } else {
        jq("#personalLogin a").trigger("click");
    }
};
/*---------------------------*/