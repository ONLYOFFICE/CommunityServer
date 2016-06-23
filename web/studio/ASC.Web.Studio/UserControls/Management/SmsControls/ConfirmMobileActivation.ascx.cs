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


using ASC.MessagingSystem;
using ASC.Web.Studio.Utility;
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.Geolocation;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Security;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.SMS;
using System;
using System.Web;
using System.Web.UI;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("AjaxPro.MobileActivationController")]
    public partial class ConfirmMobileActivation : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/SmsControls/ConfirmMobileActivation.ascx"; }
        }

        public UserInfo User;
        public bool Activation;

        protected string Country = "US";

        protected override void OnPreRender(EventArgs e)
        {
            if (Activation) return;
            if (SecurityContext.IsAuthenticated) Response.Redirect(GetRefererURL());
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SecurityContext.IsAuthenticated && User.ID != SecurityContext.CurrentAccount.ID)
            {
                Response.Redirect(GetRefererURL());
                return;
            }
            var authCommunications = (AuthCommunications)LoadControl(AuthCommunications.Location);
            authCommunications.DisableJoin = true;
            _communitations.Controls.Add(authCommunications);

            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterBodyScripts("~/usercontrols/Management/SmsControls/js/confirmmobile.js");
            Page.RegisterStyle("~/usercontrols/management/SmsControls/css/confirmmobile.less");

            Context.Session["SmsAuthData"] = User.ID;

            if (string.IsNullOrEmpty(User.MobilePhone))
                Activation = true;

            if (!Activation)
            {
                try
                {
                    SmsManager.PutAuthCode(User, false);
                }
                catch (Exception)
                {
                    Activation = true;
                }
            }

            if (Activation)
            {
                var ipGeolocationInfo = new GeolocationHelper("teamlabsite").GetIPGeolocationFromHttpContext();
                if (ipGeolocationInfo != null) Country = ipGeolocationInfo.Key;

                Page.RegisterClientLocalizationScript(typeof(CountriesResources));

                Page.RegisterBodyScripts("~/js/asc/plugins/countries.js",
                    "~/js/asc/plugins/phonecontroller.js");
                Page.RegisterStyle("~/skins/default/phonecontroller.css");
            }
        }

        private UserInfo GetUser()
        {
            return CoreContext.UserManager.GetUsers(
                SecurityContext.IsAuthenticated
                    ? SecurityContext.CurrentAccount.ID
                    : new Guid(Context.Session["SmsAuthData"].ToString()));
        }

        private string GetRefererURL()
        {
            var refererURL = (string)Context.Session["refererURL"];
            if (String.IsNullOrEmpty(refererURL))
                refererURL = CommonLinkUtility.GetDefault();

            Context.Session["refererURL"] = null;
            Context.Session["SmsAuthData"] = null;
            return refererURL;
        }

        #region AjaxMethod

        [SecurityPassthrough]
        [AjaxMethod(HttpSessionStateRequirement.Read)]
        public object SaveMobilePhone(string mobilePhone)
        {
            var user = GetUser();
            mobilePhone = SmsManager.SaveMobilePhone(user, mobilePhone);
            MessageService.Send(HttpContext.Current.Request, MessageAction.UserUpdatedMobileNumber, user.DisplayUserName(false), mobilePhone);

            var mustConfirm = StudioSmsNotificationSettings.Enable;

            return
                new
                    {
                        phoneNoise = SmsManager.BuildPhoneNoise(mobilePhone),
                        confirm = mustConfirm,
                        RefererURL = mustConfirm ? string.Empty : GetRefererURL()
                    };
        }

        [SecurityPassthrough]
        [AjaxMethod(HttpSessionStateRequirement.Read)]
        public object SendSmsCodeAgain()
        {
            var user = GetUser();
            SmsManager.PutAuthCode(user, true);

            return
                new
                    {
                        phoneNoise = SmsManager.BuildPhoneNoise(user.MobilePhone),
                        confirm = true,
                    };
        }

        [SecurityPassthrough]
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object ValidateSmsCode(string code)
        {
            var user = GetUser();

            try
            {
                SmsManager.ValidateSmsCode(user, code);
                MessageService.Send(HttpContext.Current.Request, MessageAction.LoginSuccessViaSms);
            }
            catch
            {
                MessageService.Send(HttpContext.Current.Request, user.DisplayUserName(false), MessageAction.LoginFailViaSms);
                throw;
            }

            return new { RefererURL = GetRefererURL() };
        }

        #endregion
    }
}