/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
