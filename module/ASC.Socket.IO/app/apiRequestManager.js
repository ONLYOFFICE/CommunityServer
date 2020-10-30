const apiBasePath = "/api/2.0/",
    portalManager = require('./portalManager.js'),
    request = require('request'),
    log = require("./log.js");

function makeRequest(apiMethod, req, options, onSuccess){
    makeHeaders(req, options);
    options.uri = getBasePath(req) + apiMethod;
    log.info(options.uri);

    return new Promise((resolve, reject) => {
        request(options, (error, response, body) => {
            let result = {};
            try {
                result = typeof body === "string" ? JSON.parse(body) : body;
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
        });
    });
}

function makeHeaders(req, options) {
	options.gzip = true;
    options.headers = {};
    
    if (req.cookies && req.cookies['asc_auth_key']) {
        options.headers["Authorization"] = req.cookies['asc_auth_key'];
    }

    if (req.headers) {
        const xRewriterUrlHeader = 'x-rewriter-url',
            xForwardedForHeader = 'x-forwarded-for';

        if (req.headers[xRewriterUrlHeader]) {
            options.headers[xRewriterUrlHeader] = req.headers[xRewriterUrlHeader];
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

    if (response.statusCode > 400) {
        return response.statusCode;
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
        return this.makeRequest(apiMethod, req, { method: "POST", body, json: true });
    }
    put(apiMethod, req, body) {
        return this.makeRequest(apiMethod, req, { method: "PUT", body, json: true });
    }
    dlt(apiMethod, req, body) {
        const options = { method: "DELETE" };
        if (typeof body !== "undefined") {
            options.body = req.body;
            options.json = true;
        }
        return this.makeRequest(apiMethod, req, options);
    }
    batch(batchMethod, req){
        const options = { 
            method: "POST", 
            form:{ batch: JSON.stringify(batchMethod.methods) }, 
            json: true
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