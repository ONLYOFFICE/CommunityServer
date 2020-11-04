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


window.ASC.Files.TemplateManager = (function () {
    var xslTemplate = null;

    var getTemplate = function () {
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

    try {
        var test1 = new ActiveXObject('Microsoft.XMLDOM');
        var test2 = new ActiveXObject('Microsoft.XMLHTTP');
        var supportActiveXObject = test1 && test2;
    } catch (e) {
        supportActiveXObject = false;
    }

    var createXML = (function () {
        if (supportActiveXObject) {
            return function (data) {
                if (typeof data !== 'string' || data.length === 0) {
                    return undefined;
                }
                var xmlDoc = new ActiveXObject('Microsoft.XMLDOM');
                xmlDoc.async = 'false';
                xmlDoc.loadXML(data);
                if (xmlDoc.parseError.errorCode != 0) {
                    throw 'Can\'t create xml document';
                }
                return xmlDoc;
            };
        }

        return function (data) {
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
    })();

    var xmlTranslate = (function () {
        if (supportActiveXObject) {
            return function (xml, xsl) {
                var xmlstr = '';
                if (typeof xml === 'undefined' || xml == null || typeof xsl === 'undefined' || xsl == null) {
                    return xmlstr;
                }
                try {
                    xsl.resolveExternals = true;
                } catch (err) {
                }
                try {
                    try {
                        xmlstr = xml.transformNode(xsl);
                    } catch(ex) {
                        xmlstr = transformXml(xml, xsl);
                    }
                } catch (err) {
                    throw 'Can\'t translate xml : ' + err;
                }
                return xmlstr;
            };
        }

        function transformXml(xmlDoc, xsltDoc) {
            var xml = new ActiveXObject("Microsoft.XMLDOM");
            var xslt = new ActiveXObject("MSXML2.FreeThreadedDOMDocument");
            
            xml.load(xmlDoc);
            xslt.load(xsltDoc);

            var processor = new ActiveXObject("Msxml2.XSLTemplate");
            processor.stylesheet = xslt;

            var objXsltProc = processor.createProcessor();
            objXsltProc.input = xml;
            objXsltProc.transform();
            
            return objXsltProc.output;
        }

        return function (xml, xsl) {
            var xmlstr = '';
            if (typeof xml === 'undefined' || typeof xsl === 'undefined') {
                return xmlstr;
            }
            var xmlDocument;
            var xsltProcessor;
            var xmlSerializer;
            try {
                xsltProcessor = new XSLTProcessor();
                xsltProcessor.importStylesheet(xsl);
                xmlDocument = xsltProcessor.transformToFragment(xml, document);
            } catch (err) {
                throw 'Can\'t translate xml : ' + err;
            }
            try {
                xmlSerializer = new XMLSerializer();
                xmlstr = xmlSerializer.serializeToString(xmlDocument);
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