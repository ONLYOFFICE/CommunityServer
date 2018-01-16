const winston = require('winston');
const path = require('path');
const config = require('../config');
const fs = require('fs');
const fileName = config.get("logPath") || path.join(__dirname, "..", "..", "Logs", "web.log");
const dirName = path.dirname(fileName);

if (!fs.existsSync(dirName)) {
    fs.mkdirSync(dirName);
}

const fileTransport = new (winston.transports.File)(
{
    filename: fileName,
    json: false,
    handleExceptions: true,
    humanReadableUnhandledException: true
});

const transports = [
    new (winston.transports.Console)(),
    fileTransport
];

winston.handleExceptions(fileTransport);

module.exports = new winston.Logger({ transports: transports, exitOnError: false});
