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


window.ASC = window.ASC || {};

window.ASC.TMTalk = window.ASC.TMTalk || {};

window.ASC.TMTalk.messagesManager = (function () {
  var
    isInit = false,
    multicastDomain = null,
    multicastIsSupported = false,
    history = {},
    monthNames = [],
    dayNames = [],
    maxHistoryCount = 10,
    shortDateFormat = '',
    fullDateFormat = '',
    customEvents = {
      // load history by filter complete
      loadFilteredHistory : 'onloadfilteredhistory',
      // load history complete
      loadHistory : 'onloadhistory',
      // show history
      openHistory : 'onopenhistory',
      // hide history
      closeHistory : 'onclosehistory',
      // composing message
      composingMessageFromChat : 'oncomposingmessagefromchat',
      // pause message
      pausedMessageFromChat : 'onpausedmessagefromchat',
      // sent messsage to contact
      sentMessageToChat : 'onsentmessagetochat',
      // receiving message from contact
      recvMessageFromChat : 'onrecvmessagefromchar',
      // receiving message from contact
      recvOfflineMessagesFromChat : 'onrecvofflinemessagesfromchar',
      // sent message to conference
      sentMessageToConference : 'onsentmessagetoconference',
      // receiving message from conference
      recvMessageFromConference : 'onrecvmessagefromconference'
    },
    eventManager = new CustomEvent(customEvents);

    var leftPad = function (n) {
      n = '' + n;
      return n.length === 1 ? '0' + n : n;
    };

    var formatDate = function(d, fmt, monthnames,daynames) {
      var
        c = '',
        r = [],
        escape = false,
        hours = d.getHours(),
        isam = hours < 12,
        day = '';

      monthnames = monthnames || ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
      daynames   = daynames   || ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
        
      if (fmt.search(/%p|%P/) !== -1) {
        if (hours > 12) {
          hours = hours - 12;
        } else if (hours == 0) {
          hours = 12;
        }
      }
      if (window.moment) {
          day = window.moment(d).calendar(null, {
              lastWeek: 'dddd DD.MM.YYYY',
              sameElse: 'dddd DD.MM.YYYY'
          }).split(' ')[0];
      } else {
          day = daynames[d.getDay()]
      }
      for (var i = 0, n = fmt.length; i < n; ++i) {
        c = fmt.charAt(i);
        if (escape) {
          switch (c) {
            case 'h': c = '' + hours; break;
            case 'H': c = leftPad(hours); break;
            case 'M': c = leftPad(d.getMinutes()); break;
            case 'S': c = leftPad(d.getSeconds()); break;
            case 'd': c = '' + d.getDate(); break;
            case 'm': c = '' + (d.getMonth() + 1); break;
            case 'y': c = '' + d.getFullYear(); break;
            case 'b': c = '' + monthnames[d.getMonth()]; break;
            case 'p': c = (isam) ? ('' + 'am') : ('' + 'pm'); break;
            case 'P': c = (isam) ? ('' + 'AM') : ('' + 'PM'); break;
              case 'D': c = '' + day; break;
          }
          r.push(c);
          escape = false;
        } else {
          if (c == '%') {
            escape = true;
          } else {
            r.push(c);
          }
        }
      }
      return r.join('');
  };

  var stampFormatDate = function (date) {
    return '' + date.getFullYear() + leftPad((date.getMonth() + 1)) + leftPad(date.getDate()) + 'T' + leftPad(date.getHours()) + ':' + leftPad(date.getMinutes()) + ':' + leftPad(date.getSeconds());
  };

  var filteringMessages = function (messages, from, to) {
    var
      timemessage = new Date(0).getTime(),
      timefrom = from.getTime(),
      timeto = to.getTime(),
      messagesInd = 0,
      filteredmessages = [];
    messagesInd = messages.length;
    while (messagesInd--) {
      timemessage = messages[messagesInd].date.getTime();
      if (timemessage >= timefrom && timemessage <= timeto) {
        filteredmessages.unshift(messages[messagesInd]);
      }
    }
    return filteredmessages;
  };

  var init = function (shortdate, fulldate, monthnames, historycount, daynames) {
    if (isInit === true) {
      return undefined;
    }
    isInit = true;
      // TODO
    dayNames = daynames.split(',');
    monthNames = monthnames.split(',');
    shortDateFormat = shortdate;
    fullDateFormat = fulldate;

    if (isFinite(+historycount)) {
      maxHistoryCount = +historycount;
    }

    

    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.retrievesFeatures, onRetrievesFeatures);
  };

  var onRetrievesFeatures = function (domain, features) {
    var featuresInd = features.length;
    while (featuresInd--) {
      switch (features[featuresInd].getAttribute('var')) {
        case Strophe.NS.ADDRESS :
          multicastIsSupported = true;
          multicastDomain = domain;
          break;
      }
    }
  };

  var bind = function (eventName, handler, params) {
    return eventManager.bind(eventName, handler, params);
  };

  var unbind = function (handlerId) {
    return eventManager.unbind(handlerId);
  };

  var getHistory = function (jid, historycount) {
    if (ASC.TMTalk.connectionManager.connected() === false) {
      eventManager.call(customEvents.loadHistory, window, [jid, history.hasOwnProperty(jid) ? history[jid].messages : [], 0]);
      return undefined;
    }
    if (history.hasOwnProperty(jid) && history[jid].loaded === true) {
      eventManager.call(customEvents.loadHistory, window, [jid, history[jid].messages, 0]);
      return undefined;
    }
    if (!history.hasOwnProperty(jid)) {
      history[jid] = {
        loaded    : false,
        messages  : [],
        archive   : null
      };
    }
    var messageslen = history[jid].messages.length;
    history[jid].messages = [];
    ASC.TMTalk.connectionManager.getMessagesByNumber(jid, (isFinite(+historycount) ? +historycount : maxHistoryCount) + messageslen);
  };

  var updateHistory = function (jid, startindex, count, text) {
     history[jid] = {
        loaded: false,
        messages: [],
        archive: null
     };
     ASC.TMTalk.connectionManager.getMessagesByRange(jid, startindex, count, text);
  };
  
  var updateOpenRoomsHistory = function () {
      var openedRooms = localStorageManager.getItem("openedRooms") != undefined ? localStorageManager.getItem("openedRooms") : {};
      for (var key in openedRooms) {
          ASC.TMTalk.messagesManager.updateHistory(key, 0, 20, '');
      }
  };
    
  var getHistoryByFilter = function (jid, from, to) {
    if (ASC.TMTalk.connectionManager.connected() === false) {
      eventManager.call(customEvents.loadFilteredHistory, window, [jid, history.hasOwnProperty(jid) ? filteringMessages(history[jid].archive.messages, from, to) : []]);
      return undefined;
    }
    if (history.hasOwnProperty(jid) && history[jid].archive !== null && from.getTime() >= history[jid].archive.from.getTime()) {
      eventManager.call(customEvents.loadFilteredHistory, window, [jid, filteringMessages(history[jid].archive.messages, from, to)]);
      return undefined;
    }
    if (!history.hasOwnProperty(jid)) {
      history[jid] = {
        loaded    : false,
        messages  : [],
        archive   : null
      };
    }
    if (history[jid].archive === null) {
      history[jid].archive = {from : new Date(), messages : []};
    }
    if (from.getTime() < history[jid].archive.from.getTime()) {
      history[jid].archive.messages = [];
      ASC.TMTalk.connectionManager.getMessagesByDate(jid, stampFormatDate(from), stampFormatDate(to));
    }
  };

  var loadHistory = function (iq, removeOld, searchText) {
    var
      child = null,
      isMe = false,
      childs = null,
      childsInd = 0,
      items = null,
      itemsInd = 0,
      body = '',
      date = null,
      from = '',
      type = '',
      sender = '',
      messages = [],
      jid = iq.getAttribute('from'),
      ownjid = ASC.TMTalk.connectionManager.getJid();

    if (!history.hasOwnProperty(jid)) {
      return undefined;
    }

    items = iq.getElementsByTagName('item');
    if (items.length === 0) {
      items = Strophe.getElementsByTagName(iq, 'item');
    }

    itemsInd = items.length;
    while (itemsInd--) {
      from = items[itemsInd].getAttribute('from');
      type = items[itemsInd].getAttribute('type');
      sender = type === 'groupchat' ? from.substring(from.indexOf('/') + 1, from.length) : from.substring(0, from.indexOf('/')).toLowerCase();
      sender = sender === '' ? from : sender;
      childs = items[itemsInd].childNodes;
      childsInd = childs.length;
      while (childsInd--) {
        child = childs[childsInd];
        if (child.nodeType == '1') {
          switch (child.tagName.toLowerCase()) {
            case 'body' :
              body = ASC.TMTalk.dom.getContent(child);
              break;
            case 'x' :
              if (child.getAttribute('xmlns') == Strophe.NS.TIMESTAMP) {
                date = child.getAttribute('stamp').split('T');
                date[1] = date[1].split(':');
                date = new Date(Date.UTC(+date[0].substr(0, 4), +date[0].substr(4, 2) - 1, +date[0].substr(6, 2), +date[1][0], +date[1][1], +date[1][2]));
                //date.setMinutes(date.getMinutes() - new Date().getTimezoneOffset());
                date = date instanceof Date ? date : new Date();
              }
              break;
          }
        }
      }
      date = date === null ? new Date() : date;
      isMe = type === 'groupchat' ? ASC.TMTalk.mucManager.isMe(from) : ownjid === sender;
      messages.unshift({
        jid         : jid,
        date        : date,
        displayDate : getDisplayDate(date),
        displayName : ASC.TMTalk.contactsManager.getContactName(sender),
        body        : body,
        isOwn       : isMe
      });
    }

    if (jid) {
      var newmessages = history[jid].messages.length;
      history[jid].loaded = true;
      history[jid].messages = messages;
        eventManager.call(customEvents.loadHistory, window, [jid, history[jid].messages, newmessages, removeOld, searchText]);
    }
  };

  var loadHistoryByFilter = function (iq, dateFrom, dateTo) {
    if (typeof dateFrom !== 'string' || typeof dateTo !== 'string') {
      return undefined;
    }

    dateFrom = dateFrom.split('T');
    dateFrom[1] = dateFrom[1].split(':');
    dateFrom = new Date(Date.UTC(+dateFrom[0].substr(0, 4), +dateFrom[0].substr(4, 2) - 1, +dateFrom[0].substr(6, 2), +dateFrom[1][0], +dateFrom[1][1], +dateFrom[1][2]));
    dateFrom = dateFrom instanceof Date ? dateFrom : null;

    dateFrom.setTime(dateFrom.getTime() + dateFrom.getTimezoneOffset() * 60000);

    dateTo = dateTo.split('T');
    dateTo[1] = dateTo[1].split(':');
    dateTo = new Date(Date.UTC(+dateTo[0].substr(0, 4), +dateTo[0].substr(4, 2) - 1, +dateTo[0].substr(6, 2), +dateTo[1][0], +dateTo[1][1], +dateTo[1][2]));
    dateTo = dateTo instanceof Date ? dateTo : null;

    dateTo.setTime(dateTo.getTime() + dateTo.getTimezoneOffset() * 60000);

    if (dateFrom === null || dateTo === null) {
      return undefined;
    }

    var
      child = null,
      childs = null,
      childsInd = 0,
      items = null,
      itemsInd = 0,
      body = '',
      date = null,
      from = '',
      sender = '',
      messages = [],
      jid = iq.getAttribute('from'),
      ownjid = ASC.TMTalk.connectionManager.getJid();

    if (!history.hasOwnProperty(jid)) {
      return undefined;
    }

    items = iq.getElementsByTagName('item');
    if (items.length === 0) {
      items = Strophe.getElementsByTagName(iq, 'item');
    }

    itemsInd = items.length;
    while (itemsInd--) {
      from = items[itemsInd].getAttribute('from');
      sender = from.substring(0, from.indexOf('/')).toLowerCase();
      sender = sender === '' ? from : sender;
      childs = items[itemsInd].childNodes;
      childsInd = childs.length;
      while (childsInd--) {
        child = childs[childsInd];
        if (child.nodeType == '1') {
          switch (child.tagName.toLowerCase()) {
            case 'body' :
              body = ASC.TMTalk.dom.getContent(child);
              break;
            case 'x' :
              if (child.getAttribute('xmlns') == Strophe.NS.TIMESTAMP) {
                date = child.getAttribute('stamp').split('T');
                date[1] = date[1].split(':');
                date = new Date(Date.UTC(+date[0].substr(0, 4), +date[0].substr(4, 2) - 1, +date[0].substr(6, 2), +date[1][0], +date[1][1], +date[1][2]));
                //date.setMinutes(date.getMinutes() - new Date().getTimezoneOffset());
                date = date instanceof Date ? date : new Date();
              }
              break;
          }
        }
      }
      date = date === null ? new Date() : date;
      messages.unshift({
        jid         : jid,
        date        : date,
        displayDate : getDisplayDate(date),
        displayName : ASC.TMTalk.contactsManager.getContactName(sender),
        body        : body,
        isOwn       : ownjid === sender
      });
    }
    history[jid].archive.from = dateFrom;
    history[jid].archive.messages = messages;
    eventManager.call(customEvents.loadFilteredHistory, window, [jid, filteringMessages(history[jid].archive.messages, dateFrom, dateTo)]);
  };

  var openHistory = function (jid) {
    eventManager.call(customEvents.openHistory, window, [jid]);
  };
  var clearCurrentHistory = function (jid) {
      if (history[jid]) {
         delete history[jid]; 
      }
  };
  var closeHistory = function (jid) {
    eventManager.call(customEvents.closeHistory, window, [jid]);
  };

  var composingMessageFromChat = function (message) {
    var
      from = message.getAttribute('from'),
      jid = from.substring(0, from.indexOf('/')).toLowerCase();

    eventManager.call(customEvents.composingMessageFromChat, window, [jid === '' ? from : jid]);
  };

  var pausedMessageFromChat = function (message) {
    var
      from = message.getAttribute('from'),
      jid = from.substring(0, from.indexOf('/')).toLowerCase();

    eventManager.call(customEvents.pausedMessageFromChat, window, [jid === '' ? from : jid]);
  };

  var sendMessageToChat = function (jid, body, unsend) {
    if (ASC.TMTalk.connectionManager.connected() === false) {
      return undefined;
    }
    var date = new Date();
    if (jid && typeof body === 'string' && body.length > 0) {
      if (unsend !== true) {
        ASC.TMTalk.connectionManager.sendMessage(jid, body, 'chat');
      }
      var message = {
        jid         : jid,
        date        : date,
        displayDate : getDisplayDate(date),
        displayName : ASC.TMTalk.contactsManager.getContactName(),
        body        : body,
        isOwn       : true
      };
      if (!history.hasOwnProperty(jid)) {
        history[jid] = {
          loaded    : false,
          messages  : [],
          archive   : null
        };
      }
      history[jid].messages.push(message);
      if (history[jid].archive !== null) {
        history[jid].archive.messages.push(message);
      }
      eventManager.call(customEvents.sentMessageToChat, window, [jid, message.displayName, message.displayDate, date, body]);
    }
  };

  var recvMulticastMessage = function (message) {
    var
      nodes = null,
      date = new Date(),
      body = '',
      from = message.getAttribute('from'),
      jid = from.substring(0, from.indexOf('/')).toLowerCase();

    jid = jid === '' ? from : jid;
    nodes = message.getElementsByTagName('body');
    if (nodes.length === 0) {
      nodes = Strophe.getElementsByTagName(message, 'body');
    }
    if (nodes.length > 0) {
      body = ASC.TMTalk.dom.getContent(nodes[0]);
    }

    if (jid && body.length > 0) {
      var message = {
        jid         : jid,
        date        : date,
        displayDate : getDisplayDate(date),
        displayName : ASC.TMTalk.contactsManager.getContactName(jid),
        body        : body,
        isOwn       : false
      };
      if (!history.hasOwnProperty(jid)) {
        history[jid] = {
          loaded    : false,
          messages  : [],
          archive   : null
        };
      }
      history[jid].messages.push(message);
      if (history[jid].archive !== null) {
        history[jid].archive.messages.push(message);
      }

      eventManager.call(customEvents.recvMessageFromChat, window, [jid, message.displayName, message.displayDate, date, body]);
    }
  };

  var recvOfflineMessagesFromChat = function (messages) {
      
    var
      date = null,
      nodes = null,
      body = '',
      from = '',
      jid = '',
      message = null,
      offlinemessages = {};

    for (var i = 0, n = messages.length; i < n; i++) {
      message = messages[i];

      body = '';
      date = new Date();
      from = message.getAttribute('from');
      jid = from.substring(0, from.indexOf('/')).toLowerCase();
      jid = jid === '' ? from : jid;

      nodes = message.getElementsByTagName('body');
      if (nodes.length === 0) {
        nodes = Strophe.getElementsByTagName(message, 'body');
      }
      if (nodes.length > 0) {
        body = ASC.TMTalk.dom.getContent(nodes[0]);
      }

      nodes = message.getElementsByTagName('delay');
      if (nodes.length === 0) {
        nodes = Strophe.getElementsByTagName(message, 'delay');
      }
      if (nodes.length > 0) {
        date = nodes[0].getAttribute('stamp');
        if (date) {
          date = date.split('T');
          date[1] = date[1].split(':');
          date = new Date(Date.UTC(+date[0].substr(0, 4), +date[0].substr(4, 2) - 1, +date[0].substr(6, 2), +date[1][0], +date[1][1], +date[1][2]));
          //date.setMinutes(date.getMinutes() - new Date().getTimezoneOffset());
          date = date instanceof Date ? date : new Date();
        }
      }

      if (jid && body.length > 0) {
        var msg = {
          jid         : jid,
          date        : date,
          displayDate : getDisplayDate(date),
          displayName : ASC.TMTalk.contactsManager.getContactName(jid),
          body        : body,
          isOwn       : false
        };
        if (!offlinemessages.hasOwnProperty(jid)) {
          offlinemessages[jid] = [];
        }
        offlinemessages[jid].push(msg);
        if (!history.hasOwnProperty(jid)) {
          history[jid] = {
            loaded    : false,
            messages  : [],
            archive   : null
          };
        }
        history[jid].messages.push(msg);
        if (history[jid].archive !== null) {
          history[jid].archive.messages.push(msg);
        }
      }
    }
    for (jid in offlinemessages) {
      if (offlinemessages.hasOwnProperty(jid)) {
        eventManager.call(customEvents.recvOfflineMessagesFromChat, window, [jid, offlinemessages[jid]]);
      }
    }
    //for (i = 0, n = offlinemessages.length; i < n; i++) {
    //  message = offlinemessages[i];
    //  eventManager.call(customEvents.pausedMessageFromChat, window, [message.jid]);
    //  eventManager.call(customEvents.recvOfflineMessageFromChat, window, [message.jid, message.displayName, message.displayDate, message.date, message.body, true]);
    //}
  };

  var recvMessageFromChat = function (message, offlineStorage) {
    var
      nodes = null,
      date = new Date(),
      body = '',
      from = message.getAttribute('from'),
      jid = from.substring(0, from.indexOf('/')).toLowerCase();

    jid = jid === '' ? from : jid;
    nodes = message.getElementsByTagName('body');
    if (nodes.length === 0) {
      nodes = Strophe.getElementsByTagName(message, 'body');
    }
    if (nodes.length > 0) {
      body = ASC.TMTalk.dom.getContent(nodes[0]);
    }

    nodes = message.getElementsByTagName('delay');
    if (nodes.length === 0) {
      nodes = Strophe.getElementsByTagName(message, 'delay');
    }
    if (nodes.length > 0) {
      date = nodes[0].getAttribute('stamp');
      if (date) {
        date = date.split('T');
        date[1] = date[1].split(':');
        date = new Date(Date.UTC(+date[0].substr(0, 4), +date[0].substr(4, 2) - 1, +date[0].substr(6, 2), +date[1][0], +date[1][1], +date[1][2]));
        //date.setMinutes(date.getMinutes() - new Date().getTimezoneOffset());
        date = date instanceof Date ? date : new Date();
      }
    }

    if (jid && body.length > 0) {
      var msg = {
        jid         : jid,
        date        : date,
        displayDate : getDisplayDate(date),
        displayName : ASC.TMTalk.contactsManager.getContactName(jid),
        body        : body,
        isOwn       : false
      };
      if (!history.hasOwnProperty(jid)) {
        history[jid] = {
          loaded    : false,
          messages  : [],
          archive   : null
        };
      }
      history[jid].messages.push(msg);
      if (history[jid].archive !== null) {
        history[jid].archive.messages.push(msg);
      }

      eventManager.call(customEvents.pausedMessageFromChat, window, [jid]);
      eventManager.call(customEvents.recvMessageFromChat, window, [jid, msg.displayName, msg.displayDate, date, body, offlineStorage === true]);
    }
  };

  var sendMessageToConference = function (jid, message) {
    if (ASC.TMTalk.connectionManager.connected() === false) {
      return undefined;
    }
    if (jid && typeof message === 'string' && message.length > 0) {
      ASC.TMTalk.connectionManager.sendMessage(jid, message, 'groupchat');
    }
  };

  var recvMessageFromConference = function (message) {
    var
      nodes = null,
      nodesInd = 0,
      child = null,
      childs = null,
      childsInd = 0,
      timestamp = null,
      from = message.getAttribute('from'),
      name = '',
      roomjid = from.substring(0, from.indexOf('/')).toLowerCase(),
      date = null,
      body = '';

    childs = message.childNodes;
    childsInd = childs.length;
    while (childsInd--) {
      child = childs[childsInd];
      if (child.nodeType == '1') {
        switch (child.tagName.toLowerCase()) {
          case 'body' :
            body = ASC.TMTalk.dom.getContent(child);
            break;
          case 'x' :
            if (child.getAttribute('xmlns') == Strophe.NS.TIMESTAMP) {
              date = child.getAttribute('stamp').split('T');
              date[1] = date[1].split(':');
              date = new Date(Date.UTC(+date[0].substr(0, 4), +date[0].substr(4, 2) - 1, +date[0].substr(6, 2), +date[1][0], +date[1][1], +date[1][2]));
              //date.setMinutes(date.getMinutes() - new Date().getTimezoneOffset());
              date = date instanceof Date ? date : new Date();
            }
            break;
        }
      }
    }

    name = ASC.TMTalk.mucManager.getContactName(from);
    if (name === from) {
      name = from.substring(from.indexOf('/') + 1);
    }

    if (name && roomjid && date && body.length > 0) {
      eventManager.call(customEvents.recvMessageFromConference, window, [roomjid, name, getDisplayDate(date), date, body, ASC.TMTalk.mucManager.isMe(from)]);
    }
  };

  var sendMultiuserMessage = function (jids, body) {
    if (ASC.TMTalk.connectionManager.connected() === false) {
      return undefined;
    }
    if (typeof body === 'string' && body.length > 0) {
      if (multicastIsSupported === true && multicastDomain !== null) {
        ASC.TMTalk.connectionManager.sendMulticastMessage(multicastDomain, jids, body);

        var jidsInd = jids.length;
        while (jidsInd--) {
          sendMessageToChat(jids[jidsInd], body, true);
        }
      } else {
        var jidsInd = jids.length;
        while (jidsInd--) {
          sendMessageToChat(jids[jidsInd], body);
        }
      }
    }
  };
  var getDisplayDate = function (date) {
     // var displaydate = formatDate(date, date.getTime() < new Date().getTime() ? fullDateFormat : shortDateFormat, monthNames);
      var displaydate = formatDate(date, '%D %d.%m.%y %H:%M', monthNames, dayNames);
      return displaydate !== '' ? displaydate : date.toLocaleTimeString();
  };

  var historyLoaded = function (jid) {
    return history.hasOwnProperty(jid) && history[jid].loaded;
  };

  return {
    init  : init,

    bind    : bind,
    unbind  : unbind,

    events  : customEvents,

    formatDate  : formatDate,

    composingMessageFromChat  : composingMessageFromChat,
    pausedMessageFromChat     : pausedMessageFromChat,

    sendMultiuserMessage        : sendMultiuserMessage,
    sendMessageToChat           : sendMessageToChat,
    sendMessageToConference     : sendMessageToConference,
    recvMessageFromChat         : recvMessageFromChat,
    recvMulticastMessage        : recvMulticastMessage,
    recvMessageFromConference   : recvMessageFromConference,
    recvOfflineMessagesFromChat : recvOfflineMessagesFromChat,

    updateHistory       : updateHistory,
    updateOpenRoomsHistory: updateOpenRoomsHistory,
    getHistory          : getHistory,
    getHistoryByFilter  : getHistoryByFilter,
    loadHistory         : loadHistory,
    loadHistoryByFilter : loadHistoryByFilter,
    openHistory         : openHistory,
    closeHistory        : closeHistory,
    historyLoaded       : historyLoaded,
    clearCurrentHistory : clearCurrentHistory,

    getDisplayDate  : getDisplayDate
  };
})();
