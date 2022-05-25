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

class customHTTPBasicAuthentication extends webdav.HTTPBasicAuthentication
{

    getUser (ctx, callback) {
        var _this = this;
        var onError = function (error) {
            _this.userManager.getDefaultUser(function (defaultUser) {
                callback(error, defaultUser);
            });
        };
        var authHeader = ctx.headers.find('Authorization');
        if (!authHeader) {
            onError(webdav.Errors.MissingAuthorisationHeader);
            return;
        }
        if (!/^Basic \s*[a-zA-Z0-9]+=*\s*$/.test(authHeader)) {
            onError(webdav.Errors.WrongHeaderFormat);
            return;
        }
        var value = Buffer.from(/^Basic \s*([a-zA-Z0-9]+=*)\s*$/.exec(authHeader)[1], 'base64').toString().split(':', 2);
        var username = value[0];
        var password = value[1];
        this.userManager.getUserByNamePassword(ctx, username, password, function (e, user) {
            if (e)
                onError(webdav.Errors.BadAuthentication);
            else
                callback(null, user);
        });
    }
}

module.exports = customHTTPBasicAuthentication;