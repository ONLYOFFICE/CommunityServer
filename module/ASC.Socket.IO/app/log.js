const winston = require('winston');
require('winston-daily-rotate-file')

const path = require('path');
const config = require('../config');
const fs = require('fs');
const fileName = config.get("logPath") || path.join(__dirname, "..", "..", "Logs", "web.socketio.%DATE%.log");
const dirName = path.dirname(fileName);

if (!fs.existsSync(dirName)) {
    fs.mkdirSync(dirName);
}

const fileTransport = new (winston.transports.DailyRotateFile)(
{
    filename: fileName,
    datePattern: 'MM-DD',
    handleExceptions: true,
    humanReadableUnhandledException: true,
    zippedArchive: true,
    maxSize: '50m',
    maxFiles: '30d'
});

const transports = [
    new (winston.transports.Console)(),
    fileTransport
];

winston.exceptions.handle(fileTransport);

module.exports = winston.createLogger({ transports: transports, exitOnError: false});
