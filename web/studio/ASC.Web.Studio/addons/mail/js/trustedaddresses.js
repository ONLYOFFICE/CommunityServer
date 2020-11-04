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


window.trustedAddresses = (function() {
    var trustedAddresses = [],
        isInit = false;

    function init() {
        if (!isInit) {
            if (ASC.Mail.Presets.DisplayImagesAddresses) {
                loadTrustedAddresses(ASC.Mail.Presets.DisplayImagesAddresses);
            }
        }
    }

    function loadTrustedAddresses(addresses) {
        var i, len;
        for (i = 0, len = addresses.length; i < len; i++) {
            loadAddress(addresses[i]);
        }
        
        if(addresses.length == 0)
            storeToLocalStorage([]);
    }

    function loadAddress(address) {
        if (!isTrusted(address)) {
            trustedAddresses.push(address);
            storeToLocalStorage(trustedAddresses);
            return true;
        }
        return false;
    }

    function add(address) {
        if (loadAddress(address)) {
            window.Teamlab.createDisplayImagesAddress({}, address);
        }
    }

    function getAddresses() {
        if (trustedAddresses == undefined) {
            trustedAddresses = [];
        }

        if (localStorageManager.isAvailable) {
            if (localStorageManager.getItem("trustedAddresses") == undefined) {
                storeToLocalStorage([]);
            } else {
                trustedAddresses = localStorageManager.getItem("trustedAddresses");
            }
        }

        return trustedAddresses;
    }

    function remove(address) {
        var trustedIndex = findIndex(address);
        if (trustedIndex > -1) {
            window.Teamlab.removeDisplayImagesAddress({}, address);
            trustedAddresses.splice(trustedIndex, 1);
            storeToLocalStorage(trustedAddresses);
        }
    }

    function isTrusted(address) {
        if(commonSettingsPage.AlwaysDisplayImages())
            return true;

        var i, len, addresses = getAddresses();
        for (i = 0, len = addresses.length; i < len; i++) {
            if (addresses[i] == address) {
                return true;
            }
        }
        return false;
    }

    function findIndex(address) {
        var i, len, addresses = getAddresses();
        for (i = 0, len = addresses.length; i < len; i++) {
            if (addresses[i] == address) {
                return i;
            }
        }
        return -1;
    }

    function storeToLocalStorage(addresses) {
        localStorageManager.setItem("trustedAddresses", addresses);
    }

    return {
        init: init,
        add: add,
        remove: remove,
        isTrusted: isTrusted
    };
})(jQuery);