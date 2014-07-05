/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Net;
using System.Web;
using System.Xml.Linq;
using ASC.Thrdparty;
using ASC.Web.Core.Files;

namespace ASC.Web.Files.Import.Boxnet
{
    public partial class BoxLogin : OAuthBase
    {
        public static string Location
        {
            get { return FilesLinkUtility.FilesBaseAbsolutePath + "import/boxnet/boxlogin.aspx"; }
        }

        protected const string Source = "boxnet";

        private const string InteractiveLoginRedirect = "https://www.box.com/api/1.0/auth/{0}";

        private string AuthToken
        {
            get { return TokenHolder.GetToken("box.net_auth_token"); }
            set { TokenHolder.AddToken("box.net_auth_token", value); }
        }

        private string AuthTicket
        {
            get { return TokenHolder.GetToken("box.net_auth_ticket"); }
            set { TokenHolder.AddToken("box.net_auth_ticket", value); }
        }

        protected string LoginUrl { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(AuthToken))
            {
                if (string.IsNullOrEmpty(Request["auth_token"]))
                {
                    try
                    {
                        //We are not authorized. get ticket and redirect
                        var url = "https://www.box.com/api/1.0/rest?action=get_ticket&api_key=" +
                                  ImportConfiguration.BoxNetApiKey;
                        var ticketResponce = new WebClient().DownloadString(url);
                        var response = XDocument.Parse(ticketResponce).Element("response");

                        if (response.Element("status").Value != "get_ticket_ok")
                        {
                            throw new InvalidOperationException("Can't retrieve ticket " + response.Element("status").Value + ".");
                        }

                        AuthTicket = response.Element("ticket").Value;
                        var loginRedir = string.Format(InteractiveLoginRedirect, AuthTicket);

                        var frameCallback = new Uri(ImportConfiguration.BoxNetIFrameAddress, UriKind.Absolute);
                        LoginUrl = string.Format("{0}?origin={1}&go={2}",
                                                 frameCallback,
                                                 HttpUtility.UrlEncode(new Uri(Request.GetUrlRewriter(), "/").ToString()),
                                                 HttpUtility.UrlEncode(loginRedir));
                    }
                    catch (Exception ex)
                    {
                        //Something goes wrong
                        SubmitError(ex.Message, Source);
                    }
                }
                else
                {
                    //We got token
                    AuthToken = Request["auth_token"];
                    //Now we can callback somewhere
                    SubmitToken(AuthToken, Source);
                }
            }
            else
            {
                SubmitToken(AuthToken, Source);
            }
        }
    }
}