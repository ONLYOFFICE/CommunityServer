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


const winston = require("winston");
const { format } = require("winston");
require("winston-daily-rotate-file");

const path = require("path");
const config = require("../server/config.js");
const fs = require("fs");
const logLevel = process.env.logLevel || config.logLevel || "info";
const fileName = process.env.logPath || path.join(__dirname, "..", "logs", "web.webdav.%DATE%.log");
const dirName = path.dirname(fileName);

if (!fs.existsSync(dirName)) {
    fs.mkdirSync(dirName);
}

const fileTransport = new (winston.transports.DailyRotateFile)({
    filename: fileName,
    datePattern: "MM-DD",
    handleExceptions: true,
    humanReadableUnhandledException: true,
    zippedArchive: true,
    maxSize: "50m",
    maxFiles: "30d"
});

const transports = [
    new (winston.transports.Console)(),
    fileTransport
];

winston.exceptions.handle(fileTransport);

module.exports = winston.createLogger({
    level: logLevel,
    transports: transports,
    exitOnError: false,
    format: format.combine(
        format.timestamp({
          format: "YYYY-MM-DD HH:mm:ss"
        }),
        format.printf(info => `${info.timestamp} - ${info.level}: ${info.message}`)
    )});