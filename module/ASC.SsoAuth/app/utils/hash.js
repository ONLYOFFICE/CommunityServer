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

var Hash = function () {
    var getHashBase64 = function (str) {
        const crypto = require("crypto");
        const sha256 = crypto.createHash("sha256");
        sha256.update(str, "utf8");
        const result = sha256.digest("base64");
        return result;
    };

    return {
        encode: function(str, secret) {
            try {
                const strHash = getHashBase64(str + secret) + "?" + str;

                let data = new Buffer(strHash).toString("base64");

                let cnt = 0;
                while (data.indexOf("=") !== -1) {
                    cnt++;
                    data = data.replace("=", "");
                }

                data = (data + cnt).replace(/\+/g, "-").replace(/\//g, "_");

                return data;

            } catch (ex) {
                return null;
            }
        },
        decode: function(str, secret) {
            try {

                let strDecoded = Buffer.from(unescape(str), "base64").toString();

                const lastIndex = strDecoded.lastIndexOf("}");

                if (lastIndex + 1 < strDecoded.length) {
                    strDecoded = strDecoded.substring(0, lastIndex + 1);
                }

                const index = strDecoded.indexOf("?");

                if (index > 0 && strDecoded[index + 1] == '{') {
                    let hash = strDecoded.substring(0, index);
                    let data = strDecoded.substring(index + 1);
                    if(getHashBase64(data + secret) === hash)
                    {
                        return data;
                    }
                }

                // Sig incorrect
                return null;

            } catch (ex) {
                console.error("hash.decode", str, secret, ex);
                return null;
            }
        }
    };
};

module.exports = Hash();

