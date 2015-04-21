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


using ASC.Thrdparty.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using WindowsLive;

namespace ASC.Thrdparty.Web.Live
{
    public partial class WindowsLive : BaseImportPage
    {
        //Comma-delimited list of offers to be used.
        const string Offers = "Contacts.View";
        //Name of cookie to use to cache the consent token. 
        const string AuthCookie = "authtoken";
        static DateTime ExpireCookie = DateTime.Now.AddYears(-10);
        static DateTime PersistCookie = DateTime.Now.AddYears(10);
        // Initialize the WindowsLiveLogin module.
        static readonly WindowsLiveLogin wll = new WindowsLiveLogin(KeyStorage.Get("wll_appid"), KeyStorage.Get("wll_secret"), "wsignin1.0", true, KeyStorage.Get("wll_policyurl"), KeyStorage.Get("wll_returnurl"));

        protected WindowsLiveLogin.ConsentToken Token;

        protected string ConsentUrl;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the consent URL for the specified offers.
            try
            {
                ConsentUrl = wll.GetConsentUrl(Offers);

                var action = Request["action"];

                if (action == "delauth")
                {
                    //Attempt to extract the consent token from the response.
                    var token = wll.ProcessConsent(Request.Form);

                    var authCookie = new HttpCookie(AuthCookie);
                    // If a consent token is found, store it in the cookie and then 
                    // redirect to the main page.
                    if (token != null)
                    {
                        authCookie.Value = token.Token;
                        authCookie.Expires = PersistCookie;
                    }
                    else
                    {
                        authCookie.Expires = ExpireCookie;
                    }

                    Response.Cookies.Add(authCookie);
                    Response.Redirect(Request.GetUrlRewriter().OriginalString, true);
                }
                else
                {
                    var req = HttpContext.Current.Request;
                    var authCookie = req.Cookies[AuthCookie];

                    // If the raw consent token has been cached in a site cookie, attempt to
                    // process it and extract the consent token.
                    if (authCookie != null)
                    {
                        var t = authCookie.Value;
                        Token = wll.ProcessConsentToken(t);
                        if ((Token != null) && !Token.IsValid())
                        {
                            Token = null;
                            Response.Redirect(ConsentUrl);
                        }
                        if (Token != null)
                        {
                            ProcessContacts();
                        }
                    }
                    else
                    {
                        Response.Redirect(ConsentUrl);
                    }
                }
            }
            catch (System.Threading.ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                SubmitError(ex.Message);
            }

        }

        protected void ProcessContacts()
        {
            //Read url
            var client = new WebClient();
            var requestUrl = string.Format("https://livecontacts.services.live.com/users/@L@{0}/rest/LiveContacts/",
                                           Token.LocationID);
            client.Headers.Add("Authorization", "DelegatedToken dt=\"" + Token.DelegationToken + "\"");
            var data = client.DownloadData(requestUrl);
            using (var memStream = new MemoryStream(data))
            {
                using (var reader = new StreamReader(memStream, Encoding.UTF8, true))
                {
                    try
                    {
                        ParseResponce(reader);
                    }
                    catch (Exception ex)
                    {
                        SubmitError(ex.Message);
                    }
                }
            }
        }

        private void ParseResponce(StreamReader str)
        {
            var doc = XDocument.Load(str);
            var contacts = from entry in doc.Root.Element("Contacts").Elements("Contact")
                           select new
                               {
                                   FirstName = entry.Element("Profiles").Element("Personal").Element("FirstName").ValueOf(),
                                   LastName = entry.Element("Profiles").Element("Personal").Element("LastName").ValueOf(),
                                   DisplayName = entry.Element("Profiles").Element("Personal").Element("DisplayName").ValueOf(),
                                   Email = entry.Element("Emails").Elements("Email").Select(x => x.Element("Address").Value)
                               };

            foreach (var contact in contacts)
            {
                if (string.IsNullOrEmpty(contact.FirstName) && string.IsNullOrEmpty(contact.LastName))
                {
                    AddContactInfo(contact.DisplayName, contact.Email);
                }
                else
                {
                    AddContactInfo(contact.FirstName ?? string.Empty, contact.LastName ?? string.Empty, contact.Email);
                }
            }
            SubmitContacts();
        }
    }

    internal static class XmlExt
    {
        internal static string ValueOf(this XElement element)
        {
            return element != null ? element.Value : null;
        }
    }
}
