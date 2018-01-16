module.exports = (counters) => {
    const router = require('express').Router();

    router
        .post("/updateFolders", (req, res) => {
            counters.updateFolders(req.body);
            res.end();
        })
        .post("/sendMailNotification", (req, res) => {
            counters.sendMailNotification(req.body);
            res.end();
        });

    return router;
}