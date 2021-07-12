/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


"use strict";

process.env.NODE_ENV = process.env.NODE_ENV || "development";

const fs = require("fs"),
    http = require("http"),
    express = require("express"),
    morgan = require("morgan"),
    cookieParser = require("cookie-parser"),
    bodyParser = require("body-parser"),
    session = require("express-session"),
    winston = require("winston"),
    config = require("./config").get(),
    path = require("path"),
    exphbs = require("express-handlebars"),
    favicon = require("serve-favicon"),
    cors = require("cors");

require('winston-daily-rotate-file');

const app = express();
let logDir = config["logPath"] ? config["logPath"] : config.app.logDir;
// ensure log directory exists
fs.existsSync(logDir) || fs.mkdirSync(logDir);

let transports = [];

if (config.logger.file) {
    logDir = config["logPath"] ? logDir : (config.app.logDir[0] === "." ? path.join(__dirname, config.app.logDir) : config.app.logDir);
    config.logger.file.filename = path.join(logDir, config.app.logName);
    transports.push(new (winston.transports.DailyRotateFile)(config.logger.file));
}

if (config.logger.console) {
    transports.push(new (winston.transports.Console)(config.logger.console));
}

let logger = winston.createLogger({
    transports: transports,
    exitOnError: false
});

logger.stream = {
    write: function(message) {
        logger.info(message.trim());
    }
};

// view engine setup
app.set("views", path.join(__dirname, "views"));
app.engine("handlebars", exphbs({ defaultLayout: "main" }));
app.set("view engine", "handlebars");

app.use(favicon(path.join(__dirname, "public", "favicon.ico")))
    .use(morgan("combined", { "stream": logger.stream }))
    .use(cookieParser())
    .use(bodyParser.json())
    .use(bodyParser.urlencoded({ extended: false }))
    .use(session(
    {
        resave: true,
        saveUninitialized: true,
        secret: config["core.machinekey"] ? config["core.machinekey"] : config.app.machinekey
        }))
    .use(cors());

require("./app/middleware/saml")(app, config, logger);
require("./app/routes")(app, config, logger);

const httpServer = http.createServer(app);

httpServer.listen(config.app.port,
    function () {
        logger.info(`Start SSO Service Provider listening on port ${config.app.port} for http`);
    });