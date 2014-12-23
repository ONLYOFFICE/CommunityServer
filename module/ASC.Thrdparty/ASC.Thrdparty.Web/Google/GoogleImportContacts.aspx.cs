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
                SubmitData();
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                ErrorScope = ex.Message;
                SubmitData();
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