/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using System;
using System.Collections;
using System.Globalization;
using ASC.Xmpp.Core.utils;
using ASC.Xmpp.Core.utils.Xml.Dom;
using ASC.Xmpp.Core.utils.exceptions;

namespace ASC.Xmpp.Core.protocol.iq.rpc
{
    /// <summary>
    ///   The method Response element.
    /// </summary>
    public class MethodResponse : Element
    {
        /*
         
         <methodResponse>
           <fault>
              <value>
                 <struct>
                    <member>
                       <name>faultCode</name>
                       <value><int>4</int></value>
                       </member>
                    <member>
                       <name>faultString</name>
                       <value><string>Too many parameters.</string></value>
                       </member>
                    </struct>
                 </value>
              </fault>
           </methodResponse>
         
         */

        public MethodResponse()
        {
            TagName = "methodResponse";
            Namespace = Uri.IQ_RPC;
        }

        /// <summary>
        ///   Parses the XML-RPC resonse and returns an ArrayList with all Parameters. In there is an XML-RPC Error it returns an XmlRpcException as single parameter in the ArrayList.
        /// </summary>
        /// <returns> Arraylist with parameters, or Arraylist with an exception </returns>
        public ArrayList GetResponse()
        {
            return ParseResponse();
        }

        /// <summary>
        ///   parse the response
        /// </summary>
        /// <returns> </returns>
        private ArrayList ParseResponse()
        {
            var al = new ArrayList();

            // If an error occurred, the server will return fault
            Element fault = SelectSingleElement("fault");
            if (fault != null)
            {
                Hashtable ht = ParseStruct(fault.SelectSingleElement("struct", true));
                al.Add(new XmlRpcException((int) ht["faultCode"], (string) ht["faultString"]));
            }
            else
            {
                Element elParams = SelectSingleElement("params");
                ElementList nl = elParams.SelectElements("param");


                foreach (Element p in nl)
                {
                    Element value = p.SelectSingleElement("value");
                    if (value != null)
                        al.Add(ParseValue(value));
                }
            }
            return al;
        }

        /// <summary>
        ///   Parse a single response value
        /// </summary>
        /// <param name="value"> </param>
        /// <returns> </returns>
        private object ParseValue(Element value)
        {
            object result = null;

            if (value != null)
            {
                if (value.HasChildElements)
                {
                    Element next = value.FirstChild;
                    if (next.TagName == "string")
                        result = next.Value;
                    else if (next.TagName == "boolean")
                        result = next.Value == "1" ? true : false;
                    else if (next.TagName == "i4")
                        result = Int32.Parse(next.Value);
                    else if (next.TagName == "int") // occurs in fault
                        result = int.Parse(next.Value);
                    else if (next.TagName == "double")
                    {
                        var numberInfo = new NumberFormatInfo();
                        numberInfo.NumberDecimalSeparator = ".";
                        result = Double.Parse(next.Value, numberInfo);
                    }
                    else if (next.TagName == "dateTime.iso8601")
                        result = Time.ISO_8601Date(next.Value);
                    else if (next.TagName == "base64")
                        result = Convert.FromBase64String(next.Value);
                    else if (next.TagName == "struct")
                        result = ParseStruct(next);
                    else if (next.TagName == "array")
                        result = ParseArray(next);
                }
                else
                {
                    result = value.Value;
                }
            }
            return result;
        }

        /// <summary>
        ///   parse a response array
        /// </summary>
        /// <param name="elArray"> </param>
        /// <returns> </returns>
        private ArrayList ParseArray(Element elArray)
        {
            //<array>
            //    <data>
            //        <value><string>one</string></value>
            //        <value><string>two</string></value>
            //        <value><string>three</string></value>
            //        <value><string>four</string></value>
            //        <value><string>five</string></value>
            //    </data>
            //</array>

            Element data = elArray.SelectSingleElement("data");
            if (data != null)
            {
                var al = new ArrayList();
                ElementList values = data.SelectElements("value");

                foreach (Element val in values)
                {
                    al.Add(ParseValue(val));
                }
                return al;
            }
            else
                return null;
        }

        /// <summary>
        ///   parse a response struct
        /// </summary>
        /// <param name="el"> </param>
        /// <returns> </returns>
        private Hashtable ParseStruct(Element el)
        {
            //<struct>
            //   <member>
            //      <name>x</name>
            //      <value><i4>20</i4></value>
            //   </member>
            //   <member>
            //      <name>y</name>
            //      <value><string>cow</string></value>
            //   </member>
            //   <member>
            //      <name>z</name>
            //      <value><double>3,14</double></value>
            //   </member>
            //</struct>

            var ht = new Hashtable();

            ElementList members = el.SelectElements("member");

            foreach (Element member in members)
            {
                string name = member.GetTag("name");

                // parse this member value
                Element value = member.SelectSingleElement("value");
                if (value != null)
                    ht[name] = ParseValue(value);
            }
            return ht;
        }
    }
}