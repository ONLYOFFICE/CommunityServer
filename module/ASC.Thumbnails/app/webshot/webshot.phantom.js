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


var system = require('system')
  , page = require('webpage').create()
  , fs = require('fs')
  , optUtils = require('./options');

// Read in arguments
var options = JSON.parse(system.args[1]);
var site = system.args[2];
var path = system.args[3];
var streaming = options.streaming;

page.viewportSize = {
  width: options.windowSize.width
, height: options.windowSize.height
};

// Capture JS errors and write them to stderr
page.onError = function(msg, trace) {
  var msgStack = ['ERROR: ' + msg];

  if (trace && trace.length) {
    msgStack.push('TRACE:');
    trace.forEach(function(t) {
      msgStack.push(' -> ' + t.file + ': ' + t.line +
        (t.function ? ' (in function "' + t.function +'")' : ''));
    });
  }

  system.stderr.write(msgStack.join('\n'));
};

if (options.errorIfStatusIsNot200) {
  page.onResourceReceived = function(response) {

    // If request to the page is not 200 status, fail.
    if (response.url === site && response.status !== 200) {
      system.stderr.write('Status must be 200; is ' + response.status);
      page.close();
      phantom.exit(0);
    }
  };
}

// Handle cookies
if (Array.isArray(options.cookies)) {
  for (var i=0; i<options.cookies.length; ++i) {
    phantom.addCookie(options.cookies[i]);
  }
} else if (options.cookies === null) {
  phantom.cookiesEnabled = false;
}

// Register user-provided callbacks
optUtils.phantomCallback.forEach(function(cbName) {
  var cb = options[cbName];

  if (cbName === 'onCallback' && options.takeShotOnCallback) return;
  if (cbName === 'onLoadFinished' && !options.takeShotOnCallback) return;

  if (cb) {
    page[cbName] = buildEvaluationFn(cb.fn, cb.context);
  }
})

// Set the phantom page properties
var toOverwrite = optUtils.mergeObjects(
  optUtils.filterObject(options, optUtils.phantomPage)
, page);

optUtils.phantomPage.forEach(function(key) {
  if (toOverwrite[key]) page[key] = toOverwrite[key];
});

// The function that actually performs the screen rendering
var _takeScreenshot = function(status) {
  if (status === 'fail') {
    page.close();
    phantom.exit(1);
    return;
  }

  // Wait `options.renderDelay` seconds for the page's JS to kick in
  window.setTimeout(function () {

    // Handle customCSS option
    if (options.customCSS) {
      page.evaluate(function(customCSS) {
        var style = document.createElement('style');
        var text  = document.createTextNode(customCSS);
        style.setAttribute('type', 'text/css');
        style.appendChild(text);
        document.head.insertBefore(style, document.head.firstChild);
      }, options.customCSS);
    }

    if (options.captureSelector) {

      // Handle captureSelector option
      page.clipRect = page.evaluate(function(selector, zoomFactor) {
        try {
          var selectorClipRect =
            document.querySelector(selector).getBoundingClientRect();

          return {
              top: selectorClipRect.top * zoomFactor
            , left: selectorClipRect.left * zoomFactor
            , width: selectorClipRect.width * zoomFactor
            , height: selectorClipRect.height * zoomFactor
          };
        } catch (e) {
          throw new Error("Unable to fetch bounds for element " + selector);
        }
      }, options.captureSelector, options.zoomFactor);
    } else {

      //Set the rectangle of the page to render
      page.clipRect = {
        top: options.shotOffset.top
      , left: options.shotOffset.left
      , width: pixelCount(page, 'width', options.shotSize.width)
          - options.shotOffset.right
      , height: pixelCount(page, 'height', options.shotSize.height)
          - options.shotOffset.bottom
      };
    }

    // Handle defaultWhiteBackgroud option
    if (options.defaultWhiteBackground) {
      page.evaluate(function() {
        var style = document.createElement('style');
        var text  = document.createTextNode('body { background: #fff }');
        style.setAttribute('type', 'text/css');
        style.appendChild(text);
        document.head.insertBefore(style, document.head.firstChild);
      });
    }

    // Render, clean up, and exit
    if (!streaming) {
      page.render(path, {quality: options.quality});
    } else {
      console.log(page.renderBase64(options.streamType));
    }

    page.close();
    phantom.exit(0);
  }, options.renderDelay);
}

// Avoid overwriting the user-provided onPageLoaded or onCallback options
var takeScreenshot;

if (options.onCallback && options.takeShotOnCallback) {
  takeScreenshot = function(data) {
    buildEvaluationFn(
      options.onCallback.fn
    , options.onCallback.context)(data);

    if (data == 'takeShot') {
      _takeScreenshot();
    }
  };
} else if (options.onLoadFinished && !options.takeShotOnCallback) {
  takeScreenshot = function(status) {
    buildEvaluationFn(
      options.onLoadFinished.fn
    , options.onLoadFinished.context)(status);
    _takeScreenshot(status);
  };
} else {
  takeScreenshot = _takeScreenshot;
}

// Kick off the page loading
if (options.siteType == 'url') {
  if (options.takeShotOnCallback) {
    page.onCallback = takeScreenshot;
    page.open(site);
  } else {
    page.open(site, takeScreenshot);
  }
} else {

  try {
    var f = fs.open(site, 'r');
    var pageContent = f.read();
    f.close();

    page[options.takeShotOnCallback
      ? 'onCallback'
      : 'onLoadFinished'] = takeScreenshot;

    // Set content to be provided HTML
    page.setContent(pageContent, '');

    // Issue reload to pull down any CSS or JS
    page.reload();
  } catch (e) {
    console.error(e);
    phantom.exit(1);
  }
}


/*
 * Given a shotSize dimension, return the actual number of pixels in the
 * dimension that phantom should render.
 *
 * @param (Object) page
 * @param (String) dimension
 * @param (String or Number) value
 */
function pixelCount(page, dimension, value) {

  // Determine the page's dimensions
  var pageDimensions = page.evaluate(function(zoomFactor) {
    var body = document.body || {};
    var documentElement = document.documentElement || {};
    return {
      width: Math.max(
        body.offsetWidth
      , body.scrollWidth
      , documentElement.clientWidth
      , documentElement.scrollWidth
      , documentElement.offsetWidth
      ) * zoomFactor
    , height: Math.max(
        body.offsetHeight
      , body.scrollHeight
      , documentElement.clientHeight
      , documentElement.scrollHeight
      , documentElement.offsetHeight
      ) * zoomFactor
    };
  }, options.zoomFactor || 1);

  var x = {
    window: page.viewportSize[dimension]
  , all: pageDimensions[dimension]
  }[value] || value;

  return x;
}


/*
 * Bind the function `fn` to the context `context` in a serializable manner.
 * A tiny bit of a hack.
 *
 * @param (String) fn
 * @param (Object) context
 */
function buildEvaluationFn(fn, context) {
  return function() {
    var args = Array.prototype.slice.call(arguments);
    page.evaluate(function(fn, context, args) {
      eval('(' + fn + ')').apply(context, args);
    }, fn, context, args);
  };
}
