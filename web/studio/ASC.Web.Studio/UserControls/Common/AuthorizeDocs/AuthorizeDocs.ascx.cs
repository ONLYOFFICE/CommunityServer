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
using System.Security.Authentication;
using System.Threading;
using System.Web;
using System.Web.UI;
using ASC.Core.Caching;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using Resources;
using ASC.Web.Studio.Utility;
using ASC.Core;

namespace ASC.Web.Studio.UserControls.Common.AuthorizeDocs
{
    public partial class AuthorizeDocs : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/AuthorizeDocs/AuthorizeDocs.ascx"; }
        }

        private readonly ICache cache = AscCache.Default;
        protected string Login;

        protected string LoginMessage;
        protected int LoginMessageType = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            LoginMessage = Request["m"];

            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/common/authorizedocs/css/authorizedocs.less"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/common/authorizedocs/js/authorizedocs.js"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/common/authorize/js/authorize.js"));

            Page.Title = Resource.AuthDocsTitlePage;
            Page.MetaDescription = Resource.AuthDocsMetaDescription;
            Page.MetaKeywords = Resource.AuthDocsMetaKeywords;

            PersonalFooterHolder.Controls.Add(LoadControl(PersonalFooter.PersonalFooter.Location));
            if (AccountLinkControl.IsNotEmpty)
            {
                HolderLoginWithThirdParty.Controls.Add(LoadControl(LoginWithThirdParty.Location));
                LoginSocialNetworks.Controls.Add(LoadControl(LoginWithThirdParty.Location));
            }
            pwdReminderHolder.Controls.Add(LoadControl(PwdTool.Location));

            if (IsPostBack)
            {
                try
                {
                    Login = Request["login"].Trim();
                    var password = Request["pwd"];
                    if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(password))
                    {
                        throw new InvalidCredentialException(Resource.InvalidUsernameOrPassword);
                    }

                    
                    var counter = (int)(cache.Get("loginsec/" + Login) ?? 0);
                    if (++counter%5 == 0)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                    }
                    cache.Insert("loginsec/" + Login, counter, DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));

                    var session = string.IsNullOrEmpty(Request["remember"]);

                    var cookiesKey = SecurityContext.AuthenticateMe(Login, password);
                    CookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey, session);
                    MessageService.Send(HttpContext.Current.Request, MessageAction.LoginSuccess);
                }
                catch (InvalidCredentialException)
                {
                    Auth.ProcessLogout();
                    LoginMessage = Resource.InvalidUsernameOrPassword;

                    var loginName = string.IsNullOrWhiteSpace(Login) ? AuditResource.EmailNotSpecified : Login;

                    MessageService.Send(HttpContext.Current.Request, loginName, MessageAction.LoginFailInvalidCombination);

                    return;
                }
                catch (System.Security.SecurityException)
                {
                    Auth.ProcessLogout();
                    LoginMessage = Resource.ErrorDisabledProfile;
                    MessageService.Send(HttpContext.Current.Request, Login, MessageAction.LoginFailDisabledProfile);
                    return;
                }
                catch (Exception ex)
                {
                    Auth.ProcessLogout();
                    LoginMessage = ex.Message;
                    MessageService.Send(HttpContext.Current.Request, Login, MessageAction.LoginFail);
                    return;
                }

                var refererURL = (string)Session["refererURL"];

                if (string.IsNullOrEmpty(refererURL))
                {
                    Response.Redirect(CommonLinkUtility.GetDefault());
                }
                else
                {
                    Session["refererURL"] = null;
                    Response.Redirect(refererURL);
                }
            }
            else
            {
                var confirmedEmail = Request.QueryString["confirmed-email"];

                if (String.IsNullOrEmpty(confirmedEmail) || !confirmedEmail.TestEmailRegex()) return;

                Login = confirmedEmail;
                LoginMessage = Resource.MessageEmailConfirmed + " " + Resource.MessageAuthorize;
                LoginMessageType = 1;
            }
        }
    }
}