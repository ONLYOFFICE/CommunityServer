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