/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


(function($) {
    if (typeof window.ASC === 'undefined') {
        window.ASC = {};
    }

    if (typeof window.ASC.Mail === 'undefined') {
        window.ASC.Mail = {};
    }

    if (typeof LoadingBanner !== 'undefined') {
        LoadingBanner.displayMailLoading = function(msg) {
            if (!$('.loader-page').length) {
                LoadingBanner.strLoading = msg || ASC.Resources.Master.Resource.LoadingProcessing;
                LoadingBanner.loaderCss = "mail-module";
                LoadingBanner.displayLoading();
            }
        };
    }

    window.ASC.Mail.emptyNode = function(o) {
        if (typeof o === 'string') {
            o = document.getElementById(o);
        }
        if (!o || typeof o !== 'object') {
            return null;
        }
        while (o.firstChild) {
            o.removeChild(o.firstChild);
        }
        return o;
    };

    window.ASC.Mail.updateAreaHeight = function(o) {
        o.style.height = defaultHeight + 'px';
        var height = o.offsetHeight,
            scrollHeight = o.scrollHeight;

        if (scrollHeight > height) {
            while (scrollHeight > height) {
                height += lineHeight;
            }
            o.style.height = height + 'px';
        }
    };

    window.ASC.Mail.setVariableHeight = function(areas, params) {
        if (!params || typeof params !== 'object') {
            params = {};
        }
        var defaultHeight = params.hasOwnProperty('height') ? params.height : 16;
        var lineHeight = params.hasOwnProperty('lineHeight') ? params.lineHeight : 16;
        if (typeof areas === 'string') {
            areas = document.getElementById(areas);
        }
        if (!(areas instanceof Array)) {
            areas = [areas];
        }
        var o;
        var i = areas.length;
        while (i--) {
            if (typeof areas[i] === 'string') {
                o = document.getElementById(areas[i]);
            } else {
                o = areas[i];
            }
            if (!o || typeof o !== 'object') {
                continue;
            }

            var updateAreaHeight = function(obj) {
                obj.style.height = defaultHeight + 'px';
                var height = obj.offsetHeight,
                    scrollHeight = obj.scrollHeight;

                if (scrollHeight > height) {
                    while (scrollHeight > height) {
                        height += lineHeight;
                    }
                    obj.style.height = height + 'px';
                }
            };

            $(o).keydown(function() {
                setImmediate((function(obj) { return function() { updateAreaHeight(obj); }; })(this));
            });
        }
    };

    window.ASC.Mail.arrayExclude = function(firstArr, secondArr) {
        var localArr = firstArr,
            localArrInd = localArr.length,
            secondArrInd;
        while (localArrInd--) {
            secondArrInd = secondArr.length;
            while (secondArrInd--) {
                if (localArr[localArrInd] == secondArr[secondArrInd]) {
                    firstArr.splice(localArrInd, 1);
                    break;
                }
            }
        }
        return firstArr;
    };

    window.ASC.Mail.arrayComplement = function(firstArr, secondArr) {
        var isExist,
            localArr = firstArr,
            localArrInd = localArr.length,
            secondArrInd;
        while (localArrInd--) {
            isExist = false;
            secondArrInd = secondArr.length;
            while (secondArrInd--) {
                if (localArr[localArrInd] == secondArr[secondArrInd]) {
                    isExist = true;
                    break;
                }
            }
            if (isExist) {
                localArr.splice(localArrInd, 1);
            }
        }
        return localArr.concat(secondArr);
    };

    window.ASC.Mail.stringFormat = function() {
        if (!arguments.length) {
            return '';
        }
        var pos,
            ind,
            reInd = /{(\d)}/g,
            message = arguments[0];
        while (ind = reInd.exec(message)) {
            pos = -1;
            while ((pos = message.indexOf(ind[0], pos + 1)) !== -1) {
                message = message.substring(0, pos) + (arguments[+ind[1] + 1] || '') + message.substring(pos + ind[0].length);
            }
        }
        return message;
    };

    window.ASC.Mail.cookies = (function() {
        var get = function(name) {
            if (typeof name === 'undefined') {
                return '';
            }
            var start,
                end,
                value = '',
                search = ' ' + name + '=',
                cookie = ' ' + document.cookie;

            if ((start = cookie.indexOf(search)) !== -1) {
                start += search.length;
                if ((end = cookie.indexOf(';', start)) === -1) {
                    end = cookie.length;
                }
                value = cookie.substring(start, end);
            }
            return decodeURI(value);
        };

        var set = function(name, value, expires, path, domain, secure) {
            if (typeof name === 'undefined' || typeof value === 'undefined') {
                return undefined;
            }
            expires = expires || 365;
            if (isFinite(+expires)) {
                var currentDate = new Date();
                currentDate.setTime(currentDate.getTime() + expires * 8640000);
                expires = currentDate.toUTCString();
            }
            expires = expires ? ';expires=' + expires : '';
            path = path ? ';path=' + path : ';path=/';
            domain = domain ? ';domain=' + domain : '';
            secure = secure ? ';secure=' + secure : '';
            document.cookie = name + '=' + encodeURI(value) + expires + path + domain + secure;
            return value;
        };

        var remove = function(name, path, domain) {
            if (name && document.cookie.indexOf(name + '=') != -1) {
                path = path ? ';path=' + path : ';path=/';
                domain = domain ? ';domain=' + domain : '';
                document.cookie = name + '=' + path + domain + ';expires=Thu, 01-Jan-1970 00:00:01 GMT';
            }
            return true;
        };

        return {
            get: get,
            set: set,
            remove: remove
        };
    })();

    // google analitics track
    window.ASC.Mail.ga_track = function(category, action, label) {
        trackingGoogleAnalitics(category, action, label);
    };

    // retrieves highlighted selected text
    window.ASC.Mail.getSelectionText = function() {
        var text = "";
        if (window.getSelection) {
            text = window.getSelection().toString();
        } else if (document.selection && document.selection.type != "Control") {
            text = document.selection.createRange().text;
        }
        return text;
    };

    $.fn.trackEvent = function(category, action, label) {
        $(this).on("click", function() {
            window.ASC.Mail.ga_track(category, action, label);
        });
    };


})(jQuery);

// Google Analytics const
var ga_Categories = {
    folder: "folder",
    teamlabContacts: "teamlab_contacts",
    crmContacts: "crm_contacts",
    personalContacts: "personal_contacts",
    accauntsSettings: "accaunts_settings",
    tagsManagement: "tags_management",
    createMail: "create_mail",
    message: "message",
    leftPanel: "left_panel"
};

var ga_Actions = {
    filterClick: "filter_click",
    createNew: "create_new",
    update: "update",
    next: "next",
    actionClick: "action_click",
    quickAction: "quick_action",
    buttonClick: "button_click"
};