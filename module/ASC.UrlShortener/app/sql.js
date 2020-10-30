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


const sql = require('mysql2');
const log = require('./log');
const config = require('../config');
const co = require('co');

var connection;

function createNewConnection() {
    return new Promise((resolve, reject) => {
        log.info("establishing new connection");
        connection = sql.createConnection(config.get("sql"));
        connect().then(resolve).catch(reject);
        connection.on('error', onError);
    });
}

function connect() {
    return new Promise((resolve, reject) => connection.connect(function(err) {
        if (err) {
            connection = null;
            reject(err);
            return;
        }

        log.info("connected to sql");
        resolve();
    }));
}

function reconnect() {
    co(function* () {
        var shouldReconnect = true;
        var attempts = 0;

        while(shouldReconnect) {
            try {
                yield new Promise((resolve, reject) => setTimeout(resolve, 1000 * 5));
                attempts++;
                log.warn("reconnecting to sql, attempt: " + attempts);
                yield createNewConnection();
                shouldReconnect = false;
            } catch(err) {
                log.error(err);
            }
        }
    });
}

function onError(err) {
    log.error("sql error: " + err.code);
    if (err.fatal) {
        reconnect();
    }
}

const query = (query, params) => new Promise((resolve, reject) => {
    co(function* () {
        if (!connection) yield createNewConnection();
    }).catch(reject);

    connection.query(query, params, function(err, res) {
        if (err) reject(err);
        resolve(res);
    });
});

module.exports = query;