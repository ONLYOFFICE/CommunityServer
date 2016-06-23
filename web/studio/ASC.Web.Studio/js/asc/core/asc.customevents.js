/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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