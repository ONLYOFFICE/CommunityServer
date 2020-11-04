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
using System.Linq;
using System.Web;
using System.Xml.Linq;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.Web.Core;
using ASC.Web.Studio.Utility;
using Newtonsoft.Json.Linq;

namespace ASC.Web.Studio.ThirdParty.ImportContacts
{
    public partial class Yahoo : BasePage
    {
        public static string Location
        {
            get { return CommonLinkUtility.ToAbsolute("~/ThirdParty/ImportContacts/Yahoo.aspx"); }
        }

        public static bool Enable
        {
            get { return YahooLoginProvider.Instance.IsEnabled; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var token = YahooLoginProvider.Instance.Auth(HttpContext.Current);

                var userGuid = RequestUserGuid(token.AccessToken);

                ImportContacts(userGuid, token.AccessToken);

                Master.SubmitContacts();
            }
            catch (System.Threading.ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                Master.SubmitError(ex.Message);
            }
        }

        private void ImportContacts(string userGuid, string accessToken)
        {
            var responseString = RequestHelper.PerformRequest(string.Format(YahooLoginProvider.YahooUrlContactsFormat, userGuid),
                                                              headers: new Dictionary<string, string> { { "Authorization", "Bearer " + accessToken } });

            const string xmlns = "http://social.yahooapis.com/v1/schema.rng";

            var contactsDocument = XDocument.Parse(responseString);
            var contacts = contactsDocument.Root.Elements(XName.Get("contact", xmlns))
                .Select(entry => new
                {
                    Name = entry.Elements(XName.Get("fields", xmlns))
                        .Where(field => field.Element(XName.Get("type", xmlns)).Value == "name")
                        .Select(field => field.Element(XName.Get("value", xmlns)).Element(XName.Get("givenName", xmlns)).Value).FirstOrDefault(),
                    LastName = entry.Elements(XName.Get("fields", xmlns))
                        .Where(field => field.Element(XName.Get("type", xmlns)).Value == "name")
                        .Select(field => field.Element(XName.Get("value", xmlns)).Element(XName.Get("familyName", xmlns)).Value).FirstOrDefault(),
                    Email = entry.Elements(XName.Get("fields", xmlns))
                        .Where(field => field.Element(XName.Get("type", xmlns)).Value == "email")
                        .Select(field => field.Element(XName.Get("value", xmlns)).Value).FirstOrDefault(),
                }).ToList();
            foreach (var contact in contacts)
            {
                if (String.IsNullOrEmpty(contact.Email))
                {
                    Master.AddContactInfo(contact.Name, contact.LastName, "");
                }
                else
                {
                    Master.AddContactInfo(contact.Name, contact.LastName, contact.Email);
                }
            }
        }

        public static string RequestUserGuid(string accessToken)
        {
            var responseString = RequestHelper.PerformRequest(YahooLoginProvider.YahooUrlUserGuid + "?format=json",
                                                              headers: new Dictionary<string, string> { { "Authorization", "Bearer " + accessToken } });

            var response = JObject.Parse(responseString);
            return response == null ? null : response["guid"].Value<string>("value");
        }
    }
}