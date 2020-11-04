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
ASC.Files.Constants.REQUEST_CONVERT_DELAY = 2000;
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
        if (jq.browser.safari || jq.browser.mozilla) {
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

    var characterString = "*+:\"<>?|\/";
    var characterRegExp = new RegExp("[\t*\+:\"<>?|\\\\/]", "gim");

    var replaceSpecCharacter = function (str) {
        return (str || "").trim().replace(ASC.Files.Common.characterRegExp, "_");
    };

    var keyCode = { enter: 13, ctrl: 17, esc: 27, spaceBar: 32, pageUP: 33, pageDown: 34, end: 35, home: 36, left: 37, up: 38, right: 39, down: 40, insertKey: 45, deleteKey: 46, A: 65, C: 67, F: 70, N: 78, F2: 113 };

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