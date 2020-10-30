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
using System.Threading;
using System.Web;
using ASC.Core.Common.Configuration;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;

namespace ASC.FederatedLogin.LoginProviders
{
    public abstract class BaseLoginProvider<T> : Consumer, ILoginProvider where T : Consumer, ILoginProvider, new ()
    {
        public static T Instance
        {
            get
            {
                return ConsumerFactory.Get<T>();
            }
        }

        public abstract string CodeUrl { get; }
        public abstract string AccessTokenUrl { get; }
        public abstract string RedirectUri { get; }
        public abstract string ClientID { get; }
        public abstract string ClientSecret { get; }
        public virtual string Scopes { get { return ""; } }

        public virtual bool IsEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(ClientID) &&
                       !string.IsNullOrEmpty(ClientSecret) &&
                       !string.IsNullOrEmpty(RedirectUri);
            }
        }

        protected BaseLoginProvider()
        {
            
        }

        protected BaseLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null): base(name, order, props, additional)
        {
            
        }

        public virtual LoginProfile ProcessAuthoriztion(HttpContext context, IDictionary<string, string> @params)
        {
            try
            {
                var token = Auth(context, Scopes);

                return GetLoginProfile(token == null ? null : token.AccessToken);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return LoginProfile.FromError(ex);
            }
        }

        protected virtual OAuth20Token Auth(HttpContext context, string scopes, Dictionary<string, string> additionalArgs = null)
        {
            var error = context.Request["error"];
            if (!string.IsNullOrEmpty(error))
            {
                if (error == "access_denied")
                {
                    error = "Canceled at provider";
                }
                throw new Exception(error);
            }

            var code = context.Request["code"];
            if (string.IsNullOrEmpty(code))
            {
                OAuth20TokenHelper.RequestCode<T>(HttpContext.Current, scopes, additionalArgs);
                return null;
            }

            return OAuth20TokenHelper.GetAccessToken<T>(code);
        }

        public abstract LoginProfile GetLoginProfile(string accessToken);
    }
}
