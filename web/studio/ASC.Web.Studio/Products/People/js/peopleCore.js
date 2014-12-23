/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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