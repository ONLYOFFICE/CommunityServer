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

class UserModel {
    constructor(nameId, sessionIndex, attributes, mapping) {
        this.nameID = nameId;
        this.sessionID = sessionIndex;

        const getValue = function (obj) {
            if (typeof (obj) === "string")
                return obj;

            if (Array.isArray(obj))
                return obj.length > 0 ? obj[0] : "";

            return null;
        }

        this.email = getValue(attributes[mapping.Email]);
        this.firstName = getValue(attributes[mapping.FirstName]);
        this.lastName = getValue(attributes[mapping.LastName]);
        this.location = getValue(attributes[mapping.Location]);
        this.phone = getValue(attributes[mapping.Phone]);
        this.title = getValue(attributes[mapping.Title]);
    }
}

module.exports = UserModel;