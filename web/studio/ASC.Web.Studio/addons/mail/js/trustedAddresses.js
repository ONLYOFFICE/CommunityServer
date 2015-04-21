/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


window.trustedAddresses = (function() {
    var trustedAddresses = [];

    function init() {
        serviceManager.bind(window.Teamlab.events.getMailDisplayImagesAddresses, onGetMailDisplayImagesAddresses);

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