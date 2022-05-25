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


const logger = require('./logger.js');

const logSplitter = ' | ';

var getHeader = function (contentType, token) {
    const headers = {
        ContentType: contentType,
        Accept: 'application/json'
    };
    if (token) {
        headers.Authorization = token;
    }
    return headers;
};

var getHeaderPeople = function (token) {
    const headers = {
        Accept: 'text/html,application/xhtml+xml,application/xml'
    };
    if (token) {
        headers.Authorization = token;
    }
    return headers;
};

var getErrorLogMsg = function (error) { 
    let message = '';
    if (error) {
        if (error.message) {
            message = `${logSplitter}${error.message}`;
        }
        if (error.request) {
            message += `${logSplitter}${error.request.method + ' ' + error.request.path}`
        }
        if (error.response) {
            if (error.response.config) {
                message += `${logSplitter}${error.response.config.data}`
            }
            if (error.response.data && error.response.data.error) {
                message += `${logSplitter}${error.response.data.error.message + '\n' + error.response.data.error.stack}`
            }
        }
    }
    return message;
};

var getContextLogMsg = function (ctx) { 
    let message = '';
    if (ctx) {
        if (ctx.user) {
            message += `${logSplitter}${ctx.user.username}`
        }
        if (ctx.headers) {
            message += `${logSplitter}${ctx.headers.headers["user-agent"]}`
        }
        if (ctx.request) {
            message += `${logSplitter}${ctx.request.method} ${ctx.requested.uri}`
        }
        if (ctx.response) {
            message += `${logSplitter}${ctx.response.statusCode} ${ctx.response.statusMessage || ''}`
        }
    }
    return message;
}

var getResponseLogMsg = function (response) { 
    let message = '';
    if (response) {
        if (response.config) {
            let url = response.config.baseURL
                ? response.config.baseURL + "/" + response.config.url.replace(/^\//, '')
                : response.config.url;

            message += `${logSplitter}${response.config.method} ${url}`
            if (response.config.data) {
                message += `${logSplitter}${response.config.data}`
            }
        }
        if (response.status) {
            message += `${logSplitter}${response.status} ${response.statusText}`
        }
        if (response.data) {
            //message += `${logSplitter}${JSON.stringify(response.data.response)}`
        }
    }
    return message;
}

var logError = function (error, method) { 
    let message = method || '';
    message += getErrorLogMsg(error);
    logger.error(message);
};

var logResponse = function (ctx, response, method) { 
    let message = method || '';
    message += getContextLogMsg(ctx);
    message += getResponseLogMsg(response);
    logMessage(message);
};

var logContext = function (ctx, method) { 
    let message = method || '';
    message += getContextLogMsg(ctx);
    logMessage(message);
}

var logMessage = function () { 
    if (arguments.length > 0) {
        logger.debug(Array.prototype.join.call(arguments, logSplitter));
    }
}

module.exports = {
    getHeader,
    getHeaderPeople,
    logError,
    logResponse,
    logContext,
    logMessage
};