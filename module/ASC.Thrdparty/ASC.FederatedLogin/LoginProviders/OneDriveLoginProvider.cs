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


using System.Collections.Generic;
using ASC.Core.Common.Configuration;

namespace ASC.FederatedLogin.LoginProviders
{
    public class OneDriveLoginProvider : Consumer, IOAuthProvider
    {
        private const string OneDriveOauthUrl = "https://login.live.com/";
        public const string OneDriveApiUrl = "https://api.onedrive.com";

        public static OneDriveLoginProvider Instance
        {
            get { return ConsumerFactory.Get<OneDriveLoginProvider>(); }
        }

        public string Scopes { get { return "wl.signin wl.skydrive_update wl.offline_access"; } }
        public string CodeUrl { get { return OneDriveOauthUrl + "oauth20_authorize.srf"; } }
        public string AccessTokenUrl { get { return OneDriveOauthUrl + "oauth20_token.srf"; } }
        public string RedirectUri { get { return this["skydriveRedirectUrl"]; } }
        public string ClientID { get { return this["skydriveappkey"]; } }
        public string ClientSecret { get { return this["skydriveappsecret"]; } }

        public bool IsEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(ClientID) &&
                       !string.IsNullOrEmpty(ClientSecret) &&
                       !string.IsNullOrEmpty(RedirectUri);
            }
        }

        public OneDriveLoginProvider() { }

        public OneDriveLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null)
            : base(name, order, props, additional)
        {
        }
    }
}