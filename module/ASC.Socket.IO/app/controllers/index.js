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