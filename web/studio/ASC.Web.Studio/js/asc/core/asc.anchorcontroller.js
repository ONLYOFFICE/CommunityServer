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


if (typeof window.ASC === 'undefined') {
    window.ASC = {};
}
if (typeof window.ASC.Controls === 'undefined') {
    window.ASC.Controls = {};
}

if (typeof window.ASC.Controls.AnchorController === 'undefined') {
    window.ASC.Controls.AnchorController = (function() {
        var isInit = false,
            domIsReady = false,
            checkInterval = 200,
            firstAnchor = '',
            lastAnchor = '#',
            currentAnchor = '#',
            cmdSeparator = '/',
            markCollection = [],
            customEvents = {},
            supportedCustomEvents = {
                // update location.hash
                'onupdate': true,
                'ondomready': true
            },
            browser = (function() {
                var ua = navigator.userAgent,
                    version = (ua.match(/.+(?:rv|it|ra|ie|ox|me|on)[\/:\s]([\d.]+)/i) || [0, '0'])[1];
                return {
                    version: isFinite(+version) ? +version : version,
                    // popular browsers
                    msie: '\v' == 'v' || /msie/i.test(ua),
                    opera: window.opera ? true : false,
                    chrome: window.chrome ? true : false,
                    safari: /safari/i.test(ua) && !window.chrome,
                    firefox: /firefox/i.test(ua),
                    mozilla: /mozilla/i.test(ua) && !/(compatible|webkit)/.test(ua),
                    // engine
                    gecko: /gecko/i.test(ua),
                    webkit: /webkit/i.test(ua)
                };
            })(),
            ieHelper = null,
            needHelper = browser.msie && (browser.version < 8 || document.documentMode < 8);

        var getRandomId = function(prefix) {
            return (typeof prefix !== 'undefined' ? prefix + '-' : '') + Math.floor(Math.random() * 1000000);
        };

        var getUniqueId = function(o, prefix) {
            var iterCount = 0,
                maxIterations = 1000,
                uniqueId = getRandomId(prefix);

            while (o.hasOwnProperty(uniqueId) && iterCount++ < maxIterations) {
                uniqueId = getRandomId(prefix);
            }
            return uniqueId;
        };

        var getLabelObject = function(label) {
            var labelObj = {};
            if (jq.trim(label).length == 0) {
                return labelObj;
            }
            var pairs = label.split('&');
            var regex = new RegExp("([^&=]*)=([^&=]*)");

            for (var i = 0, n = pairs.length; i < n; i++) {
                var pair = pairs[i];
                var parsedPair = regex.exec(pair);
                if (parsedPair != null) {
                    labelObj[parsedPair[1]] = parsedPair[2];
                } else {
                    labelObj[pair] = "";
                }
            }
            return labelObj;
        };

        var customEventIsSupported = function(eventType) {
            if (supportedCustomEvents.hasOwnProperty(eventType = eventType.toLowerCase())) {
                return supportedCustomEvents[eventType];
            }
            return false;
        };

        var execCustomEvent = function(eventType, thisArg, argsArray) {
            eventType = eventType.toLowerCase();
            thisArg = thisArg || window;
            argsArray = argsArray || [];

            if (!customEvents.hasOwnProperty(eventType)) {
                return undefined;
            }
            var customEvent = customEvents[eventType];

            for (var eventId in customEvent) {
                if (customEvent.hasOwnProperty(eventId)) {
                    customEvent[eventId].handler.apply(thisArg, argsArray);
                    if (customEvent[eventId].type & 1) {
                        delete customEvent[eventId];
                    }
                }
            }
        };

        var addCustomEvent = function(eventType, handler, params) {
            if (typeof eventType !== 'string' || typeof handler !== 'function') {
                return undefined;
            }

            eventType = eventType.toLowerCase();
            if (!customEventIsSupported(eventType)) {
                return undefined;
            }

            params = params || {};
            var isOnceExec = params.hasOwnProperty('once') ? params.once : false;

            var handlerType = 0;
            handlerType |= +isOnceExec * 1;

            if (!customEvents.hasOwnProperty(eventType)) {
                customEvents[eventType] = {};
            }

            var eventId = getUniqueId(customEvents[eventType]);
            customEvents[eventType][eventId] = { handler: handler, type: handlerType };
            return eventId;
        };

        var removeCustomEvent = function(eventType, eventId) {
            if (typeof eventType !== 'string' || !customEventIsSupported(eventType = eventType.toLowerCase()) || typeof eventId === 'undefined') {
                return false;
            }

            if (customEvents(eventType) && customEvents[eventType].hasOwnProperty(eventId)) {
                delete userEventHandlers[eventType][eventId];
            }
            return true;
        };

        var domReady = function() {
            // Mozilla, Opera and webkit nightlies currently support this event
            if (document.addEventListener) {
                document.addEventListener('DOMContentLoaded', function() {
                    document.removeEventListener('DOMContentLoaded', arguments.callee, false);
                    setTimeout(ASC.Controls.AnchorController.ready, 0);
                }, false);

                // If IE event model is used
            } else if (document.attachEvent) {
                // ensure firing before onload,
                // maybe late but safe also for iframes
                document.attachEvent('onreadystatechange', function() {
                    if (document.readyState === 'complete') {
                        document.detachEvent('onreadystatechange', arguments.callee);
                        setTimeout(ASC.Controls.AnchorController.ready, 0);
                    }
                });

                // If IE and not an iframe
                // continually check to see if the document is ready
                if (document.documentElement.doScroll && window == window.top) {
                    (function() {
                        try {
                            // If IE is used, use the trick by Diego Perini
                            // http://javascript.nwbox.com/IEContentLoaded/
                            document.documentElement.doScroll('left');
                        } catch(error) {
                            setTimeout(arguments.callee, 0);
                            return undefined;
                        }
                        // and execute any waiting functions
                        setTimeout(ASC.Controls.AnchorController.ready, 1000);
                    })();
                }
            }

            // A fallback to window.onload, that will always work
            if (window.addEventListener) {
                window.addEventListener('load', function() {
                    window.removeEventListener('load', arguments.callee, false);
                    setTimeout(ASC.Controls.AnchorController.ready, 0);
                }, false);
            } else if (window.attachEvent) {
                window.attachEvent('load', function() {
                    load.detachEvent('load', arguments.callee);
                    setTimeout(ASC.Controls.AnchorController.ready, 0);
                });
            }
        };

        var historyCheck = (function() {
            if (needHelper) {
                return function() {
                    var anchor = ieHelper.contentWindow.document.location.hash;
                    if (anchor.length === 0) {
                        anchor = '#';
                    }
                    if (anchor !== currentAnchor) {
                        currentAnchor = anchor;
                        var label = currentAnchor.substring(1);
                        location.hash = label;
                        execCustomEvent('onupdate', window, [label, getLabelObject(label)]);
                    }
                    anchor = location.hash;
                    if (anchor.length === 0) {
                        anchor = '#';
                    }
                    if (anchor !== currentAnchor) {
                        var iframe = ieHelper.contentWindow.document;
                        iframe.open();
                        iframe.close();
                        iframe.location.hash = anchor;
                        lastAnchor = currentAnchor;
                        currentAnchor = anchor;
                        var label = currentAnchor.substring(1);
                        execCustomEvent('onupdate', window, [label, getLabelObject(label)]);
                    }
                };
            }
            return function() {
                var anchor = location.hash;
                if (anchor.length === 0) {
                    anchor = '#';
                }
                if (anchor !== currentAnchor) {
                    lastAnchor = currentAnchor;
                    currentAnchor = anchor;
                    var label = currentAnchor.substring(1);
                    execCustomEvent('onupdate', window, [label, getLabelObject(label)]);
                }
            };
        })();

        var update = (function() {
            if (needHelper) {
                return function(anchor, safe) {
                    if (ieHelper === null) {
                        var anchor = location.hash;
                        if (anchor.length === 0) {
                            anchor = '#';
                        }
                        ieHelper = document.createElement('iframe');
                        ieHelper.style.width = '1px';
                        ieHelper.style.height = '1px';
                        ieHelper.style.lineHeight = '1px';
                        ieHelper.style.position = 'absolute';
                        ieHelper.style.left = '0';
                        ieHelper.style.top = '0';
                        ieHelper.style.zIndex = '-1';
                        ieHelper.style.visibility = 'hidden';
                        document.body.appendChild(ieHelper);
                        ieHelper.setAttribute('src', 'javascript:false;');
                        var iframe = ieHelper.contentWindow.document;
                        iframe.open();
                        iframe.close();
                        iframe.location.hash = anchor;
                        lastAnchor = currentAnchor;
                        currentAnchor = anchor;
                        var label = currentAnchor.substring(1);
                        if (safe !== true) {
                            execCustomEvent('onupdate', window, [label, getLabelObject(label)]);
                        }
                    }
                    if (typeof anchor === 'string' && anchor.charAt(0) !== '#') {
                        anchor = '#' + anchor;
                    }
                    if (typeof anchor !== 'string' || anchor.length === 0) {
                        anchor = '#';
                    }
                    if (currentAnchor === anchor) {
                        var label = currentAnchor.substring(1);
                        if (safe !== true) {
                            execCustomEvent('onupdate', window, [label, getLabelObject(label)]);
                        }
                        return undefined;
                    }
                    lastAnchor = currentAnchor;
                    var iframe = ieHelper.contentWindow.document;
                    iframe.open();
                    iframe.close();
                    iframe.location.hash = anchor;
                    var label = anchor.substring(1);
                    location.hash = anchor;
                    currentAnchor = location.hash;
                    if (safe !== true) {
                        execCustomEvent('onupdate', window, [label, getLabelObject(label)]);
                    }
                };
            }
            return function(anchor, safe) {
                if (typeof anchor === 'string' && anchor.charAt(0) !== '#') {
                    anchor = '#' + anchor;
                }
                if (typeof anchor !== 'string' || anchor.length === 0) {
                    anchor = '#';
                }
                if (currentAnchor === anchor) {
                    var label = currentAnchor.substring(1);
                    if (safe !== true) {
                        execCustomEvent('onupdate', window, [label, getLabelObject(label)]);
                    }
                    return undefined;
                }
                lastAnchor = currentAnchor;
                location.hash = anchor;
                currentAnchor = location.hash;
                var label = currentAnchor.substring(1);
                if (safe !== true) {
                    execCustomEvent('onupdate', window, [label, getLabelObject(label)]);
                }
            };
        })();

        var onUpdateAnchor = function(anchor) {
            for (var i = 0, n = markCollection.length; i < n; i++) {
                var params = [];
                if (markCollection[i].regexp === null) {
                    if (anchor.length === 0) {
                        markCollection[i].handler.apply(window, params);
                    }
                } else {
                    if ((params = markCollection[i].regexp.exec(anchor)) !== null) {
                        // TODO: place for do somthing with parameters string 
                        if (params.length > 1) {
                            params = params.slice(1);
                        } else {
                            params = [];
                        }
                        markCollection[i].handler.apply(window, params);
                    }
                }
            }
        };

        var init = function() {
            if (isInit === false) {
                isInit = true;
                if (needHelper === true && ieHelper === null) {
                    var anchor = location.hash;
                    if (anchor.length === 0) {
                        anchor = '#';
                    }
                    ieHelper = document.createElement('iframe');
                    ieHelper.style.width = '1px';
                    ieHelper.style.height = '1px';
                    ieHelper.style.lineHeight = '1px';
                    ieHelper.style.position = 'absolute';
                    ieHelper.style.left = '0';
                    ieHelper.style.top = '0';
                    ieHelper.style.zIndex = '-1';
                    ieHelper.style.visibility = 'hidden';
                    document.body.appendChild(ieHelper);
                    ieHelper.setAttribute('src', 'javascript:false;');
                    var iframe = ieHelper.contentWindow.document;
                    iframe.open();
                    iframe.close();
                    iframe.location.hash = anchor;
                    currentAnchor = anchor;
                    var label = currentAnchor.substring(1);
                    execCustomEvent('onupdate', window, [label, getLabelObject(label)]);
                } else {
                    var anchor = location.hash;
                    if (anchor.length === 0) {
                        anchor = '#';
                    }
                    currentAnchor = anchor;
                    var label = currentAnchor.substring(1);
                    execCustomEvent('onupdate', window, [label, getLabelObject(label)]);
                }
                setInterval(historyCheck, checkInterval);
            }
        };

        var move = function() {
            var hash = '';
            if (arguments.length === 0) {
                update('');
                return undefined;
            }
            for (i = 0, n = arguments.length - 1; i < n; i++) {
                hash += arguments[i] + cmdSeparator;
            }
            update(hash + arguments[i]);
        };

        var safemove = function() {
            var hash = '';
            if (arguments.length === 0) {
                update('', true);
                return undefined;
            }
            for (i = 0, n = arguments.length - 1; i < n; i++) {
                hash += arguments[i] + cmdSeparator;
            }
            update(hash + arguments[i], true);
        };

        var ready = function() {
            if (domIsReady === false) {
                domIsReady = true;
                execCustomEvent('ondomready', window, []);
            }
        };

        var bind = function(value, handler, params) {
            if (arguments.length === 0) {
                return false;
            }
            if (arguments.length === 1 && typeof value === 'function') {
                return addCustomEvent('onupdate', value);
            }
            if (arguments.length === 2 && typeof value === 'function' && typeof handler === 'object') {
                return addCustomEvent('onupdate', value, handler);
            }
            if (typeof value === 'string' && typeof handler === 'function') {
                return addCustomEvent(value, handler, params);
            }
            if (typeof value !== 'undefined' && typeof handler === 'function') {
                return markCollection.push({ regexp: value, handler: handler }) - 1;
            }
            return undefined;
        };

        var unbind = function(value, handlerId) {
            if (arguments.length === 0) {
                return false;
            }
            if (arguments.length === 1) {
                if (value >= 0 && value < markCollection.length) {
                    markCollection.splice(value, 1);
                }
                return removeCustomEvent('onupdate', value);
            }
            return removeCustomEvent(value, handlerId);
        };

        var trigger = function() {
            var label = location.hash.substring(1);
            execCustomEvent('onupdate', window, [label, getLabelObject(label)]);
        };

        var getAnchor = function() {
            if (isInit === true) {
                return currentAnchor.substring(1);
            } else {
                return firstAnchor.substring(1);
            }
        };

        var getLastAnchor = function() {
            return isInit === true ? lastAnchor.substring(1) : firstAnchor.substring(1);
        };

        firstAnchor = location.hash;
        firstAnchor = firstAnchor.charAt(0) === '#' ? firstAnchor : '#' + firstAnchor;
        addCustomEvent('onupdate', onUpdateAnchor);
        addCustomEvent('ondomready', init);
        domReady();

        return {
            init: init,
            ready: ready,
            move: move,
            safemove: safemove,
            bind: bind,
            unbind: unbind,
            trigger: trigger,
            getAnchor: getAnchor,
            getLastAnchor: getLastAnchor,
            historyCheck: historyCheck
        };
    })();
}