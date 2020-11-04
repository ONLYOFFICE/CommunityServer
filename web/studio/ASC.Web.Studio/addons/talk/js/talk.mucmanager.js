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

window.ASC.TMTalk.mucManager = (function () {
  var
    isInit = false,
    reValidSymbols = /^[äöüßña-žа-яё\w\s\.-]+$/i,
    discoitems = [],
    openedRooms = {},
    configurationForms = {},
    customEvents = {
      getRooms : 'ongetrooms',
      updateRoom : 'onupdateroom',
      updateConferences : 'onupdateconferences',
      comeContact : 'oncomecontact',
      leftContact : 'onleftcontact',
      createRoom : 'oncreateroom',
      destroyRoom : 'ondestroyroom',
      openRoom : 'onopenroom',
      closeRoom : 'oncloseroom',
      sentInvite : 'onsentinvite',
      recvInvite : 'onrecvinvite',
      acceptInvite : 'onacceptinvite',
      declineInvite : 'ondeclineinvite',
      updateSubject : 'onupdatesubject'
    },
    eventManager = new CustomEvent(customEvents);

  var sortByName = function (a, b) {
    if (!a.name) {
      return 1;
    }
    if (!b.name) {
      return -1;
    }
    return a.name === b.name ? 0 : a.name > b.name ? 1 : -1;
  };

  var getJid = function (name) {
    return name.replace(/\s+/g, '.');
  };

  var getName = function () {
    var login = ASC.TMTalk.contactsManager.getContactName();

    login = login.replace(/[\'\"\<\>\{\}\(\)\[\]\+\^\%\$\#\@\!\&\*\=\;\:\/\!\?\.\\]/g, '');

    //var strs = login.match(/(\w+)+/gi);
    //if (strs) {
    //  var
    //    str = '',
    //    strsInd = strs.length;
    //  while (strsInd--) {
    //    str = strs[strsInd];
    //    login = login.replace(str, str.charAt(0).toUpperCase() + str.substring(1));
    //  }
    //}
    return login;
  };

  var init = function (validsymbols) {
    if (isInit === true) {
      return undefined;
    }
    isInit = true;
    // TODO
    if (typeof validsymbols === 'string' && validsymbols.length > 0) {
      reValidSymbols = new RegExp('^[' + validsymbols + '\\\w\\s\\.-]+$', 'i');
    }

    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.enteringRoom, onEnteringRoom);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.creatingRoom, onCreatingRoom);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.exitingRoom, onExitingRoom);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.destroyingRoom, onDestroyingRoom);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.retrievesFeatures, onRetrievesFeatures);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.retrievesDiscoItems, onRetrievesDiscoItems);
  };

  var bind = function (eventName, handler, params) {
    return eventManager.bind(eventName, handler, params);
  };

  var unbind = function (handlerId) {
    return eventManager.unbind(handlerId);
  };

  var onRetrievesFeatures = function (domain, features) {
    if (domain === ASC.TMTalk.connectionManager.getDomain()) {
      var featuresInd = features.length;
      while (featuresInd--) {
        switch (features[featuresInd].getAttribute('var')) {
          case Strophe.NS.DISCO_ITEMS :
            ASC.TMTalk.connectionManager.getDiscoItems(domain);
            break;
        }
      }
    } else {
      var
        mucCupported = false,
        featuresInd = features.length;
      while (featuresInd--) {
        switch (features[featuresInd].getAttribute('var')) {
          case Strophe.NS.MUC :
            mucCupported = true;
            break;
        }
      }
      if (mucCupported === false) {
        var discoitemsInd = discoitems.length;
        while (discoitemsInd--) {
          if (discoitems[discoitemsInd].jid === domain) {
            discoitems.splice(discoitemsInd, 1);
            break;
          }
        }
      }
    }
  };

  var onRetrievesDiscoItems = function (domain, items) {
    if (ASC.TMTalk.properties.item('enabledConferences') !== 'true') {
      return undefined;
    }
    if (domain === ASC.TMTalk.connectionManager.getDomain()) {
      fillConferences(items);
    } else {
      var discoitemsInd = 0;
      discoitemsInd = discoitems.length;
      while (discoitemsInd--) {
        if (discoitems[discoitemsInd].jid === domain) {
          fillRooms(domain, items);
          ASC.TMTalk.connectionManager.getDiscoInfo(domain);
          break;
        }
      }
      if (discoitemsInd === -1) {
        var
          rooms = null,
          roomsInd = 0;
        discoitemsInd = discoitems.length;
        while (discoitemsInd--) {
          rooms = discoitems[discoitemsInd].rooms;
          roomsInd = rooms.length;
          while (roomsInd--) {
            if (rooms[roomsInd].jid === domain) {
              fillContacts(domain, items);
              break;
            }
          }
        }
      }
    }
  };

  var onCreatingRoom = function (presence) {
    var
      realjid = '',
      role = 'none',
      affiliation = 'none',
      from = presence.getAttribute('from'),
      roomjid = from.substring(0, from.indexOf('/')).toLowerCase(),
      roomname = roomjid.substring(0, roomjid.indexOf('@')),
      item = null;

    item = presence.getElementsByTagName('item');
    if (item,length === 0) {
      item = Strophe.getElementsByTagName(presence, 'item');
    }
    if (item.length > 0) {
      role = item[0].getAttribute('role');
      realjid = item[0].getAttribute('jid');
      affiliation = item[0].getAttribute('affiliation');
    }

    if (configurationForms.hasOwnProperty(roomjid)) {
      roomname = configurationForms[roomjid].name;
    }

    room = {
      jid             : roomjid,
      name            : roomname,
      ownjid          : from,
      ownrole         : role,
      ownaffiliation  : affiliation,
      nick            : from.substring(from.indexOf('/') + 1),
      subject         : '',
      isPersistent    : true,
      contacts        : []
    };

    var
      rooms = null,
      roomsInd = -1,
      discoitemsInd = discoitems.length,
      discoitemJid = roomjid.substring(roomjid.indexOf('@') + 1);

    while (discoitemsInd--) {
      if (discoitems[discoitemsInd].jid === discoitemJid) {
        rooms = discoitems[discoitemsInd].rooms;
        roomsInd = rooms.length;
        while (roomsInd--) {
          if (rooms[roomsInd].jid === roomsInd) {
            break;
          }
        }
        break;
      }
    }

    if (discoitemsInd !== -1 && roomsInd === -1) {
      discoitems[discoitemsInd].rooms.push(room);
      eventManager.call(customEvents.createRoom, window, [roomjid, room]);

      ASC.TMTalk.connectionManager.getConfigurationForm(roomjid);
    }
  };

  var onDestroyingRoom = function (presence) {
    var
      jid = '',
      from = presence.getAttribute('from'),
      roomjid = from.substring(0, from.indexOf('/')).toLowerCase(),
      rooms = null,
      roomsInd = -1,
      discoitemsInd = discoitems.length,
      discoitemJid = roomjid.substring(roomjid.indexOf('@') + 1);

    delete openedRooms[roomjid];

    while (discoitemsInd--) {
      if (discoitems[discoitemsInd].jid === discoitemJid) {
        rooms = discoitems[discoitemsInd].rooms;
        roomsInd = rooms.length;
        while (roomsInd--) {
          if (rooms[roomsInd].jid === roomjid) {
            jid = rooms[roomsInd].ownjid;
            rooms.splice(roomsInd, 1);
            break;
          }
        }
        break;
      }
    }

    if (roomsInd !== -1) {
      eventManager.call(customEvents.closeRoom, window, [roomjid, jid]);
      eventManager.call(customEvents.destroyRoom, window, [roomjid, jid]);
    }
  };

  var onEnteringRoom = function (presence) {
    var
      room = null,
      realjid = '',
      role = 'none',
      affiliation = 'none',
      from = presence.getAttribute('from'),
      roomjid = from.substring(0, from.indexOf('/')).toLowerCase(),
      roomname = roomjid.substring(0, roomjid.indexOf('@')),
      item = null;

    if (configurationForms.hasOwnProperty(roomjid)) {
      roomname = configurationForms[roomjid].name;
    }

    item = presence.getElementsByTagName('item');
    if (item,length === 0) {
      item = Strophe.getElementsByTagName(presence, 'item');
    }
    if (item.length > 0) {
      role = item[0].getAttribute('role');
      realjid = item[0].getAttribute('jid');
      affiliation = item[0].getAttribute('affiliation');
    }

    room = getRoom(roomjid);

    if (room === null) {
      room = {
        jid             : roomjid,
        name            : roomname,
        ownjid          : from,
        ownrole         : role,
        ownaffiliation  : affiliation,
        nick            : from.substring(from.indexOf('/') + 1),
        subject         : '',
        isPersistent    : true,
        contacts        : []
      };

      var
        rooms = null,
        roomsInd = -1,
        discoitemsInd = discoitems.length,
        discoitemJid = roomjid.substring(roomjid.indexOf('@') + 1);

      while (discoitemsInd--) {
        if (discoitems[discoitemsInd].jid === discoitemJid) {
          rooms = discoitems[discoitemsInd].rooms;
          roomsInd = rooms.length;
          while (roomsInd--) {
            if (rooms[roomsInd].jid === roomsInd) {
              break;
            }
          }
          break;
        }
      }

      if (discoitemsInd !== -1 && roomsInd === -1) {
        discoitems[discoitemsInd].rooms.push(room);
        eventManager.call(customEvents.createRoom, window, [roomjid, room]);
      }
    }

    if (room !== null) {
      room.ownjid         = from;
      room.ownrole        = role;
      room.ownaffiliation = affiliation;
      //room.name           = roomjid.substring(0, roomjid.indexOf('@'));
      room.nick           = from.substring(from.indexOf('/') + 1);
      room.subject        = '';

      openedRooms[roomjid] = {
        ownjid          : from,
        ownrole         : role,
        ownaffiliation  : affiliation,
        jid             : roomjid,
        name            : room.name,
        nick            : room.nick,
        subject         : '',
        openingdate     : new Date()
      };

      eventManager.call(customEvents.openRoom, window, [roomjid, room]);

      var contacts = room.contacts;
      for (var i = 0, n = contacts.length; i < n; i++) {
        eventManager.call(customEvents.comeContact, window, [roomjid, contacts[i], room.ownjid === contacts[i].jid]);
      }
    }
  };

  var onExitingRoom = function (presence) {
    var
      from = presence.getAttribute('from'),
      roomjid = from.substring(0, from.indexOf('/')).toLowerCase();

    if (openedRooms.hasOwnProperty(roomjid)) {
      var
        willBeEemoved = false,
        jid = openedRooms[roomjid].ownjid,
        room = null,
        contacts = null,
        contactsInd = 0;

      if ((room = getRoom(roomjid)) !== null) {
        willBeEemoved = room.isPersistent === false && (room.contacts.length === 0 || (room.contacts.length === 1 && room.contacts[0].jid === room.ownjid));

        room.ownjid         = '';
        room.ownrole        = 'none';
        room.ownaffiliation = 'none';
        //room.name           = '';
        room.nick           = '';
        room.subject        = '';
        room.contacts       = [];

        var
          contacts = room.contacts,
          contactsInd = contacts.length;
        while (contactsInd--) {
          eventManager.call(customEvents.leftContact, window, [roomjid, contacts[contactsInd]]);
        }
      }

      delete openedRooms[roomjid];
      eventManager.call(customEvents.closeRoom, window, [roomjid, jid]);

      if (willBeEemoved) {
        var
          jid = '',
          rooms = null,
          roomsInd = -1,
          discoitemsInd = discoitems.length,
          discoitemJid = roomjid.substring(roomjid.indexOf('@') + 1);

        while (discoitemsInd--) {
          if (discoitems[discoitemsInd].jid === discoitemJid) {
            rooms = discoitems[discoitemsInd].rooms;
            roomsInd = rooms.length;
            while (roomsInd--) {
              if (rooms[roomsInd].jid === roomjid) {
                jid = rooms[roomsInd].ownjid;
                rooms.splice(roomsInd, 1);
                break;
              }
            }
            break;
          }
        }

        if (roomsInd !== -1) {
          eventManager.call(customEvents.destroyRoom, window, [roomjid, jid]);
        }
      }
    }
  };

  var setSubject = function (roomjid, subject) {
    if (typeof subject !== 'string') {
      subject = '';
    }
    ASC.TMTalk.connectionManager.setSubject(roomjid, subject);
  };

  var updateSubject = function (roomjid, subject) {
    var
      discoitemsInd = discoitems.length,
      discoitemJid = roomjid.substring(roomjid.indexOf('@') + 1);

    while (discoitemsInd--) {
      if (discoitems[discoitemsInd].jid === discoitemJid) {
        var
          rooms = discoitems[discoitemsInd].rooms,
          roomsInd = rooms.length;
        while (roomsInd--) {
          if (rooms[roomsInd].jid === roomjid) {
            rooms[roomsInd].subject = subject;
            break;
          }
        }
        break;
      }
    }

    eventManager.call(customEvents.updateSubject, window, [roomjid, subject]);
  };

  var fillConferences = function (items) {
    discoitems = [];

    var
      item = null,
      itemsInd = 0,
      discoitem = null;

    itemsInd = items.length;
    while (itemsInd--) {
      item = items[itemsInd];
      discoitem = {
        jid       : item.getAttribute('jid'),
        name      : item.getAttribute('name'),
        rooms     : [],
        noloaded  : true
      };
      discoitems.push(discoitem);
      ASC.TMTalk.connectionManager.getDiscoItems(discoitem.jid);
    }
    eventManager.call(customEvents.updateConferences, window, [discoitems]);
  };

  var fillRooms = function (jid, items) {
    var
      item = null,
      itemsInd = 0,
      room = null,
      rooms = [];

    itemsInd = items.length;
    while (itemsInd--) {
      item = items[itemsInd];
      room = {
        jid             : item.getAttribute('jid'),
        name            : item.getAttribute('name'),
        ownjid          : '',
        ownrole         : 'none',
        ownaffiliation  : 'none',
        nick            : '',
        subject         : '',
        isPersistent    : true,
        contacts        : []
      };
      rooms.push(room);
    }

    var
      allLoaded = true,
      discoitemsInd = 0;
    discoitemsInd = discoitems.length;
    while (discoitemsInd--) {
      if (discoitems[discoitemsInd].jid === jid) {
        discoitems[discoitemsInd].rooms = rooms;
        delete discoitems[discoitemsInd].noloaded;
      }
      if (discoitems[discoitemsInd].hasOwnProperty('noloaded')) {
        allLoaded = false;
      }
    }

    if (allLoaded === true) {
      var allrooms = [];
      discoitemsInd = discoitems.length;
      while (discoitemsInd--) {
        allrooms = allrooms.concat(discoitems[discoitemsInd].rooms);
      }
      allrooms.sort(sortByName);
      eventManager.call(customEvents.getRooms, window, [discoitems, allrooms]);
    }
  };

  var getRoomInfo = function (roomjid, item) {
    var
      name = item.getAttribute('name');

    if (configurationForms.hasOwnProperty(roomjid)) {
      configurationForms[roomjid].name = name;

      var
        rooms = null,
        roomsInd = -1,
        discoitemsInd = discoitems.length,
        discoitemJid = roomjid.substring(roomjid.indexOf('@') + 1);

      while (discoitemsInd--) {
        if (discoitems[discoitemsInd].jid === discoitemJid) {
          rooms = discoitems[discoitemsInd].rooms;
          roomsInd = rooms.length;
          while (roomsInd--) {
            if (rooms[roomsInd].jid === roomjid) {
              rooms[roomsInd].name = name;
              break;
            }
          }
          break;
        }
      }

      if (configurationForms[roomjid].hasOwnProperty('inviterjid')) {
        var inviterjid = configurationForms[roomjid].inviterjid;
        eventManager.call(customEvents.acceptInvite, window, [roomjid, name, inviterjid]);
      }
    }
  }

  var sendConfigurationForm = function (roomjid, fields) {
    var
      room = null,
      nodes = null,
      field = null,
      fieldsInd = 0,
      formfields = [];

    room = getRoom(roomjid);
    fieldsInd = fields.length;
    while (fieldsInd--) {
      switch (fields[fieldsInd].getAttribute('var')) {
        case 'muc#roomconfig_roomtitle' :
          if (configurationForms.hasOwnProperty(roomjid)) {
            if (room !== null) {
              room.name = configurationForms[roomjid].name;
            }
            formfields.push({name : 'muc#roomconfig_roomtitle', value : configurationForms[roomjid].name});
            break;
          }
        case 'muc#roomconfig_persistentroom' :
          if (configurationForms.hasOwnProperty(roomjid)) {
            if (room !== null) {
              room.isPersistent = configurationForms[roomjid].persistent;
            }
            formfields.push({name : 'muc#roomconfig_persistentroom', value : configurationForms[roomjid].persistent ? '1' : '0'});
            break;
          }
        default :
          field = fields[fieldsInd];
          nodes = field.getElementsByTagName('value');
          if (nodes.length === 0) {
            nodes = Strophe.getElementsByTagName(field, 'value');
          }
          if (nodes.length > 0) {
            formfields.push({name : field.getAttribute('var'), value : ASC.TMTalk.dom.getContent(nodes[0])});
          }
          break;
      }
    }

    if (formfields.length > 0) {
      eventManager.call(customEvents.updateRoom, window, [roomjid, room]);
      ASC.TMTalk.connectionManager.sendConfigurationForm(roomjid, formfields);
    }
  };

  var comeContact = function (presence) {
    var
      room = null,
      contact = null,
      realjid = '',
      role = 'none',
      affiliation = 'none',
      from = presence.getAttribute('from'),
      roomjid = from.substring(0, from.indexOf('/')).toLowerCase(),
      roomname = roomjid.substring(0, roomjid.indexOf('@')),
      item = null;

    if (configurationForms.hasOwnProperty(roomjid)) {
      roomname = configurationForms[roomjid].name;
    }

    item = presence.getElementsByTagName('item');
    if (item.length === 0) {
      item = Strophe.getElementsByTagName(presence, 'item');
    }
    if (item.length > 0) {
      role = item[0].getAttribute('role');
      realjid = item[0].getAttribute('jid');
      affiliation = item[0].getAttribute('affiliation');
    }
    contact = {
      name        : from.substring(from.indexOf('/') + 1),
      jid         : from,
      role        : role,
      affiliation : affiliation,
      realjid     : realjid,
      roomjid     : roomjid
    };

    room = getRoom(roomjid);

    if (room === null) {
      room = {
        jid             : roomjid,
        name            : roomname,
        ownjid          : '',
        ownrole         : 'none',
        ownaffiliation  : 'none',
        nick            : '',
        subject         : '',
        isPersistent    : true,
        contacts        : []
      };

      var
        rooms = null,
        roomsInd = -1,
        discoitemsInd = discoitems.length,
        discoitemJid = roomjid.substring(roomjid.indexOf('@') + 1);

      while (discoitemsInd--) {
        if (discoitems[discoitemsInd].jid === discoitemJid) {
          rooms = discoitems[discoitemsInd].rooms;
          roomsInd = rooms.length;
          while (roomsInd--) {
            if (rooms[roomsInd].jid === roomsInd) {
              break;
            }
          }
          break;
        }
      }

      if (discoitemsInd !== -1 && roomsInd === -1) {
        discoitems[discoitemsInd].rooms.push(room);
        eventManager.call(customEvents.createRoom, window, [roomjid, room]);
      }
    }

    if (room !== null) {
      var
        contacts = room.contacts,
        contactsInd = contacts.length;

      while (contactsInd--) {
        if (contacts[contactsInd].jid === from) {
          break;
        }
      }

      if (contactsInd === -1) {
        room.contacts.push(contact);

        eventManager.call(customEvents.comeContact, window, [roomjid, contact, room.ownjid === contact.jid]);
      }
    }
  };

  var leftContact = function (presence) {
    var
      room = null,
      from = presence.getAttribute('from'),
      roomjid = from.substring(0, from.indexOf('/')).toLowerCase();

    if ((room = getRoom(roomjid)) !== null) {
      var
        contacts = room.contacts,
        contactsInd = contacts.length;

      while (contactsInd--) {
        if (contacts[contactsInd].jid === from) {
          eventManager.call(customEvents.leftContact, window, [roomjid, contacts[contactsInd]]);
          contacts.splice(contactsInd, 1);
          break;
        }
      }
    }
  };

  var createRoom = function (roomname, isTemporary) {
    if (discoitems.length === 0) {
      return undefined;
    }
    var
      room = null,
      roomjid = getJid(roomname).toLowerCase() + '@' + discoitems[0].jid,
      jid = roomjid + '/' + getName();

    if (!openedRooms.hasOwnProperty(roomjid)) {
      if (!configurationForms.hasOwnProperty(roomjid)) {
        configurationForms[roomjid] = {};
      }
      configurationForms[roomjid].persistent = !isTemporary;
      configurationForms[roomjid].name = roomname;

      ASC.TMTalk.connectionManager.creatingRoom(jid);
    } else {
      if ((room = getRoom(roomjid)) !== null) {
        eventManager.call(customEvents.openRoom, window, [roomjid, room]);
      }
    }
  };

  var openRoom = function (roomjid) {
    var
      room = null,
      jid = (roomjid = roomjid.toLowerCase()) + '/' + getName();

    if (!openedRooms.hasOwnProperty(roomjid)) {
      ASC.TMTalk.connectionManager.enteringRoom(jid);
    } else {
      if ((room = getRoom(roomjid)) !== null) {
        eventManager.call(customEvents.openRoom, window, [roomjid, room]);
      }
    }
  };

  var closeRoom = function (roomjid) {
    if (openedRooms.hasOwnProperty(roomjid)) {
      ASC.TMTalk.connectionManager.exitingRoom(openedRooms[roomjid].ownjid);
    }
  };

  var removeRoom = function (roomjid, reason) {
    reason = typeof reason === 'string' ? reason : '';
    ASC.TMTalk.connectionManager.destroyingRoom(roomjid, reason);
  };

  var closeRooms = function () {
    //for (var roomjid in openedRooms) {
    //  if (openedRooms.hasOwnProperty(roomjid)) {
    //    ASC.TMTalk.connectionManager.exitingRoom(openedRooms[roomjid].ownjid);
    //  }
    //}

    for (var roomjid in openedRooms) {
      if (openedRooms.hasOwnProperty(roomjid)) {
        var
          jid = openedRooms[roomjid].ownjid,
          room = null,
          contacts = null,
          contactsInd = 0;

        if ((room = getRoom(roomjid)) !== null) {
          room.ownjid         = '';
          room.ownrole        = 'none';
          room.ownaffiliation = 'none';
          room.name           = '';
          room.nick           = '';
          room.subject        = '';
          room.contacts       = [];

          var
            contacts = room.contacts,
            contactsInd = contacts.length;
          while (contactsInd--) {
            eventManager.call(customEvents.leftContact, window, [roomjid, contacts[contactsInd]]);
          }
        }

        delete openedRooms[roomjid];
        eventManager.call(customEvents.closeRoom, window, [roomjid, jid]);
      }
    }
  };

  var sendInvite = function (roomjid, contactjid, reason) {
    var discoitem = null;
    if ((discoitem = getDiscoItem(roomjid)) === null) {
      return undefined;
    }
    reason = typeof reason === 'string' ? reason : '';
    ASC.TMTalk.connectionManager.sendInvite(discoitem.jid, roomjid, contactjid, reason);
    eventManager.call(customEvents.sentInvite, window, [roomjid, contactjid]);
  };

  var recvInvite = function (roomjid, inviterjid, reason) {
    if (!openedRooms.hasOwnProperty(roomjid)) {
      eventManager.call(customEvents.recvInvite, window, [roomjid, inviterjid, reason]);
    }
  };

  var acceptInvite = function (roomjid, inviterjid) {
    var
      room = null,
      realjid = '',
      role = 'none',
      name = roomjid.substring(0, roomjid.indexOf('@')),
      affiliation = 'none',
      from = '';

    room = {
      jid             : roomjid,
      name            : name,
      ownjid          : from,
      ownrole         : role,
      ownaffiliation  : affiliation,
      nick            : from.substring(from.indexOf('/') + 1),
      subject         : '',
      contacts        : []
    };

    var
      rooms = null,
      roomsInd = -1,
      discoitemsInd = discoitems.length,
      discoitemJid = roomjid.substring(roomjid.indexOf('@') + 1);

    while (discoitemsInd--) {
      if (discoitems[discoitemsInd].jid === discoitemJid) {
        rooms = discoitems[discoitemsInd].rooms;
        roomsInd = rooms.length;
        while (roomsInd--) {
          if (rooms[roomsInd].jid === roomjid) {
            break;
          }
        }
        break;
      }
    }

    if (discoitemsInd !== -1 && roomsInd === -1) {
      if (!configurationForms.hasOwnProperty(roomjid)) {
        configurationForms[roomjid] = {};
      }
      configurationForms[roomjid].persistent = false;
      configurationForms[roomjid].name = name;
      configurationForms[roomjid].inviterjid = inviterjid;

      ASC.TMTalk.connectionManager.getRoomInfo(roomjid);
    } else {
      eventManager.call(customEvents.acceptInvite, window, [roomjid, name, inviterjid]);
    }
  };

  var declineInvite = function (roomjid, inviterjid) {
    var
      name = roomjid.substring(0, roomjid.indexOf('@'));

    eventManager.call(customEvents.declineInvite, window, [roomjid, name, inviterjid]);
  };

  var kickContact = function (jid, reason) {
    if (typeof jid !== 'string' || jid.length === 0) {
      return undefined;
    }
    reason = typeof reason === 'string' ? reason : '';
    var
      nick = jid.substring(jid.indexOf('/') + 1),
      roomjid = jid.substring(0, jid.indexOf('/'));
    if (roomjid && nick) {
      ASC.TMTalk.connectionManager.kickingOccupant(roomjid, nick, reason);
    }
  };

  var getDiscoItems = function () {
    return discoitems;
  };

  var getDiscoItem = function (roomjid) {
    var
      discoitemsInd = discoitems.length,
      discoitemJid = roomjid.substring(roomjid.indexOf('@') + 1);
    while (discoitemsInd--) {
      if (discoitems[discoitemsInd].jid === discoitemJid) {
        var
          rooms = discoitems[discoitemsInd].rooms,
          roomsInd = rooms.length;
        while (roomsInd--) {
          if (rooms[roomsInd].jid === roomjid) {
            return discoitems[discoitemsInd];
          }
        }
      }
    }
    return null;
  };

  var getRoom = function (roomjid) {
    var
      discoitemsInd = discoitems.length,
      discoitemJid = roomjid.substring(roomjid.indexOf('@') + 1);

    while (discoitemsInd--) {
      if (discoitems[discoitemsInd].jid === discoitemJid) {
        var
          rooms = discoitems[discoitemsInd].rooms,
          roomsInd = rooms.length;
        while (roomsInd--) {
          if (rooms[roomsInd].jid === roomjid) {
            return rooms[roomsInd];
          }
        }
        break;
      }
    }
    return null;
  };

  var getContact = function (roomjid, realjid) {
    var room = null;
    if ((room = getRoom(roomjid)) !== null) {
      var
        somejid = '',
        contacts = room.contacts,
        contactsInd = contacts.length;
      while (contactsInd--) {
        somejid = contacts[contactsInd].realjid;
        if (realjid === somejid.substring(0, somejid.indexOf('/'))) {
          return contacts[contactsInd];
        }
      }
    }
    return null;
  };

  var getContacts = function (roomjid) {
    var
      somecontacts = [],
      room = null;
    if ((room = getRoom(roomjid)) !== null) {
      var
        contacts = room.contacts,
        contactsInd = contacts.length;
      while (contactsInd--) {
        if (contacts[contactsInd].jid !== room.ownjid) {
          somecontacts.push(contacts[contactsInd]);
        }
      }
    }
    return somecontacts;
  };

  var getContactName = function (jid) {
    var
      discoitemsInd = discoitems.length,
      roomjid = jid.substring(0, jid.indexOf('/')),
      discoitemJid = roomjid.substring(roomjid.indexOf('@') + 1);

    while (discoitemsInd--) {
      if (discoitems[discoitemsInd].jid === discoitemJid) {
        var
          rooms = discoitems[discoitemsInd].rooms,
          roomsInd = rooms.length;
        while (roomsInd--) {
          if (rooms[roomsInd].jid === roomjid) {
            var
              contacts = rooms[roomsInd].contacts,
              contactsInd = contacts.length;
            while (contactsInd--) {
              if (contacts[contactsInd].jid === jid) {
                return ASC.TMTalk.contactsManager.getContactName(contacts[contactsInd].realjid);
              }
            }
            break;
          }
        }
        break;
      }
    }
    return jid.substring(jid.indexOf('/') + 1);
  };

  var getConferenceName = function (roomjid) {
    var
      discoitemsInd = discoitems.length,
      discoitemJid = roomjid.substring(roomjid.indexOf('@') + 1);

    while (discoitemsInd--) {
      if (discoitems[discoitemsInd].jid === discoitemJid) {
        var
          rooms = discoitems[discoitemsInd].rooms,
          roomsInd = rooms.length;
        while (roomsInd--) {
          if (rooms[roomsInd].jid === roomjid) {
            return rooms[roomsInd].name;
          }
        }
        break;
      }
    }
    return roomjid;
  };

  var getConferenceSubject = function (roomjid) {
    var
      discoitemsInd = discoitems.length,
      discoitemJid = roomjid.substring(roomjid.indexOf('@') + 1);

    while (discoitemsInd--) {
      if (discoitems[discoitemsInd].jid === discoitemJid) {
        var
          rooms = discoitems[discoitemsInd].rooms,
          roomsInd = rooms.length;
        while (roomsInd--) {
          if (rooms[roomsInd].jid === roomjid) {
            return rooms[roomsInd].subject;
          }
        }
        break;
      }
    }
    return '';
  };

  var isMe = function (jid) {
    var roomjid = jid.substring(0, jid.indexOf('/')).toLowerCase();
    if (openedRooms.hasOwnProperty(roomjid)) {
      return openedRooms[roomjid].ownjid === jid;
    }
    return false;
  };

  var getOpeningDate = function (roomjid) {
    return openedRooms.hasOwnProperty(roomjid) ? openedRooms[roomjid].openingdate : new Date(0);
  };

  var roomIsOpening = function (roomjid) {
    return openedRooms.hasOwnProperty(roomjid);
  };

  var isValidName = function (roomname) {
    return reValidSymbols.test(roomname);
  };

  return {
    init  : init,

    events  : customEvents,

    bind    : bind,
    unbind  : unbind,

    fillConferences : fillConferences,
    fillRooms       : fillRooms,

    getRoomInfo : getRoomInfo,

    sendConfigurationForm : sendConfigurationForm,

    comeContact : comeContact,
    leftContact : leftContact,

    setSubject    : setSubject,
    updateSubject : updateSubject,

    createRoom  : createRoom,
    openRoom    : openRoom,
    closeRoom   : closeRoom,
    removeRoom  : removeRoom,
    closeRooms  : closeRooms,

    sendInvite    : sendInvite,
    recvInvite    : recvInvite,
    acceptInvite  : acceptInvite,
    declineInvite : declineInvite,
    kickContact   : kickContact,

    getDiscoItems : getDiscoItems,
    getDiscoItem  : getDiscoItem,
    getContacts   : getContacts,
    getContact    : getContact,
    getRoom       : getRoom,

    getContactName        : getContactName,
    getConferenceName     : getConferenceName,
    getConferenceSubject  : getConferenceSubject,

    isMe          : isMe,
    isValidName   : isValidName,
    roomIsOpening : roomIsOpening,
    getOpeningDate : getOpeningDate
  };
})();
