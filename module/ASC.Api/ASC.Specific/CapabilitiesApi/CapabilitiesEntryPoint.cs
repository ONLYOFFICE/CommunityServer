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
using System.Configuration;
using System.Linq;
using System.Web;

using ASC.ActiveDirectory.Base.Settings;
using ASC.Api.Attributes;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Common.Logging;
using ASC.Core;
using ASC.FederatedLogin;
using ASC.FederatedLogin.LoginProviders;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Management.SingleSignOnSettings;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Studio.Utility;

namespace ASC.Specific.CapabilitiesApi
{
    /// <summary>
    /// Portal capabilities API.
    /// </summary>
    /// <name>capabilities</name>
    public class CapabilitiesEntryPoint : IApiEntryPoint
    {

        private ILog Log = LogManager.GetLogger("ASC");

        /// <summary>
        /// Entry point name.
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
        ///Returns the information about portal capabilities.
        ///</summary>
        ///<short>
        ///Get portal capabilities
        ///</short>
        ///<returns type="ASC.Specific.CapabilitiesApi.CapabilitiesData, ASC.Specific">Portal capabilities</returns>
        ///<path>api/2.0/capabilities</path>
        /// <requiresAuthorization>false</requiresAuthorization>
        ///<httpMethod>GET</httpMethod>
        [Read("", false, false)] //NOTE: this method doesn't require auth!!!  //NOTE: this method doesn't check payment!!!
        public CapabilitiesData GetPortalCapabilities()
        {
            var result = new CapabilitiesData
            {
                LdapEnabled = false,
                OauthEnabled = CoreContext.Configuration.Standalone || CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Oauth,
                Providers = new List<string>(0),
                SsoLabel = string.Empty,
                SsoUrl = string.Empty
            };

            try
            {
                if (CoreContext.Configuration.Standalone
                    || SetupInfo.IsVisibleSettings(ManagementType.LdapSettings.ToString())
                        && CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Ldap)
                {
                    var settings = LdapSettings.Load();

                    result.LdapEnabled = settings.EnableLdapAuthentication;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            try
            {
                if (result.OauthEnabled)
                {
                    result.Providers = AccountLinkControl.AuthProviders
                                                         .Where(loginProvider =>
                                                             {
                                                                 if ((loginProvider == ProviderConstants.Facebook || loginProvider == ProviderConstants.AppleId)
                                                                    && CoreContext.Configuration.Standalone && HttpContext.Current.Request.MobileApp())
                                                                 {
                                                                     return false;
                                                                 }
                                                                 var provider = ProviderManager.GetLoginProvider(loginProvider);
                                                                 return provider != null && provider.IsEnabled;
                                                             })
                                                         .ToList();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            try
            {
                if (CoreContext.Configuration.Standalone
                    || SetupInfo.IsVisibleSettings(ManagementType.SingleSignOnSettings.ToString())
                        && CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Sso)
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
                Log.Error(ex.Message);
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