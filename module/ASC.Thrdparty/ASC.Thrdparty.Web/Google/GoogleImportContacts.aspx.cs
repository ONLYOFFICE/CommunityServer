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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml.Linq;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;

namespace ASC.Thrdparty.Web.Google
{
    public partial class GoogleImportContacts : BaseImportPage
    {
        private const string GoogleContactsUrl = "https://www.google.com/m8/feeds/contacts/default/full/";
        private const string GoogleContactsScope = "https://www.googleapis.com/auth/contacts.readonly";

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var token = GoogleLoginProvider.Auth(HttpContext.Current, GoogleContactsScope);

                ImportContacts(token);
                SubmitContacts();
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                SubmitError(ex.Message);
            }
        }

        private void ImportContacts(OAuth20Token token)
        {
            var doc = RequestContacts(token);

            //selecting from xdocument
            var contacts = from e in doc.Root.Elements("{http://www.w3.org/2005/Atom}entry")
                           select new
                               {
                                   Name = e.Element("{http://www.w3.org/2005/Atom}title").Value,
                                   Email = from a in e.Elements("{http://schemas.google.com/g/2005}email")
                                           where a.Attribute("address") != null
                                           select a.Attribute("address").Value
                               };
            foreach (var contact in contacts)
            {
                AddContactInfo(contact.Name, contact.Email);
            }
        }

        public XDocument RequestContacts(OAuth20Token token)
        {
            var response = RequestHelper.PerformRequest(GoogleContactsUrl, headers: new Dictionary<string, string> { { "Authorization", "Bearer " + token.AccessToken } });

            return XDocument.Parse(response);
        }
    }
}