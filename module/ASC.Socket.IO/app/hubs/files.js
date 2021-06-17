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
    const log = require("../log.js");
    const files = io.of("/files");

    files.on("connection", (socket) => {
        const request = socket.client.request;
        if (!request.user || !request.user.id) {
            return;
        }

        const tenantId = request.portal.tenantId;

        socket
            .on("subscribeChangeEditors", (fileIds) => {
                if (typeof fileIds != "object") {
                    fileIds = [fileIds];
                }

                fileIds.forEach(function(fileId) {
                    let room = `${tenantId}-${fileId}`;

                    socket.join(room);
                });
            });
    });

    function changeEditors({ tenantId, fileId, finish } = {}) {
        if (typeof tenantId === "undefined" || typeof fileId === "undefined") {
            log.error(`files: changeEditors without arguments`);
            return;
        }

        let room = `${tenantId}-${fileId}`;

        files.to(room).emit("changeEditors", fileId);

        if (finish) {
            files.in(room).clients((error, clients) => {
                if (error) throw error;
                clients.forEach(function(client) {
                    let clientSocket = files.connected[client];
                    if(clientSocket){
                        clientSocket.leave(room);
                    }
                });
            });
         }
    }

    return { changeEditors };
};