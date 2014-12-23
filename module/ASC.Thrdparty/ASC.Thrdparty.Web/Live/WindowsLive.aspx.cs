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
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
using ASC.Thrdparty.Configuration;
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

                string action = Request["action"];

                if (action == "delauth")
                {
                    //Attempt to extract the consent token from the response.
                    WindowsLiveLogin.ConsentToken token = wll.ProcessConsent(Request.Form);

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
                    HttpRequest req = HttpContext.Current.Request;
                    HttpCookie authCookie = req.Cookies[AuthCookie];

                    // If the raw consent token has been cached in a site cookie, attempt to
                    // process it and extract the consent token.
                    if (authCookie != null)
                    {
                        string t = authCookie.Value;
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
                ErrorScope = ex.Message;
                SubmitData();
            }

        }

        protected void ProcessContacts()
        {
            //Read url
            var client = new WebClient();
            string requestUrl = string.Format("https://livecontacts.services.live.com/users/@L@{0}/rest/LiveContacts/",
                                              Token.LocationID);
            client.Headers.Add("Authorization", "DelegatedToken dt=\"" + Token.DelegationToken + "\"");
            var data = client.DownloadData(requestUrl);
            using (var memStream = new MemoryStream(data))
            {
                using (var reader = new StreamReader(memStream,Encoding.UTF8,true))
                {
                    try
                    {
                        ParseResponce(reader);
                    }
                    catch (Exception e)
                    {
                        ErrorScope = e.ToString();
                        SubmitData();
                    }
                }
            }
        }

        private void ParseResponce(StreamReader str)
        {
            XDocument doc = XDocument.Load(str);
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
            SubmitData();
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
