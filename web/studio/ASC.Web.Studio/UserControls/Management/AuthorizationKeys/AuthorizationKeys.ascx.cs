/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using AjaxPro;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Data.Storage;
using ASC.FederatedLogin.LoginProviders;
using ASC.MessagingSystem;
using ASC.Thrdparty.Configuration;
using ASC.VoipService.Twilio;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Utility;
using log4net;
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
            Page.RegisterBodyScripts("~/usercontrols/management/AuthorizationKeys/js/authorizationkeys.js");
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "authorizationkeys_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebPath.GetPath("usercontrols/management/authorizationkeys/css/authorizationkeys.css") + "\">", false);

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
            return from KeyElement keyElement in ConsumerConfigurationSection.GetSection().Keys
                   where keyElement.Type != KeyElement.KeyType.Default && !string.IsNullOrEmpty(keyElement.ConsumerName)
                   group keyElement by keyElement.ConsumerName
                   into keyGroup
                   let consumerKey = keyGroup.FirstOrDefault(key => key.Type == KeyElement.KeyType.Key)
                   let consumerSecret = keyGroup.FirstOrDefault(key => key.Type == KeyElement.KeyType.Secret)
                   let consumerKeyDefault = keyGroup.FirstOrDefault(key => key.Type == KeyElement.KeyType.KeyDefault)
                   let consumerSecretDefault = keyGroup.FirstOrDefault(key => key.Type == KeyElement.KeyType.SecretDefault)
                   select ToAuthService(keyGroup.Key, consumerKey, consumerSecret, consumerKeyDefault, consumerSecretDefault)
                   into services
                   orderby services.Order
                   select services;
        }

        private static AuthService ToAuthService(string consumerName, KeyElement consumerKey, KeyElement consumerSecret, KeyElement consumerKeyDefault, KeyElement consumerSecretDefault)
        {
            var authService = new AuthService(consumerName);
            if (consumerKey != null)
            {
                authService.WithKey(consumerKey.Name, KeyStorage.Get(consumerKey.Name));
                if (KeyStorage.CanSet(consumerKey.Name)) authService.CanSet = true;
                if (consumerKey.Order.HasValue) authService.Order = consumerKey.Order;
            }
            if (consumerSecret != null)
            {
                authService.WithSecret(consumerSecret.Name, KeyStorage.Get(consumerSecret.Name));
                if (!authService.CanSet && KeyStorage.CanSet(consumerSecret.Name)) authService.CanSet = true;
                if (!authService.Order.HasValue && consumerSecret.Order.HasValue) authService.Order = consumerSecret.Order;
            }
            if (consumerKeyDefault != null)
            {
                authService.WithKeyDefault(consumerKeyDefault.Name, KeyStorage.Get(consumerKeyDefault.Name));
                if (!authService.CanSet && KeyStorage.CanSet(consumerKeyDefault.Name)) authService.CanSet = true;
                if (!authService.Order.HasValue && consumerKeyDefault.Order.HasValue) authService.Order = consumerKeyDefault.Order;
            }
            if (consumerSecretDefault != null)
            {
                authService.WithSecretDefault(consumerSecretDefault.Name, KeyStorage.Get(consumerSecretDefault.Name));
                if (!authService.CanSet && KeyStorage.CanSet(consumerSecretDefault.Name)) authService.CanSet = true;
                if (!authService.Order.HasValue && consumerSecretDefault.Order.HasValue) authService.Order = consumerSecretDefault.Order;
            }

            if (!authService.Order.HasValue) authService.Order = int.MaxValue;

            return authService;
        }

        private static readonly Dictionary<string, IValidateKeysProvider> Providers = new Dictionary<string, IValidateKeysProvider>
            {
                {
                    "Bitly",
                    new BitlyLoginProvider()
                },
                {
                    "Twilio",
                    new TwilioLoginProvider()
                },
                {
                    "Smsc",
                    new SmscProvider()
                }
            };

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public bool SaveAuthKeys(List<AuthKey> authKeys)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            if (!SetupInfo.IsVisibleSettings(ManagementType.ThirdPartyAuthorization.ToString()))
                throw new BillingException(Resource.ErrorNotAllowedOption, "ThirdPartyAuthorization");

            var changed = false;

            var mapKeys = new Dictionary<string, List<KeyElement>>();
            foreach (var authKey in authKeys.Where(authKey => KeyStorage.Get(authKey.Name) != authKey.Value))
            {
                var keyElement = ConsumerConfigurationSection.GetSection().Keys.GetKey(authKey.Name);

                if (keyElement != null && Providers.ContainsKey(keyElement.ConsumerName))
                {
                    RemoveOldNumberFromTwilio(Providers[keyElement.ConsumerName]);

                    if (!string.IsNullOrEmpty(authKey.Value))
                    {
                        if (!mapKeys.ContainsKey(keyElement.ConsumerName))
                        {
                            mapKeys.Add(keyElement.ConsumerName, new List<KeyElement>());
                        }
                        mapKeys[keyElement.ConsumerName].Add(keyElement);
                    }
                }


                KeyStorage.Set(authKey.Name, authKey.Value);
                changed = true;
            }

            foreach (var providerKeys in mapKeys)
            {
                if (!Providers[providerKeys.Key].ValidateKeys())
                {
                    foreach (var providerKey in providerKeys.Value)
                    {
                        KeyStorage.Set(providerKey.Name, null);
                    }
                    throw new ArgumentException(Resource.ErrorBadKeys);
                }
            }

            if (changed)
                MessageService.Send(HttpContext.Current.Request, MessageAction.AuthorizationKeysSetting);

            return changed;
        }

        private static void RemoveOldNumberFromTwilio(IValidateKeysProvider provider)
        {
            try
            {
                var twilioLoginProvider = provider as TwilioLoginProvider;
                if (twilioLoginProvider != null)
                {
                    twilioLoginProvider.ClearOldNumbers();
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(AuthorizationKeys)).Error(e);
            }
        }
    }
}