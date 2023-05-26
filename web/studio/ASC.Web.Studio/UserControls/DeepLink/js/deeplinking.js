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


ASC.DeepLinking = (function () {
    var $ = jq;

    var storeLink, originalUrl;

    function init(fileName, storelink, originalurl) {

        storeLink = storelink;
        originalUrl = originalurl;
        var fileIcon = window.ASC.Files.Utility.getCssClassByFileTitle(fileName, true);

        $(".thumb-file").addClass(fileIcon);
        $("#appButton").click(appButton);
        $("#browserButton").click(browserButton);

    }
    function appButton() {
        if ($("#rememberSelector").is(":checked")) $.cookies.set("deeplink", "app", { path: "/" });
        setTimeout(function () {
            if (document.hasFocus()) {
                window.location.replace(storeLink);
            }
        }, 3000);
    }
    function browserButton() {
        if ($("#rememberSelector").is(":checked")) $.cookies.set("deeplink", "browser", { path: "/" });
        window.location.replace(originalUrl);
    }

    return jq.extend({
        init: init
    }, ASC.DeepLinking);
})();