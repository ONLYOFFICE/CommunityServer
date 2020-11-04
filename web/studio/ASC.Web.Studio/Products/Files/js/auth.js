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


/*******************************************************************************
    Auth for Thirdparty
*******************************************************************************/

var OAuthCallback = function (token) {
};

var OAuthError = function (error) {
    ASC.Files.UI.displayInfoPanel(error, true);
};

var OAuthPopup = function (url, providerKey, providerId) {
    if (ASC.Desktop) {
        var redirect = ASC.Files.Anchor.modulePath
            + "#setting=thirdparty&"
            + (providerKey ? ("providerKey=" + encodeURIComponent(providerKey))
                : (providerId ? ("providerId=" + encodeURIComponent(providerId)) : ""));
        location.href = url + "?redirect=" + encodeURIComponent(redirect);
        return null;
    }

    var newwindow;
    try {
        var params = "height=600,width=1020,resizable=0,status=0,toolbar=0,menubar=0,location=1";
        newwindow = window.open(url, "Authorization", params);
    } catch (err) {
        newwindow = window.open(url, "Authorization");
    }
    return newwindow;
};