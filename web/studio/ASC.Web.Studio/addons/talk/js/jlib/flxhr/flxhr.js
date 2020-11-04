/*	flXHR 1.0.5 <http://flxhr.flensed.com/>
	Copyright (c) 2008-2010 Kyle Simpson, Getify Solutions, Inc.
	This software is released under the MIT License <http://www.opensource.org/licenses/mit-license.php>

	====================================================================================================
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
		tmp = null,
		_flensed = null,
		_chkplyr = null,
		_cp_ins = null,
		_flxhr_idc = 0,
		_flxhr_inp = [],
		_flxhr_css = null,
		_flxhr = null,
		flXHR_js = "flxhr.js",					// SHOULD NOT rename the file or change this line
		flensed_js = "flensed.js",				// ditto
		flXHR_vbs = "flxhr.vbs",				// ditto
		checkplayer_js = "checkplayer.js",		// ditto
		flXHR_swf = "flxhr.swf",				// ditto
		PARSEINT = global.parseInt,
		SETTIMEOUT = global.setTimeout,
		CLEARTIMEOUT = global.clearTimeout,
		SETINTERVAL = global.setInterval,
		CLEARINTERVAL = global.clearInterval,
		
		_INSTANCEID_ = "instanceId",			// strings used for shortening repeated object property accesses
		_READYSTATE_ = "readyState",
		_ONREADYSTATECHANGE_ = "onreadystatechange",
		_ONTIMEOUT_ = "ontimeout",
		_ONERROR_ = "onerror",
		_BINARYRESPONSEBODY_ = "binaryResponseBody",
		_XMLRESPONSETEXT_ = "xmlResponseText",
		_LOADPOLICYURL_ = "loadPolicyURL",
		_NOCACHEHEADER_ = "noCacheHeader",
		_SENDTIMEOUT_ = "sendTimeout",
		_APPENDTOID_ = "appendToId",
		_SWFIDPREFIX_ = "swfIdPrefix";

	if (typeof global.flensed === UNDEF) { global.flensed = {}; }
	if (typeof global.flensed.flXHR !== UNDEF) { return; }	// flXHR already defined, so quit
	
	_flensed = global.flensed;
	
	SETTIMEOUT(function() {
		var base_path_known = JSFALSE,
			scriptArry = doc.getElementsByTagName("script"),
			scrlen = scriptArry.length;
		try { _flensed.base_path.toLowerCase(); base_path_known = JSTRUE; } catch(err) { _flensed.base_path = EMPTY; }
	
		function load_script(src,type,language) {
			for (var k=0; k<scrlen; k++) {
				if (typeof scriptArry[k].src !== UNDEF) {
					if (scriptArry[k].src.indexOf(src) >= 0) { break; }  // this script already loaded/loading...
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
						if (((idx=scriptArry[k].src.indexOf(flensed_js)) >= 0) || ((idx=scriptArry[k].src.indexOf(flXHR_js)) >= 0)) {
							_flensed.base_path = scriptArry[k].src.substr(0,idx);
							break;
						}
					}
				}
			}
		}
		try { _flensed.checkplayer.module_ready(); } catch (err2) { load_script(checkplayer_js,"text/javascript"); }
	
		var coreInterval = null;
		(function waitForCore() {
			try { _flensed.ua.pv.join("."); } catch (err) { coreInterval = SETTIMEOUT(arguments.callee,25); return; }
			if (_flensed.ua.win&&_flensed.ua.ie) { load_script(flXHR_vbs,"text/vbscript","vbscript"); }
			_flensed.binaryToString = function(binobj,skipVB) {
				skipVB = (((_flensed.ua.win&&_flensed.ua.ie) && typeof skipVB !== UNDEF)?(!(!skipVB)):!(_flensed.ua.win&&_flensed.ua.ie));
				if (!skipVB) {
					try { return flXHR_vb_BinaryToString(binobj); } catch (err) { }
				}
				var str = EMPTY, buf = [];
				try { 
					for (var i=0; i<binobj.length; i++) { buf[buf.length] = String.fromCharCode(binobj[i]); }
					str = buf.join(EMPTY);
				} catch (err2) { }
				return str;
			};
			_flensed.bindEvent(win,JS_ONUNLOAD,function(){
				try {
					global.flensed.unbindEvent(win,JS_ONUNLOAD,arguments.callee);
					for (var k in _flxhr) {
						if (_flxhr[k] !== Object.prototype[k]) {
							try { _flxhr[k] = null; } catch (err2) { }
						}
					}
					_flensed.flXHR = null;
					_flxhr = null;
					_flensed = null;
					_cp_ins = null;
					_chkplyr = null;
				}
				catch (err3) { }
			});
		})();
		function clearCoreInterval() { CLEARTIMEOUT(coreInterval); try { win.detachEvent(JS_ONUNLOAD,clearCoreInterval); } catch (err) {} }
		if (coreInterval !== null) { try { win.attachEvent(JS_ONUNLOAD,clearCoreInterval); } catch(err3) {} }

		var dependencyTimeout = null;
		function clearDependencyTimeout() { CLEARTIMEOUT(dependencyTimeout); try { win.detachEvent(JS_ONUNLOAD,clearDependencyTimeout); } catch (err) {} }
		try { win.attachEvent(JS_ONUNLOAD,clearDependencyTimeout); } catch (err4) {}
		dependencyTimeout = SETTIMEOUT(function(){
			clearDependencyTimeout();
			try { 
				_flensed.checkplayer.module_ready(); 
			} catch (err2) { throw new global.Error("flXHR dependencies failed to load."); }
		},20000);	// only wait 20 secs max for CheckPlayer to load
	},0);
	
	_flensed.flXHR = function(configObject) {
		var instancePooling = JSFALSE;
		if (configObject !== null && typeof configObject === OBJECT) {
			if (typeof configObject.instancePooling !== UNDEF) { 
				instancePooling = !(!configObject.instancePooling);
				if (instancePooling) {
					var ret = function(){
						for (var k=0; k<_flxhr_inp.length; k++) {
							var inst = _flxhr_inp[k];
							if (inst[_READYSTATE_] === 4) {	// must have already been used and be idle
								inst.Reset();
								inst.Configure(configObject);
								return inst;
							}
						}
						return null;
					}();
					if (ret !== null) { return ret; }
				}
			}
		}
	
		// Private Properties
		var	idNumber = ++_flxhr_idc,
			constructQueue = [],
			constructInterval = null,
			notReadyInterval = null,
			timeoutInterval = null,
			proxyId = null,
			readyState = -1,
			public_readyState = 0,
			responseBody = null,
			responseText = null,
			responseXML = null,
			status = null,
			statusText = null,
			proxyObj = null,
			publicAPI = null,
			appendTo = null,
			rawresponse = null,
			queue_empty = JSTRUE,
			_error = JSFALSE,
	
		// Configurable Properties (via instantiation constructor)
			instanceId = "flXHR_"+idNumber,
			noCacheHeader = JSTRUE,
			binaryResponseBody = JSFALSE,
			xmlResponseText = JSTRUE,
			autoUpdatePlayer = JSFALSE,
			proxyIdPrefix = "flXHR_swf",
			styleClass = "flXHRhideSwf",
			appendToId = null,
			sendTimeout = -1,
			loadPolicyURL = EMPTY,
			onreadystatechange = null,
			onerror  = null,
			ontimeout = null;
	
		// Private Methods
		var constructor = function() {
			if (typeof configObject === OBJECT && configObject !== null) {
				if ((typeof configObject[_INSTANCEID_] !== UNDEF) && (configObject[_INSTANCEID_] !== null) && (configObject[_INSTANCEID_] !== EMPTY)) { instanceId = configObject[_INSTANCEID_]; }
				if ((typeof configObject[_SWFIDPREFIX_] !== UNDEF) && (configObject[_SWFIDPREFIX_] !== null) && (configObject[_SWFIDPREFIX_] !== EMPTY)) { proxyIdPrefix = configObject[_SWFIDPREFIX_]; }
				if ((typeof configObject[_APPENDTOID_] !== UNDEF) && (configObject[_APPENDTOID_] !== null) && (configObject[_APPENDTOID_] !== EMPTY)) { appendToId = configObject[_APPENDTOID_]; }
				if ((typeof configObject[_LOADPOLICYURL_] !== UNDEF) && (configObject[_LOADPOLICYURL_] !== null) && (configObject[_LOADPOLICYURL_] !== EMPTY)) { loadPolicyURL = configObject[_LOADPOLICYURL_]; }
	
				if (typeof configObject[_NOCACHEHEADER_] !== UNDEF) { noCacheHeader = !(!configObject[_NOCACHEHEADER_]); }
				if (typeof configObject[_BINARYRESPONSEBODY_] !== UNDEF) { binaryResponseBody = !(!configObject[_BINARYRESPONSEBODY_]); }
				if (typeof configObject[_XMLRESPONSETEXT_] !== UNDEF) { xmlResponseText = !(!configObject[_XMLRESPONSETEXT_]); }
				if (typeof configObject.autoUpdatePlayer !== UNDEF) { autoUpdatePlayer = !(!configObject.autoUpdatePlayer); }
				if ((typeof configObject[_SENDTIMEOUT_] !== UNDEF) && ((tmp=PARSEINT(configObject[_SENDTIMEOUT_],10)) > 0)) { sendTimeout = tmp; }
	
				if ((typeof configObject[_ONREADYSTATECHANGE_] !== UNDEF) && (configObject[_ONREADYSTATECHANGE_] !== null)) { onreadystatechange = configObject[_ONREADYSTATECHANGE_]; }
				if ((typeof configObject[_ONERROR_] !== UNDEF) && (configObject[_ONERROR_] !== null)) { onerror = configObject[_ONERROR_]; }
				if ((typeof configObject[_ONTIMEOUT_] !== UNDEF) && (configObject[_ONTIMEOUT_] !== null)) { ontimeout = configObject[_ONTIMEOUT_]; }
			}
	
			proxyId = proxyIdPrefix+"_"+idNumber;
	
			function clearConstructInterval() { CLEARTIMEOUT(constructInterval); try { win.detachEvent(JS_ONUNLOAD,clearConstructInterval); } catch (err) { } }
			try { win.attachEvent(JS_ONUNLOAD,clearConstructInterval); } catch (err) { }	// only IEwin would leak memory this way
			(function waitForCore() {
				try { _flensed.bindEvent(win,JS_ONUNLOAD,destructor); } catch (err) { constructInterval = SETTIMEOUT(arguments.callee,25); return; }
				clearConstructInterval();
				constructInterval = SETTIMEOUT(continueConstructor,1);
			})();
		}();
	
		function continueConstructor() {
			if (appendToId === null) { appendTo = doc.getElementsByTagName("body")[0]; }
			else { appendTo = _flensed.getObjectById(appendToId); }
			
			try { appendTo.nodeName.toLowerCase(); _flensed.checkplayer.module_ready(); _chkplyr = _flensed.checkplayer; } catch (err) {	// make sure DOM object and checkplayer are ready
				// maybe set a timeout here in case the DOM obj (appendTo) never gets ready?
				constructInterval = SETTIMEOUT(continueConstructor,25);
				return;
			}

			if ((_cp_ins === null) && (typeof _chkplyr._ins === UNDEF)) {
				try {
					_cp_ins = new _chkplyr(_flxhr.MIN_PLAYER_VERSION,checkCallback,JSFALSE,updateCallback);
				}
				catch (err2) { doError(_flxhr.DEPENDENCY_ERROR,"flXHR: checkplayer Init Failed","The initialization of the 'checkplayer' library failed to complete."); return; }
			}
			else {
				_cp_ins = _chkplyr._ins;
				stillContinueConstructor();
			}
		}
	
		function stillContinueConstructor() {
			if (_cp_ins === null || !_cp_ins.checkPassed) {
				// maybe set a timeout here in the check never passes?
				constructInterval = SETTIMEOUT(stillContinueConstructor,25);
				return;
			}

			if (_flxhr_css === null && appendToId === null) {	// only if CSS hasn't been defined yet, and if flXHR's being added to the BODY of the page
				_flensed.createCSS("."+styleClass,"left:-1px;top:0px;width:1px;height:1px;position:absolute;");	// CSS to hide any flXHR instances added automatically to the BODY
				_flxhr_css = JSTRUE;
			}
	
			var holder=doc.createElement(JSDIV);
			holder.id = proxyId;
			holder.className = styleClass;
			appendTo.appendChild(holder);
			appendTo = null;
	
			var flashvars = {},
				params = { allowScriptAccess:"always" },
				attributes = { id:proxyId, name:proxyId, styleclass:styleClass },
				optionsObj = { swfCB:finishConstructor, swfEICheck:"reset" };
	
			try {
				_cp_ins.DoSWF(_flensed.base_path+flXHR_swf, proxyId, "1", "1", flashvars, params, attributes, optionsObj);
			}
			catch (err2) { doError(_flxhr.DEPENDENCY_ERROR,"flXHR: checkplayer Call Failed","A call to the 'checkplayer' library failed to complete."); return; }
		}
	
		function finishConstructor(loadStatus) {
			// maybe set an SWF_INIT triggered timeout here, in case somehow flXHR.swf fails to load/initialize?
			if (loadStatus.status !== _chkplyr.SWF_EI_READY) { return; }

			clearIntervals();
			proxyObj = _flensed.getObjectById(proxyId);
			proxyObj.setId(proxyId);
			if (loadPolicyURL !== EMPTY) { proxyObj.loadPolicy(loadPolicyURL); }
			proxyObj.autoNoCacheHeader(noCacheHeader);
			proxyObj.returnBinaryResponseBody(binaryResponseBody);

			proxyObj.doOnReadyStateChange = doOnReadyStateChange;
			proxyObj.doOnError = doError;
			proxyObj.sendProcessed = sendProcessed;
			proxyObj.chunkResponse = chunkResponse;

			readyState = 0;
			updateFromPublicAPI();
			updatePublicAPI();
			if (typeof onreadystatechange === JSFUNC) {
				try { onreadystatechange(publicAPI); }
				catch (err) { doError(_flxhr.HANDLER_ERROR,"flXHR::onreadystatechange(): Error","An error occurred in the handler function. ("+err.message+")"); return; }
			}
			executeQueue();
		}
	
		function destructor() {
			try { global.flensed.unbindEvent(win,JS_ONUNLOAD,destructor); } catch (err) { }
			try {
				for (var k=0; k<_flxhr_inp.length; k++) {
					if (_flxhr_inp[k] === publicAPI) { _flxhr_inp[k] = JSFALSE; }
				}
			} catch (err2) { }
			try {
				for (var j in publicAPI) {
					if (publicAPI[j] !== Object.prototype[j]) {
						try { publicAPI[j] = null; } catch (err3) { }
					}
				}
			}
			catch (err4) { }
			publicAPI = null;
	
			clearIntervals();
			if ((typeof proxyObj !== UNDEF) && (proxyObj !== null)) {
				try { proxyObj.abort(); } catch(err5) { }
	
				try { proxyObj.doOnReadyStateChange = null; doOnReadyStateChange = null; } catch(err6) { }
				try { proxyObj.doOnError = null; doOnError = null; } catch(err7) { }
				try { proxyObj.sendProcessed = null; sendProcessed = null; } catch (err8) { }
				try { proxyObj.chunkResponse = null; chunkResponse = null; } catch (err9) { }
				proxyObj = null;
			
				try { global.swfobject.removeSWF(proxyId); } catch(err10) { }
			}
			emptyQueue();
	
			onreadystatechange = null;
			onerror  = null;
			ontimeout = null;
			responseXML = null;
			responseBody = null;
			rawresponse = null;
			appendTo = null;
		}
		
		function chunkResponse() {
			if (binaryResponseBody && typeof arguments[0] !== UNDEF) {	// most likely an array of byte parameters for binary response
				rawresponse = ((rawresponse !== null)?rawresponse:[]);
				rawresponse = rawresponse.concat(arguments[0]);
			}
			else if (typeof arguments[0] === JSSTR) {	// a single string parameter
				rawresponse = ((rawresponse !== null)?rawresponse:EMPTY);
				rawresponse += arguments[0];
			}
		}
	
		function doOnReadyStateChange() {
			if (typeof arguments[0] !== UNDEF) { readyState = arguments[0]; }
			if (readyState === 4) {
				clearIntervals();
				if (binaryResponseBody && rawresponse !== null) {
					try { 
						responseText = _flensed.binaryToString(rawresponse,JSTRUE);
						try { responseBody = flXHR_vb_StringToBinary(responseText); } catch (err4) { responseBody = rawresponse; }
					} catch (err) { }
				} 
				else {
					responseText = rawresponse;
				}
				rawresponse = null;
				if (responseText !== EMPTY) {
					if (xmlResponseText) { 
						try { responseXML = _flensed.parseXMLString(responseText); }
						catch (err2) { responseXML = {}; }
					}
				}
			}
			if (typeof arguments[1] !== UNDEF) { status = arguments[1]; }
			if (typeof arguments[2] !== UNDEF) { statusText = arguments[2]; }
			doPublicOnReadyStateChange(readyState);
		}
		
		function doPublicOnReadyStateChange(rdySt) {
			public_readyState = rdySt;
			updateFromPublicAPI();
			updatePublicAPI();
			publicAPI[_READYSTATE_] = Math.max(0,rdySt);
	
			if (typeof onreadystatechange === JSFUNC) {
				try { onreadystatechange(publicAPI); } 
				catch (err) { doError(_flxhr.HANDLER_ERROR,"flXHR::onreadystatechange(): Error","An error occurred in the handler function. ("+err.message+")"); return; }
			}
		}
	
		function doError() {
			clearIntervals();
			emptyQueue();
			_error = JSTRUE;
			var errorObj;
			try {
				errorObj = new _flensed.error(arguments[0],arguments[1],arguments[2],publicAPI);
			}
			catch (err) {
				function ErrorObjTemplate() { this.number=0;this.name="flXHR Error: Unknown";this.description="Unknown error from 'flXHR' library.";this.message=this.description;this.srcElement=publicAPI;var a=this.number,b=this.name,c=this.description;function toString() { return a+", "+b+", "+c; } this.toString=toString; }
				errorObj = new ErrorObjTemplate();
			}
			var handled = JSFALSE;
			try { 
				if (typeof onerror === JSFUNC) { onerror(errorObj); handled = JSTRUE; }
			}
			catch (err2) { 
				var prevError = errorObj.toString();
				function ErrorObjTemplate2() { this.number=_flxhr.HANDLER_ERROR;this.name="flXHR::onerror(): Error";this.description="An error occured in the handler function. ("+err2.message+")\nPrevious:["+prevError+"]";this.message=this.description;this.srcElement=publicAPI;var a=this.number,b=this.name,c=this.description;function toString() { return a+", "+b+", "+c; } this.toString=toString; }
				errorObj = new ErrorObjTemplate2();
			}
	
			if (!handled) {
				SETTIMEOUT(function() { _flensed.throwUnhandledError(errorObj.toString()); },1);
			}
		}
	
		function doTimeout() {
			abort();	// calls clearIntervals()
			_error = JSTRUE;

			if (typeof ontimeout === JSFUNC) {
				try { ontimeout(publicAPI); }
				catch (err) {
					doError(_flxhr.HANDLER_ERROR,"flXHR::ontimeout(): Error","An error occurred in the handler function. ("+err.message+")");
					return;
				}
			}
			else { doError(_flxhr.TIMEOUT_ERROR,"flXHR: Operation Timed out","The requested operation timed out."); }
		}
	
		function clearIntervals() {
			CLEARTIMEOUT(constructInterval);
			constructInterval = null;
			CLEARTIMEOUT(timeoutInterval);
			timeoutInterval = null;
			CLEARTIMEOUT(notReadyInterval);
			notReadyInterval = null;
		}
	
		function addToQueue(func,funcName,args) {
			constructQueue[constructQueue.length] = { func:func, funcName:funcName, args:args };
			queue_empty = JSFALSE;
		}
			
		function emptyQueue() {
			if (!queue_empty) {
				queue_empty = JSTRUE;
				var queuelength = constructQueue.length;
				for (var m=0; m<queuelength; m++) {
					try { constructQueue[m] = JSFALSE; }
					catch (err) { }
				}
				constructQueue = [];
			}
		}
	
		function executeQueue() {
			if (readyState < 0) {
				notReadyInterval = SETTIMEOUT(executeQueue,25);
				return;
			}
			if (!queue_empty) {
				for (var j=0; j<constructQueue.length; j++) {
					try {
						if (constructQueue[j] !== JSFALSE) {
							constructQueue[j].func.apply(publicAPI,constructQueue[j].args);
							constructQueue[j] = JSFALSE;
						}
					}
					catch (err) {
						doError(_flxhr.HANDLER_ERROR,"flXHR::"+constructQueue[j].funcName+"(): Error","An error occurred in the "+constructQueue[j].funcName+"() function."); 
						return;
					}
				}
				queue_empty = JSTRUE;
			}
		}
	
		function updatePublicAPI() {
			try {
				publicAPI[_INSTANCEID_] = instanceId;
				publicAPI[_READYSTATE_] = public_readyState;
				publicAPI.status = status;
				publicAPI.statusText = statusText;
				publicAPI.responseText = responseText;
				publicAPI.responseXML = responseXML;
				publicAPI.responseBody = responseBody;
				publicAPI[_ONREADYSTATECHANGE_] = onreadystatechange;
				publicAPI[_ONERROR_] = onerror;
				publicAPI[_ONTIMEOUT_] = ontimeout;
				publicAPI[_LOADPOLICYURL_] = loadPolicyURL;
				publicAPI[_NOCACHEHEADER_] = noCacheHeader;
				publicAPI[_BINARYRESPONSEBODY_] = binaryResponseBody;
				publicAPI[_XMLRESPONSETEXT_] = xmlResponseText;
			}
			catch (err) { }
		}
	
		function updateFromPublicAPI() {
			try {
				instanceId = publicAPI[_INSTANCEID_];
				if (publicAPI.timeout !== null && (tmp=PARSEINT(publicAPI.timeout,10)) > 0) sendTimeout = tmp;
				onreadystatechange = publicAPI[_ONREADYSTATECHANGE_];
				onerror = publicAPI[_ONERROR_];
				ontimeout = publicAPI[_ONTIMEOUT_];
				if (publicAPI[_LOADPOLICYURL_] !== null) { 
					if ((publicAPI[_LOADPOLICYURL_] !== loadPolicyURL) && (readyState >= 0)) { proxyObj.loadPolicy(publicAPI[_LOADPOLICYURL_]); }
					loadPolicyURL = publicAPI[_LOADPOLICYURL_];
				}
				if (publicAPI[_NOCACHEHEADER_] !== null) {
					if ((publicAPI[_NOCACHEHEADER_] !== noCacheHeader) && (readyState >= 0)) { proxyObj.autoNoCacheHeader(publicAPI[_NOCACHEHEADER_]); }
					noCacheHeader = publicAPI[_NOCACHEHEADER_];
				}
				if (publicAPI[_BINARYRESPONSEBODY_] !== null) {
					if ((publicAPI[_BINARYRESPONSEBODY_] !== binaryResponseBody) && (readyState >= 0)) { proxyObj.returnBinaryResponseBody(publicAPI[_BINARYRESPONSEBODY_]); }
					binaryResponseBody = publicAPI[_BINARYRESPONSEBODY_];
				}
				if (xmlResponseText !== null) xmlResponseText = !(!publicAPI[_XMLRESPONSETEXT_]);
			}
			catch (err) { }
		}
	
		function reset() {
			abort();
			try { proxyObj.reset(); } catch (err) { }
			status = null;
			statusText = null;
			responseText = null;
			responseXML = null;
			responseBody = null;
			rawresponse = null;
			_error = JSFALSE;
			updatePublicAPI();
			loadPolicyURL = EMPTY;	// attempt to retrieve loadPolicyURL from public API and reload it, if non-empty
			updateFromPublicAPI();
		}
	
		function checkCallback(checkObj) {
			if (checkObj.checkPassed) {
				stillContinueConstructor();
			}
			else if (!autoUpdatePlayer) {

				doError(_flxhr.PLAYER_VERSION_ERROR,"flXHR: Insufficient Flash Player Version","The Flash Player was either not detected, or the detected version ("+checkObj.playerVersionDetected+") was not at least the minimum version ("+_flxhr.MIN_PLAYER_VERSION+") needed by the 'flXHR' library.");
			}
			else {
				_cp_ins.UpdatePlayer();
			}
		}
	
		function updateCallback(checkObj) {
			if (checkObj.updateStatus === _chkplyr.UPDATE_CANCELED) {
				doError(_flxhr.PLAYER_VERSION_ERROR,"flXHR: Flash Player Update Canceled","The Flash Player was not updated.");
			}
			else if (checkObj.updateStatus === _chkplyr.UPDATE_FAILED) {
				doError(_flxhr.PLAYER_VERSION_ERROR,"flXHR: Flash Player Update Failed","The Flash Player was either not detected or could not be updated.");
			}
		}
		
		function sendProcessed() {
			if (sendTimeout !== null && sendTimeout > 0) { timeoutInterval = SETTIMEOUT(doTimeout,sendTimeout); }
		}
	
		// Private Methods (XHR API functions)
		function abort() {
			clearIntervals();
			emptyQueue();
			updateFromPublicAPI();
			readyState = 0;
			public_readyState = 0;
			try { proxyObj.abort(); } 
			catch (err) { doError(_flxhr.CALL_ERROR,"flXHR::abort(): Failed","The abort() call failed to complete."); }
			updatePublicAPI();
		}
	
		function open() {
			updateFromPublicAPI();
			if (typeof arguments[0]===UNDEF||typeof arguments[1]===UNDEF) { doError(_flxhr.CALL_ERROR,"flXHR::open(): Failed","The open() call requires 'method' and 'url' parameters."); }
			else {
				if (readyState > 0 || _error) { reset(); }	// automatic reset is an instance re-use convenience, but is not part of the standard XHR API
				if (public_readyState===0) { doOnReadyStateChange(1); }	// manage readyState transitions
				else { readyState = 1; }
				var a0=arguments[0],a1=arguments[1],a2=(typeof arguments[2]!==UNDEF)?arguments[2]:JSTRUE,a3=(typeof arguments[3]!==UNDEF)?arguments[3]:EMPTY,a4=(typeof arguments[4]!==UNDEF)?arguments[4]:EMPTY;
				try { proxyObj.autoNoCacheHeader(noCacheHeader); proxyObj.open(a0,a1,a2,a3,a4); } 
				catch (err) { doError(_flxhr.CALL_ERROR,"flXHR::open(): Failed","The open() call failed to complete."); }
			}
		}
	
		function send() {
			updateFromPublicAPI();
			if (readyState <= 1 && !_error) {	// the SWF will actually catch if the open() hasn't yet been called (ie, readyState = 0)
				var a0=(typeof arguments[0]!==UNDEF)?arguments[0]:EMPTY;
				if (public_readyState===1) { doOnReadyStateChange(2); }	// manage readyState transitions
				else { readyState = 2; }
				try { proxyObj.autoNoCacheHeader(noCacheHeader); proxyObj.send(a0); } // flXHR.swf will call sendProcessed() once it begins processing
				catch (err) { doError(_flxhr.CALL_ERROR,"flXHR::send(): Failed","The send() call failed to complete."); }
			}
			else {
				doError(_flxhr.CALL_ERROR,"flXHR::send(): Failed","The send() call cannot be made at this time.");
			}
		}
	
		function setRequestHeader() {
			updateFromPublicAPI();
			if (typeof arguments[0]===UNDEF||typeof arguments[1]===UNDEF) { doError(_flxhr.CALL_ERROR,"flXHR::setRequestHeader(): Failed","The setRequestHeader() call requires 'name' and 'value' parameters."); }
			else if (!_error) {	// the SWF will actually catch if the open() hasn't yet been called (ie, readyState = 0), or if the send has already been called
								// per the native XHR API specs, setRequestHeader can only be set between open and send calls
				var a0=(typeof arguments[0]!==UNDEF)?arguments[0]:EMPTY,a1=(typeof arguments[1]!==UNDEF)?arguments[1]:EMPTY;
				try { proxyObj.setRequestHeader(a0,a1); } 
				catch (err) { doError(_flxhr.CALL_ERROR,"flXHR::setRequestHeader(): Failed","The setRequestHeader() call failed to complete."); }
			}
		}
	
		function getResponseHeader() { updateFromPublicAPI(); return EMPTY; }
	
		function getAllResponseHeaders() { updateFromPublicAPI(); return []; }	
	
		// Public API
		publicAPI = {
			// XHR API
			readyState:public_readyState,
			responseBody:responseBody,
			responseText:responseText,
			responseXML:responseXML,
			status:status,
			statusText:statusText,
			timeout:sendTimeout,
			open:function() {
				updateFromPublicAPI();
				if (publicAPI[_READYSTATE_]===0) { doPublicOnReadyStateChange(1); }	// make sure the public face of the object immediately goes through readyState changes for compatibility
				if (!queue_empty || readyState < 0) {
					addToQueue(open,"open",arguments);
					return;
				}
				open.apply({},arguments);
			},
			send:function() {
				updateFromPublicAPI();
				if (publicAPI[_READYSTATE_]===1) { doPublicOnReadyStateChange(2); }	// make sure the public face of the object immediately goes through readyState changes for compatibility
				if (!queue_empty || readyState < 0) {
					addToQueue(send,"send",arguments);
					return;
				}
				send.apply({},arguments);
			},
			abort:abort,
			setRequestHeader:function() {
				updateFromPublicAPI();
				if (!queue_empty || readyState < 0) {
					addToQueue(setRequestHeader,"setRequestHeader",arguments);
					return;
				}
				setRequestHeader.apply({},arguments);
			},
			getResponseHeader:getResponseHeader,
			getAllResponseHeaders:getAllResponseHeaders,
			onreadystatechange:onreadystatechange,
			ontimeout:ontimeout,
	
			// extended API
			instanceId:instanceId,
			loadPolicyURL:loadPolicyURL,
			noCacheHeader:noCacheHeader,
			binaryResponseBody:binaryResponseBody,
			xmlResponseText:xmlResponseText,
			onerror:onerror,
			Configure:function(configObj) {
				if (typeof configObj === OBJECT && configObj !== null) {
					if ((typeof configObj[_INSTANCEID_] !== UNDEF) && (configObj[_INSTANCEID_] !== null) && (configObj[_INSTANCEID_] !== EMPTY)) { instanceId = configObj[_INSTANCEID_]; }
					if (typeof configObj[_NOCACHEHEADER_] !== UNDEF) { 
						noCacheHeader = !(!configObj[_NOCACHEHEADER_]); 
						if (readyState >= 0) { proxyObj.autoNoCacheHeader(noCacheHeader); }
					}
					if (typeof configObj[_BINARYRESPONSEBODY_] !== UNDEF) { 
						binaryResponseBody = !(!configObj[_BINARYRESPONSEBODY_]); 
						if (readyState >= 0) { proxyObj.returnBinaryResponseBody(binaryResponseBody); }
					}
					if (typeof configObj[_XMLRESPONSETEXT_] !== UNDEF) { xmlResponseText = !(!configObj[_XMLRESPONSETEXT_]); }
					if ((typeof configObj[_ONREADYSTATECHANGE_] !== UNDEF) && (configObj[_ONREADYSTATECHANGE_] !== null)) { onreadystatechange = configObj[_ONREADYSTATECHANGE_]; }
					if ((typeof configObj[_ONERROR_] !== UNDEF) && (configObj[_ONERROR_] !== null)) { onerror = configObj[_ONERROR_]; }
					if ((typeof configObj[_ONTIMEOUT_] !== UNDEF) && (configObj[_ONTIMEOUT_] !== null)) { ontimeout = configObj[_ONTIMEOUT_]; }
					if ((typeof configObj[_SENDTIMEOUT_] !== UNDEF) && ((tmp=PARSEINT(configObj[_SENDTIMEOUT_],10)) > 0)) { sendTimeout = tmp; }
					if ((typeof configObj[_LOADPOLICYURL_] !== UNDEF) && (configObj[_LOADPOLICYURL_] !== null) && (configObj[_LOADPOLICYURL_] !== EMPTY) && (configObj[_LOADPOLICYURL_] !== loadPolicyURL)) {
						loadPolicyURL = configObj[_LOADPOLICYURL_];
						if (readyState >= 0) { proxyObj.loadPolicy(loadPolicyURL); }
					}
					updatePublicAPI();
				}
			},
			Reset:reset,
			Destroy:destructor
		};
		if (instancePooling) { _flxhr_inp[_flxhr_inp.length] = publicAPI; }
		return publicAPI;
	};
	_flxhr = _flensed.flXHR;	// frequently used variable declarations
	
	// Static Properties
	_flxhr.HANDLER_ERROR = 10;
	_flxhr.CALL_ERROR = 11;
	_flxhr.TIMEOUT_ERROR = 12;
	_flxhr.DEPENDENCY_ERROR = 13;
	_flxhr.PLAYER_VERSION_ERROR = 14;
	_flxhr.SECURITY_ERROR = 15;
	_flxhr.COMMUNICATION_ERROR = 16;
	_flxhr.MIN_PLAYER_VERSION = "9.0.124";
	_flxhr.module_ready = function(){};
})(window);