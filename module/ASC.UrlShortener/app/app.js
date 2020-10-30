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


const queryConsts = require('./sqlConsts');
const shortUrl = require('./urlShortener');
const auth = require('../middleware/auth');
const query = require('./sql');
const log = require('./log');
const co = require('co');

const linkReg = /http(s)?:\/\/.*/;

let urls = [];

function processError(err, res, code = 400) {
    log.error((err && err.message) ? err.message : err);
    res.sendStatus(code);
}

function redirect(req, res) {
    let short = req.params[0];
    if (short.length > 12)
        { res.sendStatus(400); return; }

    let id = shortUrl.decode(short);
    if (!id) { res.sendStatus(400); return; }


    query(queryConsts.find, [id])
        .then((result) => {
            log.info("redirecting (" + short + ") to " + result[0].link);
            res.redirect(result[0].link);
        })
        .catch((err) => processError(err, res));
}

function make(req, res) {
    if (!auth(req)) { res.sendStatus(401); return; }

    res.contentType('text');
    
    if (!req.query.url || !linkReg.test(req.query.url)) { processError(new Error('Empty or wrong url'), res, 400); return }

    let link = req.query.url;

    co(function* () {
        var result = yield query(queryConsts.exists, [link]);

        var key;
        if (result.length) {
            if (result.short) {
                res.write(result[0].short);
                res.end();
                return;
            }
            key = shortUrl.encode(result[0].id);
            log.info("already created shortlink (" + key + ") for " + link);
        } else {
            if (urls.find(r => r === link)) {
                processError(new Error('Link is already being made'), res, 500);
                return;
            }
            result = yield query(queryConsts.insert, [link]);
            key = shortUrl.encode(result.insertId);
            log.info("creted new shortlink (" + key + ") for " + link);
            yield query(queryConsts.update, [key, result.insertId]);
        }

        urls = urls.filter((item) => item !== link);

        res.write(key);
        res.end();
    }).catch((err) => processError(err, res));
}

module.exports = {
    redirect: redirect,
    make: make
};