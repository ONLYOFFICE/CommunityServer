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


String.prototype.format = function () {
  if (arguments.length === 0) {
    return '';
  }

  var pos = -1, str = this, cnd = '', ind = -1, cnds = str.match(/{(\d+)}/g), cndsInd = cnds ? cnds.length : 0;
  while (cndsInd--) {
    pos = -1;
    cnd = cnds[cndsInd];
    ind = cnd.replace(/[{}]+/g, '');
    while ((pos = str.indexOf(cnd, pos + 1)) !== -1) {
      str = str.substring(0, pos) + (arguments[+ind] || '') + str.substring(pos + cnd.length);
    }
  }
  return str;
};

window.ASC = window.ASC || {};

window.ASC.TMTalk = window.ASC.TMTalk || {};

window.ASC.TMTalk.flashPlayer = (function () {
  var
    description = null,
    SHOCKWAVE_FLASH = 'Shockwave Flash',
    FLASH_MIME_TYPE = 'application/x-shockwave-flash',
    SHOCKWAVE_FLASH_AX = 'ShockwaveFlash.ShockwaveFlash',
    requestedVersion = '9.0.0',
    playerVersion = [0, 0, 0];

  var hasPlayerVersion = function (reqVersion) {
	  var rv = reqVersion.split('.');
	  rv = [
	    parseInt(rv[0], 10),
	    parseInt(rv[1], 10) || 0,
	    parseInt(rv[2], 10) || 0
	  ];
	  return (playerVersion[0] > rv[0] || (playerVersion[0] == rv[0] && playerVersion[1] > v[1]) || (playerVersion[0] == rv[0] && playerVersion[1] == rv[1] && playerVersion[2] >= rv[2])) ? true : false;
  }

  if (typeof navigator.plugins !== 'undefined' && typeof navigator.plugins[SHOCKWAVE_FLASH] === 'object') {
		var description = navigator.plugins[SHOCKWAVE_FLASH].description;
		if (description && !(typeof navigator.mimeTypes !== 'undefined' && navigator.mimeTypes[FLASH_MIME_TYPE] && !navigator.mimeTypes[FLASH_MIME_TYPE].enabledPlugin)) {
			description = description.replace(/^.*\s+(\S+\s+\S+$)/, '$1');
			playerVersion = [
			  parseInt(description.replace(/^(.*)\..*$/, '$1'), 10),
			  parseInt(description.replace(/^.*\.(.*)\s.*$/, '$1'), 10),
			  /[a-zA-Z]/.test(description) ? parseInt(description.replace(/^.*[a-zA-Z]+(.*)$/, '$1'), 10) : 0
			];
		}
	}
	else if (typeof window.ActiveXObject !== 'undefined') {
		try {
			var ax = new ActiveXObject(SHOCKWAVE_FLASH_AX);
			if (ax) {
				var description = ax.GetVariable('$version');
				if (description) {
					description = description.split(' ')[1].split(',');
					playerVersion = [
					  parseInt(description[0], 10),
					  parseInt(description[1], 10),
					  parseInt(description[2], 10)
					];
				}
			}
		} catch(err) {
		}
	}

  return {
    version     : playerVersion,
    hasVersion  : hasPlayerVersion,
    isInstalled : playerVersion[0] !== 0,
    isCorrect   : hasPlayerVersion(requestedVersion)
  };
})();

window.ASC.TMTalk.sounds = (function () {
  var
    isEnabled = null,
    soundsContainer = null;

    var initHtml = function() {
        var o = document.createElement('div'),
            soundsContainerId = 'talkHtmlSoundsContainer';
        o.setAttribute('id', soundsContainerId);
        
        for (var i = 0; i < ASC.TMTalk.Config.soundsHtml.length; i++) {
            var audioFindName = ASC.TMTalk.Config.soundsHtml[i].match(/\/\w+\./g);
            if (audioFindName.length > 0) {
                var audio = document.createElement('audio');
                var audioName = audioFindName[audioFindName.length - 1].substring(1, audioFindName[audioFindName.length - 1].length - 1);
                
                audio.setAttribute('id', audioName);
                audio.setAttribute('src', ASC.TMTalk.Config.soundsHtml[i]);
                o.appendChild(audio);
            }
        }
        document.getElementById('talkWrapper').appendChild(o);
    };
    var init = function (o) {
    if (typeof o === 'string') {
      o = document.getElementById(o);
    }
    if (!o || typeof o === 'undefined') {
      return null;
    }
    soundsContainer = o;
    if (isEnabled === null) {
      isEnabled = true;
    }
  };

    var play = function (soundname) {
    if (isEnabled === true && typeof soundname === 'string') {
        var sound = document.getElementById(soundname + 'sound');
        if (sound != null) {
            try {
                sound.play().catch(function (e) {
                    console.log(e.message);
                });
            } catch(e) {}
        }
        //try { soundsContainer.playSound(soundname) } catch (err) { }
    }
  };

  var enable = function () {
      isEnabled = true;
  };

  var disable = function () {
    isEnabled = false;
  };

  var supported = function () {
      return true;
      //return ASC.TMTalk.flashPlayer.isCorrect;
  };

  return {
    supported : supported,

    init  : init,
    play  : play,

    initHtml: initHtml,
    
    enable  : enable,
    disable : disable
  };
})();

window.ASC.TMTalk.indicator = (function () {
  var
    indicators = {},
    timeoutValue = 500,
    timeoutHandler = null,
    customEvents = {
      show : 'onshow',
      start : 'onstart',
      stop : 'onstop'
    },
    eventManager = new CustomEvent(customEvents);

  var bind = function (eventName, handler, params) {
    return eventManager.bind(eventName, handler, params);
  };

  var unbind = function (handlerId) {
    return eventManager.unbind(handlerId);
  };

  var show = function () {
    eventManager.call(customEvents.show, window, []);
  };

  var start = function (id) {
    if (indicators.hasOwnProperty(id)) {
      return undefined;
    }
    indicators[id] = true;

    eventManager.call(customEvents.start, window, []);

    if (timeoutHandler === null) {
      timeoutHandler = setInterval(show, timeoutValue);
    }
  };

  var stop = function (id) {
    if (!indicators.hasOwnProperty(id)) {
      return undefined;
    }

    delete indicators[id];
    for(var id in indicators) {
      if (indicators.hasOwnProperty(id)) {
        return undefined;
      }
    }
    clearInterval(timeoutHandler);
    timeoutHandler = null;

    eventManager.call(customEvents.stop, window, []);
  };

  return {
    events  : customEvents,

    bind    : bind,
    unbind  : unbind,

    start : start,
    stop  : stop
  };
})();

window.ASC.TMTalk.properties = (function () {
  var
    isInit = false,
    cookieName = 'tmtalk',
    fieldSeparator = ':',
    propertySeparator = '::',
    storage = {},
    properties = {},
    customEvents = {
      load : 'onload',
      updateProperty : 'onupdateproperty'
    },
    eventManager = new CustomEvent(customEvents);

  var init = function (ver) {
    if (isInit === true) {
      return undefined;
    }
    // TODO
    load();

    if (typeof ver === 'string') {
      if (!properties.hasOwnProperty('ver') || properties.ver !== ver) {
        for (var fld in properties) {
          if (properties.hasOwnProperty(fld)) {
            delete properties[fld];
          }
          if (storage.hasOwnProperty(fld)) {
            delete storage[fld];
          }
        }
        ASC.TMTalk.cookies.remove(cookieName);
        storage['ver'] = ver;
        properties['ver'] = ver;
        ASC.TMTalk.cookies.set(cookieName, 'ver' + fieldSeparator + ver);
      }
    }
  };

  var bind = function (eventName, handler, params) {
    return eventManager.bind(eventName, handler, params);
  };

  var unbind = function (handlerId) {
    return eventManager.unbind(handlerId);
  };

  var item = function (name, value, inStorage) {
    if (typeof name !== 'string') {
      return '';
    }
    if (typeof value === 'undefined') {
      return properties.hasOwnProperty(name) ? properties[name] : undefined;
    }
    var oldvalue = properties[name] || '';
    properties[name] = value;
    if (inStorage === true) {
        if (jQuery.isEmptyObject(storage) && ASC.TMTalk.cookies.get(cookieName)) {
            var cookieProperty = ASC.TMTalk.cookies.get(cookieName).split(propertySeparator);
            for (var i = 0; i < cookieProperty.length; i++) {
                var field = cookieProperty[i].split(fieldSeparator);
                storage[field[0]] = field[1];
            }
        }
      storage[name] = value;
      save();
    }
    eventManager.call(customEvents.updateProperty, window, [name, value, oldvalue]);
  };

  var save = function () {
    var collect = [];
    for (var name in storage) {
      if (storage.hasOwnProperty(name)) {
        collect.push(name + fieldSeparator + storage[name]);
      }
    }
    ASC.TMTalk.cookies.set(cookieName, collect.join(propertySeparator));
  };

  var load = function () {
    var
      pos = -1,
      name = '',
      value = '',
      collect = ASC.TMTalk.cookies.get(cookieName).split(propertySeparator);

    for (var i = 0, n = collect.length; i < n; i++) {
      if ((pos = collect[i].indexOf(fieldSeparator)) === -1) {
        continue;
      }
      name = collect[i].substring(0, pos);
      value = collect[i].substring(pos + 1);
      if (!name.length) {
        continue;
      }
      storage[name] = value;
      properties[name] = value;
    }
  };

  return {
    events  : customEvents,

    init  : init,
    item  : item,

    bind    : bind,
    unbind  : unbind
  };
})();

window.ASC.TMTalk.events = (function () {
  var add = (function () {
    if (typeof window.addEventListener === 'function') {
      return function (o, event, handler) {
        if (typeof o === 'string') {
          o = document.getElementById(o);
        }
        if (!o || typeof o !== 'object') {
          return undefined;
        }
        o.addEventListener(event, handler, false);
        return handler;
      };
    } else if (typeof window.attachEvent === 'object') {
      return function (o, event, handler) {
        if (typeof o === 'string') {
          o = document.getElementById(o);
        }
        if (!o || typeof o !== 'object') {
          return undefined;
        }
        o.attachEvent('on' + event, handler);
        return handler;
      };
    } else {
      return function (o, event, handler) {
        if (typeof o === 'string') {
          o = document.getElementById(o);
        }
        if (!o || typeof o !== 'object') {
          return undefined;
        }
        if (o.hasOwnProperty('on' + event)) {
          o['on' + event] = handler;
        }
        return handler;
      };
    }
  })();

  var remove = (function () {
    if (typeof window.removeEventListener === 'function') {
      return function (o, event, handler) {
        if (typeof o === 'string') {
          o = document.getElementById(o);
        }
        if (o && typeof o === 'object') {
          o.removeEventListener(event, handler, false);
        }
      };
    } else if (typeof window.detachEvent === 'object') {
      return function (o, event, handler) {
        if (typeof o === 'string') {
          o = document.getElementById(o);
        }
        if (o && typeof o === 'object') {
          o.detachEvent('on' + event, handler);
        }
      };
    }
    return function (o, event, handler) {
      if (typeof o === 'string') {
        o = document.getElementById(o);
      }
      if (o && typeof o === 'object') {
        if (o.hasOwnProperty('on' + event)) {
          o['on' + event] = null;
        }
      }
    };
  })();

  var fix = (function () {
    if (typeof document.documentElement === 'object') {
      return function (evt) {
        evt = evt || window.event;
        if (!evt.pageX) {
          try {
            evt.pageX = evt.clientX + document.documentElement.scrollLeft;
            evt.pageY = evt.clientY + document.documentElement.scrollTop;
          } catch (err) {
          }
        }
        if (!evt.which && evt.button) {
          evt.which = evt.button & 1 ? 1 : (evt.button & 2 ? 3 : (evt.button & 4 ? 2 : 0));
        }
        return evt;
      };
    }
    return function (evt) {
      evt = evt || window.event;
      if (!evt.pageX) {
        evt.pageX = evt.clientX + document.body.scrollLeft;
        evt.pageY = evt.clientY + document.body.scrollTop;
      }
      if (!evt.which && evt.button) {
        evt.which = evt.button & 1 ? 1 : (evt.button & 2 ? 3 : (evt.button & 4 ? 2 : 0));
      }
      return evt;
    };
  })();

  return {
    fix     : fix,
    add     : add,
    remove  : remove
  };
})();

window.ASC.TMTalk.style = (function () {
  var get = (function () {
    if (typeof document.defaultView !== 'undefined' && typeof document.defaultView.getComputedStyle !== 'undefined') {
      return function (o, property) {
        if (typeof o === 'string'){
	        o = document.getElementById(o);
	      }
        if (!o) {
          return null;
        }
        if (typeof property !== 'string' || !property) {
          return o;
        }
        return document.defaultView.getComputedStyle(o, '').getPropertyValue(property);
      };
    }
    return function (o, property) {
      if (typeof o === 'string'){
	      o = document.getElementById(o);
	    }
      if (!o) {
        return null;
      }
      if (typeof property !== 'string' || !property) {
        return o;
      }
      return o.currentStyle !== 'undefined' ? o.currentStyle[property] : '';
    };
  })();

  return {
    get : get
  };
})();

window.ASC.TMTalk.dragMaster = (function () {
  var
    cursorStartPosition = null,
    selectedElement = null,
    dragableElement = null,
    cursorOffset = null,
    cursorMaxShift = 20,
    targets = [],
    onDrop = null,
    onDrag = null,
    onChose = null,
    onMouseOver = null,
    onMouseOut = null;

  var cursorIsOver = function (cursor, block) {
    return cursor.x > block.left && cursor.x < block.right && cursor.y > block.top && cursor.y < block.bottom;
  };

  var getBoundRect = function (o) {
    var
      wasHidden = false,
      oldDisplayValue = '',
      toShow = false,
      rect = null,
      maxAvailableValue = 40,
      shift = maxAvailableValue >> 1,
      offsetBlock = null;

    wasHidden = ASC.TMTalk.style.get(o, 'display') === 'none';
    if (wasHidden) {
      oldDisplayValue = o.style.display;
      o.style.display = 'block';
    }
    offsetBlock = ASC.TMTalk.dom.offset(o);
    rect = {
      left : offsetBlock.x - (o.offsetWidth > maxAvailableValue ? 0 : shift),
      top : offsetBlock.y - (o.offsetHeight > maxAvailableValue ? 0 : shift),
      right : offsetBlock.x + (o.offsetWidth > maxAvailableValue ? o.offsetWidth : shift),
      bottom : offsetBlock.y + (o.offsetHeight > maxAvailableValue ? o.offsetHeight : shift)
    };
    if (wasHidden) {
      o.style.display = oldDisplayValue ? oldDisplayValue : '';
    }
    return rect;
  };

  var execEvent = function (eventType, thisArg, argsArray) {
    thisArg = thisArg || window;
    argsArray = argsArray || [];
    if (window.getSelection) {
        window.getSelection().removeAllRanges();
    } else {
        document.selection.empty();
    }
    switch (eventType.toLowerCase()) {
      case 'ondrag' :
        if (typeof onDrop === 'function') {
          onDrag.apply(thisArg, argsArray);
        }
        break;
      case 'ondrop' :
        if (typeof onDrop === 'function') {
          onDrop.apply(thisArg, argsArray);
        }
        break;
      case 'onchose' :
        if (typeof onChose === 'function') {
          onChose.apply(thisArg, argsArray);
        }
        break;
      case 'onmouseover' :
        if (typeof onMouseOver === 'function') {
          onMouseOver.apply(thisArg, argsArray);
        }
        break;
      case 'onmouseout' :
        if (typeof onMouseOut === 'function') {
          onMouseOut.apply(thisArg, argsArray);
        }
        break;
    }
  };

  var onDragStart = function () {
    return false;
  };

  var onSelectStart = function () {
    return false;
  };

  var onMouseMoveFix = function (evt) {
    evt = ASC.TMTalk.events.fix(evt);

    if (Math.abs(cursorStartPosition.x - evt.pageX) > cursorMaxShift || Math.abs(cursorStartPosition.y - evt.pageY) > cursorMaxShift) {
      ASC.TMTalk.events.remove(document, 'mousemove', onMouseMoveFix);
      ASC.TMTalk.events.remove(document, 'mouseup', onMouseUpFix);
      ASC.TMTalk.events.add(document, 'mousemove', onMouseMove);
      ASC.TMTalk.events.add(document, 'mouseup', onMouseUp);

      ASC.TMTalk.dom.addClass(selectedElement, 'selected');

      dragableElement.style.display = 'block';
      dragableElement.style.left = evt.pageX - cursorOffset.x + 'px';
      dragableElement.style.top = evt.pageY - cursorOffset.y + 'px';

      execEvent('ondrag', dragableElement, [selectedElement]);
    }
  };

  var onMouseMove = function (evt) {
    evt = ASC.TMTalk.events.fix(evt);

    dragableElement.style.left = evt.pageX - cursorOffset.x + 'px';
    dragableElement.style.top = evt.pageY - cursorOffset.y + 'px';

    var target = null;
    for (var i = 0, n = targets.length; i < n; i++) {
      target = targets[i];
      if (cursorIsOver({x : evt.pageX, y : evt.pageY}, getBoundRect(target.element))) {
        if (!target.mouseover) {
          target.mouseover = true;
          ASC.TMTalk.dom.addClass(target.element, 'mouseover');
          execEvent('onmouseover', target.element);
        }
      } else {
        if (target.mouseover) {
          target.mouseover = false;
          ASC.TMTalk.dom.removeClass(target.element, 'mouseover');
          execEvent('onmouseout', target.element);
        }
      }
    }
  };

  var onMouseUpFix = function (evt) {
    ASC.TMTalk.events.remove(document.body, 'selectstart', onSelectStart);
    ASC.TMTalk.events.remove(document, 'dragstart', onDragStart);
    ASC.TMTalk.events.remove(document, 'mousemove', onMouseMoveFix);
    ASC.TMTalk.events.remove(document, 'mouseup', onMouseUpFix);
  };

  var onMouseUp = function (evt) {
    evt = ASC.TMTalk.events.fix(evt);

    dragableElement.style.display = 'none';

    ASC.TMTalk.events.remove(document.body, 'selectstart', onSelectStart);
    ASC.TMTalk.events.remove(document, 'dragstart', onDragStart);
    ASC.TMTalk.events.remove(document, 'mousemove', onMouseMove);
    ASC.TMTalk.events.remove(document, 'mouseup', onMouseUp);

    ASC.TMTalk.dom.removeClass(selectedElement, 'selected');

    var
      selectedTargets = [],
      targetsInd = targets.length;
    while (targetsInd--) {
      if (ASC.TMTalk.dom.hasClass(targets[targetsInd].element, 'mouseover')) {
        selectedTargets.push(targets[targetsInd].element);
        ASC.TMTalk.dom.removeClass(targets[targetsInd].element, 'mouseover');
      }
    }

    execEvent('ondrop', dragableElement, [selectedElement]);
    execEvent('onchose', dragableElement, [selectedElement, selectedTargets]);
  };

  var create = function () {
    if (dragableElement === null) {
      var node = null, background = null;
      dragableElement = document.createElement('div');
      dragableElement.style.display = 'none';
      node = document.createElement('div');
      node.className = 'state';
      dragableElement.appendChild(node);
      node = document.createElement('div');
      node.className = 'title';
      dragableElement.appendChild(node);
      background = document.createElement('div');
      background.className = 'background';
      node = document.createElement('div');
      node.className = 'helper';
      background.appendChild(node);
      node = document.createElement('div');
      node.className = 'left-side';
      background.appendChild(node);
      node = document.createElement('div');
      node.className = 'right-side';
      background.appendChild(node);
      dragableElement.appendChild(background);
      document.body.appendChild(dragableElement);
    }
  };

  var start = function (evt, el, newtargets, properties, handler) {
    if (typeof el === 'string') {
      el = document.getElementById(el);
    }
    if (!el || typeof el !== 'object') {
      return undefined;
    }

    targets = [];
    if (newtargets instanceof Array) {
      var
        newtarget = null,
        newtargetsInd = newtargets.length;
      while (newtargetsInd--) {
        newtarget = newtargets[newtargetsInd];
        if (typeof newtarget === 'string') {
          newtarget = document.getElementById(newtarget);
        }
        if (!newtarget || typeof newtarget !== 'object') {
          continue;
        }
        targets.push({
          element   : newtarget,
          boundrect : getBoundRect(newtarget),
          mouseover : false
        });
      }
    }

    if (properties && typeof properties == 'object') {
      onDrag = typeof properties.onDrag === 'function' ? properties.onDrag : null;
      onDrop = typeof properties.onDrop === 'function' ? properties.onDrop : null;
      onChose = typeof properties.onChose === 'function' ? properties.onChose : null;
      onMouseOver = typeof properties.onMouseOver === 'function' ? properties.onMouseOver : null;
      onMouseOut = typeof properties.onMouseOut === 'function' ? properties.onMouseOut : null;
    }

    create();
    selectedElement = el;
    dragableElement.style.position = 'absolute';
    dragableElement.style.overflow = 'hidden';
    dragableElement.style.zIndex = '9001';

    if (typeof handler === 'function') {
      handler.apply(dragableElement, [selectedElement]);
    }

    cursorStartPosition = {
      x : evt.pageX,
      y : evt.pageY
    };

    cursorOffset = ASC.TMTalk.dom.offset(selectedElement);
    cursorOffset.x = evt.pageX - cursorOffset.x;
    cursorOffset.y = evt.pageY - cursorOffset.y;

    ASC.TMTalk.events.add(document.body, 'selectstart', onSelectStart);
    ASC.TMTalk.events.add(document, 'dragstart', onDragStart);
    ASC.TMTalk.events.add(document, 'mousemove', onMouseMoveFix);
    ASC.TMTalk.events.add(document, 'mouseup', onMouseUpFix);
  };

  return {
    start   : start
  };
})();

window.ASC.TMTalk.cookies = (function () {
  var get = function (name) {
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
		  if ((end = cookie.indexOf(";", start)) === -1) {
  			end = cookie.length;
	  	}
		  value = cookie.substring(start, end);
	  }
    return decodeURI(value);
  };

  var set = function (name, value, expires, path, domain, secure) {
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
    path    = path    ? ';path='    + path    : ';path=/';
    domain  = domain  ? ';domain='  + domain  : '';
    secure  = secure  ? ';secure='  + secure  : '';
    document.cookie = name + '=' + encodeURI(value) + expires + path + domain + secure;
    return value;
  };

  var remove = function (name, path, domain) {
    if (name && document.cookie.indexOf(name + '=')  != -1) {
      path    = path    ? ';path='    + path    : ';path=/';
      domain  = domain  ? ';domain='  + domain  : '';
      document.cookie = name + '=' + path + domain + ';expires=Thu, 01-Jan-1970 00:00:01 GMT';
    }
    return true;
  };

  return {
    get     : get,
    set     : set,
    remove  : remove
  };
})();

window.ASC.TMTalk.dom = (function () {
  var
    browser = (function () {
      var
        ua = navigator.userAgent,
        version = (ua.match(/.+(?:rv|it|ra|ie|ox|me|on)[\/:\s]([\d.]+)/i)||[0,'0'])[1];
      return {
        version : isFinite(+version) ? +version : version,
        // browsers
        msie    : '\v' == 'v' || /msie/i.test(ua),
        opera   : window.opera ? true : false,
        chrome  : window.chrome ? true : false,
        safari  : /safari/i.test(ua) && !window.chrome,
        firefox : /firefox/i.test(ua),
        mozilla : /mozilla/i.test(ua) && !/(compatible|webkit)/.test(ua),
        // engine
        gecko   : /gecko/i.test(ua),
        webkit  : /webkit/i.test(ua)
      }
    })();

  var inArray = function (elem, array) {
	  for (var i = 0, n = array.length; i < n; i++) {
		  if (array[i] === elem) {
			  return i;
			}
    }
	  return -1;
  };

  var hasClass = function (o, className) {
    if (typeof o === 'string'){
	    o = document.getElementById(o);
	  }
    if (!o) {
      return false;
    }
    if (typeof className !== 'string' || !className) {
      return false;
    }

    if (o.nodeType === 1) {
      var currentClassName = o.className;
      return currentClassName && currentClassName.indexOf(className) !== -1 && inArray(className, currentClassName.split(/\s+/)) !== -1;
    }
    return false;
  };

  var addClass = function (o, className) {
    if (typeof o === 'string'){
	    o = document.getElementById(o);
	  }
    if (!o) {
      return null;
    }
    if (typeof className !== 'string' || !className) {
      return o;
    }
    
    if (o.nodeType === 1) {
      var currentClassName = o.className;
      if (!currentClassName) {
        o.className = className;
      } else {
        if (inArray(className, currentClassName.split(/\s+/)) === -1) {
          o.className += ' ' + className;
        }
      }
    }
    return o;
  };

  var removeClass = function (o, className) {
    if (typeof o === 'string'){
	    o = document.getElementById(o);
	  }
    if (!o) {
      return null;
    }
    if (typeof className !== 'string' || !className) {
      return o;
    }

    if (o.nodeType === 1) {
      var currentClassName = o.className;
      if (currentClassName && currentClassName.indexOf(className) !== -1) {
        var
          classPos = -1,
          classes = currentClassName.split(/\s+/);
        if ((classPos = inArray(className, classes)) !== -1) {
          classes.splice(classPos, 1);
          o.className = classes.join(' ');
        }
      }
    }
    return o;
  };

  var toggleClass = function (o, className) {
    if (typeof o === 'string'){
	    o = document.getElementById(o);
	  }
    if (!o) {
      return null;
    }
    if (typeof className !== 'string' || !className) {
      return o;
    }

    if (o.nodeType === 1) {
      var currentClassName = o.className;
      if (currentClassName && currentClassName.indexOf(className) !== -1) {
        var
          classPos = -1,
          classes = currentClassName.split(/\s+/);
        if ((classPos = inArray(className, classes)) !== -1) {
          classes.splice(classPos, 1);
          o.className = classes.join(' ');
        } else {
          o.className = currentClassName === ''? className : currentClassName + ' ' + className;
        }
      } else {
        o.className = currentClassName === ''? className : currentClassName + ' ' + className;
      }
    }
    return o;
  };

  var getElementsByClassName = (function () {
    if (typeof document.getElementsByClassName === 'function') {
      return function (o, className, tagName) {
        if (typeof o === 'string'){
	        o = document.getElementById(o);
	      }
        if (!o) {
          return [];
        }
        if (typeof tagName !== 'string') {
          return o.getElementsByClassName(className);
        }
			  var
          current = null,
          returnElements = [],
          elements = o.getElementsByClassName(className),
          nodeName = new RegExp('\\b' + tagName + '\\b', 'i');
        for (var i = 0, n = elements.length; i < n; i++) {
          current = elements[i];
          if(nodeName.test(current.nodeName)) {
            returnElements.push(current);
          }
        }
        return returnElements;
      };
    } else if (document.evaluate) {
      return function (o, className, tagName) {
        if (typeof o === 'string'){
	        o = document.getElementById(o);
	      }
        if (!o) {
          return [];
        }
        tagName = tagName || '*';
        var
          classes = className.split(' '),
          classesToCheck = '',
          xhtmlNamespace = 'http://www.w3.org/1999/xhtml',
          namespaceResolver = document.documentElement.namespaceURI === xhtmlNamespace ? xhtmlNamespace : null,
          returnElements = [],
          elements = null,
          node = null;
        for(var i = 0, n = classes.length; i < n; i++) {
          classesToCheck += '[contains(concat(" ", @class, " "), " ' + classes[i] + ' ")]';
        }
        try	{
          elements = document.evaluate(".//" + tagName + classesToCheck, o, namespaceResolver, 0, null);
        }
        catch (err) {
          elements = document.evaluate(".//" + tagName + classesToCheck, o, null, 0, null);
        }
        while ((node = elements.iterateNext())) {
          returnElements.push(node);
        }
        return returnElements;
      };
    }
    return function (o, className, tagName) {
      if (typeof o === 'string'){
	      o = document.getElementById(o);
	    }
      if (!o) {
        return [];
      }
      tagName = tagName || '*';
      var
        i = 0, j = 0, n = 0, m = 0,
        classes = className.split(' '),
        classesToCheck = [],
        elements = tagName === '*' && o.all ? o.all : o.getElementsByTagName(tagName),
        current = null,
        returnElements = [],
        match = false;
      for (i = 0, n = classes.length; i < n; i++) {
        classesToCheck.push(new RegExp('(^|\\s)' + classes[i] + '(\\s|$)'));
      }
      for (i = 0, n = elements.length; i < n; i++) {
        current = elements[i];
        match = false;
        for(j = 0, m = classesToCheck.length; j < m; j++){
					match = classesToCheck[j].test(current.className);
					if (!match) {
						break;
					}
				}
				if (match) {
					returnElements.push(current);
				}
			}
			return returnElements; 
    };
  })();

  var getContent = (function () {
    if (typeof document.createElement('div').textContent === 'string' && !browser.msie) {
      return function (o) {
        if (typeof o === 'string'){
	        o = document.getElementById(o);
	      }
        if (!o) {
          return '';
        }
        return o.textContent;
      };
    } else if (typeof document.createElement('div').text === 'string') {
      return function (o) {
        if (typeof o === 'string'){
	        o = document.getElementById(o);
	      }
        if (!o) {
          return '';
        }
        return o.text;
      };
    }
    return function (o) {
      if (typeof o === 'string'){
	      o = document.getElementById(o);
	    }
      if (!o) {
        return '';
      }
      result = '';
      var childs = o.childNodes;
      if (!childs) {
        return result;
      }
      for (var i = 0, n = childs.length; i < n; i++) {
        var child = childs.item(i);
        switch (child.nodeType) {
          case 1 :
          case 5 :
            result += arguments.callee(child);
            break;
          case 3 :
          case 2 :
          case 4 :
            result += child.nodeValue;
            break;
        }
      }
      return result;
    };
  })();

  var clear = function (o) {
    if (typeof o === 'string'){
	    o = document.getElementById(o);
	  }
    if (!o) {
      return null;
    }
	  while (o.firstChild) {
	    o.removeChild(o.firstChild);
	  }
	  return o;
  };

  var offset = (function () {
    if (typeof document.createElement('div').getBoundingClientRect !== 'undefined') {
      if (typeof document.documentElement === 'object') {
        return function (o) {
          if (typeof o === 'string'){
	          o = document.getElementById(o);
	        }
          if (!o) {
            return {
              x : 0,
              y : 0
            };
          }
          var box = o.getBoundingClientRect();
          return {
            x : Math.round(box.left + document.documentElement.scrollLeft - (document.documentElement.clientLeft || 0)),
            y : Math.round(box.top +  document.documentElement.scrollTop - (document.documentElement.clientTop || 0))
          };
        };
      }
      return function (o) {
        if (typeof o === 'string'){
	        o = document.getElementById(o);
	      }
        if (!o) {
          return {
            x : 0,
            y : 0
          };
        }
        var box = o.getBoundingClientRect();
        return {
          x : Math.round(box.left + document.body.scrollLeft - (document.body.clientLeft || 0)),
          y : Math.round(box.top +  document.body.scrollTop - (document.body.clientTop || 0))
        };
      };
    }
    return function (o) {
      if (typeof o === 'string'){
	      o = document.getElementById(o);
	    }
      if (!o) {
        return {
          x : 0,
          y : 0
        };
      }
      var  x = 0, y = 0;
      while(o) {
        y = y + parseInt(o.offsetTop, 10);
        x = x + parseInt(o.offsetLeft, 10);
        o = o.offsetParent;
      }
      return {
        x : x,
        y : y
      };
    };
  })();

  var maxScrollLeft = function (o) {
    if (typeof o === 'string'){
	    o = document.getElementById(o);
	  }
    if (!o) {
      return 0;
    }
    var origScrollLeft = o.scrollLeft, iter = 1000, scrollLeft = origScrollLeft;
    o.scrollLeft = scrollLeft += iter;
    while (o.scrollLeft === scrollLeft) {
      o.scrollLeft = scrollLeft += iter;
    }
    scrollLeft = o.scrollLeft;
    o.scrollLeft = origScrollLeft;
    return scrollLeft;
  };

  var maxScrollTop = function (o) {
    if (typeof o === 'string'){
	    o = document.getElementById(o);
	  }
    if (!o) {
      return 0;
    }
    var origScrollTop = o.scrollTop, iter = 1000, scrollTop = origScrollTop;
    o.scrollTop = scrollTop += iter;
    while (o.scrollTop === scrollTop) {
      o.scrollTop = scrollTop += iter;
    }
    scrollTop = o.scrollTop;
    o.scrollTop = origScrollTop;
    return scrollTop;
  };

  var prevElementSibling = (function () {
    if (typeof document.createElement('div').prevElementSibling === 'object') {
      return function (o) {
        if (typeof o === 'string'){
	        o = document.getElementById(o);
	      }
        if (!o) {
          return null;
        }
        return o.prevElementSibling;
      };
    } else {
      return function (o) {
        if (typeof o === 'string'){
	        o = document.getElementById(o);
	      }
        if (!o) {
          return null;
        }
        o = o.previousSibling;
        while(o && o.nodeType !== 1) {
          o = o.previousSibling;
        }
        return o && o.nodeType === 1 ? o : null;
      };
    }
  })();

  var nextElementSibling = (function () {
    if (typeof document.createElement('div').nextElementSibling === 'object') {
      return function (o) {
        if (typeof o === 'string'){
	        o = document.getElementById(o);
	      }
        if (!o) {
          return null;
        }
        return o.nextElementSibling;
      };
    } else {
      return function (o) {
        if (typeof o === 'string'){
	        o = document.getElementById(o);
	      }
        if (!o) {
          return null;
        }
        o = o.nextSibling;
        while(o && o.nodeType !== 1) {
          o = o.nextSibling;
        }
        return o && o.nodeType === 1 ? o : null;
      };
    }
  })();

  var firstElementChild = (function () {
    if (typeof document.createElement('div').firstElementChild === 'object') {
      return function (o) {
        if (typeof o === 'string'){
	        o = document.getElementById(o);
	      }
        if (!o) {
          return null;
        }
        return o.firstElementChild;
      };
    } else {
      return function (o) {
        if (typeof o === 'string'){
	        o = document.getElementById(o);
	      }
        if (!o) {
          return null;
        }
        o = o.firstChild;
        while(o && o.nodeType !== 1) {
          o = o.nextSibling;
        }
        return o && o.nodeType === 1 ? o : null;
      };
    }
  })();

  var lastElementChild = (function () {
    if (typeof document.createElement('div').lastElementChild === 'object') {
      return function (o) {
        if (typeof o === 'string'){
	        o = document.getElementById(o);
	      }
        if (!o) {
          return null;
        }
        return o.lastElementChild;
      };
    } else {
      return function (o) {
        if (typeof o === 'string'){
	        o = document.getElementById(o);
	      }
        if (!o) {
          return null;
        }
        o = o.lastChild;
        while(o && o.nodeType !== 1) {
          o = o.previousSibling;
        }
        return o && o.nodeType === 1 ? o : null;
      };
    }
  })();

  return {
    hasClass    : hasClass,
    addClass    : addClass,
    removeClass : removeClass,
    toggleClass : toggleClass,

    getElementsByClassName  : getElementsByClassName,

    getContent  : getContent,

    clear   : clear,
    offset  : offset,

    maxScrollLeft  : maxScrollLeft,
    maxScrollTop   : maxScrollTop,

    prevElementSibling  : prevElementSibling,
    nextElementSibling  : nextElementSibling,

    firstElementChild : firstElementChild,
    lastElementChild  : lastElementChild
  };
})();

window.ASC.TMTalk.notifications  = (function () {
  var isInit = false,
    propertyName = 'spnt',
    notifications = [],
    isDisabled = true,
    handlerIconPath = null,
    handlerNotifPath = null,
    supported = {
        current: window.Notification != null
    };

  var getNotificationsClickCallback = function (cid) {
      hWnd = null;
      try {hWnd = window.open('', window.name)} catch (err) {}
      try {hWnd.focus()} catch (err) {}
      try {ASC.TMTalk.contactsContainer.openItem(cid)} catch (err) {}
  };

  var init = function (iconpath, notifpath) {
    if (isInit === true) {
      return undefined;
    }
    isInit = true;
    // TODO:
    if (typeof iconpath === "string") {
      handlerIconPath = iconpath;
    }
    if (typeof notifpath === "string") {
      handlerNotifPath = notifpath;
    }

    if (supported.current === true && jq.notification.permissionLevel() !== "denied" && ASC.TMTalk.properties.item(propertyName) === "1") {
        isDisabled = false;
    }
  };

  var show = function (marker, title, content) {
      if (isDisabled) {
          return;
      }
      Teamlab.getProfile({}, marker.split("@")[0], {
          success: function (params, data) {
              jQuery.notification({
                  title: title,
                  body: content,
                  tag: marker,
                  timeout: 10000,
                  //iconUrl: window.ASC.TMTalk.Resources.iconTeamlabOffice32
                  iconUrl: location.origin + data.avatar
              }).then(function (notification) {
                  if (notification !== null) {
                      notifications.push({ marker: marker, handler: notification });
                      notification.onclick = function () {
                          getNotificationsClickCallback(marker);
                          notification.close();
                      };
                  }
              }, function () {
                  console.error('Rejected!');
              });
          }
      });
  };

  var hide = function (marker) {
      if (isDisabled) {
          return;
      }
      if (window.Notification) {
          var notificationsInd = notifications.length;
          while (notificationsInd-- > 0) {
              if (notifications[notificationsInd] && notifications[notificationsInd].tag === marker) {
                  notifications[notificationsInd].handler.close();
                  notifications.splice(notificationsInd, 1);
              }
          }
      }
  };
  var enable = function () {
      //Defer subscription
      if (jq('#talkStatusMenu').hasClass('processing')) {
          setTimeout(enable, 100);
          return;
      }
      if (window.Notification) {
          try {
              var messaging = window.firebase.messaging();
              messaging.requestPermission()
                  .then(function() {
                      if (Notification.permission != 'denied') {
                          isDisabled = false;
                          ASC.TMTalk.properties.item(propertyName, "1", true);
                          jq('#button-browser-notification').addClass('on').removeClass('off');
                      }
                      return messaging.getToken();
                  })
                  .then(function(token) {
                      var browser = getBrowser();
                      var username = ASC.TMTalk.connectionManager.getJID();
                      username = username.split('@')[0];
                      var endpoint = token;
                      ASC.TMTalk.connectionManager.savePushEndpoint(username, endpoint, browser);
                  })
                  .catch(function(err) {
                      console.log("Permission error:", err);
                  });
          } catch(exp) {
              Notification.requestPermission()
                 .then(function () {
                     if (Notification.permission === 'denied') {
                         return;
                     }
                     isDisabled = false;
                     ASC.TMTalk.properties.item(propertyName, "1", true);
                     jq('#button-browser-notification').addClass('on').removeClass('off');
                 });
          }
        }
    };

    var getBrowser = function() { 
       
        var is_chrome = navigator.userAgent.indexOf('Chrome') > -1;
        var is_explorer = navigator.userAgent.indexOf('MSIE') > -1;
        var is_firefox = navigator.userAgent.indexOf('Firefox') > -1;
        var is_safari = navigator.userAgent.indexOf("Safari") > -1;
        var is_opera = navigator.userAgent.toLowerCase().indexOf("opr") > -1;

        if ((is_chrome) && (is_safari) && (is_opera)) {
            return 'Opera';
        }
        else if ((is_chrome) && (is_safari)) {
            return 'Google Chrome';
        } else if ((is_safari)) {
            return 'Safari';
        } else if (is_firefox) {
            return 'Firefox';
        } else if (is_explorer) {
            return 'Internet Explorer';
        }
        
        return "Unknown browser";
    };
    
    var disable = function () {
        isDisabled = true;
        ASC.TMTalk.properties.item(propertyName, "0", true);
        if ('serviceWorker' in navigator) {

                navigator.serviceWorker.ready.then(function (serviceWorkerRegistration) {
                    serviceWorkerRegistration.pushManager.getSubscription().then(
                        function (subscription) {
                            if (!subscription) {
                                return;
                            }
                            subscription.unsubscribe().then(function (successful) {

                            }).catch(function (e) {
                                console.log('Unsubscription error: ', e);
                            });
                        });
                });
        }
    };

    var initialiseFirebase = function (config) {
        if (typeof config === "object") {
            if ('serviceWorker' in navigator && !jQuery.browser.msie && !jQuery.browser.safari) {
                try {
                    window.firebase.initializeApp(config);
                } catch(e) {
                    console.log("initialize firebase error: ", e);
                }
            } else {
                return;
            }
            //Are service workers supported in this browser
            if ('serviceWorker' in navigator && !jQuery.browser.msie && !jQuery.browser.safari) {
                    navigator.serviceWorker.register('talk.notification.js')
                    .then(initialiseState);
            }
        } else {
            if ('serviceWorker' in navigator && !jQuery.browser.msie && !jQuery.browser.safari) {

                    navigator.serviceWorker.ready.then(function (serviceWorkerRegistration) {
                        serviceWorkerRegistration.pushManager.getSubscription().then(
                            function (subscription) {
                                if (!subscription) {
                                    return;
                                }
                                subscription.unsubscribe().then(function (successful) {

                                }).catch(function (e) {
                                    console.log('Unsubscription error: ', e);
                                });
                            });
                    });
            }
    }
    };
 
    function initialiseState(registration) {
        var messaging = window.firebase.messaging();
        messaging.useServiceWorker(registration);
        if (!('showNotification' in ServiceWorkerRegistration.prototype)) {
            return;
        }
        if (enabled()) setTimeout(enable, 500);
    }
 
  var enabled = function () {
    return !isDisabled;
  };

  return {
    init  : init,

    show  : show,
    hide  : hide,

    enable    : enable,
    disable   : disable,
    enabled   : enabled,
    supported : supported,

    initialiseFirebase: initialiseFirebase
  };
})();

window.ASC.TMTalk.iconManager  = (function () {
  var
    isInit = false,
    defaultIcon = null,
    currentIsDefault = false,
    browser = (function () {
      var
        ua = navigator.userAgent,
        version = (ua.match(/.+(?:rv|it|ra|ie|ox|me|on)[\/:\s]([\d.]+)/i)||[0,'0'])[1];
      return {
        version : isFinite(+version) ? +version : version,
        // browsers
        msie    : '\v' == 'v' || /msie/i.test(ua),
        opera   : window.opera ? true : false,
        chrome  : window.chrome ? true : false,
        safari  : /safari/i.test(ua) && !window.chrome,
        firefox : /firefox/i.test(ua),
        mozilla : /mozilla/i.test(ua) && !/(compatible|webkit)/.test(ua),
        // engine
        gecko   : /gecko/i.test(ua),
        webkit  : /webkit/i.test(ua)
      }
    })();

  var init = function (path) {
    if (isInit === true) {
      return undefined;
    }
    isInit = true;
    // TODO
    var
      linkInd = 0,
      links = null;
    links = document.getElementsByTagName('link');
    linkInd = links.length;
    while (linkInd--) {
      if (links[linkInd].getAttribute('rel') === 'icon') {
        defaultIcon = links[linkInd].getAttribute('href');
        currentIsDefault = true;
        break;
      }
    }
  };

  var set = (function () {
    if (browser.chrome) {
      return function (icon) {
        var
          links = null,
          linkInd = 0;
        links = document.getElementsByTagName('link');
        linkInd = links.length;
        while (linkInd--) {
          if (links[linkInd].getAttribute('rel') === 'icon') {
            links[linkInd].setAttribute('href', icon);
            break;
          }
        }
      };
    }
    if (browser.msie) {
      return function (icon) {
        
      };
    }
    if (browser.opera) {
      return function (icon) {
      };
    }
    return function (icon) {
      var
        link = null,
        links = null,
        linkInd = 0;
      links = document.getElementsByTagName('link');
      linkInd = links.length;
      while (linkInd--) {
        link = links[linkInd];
        if (link.getAttribute('rel') === 'icon') {
          link.parentNode.removeChild(link);
        }
      }

      link = document.createElement('link');
      link.setAttribute('href', icon);
      link.setAttribute('type', 'image/x-icon');
      link.setAttribute('rel', 'icon');

      var head = document.getElementsByTagName('head')[0];
      if (head) {
        head.appendChild(link);
      }
    };
  })();

  var reset = (function () {
    if (browser.chrome) {
      return function () {
        if (defaultIcon === null) {
          return undefined;
        }
        var
          linkInd = 0,
          links = null;
        links = document.getElementsByTagName('link');
        linkInd = links.length;
        while (linkInd--) {
          if (links[linkInd].getAttribute('rel') === 'icon') {
            links[linkInd].setAttribute('href', defaultIcon);
            break;
          }
        }
      };
    }
    if (browser.msie) {
      return function () {
        
      };
    }
    if (browser.opera) {
      return function (icon) {
      };
    }
    return function () {
      var
        link = null,
        links = null,
        linkInd = 0;
      links = document.getElementsByTagName('link');
      linkInd = links.length;
      while (linkInd--) {
        link = links[linkInd];
        if (link.getAttribute('rel') === 'icon') {
          link.parentNode.removeChild(link);
        }
      }

      link = document.createElement('link');
      link.setAttribute('href', defaultIcon);
      link.setAttribute('type', 'image/x-icon');
      link.setAttribute('rel', 'icon');

      var head = document.getElementsByTagName('head')[0];
      if (head) {
        head.appendChild(link);
      }
    };
  })();

  return {
    init  : init,

    set   : set,
    reset : reset
  };
})();

window.ASC.TMTalk.uniqueId = function (prefix) {
  return (typeof prefix !== 'undefined' ? prefix + '-' : '') + Math.floor(Math.random() * 1000000);
};

window.ASC.TMTalk.stringFormat = function () {
  if (arguments.length === 0) {
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

window.ASC.TMTalk.getHash = function (value) {
  var i = 0, n = 0, hash = value.length;
  if (value.length === 1) {
    return hash ^= value.charCodeAt(0);
  }
  for (i = 0, n = value.length - 1; i < n; i += 2) {
    hash ^= +(value.charCodeAt(i).toString() + value.charCodeAt(i + 1).toString());
  }
  return hash;
};

window.ASC.TMTalk.trim = function (str) {
  if (typeof str !== 'string' || str.length === 0) {
    return '';
  }
  return str.replace(/^\s+|\s+$/g, '');
};
