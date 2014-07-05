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