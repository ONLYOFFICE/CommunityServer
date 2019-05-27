module.exports = (counters, chat, voip, files) => {
    const router = require('express').Router(),
        bodyParser = require('body-parser'),
        authService = require('../middleware/authService.js')();

    router.use(bodyParser.json());
    router.use(bodyParser.urlencoded({ extended: false }));
    router.use(require('cookie-parser')());
    router.use((req, res, next) => {
        if (!authService(req)) {
            res.sendStatus(403);
            return;
        }
    
        next();
    });

    router
        .use("/counters", require(`./counters.js`)(counters))
        .use("/mail", require(`./mail.js`)(counters))
        .use("/chat", require(`./chat.js`)(chat))
        .use("/voip", require(`./voip.js`)(voip))
        .use("/files", require(`./files.js`)(files));

    return router;
}