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
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using OpenIdAuth.IdLinker;
using OpenIdAuth.Profile;

namespace OpenIdAuth
{
    public partial class Login : System.Web.UI.Page
    {
        protected new string Error { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.Url.HasProfile())
            {
                //We got profile
                var profile = Request.Url.GetProfile();
                if (profile.IsAuthorized)
                {
                    //SAMPLE
                    //Do something with it
                    var username = profile.UniqueId;
                    var userData = profile.UserDisplayName;

                    var linker = new Linker(ConfigurationManager.ConnectionStrings["openids"]);
                    var linked = linker.GetLinkedObjects(profile);
                    if (linked.Count()>0)
                    {
                        //Already has something
                    }
                    else
                    {
                        linker.AddLink("Hellou!!!",profile);
                    }

                    var ticket = new FormsAuthenticationTicket(1,
                                                               username,
                                                               DateTime.Now,
                                                               DateTime.Now.AddMinutes(5),
                                                               false,
                                                               userData,
                                                               FormsAuthentication.FormsCookiePath);
                    // Encrypt the ticket.
                    string encTicket = FormsAuthentication.Encrypt(ticket);
                    // Create the cookie.
                    Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
                    // Redirect back to original URL.
                    Response.Redirect(FormsAuthentication.GetRedirectUrl(username, false));
                }
                else
                {
                    //Show errors
                    Error = profile.AuthorizationError;
                }
            }
        }
    }
}
