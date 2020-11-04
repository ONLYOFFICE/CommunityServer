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
using System.Web;
using ASC.Core.Common.Configuration;
using ASC.FederatedLogin.Profile;

namespace ASC.FederatedLogin.LoginProviders
{
    public class ProviderManager
    {
        public static ILoginProvider GetLoginProvider(string providerType)
        {
            return providerType == ProviderConstants.OpenId
                ? new OpenIdLoginProvider()
                : ConsumerFactory.GetByName(providerType) as ILoginProvider;
        }

        public static LoginProfile Process(string providerType, HttpContext context, IDictionary<string, string> @params)
        {
            return GetLoginProvider(providerType).ProcessAuthoriztion(context, @params);
        }

        public static LoginProfile GetLoginProfile(string providerType, string accessToken)
        {
            var consumer = GetLoginProvider(providerType);
            if (consumer == null) throw new ArgumentException("Unknown provider type", "providerType");

            try
            {
                return consumer.GetLoginProfile(accessToken);
            }
            catch (Exception ex)
            {
                return LoginProfile.FromError(ex);
            }
        }
    }
}