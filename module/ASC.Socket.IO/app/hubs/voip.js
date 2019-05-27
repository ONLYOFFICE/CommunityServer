module.exports = (io) => {
    const phones = new (require("./voipListPhones.js"))();
    const numberIdKey = "numberId";
    const voip = io.of("/voip");

    voip.on('connection', (socket) => {
        const request = socket.client.request;
        if(!request.user || !request.user.id) return;
        const user = request.user;
        const userId = user.id;
        const numberId = getNumberId(request);
        const numberRoom = request.portal.tenantId + numberId;

        socket.join([numberRoom, userId]);

        phones.addPhone(numberId);

        socket
            .on("status", status)
            .on("miss", miss.bind(null, numberId))
            .on("enqueue", enqueue.bind(null, numberId))
            .on("Dequeue", dequeue)
            .on("OnlineAgents", onlineAgents)
            .on("getStatus", getStatus)
            .on("getAgent", getAgent.bind(null, numberId))
            .on("incoming", incoming)
            .on("start", start)
            .on("end", end);

        function getNumberId(request) {
            let result = request._query[numberIdKey];
            if (!result) {
                result = request.headers[numberIdKey];
            }

            return result || "";
        }

        function status(agentStatus, fn) {
            voip.to(userId).emit("status", agentStatus);

            switch (agentStatus) {
                case 0:
                    socket.once('disconnect', () => {
                        status(2);
                    });
                    if (phones.anyCalls(numberId)) {
                        dequeue();
                    } else {
                        phones.addOrUpdateAgent(numberId, { id: userId, status: agentStatus });
                    }
                    break;
                case 1:
                    phones.addOrUpdateAgent(numberId, { id: userId, status: agentStatus });
                    break;
                case 2:
                    phones.removeAgent(numberId, userId);
                    break;
            }
            onlineAgents();
            if(fn) fn();

        }

        function dequeue() {
            voip.to(userId).emit('dequeue', phones.dequeueCall(numberId));
        }

        function onlineAgents() {
            voip.to(numberRoom).emit('onlineAgents', phones.onlineAgents(numberId));
        }

        function getStatus(fn) {
            const getStatusResult = phones.getStatus(numberId, userId);
            fn(getStatusResult);
        }

        function start() {
            voip.to(userId).emit('start');
        }

        function end() {
            voip.to(userId).emit('end');
        }
    });

    function enqueue(numberId, callId, agent) {
        const result = phones.enqueue(numberId, callId, agent);
        if (result) {
            voip.to(result).emit('dequeue', callId);
        }
    }

    function incoming(callId, agent) {
        voip.to(agent).emit('dequeue', callId);
    }

    function miss(numberId, callId, agent) {
        phones.removeCall(numberId, callId);

        voip.to(agent).emit('miss', callId);
    }

    function getAgent(numberId, contactsResponsibles, fn) {
        const result = phones.getAgent(numberId, contactsResponsibles);
        if (typeof fn === "function") fn(result);
        return result;
    }

    function reload(numberRoom, agent) {
        if (agent) {
            voip.to(agent).emit('reload');
        }else{
            voip.to(numberRoom).emit('reload');
        }
    }

    return { enqueue, incoming, miss, getAgent, reload };
}