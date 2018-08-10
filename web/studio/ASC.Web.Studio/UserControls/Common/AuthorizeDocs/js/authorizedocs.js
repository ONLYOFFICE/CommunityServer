/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


jq(function () {
    jq('#login').blur();
    
    if (jq.cookies.get('onluoffice_personal_cookie') == null || jq.cookies.get('onluoffice_personal_cookie') == false) {
        jq('.cookieMess').css('display', 'table');
    }

    if (jq("#agree_to_terms").prop("checked")) {
        bindConfirmEmailBtm();
        jq('.account-links a')
                    .removeClass('disabled')
                    .unbind('click');
    }

    function bindConfirmEmailBtm() {

        jq('.account-links a').removeClass('disabled');

        jq("#confirmEmailBtn")
            .removeClass('disabled')
            .on("click", function () {
                var $email = jq("#confirmEmail"),
                    email = $email.val().trim(),
                    spam = jq("#spam").prop("checked"),
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
                    "spam": spam
                };

                var onError = function (error) {
                    $error.html(error);
                    $email.addClass("error");
                };

                Teamlab.registerUserOnPersonal(data, {
                    success: function (arg, res) {
                        if (res && res.length) {
                            onError(res);
                            return;
                        }
                        $error.empty();
                        jq("#activationEmail").html(email);
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
                jq("#confirmEmail").focus();
                hideAccountLinks();
            });
        });

        var ua = window.navigator.userAgent;
        if (ua.indexOf("CrOS") != -1) {
            jq("#chromebookText").show();
        }
        // close popup window
        jq(".default-personal-popup_closer").on("click", function () {
            hideAccountLinks();
            jq(this).parents(".default-personal-popup").fadeOut(200, function() {
                enableScroll();
            });
            
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
                jq('.account-links a')
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

            if (code == 27) {
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
            showAccountLinks();
            jq("#loginPopup").show();
            jq('#login').focus();
            disableScroll();
        });

        var loginMessage = jq(".login-message[value!='']").val();
        if (loginMessage && loginMessage.length) {
            jq("#loginPopup").show();
            disableScroll();
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
        var onScroll = function (e) {
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

        jq(window).on('scroll', onScroll);
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
