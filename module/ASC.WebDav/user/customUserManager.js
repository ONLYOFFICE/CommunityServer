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
const {
    requestAuth,
    requestUser
} = require('../server/requestAPI.js');
const customUserLayout = require('./customUserLayout.js');

class customUserManager extends webdav.SimpleUserManager {
    constructor() {
        super();
        this.storeUser = new customUserLayout();
    }

    addUser(name, token, uid) {
        const user = this.storeUser.setUser(name, token, uid);
        this.users[name] = user;
        return user;
    }

    async getUserByNamePassword(ctx, username, password, callback) {
        try {
            let user = this.storeUser.getUser(username);
            if (user) {
                user = this.addUser(username, user.token, user.uid); // update last visit
            } else {
                const token = await requestAuth(ctx, username, password);
                const uid = await requestUser(ctx, token);
                user = this.addUser(username, token, uid);
                ctx.server.privilegeManager.setRights(user, '/', ['canRead']);
            }
            callback(null, user);
        } catch (error) {
            callback(webdav.Errors.UserNotFound);
        }
    }
}

module.exports = customUserManager;