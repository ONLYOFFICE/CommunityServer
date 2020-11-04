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

window.ASC.TMTalk.meseditorContainer = (function ($) {

  var
    isInit = false,
    taWindow = null,
    taStylePath = '',
    taFocused = false,
    maRange = null,
    maRangeMark = null,
    maSelection = null,
    composingMessages = {},
    pausedMesssagesTimeout = 5,
    simpleEditor = false,
    timeIntervalSendButton = null,
    browser = (function () {
      var
        ua = navigator.userAgent,
        browser = null,
        version = (ua.match(/.+(?:rv|it|ra|ie|ox|me|on|id|os)[\/:\s]([\d._]+)/i)||[0,'0'])[1].replace('_', '');

      browser = {
        version : isFinite(parseFloat(version)) ? parseFloat(version) : version,
        // browsers
        msie    : '\v' == 'v' || /msie/i.test(ua),
        opera   : window.opera ? true : false,
        chrome  : window.chrome ? true : false,
        safari  : /safari/i.test(ua) && !window.chrome,
        firefox : /firefox/i.test(ua),
        mozilla : /mozilla/i.test(ua) && !/(compatible|webkit)/.test(ua),
        // engine
        ios     : /iphone|ipad/i.test(ua),
        android : /android/i.test(ua),
        gecko   : /gecko/i.test(ua),
        webkit  : /webkit/i.test(ua),

        // support
        touch   : 'ontouchend' in document
      };

      if (browser.msie && window.addEventListener && browser.version < 9) {
        browser.version = 9;
      }

      return browser;
    })();

  //simpleEditor = simpleEditor || browser.ios || browser.android;

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

  function trim (str) {
    if (typeof str != 'string' || str.length === 0) {
      return '';
    }
    return str.replace(/^[\n\s]+|[\n\s]+$/g, '');
  }

  function translateSymbols (str, toText) { 
      var symbols = [
          ['&lt;', '<'],
          ['&gt;', '>'],
          ['&and;', '\\^'],
          ['&sim;', '~'],
          ['&amp;', '&']
      ];

    if (typeof str !== 'string') {
      return '';
    }
    // replace control symbols to spaces
    str = str.replace(/[\0-\x08\x0E-\x1F]/g, ' ');
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

  function isBlockNode (node) {
    if (!node || typeof node == 'undefined' || typeof node.nodeName == 'undefined') {
      return false;
    }
    switch (node.nodeName.toLowerCase()) {
      case 'p' :
      case 'h1' :
      case 'h2' :
      case 'h3' :
      case 'h4' :
      case 'h5' :
      case 'h6' :
      case 'ul' :
      case 'ol' :
      case 'li' :
      case 'tr' :
      case 'div' :
      case 'table' :
        return true;
      default :
        return false;
    }
    return false;
  }

  function isNonblockNode (node) {
    if (!node || typeof node == 'undefined' || typeof node.nodeName == 'undefined') {
      return false;
    }
    return !isBlockNode(node);
  }

  function getNodeContent (node) {
    var
      content = '',
      child = null,
      childrens = node.childNodes;

    for (var i = 0, n = childrens.length; i < n; i++) {
      child = childrens.item(i);
      switch (child.nodeType) {
        case 1 :
        case 5 :
          switch (child.nodeName.toLowerCase()) {
            case 'br' :
              if (child.getAttribute('type') !== 'moz_') {
                content += '\n';
              }
              break;
            case 'a' :
              var attr = child.getAttribute('href');
              if (attr) {
                content += attr;
              } else {
                var textContent = child.textContent;
                if (textContent) {
                  content += textContent;
                }
              }
              break;
            case 'img' :
              var attr = child.getAttribute('alt');
              if (attr) {
                content += attr;
              }
              break;
            case 'p' :
            case 'h1' :
            case 'h2' :
            case 'h3' :
            case 'h4' :
            case 'h5' :
            case 'h6' :
            case 'ul' :
            case 'ol' :
            case 'li' :
            case 'tr' :
            case 'div' :
            case 'table' :
              var childContent = trimN(arguments.callee(child));
              content += (i !== 0 ? '\n' : '') + childContent;
              break;
            case 'style':
              break;
            default :
              content += arguments.callee(child);
              break;
          }
          break;
        case 2 :
        case 3 :
        case 4 :
          content += child.nodeValue;
          break;
        default :
          break;
      }
    }
    return content;
  }

  function setSelected () {
    if (browser.msie && maRange && maRangeMark) {
      maRange.moveToBookmark(maRangeMark);
      maRange.select();
    }
  }

  function setFocusToTextarea (wnd) {
    window.focus();
    wnd.focus();
  }

  var insertNewLineToTextarea = (function () {
    if (simpleEditor === true) {
      return function (wnd, fin) {
        if (document.selection) {
          var s = document.selection.createRange(); 
          if (s.text) {
            s.text = '\n';
	          s.select();
          }
        } else if (typeof wnd.selectionStart === 'number') {
          var
            start = wnd.selectionStart,
            end = wnd.selectionEnd;

          wnd.value = wnd.value.substring(0, start) + '\n' + wnd.value.substring(end);
          wnd.setSelectionRange(start + 1, start + 1);
        }
      };
    }
    return function (wnd, fin) {
      if (browser.msie && browser.version < 9) {
        var range = wnd.document.selection.createRange();
        range.pasteHTML('&nbsp;<p></p>');
        range.collapse(true);
      } else {
        var el = wnd.document.createElement('br');
        var selectionRange = wnd.getSelection().getRangeAt(0);
        selectionRange.deleteContents();
        selectionRange.insertNode(el);

       
        if (!el.parentNode) {
            el = jQuery(jQuery('div#talkTextareaContainer ul.textareas li.textarea.current iframe:first')[0].contentDocument).find('br')[0];
        }
        if (el.parentNode.tagName.toLowerCase() === 'a') {
            el.parentNode.parentNode.insertBefore(el, el.parentNode.nextElementSibling);
        }

        wnd.getSelection().collapseToEnd();
        wnd.getSelection().removeAllRanges();

        selectionRange = wnd.document.createRange();
        selectionRange.selectNode(el);
        wnd.getSelection().addRange(selectionRange);
        wnd.getSelection().collapseToEnd();
          // hack for webkit
        if (browser.webkit && fin !== true && el.previousSibling && el.previousSibling.nodeType === 3 && (!el.nextSibling || !el.nextSibling.data)) {
            try { insertNewLineToTextarea(wnd, true) } catch (err) { }
        }
      }
      wnd.document.body.scrollTop = ASC.TMTalk.dom.maxScrollTop(wnd.document.body);
    };
  })();

  var insertSmileToTextarea = (function () {
    if (simpleEditor === true) {
      return function (wnd, src, title) {
        if (document.selection && maSelection !== null) {
          var s = maSelection;
          s.text = title;
          s.select();
        } else if (typeof wnd.selectionStart === 'number' && maSelection !== null) {
          var
            start = maSelection.start,
            end = maSelection.end;

          wnd.value = wnd.value.substring(0, start) + title + wnd.value.substring(end);
          wnd.setSelectionRange(start + title.length, start + title.length);
        }
      };
    }
    return function (wnd, src, title) {
      setSelected();
      setFocusToTextarea(wnd);
      var selectionRange = null;
      var el = null;

      if (browser.msie) {
        el = '<img src="' + src + '" alt="' + title + '" contentEditable="false" onResizeStart="return false">';
        selectionRange = wnd.document.selection.createRange();
        selectionRange.pasteHTML(el);
        selectionRange.collapse(true);
      } else {
        el = wnd.document.createElement('img');
        el.src = src;
        el.alt = title;

        selectionRange = wnd.getSelection().getRangeAt(0);
        selectionRange.deleteContents();
        selectionRange.insertNode(el);
        wnd.getSelection().collapseToEnd();
        wnd.getSelection().removeAllRanges();

        selectionRange = wnd.document.createRange();
        selectionRange.selectNode(el);
        wnd.getSelection().addRange(selectionRange);
        wnd.getSelection().collapseToEnd();
      }
    };
  })();

  var emptyTexrarea = (function () {
    if (simpleEditor === true) {
      return function (wnd) {
        return trim(wnd.value).length === 0;
      };
    }
    return function (wnd) {
      return trim(getNodeContent(wnd.document.body)).length === 0;
    };
  })();

  var clearTextarea = (function () {
    if (simpleEditor === true) {
      return function (wnd) {
        wnd.value = '';
      };
    }
    return function (wnd) {
      var o = wnd.document.body;
	    while (o.firstChild) {
	      o.removeChild(o.firstChild);
	    }

      if (!browser.msie) {
        var bogus = wnd.document.createElement('br');
        bogus.setAttribute('type', 'moz_');
        wnd.document.body.appendChild(bogus);

        var selectionRange = wnd.document.createRange();
        selectionRange.selectNode(bogus);
        if (wnd.getSelection().rangeCount > 0) wnd.getSelection().removeAllRanges();
        wnd.getSelection().addRange(selectionRange);
        wnd.getSelection().collapseToStart();
      }
    };
  })();

  var getTextareaContent = (function () {
    if (simpleEditor === true) {
      return function (wnd) {
        return wnd.value;
      };
    }
    return function (wnd) {
      return wnd.document.body.innerHTML.replace(/\s+/g, ' ');
    };
  })();

  var init = function (id, cssfile) {
    if (isInit === true) {
      return undefined;
    }
    isInit = true;
    // TODO
    taStylePath = cssfile;

    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.createRoom, onCreateRoom);
    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.openRoom, onOpenRoom);
    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.closeRoom, onCloseRoom);

    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.openHistory, onOpenHistory);
    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.closeHistory, onCloseHistory);

    ASC.TMTalk.msManager.bind(ASC.TMTalk.msManager.events.addContact, onAddContact);
    ASC.TMTalk.msManager.bind(ASC.TMTalk.msManager.events.removeContact, onRemoveContact);

    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.connected, onClientConnected);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.disconnected, onClientDisconnected);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.connectingFailed, onClientConnectingFailed);
  };

  var createTextarea = (function () {
    if (simpleEditor === true) {
      return function (iframe) {
        if (typeof iframe === 'string') {
          var
            textareasContainer = document.getElementById('talkTextareaContainer'),
            roomId = iframe,
            node = null,
            nodes = null,
            nodesInd = 0;

          nodes = ASC.TMTalk.dom.getElementsByClassName(textareasContainer, 'textarea', 'li');
          nodesInd = nodes.length;
          while (nodesInd--) {
            node = nodes[nodesInd];
            if (node.getAttribute('data-roomid') === roomId) {
              nodes = node.getElementsByTagName('iframe');
              if (nodes.length > 0) {
                iframe = nodes[0];
              }
              break;
            }
          }
        }

        if (!iframe) {
          return undefined;
        }

        var textarea = document.createElement('textarea');
        iframe.parentNode.insertBefore(textarea, iframe);
        iframe.parentNode.removeChild(iframe);

        jQuery(textarea)
          .keydown(function (evt) {
            if (evt.target.tagName.toLowerCase() === 'textarea') {
              evt.originalEvent.stopPropagation ? evt.originalEvent.stopPropagation() : evt.originalEvent.cancelBubble = true;
            }
          })
          .keyup(ASC.TMTalk.meseditorContainer.keyUp)
          .focusout(ASC.TMTalk.meseditorContainer.setRange)
          .keypress(ASC.TMTalk.meseditorContainer.keyPress);

        ASC.TMTalk.meseditorContainer.startIntrevalSendButton();
      };
    }
    return function (iframe) {
      if (typeof iframe === 'string') {
        var
          textareasContainer = document.getElementById('talkTextareaContainer'),
          roomId = iframe,
          node = null,
          nodes = null,
          nodesInd = 0;

        nodes = ASC.TMTalk.dom.getElementsByClassName(textareasContainer, 'textarea', 'li');
        nodesInd = nodes.length;
        while (nodesInd--) {
          node = nodes[nodesInd];
          if (node.getAttribute('data-roomid') === roomId) {
            nodes = node.getElementsByTagName('iframe');
            if (nodes.length > 0) {
              iframe = nodes[0];
            }
            break;
          }
        }
      }

      if (!iframe) {
        return undefined;
      }
      iframe.setAttribute('src', 'javascript:false;');

      var html = [
        '<html xmlns="http://www.w3.org/1999/xhtml">',
          '<head>',
            '<link rel="stylesheet" type="text/css" href="' + taStylePath + '" />',
          '</head>',
          '<body',
            ' contentEditable="true" id="editable"',
            jQuery.browser.mozilla ? '' : ' onkeyup="return parent.ASC.TMTalk.meseditorContainer.keyUp(event)"',
            jQuery.browser.mozilla ? '' : ' onkeypress="return parent.ASC.TMTalk.meseditorContainer.keyPress(event)"',
            jQuery.browser.mozilla ? '' : ' onblur="parent.ASC.TMTalk.meseditorContainer.blur(event); return parent.TMTalk.blur(event)"',
            jQuery.browser.mozilla ? '' : ' onfocus="parent.ASC.TMTalk.meseditorContainer.focus(event); return parent.TMTalk.focus(event)"',
            jQuery.browser.mozilla ? '' : ' onclick="return parent.ASC.TMTalk.meseditorContainer.onClick(event)"',
            jQuery.browser.mozilla ? '' : ' onscroll="return parent.ASC.TMTalk.meseditorContainer.onscroll(event)"',
            jQuery.browser.mozilla ? '' : ' onkeydown="return parent.ASC.TMTalk.meseditorContainer.keyDown(event)"',
          '></body>',
        '</html>'
      ].join('');

      iframe.contentWindow.document.open();
      iframe.contentWindow.document.write(html);
      iframe.contentWindow.document.close();

      if (jQuery.browser.mozilla) {
        iframe.contentWindow.document.designMode = 'on';

        jQuery(iframe.contentWindow.document)
          .keydown(ASC.TMTalk.meseditorContainer.keyDown)
          .keyup(ASC.TMTalk.meseditorContainer.keyUp)
          .keypress(ASC.TMTalk.meseditorContainer.keyPress)
          .blur(TMTalk.blur).focus(TMTalk.focus).click(parent.TMTalk.clickcallback);
      }
        
      ASC.TMTalk.meseditorContainer.startIntrevalSendButton();

      var
        o = null,
        wnd = iframe.contentWindow;
      o = wnd.document.body;
      while (o.firstChild) {
        o.removeChild(o.firstChild);
      }
      if (!jQuery.browser.msie) {
        var bogus = wnd.document.createElement('br');
        bogus.setAttribute('type', 'moz_');
        wnd.document.body.appendChild(bogus);
      }

      nodes = wnd.document.getElementsByTagName('html');
      if (nodes.length > 0) {
        nodes[0].setAttribute('dir', 'ltr');
      }

      //try { wnd.document.execCommand('undo', false, null); } catch (err) {}
      try { wnd.document.execCommand('useCSS', false, true); } catch (err) {}
      try { wnd.document.execCommand('styleWithCSS',false, true); } catch (err) {}
      try { wnd.document.execCommand('enableObjectResizing', false, false); } catch (err) {}
    };
  })();

  var blur = function (evt) {
    taFocused = false;
  };

  var onClick = function (evt) {
      var currentRoom = jQuery('div#talkRoomsContainer ul.rooms li.room.current');
      if ((currentRoom.hasClass('conference') || currentRoom.hasClass('mailing')) && !currentRoom.hasClass('minimized')) {
          ASC.TMTalk.roomsContainer.minimizingUserList();
      }
  };
  var focus = function (evt) {
    taFocused = true;
  };

  var insertSmile = function (src, title) {
    if (taWindow) {
        insertSmileToTextarea(taWindow, src, title);
        $('div#talkTextareaContainer ul.textareas li.textarea.current div.meseditorContainerPlaceholder').hide();
    }
  };

  var setRange = (function () {
    if (simpleEditor === true) {
      return function () {
        if (taWindow !== null) {
          if (browser.msie) {
            if (taFocused === true) {
              maRange = document.selection.createRange();
              maRangeMark = maRange.getBookmark();
            }
          }

          if (document.selection) {
            maSelection = document.selection.createRange();
          } else if (typeof taWindow.selectionStart === 'number') {
            maSelection = {start : taWindow.selectionStart, end : taWindow.selectionEnd};
          }
        }
      };
    }
    return function () {
      if (taWindow !== null) {
        if (browser.msie) {
          if (taFocused === true) {
            maRange = taWindow.document.selection.createRange();
            maRangeMark = maRange.getBookmark();
          }
        }
      }
    };
  })();

  var setCursor = (function () {
    /*if (browser.touch) {
      return function (wnd) {
      };
    }*/
    if (simpleEditor === true) {
      return function (wnd) {
        window.focus();
        wnd.focus();
        if (wnd.createTextRange) {
          var r = wnd.createTextRange();
          r.collapse(false);
          r.select();
        }
      };
    }
    return function (wnd)  {
      if (!wnd || typeof wnd !== 'object') {
        var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
        if (currentRoomData === null) {
          return undefined;
        }

        var
          textareasContainer = document.getElementById('talkTextareaContainer'),
          roomId = currentRoomData.roomId,
          wnd = null,
          node = null,
          nodes = null,
          nodesInd = 0;

        nodes = ASC.TMTalk.dom.getElementsByClassName(textareasContainer, 'textarea', 'li');
        nodesInd = nodes.length;
        while (nodesInd--) {
          node = nodes[nodesInd];
          if (node.getAttribute('data-roomid') === roomId) {
            nodes = node.getElementsByTagName('iframe');
            if (nodes.length > 0) {
              wnd = nodes[0].contentWindow;
            }
            break;
          }
        }
      }

      if (!wnd) {
        return undefined;
      }

      window.focus();
      wnd.focus();

      if (jQuery.browser.msie) {
        var range = wnd.document.body.createTextRange(), textnode = wnd.document.createElement('span');

        textnode.appendChild(wnd.document.createTextNode('text'));
        var lastelement = ASC.TMTalk.dom.lastElementChild(wnd.document.body);
        (lastelement ? lastelement : wnd.document.body).appendChild(textnode);
        range.moveToElementText(textnode);
        range.select();

        range.pasteHTML('');
        range.collapse(true);
      } else {
        var range = wnd.document.createRange(), textnode = wnd.document.createElement('span');

        textnode.appendChild(wnd.document.createTextNode(''));
        wnd.document.body.insertBefore(textnode, ASC.TMTalk.dom.lastElementChild(wnd.document.body));
        range.selectNode(textnode);
        if (wnd.getSelection().rangeCount > 0) wnd.getSelection().removeAllRanges();
        wnd.getSelection().addRange(range);
        wnd.getSelection().collapseToStart();
        wnd.document.body.removeChild(textnode);
      }
    };
  })();

  var pausedMessage = function (jid) {
    if (ASC.TMTalk.connectionManager.connected() && composingMessages.hasOwnProperty(jid)) {
      clearTimeout(composingMessages[jid].handlerTimeout);
      delete composingMessages[jid];
      ASC.TMTalk.connectionManager.pausedMessage(jid);
    }
  };

  var pausedMessageCallback = function (jid) {
    return function () {
      pausedMessage(jid);
    }
  };

  var startIntrevalSendButton = function () {
    var interval = 1000;
    clearInterval(ASC.TMTalk.meseditorContainer.timeIntervalSendButton);
    ASC.TMTalk.meseditorContainer.timeIntervalSendButton = setInterval(ASC.TMTalk.meseditorContainer.intrevalSendButton, interval);
  };

  var intrevalSendButton = function () {
    if (taWindow) {
      if (emptyTexrarea(taWindow)) {
        ASC.TMTalk.dom.addClass("talkMeseditorContainer", "empty");
      } else {
        ASC.TMTalk.dom.removeClass("talkMeseditorContainer", "empty");
      }
    }
  };
  var resizeMeseditorcontainer = function () {

      var $talkMeseditorContainer = $('#talkMeseditorContainer'),
          talkMeseditorContainerHeight = parseInt($talkMeseditorContainer.css('height')),
          $talkVertSlider = $('#talkVertSlider'),
          contentContainer = document.getElementById('talkContentContainer'),
          $contentContainer = $(contentContainer),
          talkRoomsContainer = document.getElementById('talkRoomsContainer'),
          $talkRoomsContainer = $(talkRoomsContainer),
          talkTextareaContainer = document.getElementById('talkTextareaContainer'),
          $talkTextareaContainer = $(talkTextareaContainer),
          $frame = $talkTextareaContainer.find('ul.textareas li.current iframe'),
          bodyFrameHeight = $frame.contents().children().children()[1].offsetHeight,
          frameHeight = $frame[0].scrollHeight,
          $currentRoom = $talkRoomsContainer.find('li.room.current'),
          subPanel = ASC.TMTalk.dom.getElementsByClassName($currentRoom[0], 'sub-panel', 'div'),
          newHeight,newRoomsContainerHeight;
      
      newHeight = talkMeseditorContainerHeight + bodyFrameHeight - frameHeight;
      newRoomsContainerHeight = $contentContainer[0].offsetHeight - newHeight;
      
      
      if ((taWindow.scrollY > 0) || ((bodyFrameHeight - frameHeight) > 0)) {
          
          if (newHeight * 3 > $contentContainer[0].offsetHeight) { // if more 33%
              newHeight = $contentContainer.offsetHeight / 3; 
              newRoomsContainerHeight = $contentContainer[0].offsetHeight - newHeight;
          }
          correctionMeseditorcontainer($talkMeseditorContainer, $talkVertSlider, $talkRoomsContainer, subPanel, newRoomsContainerHeight, newHeight);
      } else {
          if ($talkTextareaContainer[0].offsetHeight > bodyFrameHeight) {
              
              if (!ASC.TMTalk.properties.meseditorHeight || ASC.TMTalk.properties.meseditorHeight < bodyFrameHeight) {
                  newHeight = bodyFrameHeight;
                  $talkMeseditorContainer.css('height', bodyFrameHeight + 'px');
                  newRoomsContainerHeight = $contentContainer[0].offsetHeight - bodyFrameHeight;
              } else if (ASC.TMTalk.properties.meseditorHeight >= bodyFrameHeight) {
                  newHeight = ASC.TMTalk.properties.meseditorHeight;
                  $talkMeseditorContainer.css('height', ASC.TMTalk.properties.meseditorHeight + 'px');
                  newRoomsContainerHeight = $contentContainer[0].offsetHeight - ASC.TMTalk.properties.meseditorHeight;
              }
              correctionMeseditorcontainer($talkMeseditorContainer, $talkVertSlider, $talkRoomsContainer, subPanel, newRoomsContainerHeight, newHeight);
          }
      }
  };
  function correctionMeseditorcontainer($talkMeseditorContainer, $talkVertSlider, $talkRoomsContainer, subPanel, newRoomsContainerHeight, newHeight) {
      var correctionForChat = 32,
          correctionForConference = 90;
      
      $talkMeseditorContainer.css('height', newHeight + 'px');
      $talkVertSlider.css('bottom', newHeight + 'px');
      if ($talkRoomsContainer.hasClass('chat')) {
          $talkRoomsContainer.css('height', (newRoomsContainerHeight - correctionForChat) + 'px');
      } else {
          $talkRoomsContainer.css('height', newRoomsContainerHeight + 'px');
          if (subPanel.length > 0) {
              subPanel[0].style.height = Math.ceil(newRoomsContainerHeight / 2) - correctionForConference + 'px';
          }
      }
  };
  var keyUp = function (evt) {
        var o = document.createElement('div');
        o.innerHTML = getTextareaContent(taWindow);
        if (translateSymbols(trim(getNodeContent(o))).length <= 0) {
            $('div#talkTextareaContainer ul.textareas li.textarea.current div.meseditorContainerPlaceholder').show();
        } else {
            $('div#talkTextareaContainer ul.textareas li.textarea.current div.meseditorContainerPlaceholder').hide();
        }
        resizeMeseditorcontainer();
        TMTalk.keyup(evt);
      
        if (taWindow) {
            if (emptyTexrarea(taWindow)) {
            ASC.TMTalk.dom.addClass('talkMeseditorContainer', 'empty');
            } else {
            ASC.TMTalk.dom.removeClass('talkMeseditorContainer', 'empty');
            }
        }
  };
    
  function isVisible(t, w) {
      var wt = w.scrollTop();
      var tt = t.offset().top;
      var tb = tt + t.height();
      return ((tb <= wt + w.height()) && (tt >= wt));
  }
    
  var keyDown = function (evt) {
      resizeMeseditorcontainer();
      
      //pageUp, pageDown hack for chrome
      if (jQuery.browser.chrome && (evt.keyCode == 33 || evt.keyCode == 34)) {
          var iframe = jq('#talkMeseditorContainer iframe');
          var iframeHeight = iframe[1].offsetHeight;
          var _doc = iframe.contents()[1];
          var editableDiv = _doc.getElementById("editable");
          var $editableDiv = jq(editableDiv);
         
          switch (evt.keyCode) {
              case 33:
                  $editableDiv.scrollTop($editableDiv.scrollTop() - iframeHeight);
                  if (editableDiv.childNodes.length > 2) {
                      for (var i = 1; i < editableDiv.childNodes.length; i++) {
                          if (isVisible(jq(editableDiv.childNodes[i]), $editableDiv)) {
                              _doc.getSelection().setPosition(editableDiv.childNodes[i], 0);
                              break;
                          }
                      }
                  }
                  return false;
              case 34:
                  $editableDiv.scrollTop($editableDiv.scrollTop() + iframeHeight);
                  
                  if (editableDiv.childNodes.length > 2) {
                      for (var i = 1; i < editableDiv.childNodes.length; i++) {
                          
                          if (isVisible(jq(editableDiv.childNodes[i]), $editableDiv)) {
                              if (jq(editableDiv.childNodes[i]).offset().top > iframeHeight + jq(editableDiv).scrollTop()) {
                                  _doc.getSelection().setPosition(editableDiv.childNodes[i - 1], 0);
                                  break;
                              }
                          }
                      }
                  }
                  return false;
              default:
        }
      }
  };
  var onscroll = function (evt) {
      resizeMeseditorcontainer();
  };
  var keyPress = function (evt) {
    switch (evt.keyCode) {
      case 9 :
        if (evt.shiftKey) {
          ASC.TMTalk.tabsContainer.nextTab();
          return false;
        }
        break;
      case 10 :
      case 13 :
        // if send by ctrl + enter
        if (ASC.TMTalk.properties.item('sndbyctrlentr') === '1') {
          // if pressed ctrl + enter
          if (evt.ctrlKey) {
            sendMessage();
          // or \n
          } else {
            return undefined;
          }
        // if send by enter
        } else {
          // set \n
          if (evt.ctrlKey) {
            insertNewLineToTextarea(taWindow);
          // if press ctrl
          } else if (!evt.shiftKey) {
              sendMessage();
          } else {
              return undefined;
          }
        }
        // short variant
        // if (evt.ctrlKey === (+ASC.TMTalk.properties.item('sndbyctrlentr') === 1)) {
        //  sendMessage();
        //} else {
        //  echoNewLineToTextArea(maWindow);
        //}
        return false;
      default :
        var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
        if (currentRoomData !== null && currentRoomData.type === 'chat') {
          var jid = currentRoomData.id;
          if (ASC.TMTalk.connectionManager.connected()) {
            if (composingMessages.hasOwnProperty(jid)) {
              clearTimeout(composingMessages[jid].handlerTimeout);
              composingMessages[jid].handlerTimeout = setTimeout(pausedMessageCallback(jid), pausedMesssagesTimeout * 1000);
            } else {
              composingMessages[jid] = {
                jid             : jid,
                handlerTimeout  : null
              };
              ASC.TMTalk.connectionManager.composingMessage(jid);
              composingMessages[jid].handlerTimeout = setTimeout(pausedMessageCallback(jid), pausedMesssagesTimeout * 1000);
            }
          }
        }
        break;
    }
  };

  var showErrorMessage = function (cid, message) {
    var o = document.createElement('div');
    o.innerHTML = message.replace(/\n/g, '');
    ASC.TMTalk.messagesManager.sendMessageToChat(cid, translateSymbols(getNodeContent(o)), true);
  };

  var sendMessage = function (cid, type, message) {
    if (typeof cid === 'string') {
      switch (type.toLowerCase()) {
        case 'chat' :
          var o = document.createElement('div');
          o.innerHTML = message.replace(/\n/g, '');
          pausedMessage(cid);
          ASC.TMTalk.messagesManager.sendMessageToChat(cid, translateSymbols(trim(getNodeContent(o))));
          break;
        case 'mailing' :
          var o = document.createElement('div');
          o.innerHTML = message.replace(/\n/g, '');
          ASC.TMTalk.msManager.sendMessage(cid, translateSymbols(trim(getNodeContent(o))));
          break;
        case 'conference' :
          var o = document.createElement('div');
          o.innerHTML = message.replace(/\n/g, '');
          ASC.TMTalk.messagesManager.sendMessageToConference(cid, translateSymbols(trim(getNodeContent(o))));
          break;
      }
      return undefined;
    }

    if (taWindow) {
      var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
      if (currentRoomData !== null) {
        var contentContainer = document.getElementById('talkContentContainer');
        var $contentContainer = $(contentContainer);
        var newRoomsContainerHeight = 0;
        switch (currentRoomData.type) {
          case 'chat' :
            if (ASC.TMTalk.connectionManager.connected()) {
              var o = document.createElement('div');
              o.innerHTML = getTextareaContent(taWindow);
              pausedMessage(currentRoomData.id);
              ASC.TMTalk.messagesManager.sendMessageToChat(currentRoomData.id, translateSymbols(trim(getNodeContent(o))));

              clearTextarea(taWindow);
            }
            if (ASC.TMTalk.properties.meseditorHeight) {
                newRoomsContainerHeight = $contentContainer[0].offsetHeight - ASC.TMTalk.properties.meseditorHeight;
                $('#talkMeseditorContainer').css('height', ASC.TMTalk.properties.meseditorHeight + 'px');
                $('#talkVertSlider').css('bottom', ASC.TMTalk.properties.meseditorHeight + 'px');
                $('#talkRoomsContainer').css('height', newRoomsContainerHeight + 'px');
            } else {
                newRoomsContainerHeight = $contentContainer[0].offsetHeight - 82;
                $('#talkMeseditorContainer').css('height', 50 + 'px');
                $('#talkVertSlider').css('bottom', 50 + 'px');
                $('#talkRoomsContainer').css('height', newRoomsContainerHeight + 'px');
            }
            break;
          case 'mailing' :
            if (ASC.TMTalk.connectionManager.connected() && ASC.TMTalk.msManager.getContacts(currentRoomData.id).length > 0) {
              var o = document.createElement('div');
              o.innerHTML = getTextareaContent(taWindow);
              ASC.TMTalk.msManager.sendMessage(currentRoomData.id, translateSymbols(trim(getNodeContent(o))));

              clearTextarea(taWindow);
            }
            if (ASC.TMTalk.properties.meseditorHeight) {
                newRoomsContainerHeight = $contentContainer[0].offsetHeight - ASC.TMTalk.properties.meseditorHeight;
                $('#talkMeseditorContainer').css('height', ASC.TMTalk.properties.meseditorHeight + 'px');
                $('#talkVertSlider').css('bottom', ASC.TMTalk.properties.meseditorHeight + 'px');
                $('#talkRoomsContainer').css('height', newRoomsContainerHeight + 'px');
            } else {
                newRoomsContainerHeight = $contentContainer[0].offsetHeight - 50;
                $('#talkMeseditorContainer').css('height', 50 + 'px');
                $('#talkVertSlider').css('bottom', 50 + 'px');
                $('#talkRoomsContainer').css('height', newRoomsContainerHeight + 'px');
            }
            break;
          case 'conference' :
            if (ASC.TMTalk.connectionManager.connected()) {
              var o = document.createElement('div');
              o.innerHTML = getTextareaContent(taWindow);
              ASC.TMTalk.messagesManager.sendMessageToConference(currentRoomData.id, translateSymbols(trim(getNodeContent(o))));

              clearTextarea(taWindow);
            }
            if (ASC.TMTalk.properties.meseditorHeight) {
                newRoomsContainerHeight = $contentContainer[0].offsetHeight - ASC.TMTalk.properties.meseditorHeight;
                $('#talkMeseditorContainer').css('height', ASC.TMTalk.properties.meseditorHeight + 'px');
                $('#talkVertSlider').css('bottom', ASC.TMTalk.properties.meseditorHeight + 'px');
                $('#talkRoomsContainer').css('height', newRoomsContainerHeight + 'px');
            } else {
                newRoomsContainerHeight = $contentContainer[0].offsetHeight - 50;
                $('#talkMeseditorContainer').css('height', 50 + 'px');
                $('#talkVertSlider').css('bottom', 50 + 'px');
                $('#talkRoomsContainer').css('height', newRoomsContainerHeight + 'px');
            }
            break;
        }
      }
    }
   
      
  };

  var onClientConnected = function () {
    ASC.TMTalk.dom.addClass('talkMeseditorContainer', 'connected');
    if (ASC.TMTalk.roomsManager.getRoomData()) {
      TMTalk.disableFileUploader(false);
    }
  };

  var onClientDisconnected = function () {
    for (var fld in composingMessages) {
      if (composingMessages.hasOwnProperty(fld)) {
        delete composingMessages[fld];
      }
    }
    ASC.TMTalk.dom.removeClass('talkMeseditorContainer', 'connected');
    TMTalk.disableFileUploader(true);
  };

  var onClientConnectingFailed = function () {
    for (var fld in composingMessages) {
      if (composingMessages.hasOwnProperty(fld)) {
        delete composingMessages[fld];
      }
    }
    ASC.TMTalk.dom.removeClass('talkMeseditorContainer', 'connected');
    TMTalk.disableFileUploader(true);
  };

  var onAddContact = function (listId, contactjid) {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (listId === currentRoomData.id) {
      ASC.TMTalk.dom.removeClass('talkMeseditorContainer', 'unavailable');
      TMTalk.disableFileUploader(false);
    }
  };

  var onRemoveContact = function (listId, contactjid) {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (listId === currentRoomData.id) {
      if (ASC.TMTalk.msManager.getContacts(listId).length === 0) {
        ASC.TMTalk.dom.addClass('talkMeseditorContainer', 'unavailable');
        TMTalk.disableFileUploader(true);
      } else {
        ASC.TMTalk.dom.removeClass('talkMeseditorContainer', 'unavailable');
        TMTalk.disableFileUploader(false);
      }
    }
  };

  var onCreateRoom = function (roomId, data) {
    var
      textareasContainer = document.getElementById('talkTextareaContainer'),
      nodes = null,
      newTextarea = null,
      defaultTextarea = null;

    nodes = ASC.TMTalk.dom.getElementsByClassName(textareasContainer, 'textarea default', 'li');
    if (nodes.length > 0) {
      defaultTextarea = nodes[0];
    }

    if (!defaultTextarea) {
      throw 'no templates';
    }

    newTextarea = defaultTextarea.cloneNode(true);
    newTextarea.className = newTextarea.className.replace(/\s*default\s*/, ' ').replace(/^\s+|\s+$/g, '');

    newTextarea.setAttribute('data-roomid', roomId);

    switch (data.type) {
      case 'chat' :
        break;
      case 'conference' :
        newTextarea.className += ' conference';
        break;
      case 'mailing' :
        break;
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(textareasContainer, 'textareas', 'ul');
    if (nodes.length > 0) {
      nodes[0].appendChild(newTextarea);
    }

    nodes = newTextarea.getElementsByTagName('iframe');
    if (nodes.length > 0) {
      if ($.browser.mozilla && simpleEditor === false) {
        setTimeout((function (id) {
          return function () {
            ASC.TMTalk.meseditorContainer.createTextarea(id);
          };
        })(roomId), 100);
      } else {
        ASC.TMTalk.meseditorContainer.createTextarea(nodes[0]);
      }
    }
  };

  var onOpenRoom = function (roomId, data, inBackground) {
    var
      textareasContainer = document.getElementById('talkTextareaContainer'),
      currentTextarea = null,
      node = null,
      nodes = null,
      nodesInd = 0;
    nodes = ASC.TMTalk.dom.getElementsByClassName(textareasContainer, 'textarea', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      node = nodes[nodesInd];
      if (node.getAttribute('data-roomid') === roomId) {
        currentTextarea = node;
        if (inBackground !== true) {
          ASC.TMTalk.dom.addClass(node, 'current');
        }
      } else {
        if (inBackground !== true) {
          ASC.TMTalk.dom.removeClass(node, 'current');
        }
      }
    }

    var meseditorContainer = document.getElementById('talkMeseditorContainer');
    ASC.TMTalk.dom.removeClass(meseditorContainer, 'disabled');
    TMTalk.disableFileUploader(false);

    if (inBackground !== true) {
      taWindow = null;
      if (currentTextarea !== null) {
        nodes = currentTextarea.getElementsByTagName('textarea');
        if (nodes.length > 0) {
          taWindow = nodes[0];
        }
        if (taWindow === null) {
          nodes = currentTextarea.getElementsByTagName('iframe');
          if (nodes.length > 0) {
            taWindow = nodes[0].contentWindow;
          }
        }
        if (!taWindow) {
          throw 'can\'t get textarea';
        }
        // my street magic. HOLY FUCKING SHIT FUCKS IE9.
        if (nodes.length > 0) {
          nodes[0].style.position = 'relative';
          nodes[0].style.position = 'absolute';
        }
      }

      switch (data.type) {
        case 'chat' :
          ASC.TMTalk.dom.removeClass(meseditorContainer, 'conference');
          ASC.TMTalk.dom.removeClass(meseditorContainer, 'mailing');
          ASC.TMTalk.dom.addClass(meseditorContainer, 'chat');

          ASC.TMTalk.dom.removeClass(meseditorContainer, 'unavailable');
          ASC.TMTalk.dom.removeClass(meseditorContainer, 'owner');
          TMTalk.disableFileUploader(false);       
          break;
        case 'mailing' :
          ASC.TMTalk.dom.removeClass(meseditorContainer, 'chat');
          ASC.TMTalk.dom.removeClass(meseditorContainer, 'conference');
          ASC.TMTalk.dom.addClass(meseditorContainer, 'mailing');

          ASC.TMTalk.dom.removeClass(meseditorContainer, 'owner');
          if (ASC.TMTalk.msManager.getContacts(data.id).length === 0) {
            ASC.TMTalk.dom.addClass(meseditorContainer, 'unavailable');
            TMTalk.disableFileUploader(true);
          } else {
            ASC.TMTalk.dom.removeClass(meseditorContainer, 'unavailable');
            TMTalk.disableFileUploader(false);
          }
          break;
        case 'conference' :
          ASC.TMTalk.dom.removeClass(meseditorContainer, 'chat');
          ASC.TMTalk.dom.removeClass(meseditorContainer, 'mailing');
          ASC.TMTalk.dom.addClass(meseditorContainer, 'conference');

          if (data.affiliation === 'owner') {
            ASC.TMTalk.dom.addClass(meseditorContainer, 'owner');
          } else {
            ASC.TMTalk.dom.removeClass(meseditorContainer, 'owner');
          }
          ASC.TMTalk.dom.removeClass(meseditorContainer, 'unavailable');

          TMTalk.disableFileUploader(false);
          break;
      }
      if (emptyTexrarea(taWindow)) {
        ASC.TMTalk.dom.addClass(meseditorContainer, 'empty');
      } else {
        ASC.TMTalk.dom.removeClass(meseditorContainer, 'empty');
      }
      ASC.TMTalk.dom.removeClass(meseditorContainer, 'history');
      if (TMTalk.properties.focused) {
        if ($.browser.mozilla && simpleEditor === false) {
          setTimeout(ASC.TMTalk.meseditorContainer.setCursor, 100);
        } else {
          ASC.TMTalk.meseditorContainer.setCursor(taWindow);
        }
      }
    }
    $('div#talkRoomsContainer ul.rooms li.room.current div.filtering-panel:first').on('transitionend', function () {
        var $filteringPanel = $(this),
            $input = $(this).find('div.filtering-container div.textfield input');
        
        if ($filteringPanel.hasClass('show')) {
            $input.focus();
        } else {
            $filteringPanel.addClass('close');
        }
    });
  };

  var onCloseRoom = function (roomId, data) {
    var
      textareasContainer = document.getElementById('talkTextareaContainer'),
      isCurrent = false,
      nodes = null,
      nodesInd = 0;
    nodes = ASC.TMTalk.dom.getElementsByClassName(textareasContainer, 'textarea', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-roomid') === roomId) {
        isCurrent = ASC.TMTalk.dom.hasClass(nodes[nodesInd], 'current');
        nodes[nodesInd].parentNode.removeChild(nodes[nodesInd]);
        break;
      }
    }

    if (isCurrent === true) {
      var meseditorContainer = document.getElementById('talkMeseditorContainer');
      ASC.TMTalk.dom.addClass(meseditorContainer, 'disabled');
      TMTalk.disableFileUploader(true);

      ASC.TMTalk.dom.removeClass(meseditorContainer, 'chat');
      ASC.TMTalk.dom.removeClass(meseditorContainer, 'mailing');
      ASC.TMTalk.dom.removeClass(meseditorContainer, 'conference');

      ASC.TMTalk.dom.removeClass(meseditorContainer, 'owner');
      ASC.TMTalk.dom.removeClass(meseditorContainer, 'history');
      ASC.TMTalk.dom.removeClass(meseditorContainer, 'unavailable');

      //TMTalk.disableFileUploader(false);

      taWindow = null;
    }
  };

  var onOpenHistory = function (jid) {
      //jq("div.cha").removeClass('hidden').addClass('visible');
      jq.each(jq("select.historyPeriod"), function (index, item) {
          jq(item).tlCombobox();
      });
      
      
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (currentRoomData.type === 'chat' && currentRoomData.id === jid) {
        ASC.TMTalk.dom.addClass('talkMeseditorContainer', 'history');
        window.focus();
        
    }
  };

  var onCloseHistory = function (jid) {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (currentRoomData.type === 'chat' && currentRoomData.id === jid) {
      ASC.TMTalk.dom.removeClass('talkMeseditorContainer', 'history');
      if ($.browser.mozilla && simpleEditor === false) {
        setTimeout(ASC.TMTalk.meseditorContainer.setCursor, 100);
      } else {
        ASC.TMTalk.meseditorContainer.setCursor(taWindow);
      }
      //jq("div.cha").removeClass('visible').addClass('hidden');
    }
  };

  var onSendFileStart = function () {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (currentRoomData !== null) {
      jQuery('#talkMeseditorToolbarContainer div.button-container.send-file').addClass('sending');
      ASC.TMTalk.properties.item('dstcontact', {cid : currentRoomData.id, type : currentRoomData.type});
    }
  }

  var onSendFileComplete = function (response) {
    jQuery('#talkMeseditorToolbarContainer div.button-container.send-file').removeClass('sending');
    if (response.Success === true) {
      var link = response.FileURL.substring(0, response.FileURL.lastIndexOf('/')) + '/' + encodeURIComponent(response.FileName);
      var dstcontact = ASC.TMTalk.properties.item('dstcontact');
      if (dstcontact && typeof dstcontact === 'object') {
          ASC.TMTalk.meseditorContainer.sendMessage(dstcontact.cid, dstcontact.type, ASC.TMTalk.stringFormat(jq.format(ASC.TMTalk.Resources.SendFileMessage, "{0}<br/>", "{1}"), link, response.Data));
        ASC.TMTalk.properties.item('dstcontact', null);
      }
    } else {
        ASC.TMTalk.meseditorContainer.onSendFileError(response.Message);
    }
  }

  var onSendFileError = function (message) {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (currentRoomData !== null) {
      ASC.TMTalk.properties.item('dstcontact', {cid : currentRoomData.id, type : currentRoomData.type});
    }
    var dstcontact = ASC.TMTalk.properties.item('dstcontact');
    if (dstcontact && typeof dstcontact === 'object') {
      ASC.TMTalk.meseditorContainer.showErrorMessage(dstcontact.cid, message);
    }
  };
  var openCreatingConferenceDialog = function() {
      jq("#chatOrSpam").tlCombobox();
      TMTalk.showDialog('create-room', null, { roomname: '', temproomchecked: true });
  };
    
  var openClearFilesDialog = function () {
      JabberClient.GetSpaceUsage(function (response) {
          if (response && response.value) {
              jq(".delete-files b").text(response.value);
              jq("#pop").hide();
              TMTalk.showDialog("delete-files", null, { roomname: '', temproomchecked: true });
          }
      });
  };

  var searchMessages = function() {
      jq.each(jq("select.historyPeriod"), function(index, item) {
          jq(item).tlCombobox();
      });
      var currentRoomData = ASC.TMTalk.roomsManager.getRoomData(),
          roomlistContainer = document.getElementById('talkRoomsContainer'),
          jid = currentRoomData.id,
          nodes = null,
          room = null,
          messagescontainer = null,
          searchinput = null,
          nodesInd = 0;
    
      if (currentRoomData.type === 'chat') {
        nodes = ASC.TMTalk.dom.getElementsByClassName(roomlistContainer, 'room', 'li');
          nodesInd = nodes.length;
          while (nodesInd--) {
              if (nodes[nodesInd].getAttribute('data-roomcid') === jid) {
                  room = nodes[nodesInd];
                  break;
              }
          }
          if (room !== null) {
              nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'messages', 'ul');
              if (nodes.length > 0) {
                  messagescontainer = nodes[0];
              }
          }
          if (messagescontainer !== null) {
              ASC.TMTalk.roomsContainer.resetHistorySearch(room, ASC.TMTalk.dom.getElementsByClassName(messagescontainer, 'message', 'li'));
          }
          ASC.TMTalk.dom.toggleClass('talkRoomsContainer', 'searchmessage');
          if (room !== null) {
              nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'search-value', 'input');
              if (nodes.length > 0) {
                  searchinput = nodes[0];
              }
              $(searchinput).focus();
          }
      }
  };

  return {
    init  : init,

    createTextarea  : createTextarea,
    insertSmile     : insertSmile,
    setCursor       : setCursor,
    setRange        : setRange,

    startIntrevalSendButton: startIntrevalSendButton,
    intrevalSendButton: intrevalSendButton,
    keyPress  : keyPress,
    keyUp: keyUp,
    keyDown: keyDown,
    focus     : focus,
    blur    : blur,
    onscroll: onscroll,
    onClick: onClick,

    sendMessage : sendMessage,
    showErrorMessage: showErrorMessage,

    onSendFileStart     : onSendFileStart,
    onSendFileComplete  : onSendFileComplete,
    onSendFileError: onSendFileError,
    
    openCreatingConferenceDialog: openCreatingConferenceDialog,
    openClearFilesDialog: openClearFilesDialog,
    searchMessages: searchMessages
  };
})(jQuery);

(function ($) {
  $(function () {
    if ($.browser.safari) {
      $('#talkMeseditorToolbarContainer div.toolbar:first div.button-container').not('.send-file').not('.emotions').mousedown(function () {
        if (window.getSelection) {
          window.getSelection().removeAllRanges();
        }
        return false;
      });

      $('#talkSendMenu').mousedown(function () {
        if (window.getSelection) {
          window.getSelection().removeAllRanges();
        }
        return false;
      });
    }

    $(document).mousedown(!$.browser.msie ? null : function () {
      ASC.TMTalk.meseditorContainer.setRange();
    });

    if (ASC.TMTalk.properties.item('sndbyctrlentr') === undefined) {
      ASC.TMTalk.properties.item('sndbyctrlentr', '1', true);
    }

    if (ASC.TMTalk.properties.item('sndbyctrlentr') === '0') {
        $('#talkMeseditorToolbarContainer div.button-container.toggle-sendbutton:first').removeClass('send-by-ctrlenter');
        $('#button-send-ctrl-enter').removeClass('on').addClass('off');
    } else {
        $('#talkMeseditorToolbarContainer div.button-container.toggle-sendbutton:first').addClass('send-by-ctrlenter');
        $('#button-send-ctrl-enter').removeClass('off').addClass('on');
    }

    if ($.support.touch) {
      $('#talkMeseditorToolbarContainer div.button-container.send-file:first').addClass('not-available');
    }

    TMTalk.createFileUploader();

    $('#talkMeseditorToolbarContainer div.button-talk.create-massend:first').click(function() {
      TMTalk.showDialog('create-mailing', null, {mailingname : ''});
    });

   // $('#talkMeseditorToolbarContainer div.button-talk.create-conference:first').click(function() {
   //   TMTalk.showDialog('create-room', null, {roomname : '', temproomchecked : true});
   // });
    
    
    
    $('#talkMeseditorToolbarContainer div.button-talk.emotions:first').click(function () {
      var $container = null;
      if(($container = $('#talkMeseditorToolbarContainer div.button-container.emotions:first div.container:first')).is(':hidden')) {
        $container.show('fast', function () {
          if ($.browser.msie) {
            window.focus();
          }
          $(document).one('click', function () {
            $('#talkMeseditorToolbarContainer div.button-container.emotions:first div.container:first').hide();
          });
        });
      }
    });

    $('#talkMeseditorToolbarContainer ul.smiles:first').click(function (evt) {
      var $target = $(evt.target);
      if ($target.hasClass('smile')) {
        var
          title = $target.attr('title'),
          smilesrc = $target.css('backgroundImage');
        title = typeof title === 'string' ? title : '';
        smilesrc = typeof smilesrc === 'string' ? smilesrc.replace(/^url|[\("'\)]+/g, '') : null;
        if (smilesrc) {
          ASC.TMTalk.meseditorContainer.insertSmile(smilesrc, title);
        }
      }
    });

    $('#talkMeseditorToolbarContainer div.button-talk.history:first').click(function() {
        var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
        
      if (currentRoomData !== null && currentRoomData.type === 'chat') {
        var jid = currentRoomData.id;
        if (jid) {
           // jq("#cha").tlCombobox();
            ASC.TMTalk.messagesManager.openHistory(jid);
   
            //jq("#cha").tlCombobox();
            
        }
      }
    });

    $('#talkMeseditorToolbarContainer div.button-talk.toggle-sendbutton:first').click(function() {
      ASC.TMTalk.properties.item('sndbyctrlentr', $('#talkMeseditorToolbarContainer div.button-container.toggle-sendbutton:first').toggleClass('send-by-ctrlenter').hasClass('send-by-ctrlenter') ? '1' : '0', true);
    });

    $('#talkSendMenu div.button-talk.send-message:first').click(function() {
        ASC.TMTalk.meseditorContainer.sendMessage();
        var currentRoom = jQuery('div#talkRoomsContainer ul.rooms li.room.current');
        if ((currentRoom.hasClass('conference') || currentRoom.hasClass('mailing')) && !currentRoom.hasClass('minimized')) {
            ASC.TMTalk.roomsContainer.minimizingUserList();
        }
        ASC.TMTalk.meseditorContainer.setCursor();
    });

    $('#talkDialogsContainer').click(function (evt) {
        var $target = $(evt.target);
        if ($target.hasClass('button-talk') && $target.hasClass('create-room')) {
            
            var roomname = ASC.TMTalk.trim($('#txtRoomName').val());
            var type = $('#chatOrSpam:first')[0].options.selectedIndex;
            if (type == 0)
            {
                if (roomname && ASC.TMTalk.mucManager.isValidName(roomname)) {
                    ASC.TMTalk.mucManager.createRoom(roomname, $('#cbxTemporaryRoom').is(':checked'));
                    TMTalk.hideDialog();
                } else {
                    //$('#txtRoomName').parents('div.textfield:first').addClass('invalid-input');
                    $('#txtRoomName').addClass('invalid-input');
                }

            }
            else if (type == 1) {
                var mailingname = roomname;
                if (mailingname && ASC.TMTalk.msManager.isValidName(mailingname)) {
                    ASC.TMTalk.msManager.createList(mailingname);
                    TMTalk.hideDialog();
                } else {
                    // $('#txtRoomName').parents('div.textfield:first').addClass('invalid-field');
                    $('#txtRoomName').addClass('invalid-input');
                }

           }                         
        } else if ($target.hasClass('option-item')) {
            if ($target[0].getAttribute('data-value') && $target[0].getAttribute('data-value') === 'Spam message') {

                $('div.popupContainerClass.dialog.create-room input#cbxTemporaryRoom').parents('table:first').hide();
            } else {
                $('div.popupContainerClass.dialog.create-room input#cbxTemporaryRoom').parents('table:first').show();
            }
            
        } else if ($target.hasClass('delete-files-btn')) {
            JabberClient.ClearSpaceUsage($('input[name=clearType]:checked').val(), function(response) {
                PopupKeyUpActionProvider.CloseDialog();
                if (response && response.value) {
                    $('#pop').addClass('has-files');
                } else {
                    $('#pop').removeClass('has-files');
                }
            });
        }
    });
  });
})(jQuery);
