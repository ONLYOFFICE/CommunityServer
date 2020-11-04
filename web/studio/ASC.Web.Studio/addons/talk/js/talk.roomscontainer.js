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

window.ASC.TMTalk.roomsContainer = (function ($) {
  var
    isInit = false,
    inviteTimeout = 60,
    defaultRoom = null,
    defaultMessage = null,
    defaultContact = null,
    roomlistContainer = null,
    conferencesIds = {},
    messageTimeout = null,
    lastChatMessageDates = {},
    lastConferenceMessageDates = {},
    smilesCollection = [];

  var setNodes = function () {
    if (roomlistContainer === null) {
      var nodes = null;
      roomlistContainer = document.getElementById('talkRoomsContainer');
      nodes = ASC.TMTalk.dom.getElementsByClassName(roomlistContainer, 'room default', 'li');
      defaultRoom = nodes.length > 0 ? nodes[0] : null;
      nodes = ASC.TMTalk.dom.getElementsByClassName(roomlistContainer, 'message default', 'li');
      defaultMessage = nodes.length > 0 ? nodes[0] : null;
      nodes = ASC.TMTalk.dom.getElementsByClassName(roomlistContainer, 'contact default', 'li');
      defaultContact = nodes.length > 0 ? nodes[0] : null;
    }
  };

  var GetOnRottenInvite = function (roomjid, contactjid) {
    return function () {
      onRottenInvite(roomjid, contactjid);
    };
  };

  var createRoom = function (roomId) {
    var newroom = null;

    if (!defaultRoom) {
      throw 'no templates default room';
    }

    newroom = defaultRoom.cloneNode(true);
    newroom.className = newroom.className.replace(/\s*default\s*/, ' ').replace(/^\s+|\s+$/g, '');
    if (typeof roomId === 'string') {
      newroom.setAttribute('data-roomid', roomId);
    }

    return newroom;
  };

  var createMessage = function (name, date, body, isMine) {
    var
      nodes = null,
      nodesInd = 0,
      newmessage = null,
      time = "",
      weekday ="",
      day = "";
      
    if (date.split(' ')) {
        time = date.split(' ')[2],
        weekday = date.split(' ')[0],
        day = date.split(' ')[1];
        
        var currentDate = new Date;
        var today = [currentDate.getDate(), (currentDate.getMonth() + 1), currentDate.getFullYear()].join('.');
        currentDate.setDate(currentDate.getDate() - 1);
        var yesterday = currentDate;
        yesterday = [yesterday.getDate(), (yesterday.getMonth() + 1), yesterday.getFullYear()].join('.');

        day = day === today || day === yesterday ? weekday : weekday + " " + day;
    }
      
    if (!defaultMessage) {
      throw 'no templates defaultmessage';
    }

    newmessage = defaultMessage.cloneNode(true);
    newmessage.className = newmessage.className.replace(/\s*default\s*/, ' ').replace(/^\s+|\s+$/g, '');

    if (isMine === true) {
      ASC.TMTalk.dom.addClass(newmessage, 'own');
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(newmessage, 'title', 'span');
    if (nodes.length > 0) {
      nodes[0].innerHTML = name;
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(newmessage, 'date', 'div');
    if (nodes.length > 0) {
        for (var nodeNum = 0; nodeNum < nodes.length; nodeNum++) {
            var dateValue = ASC.TMTalk.dom.getElementsByClassName(nodes[nodeNum], 'value', 'span');
            if (dateValue.length > 0) {
                dateValue[0].innerHTML = time;
            }
        }
      
    }
      
    nodes = ASC.TMTalk.dom.getElementsByClassName(newmessage, 'daysplit', 'span');
    if (nodes.length > 0) {
        nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'value', 'span');
        if (nodes.length > 0) {
            $(nodes[0]).attr('date', date.split(' ')[1])
            nodes[0].innerHTML = day;
        }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(newmessage, 'body', 'div');
    if (nodes.length > 0) {
      nodes[0].innerHTML = body;
    }

    return newmessage;
  };

  var createContact = function (contactjid) {
    var newcontact = null;

    if (!defaultContact) {
      throw 'no templates defaultcontact';
    }

    newcontact = defaultContact.cloneNode(true);
    newcontact.className = newcontact.className.replace(/\s*default\s*/, ' ').replace(/^\s+|\s+$/g, '');
    if (typeof contactjid === 'string') {
      newcontact.setAttribute('data-cid', contactjid);
    }

    return newcontact;
  };

  var getRoomById = function (roomid) {
    var
      nodes = null,
      nodesInd = 0;

    nodes = ASC.TMTalk.dom.getElementsByClassName(roomlistContainer, 'room', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-roomid') === roomid) {
        return nodes[nodesInd];
      }
    }
    return null;
  }

  var getRoomByCid = function (roomjid) {
    var
      nodes = null,
      nodesInd = 0;

    nodes = ASC.TMTalk.dom.getElementsByClassName(roomlistContainer, 'room', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-roomcid') === roomjid) {
        return nodes[nodesInd];
      }
    }
    return null;
  }

  function trimS (str) {
    if (typeof str !== 'string' || str.length === 0) {
      return '';
    }
    return str.replace(/^\s+|\s+$/g, '');
  }

  function trimN (str) {
    if (typeof str !== 'string' || str.length === 0) {
      return '';
    }
    return str.replace(/^\n+|\n+$/g, '');
  }

  function trimAll (str) {
    if (typeof str != 'string' || str.length === 0) {
      return '';
    }
    return str.replace(/^[\n\s]+|[\n\s]+$/g, '');
  }

  function translateSymbols (str, toText) {
    
    var
      symbols = [
        ['&lt;',  '<'],
        ['&gt;',  '>'],
        ['&and;', '\\^'],
        ['&sim;', '~'],
        ['&amp;', '&']
      ];

    if (typeof str !== 'string') {
      return '';
    }

    // replace html to symbols
    if (typeof toText === 'undefined' || toText === true) {
      var symInd = symbols.length;
      while (symInd--) {
        str = str.replace(new RegExp(symbols[symInd][0], 'g'), symbols[symInd][1]); 
      }
    // replace symbols to html
    } else {
      var symInd = symbols.length;
      while (symInd--) {
        str = str.replace(new RegExp(symbols[symInd][1], 'g'), symbols[symInd][0]);
      }
    }
    return str;
  }

  function textToHtml (text) {
    var
      body = trimAll(text),
      shift = 0,
      pos = -1,
      i = 0, n = 0,
      link = null,
      lnkstr = '',
      hrefstr = '',
      smile = null,
      smilesInd = 0,
      aliasesInd = 0,
      //reUrl = /(http:\/\/|https:\/\/|ftp:\/\/|www\.){1}[\w\.\/\|\\:?#'~\[\]\{\}%&=+$,@\-_!~*;()]+/ig,
      foundLinks = [],
      foundSmiles = [];

    body = translateSymbols(body, false);
    body = body.replace(/\n/g, '<br>');
      
    var reUrl = new XRegExp("(http:\/\/|https:\/\/|ftp:\/\/|www\.){1}[0-9\\p{L}\\p{M}\w\.\/\|\\:?#'~\[\\]\{\}%&=+$,@\\-_!~*;()]+", 'gi');

    while (link = reUrl.exec(body)) {
      lnkstr = link[0];
      //hrefstr = lnkstr.indexOf('\\') !== -1 ? 'file:' + lnkstr.replace(/\\/g, '/') : lnkstr;
      hrefstr = lnkstr;
      hrefstr = hrefstr.substring(0, 4) === 'www.' ? 'http://' + hrefstr : hrefstr;
      // remove ',' from link if it is last character 
      var beginIndex = reUrl.lastIndex - lnkstr.length,
        endIndex = reUrl.lastIndex;
      if (lnkstr.slice(-1) == ",") {
        lnkstr = lnkstr.substring(0, lnkstr.length - 1);
        hrefstr = hrefstr.substring(0, hrefstr.length - 1);
        beginIndex--;
        endIndex--;
      }
     
      foundLinks.push({
        begin: beginIndex,
        end: endIndex,
        text: '<a href="' + hrefstr + '" target="_blank">' + lnkstr + '</a>'
      });
    }

    shift = 0;
    for (i = 0, n = foundLinks.length; i < n; i++) {
      body = body.substring(0, foundLinks[i].begin + shift) + foundLinks[i].text + body.substring(foundLinks[i].end + shift);
      shift += foundLinks[i].text.length - foundLinks[i].end + foundLinks[i].begin;
    }

    smilesInd = smilesCollection.length;
    while (smilesInd--) {
      smile = smilesCollection[smilesInd];
      aliasesInd = smile.aliases.length;
      while (aliasesInd--) {
        pos = -1;
        while ((pos = body.indexOf(smile.aliases[aliasesInd], pos + 1)) != -1) {
          foundSmiles.push({
            begin   : pos,
            end     : pos + smile.aliases[aliasesInd].length,
            text    : '<img src="' + smile.src + '" alt="' + smile.title + '">'
          });
        }
      }
    }

    foundSmiles.sort(function (a, b) {
      return a.begin - b.begin;
    });
    shift = 0;
    for (i = 0, n = foundSmiles.length; i < n; i++) {
      body = body.substring(0, foundSmiles[i].begin + shift) + foundSmiles[i].text + body.substring(foundSmiles[i].end + shift);
      shift += foundSmiles[i].text.length - foundSmiles[i].end + foundSmiles[i].begin;
    }

    return body;
  }


  var addMessage = function (roomnode, name, date, body, isMine, needParagraph, isUnreadMessage, inTop) {
      var nodes = null,
          newmessage = null,
          firstmessage = null,
          lastmessage = null,
          messagescontainer = null,
          firstmessageday = null,
          firstmessagedatesplit = null,
          lastmessageday = null,
          lastmessagedatesplit = null,
          day = "",
          cid = $(roomnode).attr('data-roomcid');
      newmessage = createMessage(name, date, body, isMine);
      
      if (date.split(' ')) {
          
          day = date.split(' ')[1];
          var currentDate = new Date;
          var today = [currentDate.getDate(), (currentDate.getMonth() + 1), currentDate.getFullYear()].join('.');
          currentDate.setDate(currentDate.getDate() - 1);
          var yesterday = currentDate;
          yesterday = [yesterday.getDate(), (yesterday.getMonth() + 1), yesterday.getFullYear()].join('.');

          day = day === today || day === yesterday ? date.split(' ')[0] : [date.split(' ')[0],date.split(' ')[1]].join(" ");
      }
      if (!ASC.TMTalk.dom.hasClass(roomnode, 'conference')) {
          $(ASC.TMTalk.dom.getElementsByClassName(newmessage, 'head', 'div')[0]).hide();
      }
    nodes = ASC.TMTalk.dom.getElementsByClassName(roomnode, 'messages', 'ul');
    if (nodes.length > 0) {
      messagescontainer = nodes[0];
    }
    $(messagescontainer).unbind('scroll');
    $(messagescontainer).scroll(function () {
        var currentMessagesCount = 0;
        if (this.scrollTop === 0) {
            currentMessagesCount = jQuery('div#talkRoomsContainer ul.rooms li.room.current ul.messages:first').find('li').not('.default').length;
            var startindex = currentMessagesCount++;
            var count = 5;
            ASC.TMTalk.messagesManager.updateHistory(cid, startindex, count, jq(".room.current .search-value").val());
        }
    });
    if (messagescontainer !== null) {
      lastmessage = ASC.TMTalk.dom.lastElementChild(messagescontainer);
      if (ASC.TMTalk.dom.hasClass(lastmessage, 'default')) {
        lastmessage = null;
      }

      if (lastmessage === null || needParagraph) {
        ASC.TMTalk.dom.addClass(newmessage, 'paragraph');
      } else {
        if (lastmessage !== null) {
          ASC.TMTalk.dom.addClass(lastmessage, 'more');
        }
      }
      if (lastmessage !== null) {
          lastmessagedatesplit = ASC.TMTalk.dom.getElementsByClassName(lastmessage, 'daysplit', 'span');
          lastmessageday = ASC.TMTalk.dom.getElementsByClassName(lastmessagedatesplit[0], 'value', 'span');
          if (day === lastmessageday[0].innerText && !inTop) {
              ASC.TMTalk.dom.addClass(ASC.TMTalk.dom.getElementsByClassName(newmessage, 'value', 'span')[0], 'hidden');
          }
      }
      if (isUnreadMessage === true) {
        ASC.TMTalk.dom.addClass(newmessage, 'unread');
        ASC.TMTalk.events.add(newmessage, 'mouseover', function () {
          ASC.TMTalk.dom.removeClass(newmessage, 'unread');
          ASC.TMTalk.events.remove(newmessage, 'mouseover', arguments.callee);
        });
      }

      if (inTop === true) {
        nodes = ASC.TMTalk.dom.getElementsByClassName(messagescontainer, 'message', 'li');
        for (var i = 0, n = nodes.length; i < n; i++) {
          if (ASC.TMTalk.dom.hasClass(nodes[i], 'default') === false) {
            firstmessage = nodes[i];
            break;
          }
        }
      }
      if (firstmessage != null) {
          firstmessagedatesplit = ASC.TMTalk.dom.getElementsByClassName(firstmessage, 'daysplit', 'span');
          firstmessageday = ASC.TMTalk.dom.getElementsByClassName(firstmessagedatesplit[0], 'value', 'span');
          if (day === firstmessageday[0].innerText) {
              ASC.TMTalk.dom.addClass(firstmessageday[0], 'hidden');
          }
      }
      if (inTop === true && firstmessage !== null) {
        messagescontainer.insertBefore(newmessage, firstmessage);
      } else {
        messagescontainer.appendChild(newmessage);
        lastmessage = newmessage;
      }
      if (lastmessage !== null) {
        messagescontainer.scrollTop = lastmessage.offsetTop + lastmessage.offsetHeight + 100;
      }
    }
  };

  var addMessageToHistory = function (roomnode, name, date, body, isMine) {
    var
      nodes = null,
      newmessage = null,
      lastmessage = null,
      messagescontainer = null;

    nodes = ASC.TMTalk.dom.getElementsByClassName(roomnode, 'history', 'div');
    if (nodes.length > 0) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'messages', 'ul');
      if (nodes.length > 0) {
        messagescontainer = nodes[0];
      }
    }
    if (messagescontainer !== null) {
      newmessage = createMessage(name, date, body, isMine);
      newmessage.className += ' paragraph';
      messagescontainer.appendChild(newmessage);
      messagescontainer.scrollTop = newmessage.offsetTop + newmessage.offsetHeight + 100;
    }
  };

  var clarificationContacts = function (contactscontainer) {
     
    var
      node = null,
      nodes = null,
      isOdd = true;
    nodes = ASC.TMTalk.dom.getElementsByClassName(contactscontainer, 'contact', 'li');
    for (var i = 0, n = nodes.length; i < n; i++) {
      node = nodes[i];
      if (ASC.TMTalk.dom.hasClass(node, 'default')) {
        continue;
      }
      ASC.TMTalk.dom.removeClass(node, isOdd ? 'even' : 'odd');
      ASC.TMTalk.dom.addClass(node, isOdd ? 'odd' : 'even');
      isOdd = !isOdd;
    }
  };

  var init = function (smiles, mestimeout) {
    if (isInit === true) {
      return undefined;
    }
    isInit = true;
    // TODO
    if (smiles instanceof Array) {
      smilesCollection = smiles;
    }

    if (isFinite(+mestimeout)) {
      messageTimeout = +mestimeout * 1000;
    }

    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.disconnected, onClientDisconnected);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.connectingFailed, onClientDisconnected);

    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.createRoom, onCreateRoom);
    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.openRoom, onOpenRoom);
    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.closeRoom, onCloseRoom);

    ASC.TMTalk.contactsManager.bind(ASC.TMTalk.contactsManager.events.comeContact, onUpdateContact);
    ASC.TMTalk.contactsManager.bind(ASC.TMTalk.contactsManager.events.leftContact, onUpdateContact);

    ASC.TMTalk.msManager.bind(ASC.TMTalk.msManager.events.addContact, onAddContact);
    ASC.TMTalk.msManager.bind(ASC.TMTalk.msManager.events.removeContact, onRemoveContact);
    ASC.TMTalk.msManager.bind(ASC.TMTalk.msManager.events.sentMessage, onSentMessageToList);

    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.sentInvite, onSentInvite);
    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.recvInvite, onRecvInvite);
    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.comeContact, onComeContact);
    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.leftContact, onLeftContact);

    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.openHistory, onOpenHistory);
    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.closeHistory, onCloseHistory);
    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.loadHistory, onGetContactHistory);
    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.loadFilteredHistory, onGetContactFilteredHistory);
    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.sentMessageToChat, onSentMessageToChat);
    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.recvMessageFromChat, onRecvMessageFromChat);
    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.recvMessageFromConference, onRecvMessageFromConference);
    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.recvOfflineMessagesFromChat, onRecvOfflineMessagesFromChat);

    //TMTalk.bind(TMTalk.events.pageFocus, onPageFocus);
  };

  var onPageFocus = function () {
    var
      nodes = null,
      nodesInd = 0;

    nodes = ASC.TMTalk.dom.getElementsByClassName(roomlistContainer, 'room', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (ASC.TMTalk.dom.hasClass(nodes[nodesInd], 'current')) {
        showLastMessage(nodes[nodesInd]);
        break;
      }
    }
  };

  var onClientDisconnected = function () {
    var
      nodes = null,
      rooms = null,
      roomsInd = 0;

    rooms = ASC.TMTalk.dom.getElementsByClassName(roomlistContainer, 'room', 'li');
    roomsInd = rooms.length;
    while (roomsInd--) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(rooms[roomsInd], 'messages', 'div');
      if (nodes.length > 0) {
        ASC.TMTalk.dom.removeClass(nodes[0], 'loading');
      }
      nodes = ASC.TMTalk.dom.getElementsByClassName(rooms[roomsInd], 'history', 'div');
      if (nodes.length > 0) {
        ASC.TMTalk.dom.removeClass(nodes[0], 'loading');
      }
    }
  };

  var onSentMessageToChat = function (jid, name, displaydate, date, body) {
    var room = null;
    if ((room = getRoomByCid(jid)) !== null) {
      if (!lastChatMessageDates.hasOwnProperty(jid)) {
        lastChatMessageDates[jid] = { date: null, ownMessage: null };
      }
      var needParagraph = true;
      if (messageTimeout !== null && lastChatMessageDates[jid].date !== null && lastChatMessageDates[jid].ownMessage) {
        needParagraph = date.getTime() - lastChatMessageDates[jid].date.getTime() > messageTimeout;
      }
      lastChatMessageDates[jid].date = date;
      lastChatMessageDates[jid].ownMessage = true;
      addMessage(room, name, displaydate, textToHtml(body), true, needParagraph);
    }
  };

  var onRecvOfflineMessagesFromChat = function (jid, messages) {
    var room = null;
    if ((room = getRoomByCid(jid)) !== null) {
      var message = null;
      for (var i = 0, n = messages.length; i < n; i++) {
        message = messages[i];
        if (!lastChatMessageDates.hasOwnProperty(jid)) {
          lastChatMessageDates[jid] = {date : null, ownMessage : null};
        }
        lastChatMessageDates[jid].date = message.date;
        lastChatMessageDates[jid].ownMessage = false;

        addMessage(room, message.displayName, message.displayDate, textToHtml(message.body), false, true, true);
      }
    }
  };

  var onRecvMessageFromChat = function (jid, name, displaydate, date, body, isOffline) {
    if (ASC.TMTalk.messagesManager.historyLoaded(jid) === false) {
      return undefined;
    }
    var room = null;
    if ((room = getRoomByCid(jid)) !== null) {
      if (!lastChatMessageDates.hasOwnProperty(jid)) {
          lastChatMessageDates[jid] = { date: null, ownMessage: null };
      }
      var needParagraph = true;
      if (messageTimeout !== null && lastChatMessageDates[jid].date !== null && !lastChatMessageDates[jid].ownMessage) {
        needParagraph = date.getTime() - lastChatMessageDates[jid].date.getTime() > messageTimeout;
      }
      lastChatMessageDates[jid].date = date;
      lastChatMessageDates[jid].ownMessage = false;
      addMessage(room, name, displaydate, textToHtml(body), false, needParagraph);
    }
  };

  var onRecvMessageFromConference = function (roomjid, name, displaydate, date, body, isMine) {
    var room = null;
    if ((room = getRoomByCid(roomjid)) !== null) {
      if (!lastChatMessageDates.hasOwnProperty(roomjid)) {
          lastChatMessageDates[roomjid] = { date: null, ownMessage: null };
      }
      var needParagraph = true;
      if (messageTimeout !== null && lastChatMessageDates[roomjid].date !== null && lastChatMessageDates[roomjid].ownMessage === isMine) {
        needParagraph = date.getTime() - lastChatMessageDates[roomjid].date.getTime() > messageTimeout;
      }
      lastChatMessageDates[roomjid].date = date;
      lastChatMessageDates[roomjid].ownMessage = isMine;
      addMessage(room, name, displaydate, textToHtml(body), isMine, needParagraph);
    }
  };

  var onRecvInvite = function (roomjid, inviterjid, reason) {
    TMTalk.showDialog(
      'recv-invite',
      ASC.TMTalk.mucManager.getConferenceName(roomjid),
      {roomjid : roomjid, inviterjid : inviterjid, contactname : ASC.TMTalk.mucManager.getContactName(inviterjid)}
    );
  };

  var onUpdateContact = function (jid, status) {
    var
      room = null,
      contact = null,
      contacts = null,
      contactsInd = 0,
      classname = '';

    contacts = ASC.TMTalk.dom.getElementsByClassName(roomlistContainer, 'contact', 'li');
    contactsInd = contacts.length;
    while (contactsInd--) {
      if (contacts[contactsInd].getAttribute('data-cid') === jid) {
        contact = contacts[contactsInd];

        room = contact.parentNode.parentNode.parentNode;
        if (ASC.TMTalk.dom.hasClass(room, 'room') && ASC.TMTalk.dom.hasClass(room, 'mailing')) {
          classname = 'contact ' + status.className;
          if (ASC.TMTalk.dom.hasClass(contact, 'master')) {
            classname += ' master';
          }
          if (ASC.TMTalk.dom.hasClass(contact, 'odd')) {
            classname += ' odd';
          }
          if (ASC.TMTalk.dom.hasClass(contact, 'even')) {
            classname += ' even';
          }
          contact.className = classname;

          // TODO:
        }
      }
    }
  };

  var onAddContact = function (listId, contactjid) {
    var
      nodes = null,
      nodesInd = 0,
      classname = '',
      roomContainer = null,
      contactsContainer = null,
      contactnode = null;

    if ((roomContainer = getRoomByCid(listId)) !== null) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(roomContainer, 'contactlist', 'ul');
      contactsContainer = nodes.length > 0 ? nodes[0] : null;
    }

    if (contactsContainer === null) {
      return undefined;
    }

    ASC.TMTalk.dom.removeClass(roomContainer, 'start-splash');

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactsContainer, 'contact', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-cid') === contactjid) {
        contactnode = nodes[nodesInd];
        break;
      }
    }

    if (contactnode === null) {
      contactnode = createContact(contactjid);

      classname = contactnode.className;
      classname += ' ' + ASC.TMTalk.contactsManager.getContactStatus(contactjid).className;
      if (contactjid === ASC.TMTalk.connectionManager.getDomain()) {
        classname += ' master';
      }
      contactnode.className = classname;

      nodes = ASC.TMTalk.dom.getElementsByClassName(contactnode, 'title', 'div');
      if (nodes.length > 0) {
        nodes[0].className += ' contact-title';
        nodes[0].innerHTML = ASC.TMTalk.contactsManager.getContactName(contactjid);
      }

      nodes = ASC.TMTalk.dom.getElementsByClassName(roomContainer, 'size', 'div');
      if (nodes.length > 0) {
        nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'all', 'span');
        if (nodes.length > 0) {
          var all = nodes[0].innerHTML;
          all = isFinite(+all) ? +all : 0;
          nodes[0].innerHTML = ++all;
        }
      }

      contactsContainer.appendChild(contactnode);
      //clarificationContacts(contactsContainer);
    }
  };

  var onRemoveContact = function (listId, contactjid) {
    var
      nodes = null,
      nodesInd = 0,
      roomContainer = null,
      contactsContainer = null,
      contactnode = null;

    if ((roomContainer = getRoomByCid(listId)) !== null) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(roomContainer, 'contactlist', 'ul');
      contactsContainer = nodes.length > 0 ? nodes[0] : null;
    }

    if (contactsContainer === null) {
      return undefined;
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactsContainer, 'contact', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-cid') === contactjid) {
        nodes[nodesInd].parentNode.removeChild(nodes[nodesInd]);

        nodes = ASC.TMTalk.dom.getElementsByClassName(roomContainer, 'size', 'div');
        if (nodes.length > 0) {
          nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'all', 'span');
          if (nodes.length > 0) {
            var all = nodes[0].innerHTML;
            all = isFinite(+all) ? +all : 0;
            if (all > 0) {
              nodes[0].innerHTML = --all;
            }
          }
        }

        //clarificationContacts(contactsContainer);
        break;
      }
    }
  };

  var onSentMessageToList = function (listId, name, displaydate, date, body) {   
    var room = null;
    if ((room = getRoomByCid(listId)) !== null) {
      var needParagraph = true;
      addMessage(room, name, displaydate, textToHtml(body), true, true);
    }
  };

  var onSentInvite = function (roomjid, contactjid) {
    var
      nodes = null,
      nodesInd = 0,
      contactname = '',
      roomContainer = null,
      contactsContainer = null,
      contactnode = null;

    if ((roomContainer = getRoomByCid(roomjid)) !== null) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(roomContainer, 'contactlist', 'ul');
      contactsContainer = nodes.length > 0 ? nodes[0] : null;
    }

    if (contactsContainer === null || ASC.TMTalk.mucManager.getContact(roomjid, contactjid) !== null) {
      return undefined;
    }

    ASC.TMTalk.dom.removeClass(roomContainer, 'start-splash');

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactsContainer, 'contact', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-cid') === contactjid) {
        contactnode = nodes[nodesInd];
        break;
      }
    }

    if (contactnode === null) {
      contactname = ASC.TMTalk.contactsManager.getContactName(contactjid);
      contactnode = createContact(contactjid);
      contactnode.className += ' invited';

      nodes = ASC.TMTalk.dom.getElementsByClassName(contactnode, 'title', 'div');
      if (nodes.length > 0) {
        nodes[0].className += ' contact-title';
        nodes[0].innerHTML = contactname;
      }

      contactsContainer.appendChild(contactnode);
      //clarificationContacts(contactsContainer);
    }

    var timeout = contactnode.getAttribute('data-inviteid');
    timeout = isFinite(+timeout) ? +timeout : null;
    if (timeout) {
      clearTimeout(timeout);
    }
    timeout = setTimeout(GetOnRottenInvite(roomjid, contactjid), inviteTimeout * 1000);
    contactnode.setAttribute('data-inviteid', timeout);
  };

  var onRottenInvite = function (roomjid, contactjid) {
    var
      nodes = null,
      nodesInd = 0,
      roomContainer = null,
      contactsContainer = null;

    if ((roomContainer = getRoomByCid(roomjid)) !== null) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(roomContainer, 'contactlist', 'ul');
      contactsContainer = nodes.length > 0 ? nodes[0] : null;

      if (contactsContainer === null) {
        return undefined;
      }

      nodes = ASC.TMTalk.dom.getElementsByClassName(contactsContainer, 'contact', 'li');
      nodesInd = nodes.length;
      while (nodesInd--) {
        if (nodes[nodesInd].getAttribute('data-cid') === contactjid) {
          if (ASC.TMTalk.dom.hasClass(nodes[nodesInd], 'invited')) {
            nodes[nodesInd].parentNode.removeChild(nodes[nodesInd]);
            //clarificationContacts(contactsContainer);
          }
          break;
        }
      }
    }
  };

  var onComeContact = function (roomjid, contact, isMe) {
    var
      nodes = null,
      nodesInd = 0,
      roomContainer = null,
      contactsContainer = null,
      contactnode = null,
      contactjid = contact.realjid.substring(0, contact.realjid.indexOf('/'));

    if ((roomContainer = getRoomByCid(roomjid)) !== null) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(roomContainer, 'contactlist', 'ul');
      contactsContainer = nodes.length > 0 ? nodes[0] : null;
    }

    if (contactsContainer === null) {
      return undefined;
    }

    if (isMe === false) {
      ASC.TMTalk.dom.removeClass(roomContainer, 'start-splash');
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactsContainer, 'contact', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-cid') === contactjid && ASC.TMTalk.dom.hasClass(nodes[nodesInd], 'invited')) {
        contactnode = nodes[nodesInd];
        contactnode.setAttribute('data-cid', contact.jid);
        ASC.TMTalk.dom.removeClass(contactnode, 'invited');

        var timeout = contactnode.getAttribute('data-inviteid');
        timeout = isFinite(+timeout) ? +timeout : null;
        if (timeout) {
          clearTimeout(timeout);
          contactnode.removeAttribute('data-inviteid');
        }
        break;
      }
      if (nodes[nodesInd].getAttribute('data-cid') === contact.jid) {
        return undefined;
      }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(roomContainer, 'size', 'div');
    if (nodes.length > 0) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'all', 'span');
      if (nodes.length > 0) {
        var all = nodes[0].innerHTML;
        all = isFinite(+all) ? +all : 0;
        nodes[0].innerHTML = ++all;
      }
    }

    if (contactnode === null) {
      contactnode = createContact(contact.jid);
      if (isMe === true) {
        contactnode.className += ' me'
      }
      nodes = ASC.TMTalk.dom.getElementsByClassName(contactnode, 'title', 'div');
      if (nodes.length > 0) {
        nodes[0].className += ' contact-title';
        nodes[0].innerHTML = ASC.TMTalk.contactsManager.getContactName(contact.realjid);
      }

      contactsContainer.appendChild(contactnode);
      //clarificationContacts(contactsContainer);
    }
  };

  var onLeftContact = function (roomjid, contact) {
    var
      nodes = null,
      nodesInd = 0,
      roomContainer = null,
      contactsContainer = null,
      contactnode = null;

    if ((roomContainer = getRoomByCid(roomjid)) !== null) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(roomContainer, 'contactlist', 'ul');
      contactsContainer = nodes.length > 0 ? nodes[0] : null;
    }

    if (contactsContainer === null) {
      return undefined;
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactsContainer, 'contact', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-cid') === contact.jid) {
        contactnode = nodes[nodesInd];
        nodes = ASC.TMTalk.dom.getElementsByClassName(roomContainer, 'size', 'div');
        if (nodes.length > 0) {
          nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'all', 'span');
          if (nodes.length > 0) {
            var all = nodes[0].innerHTML;
            all = isFinite(+all) ? +all : 0;
            if (all > 0) {
              nodes[0].innerHTML = --all;
            }
          }
        }
        contactnode.parentNode.removeChild(contactnode);
        break;
      }
    }
    //clarificationContacts(contactsContainer);
  };

  var onOpenHistory = function (jid) {
    
    //jq("#cha").tlCombobox();
      
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (currentRoomData.type === 'chat' && currentRoomData.id === jid) {
        ASC.TMTalk.dom.addClass(roomlistContainer, 'history');
                

      var room = getRoomByCid(jid);
      if (room !== null) {
        var
          nodes = null,
          messagescontainer = null;

        nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'search-value', 'input');
        if (nodes.length > 0) {
          nodes[0].value = '';
        }

        //nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'history', 'div');
        nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'messages', 'div');
        if (nodes.length > 0) {
          nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'messages', 'ul');
          if (nodes.length > 0) {
            messagescontainer = nodes[0];
          }
        }
        if (messagescontainer !== null) {
          ASC.TMTalk.roomsContainer.resetHistorySearch(room, ASC.TMTalk.dom.getElementsByClassName(messagescontainer, 'message', 'li'));
        }
      }
      
      var cha = ASC.TMTalk.dom.getElementsByClassName(ASC.TMTalk.dom.getElementsByClassName(room, 'filtering-panel', 'div')[0], 'cha', 'div');
      var filterId = cha[0].firstElementChild.getAttribute('data-value');
      if (filterId) 
        getFilteringHistory(jid, filterId)
      else 
        getFilteringHistory(jid, 0); // 0 - last day

      $(window).resize();
      
    }
    
  };

  var onCloseHistory = function (jid) {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (currentRoomData.type === 'chat' && currentRoomData.id === jid) {
      ASC.TMTalk.dom.removeClass(roomlistContainer, 'history');
      $(window).resize();
      showLastMessage(null);
    }
  };

  var onGetContactHistory = function (jid, messages, newmessagescount, removeOld, searchText) {
    var
      node = null,
      nodes = null,
      nodesInd = 0,
      room = null,
      message = null,
      messagesInd = 0;
    if ((room = getRoomByCid(jid)) !== null) {
      //nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'message', 'li');
      //nodesInd = nodes.length;
      //while (nodesInd--) {
      //  node = nodes[nodesInd];
      //  if (ASC.TMTalk.dom.hasClass(node, 'default')) {
	    //    continue;
	    //  }
	    //  node.parentNode.removeChild(node);
      //}
        var messagescontainer;
      nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'messages', 'div');
      if (nodes.length > 0) {
          ASC.TMTalk.dom.removeClass(nodes[0], 'loading');
          nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'messages', 'ul');
          if (nodes.length > 0) {
              messagescontainer = nodes[0];
          }
      }

        if (removeOld) {
            var oldMessages = ASC.TMTalk.dom.getElementsByClassName(room, 'message paragraph', 'li');
            for (var i = 0; i < oldMessages.length; i++) {
                oldMessages[i].remove();
            }
        }

      messagesInd = messages.length;
      var currentMessageCount = jQuery('div#talkRoomsContainer ul.rooms li.room.current ul.messages:first').find('li').not('.default').length;
      while (messagesInd--) {
        message = messages[messagesInd];
        addMessage(room, message.displayName, message.displayDate, textToHtml(message.body), message.isOwn, true, newmessagescount-- > 0, true);
      }
      if (messages.length != 0 && currentMessageCount > 0) {
          
          var container = jQuery('div#talkRoomsContainer ul.rooms li.room[data-roomcid="' + jid + '"] ul.messages');
          var element = jQuery('div#talkRoomsContainer li.room[data-roomcid="' + jid + '"] div.messages ul.messages li:eq(5)');
          var correction = jQuery('div#talkRoomsContainer li.room.current').hasClass('conference') ? 25 : 50;

          var top = 0;
          if (container.length > 0 && element.length > 0 && element.position() != undefined) {
              top = element.position().top + container.offset().top + container.scrollTop() - correction;
          }
          jQuery('div#talkRoomsContainer ul.rooms li.room[data-roomcid="' + jid + '"] ul.messages').scrollTo(top);
      } else if (currentMessageCount != 0) {
          jQuery('div#talkRoomsContainer ul.rooms li.room[data-roomcid="' + jid + '"] ul.messages').scrollTo(0);
      }

        if (searchText) {
            historySearch(searchText, room, ASC.TMTalk.dom.getElementsByClassName(messagescontainer, 'message', 'li'));
        }
    }
  };

  var onGetContactFilteredHistory = function (jid, messages) {
    var
      room = null,
      node = null,
      nodes = null,
      nodesInd = 0,
      message = null,
      newmessage = null,
      historyfragment = null,
      messagescontainer = null;
    if ((room = getRoomByCid(jid)) !== null) {
        //nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'history', 'div');
        nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'messages', 'div');
      if (nodes.length > 0) {
        ASC.TMTalk.dom.removeClass(nodes[0], 'loading');
        nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'messages', 'ul');
        if (nodes.length > 0) {
          messagescontainer = nodes[0];
        }
      }
      if (messagescontainer !== null) {
          if (jQuery(messagescontainer).find('li:not(.default):first').length > 0) {
              var firstMessageTopPosition = jQuery(messagescontainer).find('li:not(.default):first').position().top;
              var countMessages = jQuery(messagescontainer).find('li:not(.default)').length ?
                  jQuery(messagescontainer).find('li:not(.default)').length : 0;
          }
      }
      historyfragment = document.createDocumentFragment();
        var lastmessagedate = null,
            currentmessagedate= null;
      for (var i = 0, n = messages.length - countMessages; i < n; i++) {
        message = messages[i];
        newmessage = createMessage(message.displayName, message.displayDate, textToHtml(message.body), message.isOwn);
        newmessage.className += ' paragraph';
        $(newmessage).find('div.head').hide();

        currentmessagedate = [message.displayDate.split(' ')[0],message.displayDate.split(' ')[1]].join(" ");
        if (lastmessagedate != null && lastmessagedate === currentmessagedate) {
            $(newmessage).find('span.daysplit').hide();
        }
        lastmessagedate = [message.displayDate.split(' ')[0],message.displayDate.split(' ')[1]].join(" ");
          
        historyfragment.appendChild(newmessage);
        //addMessageToHistory(room, message.displayName, message.displayDate, textToHtml(message.body), message.isOwn);
      }

      if (messagescontainer !== null) {
        nodes = ASC.TMTalk.dom.getElementsByClassName(messagescontainer, 'message', 'li');
        nodesInd = nodes.length;
        for (var i = 0; i < nodesInd; i++)
        {
            node = nodes[i];
            if (ASC.TMTalk.dom.hasClass(node, 'default')) {
                continue;
            }
            if (ASC.TMTalk.dom.getElementsByClassName(node, 'daysplit', 'span').length > 0) {
                
                var currentDate = new Date;
                var today = [currentDate.getDate(),(currentDate.getMonth() + 1),currentDate.getFullYear()].join('.');
                currentDate.setDate(currentDate.getDate() - 1);
                var yesterday = currentDate;
                yesterday = [yesterday.getDate(), (yesterday.getMonth() + 1), yesterday.getFullYear()].join('.');
                if (lastmessagedate) {
                    var day = lastmessagedate.split(' ').length > 1 ? lastmessagedate.split(' ')[1]:'';
                    var lastMessageDate = ASC.TMTalk.dom.getElementsByClassName(node, 'daysplit', 'span')[0].textContent;

                    if ((lastmessagedate.indexOf(lastMessageDate) + 1) && day === today || day === yesterday) {
                        $(ASC.TMTalk.dom.getElementsByClassName(node, 'daysplit', 'span')[0]).hide();
                    }
                    break;
                }
                
            }
        }
        messagescontainer.insertBefore(historyfragment, nodes[0]);
        if (newmessage !== null) {
            messagescontainer.scrollTop = newmessage.offsetTop + newmessage.offsetHeight - firstMessageTopPosition;
        }
        nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'search-value', 'input');
        if (nodes.length > 0) {
            var searchstr = nodes[0].value;
            if (searchstr) {
                if (messagescontainer !== null) {
                   ASC.TMTalk.roomsContainer.historySearch(searchstr, room, ASC.TMTalk.dom.getElementsByClassName(messagescontainer, 'message', 'li'));
                }
            }
        }
      }
    }        
  };

  var onCreateRoom = function (roomId, data) {
    var
      nodes = null,
      newroom = null;

    if (getRoomById(roomId) !== null) {
      return undefined;
    }

    newroom = createRoom(roomId);

    nodes = ASC.TMTalk.dom.getElementsByClassName(roomlistContainer, 'rooms', 'ul');
    if (nodes.length > 0) {
      nodes[0].appendChild(newroom);
    }

    switch (data.type) {
      case 'chat' :
        newroom.setAttribute('data-roomcid', data.id);

        var classname = newroom.className;
        classname += ' chat';
        newroom.className = classname;

        var nodes = ASC.TMTalk.dom.getElementsByClassName(newroom, 'messages', 'div');
        if (nodes.length > 0) {
          ASC.TMTalk.dom.addClass(nodes[0], 'loading');
        }

        ASC.TMTalk.messagesManager.getHistory(data.id);
        break;
      case 'mailing' :
        newroom.setAttribute('data-roomcid', data.id);

        var classname = newroom.className;
        classname += ' mailing start-splash';
        newroom.className = classname;
        break;
        case 'conference':
        setTimeout(function () {
            if (ASC.TMTalk.dom.hasClass(newroom, 'start-splash')) {
                var $currentRoom = jQuery('div#talkRoomsContainer ul.rooms li.room.current');
                $currentRoom.removeClass('minimized');
            }
        }, 500);
        newroom.setAttribute('data-roomcid', data.id);

        var classname = newroom.className;
        classname += ' conference ' + data.classname + ' minimized start-splash';
        newroom.className = classname;
        break;
    }
  };

  var onOpenRoom = function (roomId, data, inBackground) {
    var
      node = null,
      nodes = null,
      nodesInd = 0,
      currentroom = null;

    if (inBackground !== true) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(roomlistContainer, 'room', 'li');
      nodesInd = nodes.length;
      while (nodesInd--) {
        node = nodes[nodesInd];
        if (node.getAttribute('data-roomid') === roomId) {
          currentroom = node;
          ASC.TMTalk.dom.addClass(node, 'current');
        } else {
          ASC.TMTalk.dom.removeClass(node, 'current');
        }
      }
    }

    if (currentroom === null) {
      return undefined;
    }

    if (inBackground !== true) {
      switch (data.type) {
        case 'chat' :
          ASC.TMTalk.dom.removeClass(roomlistContainer, 'multichat');
          ASC.TMTalk.dom.removeClass(roomlistContainer, 'minimized');
          ASC.TMTalk.dom.addClass(roomlistContainer, 'chat');
          break;
        case 'mailing' :
          ASC.TMTalk.dom.removeClass(roomlistContainer, 'chat');
          ASC.TMTalk.dom.addClass(roomlistContainer, 'multichat');
          if (data.minimized === true) {
            ASC.TMTalk.dom.addClass(roomlistContainer, 'minimized');
          } else {
            ASC.TMTalk.dom.removeClass(roomlistContainer, 'minimized');
          }
          break;
        case 'conference' :
          ASC.TMTalk.dom.removeClass(roomlistContainer, 'chat');
          ASC.TMTalk.dom.addClass(roomlistContainer, 'multichat');
          if (data.minimized === true) {
            ASC.TMTalk.dom.addClass(roomlistContainer, 'minimized');
          } else {
            ASC.TMTalk.dom.removeClass(roomlistContainer, 'minimized');
          }
          break;
      }
      ASC.TMTalk.dom.removeClass(roomlistContainer, 'history');
      ASC.TMTalk.dom.removeClass(roomlistContainer, 'searchmessage');
      //TMTalk.hideAllDialogs();
      showLastMessage(currentroom);
    }
    $(window).resize();
  };

  var onCloseRoom = function (roomId, data) {
    var
      isCurrent = false,
      currentroom = null;

    if ((currentroom = getRoomById(roomId)) === null) {
      return undefined;
    }

    isCurrent = ASC.TMTalk.dom.hasClass(currentroom, 'current');

    switch (data.type) {
      case 'chat' :
        if (isCurrent === true) {
          ASC.TMTalk.dom.removeClass(roomlistContainer, 'chat');
          ASC.TMTalk.dom.removeClass(roomlistContainer, 'history');
        }
        break;
      case 'mailing' :
        if (isCurrent === true) {
          ASC.TMTalk.dom.removeClass(roomlistContainer, 'multichat');
          ASC.TMTalk.dom.removeClass(roomlistContainer, 'minimized');
        }
        break;
      case 'conference' :
        var
          timeout = 0,
          nodes = null,
          nodesInd = 0,
          roomContainer = null;

        if ((roomContainer = getRoomByCid(data.id)) !== null) {
          nodes = ASC.TMTalk.dom.getElementsByClassName(roomContainer, 'contact invited', 'li');
          nodesInd = nodes.length;
          while (nodesInd--) {
            timeout = nodes[nodesInd].getAttribute('data-inviteid');
            timeout = isFinite(+timeout) ? +timeout : null;
            if (timeout) {
              clearTimeout(timeout);
            }
            nodes[nodesInd].removeAttribute('data-inviteid');
          }
        }

        if (isCurrent === true) {
          ASC.TMTalk.dom.removeClass(roomlistContainer, 'multichat');
          ASC.TMTalk.dom.removeClass(roomlistContainer, 'minimized');
        }
        break;
    }
    currentroom.parentNode.removeChild(currentroom);

    TMTalk.hideAllDialogs();
    $(window).resize();
  };

  var showLastMessage = function (roomnode) {
    var
      nodes = null,
      lastmessage = null,
      messagescontainer = null;

    if (roomnode === null) {
      var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
      if (currentRoomData !== null) {
        var nodes = ASC.TMTalk.dom.getElementsByClassName(roomlistContainer, 'room current', 'li');
        if (nodes.length > 0) {
          roomnode = nodes[0];
        }
      }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(roomnode, 'messages', 'ul');
    if (nodes.length > 0) {
      messagescontainer = nodes[0];
      lastmessage = ASC.TMTalk.dom.lastElementChild(messagescontainer);
      if (lastmessage !== null) {
        messagescontainer.scrollTop = lastmessage.offsetTop + lastmessage.offsetHeight + 100;
      }
    }
  };

  var getFilteringHistory = function (cid, filterid) {
    filterid = +filterid;
    switch (+filterid) {
      case 0 :
      case 1 :
      case 2 :
      case 3 :
        break;
      default :
        return undefined;
    }
    var room = null;
    if ((room = getRoomByCid(cid)) === null) {
      return undefined;
    }
    var
      name = '',
      title = '',
      nodes = null,
      nodesInd = 0,
      selector = null,
      seloption = null;
      //nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'history', 'div');
    nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'messages', 'div');
    if (nodes.length > 0) {
      ASC.TMTalk.dom.addClass(nodes[0], 'loading');
    }
    nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'filtering-panel', 'div');
    if (nodes.length > 0) {
      ASC.TMTalk.dom.removeClass(nodes[0], 'found');
      nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'search-value', 'input');
      if (nodes.length > 0) {
       // nodes[0].value = '';
      }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'filtering-menu', 'div');
    if (nodes.length > 0) {
      selector = nodes[0];
    }
    if (selector !== null) {
      filterid += '';
      selector.setAttribute('data-value', filterid);
      nodes = ASC.TMTalk.dom.getElementsByClassName(selector, 'filter-option', 'li');
      nodesInd = nodes.length;
      while (nodesInd--) {
        if (filterid === nodes[nodesInd].getAttribute('data-id')) {
          seloption = nodes[nodesInd];
          break;
        }
      }
      if (seloption !== null) {
        nodes = ASC.TMTalk.dom.getElementsByClassName(selector, 'filter-value', 'div');
        if (nodes.length > 0) {
          nodes[0].setAttribute('title', seloption.getAttribute('title'));
          nodes[0].innerHTML = seloption.innerHTML;
        }
      }
    }
    var from = null, to = new Date();
    to = new Date(Date.UTC(to.getFullYear(), to.getMonth(), to.getDate()));
    to.setDate(to.getDate() + 1);
    from = new Date(to.getTime());
    switch (+filterid) {
      // last day
      case 0 :
        from.setDate(from.getDate() - 1);
        break;
        // last week
      case 1 :
        from.setDate(from.getDate() - 7);
        break;
        // last month
      case 2 :
        from.setMonth(from.getMonth() - 1);
        break;
        // all
      case 3 :
        from = new Date(0);
        break;
    }
    ASC.TMTalk.messagesManager.getHistoryByFilter(cid, from, to);
  };

  var hideHistoryFilter = function (roomId) {
    var room = null;
    if ((room = getRoomById(roomId)) !== null) {
      var nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'filtering-menu', 'div');
      if (nodes.length > 0) {
        ASC.TMTalk.dom.removeClass(nodes[0], 'open');
      }
    }
  };

  var highlightText = function (str, o) {
    var strIndex = -1, c = '', oldhtml = o.innerHTML, newhtml = '', isText = true, text = '', cltext = '', strlen = str.length, lcstr = str.toLowerCase();
    for (var i = 0, n = oldhtml.length; i < n; i++) {
      c = oldhtml.charAt(i);
      switch (c) {
        case '<' :
          isText = false;
          if (text.length > 0) {
            cltext = text.toLowerCase();
            strIndex = 0;
            while ((strIndex = cltext.indexOf(lcstr)) !== -1) {
              newhtml += text.substring(0, strIndex) + '<span class="hl-text">' + text.substring(strIndex, strIndex + lcstr.length) + '</span>';
              text = text.substring(strIndex + strlen);
              cltext = cltext.substring(strIndex + strlen);
            }
            newhtml += text;
            //newhtml += text.replace(restr, '<span class="hl-text">$&</span>');
            text = '';
          }
          newhtml += c;
          break;
        case '>' :
          newhtml += c;
          isText = true;
          break;
        default :
          if (isText === true) {
            text += c;
            break;
          }
          newhtml += c;
          break;
      }
    }
    if (text.length > 0) {
      cltext = text.toLowerCase();
      strIndex = 0;
      
      while ((strIndex = cltext.indexOf(Encoder.htmlEncode(lcstr))) !== -1) {
          var endIndex = Encoder.htmlEncode(lcstr).length;
          newhtml += text.substring(0, strIndex) + '<span class="hl-text">' + text.substring(strIndex, strIndex + endIndex) + '</span>';
          text = text.substring(strIndex + Encoder.htmlEncode(str).length);
          cltext = cltext.substring(strIndex + Encoder.htmlEncode(str).length);
      }
      newhtml += text;
      //newhtml += text.replace(restr, '<span class="hl-text">$&</span>');
    }

    if (oldhtml !== newhtml) {
      o.innerHTML = newhtml;
      return true;
    }
    return false;
  };

  var clearHighlightText = function (o) {
    o.innerHTML = o.innerHTML.replace(/<span class=['"]*hl-text["']*>(.+?)<\/span>/gi, '$1');
  };

  var historySearch = function (str, room, messages) {
    var
      body = null,
      nodes = null,
      message = null,
      messagesInd = 0,
      foundmessagecount = 0,
      foundmessages = null, 
      currentfoundmessage = null, 
      lastfoundmessage = null;

    if (typeof str !== 'string' || str.length === 0 || messages.length === 0) {
      return undefined;
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'filtering-panel', 'div');
    if (nodes.length > 0) {
      ASC.TMTalk.dom.addClass(nodes[0], 'found');
    }

    messagesInd = messages.length;
    while (messagesInd--) {
      message = messages[messagesInd];
      if (ASC.TMTalk.dom.hasClass(message, 'default')) {
        continue;
      }

      ASC.TMTalk.dom.removeClass(message, 'found');
      nodes = ASC.TMTalk.dom.getElementsByClassName(message, 'body', 'div');
      if (nodes.length > 0) {
        body = nodes[0];
        clearHighlightText(body);
        if (highlightText(str, body)) {
          if (lastfoundmessage === null) {
            lastfoundmessage = message;
          }
          foundmessagecount++;
          ASC.TMTalk.dom.addClass(message, 'found');
        }
      }
    }
    nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'messages', 'div');
    if (nodes.length > 0) {
        ASC.TMTalk.dom.removeClass(nodes[0], 'loading');
    }
    nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'countfoundmessage', 'span');
    if (nodes.length > 0) {
        foundmessages = nodes[0];
        $(foundmessages).html(foundmessagecount);
        nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'currentfoundmessage', 'span');
        if (nodes.length > 0) {
            currentfoundmessage = nodes[0];
            if (foundmessagecount !== 0) $(currentfoundmessage).html("1");
            else $(currentfoundmessage).html("0");
        }
    }
    if (lastfoundmessage !== null) {
      var messagescontainer = lastfoundmessage.parentNode;
      messagescontainer.scrollTop = lastfoundmessage.offsetTop - (messagescontainer.offsetHeight - lastfoundmessage.offsetHeight) + lastfoundmessage.scrollHeight;
    }
  };
    
  function minimizingUserList() {

      var room = jQuery('div#talkRoomsContainer ul.rooms li.room.current')[0];
      if (ASC.TMTalk.dom.hasClass(room, 'room') && (ASC.TMTalk.dom.hasClass(room, 'conference') || ASC.TMTalk.dom.hasClass(room, 'mailing'))) {

          ASC.TMTalk.dom.toggleClass(room, 'minimized');
          var roomid = room.getAttribute('data-roomid'),
              roomcid = room.getAttribute('data-roomcid'),
              $messagesContainer = jQuery('div#talkRoomsContainer ul.rooms li.room.current div.messages ul.messages'),
              messagesContainerScrollTop = $messagesContainer.scrollTop();
          isMinimized = ASC.TMTalk.dom.hasClass(room, 'minimized');
          if (roomid) {
              ASC.TMTalk.roomsManager.updateRoomData(roomid, { minimized: isMinimized });
          }
          if (!isMinimized) {
              ASC.TMTalk.dom.removeClass('talkRoomsContainer', 'minimized');
              ASC.TMTalk.tabsContainer.setStatus('hint', { hint: ASC.TMTalk.Resources.HintSendInvite }, 2);
              var roomSeparator = ASC.TMTalk.dom.getElementsByClassName(room, 'room-separator');
              if (roomSeparator.length > 0) {
                  ASC.TMTalk.dom.removeClass(roomSeparator[0], 'hidden');
              }
              var subPanel = ASC.TMTalk.dom.getElementsByClassName(room, 'sub-panel noBorder')[0];
              ASC.TMTalk.dom.removeClass(subPanel, 'noBorder');
              ASC.TMTalk.dom.addClass(subPanel, 'border');
          } else {
              ASC.TMTalk.dom.addClass('talkRoomsContainer', 'minimized');
              if (ASC.TMTalk.dom.hasClass(room, 'conference') && roomcid) {
                  ASC.TMTalk.tabsContainer.resetStatus('conference', { confsubject: ASC.TMTalk.mucManager.getConferenceSubject(roomcid) }, 1);
              }
              //ASC.TMTalk.dom.addClass(target.parentNode, 'hidden');
              if ((ASC.TMTalk.dom.hasClass(room, 'conference') && ASC.TMTalk.dom.getElementsByClassName(room, 'contact').length <= 2)
                  || (ASC.TMTalk.dom.hasClass(room, 'mailing') && ASC.TMTalk.dom.getElementsByClassName(room, 'contact').length <= 1)) {

                  //ASC.TMTalk.dom.removeClass(room, 'start-splash');

              }
              var subPanel = ASC.TMTalk.dom.getElementsByClassName(room, 'sub-panel border')[0];
              ASC.TMTalk.dom.removeClass(subPanel, 'border');
              ASC.TMTalk.dom.addClass(subPanel, 'noBorder');
          }
          $(window).resize();
          ASC.TMTalk.roomsContainer.showLastMessage(null);
          $messagesContainer.scrollTop(messagesContainerScrollTop);
      }
  };

  var resetHistorySearch = function (room, messages) {
    var
      nodes = null,
      message = null,
      messagesInd = 0;

    if (messages.length === 0) {
      return undefined;
    }
    nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'currentfoundmessage', 'span');
    if (nodes.length > 0) {
        $(nodes[0]).html(0);
    }
    nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'countfoundmessage', 'span');
    if (nodes.length > 0) {
        $(nodes[0]).html(0);
    }
    nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'filtering-panel', 'div');
    if (nodes.length > 0) {
      ASC.TMTalk.dom.removeClass(nodes[0], 'found');
    }

    messagesInd = messages.length;
    while (messagesInd--) {
      message = messages[messagesInd];
      if (ASC.TMTalk.dom.hasClass(message, 'default')) {
        continue;
      }

      ASC.TMTalk.dom.removeClass(message, 'found');
      nodes = ASC.TMTalk.dom.getElementsByClassName(message, 'body', 'div');
      if (nodes.length > 0) {
        clearHighlightText(nodes[0]);
      }
    }
  };

  return {
    init  : init,
    nodes : setNodes,

    getFilteringHistory : getFilteringHistory,
    hideHistoryFilter   : hideHistoryFilter,

    resetHistorySearch  : resetHistorySearch,
    historySearch       : historySearch,
    
    minimizingUserList  :minimizingUserList,
    
    showLastMessage : showLastMessage,
    onRottenInvite: onRottenInvite,

    getRoomByCid: getRoomByCid
  };
})(jQuery);

(function ($) {
  $(function () {
    ASC.TMTalk.roomsContainer.nodes();

    if ($.browser.safari) {
      $('#talkRoomsContainer').mousedown(function (evt) {
        var target = evt.target;
        if (ASC.TMTalk.dom.hasClass(target.parentNode, 'button-container') || ASC.TMTalk.dom.hasClass(target, 'filter-value') || ASC.TMTalk.dom.hasClass(target, 'filter-option')) {
          if (window.getSelection) {
            window.getSelection().removeAllRanges();
          }
          return false;
        }
      });
    }
    
    $('#closeHistory')
      .on('click',function (evt) {
          var room = $('.room.chat.current');
          var roomcid = room.attr('data-roomcid');
          if (roomcid) {
              ASC.TMTalk.messagesManager.closeHistory(roomcid);
          }
          //var target = evt.target;
          //var room = target.parentNode.parentNode.parentNode;
          //var room = getElementByCl
         // if (ASC.TMTalk.dom.hasClass(room, 'room') && ASC.TMTalk.dom.hasClass(room, 'chat')) {
             // var roomcid = room.getAttribute('data-roomcid');
              //if (roomcid) {
                //  ASC.TMTalk.messagesManager.closeHistory(roomcid);
             // }
         // }
      })


    $('#talkRoomsContainer')
      .keydown(function (evt) {
        if (evt.target.tagName.toLowerCase() === 'input') {
          evt.originalEvent.stopPropagation ? evt.originalEvent.stopPropagation() : evt.originalEvent.cancelBubble = true;
        }
      })
      .keyup(function(evt) {
        switch (evt.keyCode) {
            case 27:
                if (evt.target.tagName.toLowerCase() === 'input' && ASC.TMTalk.dom.hasClass(evt.target, 'search-value')) {
                    if (evt.target.value) {
                        ASC.TMTalk.roomsManager.clearSearch(evt.target.parentNode.parentNode.parentNode.parentNode);
                    } else {
                        var filteringPanel = evt.target.parentNode.parentNode.parentNode;
                        if ($(filteringPanel).hasClass('show')) {
                            $(filteringPanel).removeClass('show');
                        }
                            
                    }
                }
                break;
        }
      })
      .keypress(function (evt) {
        switch (evt.keyCode) {
            case 13:
            if (evt.target.tagName.toLowerCase() === 'input' && ASC.TMTalk.dom.hasClass(evt.target, 'search-value')) {
               
              var room = evt.target.parentNode.parentNode.parentNode.parentNode;
              var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
              var jid = currentRoomData.id;
              var messageCount = 0;
              if (ASC.TMTalk.dom.hasClass(room, 'room')) {
                var searchstr = evt.target.value;
                if (searchstr) {
                  var
                    nodes = null,
                    messagescontainer = null;
                  //nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'history', 'div');
                  nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'messages', 'div');
                  if (nodes.length > 0) {
                    nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'messages', 'ul');
                    if (nodes.length > 0) {
                      messagescontainer = nodes[0];
                      nodes = ASC.TMTalk.dom.getElementsByClassName(messagescontainer, 'message', 'li');
                      messageCount = nodes.length;
                    }
                  }
                  if (messagescontainer !== null) {
                      if (!currentRoomData.fullhistory && messageCount > 20) {
                          ASC.TMTalk.roomsContainer.getFilteringHistory(jid, 3);
                          currentRoomData.fullhistory = true;
                      } else {
                          ASC.TMTalk.roomsContainer.historySearch(searchstr, room, ASC.TMTalk.dom.getElementsByClassName(messagescontainer, 'message', 'li'));
                      }
                  }
                }
              }
            }
            return false;
        }
      })
      .click(function (evt) {
          var target = evt.target;
        if (ASC.TMTalk.dom.hasClass(target, 'button-talk') && ASC.TMTalk.dom.hasClass(target, 'remove-member')) {
          var contact = target.parentNode.parentNode;
          var invited = ASC.TMTalk.dom.hasClass(contact, 'invited') ? true : false;
          if (ASC.TMTalk.dom.hasClass(contact, 'contact')) {
            var room = contact.parentNode.parentNode.parentNode;
            var roomCid = room.getAttribute('data-roomcid');
            if (ASC.TMTalk.dom.hasClass(room, 'room')) {
              var jid = contact.getAttribute('data-cid');
              if (ASC.TMTalk.dom.hasClass(room, 'conference')) {
                var contactjid = contact.getAttribute('data-cid');
                if (contactjid) {
                  TMTalk.showDialog('kick-occupant', ASC.TMTalk.mucManager.getContactName(jid), { contactjid: contactjid }, { invited: invited, roomCid: roomCid});
                }
              } else if (ASC.TMTalk.dom.hasClass(room, 'mailing')) {
                var listId = room.getAttribute('data-roomcid');
                if (listId && jid) {
                    ASC.TMTalk.msManager.removeContact(listId, jid);
                    ASC.TMTalk.msManager.storeMailingLists();
                }
              }
            }
          }
        } else if (ASC.TMTalk.dom.hasClass(target, 'button-talk') && ASC.TMTalk.dom.hasClass(target, 'toggle-minimizing')) {
            
        } else if (ASC.TMTalk.dom.hasClass(target, 'openUserList') && ASC.TMTalk.dom.hasClass(target, 'with-entity-menu')) {
            ASC.TMTalk.roomsContainer.minimizingUserList();
        } else if (ASC.TMTalk.dom.hasClass(target, 'button-talk') && ASC.TMTalk.dom.hasClass(target, 'remove-mailing') || (ASC.TMTalk.dom.hasClass(target, 'removeMailing') && ASC.TMTalk.dom.hasClass(target, 'with-entity-menu'))) {
            var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
            if (currentRoomData !== null && currentRoomData.type === 'mailing') {
                var listId = currentRoomData.id;
                if (listId) {
                    ASC.TMTalk.msManager.removeList(listId);
                }
            };
        } else if (ASC.TMTalk.dom.hasClass(target, 'button-talk') && ASC.TMTalk.dom.hasClass(target, 'remove-conference') || (ASC.TMTalk.dom.hasClass(target, 'removeRoom') && ASC.TMTalk.dom.hasClass(target, 'with-entity-menu'))) {
            var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
            if (currentRoomData !== null && currentRoomData.type === 'conference' && currentRoomData.affiliation === 'owner') {
                var roomjid = currentRoomData.id;
                if (roomjid) {
                    TMTalk.showDialog('remove-room', ASC.TMTalk.mucManager.getConferenceName(roomjid), { roomjid: roomjid });
                }
            }
        } else if (ASC.TMTalk.dom.hasClass(target, 'button-talk') && ASC.TMTalk.dom.hasClass(target, 'close-history')) {
            var room = target.parentNode.parentNode.parentNode;
            if (ASC.TMTalk.dom.hasClass(room, 'room') && ASC.TMTalk.dom.hasClass(room, 'chat')) {
                var roomcid = room.getAttribute('data-roomcid');
                if (roomcid) {
                    ASC.TMTalk.messagesManager.closeHistory(roomcid);
                }
            }
        } else if (ASC.TMTalk.dom.hasClass(target, 'filter-value')) {
            var selector = target.parentNode;
            if (ASC.TMTalk.dom.hasClass(selector, 'custom-select')) {
                var room = selector.parentNode.parentNode;
                if (ASC.TMTalk.dom.hasClass(room, 'room')) {
                    ASC.TMTalk.dom.toggleClass(selector, 'open');
                    if (ASC.TMTalk.dom.hasClass(selector, 'open')) {
                        $(document).one('click', (function(roomId) {
                            return function() {
                                ASC.TMTalk.roomsContainer.hideHistoryFilter(roomId);
                            };
                        })(room.getAttribute('data-roomid')));
                        return false;
                    }
                }
            }
            //new history filter
        } else if (ASC.TMTalk.dom.hasClass(target, 'filter-option')) {
            var selector = target.parentNode.parentNode.parentNode.parentNode;
            var optionid = target.getAttribute('data-value');
            var room = selector.parentNode.parentNode.parentNode.parentNode;
            if (ASC.TMTalk.dom.hasClass(room, 'room')) {
                var roomcid = room.getAttribute('data-roomcid');
                if (roomcid && optionid) {
                    ASC.TMTalk.roomsContainer.getFilteringHistory(roomcid, optionid);
                }
            }
        } /*else if (ASC.TMTalk.dom.hasClass(target, 'filter-option')) {
          var selector = target.parentNode.parentNode.parentNode;
          if (ASC.TMTalk.dom.hasClass(selector, 'custom-select')) {
            var optionid = target.getAttribute('data-id');
            if (optionid !== selector.getAttribute('data-value')) {
              selector.setAttribute('data-value', optionid);
              var room = selector.parentNode.parentNode;
              if (ASC.TMTalk.dom.hasClass(room, 'room')) {
                var roomcid = room.getAttribute('data-roomcid');
                if (roomcid && optionid) {
                  ASC.TMTalk.roomsContainer.getFilteringHistory(roomcid, optionid);
                }
              }
            }
          }
      }*/
        else if (ASC.TMTalk.dom.hasClass(target, 'button-talk') && ASC.TMTalk.dom.hasClass(target, 'search-start')) {
            var filteringPanel = target.parentNode.parentNode.parentNode;
            if ($(filteringPanel).hasClass('close')) {
                $(filteringPanel).removeClass('close');
            }
            $(filteringPanel).toggleClass('show');
            
        } else if ((ASC.TMTalk.dom.hasClass(target, 'button-talk') && ASC.TMTalk.dom.hasClass(target, 'search-prev-message')) ||
                    ASC.TMTalk.dom.hasClass(target, 'search_prevbutton_container') ||
                    (ASC.TMTalk.dom.hasClass(target, 'button-container') && ASC.TMTalk.dom.hasClass(target, 'search-prev-message'))) {

            var room = null;
            if (ASC.TMTalk.dom.hasClass(target, 'search-prev-message')) {
                if (ASC.TMTalk.dom.hasClass(target, 'button-talk')) 
                    room = target.parentNode.parentNode.parentNode.parentNode.parentNode;
                else
                    room = target.parentNode.parentNode.parentNode.parentNode;
            } else {
                room = target.parentNode.parentNode.parentNode;
            }
                                                                                                                                        
            if (ASC.TMTalk.dom.hasClass(room, 'room')) {
                var node = null,
                    nodes = null,
                    nodesInd = 0,
                    messagescontainer = null,
                    currentfoundmessage = null;
                //nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'history', 'div');
                nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'messages', 'div');
                if (nodes.length > 0) {
                    nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'messages', 'ul');
                    if (nodes.length > 0) {
                        messagescontainer = nodes[0];
                    }
                }
                if (messagescontainer !== null) {
                    var containerOffsetTop = messagescontainer.scrollTop;
                    nodes = ASC.TMTalk.dom.getElementsByClassName(messagescontainer, 'message found', 'li');
                    nodesInd = nodes.length;
                    var number = 0;
                    while (nodesInd--) {
                        node = nodes[nodesInd];
                        number++;
                        if (ASC.TMTalk.dom.getElementsByClassName(node, 'message', 'div')[0].offsetHeight > 0 && node.offsetTop < containerOffsetTop) {
                            messagescontainer.scrollTop = node.offsetTop;
                            break;
                        }
                        if (number == nodes.length) {
                            messagescontainer.scrollTop = nodes[nodes.length - 1].offsetTop - (messagescontainer.offsetHeight - ASC.TMTalk.dom.getElementsByClassName(node, 'message', 'div')[0].offsetHeight);
                            number = 1;
                        }
                    }
                    nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'currentfoundmessage', 'span');
                    if (nodes.length > 0) {
                        currentfoundmessage = nodes[0];
                        $(currentfoundmessage).html(number);
                    }
                }
                $('div#talkRoomsContainer ul.rooms li.room.current div.filtering-panel.show div.textfield.filtering-field input').focus();
            }
        } else if ((ASC.TMTalk.dom.hasClass(target, 'button-talk') && ASC.TMTalk.dom.hasClass(target, 'search-next-message')) ||
                    ASC.TMTalk.dom.hasClass(target, 'search_nextbutton_container') ||
                    (ASC.TMTalk.dom.hasClass(target, 'button-container') && ASC.TMTalk.dom.hasClass(target, 'search-next-message'))) {
            var room = null;
            if (ASC.TMTalk.dom.hasClass(target, 'search-next-message')) {
                if (ASC.TMTalk.dom.hasClass(target, 'button-talk'))
                    room = target.parentNode.parentNode.parentNode.parentNode.parentNode;
                else
                    room = target.parentNode.parentNode.parentNode.parentNode;
            } else {
                room = target.parentNode.parentNode.parentNode;
            }
            if (ASC.TMTalk.dom.hasClass(room, 'room')) {
                var node = null,
                    nodes = null,
                    nodesInd = 0,
                    currentnumber = 0,
                    messagescontainer = null;
                //nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'history', 'div');
                nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'messages', 'div');
                if (nodes.length > 0) {
                    nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'messages', 'ul');
                    if (nodes.length > 0) {
                        messagescontainer = nodes[0];
                    }
                }
                if (messagescontainer !== null) {
                    var containerOffsetTop = messagescontainer.scrollTop + messagescontainer.offsetHeight;
                    nodes = ASC.TMTalk.dom.getElementsByClassName(messagescontainer, 'message found', 'li');
                    nodesInd = nodes.length;
                    for (var i = 0, n = nodes.length; i < n; i++) {
                        node = nodes[i];
                        if (ASC.TMTalk.dom.getElementsByClassName(node, 'message', 'div')[0].offsetHeight > 0 && node.offsetTop + node.offsetHeight > containerOffsetTop) {
                            messagescontainer.scrollTop = node.offsetTop - (messagescontainer.offsetHeight - ASC.TMTalk.dom.getElementsByClassName(node, 'message', 'div')[0].offsetHeight);
                            break;
                        }
                    }
                    if (i !== nodes.length) {
                        currentnumber = nodes.length - i;
                    } else {
                        try {
                            messagescontainer.scrollTop = nodes[0].offsetTop - (messagescontainer.offsetHeight - ASC.TMTalk.dom.getElementsByClassName(node, 'message', 'div')[0].offsetHeight);
                            currentnumber = i ;
                        }catch (err){}
                        
                    }
                    nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'currentfoundmessage', 'span');
                    if (nodes.length > 0) {
                        currentfoundmessage = nodes[0];
                        $(currentfoundmessage).html(currentnumber);
                    }
                }
                $('div#talkRoomsContainer ul.rooms li.room.chat div.filtering-panel.show div.textfield.filtering-field input').focus();
            }
        } else if (ASC.TMTalk.dom.hasClass(target, 'button-talk') && ASC.TMTalk.dom.hasClass(target, 'clear-search')) {
            var room = target.parentNode.parentNode.parentNode;
            ASC.TMTalk.roomsManager.clearSearch(room);
            jQuery('div#talkRoomsContainer ul.rooms li.room div.filtering-panel').removeClass('show');
        }
      });

    $('#talkDialogsContainer').click(function (evt) {
       
      var $target = $(evt.target);
      
      if ($target.hasClass('button-talk') && $target.hasClass('kick-occupant')) {
        var
          jid = ASC.TMTalk.trim($('#hdnKickJId').val()),
          invited = $('#hdnKickInvited').val(),
          roomCid = ASC.TMTalk.trim($('#hdnKickRoomCid').val()),
          reason = $('#txtKickReason').val();
        if ($.parseJSON(invited)) {
            if (roomCid) {
                window.ASC.TMTalk.roomsContainer.onRottenInvite(roomCid, jid);
            }
        } else {
           if (jid) {
              reason = typeof reason === 'string' ? reason : '';
              ASC.TMTalk.mucManager.kickContact(jid, reason);
           } 
        }
        TMTalk.hideDialog();
        
    } else if ($target.hasClass('button-talk') && $target.hasClass('remove-room')) {
        var
          roomjid = ASC.TMTalk.trim($('#hdnRemoveJid').val()),
          reason = $('#txtRemoveReason').val();
        if (roomjid) {
          reason = typeof reason === 'string' ? reason : '';
          ASC.TMTalk.mucManager.removeRoom(roomjid, reason);
          TMTalk.hideDialog();
        }
    } else if ($target.hasClass('button-talk') && $target.hasClass('accept-invite')) {
        var
          roomjid = ASC.TMTalk.trim($('#hdnInvitationRoom').val()),
          inviterjid = ASC.TMTalk.trim($('#hdnInvitationContact').val());
        if (roomjid) {
          ASC.TMTalk.mucManager.acceptInvite(roomjid, inviterjid);
        }
        TMTalk.hideDialog();
    } else if ($target.hasClass('button-talk') && $target.hasClass('decline-invite')) {
        var
          roomjid = ASC.TMTalk.trim($('#hdnInvitationRoom').val()),
          inviterjid = ASC.TMTalk.trim($('#hdnInvitationContact').val());
        
        if (roomjid) {
          ASC.TMTalk.mucManager.declineInvite(roomjid, inviterjid);
        }
        TMTalk.hideDialog();
      }
    });
  });
})(jQuery);
