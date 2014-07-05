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


/**
* @project XSS.js
* @overview Replacement for innerHTML
* @author Roman Shafigullin <xss.js@shafigullin.pro>
* @version 0.0.1
* @copyright 2013, LinkedIn
* @license MIT
*/
/* jshint white:true */
/**
* Holds functionality related to this project.
* @namespace XSS
*/
var XSS = (function (undefined) {
    "use strict";
    /**
    * Called when library found some potential issue or have recommendations to a developer, define XSS.report to handle message, XSS.setDebug(1) enables debugging and all messages shows in console.
    * @function warn
    * @memberof! XSS
    * @param {string} message
    */
    var warn = function (message) {
        if (XSS.report !== undefined) {
            var data = {
                url: window.location.href,
                message: message
            };
            XSS.report(data);
        }
    };
    var debug = 0; // localStorage.setItem('XSS.debug', 1) - enables debug messages to console
    var console = window.console;
    if (console !== undefined && console.warn !== undefined && window.localStorage !== undefined) {
        debug = +localStorage.getItem('XSS.debug');
        if (debug) {
            warn = function (message) {
                console.warn(message);
            };
        }
    }
    /**
    * Encodes special chars in text to HTML entities and replaces control chars like {@link http://en.wikipedia.org/wiki/Null_character|0-byte} to {@link http://en.wikipedia.org/wiki/Specials_(Unicode_block)|replacement character}.
    * So it can be safely used with concatenation in HTML attributes and text nodes (except script tag). It protects only from HTML injection.
    * and XSS.encodeJS for JavaScript string context.<br />
    * <pre>
    * // user generated data
    * var url = 'javascript:alert(1);';
    * var alt = '"onclick="alert(1)';
    * var text = '&lt;img src=xx: onerror=alert(1)&gt;';
    * var message = '\'-alert(1)-\''<br />
    * element.innerHTML = '&lt;a href="' + XSS.encodeHTML(url) + '" alt="' + XSS.encodeHTML(alt) + '" onclick="' + XSS.encodeHTML('alert(\'' + XSS.encodeJS(message) + '\')') + '"&gt;' + XSS.encodeHTML(text) + '&lt;/a&gt;';
    *
    * // At the end we will see data properly prepared for all contexts
    * // &lt;a href&#61;&quot;&amp;#46&#59;&amp;#47&#59;javascript&amp;#58&#59;alert%281%29%3B&quot; alt&#61;&quot;&amp;quot&#59;onclick&amp;#61&#59;&amp;quot&#59;alert&amp;#40&#59;1&amp;#41&#59;&quot; onclick&#61;&quot;alert&amp;#40&#59;&amp;#39&#59;&amp;#92&#59;u0027&amp;#92&#59;u002dalert&amp;#92&#59;u00281&amp;#92&#59;u0029&amp;#92&#59;u002d&amp;#92&#59;u0027&amp;#39&#59;&amp;#41&#59;&quot;&gt;&amp;lt&#59;img src&amp;#61&#59;xx&amp;#58&#59; onerror&amp;#61&#59;alert&amp;#40&#59;1&amp;#41&#59;&amp;gt&#59;&lt;&#47;a&gt;
    * </pre>
    *
    * @function encodeHTML
    * @memberof XSS
    * @param {string} text - plain text
    * @returns {string} HTML encoded text
    */
    var encodeHTML = (function () {
        var c, i;
        var index = {
            '\t': '\t',
            '\r': '\r',
            '\n': '\n',
            '\u00a0': '&nbsp;',
            '<': '&lt;',
            '>': '&gt;',
            '&': '&amp;',
            '"': '&quot;',
            '\u007f': '&#xfffd;',
            '\u2028': '&#x2028;',
            '\u2029': '&#x2029;'
        };
        // replace all control chars with safe one
        for (i = 0; i < 32; i++) {
            c = String.fromCharCode(i);
            if (index[c] === undefined) {
                index[c] = '&#xfffd;';
            }
        }
        // this range of chars with will be replaced with HTML entities, if add char with code more than 127, check that it in index table
        var rHTMLSpecialChars = /[\u0000-\u0008\u000b\u000c\u000e-\u001f\u00a0<>'"`\\\[\]+\-=.:(){};,\/&\u007f\u2028\u2029]/g;
        // if text already encoded, to make sure that it don't have special chars we should use encodeHTML(text, /* preventDoubleEncoding */ true)
        var rHTMLSpecialCharsForDoubleEncoding = /[\u0000-\u0008\u000b\u000c\u000e-\u001f\u00a0<>'"`\\\[\]+\-=.:(){},\/\u007f\u2028\u2029]/g;
        // precache index
        for (i = 32; i < 128; i++) {
            c = String.fromCharCode(i);
            if (index[c] === undefined && c.match(rHTMLSpecialChars)) {
                index[c] = '&#' + i + ';';
            }
        }
        return function (text, preventDoubleEncoding) {
            if (text === null || text === undefined) {
                return '';
            }
            var type = typeof text;
            if (type === 'number' || type === 'boolean') {
                return text;
            }
            return (text + '').replace(preventDoubleEncoding ? rHTMLSpecialCharsForDoubleEncoding : rHTMLSpecialChars, function (c) {
                return index[c];
            });
        };
    }());
    var replacePlaceholders = function (text, data, encoder) {
        if (data !== undefined && data !== null) {
            var type = typeof text;
            text = text + '';
            if (type === 'number' || type === 'boolean') {
                return text;
            } else {
                // we allow only spaces like {{ foo.bar }}
                // {{{foo.bar} will work, but this syntax is not recommended, it was done just to simplify code
                text = text.replace(/(\{{1,3}) *([a-z\d][a-z\d.]*) *\}{1,3}/ig, function ($0, $1, $2) {
                    var path = $2.split('.');
                    var name;
                    var item = data;
                    while ((name = path.shift())) {
                        item = item.hasOwnProperty(name) ? item[name] : {};
                    }
                    if (typeof item === 'string' || typeof item === 'number') {
                        // if placeholder looks like {foo.bar} or {{foo.bar}} then second argument to encoder is false
                        // if {{{foo.bar}}} then second argument is true
                        // for XSS.encodeHTML second arguments means prevent double encoding (don't replace & to &amp;), in most cases this is safe, but use it only for already HTML encoded data
                        return encoder === undefined ? item : encoder(item, $1.length === 3);
                    } else {
                        warn('Placeholder "' + $2 + '" is invalid');
                        return '';
                    }
                });
            }
        }
        return text;
    };
    /**
    * sanitizeHTML similar to sanitizeAliasedHTML but all attributes and tags which is not listed will be plaintext.
    * @function sanitizeHTML
    * @memberof XSS
    * @param {string} html - any HTML which should be sanitized
    * @returns {string} sanitized HTML
    */
    var sanitizeHTML = (function () {
        var message;
        /*
        1. NULL to REPLACEMENT CHARACTER
        2. <good><bad> to <good>&lt;bad>
        3. good="value" bad="value" to goodNULL"value" bad="value"
        4. goodNULL"value" bad="value" to goodNULL"value" bad&#61;"value"
        5. goodNULL"value" bad&#61;"value" to good="value" bad&#61;"value"
        6. url="good" url="bad" to url="good" url="./bad"
        */
        var steps = [
        /*
        1. Replace all null-byte characters to replacement character, because normal HTML never have it.
        */
        {
            r: /\u0000/g,
            x: '\ufffd'
        },
        /*
        2. Replace all less-then character to HTML encoded if less-then is not part of allowed tag: <script to &lt;script.
        List of allowed tags:
        a, abbr, area, audio, b, bdi, bdo, big, blockquote, br, button, cite, code, datalist, del, dfn, div, em, font, form, h1, h2, h3, h4, h5, h6, hr, i, img, input, ins, kbd, label, li, map, mark, marquee, nobr, ol, optgroup, option, p, pre, q, rp, rt, ruby, s, samp, select, small, source, span, strike, strong, sub, sup, table, tbody, td, textarea, tfoot, th, thead, time, tr, u, ul, var, video, wbr
        */
        {
            r: /<(?!\/?(?:a(?:|bbr|rea|udio)|b(?:|d[io]|ig|lockquote|utton|r)|c(?:ite|ode)|d(?:atalist|el|fn|iv)|em|fo(?:nt|rm)|h[1-6r]|i(?:|mg|n(?:put|s))|kbd|l(?:abel|i)|ma(?:r(?:quee|k)|p)|nobr|o(?:pt(?:group|ion)|l)|p(?:|re)|r(?:uby|[pt])|s(?:|amp|elect|mall|ource|pan|tr(?:ike|ong)|u[bp])|t(?:able|body|extarea|foot|h(?:|ead)|ime|[dr])|ul?|v(?:ar|ideo)|wbr|q)(?:[ \r\n\t]|\/?>))/gi,
            x: function () {
                message = 'Template contains forbidden tags';
                return '&lt;';
            }
        },
        /*
        3. We can't control attributes without values because they don't contain = sign, but it's enough to prevent using scripts for allowed tags.
        Replace allowed= to allowed\0.
        List of allowed attributes:
        action, alt, border, checked, class, clear, color, cols, colspan, controls, coords, data-\w+, datetime, dir, disabled, enctype, for, frameborder, headers, height, hidden, href, hreflang, id, ismap, label, lang, loop, marginheight, marginwidth, maxlength, method, multiple, name, pattern, preload, readonly, rel, required, reversed, rows, rowspan, sandbox, scrolling, seamless, spellcheck, src, start, target, title, type, typography, usemap, value, width
        */
        {
            r: /([ \n\t](?:a(?:c(?:cept|tion)|lt)|border|c(?:hecked|l(?:ass|ear)|o(?:l(?:or|s(?:|pan))|ntrols|ords))|d(?:at(?:a-\w+|etime)|i(?:sabled|r))|enctype|f(?:or|rameborder)|h(?:e(?:aders|ight)|idden|ref(?:|lang))|i(?:smap|d)|l(?:a(?:bel|ng)|oop)|m(?:a(?:rgin(?:height|width)|x(?:|length))|ethod|in|ultiple)|name|p(?:attern|laceholder|reload)|r(?:e(?:adonly|quired|versed|l)|ows(?:|pan))|s(?:andbox|crolling|eamless|ize|pellcheck|rc|t(?:art|ep))|t(?:arget|itle|yp(?:ography|e))|usemap|value|width)[ \r\n\t]*)=(?=[ \r\n\t]*['"])/gi,
            x: '$1\u0000'
        },
        /*
        4. Replace all equals-sign characters to HTML encoded.
        */
        {
            r: /[=]/g,
            x: '&#61;'
        },
        /*
        5. Replace null-byte characters back to equals-sign, now we are sure than HTML contains only allowed attributes with values.
        */
        {
            r: /\u0000/g,
            x: '='
        },
        /*
        6. Some of allowed attributes can have URL with forbidden scheme like <a href="javascript:script"> so we check that that attributes have allowed scheme, if not, just add relative path <a href="./javascript:script">
        List of URL attributes:
        action|formaction|href|poster|src
        URL can starts from:
        1. #\/.?0-9
        2. HTML encoded version of #\/.? is &#x23;|&#35;|&#x2f;|&#47;|&#x2e;|&#46;|&#x3f;|&#63;
        3. mailto|http|ftp
        4. about:blank
        5. data:image/(gif|jpg|jpeg|png);base64,
        */
        {
            r: /([ \n\t](?:(?:form)?action|href|poster|src)[ \r\n\t]*=[ \r\n\t]*['"][ \r\n\t]*)(?![ \r\n\t]*(:?[#\/.?0-9]|&#(?:35|4[67]|63|x(?:2[3ef]|3f));|(?:mailto|http|ftp)|about(?::|&#58;)blank|data(?::|&#58;)image(?:\/|&#47;)(?:gif|jpg|jpeg|png)(?:;|&#59;)base64(?:,|&#44;)))/gi,
            x: '$1./'
        }
        ];
        var l = steps.length;
        return function (html) {
            message = null;
            for (var i = 0; i < l; i++) {
                html = html.replace(steps[i].r, steps[i].x);
            }
            if (message !== null) {
                warn(message + ':\n' + html);
            }
            return html;
        };
    }());
    /**
    * XSS.toStaticHTML sanitizes HTML and prepares it for innerHTML.
    * @function toStaticHTML
    * @memberof XSS
    * @param {string} html - any HTML which should be sanitized
    * @param {object} data - use {{foo.bar}} as placeholder for data object {foo:{bar:'text'}}
    * @returns {string} static HTML
    */
    var toStaticHTML = function (html, data) {
        return sanitizeHTML(replacePlaceholders(html, data, encodeHTML));
    };
    var TEXT_CONTENT = ('textContent' in document.createElement('span')) ? 'textContent' : 'innerText';
    /**
    * XSS.renderHTML sanitizes HTML and sets innerHTML.<br />
    * {{foo.bar}} replaces with XSS.encodeHTML(data.foo.bar).<br />
    * {{{foo.bar}}} replaces with XSS.encodeHTML(data.foo.bar, &#47;* preventDoubleEncoding *&#47; true).<br />
    * Replacement data (data.foo.bar) can be only string or number.
    * @function renderHTML
    * @memberof XSS
    * @param {element} container - parent object like container.innerHTML
    * @param {string} html - any HTML which should be sanitized
    * @param {object} data - use {{foo.bar}} as placeholder for data object {foo:{bar:'text'}}
    * @returns {element} container
    */
    var renderHTML = function (container, html, data) {
        if (html === undefined || html === null || html === '') {
            container[TEXT_CONTENT] = '';
        } else {
            container.innerHTML = toStaticHTML(html, data);
        }
        return container;
    };
    var XSS = {
        warn: warn,
        encodeHTML: encodeHTML,
        toStaticHTML: toStaticHTML,
        renderHTML: renderHTML,
        TEXT_CONTENT: TEXT_CONTENT
    };
    return XSS;
}());
