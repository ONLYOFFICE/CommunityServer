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
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.UI;

using AjaxPro;

using ASC.Core;
using ASC.Core.Users;
using ASC.Geolocation;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Core.Security;
using ASC.Web.Core.Sms;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.Utility;

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
            if (SecurityContext.IsAuthenticated) Response.Redirect(Context.GetRefererURL());
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SecurityContext.IsAuthenticated && User.ID != SecurityContext.CurrentAccount.ID)
            {
                Response.Redirect(Context.GetRefererURL(), true);
                return;
            }
            if (!Activation && (!StudioSmsNotificationSettings.IsVisibleAndAvailableSettings || !StudioSmsNotificationSettings.Enable))
            {
                Response.Redirect(Context.GetRefererURL(), true);
                return;
            }

            if (IsPostBack && !Activation)
            {
                try
                {
                    SmsManager.ValidateSmsCode(User, Request["phoneAuthcode"]);

                    Response.Redirect(Context.GetRefererURL(), true);
                }
                catch
                {
                    MessageService.Send(HttpContext.Current.Request, User.DisplayUserName(false), MessageAction.LoginFailViaSms, MessageTarget.Create(User.ID));
                }
            }

            var authCommunications = (AuthCommunications)LoadControl(AuthCommunications.Location);
            authCommunications.DisableJoin = true;
            _communitations.Controls.Add(authCommunications);

            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterBodyScripts("~/UserControls/Management/SmsControls/js/confirmmobile.js")
                .RegisterStyle("~/UserControls/Management/SmsControls/css/confirmmobile.less");

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
                Country = new RegionInfo(Thread.CurrentThread.CurrentCulture.LCID).TwoLetterISORegionName;

                if (!CoreContext.Configuration.Standalone)
                {
                    var ipGeolocationInfo = new GeolocationHelper("teamlabsite").GetIPGeolocationFromHttpContext();
                    if (ipGeolocationInfo != null && !string.IsNullOrEmpty(ipGeolocationInfo.Key)) Country = ipGeolocationInfo.Key;
                }

                Page
                    .RegisterClientScript(new CountriesResources())
                    .RegisterBodyScripts(
                        "~/js/asc/plugins/countries.js",
                        "~/js/asc/plugins/phonecontroller.js");
                if(ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark)
                {
                    Page.RegisterStyle("~/skins/dark/dark-phonecontroller.less");
                }
                else
                {
                    Page.RegisterStyle("~/skins/default/phonecontroller.less");
                }
            }
        }

        private static UserInfo GetUser(string query)
        {
            if (SecurityContext.IsAuthenticated)
            {
                return CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            }

            var queryString = HttpUtility.ParseQueryString(query);

            var email = (queryString["email"] ?? "").Trim();
            var type = typeof(ConfirmType).TryParseEnum(queryString["type"] ?? "", ConfirmType.EmpInvite);
            var checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(email + type, queryString["key"], SetupInfo.ValidAuthKeyInterval);

            if (checkKeyResult == EmailValidationKeyProvider.ValidationResult.Expired)
            {
                throw new Exception(Resource.ErrorExpiredActivationLink);
            }

            if (checkKeyResult == EmailValidationKeyProvider.ValidationResult.Invalid)
            {
                throw new Exception(Resource.ErrorConfirmURLError);
            }

            var user = CoreContext.UserManager.GetUserByEmail(email);
            return user;
        }

        #region AjaxMethod

        [SecurityPassthrough]
        [AjaxMethod(HttpSessionStateRequirement.Read)]
        public object SaveMobilePhone(string query, string mobilePhone)
        {
            var user = GetUser(query);
            var request = HttpContext.Current.Request;
            mobilePhone = SmsManager.SaveMobilePhone(user, mobilePhone);
            MessageService.Send(request, MessageAction.UserUpdatedMobileNumber, MessageTarget.Create(user.ID), user.DisplayUserName(false), mobilePhone);

            var mustConfirm = StudioSmsNotificationSettings.TfaEnabledForUser(user.ID);

            var refererUrl = HttpUtility.ParseQueryString(query)["refererurl"];
            if (string.IsNullOrEmpty(refererUrl))
            {
                refererUrl = Context.GetRefererURL();
            }

            return
                new
                {
                    phoneNoise = SmsSender.BuildPhoneNoise(mobilePhone),
                    confirm = mustConfirm,
                    RefererURL = mustConfirm ? string.Empty : refererUrl
                };
        }

        [SecurityPassthrough]
        [AjaxMethod(HttpSessionStateRequirement.Read)]
        public object SendSmsCodeAgain(string query)
        {
            var user = GetUser(query);
            SmsManager.PutAuthCode(user, true);

            return
                new
                {
                    phoneNoise = SmsSender.BuildPhoneNoise(user.MobilePhone),
                    confirm = true,
                };
        }

        [SecurityPassthrough]
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object ValidateSmsCode(string query, string code)
        {
            var user = GetUser(query);

            try
            {
                SmsManager.ValidateSmsCode(user, code);
            }
            catch (Authorize.BruteForceCredentialException)
            {
                MessageService.Send(Request, user.DisplayUserName(false), MessageAction.LoginFailBruteForce);
                throw;
            }
            catch
            {
                MessageService.Send(HttpContext.Current.Request, user.DisplayUserName(false), MessageAction.LoginFailViaSms, MessageTarget.Create(user.ID));
                throw;
            }

            var refererUrl = HttpUtility.ParseQueryString(query)["refererurl"];
            if (string.IsNullOrEmpty(refererUrl))
            {
                refererUrl = Context.GetRefererURL();
            }

            return new { RefererURL = refererUrl };
        }

        #endregion
    }
}