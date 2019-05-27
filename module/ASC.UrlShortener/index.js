const express = require('express');
const cookieParser = require('cookie-parser');

const short = require('./app/app');
const log = require('./app/log');
log.stream = {
    write: (message) => log.info(message)
};

const sql = require('./app/sql');
const query = require('./app/sqlConsts');
const config = require('./config');
const co = require('co');



var app = express();

app.use(cookieParser());

app.get('/', short.make);
app.get('/*', short.redirect);

app.listen(config.get("port"));