/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

window.trustedAddresses = (function ($) {
    var trusted_addresses = [];

    function init() {
        serviceManager.bind(window.Teamlab.events.getMailDisplayImagesAddresses, onGetMailDisplayImagesAddresses);

        if (TMMail.isLocalStorageAvailable())
            storeToLocalStorage([]);

        window.Teamlab.getMailDisplayImagesAddresses();
    }

    function onGetMailDisplayImagesAddresses(params, addresses) {
        loadTrustedAddresses(addresses);
    }

    function loadTrustedAddresses(addresses) {
        var i, len;
        for (i = 0, len = addresses.length; i < len; i++) {
            loadAddress(addresses[i]);
        }
    }

    function loadAddress(address) {
        if (!isTrusted(address)) {
            trusted_addresses.push(address);
            storeToLocalStorage(trusted_addresses);
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
        if (trusted_addresses == undefined)
            trusted_addresses = [];

        if (TMMail.isLocalStorageAvailable()) {
            if (localStorage.trustedAddresses == undefined)
                storeToLocalStorage([]);
            else {
                trusted_addresses = JSON.parse(localStorage.trustedAddresses);
            }
        }

        return trusted_addresses;
    }

    function remove(address) {
        var trusted_index = findIndex(address);
        if (trusted_index > -1) {
            window.Teamlab.removeDisplayImagesAddress({}, address);
            trusted_addresses.splice(trusted_index, 1);
            storeToLocalStorage(trusted_addresses);
        }
    }

    function isTrusted(address) {
        var i, len, addresses = getAddresses();
        for (i = 0, len = addresses.length; i < len; i++) {
            if (addresses[i] == address)
                return true;
        }
        return false;
    }

    function findIndex(address) {
        var i, len, addresses = getAddresses();
        for (i = 0, len = addresses.length; i < len; i++) {
            if (addresses[i] == address)
                return i;
        }
        return -1;
    }

    function storeToLocalStorage(addresses) {
        try {
            if (TMMail.isLocalStorageAvailable()) {
                localStorage.trustedAddresses = JSON.stringify(addresses);
            }
        }
        catch (e) { }
    }

    return {
        init: init,
        add: add,
        remove: remove,
        isTrusted: isTrusted
    };
})(jQuery);