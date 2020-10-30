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
    public class DropboxLoginProvider: Consumer, IOAuthProvider
    {
        public static DropboxLoginProvider Instance
        {
            get { return ConsumerFactory.Get<DropboxLoginProvider>(); }
        }

        public string Scopes { get { return ""; } }
        public string CodeUrl { get { return "https://www.dropbox.com/oauth2/authorize"; } }
        public string AccessTokenUrl { get { return "https://api.dropboxapi.com/oauth2/token"; } }
        public string RedirectUri { get { return this["dropboxRedirectUrl"]; } }
        public string ClientID { get { return this["dropboxClientId"]; } }
        public string ClientSecret { get { return this["dropboxClientSecret"]; } }

        public bool IsEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(ClientID) &&
                       !string.IsNullOrEmpty(ClientSecret) &&
                       !string.IsNullOrEmpty(RedirectUri);
            }
        }

        public DropboxLoginProvider() { }

        public DropboxLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null)
            : base(name, order, props, additional)
        {
        }
    }
}