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

using ASC.Core.Common.Configuration;
using ASC.FederatedLogin.Helpers;

using Newtonsoft.Json.Linq;

namespace ASC.FederatedLogin.LoginProviders
{
    public class BitlyLoginProvider : Consumer, IValidateKeysProvider
    {
        private static string BitlyToken
        {
            get { return Instance["bitlyToken"]; }
        }

        private readonly static string BitlyUrl = "https://api-ssl.bitly.com/v4/shorten";

        private static BitlyLoginProvider Instance
        {
            get { return ConsumerFactory.Get<BitlyLoginProvider>(); }
        }

        public BitlyLoginProvider() { }

        public BitlyLoginProvider(string name, int order, Dictionary<string, Prop> props, Dictionary<string, Prop> additional = null)
            : base(name, order, props, additional)
        {
        }

        public bool ValidateKeys()
        {
            try
            {
                return !string.IsNullOrEmpty(GetShortenLink("https://www.onlyoffice.com"));
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Enabled
        {
            get
            {
                return !String.IsNullOrEmpty(BitlyToken);
            }
        }

        public static String GetShortenLink(String shareLink)
        {
            var data = string.Format("{{\"long_url\":\"{0}\"}}", shareLink);
            var headers = new Dictionary<string, string>
            {
                {"Authorization" ,"Bearer " + BitlyToken}
            };

            var response = RequestHelper.PerformRequest(BitlyUrl, "application/json", "POST", data, headers);

            var parser = JObject.Parse(response);
            if (parser == null) return null;

            var link = parser.Value<string>("link");

            return link;
        }
    }
}
