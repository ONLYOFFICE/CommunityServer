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


jQuery.extend({
    /**
    * Returns get parameters.
    *
    * If the desired param does not exist, null will be returned
    *
    * @example value = jq.getAnchorParam("paramName", url);
    */

    getAnchorParam: function(paramName, url) {
        var url = url || window.location.href.toLowerCase();
        var regex = new RegExp("[#&]" + paramName + "=([^&]*)");
        var results = regex.exec('#' + url);
        if (results == null) {
            return "";
        } else {
            return results[1];
        }
    },
    hasParam: function(paramName, url) {
        var regex = new RegExp('(\\#|&|^)' + paramName + '=', 'g'); //matches `#param=` or `&param=` or `param=`
        return regex.test(url);
    },
    removeParam: function(paramName, url) {
        if (url === '')
            return '';

        var regex = new RegExp("(\\#|&|^)" + paramName + "=([^&]*)");
        return url.replace(regex, '');
    },
    addParam: function(paramsList, name, value) {
        if (paramsList.length) paramsList += '&';
        paramsList = paramsList + name + '=' + value;
        return paramsList;
    },
    changeParamValue: function(paramsList, name, value) {
        var symbols = ['\\#', '&', '^'];
        if (jq.hasParam(name, paramsList)) {
            for (var i = 0; i < symbols.length; i++) {

                var regex = new RegExp(symbols[i] + name + "[=][0-9a-z\-]*");
                var s = symbols[i] == '^' ? '' : symbols[i];
                paramsList = paramsList.replace(regex, s + name + '=' + value);


            }
            return paramsList;
        } else {
            return jq.addParam(paramsList, name, value);
        }
    },
    clearUrl: function() {
        var url = window.location.href;
        if (url.indexOf("#") == -1)
            return url;
        return url.split("#")[0];
    },

    mergeAnchors: function mergeAnchors(a1, a2) {
        var a = {};
        for (var fld in a1) {
            if (a1.hasOwnProperty(fld)) {
                a[fld] = a1[fld];
            }
        }

        for (var fld in a2) {
            if (a2.hasOwnProperty(fld)) {
                a[fld] = a2[fld];
            }
        }
        return a;
    },

    anchorToObject: function (s) {
        s = s.charAt(0) === '#' ? s.substring(1) : s;

        var o = {};
        s = s.split('&');
        for (var i = 0, n = s.length; i < n; i++) {
            s[i] = s[i].split('=');
            if (s[i].length === 2) {
                o[s[i][0]] = s[i][1];
            }
        }
        return o;
    },
    objectToAnchor: function (o) {
        var s = [];
        for (var fld in o) {
            if (o.hasOwnProperty(fld)) {
                s.push(fld + '=' + o[fld]);
            }
        }
        return s.join('&');
    },
    isEqualAnchors: function (a1, a2) {
        for (var fld in a1) {
            if (a1.hasOwnProperty(fld) && !a2.hasOwnProperty(fld) || a1[fld] !== a2[fld]) {
                return false;
            }
        }

        for (var fld in a2) {
            if (a2.hasOwnProperty(fld) && !a1.hasOwnProperty(fld) || a2[fld] !== a1[fld]) {
                return false;
            }
        }
        return true;
    }
});

String.prototype.htmlEncode = function () {
    return jQuery(document.createElement('div')).text(this.toString()).html();
};

if (typeof(ASC) == 'undefined')
    ASC = {};
    
ASC.People = ASC.People || {};
ASC.People.model = ASC.People.model || {};

ASC.People.eventSource = {
  document: 'document',
  filter: 'filter',
  anchor: 'anchor',
  navigator: 'navigator'
};

ASC.People.model.groups = ASC.People.model.groups || [];
ASC.People.model.profiles = ASC.People.model.profiles || [];

ASC.People.eventHandler = {
  create: function (s, handler) {
    handler.prototype = {
      source: s,
      handle: function (type, data) {
        if (this.source && this.source === data.source) {
          return;
        } else if (typeof this[type] === 'function') {
          this[type](data);
        }
      }
    };
    return new handler();
  }
};

ASC.People.controller = {
  registryHandler: function (handler) {
    this.subscribers[this.subscribers.length] = handler;
  },

  publish: function (eventSource, type, data) {
    for (var i = 0, n = this.subscribers.length; i < n; i++) {
      if (typeof this.subscribers[i].handle === 'function') {
        this.subscribers[i].handle(type, data);
      }
    }
  },

  make: function(controller, handlers) {
    for (var i in this) {
      controller.prototype[i] = this[i];
    }
    controller.prototype.subscribers = [];

    if (typeof handlers === 'object' && typeof handlers.length !== 'undefined') {
      for (var i = 0, n = handlers.length; i < n; i++) {
        controller.prototype.subscribers.push(handlers[i]);
      }
    } else if (typeof views === 'object') {
      controller.prototype.subscribers.push(handlers);
    }

    return new controller();
  }
};