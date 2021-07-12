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

const config = require("../../config").get(),
    hash = require("./hash");

var Coder = function() {
    return {
        encodeData: function(data) {
            if (!data && typeof (data) !== "object")
                return undefined;

            const jsonStr = JSON.stringify(data);
            const dataEncoded = hash.encode(jsonStr, config["core.machinekey"] ? config["core.machinekey"] : config.app.machinekey);

            return dataEncoded;
        },

        decodeData: function(data) {
            if (!data && typeof (data) !== "string")
                return undefined;

            const jsonStr = hash.decode(data, config["core.machinekey"] ? config["core.machinekey"] : config.app.machinekey);
            const dataDecoded = JSON.parse(jsonStr);          

            return dataDecoded;
        }
    };
};

module.exports = Coder();