/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


// Options for the phantom script
exports.phantom = {
  windowSize: {
    width: 1024
  , height: 768
  }
, shotSize: {
    width: 'window'
  , height: 'window'
  }
, shotOffset: {
    left: 0
  , right: 0
  , top: 0
  , bottom: 0
  }
, defaultWhiteBackground: false
, customCSS: ''
, takeShotOnCallback: false
, streamType: 'png'
, siteType: 'url'
, renderDelay: 0
, quality: 75
, errorIfStatusIsNot200: false
, errorIfJSException: false
, cookies: []
, captureSelector: false
, zoomFactor: 1
};

// Options that are just passed to the phantom page object
exports.phantomPage = ['paperSize', 'customHeaders', 'settings', 'zoomFactor'];

// Options that are callbacks for various phantom events
exports.phantomCallback = ['onAlert', 'onCallback', 'onClosing', 'onConfirm',
  'onConsoleMessage', 'onError', 'onFilePicker', 'onInitialized',
  'onLoadFinished', 'onLoadStarted', 'onNavigationRequested', 'onPageCreated',
  'onPrompt', 'onResourceRequested', 'onResourceReceived',
  'onResourceTimeout', 'onResourceError', 'onUrlChanged'];

// Options that are used in the calling node script
exports.caller = {
  phantomPath: 'phantomjs'
, phantomConfig: ''
, timeout: 0
};


/*
 * Merge the two objects, using the value from `a` when the objects conflict
 *
 * @param (Object) a
 * @param (Object) b
 * @return (Object)
 */
exports.mergeObjects = function mergeObjects(a, b) {
  var merged = {};

  Object.keys(a).forEach(function(key) {
    merged[key] = toString.call(a[key]) === '[object Object]'
      ? mergeObjects(a[key], b[key] || {})
      : a[key] || b[key];
  });

  Object.keys(b).forEach(function(key) {
    if (merged.hasOwnProperty(key)) return;
    merged[key] = b[key];
  });

  return merged;
};


/*
 * Filter the object `obj` to contain only the given keys
 *
 * @param (Object) obj
 * @param (Array) keys
 * @return (Object)
 */
exports.filterObject = function filterObject(obj, keys) {
  var filtered = {};

  keys.forEach(function(key) {
    if (obj[key]) filtered[key] = obj[key];
  });

  return filtered;
};
