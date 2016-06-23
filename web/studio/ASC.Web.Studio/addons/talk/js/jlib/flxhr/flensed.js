/*	flensedCore 1.0 <http://www.flensed.com/>
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
		JSSTR = "string",
		_flensed = null,
		dynamicStylesheet = null,
		dynamicStylesheetMedia = null,
		PARSEINT = global.parseInt,
		SETTIMEOUT = global.setTimeout;

	if (typeof global.flensed === UNDEF) { global.flensed = {}; }
	else if (typeof global.flensed.ua !== UNDEF) { return; }	// flensed already defined, so quit

	_flensed = global.flensed;

	SETTIMEOUT(function() {
		var this_script = "flensed.js",		// DO NOT rename this file or change this line.
			base_path_known = JSFALSE,
			scriptArry = doc.getElementsByTagName("script"),
			scrlen = scriptArry.length;
		try { _flensed.base_path.toLowerCase(); base_path_known = JSTRUE; } catch(err) { _flensed.base_path = EMPTY; }
		
		if ((typeof scriptArry !== UNDEF) && (scriptArry !== null)) {
			if (!base_path_known) {
				var idx=0;
				for (var k=0; k<scrlen; k++) {
					if (typeof scriptArry[k].src !== UNDEF) {
						if ((idx=scriptArry[k].src.indexOf(this_script)) >= 0) {
							_flensed.base_path = scriptArry[k].src.substr(0,idx);
							break;
						}
					}
				}
			}
		}
	},0);
	
	_flensed.parseXMLString = function(xmlStr) {
		var xmlDoc = null;
		if (win.ActiveXObject) {
			xmlDoc = new global.ActiveXObject("Microsoft.XMLDOM"); 
			xmlDoc.async=JSFALSE;
			xmlDoc.loadXML(xmlStr);
		}
		else {
			var parser = new global.DOMParser();
			xmlDoc = parser.parseFromString(xmlStr,"text/xml");
		}
		return xmlDoc;
	};
	_flensed.getObjectById = function(idStr) {
		try {
			if (doc.layers) { return doc.layers[idStr]; }
			else if (doc.all) { return doc.all[idStr]; }
			else if (doc.getElementById) { return doc.getElementById(idStr); }
		}
		catch (err) { }
		return null;
	};
	
	_flensed.createCSS = function(sel, decl, media, newStyle) {
		if (_flensed.ua.ie && _flensed.ua.mac) { return; }
		var h = doc.getElementsByTagName("head")[0];
		if (!h) { return; } // to also support badly authored HTML pages that lack a head element
		var m = (media && typeof media === JSSTR) ? media : "screen";
		if (newStyle) {
			dynamicStylesheet = null;
			dynamicStylesheetMedia = null;
		}
		if (!dynamicStylesheet || dynamicStylesheetMedia !== m) { 
			// create dynamic stylesheet + get a global reference to it
			var s = doc.createElement("style");
			s.setAttribute("type", "text/css");
			s.setAttribute("media", m);
			dynamicStylesheet = h.appendChild(s);
			if (_flensed.ua.ie && _flensed.ua.win && typeof doc.styleSheets !== UNDEF && doc.styleSheets.length > 0) {
				dynamicStylesheet = doc.styleSheets[doc.styleSheets.length - 1];
			}
			dynamicStylesheetMedia = m;
		}
		// add style rule
		if (_flensed.ua.ie && _flensed.ua.win) {
			if (dynamicStylesheet && typeof dynamicStylesheet.addRule === OBJECT) {
				dynamicStylesheet.addRule(sel, decl);
			}
		}
		else {
			if (dynamicStylesheet && typeof doc.createTextNode !== UNDEF) {
				dynamicStylesheet.appendChild(doc.createTextNode(sel + " {" + decl + "}"));
			}
		}
	};
	_flensed.bindEvent = function(obj,eventName,handlerFunc) {
		eventName = eventName.toLowerCase();
		try {
			if (typeof obj.addEventListener !== UNDEF) { obj.addEventListener(eventName.replace(/^on/,EMPTY),handlerFunc,JSFALSE); }
			else if (typeof obj.attachEvent !== UNDEF) { obj.attachEvent(eventName,handlerFunc); }
		} catch (err) { }
	};
	_flensed.unbindEvent = function(obj,eventName,handlerFunc) {
		eventName = eventName.toLowerCase();
		try {
			if (typeof obj.removeEventListener !== UNDEF) { obj.removeEventListener(eventName.replace(/^on/,EMPTY),handlerFunc,JSFALSE); }
			else if (typeof obj.detachEvent !== UNDEF) { obj.detachEvent(eventName,handlerFunc); }
		} catch (err) { }
	};
	_flensed.throwUnhandledError = function(errDescription) {
		throw new global.Error(errDescription);
	};
	_flensed.error = function(code,name,description,srcElement) {
		return {
			number:code,
			name:name,
			description:description,
			message:description,
			srcElement:srcElement,
			toString:function() { return code+", "+name+", "+description; }
		};
	};
	_flensed.ua = function() {
		var	SHOCKWAVE_FLASH = "Shockwave Flash",
			SHOCKWAVE_FLASH_AX = "ShockwaveFlash.ShockwaveFlash",
			FLASH_MIME_TYPE = "application/x-shockwave-flash",
			nav = global.navigator,
			w3cdom = typeof doc.getElementById !== UNDEF && typeof doc.getElementsByTagName !== UNDEF && typeof doc.createElement !== UNDEF,
			playerVersion = [0,0,0],
			d = null;
		if (typeof nav.plugins !== UNDEF && typeof nav.plugins[SHOCKWAVE_FLASH] === OBJECT) {
			d = nav.plugins[SHOCKWAVE_FLASH].description;
			if (d && !(typeof nav.mimeTypes !== UNDEF && nav.mimeTypes[FLASH_MIME_TYPE] && !nav.mimeTypes[FLASH_MIME_TYPE].enabledPlugin)) { 
			// navigator.mimeTypes["application/x-shockwave-flash"].enabledPlugin indicates whether plug-ins are enabled or disabled in Safari 3+
				//plugin = JSTRUE;
				d = d.replace(/^.*\s+(\S+\s+\S+$)/, "$1");
				playerVersion[0] = PARSEINT(d.replace(/^(.*)\..*$/, "$1"), 10);
				playerVersion[1] = PARSEINT(d.replace(/^.*\.(.*)\s.*$/, "$1"), 10);
				playerVersion[2] = /r/.test(d) ? PARSEINT(d.replace(/^.*r(.*)$/, "$1"), 10) : 0;
			}
		}
		else if (typeof win.ActiveXObject !== UNDEF) {
			try {
				var a = new global.ActiveXObject(SHOCKWAVE_FLASH_AX);
				if (a) { // a will return null when ActiveX is disabled
					d = a.GetVariable("$version");
					if (d) {
						d = d.split(" ")[1].split(",");
						playerVersion = [PARSEINT(d[0], 10), PARSEINT(d[1], 10), PARSEINT(d[2], 10)];
					}
				}
			}
			catch(err) {}
		}
		var u = nav.userAgent.toLowerCase(),
			p = nav.platform.toLowerCase(),
			webkit = /webkit/.test(u) ? parseFloat(u.replace(/^.*webkit\/(\d+(\.\d+)?).*$/, "$1")) : JSFALSE, // returns either the webkit version or false if not webkit
			ie = JSFALSE,
			ieVer = 0,
			windows = p ? /win/.test(p) : /win/.test(u),
			mac = p ? /mac/.test(p) : /mac/.test(u);
		/*@cc_on
			ie = JSTRUE;
			try { ieVer = PARSEINT(u.match(/msie (\d+)/)[1],10); } catch (err2) { }
			@if (@_win32)
				windows = JSTRUE;
			@elif (@_mac)
				mac = JSTRUE;
			@end
		@*/
		return { w3cdom:w3cdom, pv:playerVersion, webkit:webkit, ie:ie, ieVer:ieVer, win:windows, mac:mac };
	}();

})(window);
