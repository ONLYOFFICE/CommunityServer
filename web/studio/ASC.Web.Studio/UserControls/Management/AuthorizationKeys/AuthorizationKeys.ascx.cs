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
using System.Collections.Generic;
using System.Globalization;
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
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.ThirdPartyAuthorization, Location)]
    [AjaxNamespace("AuthorizationKeys")]
    public partial class AuthorizationKeys : UserControl
    {
        public const string Location = "~/UserControls/Management/AuthorizationKeys/AuthorizationKeys.ascx";

        private List<AuthService> _authServiceList;

        public List<AuthService> AuthServiceList
        {
            get { return _authServiceList ?? (_authServiceList = GetAuthServices().ToList()); }
        }

        protected string HelpLink { get; set; }

        protected string SupportLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType(), Page);
            Page.RegisterBodyScripts("~/UserControls/Management/AuthorizationKeys/js/authorizationkeys.js");
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "authorizationkeys_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebPath.GetPath("UserControls/Management/AuthorizationKeys/css/authorizationkeys.css") + "\">", false);

            HelpLink = CommonLinkUtility.GetHelpLink();

            SupportLink = GetFeedbackAndSupportUrl();
        }

        private static string GetFeedbackAndSupportUrl()
        {
            var settings = AdditionalWhiteLabelSettings.Instance;

            if (!settings.FeedbackAndSupportEnabled || String.IsNullOrEmpty(settings.FeedbackAndSupportUrl))
                return string.Empty;

            return CommonLinkUtility.GetRegionalUrl(settings.FeedbackAndSupportUrl, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
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

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public bool SaveAuthKeys(string name, List<AuthKey> props)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            if (!SetupInfo.IsVisibleSettings(ManagementType.ThirdPartyAuthorization.ToString()))
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
                foreach (var authKey in props.Where(authKey => consumer[authKey.Name] != authKey.Value))
                {
                    consumer[authKey.Name] = authKey.Value;
                    changed = true;
                }
            }

            if (validateKeyProvider != null && !validateKeyProvider.ValidateKeys() && !consumer.All(r=> string.IsNullOrEmpty(r.Value)))
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