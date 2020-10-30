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
using System.Globalization;
using System.Net;
using System.Xml.Linq;
using System.Xml.XPath;
using ASC.Core.Common.Configuration;

namespace ASC.FederatedLogin.LoginProviders
{
    public class BitlyLoginProvider : Consumer, IValidateKeysProvider
    {
        private static string BitlyClientId
        {
            get { return Instance["bitlyClientId"]; }
        }

        private static string BitlyClientSecret
        {
            get { return Instance["bitlyClientSecret"]; }
        }

        private static string BitlyUrl
        {
            get { return Instance["bitlyUrl"]; }
        }

        private static BitlyLoginProvider Instance
        {
            get { return ConsumerFactory.Get<BitlyLoginProvider>(); }
        }

        public BitlyLoginProvider() { }

        public BitlyLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null)
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
                return !String.IsNullOrEmpty(BitlyClientId) &&
                       !String.IsNullOrEmpty(BitlyClientSecret) &&
                       !String.IsNullOrEmpty(BitlyUrl);
            }
        }

        public static String GetShortenLink(String shareLink)
        {
            var uri = new Uri(shareLink);

            var bitly = string.Format(BitlyUrl, BitlyClientId, BitlyClientSecret, Uri.EscapeDataString(uri.ToString()));
            XDocument response;
            try
            {
                response = XDocument.Load(bitly);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(e.Message, e);
            }

            var status = response.XPathSelectElement("/response/status_code").Value;
            if (status != ((int)HttpStatusCode.OK).ToString(CultureInfo.InvariantCulture))
            {
                throw new InvalidOperationException(status);
            }

            var data = response.XPathSelectElement("/response/data/url");

            return data.Value;
        }
    }
}
