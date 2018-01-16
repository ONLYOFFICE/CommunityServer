module.exports = (chat) => {
    const router = require('express').Router();

    router
        .post("/send", (req, res) => {
            chat.send(req.body);
            res.end();
        })
        .post("/sendInvite", (req, res) => {
            chat.sendInvite(req.body);
            res.end();
        })
        .post("/setState", (req, res) => {
            chat.setState(req.body);
            res.end();
        })
        .post("/sendOfflineMessages", (req, res) => {
            chat.sendOfflineMessages(req.body);
            res.end();
        });

    return router;
}