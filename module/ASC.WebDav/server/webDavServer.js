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


const webdav = require('webdav-server').v2;
const express = require('express');

const FileSystem = require('../manager/customFileSystem');
const customUserManager = require('../user/customUserManager');
const customHTTPBasicAuthentication = require('../user/authentication/customHTTPBasicAuthentication');
const fs = require('fs');
const {
    port,
    usersCleanupInterval,
    pfxKeyPath,
    pfxPassPhrase,
    certPath,
    keyPath,
    isHttps,
    virtualPath
} = require('./config.js');
const { logContext, logMessage } = require('../helper/helper.js');

const userManager = new customUserManager();
const privilegeManager = new webdav.SimplePathPrivilegeManager();

const options = {
    port: process.env.port || port,
    requireAuthentification: true,
    httpAuthentication: new customHTTPBasicAuthentication(userManager),
    rootFileSystem: new FileSystem(),
    privilegeManager: privilegeManager
};
if (isHttps) {
    if (!(pfxKeyPath && pfxPassPhrase) && !(certPath && keyPath)) {
        throw new Error("A secure connection is activated, but there are no keys");
    }
    if (pfxKeyPath && pfxPassPhrase) {
        options.https = {
            pfx: fs.readFileSync(pfxKeyPath),
            passphrase: pfxPassPhrase
        };
    } else {
        options.https = {
            cert: fs.readFileSync(certPath),
            key: fs.readFileSync(keyPath)
        };
    }
}

const server = new webdav.WebDAVServer(
    options
);

setInterval(function () {
    userManager.storeUser.deleteExpiredUsers((expiredUserIds) => {
        logMessage("server.deleteExpiredUsers", expiredUserIds);
        server.fileSystems['/'].manageResource.structСache.deleteStructs(expiredUserIds);
    })
}, usersCleanupInterval);

server.afterRequest((ctx, next) => {
    //logContext(ctx, "afterRequest");
    next();
});
server.beforeRequest((ctx, next) => {
    if (virtualPath) {
        if (ctx.requested.path.paths[0] != virtualPath) {
            ctx.requested.path.paths.unshift(virtualPath);
            ctx.requested.uri = ctx.requested.path.toString();
        }
    }
    //logContext(ctx, "beforeRequest");
    next();
});
//server.start((s) => console.log('Ready on port', s.address().port));

const app = express();

app.use("/isLife", (req, res) => {
    res.sendStatus(200);
});

app.use(webdav.extensions.express('/', server));

app.listen(options.port);