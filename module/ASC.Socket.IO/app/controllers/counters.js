module.exports = (counters) => {
    const router = require('express').Router();

    router
        .post("/sendUnreadUsers", (req, res) => {
            counters.sendUnreadUsers(req.body);
            res.end();
        })
        .post("/sendUnreadCounts", (req, res) => {
            counters.sendUnreadCounts(req.body);
            res.end();
        });

    return router;
}