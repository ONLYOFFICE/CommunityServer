/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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