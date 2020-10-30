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


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.UI;

namespace ASC.Web.Studio.ThirdParty.ImportContacts
{
    public partial class ImportLogin : MasterPage
    {
        private readonly List<ContactInfo> _contacts = new List<ContactInfo>();

        public static string EncodeJsString(string s)
        {
            var sb = new StringBuilder();
            sb.Append("\"");
            foreach (var c in s)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        var i = (int)c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append("\"");

            return sb.ToString();
        }

        private const string CallbackJavascript =
            @"function snd(){{client.sendAndClose({0},{1});}} window.onload = snd;";

        public void AddContactInfo(string name, string emails)
        {
            var lastname = string.Empty;
            if (!string.IsNullOrEmpty(name))
            {
                name = name.Trim();
                if (name.Contains(' '))
                {
                    lastname = name.Substring(name.IndexOf(' ') + 1);
                    name = name.Substring(0, name.IndexOf(' '));
                }
            }

            AddContactInfo(name, lastname, emails);
        }

        public void AddContactInfo(string name, string lastname, string emails)
        {
            if (String.IsNullOrEmpty(name) && String.IsNullOrEmpty(lastname) && !String.IsNullOrEmpty(emails))
            {
                var _name = emails.Contains("@") ? emails.Substring(0, emails.IndexOf("@")).Split('.') : emails.Split('.');
                if (_name.Length > 1)
                {
                    name = _name[0];
                    lastname = _name[1];
                }
            }

            var info = new ContactInfo
            {
                FirstName = String.IsNullOrEmpty(name) ? String.Empty : name,
                Email = String.IsNullOrEmpty(emails) ? String.Empty : emails,
                LastName = String.IsNullOrEmpty(lastname) ? String.Empty : lastname
            };

            if (!string.IsNullOrEmpty(info.Email))
            {
                _contacts.Add(info);
            }
        }

        protected void SubmitData(string data, string errorMessage = null)
        {
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "posttoparent",
                                                   string.Format(CallbackJavascript,
                                                                 string.IsNullOrEmpty(data) ? "null" : data,
                                                                 string.IsNullOrEmpty(errorMessage) ? "null" : EncodeJsString(errorMessage)),
                                                   true);
        }

        public void SubmitContacts()
        {
            SubmitData(ToJson(_contacts.Distinct().ToList()));
        }

        public void SubmitError(string message)
        {
            SubmitData(string.Empty, message);
        }

        protected static string ToJson(object obj)
        {
            var serializer = new DataContractJsonSerializer(obj.GetType());
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }
    }
}