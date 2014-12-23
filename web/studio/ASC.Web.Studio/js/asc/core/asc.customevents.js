/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

;window.CustomEvent = function (newEvents) {
  if (typeof this.supportedEvents === 'undefined') {
    this.supportedEvents = [];
  }
  if (typeof this.eventHandlers === 'undefined') {
    this.eventHandlers = [];
    this.handlerIds = {};
  }
  if (!newEvents) {
    return undefined;
  }
  if (typeof newEvents === 'object') {
    if (newEvents instanceof Array) {
      for (var i = 0, n = newEvents.length; i < n; i++) {
        if (typeof newEvents[i] === 'string') {
          this.supportedEvents.push(newEvents[i].toLowerCase());
        }
      }
    } else {
      for (var fld in newEvents) {
        if (newEvents.hasOwnProperty(fld)) {
          if (typeof newEvents[fld] === 'string') {
            this.supportedEvents.push(newEvents[fld].toLowerCase());
          }
        }
      }
    }
  } else if (typeof newEvents === 'string') {
    this.supportedEvents.push(newEvents.toLowerCase());
  }
};

window.CustomEvent.prototype.isSupported = function (eventName) {
  if (!(this.supportedEvents instanceof Array) || !(this.eventHandlers instanceof Array)) {
    return false;
  }
  var eInd = this.supportedEvents.length;
  while (eInd--) {
    if (this.supportedEvents[eInd] === eventName) {
      return true;
    }
  }
  return false;
};

window.CustomEvent.prototype.bind = function (eventName, handler, params) {
  if (!(this.supportedEvents instanceof Array) || !(this.eventHandlers instanceof Array)) {
    //console.trace();
    throw 'Invalid CustomEvent object';
  }
  if (typeof eventName !== 'string' || typeof handler !== 'function') {
    //console.trace();
    //console.log('#bind eventName: ', eventName, 'handler: ', handler);
    throw 'Incorrect params';
  }
  if (!this.isSupported(eventName = eventName.toLowerCase())) {
    //console.trace();
    throw 'Unsupported event ' + eventName;
  }

  params = params || {};
  var isOnceExec = params.hasOwnProperty('once') ? params.once : false;

  // generate handler mask
  var handlerType = 0;
  handlerType |= +isOnceExec * 1;  // isOnceExec - execute once

  var
    iterCount = 0,
    maxIterations = 1000,
    handlerId = Math.floor(Math.random() * 1000000);
  while (this.handlerIds.hasOwnProperty(handlerId) && iterCount++ < maxIterations) {
    handlerId = Math.floor(Math.random() * 1000000);
  }

  this.handlerIds[handlerId] = {};
  this.eventHandlers.push({id : handlerId, name : eventName, handler : handler, type : handlerType});
  return handlerId;
};

window.CustomEvent.prototype.unbind = function (handlerId) {
  if (!(this.supportedEvents instanceof Array) || !(this.eventHandlers instanceof Array)) {
    //console.trace();
    throw 'Invalid CustomEvent object';
  }
  if (typeof handlerId === 'undefined') {
    return undefined;
  }
  if (this.handlerIds.hasOwnProperty(handlerId)) {
    var
      handlers = this.eventHandlers,
      handlerInd = this.eventHandlers.length;
    while (handlerInd--) {
        if (handlers[handlerInd].id === handlerId) {
            delete this.handlerIds[handlerInd];
            handlers.splice(handlerInd, 1);
        break;
      }
    }
  }
};

window.CustomEvent.prototype.call = function (eventName, thisArg, argsArray) {
  if (!(this.supportedEvents instanceof Array) || !(this.eventHandlers instanceof Array)) {
    //console.trace();
    throw 'Invalid CustomEvent object';
  }
  if (typeof eventName !== 'string') {
    //console.trace();
    //console.log('#call eventName: ', eventName);
    throw 'Incorrect params';
  }
  if (!this.isSupported(eventName = eventName.toLowerCase())) {
    //console.trace();
    throw 'Unsupported event ' + eventName;
  }

  thisArg = thisArg || window;
  argsArray = argsArray || [];
  var handlers = this.eventHandlers;
  for (var i = 0, n = handlers.length; i < n; i++) {
    if (handlers[i].name === eventName) {
      handlers[i].handler.apply(thisArg, argsArray);
      n = handlers.length;
      if (handlers[i] && handlers[i].type & 1) {
        handlers.splice(i, 1);
        i--; n--;
      }
    }
  }
};

window.CustomEvent.prototype.extend = function (newEvents) {
  if (typeof this.supportedEvents === 'undefined') {
    this.supportedEvents = [];
  }
  if (typeof this.eventHandlers === 'undefined') {
    this.eventHandlers = [];
    this.handlerIds = {};
  }
  if (!newEvents) {
    return undefined;
  }
  if (typeof newEvents === 'object') {
    if (newEvents instanceof Array) {
      for (var i = 0, n = newEvents.length; i < n; i++) {
        if (typeof newEvents[i] === 'string') {
          this.supportedEvents.push(newEvents[i].toLowerCase());
        }
      }
    } else {
      for (var fld in newEvents) {
        if (newEvents.hasOwnProperty(fld)) {
          if (typeof newEvents[fld] === 'string') {
            this.supportedEvents.push(newEvents[fld].toLowerCase());
          }
        }
      }
    }
  } else if (typeof newEvents === 'string') {
    this.supportedEvents.push(newEvents.toLowerCase());
  }
};