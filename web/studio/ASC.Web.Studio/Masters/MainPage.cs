/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

using AjaxPro;

using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Data;
using ASC.Core.Users;
using ASC.Geolocation;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Core.Statistic;
using ASC.Web.Studio.Core.TFA;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio
{
    /// <summary>
    /// Base page for all pages in projects
    /// </summary>
    public class MainPage : BasePage
    {
        protected virtual bool MayNotAuth { get; set; }

        protected virtual bool MayNotPaid { get; set; }

        protected virtual bool MayPhoneNotActivate { get; set; }

        protected virtual bool CheckWizardCompleted
        {
            get
            {
                return !CoreContext.Configuration.Standalone || Request.QueryString["warmup"] != "true";
            }
        }

        protected static ILog Log
        {
            get { return LogManager.GetLogger("ASC.Web"); }
        }

        protected void Page_PreInit(object sender, EventArgs e)
        {
            if (CheckWizardCompleted)
            {
                var s = WizardSettings.Load();
                if (!s.Completed)
                {
                    Response.Redirect("~/Wizard.aspx");
                }
            }

            //check auth
            if (!SecurityContext.IsAuthenticated
                && !AuthByCookies()
                && !MayNotAuth)
            {
                if (TenantAccessSettings.Load().Anyone)
                {
                    OutsideAuth();
                }
                else
                {
                    var authUrl = "~/Auth.aspx";
                    if (Request.DesktopApp())
                    {
                        authUrl += "?desktop=true";
                    }
                    Response.Redirect(Request.AppendRefererURL(authUrl), true);
                }
            }

            var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            if (!MayNotPaid
                && TenantExtra.EnableTariffSettings
                && (TenantStatisticsProvider.IsNotPaid())
                && WarmUp.Instance.CheckCompleted() && Request.QueryString["warmup"] != "true")
            {
                if (TariffSettings.HidePricingPage && !user.IsAdmin())
                {
                    Response.StatusCode = (int)HttpStatusCode.PaymentRequired;
                    Response.End();
                }
                else
                {
                    Response.Redirect(TenantExtra.GetTariffPageLink() + (Request.DesktopApp() ? "?desktop=true" : ""), true);
                }
            }

            if (!MayPhoneNotActivate
                && SecurityContext.IsAuthenticated)
            {

                if (StudioSmsNotificationSettings.IsVisibleAndAvailableSettings && StudioSmsNotificationSettings.TfaEnabledForUser(user.ID)
                    && (string.IsNullOrEmpty(user.MobilePhone) || user.MobilePhoneActivationStatus == MobilePhoneActivationStatus.NotActivated))
                {
                    Response.Redirect(CommonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.PhoneActivation), true);
                }

                if (TfaAppAuthSettings.IsVisibleSettings && TfaAppAuthSettings.TfaEnabledForUser(user.ID)
                    && !TfaAppUserSettings.EnableForUser(user.ID))
                {
                    Response.Redirect(CommonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.TfaActivation), true);
                }
            }

            //check disable and public 
            var webitem = CommonLinkUtility.GetWebItemByUrl(Request.Url.ToString());
            var parentItemID = webitem == null ? Guid.Empty : webitem.ID;
            var parentIsDisabled = false;
            if (webitem != null && webitem.IsSubItem())
            {
                parentItemID = WebItemManager.Instance.GetParentItemID(webitem.ID);
                parentIsDisabled = WebItemManager.Instance[parentItemID].IsDisabled();
            }

            if (webitem != null && (webitem.IsDisabled() || parentIsDisabled) && !MayNotAuth)
            {
                if (webitem.ID == WebItemManager.PeopleProductID
                    && string.Equals(GetType().BaseType.FullName, "ASC.Web.People.Profile"))
                {
                    Response.Redirect("~/My.aspx", true);
                }

                Response.Redirect("~/", true);
            }

            if (SecurityContext.IsAuthenticated && !CoreContext.Configuration.Personal)
            {
                try
                {
                    StatisticManager.SaveUserVisit(TenantProvider.CurrentTenantID, SecurityContext.CurrentAccount.ID, parentItemID);
                }
                catch (Exception exc)
                {
                    Log.Error("failed save user visit", exc);
                }
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            InitInlineScript();
        }

        private static bool AuthByCookies()
        {
            var cookiesKey = CookiesManager.GetCookies(CookiesType.AuthKey);
            if (string.IsNullOrEmpty(cookiesKey)) return false;

            try
            {
                if (SecurityContext.AuthenticateMe(cookiesKey)) return true;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("AutoAuthByCookies Error {0}", ex);
            }

            return false;
        }

        private void InitInlineScript()
        {
            var scripts = HttpContext.Current.Items[Constant.AjaxID + ".pagescripts"] as ListDictionary;

            if (scripts == null) return;

            var sb = new StringBuilder();

            foreach (var key in scripts.Keys)
            {
                sb.Append(scripts[key]);
            }

            this.RegisterInlineScript(sb.ToString(), onReady: false);
        }

        protected void SetLanguage(bool checkIp = true)
        {
            if (Request.QueryString.Count == 0)
            {
                var ipGeolocationInfo = new GeolocationHelper("teamlabsite").GetIPGeolocationFromHttpContext();
                if (checkIp && ipGeolocationInfo != null && !string.IsNullOrEmpty(ipGeolocationInfo.Key))
                {
                    var culture = SetupInfo.GetPersonalCulture(ipGeolocationInfo.Key);
                    if (culture.Value != null)
                    {

                        var redirectUrl = String.Format("/{0}/{1}", culture.Key, Request.Path);

                        if (redirectUrl.EndsWith("Auth.aspx", StringComparison.InvariantCultureIgnoreCase))
                            redirectUrl = redirectUrl.Remove(redirectUrl.IndexOf("Auth.aspx", StringComparison.OrdinalIgnoreCase));

                        Response.Redirect(redirectUrl, true);

                    }
                }
            }
            else if (!String.IsNullOrEmpty(Request["lang"]))
            {
                var lang = Request["lang"].Split(',')[0];
                var cultureInfo = SetupInfo.GetPersonalCulture(lang).Value;

                if (cultureInfo != null)
                {
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;
                    Thread.CurrentThread.CurrentCulture = cultureInfo;
                }
                else
                {
                    Log.WarnFormat("Lang {0} not supported", lang);
                }
            }
            else if (!String.IsNullOrEmpty(Request["email"]))
            {
                var user = CoreContext.UserManager.GetUserByEmail(Request["email"]);

                if (user.ID.Equals(Constants.LostUser.ID))
                {
                    return;
                }

                if (user.CultureName != null)
                {
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(user.CultureName);
                    Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(user.CultureName);
                }
            }
        }

        private static void OutsideAuth()
        {
            var action = MessageAction.LoginSuccess;
            Func<int> funcLoginEvent = () => { return CookiesManager.GetLoginEventId(action); };
            var cookie = string.Empty;
            try
            {
                cookie = SecurityContext.AuthenticateMe(Constants.OutsideUser.ID, funcLoginEvent);
            }
            catch (Exception)
            {
                throw;
            }

            if (HttpContext.Current != null)
            {
                CookiesManager.SetCookies(CookiesType.AuthKey, cookie);
                DbLoginEventsManager.ResetCache();
            }
            else
            {
                SecurityContext.AuthenticateMe(cookie);
            }
        }
    }
}