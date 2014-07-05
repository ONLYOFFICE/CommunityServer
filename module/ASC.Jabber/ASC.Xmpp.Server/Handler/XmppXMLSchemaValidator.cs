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

using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Server.Handler
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using log4net;
    using Storage;
    using Streams;


    public class XmppXMLSchemaValidator
    {
        private static readonly Dictionary<string, XmlSchema> schemaSet = new Dictionary<string, XmlSchema>();
        private static readonly ILog log = LogManager.GetLogger(typeof(XmppXMLSchemaValidator));

        public XmppXMLSchemaValidator()
        {
#if (SCHEME)
            log.Debug("loading xsd");
            if (schemaSet.Count == 0)
            {
                LoadSchemes();
            }
#endif

        }

        private void LoadSchemes()
        {
            string schemaDir = Path.GetFullPath(".\\schemas\\");
            if (Directory.Exists(schemaDir))
            {
                string[] schemas = Directory.GetFiles(schemaDir, "*.xsd", SearchOption.TopDirectoryOnly);
                foreach (var schemaPath in schemas)
                {
                    //Load
                    try
                    {
                        using (TextReader reader = File.OpenText(schemaPath))
                        {
                            XmlSchema schema = XmlSchema.Read(reader, XmppSchemeValidationEventHandler);
                            if (!schemaSet.ContainsKey(schema.TargetNamespace))
                            {
#pragma warning disable 618,612
                                schema.Compile(XmppSchemeValidationEventHandler);
#pragma warning restore 618,612
                                schemaSet.Add(schema.TargetNamespace, schema);

                            }
                        }
                    }
                    catch (Exception)
                    {
                        log.ErrorFormat("error loading scheme {0}", schemaPath);
                    }
                }
            }
        }

       

        private void XmppSchemeValidationEventHandler(object sender, ValidationEventArgs e)
        {
            //log.DebugFormat("{1} validation:{0}", e.Message, e.Severity);
        }


        public bool ValidateNode(Node node, XmppStream stream, XmppHandlerContext handlercontext)
        {
#if (SCHEME)
            var stopwatch = new Stopwatch();
            if (log.IsDebugEnabled)
            {
                stopwatch.Reset();
                stopwatch.Start();
            }
            int  result = ValidateNodeInternal(node);
            if (log.IsDebugEnabled)
            {
                stopwatch.Stop();
                log.DebugFormat("Node validated. Error count: {1}. time: {0}ms", stopwatch.ElapsedMilliseconds, result);
            }
            return result==0;
#else
            return true;
#endif
            
        }


        private int ValidateNodeInternal(Node node)
        {
            int[] errorcount = {0};
            try
            {

                if (node is Element)
                {
                    List<XmlSchema> schemasUsed = new List<XmlSchema>();
                    AddSchemas(node as Element, schemasUsed);
                    if (schemasUsed.Count > 0)
                    {
                        using (StringReader reader = new StringReader(node.ToString()))
                        {
                            StringBuilder validationErrors = new StringBuilder();
                            XmlReaderSettings xmppSettings = new XmlReaderSettings
                                                                 {ValidationType = ValidationType.Schema};
                            xmppSettings.ValidationEventHandler += (x, y) =>
                                                                       {
                                                                           validationErrors.AppendLine(
                                                                               GetErrorString(x, y));
                                                                           errorcount[0]++;
                                                                       };
                        
                        
                            foreach (XmlSchema schema in schemasUsed)
                            {
                                xmppSettings.Schemas.Add(schema);
                            }


                            XmlReader validator = XmlReader.Create(reader, xmppSettings);
                            bool bContinue = true;
                            while (bContinue)
                            {
                                try
                                {

                                    bContinue = validator.Read();
                                }
                                catch (Exception ex)
                                {
                                    errorcount[0]++;
                                    validationErrors.AppendLine(ex.Message);
                                }
                            }
                            if (validationErrors.Length > 0)
                            {
                                log.DebugFormat("validation summary:{1} {0}", validationErrors.ToString(), Environment.NewLine);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return errorcount[0];
        }

        private string GetErrorString(object sender, ValidationEventArgs args)
        {
            return string.Format("{0} at line {1} symbol {2}. Namespace {3}",
                                 args.Message,
                                 args.Exception.LineNumber,
                                 args.Exception.LinePosition, ((XmlReader)sender).NamespaceURI);
        }


        private void AddSchemas(Element element, List<XmlSchema> schemasUsed)
        {
            if (schemaSet.ContainsKey(element.Namespace))
            {
                if (!schemasUsed.Contains(schemaSet[element.Namespace]))
                {
                    schemasUsed.Add(schemaSet[element.Namespace]);
                }
            }
            if (element.HasChildElements)
            {
                foreach (Node childNode in element.ChildNodes)
                {
                    if (childNode is Element)
                    {
                        AddSchemas(childNode as Element, schemasUsed);
                    }
                }
            }
        }
    }
}