/*	CheckPlayer 1.0.2 <http://checkplayer.flensed.com/>
	Copyright (c) 2008 Kyle Simpson, Getify Solutions, Inc.
	This software is released under the MIT License <http://www.opensource.org/licenses/mit-license.php>

	====================================================================================================
	Portions of this code were extracted and/or derived from:

	SWFObject v2.1 & 2.2a8 <http://code.google.com/p/swfobject/>
	Copyright (c) 2007-2008 Geoff Stearns, Michael Williams, and Bobby van der Sluis
	This software is released under the MIT License <http://www.opensource.org/licenses/mit-license.php>
*/

(function(global){
	// frequently used variable declarations, for optimized compression
	var win = global,
		doc = global.document,
		UNDEF = "undefined",
		JSTRUE = true,
		JSFALSE = false,
		EMPTY = "",
		OBJECT = "object",
		JSFUNC = "function",
		JSSTR = "string",
		JSDIV = "div",
		JS_ONUNLOAD = "onunload",
		JSNONE = "none",
		tmp = null,
		_flensed = null,
		_chkplyr = null,
		_swfobj = null,
		flensed_js = "flensed.js",				// SHOULD NOT rename the file or change this line
		checkplayer_js = "checkplayer.js",		// ditto
		swfobject_js = "swfobject.js",			// ditto
		SETTIMEOUT = global.setTimeout,
		CLEARTIMEOUT = global.clearTimeout,
		SETINTERVAL = global.setInterval,
		CLEARINTERVAL = global.clearInterval;

	if (typeof global.flensed === UNDEF) { global.flensed = {}; }
	if (typeof global.flensed.checkplayer !== UNDEF) { return; }	// checkplayer already defined, so quit
		
	_flensed = global.flensed;
	
	SETTIMEOUT(function() {
		var base_path_known = JSFALSE,
			scriptArry = doc.getElementsByTagName("script"),
			scrlen = scriptArry.length;
		try { _flensed.base_path.toLowerCase(); base_path_known = JSTRUE; } catch(err) { _flensed.base_path = ""; }
	
		function load_script(src,type,language) {
			for (var k=0; k<scrlen; k++) {
				if (typeof scriptArry[k].src !== UNDEF) {
					if (scriptArry[k].src.indexOf(src) >= 0) { break; }  // script already loaded/loading...
				}
			}
			var scriptElem = doc.createElement("script");
			scriptElem.setAttribute("src",_flensed.base_path+src);
			if (typeof type !== UNDEF) { scriptElem.setAttribute("type",type); }
			if (typeof language !== UNDEF) { scriptElem.setAttribute("language",language); }
			doc.getElementsByTagName("head")[0].appendChild(scriptElem);
		}
		
		if ((typeof scriptArry !== UNDEF) && (scriptArry !== null)) {
			if (!base_path_known) {
				var idx=0;
				for (var k=0; k<scrlen; k++) {
					if (typeof scriptArry[k].src !== UNDEF) {
						if (((idx=scriptArry[k].src.indexOf(flensed_js)) >= 0) || ((idx=scriptArry[k].src.indexOf(checkplayer_js)) >= 0)) {
							_flensed.base_path = scriptArry[k].src.substr(0,idx);
							break;
						}
					}
				}
			}
		}
	
		try { global.swfobject.getObjectById("a"); } catch (err2) { load_script(swfobject_js,"text/javascript"); }
		try { _flensed.ua.pv.join("."); } catch (err3) { load_script(flensed_js,"text/javascript"); }
	
		function clearDependencyTimeout() { CLEARTIMEOUT(dependencyTimeout); try { win.detachEvent(JS_ONUNLOAD,arguments.callee); } catch (err) {} }
		try { win.attachEvent(JS_ONUNLOAD,clearDependencyTimeout); } catch (err4) {}
		var dependencyTimeout = SETTIMEOUT(function(){
			clearDependencyTimeout();
			try { global.swfobject.getObjectById("a"); _flensed.ua.pv.join("."); } catch (err) { throw new global.Error("CheckPlayer dependencies failed to load."); }
		},20000);	// only wait 20 secs max for swfobject and flensedCore to load
	},0);
	
	_flensed.checkplayer = function(playerVersionCheck,checkCB,autoUpdate,updateCB) {
		if (typeof _chkplyr._ins !== UNDEF) {	// checkplayer is a singleton
			if (_chkplyr._ins.ready()) { setTimeout(function(){checkCB(_chkplyr._ins);},0); }
			return _chkplyr._ins;
		}

		// Private Properties
		var	MIN_XI_VERSION = "6.0.65",
			updateInterval = [],
			notReadyInterval = null,
			updateCalled = JSFALSE,
			constructInterval = null,
			updateSWFId = null,
			updateContainerId = EMPTY,
			ready = JSFALSE,
			updateObj = null,
			swfIdArr = [],
			swfIntHash = {},
			swfQueue = [],
			bodyel = null,
	
		// Configurable Properties (via new() constructor)
			versionToCheck = null,
			checkCallback = null,
			updateCallback = null,
			updateFlash = JSFALSE,
	
		// Properties Exposed (indirectly, read-only) in Public API
			flashVersionDetected = null,
			versionCheckPassed = JSFALSE,
			updateable = JSFALSE,
			updateStatus = JSFALSE,
			holder = null;
	
		// Private Methods
		var constructor = function() {
			if ((typeof playerVersionCheck !== UNDEF) && (playerVersionCheck !== null) && (playerVersionCheck !== JSFALSE)) { versionToCheck = playerVersionCheck + EMPTY; }	// convert to string
			else { versionToCheck = "0.0.0"; }
			if (typeof checkCB === JSFUNC) { checkCallback = checkCB; }
			if (typeof autoUpdate !== UNDEF) { updateFlash = !(!autoUpdate); }	// convert to boolean
			if (typeof updateCB === JSFUNC) { updateCallback = updateCB; }
	
			function clearConstructInterval() { CLEARTIMEOUT(constructInterval); try { win.detachEvent(JS_ONUNLOAD,clearConstructInterval); } catch (err) { } }
			try { win.attachEvent(JS_ONUNLOAD,clearConstructInterval); } catch (err) { }
			(function waitForCore() {
				try { _flensed.bindEvent(win,JS_ONUNLOAD,destructor); } catch (err) { constructInterval = SETTIMEOUT(arguments.callee,25); return; }
				clearConstructInterval();
				flashVersionDetected = _flensed.ua.pv.join(".");
				constructInterval = SETTIMEOUT(continueConstructor,1);
			})();
		}();
	
		function continueConstructor() {
			try { bodyel = doc.getElementsByTagName("body")[0]; } catch (err) { }
	
			if ((typeof bodyel === UNDEF) || (bodyel === null)) { constructInterval = SETTIMEOUT(continueConstructor,25); return; }
			try { global.swfobject.getObjectById("a"); _swfobj = global.swfobject; } catch (swfobject_err) { constructInterval = SETTIMEOUT(continueConstructor,25); return; }
	
			updateable = _swfobj.hasFlashPlayerVersion(MIN_XI_VERSION);
			versionCheckPassed = _swfobj.hasFlashPlayerVersion(versionToCheck);
			updatePublicAPI();
	
			if (typeof checkCallback === JSFUNC) { checkCallback(publicAPI); }
			ready = JSTRUE;
	
			if (versionCheckPassed) { executeQueue(); }
			else if (updateFlash && !updateCalled) { updateFlashPlayer(); }
		}
	
		function destructor() {
			if (typeof win.detachEvent !== UNDEF) { win.detachEvent(JS_ONUNLOAD,destructor); }
			_chkplyr._ins = null;
			if ((typeof updateObj !== UNDEF) && (updateObj !== null)) {
				try { updateObj.updateSWFCallback = null; updateSWFCallback = null; } catch(err) { }
				updateObj = null;
			}
			try {
				for (var k in publicAPI) {
					if (publicAPI[k] !== Object.prototype[k]) {
						try { publicAPI[k] = null; } catch (err2) { }
					}
				}
			}
			catch (err3) { }
			publicAPI = null;
	
			bodyel = null;
			clearIntervals();
			swfQueue = null;
			checkCallback = null;
			updateCallback = null;
	
			try {
				for (var n in _chkplyr) {
					if (_chkplyr[n] !== Object.prototype[n]) {
						try { _chkplyr[n] = null; } catch (err4) { }
					}
				}
			}
			catch (err5) { }
			_chkplyr = null;
			_flensed.checkplayer = null;
			_flensed = null;
			global = null;
		}
	
		function addToQueue(func,funcName,args) {
			swfQueue[swfQueue.length] = { func:func, funcName:funcName, args:args };
		}
	
		function executeQueue() {
			if (!ready) {
				notReadyInterval = SETTIMEOUT(executeQueue,25);
				return;
			}
			var swfQueueLength = 0;
			try { swfQueueLength = swfQueue.length; } catch (err) { }
			for (var j=0; j<swfQueueLength; j++) {
				try {
					swfQueue[j].func.apply(publicAPI,swfQueue[j].args);
					swfQueue[j] = JSFALSE;
				}
				catch (err2) {
					versionCheckPassed = JSFALSE;
					updatePublicAPI();
	
					if (typeof checkCallback === JSFUNC) { checkCallback(publicAPI); }
					else { throw new global.Error("checkplayer::"+swfQueue[j].funcName+"() call failed."); }
				}
			}
			swfQueue = null;
		}
	
		function clearIntervals() {
			CLEARTIMEOUT(constructInterval);
			constructInterval = null;
			CLEARTIMEOUT(notReadyInterval);
			notReadyInterval = null;
			for (var j in swfIntHash) {
				if (swfIntHash[j] !== Object.prototype[j]) {
					CLEARINTERVAL(swfIntHash[j]);
					swfIntHash[j] = JSFALSE;
				}
			}
			for (var k in updateInterval) {
				if (updateInterval[k] !== Object.prototype[k]) {
					CLEARTIMEOUT(updateInterval[k]);
					updateInterval[k] = JSFALSE;
				}
			}
		}
	
		function updatePublicAPI() {
			try {
				publicAPI.playerVersionDetected = flashVersionDetected;
				publicAPI.checkPassed = versionCheckPassed;
				publicAPI.updateable = updateable;
				publicAPI.updateStatus = updateStatus;
				publicAPI.updateControlsContainer = holder;
			}
			catch (err) { }
		}
	
		function setVisibility(id, isVisible) {
			var v = isVisible ? "visible" : "hidden";
			var obj = _flensed.getObjectById(id);
			try {
				if (obj !== null && (typeof obj.style !== UNDEF) && (obj.style !== null)) { obj.style.visibility = v; }
				else { 
					try { _flensed.createCSS("#" + id, "visibility:" + v); } catch (err) { }
				}
			}
			catch (err2) { 
				try { _flensed.createCSS("#" + id, "visibility:" + v); } catch (err3) { }
			}
		}
	
		function updateFlashPlayer() {
			var appendTo = bodyel;
	
			if ((typeof appendTo === UNDEF) || (appendTo === null)) {
				updateInterval[updateInterval.length] = SETTIMEOUT(updateFlashPlayer,25);
				return;
			}
			try { _swfobj.getObjectById("a"); } catch (swfobject_err) {
				updateInterval[updateInterval.length] = SETTIMEOUT(updateFlashPlayer,25);
				return;
			}
			
			if (!updateCalled) {
				updateCalled = JSTRUE;
				clearIntervals();
				if (updateable) {
					updateContainerId = "CheckPlayerUpdate";
					updateSWFId = updateContainerId + "SWF";
	
					_flensed.createCSS("#"+updateContainerId,"width:221px;height:145px;position:absolute;left:5px;top:5px;border:none;background-color:#000000;display:block;");
					_flensed.createCSS("#"+updateSWFId,"display:inline;position:absolute;left:1px;top:1px;");
	
					holder=doc.createElement(JSDIV);
					holder.id = updateContainerId;
					appendTo.appendChild(holder);
					setVisibility(holder.id,JSFALSE);
	
					updatePublicAPI();
	
					var loc = null;
					try { loc = win.top.location.toString(); } catch (err3) { loc = win.location.toString(); }
					var flashvars = { swfId:updateSWFId, MMredirectURL:loc.replace(/&/g,"%26"), MMplayerType:(_flensed.ua.ie && _flensed.ua.win ? "ActiveX" : "PlugIn"), MMdoctitle:doc.title.slice(0, 47) + " - Flash Player Installation" };
					var params = { allowScriptAccess:"always" };
					var attributes = { id:updateSWFId, name:updateSWFId };
	
					try {
						doSWF(_flensed.base_path+"updateplayer.swf", {appendToId:updateContainerId}, "219", "143", flashvars, params, attributes, {swfTimeout:3000,swfCB:continueUpdate}, JSTRUE);
					}
					catch (err2) { updateFailed(); return; }
				}
				else { updateFailed(); }
			}
		}
	
		function updateFailed(errMsg) {
			if (typeof errMsg === UNDEF) { errMsg = "Flash Player not detected or not updateable."; }
			updateStatus = _chkplyr.UPDATE_FAILED;
			updatePublicAPI();
			if (typeof updateCallback === JSFUNC) { updateCallback(publicAPI); }
			else {
				throw new global.Error("checkplayer::UpdatePlayer(): "+errMsg);
			}
		}
	
		function continueUpdate(loadStatus) {
			if (loadStatus.status === _chkplyr.SWF_LOADED) {
				CLEARTIMEOUT(swfIntHash["continueUpdate_"+updateSWFId]);
				swfIntHash["continueUpdate_"+updateSWFId] = JSFALSE;
				updateObj = loadStatus.srcElem;
				updateObj.updateSWFCallback = updateSWFCallback;
	
				updateStatus = _chkplyr.UPDATE_INIT;
				updatePublicAPI();
				if (typeof updateCallback === JSFUNC) { updateCallback(publicAPI); }
				setVisibility(holder.id,JSTRUE);
			}
			else if (loadStatus.status === _chkplyr.SWF_FAILED || loadStatus.status === _chkplyr.SWF_TIMEOUT) {
				updateFailed();
			}
		}
	
		function updateSWFCallback(statusCode) {
			try {
				if (statusCode === 0) {			// update successful
					updateStatus = _chkplyr.UPDATE_SUCCESSFUL;
					holder.style.display = JSNONE;
					try {
						win.open(EMPTY,"_self",EMPTY);	// tricky IE syntax to force a self-close of window
						win.close();
						global.self.opener = win;
						global.self.close();
					}
					catch (err) { }
				}
				else if (statusCode === 1) {	// user canceled
					updateStatus = _chkplyr.UPDATE_CANCELED;
					holder.style.display = JSNONE;
				}
				else if (statusCode === 2) {	// update failed
					holder.style.display = JSNONE;
					updateFailed("The Flash Player update failed.");
					return;
				}
				else if (statusCode === 3) {	// update timeout
					holder.style.display = JSNONE;
					updateFailed("The Flash Player update timed out.");
					return;
				}
			}
			catch (err2) { }
	
			updatePublicAPI();
	
			if (typeof updateCallback === JSFUNC) { updateCallback(publicAPI); }
		}
	
		function doSWF(swfUrlStr, targetElem, widthStr, heightStr, flashvarsObj, parObj, attObj, optObj, ignoreVersionCheck) {
			if (targetElem !== null && (typeof targetElem === JSSTR || typeof targetElem.replaceId === JSSTR)) { setVisibility(((typeof targetElem === JSSTR)?targetElem:targetElem.replaceId),JSFALSE); }
			if (!ready && !ignoreVersionCheck) {
				addToQueue(doSWF,"DoSWF",arguments);
				return;
			}
			
			if (versionCheckPassed || ignoreVersionCheck) {
				widthStr += EMPTY; // Auto-convert to string to make it idiot proof
				heightStr += EMPTY;
	
				var att = (typeof attObj === OBJECT) ? attObj : {};
				att.data = swfUrlStr;
				att.width = widthStr;
				att.height = heightStr;
				var par = (typeof parObj === OBJECT) ? parObj : {};
				if (typeof flashvarsObj === OBJECT) {
					for (var i in flashvarsObj) {
						if (flashvarsObj[i] !== Object.prototype[i]) { // Filter out prototype additions from other potential libraries
							if (typeof par.flashvars !== UNDEF) {
								par.flashvars += "&" + i + "=" + flashvarsObj[i];
							}
							else {
								par.flashvars = i + "=" + flashvarsObj[i];
							}
						}
					}
				}
	
				var swfId = null;
				if (typeof attObj.id !== UNDEF) { swfId = attObj.id; }
				else if (targetElem !== null && (typeof targetElem === JSSTR || typeof targetElem.replaceId === JSSTR)) { swfId = ((typeof targetElem === JSSTR)?targetElem:targetElem.replaceId); }
				else { swfId = "swf_"+swfIdArr.length; }
				
				var replaceId = null;
				if (targetElem === null || targetElem === JSFALSE || typeof targetElem.appendToId === JSSTR) {
					var appendTo = null;
					if (targetElem !== null && targetElem !== JSFALSE && typeof targetElem.appendToId === JSSTR) { appendTo = _flensed.getObjectById(targetElem.appendToId); }
					else { appendTo = bodyel; }
					var targetObj = doc.createElement(JSDIV);
					replaceId = (targetObj.id = swfId);
					appendTo.appendChild(targetObj);
				}
				else { replaceId = ((typeof targetElem.replaceId === JSSTR) ? targetElem.replaceId : targetElem); }
				
				var swfCB = function(){}, swfTimeout = 0, swfEICheck = EMPTY, srcelem = null;
				if (typeof optObj !== UNDEF && optObj !== null) {
					if (typeof optObj === OBJECT) {
						if (typeof optObj.swfCB !== UNDEF && optObj.swfCB !== null) { swfCB = optObj.swfCB; }
						if (typeof optObj.swfTimeout !== UNDEF && (global.parseInt(optObj.swfTimeout,10) > 0)) { swfTimeout = optObj.swfTimeout; }
						if (typeof optObj.swfEICheck !== UNDEF && optObj.swfEICheck !== null && optObj.swfEICheck !== EMPTY) { swfEICheck = optObj.swfEICheck; }
					}
					else if (typeof optObj === JSFUNC) { swfCB = optObj; }
				}
	
				try { srcelem = _swfobj.createSWF(att, par, replaceId); }
				catch (err) { }

				if (srcelem !== null) {
					swfIdArr[swfIdArr.length] = swfId;
					if (typeof swfCB === JSFUNC) {
						swfCB({status:_chkplyr.SWF_INIT,srcId:swfId,srcElem:srcelem});
						swfIntHash[swfId] = SETINTERVAL(function() {
							var theObj = _flensed.getObjectById(swfId);
							if ((typeof theObj !== UNDEF) && (theObj !== null) && (theObj.nodeName === "OBJECT" || theObj.nodeName === "EMBED")) {
								var perloaded = 0;
								try { perloaded = theObj.PercentLoaded(); } catch (err) { }

								if (perloaded > 0) {
									if (swfTimeout > 0) { CLEARTIMEOUT(swfIntHash["DoSWFtimeout_"+swfId]); swfIntHash["DoSWFtimeout_"+swfId] = JSFALSE; }
									if (perloaded < 100) {
										// prevent swfCB from blocking this interval call if the user-defined function is long running
										SETTIMEOUT(function(){swfCB({status:_chkplyr.SWF_LOADING,srcId:swfId,srcElem:theObj});},1);
									}
									else {
										CLEARINTERVAL(swfIntHash[swfId]);
										swfIntHash[swfId] = JSFALSE;
										// prevent swfCB from blocking this interval call if the user-defined function is long running
										SETTIMEOUT(function(){swfCB({status:_chkplyr.SWF_LOADED,srcId:swfId,srcElem:theObj});},1);
										
										if (swfEICheck !== EMPTY) {
											var processing = JSFALSE;
											swfIntHash[swfId] = SETINTERVAL(function() {
												if (!processing && typeof theObj[swfEICheck] === JSFUNC) {
													processing = JSTRUE;
													try { 
														theObj[swfEICheck]();
														CLEARINTERVAL(swfIntHash[swfId]);
														swfIntHash[swfId] = JSFALSE;
														swfCB({status:_chkplyr.SWF_EI_READY,srcId:swfId,srcElem:theObj});
													} catch (err) { }
													processing = JSFALSE;
												}
											},25);
										}
									}
								}
							}
						},50);
						if (swfTimeout > 0) {
							swfIntHash["DoSWFtimeout_"+swfId] = SETTIMEOUT(function() {
								var theObj = _flensed.getObjectById(swfId);
								if ((typeof theObj !== UNDEF) && (theObj !== null) && (theObj.nodeName === "OBJECT" || theObj.nodeName === "EMBED")) {
									var perloaded = 0;
									try { perloaded = theObj.PercentLoaded(); } catch (err) { }
									if (perloaded <= 0) {
										CLEARINTERVAL(swfIntHash[swfId]);
										swfIntHash[swfId] = JSFALSE;
										if (_flensed.ua.ie && _flensed.ua.win && theObj.readyState !== 4) {
											theObj.id = "removeSWF_"+theObj.id;
											theObj.style.display = JSNONE;
											swfIntHash[theObj.id] = SETINTERVAL(function() {
												if (theObj.readyState === 4) {
													CLEARINTERVAL(swfIntHash[theObj.id]);
													swfIntHash[theObj.id] = JSFALSE;
													_swfobj.removeSWF(theObj.id);
												}
											},500);
										}
										else { _swfobj.removeSWF(theObj.id); }
										swfIntHash[swfId] = JSFALSE;
										swfIntHash["DoSWFtimeout_"+swfId] = JSFALSE;
										swfCB({status:_chkplyr.SWF_TIMEOUT,srcId:swfId,srcElem:theObj});
									}
								}
							},swfTimeout);
						}
					}
				}
				else {
					if (typeof swfCB === JSFUNC) { swfCB({status:_chkplyr.SWF_FAILED,srcId:swfId,srcElem:null}); }
					else { throw new global.Error("checkplayer::DoSWF(): SWF could not be loaded."); }
				}
			}
			else {
				if (typeof swfCB === JSFUNC) { swfCB({status:_chkplyr.SWF_FAILED,srcId:swfId,srcElem:null}); }
				else { throw new global.Error("checkplayer::DoSWF(): Minimum Flash Version not detected."); }
			}
		}
	
		// Public API
		var publicAPI = {
			playerVersionDetected:flashVersionDetected,
			versionChecked:versionToCheck,
			checkPassed:versionCheckPassed,
	
			UpdatePlayer:updateFlashPlayer,
			DoSWF:function(swfUrlStr, targetElem, widthStr, heightStr, flashvarsObj, parObj, attObj, optObj) {
				doSWF(swfUrlStr,targetElem,widthStr,heightStr,flashvarsObj,parObj,attObj,optObj,JSFALSE);
			},
			ready:function(){return ready;},
	
			updateable:updateable,
			updateStatus:updateStatus,
			updateControlsContainer:holder
		};
		_chkplyr._ins = publicAPI;
		return publicAPI;
	};
	_chkplyr = _flensed.checkplayer;	// frequently used variable declaration(s), for optimized compression
	
	_chkplyr.UPDATE_INIT = 1;
	_chkplyr.UPDATE_SUCCESSFUL = 2;
	_chkplyr.UPDATE_CANCELED = 3;
	_chkplyr.UPDATE_FAILED = 4;
	_chkplyr.SWF_INIT = 5;
	_chkplyr.SWF_LOADING = 6;
	_chkplyr.SWF_LOADED = 7;
	_chkplyr.SWF_FAILED = 8;
	_chkplyr.SWF_TIMEOUT = 9;
	_chkplyr.SWF_EI_READY = 10;
	_chkplyr.module_ready = function(){};
})(window);
