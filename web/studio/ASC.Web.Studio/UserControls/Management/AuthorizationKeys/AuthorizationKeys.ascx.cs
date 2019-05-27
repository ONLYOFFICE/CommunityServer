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
    }
}