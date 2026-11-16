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


const apiBasePath = "/api/2.0/",
    portalManager = require('./portalManager.js'),
    log = require("./log.js");

const requestTimeout = 30000;

function makeRequest(apiMethod, req, options, onSuccess){
    makeHeaders(req, options);
    options.uri = getBasePath(req) + apiMethod;
    log.info(options.uri);

    return new Promise((resolve, reject) => {
        send(options).then(({ error, response, body }) => {
            let result = {};
            try {
                result = body ? JSON.parse(body) : body;
            } catch (err) {
                log.error(options.uri, err);
            }

            error = checkError(error, response, result);
            if(error) {
                log.error(options.uri, error);
                if (error == 401 && req.session) {
                    req.session.destroy(() => reject(error));
                    return
                } else {
                    reject(error);
                    return;
                }
            }

            resolve(onSuccess(result));
        }).catch(reject);
    });
}

async function send(options) {
    const init = {
        method: options.method,
        headers: options.headers,
        signal: AbortSignal.timeout(requestTimeout)
    };

    if (options.form) {
        init.body = new URLSearchParams(options.form);
    } else if (typeof options.body !== "undefined") {
        init.headers["Content-Type"] = "application/json";
        init.body = JSON.stringify(options.body);
    }

    try {
        const response = await fetch(options.uri, init);
        return { response, body: await response.text() };
    } catch (error) {
        return { error };
    }
}

function deleteTrailingSlash(str) {
    return str.endsWith('/') ? str.slice(0, -1) : str;
}

function makeHeaders(req, options) {
    options.headers = {};
    
    if (req.cookies && req.cookies['asc_auth_key']) {
        options.headers["Authorization"] = req.cookies['asc_auth_key'];
    }

    if (req.headers) {
        const xRewriterUrlHeader = 'x-rewriter-url',
            xForwardedForHeader = 'x-forwarded-for';

        if (req.headers[xRewriterUrlHeader]) {
            options.headers[xRewriterUrlHeader] = deleteTrailingSlash(req.headers[xRewriterUrlHeader]) + "/";
        }
        if (req.headers[xForwardedForHeader]) {
            options.headers[xForwardedForHeader] = req.headers[xForwardedForHeader];
        }
    }
}

function getBasePath(req) {
    return portalManager(req).replace(/\/$/g, '') + apiBasePath;
}

function checkError(error, response, result) {
    if (error) {
        return error;
    }
    
    if (result && result.error && result.error.message) {
        return result.error.message;
    }

    if (response.status > 400) {
        return response.status;
    }
}

class RequestManager {
    constructor() { }

    makeRequest(apiMethod, req, options) {
        return makeRequest(apiMethod, req, options, (result) => {
            return typeof result.response !== "undefined" ? result.response : result;
        });
    }
    get(apiMethod, req, body) {
        if (body) {
            apiMethod = `${apiMethod}?`;
            for (const item in body) {
                if (body.hasOwnProperty(item)) {
                    apiMethod = `${apiMethod}${item}=${body[item]}&`;
                }
            }
            apiMethod = apiMethod.substring(0, apiMethod.length - 1);
        }
        return this.makeRequest(apiMethod, req, { method: "GET" });
    }
    post(apiMethod, req, body) {
        return this.makeRequest(apiMethod, req, { method: "POST", body });
    }
    put(apiMethod, req, body) {
        return this.makeRequest(apiMethod, req, { method: "PUT", body });
    }
    dlt(apiMethod, req, body) {
        const options = { method: "DELETE" };
        if (typeof body !== "undefined") {
            options.body = body;
        }
        return this.makeRequest(apiMethod, req, options);
    }
    batch(batchMethod, req){
        const options = { 
            method: "POST", 
            form:{ batch: JSON.stringify(batchMethod.methods) }
        };

        return makeRequest("batch.json", req, options, (result) => {
            const resultResponse = result.response;
            if(result.response !== "undefined" && Array.isArray(result.response)) {
                let data = [];

                for(let i = 0, j = resultResponse.length; i < j; i++){
                    const dataItem = typeof resultResponse[i].data === "string" ? JSON.parse(resultResponse[i].data) : resultResponse[i].data
                    data.push(dataItem.response);
                }

                return data;
            }

            return result;
        });
    }
    batchFactory(){
        return new batchFactory();
    }
}

class batchFactory {
    constructor() { 
        this.methods = [];
    }
    get(url) {
        this.methods.push({method: "get", RelativeUrl: `${apiBasePath}${url}`});
        return this;
    }
}

module.exports = new RequestManager();