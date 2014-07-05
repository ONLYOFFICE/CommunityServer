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

(function (jQuery, window, undefined) {

    // Don't clobber any existing jQuery.browser in case it's different
    if (!jQuery.browser) {

        jQuery.uaMatch = function (ua) {
            ua = ua.toLowerCase();

            var match = /(chrome)[ \/]([\w.]+)/.exec(ua) ||
                /(webkit)[ \/]([\w.]+)/.exec(ua) ||
                /(opera)(?:.*version|)[ \/]([\w.]+)/.exec(ua) ||
                /(opr)[\/]([\w.]+)/.exec(ua) ||
                /(msie) ([\w.]+)/.exec(ua) ||
                ua.indexOf("trident") >= 0 && /(rv)(?::| )([\w.]+)/.exec(ua) ||
                ua.indexOf("compatible") < 0 && /(mozilla)(?:.*? rv:([\w.]+)|)/.exec(ua) ||
                [];

            return {
                browser: match[1] || "",
                version: match[2] || "0"
            };
        };

        var ua = window.navigator.userAgent;
        //This fixes an ie7 bug that causes crashes from incorrect version identification
        if (/*@cc_on/*@if(@_jscript_version<=5.6)1@else@*/0/*@end@*/) {
            ua = "msie 6.0";
        }

        var matched = jQuery.uaMatch(ua);
        var browser = {};

        if (matched.browser) {
            browser[matched.browser] = true;
            browser.version = matched.version;
        }

        // Chrome is Webkit, but Webkit is also Safari.
        if (browser.chrome || browser.opr) {
            browser.webkit = true;
        } else if (browser.webkit) {
            browser.safari = true;
        }

        // IE11 has a new token so we will assign it msie to avoid breaking changes
        if (browser.rv) {
            browser.msie = true;
        }

        // Opera 15+ are identified as opr
        if (browser.opr) {
            browser.opera = true;
        }

        browser.chrome = browser.chrome === true && typeof(window.chrome) === "object";
        browser.safari = browser.safari === true && !browser.chrome;
        browser.versionCorrect = function () {
            var ver = (ua.match(/.+(?:rv|it|ra|ie|ox|me|id|on|os)[\/:\s]([\d._]+)/i) || [0, '0'])[1].replace('_', '');
            var floatVer = parseFloat(ver);
            return isFinite(floatVer) ? floatVer : ver;
        }();

        browser.mobile = document.body.className.indexOf("mobile") != -1;

        jQuery.browser = browser;
    }

})(jQuery, window);