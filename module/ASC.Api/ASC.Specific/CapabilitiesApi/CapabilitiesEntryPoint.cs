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
using System.Configuration;
using System.Linq;
using System.Web;
using ASC.ActiveDirectory.Base.Settings;
using ASC.Api.Attributes;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Common.Logging;
using ASC.Core;
using ASC.FederatedLogin.LoginProviders;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Management.SingleSignOnSettings;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Studio.Utility;

namespace ASC.Specific.CapabilitiesApi
{
    /// <summary>
    /// Capabilities for api
    /// </summary>
    public class CapabilitiesEntryPoint : IApiEntryPoint
    {
        /// <summary>
        /// Entry point name
        /// </summary>
        public string Name
        {
            get { return "capabilities"; }
        }

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="context"></param>
        public CapabilitiesEntryPoint(ApiContext context)
        {
        }

        ///<summary>
        ///Returns the information about portal capabilities
        ///</summary>
        ///<short>
        ///Get portal capabilities
        ///</short>
        ///<returns>CapabilitiesData</returns>
        [Read("", false, false)] //NOTE: this method doesn't requires auth!!!  //NOTE: this method doesn't check payment!!!
        public CapabilitiesData GetPortalCapabilities()
        {
            var result = new CapabilitiesData
                {
                    LdapEnabled = false,
                    Providers = null,
                    SsoLabel = string.Empty,
                    SsoUrl = string.Empty
                };

            try
            {
                if (SetupInfo.IsVisibleSettings(ManagementType.LdapSettings.ToString())
                    && (!CoreContext.Configuration.Standalone
                        || CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Ldap))
                {
                    var settings = LdapSettings.Load();

                    result.LdapEnabled = settings.EnableLdapAuthentication;
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC").Error(ex.Message);
            }

            try
            {
                result.Providers = AccountLinkControl.AuthProviders
                                                     .Where(loginProvider =>
                                                         {
                                                             var provider = ProviderManager.GetLoginProvider(loginProvider);
                                                             return provider != null && provider.IsEnabled;
                                                         })
                                                     .ToList();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC").Error(ex.Message);
            }

            try
            {
                if (SetupInfo.IsVisibleSettings(ManagementType.SingleSignOnSettings.ToString())
                    && (!CoreContext.Configuration.Standalone
                        || CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Sso))
                {
                    var settings = SsoSettingsV2.Load();

                    if (settings.EnableSso)
                    {
                        var uri = HttpContext.Current.Request.GetUrlRewriter();

                        var configUrl = GetAppSettings("web.sso.saml.login.url", "");

                        result.SsoUrl = string.Format("{0}://{1}{2}{3}", uri.Scheme, uri.Host,
                                                      (uri.Port == 80 || uri.Port == 443) ? "" : ":" + uri.Port, configUrl);

                        result.SsoLabel = settings.SpLoginLabel;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC").Error(ex.Message);
            }

            return result;
        }

        private static string GetAppSettings(string key, string defaultValue)
        {
            var result = ConfigurationManager.AppSettings[key] ?? defaultValue;

            if (!string.IsNullOrEmpty(result))
                result = result.Trim();

            return result;
        }
    }
}