/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


"use strict";

const fs = require('fs'),
    path = require('path'),
    http = require('http'),
    https = require('https'),
    co = require("co"),
    { Transform } = require('stream'),
    { pipeline } = require('stream/promises');

const downloadTimeout = 30000;
const maxRedirects = 10;

const config = require('../config');
const appDataDirPath = path.join(__dirname, config.get("web:data"));

function getDataDirPath(subdir = "", createIfNotExist = true) {
    var fullPath = path.join(appDataDirPath, subdir);
    if (!createIfNotExist) return fullPath;

    return co(function* () {
        yield createDir(appDataDirPath);
        return createDir(fullPath);
    });
}

function createDir(pathToDir) {
    return co(function* () {
        const exist = yield checkDirExist(pathToDir);
        if (exist) {
            return pathToDir;
        }
        return new Promise((resolve, reject) => {
            fs.mkdir(pathToDir,
                (err) => {
                    if (err) {
                        reject(err);
                        return;
                    }

                    resolve(pathToDir);
                });
        });
    }).catch((err) => {
        log.error("Create App_Data error", err);
    });
}

function checkDirExist(pathToDir) {
    return new Promise((resolve, reject) => {
        fs.stat(pathToDir,
            function (err, stats) {
                if (err) {
                    if (err.code === 'ENOENT') {
                        resolve(false);
                    } else {
                        reject(err);
                    }
                    return;
                }

                resolve(stats.isDirectory());
            });
    });
}

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

function copyFile(source, target, append = false) {
    return new Promise(function (resolve, reject) {
        var rd = fs.createReadStream(source);
        rd.on('error', rejectCleanup);

        const writeOptions = { flags: append ? 'a' : 'w' };

        var wr = fs.createWriteStream(target, writeOptions);
        wr.on('error', rejectCleanup);

        function rejectCleanup(err) {
            rd.destroy();
            wr.end();
            reject(err);
        }
        wr.on('finish', resolve);
        rd.pipe(wr);
    });
}

function moveFile(from, to) {
    return co(function* () {
        const isExist = yield checkFileExist(from);
        if (!isExist) return;

        return new Promise((resolve, reject) => {
            fs.rename(from, to, () => { resolve(); });
        });
    })
        .catch((error) => {
            throw error;
        });
}

function deleteFile(pathToFile) {
    return co(function* () {
        const isExist = yield checkFileExist(pathToFile);
        if (!isExist) return;

        return new Promise((resolve, reject) => {
            fs.unlink(pathToFile, () => { resolve(); });
        });
    })
        .catch((error) => {
            throw error;
        });
}

function checkUrl(url) {
    if (url.protocol !== "http:" && url.protocol !== "https:") {
        throw new Error(`Unsupported protocol ${url.protocol}`);
    }
    return url;
}

// IdP metadata endpoints often use self-signed certificates, so TLS errors are ignored here
function get(url, signal, redirects = 0) {
    return new Promise((resolve, reject) => {
        const client = url.protocol === "https:" ? https : http;
        const request = client.get(url, { rejectUnauthorized: false, signal }, (response) => {
            const { statusCode, headers } = response;

            if (statusCode >= 300 && statusCode < 400 && headers.location) {
                response.resume();
                if (redirects >= maxRedirects) {
                    reject(new Error("Too many redirects"));
                    return;
                }
                try {
                    resolve(get(checkUrl(new URL(headers.location, url)), signal, redirects + 1));
                } catch (err) {
                    reject(err);
                }
                return;
            }

            if (statusCode < 200 || statusCode >= 300) {
                response.resume();
                reject(new Error(`Download failed with status ${statusCode}`));
                return;
            }

            resolve(response);
        });

        request.on('error', reject);
    });
}

async function downloadFile(uri, filePath, maxSize) {
    // one deadline for the redirects and the body, not a per-socket inactivity timeout
    const signal = AbortSignal.timeout(downloadTimeout);

    try {
        const response = await get(checkUrl(new URL(uri)), signal);

        let size = 0;
        const limit = new Transform({
            transform(chunk, encoding, callback) {
                size += chunk.length;
                callback(size > maxSize ? new Error("File is too large") : null, chunk);
            }
        });

        await pipeline(response, limit, fs.createWriteStream(filePath), { signal });
        return filePath;
    } catch (err) {
        throw signal.aborted ? new Error("Download timed out") : err;
    }
}

module.exports = { checkFileExist, createDir, copyFile, moveFile, deleteFile, getDataDirPath, downloadFile };

