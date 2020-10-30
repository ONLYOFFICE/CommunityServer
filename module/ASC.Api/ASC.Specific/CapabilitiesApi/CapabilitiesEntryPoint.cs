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
            var result = ConfigurationManagerExtension.AppSettings[key] ?? defaultValue;

            if (!string.IsNullOrEmpty(result))
                result = result.Trim();

            return result;
        }
    }
}