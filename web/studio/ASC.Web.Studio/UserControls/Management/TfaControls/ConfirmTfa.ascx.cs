/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Web;
using System.Web.UI;
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Core.Security;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.TFA;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.Utility;
using Google.Authenticator;
using Resources;

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
            if (SecurityContext.IsAuthenticated) Response.Redirect(GetRefererURL());
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SecurityContext.IsAuthenticated && User.ID != SecurityContext.CurrentAccount.ID)
            {
                Response.Redirect(GetRefererURL(), true);
                return;
            }
            if (!TfaAppAuthSettings.IsVisibleSettings || !TfaAppAuthSettings.Enable)
            {
                Response.Redirect(GetRefererURL(), true);
                return;
            }
            if (!Activation && !TfaAppUserSettings.EnableForUser(User.ID))
            {
                Response.Redirect(GetRefererURL(), true);
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
                SetupCode = User.GenerateSetupCode(300);
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
            var type = typeof (ConfirmType).TryParseEnum(queryString["type"] ?? "", ConfirmType.EmpInvite);
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

        private string GetRefererURL()
        {
            var refererURL = (string)Context.Session["refererURL"];
            Context.Session["refererURL"] = null;

            if (string.IsNullOrEmpty(refererURL))
                refererURL = CommonLinkUtility.GetDefault();

            return refererURL;
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

                MessageService.Send(HttpContext.Current.Request, MessageAction.LoginSuccesViaTfaApp);
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

            var refererUrl = GetRefererURL();

            if (newBackupCodes)
            {
                MessageService.Send(HttpContext.Current.Request, MessageAction.UserConnectedTfaApp, MessageTarget.Create(user.ID));

                refererUrl = CommonLinkUtility.GetUserProfile(user.ID) + "#codes";
            }

            return new { RefererURL = refererUrl };
        }
    }
}