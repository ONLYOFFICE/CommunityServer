/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.Core.Users;
using ASC.Geolocation;
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
                    var refererURL = GetRefererUrl();
                    Session["refererURL"] = refererURL;
                    var authUrl = "~/Auth.aspx";
                    if (Request.DesktopApp())
                    {
                        authUrl += "?desktop=" + Request["desktop"];
                    }
                    Response.Redirect(authUrl, true);
                }
            }

            var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            if (!MayNotPaid
                && TenantExtra.EnableTarrifSettings
                && (TenantStatisticsProvider.IsNotPaid() || TenantExtra.UpdatedWithoutLicense)
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
                if (StudioSmsNotificationSettings.IsVisibleSettings && StudioSmsNotificationSettings.Enable
                    && (string.IsNullOrEmpty(user.MobilePhone) || user.MobilePhoneActivationStatus == MobilePhoneActivationStatus.NotActivated))
                {
                    Response.Redirect(CommonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.PhoneActivation), true);
                }

                if (TfaAppAuthSettings.IsVisibleSettings && TfaAppAuthSettings.Enable
                    && !TfaAppUserSettings.EnableForUser(user.ID))
                {
                    Response.Redirect(CommonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.TfaActivation), true);
                }
            }

            //check disable and public 
            var webitem = CommonLinkUtility.GetWebItemByUrl(Request.Url.ToString());
            var parentIsDisabled = false;
            if (webitem != null && webitem.IsSubItem())
            {
                var parentItemID = WebItemManager.Instance.GetParentItemID(webitem.ID);
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
                    StatisticManager.SaveUserVisit(TenantProvider.CurrentTenantID, SecurityContext.CurrentAccount.ID, CommonLinkUtility.GetProductID());
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

        private string GetRefererUrl()
        {
            var refererURL = Request.GetUrlRewriter().AbsoluteUri;
            if (this is _Default)
            {
                refererURL = "/";
            }
            else if (String.IsNullOrEmpty(refererURL)
                        || refererURL.IndexOf("Subgurim_FileUploader", StringComparison.InvariantCultureIgnoreCase) != -1
                        || (this is ServerError))
            {
                refererURL = (string)Session["refererURL"];
            }

            return refererURL;
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
                    var cultureInfo = SetupInfo.EnabledCulturesPersonal.Find(c => String.Equals(c.TwoLetterISOLanguageName, ipGeolocationInfo.Key, StringComparison.InvariantCultureIgnoreCase));
                    if (cultureInfo != null)
                    {

                        var redirectUrl = String.Format("/{0}/{1}", cultureInfo.TwoLetterISOLanguageName, Request.Path);

                        if (redirectUrl.EndsWith("Auth.aspx", StringComparison.InvariantCultureIgnoreCase))
                            redirectUrl = redirectUrl.Remove(redirectUrl.IndexOf("Auth.aspx", StringComparison.OrdinalIgnoreCase));

                        Response.Redirect(redirectUrl, true);

                    }
                }
            }
            else if (!String.IsNullOrEmpty(Request["lang"]))
            {
                var lang = Request["lang"].Split(',')[0];
                var cultureInfo = SetupInfo.EnabledCulturesPersonal.Find(c => String.Equals(c.TwoLetterISOLanguageName, lang, StringComparison.InvariantCultureIgnoreCase));
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
            var cookie = SecurityContext.AuthenticateMe(Constants.OutsideUser.ID);
            if (HttpContext.Current != null)
            {
                CookiesManager.SetCookies(CookiesType.AuthKey, cookie);
            }
            else
            {
                SecurityContext.AuthenticateMe(cookie);
            }
        }
    }
}