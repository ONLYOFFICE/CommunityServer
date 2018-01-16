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


using ASC.Common.Caching;
using ASC.Core;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Studio.Utility;
using Resources;
using System;
using System.Security.Authentication;
using System.Threading;
using System.Web;
using System.Web.UI;

namespace ASC.Web.Studio.UserControls.Common.AuthorizeDocs
{
    public partial class AuthorizeDocs : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/AuthorizeDocs/AuthorizeDocs.ascx"; }
        }

        private readonly ICache cache = AscCache.Memory;
        protected string Login;

        protected string LoginMessage;
        protected int LoginMessageType = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            LoginMessage = Request["m"];

            Page.RegisterStyle("~/usercontrols/common/authorizedocs/css/authorizedocs.less")
                .RegisterBodyScripts("~/usercontrols/common/authorizedocs/js/authorizedocs.js", "~/usercontrols/common/authorize/js/authorize.js");

            Page.Title = Resource.AuthDocsTitlePage;
            Page.MetaDescription = Resource.AuthDocsMetaDescription.HtmlEncode();
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

                    int counter;

                    int.TryParse(cache.Get<string>("loginsec/" + Login), out counter);

                    if (++counter%5 == 0)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                    }
                    cache.Insert("loginsec/" + Login, counter.ToString(), DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));

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
                var confirmedEmail = (Request.QueryString["confirmed-email"] ?? "").Trim();

                if (String.IsNullOrEmpty(confirmedEmail) || !confirmedEmail.TestEmailRegex()) return;

                Login = confirmedEmail;
                LoginMessage = Resource.MessageEmailConfirmed + " " + Resource.MessageAuthorize;
                LoginMessageType = 1;
            }
        }
    }
}