var addNamespace = function(ns) {
	var nsParts = ns.split(".");
	var root = window;
	for(var i=0; i<nsParts.length; i++) {
		if(typeof root[nsParts[i]] == "undefined") {
			root[nsParts[i]] = {};
		}
		root = root[nsParts[i]];
	}
};

Object.extend(window, {
	$: function() {
		var elements = [];
		for(var i=0; i<arguments.length; i++) {
			var e = arguments[i];
			if(typeof e == 'string') {
				e = document.getElementById(e);
			}
			if(arguments.length == 1) {
				return e;
			}
			elements.push(e);
		}
		return elements;
	},
	Class: {
		create: function() {
			return function() {
				if(typeof this.initialize == "function") {
					this.initialize.apply(this, arguments);
				}
			};
		}
	}
}, false);

addNamespace("MS.Debug");
MS.Debug = {};		// has been removed to debug version of core.ashx

addNamespace("MS.Position");

Object.extend(MS.Position, {
	getLocation: function(ele) {
		var x = 0;
		var y = 0;
		var p;
		for(p=ele; p; p=p.offsetParent) {
			// if(p.style.position == "relative" || p.style.position == "absolute") break;
			if(p.offsetLeft && p.offsetTop) {
				x += p.offsetLeft;
				y += p.offsetTop;
			}
		}
		return {left:x,top:y};
	},
	getBounds: function(ele) {
		var offset = MS.Position.getLocation(ele);
		var width = ele.offsetWidth;
		var height = ele.offsetHeight;
		return {left:offset.left,top:offset.top,width:width,height:height};
	},
	setLocation: function(ele, loc) {
		ele.style.position = "absolute";
		ele.style.left = loc.left + "px";
		ele.style.top = loc.top + "px";
	},
	setBounds: function(ele, rect) {
		if(rect.left && rect.top) {
			MS.Position.setLocation(ele, rect);
		}
		ele.style.width = rect.width + "px";
		ele.style.height = rect.height + "px";
	}
}, false);

addNamespace("MS.Keys");

Object.extend(MS.Keys, {
	TAB: 9,
	ESC: 27,
	KEYUP: 38,
	KEYDOWN: 40,
	KEYLEFT: 37,
	KEYRIGHT: 39,
	SHIFT: 16,
	CTRL: 17,
	ALT: 18,
	ENTER: 13,
	getCode: function(e) {
		e = MS.getEvent(e);
		if(e != null) { return e.keyCode; }
		return -1;
	}
}, false);

Object.extend(MS, {
	setText: function(ele, text) {
		if(ele == null) { return; }
		if(document.all) {
			ele.innerText = text;
		} else {
			ele.textContent = text;
		}
	},
	setHtml: function(ele, html) {
		if(ele == null) { return; }
		ele.innerHTML = html;
	},
	cancelEvent: function(e) {
		e = MS.getEvent(e);
		if(window.event) {
			e.returnValue = false;
		} else if(e) {
			e.preventDefault();
			e.stopPropagation();
		}
	},
	getEvent: function(e) {
		if(window.event) { return window.event; }
		if(e) { return e; }
		return null;
	},
	getTarget: function(e) {
		e = MS.getEvent(e);
		if(window.event) { return e.srcElement; }
		if(e) { return e.target; }
	}
}, false);

var StringBuilder = function() {
	this.v = [];
};

Object.extend(StringBuilder.prototype, {
	append: function(s) {
		this.v.push(s);
	},
	appendLine: function(s) {
		this.v.push(s + "\r\n");
	},
	clear: function() {
		this.v.clear();
	},
	toString: function() {
		return this.v.join("");
	}
}, true);