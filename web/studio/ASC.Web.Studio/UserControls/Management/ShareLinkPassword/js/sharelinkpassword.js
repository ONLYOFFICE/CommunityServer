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


window.ShareLinkPasswordManager = new function () {

    var $shareLinkPasswordSendBlock = jq("#shareLinkPasswordSendBlock");
    var $passwordInput = jq("#shareLinkPasswordInput");
    var $passwordLabel = jq("#shareLinkPasswordLabel");
    var $shareLinkPasswordInfo = jq("#shareLinkPasswordInfo");
    var $sendBtn = jq("#shareLinkPasswordSendBtn");

    var $shareLinkPasswordDownloadBlock = jq("#shareLinkPasswordDownloadBlock");
    var $shareLinkPasswordDownloadBtn = jq("#shareLinkPasswordDownloadBtn");

    var init = function () {
        $sendBtn.on("click", sendPassword);
        $passwordLabel.on("click", togglePasswordType);
        $passwordInput.on("keydown", inputKeyDown).trigger("focus");
    };

    var inputKeyDown = function (event) {
        if ((event.keyCode || event.which) == 13) {
            sendPassword();
            return false;
        }
        return true;
    };

    var togglePasswordType = function () {
        if ($passwordInput.prop("disabled")) return;

        if ($passwordInput.attr("type") == "password") {
            $passwordInput.attr("type", "text");
            $passwordLabel.removeClass("hide-label").addClass("show-label");
        } else {
            $passwordInput.attr("type", "password");
            $passwordLabel.removeClass("show-label").addClass("hide-label");
        }
    };

    var blockElements = function (block) {
        if (block) {
            $passwordInput.prop("disabled", true);
            $shareLinkPasswordInfo.addClass("visibility-hidden");
            LoadingBanner.showLoaderBtn($shareLinkPasswordSendBlock);
        } else {
            $passwordInput.prop("disabled", false);
            $shareLinkPasswordInfo.removeClass("visibility-hidden");
            LoadingBanner.hideLoaderBtn($shareLinkPasswordSendBlock);
        }
    }

    var sendPassword = function () {
        if ($passwordInput.prop("disabled")) return;

        var password = $passwordInput.val().trim();

        if (!password) return;

        blockElements(true);

        window.hashPassword(password, function (passwordHash) {
            var params = new URL(location.href).searchParams;
            var isFolder = (params.get('folder') ?? 'false').toLowerCase() === 'true';
            var data = {
                key: params.get("key"),
                passwordHash: passwordHash,
                isFolder: isFolder
            };

            Teamlab.applySharedLinkPassword(password, data, {
                after: function () {
                    blockElements(false);
                },
                success: function (_, link) {
                    $shareLinkPasswordSendBlock.addClass("display-none");
                    $shareLinkPasswordDownloadBtn.attr("href", link);
                    $shareLinkPasswordDownloadBlock.removeClass("display-none");
                    window.location.href = link;
                },
                error: function (_, errors) {
                    toastr.error(errors[0]);
                },
                processUrl: function (url) {
                    return ASC.Files.Utility.AddExternalShareKey(url);
                }
            });
        });
    };

    return {
        init: init
    };
};

jq(function () {
    window.ShareLinkPasswordManager.init();
});