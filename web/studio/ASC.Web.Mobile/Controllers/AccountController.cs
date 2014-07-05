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
using System.Linq;
using System.Web.Mvc;
using ASC.Core.Users;
using ASC.FederatedLogin;
using ASC.Security.Cryptography;
using ASC.Web.Mobile.Attributes;
using ASC.Web.Mobile.Models;
using ASC.Core;
using ASC.Web.Core;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.FederatedLogin.Profile;
using System.Configuration;
using ASC.Web.Studio.Core.SMS;
using Resources;
using log4net;
using ASC.MessagingSystem;

namespace ASC.Web.Mobile.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILog _log = LogManager.GetLogger("ASC.Mobile.SignIn");
        //
        // GET: /Login/

        public ActionResult SignIn()
        {
            AscAuthorization.TryAuthorize();

            var authFromSite = Request["asc_auth_key"] ?? Request["id"];
            if (!string.IsNullOrEmpty(authFromSite) && SecurityContext.AuthenticateMe(authFromSite))
            {
                CookiesManager.SetCookies(CookiesType.AuthKey, authFromSite);
                MessageService.Send(System.Web.HttpContext.Current.Request, MessageAction.LoginSuccess);
                return RedirectAuthorized();
            }

            UserInfo user = null;
            var byThirdParty = false;

            var profile = Request.Url.GetProfile();
            if (profile != null)
            {
                try
                {
                    Guid userId;
                    byThirdParty = TryByHashId(profile.HashId, out userId);

                    user = CoreContext.UserManager.GetUsers(userId);

                    if (!StudioSmsNotificationSettings.IsVisibleSettings
                        || !StudioSmsNotificationSettings.Enable)
                    {
                        var key = SecurityContext.AuthenticateMe(user.ID);
                        CookiesManager.SetCookies(CookiesType.AuthKey, key);
                        MessageService.Send(System.Web.HttpContext.Current.Request, MessageAction.LoginSuccessViaSocialAccount);
                        return RedirectAuthorized();
                    }

                    if (string.IsNullOrEmpty(user.MobilePhone) || user.MobilePhoneActivationStatus == MobilePhoneActivationStatus.NotActivated)
                    {
                        user = null;
                        ModelState.AddModelError("InvalidPassword", MobileResource.ErrMobileNotActivate);
                    }
                    else
                    {
                        SmsManager.PutAuthCode(user, false);
                    }
                }
                catch (Exception e)
                {
                    user = null;
                    byThirdParty = false;
                    _log.Error("Error authentificating by profile", e);
                    MessageService.Send(System.Web.HttpContext.Current.Request, profile.UniqueId, MessageAction.LoginFailSocialAccountNotFound);
                    ModelState.AddModelError("InvalidPassword", MobileResource.ErrNoToken);
                }
            }

            return LoginView(user, byThirdParty, string.Empty, string.Empty);
        }

        [HttpPost]
        public ActionResult SignIn(LoginModel login)
        {
            //First try federated
            UserInfo user = null;
            var byThirdParty = false;

            if (!StudioSmsNotificationSettings.IsVisibleSettings
                || !StudioSmsNotificationSettings.Enable)
            {
                if (ModelState.IsValid)
                {
                    //Validate login password
                    try
                    {
                        var key = SecurityContext.AuthenticateMe(login.Email, login.Password);
                        CookiesManager.SetCookies(CookiesType.AuthKey, key);
                        MessageService.Send(System.Web.HttpContext.Current.Request, MessageAction.LoginSuccess);
                        return RedirectAuthorized();
                    }
                    catch (Exception e)
                    {
                        _log.Error(string.Format("Error authentificating by password. email:{0}", login.Email), e);
                        MessageService.Send(System.Web.HttpContext.Current.Request, login.Email, MessageAction.LoginFail);
                        ModelState.AddModelError("InvalidPassword", MobileResource.ErrPasswordInvalid);
                    }
                }
            }
            else
            {
                var profile = Request.Url.GetProfile();
                if (profile != null)
                {
                    try
                    {
                        Guid userId;
                        TryByHashId(profile.HashId, out userId);

                        user = CoreContext.UserManager.GetUsers(userId);
                        byThirdParty = true;
                    }
                    catch (Exception e)
                    {
                        user = null;
                        _log.Error("Error authentificating by profile", e);
                    }
                    finally
                    {
                        //If we got here than no token associated. add error
                        ModelState.AddModelError("InvalidPassword", MobileResource.ErrNoToken);
                    }
                }
                else if (ModelState.IsValid)
                {
                    user = CoreContext.UserManager.GetUsers(CoreContext.TenantManager.GetCurrentTenant(false).TenantId, login.Email, Hasher.Base64Hash(login.Password, HashAlg.SHA256));
                }

                if (user != null)
                {
                    if (!string.IsNullOrEmpty(Request.Form["Resend"] + Request.Form["SendCode"]))
                    {
                        if (string.IsNullOrEmpty(user.MobilePhone) || user.MobilePhoneActivationStatus == MobilePhoneActivationStatus.NotActivated)
                        {
                            user = null;
                            ModelState.AddModelError("InvalidPassword", MobileResource.ErrMobileNotActivate);
                        }
                        else
                        {
                            try
                            {
                                SmsManager.PutAuthCode(user, !string.IsNullOrEmpty(Request.Form["Resend"]));
                            }
                            catch (Exception e)
                            {
                                ModelState.AddModelError("InvalidCode", e.Message);
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            SmsManager.ValidateSmsCode(user, login.Code);

                            var key = SecurityContext.AuthenticateMe(user.ID);
                            CookiesManager.SetCookies(CookiesType.AuthKey, key);
                            MessageService.Send(System.Web.HttpContext.Current.Request, MessageAction.LoginSuccessViaSms);
                            return RedirectAuthorized();
                        }
                        catch (Exception e)
                        {
                            _log.Error(string.Format("Error authentificating by sms code. email:{0}", login.Email), e);
                            MessageService.Send(System.Web.HttpContext.Current.Request, user.DisplayUserName(false), MessageAction.LoginFailViaSms);
                            ModelState.AddModelError("InvalidCode", e.Message);
                        }
                    }
                }
            }
            return LoginView(user, byThirdParty, login.Email, login.Password);
        }

        private RedirectToRouteResult RedirectAuthorized()
        {
            return RedirectToRoute("Default", new { controller = "Home", action = "Index" });
        }

        private ActionResult LoginView(UserInfo user, bool byThirdParty, string email, string password)
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
            if (tenant == null)
            {
                return Redirect(ConfigurationManager.AppSettings["web.notenant-url"]);
            }

            var logopath = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(tenant.TenantId).GetAbsoluteCompanyLogoPath();
            var mobiledir = ConfigurationManager.AppSettings["mobile.redirect-url"];
            if (!string.IsNullOrEmpty(logopath) && !string.IsNullOrEmpty(mobiledir))
            {
                mobiledir = "/" + mobiledir.Trim('~', '/') + "/";
                if (logopath.Contains(mobiledir))
                {
                    logopath = logopath.Replace(mobiledir, "/");
                }
            }
            if (SecurityContext.IsAuthenticated)
            {
                //If already authorized redir to home
                return RedirectAuthorized();
            }
            return View("SignIn", new LoginModel
                {
                    Email = email,
                    Password = password,
                    CompanyInfo = CoreContext.TenantManager.GetCurrentTenant().Name,
                    CompanyLogo = logopath,
                    TenantAddress = CoreContext.TenantManager.GetCurrentTenant().TenantDomain,
                    IsAuthentificated = SecurityContext.IsAuthenticated,
                    UserInfo = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID),
                    UserPhoto = UserPhotoManager.GetSmallPhotoURL(SecurityContext.CurrentAccount.ID),
                    RequestCode = user != null,
                    ByThirdParty = byThirdParty
                });
        }

        [AscAuthorization]
        public ActionResult SignOut()
        {
            var loginName = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).DisplayUserName(false);

            //Logout
            CookiesManager.ClearCookies(CookiesType.AuthKey);
            SecurityContext.Logout();
            Session.Clear();
            MessageService.Send(System.Web.HttpContext.Current.Request, loginName, MessageAction.Logout);

            return RedirectToRoute("Login", null);
        }

        private static bool TryByHashId(string hashId, out Guid userId)
        {
            userId = Guid.Empty;
            if (string.IsNullOrEmpty(hashId))
            {
                return false;
            }

            var accountsStrId = new AccountLinker("webstudio").GetLinkedObjectsByHashId(hashId);
            userId = accountsStrId
                .Select(x =>
                {
                    try
                    {
                        return new Guid(x);
                    }
                    catch
                    {
                        return Guid.Empty;
                    }
                })
                .Where(x => x != Guid.Empty)
                .FirstOrDefault(x => CoreContext.UserManager.UserExists(x));

            return true;
        }
    }
}
