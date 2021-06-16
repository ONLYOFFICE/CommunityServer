﻿/*
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


module.exports = function (socket, next) {
    const apiRequestManager = require('../apiRequestManager.js');
    const req = socket.client.request;
    const authService = require('./authService.js')();
    const co = require('co');
    const session = socket.handshake.session;

    if (req.user) {
        next();
        return;
    }

    if (!req.cookies || (!req.cookies['asc_auth_key'] && !req.cookies['authorization'])) {
        socket.disconnect('unauthorized');
        next(new Error('Authentication error'));
        return;
    }

    if(session && session.user && session.portal && typeof(session.mailEnabled) !== "undefined") {
        req.user = session.user;
        req.portal = session.portal;
        req.mailEnabled = session.mailEnabled;
        next();
        return;
    }

    if(req.cookies['authorization']){
        if(!authService(req)){
            next(new Error('Authentication error'));
        } else{
            next();
        }
        return;
    }

    co(function*(){
        var batchRequest = apiRequestManager.batchFactory()
            .get("people/@self.json?fields=id,userName,displayName")
            .get("portal.json?fields=tenantId,tenantDomain")
            .get("settings/security/2A923037-8B2D-487b-9A22-5AC0918ACF3F");

        [session.user, session.portal, session.mailEnabled] = [req.user, req.portal, req.mailEnabled] = yield apiRequestManager.batch(batchRequest, req);
        session.save();
        next();
    }).catch((err) => {
        socket.disconnect('unauthorized');
        next(new Error('Authentication error'));
    });
}