// This code was butchered out of https://github.com/jquery/jquery-tmpl
// It provides just enough to build HTML templates into a JavaScript function.

/*!
* jQuery Templates Plugin 1.0.0pre
* http://github.com/jquery/jquery-tmpl
* Requires jQuery 1.4.2
*
* Copyright Software Freedom Conservancy, Inc.
* Dual licensed under the MIT or GPL Version 2 licenses.
* http://jquery.org/license
*/
var tmplTags = {
    "tmpl": {
        _default: { $2: "null" },
        open: "if($notnull_1){__=__.concat($item.nest($1,$2));}"
        // tmpl target parameter can be of type function, so use $1, not $1a (so not auto detection of functions)
        // This means that {{tmpl foo}} treats foo as a template (which IS a function).
        // Explicit parens can be used if foo is a function that returns a template: {{tmpl foo()}}.
    },
    "wrap": {
        _default: { $2: "null" },
        open: "$item.calls(__,$1,$2);__=[];",
        close: "call=$item.calls();__=call._.concat($item.wrap(call,__));"
    },
    "each": {
        _default: { $2: "$index, $value" },
        open: "if($notnull_1){$.each($1a,function($2){with(this){",
        close: "}});}"
    },
    "if": {
        open: "if(($notnull_1) && $1a){",
        close: "}"
    },
    "else": {
        _default: { $1: "true" },
        open: "}else if(($notnull_1) && $1a){"
    },
    "html": {
        // Unecoded expression evaluation.
        open: "if($notnull_1){__.push($1a);}"
    },
    "=": {
        // Encoded expression evaluation. Abbreviated form is ${}.
        _default: { $1: "$data" },
        open: "if($notnull_1){__.push($.encode($1a));}"
    },
    "!": {
        // Comment tag. Skipped by parser
        open: ""
    }
};

function trim(s) {
    return s.replace(/^\s+/, '').replace(/\s+$/, '');
}

function unescape(args) {
    return args ? args.replace(/\\'/g, "'").replace(/\\\\/g, "\\") : null;
}

function buildTmplFn(markup) {
    return "function(jQuery, $item) {" +
    // Use the variable __ to hold a string array while building the compiled template. (See https://github.com/jquery/jquery-tmpl/issues#issue/10).
			"var $=jQuery,call,__=[],$data=$item.data;" +

    // Introduce the data as local variables using with(){}
			"with($data){__.push('" +

    // Convert the template into pure JavaScript
			trim(markup)
				.replace(/([\\'])/g, "\\$1")
				.replace(/[\r\t\n]/g, " ")
				.replace(/\$\{([^\}]*)\}/g, "{{= $1}}")
				.replace(/\{\{(\/?)(\w+|.)(?:\(((?:[^\}]|\}(?!\}))*?)?\))?(?:\s+(.*?)?)?(\(((?:[^\}]|\}(?!\}))*?)\))?\s*\}\}/g,
				function(all, slash, type, fnargs, target, parens, args) {
				    var tag = tmplTags[type], def, expr, exprAutoFnDetect;
				    if (!tag) {
				        throw "Unknown template tag: " + type;
				    }
				    def = tag._default || [];
				    if (parens && !/\w$/.test(target)) {
				        target += parens;
				        parens = "";
				    }
				    if (target) {
				        target = unescape(target);
				        args = args ? ("," + unescape(args) + ")") : (parens ? ")" : "");
				        // Support for target being things like a.toLowerCase();
				        // In that case don't call with template item as 'this' pointer. Just evaluate...
				        expr = parens ? (target.indexOf(".") > -1 ? target + unescape(parens) : ("(" + target + ").call($item" + args)) : target;
				        exprAutoFnDetect = parens ? expr : "(typeof(" + target + ")==='function'?(" + target + ").call($item):(" + target + "))";
				    } else {
				        exprAutoFnDetect = expr = def.$1 || "null";
				    }
				    fnargs = unescape(fnargs);
				    return "');" +
						tag[slash ? "close" : "open"]
							.split("$notnull_1").join(target ? "typeof(" + target + ")!=='undefined' && (" + target + ")!=null" : "true")
							.split("$1a").join(exprAutoFnDetect)
							.split("$1").join(expr)
							.split("$2").join(fnargs || def.$2 || "") +
						"__.push('";
				}) +
			"');}return __;}";
}