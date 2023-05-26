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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Web;

using ASC.FederatedLogin;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.Web.Core;
using ASC.Web.Studio.Utility;

using Newtonsoft.Json;

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
            var response = RequestContacts(token);

            if (response != null && response.Connections != null)
            {
                foreach (var contact in response.Connections)
                {
                    string name = null;
                    if (contact.Names != null)
                    {
                        var first = contact.Names.FirstOrDefault(contactName => !string.IsNullOrEmpty(contactName.displayName));
                        if (first != null)
                        {
                            name = first.displayName;
                        }
                    }
                    string email = null;
                    if (contact.EmailAddresses != null)
                    {
                        var first = contact.EmailAddresses.FirstOrDefault(contactEmail => !string.IsNullOrEmpty(contactEmail.value));
                        if (first != null)
                        {
                            email = first.value;
                        }
                    }

                    Master.AddContactInfo(name, email);
                }
            }
        }

        protected GoogleContacts RequestContacts(OAuth20Token token)
        {
            var response = RequestHelper.PerformRequest(GoogleLoginProvider.GoogleUrlContacts, headers: new Dictionary<string, string> { { "Authorization", "Bearer " + token.AccessToken } });

            var contacts = JsonConvert.DeserializeObject<GoogleContacts>(response);
            return contacts;
        }


        [DataContract(Name = "GoogleContact", Namespace = "")]
        protected class GoogleContacts
        {
            [DataMember(Name = "connections")]
            public List<People> Connections;

            [DataContract(Name = "People", Namespace = "")]
            public class People
            {
                [DataMember(Name = "emailAddresses")]
                public List<EmailAddress> EmailAddresses;

                [DataMember(Name = "names")]
                public List<Name> Names;


                [DataContract(Name = "EmailAddress", Namespace = "")]
                public class EmailAddress
                {
                    [DataMember(Name = "value")]
                    public string value;
                }

                [DataContract(Name = "Name", Namespace = "")]
                public class Name
                {
                    [DataMember(Name = "displayName")]
                    public string displayName;
                }
            }
        }
    }
}