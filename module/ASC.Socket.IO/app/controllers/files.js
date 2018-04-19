module.exports = (files) => {
    const router = require('express').Router();

    router
        .post("/changeEditors", (req, res) => {
            files.changeEditors(req.body);
            res.end();
        });

    return router;
};