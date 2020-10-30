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

    var hash = window.location.hash;

    if (hash && hash.indexOf("#preview") == 0) {
        return;
    } else {
        jq("#dashboardBackdrop, #dashboardContent").removeClass("display-none");
    }

    jq("#dashboardContent .close").on("click", function () {
        jq("[blank-page]").remove();
        var href = jq(this).attr("href");
        if (href) {
            location.href = href;
        }
    });

    jq(document).keyup(function (event) {
        var code;

        if (event.keyCode) {
            code = event.keyCode;
        } else if (event.which) {
            code = event.which;
        }

        if (code == 27) {
            jq("#dashboardContent .close").click();
        }
    });

    jq("#dashboardContent .slick-carousel").slick({
        slidesToShow: 1,
        slidesToScroll: 1,
        arrows: true,
        dots: true,
        fade: true,
        centerMode: true
    });

    jq("#dashboardContent .slick-next").focus();

});