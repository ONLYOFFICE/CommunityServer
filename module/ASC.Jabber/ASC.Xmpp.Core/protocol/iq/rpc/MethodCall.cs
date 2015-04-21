/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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

namespace ASC.Xmpp.Core.protocol.iq.rpc
{
    /// <summary>
    ///   The methodCall element.
    /// </summary>
    public class MethodCall : Element
    {
        /*
        
         <methodCall>
            <methodName>examples.getStateName</methodName>
            <params>
                <param><value><i4>41</i4></value></param>
            </params>
         </methodCall>        
         
        */

        /// <summary>
        /// </summary>
        public MethodCall()
        {
            TagName = "methodCall";
            Namespace = Uri.IQ_RPC;
        }

        /// <summary>
        /// </summary>
        /// <param name="methodName"> </param>
        /// <param name="Params"> </param>
        public MethodCall(string methodName, ArrayList Params) : this()
        {
            WriteCall(methodName, Params);
        }

        /// <summary>
        /// </summary>
        public string MethodName
        {
            set { SetTag("methodName", value); }
            get { return GetTag("methodName"); }
        }

        /// <summary>
        ///   Write the functions call with params to this Element
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="Params"> </param>
        public void WriteCall(string name, ArrayList Params)
        {
            MethodName = name;

            if (Params != null && Params.Count > 0)
            {
                // remote this tag if exists, in case this function gets
                // calles multiple times by some guys
                RemoveTag("params");
                var elParams = new Element("params");

                for (int i = 0; i < Params.Count; i++)
                {
                    var param = new Element("param");
                    WriteValue(Params[i], param);
                    elParams.AddChild(param);
                }
                AddChild(elParams);
            }
        }

        /// <summary>
        ///   Writes a single value to a call
        /// </summary>
        /// <param name="param"> </param>
        /// <param name="parent"> </param>
        private void WriteValue(object param, Element parent)
        {
            var value = new Element("value");

            if (param is String)
            {
                value.AddChild(new Element("string", param as string));
            }
            else if (param is Int32)
            {
                value.AddChild(new Element("i4", ((Int32) param).ToString()));
            }
            else if (param is Double)
            {
                var numberInfo = new NumberFormatInfo();
                numberInfo.NumberDecimalSeparator = ".";
                //numberInfo.NumberGroupSeparator = ",";
                value.AddChild(new Element("double", ((Double) param).ToString(numberInfo)));
            }
            else if (param is Boolean)
            {
                value.AddChild(new Element("boolean", ((bool) param) ? "1" : "0"));
            }
                // XML-RPC dates are formatted in iso8601 standard, same as xmpp,
            else if (param is DateTime)
            {
                value.AddChild(new Element("dateTime.iso8601", Time.ISO_8601Date((DateTime) param)));
            }
                // byte arrays must be encoded in Base64 encoding
            else if (param is byte[])
            {
                var b = (byte[]) param;
                value.AddChild(new Element("base64", Convert.ToBase64String(b, 0, b.Length)));
            }
                // Arraylist maps to an XML-RPC array
            else if (param is ArrayList)
            {
                //<array>  
                //    <data>
                //        <value>  <string>one</string>  </value>
                //        <value>  <string>two</string>  </value>
                //        <value>  <string>three</string></value>  
                //    </data> 
                //</array>
                var array = new Element("array");
                var data = new Element("data");

                var list = param as ArrayList;

                for (int i = 0; i < list.Count; i++)
                {
                    WriteValue(list[i], data);
                }

                array.AddChild(data);
                value.AddChild(array);
            }
                // java.util.Hashtable maps to an XML-RPC struct
            else if (param is Hashtable)
            {
                var elStruct = new Element("struct");

                var ht = (Hashtable) param;
                IEnumerator myEnumerator = ht.Keys.GetEnumerator();
                while (myEnumerator.MoveNext())
                {
                    var member = new Element("member");
                    object key = myEnumerator.Current;

                    member.AddChild(new Element("name", key.ToString()));
                    WriteValue(ht[key], member);

                    elStruct.AddChild(member);
                }

                value.AddChild(elStruct);
            }
            else
            {
                // Unknown Type
            }
            parent.AddChild(value);
        }
    }
}