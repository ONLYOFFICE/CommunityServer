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