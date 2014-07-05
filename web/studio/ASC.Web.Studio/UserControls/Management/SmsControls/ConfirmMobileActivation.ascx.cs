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

            _communitations.Controls.Add(LoadControl(AuthCommunications.Location));

            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/Management/SmsControls/js/confirmmobile.js"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/management/SmsControls/css/confirmmobile.less"));

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

                var clientScriptReference = new ClientScriptReference();
                clientScriptReference.Includes.Add(typeof(CountriesResources));
                Page.RegisterBodyScripts(clientScriptReference);

                Page.RegisterBodyScripts(ResolveUrl("~/js/asc/plugins/countries.js"));
                Page.RegisterBodyScripts(ResolveUrl("~/js/asc/plugins/phonecontroller.js"));
                Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/skins/default/phonecontroller.css"));
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