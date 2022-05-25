/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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

window.ASC.Files.PrivateRoomOpenFile = (function () {
    function scriptProtocolCheck() {
        protocolCheck(jq('#dialogBoxCustomProtocol').attr('href'),
            function () {
                console.log("custom protocol not detected");
            },
            function () {
                localStorage.setItem('protocoldetector', 1);
            },
            function () {
                console.log("custom protocol detection is not supported");
            }
        );
    };

    function registerEvent(target, eventType, cb) {
        if (target.addEventListener) {
            target.addEventListener(eventType, cb);
            return {
                remove: function () {
                    target.removeEventListener(eventType, cb);
                }
            };
        } else {
            target.attachEvent(eventType, cb);
            return {
                remove: function () {
                    target.detachEvent(eventType, cb);
                }
            };
        }
    };

    function createHiddenIframe(target, uri) {
        var iframe = document.createElement("iframe");
        iframe.src = uri;
        iframe.id = "hiddenIframe";
        iframe.style.display = "none";
        target.appendChild(iframe);
        return iframe;
    };

    /// Google Chrome version 90 and Microsoft Edge version 90 and Opera version 75 on Win10 64 bit - Work
    function openUriWithTimeoutHack(uri, failCb, successCb) {
        var timeout = setTimeout(function () {
            failCb();
            handler.remove();
        }, 1000);

        var target = window;
        while (target != target.parent) {
            target = target.parent;
        }

        var handler = registerEvent(target, "blur", onBlur);

        function onBlur() {
            clearTimeout(timeout);
            handler.remove();
            successCb();
        }

        window.location = uri;
    };

    /// Firefox version 88 on Win10 64 bit - Work
    function openUriUsingFirefox(uri, failCb, successCb) {
        var iframe = document.querySelector("#hiddenIframe");
        if (!iframe) {
            iframe = createHiddenIframe(document.body, "about:blank");
        }
        try {
            iframe.contentWindow.location.href = uri;
            setTimeout(function () {
                try {
                    if (iframe.contentWindow.location.protocol === "about:") {
                        successCb();
                    } else {
                        failCb();
                    }
                } catch (e) {
                    if (e.name === "NS_ERROR_UNKNOWN_PROTOCOL" || e.name === "NS_ERROR_FAILURE" || e.name === "SecurityError") {
                        failCb();
                    }
                }
            }, 1000);
        } catch (e) {
            if (e.name === "NS_ERROR_UNKNOWN_PROTOCOL" || e.name === "NS_ERROR_FAILURE" || e.name === "SecurityError") {
                failCb();
            }
        }
    };

    /// Safari - No checked
    function openUriWithHiddenFrame(uri, failCb, successCb) {
        var timeout = setTimeout(function () {
            failCb();
            handler.remove();
        }, 1000);

        var iframe = document.querySelector("#hiddenIframe");
        if (!iframe) {
            iframe = createHiddenIframe(document, "about:blank");
        }

        var handler = registerEvent(window, "blur", onBlur);

        function onBlur() {
            clearTimeout(timeout);
            handler.remove();
            successCb();
        }

        iframe.contentWindow.location.href = uri;
    };

    function checkBrowser() {
        return {
            isOpera: jq.browser.opera,
            isFirefox: jq.browser.mozilla,
            isChrome: jq.browser.chrome,
            isSafari: jq.browser.safari,
            isIOS: /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream
        }
    };

    function protocolCheck(uri, failCb, successCb, unsupportedCb) {
        function failCallback() {
            failCb && failCb();
        }

        function successCallback() {
            successCb && successCb();
        }

        if (navigator.msLaunchUri) {
            /// For Microsoft Edge version 90 Win 10
            openUriWithTimeoutHack(uri, failCallback, successCallback);
        } else {
            var browser = checkBrowser();
            if (browser.isFirefox) {
                openUriUsingFirefox(uri, failCallback, successCallback);
            } else if (browser.isChrome || browser.isOpera || browser.isIOS) {
                openUriWithTimeoutHack(uri, failCallback, successCallback);
            } else if (browser.isSafari) {
                openUriWithHiddenFrame(uri, failCallback, successCallback);
            } else {
                unsupportedCb();
            }
        }
    };

    return {
        scriptProtocolCheck: scriptProtocolCheck,
    };
})();

(function ($) {
    jq(function () {
        ASC.Files.PrivateRoomOpenFile.scriptProtocolCheck();
    });
})(jQuery);
