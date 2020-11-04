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


jq(function () {
    var recaptchaEmail = null;
    var recaptchaLogin = null;

    jq('#login').blur();
    
    if (jq.cookies.get('onluoffice_personal_cookie') == null || jq.cookies.get('onluoffice_personal_cookie') == false) {
        jq('.cookieMess').css('display', 'table');
    }

    if (jq("#agree_to_terms").prop("checked")) {
        bindConfirmEmailBtm();
        jq('.auth-form-with_btns_social .account-links a')
                    .removeClass('disabled')
                    .unbind('click');
    } else {
        jq('.auth-form-with_btns_social .account-links a')
            .addClass('disabled');
    }

    jq('.auth-form-with_form_btn')
        .click(function () {
            return false;
        });

    function bindConfirmEmailBtm() {

        jq('.auth-form-with_btns_social .account-links a').removeClass('disabled');

        jq("#confirmEmailBtn")
            .removeClass('disabled')
            .on("click", function () {
                var $email = jq("#confirmEmail"),
                    email = $email.val().trim(),
                    spam = jq("#spam").prop("checked"),
                    analytics = jq("#analytics").prop("checked"),
                    $error = jq("#confirmEmailError"),
                    errorText = "",
                    isError = false;

                $email.removeClass("error");

                if (!email) {
                    errorText = ASC.Resources.Master.Resource.ErrorEmptyEmail;
                    isError = true;
                } else if (!jq.isValidEmail(email)) {
                    errorText = ASC.Resources.Master.Resource.ErrorNotCorrectEmail;
                    isError = true;
                }

                if (isError) {
                    $error.html(errorText);
                    $email.addClass("error");
                    return;
                }

                var data = {
                    "email": email,
                    "lang": jq(".personal-languages_select").attr("data-lang"),
                    "campaign": jq("#confirmEmailBtn").attr("data-campaign") ? !!(jq("#confirmEmailBtn").attr("data-campaign").length) : false,
                    "spam": spam,
                    "analytics": analytics,
                    "recaptchaResponse": recaptchaEmail != null ? window.grecaptcha.getResponse(recaptchaEmail) : ""
                };

                var onError = function (error) {
                    $error.html(error);
                    $email.addClass("error");

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
                        jq(".auth-form-with_form_w").hide();
                        jq("#sendEmailSuccessPopup").show();
                    },
                    error: function (params, errors) {
                        onError(errors[0]);
                    }
            });
        });
    }
    
    function bindEvents () {
        jq(function () {
            jq("#loginSignUp").on("click", function () {
                enableScroll();
                jq('#login').blur();
                jq("#loginPopup").hide();
                hideAccountLinks();
                jq(".auth-form-with_form_w").show();
                jq("#confirmEmail").focus();
            });
        });
        
        jq('.create-link').on("click", function () {
            jq('html, body').animate({ scrollTop: 0 }, 300);
            jq(".auth-form-with_form_w").show();
            jq("#confirmEmail").focus();
        });
        // close popup window
        jq(".default-personal-popup_closer").on("click", function () {
            hideAccountLinks();
            jq(this).parents(".default-personal-popup").fadeOut(200, function() {
                enableScroll();
            });
            if ((jq('body').hasClass('desktop'))) {
                jq("#personalLogin a").click();
            }
            jq('.auth-form-with_form_btn')
                .click(function () {
                    return false;
                });
        });
        // close register form
        jq(".register_form_closer").on("click", function () {
            jq(this).parents(".auth-form-with_form_w").fadeOut(200, function () {});
        });
        // close cookie mess
        jq(".cookieMess_closer").on("click", function () {
            jq.cookies.set('onluoffice_personal_cookie', true);
            closeCookieMess();
        });
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
        jq('.account-links a').click(function () {
            return false;
        });
        // change in consent to terms

        function showAccountLinks() {
            jq('.account-links a')
                   .removeClass('disabled')
                   .unbind('click');
            bindConfirmEmailBtm();
        }
        function hideAccountLinks() {
            if (!jq("#agree_to_terms")[0].checked) {
                jq('.auth-form-with_btns_social .account-links a')
                    .click(function () { return false; })
                    .addClass('disabled');
                jq("#confirmEmailBtn")
                    .addClass('disabled')
                    .unbind('click');
            }
        }
        jq("#agree_to_terms").change(function () {
            if (this.checked) {
                showAccountLinks();
            } else {
                hideAccountLinks();
            }
        });
        jq("#desktop_agree_to_terms").change(function () {
            var btn = jq("#loginBtn");
            if (this.checked) {
                btn.removeClass('disabled').unbind('click');
            } else {
                btn.addClass('disabled').click(function() {
                    return false;
                });
            }
        });

        jq(document).on("click", "#loginBtn:not(.disabled)", function () {
            Authorize.SubmitDocs();
            return false;
        });
        
        jq(document).keyup(function (event) {
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
            jq(".auth-form-with_form_w").fadeOut(200, function () { });
            showAccountLinks();
            jq('.auth-form-with_form_btn').removeClass('disabled').unbind('click');
            jq("#loginPopup").show();
            jq('#login').focus();
            disableScroll();

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

        // open form
        jq(".open-form").on("click", function () {
            jq(".auth-form-with_form_w").show();
            jq("#confirmEmail").focus();
            //disableScroll();

            if (jq("#recaptchaEmail").length) {
                if (recaptchaEmail != null) {
                    window.grecaptcha.reset(recaptchaEmail);
                } else {
                    recaptchaEmail = window.grecaptcha.render("recaptchaEmail", {"sitekey": jq("#recaptchaData").val()});
                }
            }
        });

        var loginMessage = jq(".login-message[value!='']").val();
        if (loginMessage && loginMessage.length) {
            jq("#personalLogin a").click();

            var type = jq(".login-message[value!='']").attr("data-type");
            if (type | 0) {
                toastr.success(loginMessage);
            } else {
                toastr.error(loginMessage);
            }
        }

        try {
            var anch = ASC.Controls.AnchorController.getAnchor();
            if (jq.trim(anch) == "passrecovery") {
                PasswordTool.ShowPwdReminderDialog();
            }
        } catch (e) {
        }
        if (jq('body').hasClass('desktop')) {
            showAccountLinks();
        }
    }

    function getReviewList () {
        var lng = jq("#reviewsContainer").attr("data-lng");
        lng = lng ? lng.toLowerCase() : "en";

        jq.getJSON("/UserControls/Common/AuthorizeDocs/js/reviews.json", function (data) {
            var reviews = data.en.reviews;

            jq.each(data, function (key, val) {
                if (key == lng) {
                    reviews = val.reviews;
                }
            });

            //shuffle(reviews);
            reviews.forEach(function (review) {
                review.stars = new Array(parseInt(review.rating));
                if (review.photo) {
                    review.photoUrl = "/UserControls/Common/AuthorizeDocs/css/images/foto_commets/" + review.photo;
                }
                jq("#personalReviewTmpl").tmpl(review).appendTo("#reviewsContainer");
            });
            
            carouselAuto(reviews.length);
        });
    }

    function shuffle (array) {
        var counter = array.length, temp, index;

        // While there are elements in the array
        while (counter > 0) {
            // Pick a random index
            index = Math.floor(Math.random() * counter);

            // Decrease counter by 1
            counter--;

            // And swap the last element with it
            temp = array[counter];
            array[counter] = array[index];
            array[index] = temp;
        }
    }

    function carouselSlider ($carousel) {
        var blockWidth = $carousel.find('.carousel-block').outerWidth(true);
        $carousel.animate({ left: "-" + blockWidth + "px" }, 800, function () {
            $carousel.find(".carousel-block").eq(0).clone().appendTo($carousel);
            $carousel.find(".carousel-block").eq(0).remove();
            $carousel.css({ "left": "0px" });
        });
    }
    var StickyElement = function (node) {
        var doc = jq(document),
            fixed = false,
            anchor = node.find('.auth-form-with_form_w_anchor'),
            content = node.find('.auth-form-with_form_w');
       /* var onScroll = function (e) {
            var docTop = doc.scrollTop(),
                anchorTop = anchor.offset().top;

            if (docTop > anchorTop) {
                if (!fixed) {
                    anchor.height(content.outerHeight());
                    content.addClass('fixed');
                    fixed = true;
                }
            } else {
                if (fixed) {
                    anchor.height(0);
                    content.removeClass('fixed');
                    fixed = false;
                }
            }
        };

        jq(window).on('scroll', onScroll);*/
    };

    
    var StEl = jq(window).width() >= '700'? new StickyElement(jq('.auth-form-head')) : null;
    
    jq('.share-collaborate-picture-carousel').slick({
           slidesToShow: 1,
           dots: true,
           arrows: true,
        });
    function carouselAuto(slidesCount) {
       jq('.carousel').slick({
           slidesToShow: slidesCount < 3 ? slidesCount : 2,
           centerMode: false,
           responsive: [
            {
                breakpoint: 1041,
                settings: {
                    slidesToShow: 1,
                }
            }]
        });
       
       /* var $carousel = jq("#reviewsContainer");
        setInterval(function () {
            carouselSlider($carousel);
        }, 8000);*/
    }

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
        dots: true,
        centerMode: true,
        asNavFor: '.create-carousel'
    });

    bindEvents();
    getReviewList();
    
});
