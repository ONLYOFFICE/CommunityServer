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


module.exports = (io) => {
    const apiRequestManager = require('../apiRequestManager.js');
    const co = require('co');
    const counters = io.of('/counters');
    const onlineUsers = [];
    const uaParser = require('ua-parser-js');
    var timeInterval;

    counters.on('connection', (socket) => {
        const request = socket.client.request;
        if(!request.user || !request.user.id) return;
        const userId = request.user.id;
        const tenantId = request.portal.tenantId;
        let ipAddress = socket.handshake.headers['x-forwarded-for'];
        const userAgent = socket.request.headers['user-agent'];
        const parser = new uaParser();
        parser.setUA(userAgent);
        
        const [os, browser] = [parser.getOS(), parser.getBrowser()];
        const operationSystem = os.version !== undefined ?  `${os.name} ${os.version}` : `${os.name}`;  
        const browserVersion = browser.version ? browser.version : '';

        ipAddress = getCleanIP(ipAddress);
        const browserName = `${browser.name} ${browserVersion}`;
        const userName = (request.user.userName || "").toLowerCase();
        getCityByIP(ipAddress);

        socket.join([tenantId, `${tenantId}-${userId}`, `${tenantId}-${userName}`]);

        getNewMessagesCount();
        
        socket
            .on('disconnect', () => {
                if (!onlineUsers[tenantId]) return;
                if (!onlineUsers[tenantId][userId]) return;
                if (!onlineUsers[tenantId][userId].browsers) return;
                if (!onlineUsers[tenantId][userId].browsers[browserName]) return;

                onlineUsers[tenantId][userId].browsers[browserName].counter--;
                if (onlineUsers[tenantId][userId].browsers[browserName].counter === 0) {
                    delete onlineUsers[tenantId][userId].browsers[browserName];
                }
                if (Object.keys(onlineUsers[tenantId][userId].browsers).length === 0) {
                    
                    timeInterval = setTimeout(function(){
                        delete onlineUsers[tenantId][userId];
                        if(typeof onlineUsers[tenantId][userId] != "undefined") return;
            
                        counters.to(tenantId).emit('renderOfflineUser', userId);
                        updateMailUserActivity(socket.client.request, false);
                        timeInterval = undefined;
                        console.log(`a user ${userName} in portal ${tenantId} disconnected`);
                    }, 3000);
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
            if(!request.mailEnabled) return;
            if((!userOnline && typeof onlineUsers[tenantId][userId] != "undefined") || 
            (userOnline && !onlineUsers[tenantId][userId])) return;

            apiRequestManager.put("mail/accounts/updateuseractivity.json", request, { userOnline });
            console.log(`updateuseractivity ${userOnline}`);
        }

        function getNewMessagesCount() {
            co(function* () {
                let mailMessageFolders = [], feedCount = 0, messageCount = 0, mailCount = 0;
                var batchRequest = apiRequestManager.batchFactory()
                    .get("feed/newfeedscount.json")
                    .get("portal/talk/unreadmessages.json");

                if(!request.mailEnabled){
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
            if(!request.mailEnabled) return new Promise((resolve) => { resolve(0)});
            return apiRequestManager.get("mail/folders.json", request).then(getInreadMessageCount);
        }

        function getCityByIP (ip){
            apiRequestManager.get("portal/ip/" + ip, request, false)
                    .then((result) => {
                        const city = result.city;
                        console.log(`a user ${userName} in portal ${tenantId} connected, IP ${ipAddress}, city ${city}, OS ${operationSystem}, browser ${browserName}`);
                        console.log("-----");
                        if (!onlineUsers[tenantId]) {
                            onlineUsers[tenantId] = {};
                        }
                        if(timeInterval == undefined){
                            if (!onlineUsers[tenantId][userId]) {
                                onlineUsers[tenantId][userId] = {browsers: {},FirstConnection: new Date(), LastConnection: new Date() };
                                socket.broadcast.to(tenantId).emit('renderOnlineUser', userId);
                                updateMailUserActivity(socket.client.request);
                            }
                            else {
                                onlineUsers[tenantId][userId].LastConnection = new Date();
                            }
                        }else{
                            clearTimeout(timeInterval);
                            timeInterval = undefined;
                        }
                    
                        if (!onlineUsers[tenantId][userId].browsers[browserName]) {
                            onlineUsers[tenantId][userId].browsers[browserName] = {counter:1, ipAddress: ipAddress, city: city, operationSystem: operationSystem };
                        } else {
                            onlineUsers[tenantId][userId].browsers[browserName].counter++;
                        } 
                            })
                    .catch((err) => {
                        console.log(err);
                    });
        };

        function getCleanIP (ipAddress) {
			if(typeof(ipAddress) == "undefined"){
				return "127.0.0.1";
			}
            const indexOfColon = ipAddress.indexOf(':');
            if (indexOfColon === -1){
                return ipAddress;
            } else if (indexOfColon > 3){
                return ipAddress.substring(0, indexOfColon);
            }
            else {
                return "127.0.0.1";
            }
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