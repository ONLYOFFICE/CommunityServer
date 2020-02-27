/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


const sql = require('mysql');
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