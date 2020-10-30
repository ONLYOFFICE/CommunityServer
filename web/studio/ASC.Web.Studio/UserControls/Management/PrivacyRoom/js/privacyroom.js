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


var Encryption = function () {
    function init () {
        jq("#saveEncryption").click(setStatus);
    }

    function showStatusValue (data) {
        var on = !!data;

        jq("#encryption_off").prop("checked", !on);
        jq("#encryption_on").prop("checked", on);
    }

    function setStatus () {
        showLoader();

        Teamlab.setPrivacyRoom(
            null,
            jq("#encryption_on").prop("checked"),
            {
                success: function (params, data) {
                    hideLoader();
                    showStatusValue(data);

                    LoadingBanner.showMesInfoBtn("#studio_encryptionSettings", ASC.Resources.Master.Resource.SuccessfullySaveSettingsMessage, "success");
                },
                error: function (e, err) {
                    LoadingBanner.showMesInfoBtn("#studio_encryptionSettings", err[0], "error");
                    hideLoader();
                }
            });
    }

    function showLoader () {
        LoadingBanner.showLoaderBtn("#studio_encryptionSettings");
    }

    function hideLoader () {
        LoadingBanner.hideLoaderBtn("#studio_encryptionSettings");
    }

    return {
        init: init
    };
}();

jQuery(function() {
    Encryption.init();
});