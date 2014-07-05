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


;window.ASC = window.ASC || {};

window.ASC.Controls = window.ASC.Controls || {};

window.ASC.Controls.AnchorController = (function () {
  var
    isInit = false,
    domIsReady = false,
    safeTransition = false,
    checkInterval = 200,
    firstAnchor = '',
    lastAnchor = '#',
    lazyAnchor = null,
    currentAnchor = '#',
    cmdSeparator = '/',
    markCollection = [],
    customEvents = {},
    anchorParams = null,
    supportedCustomEvents = {
      // update location.hash
      'onupdate' : true,
      'ondomready' : true
    },
    browser = (function () {
      var
        ua = navigator.userAgent,
        version = (ua.match(/.+(?:rv|it|ra|ie|ox|me|on)[\/:\s]([\d.]+)/i)||[0,'0'])[1];
      return {
        version : isFinite(+version) ? +version : version,
        // popular browsers
        msie : '\v' == 'v' || /msie/i.test(ua),
        opera : !!window.opera,
        chrome : /chrome/i.test(ua),
        safari : /safari/i.test(ua) && !/chrome/i.test(ua),
        firefox : /firefox/i.test(ua),
        mozilla : /mozilla/i.test(ua) && !/(compatible|webkit)/.test(ua),
        // engine
        gecko  : /gecko/i.test(ua),
        webkit  : /webkit/i.test(ua)
      };
    })(),
    ieHelper = null,
    needHelper = browser.msie && (browser.version < 8 || document.documentMode < 8);

  var getRandomId = function (prefix) {
    return (typeof prefix !== 'undefined' ? prefix + '-' : '') + Math.floor(Math.random() * 1000000);
  };

  var getUniqueId = function (o, prefix) {
    var
      iterCount = 0,
      maxIterations = 1000,
      uniqueId = getRandomId(prefix);

    while (o.hasOwnProperty(uniqueId) && iterCount++ < maxIterations) {
      uniqueId = getRandomId(prefix);
    }
    return uniqueId;
  };

  var createHelper = function () {
    if (!ieHelper) {
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
    }
    return ieHelper;
  };

  var customEventIsSupported = function (eventType) {
    if (supportedCustomEvents.hasOwnProperty(eventType = eventType.toLowerCase())) {
      return supportedCustomEvents[eventType];
    }
    return false;
  };

  var execCustomEvent = function (eventType, thisArg, argsArray) {
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

  var addCustomEvent = function (eventType, handler, params) {
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
    customEvents[eventType][eventId] = {handler : handler, type : handlerType};
    return eventId;
  };

  var removeCustomEvent = function (eventType, eventId) {
    if (typeof eventType !== 'string' || !customEventIsSupported(eventType = eventType.toLowerCase()) || typeof eventId === 'undefined') {
      return false;
    }

    if (customEvents(eventType) && customEvents[eventType].hasOwnProperty(eventId)) {
      delete userEventHandlers[eventType][eventId];
    }
    return true;
  };

  var domReady = function () {
    // Mozilla, Opera and webkit nightlies currently support this event
    if (document.addEventListener) {
      document.addEventListener('DOMContentLoaded', function () {
        document.removeEventListener('DOMContentLoaded', arguments.callee, false);
        setTimeout(ASC.Controls.AnchorController.ready, 0);
      }, false);

      // If IE event model is used
    } else if (document.attachEvent) {
      // ensure firing before onload,
      // maybe late but safe also for iframes
      document.attachEvent('onreadystatechange', function () {
        if (document.readyState === 'complete') {
          document.detachEvent('onreadystatechange', arguments.callee);
          setTimeout(ASC.Controls.AnchorController.ready, 0);
        }
      });

      // If IE and not an iframe
      // continually check to see if the document is ready
      if (document.documentElement.doScroll && window == window.top) {
        (function () {
          try {
            // If IE is used, use the trick by Diego Perini
            // http://javascript.nwbox.com/IEContentLoaded/
            document.documentElement.doScroll('left');
          } catch (error) {
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
      window.addEventListener('load', function () {window.removeEventListener('load', arguments.callee, false); setTimeout(ASC.Controls.AnchorController.ready, 0);}, false);
    } else if (window.attachEvent) {
      window.attachEvent('load', function () {load.detachEvent('load', arguments.callee); setTimeout(ASC.Controls.AnchorController.ready, 0);});
    }
  };

  var historyCheck = (function () {
    if (needHelper) {
      return function () {
        var anchor = ieHelper.contentWindow.document.location.hash;
        if (anchor.length === 0) {
          anchor = '#';
        }
        if (anchor !== currentAnchor) {
          currentAnchor = anchor;
          var label = currentAnchor.substring(1);
          location.hash = label;
          if (safeTransition === true) {
            safeTransition = false;
          } else {
            execCustomEvent('onupdate', window, [label]);
          }
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
          currentAnchor = anchor;
          var label = currentAnchor.substring(1);
          if (safeTransition === true) {
            safeTransition = false;
          } else {
            execCustomEvent('onupdate', window, [label]);
          }
        }
      };
    }
    return function () {
      var anchor = location.hash;
      if (anchor.length === 0) {
        anchor = '#';
      }
      if (anchor !== currentAnchor) {
        currentAnchor = anchor;
        var label = currentAnchor.substring(1);
        if (safeTransition === true) {
          safeTransition = false;
        } else {
          execCustomEvent('onupdate', window, [label]);
        }
      }
    };
  })();

  var update = (function () {
    /* if (needHelper) {
      return function (anchor, safe, unchange) {
        if (ieHelper === null) {
          var anchor = location.hash;
          if (anchor.length === 0) {
            anchor = '#';
          }
          createHelper();
          var iframe = ieHelper.contentWindow.document;
          iframe.open();
          iframe.close();
          iframe.location.hash = anchor;
          lastAnchor = currentAnchor;
          currentAnchor = anchor;
          var label = currentAnchor.substring(1);
          if (safe !== true) {
            execCustomEvent('onupdate', window, [label]);
          }
        }
        if (typeof anchor === 'string' && anchor.charAt(0) !== '#') {
          anchor = '#' + anchor;
        }
        if (typeof anchor !== 'string' || anchor.length === 0) {
          anchor = '#';
        }
        if (currentAnchor === anchor) {
          //var label = currentAnchor.substring(1);
          //if (safe !== true) {
          //  execCustomEvent('onupdate', window, [label]);
          //}
          return undefined;
        }
        lastAnchor = currentAnchor;
        currentAnchor = anchor;
        var iframe = ieHelper.contentWindow.document;
        iframe.open();
        iframe.close();
        iframe.location.hash = currentAnchor;
        var label = currentAnchor.substring(1);
        location.hash = label;
        if (safe !== true) {
          execCustomEvent('onupdate', window, [label]);
        }
      };
    } */
    return function (anchor, safe, unchange) {
      if (typeof anchor === 'string' && anchor.charAt(0) !== '#') {
        anchor = '#' + anchor;
      }
      if (typeof anchor !== 'string' || anchor.length === 0) {
        anchor = '#';
      }
      if (currentAnchor === anchor) {
        //var label = currentAnchor.substring(1);
        //if (safe !== true) {
        //  execCustomEvent('onupdate', window, [label]);
        //}
        return undefined;
      }
      var label = anchor.substring(1);
      if (unchange !== true) {
        lastAnchor = currentAnchor;
        currentAnchor = anchor;
        location.hash = label;
      }
      if (safe !== true) {
        execCustomEvent('onupdate', window, [label]);
      }
    };
  })();

  var onUpdateAnchor = function (anchor) {
      if (anchor.substring(0, 8) == "projects" && $(".smart-banner").length) {
          $(".smart-banner").show();
          $('html').animate({ marginTop: origHtmlMargin + 78 }, 300);
          $(".ui-content").height($(".ui-content").height() - 50)
      }
      else {
          $(".smart-banner").hide();
          $('html').css("marginTop", 0);
      }
    var params = [];
    anchorParams = anchorParams || {};
    for (var i = 0, n = markCollection.length; i < n; i++) {
      if (markCollection[i].regexp === null) {
        if (anchor.length === 0) {
          markCollection[i].handler.apply(window, [anchorParams]);
        }
        continue;
      }
      if ((params = markCollection[i].regexp.exec(anchor)) !== null) {
        // TODO: place for do somthing with parameters string
        params = params.length > 1 ? params.slice(1) : [];
        params.unshift(anchorParams);
        markCollection[i].handler.apply(window, params);
      }
    }
    anchorParams = null;
  };

  var init = function () {
    if (isInit === true) {
      return undefiend;
    }
    isInit = true;
    if (needHelper === true && ieHelper === null) {
      var anchor = location.hash;
      if (anchor.length === 0) {
        anchor = '#';
      }
      createHelper();
      var iframe = ieHelper.contentWindow.document;
      iframe.open();
      iframe.close();
      iframe.location.hash = anchor;
      currentAnchor = anchor;
      var label = currentAnchor.substring(1);
      execCustomEvent('onupdate', window, [label]);
    } else {
      var anchor = location.hash;
      if (anchor.length === 0) {
        anchor = '#';
      }
      currentAnchor = anchor;
      var label = currentAnchor.substring(1);
      execCustomEvent('onupdate', window, [label]);
    }
    setInterval(historyCheck, checkInterval);
  };

  var move = function () {
    var  hash = '';
    anchorParams = arguments[0] && typeof arguments[0] === 'object' ? arguments[0] : {};
    if (arguments.length === 0) {
      update('');
      return undefined;
    }

    for (var i = 0, n = arguments.length - 1; i < n; i++) {
      if (typeof arguments[i] === 'string') {
        hash += arguments[i] + cmdSeparator;
      }
    }
    update(hash + arguments[i]);
  };

  var lazymove = function () {
    var  hash = '';
    anchorParams = arguments[0] && typeof arguments[0] === 'object' ? arguments[0] : {};
    for (var i = 0, n = arguments.length - 1; i < n; i++) {
      if (typeof arguments[i] === 'string') {
        hash += arguments[i] + cmdSeparator;
      }
    }
    hash += arguments[i];
    hash = hash.charAt(0) === '#' ? hash : '#' + hash;

    if (arguments.length === 0) {
      if (lazyAnchor) {
        update(lazyAnchor, true);
      }
      lazyAnchor = null;
    } else if (arguments.length === 1 && arguments[0] === null) {
        lazyAnchor = null;
    } else {
      if (lazyAnchor !== hash) {
        lazyAnchor = hash;
        update(lazyAnchor, false, true);
      }
    }
  };

  var safemove = function () {
    var  hash = '';
    if (arguments.length === 0) {
      update('', true);
      return undefined;
    }
    for (var i = 0, n = arguments.length - 1; i < n; i++) {
      if (typeof arguments[i] === 'string') {
        hash += arguments[i] + cmdSeparator;
      }
    }
    update(hash + arguments[i], true);
  };

  var safeback = function () {
    if (history.length > 1) {
      safeTransition = true;
      history.go(-1);
      return true;
    }
    return false;
  };

  var ready = function () {
    if (domIsReady === false) {
      domIsReady = true;
      execCustomEvent('ondomready', window, []);
    }
  };

  var bind = function (value, handler, params) {
    if (arguments.length === 0) {
      return undefined;
    }
    if (arguments.length === 1 && typeof value === 'function') {
      return addCustomEvent('onupdate', value);
    }
    if (typeof value === 'string' && typeof handler === 'function') {
      return addCustomEvent(value, handler, params);
    }
    if (typeof value !== 'undefined' && typeof handler === 'function') {
      return markCollection.push({regexp  : value, handler : handler}) - 1;
    }
    return undefined;
  };

  var unbind = function (value, handlerId) {
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

  var getAnchor = function () {
    return isInit === true ? currentAnchor.substring(1) : firstAnchor.substring(1);
  };

  var getLastAnchor = function () {
    return isInit === true ? lastAnchor.substring(1) : firstAnchor.substring(1);
  };

  var testAnchor = function (re, onlycurrent) {
    return re ? re.test(isInit === true ? (onlycurrent === true || !lazyAnchor ? currentAnchor.substring(1) : lazyAnchor.substring(1)) : firstAnchor.substring(1)) : false;
  };

  firstAnchor = location.hash;
  firstAnchor = firstAnchor.charAt(0) === '#' ? firstAnchor : '#' + firstAnchor;
  addCustomEvent('onupdate', onUpdateAnchor);
  // addCustomEvent('ondomready', init);
  // addCustomEvent('ondomready', function () {setTimeout(ASC.Controls.AnchorController.init, 500)});
  domReady();

  return {
    init          : init,
    ready         : ready,
    move          : move,
    safemove      : safemove,
    safeback      : safeback,
    lazymove      : lazymove,
    bind          : bind,
    unbind        : unbind,
    getAnchor     : getAnchor,
    getLastAnchor : getLastAnchor,
    testAnchor    : testAnchor
  };
})();
