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
