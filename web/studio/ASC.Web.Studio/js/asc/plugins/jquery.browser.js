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


        /*** isFlashEnabled ***/
        browser.flashEnabled = function () {
            try {
                var fo = new ActiveXObject('ShockwaveFlash.ShockwaveFlash');
                if (fo) {
                    return true;
                }
            } catch (e) {
                if (navigator.mimeTypes
                        && navigator.mimeTypes['application/x-shockwave-flash'] != undefined
                        && navigator.mimeTypes['application/x-shockwave-flash'].enabledPlugin) {
                    return true;
                }
            }
            return false;
        };
        /*** ***/

        jQuery.browser = browser;
       

    }

})(jQuery, window);