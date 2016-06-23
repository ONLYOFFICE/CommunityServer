window.ASC = window.ASC || {};

window.ASC.TMTalk = window.ASC.TMTalk || {};

window.ASC.TMTalk.tabsContainer = (function ($) {
  var
    isInit = false,
    tablistContainer = null,
    defaultTab = null,
    incomingMessages = {},
    roomIds = {};

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

  var setNodes = function () {
    if (tablistContainer === null) {
      var nodes = null;
      tablistContainer = document.getElementById('talkTabContainer');
      nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab default', 'li');
      defaultTab = nodes.length > 0 ? nodes[0] : null;
      defaultTab = defaultTab.cloneNode(true);
      defaultTab.className = defaultTab.className.replace(/\s*default\s*/, ' ').replace(/^\s+|\s+$/g, '');
    }
  };

  var init = function () {
    if (isInit === true) {
      return undefined;
    }
    isInit = true;
    // TODO
    ASC.TMTalk.contactsManager.bind(ASC.TMTalk.contactsManager.events.comeContact, onUpdateContact);
    ASC.TMTalk.contactsManager.bind(ASC.TMTalk.contactsManager.events.leftContact, onUpdateContact);

    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.updateSubject, onUpdateSubject);

    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.createRoom, onCreateRoom);
    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.openRoom, onOpenRoom);
    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.closeRoom, onCloseRoom);

    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.updateRoom, onUpdateConference);

    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.recvMessageFromConference, onRecvMessageFromConference);
    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.composingMessageFromChat, onComposingMessageFromChat);
    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.pausedMessageFromChat, onPausedMessageFromChat);
    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.recvMessageFromChat, onRecvMessageFromChat);
    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.recvOfflineMessagesFromChat, onRecvMessageFromChat);

    TMTalk.bind(TMTalk.events.pageFocus, onPageFocus);
    TMTalk.bind(TMTalk.events.pageKeyup, onPageKeyup);
  };

  var onPageFocus = function () {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (currentRoomData !== null) {
      var
        jid = '',
        nodes = null,
        nodesInd = 0;
      jid = currentRoomData.id;
      nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li');
      nodesInd = nodes.length;
      while (nodesInd--) {
        if (nodes[nodesInd].getAttribute('data-cid') === jid) {
          ASC.TMTalk.dom.removeClass(nodes[nodesInd], 'new-message');
        }
      }
    }
  };

  var onPageKeyup = function (evt) {
    switch (evt.keyCode) {
      case 48 :
      case 49 :
      case 50 :
      case 51 :
      case 52 :
      case 53 :
      case 54 :
      case 55 :
      case 56 :
      case 57 :
        if (evt.altKey === true) {
          var
            ntab = null,
            tabnum = evt.keyCode - 48,
            tabs = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li'),
            tabsInd = 0;

          //tabsInd = tabs.length;
          //while (tabsInd--) {
          //  if (ASC.TMTalk.dom.hasClass(tabs[tabsInd], 'current')) {
          //    ntab = ASC.TMTalk.dom.nextElementSibling(tabs[tabsInd]);
          //    break;
          //  }
          //}
          //if (!ntab && tabs.length > 0) {
          //  for (var i = 0, n = tabs.length; i < n; i++) {
          //    if (ASC.TMTalk.dom.hasClass(tabs[i], 'default')) {
          //      continue;
          //    }
          //    ntab = tabs[i];
          //    break;
          //  }
          //}

          if (tabs.length >= tabnum) {
            for (var i = 0, n = tabs.length; i < n; i++) {
              if (ASC.TMTalk.dom.hasClass(tabs[i], 'default')) {
                continue;
              }
              if (--tabnum === 0) {
                ntab = tabs[i];
                break;
              }
            }
          }

          if (ntab && !ASC.TMTalk.dom.hasClass(ntab, 'current')) {
            var roomId = ntab.getAttribute('data-roomid');
            if (roomId) {
              ASC.TMTalk.roomsManager.openRoom(roomId);
            }
          }
        }
        break;
    }
  };

  var onComposingMessageFromChat = function (jid) {
    var
      nodes = null,
      nodesInd = 0;
    nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-cid') === jid) {
        ASC.TMTalk.dom.addClass(nodes[nodesInd], 'typing');
        break;
      }
    }
  };

  var onPausedMessageFromChat = function (jid) {
    var
      nodes = null,
      nodesInd = 0;
    nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-cid') === jid) {
        ASC.TMTalk.dom.removeClass(nodes[nodesInd], 'typing');
        break;
      }
    }
  };

  var onRecvMessageFromChat = function (jid, name, date, body) {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (TMTalk.properties.focused === false || currentRoomData !== null && currentRoomData.id !== jid) {
      var
        found = false,
        nodes = null,
        nodesInd = 0;
      nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li');
      nodesInd = nodes.length;
      while (nodesInd--) {
        if (nodes[nodesInd].getAttribute('data-cid') === jid) {
          ASC.TMTalk.dom.addClass(nodes[nodesInd], 'new-message');
          found = true;
        }
      }
      if (found === false) {
        incomingMessages[jid] = true;
      }
    }
  };

  var onRecvMessageFromConference = function (roomjid, name, displaydate, date, body, isMine) {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (TMTalk.properties.focused === false || currentRoomData !== null && currentRoomData.id !== roomjid) {
      var
        found = false,
        nodes = null,
        nodesInd = 0;
      nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li');
      nodesInd = nodes.length;
      while (nodesInd--) {
        if (nodes[nodesInd].getAttribute('data-cid') === roomjid) {
          ASC.TMTalk.dom.addClass(nodes[nodesInd], 'new-message');
          found = true;
        }
      }
    }
  };

  var onUpdateContact = function (jid, status, resource) {
    var
      classname = '',
      roomId = '',
      node = null,
      nodes = null,
      nodesInd = 0;

    if (!roomIds.hasOwnProperty(jid)) {
      return undefined;
    }
    roomId = roomIds[jid].id;

    nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-roomid') === roomId) {
        node = nodes[nodesInd];
        classname = 'tab ' + status.className;
        if (ASC.TMTalk.dom.hasClass(node, 'current')) {
          classname += ' current';
        }
        if (ASC.TMTalk.dom.hasClass(node, 'master')) {
          classname += ' master';
        }
        if (ASC.TMTalk.dom.hasClass(node, 'new-message')) {
          classname += ' new-message';
        }
        node.className = classname;
        break;
      }
    }

    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (currentRoomData !== null && currentRoomData.type === 'chat' && currentRoomData.id === jid) {
      setStatus('chat', {show : status.title, message : typeof resource === 'object' ? resource.message : ''}, 1);
    }
  };

  var onUpdateSubject = function (roomjid, subject) {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (currentRoomData !== null && currentRoomData.type === 'conference' && currentRoomData.id === roomjid) {
      setStatus('conference', {confsubject : subject}, 1);
    }
  };

  var onUpdateConference = function (roomjid, room) {
    var
      tab = null,
      tabs = null,
      tabsInd = 0,
      nodes = null,
      roomId = '';

    var roomdata = ASC.TMTalk.roomsManager.getRoomDataById(roomjid);
    if (roomdata === null) {
      return undefined;
    }

    roomId = roomdata.roomId;
    tabs = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li');
    tabsInd = tabs.length;
    while (tabsInd--) {
      if (tabs[tabsInd].getAttribute('data-roomid') === roomId) {
        tab = tabs[tabsInd];
        nodes = ASC.TMTalk.dom.getElementsByClassName(tab, 'tab-title', 'div');
        if (nodes.length > 0) {
          nodes[0].innerHTML = translateSymbols(room.name, false);
          
        }
      }
    }
  };

  var onCreateRoom = function (roomId, data) {
    var
      tabname = '',
      classname = '',
      nodes = null,
      newtab = null;

    if (defaultTab === null) {
      throw 'no templates tab';
    }

    newtab = defaultTab.cloneNode(true);
    newtab.setAttribute('data-roomid', roomId);
    classname = newtab.className;

    switch (data.type) {
      case 'chat' :
        roomIds[data.id] = {
          id : roomId,
          data : data
        };
        newtab.setAttribute('data-cid', data.id);
        tabname = ASC.TMTalk.contactsManager.getContactName(data.id);
        classname += ' ' + ASC.TMTalk.contactsManager.getContactStatus(data.id).className;
        if (data.id === ASC.TMTalk.connectionManager.getDomain()) {
          classname += ' master';
        }
        if (incomingMessages.hasOwnProperty(data.id)) {
          classname += ' new-message';
          delete incomingMessages[data.id];
        }
        break;
      case 'conference' :
        roomIds[data.id] = {
          id : roomId,
          data : data
        };
        newtab.setAttribute('data-cid', data.id);
        tabname = ASC.TMTalk.mucManager.getConferenceName(data.id);
        classname += ' ' + data.type;
        break;
      case 'mailing' :
        roomIds[data.id] = {
          id : roomId,
          data : data
        };
        newtab.setAttribute('data-cid', data.id);
        tabname = ASC.TMTalk.msManager.getListName(data.id);
        classname += ' ' + data.type;
        break;
    }

    newtab.className = classname;

    nodes = ASC.TMTalk.dom.getElementsByClassName(newtab, 'tab-title', 'div');
    if (nodes.length > 0) {
      nodes[0].innerHTML = translateSymbols(tabname, false);
      nodes[0].setAttribute('title', tabname);
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tabs', 'ul');
    if (nodes.length > 0) {
      nodes[0].appendChild(newtab);
    }
  };

  var onOpenRoom = function (roomId, data, inBackground) {
    var
      pageHasFocus = false,
      nodes = null,
      currentTab = null,
      showSibling = false;

    if (inBackground !== true) {
      pageHasFocus = TMTalk.properties.focused;
      nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li');

      for (var i = 0, n = nodes.length; i < n; i++) {
        if (nodes[i].getAttribute('data-roomid') === roomId) {
          currentTab = nodes[i];

          ASC.TMTalk.dom.addClass(currentTab, 'current');
          if (pageHasFocus === true) {
            ASC.TMTalk.dom.removeClass(currentTab, 'new-message');
          }
          if (ASC.TMTalk.dom.hasClass(currentTab, 'hidden')) {
            showSibling = true;
            ASC.TMTalk.dom.removeClass(currentTab, 'hidden');
          }
        } else {
          ASC.TMTalk.dom.removeClass(nodes[i], 'current');
          if (showSibling === true) {
            ASC.TMTalk.dom.removeClass(nodes[i], 'hidden');
          }
        }
      }
    }
  };

  var onCloseRoom = function (roomId, data) {
    var
      isCurrent = false,
      node = null,
      nodes = null,
      nodesInd = 0;

    nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-roomid') === roomId) {
        node = nodes[nodesInd];
        isCurrent = ASC.TMTalk.dom.hasClass(node, 'current');
        var newCurrentRoom = null;
        if (isCurrent === true) {
          newCurrentRoom = ASC.TMTalk.dom.nextElementSibling(node);
          if (newCurrentRoom === null) {
            newCurrentRoom = ASC.TMTalk.dom.prevElementSibling(node);
          }
        }
        if (newCurrentRoom !== null && ASC.TMTalk.dom.hasClass(newCurrentRoom, 'default')) {
          newCurrentRoom = null;
        }
        node.parentNode.removeChild(node);
        if (newCurrentRoom !== null) {
          var roomId = newCurrentRoom.getAttribute('data-roomid');
          if (roomId) {
            ASC.TMTalk.roomsManager.openRoom(roomId);
          }
        }
        if (isCurrent === true && newCurrentRoom === null) {
          resetStatus('information', {}, -1);
        }
        break;
      }
    }
  };

  var resetStatus = function (type, value, priorityLevel) {
    var infoblocknode = document.getElementById('talkTabInfoBlock');

    if (!infoblocknode) {
      return undefined;
    }

    priorityLevel = priorityLevel && isFinite(+priorityLevel) ? +priorityLevel : -1;

    infoblocknode.setAttribute('data-level', priorityLevel);
    setStatus(type, value, priorityLevel);
  };

  var setStatus = function (type, value, priorityLevel) {
    var
      nodes = null,
      infoblocknode = null,
      lastPriorityLevel = -1;

    infoblocknode = document.getElementById('talkTabInfoBlock');

    if (!infoblocknode) {
      return undefined;
    }

    priorityLevel = priorityLevel && isFinite(+priorityLevel) ? +priorityLevel : -1;

    lastPriorityLevel = infoblocknode.getAttribute('data-level');
    lastPriorityLevel = lastPriorityLevel && isFinite(+lastPriorityLevel) ? +lastPriorityLevel : -1;

    if (value.hasOwnProperty('title')) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(infoblocknode, 'information', 'span');
      if (nodes.length > 0) {
        nodes[0].innerHTML = value.title;
      }
    }

    if (value.hasOwnProperty('hint')) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(infoblocknode, 'hint', 'span');
      if (nodes.length > 0) {
        nodes[0].innerHTML = value.hint;
      }
    }

    if (value.hasOwnProperty('department')) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(infoblocknode, 'department', 'span');
      if (nodes.length > 0) {
        nodes[0].innerHTML = value.department;
      }
    }

    if (value.hasOwnProperty('show')) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(infoblocknode, 'show', 'span');
      if (nodes.length > 0) {
        nodes[0].innerHTML = value.show;
      }
    }

    if (value.hasOwnProperty('message')) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(infoblocknode, 'message', 'span');
      if (nodes.length > 0) {
        nodes[0].innerHTML = value.message;
      }
    }

    if (value.hasOwnProperty('confsubject')) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(infoblocknode, 'conference-subject', 'span');
      if (nodes.length > 0) {
        nodes[0].innerHTML = value.confsubject;
      }
    }

    if (priorityLevel < lastPriorityLevel) {
      return undefined;
    }

    infoblocknode.setAttribute('data-level', priorityLevel);

    switch (type.toLowerCase()) {
      case 'information' :
        ASC.TMTalk.dom.removeClass(infoblocknode, 'hint');
        ASC.TMTalk.dom.removeClass(infoblocknode, 'chat');
        ASC.TMTalk.dom.removeClass(infoblocknode, 'conference');
        ASC.TMTalk.dom.addClass(infoblocknode, 'information');
        break;
      case 'chat' :
        ASC.TMTalk.dom.removeClass(infoblocknode, 'hint');
        ASC.TMTalk.dom.removeClass(infoblocknode, 'conference');
        ASC.TMTalk.dom.removeClass(infoblocknode, 'information');
        ASC.TMTalk.dom.addClass(infoblocknode, 'chat');
        break;
      case 'conference' :
        ASC.TMTalk.dom.removeClass(infoblocknode, 'hint');
        ASC.TMTalk.dom.removeClass(infoblocknode, 'chat');
        ASC.TMTalk.dom.removeClass(infoblocknode, 'information');
        ASC.TMTalk.dom.addClass(infoblocknode, 'conference');
        break;
      case 'hint' :
        ASC.TMTalk.dom.removeClass(infoblocknode, 'chat');
        ASC.TMTalk.dom.removeClass(infoblocknode, 'information');
        ASC.TMTalk.dom.removeClass(infoblocknode, 'conference');
        ASC.TMTalk.dom.addClass(infoblocknode, 'hint');
        break;
    }
  };

  var prevTab = function () {
    var
      ptab = null,
      nodes = null,
      nodesInd = 0;

    nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (ASC.TMTalk.dom.hasClass(nodes[nodesInd], 'current')) {
        ptab = ASC.TMTalk.dom.prevElementSibling(nodes[nodesInd]);
        break;
      }
    }
    if (ptab !== null) {
      var roomId = ptab.getAttribute('data-roomid');
      if (roomId) {
        ASC.TMTalk.roomsManager.openRoom(roomId);
      }
    }
  };

  var nextTab = function () {
    var
      ntab = null,
      nodes = null,
      nodesInd = 0;

    nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (ASC.TMTalk.dom.hasClass(nodes[nodesInd], 'current')) {
        ntab = ASC.TMTalk.dom.nextElementSibling(nodes[nodesInd]);
        break;
      }
    }
    if (ntab === null) {
      for (var i = 0, n = nodes.length; i < n; i++) {
        if (ASC.TMTalk.dom.hasClass(nodes[i], 'default')) {
          continue;
        }
        ntab = nodes[i];
        break;
      }
    }
    if (ntab !== null) {
      var roomId = ntab.getAttribute('data-roomid');
      if (roomId) {
        ASC.TMTalk.roomsManager.openRoom(roomId);
      }
    }
  };

  return {
    init  : init,
    nodes : setNodes,

    setStatus   : setStatus,
    resetStatus : resetStatus,

    prevTab : prevTab,
    nextTab : nextTab
  };
})(jQuery);

(function ($) {
  function onChoseMoveTab (selectedElement, selectedTargets) {
    if (selectedTargets.length === 0) {
      return undefined;
    }
    if (ASC.TMTalk.dom.hasClass(selectedTargets[0], 'tab')) {
      var
        selectedTab = selectedElement,
        targetTab = selectedTargets[0];
      if (ASC.TMTalk.dom.hasClass(selectedTab, 'tab') && ASC.TMTalk.dom.hasClass(targetTab, 'tab')) {
        selectedTab.parentNode.insertBefore(selectedTab, targetTab);
      }
    }
  }

  $(function () {
    ASC.TMTalk.tabsContainer.nodes();

    if ($.browser.safari) {
      $('#talkTabContainer ul.tabs:first').mousedown(function () {
        if (window.getSelection) {
          window.getSelection().removeAllRanges();
        }
        return false;
      });
    }

    $('#talkTabContainer ul.tabs:first')
      .bind($.browser.safari || ($.browser.msie && $.browser.version < 9) ? 'mousewheel' : 'DOMMouseScroll', function (evt) {
        var
          tablistContainer = document.getElementById('talkTabContainer'),
          i = 0, n = 0,
          tabs = null,
          firsttab = null,
          lasttab = null,
          wheelDelta = 0;

        wheelDelta = evt.wheelDelta ? -evt.wheelDelta / 120 : evt.detail ? evt.detail / 3 : 0;
        if (wheelDelta === 0) {
          return undefined;
        }

        tabs = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li');
        for (var i = 0, n = tabs.length; i < n; i++) {
          if (!ASC.TMTalk.dom.hasClass(tabs[i], 'default')) {
            firsttab = tabs[i];
            break;
          }
        }
        i = tabs.length;
        while (i--) {
          if (!ASC.TMTalk.dom.hasClass(tabs[i], 'default')) {
            lasttab = tabs[i];
            break;
          }
        }

        if (firsttab === null || lasttab === null) {
          return undefined;
        }

        if (wheelDelta < 0 && ASC.TMTalk.dom.hasClass(firsttab, 'hidden')) {
          wheelDelta *= -1;
          var nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab hidden', 'li');
          var nodesInd = nodes.length;
          while (wheelDelta > 0 && nodesInd--) {
            if (ASC.TMTalk.dom.hasClass(nodes[nodesInd], 'default')) {
              continue;
            }
            ASC.TMTalk.dom.removeClass(nodes[nodesInd], 'hidden');
            wheelDelta--;
          }
        } else if (wheelDelta > 0 && lasttab.offsetTop >= lasttab.offsetHeight) {
          for (i = 0, n = tabs.length; wheelDelta > 0 && i < n; i++) {
            if (ASC.TMTalk.dom.hasClass(tabs[i], 'default') || ASC.TMTalk.dom.hasClass(tabs[i], 'hidden')) {
              continue;
            }
            ASC.TMTalk.dom.addClass(tabs[i], 'hidden');
            wheelDelta--;
          }
        }

        if (ASC.TMTalk.dom.hasClass(firsttab, 'hidden')) {
          ASC.TMTalk.dom.addClass(tablistContainer, 'has-hidden');
        } else {
          ASC.TMTalk.dom.removeClass(tablistContainer, 'has-hidden');
        }
        if (lasttab.offsetTop >= lasttab.offsetHeight) {
          ASC.TMTalk.dom.addClass(tablistContainer, 'has-repressed');
        } else {
          ASC.TMTalk.dom.removeClass(tablistContainer, 'has-repressed');
        }
      })
      .mousedown(function (evt) {
        var element = evt.target;
        if (!element || typeof element !== 'object') {
          return undefined;
        }
        if (ASC.TMTalk.dom.hasClass(element, 'tab-title')) {
          var tab = element.parentNode.parentNode;
          if (ASC.TMTalk.dom.hasClass(tab, 'tab')) {
            var
              tablistContainer = document.getElementById('talkTabContainer'),
              nodes = null,
              tabsInd = 0,
              tabs = null,
              targets = [];
            tabs = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li');
            tabsInd = tabs.length;
            while (tabsInd--) {
              if (ASC.TMTalk.dom.hasClass(tabs[tabsInd], 'default')) {
                continue;
              }
              targets.push(tabs[tabsInd]);
            }
            ASC.TMTalk.dragMaster.start(
              evt,
              tab,
              targets,
              {onChose : onChoseMoveTab},
              function (el) {
                var
                  name = '',
                  nodes = null,
                  classname = '';
                nodes = ASC.TMTalk.dom.getElementsByClassName(el, 'tab-title', 'div');
                name = nodes.length > 0 ? nodes[0].innerHTML : 'none';
                if (!name) {
                  name = el.getAttribute('data-name');
                }
                classname = 'tab-dragable';
                this.style.width = el.offsetWidth + 'px';
                nodes = ASC.TMTalk.dom.getElementsByClassName(this, 'title', 'div');
                if (nodes.length > 0) {
                  nodes[0].innerHTML = name;
                }
                var contactjid = el.getAttribute('data-cid');
                if (contactjid) {
                  var status = ASC.TMTalk.contactsManager.getContactStatus(contactjid);
                  if (status) {
                    classname += ' ' + status.className;
                  }
                }
                if (ASC.TMTalk.dom.hasClass(el, 'master')) {
                  classname += ' master';
                }
                if (ASC.TMTalk.dom.hasClass(el, 'current')) {
                  classname += ' current';
                }
                this.className = classname;
              }
            );
          }
        }
      })
      .mouseup(function (evt) {
        var element = evt.target;

        if (evt.which === 2) {
          var selectTab = element.parentNode.parentNode;
          if (ASC.TMTalk.dom.hasClass(selectTab, 'tab')) {
            var roomId = selectTab.getAttribute('data-roomid');
            if (roomId) {
              ASC.TMTalk.roomsManager.closeRoom(roomId);
            }
          }
        }
      })
      .click(function (evt) {
        var element = evt.target;

        if (ASC.TMTalk.dom.hasClass(element, 'button-talk') && ASC.TMTalk.dom.hasClass(element, 'close')) {
          var selectTab = element.parentNode.parentNode;
          if (ASC.TMTalk.dom.hasClass(selectTab, 'tab')) {
            var roomId = selectTab.getAttribute('data-roomid');
            if (roomId) {
              ASC.TMTalk.roomsManager.closeRoom(roomId);
            }
          }
        } else if (ASC.TMTalk.dom.hasClass(element, 'tab-title')) {
          var selectTab = element.parentNode.parentNode;
          if (ASC.TMTalk.dom.hasClass(selectTab, 'tab')) {
            if (!ASC.TMTalk.dom.hasClass(selectTab, 'current')) {
              var roomId = selectTab.getAttribute('data-roomid');
              if (roomId) {
                ASC.TMTalk.roomsManager.openRoom(roomId);
              }
            }
          }
        }
      });

    $('#talkTabContainer div.navigation:first').click(function (evt) {
      var element = evt.target;

      if (ASC.TMTalk.dom.hasClass(element, 'button-talk') && ASC.TMTalk.dom.hasClass(element, 'move-to-left')) {
        var
          tablistContainer = document.getElementById('talkTabContainer'),
          nodes = null,
          lasttab = null;

        nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab hidden', 'li');

        if (nodes.length > 0) {
          ASC.TMTalk.dom.removeClass(nodes[nodes.length - 1], 'hidden');
        }

        nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab hidden', 'li');
        var hiddenLen = (nodes.length === 1 && ASC.TMTalk.dom.hasClass(nodes[0], 'default')) ? 0 : nodes.length;
        if (hiddenLen > 0) {
          ASC.TMTalk.dom.addClass(tablistContainer, 'has-hidden');
        } else {
          ASC.TMTalk.dom.removeClass(tablistContainer, 'has-hidden');
        }

        nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li');
        if (nodes.length > 0 && ASC.TMTalk.dom.hasClass(nodes[nodes.length - 1], 'default') === false) {
          lasttab = nodes[nodes.length - 1];
          if (lasttab.offsetTop >= lasttab.offsetHeight) {
            ASC.TMTalk.dom.addClass(tablistContainer, 'has-repressed');
          } else {
            ASC.TMTalk.dom.removeClass(tablistContainer, 'has-repressed');
          }
        }
      } else if (ASC.TMTalk.dom.hasClass(element, 'move-to-right')) {
        var
          tablistContainer = document.getElementById('talkTabContainer'),
          nodes = null,
          nodesInd = 0,
          lasttab = null;

        nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li');
        if (nodes.length > 0 && ASC.TMTalk.dom.hasClass(nodes[nodes.length - 1], 'default') === false) {
          lasttab = nodes[nodes.length - 1];
        }

        if (lasttab === null || lasttab.offsetTop < lasttab.offsetHeight) {
          return undefined;
        }

        for (var i = 0, n = nodes.length; i < n; i++) {
          if (ASC.TMTalk.dom.hasClass(nodes[i], 'hidden')) {
            continue;
          }
          ASC.TMTalk.dom.addClass(nodes[i], 'hidden');
          break;
        }

        nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab hidden', 'li');
        var hiddenLen = (nodes.length === 1 && ASC.TMTalk.dom.hasClass(nodes[0], 'default')) ? 0 : nodes.length;
        if (hiddenLen > 0) {
          ASC.TMTalk.dom.addClass(tablistContainer, 'has-hidden');
        } else {
          ASC.TMTalk.dom.removeClass(tablistContainer, 'has-hidden');
        }

        nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li');
        if (nodes.length > 0 && ASC.TMTalk.dom.hasClass(nodes[nodes.length - 1], 'default') === false) {
          lasttab = nodes[nodes.length - 1];
          if (lasttab.offsetTop >= lasttab.offsetHeight) {
            ASC.TMTalk.dom.addClass(tablistContainer, 'has-repressed');
          } else {
            ASC.TMTalk.dom.removeClass(tablistContainer, 'has-repressed');
          }
        }
      }
    });

    $(window).resize(function () {
      var
        tablistContainer = document.getElementById('talkTabContainer'),
        nodes = null,
        lastTab = null,
        currentTab = null;

      nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab current', 'li');
      if (nodes.length > 0) {
        currentTab = nodes[0];
      }

      nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li');
      if (nodes.length > 0) {
        lastTab = nodes[nodes.length - 1];
        if (ASC.TMTalk.dom.hasClass(lastTab, 'default')) {
          lastTab = null;
        }
      }

      ASC.TMTalk.dom.removeClass(tablistContainer, 'overflow');

      // has hidden tabs
      if (lastTab && lastTab.offsetTop < lastTab.offsetHeight) {
        nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab hidden', 'li');
        var nodesInd = nodes.length;
        while (nodesInd--) {
          ASC.TMTalk.dom.removeClass(nodes[nodesInd], 'hidden');
          if (lastTab.offsetTop >= lastTab.offsetHeight) {
            ASC.TMTalk.dom.addClass(nodes[nodesInd], 'hidden');
            ASC.TMTalk.dom.addClass(tablistContainer, 'overflow');
            break;
          }
        }
      }

      if (lastTab && lastTab.offsetTop >= lastTab.offsetHeight) {
        ASC.TMTalk.dom.addClass(tablistContainer, 'overflow');
      }

      // if need to hide
      if (currentTab && currentTab.offsetTop >= currentTab.offsetHeight) {
        ASC.TMTalk.dom.addClass(tablistContainer, 'overflow');
        if (!ASC.TMTalk.dom.hasClass(currentTab, 'hidden')) {
          nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab', 'li');
          for (var i = 0, n = nodes.length; i < n; i++) {
            ASC.TMTalk.dom.addClass(nodes[i], 'hidden');
            if (currentTab.offsetTop < currentTab.offsetHeight) {
              break;
            }
          }
        }
      }

      nodes = ASC.TMTalk.dom.getElementsByClassName(tablistContainer, 'tab hidden', 'li');
      var hiddenLen = (nodes.length === 1 && ASC.TMTalk.dom.hasClass(nodes[0], 'default')) ? 0 : nodes.length;

      if (hiddenLen > 0 || (lastTab && lastTab.offsetTop >= lastTab.offsetHeight)) {
        ASC.TMTalk.dom.addClass(tablistContainer, 'overflow');

        if (hiddenLen > 0) {
          ASC.TMTalk.dom.addClass(tablistContainer, 'has-hidden');
        } else {
          ASC.TMTalk.dom.removeClass(tablistContainer, 'has-hidden');
        }

        if (lastTab && lastTab.offsetTop >= lastTab.offsetHeight) {
          ASC.TMTalk.dom.addClass(tablistContainer, 'has-repressed');
        } else {
          ASC.TMTalk.dom.removeClass(tablistContainer, 'has-repressed');
        }
      }
    });
  });
})(jQuery);
