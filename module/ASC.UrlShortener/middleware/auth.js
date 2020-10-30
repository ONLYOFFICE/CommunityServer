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


const
    config = require('../config'),
    crypto = require('crypto'),
    moment = require('moment');

const skey = config.get("core.machinekey");
const trustInterval = 5 * 60 * 1000;

function check(req) {
    const authHeader = req.headers["authorization"] || req.cookies["authorization"];
    if(!authHeader) return false;

    const splitted = authHeader.split(':');
    if (splitted.length < 3) return false;

    const pkey = splitted[0].substr(4);
    const date = splitted[1];
    const orighash = splitted[2];

    const timestamp = moment.utc(date, "YYYYMMDDHHmmss");
    if (moment.utc() - timestamp > trustInterval) {
        return false;
    }

    const hasher = crypto.createHmac('sha1', skey);
    const hash = hasher.update(date + "\n" + pkey);

    if (hash.digest('base64') !== orighash) {
        return false;
    }

    return true;
}

module.exports = (req) => {
    return check(req);
};