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

window.ASC.TMTalk.msManager = (function () {
  var
    isInit = false,
    reValidSymbols = /^[äöüßña-žа-яё\w\s\.-]+$/i,
    listIds = {},
    mailingLists = [],
    nsMailingLists = 'tmtalk:mailinglists',
    customEvents = {
      getLists : 'ongetlists',
      updateList : 'onupdatelist',
      addContact : 'onaddcontact',
      removeContact : 'onremovecontact',
      createList : 'oncreatelist',
      removeList : 'onremovelist',
      openList : 'onopenlist',
      sentMessage : 'onsentmessage'
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

  var getUniqueId = function (o, prefix) {
    var
      iterCount = 0,
      maxIterations = 1000,
      uniqueId = (typeof prefix !== 'undefined' ? prefix + '-' : '') + Math.floor(Math.random() * 1000000);
    while (o.hasOwnProperty(uniqueId) && iterCount++ < maxIterations) {
      uniqueId = (typeof prefix !== 'undefined' ? prefix + '-' : '') + Math.floor(Math.random() * 1000000);
    }
    return uniqueId;
  };

  var getListById = function (listId) {
    var mailingListsInd = mailingLists.length;
    while (mailingListsInd--) {
      if (mailingLists[mailingListsInd].id === listId) {
        return mailingLists[mailingListsInd];
      }
    }
    return null;
  };

  var getListByName = function (name) {
    var mailingListsInd = mailingLists.length;
    while (mailingListsInd--) {
      if (mailingLists[mailingListsInd].name === name) {
        return mailingLists[mailingListsInd];
      }
    }
    return null;
  };

  var storeMailingLists = function () {
    var
      items = [],
      listname = '',
      contacts = null,
      contactsInd = 0,
      mailingListsInd = mailingLists.length;

    while (mailingListsInd--) {
      listname = mailingLists[mailingListsInd].name;
      contacts = mailingLists[mailingListsInd].contacts;
      contactsInd = contacts.length;
      if (contactsInd === 0) {
        items.push({jid : null, listname : listname})
      }
      while (contactsInd--) {
        items.push({jid : contacts[contactsInd].jid, listname : listname});
      }
    }

    ASC.TMTalk.connectionManager.sendMailingLists(nsMailingLists, items);
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

    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.retrievesData, onRetrievesData);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.retrievesFeatures, onRetrievesFeatures);
  };

  var bind = function (eventName, handler, params) {
    return eventManager.bind(eventName, handler, params);
  };

  var unbind = function (handlerId) {
    return eventManager.unbind(handlerId);
  };

  var onRetrievesData = function (storages) {
    var storagesInd = storages.length;
    while (storagesInd--) {
      if (storages[storagesInd].getAttribute('xmlns') === nsMailingLists) {
        var
          items = null,
          storage = storages[storagesInd];

        items = storage.getElementsByTagName('item');
        if (items.length === 0) {
          items = Strophe.getElementsByTagName(storage, 'item');
        }
        if (items.length > 0 && mailingLists.length === 0) {
          fillLists(items);
        }
        break;
      }
    }
  };

  var onRetrievesFeatures = function (domain, features) {
    if (domain === ASC.TMTalk.connectionManager.getDomain()) {
      var featuresInd = features.length;
      while (featuresInd--) {
        switch (features[featuresInd].getAttribute('var')) {
          case Strophe.NS.PRIVATE :
            if (ASC.TMTalk.properties.item('enabledMassend') === 'true') {
              ASC.TMTalk.connectionManager.retrievesData(nsMailingLists);
            }
            break;
        }
      }
    }
  };

  var fillLists = function (items) {
    var
      jid = '',
      contact = null,
      nodes = null,
      item = null,
      itemsInd = 0,
      listname = '',
      mailingListsInd = 0;

    itemsInd = items.length;
    while (itemsInd--) {
      item = items[itemsInd];
      nodes = item.getElementsByTagName('list');
      if (nodes.length === 0) {
        nodes = Strophe.getElementsByTagName(item, 'list');
      }
      listname = nodes.length > 0 ? ASC.TMTalk.dom.getContent(nodes[0]) : '';
      jid = item.getAttribute('jid');

      contact = !jid || listname.length === 0 ? null : {jid : jid, list : listname};

      mailingListsInd = mailingLists.length;
      while (mailingListsInd--) {
        if (mailingLists[mailingListsInd].name === listname) {
          if (contact !== null) {
            mailingLists[mailingListsInd].contacts.push(contact);
          }
          break;
        }
      }
      if (mailingListsInd === -1) {
        mailingLists.push({
          id        : getUniqueId(listIds, 'list'),
          name      : listname,
          contacts  : contact !== null ? [contact] : []
        });
      }
    }

    mailingLists.sort(sortByName);

    eventManager.call(customEvents.getLists, window, [mailingLists]);
  };

  var createList = function (listName) {
    var mailingListsInd = mailingLists.length;
    while (mailingListsInd--) {
      if (mailingLists[mailingListsInd].name === listName) {
        break;
      }
    }
    if (mailingListsInd === -1) {
      mailingLists.push(list = {
        id        : getUniqueId(listIds, 'list'),
        name      : listName,
        contacts  : []
      });
      storeMailingLists();

      eventManager.call(customEvents.createList, window, [list.id, list]);
      eventManager.call(customEvents.openList, window, [list.id, list]);
    } else {
      var list = mailingLists[mailingListsInd];
      eventManager.call(customEvents.openList, window, [list.id, list]);

      var contacts = list.contacts;
      for (var i = 0, n = contacts.length; i < n; i++) {
        eventManager.call(customEvents.addContact, window, [list.id, contacts[i].jid]);
      }
    }
  };

  var removeList = function (listId) {
    var mailingListsInd = mailingLists.length;
    while (mailingListsInd--) {
      if (mailingLists[mailingListsInd].id === listId) {
        break;
      }
    }
    if (mailingListsInd !== -1) {
      mailingLists.splice(mailingListsInd, 1);
      storeMailingLists();

      eventManager.call(customEvents.removeList, window, [listId]);
    }
  };

  var openList = function (listId) {
    var list = null;

    if ((list = getListById(listId)) !== null) {
      eventManager.call(customEvents.openList, window, [list.id, list]);

      var contacts = list.contacts;
      if (contacts.length > 0) {
          jQuery('div#talkRoomsContainer ul.rooms li.room.current').addClass('minimized');
      }
      for (var i = 0, n = contacts.length; i < n; i++) {
        eventManager.call(customEvents.addContact, window, [list.id, contacts[i].jid]);
      }
    }
  };

  var addContact = function (listId, jid) {
    if (jid === ASC.TMTalk.connectionManager.getDomain()) {
      return undefined;
    }

    var list = null;

    if ((list = getListById(listId)) !== null) {
      var
        contacts = list.contacts,
        contactsInd = contacts.length;

      while (contactsInd--) {
        if (contacts[contactsInd].jid === jid) {
          break;
        }
      }

      if (contactsInd === -1) {
        list.contacts.push({
          jid   : jid,
          list  : list.name
        });
        eventManager.call(customEvents.addContact, window, [list.id, jid]);
      }
    }
  };

  var removeContact = function (listId, jid) {
    var list = null;

    if ((list = getListById(listId)) !== null) {
      var
        contacts = list.contacts,
        contactsInd = contacts.length;

      while (contactsInd--) {
        if (contacts[contactsInd].jid === jid) {
          contacts.splice(contactsInd, 1);

          eventManager.call(customEvents.removeContact, window, [list.id, jid]);
          break;
        }
      }
    }
  };

  var sendMessage = function (listId, message) {
    var list = null;

    if ((list = getListById(listId)) !== null && typeof message === 'string' && message.length > 0) {
      var
        jids = [],
        domain = ASC.TMTalk.connectionManager.getDomain(),
        contacts = list.contacts,
        contactsInd = contacts.length;

      while (contactsInd--) {
        if (!domain || domain !== contacts[contactsInd].jid) {
          jids.push(contacts[contactsInd].jid);
        }
      }

      if (jids.length > 0) {
        ASC.TMTalk.messagesManager.sendMultiuserMessage(jids, message);

        var date = new Date();
        eventManager.call(customEvents.sentMessage, window, [listId, ASC.TMTalk.contactsManager.getContactName(), ASC.TMTalk.messagesManager.getDisplayDate(date), date, message]);
      }
    }
  };

  var getContacts = function (listId) {
    if (typeof listId === 'undefined') {
      return [];
    }
    var mailingListsInd = mailingLists.length;
    while (mailingListsInd--) {
      if (mailingLists[mailingListsInd].id === listId) {
        return mailingLists[mailingListsInd].contacts;
      }
    }
    return [];
  };

  var getListName = function (listId) {
    var mailingListsInd = mailingLists.length;
    while (mailingListsInd--) {
      if (mailingLists[mailingListsInd].id === listId) {
        return mailingLists[mailingListsInd].name;
      }
    }
    return listId;
  };

  var isValidName = function (roomname) {
    return reValidSymbols.test(roomname);
  };

  return {
    init  : init,

    bind    : bind,
    unbind  : unbind,

    events  : customEvents,

    fillLists : fillLists,

    createList  : createList,
    removeList  : removeList,
    openList    : openList,

    addContact    : addContact,
    removeContact : removeContact,

    storeMailingLists : storeMailingLists,

    sendMessage : sendMessage,

    getContacts : getContacts,

    getListName : getListName,

    isValidName : isValidName
  };
})();
