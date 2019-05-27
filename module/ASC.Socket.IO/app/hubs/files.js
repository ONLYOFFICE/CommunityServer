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