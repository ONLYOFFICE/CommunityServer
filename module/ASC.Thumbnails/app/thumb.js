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


const fs = require('fs');
const path = require('path');

const co = require("co");
const filenamify = require('filenamify-url');
const webshot = require('./webshot/webshot');
const config = require('../config');
const log = require('./log.js');

const linkReg = /http(s)?:\/\/.*/;
let urls = [];
const queue = config.get("queue");
const noThumb = path.resolve(__dirname, "..", config.noThumb);

const nodeCache = require('node-cache');

const cache = new nodeCache({
    stdTTL: 60 * 60,
    checkperiod: 60 * 60,
    useClones: false
});

function checkFileExist(pathToFile) {
  return new Promise((resolve, reject) => {
      fs.stat(pathToFile,
          function (err, stats) {
              if (err) {
                  if (err.code === 'ENOENT') {
                      resolve(false);
                  } else {
                      reject(err);
                  }
                  return;
              }

              resolve(stats.isFile());
          });
  });
}

function takeWebShot(url, pathToFile) {
  return new Promise((resolve, reject) => {
    urls.push(url);
    webshot(url, pathToFile, config.get("webshotOptions"), function(err) {      
        urls = urls.filter((item) => item !== url);

        if (err) {
            var cached = cache.get(url);
            if (cached != undefined) {
                cache.set(url, ++cached);
                log.warn("failed to load '" + url + "': " + cached);
            } else {
                cache.set(url, 1);
                log.warn("failed to load " + url);
            }

            reject(err);
            return;
        }
        resolve(pathToFile);
        });
    });
}

function error(res, e) {
    res.sendFile(noThumb);
    if(e) {
        log.error(e);
    }
}

module.exports = function (req, res) {
    try {
      if (!req.query.url || !linkReg.test(req.query.url)) throw new Error('Empty or wrong url');

      var url = req.query.url;

      const fileName = filenamify(url);
      const pathToFile = config.pathToFile(fileName);
      const root = path.join(__dirname, "..");

      function success() {
        res.sendFile(pathToFile);
      }
  
      co(function* () {
        const exists = yield checkFileExist(pathToFile);
        if (exists) {
          success();
          return;
        }

        if (urls.find(r => r === url) || urls.count > queue) {
            error(res);
        } else {
            res.sendFile(noThumb);

            var cached = cache.get(url);
            if (cached != undefined && cached > 2) return;

            yield takeWebShot(url, pathToFile);
        }

      }).catch((e) => error(res, e));
    } catch (e) {
      error(res, e);
    }
  }