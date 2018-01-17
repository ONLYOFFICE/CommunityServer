/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
            get { return CommonLinkUtility.ToAbsolute("~/thirdparty/importcontacts/yahoo.aspx"); }
        }

        public static bool Enable
        {
            get { return !(string.IsNullOrEmpty(YahooLoginProvider.YahooOAuth20ClientId) || string.IsNullOrEmpty(YahooLoginProvider.YahooOAuth20ClientSecret) || string.IsNullOrEmpty(YahooLoginProvider.YahooOAuth20RedirectUrl)); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var token = YahooLoginProvider.Auth(HttpContext.Current, YahooLoginProvider.YahooScopeContacts);

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
            var contacts = from entry in contactsDocument.Root.Elements(XName.Get("contact", xmlns))
                           select
                               new
                               {
                                   Name = from field in entry.Elements(XName.Get("fields", xmlns))
                                          where field.Element(XName.Get("type", xmlns)).Value == "name"
                                          select field.Element(XName.Get("value", xmlns)).Element(XName.Get("givenName", xmlns)).Value,
                                   LastName = from field in entry.Elements(XName.Get("fields", xmlns))
                                              where field.Element(XName.Get("type", xmlns)).Value == "name"
                                              select field.Element(XName.Get("value", xmlns)).Element(XName.Get("familyName", xmlns)).Value,
                                   Email = from field in entry.Elements(XName.Get("fields", xmlns))
                                           where field.Element(XName.Get("type", xmlns)).Value == "email"
                                           select field.Element(XName.Get("value", xmlns)).Value,
                               };
            foreach (var contact in contacts)
            {
                Master.AddContactInfo(contact.Name.FirstOrDefault(), contact.LastName.FirstOrDefault(), contact.Email);
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