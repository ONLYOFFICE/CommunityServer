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


if (typeof window.console === 'undefined') {
  window.console = {};
}

//window.console._log = function () {
//  console.log.apply(this, arguments);
//}

/* (function () {
  var
    consoleId = 'console-' + Math.floor(Math.random() * 1000000),
    _log = [];

  if (typeof window.console == 'undefined') {
    window.console = {};
  }
  if (typeof window.console.log === 'undefined') {
    window.console.log = function (message) {
      return undefined;
      var console = document.getElementById(consoleId);
      if (!console) {
        console = document.createElement('textarea');
        document.body.appendChild(console);
        console.id = consoleId;
        console.setAttribute('readonly', 'readonly');
        console.style.position = 'absolute';
        console.style.zIndex = '6666';
        console.style.left = '0';
        console.style.bottom = '0';
        console.style.width = '50%';
        console.style.height = '200px';
        console.style.overflowY = 'scroll';
        console.style.whiteSpace = 'pre';
        console.style.background = '#FFF';
        console.style.border = '0';
      }
      console.value += message + '\n';
    };
  }

  window.console._log = function (msg) {
    _log.push(msg);
  };
  window.console._printLog = function () {
    for (var i = 0, n = _log.length; i < n; i++) {
      window.console.log(_log[i]);
    }
    _log = [];
  };
})(); */

window.TMTalk = (function ($) {
  var
    isInit = false,
    $body = null,
    onblurTimeoutHandler = 0,
    maxlengthMessage = 50,
    setDefaultIcon = false,
    originalTitle = '',
    indicatorTitle = '***',
    postfixTitle = '',
    unreadMessageCount = 0,
    maxIndicatorLength = 3,
    dialogsQueue = [],
    properties = {
      focused : false
    },
    customEvents = {
      pageKeyup : 'onpagekeyup',
      pageBlur : 'onpageblur',
      pageFocus : 'onpagefocus'
    },
    eventManager = new CustomEvent(customEvents);

  var init = function () {

    if (isInit === true) {
      return undefined;
    }
    isInit = true;

    // TODO
    //properties.focused = true;
    originalTitle = document.title;
    if (!window.name) {
      try {window.name = ASC.Controls.JabberClient.winName} catch (err) {}
    }

    postfixTitle = ASC.TMTalk.Resources.LabelNewMessage || '';

    if (window.moment) {
        window.moment.locale(ASC.Resources.Master.TwoLetterISOLanguageName);
    }
    var
      fld = '',
      engine = {},
      browser = {},
      version = -1,
      $body = $(document.body),
      ua = navigator.userAgent;

    version = (ua.match(/.+(?:rv|it|ra|ie|ox|me|on|id|os)[\/:\s]([\d._]+)/i)||[0,'0'])[1].replace('_', '');
    version = isFinite(parseFloat(version)) ? parseFloat(version) : version;
    engine = {
      'engn-ios'      : /iphone|ipad/i.test(ua),
      'engn-android'  : /android/i.test(ua),
      'engn-gecko'    : /gecko/i.test(ua) && !/webkit/i.test(ua),
      'engn-webkit'   : /webkit/i.test(ua)
    };
    browser = {
      'brwr-msie'     : '\v' == 'v' || /msie/i.test(ua),
      'brwr-opera'    : window.opera ? true : false,
      'brwr-chrome'   : window.chrome ? true : false,
      'brwr-safari'   : /safari/i.test(ua) && !window.chrome,
      'brwr-firefox'  : /firefox/i.test(ua)
    };
    for (fld in engine) {
      if (engine.hasOwnProperty(fld)) {
        if (engine[fld]) {
          $body.addClass(fld);
        }
      }
    }
    for (fld in browser) {
      if (browser.hasOwnProperty(fld)) {
        if (browser[fld]) {
          $body.addClass(fld);
          $body.addClass(fld + '-' + version);
        }
      }
    }

    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.recvInvite, onRecvInvite);
    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.acceptInvite, onCloseInvite);
    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.declineInvite, onCloseInvite);

    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.recvMessageFromChat, onRecvMessageFromChat);

    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.createRoom, onCreateConference);

    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.openRoom, onOpenRoom);
    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.closeRoom, onCloseRoom);

    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.connected, onClientConnected);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.disconnected, onClientDisconnected);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.connectingFailed, onClientConnectingFailed);

    TMTalk.bind(TMTalk.events.pageFocus, onPageFocus);

    ASC.TMTalk.indicator.bind(ASC.TMTalk.indicator.events.start, onStartIndicator);
    ASC.TMTalk.indicator.bind(ASC.TMTalk.indicator.events.show, onShowIndicator);
    ASC.TMTalk.indicator.bind(ASC.TMTalk.indicator.events.stop, onStopIndicator);
      
    if (document.getElementById('talkHtmlSoundsContainer') == null) {
        ASC.TMTalk.sounds.initHtml();
    }
  };

  var bind = function (eventName, handler, params) {
    return eventManager.bind(eventName, handler, params);
  };

  var unbind = function (handlerId) {
    return eventManager.unbind(handlerId);
  };

  var blur = function (evt) {
    clearTimeout(onblurTimeoutHandler);
    onblurTimeoutHandler = setTimeout(TMTalk.blurcallback, 1);
  };

  var focus = function (evt) {
    clearTimeout(onblurTimeoutHandler);

    if (properties.focused === false) {
      properties.focused = true;
      eventManager.call(customEvents.pageFocus, window, [evt]);
    }
  };

  var blurcallback = function (evt) {
    if (properties.focused === true) {
      properties.focused = false;
      eventManager.call(customEvents.pageBlur, window, [evt]);
    }
  };

  var click = function (evt) {
    clearTimeout(onblurTimeoutHandler);
    setTimeout(TMTalk.focus, 2);
  };

  var clickcallback = function (evt) {
    if (window.getSelection) {
      window.getSelection().removeAllRanges();
    }

    $(document).trigger('click');
  };

  var keyup = function (evt) {
    eventManager.call(customEvents.pageKeyup, window, [evt]);
  };

  var onStartIndicator = function () {
    setDefaultIcon = true;
  };

  var onShowIndicator = function () {
    //var currenttitle = document.title;
    //if (currenttitle !== indicatorTitle + ' ' + originalTitle) { 
    //  originalTitle = currenttitle;
    //}
    indicatorTitle += indicatorTitle.charAt(0);
    if (indicatorTitle.length > maxIndicatorLength) {
      indicatorTitle = indicatorTitle.charAt(0);
    }
    unreadMessageCount = window.ASC.TMTalk.contactsContainer.getUnreadMessageCount();
    document.title = indicatorTitle + "(" + unreadMessageCount+ ") -" + ' ' + originalTitle;

    if (setDefaultIcon) {
        ASC.TMTalk.iconManager.set(ASC.TMTalk.Icons.iconNewMessage);
        setDefaultIcon = !setDefaultIcon;
    }
  };

  var onStopIndicator = function () {
    indicatorTitle = '***';
    document.title = originalTitle;
    ASC.TMTalk.iconManager.reset();
    setDefaultIcon = true;
  };

  var onPageFocus = function () {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (currentRoomData !== null && currentRoomData.type === 'chat') {
      ASC.TMTalk.notifications.hide(currentRoomData.id);
    }

  };
    var openRoom = function(key, inBackground) {
        setTimeout(function() {
            ASC.TMTalk.contactsContainer.openRoom(key, inBackground);
        }, 300);
    };
    var reopenOpenedRooms = function (openedRooms) {
        for (key in openedRooms) {
            if (ASC.TMTalk.contactsManager.getContact(key) != null) {
                if (openedRooms[key].type == 'chat') {
                    if (ASC.TMTalk.roomsManager.getRoomDataById(key) == null) {
                        openRoom(key, openedRooms[key].inBackground);
                    }
                }
            } else {
                setTimeout(function () {
                    reopenOpenedRooms(openedRooms);
                }, 300);
                break;
            }
        }
    };

  var onClientConnected = function () {
      $(document.body).addClass('connected');
      var openedRooms = localStorageManager.getItem("openedRooms");

      if (openedRooms != undefined) {
          reopenOpenedRooms(openedRooms);
      }
  };

  var onClientDisconnected = function () {
    $(document.body).removeClass('connected');
  };

  var onClientConnectingFailed = function () {
    $(document.body).removeClass('connected');
  };

  var onCreateConference = function (roomjid, room) {
    ASC.TMTalk.mucManager.setSubject(roomjid, ASC.TMTalk.stringFormat(ASC.TMTalk.Resources.DefaultConferenceSubjectTemplate, room.name));
  };

  var onOpenRoom = function (roomId, data, inBackground) {
    if (inBackground !== true) {
      if (TMTalk.properties.focused && data.type === 'chat') {
          ASC.TMTalk.notifications.hide(data.id);
      }

      $('#talkContentContainer').removeClass('disabled');
      TMTalk.disableFileUploader(false);
    }
  };

  var onCloseRoom = function (roomId, data, isCurrent) {
    if (isCurrent === true) {
      $('#talkContentContainer').addClass('disabled');
      TMTalk.disableFileUploader(true);
    }
  };

  var onRecvInvite = function (roomjid, inviterjid, reason) {
    if (TMTalk.properties.focused === false) {
      ASC.TMTalk.notifications.show(
        roomjid,
        ASC.TMTalk.Resources.TitleRecvInvite + ' ' + ASC.TMTalk.mucManager.getConferenceName(roomjid),
        ASC.TMTalk.mucManager.getContactName(inviterjid) + ' ' + ASC.TMTalk.Resources.LabelRecvInvite
      );
    }
  };

  var onCloseInvite = function (roomjid, name, inviterjid) {
    ASC.TMTalk.notifications.hide(roomjid);
  };

  var onRecvMessageFromChat = function (jid, displayname, displaydate, date, body) {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (TMTalk.properties.focused === false || currentRoomData !== null && currentRoomData.type === 'chat' && currentRoomData.id !== jid) {
      ASC.TMTalk.notifications.show(
        jid,
        ASC.TMTalk.contactsManager.getContactName(jid),
        body.length < maxlengthMessage ? body : body.substring(0, maxlengthMessage) + '...'
      );
    }
  };

  var showDialog = function (name, titleval, data, invited) {
    if (arguments.length === 0 && dialogsQueue.length > 0) {
      var dialog = dialogsQueue.pop();
      if (dialog) {
        name      = dialog.name;
        titleval  = dialog.titleval;
        data      = dialog.data;
      }
    }

    if (typeof name !== 'string') {
      return undefined;
    }

    name = name.toLowerCase();
    switch (name) {
      case 'create-room' :
      case 'remove-room' :
      case 'recv-invite' :
      case 'kick-occupant' :
      case 'create-mailing' :
      case 'browser-notifications':
      case 'delete-files':
        break;
      default :
        return undefined;
    }
    //hideAllDialogs();

    var dialogsContainer = document.getElementById('talkDialogsContainer');

    if (!!dialogsContainer.className) {
      if (name === 'recv-invite' && dialogsContainer.className === name) {
        var roomjid = $('#hdnInvitationRoom').val(), inviterjid = $('#hdnInvitationContact').val();
        if (data.roomjid === roomjid && data.inviterjid === inviterjid) {
          return undefined;
        }
      }
      if (name === 'recv-invite' && dialogsQueue.length > 0) {
        var roomjid = data.roomjid, inviterjid = data.inviterjid, dialogsInd = dialogsQueue.length;
        while (dialogsInd--) {
          if (dialogsQueue[dialogsInd].data.roomjid === roomjid && dialogsQueue[dialogsInd].data.inviterjid === inviterjid) {
            break;
          }
        }
        if (dialogsInd >= 0) {
          return undefined;
        }
      }

      dialogsQueue.push({name : name, titleval : titleval, data : data});
      return undefined;
    }

    if (typeof titleval === 'string') {
      $(dialogsContainer)
        .find('div.dialog.' + name + ':first')
        .find('div.head:first div.title:first span.value:first').empty().html(titleval);

      $(dialogsContainer)
      .find('div.dialog.' + name + ':first')
      .find('div.title:first span.value:first').empty().html(titleval);
    }

    $('#txtRoomName').removeClass('invalid-input');
    $('#txtMailingName').parents('div.textfield:first').removeClass('invalid-field');

    if (data && typeof data === 'object') {
      switch (name) {
        case 'create-room' :
          if (data.hasOwnProperty('roomname')) {
            $('#txtRoomName').val(data.roomname);
          }
          if (data.hasOwnProperty('temproomchecked')) {
            $('#cbxTemporaryRoom').attr('checked', data.temproomchecked);
          }
          break;
        case 'remove-room' :
          if (data.hasOwnProperty('roomjid')) {
            $('#hdnRemoveJid').val(data.roomjid);
          }
          break;
        case 'recv-invite' :
          if (data.hasOwnProperty('roomjid')) {
            $('#hdnInvitationRoom').val(data.roomjid);
          }
          if (data.hasOwnProperty('inviterjid')) {
            $('#hdnInvitationContact').val(data.inviterjid);
          }
          if (data.hasOwnProperty('contactname')) {
            $('#lblInviterName').empty().html(data.contactname);
          }
          break;
        case 'kick-occupant' :
          if (data.hasOwnProperty('contactjid')) {
            $('#hdnKickJId').val(data.contactjid); 
            $('#hdnKickInvited').val(invited.invited);
            $('#hdnKickRoomCid').val(invited.roomCid);
          }
          break;
        case 'create-mailing' :
          if (data.hasOwnProperty('mailingname')) {
            $('#txtMailingName').val(data.mailingname);
          }
          break;
      }
    }

    $(dialogsContainer)
      .attr('class', name)
      .find('div.dialog.' + name + ':first')
        .css({opacity : '0', display : 'block'})
        .animate({opacity : 1}, $.browser.msie && $.browser.version < 9 ? 0 : 'middle', function () {
          // TODO:
          var $block = $(this);
          $block.find('div.textfield:first input:first').focus();
          if ($.browser.msie && $.browser.version < 9 && $block.length !== 0) {
            var
              prefix = ' ',
	            block = $block.get(0),
	            cssText = prefix + block.style.cssText,
	            startPos = cssText.toLowerCase().indexOf(prefix + 'filter:'),
	            endPos = cssText.indexOf(';', startPos);
	          if (startPos !== -1) {
	            if (endPos !== -1) {
	              block.style.cssText = [cssText.substring(prefix.length, startPos), cssText.substring(endPos + 1)].join('');
	            } else {
	              blockUI.style.cssText = cssText.substring(prefix.length, startPos);
	            }
	          }
	        }
        });
  };

  var hideDialog = function () {
    $('#talkDialogsContainer').find('div.dialog:visible').animate({opacity : 0}, $.browser.msie && $.browser.version < 9 ? 0 : 'middle', function () {
      jQuery(this).hide();

      var container = document.getElementById('talkDialogsContainer');
      if (container) {
        container.className = '';
      }

      TMTalk.showDialog();
    });
  };

  var hideAllDialogs = function () {
    $('#talkDialogsContainer').find('div.dialog').hide();
    var container = document.getElementById('talkDialogsContainer');
    if (container) {
      container.className = '';
    }
  };

  function createFileuploadInput(browseButtonId) {
      var buttonObj = jq("#" + browseButtonId);

      var inputObj = jq("<input/>")
          .attr("id", "fileupload")
          .attr("type", "file")
          .css("width", "0")
          .css("height", "0");

      inputObj.appendTo(buttonObj.parent());

      buttonObj.on("click", function (e) {
          e.preventDefault();
          jq("#fileupload").click();
      });

      return inputObj;
  }

  function correctFile(file) {
      if (file.size > ASC.TMTalk.properties.item("maxUploadSize")) {
          var message = ASC.TMTalk.properties.item("maxUploadSizeError");
          ASC.TMTalk.meseditorContainer.onSendFileError(message);
          return false;
      }

      return true;
  }

    var createFileUploader = function () {
        var uploader = createFileuploadInput("talkFileUploader");

        uploader.fileupload({
            url: "UploadProgress.ashx?submit=ASC.Web.Talk.HttpHandlers.UploadFileHandler,ASC.Web.Talk",
            autoUpload: false,
            singleFileUploads: true,
            sequentialUploads: true,
            progressInterval: 1000,
            dropZone: "talkRoomsContainer"
        });

        uploader
            .bind("fileuploadadd", function (e, data) {
                if (correctFile(data.files[0])) {
                    ASC.TMTalk.meseditorContainer.onSendFileStart();
                    TMTalk.disableFileUploader(true);
                    data.submit();
                }
            })
            .bind("fileuploaddone", function (e, data) {
                var res = jq.parseJSON(data.result);
                ASC.TMTalk.meseditorContainer.onSendFileComplete(res);
                TMTalk.disableFileUploader(false);
                jq("#pop").addClass("has-files");
            })
            .bind("fileuploadfail", function (e, data) {
                var msg = data.errorThrown || data.textStatus;
                if (data.jqXHR && data.jqXHR.responseText)
                    msg = jq.parseJSON(data.jqXHR.responseText).Message;

                ASC.TMTalk.meseditorContainer.onSendFileError(msg);
                TMTalk.disableFileUploader(false);
            });

    };

    var disableFileUploader = function (disable) {
        if (!jq("#talkMeseditorContainer").is(".connected")
            || jq("#talkMeseditorContainer").is(".disabled")
            || jq("#talkMeseditorContainer").is(".unavailable")) {
            disable = true;
        }

        if (disable === false) {
            jq("#talkFileUploaderFake").remove();
            jq("#talkFileUploader").removeClass("display-none");
        }
        jq("#fileupload").prop("disabled", disable);
    };

  return {
    events      : customEvents,
    properties  : properties,

    init    : init,
    bind    : bind,
    unbind  : unbind,

    keyup         : keyup,
    focus         : focus,
    blur          : blur,
    click         : click,
    blurcallback  : blurcallback,
    clickcallback : clickcallback,

    showDialog      : showDialog,
    hideDialog      : hideDialog,
    hideAllDialogs  : hideAllDialogs,

    createFileUploader: createFileUploader,
    disableFileUploader: disableFileUploader,
  };
})(jQuery);

(function ($) {
  var
    constants = {
      propertySidebarWidth : 'sbw',
      propertyMeseditorHeight : 'meh'
    },
    minPageHeight = 300,
    offsetSidebarContainer = 0,
    offsetMeseditorContainer = 0,
    minMeseditorContainer = 50,
    minSidebarContainerWidth = 252,
    mcHeightOffset = 0,
    ccWidthOffset = 0,
    ccHeightOffset = 0,
    rcHeightOffset = 0,
    vsBottomOffset = 0,
    ssHeightOffset = 0,
    $window = null,
    horSlider = null, $horSlider = null,
    vertSlider = null, $vertSlider = null,
    startSplash = null, $startSplash = null,
    mainContainer = null, $mainContainer = null,
    contentContainer = null, $contentContainer = null,
    roomsContainer = null, $roomsContainer = null,
    meseditorContainer = null, $meseditorContainer = null,
    sidebarContainer = null, $sidebarContainer = null,
    contactsContainer = null, $contactsContainer = null,
    contactToolbarContainer = null, $contactToolbarContainer = null,
    meseditorToolbarContainer = null, $meseditorToolbarContainer = null;

  var platform = (function () {
    var
      ua = navigator.userAgent,
      version = (ua.match(/.+(?:rv|it|ra|ie|ox|me|on|id|os)[\/:\s]([\d._]+)/i)||[0,'0'])[1].replace('_', '');

    return {
      version : isFinite(parseFloat(version)) ? parseFloat(version) : version,
      android : /android/i.test(ua),
      ios     : /iphone|ipad/i.test(ua)
    }
  })();

  $.extend($, {platform : platform});

  $.extend(
    $.support,
    {
      webapp            : window.innerHeight === window.screen.availHeight,
      platform          : platform,
      orientation       : 'orientation' in window,
      touch             : 'ontouchend' in document,
      csstransitions    : 'WebKitTransitionEvent' in window,
      pushState         : !!history.pushState,
      cssPositionFixed  : !('ontouchend' in document),
      iscroll           : $.platform.ios || $.platform.android,
      svg               : !($.browser.mozilla === true),
      dataimage         : !($.browser.msie && $.browser.version < 9)
    }
  );

//-------------------------------------------------------------------------------------------
  function onDragStart () {
    return false;
  }

  function onSelectStart () {
    return false;
  }
//-------------------------------------------------------------------------------------------
  function onMouseMoveVertSlider(evt) {
    var
      contentContainerHeight = $contentContainer.height(),
      newMeseditorHeight = document.body.offsetHeight - evt.pageY - offsetMeseditorContainer;

    if (newMeseditorHeight * 3 > contentContainerHeight) { // if more 33%
      newMeseditorHeight = Math.floor(contentContainerHeight / 3);
    }
    if (newMeseditorHeight < minMeseditorContainer) {
      newMeseditorHeight = minMeseditorContainer;
    }
    ASC.TMTalk.properties.meseditorHeight = newMeseditorHeight;
    meseditorContainer.style.height = newMeseditorHeight + 'px';
    //$meseditorContainer.height(newMeseditorHeight);
    vertSlider.style.bottom = (newMeseditorHeight - 5) + 'px';
    //$vertSlider.css('bottom', newMeseditorHeight + vsBottomOffset + 'px');
    roomsContainer.style.height = contentContainerHeight - meseditorContainer.offsetHeight - roomsContainer.offsetTop - rcHeightOffset - 22 + (ASC.TMTalk.dom.hasClass(roomsContainer, 'multichat') ? 32:0) + 'px';
    //$roomsContainer.height(contentContainerHeight - $meseditorContainer.height() - parseInt($roomsContainer.css('top')) - rcHeightOffset);

    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (currentRoomData !== null && currentRoomData.type === 'conference' && currentRoomData.minimized === false) {
      var nodes = ASC.TMTalk.dom.getElementsByClassName(roomsContainer, 'room conference current', 'li');
      if (nodes.length > 0) {
        var roomHeight = nodes[0].offsetHeight;
        nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'sub-panel', 'div');
        if (nodes.length > 0) {
          nodes[0].style.height = Math.ceil(roomHeight / 2) - 90 + 'px';
        }
      }
    }
    if (currentRoomData !== null && currentRoomData.type === 'mailing' && currentRoomData.minimized === false) {
        var nodes = ASC.TMTalk.dom.getElementsByClassName(roomsContainer, 'room mailing current', 'li');
        if (nodes.length > 0) {
            var roomHeight = nodes[0].offsetHeight;
            nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'sub-panel', 'div');
            if (nodes.length > 0) {
                nodes[0].style.height = Math.ceil(roomHeight / 2) - 90 + 'px';
            }
        }
    }
    return false;
  }

  function onMouseUpVertSlider (evt) {
    ASC.TMTalk.properties.item(constants.propertyMeseditorHeight, meseditorContainer.offsetHeight, true);
    //ASC.TMTalk.properties.item(constants.propertyMeseditorHeight, $meseditorContainer.height(), true);
    $meseditorContainer.removeClass('blocked');
    $window.focus();

    $(document).unbind('dragstart', onDragStart);
    $(document).unbind('mouseup', onMouseUpVertSlider);
    $(document).unbind('mousemove', onMouseMoveVertSlider);
    $(document.body).unbind('selectstart', onSelectStart);
    return false;
  }

  function dragVertSlider (evt) {
    offsetMeseditorContainer = document.body.offsetHeight - evt.pageY - meseditorContainer.offsetHeight;
    //offsetMeseditorContainer = document.body.offsetHeight - evt.pageY - $meseditorContainer.height();
    $meseditorContainer.addClass('blocked');

    $(document).bind('dragstart', onDragStart);
    $(document).bind('mouseup', onMouseUpVertSlider);
    $(document).bind('mousemove', onMouseMoveVertSlider);
    $(document.body).bind('selectstart', onSelectStart);
    return false;
  }
//-------------------------------------------------------------------------------------------
  function onMouseMoveHorSlider(evt) {
    var
      mainContainerWidth = $mainContainer.width(),
      newSidebarWidth = evt.pageX ;

    if (newSidebarWidth * 2 > mainContainerWidth) { // if more 50%
      newSidebarWidth = Math.floor(mainContainerWidth / 2);
    }
    if (newSidebarWidth < minSidebarContainerWidth) {
      newSidebarWidth = minSidebarContainerWidth;
    }
    $sidebarContainer.width(newSidebarWidth);
    $horSlider.css('left', newSidebarWidth + 'px');
    $contentContainer.width(mainContainerWidth - newSidebarWidth - ccWidthOffset);
    return false;
  }

  function onMouseUpHorSlider (evt) {
    ASC.TMTalk.properties.item(constants.propertySidebarWidth, sidebarContainer.offsetWidth, true);
    //ASC.TMTalk.properties.item(constants.propertySidebarWidth, $sidebarContainer.width(), true);
    $meseditorContainer.removeClass('blocked');
    $window.focus();

    $(document).unbind('dragstart', onDragStart);
    $(document).unbind('mouseup', onMouseUpHorSlider);
    $(document).unbind('mousemove', onMouseMoveHorSlider);
    $(document.body).unbind('selectstart', onSelectStart);
    return false;
  }

  function dragHorSlider (evt) {
    offsetSidebarContainer = document.body.offsetWidth - evt.pageX - sidebarContainer.offsetWidth;
    //offsetSidebarContainer = document.body.offsetWidth - evt.pageX - $sidebarContainer.width();
    $meseditorContainer.addClass('blocked');

    $(document).bind('dragstart', onDragStart);
    $(document).bind('mouseup', onMouseUpHorSlider);
    $(document).bind('mousemove', onMouseMoveHorSlider);
    $(document.body).bind('selectstart', onSelectStart);
    return false;
  }
//-------------------------------------------------------------------------------------------
  $(window).keyup(TMTalk.keyup).blur(TMTalk.blur).focus(TMTalk.focus);
  $(document).click(TMTalk.click);

  $(function () {
    //$(document.body).addClass('focused');

    if (ASC.TMTalk.flashPlayer.isCorrect) {
        var o = document.createElement('div'),
            soundsContainerId = 'talkSoundsContainer-' + Math.floor(Math.random() * 1000000);

        o.setAttribute('id', soundsContainerId);
        document.body.appendChild(o);
        swfobject.embedSWF(
            ASC.TMTalk.properties.item('sounds') ? ASC.TMTalk.properties.item('sounds') : ASC.TMTalk.Config.sounds,
            soundsContainerId,
            1,
            1,
            '9.0.0',
            ASC.TMTalk.properties.item('expressInstall'),
            { apiInit: function(id) { ASC.TMTalk.sounds.init(id); }, apiId: soundsContainerId },
            { allowScriptAccess: 'always', wmode: 'transparent' },
            { styleclass: 'soundsContainer', wmode: 'transparent' }
        );
        ASC.TMTalk.sounds.init(soundsContainerId);
    } else {
        ASC.TMTalk.sounds.initHtml();
    }

    $('#talkWrapper').show();
    $window = $(window);
    horSlider = document.getElementById('talkHorSlider'); $horSlider = $(horSlider);
    vertSlider = document.getElementById('talkVertSlider'); $vertSlider = $(vertSlider);
    startSplash = document.getElementById('talkStartSplash'); $startSplash = $(startSplash);
    mainContainer = document.getElementById('talkMainContainer'); $mainContainer = $(mainContainer);
    sidebarContainer = document.getElementById('talkSidebarContainer'); $sidebarContainer = $(sidebarContainer);
    contentContainer = document.getElementById('talkContentContainer'); $contentContainer = $(contentContainer);
    meseditorContainer = document.getElementById('talkMeseditorContainer'); $meseditorContainer = $(meseditorContainer);
    roomsContainer = document.getElementById('talkRoomsContainer'); $roomsContainer = $(roomsContainer);
    contactsContainer = document.getElementById('talkContactsContainer'); $contactsContainer = $(contactsContainer);
    contactToolbarContainer = document.getElementById('talkContactToolbarContainer'); $contactToolbarContainer = $(contactToolbarContainer);
    meseditorToolbarContainer = document.getElementById('talkMeseditorToolbarContainer'); $meseditorToolbarContainer = $(meseditorToolbarContainer);

    mcHeightOffset = $('#talkTabContainer').height() + 10 * 2 + 10 * 2 + 1;
    ccWidthOffset = $horSlider.width();
    ccHeightOffset = $('#talkStatusContainer').outerHeight(true) + 1 * 2;
    rcHeightOffset = parseInt($meseditorContainer.css('bottom')) + $vertSlider.height() + 1 * 2 + 1;
    vsBottomOffset = $meseditorToolbarContainer.outerHeight(true) + 1;
    ssHeightOffset = $meseditorToolbarContainer.outerHeight(true) + 10 + 1;

    var windowHeight = $window.height();
    if (windowHeight < minPageHeight) {
      windowHeight = minPageHeight;
    }

    if ($.browser.msie && $.browser.version < 9) {
      $(document.body).add('#studioPageContent').css('backgroundColor', '#0A4462');
      $startSplash.find('div.background').css('backgroundColor', '#4A8CAC');
    }

    var mainContainerHeight = windowHeight - mcHeightOffset;
    mainContainer.style.height = mainContainerHeight + 'px';
    //$mainContainer.height(windowHeight - mcHeightOffset);
    contactsContainer.style.height = mainContainerHeight;
    //$contactsContainer.height($mainContainer.height() - $contactToolbarContainer.height() - ccHeightOffset);

    var
      contentContainerHeight = contentContainer.offsetHeight,
      //contentContainerHeight = $contentContainer.height(),
      meseditorHeight = ASC.TMTalk.properties.item(constants.propertyMeseditorHeight);
    meseditorHeight = isFinite(+meseditorHeight) ? +meseditorHeight : minMeseditorContainer;
    if (meseditorHeight * 3 > contentContainerHeight) { // if more 33%
      meseditorHeight = Math.floor(contentContainerHeight / 3);
    }
    if (meseditorHeight < minMeseditorContainer) {
      meseditorHeight = minMeseditorContainer;
    }
    meseditorContainer.style.height = meseditorHeight + 'px';
      //$meseditorContainer.height(meseditorHeight);
    vertSlider.style.bottom = (meseditorHeight - 5) + 'px';
    //$vertSlider.css('bottom', meseditorHeight + vsBottomOffset + 'px');

    var roomsContainerHeight = contentContainerHeight - roomsContainer.offsetTop - ($roomsContainer.hasClass('history') ? 2 : meseditorContainer.offsetHeight + rcHeightOffset);
    if (roomsContainerHeight < 0) {
      roomsContainerHeight = -roomsContainerHeight;
    }
    roomsContainer.style.height = roomsContainerHeight-2 + 'px';
    //$roomsContainer.height(contentContainerHeight - parseInt($roomsContainer.css('top')) - ($roomsContainer.hasClass('history') ? 2 : $meseditorContainer.height() + rcHeightOffset));

    var
      mainContainerWidth = $mainContainer.width(),
      sidebarWidth = ASC.TMTalk.properties.item(constants.propertySidebarWidth);
    sidebarWidth = isFinite(+sidebarWidth) ? +sidebarWidth : minSidebarContainerWidth;
    if (sidebarWidth * 2 > mainContainerWidth) { // if more 50%
      sidebarWidth = Math.floor(mainContainerWidth / 2);
    }
    if (sidebarWidth < minSidebarContainerWidth) {
      sidebarWidth = minSidebarContainerWidth;
    }
    sidebarContainer.style.width = sidebarWidth + 'px';
    //$sidebarContainer.width(sidebarWidth);
    horSlider.style.left = sidebarWidth + 'px';
    //$horSlider.css('right', sidebarWidth + 'px');
    contentContainer.style.width = mainContainerWidth - sidebarWidth - ccWidthOffset + 'px';
    //$contentContainer.width(mainContainerWidth - sidebarWidth - ccWidthOffset);

    startSplash.style.height = contentContainer.offsetHeight - ssHeightOffset + 'px';
    //$startSplash.height($contentContainer.height() - ssHeightOffset);

    if (ASC.TMTalk.properties.item('hidscd') !== '1') {
      try {
        google.gears.factory.create('beta.desktop').createShortcut(
          ASC.TMTalk.Resources.ProductName,
          location.href,
            {
                '16x16': ASC.TMTalk.Icons.addonIcon16,
                '32x32': ASC.TMTalk.Icons.addonIcon32,
                '48x48': ASC.TMTalk.Icons.addonIcon48,
                '128x128': ASC.TMTalk.Icons.addonIcon128
            },
          ASC.TMTalk.Resources.HintCreateShortcutDialog
        );
      } catch (err) {}
    }
    ASC.TMTalk.properties.item('hidscd', '1', true);

    if ($.browser.safari) {
      $('#talkStartSplash').mousedown(function () {
        if (window.getSelection) {
          window.getSelection().removeAllRanges();
        }
        return false;
      });
    }

    //if (ASC.TMTalk.properties.item('hidnd') !== '1') {
    //  if (ASC.TMTalk.notifications.supported.current === true) {
    //    if (ASC.TMTalk.notifications.enabled() === true) {
    //      $('#cbxToggleNotifications').removeClass('disabled');
    //    } else {
    //      $('#cbxToggleNotifications').addClass('disabled');
    //    }
    //    TMTalk.showDialog('browser-notifications');
    //  }
    //}

    //switch (ASC.TMTalk.properties.item('hidnd')) {
    //  case '0' :
    //    $('#cbxToggleNotificationsDialog').attr('checked', false);
    //    break;
    //  case '1' :
    //    $('#cbxToggleNotificationsDialog').attr('checked', true);
    //    break;
    //  default :
    //    $('#cbxToggleNotificationsDialog').attr('checked', true);
    //    ASC.TMTalk.properties.item('hidnd', '1', true);
    //    break;
    //}

    $(document).keydown($.support.touch ? null : function (evt) {
      // shift + tab
      if (evt.shiftKey === true && evt.keyCode === 9) {
        // TODO :
        ASC.TMTalk.tabsContainer.nextTab();
        return false;
      }
      // esc
      if (evt.keyCode === 27) {
        // TODO :
        TMTalk.hideAllDialogs();
        return false;
      }
      // shift + F
      if (evt.shiftKey === true && evt.keyCode === 70) {
         // TODO :
        if (ASC.TMTalk.properties.item('enblfltr') === '0') {
          ASC.TMTalk.properties.item('enblfltr', '1');
        }
        return false;
      }
    });

    $(window)
      .keypress(function (evt) {
        if (evt.ctrlKey && evt.charCode === 119 && ASC.TMTalk.connectionManager.connected()) {
          ASC.TMTalk.connectionManager.terminate();
        }
      })
      .on("beforeunload", ASC.TMTalk.connectionManager.terminate)
      .resize(function () {
        var windowHeight = $window.height();
        if (windowHeight < minPageHeight) {
          windowHeight = minPageHeight;
        }

        var
          containerHeight = windowHeight - mcHeightOffset,
          contactsContainerHeight = containerHeight,
          roomsContainerHeight = containerHeight - roomsContainer.offsetTop - ($roomsContainer.hasClass('history') ? 2 : meseditorContainer.offsetHeight + rcHeightOffset) + 
                                ($roomsContainer.hasClass('multichat') ? 32 : 0);

        if (containerHeight < 0) {
          containerHeight = -containerHeight;
        }
        if (contactsContainerHeight < 0) {
          contactsContainerHeight = -contactsContainerHeight;
        }
        if (roomsContainerHeight < 0) {
          roomsContainerHeight = -roomsContainerHeight;
        }
        containerHeight = containerHeight +40;
        mainContainer.style.height = containerHeight + 'px';
        //$mainContainer.height(containerHeight);
        contactsContainer.style.height = contactsContainerHeight + 'px';
          //$contactsContainer.height(containerHeight - $contactToolbarContainer.height() - ccHeightOffset);
        roomsContainer.style.height = roomsContainerHeight + 18  + 'px';
        if ($roomsContainer.hasClass('history')) {
            //jq("#cha").tlCombobox();
            roomsContainer.style.height = roomsContainerHeight -71 + 'px';
        }
          
        //$roomsContainer.height(containerHeight - parseInt($roomsContainer.css('top')) - ($roomsContainer.hasClass('history') ? 2 : $meseditorContainer.height() + rcHeightOffset));

        var
          sidebarWidth = sidebarContainer.offsetWidth,
          mainContainerWidth = mainContainer.offsetWidth;
          //sidebarWidth = $sidebarContainer.width(),
          //mainContainerWidth = $mainContainer.width();

        sidebarWidth = isFinite(+sidebarWidth) ? +sidebarWidth : minSidebarContainerWidth;
        if (sidebarWidth * 2 > mainContainerWidth) { // if more 50%
          sidebarWidth = Math.floor(mainContainerWidth / 2);
        }
        if (sidebarWidth < minSidebarContainerWidth) {
          sidebarWidth = minSidebarContainerWidth;
        }
        sidebarContainer.style.width = sidebarWidth + 'px';
        //$sidebarContainer.width(sidebarWidth);
        horSlider.style.left = sidebarWidth + 'px';
        //$horSlider.css('right', sidebarWidth + 'px');
        contentContainer.style.width = mainContainerWidth - sidebarWidth - ccWidthOffset + 'px';
        //$contentContainer.width(mainContainerWidth - sidebarWidth - ccWidthOffset);

        startSplash.style.height = contentContainer.offsetHeight - ssHeightOffset + 'px';
        //$startSplash.height($contentContainer.height() - ssHeightOffset);

        var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
        if (currentRoomData !== null && currentRoomData.type === 'conference' && currentRoomData.minimized === false) {
          var nodes = ASC.TMTalk.dom.getElementsByClassName(roomsContainer, 'room conference current', 'li');
          if (nodes.length > 0) {
            var roomHeight = nodes[0].offsetHeight;
            nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'sub-panel', 'div');
            if (nodes.length > 0) {
              nodes[0].style.height = Math.ceil(roomHeight / 2) - 90 + 'px';
            }
          }
        }
        if (currentRoomData !== null && currentRoomData.type === 'mailing' && currentRoomData.minimized === false) {
            var nodes = ASC.TMTalk.dom.getElementsByClassName(roomsContainer, 'room mailing current', 'li');
            if (nodes.length > 0) {
                var roomHeight = nodes[0].offsetHeight;
                nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'sub-panel', 'div');
                if (nodes.length > 0) {
                    nodes[0].style.height = Math.ceil(roomHeight / 2) - 90 + 'px';
                }
            }
        }
        if ((($('#talkContentContainer').width() + $('#talkSidebarContainer').width()) > $('#talkMainContainer').width())) {
            $(window).resize();
        }
      });

    $('#talkHorSlider').mousedown(function (evt) {
      return dragHorSlider(evt);
    });

    $('#talkVertSlider').mousedown(function (evt) {
      return dragVertSlider(evt);
    });

    $('#talkDialogsContainer')
      .keydown(function (evt) {
        evt.originalEvent.stopPropagation ? evt.originalEvent.stopPropagation() : evt.originalEvent.cancelBubble = true;
      })
      .keypress(function (evt) {
        switch (evt.keyCode) {
          case 13 :
              $('#talkDialogsContainer').find('div.dialog:visible:first div.toolbar:first div.button-talk:first').click();
            return false;
          case 27 :
            TMTalk.hideDialog();
            break;
          default :
            if (evt.target.tagName.toLowerCase() === 'input') {
              $(evt.target).parents('div.textfield:first').removeClass('invalid-field');
            }
        }
      })
      .click(function (evt) {
        var $target = $(evt.target);
        if ($target.hasClass('button-talk') && $target.hasClass('close-dialog')) {
          TMTalk.hideDialog();
        }
      });

    //$('#cbxToggleNotifications').click(function (evt) {
    //  var $target = $(evt.target);
      //  if ($target.hasClass('button-talk')) {
    //    var $this = $(this);
    //    if ($this.toggleClass('disabled').hasClass('disabled')) {
    //      $this.addClass('disabled');
    //      ASC.TMTalk.notifications.disable();
    //    } else {
    //      $this.addClass('disabled');
    //      ASC.TMTalk.notifications.enable(function (val) {
    //        if (val === true) {
    //          $('#cbxToggleNotifications').removeClass('disabled');
    //        } else {
    //          $('#cbxToggleNotifications').addClass('disabled');
    //        }
    //      });
    //    }
    //  }
    //});

    //$('#cbxToggleNotificationsDialog').click(function (evt) {
    //  ASC.TMTalk.properties.item('hidnd', $(this).is(':checked') ? '1' : '0', true);
      //});
    //hack for resizing
    jq(window).resize();
  });
})(jQuery);
