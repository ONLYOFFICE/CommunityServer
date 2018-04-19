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
using System.Linq;
using System.Web;
using ASC.Api.Attributes;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Management.SingleSignOnSettings;
using ASC.Web.Studio.Utility;
using Newtonsoft.Json;
using Resources;

namespace ASC.Api.Settings
{
    public partial class SettingsApi
    {
        /// <summary>
        /// Returns current portal SSO settings
        /// </summary>
        /// <short>
        /// Get SSO settings
        /// </short>
        /// <returns>SsoSettingsV2 object</returns>
        [Read("ssov2")]
        public SsoSettingsV2 GetSsoSettingsV2()
        {
            CheckSsoPermissions();

            var settings = SsoSettingsV2.Load();

            if(string.IsNullOrEmpty(settings.SpLoginLabel))
                settings.SpLoginLabel = SsoSettingsV2.SSO_SP_LOGIN_LABEL;

            return settings;
        }

        /// <summary>
        /// Returns default portal SSO settings
        /// </summary>
        /// <short>
        /// Get default SSO settings
        /// </short>
        /// <returns>SsoSettingsV2 object</returns>
        [Read("ssov2/default")]
        public SsoSettingsV2 GetDefaultSsoSettingsV2()
        {
            CheckSsoPermissions();

            return new SsoSettingsV2().GetDefault() as SsoSettingsV2;
        }

        /// <summary>
        /// Returns SSO settings constants
        /// </summary>
        /// <short>
        /// Get SSO settings constants
        /// </short>
        /// <returns>object</returns>
        [Read("ssov2/constants")]
        public object GetSsoSettingsV2Constants()
        {
            return new
                {
                    SsoNameIdFormatType = new SsoNameIdFormatType(),
                    SsoBindingType = new SsoBindingType(),
                    SsoSigningAlgorithmType = new SsoSigningAlgorithmType(),
                    SsoEncryptAlgorithmType = new SsoEncryptAlgorithmType(),
                    SsoSpCertificateActionType = new SsoSpCertificateActionType(),
                    SsoIdpCertificateActionType = new SsoIdpCertificateActionType()
                };
        }

        /// <summary>
        /// Save SSO settings for current portal
        /// </summary>
        /// <short>
        /// Save SSO settings
        /// </short>
        /// <param name="serializeSettings">serialized SsoSettingsV2 object</param>
        /// <returns>SsoSettingsV2 object</returns>
        [Create("ssov2")]
        public SsoSettingsV2 SaveSsoSettingsV2(string serializeSettings)
        {
            CheckSsoPermissions();

            if (string.IsNullOrEmpty(serializeSettings))
                throw new ArgumentException(Resource.SsoSettingsCouldNotBeNull);

            var settings = JsonConvert.DeserializeObject<SsoSettingsV2>(serializeSettings);

            if (settings == null)
                throw new ArgumentException(Resource.SsoSettingsCouldNotBeNull);

            if (string.IsNullOrWhiteSpace(settings.IdpSettings.EntityId))
                throw new Exception(Resource.SsoSettingsInvalidEntityId);

            if (string.IsNullOrWhiteSpace(settings.IdpSettings.SsoUrl) ||
                !CheckUri(settings.IdpSettings.SsoUrl))
                throw new Exception(string.Format(Resource.SsoSettingsInvalidBinding, "SSO " + settings.IdpSettings.SsoBinding));

            if (!string.IsNullOrWhiteSpace(settings.IdpSettings.SloUrl) &&
                !CheckUri(settings.IdpSettings.SloUrl))
                throw new Exception(string.Format(Resource.SsoSettingsInvalidBinding, "SLO " + settings.IdpSettings.SloBinding));

            if (string.IsNullOrWhiteSpace(settings.FieldMapping.FirstName) ||
                string.IsNullOrWhiteSpace(settings.FieldMapping.LastName) ||
                string.IsNullOrWhiteSpace(settings.FieldMapping.Email))
                throw new Exception(Resource.SsoSettingsInvalidMapping);

            if (string.IsNullOrEmpty(settings.SpLoginLabel))
            {
                settings.SpLoginLabel = SsoSettingsV2.SSO_SP_LOGIN_LABEL;
            }
            else if (settings.SpLoginLabel.Length > 100)
            {
                settings.SpLoginLabel = settings.SpLoginLabel.Substring(0, 100);
            }

            if (!settings.Save())
                throw new Exception(Resource.SsoSettingsCantSaveSettings);

            if(!settings.EnableSso)
                ConverSsoUsersToOrdinary();

            var messageAction = settings.EnableSso ? MessageAction.SSOEnabled : MessageAction.SSODisabled;

            MessageService.Send(HttpContext.Current.Request, messageAction);

            return settings;
        }

        /// <summary>
        /// Reset SSO settings for current portal
        /// </summary>
        /// <short>
        /// Reset SSO settings
        /// </short>
        /// <returns>SsoSettingsV2 object</returns>
        [Delete("ssov2")]
        public SsoSettingsV2 ResetSsoSettingsV2()
        {
            CheckSsoPermissions();

            var defaultSettings = new SsoSettingsV2().GetDefault() as SsoSettingsV2;

            if (defaultSettings != null && !defaultSettings.Save())
                throw new Exception(Resource.SsoSettingsCantSaveSettings);

            ConverSsoUsersToOrdinary();

            MessageService.Send(HttpContext.Current.Request, MessageAction.SSODisabled);

            return defaultSettings;
        }

        private static void ConverSsoUsersToOrdinary()
        {
            var ssoUsers = CoreContext.UserManager.GetUsers().Where(u => u.IsSSO()).ToList();

            if(!ssoUsers.Any())
                return;

            foreach (var existingSsoUser in ssoUsers)
            {
                existingSsoUser.SsoNameId = null;
                existingSsoUser.SsoSessionId = null;

                existingSsoUser.ConvertExternalContactsToOrdinary();

                CoreContext.UserManager.SaveUserInfo(existingSsoUser);
            }
        }

        private static bool CheckUri(string uriName)
        {
            Uri uriResult;
            return Uri.TryCreate(uriName, UriKind.Absolute, out uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private static void CheckSsoPermissions()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (!SetupInfo.IsVisibleSettings(ManagementType.SingleSignOnSettings.ToString()) ||
                (CoreContext.Configuration.Standalone &&
                 !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Sso))
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "Sso");
            }
        }
    }
}
