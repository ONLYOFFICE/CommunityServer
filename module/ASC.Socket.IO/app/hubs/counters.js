module.exports = (io) => {
    const apiRequestManager = require('../apiRequestManager.js');
    const co = require('co');
    const counters = io.of('/counters');
    const onlineUsers = [];

    counters.on('connection', (socket) => {
        const request = socket.client.request;
        if(!request.user || !request.user.id) return;
        const userId = request.user.id;
        const tenantId = request.portal.tenantId;
        const userName = (request.user.userName || "").toLowerCase();

        console.log(`a user ${userName} in portal ${tenantId} connected`);

        socket.join([tenantId, `${tenantId}-${userId}`, `${tenantId}-${userName}`]);

        if (!onlineUsers[tenantId]) {
            onlineUsers[tenantId] = {};
        }
        if (!onlineUsers[tenantId][userId]) {
            onlineUsers[tenantId][userId] = { counter: 1, FirstConnection: new Date(), LastConnection: new Date() };
            socket.broadcast.to(tenantId).emit('renderOnlineUser', userId);
            updateMailUserActivity(socket.client.request);
        } else {
            onlineUsers[tenantId][userId].counter++;
            onlineUsers[tenantId][userId].LastConnection = new Date();
        }

        getNewMessagesCount();

        socket
            .on('disconnect', () => {
                if (!onlineUsers[tenantId]) return;
                if (!onlineUsers[tenantId][userId]) return;

                onlineUsers[tenantId][userId].counter--;
                if (onlineUsers[tenantId][userId].counter === 0) {
                    delete onlineUsers[tenantId][userId];
                    counters.to(tenantId).emit('renderOfflineUser', userId);
                    updateMailUserActivity(socket.client.request, false);
                    console.log(`a user ${userName} in portal ${tenantId} disconnected`);
                }
            })
            .on('renderOnlineUsers', () => {
                counters.to(tenantId).emit('renderOnlineUsers', onlineUsers[tenantId] || []);
            })
            .on('sendMessagesCount', (count) => {
                socket.broadcast.to(`${tenantId}-${userId}`).emit('sendMessagesCount', count);
            })
            .on('sendFeedsCount', () => {
                socket.broadcast.to(`${tenantId}-${userId}`).emit('sendFeedsCount', 0);
            })
            .on('updateFolders', (shouldUpdateMailBox) => {
                counters.in(`${tenantId}-${userId}`).clients((error, clients) => {
                    if (error) return;
                    if (clients.length > 1) {
                        getMessageCount().then((count) => {
                            socket.broadcast.to(`${tenantId}-${userId}`).emit('updateFolders', count, shouldUpdateMailBox);
                        });
                    }
                });
        });

        function updateMailUserActivity(request, userOnline = true) {
            if(request.user.isVisitor) return;

            setTimeout(function(){
                if((!userOnline && typeof onlineUsers[tenantId][userId] != "undefined") || 
                (userOnline && !onlineUsers[tenantId][userId])) return;
    
                apiRequestManager.put("mail/accounts/updateuseractivity.json", request, { userOnline });
                console.log(`updateuseractivity ${userOnline}`);
            }, 3000);
        }

        function getNewMessagesCount() {
            co(function* () {
                let mailMessageFolders = [], feedCount = 0, messageCount = 0, mailCount = 0;
                var batchRequest = apiRequestManager.batchFactory()
                    .get("feed/newfeedscount.json")
                    .get("portal/talk/unreadmessages.json");

                if(request.user.isVisitor){
                    [feedCount, messageCount] = yield apiRequestManager.batch(batchRequest,request);
                }else{
                    [feedCount, messageCount, mailMessageFolders] = yield apiRequestManager.batch(batchRequest.get("mail/folders.json"), request);
                }

                mailCount = getInreadMessageCount(mailMessageFolders);

                counters.to(`${tenantId}-${userId}`).emit('getNewMessagesCount',
                    {
                        me: messageCount,
                        f: feedCount,
                        ma: mailCount
                    });
            })
            .catch((err)=>{
            });
        }

        function getMessageCount() {
            if(request.user.isVisitor) return 0;
            return apiRequestManager.get("mail/folders.json", request).then(getInreadMessageCount);
        }
    });

    function getInreadMessageCount(mailMessageFolders){
        let mailMessageFoldersCount = mailMessageFolders.length;
        while (mailMessageFoldersCount--) {
            const mailMessageFolder = mailMessageFolders[mailMessageFoldersCount];
            if (mailMessageFolder && mailMessageFolder.id === 1) {
                return mailMessageFolder.unread_messages;
            }
        }
        return 0;
    }
    function sendUnreadUsers(unreadUsers) {
        if (!unreadUsers) {
            return;
        }

        for (let tenant in unreadUsers) {
            if (!unreadUsers.hasOwnProperty(tenant)) continue;

            for (let user in unreadUsers[tenant]) {
                if (!unreadUsers[tenant].hasOwnProperty(user)) continue;

                counters.to(`${tenant}-${user}`).emit('sendFeedsCount', unreadUsers[tenant][user]);
            }
        }
    }

    function sendUnreadCounts({ tenantId, unreadCounts } = {}) {
        if (typeof tenantId === "undefined") {
            return;
        }

        for (let user in unreadCounts) {
            if (unreadCounts.hasOwnProperty(user)) {
                counters.to(`${tenantId}-${user}`).emit('sendMessagesCount', unreadCounts[user]);
            }
        }
    }

    function updateFolders({ tenant, userId, count } = {}) {
        if (typeof tenant === "undefined" || !userId || !count) {
            return;
        }

        counters.to(`${tenant}-${userId}`).emit('updateFolders', count);
    }

    function sendMailNotification({ tenant, userId, state } = {}) {
        if (typeof tenant === "undefined" || !userId || typeof state === "undefined") {
            return;
        }

        counters.to(`${tenant}-${userId}`).emit('sendMailNotification', state);
    }

    return { sendUnreadUsers, sendUnreadCounts, updateFolders, sendMailNotification };
}