/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


var storageManagerPrototype = function (storage) {
    var isAvailable;
    try {
        if (storage == localStorage) {
            isAvailable = "localStorage" in window && window["localStorage"] !== null;
        } else if (storage == sessionStorage) {
            isAvailable = "sessionStorage" in window && window["sessionStorage"] !== null;
        } else {
            throw "Unknown storage " + storage;
        }
    } catch (ex) {
        if (typeof console != "undefined" && console.log) {
            console.log(ex);
        }
        isAvailable = false;
    }

    var getItem = function (key) {
        if (!key || !isAvailable) {
            return null;
        }
        try {
            return JSON.parse(storage.getItem(key));
        } catch (e) {
            removeItem(key);
            if (typeof console != "undefined" && console.log) {
                console.log(e);
            }
            return null;
        }
    };

    var setItem = function (key, value) {
        if (!key || !isAvailable) {
            return;
        }
        if (value === undefined) {
            removeItem(key);
        }
        try {
            storage.setItem(key, JSON.stringify(value));
        } catch (e) {
            if (typeof QUOTA_EXCEEDED_ERR != "undefined" && e == QUOTA_EXCEEDED_ERR) {
                if (typeof console != "undefined" && console.log) {
                    console.log("Local storage is full");
                } else {
                    throw "Local storage is full";
                }
            }
        }
    };

    var removeItem = function (key) {
        if (!key || !isAvailable) {
            return;
        }
        storage.removeItem(key);
    };

    var clear = function () {
        if (!isAvailable) {
            return;
        }
        storage.clear();
    };

    return {
        isAvailable: isAvailable,

        getItem: getItem,
        setItem: setItem,
        removeItem: removeItem,
        clear: clear
    };
};

var localStorageManager;
try {
    localStorageManager = storageManagerPrototype(localStorage);
} catch (e) { }

var sessionStorageManager;
try {
    sessionStorageManager = storageManagerPrototype(sessionStorage);
} catch (e) { }
