/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
if (typeof jq === "undefined") {
    jq = jQuery.noConflict();
}

if (typeof ASC === "undefined") {
    ASC = {};
}

if (typeof ASC.Files === "undefined") {
    ASC.Files = (function () {
        return {};
    })();
}

if (typeof (ASC.Files.Constants) === 'undefined') {
    ASC.Files.Constants = {};
}

ASC.Files.Constants.REQUEST_STATUS_DELAY = 2000;
ASC.Files.Constants.COUNT_ON_PAGE = 30,
ASC.Files.Constants.entryIdRegExpStr = "(\\d+|[a-z]+-\\d+(-.+)*)",
ASC.Files.Constants.storageKeyRecent = "TeamLabRecentDocuments",
ASC.Files.Constants.storageKeyCompactView = "TeamLabDocumentsCompactView",
ASC.Files.Constants.storageKeyUploaderCompactView = "TeamLabDocumentsUploaderCompactView";

ASC.Files.Common = (function () {

    var storeOriginal = false;

    var isCorrectId = function (id) {
        if (typeof id === "undefined") {
            return false;
        }
        if (id === null) {
            return false;
        }
        if (id === 0) {
            return false;
        }
        var regExp = new RegExp("^" + ASC.Files.Constants.entryIdRegExpStr);
        return regExp.test(id);
    };

    var jsonToXml = function (o, parent) {
        var xml = "";

        if (typeof o === "object") {
            if (o === null) {
                if (typeof parent !== "undefined") {
                    xml += "<" + parent + "></" + parent + ">";
                }
            } else if (o.constructor.toString().indexOf("Array") !== -1) {
                var n;
                for (i = 0, n = o.length; i < n; i++) {
                    if (typeof parent !== "undefined") {
                        xml += "<" + parent + ">" + arguments.callee(o[i]) + "</" + parent + ">";
                    } else {
                        xml += arguments.callee(o[i]);
                    }
                }
            } else {
                for (var i in o) {
                    xml += arguments.callee(o[i], i);
                }
                if (typeof parent !== "undefined") {
                    xml = "<" + parent + ">" + xml + "</" + parent + ">";
                }
            }
        } else if (typeof o === "string") {
            xml = o;
            if (typeof parent !== "undefined") {
                xml = "<" + parent + ">" + xml + "</" + parent + ">";
            }
        } else if (typeof o !== "undefined" && typeof o.toString !== "undefined") {
            xml = o.toString();
            if (typeof parent !== "undefined") {
                xml = "<" + parent + ">" + xml + "</" + parent + ">";
            }
        }
        return xml;
    };

    var getSitePath = function () {
        var sitePath = jq.url.attr("protocol");
        sitePath += "://";
        sitePath += jq.url.attr("host");
        if (jq.url.attr("port") != null) {
            sitePath += ":";
            sitePath += jq.url.attr("port");
        }
        return sitePath;
    };

    var cancelBubble = function (e) {
        if (!e) {
            e = window.event;
        }
        e.cancelBubble = true;
        if (e.stopPropagation) {
            e.stopPropagation();
        }
    };

    var fixEvent = function (e) {
        e = e || window.event;
        if (!e) {
            return {};
        }
        if (e.pageX == null && e.clientX != null) {
            var html = document.documentElement;
            var body = document.body;
            e.pageX = e.clientX + (html && html.scrollLeft || body && body.scrollLeft || 0) - (html.clientLeft || 0);
            e.pageY = e.clientY + (html && html.scrollTop || body && body.scrollTop || 0) - (html.clientTop || 0);
        }

        if (!e.which && e.button) {
            e.which = e.button & 1 ? 1 : (e.button & 2 ? 3 : (e.button & 4 ? 2 : 0));
        }

        return e;
    };

    var localStorageManager = function () {
        var isAvailable;
        try {
            isAvailable = "localStorage" in window && window["localStorage"] !== null;
        } catch (e) {
            isAvailable = false;
        }

        var getItem = function (key) {
            if (!key && !ASC.Files.Common.localStorageManager.isAvailable) {
                return null;
            }
            return JSON.parse(localStorage.getItem(key));
        };

        var setItem = function (key, value) {
            if (!key || !ASC.Files.Common.localStorageManager.isAvailable) {
                return;
            }
            try {
                localStorage.setItem(key, JSON.stringify(value));
            } catch (e) {
                if (typeof QUOTA_EXCEEDED_ERR != "undefined" && e == QUOTA_EXCEEDED_ERR) {
                    //throw "Local storage is full";
                }
            }
        };

        return {
            isAvailable: isAvailable,

            getItem: getItem,
            setItem: setItem
        };
    }();

    var characterString = "@#$%&*+:;\"'<>?|\/";
    var characterRegExp = new RegExp("[@#$%&*\+:;\"'<>?|\\\\/]", "gim");

    var replaceSpecCharacter = function (str) {
        return str.trim().replace(ASC.Files.Common.characterRegExp, "_");
    };

    var fixHash = function (hash) {
        if (jq.browser.mozilla || jq.browser.safari) {
            hash = encodeURIComponent(hash);
        }
        return hash;
    };

    var keyCode = { enter: 13, esc: 27, spaceBar: 32, pageUP: 33, pageDown: 34, end: 35, home: 36, left: 37, up: 38, right: 39, down: 40, deleteKey: 46, a: 65, f: 70, n: 78 };

    return {
        getSitePath: getSitePath,
        cancelBubble: cancelBubble,
        fixEvent: fixEvent,
        keyCode: keyCode,

        characterString: characterString,
        characterRegExp: characterRegExp,
        replaceSpecCharacter: replaceSpecCharacter,

        isCorrectId: isCorrectId,
        jsonToXml: jsonToXml,

        storeOriginal: storeOriginal,
        localStorageManager: localStorageManager,

        fixHash: fixHash,
    };
})();