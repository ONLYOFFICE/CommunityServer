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