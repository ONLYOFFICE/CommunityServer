/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/


(function () {
  var inArray = function (elem, array) {
	  for (var i = 0, n = array.length; i < n; i++) {
		  if (array[i] === elem) {
			  return i;
			}
    }
	  return -1;
  };

  var hasClass = function (o, className) {
    if (typeof o === 'string'){
	    o = document.getElementById(o);
	  }
    if (!o) {
      return false;
    }
    if (typeof className !== 'string' || !className) {
      return false;
    }

    if (o.nodeType === 1) {
      var currentClassName = o.className;
      return currentClassName && currentClassName.indexOf(className) !== -1 && inArray(className, currentClassName.split(/\s+/)) !== -1;
    }
    return false;
  };

  var getElementsByClassName = (function () {
    if (typeof document.getElementsByClassName === 'function') {
      return function (o, className, tagName) {
        if (typeof o === 'string'){
	        o = document.getElementById(o);
	      }
        if (!o) {
          return [];
        }
        if (typeof tagName !== 'string') {
          return o.getElementsByClassName(className)
        }
			  var
          current = null,
          returnElements = [],
          elements = o.getElementsByClassName(className),
          nodeName = new RegExp('\\b' + tagName + '\\b', 'i');
        for (var i = 0, n = elements.length; i < n; i++) {
          current = elements[i];
          if(nodeName.test(current.nodeName)) {
            returnElements.push(current);
          }
        }
        return returnElements;
      };
    } else if (document.evaluate) {
      return function (o, className, tagName) {
        if (typeof o === 'string'){
	        o = document.getElementById(o);
	      }
        if (!o) {
          return [];
        }
        tagName = tagName || '*';
        var
          classes = className.split(' '),
          classesToCheck = '',
          xhtmlNamespace = 'http://www.w3.org/1999/xhtml',
          namespaceResolver = document.documentElement.namespaceURI === xhtmlNamespace ? xhtmlNamespace : null,
          returnElements = [],
          elements = null,
          node = null;
        for(var i = 0, n = classes.length; i < n; i++) {
          classesToCheck += '[contains(concat(" ", @class, " "), " ' + classes[i] + ' ")]';
        }
        try	{
          elements = document.evaluate(".//" + tagName + classesToCheck, o, namespaceResolver, 0, null);
        }
        catch (err) {
          elements = document.evaluate(".//" + tagName + classesToCheck, o, null, 0, null);
        }
        while ((node = elements.iterateNext())) {
          returnElements.push(node);
        }
        return returnElements;
      };
    }
    return function (o, className, tagName) {
      if (typeof o === 'string'){
	      o = document.getElementById(o);
	    }
      if (!o) {
        return [];
      }
      tagName = tagName || '*';
      var
        i = 0, j = 0, n = 0, m = 0,
        classes = className.split(' '),
        classesToCheck = [],
        elements = tagName === '*' && o.all ? o.all : o.getElementsByTagName(tagName),
        current = null,
        returnElements = [],
        match = false;
      for (i = 0, n = classes.length; i < n; i++) {
        classesToCheck.push(new RegExp('(^|\\s)' + classes[i] + '(\\s|$)'));
      }
      for (i = 0, n = elements.length; i < n; i++) {
        current = elements[i];
        match = false;
        for(j = 0, m = classesToCheck.length; j < m; j++){
					match = classesToCheck[j].test(current.className);
					if (!match) {
						break;
					}
				}
				if (match) {
					returnElements.push(current);
				}
			}
			return returnElements; 
    };
  })();

  var page = getElementsByClassName(document.body, 'ui-page-active')[0];
  if (!page) {
    return undefined;
  }

  if (hasClass(page, 'page-documents-file')) {
    var
      viewer = getElementsByClassName(page, 'file-container')[0],
      header = getElementsByClassName(page, 'ui-header')[0];

    if (viewer) {
      var screenheight = document.documentElement.clientHeight > 0 ? document.documentElement.clientHeight : document.body.clientHeight;
      viewer.style.height = screenheight - (header ? header.offsetHeight : 0) + 'px';
    }
  }
})();
