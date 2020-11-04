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
using System.Configuration;
using System.Linq;

using ASC.FederatedLogin.LoginProviders;
using ASC.Web.Files.Classes;

namespace ASC.Web.Files.Helpers
{
    public static class ThirdpartyConfiguration
    {
        public static IEnumerable<string> ThirdPartyProviders
        {
            get { return (ConfigurationManagerExtension.AppSettings["files.thirdparty.enable"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries); }
        }

        public static bool SupportInclusion
        {
            get
            {
                using (var providerDao = Global.DaoFactory.GetProviderDao())
                {
                    if (providerDao == null) return false;
                }

                return SupportBoxInclusion || SupportDropboxInclusion || SupportDocuSignInclusion || SupportGoogleDriveInclusion || SupportOneDriveInclusion || SupportSharePointInclusion || SupportWebDavInclusion || SupportNextcloudInclusion || SupportOwncloudInclusion || SupportkDriveInclusion || SupportYandexInclusion;
            }
        }

        public static bool SupportBoxInclusion
        {
            get
            {
                return ThirdPartyProviders.Contains("box") && BoxLoginProvider.Instance.IsEnabled;
            }
        }

        public static bool SupportDropboxInclusion
        {
            get
            {
                return ThirdPartyProviders.Contains("dropboxv2") && DropboxLoginProvider.Instance.IsEnabled;
            }
        }

        public static bool SupportOneDriveInclusion
        {
            get
            {
                return ThirdPartyProviders.Contains("onedrive") && OneDriveLoginProvider.Instance.IsEnabled;
            }
        }

        public static bool SupportSharePointInclusion
        {
            get { return ThirdPartyProviders.Contains("sharepoint"); }
        }

        public static bool SupportWebDavInclusion
        {
            get { return ThirdPartyProviders.Contains("webdav"); }
        }

        public static bool SupportNextcloudInclusion
        {
            get { return ThirdPartyProviders.Contains("nextcloud"); }
        }

        public static bool SupportOwncloudInclusion
        {
            get { return ThirdPartyProviders.Contains("owncloud"); }
        }

        public static bool SupportkDriveInclusion
        {
            get { return ThirdPartyProviders.Contains("kdrive"); }
        }

        public static bool SupportYandexInclusion
        {
            get { return ThirdPartyProviders.Contains("yandex"); }
        }

        public static string DropboxAppKey
        {
            get { return DropboxLoginProvider.Instance["dropboxappkey"]; }
        }

        public static string DropboxAppSecret
        {
            get { return DropboxLoginProvider.Instance["dropboxappsecret"]; }
        }

        public static bool SupportDocuSignInclusion
        {
            get
            {
                return ThirdPartyProviders.Contains("docusign") && DocuSignLoginProvider.Instance.IsEnabled;
            }
        }

        public static bool SupportGoogleDriveInclusion
        {
            get
            {
                return ThirdPartyProviders.Contains("google") && GoogleLoginProvider.Instance.IsEnabled;
            }
        }
    }
}