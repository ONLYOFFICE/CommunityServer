/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ASC.Thrdparty.Web
{
    public class BaseImportPage : System.Web.UI.Page
    {
        private readonly List<ContactInfo> _contacts = new List<ContactInfo>();

        public static string EncodeJsString(string s)
        {
            var sb = new StringBuilder();
            sb.Append("\"");
            foreach (char c in s)
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
                        int i = (int)c;
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

        protected string ErrorScope { get; set; }

        protected void AddContactInfo(string name, IEnumerable<string> emails)
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

        protected void AddContactInfo(string name, string lastname, IEnumerable<string> emails)
        {
            if (String.IsNullOrEmpty(name) && String.IsNullOrEmpty(lastname))
            {
                var _name = emails.FirstOrDefault().Contains("@") ? emails.FirstOrDefault().Substring(0, emails.FirstOrDefault().IndexOf("@")).Split('.') : emails.FirstOrDefault().Split('.');
                if (_name.Length > 1)
                {
                    name = _name[0];
                    lastname = _name[1];
                }
            }
            AddContactInfo(new ContactInfo { FirstName = String.IsNullOrEmpty(name) ? String.Empty : name, Email = String.IsNullOrEmpty(emails.FirstOrDefault()) ? String.Empty : emails.FirstOrDefault(), LastName = String.IsNullOrEmpty(lastname) ? String.Empty : lastname });
        }

        protected void AddContactInfo(string name, string email)
        {
            var _name = name.Split(' ');
            if (_name.Length > 1)
                AddContactInfo(_name[0], _name[1], email);
            else
                AddContactInfo(name, string.Empty, email);
        }

        protected void AddContactInfo(string name, string lastname, string email)
        {
            if (String.IsNullOrEmpty(name) && String.IsNullOrEmpty(lastname))
            {
                var _name = email.Contains("@") ? email.Substring(0, email.IndexOf("@")).Split('.') : email.Split('.');
                if (_name.Length > 1)
                {
                    name = _name[0];
                    lastname = _name[1];
                }                
            }
            AddContactInfo(new ContactInfo() { FirstName = name, Email = email, LastName = lastname });
        }

        protected void AddContactInfo(ContactInfo info)
        {
            if (!string.IsNullOrEmpty(info.Email))
            {
                _contacts.Add(info);
            }
        }

        protected void AddContactInfo(IEnumerable<ContactInfo> contacts)
        {
            if (contacts != null)
            {
                _contacts.AddRange(contacts);
            }
        }

        protected void SubmitData()
        {
            ClientScript.RegisterClientScriptBlock(GetType(), "posttoparent",
                string.Format(CallbackJavascript, JsonContacts(), string.IsNullOrEmpty(ErrorScope) ? "null" : EncodeJsString(ErrorScope)),
                true);
        }

        protected void SubmitEmailInfo(EmailAccessInfo emailInfo)
        {
            ClientScript.RegisterClientScriptBlock(GetType(), "posttoparent",
                string.Format(CallbackJavascript, JsonEmailAccess(emailInfo), string.IsNullOrEmpty(ErrorScope) ? "null" : EncodeJsString(ErrorScope)),
                true);
        }

        protected string JsonContacts()
        {
            return JsonContacts(_contacts.Distinct().ToList());
        }

        private static string JsonContacts(List<ContactInfo> contacts)
        {
            /*var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
		    return serializer.Serialize(contacts);*/
            var serializer = new DataContractJsonSerializer(contacts.GetType());
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, contacts);
                return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }

        private static string JsonEmailAccess(EmailAccessInfo emailInfo)
        {
            var serializer = new DataContractJsonSerializer(emailInfo.GetType());
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, emailInfo);
                return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }
    }
}