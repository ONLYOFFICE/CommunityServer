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


var CardDavSettings = new function () {

    var $cardDavSettingsBtn = jq("#cardDavSettingsCheckbox"),
        $cardDavUrlInput = jq("#cardDavUrl"),
        $cardDavCopyBtn = jq(".copy"),
        $cardDavTryAgainBtn = jq(".try-again");


    var init = function () {
        if ($cardDavSettingsBtn.hasClass("off")) {
            $cardDavUrlInput.prop("disabled", true).addClass("display-none");
        } else {
            $cardDavUrlInput.prop("disabled", false).removeClass("display-none");
            $cardDavCopyBtn.removeClass("display-none");
        }

        $cardDavSettingsBtn.on("click", getCardDavLink);

        $cardDavCopyBtn.on("click", copyCardDavLink);

    }
    function copyCardDavLink() {
        $cardDavUrlInput.trigger("select");
        try {
            document.execCommand('copy');
            toastr.success("Link has been copied to the clipboard");
        } catch (err) { }
    }

    function showError(params, errors) {
        
        if (errors && errors.length > 0) {
            toastr.error(errors[0]);
            console.error(errors)
        }
        window.LoadingBanner.hideLoading();
    }


    function getCardDavLink() {
        var $this = jq(this);
        var on = $this.hasClass("off");
        window.LoadingBanner.displayLoading();
        if (on) {
            Teamlab.getCardDavLink({}, {
                success: function (params, response) {
                    if (response.completed) {
                        $cardDavUrlInput.val(response.data);
                        $cardDavSettingsBtn.removeClass("off").addClass("on");
                        $cardDavCopyBtn.removeClass("display-none");
                        $cardDavUrlInput.prop("disabled", false).removeClass("display-none");
                    } else {
                        toastr.error(response.error);
                        console.error([response.statusCode, response.error].join(' '));
                    }
                    window.LoadingBanner.hideLoading();
                },
                error: showError
            });
        } else {
            Teamlab.deleteCardDavLink({}, {
                success: function (params, response) {
                    if (response.completed) {
                        $cardDavUrlInput.val("");
                        $cardDavSettingsBtn.removeClass("on").addClass("off");
                        $cardDavCopyBtn.addClass("display-none");
                        $cardDavUrlInput.prop("disabled", true).addClass("display-none");
                    } else {
                        console.error([response.statusCode, response.error].join(' '));
                    }
                    window.LoadingBanner.hideLoading();
                },
                error: showError
            });
        }
    }

    return {
        init: init
    };
}

jq(function () {
    CardDavSettings.init();
});