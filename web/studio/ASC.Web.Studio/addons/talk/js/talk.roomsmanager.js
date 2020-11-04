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

window.ASC.TMTalk.roomsManager = (function () {
  var
    isInit = false,
    currentRoomId = '',
    roomIds = {},
    customEvents = {
      // creating the room
      createRoom : 'oncreateroom',
      // open the room
      openRoom : 'onopenroom',
      // close the room
      closeRoom : 'oncloseroom'
    },
    eventManager = new CustomEvent(customEvents);

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

  var init = function () {
    if (isInit === true) {
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

  var createRoom = function (param) {
    var roomId = getUniqueId(roomIds, 'room');
    param.roomId = roomId;
    roomIds[roomId] = {
      data : param
    };
    eventManager.call(customEvents.createRoom, window, [roomId, roomIds[roomId].data]);
    return roomId;
  };

  var openRoom = function (roomId, inBackground) {
    if (currentRoomId !== roomId && roomIds.hasOwnProperty(roomId)) {
      if (inBackground !== true) {
        currentRoomId = roomId;
      }
      eventManager.call(customEvents.openRoom, window, [roomId, roomIds[roomId].data, inBackground === true]);
    }
  };

  var closeRoom = function (roomId) {
    if (roomIds.hasOwnProperty(roomId)) {
      var
        isCurrent = false,
        data = roomIds[roomId].data;

      if (currentRoomId === roomId) {
        isCurrent = true;
        currentRoomId = '';
      }

      delete roomIds[roomId];
      eventManager.call(customEvents.closeRoom, window, [roomId, data, isCurrent]);
    }
  };

  var getCurrentRoomId = function () {
    return currentRoomId;
  };

  var getRoomData = function (roomId) {
    roomId = typeof roomId === 'undefined' ? currentRoomId : roomId;
    return roomIds.hasOwnProperty(roomId) ? roomIds[roomId].data : null;
  };

  var updateRoomData = function (roomId, data) {
    roomId = typeof roomId === 'undefined' ? currentRoomId : roomId;
    if (roomIds.hasOwnProperty(roomId)) {
      for (var fld in data) {
        if (data.hasOwnProperty(fld)) {
          roomIds[roomId].data[fld] = data[fld];
        }
      }
    }
  };

  var getRoomDataById = function (id) {
    var fld = '', data = null;
    for (fld in roomIds) {
      if (roomIds.hasOwnProperty(fld)) {
        data = roomIds[fld].data;
        if (data.hasOwnProperty('id') && data.id === id) {
          return data;
        }
      }
    }
    return null;
  };
    var focusHistorySearch = function () {
        
    };
    var blurHistorySearch = function () {
    };
    var clearSearch = function (room) {
        var messages = null;
        if (ASC.TMTalk.dom.hasClass(room, 'room')) {
            var searchInput = ASC.TMTalk.dom.getElementsByClassName(room, 'search-value', 'input');
            if (searchInput.length > 0) {
                jQuery(searchInput[0]).val('');
            }

            nodes = ASC.TMTalk.dom.getElementsByClassName(room, 'messages', 'ul');
            if (nodes.length > 0) {
                messages = nodes[0];
            }
            ASC.TMTalk.roomsContainer.resetHistorySearch(room, ASC.TMTalk.dom.getElementsByClassName(messages, 'message', 'li'));
            ASC.TMTalk.dom.removeClass(room.parentNode.parentNode, 'searchmessage');

            jQuery('div#talkRoomsContainer ul.rooms li.room.chat div.filtering-panel div.filtering-panel-tools').removeClass('show');
            jQuery('div#talkRoomsContainer ul.rooms li.room.chat div.filtering-panel div.filtering-container').removeClass('show_tools');

            if (ASC.TMTalk.Config.fullText) {
                var jid = ASC.TMTalk.roomsManager.getRoomData().id;
                ASC.TMTalk.messagesManager.clearCurrentHistory(jid);
                ASC.TMTalk.messagesManager.getHistory(jid);
            }
        }
    };
    var find = null;
    var keyupHistorySearch = function () {

        var filteringPanel,
            filteringPanelTools,
            filteringContainer,
            input,
            clearSearchButton,

            room;

        room = jQuery('div#talkRoomsContainer ul.rooms li.room.current')[0];
        
        filteringPanel = jQuery('div#talkRoomsContainer ul.rooms li.room.current div.filtering-panel:first');
        input = jQuery('div#talkRoomsContainer ul.rooms li.room.current div.filtering-panel div.filtering-container div.textfield input:first')[0];
        clearSearchButton = jQuery('div#talkRoomsContainer ul.rooms li.room.current div.filtering-panel div.clear-search');

        filteringContainer = filteringPanel.find('div.filtering-container');
        filteringPanelTools = filteringPanel.find('div.filtering-panel-tools');
        
        var currentRoomData = ASC.TMTalk.roomsManager.getRoomData(),
            jid = currentRoomData.id,
            messageCount = 0;
        
        
        
        if (filteringPanel.hasClass('found')) {
            filteringPanel.addClass('input-text');
        }
        clearTimeout(find);
        find = setTimeout(function () {
            if (input.value) {
                filteringPanelTools.addClass('show');
                filteringContainer.addClass('show_tools');

                if (ASC.TMTalk.dom.hasClass(room, 'room')) {
                    var searchstr = input.value;
                    if (searchstr) {
                        if (ASC.TMTalk.Config.fullText) {
                            ASC.TMTalk.connectionManager.searchMessage(jid, input.value);
                            return;
                        }
                        var nodes = null,
                            messagescontainer = null;
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
                            if (ASC.TMTalk.dom.hasClass(room, 'chat')) {
                                if (!currentRoomData.fullhistory && messageCount > 20) {
                                    ASC.TMTalk.roomsContainer.getFilteringHistory(jid, 3);
                                    currentRoomData.fullhistory = true;
                                } else {
                                    ASC.TMTalk.roomsContainer.historySearch(searchstr, room, ASC.TMTalk.dom.getElementsByClassName(messagescontainer, 'message', 'li'));
                                }
                            } else {
                                ASC.TMTalk.roomsContainer.historySearch(searchstr, room, ASC.TMTalk.dom.getElementsByClassName(messagescontainer, 'message', 'li'));
                            }
                        }
                    }
                }
            } else {
                if (ASC.TMTalk.Config.fullText) {
                    ASC.TMTalk.messagesManager.updateHistory(jid, 0, 20, "");
                } else {
                    ASC.TMTalk.roomsManager.clearSearch(room);
                }
            }
            filteringPanel.removeClass('input-text');
            
        }, 1000);
    };
    var keydownHistorySearch = function () {
        
    };
    return {
    init  : init,

    bind    : bind,
    unbind  : unbind,

    events  : customEvents,

    createRoom  : createRoom,
    openRoom    : openRoom,
    closeRoom   : closeRoom,
    focusHistorySearch      : focusHistorySearch,
    blurHistorySearch       : blurHistorySearch,
    keyupHistorySearch      : keyupHistorySearch,
    keydownHistorySearch    : keydownHistorySearch,
    getCurrentRoomId: getCurrentRoomId,
    
    clearSearch: clearSearch,

    getRoomData     : getRoomData,
    updateRoomData  : updateRoomData,
    getRoomDataById : getRoomDataById
  };
})();
