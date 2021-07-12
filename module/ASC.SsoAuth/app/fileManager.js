/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
    co = require("co"),
    request = require("request");

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

function downloadFile(uri, filePath) {
    return new Promise((resolve, reject) => {
        const data = request
            .get(uri, { rejectUnauthorized: false })
            .on('error', (err) => {
                reject(err);
            })
            .pipe(fs.createWriteStream(filePath))
            .on('error', (err) => {
                reject(err);
            })
            .on('finish', () => {
                resolve(filePath);
            });
    });
}

module.exports = { checkFileExist, createDir, copyFile, moveFile, deleteFile, getDataDirPath, downloadFile };

