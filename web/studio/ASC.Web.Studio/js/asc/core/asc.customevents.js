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