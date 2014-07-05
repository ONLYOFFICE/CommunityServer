/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/


;String.prototype.format = function () {
  if (arguments.length === 0) {
    return '';
  }

  var pos = -1, str = this, cnd = '', ind = -1, cnds = str.match(/{(\d+)}/g), cndsInd = cnds ? cnds.length : 0;
  while (cndsInd--) {
    pos = -1;
    cnd = cnds[cndsInd];
    ind = cnd.replace(/[{}]+/g, '');
    while ((pos = str.indexOf(cnd, pos + 1)) !== -1) {
      str = str.substring(0, pos) + (arguments[+ind] || '') + str.substring(pos + cnd.length);
    }
  }
  return str;
};

window.ASC = window.ASC || {};

window.ASC.Controls = window.ASC.Controls || {};

window.ASC.Resources = window.ASC.Resources || {};

window.ASC.Controls.messages = (function () {
  var
    ttl = 6 * 1000,
    maxshowed = 5,
    messages = [],
    lazymessages = [];

  function uniqueId (prefix) {
    var
      id = 0,
      count = 100,
      messagesInd = 0,
      messagesLen = messages.length;

    while (--count > 0) {
      id = Math.floor(Math.random() * 1000000) + '';
      messagesInd = messagesLen;
      while (messagesInd--) {
        if (messages[messagesInd].id == id) {
          break;
        }
      }
      if (messagesInd !== -1) {
        continue;
      }
      return id;
    }
    return '0';
  };

  function createMessage (id, type, text) {
    var o = document.createElement('div');
    o.setAttribute('data-mesid', id);
    o.className = 'custommessage-wrapper ' + type.toLowerCase() + '-message';
    var c = document.createElement('div');
    c.className = 'custommessage-container';
    c.appendChild(document.createTextNode(text));
    o.appendChild(c);
    document.body.appendChild(o);

    c.innerHTML = c.innerHTML.replace(/\n/g, '<br />');
    o.style.maxWidth = '90%';
    o.style.position = 'absolute';
    o.style.top = '0';
    o.style.left = '0';
    o.style.zIndex = '666';
    o.style.marginLeft = (-o.offsetWidth >> 1) + 'px';
    o.style.left = '50%';
    return o;
  }

  function addMessage (id, type, text) {
    var p = null;
    var o = createMessage(id, type, text), p = {o : o, h : null};
    p.h = setTimeout((function (id) {return function () {ASC.Controls.messages.hide(id)}})(id), ttl);
    messages.push({id : id, obj : o, param : p});
  }

  //id, type, text
  var show = function () {
    var mesid = -1, type = '', text = '';
    switch (arguments.length) {
      case 0 :
        return mesid;
      case 1 :
        type  = 'information';
        text  = arguments[0];
        break;
      case 2 :
        type  = arguments[0];
        text  = arguments[1];
        break;
      default :
        mesid = arguments[0];
        type  = arguments[1];
        text  = arguments[2];
        break;
    }

    switch (type.toLowerCase()) {
      case 'information' :
      case 'warning' :
      case 'error' :
        break;
      default :
        return mesid;
    }

    if (mesid === -1) {
      mesid = uniqueId();
    }

    var messagesInd = messages.length;
    while (messagesInd--) {
      if (messages[messagesInd].id == mesid) {
        return mesid;
      }
    }

    if (messages.length >= maxshowed) {
      lazymessages.push({id : mesid, type : type, text : text});
      return mesid;
    }

    addMessage(mesid, type, text);

    var offsettop = 0;
    for (var i = 0, n = messages.length; i < n; i++) {
      messages[i].obj.style.top = offsettop + 'px';
      offsettop += messages[i].obj.offsetHeight;
    }

    return mesid;
  };

  var hide = function (id) {
    var message = null, messagesInd = messages.length;
    while (messagesInd--) {
      if (messages[messagesInd].id == id) {
        message = messages[messagesInd];
        clearTimeout(message.param.h);
        messages.splice(messagesInd, 1);
        message.obj.parentNode.removeChild(message.obj);
      }
    }

    if (messages.length < maxshowed) {
      for (var i = 0, n = lazymessages.length < maxshowed - messages.length ? lazymessages.length : maxshowed - messages.length; i < n; i++) {
        message = lazymessages[i];
        addMessage(message.id, message.type, message.text);
        lazymessages.splice(i, 1);
      }
    }

    var offsettop = 0;
    for (var i = 0, n = messages.length; i < n; i++) {
      messages[i].obj.style.top = offsettop + 'px';
      offsettop += messages[i].obj.offsetHeight;
    }
  };

  var hideAll = function () {
    var message = null, messagesInd = 0;

    messagesInd = messages.length;
    while (messagesInd--) {
      message = messages[messagesInd];
      clearTimeout(message.param.h);
      messages.splice(messagesInd, 1);
      message.obj.parentNode.removeChild(message.obj);
    }

    messagesInd = lazymessages.length;
    while (messagesInd--) {
      lazymessages.splice(messagesInd, 1);
    }
  };

  return {
    show    : show,
    hide    : hide,
    hideAll : hideAll
  };
})();
