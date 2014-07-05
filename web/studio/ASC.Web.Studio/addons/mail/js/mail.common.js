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
(function($) {
    if (typeof window.ASC === 'undefined') {
        window.ASC = {};
    }

    if (typeof window.ASC.Mail === 'undefined') {
        window.ASC.Mail = {};
    }

    if (typeof LoadingBanner !== 'undefined') {
        LoadingBanner.displayMailLoading = function(withoutDelay, withBackdrop, config) {
            LoadingBanner.animateDelay = config && config.animateDelay || 300;
            LoadingBanner.displayDelay = config && config.displayDelay || 2000;
            LoadingBanner.loaderCss = config && config.loaderCss || "mail-module";
            LoadingBanner.displayOpacity = config && config.displayOpacity || 1;

            LoadingBanner.displayLoading(withoutDelay, withBackdrop);
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
        var 
      height = o.offsetHeight,
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
        var 
      defaultHeight = params.hasOwnProperty('height') ? params.height : 16;
        lineHeight = params.hasOwnProperty('lineHeight') ? params.lineHeight : 16;
        if (typeof areas === 'string') {
            areas = document.getElementById(areas);
        }
        if (!(areas instanceof Array)) {
            areas = [areas];
        }
        var o = null;
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

            var updateAreaHeight = function(o) {
                o.style.height = defaultHeight + 'px';
                var 
          height = o.offsetHeight,
          scrollHeight = o.scrollHeight;

                if (scrollHeight > height) {
                    while (scrollHeight > height) {
                        height += lineHeight;
                    }
                    o.style.height = height + 'px';
                }
            };

            $(o).keydown(function() {
                setImmediate((function (o) { return function () { updateAreaHeight(o); }; })(this));
            });
        }
    };

    window.ASC.Mail.arrayExclude = function(firstArr, secondArr) {
        var 
      isExist = false,
      localArr = firstArr,
      localArrInd = localArr.length,
      secondArrInd = 0;
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
        var 
      isExist = false,
      localArr = firstArr,
      localArrInd = localArr.length,
      secondArrInd = 0;
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
        var 
      pos = -1,
      ind = [],
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
            var 
        start = 0,
        end = 0,
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
        try {
            if (window._gat) {
                window._gaq.push(['_trackEvent', category, action, label]);
            }
        } catch (err) {
        }
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