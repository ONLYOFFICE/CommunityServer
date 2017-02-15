/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
ASC.Files.Constants.REQUEST_CONVERT_DELAY = 500;
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

    var getCorrectHash = function (anchor) {
        if (jq.browser.mozilla) {
            return encodeURI(anchor);
        }
        return anchor;
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
            xml = replaceInXml(o);
            if (typeof parent !== "undefined") {
                xml = "<" + parent + ">" + xml + "</" + parent + ">";
            }
        } else if (typeof o !== "undefined" && typeof o.toString !== "undefined") {
            xml = replaceInXml(o.toString());
            if (typeof parent !== "undefined") {
                xml = "<" + parent + ">" + xml + "</" + parent + ">";
            }
        }
        return xml;
    };

    var replaceInXml = function (str) {
        return str.replace(/&/gim, "&amp;").replace(/</gim, "&lt;").replace(/>/gim, "&gt;");
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

    var characterString = "@#$%&*+:;\"'<>?|\/";
    var characterRegExp = new RegExp("[\t@#$%&*\+:;\"'<>?|\\\\/]", "gim");

    var replaceSpecCharacter = function (str) {
        return (str || "").trim().replace(ASC.Files.Common.characterRegExp, "_");
    };

    var keyCode = { enter: 13, esc: 27, spaceBar: 32, pageUP: 33, pageDown: 34, end: 35, home: 36, left: 37, up: 38, right: 39, down: 40, insertKey: 45, deleteKey: 46, a: 65, f: 70, n: 78 };

    return {
        getSitePath: getSitePath,
        cancelBubble: cancelBubble,
        keyCode: keyCode,

        characterString: characterString,
        characterRegExp: characterRegExp,
        replaceSpecCharacter: replaceSpecCharacter,

        isCorrectId: isCorrectId,
        getCorrectHash: getCorrectHash,
        jsonToXml: jsonToXml,

        storeOriginal: storeOriginal,
    };
})();