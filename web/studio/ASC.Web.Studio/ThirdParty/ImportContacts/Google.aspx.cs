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
using System.Threading;
using System.Web;
using System.Xml.Linq;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.Web.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.ThirdParty.ImportContacts
{
    public partial class Google : BasePage
    {
        public static string Location
        {
            get { return CommonLinkUtility.ToAbsolute("~/ThirdParty/ImportContacts/Google.aspx"); }
        }

        public static bool Enable
        {
            get { return GoogleLoginProvider.Instance.IsEnabled; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var token = GoogleLoginProvider.Instance.Auth(HttpContext.Current);

                ImportContacts(token);
                Master.SubmitContacts();
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                Master.SubmitError(ex.Message);
            }
        }

        private void ImportContacts(OAuth20Token token)
        {
            var doc = RequestContacts(token);

            //selecting from xdocument
            var contacts = doc.Root.Elements("{http://www.w3.org/2005/Atom}entry")
                .Select(e => new
                {
                    Name = e.Element("{http://www.w3.org/2005/Atom}title").Value,
                    Email = e.Elements("{http://schemas.google.com/g/2005}email")
                        .Where(a => a.Attribute("address") != null)
                        .Select(a => a.Attribute("address").Value)
                        .FirstOrDefault()
                }).ToList();
            foreach (var contact in contacts)
            {
                Master.AddContactInfo(contact.Name, contact.Email);
            }
        }

        public XDocument RequestContacts(OAuth20Token token)
        {
            var response = RequestHelper.PerformRequest(GoogleLoginProvider.GoogleUrlContacts, headers: new Dictionary<string, string> { { "Authorization", "Bearer " + token.AccessToken } });

            return XDocument.Parse(response);
        }
    }
}