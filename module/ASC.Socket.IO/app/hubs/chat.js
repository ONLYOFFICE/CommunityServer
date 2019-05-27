module.exports = (io, countersHub) => {
    const apiRequestManager = require('../apiRequestManager.js');
    const co = require('co');
    const baseTalkApiUrl = 'portal/talk/';
    const chat = io.of("/chat");
    const onlineUsers = [];

    chat.on('connection', (socket) => {
        const request = socket.client.request;
        if(!request.user || !request.user.id) return;
        const user = request.user;
        const portal = request.portal;
        const userId = user.id;
        const userName = user.userName.toLowerCase();
        const tenantId = portal.tenantId;
        const tenantDomain = portal.tenantDomain;

        socket
            .on('disconnect', onDisconnectUser)
            .on('connectUser', onConnectUser)
            .on('disconnectUser', onDisconnectUser)
            .on('send', onSend)
            .on('getStates', onGetStates)
            .on('getContactInfo', onGetContactInfo)
            .on('getInitData', onGetInitData)
            .on('sendTyping', onSendTyping)
            .on('sendStateToTenant', onSendStateToTenant)
            .on('getRecentMessages', onGetRecentMessages)
            .on('chatPing', onPing);

        function onDisconnectUser(change, fn) {
            co(function *() {
                let state;

                if (!onlineUsers[tenantId]) return;
                if (!onlineUsers[tenantId][userId] || !socket.onConnectUser) {
                    if(typeof fn == "function") fn();
                    return;
                }

                if (change === true || onlineUsers[tenantId][userId].counter === 1) {
                    delete onlineUsers[tenantId][userId];
                    state = yield apiRequestManager.dlt(`${baseTalkApiUrl}connection?connectionId=${userId}`, request);
                    socket.emit("disconnectUser");
                } else {
                    onlineUsers[tenantId][userId].counter--;
                    state = yield apiRequestManager.get(`${baseTalkApiUrl}state`, request, { userName });
                    if (state !== 4) {
                        // setStatus
                        socket.broadcast.to(`${tenantId}-${userName}`).emit('setStatus', state);
                    }
                }

                socket.broadcast.to(tenantId).emit('setState', userName, state);
            })
            .catch((err)=>{
                if(typeof fn == "function") fn(err);
            });
        }

        function onConnectUser(state) {
            socket.join([tenantId, `${tenantId}-${userName}`]);
            socket.onConnectUser = true;

            if (!onlineUsers[tenantId]) {
                onlineUsers[tenantId] = {};
            }

            var counter = onlineUsers[tenantId][userId];
            if (!counter) {
                counter = onlineUsers[tenantId][userId] = { counter: 1 };
            } else {
                counter.counter++;
            }

            co(function* () {
                if (counter.counter === 1) {
                    state = yield apiRequestManager.post(`${baseTalkApiUrl}connection`, request, { connectionId: userId, state });
                } else {
                    state = yield apiRequestManager.post(`${baseTalkApiUrl}state`, request, { state });

                    if (state !== 4) {
                        socket.broadcast.to(`${tenantId}-${userName}`).emit('setStatus', state);
                    }
                }
                socket.broadcast.to(tenantId).emit('setState', userName, state, false);
                chat.to(`${tenantId}-${userName}`).emit("connectUser");
            })
            .catch((err)=>{
                chat.to(`${tenantId}-${userName}`).emit("connectUser", err);
            });
        }

        function onSend(calleeUserName, messageText) {
            co(function* () {
                const message = { u: userName, t: messageText };

                if (calleeUserName) {
                    chat.to(`${tenantId}-${calleeUserName}`).emit('send', message, calleeUserName);
                    socket.broadcast.to(`${tenantId}-${userName}`).emit('send', message, calleeUserName);
                }
                yield apiRequestManager.post(`${baseTalkApiUrl}message`, request, { to: calleeUserName, text: messageText });
            });
        }

        function onGetStates(fn) {
            apiRequestManager.get(`${baseTalkApiUrl}states`, request)
                .then((result) => {
                    socket.emit('statesRetrieved', result);
                })
                .catch((err) => {
                    if (typeof fn === "function") fn(err);
                });;
        }

        function onGetContactInfo(calleeUserName, fn) {
            co(function* () {
                const calleeUser = yield apiRequestManager.get('people/' + calleeUserName, request);
                if (!calleeUser || calleeUser.id === "4A515A15-D4D6-4b8e-828E-E0586F18F3A3") throw "Can't get UserInfo";

                const calleeUserState = yield apiRequestManager.get(`${baseTalkApiUrl}state`, request, { userName: calleeUserName });
                if(typeof fn === "function") fn(calleeUserName, calleeUserState);
            })
            .catch((err) => {
                    if (err && typeof fn === "function") fn(null, null, err);
                });
        }

        function onGetInitData(fn) {
            co(function* () {
                const states = yield apiRequestManager.get(`${baseTalkApiUrl}states`, request);
                const users = yield apiRequestManager.get('people', request);
                const result = users
                    .filter((item) => item.id !== userId)
                    .sort((item1, item2) => {
                        if (item1.displayName < item2.displayName) return -1;
                        if (item1.displayName > item2.displayName) return 1;
                        return 0;
                    })
                    .map((item) => {
                    const uName = item.userName.toLowerCase();
                    return { u: uName, d: item.displayName, s: states[uName] || 4}
                });

                socket.emit('initDataRetrieved', userName, user.displayName, result, tenantId, tenantDomain);
            })
            .catch((err) => {
                if (typeof fn === "function") fn(err);
            });
        }

        function onSendTyping(calleeUserName) {
            chat.to(`${tenantId}-${calleeUserName}`).emit('sendTypingSignal', userName);
        }

        function onSendStateToTenant(state) {
            apiRequestManager.post(`${baseTalkApiUrl}state`, request, { state })
                .then((result) => {
                    socket.broadcast.to(tenantId).emit('setState', userName, result, false);
                });
        }

        function onGetRecentMessages(calleeUserName, id, fn) {
            apiRequestManager.get(`${baseTalkApiUrl}recentMessages`, request, { calleeUserName, id })
                .then((recentMessages) => {
                    if (typeof fn === "function") fn(recentMessages);
                })
                .catch((err) => {
                    if (typeof fn === "function") fn(null, err);
                });
        }

        function onPing(state) {
            apiRequestManager.post(`${baseTalkApiUrl}ping`, request, { state: state });
        }
    });

    function tenantPlusUserRoom(tenantId, userName) {
        return chat.to(`${tenantId}-${userName}`);
    }

    function send({ tenantId, callerUserName, calleeUserName, message, isTenantUser } = {}) {
        if (typeof tenantId === "undefined" || !calleeUserName || !message) {
            return;
        }

        tenantPlusUserRoom(tenantId, calleeUserName).emit('send', message, calleeUserName, isTenantUser);

        if (!isTenantUser) {
            tenantPlusUserRoom(tenantId, callerUserName).emit('send', message, calleeUserName, isTenantUser);
        }
    }

    function sendInvite({ tenantId, calleeUserName, message } = {}) {
        if (typeof tenantId === "undefined" || !calleeUserName || !message) {
            return;
        }

        tenantPlusUserRoom(tenantId, calleeUserName).emit('sendInvite', message);
    }

    function setState({ tenantId, from, state } = {}) {
        if (typeof tenantId === "undefined" || !from) {
            return;
        }

        chat.to(`${tenantId}`).emit('setState', from, state);
    }

    function sendOfflineMessages({ tenantId, callerUserName, users } = {}) {
        if (typeof tenantId === "undefined" || !callerUserName || !users) {
            return;
        }

        tenantPlusUserRoom(tenantId, callerUserName).emit('sendOfflineMessages', users);
    }

    return { send, sendInvite, setState, sendOfflineMessages };
}