/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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

window.ASC.TMTalk.contactsManager = (function () {
  var
    isInit = false,
    groups = [],
    contacts = [],
    onlineContacts = {},
    customEvents = {
      // update contactlist
      updateContacts : 'onupdategroups',
      // adding new contact
      addContact : 'onaddcontact',
      // remove contact
      removeContact : 'onremovecontact',
      // contact is come
      comeContact : 'oncomecontact',
      // contact is leave
      leftContact : 'onleftcontact',
      // update vCard
      updateCard : 'oncardupdate'
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

  var translateSymbols = function (str, toText) {
    
    var
      symbols = [
        ['&lt;',  '<'],
        ['&gt;',  '>'],
        ['&and;', '\\^'],
        ['&sim;', '~'],
        ['&amp;', '&']
      ];

    if (typeof str !== 'string' || str.length === 0) {
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
  };

  var init = function () {
    if (isInit = true) {
      return undefined;
    }
    isInit = true;
    // TODO
  };

  var bind = function (eventName, handler, params) {
    return eventManager.bind(eventName, handler, params);
  };

  var unbind = function (handlerId) {
    return eventManager.unbind(handlerId);
  };

  var fillContacts = function (items) {
    groups = [];
    contacts = [];
    onlineContacts = {};

    var
      item = null,
      itemsInd = 0,
      groupsInd = 0,
      contact = null,
      groupname = '',
      contactname = '',
      contactShow = '',
      groupNode = null,
      statuses = ASC.TMTalk.connectionManager.statuses;

    itemsInd = items.length;
    while (itemsInd--) {
      item = items[itemsInd];
      groupNode = item.getElementsByTagName('group');
      if (groupNode.length === 0) {
        groupNode = Strophe.getElementsByTagName(item, 'group');
      }
      groupname = groupNode.length > 0 ? ASC.TMTalk.dom.getContent(groupNode[0]) : '';
      contactname = item.getAttribute('name') || '';

      // groupname = translateSymbols(groupname, false);
      // contactname = translateSymbols(contactname, false);

      contact = {
        jid     : item.getAttribute('jid'),
        group   : groupname,
        name    : contactname,
        state   : ASC.TMTalk.connectionManager.offlineStatusId,
        show    : statuses[ASC.TMTalk.connectionManager.offlineStatusId].title,
        status  : ''
      };
      contacts.push(contact);

      groupsInd = groups.length;
      while (groupsInd--) {
        if (groups[groupsInd].name === groupname) {
          groups[groupsInd].contacts.push(contact);
          break;
        }
      }
      if (groupsInd === -1) {
        groups.push({
          name      : groupname,
          contacts  : [contact]
        });
      }
    }

    groupsInd = groups.length;
    while (groupsInd--) {
      groups[groupsInd].contacts.sort(sortByName);
    }
    groups.sort(sortByName);
    contacts.sort(sortByName);

    eventManager.call(customEvents.updateContacts, window, [groups, contacts]);
  };

  var clearOnlineContacts = function () {
    var
      jid = '',
      contactsInd = 0,
      currentStatus = ASC.TMTalk.connectionManager.statuses[ASC.TMTalk.connectionManager.offlineStatusId];
    for (jid in onlineContacts) {
      if (onlineContacts.hasOwnProperty(jid)) {
        delete onlineContacts[jid];

        contactsInd = contacts.length;
        while (contactsInd--) {
          if (contacts[contactsInd].jid === jid) {
            contacts[contactsInd].show = currentStatus.title;
            contacts[contactsInd].state = currentStatus.id;
            contacts[contactsInd].status = '';
            break;
          }
        }

        eventManager.call(customEvents.leftContact, window, [jid, currentStatus]);
      }
    }
  };

  var comeContact = function (presence) {
    var
      nodes = null,
      newResource = null,
      resources = null,
      statuses = ASC.TMTalk.connectionManager.statuses,
      currentStatus = statuses[ASC.TMTalk.connectionManager.onlineStatusId],
      show = currentStatus.show,
      message = '',
      from = presence.getAttribute('from'),
      jid = '',
      resource = from ? from.substring(from.indexOf('/') + 1) : '',
      priority = 0;

    if (typeof from !== 'string') {
      return undefined;
    }

    jid = from.substring(0, from.indexOf('/')).toLowerCase();
    jid = jid === '' ? from : jid;

    nodes = presence.getElementsByTagName('priority');
    priority = nodes.length > 0 ? ASC.TMTalk.dom.getContent(nodes[0]) : priority;

    nodes = presence.getElementsByTagName('show');
    show = nodes.length > 0 ? ASC.TMTalk.dom.getContent(nodes[0]) : show;

    nodes = presence.getElementsByTagName('status');
    message = nodes.length > 0 ? ASC.TMTalk.dom.getContent(nodes[0]) : message;

    message = translateSymbols(message, false);

    priority = isFinite(+priority) ? +priority : priority;
    var statusesInd = statuses.length;
    while (statusesInd--) {
      if (statuses[statusesInd].show === show) {
        currentStatus = statuses[statusesInd];
        break;
      }
    }
    newResource = {
      name      : resource,
      priority  : priority,
      show      : show,
      message   : message
    };

    if (onlineContacts.hasOwnProperty(jid)) {
      resources = onlineContacts[jid];
      var
        resourceExist = false,
        newResourcePos = -1,
        resourceInd = resources.length;
      while (resourceInd--) {
        if (resources[resourceInd].name === resource) {
          newResourcePos = resourceInd;
          resourceExist = true;
          break;
        }
        if (resources[resourceInd].priority > priority) {
          newResourcePos = resourceInd + 1;
        }
      }
      // if check all and not found resource with larger priority, add to front
      if (newResourcePos === -1 && resourceInd === -1) {
        newResourcePos = 0;
      }
      if (resourceExist === true) {
        // if resource exist
        resources[newResourcePos].show = show;
      } else {
        // if not found, add in place
        resources.splice(newResourcePos, 0, newResource);
      }

      if (newResourcePos === 0) {
        var contactsInd = contacts.length;
        while (contactsInd--) {
          if (contacts[contactsInd].jid === jid) {
            contacts[contactsInd].show = currentStatus.title;
            contacts[contactsInd].state = currentStatus.id;
            contacts[contactsInd].status = newResource.message;
            break;
          }
        }
        eventManager.call(customEvents.comeContact, window, [jid, currentStatus, newResource]);
      }
    } else {
      var contactsInd = contacts.length;
      while (contactsInd--) {
        if (contacts[contactsInd].jid === jid) {
          contacts[contactsInd].show = currentStatus.title;
          contacts[contactsInd].state = currentStatus.id;
          contacts[contactsInd].status = newResource.message;
          break;
        }
      }
      onlineContacts[jid] = [newResource];
      eventManager.call(customEvents.comeContact, window, [jid, currentStatus, newResource]);
    }
  };

  var leftContact = function (presence) {
    var
      statusesInd = 0,
      resources = null,
      newResource = null,
      statuses = ASC.TMTalk.connectionManager.statuses,
      currentStatus = statuses[ASC.TMTalk.connectionManager.onlineStatusId],
      show = currentStatus.show,
      message = '',
      jid = presence.getAttribute('from'),
      resource = jid ? jid.substring(jid.indexOf('/') + 1) : '';

    jid = jid.substring(0, jid.indexOf('/')).toLowerCase();
    if (!onlineContacts.hasOwnProperty(jid)) {
      return undefined;
    }

    if (onlineContacts[jid].length === 1) {
        currentStatus = statuses[ASC.TMTalk.connectionManager.offlineStatusId];
        var contactsInd = contacts.length;
        while (contactsInd--) {
            if (contacts[contactsInd].jid === jid) {
                contacts[contactsInd].show = currentStatus.title;
                contacts[contactsInd].state = currentStatus.id;
                contacts[contactsInd].status = '';
                break;
            }
        }
        delete onlineContacts[jid];
        eventManager.call(customEvents.leftContact, window, [jid, currentStatus]);
        return undefined;
    }
    resources = onlineContacts[jid];
    var resourceInd = resources.length;
    while (resourceInd--) {
      if (resources[resourceInd].name === resource) {
        resources.splice(resourceInd, 1);
        break;
      }
    }
   // commented for signalr
   // if (resourceInd === 0) {
        newResource = resources[0];
        show = newResource.show;
        message = newResource.message;
        var statusesInd = statuses.length;
        while (statusesInd--) {
            if (statuses[statusesInd].show === show) {
                currentStatus = statuses[statusesInd];
                break;
            }
        }
        var contactsInd = contacts.length;
        while (contactsInd--) {
            if (contacts[contactsInd].jid === jid) {
                contacts[contactsInd].show = currentStatus.title;
                contacts[contactsInd].state = currentStatus.id;
                contacts[contactsInd].status = newResource.message;
                break;
            }
        }
        eventManager.call(customEvents.comeContact, window, [jid, currentStatus, newResource]);
   // }
  };

  var addContact = function () {
    
  };

  var removeContact = function () {
    
  };

  var updateCard = function (iq) {
    var
      nodes = null,
      nodesInd = 0,
      child = null,
      childs = null,
      childsInd = 0,
      from = iq.getAttribute('from'),
      groupname = '',
      contactname = '',
      jid = from.substring(0, from.indexOf('/')).toLowerCase();

    nodes = iq.getElementsByTagName('vCard');
    if (nodes.length === 0) {
      nodes = Strophe.getElementsByTagName(iq, 'vCard');
    }

    if (nodes.length === 0) {
      return undefined;
    }

    childs = nodes[0].childNodes;
    childsInd = childs.length;
    while (childsInd--) {
      child = childs[childsInd];
      switch (child.tagName.toLowerCase()) {
        case 'fn' :
          contactname = ASC.TMTalk.dom.getContent(child);
          break;
        case 'org' :
          nodes = child.getElementsByTagName('ORGUNIT');
          if (nodes.length === 0) {
            nodes = Strophe.getElementsByTagName(child, 'ORGUNIT');
          }
          if (nodes.length > 0) {
            groupname = ASC.TMTalk.dom.getContent(nodes[0]);
          }
          break;
      }
    }

    //groupname = translateSymbols(groupname, false);
    //contactname = translateSymbols(contactname, false);

    var contact = {
      jid     : jid,
      group   : groupname,
      name    : contactname,
      state   : ASC.TMTalk.connectionManager.offlineStatusId,
      show    : ASC.TMTalk.connectionManager.statuses[ASC.TMTalk.connectionManager.offlineStatusId].title,
      status  : ''
    };

    var contactsInd = contacts.length;
    while (contactsInd--) {
      if (contacts[contactsInd].jid === jid) {
        contacts[contactsInd].name = contactname;
        contacts[contactsInd].group = groupname;
        break;
      }
    }
    if (contactsInd === -1) {
      contacts.push(contact);
    }

    var groupsInd = groups.length;
    while (groupsInd--) {
      if (groups[groupsInd].name === groupname) {
        groups[groupsInd].contacts.push(contact);
        break;
      }
    }
    if (groupsInd === -1) {
      groups.push({
        name      : groupname,
        contacts  : [contact]
      });
    }
  };

  var getContact = function (jid) {
    if (typeof jid !== 'string') {
      jid = ASC.TMTalk.connectionManager.getJid();
    }
    var contactsInd = contacts.length;
    while (contactsInd--) {
      if (contacts[contactsInd].jid === jid) {
        return contacts[contactsInd];
      }
    }
    return null;
  };

  var getContactName = function (jid) {
    if (typeof jid !== 'string') {
      jid = ASC.TMTalk.connectionManager.getJid();
    }
    var resource = jid.indexOf('/');
    if (resource !== -1) {
      jid = jid.substring(0, resource);
    }
    var contactsInd = contacts.length;
    while (contactsInd--) {
      if (contacts[contactsInd].jid === jid) {
        return contacts[contactsInd].name;
      }
    }
    return jid;
  };

  var getContactStatus = function (jid) {
    if (typeof jid === 'undefined' || jid === ASC.TMTalk.connectionManager.getJid()) {
      ASC.TMTalk.connectionManager.statuses[ASC.TMTalk.connectionManager.onlineStatusId];
    }

    var
      statusId = -1,
      statuses = ASC.TMTalk.connectionManager.statuses,
      status = statuses[ASC.TMTalk.connectionManager.offlineStatusId],
      contactsInd = contacts.length;
    while (contactsInd--) {
      if (contacts[contactsInd].jid === jid) {
        statusId = contacts[contactsInd].state;
        break;
      }
    }
    if (statusId !== -1) {
      var statusesInd = statuses.length;
      while (statusesInd--) {
        if (statuses[statusesInd].id === statusId) {
          status = statuses[statusesInd];
          break;
        }
      }
    }
    return status;
  };

  var getContactsGroupByName = function (groupname) {
    if (typeof groupname !== 'string') {
      return [];
    }
    var groupsInd = groups.length;
    while (groupsInd--) {
      if (groups[groupsInd].name === groupname) {
        return groups[groupsInd].contacts;
      }
    }
    return [];
  };

  return {
    init  : init,

    bind    : bind,
    unbind  : unbind,

    events  : customEvents,

    fillContacts  : fillContacts,

    clearOnlineContacts : clearOnlineContacts,

    comeContact   : comeContact,
    leftContact   : leftContact,
    addContact    : addContact,
    removeContact : removeContact,
    updateCard    : updateCard,

    getContact        : getContact,
    getContactName    : getContactName,
    getContactStatus  : getContactStatus,

    getContactsGroupByName  : getContactsGroupByName
  };
})();
