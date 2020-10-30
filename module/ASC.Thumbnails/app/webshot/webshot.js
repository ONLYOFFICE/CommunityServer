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


var url = require('url')
  , fs = require('graceful-fs')
  , tmp = require('tmp')
  , stream = require('stream')
  , crossSpawn = require('cross-spawn')
  , optUtils = require('./options')
  , phantomScript = __dirname + '/webshot.phantom.js'
  , extensions = ['jpeg', 'jpg', 'png', 'pdf']
  , siteTypes = ['url', 'html', 'file'];

module.exports = function() {

  // Process arguments
  var args = Array.prototype.slice.call(arguments, 0);
  var cb = null;
  var options = {};
  var path = null;

  /*
    Possible valid arguments:
    SITE, <OPTIONS>, CB
    SITE, <OPTIONS>
    SITE, PATH, <OPTIONS>, CB
  */
  var site = args.shift();

  /*
    <OPTIONS>, CB
    <OPTIONS>
    PATH, <OPTIONS>, CB
  */
  var last = args[args.length - 1];

  if (Object.prototype.toString.call(last) == '[object Function]') {
    cb = args.pop();
  }

  /*
    <OPTIONS>
    <OPTIONS>
    PATH, <OPTIONS>
  */
  switch (args.length) {

    case 1:
      var arg = args.pop();

      if (toString.call(arg) === '[object String]') {
        path = arg;
      } else {
        options = arg;
      }
    break;

    case 2:
      path = args.shift();
      options = args.shift();
    break;
  }

  var streaming = !path;
  var defaults = optUtils.mergeObjects(optUtils.caller, optUtils.phantom);

  // Apply the compiled phantomjs path only if it compiled successfully
  try {
    defaults.phantomPath = require('phantomjs-prebuilt').path;
  } catch (ex) {}

  options = processOptions(options, defaults);

  // Check that a valid fileType was given for the output image
  var extension = (path)
    ? path.substring(~(~path.lastIndexOf('.') || ~path.length) + 1)
    : options.streamType;

  if (!~extensions.indexOf(extension.toLowerCase())) {
    return cb(
      new Error('All files must end with one of the following extensions: '
        + extensions.join(', ')));
  }

  // Check that a valid siteType was provided
  if (!~siteTypes.indexOf(options.siteType)) {
    var err = new Error(args.siteType + ' is not a valid sitetype.');
    if (cb) return cb(err);
    throw err;
  }

  // Add protocol to the site url if not present
  if (options.siteType === 'url') {
    site = url.parse(site).protocol ? site : 'http://' + site;
  }

  // Remove the given file if it already exists, then call phantom
  var spawn = function() {
    if (options.siteType === 'html') {
      var obj = tmp.fileSync();
      var tmpPath = obj.name;
      fs.writeSync(obj.fd, site, null, 'utf-8');
      fs.close(obj.fd);
      options.siteType = 'file';
      site = tmpPath;
      return spawn();
    } else {
      return spawnPhantom(site, path, streaming, options, cb);
    }
  };

  if (path) {
    fs.exists(path, function(exists) {
      if (exists) {
        fs.unlink(path, function(err) {
          if (err) return cb(err);
          return spawn();
        });
      } else {
        return spawn();
      }
    });
  } else {
    return spawn();
  }
};


/*
 * Process the options object into the values to be exposed to phantom
 *
 * @param (Object) options
 * @param (Object) defaults
 * @return (Object)
 */
function processOptions(options, defaults) {

  // Alias 'screenSize' to 'windowSize'
  options.windowSize = options.windowSize || options.screenSize;

  // Alias 'userAgent' to 'settings.userAgent'
  if (options.userAgent) {
    options.settings = options.settings || {};
    options.settings.userAgent = options.userAgent;
  }

  // Alias 'script' to 'onLoadFinished'
  if (options.script) {
    options.onLoadFinished = options.onLoadFinished || options.script;
  }

  // Fill in defaults for undefined options
  var withDefaults = optUtils.mergeObjects(options, defaults);

  // Convert function options to strings for later JSON serialization
  optUtils.phantomCallback.forEach(function(optionName) {
    var fnArg = withDefaults[optionName];

    if (fnArg) {
      if (toString.call(fnArg) === '[object Function]') {
        withDefaults[optionName] = {
          fn: fnArg.toString()
        , context: {}
        };
      } else {
        fnArg.fn = fnArg.fn.toString();
      }
    }
  });

  return withDefaults;
}


/*
 * Spawn a phantom instance to take the screenshot
 *
 * @param (String) site
 * @param (String) path
 * @param (Boolean) streaming
 * @param (Object) options
 * @param (Function) cb
 */
function spawnPhantom(site, path, streaming, options, cb) {

  // Filter out options that shouldn't be passed to the phantom process
  var filteredOptions = optUtils.filterObject(options,
    Object.keys(optUtils.phantom)
      .concat(optUtils.phantomPage)
      .concat(optUtils.phantomCallback));

  filteredOptions.streaming = streaming;

  var phantomArgs = [phantomScript, JSON.stringify(filteredOptions), site, path];

  if (options.phantomConfig) {
    phantomArgs = Object.keys(options.phantomConfig).map(function (key) {
      return '--' + key + '=' + options.phantomConfig[key];
    }).concat(phantomArgs);
  }

  var phantomProc = crossSpawn.spawn(options.phantomPath, phantomArgs);

  // This variable will contain our timeout ID.
  var timeoutID = null;

  // Whether or not we've called our callback already.
  var calledCallback = false;

  // Only set the timer if the timeout has been specified (by default it's not)
  if (options.timeout) {
    timeoutID = setTimeout(function() {

      // The phantomjs process didn't exit in time.
      // Double-check we didn't already call the callback already as that would
      // happen when the process has already exited. Sending a SIGKILL to a PID
      // that might be handed out to another process could be potentially very
      // dangerous.
      if (!calledCallback) {
        calledCallback = true;

        // Send the kill signal
        phantomProc.kill('SIGKILL');

        // Call our callback.
        var err = new Error('PhantomJS did not respond within the given ' +
                            'timeout setting.');
        if (cb) return cb(err);
        s.emit('error', err);
      }
    }, options.timeout);
  }

  if (!streaming) {
    phantomProc.stderr.on('data', function(data) {
      if (options.errorIfJSException) {
        calledCallback = true;
        clearTimeout(timeoutID);
        cb(new Error('' + data))
      }
    });

    phantomProc.on('exit', function(code) {
      if (!calledCallback) {
        calledCallback = true;

        // No need to run the timeout anymore.
        clearTimeout(timeoutID);
        cb(code
          ? new Error('PhantomJS exited with return value ' + code)
          : null);
      }
    });
  } else {
    var s = new stream.Stream();
    s.readable = true;

    phantomProc.stdout.on('data', function(data) {
      clearTimeout(timeoutID);
      s.emit('data', new Buffer(''+data, 'base64'));
    });

    phantomProc.stderr.on('data', function(data) {
      if (options.errorIfJSException) {
        s.emit('error', ''+data);
      }
    });

    phantomProc.on('exit', function() {
      s.emit('end');
    });

    if (cb) {
      cb(null, s);
    } else {
      return s;
    }
  }
}
