/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

    function bindEvents () {
        jq(function () {
            jq("#loginSignUp").on("click", function () {
                jq("#loginPopup").hide();
                jq("#confirmEmail").focus();
            });
        });

        var ua = window.navigator.userAgent;
        if (ua.indexOf("CrOS") != -1) {
            jq("#chromebookText").show();
        }
        // close popup window
        jq(".default-personal-popup_closer").on("click", function () {
            jq(this).parents(".default-personal-popup").fadeOut(200);
        });

        // confirm the email
        jq(document).on("keypress", "#confirmEmail", function (evt) {
            jq(this).removeClass("error");
            if (evt.keyCode == 13) {
                evt.preventDefault();
                jq("#confirmEmailBtn").trigger("click");
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
                jq(".default-personal-popup").fadeOut(200);
            }
        });

        jq("#confirmEmailBtn").on("click", function () {
            var $email = jq("#confirmEmail"),
                email = $email.val(),
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
                "campaign": !!(jq("#confirmEmailBtn").attr("data-campaign").length),
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

        // Login
        jq("#personalLogin a").on("click", function () {
            jq("#loginPopup").show();
        });

        var loginMessage = jq(".login-message[value!='']").val();
        if (loginMessage && loginMessage.length) {
            jq("#loginPopup").show();
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
        var blockHeight = $carousel.find('.carousel-block').outerHeight(true);
        $carousel.animate({top: "-" + blockHeight + "px"}, 800, function () {
            $carousel.find(".carousel-block").eq(0).clone().appendTo($carousel);
            $carousel.find(".carousel-block").eq(0).remove();
            $carousel.css({"top": "0px"});
        });
    }

    function carouselAuto () {
        var $carousel = jq("#reviewsContainer");
        setInterval(function () {
            carouselSlider($carousel);
        }, 8000);
    }

    jq.fn.duplicate = function (count, cloneEvents) {
        var tmp = [];
        for (var i = 0; i < count; i++) {
            jq.merge(tmp, this.clone(cloneEvents).get());
        }
        return this.pushStack(tmp);
    };

    bindEvents();
    getReviewList();
    carouselAuto();
});
