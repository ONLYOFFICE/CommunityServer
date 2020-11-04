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

window.ASC.TMTalk.connectionManager = (function () {
  var
    isInit = false,
    isLoaded = false,
    isConnected = false,
    JID = '',
    servicePath = '',
    clientInactivity = 30,
    wait = 30,
    hold = 2,
    resourcePriority = '0',
    idPrefix = 'tmtalk',
    statusTitle = 'Talk',
    messageId = Math.round(Math.random() * 10000) + 100000,
    messageIds = {},
    lazyMessages = [],
    requestRoomInfo = {},
    connectionManager = null,
    onlineStatusId = 1,
    offlineStatusId = 0,
    newStatus = null,
    reconnectionAttempts = 5,
    reconnectTimeout = 2000,
    reconnectCount = 0;
    statuses = [
      {id : 0, name : 'Offline', show : 'offline', title : 'Offline', isCurrent : true, className : 'offline'},
      {id : 1, name : 'Online', show : 'online', title : 'Online', isCurrent : false, className : 'online'},
      {id : 2, name : 'Away', show : 'away', title : 'Away', isCurrent : false, className : 'away'},
      {id : 3, name : 'Not Available', show : 'xa', title : 'Not Available', isCurrent : false, className : 'xa'}
    ],
    customEvents = {
      // before attempt to connect to server
      initConnection : 'oninitconnection',
      // attempt to connect to server
      connecting : 'onconnecting',
      // before attempt to disconnect from the server
      disconnecting : 'ondisconnecting',
      // after conected
      connected : 'onconnected',
      // after disconnected
      disconnected : 'ondisconnected',
      // connection failed
      connectingFailed : 'onconnectingfailed',
      // change status
      updateStatus : 'onupdatestatus',
      // join to conference
      enteringRoom : 'onenteringroom',
      // create conference
      creatingRoom : 'oncreatingroom',
      // leave conference
      exitingRoom : 'onexitingroom',
      // remove conference (only owner)
      destroyingRoom : 'ondestroyingroom',
      // kicked occupant
      kickingOccupant : 'onkickingoccupant',
      // Retrieves Private Data
      retrievesData : 'onretrievesdata',
      // Retrieves Command
      retrievesCommand : 'onretrievescommand',
      // Retrieves Features
      retrievesFeatures : 'onretrievesfeatures',
      // Retrieves DiscoItems
      retrievesDiscoItems : 'onretrievesdiscoitems'
    },
    eventManager = new CustomEvent(customEvents);
  var getMessageId = function () {
    messageId++;
    return idPrefix + messageId.toString(16);
  };

  var getXHR = (function () {
    if (window.XMLHttpRequest) {
      return function () {
        try {
          xhr = new XMLHttpRequest();
          if (xhr.overrideMimeType) {
            xhr.overrideMimeType("text/xml");
          }
          return xhr;
        } catch (err) {
          return null;
        }
      };
    }
    return function () {
      try {
        return new ActiveXObject('Msxml2.XMLHTTP');
      } catch (err1) {
        try {
          return new ActiveXObject('Microsoft.XMLHTTP');
        } catch (err2) {
          return null;      
        }
      }
    };
  })();

  var conflict;
  var init = function (servicepath, jid, priority, inactivity) {
    if (isInit === true) {
      return undefined;
    }
    this.conflict = false;
    isInit = true;
    // TODO
    if (typeof jid === 'string') {
        JID = jid;
    }
    if (typeof priority === 'string') {
      resourcePriority = priority;
    }
    if (typeof inactivity === 'string' && isFinite(+inactivity)) {
      clientInactivity = +inactivity;
    }
    if (typeof servicepath === 'string') {
      servicePath = servicepath;
      connectionManager = new Strophe.Connection(servicePath);
    }

    if (ASC.TMTalk.Resources.hasOwnProperty('statusTitles') && typeof ASC.TMTalk.Resources.StatusTitles === 'object') {
      var
        status = null,
        statusesInd = statuses.length;
      while (statusesInd--) {
        status = statuses[statusesInd];
        if (ASC.TMTalk.Resources.StatusTitles.hasOwnProperty(status.show)) {
            statuses[statusesInd].title = ASC.TMTalk.Resources.StatusTitles[status.show];
        }
      }
    }
  };

  var bind = function (eventName, handler, params) {
    return eventManager.bind(eventName, handler, params);
  };

  var unbind = function (handlerId) {
    return eventManager.unbind(handlerId);
  };

  var addIqHandler = function (ns, handler) {
    if (typeof ns === 'string' && typeof handler === 'function') {
      connectionManager.addHandler(handler, ns, 'iq', null, null, null);
    }
  };

  var sendIqMessage = function (iq) {
    if (connectionManager !== null && typeof iq === 'object') {
      connectionManager.send(iq);
    }
  };

  var changeStatus = function (statusId, show, title, priority) {
    var pres = $pres({from : connectionManager.jid});
    if (statusId !== onlineStatusId) {
      pres.c('show').t(show).up();
    }
    connectionManager.send(
      pres
        .c('status').t(title).up()
        .c('priority').t(priority).up()
        .c('c', {xmlns : Strophe.NS.CAPS, ver : '2.3', node : Strophe.NS.CAPS})
        .tree()
    );
  };

  var connect = function (password, authtoken) {
    if (isInit === false) {
      throw 'uninit connectionManager';
    }
    if (typeof password === 'string' && password) {
      eventManager.call(customEvents.initConnection);
      connectionManager.connect(JID, password, onConnect, clientInactivity, wait, hold);
      return undefined;
    }
    if (typeof authtoken === 'string' && authtoken) {
      eventManager.call(customEvents.initConnection);
      connectionManager.connect1(JID, authtoken, onConnect, clientInactivity, wait, hold);

      return undefined;
    }
    JabberClient.GetAuthToken(function (response) {
        if (response && typeof response.value === 'string' && response.value) {
            ASC.TMTalk.connectionManager.connect(null, response.value);
        } else {
            eventManager.call(customEvents.connecting);
            setTimeout(function () {
                ASC.TMTalk.connectionManager.status(Strophe.Status.CONNECTING);
            }, reconnectTimeout);
        }
    });
  };

  var disconnect = function () {
    if (isConnected) {
      connectionManager.disconnect();
    }
  };

  var terminate = function () {
      if (isConnected) {
          var body = $build("body", {rid : connectionManager.rid++, sid : connectionManager.sid, xmlns: Strophe.NS.HTTPBIND, type : "terminate"});
          if (connectionManager.authenticated) {
              body.c("presence", {xmlns : Strophe.NS.CLIENT, type : "unavailable"});
          }
          var data = Strophe.serialize(body.tree());
          try {
              if (window.opener) {
                  window.opener.ASC.Controls.JabberClient.terminate(data, connectionManager.service);
              } else {
                  var xhr = getXHR();
                  xhr.onreadystatechange = function () {};
                  xhr.open("POST", connectionManager.service, true);
                  xhr.send(data);
              }
          } catch (e) {
              console.error(e.message);
          }
      }
  };

  var connected = function () {
    return isConnected;
  };

  var setStatus = function (status) {
    if (typeof status !== 'undefined') {
      status = isFinite(+status) ? +status : status;
      newStatus = null;
      var statusInd = 0;
      statusInd = statuses.length;
      while (statusInd--) {
        if ((statuses[statusInd].id === status || statuses[statusInd].name === status)) {
          newStatus = statuses[statusInd];
          break;
        }
      }
      if (newStatus === null) {
        return undefined;
      }

      if (!isConnected && newStatus.id !== offlineStatusId) {
        connect();
        return undefined;
      }
      if (isConnected && newStatus.id === offlineStatusId) {
        disconnect();
        return undefined;
      }

      if (newStatus.id !== offlineStatusId) {
        changeStatus(newStatus.id, newStatus.show, statusTitle, resourcePriority);
      }
      statusInd = statuses.length;
      var wasOffline = false;
      while (statusInd--) {
        if (statuses[statusInd].isCurrent && statuses[statusInd].id === offlineStatusId) {
          wasOffline = true;
        }
        statuses[statusInd].isCurrent = statuses[statusInd].id === newStatus.id;
      }
      eventManager.call(customEvents.updateStatus, window, [newStatus, wasOffline]);
      newStatus = null;
      return undefined;
    }
    var statusInd = statuses.length;
    while (statusInd--) {
      if (statuses[statusInd].isCurrent) {
        return statuses[statusInd];
      }
    }
    return null;
  };

  var onConnect = function (status) {
    switch (status) {
      case Strophe.Status.DISCONNECTING :
        eventManager.call(customEvents.disconnecting);
        break;
      case Strophe.Status.DISCONNECTED :
        isLoaded = false;
        isConnected = false;
        setStatus(offlineStatusId);
        ASC.TMTalk.mucManager.closeRooms();
        ASC.TMTalk.contactsManager.clearOnlineContacts();
        eventManager.call(customEvents.disconnected);
        break;
      case Strophe.Status.CONNECTING :
        eventManager.call(customEvents.connecting);
        break;
      case Strophe.Status.CONNECTED :
        connectionManager.addHandler(handleIq, null, 'iq', null, null, null);
        connectionManager.addHandler(handleMessage, null, 'message',  null, null, null);
        connectionManager.addHandler(handlePresence, null, 'presence', null, null, null);

        connectionManager.addHandler(handleIqVcard, Strophe.NS.VCARD, 'iq', null, null, null);
        connectionManager.addHandler(handleIqRoster, Strophe.NS.ROSTER, 'iq', null, null, null);
        connectionManager.addHandler(handleIqVersion, Strophe.NS.VERSION, 'iq', null, null, null);
        connectionManager.addHandler(handleIqHistory, Strophe.NS.HISTORY, 'iq', null, null, null);
        connectionManager.addHandler(handleIqPrivate, Strophe.NS.PRIVATE, 'iq', null, null, null);
        connectionManager.addHandler(handleIqCommands, Strophe.NS.COMMANDS, 'iq', null, null, null);
        connectionManager.addHandler(handleIqMucOwner, Strophe.NS.MUC_OWNER, 'iq', null, null, null);
        connectionManager.addHandler(handleIqDiscoInfo, Strophe.NS.DISCO_INFO, 'iq', null, null, null);
        connectionManager.addHandler(handlerIqDiscoItems, Strophe.NS.DISCO_ITEMS, 'iq', null, null, null);

        connectionManager.send($iq({id : getMessageId(), from : connectionManager.jid, type : 'get'}).c('query', {xmlns : Strophe.NS.ROSTER}).tree());
        connectionManager.send($iq({id : getMessageId(), from : connectionManager.jid, to : connectionManager.domain, type : 'get'}).c('query', {xmlns : Strophe.NS.DISCO_INFO}).tree());

        isConnected = true;
        setStatus(newStatus.id);
        eventManager.call(customEvents.connected);
        break;
      case Strophe.Status.CONNFAIL :
        eventManager.call(customEvents.connectingFailed);
        break;
    }
  };

  // strophe callback functions.
  // return only true.
  var handlePresence = function (presence) {
    //console.log('handlePresence', presence);
    var x = presence.getElementsByTagName('x');
    if (x.length === 0) {
      x = Strophe.getElementsByTagName(presence, 'x');
    }
    if (x.length > 0 && x[0].getAttribute('xmlns') === Strophe.NS.MUC_USER) {
      // message from conference
      var
        items = null,
        statuses = null,
        destroynodes = null,
        type = presence.getAttribute('type');

      destroynodes = presence.getElementsByTagName('destroy');
      if (destroynodes.length === 0) {
        destroynodes = Strophe.getElementsByTagName(presence, 'destroy');
      }

      items = presence.getElementsByTagName('item');
      if (items.length === 0) {
        items = Strophe.getElementsByTagName(presence, 'item');
      }

      statuses = presence.getElementsByTagName('status');
      if (statuses.length === 0) {
        statuses = Strophe.getElementsByTagName(presence, 'status');
      }

      if (destroynodes.length > 0) {
        eventManager.call(customEvents.destroyingRoom, window, [presence]);
      }

      // get the presence's status
      // 201 need processed before 110
      var statusesInd = statuses.length;
      while (statusesInd--) {
        if (statuses[statusesInd].getAttribute('code') === '201') {
          eventManager.call(customEvents.creatingRoom, window, [presence]);
          break;
        }
      }
      statusesInd = statuses.length;
      while (statusesInd--) {
        if (statuses[statusesInd].getAttribute('code') === '110') {
          eventManager.call(customEvents.enteringRoom, window, [presence]);
          break;
        }
      }
      statusesInd = statuses.length;
      while (statusesInd--) {
        if (statuses[statusesInd].getAttribute('code') === '307') {
          eventManager.call(customEvents.kickingOccupant, window, [presence]);
          break;
        }
      }

      if (items.length > 0 && destroynodes.length === 0) {
        if (type === 'unavailable') {
          if (ASC.TMTalk.mucManager.isMe(presence.getAttribute('from'))) {
            eventManager.call(customEvents.exitingRoom, window, [presence]);
          } else {
            ASC.TMTalk.mucManager.leftContact(presence);
          }
        } else {
          ASC.TMTalk.mucManager.comeContact(presence);
        }
      }
      return true;
    }
    // message from contact
    if (presence.getAttribute('type') === 'unavailable') {
      ASC.TMTalk.contactsManager.leftContact(presence);
    } else {
      ASC.TMTalk.contactsManager.comeContact(presence);
    }
    return true;
  };

  var handleOfflineMessages = function (messages) {
    if (isLoaded === false) {
      return true;
    }
    var
      type = null,
      body = null,
      delay = null,
      message = null,
      messagesInd = 0;
    messagesInd = messages.length;
    while (messagesInd--) {
      message = messages[messagesInd];
      type = message.getAttribute('type');
      switch (type) {
        case 'chat' :
          delay = message.getElementsByTagName('delay');
          if (delay.length === 0) {
            delay = Strophe.getElementsByTagName(message, 'delay');
          }
          delay = delay.length > 0 ? delay[0] : null;

          body = message.getElementsByTagName('body');
          if (body.length === 0) {
            body = Strophe.getElementsByTagName(message, 'body');
          }
          body = body.length > 0 ? body[0] : null;

          if (delay === null || body === null) {
            messages.splice(messagesInd, 1);
          }
          break;
      }
    }
    ASC.TMTalk.messagesManager.recvOfflineMessagesFromChat(messages);
    return true;
  };

  var handleMessage = function (message) {
    var
      id = message.getAttribute('id'),
      from = message.getAttribute('from');

    // message[id] unique test
    if (id && from) {
      if (!messageIds.hasOwnProperty(from)) {
        messageIds[from] = {};
      }
      /*if (messageIds[from].hasOwnProperty(id)) {
        return true;
      }*/
      messageIds[from][id] = true;
    }

    // lazy messages
    if (isLoaded === false) {
      lazyMessages.push(message);
      return true;
    }

    //console.log('handleMessage', message);
    var type = message.getAttribute('type');
    type = typeof type === 'string' ? type.toLowerCase() : null;
    switch (type) {
      case 'headline' :
        ASC.TMTalk.messagesManager.recvMessageFromChat(message);
        break;
      case 'chat' :
        var composing = null;
        composing = message.getElementsByTagName('composing');
        if (composing.length === 0) {
          composing = Strophe.getElementsByTagName(message, 'composing');
        }
        composing = composing.length > 0 ? composing[0] : null;
        if (composing !== null) {
          ASC.TMTalk.messagesManager.composingMessageFromChat(message);
        }

        var paused = null;
        paused = message.getElementsByTagName('paused');
        if (paused.length === 0) {
          paused = Strophe.getElementsByTagName(message, 'paused');
        }
        if (paused.length === 0) {
          paused = message.getElementsByTagName('active');
          if (paused.length === 0) {
            paused = Strophe.getElementsByTagName(message, 'active');
          }
        }
        paused = paused.length > 0 ? paused[0] : null;
        if (paused !== null) {
          ASC.TMTalk.messagesManager.pausedMessageFromChat(message);
        }

        var delay = null;
        delay = message.getElementsByTagName('delay');
        if (delay.length === 0) {
          delay = Strophe.getElementsByTagName(message, 'delay');
        }
        delay = delay.length > 0 ? delay[0] : null;

        var body = null;
        body = message.getElementsByTagName('body');
        if (body.length === 0) {
          body = Strophe.getElementsByTagName(message, 'body');
        }
        body = body.length > 0 ? body[0] : null;
        if (body !== null) {
          ASC.TMTalk.messagesManager.recvMessageFromChat(message, delay !== null);
        }

        var event = null;
        event = message.getElementsByTagName('x');
        if (event.length === 0) {
          event = Strophe.getElementsByTagName(message, 'x');
        }
        event = event.length > 0 ? event[0] : null;
        if (event !== null && event.getAttribute('xmlns') !== 'jabber:x:event') {
          event === null;
        }

        if (event !== null && composing === null && paused === null && body === null) {
          ASC.TMTalk.messagesManager.pausedMessageFromChat(message);
        }
        break;
      case 'groupchat' :
        var
          from = message.getAttribute('from'),
          nodes = null;
        nodes = message.getElementsByTagName('subject');
        if (nodes.length === 0) {
          nodes = Strophe.getElementsByTagName(message, 'subject');
        }
        if (from && nodes.length > 0) {
          from = from.substring(0, from.indexOf('/')).toLowerCase();
          if (from.length > 0) {
            ASC.TMTalk.mucManager.updateSubject(from, nodes.length > 0 ? ASC.TMTalk.dom.getContent(nodes[0]) : '');
          }
          break;
        }
        ASC.TMTalk.messagesManager.recvMessageFromConference(message);
        break;
      default :
        // recv invite
        var invite = null;
        invite = message.getElementsByTagName('invite');
        if (invite.length === 0) {
          invite = Strophe.getElementsByTagName(message, 'invite');
        }
        invite = invite.length > 0 ? invite[0] : null;
        if (invite !== null) {
          var
            inviterjid = invite.getAttribute('from'),
            roomjid = message.getAttribute('from'),
            reason = '',
            nodes = null;
          nodes = invite.getElementsByTagName('reason');
          if (nodes.length === 0) {
            nodes = Strophe.getElementsByTagName(invite, 'reason');
          }
          reason = nodes.length > 0 ? ASC.TMTalk.dom.getContent(nodes[0]) : '';
          if (roomjid && inviterjid) {
            ASC.TMTalk.mucManager.recvInvite(roomjid, inviterjid, reason);
          }
          break;
        }

        // get decline
        var decline = null;
        decline = message.getElementsByTagName('invite');
        if (decline.length === 0) {
          decline = Strophe.getElementsByTagName(message, 'invite');
        }
        decline = decline.length > 0 ? decline[0] : null;
        if (decline !== null && decline.getAttribute('from') === connectionManager.jid) {
          var
            deviatorjid = message.getAttribute('from'),
            reason = '',
            nodes = null;
          nodes = invite.getElementsByTagName('reason');
          if (nodes.length === 0) {
            nodes = Strophe.getElementsByTagName(invite, 'reason');
          }
          reason = nodes.length > 0 ? ASC.TMTalk.dom.getContent(nodes[0]) : '';
          if (deviatorjid) {
            //console.log(deviatorjid, ' decline invite');
          }
          break;
        }

        // get multicastmessage
        var addresses = null;
        addresses = message.getElementsByTagName('addresses');
        if (addresses.length === 0) {
          addresses = Strophe.getElementsByTagName(message, 'addresses');
        }
        addresses = addresses.length > 0 ? addresses[0] : null;
        if (addresses !== null) {
          var body = null;
          body = message.getElementsByTagName('body');
          if (body.length === 0) {
            body = Strophe.getElementsByTagName(message, 'body');
          }
          body = body.length > 0 ? body[0] : null;
          if (body !== null) {
            ASC.TMTalk.messagesManager.recvMulticastMessage(message);
          }
        }

        var
          from = message.getAttribute('from'),
          nodes = null;
        nodes = message.getElementsByTagName('subject');
        if (nodes.length === 0) {
          nodes = Strophe.getElementsByTagName(message, 'subject');
        }
        if (from && nodes.length > 0) {
          ASC.TMTalk.mucManager.updateSubject(from, nodes.length > 0 ? ASC.TMTalk.dom.getContent(nodes[0]) : '');
        }
    }
    return true;
  };

  var handleIq = function (iq) {
    //console.log('handleIq', iq);
    return true;
  };

  var handleIqVersion = function (iq) {
    //console.log('handleIqVersion', iq);
    switch (iq.getAttribute('type').toLowerCase()) {
      case 'get' :
        connectionManager.send(
          $iq({id : iq.getAttribute('id'), from : iq.getAttribute('to'), to : iq.getAttribute('from'), type : 'result'})
            .c('query', {xmlns : Strophe.NS.VERSION})
            .c('name').t('Talk').up()
            .c('version').t(' ').up()
            .c('os').t(navigator.userAgent)
            .tree()
        );
        break;
    }
    return true;
  };

  var handleIqMucOwner = function (iq) {
    //console.log('handleIqMucOwner', iq);
    switch (iq.getAttribute('type').toLowerCase()) {
      case 'get' :
        break;
      case 'result' :
        var
          from = iq.getAttribute('from'),
          fields = null;
        fields = iq.getElementsByTagName('field');
        if (fields.length === 0) {
          fields = Strophe.getElementsByTagName(iq, 'field');
        }
        if (fields.length > 0) {
          ASC.TMTalk.mucManager.sendConfigurationForm(from, fields);
        }
        break;
    }
    return true;
  };

  var handleIqDiscoInfo = function (iq) {
    //console.log('handleIqDiscoInfo', iq);
    switch (iq.getAttribute('type').toLowerCase()) {
      case 'get' :
        connectionManager.send(
          $iq({id : iq.getAttribute('id'), from : iq.getAttribute('to'), to : iq.getAttribute('from'), type : 'result'})
            .c('query', {xmlns : Strophe.NS.DISCO_INFO})
            .c('feature', {'var' : Strophe.NS.VERSION}).up()
            .c('feature', {'var' : Strophe.NS.CONFERENCE}).up()
            .c('feature', {'var' : Strophe.NS.CHATSTATES}).up()
            .c('feature', {'var' : Strophe.NS.DISCO_INFO}).up()
            .c('feature', {'var' : Strophe.NS.DISCO_ITEMS}).up()
            .c('feature', {'var' : Strophe.NS.MUC}).up()
            .c('feature', {'var' : Strophe.NS.MUC_USER}).up()
            .tree()
        );
        break;
      case 'result' :
        var
          features = null,
          from = iq.getAttribute('from');

        features = iq.getElementsByTagName('feature');
        if (features.length === 0) {
          features = Strophe.getElementsByTagName(iq, 'feature');
        }
        if (from) {
          eventManager.call(customEvents.retrievesFeatures, window, [from, features]);
        }
        break;
    }
    return true;
  };

  var handlerIqDiscoItems = function (iq) {
    //console.log('handlerIqDiscoItems', iq);
    switch (iq.getAttribute('type').toLowerCase()) {
      case 'get' :
        connectionManager.send(
          $iq({id : iq.getAttribute('id'), type : 'result', to : iq.getAttribute('from')})
            .c('query', {xmlns : Strophe.NS.DISCO_ITEMS})
            .tree()
        );
        break;
      case 'result' :
        var
          item = null,
          items = null,
          itemsInd = 0,
          roomjid = '',
          isDiscoItemsRequest = true,
          from = iq.getAttribute('from');

        items = iq.getElementsByTagName('item');
        if (items.length === 0) {
          items = Strophe.getElementsByTagName(iq, 'item');
        }
        isDiscoItemsRequest = true;
        itemsInd = items.length;
        while (itemsInd--) {
          item = items[itemsInd];
          roomjid = item.getAttribute('jid');
          for (var fld in requestRoomInfo) {
            if (requestRoomInfo.hasOwnProperty(fld)) {
              if (fld === roomjid) {
                delete requestRoomInfo[fld];
                ASC.TMTalk.mucManager.getRoomInfo(roomjid, item);
                isDiscoItemsRequest = false;
              }
            }
          }
        }
        if (isDiscoItemsRequest && from) {
          eventManager.call(customEvents.retrievesDiscoItems, window, [from, items]);
        }
        break;
    }
    return true;
  };

  var handleIqVcard = function (iq) {
    //console.log('handleIqVcard', iq);
    if (iq.getAttribute('type').toLowerCase() === 'result') {
      if (iq.getAttribute('from') === connectionManager.jid) {
        isLoaded = true;
        if (lazyMessages.length > 0) {
          //for (var i = 0, n = lazyMessages.length; i < n; i++) {
          //  handleMessage(lazyMessages[i]);
          //}
          handleOfflineMessages(lazyMessages);
          lazyMessages = [];
        }
      }
      ASC.TMTalk.contactsManager.updateCard(iq);
    }
    return true;
  };

  var handleIqRoster = function (iq) {
    //console.log('handleIqRoster', iq);
    if (iq.getAttribute('type').toLowerCase() === 'result') {
      var items = iq.getElementsByTagName('item');
      if (items.length === 0) {
        items = Strophe.getElementsByTagName(iq, 'item');
      }
      ASC.TMTalk.contactsManager.fillContacts(items);
      connectionManager.send(
        $iq({id : getMessageId(), from : connectionManager.jid, to : connectionManager.jid, type : 'get'})
          .c('vCard', {xmlns : Strophe.NS.VCARD})
          .tree()
      );
    }
    try {
        var opener = window.opener;
        if (opener && opener.SmallChat) {
            opener.SmallChat.openContacts();
        }
    } catch (e) {
        console.error(e.message);
    }

    return true;
  };

  var handleIqHistory = function (iq) {
    //console.log('handleIqHistory', iq);
    if (iq.getAttribute('type').toLowerCase() === 'result') {
      var query = null;
      var query = iq.getElementsByTagName('query');
      if (query.length === 0) {
        query = Strophe.getElementsByTagName(iq, 'query');
      }
      query = query.length > 0 ? query[0] : null;
      if (query === null) {
        return undefined;
      }
      if (query.getAttribute('count') || query.getAttribute('text')) {
          var startIndex = query.getAttribute('startindex');

          ASC.TMTalk.messagesManager.loadHistory(iq, ASC.TMTalk.Config.fullText && startIndex == null || parseInt(startIndex) === 0, query.getAttribute('text'));
      }
      if (query.getAttribute('from') && query.getAttribute('to')) {
        ASC.TMTalk.messagesManager.loadHistoryByFilter(iq, query.getAttribute('from'), query.getAttribute('to'));
      }
    }
    return true;
  };

  var handleIqPrivate = function (iq) {
    //console.log('handleIqPrivate', iq);
    if (iq.getAttribute('type').toLowerCase() === 'result') {
      var storages = iq.getElementsByTagName('tmtalk');
      if (storages.length === 0) {
        storages = Strophe.getElementsByTagName(iq, 'tmtalk');
      }
      if (storages.length > 0) {
        eventManager.call(customEvents.retrievesData, window, [storages]);
      }
    }
    return true;
  };

  var handleIqCommands = function (iq) {
    //console.log('handleIqCommands', iq);
    switch (iq.getAttribute('type').toLowerCase()) {
      case 'set' :
        var
          from = iq.getAttribute('from'),
          node = null,
          commands = null
          commandsInd = 0;
        commands = Strophe.getElementsByTagName(iq, 'command');
        if (commands.length === 0) {
          commands = Strophe.getElementsByTagName(iq, 'command');
        }

        commandsInd = commands.length;
        while (commandsInd--) {
          node = commands[commandsInd].getAttribute('node');
          if (node) {
            eventManager.call(customEvents.retrievesCommand, window, [from, node]);
          }
        }
        break;
    }
    return true;
  };

  var creatingRoom = function (jid) {
    connectionManager.send(
      $pres({id : getMessageId(), from : connectionManager.jid, to : jid, type : 'set'})
        .c('query', {xmlns : Strophe.NS.MUC_USER})
        .tree()
    );
  };

  var enteringRoom = function (jid) {
    connectionManager.send(
      $pres({from : connectionManager.jid, to : jid})
        .c('priority').t(resourcePriority)
        .tree()
    );
  };

  var exitingRoom = function (jid) {
    connectionManager.send(
      $pres({from : connectionManager.jid, to : jid, type : 'unavailable'})
        .tree()
      );
  };
  var savePushEndpoint = function (username, endpoint, browser) {

      connectionManager.send(
          $iq({ username: username, endpoint: endpoint, browser: browser, type: 'notification' })
          .c('query', { xmlns: Strophe.NS.MUC_ADMIN })
          .tree()
      );
  };
  var kickingOccupant = function (jid, nick, reason) {
    connectionManager.send(
      $iq({id : getMessageId(), from : connectionManager.jid, to : jid, type : 'set'})
        .c('query', {xmlns : Strophe.NS.MUC_ADMIN})
        .c('item', {nick : nick, role : 'none'})
        .c('reason').t(reason)
        .tree()
    );
  };

  var destroyingRoom = function (jid, reason) {
    connectionManager.send(
      $iq({id : getMessageId(), from : connectionManager.jid, to : jid, type : 'set'})
        .c('query', {xmlns : Strophe.NS.MUC_OWNER})
        .c('destroy', {jid : jid})
        .c('reason').t(reason)
        .tree()
    );
  };

  var getConfigurationForm = function (jid) {
    connectionManager.send(
      $iq({id : getMessageId(), from : connectionManager.jid, to : jid, type : 'set'})
        .c('query', {xmlns : Strophe.NS.MUC_OWNER})
        .tree()
    );
  };

  var sendConfigurationForm = function (jid, fields) {
    var
      iq = null,
      field = null,
      fieldsInd = 0;
    iq =  $iq({id : getMessageId(), from : connectionManager.jid, to : jid, type : 'set'})
            .c('query', {xmlns : Strophe.NS.MUC_OWNER})
            .c('x', {xmlns : Strophe.NS.DATA, type : 'submit'});
    fieldsInd = fields.length;
    while (fieldsInd--) {
      field = fields[fieldsInd];
      iq.c('field', {'var' : field.name}).c('value').t(field.value).up().up();
    }
    connectionManager.send(iq.tree());
  };

  var sendMailingLists = function (xmlns, items) {
    var
      iq = null,
      item = null,
      itemsInd = items.length;

    iq =  $iq({id : getMessageId(), from : connectionManager.jid, type : 'set'})
            .c('query', {xmlns : Strophe.NS.PRIVATE})
              .c('tmtalk', {xmlns : xmlns});
    while (itemsInd--) {
      item = items[itemsInd];
      if (item.jid !== null) {
        iq.c('item', {jid : item.jid}).c('list').t(item.listname).up().up();
      } else {
        iq.c('item').c('list').t(item.listname).up().up();
      }
    }
    connectionManager.send(iq.tree());
  };

  var sendInvite = function (discoitem, roomjid, contactjid, reason) {
    connectionManager.send(
      $msg({id : getMessageId(), from : connectionManager.jid, to : roomjid})
        .c('x', {xmlns : Strophe.NS.MUC_USER})
        .c('invite', {to : contactjid})
        .c('reason').t(reason)
        .tree()
    );
  };

  var composingMessage = function (jid) {
    connectionManager.send(
      $msg({ id: getMessageId(), from: connectionManager.jid, to: jid, type: 'chat' })
        .c('composing', {xmlns : Strophe.NS.CHATSTATES})
        .tree()
    );
  };

  var pausedMessage = function (jid) {
    connectionManager.send(
      $msg({id : getMessageId(), from : connectionManager.jid, to : jid, type : 'chat'})
        .c('paused', {xmlns : Strophe.NS.CHATSTATES})
        .tree()
    );
  };

  var sendMulticastMessage = function (domain, jids, body) {
    var
      msg = null,
      jidsInd = jids.length;
    msg = $msg({id : getMessageId(), from : connectionManager.jid, to : domain})
            .c('addresses', {xmlns : Strophe.NS.ADDRESS});
    while (jidsInd--) {
      msg.c('address', {type : jidsInd === 0 ? 'to' : 'cc', jid : jids[jidsInd]}).up();
    }
    msg.up().c('body').t(body);
    connectionManager.send(msg.tree());
  };

  var sendMessage = function (jid, body, type) {
    var userName = escape(ASC.TMTalk.contactsManager.getContactName());
    connectionManager.send(
      $msg({ id: getMessageId(), from: connectionManager.jid, to: jid, username: userName, type: type })
        .c('active', {xmlns : Strophe.NS.CHATSTATES}).up()
        .c('body').t(body).up()
        .tree()
    );
  };

  var getRoomInfo = function (jid) {
    var discoitemJid = jid.substring(jid.indexOf('@') + 1);
    requestRoomInfo[jid] = {
      jid           : jid,
      discoitemJid  : discoitemJid
    };
    getDiscoItems(discoitemJid);
  };

  var sendCommand = function () {
    connectionManager.send(
      $iq({id : getMessageId(), from : connectionManager.jid, to : connectionManager.jid, type : 'set'})
        .c('command', {xmlns : Strophe.NS.COMMANDS, node : 'tm:open-chat'})
        .tree()
    );
  };

  var retrievesData = function (xmlns) {
    connectionManager.send(
      $iq({id : getMessageId(), type : 'get'})
       .c('query', {xmlns : Strophe.NS.PRIVATE})
          .c('tmtalk', {xmlns : xmlns})
          .tree()
    );
  };

  var getMessagesByDate = function (jid, from, to) {
    connectionManager.send(
      $iq({id : getMessageId(), from : connectionManager.jid, to : jid, type : 'get'})
        .c('query', {xmlns : Strophe.NS.HISTORY, from : from, to : to})
        .tree()
    );
  };

  var getMessagesByNumber = function (jid, count) {
    connectionManager.send(
      $iq({id : getMessageId(), from : connectionManager.jid, to : jid, type : 'get'})
        .c('query', {xmlns : Strophe.NS.HISTORY, count : count})
        .tree()
    );
  };
  var getMessagesByRange = function(jid, startindex, count, text) {
      connectionManager.send(
          $iq({ id: getMessageId(), from: connectionManager.jid, to: jid, type: 'get' })
          .c('query', { xmlns: Strophe.NS.HISTORY, count: count, startindex: startindex, text: Encoder.htmlEncode(text) })
          .tree()
      );
  };
    
  var clearUnreadMessage = function (jid, to) {
      connectionManager.send(
          $iq({ id: getMessageId(), from: connectionManager.jid, to: to, type: 'get' })
          .c('query', { xmlns: 'urn:xmpp:chat-markers:0'})
          .tree()
      );
  };

  var searchMessage = function (jid, text) {
      connectionManager.send(
        $iq({ id: getMessageId(), from: connectionManager.jid, to: jid, type: 'get' })
          .c('query', { xmlns: Strophe.NS.HISTORY, text: Encoder.htmlEncode(text) })
          .tree()
      );
  };

  var setSubject = function (jid, subject) {
    connectionManager.send(
      $msg({id : getMessageId(), to : jid, type : 'groupchat'})
        .c('subject').t(subject).up()
        .tree()
    );
  };

  var getDiscoInfo = function (jid) {
    connectionManager.send(
      $iq({id : getMessageId(), from : connectionManager.jid, to : jid, type : 'get'})
        .c('query', {xmlns : Strophe.NS.DISCO_INFO})
        .tree()
    );
  };

  var getDiscoItems = function (jid) {
    connectionManager.send(
      $iq({id : getMessageId(), from : connectionManager.jid, to : jid, type : 'get'})
        .c('query', {xmlns : Strophe.NS.DISCO_ITEMS})
        .tree()
    );
  };

  var getJid = function () {
    return Strophe.getBareJidFromJid(connectionManager.jid);
  };
  var getJID = function () {
      return JID;
  };
  var getDomain = function () {
    return connectionManager.domain;
  };

  var getUsername = function () {
    return Strophe.getNodeFromJid(connectionManager.jid);
  }

  return {
    init    : init,

    conflict: conflict,

    bind          : bind,
    unbind        : unbind,
    addIqHandler  : addIqHandler,
    sendIqMessage : sendIqMessage,

    status      : setStatus,
    connect     : connect,
    disconnect  : disconnect,
    terminate   : terminate,
    connected   : connected,

    events          : customEvents,
    statuses        : statuses,
    onlineStatusId  : onlineStatusId,
    offlineStatusId : offlineStatusId,

    creatingRoom          : creatingRoom,
    enteringRoom          : enteringRoom,
    exitingRoom           : exitingRoom,
    destroyingRoom        : destroyingRoom,
    getConfigurationForm  : getConfigurationForm,
    sendConfigurationForm : sendConfigurationForm,

    sendMailingLists  : sendMailingLists,

    kickingOccupant : kickingOccupant,

    sendInvite            : sendInvite,
    composingMessage      : composingMessage,
    pausedMessage         : pausedMessage,
    sendMulticastMessage  : sendMulticastMessage,
    sendMessage           : sendMessage,
    getRoomInfo           : getRoomInfo,
    sendCommand           : sendCommand,

    retrievesData : retrievesData,

    getMessagesByDate   : getMessagesByDate,
    getMessagesByNumber : getMessagesByNumber,
    getMessagesByRange: getMessagesByRange,
    clearUnreadMessage: clearUnreadMessage,

    searchMessage: searchMessage,

    setSubject  : setSubject,

    getDiscoInfo  : getDiscoInfo,
    getDiscoItems : getDiscoItems,

    getJid        : getJid,
    getDomain     : getDomain,
    getUsername: getUsername,

    getJID: getJID,

    savePushEndpoint: savePushEndpoint

  };
})();
