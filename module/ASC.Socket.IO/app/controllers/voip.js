module.exports = (voip) => {
    const router = require('express').Router();

    router
        .post("/enqueue", (req, res) => {
            const { numberId, callId, agent } = req.body;
            voip.enqueue(numberId, callId, agent);
            res.end();
        })
        .post("/incoming", (req, res) => {
            const { callId, agent } = req.body;
            voip.incoming(callId, agent);
            res.end();
        })
        .post("/miss", (req, res) => {
            const { numberId, callId, agent } = req.body;
            voip.miss(numberId, callId, agent);
            res.end();
        })
        .post("/getAgent", (req, res) => {
            const { numberId, contactsResponsibles } = req.body;
            const result = voip.getAgent(numberId, contactsResponsibles);
            res.send(JSON.stringify({item1: result.result, item2: result.isAnyNotOffline}));
        })
        .post("/reload", (req, res) => {
            const { numberRoom, agentId } = req.body;
            voip.reload(numberRoom, agentId);
            res.end();
        });

    return router;
}