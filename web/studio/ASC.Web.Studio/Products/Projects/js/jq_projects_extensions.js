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


if (!String.prototype.trim) {
    String.prototype.trim = function() {
        return jq.trim(arguments.length ? arguments[0] : "");
    };
}

if (!Object.assign) {
    Object.defineProperty(Object, 'assign', {
        enumerable: false,
        configurable: true,
        writable: true,
        value: function (target, firstSource) {
            'use strict';
            if (target === undefined || target === null) {
                throw new TypeError('Cannot convert first argument to object');
            }

            var to = Object(target);
            for (var i = 1; i < arguments.length; i++) {
                var nextSource = arguments[i];
                if (nextSource === undefined || nextSource === null) {
                    continue;
                }

                var keysArray = Object.keys(Object(nextSource));
                for (var nextIndex = 0, len = keysArray.length; nextIndex < len; nextIndex++) {
                    var nextKey = keysArray[nextIndex];
                    var desc = Object.getOwnPropertyDescriptor(nextSource, nextKey);
                    if (desc !== undefined && desc.enumerable) {
                        to[nextKey] = nextSource[nextKey];
                    }
                }
            }
            return to;
        }
    });
}

/*******************************************************************************
JQuery Extension
*******************************************************************************/

jQuery.extend({
    getAnchorParam: function(paramName, url) {
        var regex = new RegExp("[#&]" + paramName + "=([^&]*)");
        var results = regex.exec('#' + url);
        if (results == null)
            return "";
        else
            return results[1];
    },
    hasParam: function(paramName, url) {
        var regex = new RegExp('(\\#|&|^)' + paramName + '=', 'g'); //matches `#param=` or `&param=` or `param=`
        return regex.test(url);
    },
    removeParam: function(paramName, url) {
        var regex = new RegExp("[#&]" + paramName + "=([^&]*)");
        return url.replace(regex, '');
    },
    addParam: function(paramsList, name, value) {
        if (paramsList.length) paramsList += '&';
        paramsList = paramsList + name + '=' + value;
        return paramsList;
    },
    changeParamValue: function(paramsList, name, value) {
        if (jq.hasParam(name, paramsList)) {
            var regex = new RegExp(name + "[=][0-9a-z\-]*");
            return paramsList.replace(regex, name + '=' + value);
        } else {
            return jq.addParam(paramsList, name, value);
        }
    },
    format: function jQuery_dotnet_string_format(text) {
        //check if there are two arguments in the arguments list
        if (arguments.length <= 1) {
            //if there are not 2 or more arguments there's nothing to replace
            //just return the text
            return text;
        }
        //decrement to move to the second argument in the array
        var tokenCount = arguments.length - 2;
        for (var token = 0; token <= tokenCount; ++token) {
            //iterate through the tokens and replace their placeholders from the text in order
            text = text.replace(new RegExp("\\{" + token + "\\}", "gi"), arguments[token + 1]);
        }
        return text;
    },
    timeFormat: function (hours) { // convert time to format h:mm
        var parsed = parseFloat(hours);
        var h = Math.floor(parsed);
        var m = Math.floor((parsed - h) * 60);
        var s = Math.round(((parsed  - h) * 60 - m) * 60);
        if (s < 10) {
            s = '0' + s;
        } else {
            if (s == 60) {
                s = "00";
                m = m + 1;
            }
        }

        if (m < 10) {
            m = '0' + m;
        } else {
            if (m == 60) {
                m = "00";
                h = h + 1;
            }
        }
        return h + ':' + m + ':' + s;
    }
});


jQuery.fn.swap = function(b) {
    b = jQuery(b)[0];
    var a = this[0],
        a2 = a.cloneNode(true),
        b2 = b.cloneNode(true),
        stack = this;

    a.parentNode.replaceChild(b2, a);
    b.parentNode.replaceChild(a2, b);

    stack[0] = a2;
    return this.pushStack(stack);
};

if (!String.prototype.contains) {
    String.prototype.contains = function () {
        return String.prototype.indexOf.apply(this, arguments) !== -1;
    };
}

if (!Array.prototype.findIndex) {
    Array.prototype.findIndex = function (predicate) {
        if (this == null) {
            throw new TypeError('Array.prototype.findIndex called on null or undefined');
        }
        if (typeof predicate !== 'function') {
            throw new TypeError('predicate must be a function');
        }
        var list = Object(this);
        var length = list.length >>> 0;
        var thisArg = arguments[1];
        var value;

        for (var i = 0; i < length; i++) {
            value = list[i];
            if (predicate.call(thisArg, value, i, list)) {
                return i;
            }
        }
        return -1;
    };
}