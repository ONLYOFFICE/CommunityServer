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
using System.Web;
using System.Web.UI;

using AjaxPro;

using ASC.Core;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Core;
using ASC.Web.Core.Security;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.TFA;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.Utility;

using Google.Authenticator;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("AjaxPro.TfaActivationController")]
    public partial class TfaActivation : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/TfaControls/ConfirmTfa.ascx"; }
        }

        public UserInfo User { get; set; }

        public bool Activation { get; set; }

        public SetupCode SetupCode { get; set; }

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

            if (!TfaAppAuthSettings.IsVisibleSettings || !TfaAppAuthSettings.TfaEnabledForUser(User.ID))
            {
                Response.Redirect(Context.GetRefererURL(), true);
                return;
            }
            if (!Activation && !TfaAppUserSettings.EnableForUser(User.ID))
            {
                Response.Redirect(Context.GetRefererURL(), true);
                return;
            }

            var authCommunications = (AuthCommunications)LoadControl(AuthCommunications.Location);
            authCommunications.DisableJoin = true;
            _communitations.Controls.Add(authCommunications);

            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterBodyScripts("~/UserControls/Management/TfaControls/js/confirmtfa.js")
                .RegisterStyle("~/UserControls/Management/TfaControls/css/confirmtfa.less");

            if (Activation)
            {
                SetupCode = User.GenerateSetupCode();
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

        [SecurityPassthrough]
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object ValidateTfaCode(string query, string code)
        {
            var user = GetUser(query);
            bool newBackupCodes;

            try
            {
                newBackupCodes = user.ValidateAuthCode(code, !Activation);
            }
            catch (Authorize.BruteForceCredentialException)
            {
                MessageService.Send(HttpContext.Current.Request, user.DisplayUserName(false), MessageAction.LoginFailBruteForce);
                throw;
            }
            catch
            {
                MessageService.Send(HttpContext.Current.Request, user.DisplayUserName(false), MessageAction.LoginFailViaTfaApp, MessageTarget.Create(user.ID));
                throw;
            }

            var refererUrl = HttpUtility.ParseQueryString(query)["refererurl"];
            if (string.IsNullOrEmpty(refererUrl))
            {
                refererUrl = Context.GetRefererURL();
            }

            if (newBackupCodes)
            {
                MessageService.Send(HttpContext.Current.Request, MessageAction.UserConnectedTfaApp, MessageTarget.Create(user.ID));

                refererUrl = CommonLinkUtility.GetUserProfile(user.ID) + "&codes=true";
            }

            if (query.Contains("desktop=true"))
            {
                refererUrl = CommonLinkUtility.GetFullAbsolutePath(WebItemManager.Instance[WebItemManager.DocumentsProductID].StartURL + "?desktop=true");
            }

            return new { RefererURL = refererUrl };
        }
    }
}