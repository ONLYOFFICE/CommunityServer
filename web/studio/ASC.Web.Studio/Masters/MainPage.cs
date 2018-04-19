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
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.Geolocation;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Core.Statistic;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using log4net;
using System.Globalization;

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

        protected virtual bool CheckWizardCompleted { get { return true; } }

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
                    Response.Redirect("~/wizard.aspx");
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
                    Response.Redirect("~/auth.aspx", true);
                }
            }

            var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            if (!MayNotPaid && TenantStatisticsProvider.IsNotPaid())
            {
                if (TariffSettings.HidePricingPage && !user.IsAdmin())
                {
                    Response.StatusCode = (int)HttpStatusCode.PaymentRequired;
                    Response.End();
                }
                else
                {
                    Response.Redirect(TenantExtra.GetTariffPageLink(), true);
                }
            }

            if (SecurityContext.IsAuthenticated
                && StudioSmsNotificationSettings.IsVisibleSettings
                && StudioSmsNotificationSettings.Enable
                && !MayPhoneNotActivate)
            {
                if (!CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Constants.GroupAdmin.ID)
                    && (string.IsNullOrEmpty(user.MobilePhone)
                        || user.MobilePhoneActivationStatus == MobilePhoneActivationStatus.NotActivated))
                {
                    Response.Redirect(CommonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.PhoneActivation), true);
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
                    Response.Redirect("~/my.aspx", true);
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

                        if (redirectUrl.EndsWith("auth.aspx", StringComparison.InvariantCulture))
                            redirectUrl = redirectUrl.Remove(redirectUrl.IndexOf("auth.aspx", StringComparison.Ordinal));

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

        private void OutsideAuth()
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