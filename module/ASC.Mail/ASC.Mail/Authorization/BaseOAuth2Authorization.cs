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
using ASC.Common.Logging;
using ASC.Core.Common.Configuration;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;

namespace ASC.Mail.Authorization
{
    public class BaseOAuth2Authorization<T> where T : Consumer, ILoginProvider, new()
    {
        public readonly ILog log;

        private readonly T loginProvider;

        public string ClientId
        {
            get { return loginProvider.ClientID; }
        }

        public string ClientSecret
        {
            get { return loginProvider.ClientSecret; }
        }

        public string RedirectUrl
        {
            get { return loginProvider.RedirectUri; }
        }

        public string RefreshUrl
        {
            get { return loginProvider.AccessTokenUrl; }
        }

        public BaseOAuth2Authorization(ILog log)
        {
            if (null == log)
                throw new ArgumentNullException("log");

            this.log = log;
            loginProvider = ConsumerFactory.Get<T>();

            try
            {
                if (String.IsNullOrEmpty(loginProvider.ClientID))
                    throw new ArgumentNullException("ClientId");

                if (String.IsNullOrEmpty(loginProvider.ClientSecret))
                    throw new ArgumentNullException("ClientSecret");

                if (String.IsNullOrEmpty(loginProvider.RedirectUri))
                    throw new ArgumentNullException("RedirectUrl");
        }
            catch (Exception ex)
            {
                log.ErrorFormat("GoogleOAuth2Authorization() Exception:\r\n{0}\r\n", ex.ToString());
            }
        }

        public OAuth20Token RequestAccessToken(string refreshToken)
        {
            var token = new OAuth20Token
                {
                    ClientID = ClientId,
                    ClientSecret = ClientSecret,
                    RedirectUri = RedirectUrl,
                    RefreshToken = refreshToken,
                };

            try
            {
                return OAuth20TokenHelper.RefreshToken<T>(token);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("RequestAccessToken() Exception:\r\n{0}\r\n", ex.ToString());
                return null;
            }
        }
    }
}