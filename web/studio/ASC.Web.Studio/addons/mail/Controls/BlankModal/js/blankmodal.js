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


window.blankModal = (function ($) {

    var init = function () {
        jq(".dashboard-center-box .close").on("click", close);

        jq(document).keyup(function (event) {
            var code;

            if (event.keyCode) {
                code = event.keyCode;
            } else if (event.which) {
                code = event.which;
            }

            if (code == 27) {
                close();
            }
        });

        jq(".dashboard-center-box .slick-carousel").slick({
            slidesToShow: 1,
            slidesToScroll: 1,
            arrows: true,
            dots: true,
            fade: true,
            centerMode: true
        });
    }

    var show = function () {
        init();
        $('[blank-page]').removeClass("hidden");
        jq(".dashboard-center-box .slick-next").focus();
    };

    var close = function() {
        $('[blank-page]').remove();
    };

    var addAccount = function() {
        ASC.Controls.AnchorController.move('#accounts');
        accountsModal.addBox(undefined);
        close();
    };

    var setUpDomain = function() {
        ASC.Controls.AnchorController.move('#administration');
        close();
    };

    var addMailbox = function () {
        ASC.Controls.AnchorController.move('#accounts');
        accountsModal.addMailbox(undefined);
        close();
    };

    return {
        show: show,
        close: close,
        addAccount: addAccount,
        setUpDomain: setUpDomain,
        addMailbox: addMailbox
    };
})(jQuery);