/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


window.ASC.Files.TemplateManager = (function () {
    var xslTemplate = null;

    var getTemplate = function () {
        // the template comes with the page scripts (FilesTemplateResources)
        if (!xslTemplate && ASC.Files.Templates && ASC.Files.Templates.collection) {
            xslTemplate = createXML(ASC.Files.Templates.collection);
        }

        // fallback for a page that does not register the template script
        if (!xslTemplate) {
            jq.ajax({
                url: ASC.Files.Constants.URL_TEMPLATES_HANDLER,
                async: false,
                success: function (data) {
                    xslTemplate = data;
                },
            });
        }

        return xslTemplate;
    };

    var translate = function (xmlData) {
        var xslData = getTemplate();
        return xmlTranslate(xmlData, xslData);
    };

    var translateFromString = function (stringData) {
        if (typeof stringData === 'undefined') {
            return '';
        }

        var xmlData = stringData;
        if (typeof stringData === 'string') {
            xmlData = createXML(stringData);
        }

        var xslData = getTemplate();
        return xmlTranslate(xmlData, xslData);
    };

    var createXML = function (data) {
        if (typeof data !== 'string' || data.length === 0) {
            return undefined;
        }
        var xmlDoc = new DOMParser();
        try {
            xmlDoc = xmlDoc.parseFromString(data, 'text/xml');
        } catch (err) {
            throw 'Can\'t create xml document : ' + err;
        }
        return xmlDoc;
    };

    /*
     * Browsers are removing XSLT (XSLTProcessor is gone from Chrome), so the
     * templates are applied here. Only the part of XSLT 1.0 used by
     * Products/Files/Templates/*.xsl is supported: xsl:template with match
     * by element name, value-of, attribute, if, choose/when/otherwise,
     * for-each and text. The XPath expressions are evaluated by the browser
     * (document.evaluate), which is not being removed.
     */
    var xmlTranslate = (function () {
        var XSL_NS = 'http://www.w3.org/1999/XSL/Transform';
        var XMLNS_NS = 'http://www.w3.org/2000/xmlns/';
        var xpathEvaluator = new XPathEvaluator();
        var expressions = {};

        // XSLT treats only these characters as whitespace (not &#160;)
        var isWhitespace = function (text) {
            return !/[^ \t\r\n]/.test(text);
        };

        var evaluate = function (expr, context, type) {
            var compiled = expressions[expr];
            if (!compiled) {
                compiled = expressions[expr] = xpathEvaluator.createExpression(expr, null);
            }
            return compiled.evaluate(context, type, null);
        };

        var evaluateString = function (expr, context) {
            return evaluate('string(' + expr + ')', context, XPathResult.STRING_TYPE).stringValue;
        };

        var evaluateBoolean = function (expr, context) {
            return evaluate(expr, context, XPathResult.BOOLEAN_TYPE).booleanValue;
        };

        var getTemplates = function (xsl) {
            var stylesheet = xsl.nodeType === 9 ? xsl.documentElement : xsl;
            var templates = {};
            for (var node = stylesheet.firstChild; node; node = node.nextSibling) {
                if (node.nodeType === 1 && node.namespaceURI === XSL_NS && node.localName === 'template') {
                    var match = node.getAttribute('match');
                    if (match && !templates.hasOwnProperty(match)) {
                        templates[match] = node;
                    }
                }
            }
            return templates;
        };

        // xsl:apply-templates with the built-in rules for nodes without a template
        var applyTemplates = function (node, templates, output) {
            switch (node.nodeType) {
                case 1:
                    if (templates.hasOwnProperty(node.nodeName)) {
                        processChildren(templates[node.nodeName], node, output);
                        return;
                    }
                    // falls through
                case 9:
                case 11:
                    for (var child = node.firstChild; child; child = child.nextSibling) {
                        applyTemplates(child, templates, output);
                    }
                    return;
                case 3:
                case 4:
                    output.appendChild(document.createTextNode(node.nodeValue));
                    return;
            }
        };

        var processChildren = function (instruction, context, output) {
            for (var node = instruction.firstChild; node; node = node.nextSibling) {
                switch (node.nodeType) {
                    case 1:
                        if (node.namespaceURI === XSL_NS) {
                            processInstruction(node, context, output);
                        } else {
                            processLiteralElement(node, context, output);
                        }
                        break;
                    case 3:
                    case 4:
                        if (!isWhitespace(node.nodeValue)) {
                            output.appendChild(document.createTextNode(node.nodeValue));
                        }
                        break;
                }
            }
        };

        var processLiteralElement = function (node, context, output) {
            var element = document.createElement(node.localName);
            for (var i = 0; i < node.attributes.length; i++) {
                var attribute = node.attributes[i];
                if (attribute.namespaceURI !== XMLNS_NS && attribute.name !== 'xmlns') {
                    if (attribute.value.indexOf('{') !== -1) {
                        throw 'Attribute value templates are not supported: ' + attribute.value;
                    }
                    element.setAttribute(attribute.name, attribute.value);
                }
            }
            processChildren(node, context, element);
            output.appendChild(element);
        };

        var processInstruction = function (node, context, output) {
            switch (node.localName) {
                case 'value-of':
                    var value = evaluateString(node.getAttribute('select'), context);
                    if (value) {
                        output.appendChild(document.createTextNode(value));
                    }
                    break;

                case 'text':
                    if (node.textContent) {
                        output.appendChild(document.createTextNode(node.textContent));
                    }
                    break;

                case 'attribute':
                    if (output.nodeType === 1) {
                        var content = document.createDocumentFragment();
                        processChildren(node, context, content);
                        output.setAttribute(node.getAttribute('name'), content.textContent);
                    }
                    break;

                case 'if':
                    if (evaluateBoolean(node.getAttribute('test'), context)) {
                        processChildren(node, context, output);
                    }
                    break;

                case 'choose':
                    for (var branch = node.firstChild; branch; branch = branch.nextSibling) {
                        if (branch.nodeType !== 1 || branch.namespaceURI !== XSL_NS) {
                            continue;
                        }
                        if (branch.localName === 'otherwise'
                            || branch.localName === 'when' && evaluateBoolean(branch.getAttribute('test'), context)) {
                            processChildren(branch, context, output);
                            break;
                        }
                    }
                    break;

                case 'for-each':
                    var nodes = evaluate(node.getAttribute('select'), context, XPathResult.ORDERED_NODE_SNAPSHOT_TYPE);
                    for (var i = 0; i < nodes.snapshotLength; i++) {
                        processChildren(node, nodes.snapshotItem(i), output);
                    }
                    break;

                default:
                    throw 'Unsupported instruction xsl:' + node.localName;
            }
        };

        return function (xml, xsl) {
            var xmlstr = '';
            if (typeof xml === 'undefined' || xml == null || typeof xsl === 'undefined' || xsl == null) {
                return xmlstr;
            }
            var fragment = document.createDocumentFragment();
            try {
                applyTemplates(xml, getTemplates(xsl), fragment);
            } catch (err) {
                throw 'Can\'t translate xml : ' + err;
            }
            try {
                xmlstr = jq('<div>').append(fragment).html();
            } catch (err) {
                throw 'Can\'t serialized xml : ' + err;
            }
            return xmlstr;
        };
    })();

    return {
        createXML: createXML,

        translate: translate,
        translateFromString: translateFromString
    };
})();
