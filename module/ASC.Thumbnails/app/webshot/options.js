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
