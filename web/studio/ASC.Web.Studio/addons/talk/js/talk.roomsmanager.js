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

  return {
    init  : init,

    bind    : bind,
    unbind  : unbind,

    events  : customEvents,

    createRoom  : createRoom,
    openRoom    : openRoom,
    closeRoom   : closeRoom,

    getCurrentRoomId  : getCurrentRoomId,

    getRoomData     : getRoomData,
    updateRoomData  : updateRoomData,
    getRoomDataById : getRoomDataById
  };
})();
