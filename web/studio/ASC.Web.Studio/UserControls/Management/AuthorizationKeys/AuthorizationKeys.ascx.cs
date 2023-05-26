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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

using AjaxPro;

using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Common.Configuration;
using ASC.Data.Storage;
using ASC.FederatedLogin.LoginProviders;
using ASC.MessagingSystem;
using ASC.Web.Core.Sms;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.ThirdPartyAuthorization, Location)]
    [AjaxNamespace("AuthorizationKeys")]
    public partial class AuthorizationKeys : UserControl
    {
        public const string Location = "~/UserControls/Management/AuthorizationKeys/AuthorizationKeys.ascx";

        private List<AuthService> _authServiceList;

        protected string TariffPageLink { get; set; }

        public List<AuthService> AuthServiceList
        {
            get { return _authServiceList ?? (_authServiceList = GetAuthServices().ToList()); }
        }

        protected string HelpLink { get; set; }

        protected string SupportLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            TariffPageLink = TenantExtra.GetTariffPageLink();
            AjaxPro.Utility.RegisterTypeForAjax(GetType(), Page);
            Page.RegisterBodyScripts("~/UserControls/Management/AuthorizationKeys/js/authorizationkeys.js");
            if(ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark)
            {
                Page.ClientScript.RegisterClientScriptBlock(GetType(), "authorizationkeys_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebPath.GetPath("UserControls/Management/AuthorizationKeys/css/dark-authorizationkeys.less") + "\">", false);
            }
            else
            {
                Page.ClientScript.RegisterClientScriptBlock(GetType(), "authorizationkeys_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebPath.GetPath("UserControls/Management/AuthorizationKeys/css/authorizationkeys.less") + "\">", false);
            }

            HelpLink = CommonLinkUtility.GetHelpLink();

            SupportLink = CommonLinkUtility.GetFeedbackAndSupportLink();
        }

        private static IEnumerable<AuthService> GetAuthServices()
        {
            return ConsumerFactory.Consumers
                .Where(consumer => consumer.ManagedKeys.Any())
                .Select(ToAuthService)
                .OrderBy(services => services.Order);
        }

        private static AuthService ToAuthService(Consumer consumer)
        {
            return new AuthService(consumer);
        }

        protected bool SaveAvailable
        {
            get { return CoreContext.Configuration.Standalone || CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId).ThirdParty; }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public bool SaveAuthKeys(string name, List<AuthKey> props)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (!SetupInfo.IsVisibleSettings(ManagementType.ThirdPartyAuthorization.ToString())
                || !SaveAvailable)
                throw new BillingException(Resource.ErrorNotAllowedOption, "ThirdPartyAuthorization");

            var changed = false;
            var consumer = ConsumerFactory.GetByName(name);

            var validateKeyProvider = (IValidateKeysProvider)ConsumerFactory.Consumers.FirstOrDefault(r => r.Name == consumer.Name && r is IValidateKeysProvider);
            if (validateKeyProvider != null)
            {
                RemoveOldNumberFromTwilio(validateKeyProvider);
                ClearUrlShortenerInstance(validateKeyProvider);
            }

            if (props.All(r => string.IsNullOrEmpty(r.Value)))
            {
                consumer.Clear();
                changed = true;
            }
            else
            {
                if(props.Any(r=> string.IsNullOrEmpty(r.Value) && !r.IsOptional))
                {
                    throw new Exception(Resource.ErrorEmptyFields);
                }
                foreach (var authKey in props.Where(authKey => consumer[authKey.Name] != authKey.Value))
                {
                    consumer[authKey.Name] = authKey.Value;
                    changed = true;
                }
            }

            //TODO: Consumer implementation required (Bug 50606)
            var allPropsIsEmpty = consumer.GetType() == typeof(SmscProvider)
                ? consumer.ManagedKeys.All(key => string.IsNullOrEmpty(consumer[key]))
                : consumer.All(r => string.IsNullOrEmpty(r.Value));

            if (validateKeyProvider != null && !validateKeyProvider.ValidateKeys() && !allPropsIsEmpty)
            {
                consumer.Clear();
                throw new ArgumentException(Resource.ErrorBadKeys);
            }

            if (changed)
                MessageService.Send(HttpContext.Current.Request, MessageAction.AuthorizationKeysSetting);

            return changed;
        }

        private static void RemoveOldNumberFromTwilio(IValidateKeysProvider provider)
        {
            try
            {
                var twilioLoginProvider = provider as TwilioProvider;
                if (twilioLoginProvider != null)
                {
                    twilioLoginProvider.ClearOldNumbers();
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC").Error(e);
            }
        }

        private static void ClearUrlShortenerInstance(IValidateKeysProvider provider)
        {
            try
            {
                var twilioLoginProvider = provider as BitlyLoginProvider;
                if (twilioLoginProvider != null)
                {
                    Web.Core.Utility.UrlShortener.Instance = null;
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC").Error(e);
            }
        }
    }
}