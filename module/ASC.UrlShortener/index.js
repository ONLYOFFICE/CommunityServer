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