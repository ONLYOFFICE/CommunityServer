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


; window.ServiceHelper = (function (factory) {
    var
      isInit = false,
      useBatch = true,
      useJoint = true,
      jointStart = 0,
      jointLimit = 500,
      jointRequests = [],
      completeInit = 0,
      apiPath = null,
      cmdRequestId = '__',
      cmdSeparator = '/',
      observerHandler = 0,
      observerTimeout = 50,
      maxRequestAttempts = 3,
      requestTimeout = 60 * 1000,
      myProfile = null,
      portalSettings = null,
      customEvents = {},
      uploaders = {},
      requests = [],
      lastTimeCall = new Date();

    function xmlToJson(xml) {
        var
          attr,
          child,
          attrs = xml.attributes,
          children = xml.childNodes,
          key = xml.nodeType,
          obj = {},
          val = null,
          i = -1;

        if (key == 1 && attrs.length) {
            obj[key = '@attributes'] = {};
            while (attr = attrs.item(++i)) {
                obj[key][attr.nodeName] = attr.nodeValue;
            }
            i = -1;
        } else if (key == 3) {
            obj = xml.nodeValue;
        }
        while (child = children.item(++i)) {
            key = child.nodeName;
            if (obj.hasOwnProperty(key)) {
                if (obj.toString.call(obj[key]) != '[object Array]') {
                    obj[key] = [obj[key]];
                }
                obj[key].push(xmlToJson(child));
            }
            else {
                if (key === '#text') {
                    val = xmlToJson(child);
                    obj = isFinite(+val) ? +val : val;
                    if (val === 'true' || val === 'false') {
                        obj = val === 'true';
                    }
                } else {
                    obj[key] = xmlToJson(child);
                }
            }
        }
        return obj;
    }

    function isArray(o) {
        return o ? o.constructor.toString().indexOf("Array") != -1 : false;
    }

    function isFile(o) {
        return o ? o.constructor.toString().indexOf("File") != -1 : false;
    }

    function processedDateType(o) {
        if (isFile(o)) {
            return false;
        }
        //check other types
        return true;
    }

    function getRandomId(prefix) {
        return '' + (prefix ? prefix + '-' : '') + Math.floor(Math.random() * 1000000);
    }

    function getUniqueId(o, prefix) {
        var
          iterCount = 0,
          maxIterations = 1000,
          uniqueId = getRandomId(prefix);
        while (o.hasOwnProperty(uniqueId) && iterCount++ < maxIterations) {
            uniqueId = getRandomId(prefix);
        }
        return uniqueId;
    }

    function objectToParams(opts, obj, hash) {
        var value = null,
            items = null;
        for (var fld in obj) {
            if (obj.hasOwnProperty(fld)) {
                var wasAdded = false;
                value = obj[fld];
                hash ? hash[fld] = value : null;
                if (typeof value === 'string' && value.length > 0) {
                    value = encodeURIComponent(value);
                }
                if (value && isArray(value)) {
                    items = [];
                    for (var i = 0, n = value.length; i < n; i++) {
                        items.push(fld + '[]=' + encodeURIComponent(value[i]));
                    }
                    opts.push(items.join('&'));
                    wasAdded = true;
                }
                wasAdded === false ? opts.push(fld + '=' + value) : null;
            }
        }
        return opts;
    }

    function getUrlByFilter(type, url, params, data, options) {
        var opts = [];

        switch (type) {
            case 'get':
            case 'delete':
                if (data && typeof data === 'object') {
                    opts = objectToParams(opts, data, null);
                    data = null;
                }
                break;
        }

        if (!options || typeof options !== 'object') {
            return url;
        }

        params.__filter = {};
        var filter = options.hasOwnProperty('filter') ? options.filter || {} : {};
        opts = objectToParams(opts, filter, params.__filter);
        return url + (opts.length > 0 ? '?' + opts.join('&') : '');
    }

    function canAddRequest(req, requests) {
        var item = null;
        if (!req) {
            return false;
        }
        if (req.__once === true) {
            var requestsInd = requests.length;
            while (requestsInd--) {
                item = requests[requestsInd];
                if (item.__eventname === req.__eventname && item.type === req.type && item.url === req.url) {
                    return false;
                }
            }
        }
        return true;
    }

    function getUrl() {
        var url = '',
            cmds = [];
        for (var i = 0, n = arguments.length; i < n; i++) {
            cmds.push(arguments[i]);
        }
        url = cmds.join(cmdSeparator);
        //console.log(apiPath + (url.charAt(0) === cmdSeparator ? url.substring(1) : url))
        return apiPath + (url.charAt(0) === cmdSeparator ? url.substring(1) : url);
    }

    function clearRequest(req) {
        for (var fld in req) {
            delete req[fld];
            req[fld] = null;
        }
    }

    function getRequestById(id) {
        for (var i = 0, n = requests.length; i < n; i++) {
            if (requests[i].__id == id) {
                return requests[i];
            }
        }
        return null;
    }

    function removeRequestById(id) {
        var requestsInd = requests.length;
        while (requestsInd--) {
            if (requests[requestsInd].__id == id) {
                clearRequest(requests[requestsInd]);
                delete requests[requestsInd];
                requests[requestsInd] = null;
                requests.splice(requestsInd, 1);
            }
        }
        delete req;
    }

    function getRequestByObject(o) {
        if (!o || typeof o !== 'object' || !o.__id) {
            return null;
        }
        for (var i = 0, n = requests.length; i < n; i++) {
            if (requests[i].__id == o.__id) {
                return requests[i];
            }
        }
        return null;
    }

    function execExtention(req, nonetrigger) {
        var e;
        if (typeof req.__errorcallback === 'function') {
            try {
                req.__errorcallback(req.__params, req.__errors);
            } catch (e) {
                console.log(e);
            }
        }
        if (nonetrigger !== true) {
            try {
                exec('extention', this, [req.__eventname, req.__params, req.__errors]);
            } catch (e) {
                console.log(e);
            }
        }
    }

    function checkRequest(req) {
        if (!req) {
            return false;
        }

        if ((req.__attcount != 0 && req.type != "get") || (req.__attcount >= req.__max_request_attempts)) {
            if (req.hasOwnProperty("__jointrequests") && req.__jointrequests) {
                for (var i = 0, n = req.__jointrequests.length; i < n; i++) {
                    var jointrequest = req.__jointrequests[i];
                    if (typeof jointrequest.__aftercallback === "function") {
                        jointrequest.__aftercallback(jointrequest.__params);
                    }
                }
            }
            if (typeof req.__aftercallback === 'function') {
                req.__aftercallback(req.__params);
            }
            execExtention(req);
            var requestsInd = requests.length;
            while (requestsInd--) {
                if (requests[requestsInd] === req) {
                    clearRequest(requests[requestsInd]);
                    delete requests[requestsInd];
                    requests[requestsInd] = null;
                    requests.splice(requestsInd, 1);
                }
            }
            delete req;
            return false;
        }
        return true;
    }

    function checkResponse(req, jqXHR, textStatus) {
        if (!req) {
            return false;
        }

        var
          errorMsg = null,
          isInvalidRequest = false,
          isUnauthenticationRequest = false,
          response = null;

        switch (textStatus) {
            case 'error':
                //  req.__errcount++;
                isInvalidRequest = true;
                //errorMsg = 'throw error';
                break;
            case 'timeout':
                req.__totcount++;
                isInvalidRequest = true;
                errorMsg = 'timeout expected';
                break;
            case 'parsererror':
                //  isInvalidRequest = true;
                //  errorMsg = 'parse error';
                break;
        }

        switch (jqXHR.status) {
            case 401:
                isInvalidRequest = true;
                isUnauthenticationRequest = true;
                req.__attcount += req.__max_request_attempts;
                errorMsg = 'unauthorized request';
                break;
        }

        //if (jqXHR.isRejected()) {
        //  isInvalidRequest = true;
        //  isUnauthenticationRequest = true;
        //  errorMsg = 'rejected request';
        //}

        if (!isInvalidRequest) {
            try {
                response = jqXHR.responseObject || jQuery.parseJSON(jqXHR.responseText);
            } catch (err) {
                response = null;
                isInvalidRequest = true;
                errorMsg = 'response parse error';
            }
            if (!isInvalidRequest && !response) {
                isInvalidRequest = true;
                errorMsg = 'response parse error';
            }
            if (!isInvalidRequest && response.response == null) {
                if (response.hasOwnProperty('error')) {
                    isInvalidRequest = true;
                    req.__attcount += req.__max_request_attempts;
                    errorMsg = response.error.message || 'throw error';
                } else {
                    isInvalidRequest = true;
                    errorMsg = 'empty response';
                }
            }
        }
        
        if (!isInvalidRequest) {
            if (response && response.status != 0) {
                isInvalidRequest = true;
                errorMsg = 'invalid status';
            }
        }

        if (isInvalidRequest) {
            if (errorMsg) {
                req.__errors.push(errorMsg);
            } else {
                try {
                    response = jqXHR.responseObject || jQuery.parseJSON(jqXHR.responseText);
                } catch (err) {
                    response = null;
                }

                if (response && response.error) {
                    req.__errors.push(response.error.message);
                    req.__errors.push(response.error);
                }
            }
            //if (req.__errors.length > 0) {
                if (isUnauthenticationRequest) exec('unauthenticated', this, [req.__params]);

                if (req.hasOwnProperty("__jointrequests") && req.__jointrequests) {
                    for (var i = 0, n = req.__jointrequests.length; i < n; i++) {
                        var jointrequest = req.__jointrequests[i];
                        if (typeof jointrequest.__errorcallback === "function") {
                            jointrequest.__errorcallback(jointrequest.__params, req.__errors);
                        }
                    }
                }

                var batchrequests = req.__batchrequests && isArray(req.__batchrequests) ? req.__batchrequests : [];
                for (var requestInd = 0, requestCnt = batchrequests.length; requestInd < requestCnt; requestInd++) {
                    var name = batchrequests[requestInd].Name,
                        request = getRequestById(name);
                    if (request) {
                        removeRequestById(request.__id);
                    }
                }
                req.__processing = false;

            //}
            delete req;
            return false;
        }
        return true;
    }

    function checkUpload(req, jqXHR, textStatus) {
        if (!req) {
            return false;
        }

        var
          errorMsg = null,
          isInvalidRequest = false,
          isUnauthenticationRequest = false,
          response = null;

        isInvalidRequest = false;
        isUnauthenticationRequest = false;

        //switch (textStatus) {
        //  //case 'error' :
        //  //  req.__errcount++;
        //  //  isInvalidRequest = true;
        //  //  errorMsg = 'throw error';
        //  //  break;
        //  case 'timeout' :
        //    req.__totcount++;
        //    isInvalidRequest = true;
        //    errorMsg = 'timeout expected';
        //    break;
        //  case 'parsererror' :
        //  //  isInvalidRequest = true;
        //  //  errorMsg = 'parse error';
        //    break;
        //}

        //switch (jqXHR.status) {
        //  case 401 :
        //    isInvalidRequest = true;
        //    isUnauthenticationRequest = true;
        //    req.__attcount += maxRequestAttempts;
        //    errorMsg = 'unauthorized request';
        //    break;
        //}

        //if (jqXHR.isRejected()) {
        //  isInvalidRequest = true;
        //  isUnauthenticationRequest = true;
        //  errorMsg = 'rejected request';
        //}

        if (!isInvalidRequest) {
            try {
                if (jQuery.isXMLDoc(jqXHR.responseText)) {
                    response = xmlToJson(jqXHR.responseText);
                    response = response.result || null;
                }
                if (typeof jqXHR.responseText === 'string') {
                    response = jqXHR.responseText.replace(/^<pre>/, '').replace(/<\/pre>$/, '');
                    response = jQuery.parseJSON(jqXHR.responseText);
                }
            } catch (err) {
                response = null;
                isInvalidRequest = true;
                errorMsg = 'response parse error';
            }
        }

        if (!isInvalidRequest && !response) {
            isInvalidRequest = true;
            errorMsg = 'response parse error';
        }

        if (!isInvalidRequest && !response.response) {
            if (response.hasOwnProperty('error')) {
                isInvalidRequest = true;
                req.__attcount += req.__max_request_attempts;
                errorMsg = response.error.message || 'throw error';
            } else {
                isInvalidRequest = true;
                errorMsg = 'empty response';
            }
        }

        if (!isInvalidRequest) {
            if (response && response.status != 0) {
                isInvalidRequest = true;
                errorMsg = 'invalid status';
            }
        }

        if (isInvalidRequest) {
            if (errorMsg) {
                req.__errors.push(errorMsg);
                execExtention(req);
            }
            if (isUnauthenticationRequest) {
                exec('unauthenticated', this, []);
            }
            req.__processing = false;
            delete req;
            return false;
        }
        return true;
    }

    function requestSuccess(req, eventname, params, responses, paramses, jqXHR, textStatus) {
        var obj = responses && req.__singleresponse === true ? factory.create(req.url, req.type, responses[0], responses) : null;

        if (typeof obj !== 'undefined' && obj != null) {
            var responseParams = paramses[0];
            for (var fld in responseParams) {
                if (responseParams.hasOwnProperty(fld)) {
                    params['__' + fld] = responseParams[fld];
                }
            }

            if (req.__isme === true) {
                onGetMe(req.__params, obj);
            }
            if (req.__isauth === true) {
                onGetAuthentication(req.__params, obj);
            }
            if (req.__issettings === true) {
                onGetSettings(req.__params, obj);
            }

            //TODO: Replace to execSuccess.call(this, req, [req.__params, obj]); // when we give up of -> this.__responses[0];

            var needexec = true;
            if (typeof req.__successcallback === 'function') {
                try {
                    if (req.__successcallback(req.__params, obj) === false) {
                        needexec = false;
                    }
                } catch (e) {
                    console.log(e);
                }
            }
            if (needexec === true) {
                try {
                    exec(null, this, [req.__params, obj]);
                    if (req.__eventname) {
                        exec(req.__eventname, this, [req.__params, obj]);
                    }
                } catch (e) {
                    console.log(e);
                }
            }
        }

        if (obj === null && responses && req.__singleresponse === false) {
            var args = [], paramsCallback = [], IsValidData = true;

            for (var par in paramses) {
                if (paramses.hasOwnProperty(par)) {
                    var paramForPush = {};
                    var currentParam = paramses[par];
                    for (var fld in currentParam) {
                        if (currentParam.hasOwnProperty(fld)) {
                            paramForPush['__' + fld] = currentParam[fld];
                        }
                    }
                    paramsCallback.push(paramForPush);
                }
            }
            paramsCallback.push(params); //Params for batch when exec .start()
            args.push(paramsCallback);
            for (var i = 0, n = responses.length; i < n; i++) {
                obj = factory.create(req.__urls[i], req.__methods[i], responses[i], responses);
                args.push(obj);
            }
            if (IsValidData) {
                execSuccess.call(this, req, args);
            }
        }

        removeRequestById(req.__id);
    }

    function execSuccess(req, args) {
        var needexec = true;
        if (typeof req.__successcallback === 'function') {
            try {
                if (req.__successcallback.apply(this, args) === false) {
                    needexec = false;
                }
            } catch (e) {
                console.log(e);
            }
        }
        if (needexec === true) {
            try {
                exec(null, this, args);
                if (req.__eventname) {
                    exec(req.__eventname, this, args);
                }
            } catch (e) {
                console.log(e);
            }
        }
    }

    function requestComplete(jqXHR, textStatus) {
        var
          response = null,
          req = getRequestByObject(this);

        if (!checkResponse(req, jqXHR, textStatus)) {
            return undefined;
        }

        response = jQuery.parseJSON(jqXHR.responseText);
        req = req.__processing === true ? req : null;
        if (!req) {
            return undefined;
        }

        if (typeof req.__aftercallback === 'function') {
            req.__aftercallback(req.__params);
        }

        req.__urls.push(req.url);
        req.__methods.push(req.type);
        req.__responses.push(response.response);
        req.__paramses.push({ count: response.count, startIndex: response.startIndex, nextIndex: response.nextIndex || undefined, total: response.total || undefined });

        if (req.batch.length > 0) {
            delete req.__errors;
            req.__errors = [];
            req.__errcount = 0;
            req.__totcount = 0;
            req.__attcount = 0;
            req.__processing = false;
            req.url = req.batch[0];
            req.batch = req.batch.slice(1);
            return undefined;
        }

        var args = [req, req.__eventname, req.__params, req.__responses, req.__paramses];
        for (var i = 0, n = arguments.length; i < n; i++) {
            args.push(arguments[i]);
        }
        requestSuccess.apply(this, args);
        removeRequestById(req.__id);
    }

    function batchrequestComplete(jqXHR, textStatus) {
        var
          batchrequests = null,
          batchrequestsInd = 0,
          batchresponses = [],
          batchurls = [],
          batchmethods = [],
          paramses = [],
          batchreq = getRequestByObject(this),
          responses = null,
          response = null,
          resp = null,
          name = null,
          url = null,
          req = null;

        if (!checkResponse(batchreq, jqXHR, textStatus)) {
            return undefined;
        }

        response = jqXHR.responseObject || jQuery.parseJSON(jqXHR.responseText);
        batchreq = batchreq.__processing === true ? batchreq : null;
        if (!batchreq) {
            return undefined;
        }

        if (batchreq.hasOwnProperty("__jointrequests") && batchreq.__jointrequests) {
            for (var i = 0, n = batchreq.__jointrequests.length; i < n; i++) {
                var jointrequest = batchreq.__jointrequests[i];
                if (typeof jointrequest.__aftercallback === "function") {
                    jointrequest.__aftercallback(jointrequest.__params);
                }
            }
        }

        if (typeof batchreq.__aftercallback === 'function') {
            batchreq.__aftercallback(batchreq.__params);
        }

        batchrequests = batchreq.__batchrequests && isArray(batchreq.__batchrequests) ? batchreq.__batchrequests : [];

        var
          somereq = null,
          args = null,
          defaultargs = [],
          responses = isArray(response.response) ? response.response : [response.response];

        for (var i = 0, n = arguments.length; i < n; i++) {
            defaultargs.push(arguments[i]);
        }

        for (var responsesInd = 0, responsesCnt = responses.length; responsesInd < responsesCnt; responsesInd++) {
            resp = responses[responsesInd];
            name = resp.name;
            req = getRequestById(name);

            batchrequestsInd = batchrequests.length;
            while (batchrequestsInd--) {
                if (batchrequests[batchrequestsInd].Name === name) {
                    url = batchrequests[batchrequestsInd].RelativeUrl;
                    method = batchrequests[batchrequestsInd].RelativeMethod;
                }
            }

            try {
                resp = jQuery.parseJSON(resp.data);
            } catch (err) {
                resp = null;
            }

            if (!checkResponse(batchreq, { status: resp.statusCode || 200, responseObject: resp }, textStatus)) {
                resp = { response: null };
            }

            if (resp && url) {
                batchurls.push(url);
                batchmethods.push(method);
                batchresponses.push(resp.response);
                paramses.push({ count: resp.count, startIndex: resp.startIndex, nextIndex: resp.nextIndex || undefined, total: resp.total || undefined });
            }

            if (req && resp && req.url && resp.response) {
                req.__urls.push(req.url);
                req.__methods.push(req.type);
                req.__responses.push(resp.response);
                req.__paramses.push({ count: resp.count, startIndex: resp.startIndex, nextIndex: resp.nextIndex || undefined, total: resp.total || undefined });

                args = [req, req.__eventname, req.__params, req.__responses, req.__paramses].concat(defaultargs);
                requestSuccess.apply(this, args);
                removeRequestById(req.__id);
            }
        }

        for (var batchrequestsInd = 0, batchrequestsCnt = batchrequests.length; batchrequestsInd < batchrequestsCnt; batchrequestsInd++) {
            req = getRequestById(batchrequests[batchrequestsInd].Name);
            if (req && req.__id) {
                removeRequestById(req.__id);
            }
        }

        req = getRequestByObject(this);
        if (req) {
            req.__urls = [].concat(batchurls);
            req.__methods = [].concat(batchmethods);
            req.__responses = [].concat(batchresponses);
            req.__paramses = [].concat(paramses);

            if (req.__errors.length > 0) {
                req.__params.__errors = req.__errors;
                execExtention(req, true);
            }

            args = [req, req.__eventname, req.__params, req.__responses, req.__paramses].concat(defaultargs);
            requestSuccess.apply(this, args);
            removeRequestById(req.__id);
        }
    }

    function uploadSubmit(reqid, file, extension) {
        var
          req = uploaders.hasOwnProperty(reqid) ? uploaders[reqid] : null;

        if (req) {
            if (typeof req.__beforecallback === 'function') {
                req.__beforecallback(req.__params, file, extension);
            }
        }
    }

    function uploadComplete(reqid, filename, response) {
        var
          req = uploaders.hasOwnProperty(reqid) ? uploaders[reqid] : null;

        if (req && typeof req.__aftercallback === 'function') {
            req.__aftercallback(req.__params);
        }

        if (!checkUpload(req, { status: 0, responseText: response }, 'success')) {
            return undefined;
        }

        if (jQuery.isXMLDoc(response)) {
            response = xmlToJson(response);
            response = response && typeof response === 'object' ? response : null;
            response = response && response.hasOwnProperty('result') ? response.result : response;
        }
        if (typeof response === 'string') {
            response = response.replace(/^<pre>/, '').replace(/<\/pre>$/, '');
            try {
                response = jQuery.parseJSON(response);
            } catch (err) {
                response = null;
            }
        }

        if (req && response) {
            if (filename) {
                response.response.__filenames = typeof filename === 'string' ? [filename] : [];
            }
            req.__urls.push(req.url);
            req.__methods.push(req.type);
            req.__responses.push(response.response);
            req.__paramses.push({ count: response.count, startIndex: response.startIndex, nextIndex: response.nextIndex || undefined, total: response.total || undefined });

            var args = [req, req.__eventname, req.__params, req.__responses, req.__paramses, null, 'success'];
            requestSuccess.apply(this, args);
            removeRequestById(req.__id);
            if (uploaders.hasOwnProperty(reqid)) {
                delete uploaders[reqid];
            }
        }
    }

    function requestsObserver() {
        if (requests.length > 0 && (requests[0].__processing === false || requests[0].__isasync === true)) {
            var batchrequests = [];
            if (useBatch === true && requests.length > 1 && requests[0].__issimple === true) {
                for (var i = 0, n = requests.length; i < n; i++) {
                    if (requests[i].__issimple === true && requests[i].__processing === false) {
                        batchrequests.push(requests[i]);
                    }
                }
            }
            if (batchrequests.length > 1) {
                sendRequests(batchrequests);
            } else {
                for (var a = 0, b = requests.length; a < b; a++) {
                    if (requests[a].__processing === false) {
                        sendRequest(requests[a]);
                        break;
                    }
                }
            }
        }
        lastTimeCall = new Date();
    }

    function createRequest(eventname, params, type, url, data, options) {
        params = params || {};
        type = typeof type === 'string' ? type.toLowerCase() : 'get';

        if (data && typeof data === 'object') {
            for (var fld in data) {
                if (data.hasOwnProperty(fld)) {
                    if (data[fld] instanceof Date) {
                        data[fld] = factory.serializeTimestamp(data[fld]);
                    }
                }
            }
        }

        if (options && typeof options === 'object' && options.hasOwnProperty('filter')) {
            var filter = options.filter;
            for (var fld in filter) {
                if (filter.hasOwnProperty(fld)) {
                    if (filter[fld] instanceof Date) {
                        filter[fld] = factory.serializeTimestamp(filter[fld], true);
                    }
                }
            }
        }

        url = getUrlByFilter(type, url, params, data, options);

        var
          id = getUniqueId({}),
          beforeCallback = null,
          afterCallback = null,
          errorCallback = null,
          max_request_attempts = maxRequestAttempts,
          successCallback = typeof options === 'function' ? options : null,
          is_single = false;
        if (options && typeof options === 'object') {
            beforeCallback = options.hasOwnProperty('before') && typeof options.before === 'function' ? options.before : beforeCallback;
            afterCallback = options.hasOwnProperty('after') && typeof options.after === 'function' ? options.after : afterCallback;
            errorCallback = options.hasOwnProperty('error') && typeof options.error === 'function' ? options.error : errorCallback;
            successCallback = options.hasOwnProperty('success') && typeof options.success === 'function' ? options.success : successCallback;
            if (options.hasOwnProperty('max_request_attempts'))
                max_request_attempts = options.max_request_attempts;
            if (options.hasOwnProperty('is_single')) {
                is_single = options.is_single;
            }
        }

        url = typeof url === 'string' ? getUrl(url) : url;
        if (isArray(url)) {
            for (var i = 0, n = url.length; i < n; i++) {
                url[i] = getUrl(url[i]);
            }
        }

        return {
            __id: id,
            __errors: [],
            __errcount: 0,
            __totcount: 0,
            __attcount: 0,
            __eventname: eventname || '',
            __processing: false,
            __params: params,
            __once: params.hasOwnProperty('__once') ? params.__once === true : false,
            __beforecallback: beforeCallback,
            __aftercallback: afterCallback,
            __errorcallback: errorCallback,
            __successcallback: successCallback,
            __isauth: eventname === 'authentication',
            __issettings: eventname === 'settings',
            __isme: eventname === 'me',
            __issimple: eventname !== 'authentication' && eventname !== 'settings' && eventname !== 'me' && type === 'get' && typeof url === 'string' && is_single === false,
            __urls: [],
            __methods: [],
            __paramses: [],
            __responses: [],
            __singleresponse: typeof url === 'string',
            __uploader: null,
            __max_request_attempts: max_request_attempts,
            __isasync: options && options.hasOwnProperty('async') && typeof options.async === "boolean" ? options.async : false,
            jsonp: false,
            async: true,
            dataType: 'json',
            processData: processedDateType(data),
            // contentType : contentType,
            cache: true,
            converters: { '* text': false, 'text script': false, 'text html': false, 'text json': false, 'text xml': false },
            url: typeof url === 'string' ? url : isArray(url) ? url[0] : null,
            batch: typeof url === 'object' && isArray(url) && url.length > 1 ? url.slice(1) : [],
            type: type,
            data: data,
            timeout: requestTimeout,
            complete: requestComplete
        };
    }

    function createBatchrequest(eventname, params, type, url, data, options, batchrequests, localJointRequests) {
        params = params || {};

        var
          id = getUniqueId({}),
          beforeCallback = null,
          afterCallback = null,
          errorCallback = null,
          max_request_attempts = maxRequestAttempts,
          successCallback = typeof options === 'function' ? options : null;
        if (options && typeof options === 'object') {
            beforeCallback = options.hasOwnProperty('before') && typeof options.before === 'function' ? options.before : beforeCallback;
            afterCallback = options.hasOwnProperty('after') && typeof options.after === 'function' ? options.after : afterCallback;
            errorCallback = options.hasOwnProperty('error') && typeof options.error === 'function' ? options.error : errorCallback;
            successCallback = options.hasOwnProperty('success') && typeof options.success === 'function' ? options.success : successCallback;
            if (options.hasOwnProperty('max_request_attempts'))
                max_request_attempts = options.max_request_attempts;
        }

        url = typeof url === 'string' ? getUrl(url) : url;

        return {
            __id: id,
            __errors: [],
            __errcount: 0,
            __totcount: 0,
            __attcount: 0,
            __eventname: eventname || '',
            __processing: false,
            __params: params,
            __beforecallback: beforeCallback,
            __aftercallback: afterCallback,
            __errorcallback: errorCallback,
            __successcallback: successCallback,
            __isbatch: true,
            __urls: [],
            __methods: [],
            __paramses: [],
            __responses: [],
            __singleresponse: false,
            __uploader: null,
            __batchrequests: batchrequests && isArray(batchrequests) ? batchrequests : null,
            __jointrequests: localJointRequests && typeof localJointRequests === "object" && isArray(localJointRequests) ? localJointRequests : null,
            __max_request_attempts: max_request_attempts,
            jsonp: false,
            async: true,
            dataType: 'json',
            // contentType : contentType,
            cache: true,
            converters: { '* text': false, 'text script': false, 'text html': false, 'text json': false, 'text xml': false },
            url: url,
            type: type,
            data: data,
            timeout: requestTimeout,
            complete: batchrequestComplete
        };
    }

    function sendAjaxRequest(req) {
        //console.timeEnd("test");

        req.__processing = true;
        req.url += (req.url.indexOf('?') === -1 ? '?' : '&') + cmdRequestId + '=' + req.__id;
        jQuery.ajax(req);
    }

    function sendRequests(reqs) {
        var reqsInd = reqs.length;
        while (reqsInd--) {
            if (!checkRequest(reqs[reqsInd])) {
                break;
            }
        }
        if (reqsInd === -1) {
            var
              req = null,
              batch = [],
              localJointRequests = [],
              max_request_attempts = maxRequestAttempts;
            for (var i = 0, n = reqs.length; i < n; i++) {
                req = reqs[i];
                req.__attcount++;
                req.__processing = true;
                req.url += (req.url.indexOf('?') === -1 ? '?' : '&') + cmdRequestId + '=' + req.__id;
                batch.push({ Name: req.__id, RelativeUrl: req.url, RelativeMethod: req.type });
                localJointRequests.push(req);
                if (req.__max_request_attempts < max_request_attempts)
                    max_request_attempts = req.__max_request_attempts;
            }
            var batchreq = createBatchrequest(null, null, 'post', 'batch.json', { batch: jQuery.toJSON(batch) }, { max_request_attempts: max_request_attempts }, batch, localJointRequests);

            var wasadded = false;
            for (var i = 0, n = requests.length; i < n; i++) {
                if (requests[i].issimple === true) {
                    requests.splice(i, 0, batchreq);
                    wasadded = true;
                    break;
                }
            }
            if (wasadded === false) {
                requests.push(batchreq);
            }
            batchreq.__attcount++;
            sendAjaxRequest(batchreq);
        }
    }

    function sendRequest(req) {
        if (checkRequest(req)) {
            req.__attcount++;
            sendAjaxRequest(req);
        }
    }

    function joint() {
        jointStart = new Date().getTime();
        delete jointRequests;
        jointRequests = [];
    }

    function start(params, options) {
        var req = null;

        jointStart = 0;
        req = createBatchrequest('', params, 'post', 'batch.json', { batch: jQuery.toJSON(jointRequests) }, options, jointRequests);

        if (canAddRequest(req, requests)) {
            if (typeof req.__beforecallback === 'function') {
                req.__beforecallback(req.__params);
            }
            requests.push(req);
        }
    }

    function addRequest(eventname, params, type, url, data, options) {
        var req = null;

        var currentDate = new Date();
        if (currentDate - lastTimeCall > 3 * observerTimeout) {
            clearInterval(observerHandler);
            observerHandler = setInterval(requestsObserver, observerTimeout);
        }

        data = factory.fixData(data);
        if (jointStart > 0 && (new Date().getTime() - jointStart < jointLimit)) {
            req = createRequest(eventname, params, type, url, data, options);
            req.url += (req.url.indexOf('?') === -1 ? '?' : '&') + cmdRequestId + '=' + req.__id;
            jointRequests.push({ Name: req.__id, method: req.type, RelativeUrl: req.url, RelativeMethod: req.type });
            return jointRequests;
        }

        if (isArray(url)) {
            var
              batch = [],
              localJointRequests = [];
            for (var i = 0, n = url.length; i < n; i++) {
                req = createRequest(eventname, params, type, url[i], data, options);
                req.url += (req.url.indexOf('?') === -1 ? '?' : '&') + cmdRequestId + '=' + req.__id;
                batch.push({ Name: req.__id, method: req.type, RelativeUrl: req.url, RelativeMethod: req.type });
                localJointRequests.push(req);
            }
            req = createBatchrequest(eventname, params, 'post', 'batch.json', { batch: jQuery.toJSON(batch) }, options, batch, localJointRequests);
        } else {
            req = createRequest(eventname, params, type, url, data, options);
        }

        if (canAddRequest(req, requests)) {
            if (req.hasOwnProperty("__jointrequests") && req.__jointrequests) {
                for (var i = 0, n = req.__jointrequests.length; i < n; i++) {
                    var jointrequest = req.__jointrequests[i];
                    if (typeof jointrequest.__beforecallback === "function") {
                        jointrequest.__beforecallback(jointrequest.__params);
                    }
                }
            }
            if (typeof req.__beforecallback === 'function') {
                req.__beforecallback(req.__params);
            }
            requests.push(req);
        }
    }

    function firstRequest(eventname, params, type, url, data, options) {
        var req = createRequest(eventname, params, type, url, data, options);
        if (canAddRequest(req, requests)) {
            if (typeof req.__beforecallback === 'function') {
                req.__beforecallback(req.__params);
            }
            requests.unshift(req);
        }
    }

    var createUploader = function (eventname, params, type, url, data, options) {
        var req = createRequest(eventname, params, type, url, data, options);

        data = data || {};
        data.action = req.url;
        data.onSubmit = (function (reqid) { return function (file, extension) { uploadSubmit(reqid, file, extension) } })(req.__id);
        data.onComplete = (function (reqid) { return function (file, response) { uploadComplete(reqid, file, response) } })(req.__id);

        uploaders[req.__id] = req;

        return (req.__uploader = new AjaxUpload(data.buttonId, data));
    };

    function onGetMe(params, obj) {
        completeInit |= 1;
        myProfile = obj;

        if (completeInit === 3) {
            exec('completeinit', this, [myProfile, portalSettings]);
        }
    }

    function onGetSettings(params, obj) {
        completeInit |= 2;
        portalSettings = obj;

        if (completeInit === 3) {
            exec('completeinit', this, [myProfile, portalSettings]);
        }
    }

    function onGetAuthentication(params, obj) {
        //settings({__once : true});
        me({ __once: true });
    }

    function onUnauthenticatedRequest(params) {
        //location.href = '/';
    }

    var init = function (apipath, needMe) {
        if (isInit === true) {
            return undefined;
        }
        isInit = true;

        apiPath = typeof apipath === 'string' && apipath.length > 0 ? apipath : apiPath;
        if (apiPath) {
            apiPath = apiPath.charAt(apiPath.length - 1) === '/' ? apiPath : apiPath + '/';
        }

        observerHandler = setInterval(requestsObserver, observerTimeout);

        bind('unauthenticated', onUnauthenticatedRequest);

        //settings({__once : true});
        if (needMe) {
            me({ __once: true });
        }
    };

    var bind = function (eventname, handler) {
        if (!customEvents.hasOwnProperty(eventname)) {
            customEvents[eventname] = [];
        }
        if (typeof handler === 'function') {
            customEvents[eventname].push(handler);
        }
    };

    var exec = function (eventname, obj, args) {
        if (customEvents.hasOwnProperty(eventname)) {
            var handlers = customEvents[eventname];
            obj = obj || window;
            args = args || [];
            for (var i = 0, n = handlers.length; i < n; i++) {
                handlers[i].apply(obj, args);
            }
            return undefined;
        }
        if (customEvents.hasOwnProperty('event')) {
            var handlers = customEvents['event'];
            obj = obj || window;

            for (var i = 0, n = handlers.length; i < n; i++) {
                handlers[i].apply(obj, [eventname, obj, args]);
            }
        }
    };

    var me = function (params, callback) {
        params = params || {};

        firstRequest(
          'me',
          params,
          'get',
          'people/@self.json',
          null,
          callback
        );
    };

    var settings = function (params, callback) {
        params = params || {};

        firstRequest(
          'settings',
          params,
          'get',
          'settings.json',
          null,
          callback
        );
    };

    var test = function () {
        //
    };

    return {
        test: test,
        init: init,
        bind: bind,
        exec: exec,
        joint: joint,
        start: start,
        request: addRequest,
        uploader: createUploader
    };
})(ServiceFactory);
